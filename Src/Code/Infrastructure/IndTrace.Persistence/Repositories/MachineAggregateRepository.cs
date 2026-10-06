// <copyright file="MachineAggregateRepository.cs" company="Exxerpro Solutions SA de CV">
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
/// #95 Phase 2 Slice C: operation-scoped unit of work for the <see cref="Machine"/> aggregate
/// (Machine -&gt; MachinePlc + Setting — the machine's configuration cluster). Each call obtains one pooled
/// <see cref="IIndTraceDbContext"/> from the factory, uses it, and disposes it inside the call — no scoped
/// state is captured, so it is safe to invoke from a singleton worker (identical to how
/// <see cref="Repository{T}"/>, <see cref="ProductRoutingRepository"/>, <see cref="BarCodeAggregateRepository"/>
/// and <see cref="ProductAggregateRepository"/> use the factory). Consumes the shared
/// <see cref="IAggregateRepository{TRoot}"/> contract.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SaveAsync"/> persists the root's staged member changes (the
/// <see cref="Machine.PendingMachinePlcAppends"/>/<see cref="Machine.PendingSettingAppends"/> insert sets
/// attached <c>Added</c> and the <see cref="Machine.PendingMachinePlcUpdates"/>/
/// <see cref="Machine.PendingSettingUpdates"/> update sets attached <c>Modified</c>) in ONE explicit local
/// transaction with ONE flush, wrapped in the context's retrying execution strategy — the
/// <see cref="BarCodeAggregateRepository"/> single-flush update+append shape. The <see cref="Machine"/> root
/// row itself is NEVER mutated by this save (members carry the scalar <c>MachineId</c>; Machine, MachinePlc
/// and Setting have no <c>rowversion</c> token — deliberately no schema change), so no root-row attach or
/// concurrency predicate exists.
/// </para>
/// <para>
/// A <see cref="DbUpdateConcurrencyException"/> (a staged update whose row a competing writer already
/// deleted/changed) and any other infrastructure failure are caught and surfaced as a <see cref="Result"/>
/// failure — this repository NEVER throws across the boundary. The staged sets are CONSUMED by every save
/// attempt (the ratified Slice B <c>914adfbff</c> contract, identical to
/// <see cref="ProductAggregateRepository"/>): after a durable commit they are cleared
/// (<see cref="Machine.ClearStagedMachineChanges"/>) so a repeated save cannot double-apply them, and after
/// a FAILED attempt they are likewise discarded so a later save of the same root cannot silently double-apply
/// a stale batch — the caller must re-load and re-stage to retry. The per-attempt
/// <c>ChangeTracker.Clear()</c> inside the retriable unit drops any staging residue a failed attempt left in
/// the tracker.
/// </para>
/// </remarks>
public sealed class MachineAggregateRepository : IAggregateRepository<Machine>
{
    private const string ContextInactive = "Database context is not active.";
    private const string ConcurrencyConflict =
        "The machine's members were modified by another writer; reload and retry.";

    private readonly IIndTraceDbContextFactory contextFactory;
    private readonly ILogger<MachineAggregateRepository> logger;

    // #116: optional on purpose (see ctor docs) — null means "host registered no cache", never an error.
    private readonly ICacheService? cacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineAggregateRepository"/> class.
    /// </summary>
    /// <param name="contextFactory">The pooled database context factory (one context per operation).</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cacheService">
    /// #116: OPTIONAL write-side cache invalidation seam (explicitly nullable, mirroring
    /// <see cref="ProductAggregateRepository"/>). When the composition root registers an
    /// <see cref="ICacheService"/> (production hosts do), DI injects it and a successful
    /// <see cref="SaveAsync"/> invalidates the read caches of the entity types the aggregate write touched
    /// (MachinePlc and Setting — the root row is never mutated by this save). Roots without a cache (test
    /// hosts) resolve <see langword="null"/> and skip invalidation — there is nothing cached to invalidate.
    /// </param>
    public MachineAggregateRepository(
        IIndTraceDbContextFactory contextFactory,
        ILogger<MachineAggregateRepository> logger,
        ICacheService? cacheService = null)
    {
        this.contextFactory = contextFactory;
        this.logger = logger;
        this.cacheService = cacheService;
    }

