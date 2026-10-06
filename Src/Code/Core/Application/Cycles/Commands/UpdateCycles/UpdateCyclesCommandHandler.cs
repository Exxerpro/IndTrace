// <copyright file="UpdateCyclesCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.UpdateCycles;

using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Models.Extensions;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Unified handler for cycle update commands using SRP services.
/// </summary>
/// <remarks>
/// Issue #179 (epic #174) — the former consecutive <c>if (result.IsFailure) { log; return ... }</c> blocks are
/// re-railwayed as ONE fluent <c>BindAsync</c> chain over small provenance-annotated step methods, modeled on the
/// #178 CreateCycles refactor. The chain uses only the no-catch in-tree
/// <see cref="ResultExtensions.BindAsync{T,U}"/> combinator (the IQR fluent combinators re-wrap exceptions — the
/// 18/06/2026 revert regression), so a thrown side effect still surfaces via the as-built outer catch with the
/// pinned "Exception occurred" message. Unlike the flag-gated CreateCycles diagnostics, this handler's failure
/// mode is ALWAYS-CARRY (Story 6.5 Task 5, ratified as-built pin): every terminal failure carries a §7 diagnostic
/// DTO built through the shared #176 <see cref="GatewayFailureFactory"/>, and because the value-dropping failure
/// propagation of <c>BindAsync</c> cannot thread the DTO itself, the failing step stashes the exact value-carrying
/// failure on a per-request <see cref="UpdateGatewayFaultBox"/> that the single terminal returns. Every failure code,
/// log entry, §7 field, write ordering and value-carrying-vs-value-less failure shape is pinned byte-identically
/// by <c>UpdateCyclesGoldenMasterTests</c> / <c>UpdateCyclesDiagnosticsGoldenTests</c>.
/// </remarks>
public class UpdateCyclesCommandHandler :
    IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>,
    IGatewayRequestHandler<UpdateCyclesNotOkCommand, TaskGatewayResponseDto>
{
    private readonly IBarCodeInfoProvider _barCodeInfoProvider;
    private readonly IStationValidator _stationValidator;
    private readonly ICycleUpdateStrategyFactory _strategyFactory;
    private readonly ICommandLogger _commandLogger;
    private readonly ILogger<UpdateCyclesCommandHandler> _logger;

    // D2 / FR1+FR5 (docs/architecture/state-machine/d2-cycle-path-routing-design.md): the injected (decorated)
    // machine is the lifecycle gate for the two cycle-update triggers and writes the FlowTransitionLog row. The
    // trailing-optional ctor params mirror RejectBarCodeCommandHandler: legacy positional construction (e.g. the
    // golden masters) falls back to the as-built engine + default-ON routing so behavior matches DI (CR1).
    private readonly IItemStateMachine _machine;
    private readonly StateMachineRoutingOptions _routing;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCyclesCommandHandler"/> class.
    /// </summary>
    public UpdateCyclesCommandHandler(
        IBarCodeInfoProvider barCodeInfoProvider,
        IStationValidator stationValidator,
        ICycleUpdateStrategyFactory strategyFactory,
        ICommandLogger commandLogger,
        ILogger<UpdateCyclesCommandHandler> logger,
        IItemStateMachine? stateMachine = null,
        IOptions<StateMachineRoutingOptions>? routingOptions = null)
    {
        _barCodeInfoProvider = barCodeInfoProvider;
        _stationValidator = stationValidator;
        _strategyFactory = strategyFactory;
        _commandLogger = commandLogger;
        _logger = logger;
        _machine = stateMachine ?? new ItemStateMachine();
        _routing = routingOptions?.Value ?? new StateMachineRoutingOptions();
    }

    /// <summary>
    /// Processes an OK cycle update command.
    /// </summary>
    public Task<Result<TaskGatewayResponseDto>> ProcessAsync(
        UpdateCyclesOkCommand cmd,
        CancellationToken cancellationToken)
    {
        return ProcessInternalAsync(
            new UpdateCycleCommandAdapter(cmd),
            CycleStatus.FinishedOk,
            GatewayTask.UpdateCycleOkAsync,
            cancellationToken);
    }

    /// <summary>
    /// Processes a NOT OK cycle update command.
    /// </summary>
    public Task<Result<TaskGatewayResponseDto>> ProcessAsync(
        UpdateCyclesNotOkCommand cmd,
        CancellationToken cancellationToken)
    {
        return ProcessInternalAsync(
            new UpdateCycleCommandAdapter(cmd),
            CycleStatus.FinishedNok,
            GatewayTask.UpdateCycleNotOkAsync,
            cancellationToken);
    }

    private async Task<Result<TaskGatewayResponseDto>> ProcessInternalAsync(
        IUpdateCycleCommand command,
        CycleStatus targetStatus,
        GatewayTask gatewayTask,
        CancellationToken cancellationToken)
    {
        if (command is null)
        {
            return Result<TaskGatewayResponseDto>.WithFailure("Command cannot be null");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return IndQuestResults.Operations.ResultExtensions.Cancelled<TaskGatewayResponseDto>();
        }

        _logger.LogInformation(
            "Processing cycle update: BarCode={BarCode}, MachineId={MachineId}, TargetStatus={TargetStatus}",
            command.BarCode, command.MachineId, targetStatus);

        try
        {
            // #179: the per-request carrier for the always-carry §7 failure diagnostic (see UpdateGatewayFaultBox docs).
            var faults = new UpdateGatewayFaultBox();

            var outcome = await this.LoadStateAsync(command, faults, cancellationToken)
                .BindAsync((context, _) => Task.FromResult(this.ValidateStationStep(command, targetStatus, context, faults)), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(BuildDecideContextStep(context)), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(this.FireLifecycleGateStep(command, targetStatus, gatewayTask, context, faults)), cancellationToken)
                .BindAsync((context, ct) => this.ExecuteStrategyStepAsync(command, targetStatus, context, faults, ct), cancellationToken)
                .BindAsync((context, ct) => this.LogCommandStepAsync(command, gatewayTask, context, ct), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(this.ProjectResponseStep(targetStatus, context)), cancellationToken);

            // On a step failure the box carries the exact as-built value-carrying failure (errors + diagnostic
            // DTO); the value-less chain outcome is the fallback for the two box-less failure shapes — the
            // strategy's success-with-null-result guard (value-less as-built) and a mid-chain cancellation
            // short-circuit inside BindAsync (the #178-ratified precedent).
            return faults.Terminal ?? outcome;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception processing cycle update");
            var message = $"Exception occurred: {ex.Message}";
            //#179: expressed through the shared #176 factory's always-carry mode — byte-identical to the previous
            //inline BuildFailureDto + WithFailure(message, dto) pair (Classify of the message, no references).
            return GatewayFailureFactory.FailAlways(message, command.MachineId);
        }
    }

    /// <summary>
    /// Pipeline step 1 — Story 6.5 (Task 4): loads an IMMUTABLE snapshot of the cycle-update state. The
    /// god-object is constructed and snapshotted INSIDE the loader and never escapes it; station validation,
    /// command logging and the §7 projection all ride this snapshot + the DECIDE result.
    /// </summary>
    private async Task<Result<UpdateLoadedContext>> LoadStateAsync(IUpdateCycleCommand command, UpdateGatewayFaultBox faults, CancellationToken cancellationToken)
    {
        var loadResult = await _barCodeInfoProvider
            .GetCycleUpdateLoadStateAsync(command.MachineId, command.BarCode, command.PartNumber, cancellationToken)
            .ConfigureAwait(false);

        if (loadResult.IsFailure || loadResult.Value is null)
        {
            // Story 6.5 (Task 5) — a load failure (barcode/cycle not found, missing references, corrupted
            // lookup) must reach the PLC as its SPECIFIC code, not the transport's generic -1 (user-confirmed
            // 2026-06-21). No snapshot exists yet, so classify the message and build a diagnostic response.
            _logger.LogError("Failed to get barcode info: {Errors}", string.Join(", ", loadResult.Errors));
            var dto = GatewayFailureFactory.BuildFailureDto(loadResult.Errors.FirstOrDefault(), command.MachineId);
            return FaultStep<UpdateLoadedContext>(faults, Result<TaskGatewayResponseDto>.WithFailure(loadResult.Errors, dto));
        }

        return Result<UpdateLoadedContext>.Success(new UpdateLoadedContext(loadResult.Value));
    }

    /// <summary>
    /// Pipeline step 2: validates the station's capability to perform this cycle update. Three refusal shapes
    /// are preserved byte-identically: a failed validation Result, a null validation value, and the validator's
    /// own CanUpdate refusal carrying its SPECIFIC <see cref="ResultValidation"/> code.
    /// </summary>
    private Result<UpdateLoadedContext> ValidateStationStep(IUpdateCycleCommand command, CycleStatus targetStatus, UpdateLoadedContext context, UpdateGatewayFaultBox faults)
    {
        var load = context.Load;

        var validationResult = _stationValidator.ValidateStation(command.MachineId, targetStatus, load);
        if (validationResult is { IsFailure: true })
        {
            _logger.LogError("Station validation failed: {Errors}", string.Join(", ", validationResult.Errors));
            var dto = GatewayFailureFactory.BuildFailureDto(validationResult.Errors.FirstOrDefault(), command.MachineId, references: load.References);
            return FaultStep<UpdateLoadedContext>(faults, Result<TaskGatewayResponseDto>.WithFailure(validationResult.Errors, dto));
        }

        if (validationResult.Value is null)
        {
            _logger.LogError("Station validation returned null result");
            const string nullReason = "Station validation returned null result";
            return FaultStep<UpdateLoadedContext>(faults, GatewayFailureFactory.FailAlways(nullReason, command.MachineId, references: load.References));
        }

        if (validationResult.Value is { CanUpdate: false } refusal)
        {
            // Story 6.5 (Task 5) — surface the station validator's OWN code (DestinationNotValid when the cycle
            // belongs to another station, WorkFlowNotValid for a wrong/late machine, …) so the operator learns
            // WHY the station refused, not a generic -1. Applies to both the OK and NotOk dispatch.
            var reason = refusal.FailureReason ?? "Station validation failed";
            _logger.LogError("Station cannot update: {Reason}", reason);
            return FaultStep<UpdateLoadedContext>(faults, GatewayFailureFactory.FailAlways(reason, command.MachineId, refusal.Validation, load.References));
        }

        return Result<UpdateLoadedContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 3 — Story 6.5: builds the immutable DECIDE-step context carrying the SAME tracked
    /// cycle/barcode entities the loader snapshotted, so the strategy's in-place mutations stay observable to
    /// the terminal projection. Never fails.
    /// </summary>
    private static Result<DecidedContext> BuildDecideContextStep(UpdateLoadedContext context)
    {
        return Result<DecidedContext>.Success(new DecidedContext(context.Load, context.Load.ToDecideContext()));
    }

    /// <summary>
    /// Pipeline step 4 — D2 / FR1+FR5 (docs/architecture/state-machine/d2-cycle-path-routing-design.md §B.4):
    /// routes the cycle transition through the injected (decorated) machine ONCE, PRE-strategy, as the lifecycle
    /// gate. The machine fires on the LOAD-TIME barcode (FlowStatus InProcess — or Created when the create
    /// station closes its own first cycle, the 2026-07-21 E2E row for OK and its 2026-07-23 #189-ratified NOK
    /// mirror) with the DETERMINISTIC target tuple (OK -&gt; FinishedOk/Ok; NotOk -&gt; FinishedNok/NOk) so the
    /// decorator records the correct realized transition To (Final-OK -&gt; Finished, else InProcess) and exactly
    /// ONE FlowTransitionLog row (the PLC dispatcher already stamped TransitionPath.Plc). With CycleTimeGuard
    /// removed from the cycle rows the remaining guard is (Machine, Shift) — both satisfied here (station
    /// validated, shift retrieved by the strategy) — so a legal InProcess or Created barcode SUCCEEDS regardless
    /// of cycle time. An out-of-order trigger from any other source state (e.g. already Finished/Rejected) is
    /// TABLE-REJECTED: the handler returns a value-carrying failure with the machine's SPECIFIC ResultValidation
    /// and NOTHING is mutated or persisted (exactly like RejectBarCodeCommandHandler). The fire does NOT alter
    /// the strategy's Result: FlowStatus stays strategy-derived via the identical FlowStatusCalculator and the
    /// cycle-time verdict stays in Cycle.FinishOk/FinishNok (D1 preserved). Flag-OFF skips the fire -&gt;
    /// byte-identical to today. (PO decision 2026-07-28, issue #174: this single pre-strategy fire stays exactly
    /// as-built — NOT routed through the domain aggregate; the spy-observable call shape is pinned by
    /// <c>UpdateCycleStateMachineRoutingTests</c>.)
    /// </summary>
    private Result<DecidedContext> FireLifecycleGateStep(IUpdateCycleCommand command, CycleStatus targetStatus, GatewayTask gatewayTask, DecidedContext context, UpdateGatewayFaultBox faults)
    {
        var load = context.Load;

        var route = targetStatus == CycleStatus.FinishedOk
            ? _routing.RouteUpdateCycleOk
            : _routing.RouteUpdateCycleNotOk;
        if (!route || load.BarCode is null)
        {
            return Result<DecidedContext>.Success(context);
        }

        var (gateCycleStatus, gatePartStatus) = targetStatus == CycleStatus.FinishedOk
            ? (CycleStatus.FinishedOk, PartStatus.Ok)
            : (CycleStatus.FinishedNok, PartStatus.NOk);

        var gateContext = new TransitionContext(
            load.MachineType,
            gateCycleStatus,
            gatePartStatus,
            load.Cycle.CycleTime,
            load.Recipe,
            MachineFound: true,   // station already validated above
            ShiftValid: true);    // shift retrieved by the strategy (hard precondition)

        var gate = _machine.Fire(load.BarCode, gatewayTask, gateContext);
        if (gate is { IsFailure: true })
        {
            // Out-of-order source state (e.g. barcode already Finished/Rejected): reject BEFORE the
            // strategy mutates/persists.
            var gateCode = gate.Value?.Result ?? ResultValidation.OperationCancelled;
            _logger.LogError(
                "Cycle update rejected by state machine: ({FlowStatus}, {Trigger}) -> {Code}",
                load.BarCode.FlowStatus.Name, gatewayTask.Name, gateCode.Name);
            var gateDto = GatewayFailureFactory.BuildFailureDto(
                $"Cycle update rejected by state machine: ({load.BarCode.FlowStatus.Name}, {gatewayTask.Name})",
                command.MachineId,
                gateCode,
                load.References);
            return FaultStep<DecidedContext>(
                faults,
                Result<TaskGatewayResponseDto>.WithFailure(
                    $"Cycle update rejected by state machine: ({load.BarCode.FlowStatus.Name}, {gatewayTask.Name}) -> {gateCode.Name} ({gateCode.Value})",
                    gateDto));
        }

        return Result<DecidedContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 5: executes the dispatch-selected strategy on the DECIDE context.
    /// </summary>
    /// <remarks>
    /// Story 6.5 (Task 5) — the OK strategy is the ONLY path that returns a FAILURE carrying a value: the
    /// cycle-time-override (Cycle.FinishOk guard forces FinishedNok/NOk and fails). The PLC consumes the
    /// negative-code projection on this failure (user-confirmed 2026-06-21), so reproduce the legacy Diagnose
    /// path byte-equal — project the §7 DTO from the LOAD snapshot + the override result, stamp the recipe-aware
    /// code (CycleTimeGuard authority: null recipe -&gt; RecipeNotFound(-512); out-of-range with a valid recipe
    /// -&gt; PartNotValid(-64)) via Promote, and return a value-CARRYING failure so the transport's
    /// "ResultValidation &gt;= 0 collapse to -1" branch stays false. The infra failure (shift create /
    /// register-clean / persist) carries no value: classify the message and still hand the PLC a specific
    /// diagnostic code (ShiftInvalid, …) instead of a generic -1. The success-with-null-result guard stays a
    /// value-LESS failure (as-built) and rides the chain without touching the fault box.
    /// </remarks>
    private async Task<Result<ExecutedContext>> ExecuteStrategyStepAsync(IUpdateCycleCommand command, CycleStatus targetStatus, DecidedContext context, UpdateGatewayFaultBox faults, CancellationToken cancellationToken)
    {
        var load = context.Load;

        var strategy = _strategyFactory.CreateStrategy(targetStatus);
        var updateResult = await strategy.ExecuteAsync(command, context.Decide, cancellationToken)
            .ConfigureAwait(false);

        if (updateResult is { IsFailure: true })
        {
            _logger.LogError("Strategy execution failed: {Errors}", string.Join(", ", updateResult.Errors));

            if (updateResult.Value is not null)
            {
                var failureDto = CycleUpdateProjection.ToResponse(load, updateResult.Value, targetStatus);
                var failureCode = load.Recipe is null
                    ? ResultValidation.RecipeNotFound
                    : ResultValidation.PartNotValid;
                failureDto = PlcFailureDiagnostics.Promote(failureDto, failureCode);
                return FaultStep<ExecutedContext>(faults, Result<TaskGatewayResponseDto>.WithFailure(updateResult.Errors, failureDto));
            }

            var infraDto = GatewayFailureFactory.BuildFailureDto(updateResult.Errors.FirstOrDefault(), command.MachineId, references: load.References);
            return FaultStep<ExecutedContext>(faults, Result<TaskGatewayResponseDto>.WithFailure(updateResult.Errors, infraDto));
        }

        if (updateResult.Value is null)
        {
            _logger.LogError("Strategy returned success with a null result");
            return Result<ExecutedContext>.WithFailure("Strategy returned a null result");
        }

        return Result<ExecutedContext>.Success(new ExecutedContext(load, updateResult.Value));
    }

    /// <summary>
    /// Pipeline step 6 — #65 (traceability integrity): writes the command-log audit row. This write is on the
    /// LIVE OK/NOK success path whose §7 response is FROZEN (byte-identical, user-confirmed 2026-06-21). A
    /// dropped command-log must not flip the wire response, so the audit Result is routed through the chain as a
    /// step whose failure is logged-and-flagged (CommandLogger also logs internally) but NEVER fatal — the step
    /// always succeeds, preserving the as-built log-and-discard semantics observably.
    /// </summary>
    private async Task<Result<ExecutedContext>> LogCommandStepAsync(IUpdateCycleCommand command, GatewayTask gatewayTask, ExecutedContext context, CancellationToken cancellationToken)
    {
        var logCommand = _commandLogger.CreateCommand(context.Load, gatewayTask);
        var logResult = await _commandLogger.LogCommandAsync(logCommand, cancellationToken)
            .ConfigureAwait(false);
        if (logResult is { IsFailure: true })
        {
            _logger.LogError(
                "Cycle-update command-log write did not land: BarCode={BarCode}, MachineId={MachineId}, Task={Task}, Error={Error}",
                command.BarCode, command.MachineId, gatewayTask.Name, logResult.Errors.FirstOrDefault());
        }

        return Result<ExecutedContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 7 (terminal projection) — Story 6.5 (Task 4): projects from the immutable snapshot + DECIDE
    /// result. ToResponse reproduces the god-object's ToDto field-for-field and replays the former handler
    /// write-back: CyclesOk gated &gt;0, the NotOk-gated status echo (FinishedNok path) and the OK path's
    /// LOAD-TIME scalars — byte-equal to the retired god-object projection. No stateful IBarCodeResult on this
    /// path anymore. The §7 References are stamped via the pure ReferenceStamper (#32 C2).
    /// </summary>
    private Result<TaskGatewayResponseDto> ProjectResponseStep(CycleStatus targetStatus, ExecutedContext context)
    {
        var response = CycleUpdateProjection.ToResponse(context.Load, context.Update, targetStatus);

        var stampedResponse = ReferenceStamper.Apply(response);
        if (stampedResponse.Value is not null)
        {
            response = stampedResponse.Value;
        }

        _logger.LogInformation(
            "Cycle update completed successfully: RegistersSaved={RegistersSaved}, CyclesOk={CyclesOk}",
            context.Update.RegistersSaved, context.Update.CyclesOk);

        return Result<TaskGatewayResponseDto>.Success(response);
    }

    /// <summary>
    /// #179 failure terminal for a pipeline step: records the exact always-carry §7 failure (Story 6.5 Task 5 —
    /// the diagnostic DTO publishes the SPECIFIC negative code so the PLC and the station operator learn WHAT
    /// failed instead of the transport's generic -1) on the per-request <see cref="UpdateGatewayFaultBox"/> and returns
    /// the chain-typed failure carrying the same errors. The chain short-circuits at the first failure, so at
    /// most one terminal is ever recorded per request.
    /// </summary>
    /// <typeparam name="T">The pipeline context type of the failing step.</typeparam>
    private static Result<T> FaultStep<T>(UpdateGatewayFaultBox faults, Result<TaskGatewayResponseDto> terminal)
    {
        faults.Terminal = terminal;
        return Result<T>.WithFailure(terminal.Errors);
    }

    /// <summary>
    /// #179: pipeline context after the load step — the immutable Story 6.5 (Task 4) load snapshot.
    /// </summary>
    private sealed record UpdateLoadedContext(CycleUpdateLoadState Load);

    /// <summary>
    /// #179: pipeline context after the DECIDE-context step — adds the immutable
    /// <see cref="CycleUpdateContext"/> carrying the SAME tracked cycle/barcode entities the loader snapshotted
    /// (the strategy mutates them in place; the projection observes the mutations).
    /// </summary>
    private sealed record DecidedContext(CycleUpdateLoadState Load, CycleUpdateContext Decide);

    /// <summary>
    /// #179: pipeline context after the strategy step — adds the strategy's update result for the command-log
    /// audit and the terminal §7 projection.
    /// </summary>
    private sealed record ExecutedContext(CycleUpdateLoadState Load, CycleUpdateResult Update);

    /// <summary>
    /// #179: per-request carrier for the always-carry §7 failure diagnostic. The in-tree no-catch
    /// <see cref="ResultExtensions.BindAsync{T,U}"/> combinator propagates a failed link as a VALUE-LESS failure
    /// (only the errors survive), so the diagnostic-carrying <c>Result&lt;TaskGatewayResponseDto&gt;</c> a
    /// failing step builds (Story 6.5 Task 5 — ALWAYS value-carrying on this handler, the ratified as-built pin)
    /// cannot ride the chain itself; it rides this box to the single terminal instead. Instantiated per
    /// ProcessInternalAsync call — never shared, so the captive-dependency race class cannot recur.
    /// </summary>
    private sealed class UpdateGatewayFaultBox
    {
        /// <summary>Gets or sets the exact value-carrying failure the single failing step recorded, if any.</summary>
        public Result<TaskGatewayResponseDto>? Terminal { get; set; }
    }
}
