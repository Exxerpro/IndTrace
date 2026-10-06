// <copyright file="BarCodeAggregateRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using System.Text;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Caching;
using IndTrace.Persistence.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IndTrace.Persistence.Repositories;

/// <summary>
/// #40 Chunk 40-C: operation-scoped unit of work for the <see cref="BarCode"/> aggregate (BarCode -&gt; Cycle
/// -&gt; Register + the idempotency <see cref="CycleCompletion"/> marker). Each call obtains one pooled
/// <see cref="IIndTraceDbContext"/> from the factory, uses it, and disposes it inside the call — no scoped
/// state is captured, so it is safe to invoke from a singleton PLC worker (identical to how
/// <see cref="Repository{T}"/> and <see cref="ProductRoutingRepository"/> use the factory). Consumes the
/// #41 shared <see cref="IAggregateRepository{TRoot}"/> contract; the body differs from
/// <see cref="ProductRoutingRepository"/> because #40 is an in-place UPDATE + append (a single flush), not a
/// whole-route delete-then-insert replace.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="LoadAsync"/> is a <strong>windowed</strong> load (M2): it materialises the barcode plus the
/// COMPLETE set of its cycles for the machine window carried by <see cref="AggregateLoadOptions.MachineWindow"/>
/// (a <c>WHERE MachineId IN (...)</c> filter with <strong>no paging/Top</strong>). Undercounting the current
/// machine's cycles would let a part slip past the PO-ratified rework cap and ship non-conforming, so the exact
/// multiset the cap enumerates is loaded in full and attached to the aggregate for the caller to pass as the
/// cap window.
/// </para>
/// <para>
/// <see cref="SaveAsync"/> applies the staged changes in ONE explicit transaction (the barcode + applied cycle
/// updated in place carrying their <c>rowversion</c> originals, the appended registers, and the completion
/// marker inserted under <c>UNIQUE(CycleId)</c>). The whole transaction is wrapped in the context's
/// <see cref="Microsoft.EntityFrameworkCore.Storage.IExecutionStrategy"/> because the context enables a
/// retrying execution strategy (<c>EnableRetryOnFailure</c>), which forbids a user-initiated
/// <c>BeginTransaction</c> unless it is inside the strategy's retriable unit (#41 Chunk D proved the naive
/// version non-functional on real SQL — InMemory hid it). A concurrency conflict (rowversion), a
/// duplicate-completion re-send (idempotency), and any other infrastructure failure are all caught and surfaced
/// as a <see cref="Result"/> — this repository NEVER throws across the boundary.
/// </para>
/// <para>
/// #95 Phase 2 Slice E: the same single-flush transaction also carries the root's staged MEMBER writes — the
/// <see cref="BarCode.PendingNewCycles"/> INSERT batch (the CycleCreator initial-INSERT migration) and the
/// <see cref="BarCode.PendingCycleUpdates"/> in-place status updates (the CancelCycle migration). A save may be
/// member-only (no applied completion): the barcode ROOT row is then deliberately NOT written (the retired raw
/// writers never wrote it either). The Slice E staged sets are consumed on EVERY save attempt — success AND
/// failure (the ratified PR #170 contract) — via <see cref="BarCode.ClearStagedCycleChanges"/>.
/// </para>
/// </remarks>
public sealed class BarCodeAggregateRepository : IAggregateRepository<BarCode>
{
    private const string ContextInactive = "Database context is not active.";

    private const string ConcurrencyConflict =
        "The barcode/cycle was modified concurrently; reload and retry.";

    // #40 B2-idempotency: the ONE constraint whose violation is an idempotent PLC re-send, not an error. The
    // detection matches THIS name specifically so a DIFFERENT unique violation is surfaced as a failure, never
    // swallowed as success. The name is authored identically by the EF config (CycleCompletionConfiguration,
    // HasDatabaseName) and the hand-authored 01_forward.sql, so the physical UNIQUE constraint and the index
    // share it.
    private const string CompletionUniqueConstraint = "UX.IndTraceData.CycleCompletion.CycleId";

