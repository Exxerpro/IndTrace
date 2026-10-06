// <copyright file="NotOkUpdateStrategy.cs" company="Exxerpro Solutions SA de CV">
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

/// <summary>
/// Strategy for updating cycles with NOT OK status.
/// </summary>
/// <remarks>
/// <para>
/// #40 Chunk 40-E: the persistence tail is the BarCode aggregate operation-scoped unit of work — <c>LoadAsync</c>
/// (the M2 machine-windowed rework-cap set) → <see cref="BarCode.CompleteNotOkCycle"/> (byte-identical mutation +
/// WRITE-side NOT-OK rework-cap invariant) → <c>SaveAsync</c> (one atomic single-flush transaction). It replaces
/// the retired three-commit <c>PersistenceOrchestrator</c>. Per Option B the Shift create-or-get + <c>CyclesOk</c>
/// COUNT stays OUTSIDE the aggregate boundary (unchanged, before the atomic save), and <c>cycle.CyclesOk</c> is set
/// here, never inside <see cref="BarCode.CompleteNotOkCycle"/>.
/// </para>
/// <para>
/// Issue #179 (epic #174) — the former consecutive <c>if (result.IsFailure) { log; return ... }</c> blocks are
/// re-railwayed as ONE fluent <c>BindAsync</c> chain over small provenance-annotated step methods (the #178
/// shape), using only the no-catch in-tree <see cref="ResultExtensions.BindAsync{T,U}"/> combinator so a thrown
/// side effect still surfaces via the as-built outer catch. Every failure on this strategy is value-LESS
/// as-built, so the errors ride the chain itself (no fault box needed); unlike the OK twin, a staged NOT-OK
/// completion always finishes with a plain Success (the NotOk cycle-time anomaly is intentionally disregarded —
/// user-confirmed 2026-06-21).
/// </para>
/// </remarks>
public class NotOkUpdateStrategy : ICycleUpdateStrategy
{
    private readonly IRegisterCleaner _registerCleaner;
    private readonly IAggregateRepository<BarCode> _barCodeAggregateRepository;
    private readonly IShiftService _shiftService;
    private readonly IFlowStatusCalculator _flowStatusCalculator;
    private readonly IDateTimeMachine _dateTimeMachine;
    private readonly ILogger<NotOkUpdateStrategy> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotOkUpdateStrategy"/> class.
    /// </summary>
    public NotOkUpdateStrategy(
        IRegisterCleaner registerCleaner,
        IAggregateRepository<BarCode> barCodeAggregateRepository,
        IShiftService shiftService,
        IFlowStatusCalculator flowStatusCalculator,
        IDateTimeMachine dateTimeMachine,
        ILogger<NotOkUpdateStrategy> logger)
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
            "Executing NOT OK update strategy for CycleId={CycleId}, MachineId={MachineId}",
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
            _logger.LogError(ex, "Exception in NOT OK update strategy");
            return Result<CycleUpdateResult>.WithFailure($"Exception in NOT OK update: {ex.Message}");
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
    /// boundary (NOK cycles don't increment CyclesOk; the existing count is copied through).
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
    /// Pipeline step 3: cleans and prepares registers (pre-work, unchanged).
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
    /// Pipeline step 4 — the #40 Chunk 40-E completion: loads the machine-windowed aggregate (M2 complete cap
    /// set) and applies the NOT-OK completion through the aggregate root (mutates the SAME tracked cycle/barcode
    /// the load snapshot holds; finishes FinishedNok/NOk; demotes the barcode; enforces the NOT-OK cap
    /// WRITE-side) over the minimal lone-station graph for the required-but-discarded RoutingAdvancePolicy
    /// output (see <see cref="OkUpdateStrategy"/>).
    /// </summary>
    /// <remarks>
    /// Story 6.5: the god-object echo (UpdateBarCodeInformationOnCycle with the DERIVED cycle status) is no
    /// longer performed here — the handler replays that echo AFTER this strategy returns, using the derived
    /// values carried on the result entities, so the §7 projection stays byte-equal. A cap-refusal (or
    /// null-guard failure) stages NOTHING: refuse WITHOUT persisting.
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

        var apply = barCode.CompleteNotOkCycle(
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
            _logger.LogWarning("NOT OK cycle refused for CycleId={CycleId}: {Error}", context.CycleId, apply.Error);
            return Result<AppliedContext>.WithFailure(apply.Errors);
        }

        return Result<AppliedContext>.Success(new AppliedContext(barCode, cycle, prepared.Shift, prepared.CleanedRegisters));
    }

    /// <summary>
    /// Pipeline step 5: sets the shift-derived <c>cycle.CyclesOk</c> projection (Option B — use existing count,
    /// don't increment; shift-derived, outside the aggregate boundary) and persists the aggregate atomically
    /// (single-flush transaction).
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
    /// Pipeline step 6 (terminal): builds the strategy result — always a plain Success once the completion was
    /// staged and persisted (the NotOk cycle-time anomaly never surfaces as a failure; user-confirmed
    /// 2026-06-21).
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
            "NOT OK update completed successfully: RegistersSaved={RegistersSaved}, CyclesOk={CyclesOk}",
            result.RegistersSaved, result.CyclesOk);

        return Result<CycleUpdateResult>.Success(result);
    }

    // Minimal lone-station ProductionGraph required by BarCode.CompleteNotOkCycle; the resolved advance is discarded.
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
    /// #179: pipeline context after the staged NOT-OK completion (no apply verdict is carried — the NotOk
    /// cycle-time anomaly is intentionally disregarded downstream).
    /// </summary>
    private sealed record AppliedContext(BarCode BarCode, Cycle Cycle, ShiftCreatedEvent Shift, List<Register> CleanedRegisters);
}
