// <copyright file="CreateCyclesCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Validation;
using IndTrace.Application.Gateway.Auditing;
using IndTrace.Application.Models.Extensions;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

namespace IndTrace.Application.Cycles.Commands.Create;

/// <summary>
/// Handles the creation of production cycles with comprehensive validation and workflow management.
/// </summary>
/// <remarks>
/// This handler is a critical component of the manufacturing execution system that manages the lifecycle
/// of production cycles, including validation of barcode status, workflow compliance, and cycle limits.
/// It implements the gateway pattern for communication with PLC systems and ensures data integrity
/// across the production tracking system.
/// <para>
/// Issue #33 (Chunk 4) — cut off the mutable god-object <c>IBarCodeResult.GetBarCodeDetails</c> onto the
/// stateless <see cref="IBarCodeDetailsLoader"/> + immutable <see cref="BarCodeSnapshot"/> + pure
/// <see cref="BarCodeResultProjection"/>. The §7 PLC surface is preserved byte-identically: the loader returns
/// <c>Success(snapshot)</c> even on a validation failure (the snapshot carries the specific negative
/// <c>ResultValidation</c> code), so the <c>WithFailure</c>-vs-<c>Success</c> decision keeps gating on the
/// snapshot's <c>Error</c> length — NOT on <c>Result.IsSuccess</c>. The post-load
/// <c>UpdateBarCodeInformationOnCycle</c> + <c>SetCycle</c> + <c>ToDto</c> triple becomes one
/// <see cref="BarCodeResultProjection.ToCreateResponse"/>, and the created cycle / CyclesOk / register
/// persistence side effects and their ordering are unchanged. The station validator, the cycle-limit policy and
/// the create-cycle state-machine resolution still consume the frozen <c>IBarCodeResult</c> shape, so they are
/// fed a thin read-only view over the immutable snapshot (<see cref="SnapshotBarCodeResultView"/>) — leaving the
/// god-object, <c>IBarCodeResult</c> and the two collaborators untouched.
/// </para>
/// <para>
/// Issue #178 (epic #174) — the former ~10 consecutive <c>if (result.IsFailure) { log; return Fail(...) }</c>
/// blocks are re-railwayed as ONE fluent <c>BindAsync</c> chain over small provenance-annotated step methods,
/// modeled on the #177 pilot (RejectBarCode/RestoreBarCode). The chain uses only the no-catch in-tree
/// <see cref="ResultExtensions.BindAsync{T,U}"/> combinator (the IQR fluent combinators re-wrap exceptions —
/// the 18/06/2026 revert regression), so a thrown side effect still surfaces via the as-built outer catch with
/// the pinned "Operation finished with an exception" message. The flag-gated §7 failure diagnostics are built
/// through the shared #176 <see cref="GatewayFailureFactory"/> (replacing the private
/// <c>Fail</c>/<c>FailWithoutProjection</c> pair) and carried to the single terminal through a per-request
/// <see cref="GatewayFaultBox"/>, because the value-dropping failure propagation of <c>BindAsync</c> cannot
/// thread the diagnostic DTO itself. Every failure code, log entry, write ordering and the #59 no-write
/// semantics are pinned byte-identically by <c>CreateCyclesGoldenMasterTests</c>.
/// </para>
/// <para>
/// Issue #114 (chunk B) — the former three-transaction saga (cycle INSERT via the aggregate save, barcode
/// UPDATE via a separate <c>BarCodeUpdater</c> auto-commit, audit INSERT via a third) is collapsed to two:
/// the barcode status write now rides the SAME transactional aggregate save as the cycle INSERT (the
/// <see cref="ICycleCreator"/> request carries the resolved flow/part/machine/modified fields and the creator
/// stages them on the loaded root via <c>BarCode.StageStatusWrite</c>). A barcode-write failure therefore
/// rolls the cycle back too — the PLC "failed" response can no longer leave an orphan Started cycle for a
/// retry to duplicate. The §7 request-audit row stays a separate post-commit step ON PURPOSE: it is a
/// different aggregate (<see cref="TaskGatewayRequest"/>), and its failure semantics are unchanged
/// (ExceptionResultValidation with the cycle+barcode write already durable, exactly as-built).
/// </para>
/// </remarks>
public class CreateCyclesCommandHandler(
    ILogger<CreateCyclesCommandHandler> logger,
    IDateTimeMachine dateTimeMachine,
    IBarCodeDetailsLoader barCodeDetailsLoader,
    Validation.IStationValidator stationValidator,
    ICycleLimitPolicy cycleLimitPolicy,
    ICycleCreator cycleCreator,
    IGatewayAuditFactory gatewayAuditFactory,
    IItemStateMachine? stateMachine = null,
    IOptions<StateMachineRoutingOptions>? routingOptions = null) : IGatewayRequestHandler<CreateCyclesCommand, TaskGatewayResponseDto>, IResettable
{
    // Story 3.1: fall back to the as-built engine and default-ON routing when not injected (legacy
    // positional construction in characterization tests), so behavior is identical to DI.
    private readonly IItemStateMachine machine = stateMachine ?? new ItemStateMachine();
    private readonly StateMachineRoutingOptions routing = routingOptions?.Value ?? new StateMachineRoutingOptions();

    /// <summary>
    /// Processes the creation of a production cycle with comprehensive validation and workflow management.
    /// </summary>
    /// <param name="cmd">The command containing cycle creation details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the gateway response with cycle information.</returns>
    /// <remarks>
    /// This method performs critical manufacturing validations including:
    /// - Station type verification (only process stations can start cycles)
    /// - Maximum cycle count enforcement per recipe configuration
    /// - Barcode flow status validation
    /// - Part status tracking and workflow compliance.
    /// </remarks>
    public async Task<Result<TaskGatewayResponseDto>> ProcessAsync(CreateCyclesCommand cmd, CancellationToken cancellationToken)
    {
        if (cmd is null)
        {
            return Result<TaskGatewayResponseDto>.WithFailure("cmd cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<TaskGatewayResponseDto>.WithFailure("Operation was canceled.");
        }

        try
        {
            var request = cmd.Command;

            logger.LogInformation(
                "Starting cycle creation process for MachineId: {MachineId}, BarCode: {BarCode}",
                request.MachineId, request.BarCode);

            // #178: the per-request carrier for the flag-gated §7 failure diagnostic (see GatewayFaultBox docs).
            var faults = new GatewayFaultBox();

            var outcome = await this.LoadSnapshotAsync(request, faults, cancellationToken)
                .BindAsync((context, _) => Task.FromResult(this.ValidateStationStep(context, faults)), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(this.EnforceCycleLimitStep(cmd, context, faults)), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(this.GuardLoadCarriedErrorStep(context, faults)), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(this.ResolveStatusesStep(context)), cancellationToken)
                .BindAsync((context, ct) => this.CreateCycleStepAsync(context, faults, ct), cancellationToken)
                .BindAsync((context, ct) => this.WriteAuditStepAsync(context, faults, ct), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(ProjectResponseStep(context)), cancellationToken);

            // #59: the load-carried validation error is short-circuited BEFORE the writes (the guard step after
            // the cycle-limit gate), so a successful outcome means the snapshot was clean and the create landed.
            // On a step failure the box carries the exact as-built flag-gated failure (value-carrying when
            // SpecificDiagnostics is ON, value-less when OFF); the value-less chain outcome is the fallback for
            // the only box-less failure shape (a mid-chain cancellation short-circuit inside BindAsync).
            return faults.Terminal ?? outcome;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in CreateCyclesCommandHandler");
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 row 11] - unhandled exception maps to the specific
            //ExceptionResultValidation(-131072) carried on a non-null value so the PLC tag carries the real reason.
            //#178: expressed through the shared #176 factory — byte-identical to the previous inline
            //flag-check + BuildDiagnosticResponse pair, including the SpecificDiagnostics OFF value-less rollback.
            return GatewayFailureFactory.FailWithoutProjection(
                this.routing,
                $"Operation finished with an exception {ex.Message}",
                cmd.Command.MachineId,
                ResultValidation.ExceptionResultValidation);
        }
    }

    /// <summary>
    /// Pipeline step 1: loads the immutable <see cref="BarCodeSnapshot"/> and builds the frozen
    /// <see cref="IBarCodeResult"/> view the downstream validators consume. The loader returns
    /// <c>Success(snapshot)</c> even on a validation failure (the snapshot carries the specific negative
    /// <c>ResultValidation</c> + <c>Error</c>). Only a null/exception load yields a failed Result — that is the
    /// equivalent of the god-object path's "GetBarCodeDetails returned nothing" branch.
    /// </summary>
    private async Task<Result<LoadedContext>> LoadSnapshotAsync(TaskGatewayRequest request, GatewayFaultBox faults, CancellationToken cancellationToken)
    {
        var barCodeDetailsRequest = new BarCodeDetailsRequest(request.MachineId, request.BarCode, request.PartNumber);

        var loadResult = await barCodeDetailsLoader.LoadAsync(barCodeDetailsRequest, cancellationToken).ConfigureAwait(false);

        if (loadResult.IsFailure || loadResult.Value is not { } snapshot)
        {
            logger.LogError("CreateCyclesCommandHandler:: Failed to retrieve barcode information. MachineId={MachineId}, BarCode={BarCode}", request.MachineId, request.BarCode);
            //[Fix] CLAUDE Date: 07/07/2026 Reason: [PO-ratified §7 change 2026-07-07] - a FAILED load Result is
            //NOT a genuine barcode-not-found: BarCodeDetailsLoader.LoadAsync returns a failed Result ONLY from
            //its catch block (an exception on the read pipeline) or cancellation/null-request; a genuine
            //not-found instead returns Success(snapshot) with BarCodeNotFound + a non-empty Error and is handled
            //by the load-carried-error guard step below. BarCodeNotFound(-2) is reserved for GENUINE not-found only,
            //so this infrastructure/exception load must surface InfrastructureFailure(-262144), not -2. Honors
            //the SpecificDiagnostics flag for zero-redeploy rollback (AC8).
            return FaultStep<LoadedContext>(
                faults,
                GatewayFailureFactory.FailWithoutProjection(this.routing, "Failed to retrieve barcode information", request.MachineId, ResultValidation.InfrastructureFailure),
                "Failed to retrieve barcode information");
        }

        logger.LogInformation(
            "Retrieved barcode information: BarCodeId={BarCodeId}, PartNumber={PartNumber}, FlowStatus={FlowStatus}, MachineType={MachineType}",
            snapshot.BarCodeId, snapshot.PartNumber, snapshot.FlowStatus, snapshot.MachineType);

        // The station validator, cycle-limit policy and create-cycle state-machine resolution still consume the
        // frozen IBarCodeResult shape; feed them a read-only view over the immutable snapshot (no mutation).
        return Result<LoadedContext>.Success(new LoadedContext(request, snapshot, new SnapshotBarCodeResultView(snapshot)));
    }

    /// <summary>
    /// Pipeline step 2: just the process station can start cycles — any other station is refused here.
    /// </summary>
    private Result<LoadedContext> ValidateStationStep(LoadedContext context, GatewayFaultBox faults)
    {
        var stationValidationResult = stationValidator.ValidateCanStartCycles(context.View);
        if (stationValidationResult is { IsFailure: true })
        {
            logger.LogWarning("CreateCyclesCommandHandler:: Only process stations can start cycles. MachineId={MachineId}, MachineType={MachineType}", context.Request.MachineId, context.Snapshot.MachineType);
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3] - station-cannot-start-cycle maps to the
            //specific InvalidMachine(-4096) so the PLC tag carries why the cycle was refused (AC2 row 9).
            var error = stationValidationResult.Error ?? "Station validation failed";
            return FaultStep<LoadedContext>(
                faults,
                GatewayFailureFactory.Fail(this.routing, error, context.Snapshot, ResultValidation.InvalidMachine),
                error);
        }

        return Result<LoadedContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 3: evaluates the cycle-limit policy — SKIPPED entirely when the barcode's flow status is
    /// <see cref="FlowStatus.Restored"/> (as-built restore exemption). An evaluation failure and a denial carry
    /// different codes (see the per-branch provenance below).
    /// </summary>
    private Result<LoadedContext> EnforceCycleLimitStep(CreateCyclesCommand cmd, LoadedContext context, GatewayFaultBox faults)
    {
        // Check if the barcode does not have FlowStatus == FlowStatus.Restored
        if (Equals(context.Snapshot.FlowStatus, FlowStatus.Restored))
        {
            return Result<LoadedContext>.Success(context);
        }

        // Evaluate cycle limits using the policy service
        var cycleLimitResult = cycleLimitPolicy.EvaluateCycleLimits(context.View, cmd);
        if (cycleLimitResult is { IsFailure: true })
        {
            logger.LogError("CreateCyclesCommandHandler:: Cycle limit policy evaluation failed. Error={Error}", cycleLimitResult.Error);
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 FIX 3] - the cycle-limit policy could not be
            //evaluated; carry the specific PartRejected(-2048) (the cycle-limit failure family) so the
            //projection-carrying DTO is not silently collapsed to -1 (it had no negative code before).
            var error = cycleLimitResult.Error ?? "Cycle limit evaluation failed";
            return FaultStep<LoadedContext>(
                faults,
                GatewayFailureFactory.Fail(this.routing, error, context.Snapshot, ResultValidation.PartRejected),
                error);
        }

        if (cycleLimitResult.Value is { IsAllowed: false } cycleLimitDecision)
        {
            logger.LogWarning("CreateCyclesCommandHandler:: Cycle limit policy rejected creation. MachineId={MachineId}, BarCodeId={BarCodeId}, Reason={Reason}", context.Request.MachineId, context.Snapshot.BarCodeId, cycleLimitDecision.Reason);
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3] - the cycle-limit policy already produces a
            //specific ValidationResult (e.g. PartRejected); publish THAT specific negative code to the tag.
            //The god-object path set barCodeInfo.ResultValidation before projecting; reproduce that with a
            //`with` so the projected DTO carries the same code (byte-identical, Promote overrides to the same).
            return FaultStep<LoadedContext>(
                faults,
                GatewayFailureFactory.Fail(this.routing, cycleLimitDecision.Reason, context.Snapshot with { ResultValidation = cycleLimitDecision.ValidationResult }, cycleLimitDecision.ValidationResult),
                cycleLimitDecision.Reason);
        }

        return Result<LoadedContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 4 — #59 (traceability integrity): a load-carried validation error must short-circuit BEFORE
    /// any cycle / barcode / audit write. The loader returns Success(snapshot) even when the load-time arrival
    /// or flow gate soft-rejected (e.g. DestinationNotValid, WorkFlowNotValid): the snapshot carries the
    /// negative ResultValidation plus a non-empty Error. Persisting a cycle + barcode update + audit for a
    /// REFUSED part would leave a phantom traceability record that diverges from physical reality (the part
    /// was rejected, yet the DB shows it processed). The invariant itself lives on the snapshot
    /// (<see cref="BarCodeSnapshot.RequireNoLoadCarriedError"/>); this step owns the log + §7 mapping.
    /// </summary>
    private Result<LoadedContext> GuardLoadCarriedErrorStep(LoadedContext context, GatewayFaultBox faults)
    {
        var guard = context.Snapshot.RequireNoLoadCarriedError();
        if (guard is { IsFailure: true })
        {
            logger.LogWarning(
                "CreateCyclesCommandHandler:: Load-carried validation error; refusing to persist. MachineId={MachineId}, BarCodeId={BarCodeId}, Validation={Validation}, Error={Error}",
                context.Snapshot.MachineId, context.Snapshot.BarCodeId, context.Snapshot.ResultValidation, context.Snapshot.Error);
            //[Fix] CLAUDE Date: 07/07/2026 Reason: [PO-ratified §7 change 2026-07-07] - the snapshot ALREADY
            //carries the SPECIFIC load-carried code (DestinationNotValid/WorkFlowNotValid/PartNumberNotValid/...);
            //surface THAT specific code instead of collapsing every soft-reject to a misleading generic
            //BarCodeNotFound(-2). Priority preserved: when the load-carried reason genuinely IS a not-found,
            //snapshot.ResultValidation is BarCodeNotFound(-2) already, so -2 still wins for that path.
            var error = guard.Error ?? string.Empty;
            return FaultStep<LoadedContext>(
                faults,
                GatewayFailureFactory.Fail(this.routing, error, context.Snapshot, context.Snapshot.ResultValidation),
                error);
        }

        return Result<LoadedContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 5 — Story 3.1 (AC2): obtains FlowStatus/CycleStatus/PartStatus from the CreateCycleAsync
    /// transition instead of the inline <c>var flowStatus = FlowStatus.InProcess;</c> literal. Never fails —
    /// a machine rejection preserves the legacy statuses and the create CONTINUES (as-built fallback).
    /// </summary>
    private Result<ResolvedContext> ResolveStatusesStep(LoadedContext context)
    {
        var (flowStatus, cycleStatus, partStatus) = this.ResolveCreateCycleStatuses(context.Snapshot, context.Request);
        return Result<ResolvedContext>.Success(new ResolvedContext(context.Request, context.Snapshot, flowStatus, cycleStatus, partStatus));
    }

    /// <summary>
    /// Pipeline step 6: creates the cycle row via the cycle creator service. Both the failed create and the
    /// null created cycle are refused with the cycle-step guard code. #114 chunk B: the request also carries
    /// the barcode ROOT status write (the resolved FlowStatus/PartStatus, the request machine and the
    /// modified stamp — exactly the fields the retired separate <c>BarCodeUpdater</c> step assigned), so the
    /// creator persists the cycle INSERT and the barcode UPDATE in ONE transactional aggregate save. A
    /// failure of EITHER write therefore rolls back BOTH and surfaces here as the cycle-step failure — the
    /// PLC "failed" response can no longer coexist with a committed orphan Started cycle for a retry to
    /// duplicate.
    /// </summary>
    private async Task<Result<PersistContext>> CreateCycleStepAsync(ResolvedContext context, GatewayFaultBox faults, CancellationToken cancellationToken)
    {
        // Create cycle using the cycle creator service
        var cycleCreateRequest = new CycleCreateRequest(
            context.Request.MachineId,
            context.Snapshot.BarCodeId,
            context.CycleStatus.Value,
            context.PartStatus.Value,
            dateTimeMachine.Now.ToLocalTime(),
            dateTimeMachine.Now.ToLocalTime(),
            context.FlowStatus.Value,
            dateTimeMachine.Now.ToLocalTime());

        var cycleCreationResult = await cycleCreator.CreateAsync(cycleCreateRequest, cancellationToken);
        if (cycleCreationResult is { IsFailure: true })
        {
            logger.LogError("CreateCyclesCommandHandler:: Failed to create cycle. Error={Error}", cycleCreationResult.Error);
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 FIX 3] - the cycle could not be created; carry the
            //specific CycleNotFound(-16) (nearest guard code for the cycle persistence step) so the DTO does not
            //collapse to -1.
            var error = cycleCreationResult.Error ?? "Failed to create cycle";
            return FaultStep<PersistContext>(
                faults,
                GatewayFailureFactory.Fail(this.routing, error, context.Snapshot, ResultValidation.CycleNotFound),
                error);
        }

        if (cycleCreationResult.Value is not { } cycle)
        {
            logger.LogError("CreateCyclesCommandHandler:: Created cycle is null");
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 FIX 3] - created cycle is null; same cycle-step
            //family as above -> CycleNotFound(-16).
            return FaultStep<PersistContext>(
                faults,
                GatewayFailureFactory.Fail(this.routing, "Created cycle is null", context.Snapshot, ResultValidation.CycleNotFound),
                "Created cycle is null");
        }

        return Result<PersistContext>.Success(new PersistContext(context.Request, context.Snapshot, context.FlowStatus, context.CycleStatus, context.PartStatus, cycle));
    }

    /// <summary>
    /// Pipeline step 7: writes the gateway audit entry. The god-object path read these fields off the
    /// barCodeInfo AFTER UpdateBarCodeInformationOnCycle(flow, part, cycle) + SetCycle(cycle) mutated them, so
    /// the audit carries the post-create statuses + created cycle id — reproduced here from the resolved
    /// context + the created cycle (snapshot scalars for the unmutated fields).
    /// </summary>
    private async Task<Result<PersistContext>> WriteAuditStepAsync(PersistContext context, GatewayFaultBox faults, CancellationToken cancellationToken)
    {
        var gatewayAuditRequest = new GatewayAuditRequest(
            context.Snapshot.MachineId,
            context.Snapshot.BarCodeId,
            context.Cycle.CycleId.Value,
            context.CycleStatus,
            context.PartStatus,
            context.FlowStatus,
            context.Snapshot.ResultValidation,
            GatewayTask.CreateCycleAsync,
            dateTimeMachine.Now.ToLocalTime());

        var auditResult = await gatewayAuditFactory.CreateAuditEntryAsync(gatewayAuditRequest, cancellationToken);
        if (auditResult is { IsFailure: true })
        {
            logger.LogError("CreateCyclesCommandHandler:: Failed to create gateway audit entry. Error={Error}", auditResult.Error);
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 FIX 3] - the gateway audit write failed; this is an
            //unexpected infrastructure failure with no dedicated guard code -> ExceptionResultValidation(-131072),
            //so the DTO carries a negative code instead of collapsing to -1.
            var error = auditResult.Error ?? "Failed to create gateway audit entry";
            return FaultStep<PersistContext>(
                faults,
                GatewayFailureFactory.Fail(this.routing, error, context.Snapshot, ResultValidation.ExceptionResultValidation),
                error);
        }

        return Result<PersistContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 8 (terminal projection) — #33 Chunk 4: UpdateBarCodeInformationOnCycle(flow, part, cycle) +
    /// SetCycle(cycle) + ToDto collapse into one immutable projection — byte-identical to the god-object
    /// write-back-then-project path.
    /// </summary>
    private static Result<TaskGatewayResponseDto> ProjectResponseStep(PersistContext context)
    {
        var result = BarCodeResultProjection.ToCreateResponse(context.Snapshot, context.FlowStatus, context.PartStatus, context.CycleStatus, context.Cycle);

        // #32 C2: stamp the §7 References via the pure ReferenceStamper (replaces the retired instance method).
        var stampedCreate = ReferenceStamper.Apply(result);
        return Result<TaskGatewayResponseDto>.Success(stampedCreate.Value ?? result);
    }

    /// <summary>
    /// #178 failure terminal for a pipeline step: records the exact flag-gated §7 failure (built by the shared
    /// #176 <see cref="GatewayFailureFactory"/>) on the per-request <see cref="GatewayFaultBox"/> and returns
    /// the chain-typed failure carrying the same error text. The chain short-circuits at the first failure, so
    /// at most one terminal is ever recorded per request.
    /// </summary>
    /// <typeparam name="T">The pipeline context type of the failing step.</typeparam>
    private static Result<T> FaultStep<T>(GatewayFaultBox faults, Result<TaskGatewayResponseDto> terminal, string error)
    {
        faults.Terminal = terminal;
        return Result<T>.WithFailure(error);
    }

    /// <summary>
    /// Story 3.1 (AC2): resolves the post-create-cycle <see cref="FlowStatus"/>/<see cref="CycleStatus"/>/
    /// <see cref="PartStatus"/> from the <see cref="GatewayTask.CreateCycleAsync"/> transition, replacing the
    /// inline <c>var flowStatus = FlowStatus.InProcess;</c> literal. The fail-closed lookup flags that this
    /// trigger's guard requires (BarCode + Machine) are set from this handler's OWN successful lookups: the
    /// barcode was retrieved by the loader and the station was accepted by the <c>stationValidator</c>/cycle-limit
    /// policy before this point. Cycle/part status are the request's values (mirrored by the machine). When the
    /// flag is OFF, or the machine rejects, the legacy values are preserved.
    /// </summary>
    /// <remarks>
    /// #178 byte-identity finding — the issue's AC 2 (<c>barCode.CreateCycle(context)</c> domain route) is NOT
    /// adopted here, deliberately: (1) <c>PlcHandlerStateMachineRoutingTests</c> pins that the handler fires its
    /// OWN injected <see cref="IItemStateMachine"/> (a test spy must be the machine fired), while the domain
    /// seam fires the entity's internal engine; (2) <c>BarCode.FireAndApply</c> MUTATES the loaded (possibly
    /// EF-tracked) entity on success, while this as-built resolution is PURE — the statuses reach persistence
    /// only through the BarCodeUpdater; (3) on a machine rejection the as-built path falls back to the legacy
    /// statuses and CONTINUES, while the domain seam fails the transition. Golden preservation (AC 1) outranks
    /// AC 2 per the ratified pilot precedent.
    /// </remarks>
    private (FlowStatus FlowStatus, CycleStatus CycleStatus, PartStatus PartStatus) ResolveCreateCycleStatuses(BarCodeSnapshot snapshot, TaskGatewayRequest request)
    {
        var legacyFlow = FlowStatus.InProcess;
        var requestCycle = request.CycleStatus;
        var requestPart = request.PartStatus;

        if (!routing.RouteCreateCycle)
        {
            return (legacyFlow, requestCycle, requestPart);
        }

        // Story 27.2b-2: snapshot.BarCode is nullable ("no part" = absent). This resolution runs only on the found
        // path; an absent barcode preserves the legacy status fallback (byte-equal to the machine-rejected branch).
        if (snapshot.BarCode is null)
        {
            return (legacyFlow, requestCycle, requestPart);
        }

        var transitionContext = new TransitionContext(
            snapshot.MachineType,
            requestCycle,
            requestPart,
            CycleTime: 0,
            Recipe: snapshot.Recipe,
            BarCodeFound: true,
            MachineFound: true);

        var outcome = machine.Fire(snapshot.BarCode, GatewayTask.CreateCycleAsync, transitionContext);
        if (outcome.IsFailure || outcome.Value is null)
        {
            logger.LogWarning(
                "CreateCycle: state machine rejected CreateCycleAsync ({Errors}); preserving legacy InProcess + request status.",
                string.Join("; ", outcome.Errors));
            return (legacyFlow, requestCycle, requestPart);
        }

        return (outcome.Value.NextFlowStatus, outcome.Value.NextCycleStatus, outcome.Value.NextPartStatus);
    }

    /// <summary>
    /// Resets the handler state for reuse in pooled scenarios.
    /// </summary>
    /// <returns>True indicating successful reset.</returns>
    public bool TryReset()
    {
        return true;
    }

    /// <summary>
    /// #178: pipeline context after the load step — the immutable request/snapshot pair plus the frozen
    /// <see cref="IBarCodeResult"/> view fed to the station validator and the cycle-limit policy.
    /// </summary>
    private sealed record LoadedContext(TaskGatewayRequest Request, BarCodeSnapshot Snapshot, SnapshotBarCodeResultView View);

    /// <summary>
    /// #178: pipeline context after the status-resolution step — carries the machine-resolved (or legacy
    /// fallback) FlowStatus/CycleStatus/PartStatus tuple the write steps consume.
    /// </summary>
    private sealed record ResolvedContext(TaskGatewayRequest Request, BarCodeSnapshot Snapshot, FlowStatus FlowStatus, CycleStatus CycleStatus, PartStatus PartStatus);

    /// <summary>
    /// #178: pipeline context after the cycle-create step — adds the created cycle for the audit + projection.
    /// </summary>
    private sealed record PersistContext(TaskGatewayRequest Request, BarCodeSnapshot Snapshot, FlowStatus FlowStatus, CycleStatus CycleStatus, PartStatus PartStatus, Cycle Cycle);

    /// <summary>
    /// #178: per-request carrier for the flag-gated §7 failure diagnostic. The in-tree no-catch
    /// <see cref="ResultExtensions.BindAsync{T,U}"/> combinator propagates a failed link as a VALUE-LESS
    /// failure (only the errors survive), so the diagnostic-carrying <c>Result&lt;TaskGatewayResponseDto&gt;</c>
    /// a failing step builds (Story 3.3 AC3/AC8 — value-carrying when <c>SpecificDiagnostics</c> is ON,
    /// value-less when OFF) cannot ride the chain itself; it rides this box to the single terminal instead.
    /// Instantiated per ProcessAsync call — never shared, so the captive-dependency race class cannot recur.
    /// </summary>
    private sealed class GatewayFaultBox
    {
        /// <summary>Gets or sets the exact flag-gated failure the single failing step recorded, if any.</summary>
        public Result<TaskGatewayResponseDto>? Terminal { get; set; }
    }

    /// <summary>
    /// Issue #33 (Chunk 4): a thin, read-only <see cref="IBarCodeResult"/> view over an immutable
    /// <see cref="BarCodeSnapshot"/>. It exists solely to feed the frozen <c>IBarCodeResult</c>-typed
    /// collaborators (<see cref="Validation.IStationValidator"/>, <see cref="ICycleLimitPolicy"/>) without
    /// touching them or the god-object. Every getter delegates to the snapshot; the mutators are inert (the
    /// handler never writes through this view — the immutable projection replaces the old write-back), so there
    /// is no shared mutable state and the captive-dependency race cannot recur through this seam.
    /// </summary>
    private sealed class SnapshotBarCodeResultView(BarCodeSnapshot snapshot) : IBarCodeResult
    {
        public int MachineId => snapshot.MachineId;

        public int BarCodeId => snapshot.BarCodeId;

        public int CycleId => snapshot.CycleId;

        public int CyclesOk => snapshot.CyclesOk;

        public int ShiftId => snapshot.ShiftId;

        public int CommandId => snapshot.CommandId;

        public ResultValidation ResultValidation => snapshot.ResultValidation;

        public string? Error => snapshot.Error;

        public string? Label => snapshot.Label;

        public string? PartNumber => snapshot.PartNumber;

        public Product Product => snapshot.Product;

        public string Description => snapshot.Description ?? string.Empty;

        public int LastMachineId => snapshot.LastMachineId;

        public int NextMachineId => snapshot.NextMachineId;

        public int RegistersSaved => snapshot.RegistersSaved;

        public CycleStatus CycleStatus => snapshot.CycleStatus;

        public FlowStatus FlowStatus => snapshot.FlowStatus;

        public PartStatus PartStatus => snapshot.PartStatus;

        public MachineType MachineType => snapshot.MachineType;

        public WorkFlowType WorkFlowType => snapshot.WorkFlowType;

        public Recipe Recipe => snapshot.Recipe;

        public Cycle Cycle => snapshot.Cycle;

        public IEnumerable<Cycle> Cycles => snapshot.Cycles;

        public BarCode? BarCode => snapshot.BarCode;

        public MasterLabel MasterLabel => snapshot.MasterLabel;

        public IDictionary<string, Register> References => snapshot.References;

        public Task<IBarCodeResult> GetBarCodeDetails(BarCodeDetailsRequest barCodeDetailsRequest, CancellationToken cancellationToken)
            => Task.FromResult<IBarCodeResult>(this);
    }
}