    // SQL Server unique-constraint / unique-index violation error numbers.
    private const int UniqueConstraintViolation = 2627;
    private const int DuplicateKeyRowViolation = 2601;

    private readonly IIndTraceDbContextFactory contextFactory;
    private readonly ILogger<BarCodeAggregateRepository> logger;

    // #116: optional on purpose (see ctor docs) — null means "host registered no cache", never an error.
    private readonly ICacheService? cacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeAggregateRepository"/> class.
    /// </summary>
    /// <param name="contextFactory">The pooled database context factory (one context per operation).</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cacheService">
    /// #116: OPTIONAL write-side cache invalidation seam (explicitly nullable, mirroring the optional
    /// <c>toggleOptions</c> on <see cref="ReadOnlyRepository{T}"/>). When the composition root registers an
    /// <see cref="ICacheService"/> (production hosts do), DI injects it and a successful
    /// <see cref="SaveAsync"/> invalidates the read caches of every entity type the aggregate write touched
    /// (BarCode, Cycle, Register, CycleCompletion). Roots without a cache (test hosts) resolve
    /// <see langword="null"/> and skip invalidation — there is nothing cached to invalidate.
    /// </param>
    public BarCodeAggregateRepository(
        IIndTraceDbContextFactory contextFactory,
        ILogger<BarCodeAggregateRepository> logger,
        ICacheService? cacheService = null)
    {
        this.contextFactory = contextFactory;
        this.logger = logger;
        this.cacheService = cacheService;
    }

    /// <summary>
    /// Loads the barcode aggregate windowed to the machines in <paramref name="options"/> (tracked, so the
    /// loaded rows carry their <c>rowversion</c> originals). The barcode and the COMPLETE set of its cycles for
    /// <c>(BarCodeId, MachineId IN window)</c> are materialised and attached to the aggregate (M2 — no paging).
    /// A <see langword="null"/> window (<see cref="AggregateLoadOptions.Full"/>) loads ALL of the barcode's
    /// cycles.
    /// </summary>
    /// <param name="id">The barcode id whose aggregate is loaded (the aggregate identity).</param>
    /// <param name="options">The load options carrying the machine window; must be non-null.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result{T}"/> wrapping the aggregate (barcode + windowed cycles), or a failure.</returns>
    public async Task<Result<BarCode>> LoadAsync(int id, AggregateLoadOptions options, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return Result<BarCode>.WithFailure("Operation was canceled.");
        }

        if (options is null)
        {
            return Result<BarCode>.WithFailure("The aggregate load options must not be null.");
        }

