// <copyright file="ProductRoutingRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.BarCodes.Services;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Routing;
using IndTrace.Persistence.Caching;
using IndTrace.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IndTrace.Persistence.Repositories;

/// <summary>
/// Operation-scoped unit of work for the <see cref="ProductRouting"/> aggregate (#41). Each call obtains one
/// pooled <see cref="IIndTraceDbContext"/> from the factory, uses it, and disposes it inside the call — no
/// scoped state is captured, so it is safe to invoke from a singleton PLC worker (identical to how
/// <see cref="Repository{T}"/> uses the factory).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SaveAsync"/> realises the whole-route replace as the §1.2 <strong>two-flush transaction</strong>
/// of the build-ready design: one explicit local transaction on a single connection, a delete batch
/// (<see cref="ProductRouting.DeletedNodes"/>/<see cref="ProductRouting.DeletedEdges"/>) flushed first to
/// free the <c>UNIQUE(ProductId, MachineId)</c> key, then an insert batch
/// (<see cref="ProductRouting.PendingNodes"/>/<see cref="ProductRouting.PendingEdges"/>). SQL Server has no
/// deferrable constraints, so deleting before inserting is what makes an overlapping-machine edit
/// collision-free; wrapping both flushes in one transaction is what makes the replace atomic.
/// </para>
/// <para>
/// The delete batch carries the <c>rowversion</c> concurrency predicate for every loaded row, so a
/// competing writer (another author, or a C2-style migration) that committed first trips a
/// <see cref="DbUpdateConcurrencyException"/>. That, and any other infrastructure failure, is caught and
/// surfaced as a <see cref="Result"/> failure — this repository NEVER throws across the boundary.
/// </para>
/// </remarks>
public sealed class ProductRoutingRepository : IAggregateRepository<ProductRouting>
{
    private const string ContextInactive = "Database context is not active.";
    private const string ConcurrencyConflict =
        "The product routing was modified by another author or a migration; reload and retry.";

    private readonly IIndTraceDbContextFactory contextFactory;
    private readonly ILogger<ProductRoutingRepository> logger;

    // #116: optional on purpose (see ctor docs) — null means "host registered no cache", never an error.
    private readonly ICacheService? cacheService;

    // #219: optional on purpose (see ctor docs) — null means "host registered no graph cache", never an error.
    private readonly IProductionGraphCache? graphCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRoutingRepository"/> class.
    /// </summary>
    /// <param name="contextFactory">The pooled database context factory (one context per operation).</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cacheService">
    /// #116: OPTIONAL write-side cache invalidation seam (explicitly nullable, mirroring the optional
    /// <c>toggleOptions</c> on <see cref="ReadOnlyRepository{T}"/>). When the composition root registers an
    /// <see cref="ICacheService"/> (production hosts do), DI injects it and a successful
    /// <see cref="SaveAsync"/> invalidates the read caches of the entity types the whole-route replace
    /// touched (RoutingNodeRow, WorkFlow). Roots without a cache (test hosts) resolve <see langword="null"/>
    /// and skip invalidation — there is nothing cached to invalidate.
    /// </param>
    /// <param name="graphCache">
    /// #219: OPTIONAL per-product routing-graph invalidation seam (explicitly nullable, mirroring
    /// <paramref name="cacheService"/>). When the composition root registers an
    /// <see cref="IProductionGraphCache"/> (production hosts do, as a singleton), DI injects it and a
    /// successful <see cref="SaveAsync"/> calls <see cref="IProductionGraphCache.Invalidate(int)"/> for the
    /// edited product — the #83 cache-doc contract — so a stale topology can never validate an arrival
    /// against outdated routing. Roots without a graph cache (test hosts) resolve <see langword="null"/>
    /// and skip invalidation — there is nothing cached to invalidate.
    /// </param>
    public ProductRoutingRepository(
        IIndTraceDbContextFactory contextFactory,
        ILogger<ProductRoutingRepository> logger,
        ICacheService? cacheService = null,
        IProductionGraphCache? graphCache = null)
    {
        this.contextFactory = contextFactory;
        this.logger = logger;
        this.cacheService = cacheService;
        this.graphCache = graphCache;
    }

    /// <summary>
    /// Loads the whole routing aggregate for a product (tracked, so the loaded child rows carry their
    /// <c>rowversion</c> originals). <see cref="ProductRouting"/> is always a <see cref="AggregateLoadOptions.Full"/>
    /// load — the machine window is for the #40 BarCode aggregate and is ignored here.
    /// </summary>
    /// <param name="id">The product id whose routing is loaded (the aggregate identity).</param>
    /// <param name="options">The load options; must be non-null (ProductRouting always loads Full).</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result{T}"/> wrapping the aggregate (empty = "no route yet"), or a failure.</returns>
    public async Task<Result<ProductRouting>> LoadAsync(int id, AggregateLoadOptions options, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return Result<ProductRouting>.WithFailure("Operation was canceled.");
        }

