// <copyright file="ProductAggregateRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Caching;
using IndTrace.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IndTrace.Persistence.Repositories;

/// <summary>
/// #95 Phase 2 Slice B: operation-scoped unit of work for the <see cref="Product"/> aggregate
/// (Product -&gt; Recipe; <c>ProductSpec</c> has zero call-site surface today). Each call obtains one pooled
/// <see cref="IIndTraceDbContext"/> from the factory, uses it, and disposes it inside the call — no scoped
/// state is captured, so it is safe to invoke from a singleton worker (identical to how
/// <see cref="Repository{T}"/>, <see cref="ProductRoutingRepository"/> and
/// <see cref="BarCodeAggregateRepository"/> use the factory). Consumes the shared
/// <see cref="IAggregateRepository{TRoot}"/> contract.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SaveAsync"/> persists the root's staged Recipe changes
/// (<see cref="Product.PendingRecipeRemovals"/> delete batch first, then
/// <see cref="Product.PendingRecipeAppends"/> insert batch) inside ONE explicit local transaction wrapped in
/// the context's retrying execution strategy — the <see cref="ProductRoutingRepository"/> two-flush shape.
/// The <see cref="Product"/> root row itself is NEVER mutated by this save (recipes carry a scalar
/// <c>ProductId</c>, no DB FK, and Product has no <c>rowversion</c> token — deliberately no schema change),
/// so no root-row concurrency predicate is attached.
/// </para>
/// <para>
/// A <see cref="DbUpdateConcurrencyException"/> (a staged removal whose row a competing writer already
/// deleted/changed) and any other infrastructure failure are caught and surfaced as a <see cref="Result"/>
/// failure — this repository NEVER throws across the boundary. After a durable commit the staged sets are
/// cleared (<see cref="Product.ClearStagedRecipeChanges"/>) so a repeated save cannot double-apply them.
/// </para>
/// </remarks>
public sealed class ProductAggregateRepository : IAggregateRepository<Product>
{
    private const string ContextInactive = "Database context is not active.";
    private const string ConcurrencyConflict =
        "The product's recipes were modified by another writer; reload and retry.";

    private readonly IIndTraceDbContextFactory contextFactory;
    private readonly ILogger<ProductAggregateRepository> logger;

    // #116: optional on purpose (see ctor docs) — null means "host registered no cache", never an error.
    private readonly ICacheService? cacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductAggregateRepository"/> class.
    /// </summary>
    /// <param name="contextFactory">The pooled database context factory (one context per operation).</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cacheService">
    /// #116: OPTIONAL write-side cache invalidation seam (explicitly nullable, mirroring
    /// <see cref="ProductRoutingRepository"/>). When the composition root registers an
    /// <see cref="ICacheService"/> (production hosts do), DI injects it and a successful
    /// <see cref="SaveAsync"/> invalidates the read caches of the entity type the aggregate write touched
    /// (Recipe — the root row is never mutated by this save). Roots without a cache (test hosts) resolve
    /// <see langword="null"/> and skip invalidation — there is nothing cached to invalidate.
    /// </param>
    public ProductAggregateRepository(
        IIndTraceDbContextFactory contextFactory,
        ILogger<ProductAggregateRepository> logger,
        ICacheService? cacheService = null)
    {
        this.contextFactory = contextFactory;
        this.logger = logger;
        this.cacheService = cacheService;
    }

    /// <summary>
    /// Loads the product aggregate: the <see cref="Product"/> root row plus ALL of its <see cref="Recipe"/>
    /// member rows (matched on the scalar <c>ProductId</c> — recipes carry no DB FK), folded onto the root
    /// via <see cref="Product.AttachLoadedRecipes"/>. Always a full load — the machine window is for the #40
    /// BarCode aggregate and is ignored here.
    /// </summary>
    /// <param name="id">The product id whose aggregate is loaded (the aggregate identity).</param>
    /// <param name="options">The load options; must be non-null (Product always loads Full).</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result{T}"/> wrapping the aggregate (root + loaded recipes), or a failure.</returns>
    public async Task<Result<Product>> LoadAsync(int id, AggregateLoadOptions options, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return Result<Product>.WithFailure("Operation was canceled.");
        }