        try
        {
            await using var ctx = await this.contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            if (this.InvalidContext(ctx))
            {
                return Result<BarCode>.WithFailure(ContextInactive);
            }

            var barCodeKey = new BarCodeId(id);
            var barCode = await ctx.Set<BarCode>()
                .AsTracking()
                .FirstOrDefaultAsync(b => b.BarCodeId == barCodeKey, ct)
                .ConfigureAwait(false);

            if (barCode is null)
            {
                return Result<BarCode>.WithFailure($"BarCode {id} was not found.");
            }

            // Windowed cycle load (M2): the COMPLETE multiset for (BarCodeId, machine window). No paging/Top —
            // the rework cap counts every one of the current machine's cycles, so an undercount would ship a
            // non-conforming part. A null window (Full) loads all of the barcode's cycles.
            var cyclesQuery = ctx.Set<Cycle>()
                .AsTracking()
                .Where(c => c.BarCodeId == barCodeKey);

            if (options.MachineWindow is not null)
            {
                var windowKeys = options.MachineWindow.Select(m => new MachineId(m)).ToList();
                cyclesQuery = cyclesQuery.Where(c => windowKeys.Contains(c.MachineId));
            }

            var cycles = await cyclesQuery
                .ToListAsync(ct)
                .ConfigureAwait(false);

            barCode.AttachLoadedCycles(cycles);

            return Result<BarCode>.Success(barCode);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "BarCodeAggregateRepository: error loading aggregate for barcode {BarCodeId}.", id);
            return Result<BarCode>.WithFailure(ex.Message);
        }
    }

    /// <summary>
    /// Persists the staged aggregate changes in ONE explicit transaction: the barcode + applied cycle updated
    /// in place (carrying their <c>rowversion</c> concurrency predicate), the Slice E member writes (the
    /// <see cref="BarCode.PendingNewCycles"/> INSERT batch and the <see cref="BarCode.PendingCycleUpdates"/>
    /// in-place status updates, each carrying its <c>rowversion</c> original), the appended registers, and the
    /// completion marker inserted under <c>UNIQUE(CycleId)</c>, then commit. A save may carry an applied
    /// completion AND/OR new cycles AND/OR staged cycle updates — the shapes compose in the same single flush.
    /// A rowversion conflict, a duplicate-completion re-send, or any other infrastructure failure is caught
    /// and returned as a <see cref="Result"/>; the transaction rolls back so no partial write leaks. Never
    /// throws across the boundary. The Slice E staged sets are CONSUMED by the attempt (success AND failure —
    /// the ratified PR #170 contract, <see cref="BarCode.ClearStagedCycleChanges"/>): the caller must re-stage
    /// to retry.
    /// </summary>
    /// <param name="root">The aggregate whose staged cycle/registers/marker/member writes are persisted.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A success <see cref="Result"/> (including the idempotent no-op on a duplicate completion), or a failure
    /// carrying the reason (including a concurrency conflict).
    /// </returns>
    public async Task<Result> SaveAsync(BarCode root, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        if (root is null)
        {
            return Result.WithFailure("The barcode aggregate must not be null.");
        }

        // A null AppliedCycle now carries TWO distinct meanings after #81 (see BarCode.LastCompletionWasIdempotentNoOp):
        //   (a) an IDEMPOTENT PLC resend of an already-completed cycle — the #81 already-in-target-state no-op
        //       returned Success WITHOUT staging anything. There is nothing to persist because the completion
        //       already happened; the DB is already correct. Return an idempotent no-op SUCCESS (DB unchanged),
        //       matching #81's intent and the UNIQUE(CycleId)-collision path below which the pre-stage no-op replaced.
        //   (b) a GENUINE refusal (cap-refused / illegal-source / null-guard) — no coherent aggregate write exists,
        //       so this stays a FAILURE.
        // #95 Slice E: a save may now ALSO be a member-only write (staged new cycles / staged cycle-status
        // updates with NO applied completion — the CycleCreator / CancelCycle paths). Only when NOTHING at all
        // is staged does the #81 disambiguation above apply unchanged.
        // Capture into a local so the null-narrowing survives across the awaits (no null-forgiving operator).
        var appliedCycle = root.AppliedCycle;
        var hasMemberWrites = root.PendingNewCycles.Count > 0
            || root.PendingCycleUpdates.Count > 0
            || root.HasPendingStatusWrite;

        // #114 chunk A: a root without a persisted identity is a brand-new barcode — the save INSERTs the root
        // row (plus its staged new cycles) in the same single-flush transaction, so the CreateBarCode path can
        // never leave a barcode without its Started cycle (or vice versa). A new root is itself something to
        // persist, so it passes the nothing-staged guard below.
        var rootIsNew = root.BarCodeId.Value == 0;
        if (appliedCycle is null && !hasMemberWrites && !rootIsNew)
        {
            return root.LastCompletionWasIdempotentNoOp
                ? Result.Success()
                : Result.WithFailure(
                    "No staged cycle completion to save; apply an OK/NotOk cycle to the aggregate first.");
        }

        // #114 chunk A coherence guards: a completion updates persisted rows in place and a staged cycle-status
        // update requires a persisted cycle — neither shape can ride a brand-new (never-persisted) root.
        if (rootIsNew && appliedCycle is not null)
        {
            return Result.WithFailure(
                "A brand-new barcode root cannot carry an applied cycle completion; persist the barcode first.");
        }

        if (rootIsNew && root.PendingCycleUpdates.Count > 0)
        {
            root.ClearStagedCycleChanges();
            return Result.WithFailure(
                "A brand-new barcode root cannot carry staged cycle-status updates; persist the barcode first.");
        }

        var completionMarker = root.CompletionMarker;

        try
        {
            await using var ctx = await this.contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            if (this.InvalidContext(ctx))
            {
                root.ClearStagedCycleChanges();
                return Result.WithFailure(ContextInactive);
            }

            // The context is configured with a retrying execution strategy (EnableRetryOnFailure), which forbids
            // a user-initiated BeginTransaction unless it is wrapped in the strategy's retriable unit. Run the
            // whole single-flush transaction inside CreateExecutionStrategy().ExecuteAsync so it is one retriable
            // unit (mirrors ProductRoutingRepository / #41 Chunk D — the naive version is non-functional on real
            // SQL, and InMemory hides it). A concurrency/FK/unique failure is non-transient, so it is NOT
            // retried — it propagates out to the catch blocks below and is surfaced as a Result.
            var strategy = ctx.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // One explicit local transaction on this single connection — no ambient TransactionScope, no
                // MSDTC. Production ALWAYS uses the real transaction (the atomicity mechanism proven on real SQL
                // in Chunk 40-D); the EF InMemory provider returns a no-op transaction, so this path is exercised
                // functionally in tests.
                await using var transaction = await ctx.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

                // F5 (#117) + C4 (#126) RETRY IDEMPOTENCY CONTRACT: this lambda is the strategy's retriable
                // unit and may re-execute after a transient fault — including a fault AFTER a successful flush
                // (e.g. on the commit), where the transaction rolls the rows back but nothing un-runs the
                // flush's effect on the tracker. Two halves keep every attempt pristine:
                //   1. ChangeTracker.Clear() below drops whatever the failed attempt staged (entities attached
                //      Modified / Added, plus any store-generated sidecar values a flushed attempt recorded).
                //   2. The flush at the bottom runs with acceptAllChangesOnSuccess:false, so a
                //      flushed-then-rolled-back attempt NEVER mutates the aggregate instances: EF keeps the
                //      regenerated rowversions and DB-assigned identity keys in the tracker sidecar and only
                //      writes them onto the entities at AcceptAllChanges(), which runs strictly AFTER the
                //      commit succeeds. root.RowVersion / appliedCycle.RowVersion therefore still hold the
                //      LOADED originals on every attempt, and the marker/registers still carry default keys —
                //      re-stamping the concurrency originals from them below is safe. (Verified on EF Core 10:
                //      with the default accept-on-success flush, a rolled-back attempt leaves the NEW
                //      rowversion + assigned identity keys on the CLR objects, and the retry then surfaces a
                //      spurious ConcurrencyConflict with no competing writer.)
                ctx.ChangeTracker.Clear();

                // #114 chunk A: a brand-new root is INSERTed, and its staged new cycles ride the SAME flush with
                // their BarCodeId FK set to the root's TEMPORARY key (stored in the tracker sidecar, marked
                // IsTemporary) so EF associates them with the new principal and propagates the real
                // store-generated identity at insert time. With acceptAllChangesOnSuccess:false the generated
                // keys land on the CLR objects only at AcceptAllChanges() after a durable commit, keeping the
                // F5/C4 retry contract intact (a rolled-back attempt leaves default keys, so the strategy's
                // retry re-adds cleanly).
                if (rootIsNew)
                {
                    var newRootEntry = ctx.Set<BarCode>().Add(root);
                    var rootTemporaryKey = newRootEntry.Property(b => b.BarCodeId).CurrentValue;
                    foreach (var newCycle in root.PendingNewCycles)
                    {
                        var newCycleEntry = ctx.Set<Cycle>().Add(newCycle);
                        var foreignKey = newCycleEntry.Property(c => c.BarCodeId);
                        foreignKey.CurrentValue = rootTemporaryKey;
                        foreignKey.IsTemporary = true;
                    }
                }

                // The barcode root row is written ONLY on the completion path (CompleteOkCycle/CompleteNotOkCycle
                // mutate the barcode's machine/modified/flow/part state). A #95 Slice E MEMBER-ONLY save (staged
                // new cycles / cycle-status updates) deliberately leaves the root row untouched — the retired raw
                // writers (CycleCreator AddAsync, CancelCycle UpdateAsync) never wrote the barcode row either, so
                // attaching it here would add a phantom UPDATE (and a spurious rowversion bump) to those paths.
                if (appliedCycle is not null)
                {
                    // The barcode is updated in place, carrying its rowversion original as the concurrency predicate
                    // (UPDATE ... WHERE PK = @id AND RowVersion = @original). A competing writer that committed first
                    // trips a DbUpdateConcurrencyException.
                    var barEntry = ctx.Set<BarCode>().Attach(root);
                    barEntry.State = EntityState.Modified;
                    barEntry.Property(b => b.RowVersion).OriginalValue = root.RowVersion;

                    // The applied cycle is likewise updated in place with its rowversion original.
                    var cycleEntry = ctx.Set<Cycle>().Attach(appliedCycle);
                    cycleEntry.State = EntityState.Modified;
                    cycleEntry.Property(c => c.RowVersion).OriginalValue = appliedCycle.RowVersion;
                }

                // #114 chunk B: the staged barcode ROOT status write (the CreateCycles migration of the retired
                // separate BarCodeUpdater auto-commit). The root is updated in place carrying its rowversion
                // original as the concurrency predicate, INSIDE the same single-flush transaction as the staged
                // new-cycle INSERT below — so a failure of either write rolls back both (no orphan Started
                // cycle). Skipped on the completion path (the root is already attached Modified above) and on
                // the new-root path (the INSERT carries every field). Deliberately attached BEFORE the
                // new-cycle AddRange so the root UPDATE precedes the dependent INSERT in the flush.
                if (!rootIsNew && appliedCycle is null && root.HasPendingStatusWrite)
                {
                    var rootEntry = ctx.Set<BarCode>().Attach(root);
                    rootEntry.State = EntityState.Modified;
                    rootEntry.Property(b => b.RowVersion).OriginalValue = root.RowVersion;
                }

                // #95 Slice E: staged in-place cycle-status updates (the CancelCycle path). Each carries its
                // loaded rowversion as the concurrency original — a competing writer that committed first trips
                // a DbUpdateConcurrencyException, exactly like the applied-cycle update above.
                foreach (var updatedCycle in root.PendingCycleUpdates)
                {
                    var updateEntry = ctx.Set<Cycle>().Attach(updatedCycle);
                    updateEntry.State = EntityState.Modified;
                    updateEntry.Property(c => c.RowVersion).OriginalValue = updatedCycle.RowVersion;
                }

                // #95 Slice E: staged brand-new cycles (the CycleCreator initial-INSERT path). Plain AddRange so
                // the INSERT rides the same single flush; the store assigns the identity, written back onto the
                // instances only at AcceptAllChanges() after a durable commit. (#114: on the new-root path the
                // cycles were already added above, carrying the root's temporary key.)
                if (!rootIsNew && root.PendingNewCycles.Count > 0)
                {
                    ctx.Set<Cycle>().AddRange(root.PendingNewCycles);
                }

                // Append-only registers. #184: dbo.Registers is an APPEND-ONLY LEDGER table (#39) and SQL Server
                // rejects ANY MERGE against it with error 37359 — but the SQL Server provider batches >=4
                // identity-key inserts as MERGE (with OUTPUT, or OUTPUT INTO when the OUTPUT clause is disabled
                // in the mapping), so a completion staging 4+ registers could never flush. EF has no ledger-aware
                // pipeline yet (dotnet/efcore#33226), so on SQL Server the registers bypass the EF save pipeline:
                // ONE set-based plain multi-row INSERT per chunk on this SAME connection + transaction, which the
                // ledger accepts for any register count. Registers are append-only audit VALUES — nothing reads
                // the generated RegisterId back onto the instances, so no OUTPUT/read-back is needed. On the
                // non-relational InMemory provider (Aggregation.BoundedTests) raw SQL is unsupported and there is
                // no MERGE hazard, so the tracked AddRange rides the single flush as before. (Never
                // AddRangeBulkAsync — its own transaction would break the single-commit atomicity.)
                if (root.PendingRegisters.Count > 0)
                {
                    if (ctx.Database.IsSqlServer())
                    {
                        await AppendRegistersLedgerSafeAsync(ctx, root.PendingRegisters, ct).ConfigureAwait(false);
                    }
                    else
                    {
                        ctx.Set<Register>().AddRange(root.PendingRegisters);
                    }
                }

                // The idempotency completion marker (UNIQUE(CycleId)). A PLC re-send collides on this one row.
                if (completionMarker is not null)
                {
                    ctx.Set<CycleCompletion>().Add(completionMarker);
                }

                // ONE flush for the tracked changes (barcode + cycle updates, completion marker; on SQL Server
                // the register append already executed above as set-based INSERTs on this same transaction).
                // No delete batch. Acceptance is DEFERRED (C4, #126):
                // the commit below can still fail transiently, and only an unaccepted flush leaves the
                // aggregate instances pristine for the strategy's retry (see the contract comment above).
                await ctx.SaveChangesAsync(acceptAllChangesOnSuccess: false, ct).ConfigureAwait(false);

                await transaction.CommitAsync(ct).ConfigureAwait(false);

                // The commit is durable — NOW accept: the tracker settles and EF writes the store-generated
                // values (new rowversions, marker identity key — #184: registers are never tracked on SQL
                // Server, their instances intentionally keep RegisterId 0) onto the aggregate instances.
                ctx.ChangeTracker.AcceptAllChanges();
            }).ConfigureAwait(false);

            // #95 Slice E: the commit is durable — consume the staged member sets so a repeated SaveAsync on
            // the same root cannot double-insert/double-update (clear-after-attempt, the PR #170 contract).
            root.ClearStagedCycleChanges();

            // #116: the commit is durable — invalidate the read caches of EVERY entity type this single-flush
            // transaction touched (BarCode + Cycle updated in place, Registers appended, CycleCompletion
            // inserted). Best-effort: a failure is logged and never fails the committed write. The idempotent
            // no-op paths (nothing staged / duplicate re-send) deliberately do NOT invalidate — they wrote
            // nothing, and the original completion already invalidated when it committed.
            await CacheWriteInvalidation.InvalidateTypesAsync(
                this.cacheService,
                this.logger,
                nameof(this.SaveAsync),
                nameof(BarCode),
                nameof(Cycle),
                nameof(Register),
                nameof(CycleCompletion)).ConfigureAwait(false);

            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // The M2 rowversion conflict: a competing cycle-OK on the same part (or a migration) committed
            // first, so the UPDATE predicate matched zero rows. Surface as a failure; the transaction disposes /
            // rolls back uncommitted so no partial write leaks.
            this.logger.LogError(
                ex, "BarCodeAggregateRepository: optimistic-concurrency conflict saving barcode {BarCodeId}.", root.BarCodeId.Value);
            root.ClearStagedCycleChanges();
            return Result.WithFailure(ConcurrencyConflict);
        }
        catch (DbUpdateException ex) when (IsCycleCompletionDuplicate(ex))
        {
            // B2-idempotency: a PLC re-send hit the UNIQUE(CycleId) completion marker. The whole batch rolled
            // back (no double-append / double-count / double-advance); the original completion stands. Return an
            // idempotent success so the caller treats the re-send as the no-op it is. (The marker exists only on
            // the completion path, so AppliedCycle is non-null here; the null-conditional keeps the log total.)
            this.logger.LogInformation(
                ex,
                "BarCodeAggregateRepository: cycle-completion for barcode {BarCodeId} cycle {CycleId} already recorded; treating the re-send as an idempotent no-op.",
                root.BarCodeId.Value,
                appliedCycle?.CycleId.Value ?? 0);
            root.ClearStagedCycleChanges();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            this.logger.LogError(
                ex, "BarCodeAggregateRepository: persistence failure saving barcode {BarCodeId}.", root.BarCodeId.Value);
            root.ClearStagedCycleChanges();
            return Result.WithFailure(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex, "BarCodeAggregateRepository: unexpected failure saving barcode {BarCodeId}.", root.BarCodeId.Value);
            root.ClearStagedCycleChanges();
            return Result.WithFailure(ex.Message);
        }
    }

    /// <summary>
    /// #184: appends the pending registers to the append-only <c>dbo.Registers</c> LEDGER table with set-based
    /// plain parameterized <c>INSERT</c> statements on the context's ambient connection and transaction. The
    /// ledger rejects ANY <c>MERGE</c> (SQL error 37359), which the EF save pipeline emits for &gt;=4 identity-key
    /// inserts, so this path deliberately avoids EF's insert batching; registers need no identity read-back, so
    /// no <c>OUTPUT</c> clause is used. Runs INSIDE the caller's execution-strategy unit and explicit
    /// transaction — a failure throws (SqlException) and is surfaced/rolled back by the caller, and a retried
    /// attempt re-executes on a fresh transaction leaving the register instances untouched (they are never
    /// tracked, keeping the F5/C4 retry-idempotency contract trivially intact).
    /// </summary>
    /// <param name="ctx">The active context whose connection/transaction the inserts ride.</param>
    /// <param name="registers">The pending registers to append.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    private static async Task AppendRegistersLedgerSafeAsync(
        IIndTraceDbContext ctx, IReadOnlyList<Register> registers, CancellationToken ct)
    {
        // SQL Server caps a command at 2100 parameters (and a VALUES table constructor at 1000 rows); at 9
        // parameters per row the chunk must stay at or below 233 rows. Column list mirrors
        // RegistersConfiguration (HasColumnName(nameof(...))); RegisterId is IDENTITY and never supplied.
        const int chunkSize = 200;
        const int columnsPerRow = 9;
        for (var offset = 0; offset < registers.Count; offset += chunkSize)
        {
            var count = Math.Min(chunkSize, registers.Count - offset);
            var sql = new StringBuilder(
                "INSERT INTO dbo.Registers (Name, Description, MachineId, VariableId, CycleId, Value, DataType, StatusValueId, TimeStamp) VALUES ");
            var parameters = new List<object>(count * columnsPerRow);
            for (var i = 0; i < count; i++)
            {
                var p = i * columnsPerRow;
                sql.Append(i == 0 ? "(" : ", (");
                for (var c = 0; c < columnsPerRow; c++)
                {
                    sql.Append(c == 0 ? "{" : ", {")
                        .Append(CultureInfo.InvariantCulture, $"{p + c}")
                        .Append('}');
                }

                sql.Append(')');

                var register = registers[offset + i];
                parameters.Add(register.Name);
                parameters.Add(register.Description);
                parameters.Add(register.MachineId);
                parameters.Add(register.VariableId);
                parameters.Add(register.CycleId.Value);
                parameters.Add(register.Value);
                parameters.Add(register.DataType);
                parameters.Add(register.StatusValueId);
                parameters.Add(register.TimeStamp);
            }

            sql.Append(';');
            await ctx.Database.ExecuteSqlRawAsync(sql.ToString(), parameters, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Recognises the ONE unique violation that is an idempotent PLC re-send: a SQL Server unique-constraint
    /// (2627) or unique-index (2601) violation whose message names
    /// <c>UX.IndTraceData.CycleCompletion.CycleId</c> specifically. Any other unique violation returns
    /// <see langword="false"/> so it is surfaced as a failure, never swallowed as success.
    /// </summary>
    private static bool IsCycleCompletionDuplicate(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sql
            && (sql.Number == UniqueConstraintViolation || sql.Number == DuplicateKeyRowViolation)
            && sql.Message.Contains(CompletionUniqueConstraint, StringComparison.Ordinal);
    }

    private bool InvalidContext(IIndTraceDbContext? context)
    {
        if (context is null)
        {
            this.logger.LogError("BarCodeAggregateRepository: database context is null.");
            return true;
        }

        return false;
    }
}