    /// <summary>
    /// Loads the machine aggregate: the <see cref="Machine"/> root row plus ALL of its <see cref="MachinePlc"/>
    /// and <see cref="Setting"/> member rows (matched on the typed <c>MachineId</c> key), folded onto the root
    /// via <see cref="Machine.AttachLoadedMachinePlcs"/>/<see cref="Machine.AttachLoadedSettings"/>. Always a
    /// full load — the machine window is for the #40 BarCode aggregate and is ignored here.
    /// </summary>
    /// <param name="id">The machine id whose aggregate is loaded (the aggregate identity).</param>
    /// <param name="options">The load options; must be non-null (Machine always loads Full).</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result{T}"/> wrapping the aggregate (root + loaded members), or a failure.</returns>
    public async Task<Result<Machine>> LoadAsync(int id, AggregateLoadOptions options, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return Result<Machine>.WithFailure("Operation was canceled.");
        }

        if (options is null)
        {
            return Result<Machine>.WithFailure("The aggregate load options must not be null.");
        }

        try
        {
            await using var ctx = await this.contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            if (this.InvalidContext(ctx))
            {
                return Result<Machine>.WithFailure(ContextInactive);
            }

            var machineKey = new MachineId(id);
            var machine = await ctx.Set<Machine>()
                .AsTracking()
                .FirstOrDefaultAsync(m => m.MachineId == machineKey, ct)
                .ConfigureAwait(false);

            if (machine is null)
            {
                return Result<Machine>.WithFailure($"Machine {id} was not found.");
            }

            var machinePlcs = await ctx.Set<MachinePlc>()
                .AsTracking()
                .Where(p => p.MachineId == machineKey)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var settings = await ctx.Set<Setting>()
                .AsTracking()
                .Where(s => s.MachineId == machineKey)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            machine.AttachLoadedMachinePlcs(machinePlcs);
            machine.AttachLoadedSettings(settings);

            return Result<Machine>.Success(machine);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "MachineAggregateRepository: error loading aggregate for machine {MachineId}.", id);
            return Result<Machine>.WithFailure(ex.Message);
        }
    }

    /// <summary>
    /// Persists the root's staged MachinePlc/Setting changes in ONE explicit transaction with ONE flush: the
    /// staged appends attached <c>Added</c>, the staged updates attached <c>Modified</c>, then commit. The
    /// <see cref="Machine"/> root row is NOT written (no root mutation belongs to this save; Machine carries
    /// no concurrency token — no schema change). A concurrency conflict (a staged update whose row vanished)
    /// or any other infrastructure failure is caught and returned as a <see cref="Result"/> failure; the
    /// transaction rolls back so no partial write leaks. Never throws across the boundary. The staged sets
    /// are CONSUMED by the attempt: after a durable commit they are cleared so a repeated save is a no-op,
    /// and after a failed attempt they are discarded so a later save of the same root cannot double-apply a
    /// stale batch — the caller must re-load and re-stage to retry (the adversarial-review PR #170 contract,
    /// identical to <see cref="ProductAggregateRepository"/>).
    /// </summary>
    /// <param name="root">The aggregate whose staged member appends/updates are persisted.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the reason (including a concurrency conflict).</returns>
    public async Task<Result> SaveAsync(Machine root, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        if (root is null)
        {
            return Result.WithFailure("The machine aggregate must not be null.");
        }

        if (root.PendingMachinePlcAppends.Count == 0
            && root.PendingMachinePlcUpdates.Count == 0
            && root.PendingSettingAppends.Count == 0
            && root.PendingSettingUpdates.Count == 0)
        {
            // Nothing staged — the DB is already correct. An idempotent no-op success (nothing was written,
            // so nothing is cache-invalidated either), mirroring the Product/BarCode idempotent no-op precedent.
            return Result.Success();
        }

        try
        {
            await using var ctx = await this.contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            if (this.InvalidContext(ctx))
            {
                root.ClearStagedMachineChanges();
                return Result.WithFailure(ContextInactive);
            }

            // The context is configured with a retrying execution strategy (EnableRetryOnFailure), which forbids
            // a user-initiated BeginTransaction unless it is wrapped in the strategy's retriable unit. Follow the
            // BarCodeAggregateRepository convention: run the whole single-flush transaction inside
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

                // F5 (#117) + C4 (#126) RETRY IDEMPOTENCY CONTRACT (the ProductAggregateRepository shape): this
                // lambda may re-execute after a transient fault — including a fault AFTER a successful flush.
                //   1. ChangeTracker.Clear() drops whatever the failed attempt staged, so the re-stage from the
                //      aggregate's sets starts from a clean tracker (the 914adfbff tracker-hygiene semantics).
                //   2. The flush runs with acceptAllChangesOnSuccess:false, so a flushed-then-rolled-back
                //      attempt NEVER mutates the staged instances: EF keeps the DB-assigned identity keys in the
                //      tracker sidecar and only writes them onto the entities at AcceptAllChanges(), which runs
                //      strictly AFTER the commit succeeds. PendingSettingAppends therefore still carry
                //      SettingId 0 on every attempt.
                ctx.ChangeTracker.Clear();

                // The staged appends — attached Added (identity assignment is deferred to AcceptAllChanges).
                if (root.PendingMachinePlcAppends.Count > 0)
                {
                    ctx.Set<MachinePlc>().AddRange(root.PendingMachinePlcAppends);
                }

                if (root.PendingSettingAppends.Count > 0)
                {
                    ctx.Set<Setting>().AddRange(root.PendingSettingAppends);
                }

                // The staged updates — attached Modified (in-place update by key; MachinePlc/Setting carry no
                // rowversion, so a row a competing writer already removed surfaces as a
                // DbUpdateConcurrencyException "expected 1 row affected, 0" caught below). The staging seams
                // guarantee an update never shares a key with a staged append, so the tracker never sees the
                // same entity twice.
                foreach (var machinePlc in root.PendingMachinePlcUpdates)
                {
                    var entry = ctx.Set<MachinePlc>().Attach(machinePlc);
                    entry.State = EntityState.Modified;
                }

                foreach (var setting in root.PendingSettingUpdates)
                {
                    var entry = ctx.Set<Setting>().Attach(setting);
                    entry.State = EntityState.Modified;
                }

                // ONE flush — an in-place update + append, no delete batch. Acceptance is DEFERRED (C4, #126):
                // the commit below can still fail transiently, and only an unaccepted flush leaves the staged
                // instances pristine for the strategy's retry (see the contract comment above).
                await ctx.SaveChangesAsync(acceptAllChangesOnSuccess: false, ct).ConfigureAwait(false);

                await transaction.CommitAsync(ct).ConfigureAwait(false);

                // The commit is durable — NOW accept: the tracker settles and EF writes the store-generated
                // identity keys onto the appended Setting instances (the caller reads these back).
                ctx.ChangeTracker.AcceptAllChanges();
            }).ConfigureAwait(false);

            // The commit is durable — clear the staged sets so a repeated SaveAsync on the same root cannot
            // double-insert/double-update (clear-after-save; the loaded folds stay, refreshed by the next load).
            root.ClearStagedMachineChanges();

            // #116: invalidate the read caches of the entity types the transaction touched (MachinePlc and
            // Setting only — the Machine root row is never mutated by this save). Best-effort: a failure is
            // logged and never fails the committed write.
            await CacheWriteInvalidation.InvalidateTypesAsync(
                this.cacheService,
                this.logger,
                nameof(this.SaveAsync),
                nameof(MachinePlc),
                nameof(Setting)).ConfigureAwait(false);

            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // A competing writer deleted/changed a staged-update row first: the update predicate matched zero
            // rows. Surface as a Result failure; the transaction disposes/rolls back uncommitted. The staged
            // sets are consumed by the failed attempt (the PR #170 contract) — re-load and re-stage to retry.
            this.logger.LogError(
                ex, "MachineAggregateRepository: optimistic-concurrency conflict saving members for machine {MachineId}.", root.MachineId.Value);
            root.ClearStagedMachineChanges();
            return Result.WithFailure(ConcurrencyConflict);
        }
        catch (DbUpdateException ex)
        {
            this.logger.LogError(
                ex, "MachineAggregateRepository: persistence failure saving members for machine {MachineId}.", root.MachineId.Value);
            root.ClearStagedMachineChanges();
            return Result.WithFailure(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex, "MachineAggregateRepository: unexpected failure saving members for machine {MachineId}.", root.MachineId.Value);
            root.ClearStagedMachineChanges();
            return Result.WithFailure(ex.Message);
        }
    }

    private bool InvalidContext(IIndTraceDbContext? context)
    {
        if (context is null)
        {
            this.logger.LogError("MachineAggregateRepository: database context is null.");
            return true;
        }

        return false;
    }
}