        if (options is null)
        {
            return Result<Product>.WithFailure("The aggregate load options must not be null.");
        }

        try
        {
            await using var ctx = await this.contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            if (this.InvalidContext(ctx))
            {
                return Result<Product>.WithFailure(ContextInactive);
            }

            var productKey = new ProductId(id);
            var product = await ctx.Set<Product>()
                .AsTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productKey, ct)
                .ConfigureAwait(false);

            if (product is null)
            {
                return Result<Product>.WithFailure($"Product {id} was not found.");
            }

            var recipes = await ctx.Set<Recipe>()
                .AsTracking()
                .Where(r => r.ProductId == id)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            product.AttachLoadedRecipes(recipes);

            return Result<Product>.Success(product);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "ProductAggregateRepository: error loading aggregate for product {ProductId}.", id);
            return Result<Product>.WithFailure(ex.Message);
        }
    }

    /// <summary>
    /// Persists the root's staged Recipe changes in ONE explicit transaction: the removal batch first
    /// (delete-by-key — Recipe carries no <c>rowversion</c>), then the append batch, then commit. The
    /// <see cref="Product"/> root row is NOT written (no root mutation belongs to this save; Product carries
    /// no concurrency token — no schema change). A concurrency conflict (a staged removal already gone) or
    /// any other infrastructure failure is caught and returned as a <see cref="Result"/> failure; the
    /// transaction rolls back so no partial write leaks. Never throws across the boundary. The staged sets
    /// are CONSUMED by the attempt: after a durable commit they are cleared so a repeated save is a no-op,
    /// and after a failed attempt they are discarded so a later save of the same root cannot double-apply a
    /// stale batch — the caller must restage to retry (adversarial-review finding, PR #170).
    /// </summary>
    /// <param name="root">The aggregate whose staged recipe appends/removals are persisted.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the reason (including a concurrency conflict).</returns>
    public async Task<Result> SaveAsync(Product root, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        if (root is null)
        {
            return Result.WithFailure("The product aggregate must not be null.");
        }

        if (root.PendingRecipeAppends.Count == 0 && root.PendingRecipeRemovals.Count == 0)
        {
            // Nothing staged — the DB is already correct. An idempotent no-op success (nothing was written,
            // so nothing is cache-invalidated either), mirroring the BarCode idempotent no-op precedent.
            return Result.Success();
        }

        try
        {
            await using var ctx = await this.contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            if (this.InvalidContext(ctx))
            {
                root.ClearStagedRecipeChanges();
                return Result.WithFailure(ContextInactive);
            }

            // The context is configured with a retrying execution strategy (EnableRetryOnFailure), which forbids
            // a user-initiated BeginTransaction unless it is wrapped in the strategy's retriable unit. Follow the
            // ProductRoutingRepository convention: run the whole two-flush transaction inside
            // CreateExecutionStrategy().ExecuteAsync so it is a single retriable unit. A concurrency/FK failure is
            // non-transient, so it is NOT retried — it propagates out to the catch blocks below and is surfaced
            // as a Result failure (never thrown across the boundary).
            var strategy = ctx.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // One explicit local transaction on this single connection — no ambient TransactionScope, no
                // MSDTC. Production ALWAYS uses the real transaction; the EF InMemory provider returns a no-op
                // transaction, so this path is exercised functionally in tests (atomicity proofs are real-SQL).
                await using var transaction = await ctx.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

                // F5 (#117) + C4 (#126) RETRY IDEMPOTENCY CONTRACT (the ProductRoutingRepository shape): this
                // lambda may re-execute after a transient fault — including a fault AFTER a successful flush.
                //   1. ChangeTracker.Clear() drops whatever the failed attempt staged, so the re-stage from the
                //      aggregate's sets starts from a clean tracker.
                //   2. The INSERT flush runs with acceptAllChangesOnSuccess:false, so a flushed-then-rolled-back
                //      attempt NEVER mutates the staged instances: EF keeps the DB-assigned identity keys in the
                //      tracker sidecar and only writes them onto the entities at AcceptAllChanges(), which runs
                //      strictly AFTER the commit succeeds. PendingRecipeAppends therefore still carry default
                //      keys on every attempt.
                // The DELETE flush deliberately KEEPS accept-on-success: deleted rows receive no store-generated
                // values, and its entries MUST leave the tracker before the insert flush, otherwise the second
                // flush would re-emit the DELETE statements against the already-deleted rows.
                ctx.ChangeTracker.Clear();

                // BATCH 1 — the staged removals (the compensating-delete path). Delete-by-key: Recipe carries
                // no rowversion token, so a row a competing writer already removed surfaces as a
                // DbUpdateConcurrencyException ("expected 1 row affected, 0") caught below.
                if (root.PendingRecipeRemovals.Count > 0)
                {
                    ctx.Set<Recipe>().RemoveRange(root.PendingRecipeRemovals);
                    await ctx.SaveChangesAsync(ct).ConfigureAwait(false);
                }

                // BATCH 2 — the staged appends. Acceptance is DEFERRED (C4, #126): the commit below can still
                // fail transiently, and only an unaccepted flush leaves the staged instances pristine for the
                // strategy's retry (see the contract comment above).
                if (root.PendingRecipeAppends.Count > 0)
                {
                    ctx.Set<Recipe>().AddRange(root.PendingRecipeAppends);
                }

                await ctx.SaveChangesAsync(acceptAllChangesOnSuccess: false, ct).ConfigureAwait(false);

                await transaction.CommitAsync(ct).ConfigureAwait(false);

                // The commit is durable — NOW accept: the tracker settles and EF writes the store-generated
                // identity keys onto the appended recipe instances (the caller returns these to the saga).
                ctx.ChangeTracker.AcceptAllChanges();
            }).ConfigureAwait(false);

            // The commit is durable — clear the staged sets so a repeated SaveAsync on the same root cannot
            // double-insert/double-delete (clear-after-save; the loaded fold stays, refreshed by the next load).
            root.ClearStagedRecipeChanges();

            // #116: invalidate the read caches of the entity type the transaction touched (Recipe only — the
            // Product root row is never mutated by this save). Best-effort: a failure is logged and never fails
            // the committed write.
            await CacheWriteInvalidation.InvalidateTypesAsync(
                this.cacheService,
                this.logger,
                nameof(this.SaveAsync),
                nameof(Recipe)).ConfigureAwait(false);

            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // A competing writer deleted/changed a staged-removal row first: the delete predicate matched zero
            // rows. Surface as a Result failure; the transaction disposes/rolls back uncommitted.
            this.logger.LogError(
                ex, "ProductAggregateRepository: optimistic-concurrency conflict saving recipes for product {ProductId}.", root.ProductId.Value);
            root.ClearStagedRecipeChanges();
            return Result.WithFailure(ConcurrencyConflict);
        }
        catch (DbUpdateException ex)
        {
            this.logger.LogError(
                ex, "ProductAggregateRepository: persistence failure saving recipes for product {ProductId}.", root.ProductId.Value);
            root.ClearStagedRecipeChanges();
            return Result.WithFailure(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex, "ProductAggregateRepository: unexpected failure saving recipes for product {ProductId}.", root.ProductId.Value);
            root.ClearStagedRecipeChanges();
            return Result.WithFailure(ex.Message);
        }
    }

    private bool InvalidContext(IIndTraceDbContext? context)
    {
        if (context is null)
        {
            this.logger.LogError("ProductAggregateRepository: database context is null.");
            return true;
        }

        return false;
    }
}
