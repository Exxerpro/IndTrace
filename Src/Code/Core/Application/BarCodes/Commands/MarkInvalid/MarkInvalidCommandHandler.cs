// <copyright file="MarkInvalidCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.MarkInvalid;

using IndTrace.Application.Models.Extensions;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Config;
using Microsoft.Extensions.Options;

/// <summary>
/// Story 5.3 — WEBAPP/monitor handler that wires the config-gated <see cref="GatewayTask.MarkInvalid"/>
/// completeness trigger (Story 4.1) through the Epic-2 <see cref="IItemStateMachine"/>. With the
/// <c>EnableInvalidState</c> gate ON a LEGAL MarkInvalid (item is <c>Created</c>/<c>InProcess</c>) resolves
/// <see cref="FlowStatus.Invalid"/> and is persisted. With the gate OFF (DEFAULT, fail-closed) <c>Fire</c>
/// default-rejects the trigger with a specific <see cref="ResultValidation"/>, NOTHING is persisted, and no
/// new state is reachable (PRD NFR4). The trigger is OFF the PLC bus (NFR1).
/// </summary>
public class MarkInvalidCommandHandler(
    IRepository<BarCode> barCodeRepository,
    IRepository<TaskGatewayRequest> repositoryCommand,
    IReadOnlyRepository<Cycle> repositoryCycles,
    IDateTimeMachine dateTimeMachine,
    IItemStateMachine? stateMachine = null,
    IOptions<StateMachineRoutingOptions>? routingOptions = null,
    ILogger<MarkInvalidCommandHandler>? logger = null)
    : IMonitorRequestHandler<MarkInvalidCommand, BarCodeMarkedInvalidView>
{
    // Mirror Restore/Reject: when no machine/options are injected (legacy positional construction), fall back
    // to the as-built engine and default routing so behavior matches DI (CR1).
    private readonly IItemStateMachine machine = stateMachine ?? new ItemStateMachine();

    // Map the Application-layer EnableInvalidState flag onto the Domain completeness gate. OFF (default,
    // fail-closed) ⇒ CompletenessOptions.Disabled ⇒ the gate guard rejects the MarkInvalid trigger and no
    // new state is reachable. ON ⇒ the gated (Created|InProcess) → Invalid transition resolves (Story 4.1).
    private readonly CompletenessOptions completeness =
        (routingOptions?.Value?.EnableInvalidState ?? false)
            ? new CompletenessOptions { EnableInvalidState = true }
            : CompletenessOptions.Disabled;

    /// <summary>
    /// Processes a <see cref="MarkInvalidCommand"/>: finds the barcode by label and fires
    /// <see cref="GatewayTask.MarkInvalid"/> through the machine BEFORE any mutation. On a gated/illegal
    /// rejection NOTHING is persisted and a value-less failure carrying the specific code is returned; on
    /// success the resolved <see cref="FlowStatus"/> is persisted.
    /// </summary>
    /// <param name="request">The command containing the barcode label.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Result with the marked-invalid view on success or failure information.</returns>
    // #180 (epic #174): re-railwayed as a single fluent chain over the shared completeness pipeline steps,
    // PRESERVING the as-built behavior byte-identically: the chain uses only no-catch combinators
    // (exceptions still surface via the outer catch with the exact message below), and RequireFound keeps
    // the barcode mandatory (a missing/null/failed lookup fails).
    public async Task<Result<BarCodeMarkedInvalidView>> ProcessAsync(MarkInvalidCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeMarkedInvalidView>.WithFailure("Operation was canceled.");
        }

        if (request is null)
        {
            return Result<BarCodeMarkedInvalidView>.WithFailure("request cannot be null.");
        }

        try
        {
            return await CompletenessPipelineSteps.FindBarCodeAsync(barCodeRepository, request.Label, cancellationToken)
                .BindAsync((barcode, ct) => this.ResolveTransitionAsync(request, barcode, ct), cancellationToken)
                .BindAsync((context, ct) => this.PersistStatusAsync(context, ct), cancellationToken)
                .BindAsync((context, ct) => this.WriteSuccessAuditAsync(context, ct), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(BarCodeMarkedInvalidView.ToDto(context.BarCode)), cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<BarCodeMarkedInvalidView>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }

    /// <summary>
    /// Pipeline step 2: resolve the MarkInvalid transition through the machine BEFORE any mutation or
    /// persistence. The MarkInvalid rows (Created|InProcess) carry only a CompletenessGateGuard, so with
    /// the gate OFF (default) the pair default-rejects exactly like an absent row. Mirror the item's
    /// current cycle/part status into the context (the gate guard does not read them).
    /// </summary>
    private async Task<Result<MarkInvalidContext>> ResolveTransitionAsync(MarkInvalidCommand request, BarCode barcode, CancellationToken cancellationToken)
    {
        var transitionContext = new TransitionContext(
            MachineType.Process,
            CycleStatus.None,
            barcode.PartStatus,
            CycleTime: 0,
            Recipe: null,
            BarCodeFound: true,
            Completeness: this.completeness);

        var outcome = this.machine.Fire(barcode, GatewayTask.MarkInvalid, transitionContext);

        // The failure fork must read the SPECIFIC code carried on the FAILED outcome's value
        // (outcome.Value?.Result) — a failure-value carry the success-biased combinators cannot express,
        // so the three-way fork stays an explicit pattern match.
        return await (outcome switch
        {
            { IsSuccess: true, Value: { } transition } => Task.FromResult(ApplyTransition(barcode, transition)),
            { IsSuccess: true } => Task.FromResult(Result<MarkInvalidContext>.WithFailure(
                $"MarkInvalid for BarCode {request.Label} succeeded without a transition outcome.")),
            _ => this.AuditIllegalTransitionAsync(request, barcode, outcome.Value?.Result ?? ResultValidation.OperationCancelled, cancellationToken),
        });
    }

    /// <summary>
    /// Pipeline step 3 — Story 5.3 (review fix): a persistence failure fails the operation (railway),
    /// mirroring Story 5.1.
    /// </summary>
    private async Task<Result<MarkInvalidContext>> PersistStatusAsync(MarkInvalidContext context, CancellationToken cancellationToken)
    {
        context.BarCode.ModifiedOn = dateTimeMachine.Now.ToLocalTime();

        var persisted = await barCodeRepository.UpdateAsync(context.BarCode, cancellationToken);
        if (persisted.IsFailure)
        {
            return Result<MarkInvalidContext>.WithFailure(persisted.Errors);
        }

        return Result<MarkInvalidContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 4: best-effort traceability audit of the legal MarkInvalid operation
    /// (TryAudit semantics — written only when a latest cycle exists).
    /// </summary>
    private async Task<Result<MarkInvalidContext>> WriteSuccessAuditAsync(MarkInvalidContext context, CancellationToken cancellationToken)
    {
        await this.TryAuditAsync(context.BarCode, ResultValidation.None, GatewayTask.MarkInvalid, cancellationToken);
        return Result<MarkInvalidContext>.Success(context);
    }

    /// <summary>
    /// Gate OFF or illegal source: persist NOTHING. Record the specific ResultValidation on an audit
    /// TaskGatewayRequest when a cycle exists, then return a value-less failure carrying that code.
    /// </summary>
    private async Task<Result<MarkInvalidContext>> AuditIllegalTransitionAsync(MarkInvalidCommand request, BarCode barcode, ResultValidation markCode, CancellationToken cancellationToken)
    {
        await this.TryAuditAsync(barcode, markCode, GatewayTask.MarkInvalid, cancellationToken);

        return Result<MarkInvalidContext>.WithFailure(
            $"MarkInvalid rejected by state machine for BarCode {request.Label}: ({barcode.FlowStatus.Name}, {GatewayTask.MarkInvalid.Name}) -> {markCode.Name} ({markCode.Value})");
    }

    /// <summary>
    /// TryAudit semantics: write the audit row only when a latest cycle EXISTS (no cycle → silently skip).
    /// #65 (traceability integrity): the audit Result was previously discarded. Keep the audit
    /// best-effort (both the rejection and success paths already carry their own outcome), but a dropped
    /// audit row must not be silent — log-and-flag it.
    /// </summary>
    private async Task TryAuditAsync(BarCode barcode, ResultValidation code, GatewayTask trigger, CancellationToken cancellationToken)
    {
        await CompletenessPipelineSteps.TryAuditLatestCycleAsync(
            repositoryCycles,
            repositoryCommand,
            barcode,
            code,
            trigger,
            dateTimeMachine,
            error => logger?.LogError(
                "MarkInvalid audit write did not land for BarCode {Label} ({Code}): {Error}",
                barcode.Label.Value, code.Name, error),
            cancellationToken);
    }

    /// <summary>
    /// Story 6.4: machine fired above (own/injected); route the resolved field-write through the apply
    /// seam, then thread the barcode into the pipeline context.
    /// </summary>
    private static Result<MarkInvalidContext> ApplyTransition(BarCode barcode, TransitionOutcome transition)
    {
        barcode.ApplyFlowStatus(transition.NextFlowStatus);
        return Result<MarkInvalidContext>.Success(new MarkInvalidContext(barcode));
    }

    /// <summary>
    /// Immutable pipeline context threading the resolved barcode through the mark-invalid chain.
    /// </summary>
    private sealed record MarkInvalidContext(BarCode BarCode);
}