        if (options is null)
        {
            return Result<ProductRouting>.WithFailure("The aggregate load options must not be null.");
        }

        try
        {
            await using var ctx = await this.contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            if (this.InvalidContext(ctx))
            {
                return Result<ProductRouting>.WithFailure(ContextInactive);
            }

            var nodes = await ctx.Set<RoutingNodeRow>()
                .AsTracking()
                .Where(n => n.ProductId == id)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var edges = await ctx.Set<WorkFlow>()
                .AsTracking()
                .Where(e => e.ProductId == id)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return ProductRouting.FromPersisted(id, nodes, edges);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "ProductRoutingRepository: error loading routing for product {ProductId}.", id);
            return Result<ProductRouting>.WithFailure(ex.Message);
        }
    }

    /// <summary>
    /// Persists a whole-route replace in ONE explicit transaction: delete batch first (frees the unique key
    /// and carries the <c>rowversion</c> concurrency predicate), then insert batch, then commit. A concurrency
    /// conflict or any other infrastructure failure is caught and returned as a <see cref="Result"/> failure;
    /// the transaction rolls back so the prior route stays byte-intact. Never throws across the boundary.
    /// </summary>
    /// <param name="root">The aggregate whose staged delete/insert sets are persisted.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the reason (including a concurrency conflict).</returns>
    public async Task<Result> SaveAsync(ProductRouting root, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        if (root is null)
        {
            return Result.WithFailure("The product routing aggregate must not be null.");
        }

        try
        {
            await using var ctx = await this.contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            if (this.InvalidContext(ctx))
            {
                return Result.WithFailure(ContextInactive);
            }

            // The context is configured with a retrying execution strategy (EnableRetryOnFailure), which forbids
            // a user-initiated BeginTransaction unless it is wrapped in the strategy's retriable unit. Follow the
            // repo convention (IndTraceDbContext.SaveChangesAsync(tableName)): run the whole
            // two-flush transaction inside CreateExecutionStrategy().ExecuteAsync so it is a single retriable unit.
            // A concurrency/FK failure is non-transient, so it is NOT retried — it propagates out to the catch
            // blocks below and is surfaced as a Result failure (never thrown across the boundary).
            var strategy = ctx.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // One explicit local transaction on this single connection — no ambient TransactionScope, no MSDTC.
                // Production ALWAYS uses the real transaction (the atomicity mechanism proven on real SQL); the EF
                // InMemory provider returns a no-op transaction, so this path is exercised functionally in tests.
                await using var transaction = await ctx.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

                // F5 (#117) + C4 (#126) RETRY IDEMPOTENCY CONTRACT: this lambda is the strategy's retriable
                // unit and may re-execute after a transient fault — including a fault AFTER a successful flush
                // (e.g. on the commit), where the transaction rolls the rows back but nothing un-runs the
                // flush's effect on the tracker. Two halves keep every attempt pristine:
                //   1. ChangeTracker.Clear() below drops whatever the failed attempt staged (Deleted/Added
                //      entries plus any store-generated sidecar values a flushed attempt recorded), so the
                //      re-stage from the aggregate's sets starts from a clean tracker.
                //   2. The INSERT flush (BATCH 2) runs with acceptAllChangesOnSuccess:false, so a
                //      flushed-then-rolled-back attempt NEVER mutates the staged instances: EF keeps the
                //      DB-assigned identity keys and rowversions in the tracker sidecar and only writes them
                //      onto the entities at AcceptAllChanges(), which runs strictly AFTER the commit succeeds.
                //      PendingNodes/PendingEdges therefore still carry default keys on every attempt.
                //      (Verified on EF Core 10: with the default accept-on-success flush, a rolled-back
                //      attempt leaves the assigned identity keys on the CLR objects and the retry's re-insert
                //      is rejected as an explicit identity insert.)
                // The DELETE flush (BATCH 1) deliberately KEEPS accept-on-success: deleted rows receive no
                // store-generated values (the instances are never mutated by a delete flush — re-staging them
                // from root.DeletedNodes/DeletedEdges per attempt is always pristine), and BATCH 1's entries
                // MUST leave the tracker before BATCH 2 flushes, otherwise the second flush would re-emit the
                // DELETE statements and trip their rowversion predicate on the already-deleted rows.
                ctx.ChangeTracker.Clear();

                // BATCH 1 — delete the loaded (old) rows first. This frees UNIQUE(ProductId, MachineId) within the
                // transaction's view so an overlapping-machine re-insert in BATCH 2 cannot collide, and it emits the
                // rowversion concurrency predicate (DELETE ... WHERE PK = @id AND RowVersion = @original) per row.
                ctx.Set<RoutingNodeRow>().RemoveRange(root.DeletedNodes);
                ctx.Set<WorkFlow>().RemoveRange(root.DeletedEdges);
                await ctx.SaveChangesAsync(ct).ConfigureAwait(false);

                // BATCH 2 — insert the staged (new) rows. Acceptance is DEFERRED (C4, #126): the commit below
                // can still fail transiently, and only an unaccepted flush leaves the staged instances
                // pristine for the strategy's retry (see the contract comment above).
                ctx.Set<RoutingNodeRow>().AddRange(root.PendingNodes);
                ctx.Set<WorkFlow>().AddRange(root.PendingEdges);
                await ctx.SaveChangesAsync(acceptAllChangesOnSuccess: false, ct).ConfigureAwait(false);

                await transaction.CommitAsync(ct).ConfigureAwait(false);

                // The commit is durable — NOW accept: the tracker settles and EF writes the store-generated
                // values (identity keys, rowversions) onto the inserted node/edge instances.
                ctx.ChangeTracker.AcceptAllChanges();
            }).ConfigureAwait(false);

            // #116: the commit is durable — invalidate the read caches of both routing entity types the
            // whole-route replace touched (RoutingNodeRow nodes deleted+inserted, WorkFlow edges likewise).
            // Best-effort: a failure is logged and never fails the committed write.
            await CacheWriteInvalidation.InvalidateTypesAsync(
                this.cacheService,
                this.logger,
                nameof(this.SaveAsync),
                nameof(RoutingNodeRow),
                nameof(WorkFlow)).ConfigureAwait(false);

            // #219: the #83 graph-cache contract — a committed whole-route replace drops the product's cached
            // ProductionGraph IN THIS PROCESS, so this host's next arrival validation rebuilds from the new
            // rows without even a version probe. This in-process Invalidate is now only an immediate FAST PATH:
            // cross-process staleness is closed by the #224 version probe — every cached entry carries the max
            // rowversion it was built at, and consumers (BarCodeResult.FetchWorkflowsByProductIdAsync) probe
            // the current version before serving, so OTHER hosts (notably the Communications gateway, when
            // routes are authored in Monitor) detect this commit's rowversion bump on their next fetch and
            // rebuild — no restart, no invalidation transport. Best-effort like the tag invalidation above:
            // never fails the committed write.
            this.InvalidateGraphCache(root.ProductId);

            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // A competing author or migration deleted/changed the loaded rows first: the delete predicate
            // matched zero rows. Surface as a Result failure; the transaction disposes/rolls back uncommitted.
            this.logger.LogError(
                ex, "ProductRoutingRepository: optimistic-concurrency conflict saving routing for product {ProductId}.", root.ProductId);
            return Result.WithFailure(ConcurrencyConflict);
        }
        catch (DbUpdateException ex)
        {
            this.logger.LogError(
                ex, "ProductRoutingRepository: persistence failure saving routing for product {ProductId}.", root.ProductId);
            return Result.WithFailure(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex, "ProductRoutingRepository: unexpected failure saving routing for product {ProductId}.", root.ProductId);
            return Result.WithFailure(ex.Message);
        }
    }

    /// <summary>
    /// #219: drops the product's cached routing graph after a durable commit. A <see langword="null"/>
    /// <see cref="graphCache"/> (host registered no graph cache) is a no-op; a throwing implementation is
    /// logged and swallowed — a failed invalidation must never turn a committed write into a failure.
    /// </summary>
    /// <param name="productId">The product whose cached routing graph is dropped.</param>
    private void InvalidateGraphCache(int productId)
    {
        if (this.graphCache is null)
        {
            return;
        }

        try
        {
            this.graphCache.Invalidate(productId);
            this.logger.LogDebug(
                "ProductRoutingRepository: invalidated the cached routing graph for product {ProductId} after a committed replace.", productId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex, "ProductRoutingRepository: graph-cache invalidation failed for product {ProductId}; the committed write stands.", productId);
        }
    }

    private bool InvalidContext(IIndTraceDbContext? context)
    {
        if (context is null)
        {
            this.logger.LogError("ProductRoutingRepository: database context is null.");
            return true;
        }

        return false;
    }
}
