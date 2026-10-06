// <copyright file="CancelCycleCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.CancelCycle;

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Models.Extensions;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Config;
using Microsoft.Extensions.Options;

/// <summary>
/// Story 5.3 — WEBAPP/monitor handler that wires the config-gated <see cref="GatewayTask.Cancel"/>
/// completeness trigger (Story 4.1) through the Epic-2 <see cref="IItemStateMachine"/>. With the
/// <c>EnableCanceledState</c> gate ON a LEGAL Cancel (item is <c>InProcess</c> and the latest cycle is
/// <see cref="CycleStatus.Started"/>) resolves <c>NextCycleStatus = Canceled</c> and is persisted ON THE
/// CYCLE. With the gate OFF (DEFAULT, fail-closed) <c>Fire</c> default-rejects the trigger, NOTHING is
/// persisted (PRD NFR4). The trigger is OFF the PLC bus (NFR1).
/// </summary>
/// <remarks>
/// #95 Phase 2 Slice E: the resolved cycle-status write goes through the BarCode aggregate
/// (<see cref="BarCode.StageCycleStatusUpdate"/> on the loaded barcode root →
/// <see cref="IAggregateRepository{TRoot}"/>.<c>SaveAsync</c> — a member-only save that never writes the
/// barcode ROOT row), replacing the raw <c>IRepository&lt;Cycle&gt;.UpdateAsync</c>. The injected
/// <see cref="IRepository{T}"/> of <see cref="Cycle"/> remains for the latest-cycle READ only (the top-1
/// paged query — downgrading the read injectors is deferred to the #114 slice). The best-effort
/// <see cref="TaskGatewayRequest"/> audits stay OUTSIDE the aggregate transaction, exactly as before.
/// </remarks>
public class CancelCycleCommandHandler(
    IRepository<BarCode> barCodeRepository,
    IRepository<TaskGatewayRequest> repositoryCommand,
    IReadOnlyRepository<Cycle> repositoryCycles,
    IAggregateRepository<BarCode> barCodeAggregateRepository,
    IDateTimeMachine dateTimeMachine,
    IItemStateMachine? stateMachine = null,
    IOptions<StateMachineRoutingOptions>? routingOptions = null,
    ILogger<CancelCycleCommandHandler>? logger = null)
    : IMonitorRequestHandler<CancelCycleCommand, CycleCanceledView>
{
    private readonly IItemStateMachine machine = stateMachine ?? new ItemStateMachine();

    // Map the Application-layer EnableCanceledState flag onto the Domain completeness gate. OFF (default,
    // fail-closed) ⇒ Disabled ⇒ the gate guard rejects Cancel. ON ⇒ the gated Cancel transition resolves
    // NextCycleStatus = Canceled when CycleStartedGuard also passes (Story 4.1).
    private readonly CompletenessOptions completeness =
        (routingOptions?.Value?.EnableCanceledState ?? false)
            ? new CompletenessOptions { EnableCanceledState = true }
            : CompletenessOptions.Disabled;

    /// <summary>
    /// Processes a <see cref="CancelCycleCommand"/>: finds the barcode by label and its latest cycle, then
    /// fires <see cref="GatewayTask.Cancel"/> through the machine BEFORE any mutation. On a gated/illegal
    /// rejection NOTHING is persisted and a value-less failure carrying the specific code is returned; on
    /// success the resolved <see cref="CycleStatus"/> is persisted on the cycle.
    /// </summary>
    /// <param name="request">The command containing the barcode label.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Result with the canceled-cycle view on success or failure information.</returns>
    // #180 (epic #174): re-railwayed as a single fluent chain over the shared completeness pipeline steps,
    // PRESERVING the as-built behavior byte-identically: the chain uses only no-catch combinators
    // (exceptions still surface via the outer catch with the exact message below), and RequireFound keeps
    // both the barcode and the latest cycle mandatory (missing/null/failed lookups fail).
    public async Task<Result<CycleCanceledView>> ProcessAsync(CancelCycleCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<CycleCanceledView>.WithFailure("Operation was canceled.");
        }

        if (request is null)
        {
            return Result<CycleCanceledView>.WithFailure("request cannot be null.");
        }

        try
        {
            return await CompletenessPipelineSteps.FindBarCodeAsync(barCodeRepository, request.Label, cancellationToken)
                .BindAsync((barcode, ct) => this.FindRequiredCycleAsync(request, barcode, ct), cancellationToken)
                .BindAsync((context, ct) => this.ResolveTransitionAsync(request, context, ct), cancellationToken)
                .BindAsync((context, ct) => this.PersistStagedCycleAsync(context, ct), cancellationToken)
                .BindAsync((context, ct) => this.WriteSuccessAuditAsync(request, context, ct), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(CycleCanceledView.ToDto(context.Cycle)), cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<CycleCanceledView>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }

    /// <summary>
    /// Pipeline step 2: the most recent cycle is REQUIRED for cancellation (mirrors the Reject
    /// latest-cycle pattern) — a missing/null/failed cycle fails the operation.
    /// </summary>
    private async Task<Result<CancelContext>> FindRequiredCycleAsync(CancelCycleCommand request, BarCode barcode, CancellationToken cancellationToken)
    {
        var cycleResult = await CompletenessPipelineSteps.RequireFound(
            CompletenessPipelineSteps.FindLatestCycleAsync(repositoryCycles, barcode, cancellationToken),
            $"Cycles for BarCode {request.Label} not found");
        return cycleResult.Bind(cycle => Result<CancelContext>.Success(new CancelContext(barcode, cycle)));
    }

    /// <summary>
    /// Pipeline step 3: resolve the Cancel transition through the machine BEFORE any mutation or
    /// persistence. The Cancel row carries CompletenessGateGuard(EnableCanceledState) + CycleStartedGuard.
    /// The latter requires CycleStatus == Started, so the context MUST carry the cycle's REAL status
    /// (a non-Started cycle is rejected even when the gate is ON).
    /// </summary>
    private async Task<Result<CancelContext>> ResolveTransitionAsync(CancelCycleCommand request, CancelContext context, CancellationToken cancellationToken)
    {
        var transitionContext = new TransitionContext(
            MachineType.Process,
            context.Cycle.CycleStatus,
            context.BarCode.PartStatus,
            CycleTime: 0,
            Recipe: null,
            BarCodeFound: true,
            Completeness: this.completeness);

        var outcome = this.machine.Fire(context.BarCode, GatewayTask.Cancel, transitionContext);

        // The failure fork must read the SPECIFIC code carried on the FAILED outcome's value
        // (outcome.Value?.Result) — a failure-value carry the success-biased combinators cannot express,
        // so the three-way fork stays an explicit pattern match.
        return await (outcome switch
        {
            { IsSuccess: true, Value: { } transition } => Task.FromResult(StageTransition(context, transition)),
            { IsSuccess: true } => Task.FromResult(Result<CancelContext>.WithFailure(
                $"Cancel for BarCode {request.Label} succeeded without a transition outcome.")),
            _ => this.AuditIllegalTransitionAsync(request, context, outcome.Value?.Result ?? ResultValidation.OperationCancelled, cancellationToken),
        });
    }

    /// <summary>
    /// Pipeline step 4 — Story 5.3 (review fix): a persistence failure fails the operation (railway),
    /// mirroring Story 5.1. #95 Slice E: <c>SaveAsync</c> is the member-only single-flush aggregate save
    /// that never writes the barcode ROOT row (Cancel does NOT touch <c>barcode.ModifiedOn</c>).
    /// </summary>
    private async Task<Result<CancelContext>> PersistStagedCycleAsync(CancelContext context, CancellationToken cancellationToken)
    {
        var persisted = await barCodeAggregateRepository.SaveAsync(context.BarCode, cancellationToken);
        if (persisted.IsFailure)
        {
            return Result<CancelContext>.WithFailure(persisted.Errors);
        }

        return Result<CancelContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 5: best-effort traceability audit of the legal Cancel operation.
    /// #65: best-effort success audit — keep the canceled-cycle view byte-identical, but a dropped audit
    /// row must not be silent. Log-and-flag the discarded Result.
    /// </summary>
    private async Task<Result<CancelContext>> WriteSuccessAuditAsync(CancelCycleCommand request, CancelContext context, CancellationToken cancellationToken)
    {
        await CompletenessPipelineSteps.WriteAuditAsync(
            repositoryCommand,
            context.BarCode,
            context.Cycle,
            ResultValidation.None,
            GatewayTask.Cancel,
            dateTimeMachine,
            error => logger?.LogError(
                "Cancel success-audit write did not land for BarCode {Label}: {Error}",
                request.Label, error),
            cancellationToken);

        return Result<CancelContext>.Success(context);
    }

    /// <summary>
    /// Gate OFF or illegal source/cycle: persist NOTHING. Record the specific ResultValidation on an
    /// audit TaskGatewayRequest, then return a value-less failure carrying that code.
    /// #65: this path already returns a SPECIFIC failure (the state-machine code); do not clobber it
    /// by propagating an audit-write fault. Log-and-flag the dropped audit row.
    /// </summary>
    private async Task<Result<CancelContext>> AuditIllegalTransitionAsync(CancelCycleCommand request, CancelContext context, ResultValidation cancelCode, CancellationToken cancellationToken)
    {
        await CompletenessPipelineSteps.WriteAuditAsync(
            repositoryCommand,
            context.BarCode,
            context.Cycle,
            cancelCode,
            GatewayTask.Cancel,
            dateTimeMachine,
            error => logger?.LogError(
                "Cancel rejection-audit write did not land for BarCode {Label}: {Error}",
                request.Label, error),
            cancellationToken);

        return Result<CancelContext>.WithFailure(
            $"Cancel rejected by state machine for BarCode {request.Label}: ({context.BarCode.FlowStatus.Name}, {GatewayTask.Cancel.Name}) -> {cancelCode.Name} ({cancelCode.Value})");
    }

    /// <summary>
    /// Story 6.4 + #95 Slice E: machine fired above (own/injected); route the resolved cycle-status
    /// write through the AGGREGATE — StageCycleStatusUpdate applies the resolved status via the trusted
    /// Cycle.ApplyCycleStatus seam (byte-equal to the retired inline apply) and stages the cycle for the
    /// member-only single-flush save. A staging refusal persists NOTHING.
    /// </summary>
    private static Result<CancelContext> StageTransition(CancelContext context, TransitionOutcome transition)
    {
        var staged = context.BarCode.StageCycleStatusUpdate(context.Cycle, transition.NextCycleStatus);
        if (staged.IsFailure)
        {
            return Result<CancelContext>.WithFailure(staged.Errors);
        }

        return Result<CancelContext>.Success(context);
    }

    /// <summary>
    /// Immutable pipeline context threading the resolved barcode and its mandatory most-recent cycle
    /// through the cancellation chain.
    /// </summary>
    private sealed record CancelContext(BarCode BarCode, Cycle Cycle);
}
