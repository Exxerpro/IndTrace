// <copyright file="OkUpdateStrategy.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services.Strategies;

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Models.Extensions;
using IndTrace.Application.Shifts.Commands.Create;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Strategy for updating cycles with OK status.
/// </summary>
/// <remarks>
/// <para>
/// #40 Chunk 40-E: the persistence tail is the BarCode aggregate operation-scoped unit of work
/// (<see cref="IAggregateRepository{TRoot}"/> of <see cref="BarCode"/>) — <c>LoadAsync</c> (the M2 machine-windowed
/// rework-cap set) → <see cref="BarCode.CompleteOkCycle"/> (byte-identical mutation + WRITE-side rework-cap
/// invariant) → <c>SaveAsync</c> (one atomic single-flush transaction: in-place cycle/barcode update, register
/// append, idempotency marker). It replaces the retired three-commit <c>PersistenceOrchestrator</c>. Per Option B
/// the Shift create-or-get + <c>CyclesOk</c> COUNT stays OUTSIDE the aggregate boundary: it runs before the atomic
/// save exactly as before, and <c>cycle.CyclesOk</c> is set here (a recomputable shift-derived projection), never
/// inside <see cref="BarCode.CompleteOkCycle"/>.
/// </para>
/// <para>
/// Issue #179 (epic #174) — the former consecutive <c>if (result.IsFailure) { log; return ... }</c> blocks are
/// re-railwayed as ONE fluent <c>BindAsync</c> chain over small provenance-annotated step methods (the #178
/// shape), using only the no-catch in-tree <see cref="ResultExtensions.BindAsync{T,U}"/> combinator so a thrown
/// side effect still surfaces via the as-built outer catch. Every mid-chain failure here is value-LESS as-built,
/// so the errors ride the chain itself (no fault box needed); the ONLY value-carrying failure is the terminal
/// cycle-time-override shape ("Cycle time is invalid" + the persisted result), which the LAST chain step returns
/// directly — nothing downstream can strip its value.
/// </para>
/// </remarks>
public class OkUpdateStrategy : ICycleUpdateStrategy
{
    private readonly IRegisterCleaner _registerCleaner;
    private readonly IAggregateRepository<BarCode> _barCodeAggregateRepository;
    private readonly IShiftService _shiftService;
    private readonly IFlowStatusCalculator _flowStatusCalculator;
    private readonly IDateTimeMachine _dateTimeMachine;
    private readonly ILogger<OkUpdateStrategy> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OkUpdateStrategy"/> class.
    /// </summary>
    public OkUpdateStrategy(
        IRegisterCleaner registerCleaner,
        IAggregateRepository<BarCode> barCodeAggregateRepository,
        IShiftService shiftService,
        IFlowStatusCalculator flowStatusCalculator,
        IDateTimeMachine dateTimeMachine,
        ILogger<OkUpdateStrategy> logger)
    {
        _registerCleaner = registerCleaner;
        _barCodeAggregateRepository = barCodeAggregateRepository;
        _shiftService = shiftService;
        _flowStatusCalculator = flowStatusCalculator;
        _dateTimeMachine = dateTimeMachine;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CycleUpdateResult>> ExecuteAsync(
        IUpdateCycleCommand command,
        CycleUpdateContext context,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("ExecuteAsync cancelled");
            return IndQuestResults.Operations.ResultExtensions.Cancelled<CycleUpdateResult>();
        }

        if (command is null)
        {
            return Result<CycleUpdateResult>.WithFailure("Command cannot be null.");
        }

        if (context is null)
        {
            return Result<CycleUpdateResult>.WithFailure("Context cannot be null.");
        }

        _logger.LogInformation(
            "Executing OK update strategy for CycleId={CycleId}, MachineId={MachineId}",
            context.CycleId, command.MachineId);

        try
        {
            return await Task.FromResult(GuardBarCodeStep(context))
                .BindAsync((seed, ct) => this.ResolveShiftStepAsync(command, seed, ct), cancellationToken)
                .BindAsync((shifted, _) => Task.FromResult(this.CleanRegistersStep(command, context, shifted)), cancellationToken)
                .BindAsync((prepared, ct) => this.ApplyCompletionStepAsync(command, context, prepared, ct), cancellationToken)
                .BindAsync((applied, ct) => this.PersistStepAsync(applied, ct), cancellationToken)
                .BindAsync((applied, _) => Task.FromResult(this.FinalizeStep(applied)), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in OK update strategy");
            return Result<CycleUpdateResult>.WithFailure($"Exception in OK update: {ex.Message}");
        }
    }

    /// <summary>
    /// Pipeline step 1 — Story 27.2b-2: <c>context.BarCode</c> is nullable ("no part" = absent). A strategy only
    /// executes on the found/validated path, so an absent barcode here is an error; guard for the compiler
    /// (byte-equal — the guard never triggers on the reachable path).
    /// </summary>
    private static Result<SeedContext> GuardBarCodeStep(CycleUpdateContext context)
    {
        if (context.BarCode is null)
        {
            return Result<SeedContext>.WithFailure("BarCode not found.");
        }

        return Result<SeedContext>.Success(new SeedContext(context.BarCode, context.Cycle));
    }

    /// <summary>
    /// Pipeline step 2 — Option B (#40 B1): Shift create-or-get + the CyclesOk COUNT stay OUTSIDE the aggregate
    /// boundary. Shift is its own per-machine aggregate and CyclesOk is a recomputable projection, so this runs
    /// BEFORE the atomic save exactly as before and is NOT pulled into <see cref="BarCode.CompleteOkCycle"/>.
    /// </summary>
    private async Task<Result<ShiftContext>> ResolveShiftStepAsync(IUpdateCycleCommand command, SeedContext seed, CancellationToken cancellationToken)
    {
        var shiftResult = await _shiftService
            .CreateOrRetrieveShiftAndCyclesOkAsync(command.MachineId, cancellationToken)
            .ConfigureAwait(false);

        if (shiftResult is { IsFailure: true })
        {
            _logger.LogError("Failed to get shift: {Error}", shiftResult.Error);
            return Result<ShiftContext>.WithFailure($"Cannot create shift: {shiftResult.Error}");
        }

        if (shiftResult.Value is not { } shift)
        {
            _logger.LogError("Shift service returned a null shift on success.");
            return Result<ShiftContext>.WithFailure("Shift service returned a null shift.");
        }

        return Result<ShiftContext>.Success(new ShiftContext(seed.BarCode, seed.Cycle, shift));
    }

    /// <summary>
    /// Pipeline step 3: cleans and prepares registers (pre-work, unchanged) — the append-only readings the
    /// aggregate stages.
    /// </summary>
    private Result<PreparedContext> CleanRegistersStep(IUpdateCycleCommand command, CycleUpdateContext context, ShiftContext shifted)
    {
        var cleanResult = _registerCleaner.CleanRegisters(
            command.Registers,
            context.CycleId,
            command.MachineId,
            _dateTimeMachine.Now.ToLocalTime());

        if (cleanResult.IsFailure || cleanResult.Value is null)
        {
            _logger.LogError("Failed to clean registers: {Error}", cleanResult.Error);
            return Result<PreparedContext>.WithFailure(cleanResult.Errors);
        }

        return Result<PreparedContext>.Success(new PreparedContext(shifted.BarCode, shifted.Cycle, shifted.Shift, cleanResult.Value.ToList()));
    }

    /// <summary>
    /// Pipeline step 4 — the #40 Chunk 40-E completion. Loads the machine-windowed aggregate: the COMPLETE set of
    /// this label's cycles on the current machine (M2 — a WHERE MachineId IN (...) with no paging), which is the
    /// exact multiset the write-side rework cap counts. Then applies the OK completion through the aggregate
    /// root: it mutates the SAME tracked cycle/barcode the load snapshot holds (so the §7 projection stays
    /// byte-identical), finishes the cycle via the guarded Cycle.FinishOk (cycle-time verdict), applies the
    /// calculator-derived flow status, and enforces the rework cap as a WRITE-side invariant.
    /// </summary>
    /// <remarks>
    /// CompleteOkCycle requires a non-null ProductionGraph for the shared RoutingAdvancePolicy. Its output
    /// (BarCode.ResolvedNextMachineId) is ADDITIVE and NOT consumed on this path — the §7 NextMachineId comes
    /// from the read-path load snapshot; a later chunk rewires the read path onto the same policy. A minimal
    /// lone-station graph satisfies the guard; the discarded advance does not affect the byte-parity contract.
    /// A cap-refusal (or a null-guard failure) stages NOTHING (AppliedCycle stays null): refuse WITHOUT
    /// persisting. A cycle-time-invalid override IS staged (AppliedCycle set) and must still persist.
    /// </remarks>
    private async Task<Result<AppliedContext>> ApplyCompletionStepAsync(IUpdateCycleCommand command, CycleUpdateContext context, PreparedContext prepared, CancellationToken cancellationToken)
    {
        var barCode = prepared.BarCode;
        var cycle = prepared.Cycle;

        var loadResult = await _barCodeAggregateRepository
            .LoadAsync(
                barCode.BarCodeId.Value,
                AggregateLoadOptions.ForMachineWindow([command.MachineId]),
                cancellationToken)
            .ConfigureAwait(false);

        if (loadResult.IsFailure || loadResult.Value is null)
        {
            _logger.LogError("Failed to load barcode aggregate: {Error}", loadResult.Error);
            return Result<AppliedContext>.WithFailure(loadResult.Errors);
        }

        var capWindow = loadResult.Value.LoadedCycles;

        var graphResult = BuildAdvanceGraph(command.MachineId);
        if (graphResult.IsFailure || graphResult.Value is null)
        {
            _logger.LogError("Failed to build routing advance graph: {Error}", graphResult.Error);
            return Result<AppliedContext>.WithFailure(graphResult.Errors);
        }

        var apply = barCode.CompleteOkCycle(
            cycle,
            command.MachineId,
            context.MachineType,
            context.Recipe,
            prepared.CleanedRegisters,
            capWindow,
            _flowStatusCalculator,
            graphResult.Value,
            _dateTimeMachine);

        if (barCode.AppliedCycle is null)
        {
            _logger.LogWarning("OK cycle refused for CycleId={CycleId}: {Error}", context.CycleId, apply.Error);
            return Result<AppliedContext>.WithFailure(apply.Errors);
        }

        if (apply is { IsFailure: true })
        {
            _logger.LogWarning("Cycle time validation failed: cycle {CycleId} forced to FinishedNok", context.CycleId);
        }

        return Result<AppliedContext>.Success(new AppliedContext(barCode, cycle, prepared.Shift, prepared.CleanedRegisters, apply));
    }

    /// <summary>
    /// Pipeline step 5: sets the shift-derived <c>cycle.CyclesOk</c> projection (Option B — OUTSIDE the aggregate
    /// boundary, pre-save, documented design: Shift is its own aggregate and CyclesOk is recomputable) and then
    /// persists the aggregate atomically (single-flush transaction), replacing the retired 3-commit tail.
    /// </summary>
    private async Task<Result<AppliedContext>> PersistStepAsync(AppliedContext applied, CancellationToken cancellationToken)
    {
        applied.Cycle.CyclesOk = applied.Shift.CyclesOk;

        var saveResult = await _barCodeAggregateRepository
            .SaveAsync(applied.BarCode, cancellationToken)
            .ConfigureAwait(false);

        if (saveResult is { IsFailure: true })
        {
            _logger.LogError("Failed to persist: {Error}", saveResult.Error);
            return Result<AppliedContext>.WithFailure(saveResult.Errors);
        }

        return Result<AppliedContext>.Success(applied);
    }

    /// <summary>
    /// Pipeline step 6 (terminal): builds the strategy result. Byte-parity with the legacy strategy return shape:
    /// on a cycle-time-invalid override the state is persisted and the failure is reported with the LEGACY
    /// message + the value-carrying result (the handler consumes the value to project the recipe-aware §7
    /// negative code). This is the LAST chain link, so the value-carrying failure is the chain outcome itself —
    /// no downstream <c>BindAsync</c> can strip its value.
    /// </summary>
    private Result<CycleUpdateResult> FinalizeStep(AppliedContext applied)
    {
        var result = new CycleUpdateResult(
            UpdatedCycle: applied.Cycle,
            UpdatedBarCode: applied.BarCode,
            RegistersSaved: applied.CleanedRegisters.Count,
            CyclesOk: applied.Shift.CyclesOk,
            ShiftInfo: new ShiftInfo(applied.Shift.ShiftId, applied.Shift.CyclesOk));

        _logger.LogInformation(
            "OK update completed successfully: RegistersSaved={RegistersSaved}, CyclesOk={CyclesOk}",
            result.RegistersSaved, result.CyclesOk);

        return applied.Apply.IsSuccess
            ? Result<CycleUpdateResult>.Success(result)
            : Result<CycleUpdateResult>.WithFailure("Cycle time is invalid", result);
    }

    // Builds the minimal lone-station ProductionGraph (the current machine as Initial|Serial|Final, a sanctioned
    // role) required by BarCode.CompleteOkCycle. The resolved advance is discarded (see call site), so this graph's
    // only job is to be a valid non-null topology; NEVER throws (ProductionGraph.Create returns a Result).
    private static Result<ProductionGraph> BuildAdvanceGraph(int machineId)
    {
        IReadOnlyCollection<RoutingTransition> transitions =
        [
            new RoutingTransition(
                machineId,
                0,
                WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial | WorkFlowType.Final)),
        ];

        return ProductionGraph.Create(transitions);
    }

    /// <summary>
    /// #179: pipeline context after the barcode guard — the non-null tracked barcode/cycle pair the strategy
    /// mutates in place.
    /// </summary>
    private sealed record SeedContext(BarCode BarCode, Cycle Cycle);

    /// <summary>
    /// #179: pipeline context after the shift step — adds the Option B shift create-or-get outcome.
    /// </summary>
    private sealed record ShiftContext(BarCode BarCode, Cycle Cycle, ShiftCreatedEvent Shift);

    /// <summary>
    /// #179: pipeline context after the register-clean step — adds the cleaned append-only readings the
    /// aggregate stages.
    /// </summary>
    private sealed record PreparedContext(BarCode BarCode, Cycle Cycle, ShiftCreatedEvent Shift, List<Register> CleanedRegisters);

    /// <summary>
    /// #179: pipeline context after the aggregate completion — adds the staged apply verdict (success, or the
    /// cycle-time-invalid override that must still persist and surface as the value-carrying terminal failure).
    /// </summary>
    private sealed record AppliedContext(BarCode BarCode, Cycle Cycle, ShiftCreatedEvent Shift, List<Register> CleanedRegisters, Result Apply);
}
