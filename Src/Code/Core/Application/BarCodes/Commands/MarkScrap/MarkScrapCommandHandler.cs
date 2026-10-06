// <copyright file="MarkScrapCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.MarkScrap;

using IndTrace.Application.Models.Extensions;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Config;
using Microsoft.Extensions.Options;

/// <summary>
/// Story 5.3 — WEBAPP/monitor handler that wires the config-gated <see cref="GatewayTask.MarkScrap"/>
/// completeness trigger (Story 4.1) through the Epic-2 <see cref="IItemStateMachine"/>. With the
/// <c>EnableScrapState</c> gate ON a LEGAL MarkScrap (item is <c>InProcess</c>/<c>Finished</c> and the part is
/// not already <see cref="PartStatus.Scrap"/>/<see cref="PartStatus.Rejected"/>) resolves
/// <c>NextPartStatus = Scrap</c> and is persisted. With the gate OFF (DEFAULT, fail-closed) <c>Fire</c>
/// default-rejects the trigger, NOTHING is persisted (PRD NFR4). The trigger is OFF the PLC bus (NFR1).
/// </summary>
public class MarkScrapCommandHandler(
    IRepository<BarCode> barCodeRepository,
    IRepository<TaskGatewayRequest> repositoryCommand,
    IReadOnlyRepository<Cycle> repositoryCycles,
    IDateTimeMachine dateTimeMachine,
    IItemStateMachine? stateMachine = null,
    IOptions<StateMachineRoutingOptions>? routingOptions = null,
    ILogger<MarkScrapCommandHandler>? logger = null)
    : IMonitorRequestHandler<MarkScrapCommand, BarCodeMarkedScrapView>
{
    private readonly IItemStateMachine machine = stateMachine ?? new ItemStateMachine();

    // Map the Application-layer EnableScrapState flag onto the Domain completeness gate. OFF (default,
    // fail-closed) ⇒ Disabled ⇒ the gate guard rejects MarkScrap. ON ⇒ the gated MarkScrap transition
    // resolves NextPartStatus = Scrap when PartScrappableGuard also passes (Story 4.1).
    private readonly CompletenessOptions completeness =
        (routingOptions?.Value?.EnableScrapState ?? false)
            ? new CompletenessOptions { EnableScrapState = true }
            : CompletenessOptions.Disabled;

    /// <summary>
    /// Processes a <see cref="MarkScrapCommand"/>: finds the barcode by label and fires
    /// <see cref="GatewayTask.MarkScrap"/> through the machine BEFORE any mutation. On a gated/illegal
    /// rejection NOTHING is persisted and a value-less failure carrying the specific code is returned; on
    /// success the resolved <see cref="PartStatus"/> is persisted.
    /// </summary>
    /// <param name="request">The command containing the barcode label.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Result with the marked-scrap view on success or failure information.</returns>
    // #180 (epic #174): re-railwayed as a single fluent chain over the shared completeness pipeline steps,
    // PRESERVING the as-built behavior byte-identically: the chain uses only no-catch combinators
    // (exceptions still surface via the outer catch with the exact message below), and RequireFound keeps
    // the barcode mandatory (a missing/null/failed lookup fails).
    public async Task<Result<BarCodeMarkedScrapView>> ProcessAsync(MarkScrapCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeMarkedScrapView>.WithFailure("Operation was canceled.");
        }

        if (request is null)
        {
            return Result<BarCodeMarkedScrapView>.WithFailure("request cannot be null.");
        }

        try
        {
            return await CompletenessPipelineSteps.FindBarCodeAsync(barCodeRepository, request.Label, cancellationToken)
                .BindAsync((barcode, ct) => this.ResolveTransitionAsync(request, barcode, ct), cancellationToken)
                .BindAsync((context, ct) => this.PersistStatusAsync(context, ct), cancellationToken)
                .BindAsync((context, ct) => this.WriteSuccessAuditAsync(context, ct), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(BarCodeMarkedScrapView.ToDto(context.BarCode)), cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<BarCodeMarkedScrapView>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }

    /// <summary>
    /// Pipeline step 2: resolve the MarkScrap transition through the machine BEFORE any mutation or
    /// persistence. The MarkScrap rows carry CompletenessGateGuard(EnableScrapState) + PartScrappableGuard.
    /// The latter requires PartStatus != Scrap &amp;&amp; != Rejected, so the context MUST carry the barcode's
    /// REAL part status (an already-scrapped/rejected part is rejected even when the gate is ON).
    /// </summary>
    private async Task<Result<MarkScrapContext>> ResolveTransitionAsync(MarkScrapCommand request, BarCode barcode, CancellationToken cancellationToken)
    {
        var transitionContext = new TransitionContext(
            MachineType.Process,
            CycleStatus.None,
            barcode.PartStatus,
            CycleTime: 0,
            Recipe: null,
            BarCodeFound: true,
            Completeness: this.completeness);

        var outcome = this.machine.Fire(barcode, GatewayTask.MarkScrap, transitionContext);

        // The failure fork must read the SPECIFIC code carried on the FAILED outcome's value
        // (outcome.Value?.Result) — a failure-value carry the success-biased combinators cannot express,
        // so the three-way fork stays an explicit pattern match.
        return await (outcome switch
        {
            { IsSuccess: true, Value: { } transition } => Task.FromResult(ApplyTransition(barcode, transition)),
            { IsSuccess: true } => Task.FromResult(Result<MarkScrapContext>.WithFailure(
                $"MarkScrap for BarCode {request.Label} succeeded without a transition outcome.")),
            _ => this.AuditIllegalTransitionAsync(request, barcode, outcome.Value?.Result ?? ResultValidation.OperationCancelled, cancellationToken),
        });
    }

    /// <summary>
    /// Pipeline step 3 — Story 5.3 (review fix): a persistence failure fails the operation (railway),
    /// mirroring Story 5.1.
    /// </summary>
    private async Task<Result<MarkScrapContext>> PersistStatusAsync(MarkScrapContext context, CancellationToken cancellationToken)
    {
        context.BarCode.ModifiedOn = dateTimeMachine.Now.ToLocalTime();

        var persisted = await barCodeRepository.UpdateAsync(context.BarCode, cancellationToken);
        if (persisted.IsFailure)
        {
            return Result<MarkScrapContext>.WithFailure(persisted.Errors);
        }

        return Result<MarkScrapContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 4: best-effort traceability audit of the legal MarkScrap operation
    /// (TryAudit semantics — written only when a latest cycle exists).
    /// </summary>
    private async Task<Result<MarkScrapContext>> WriteSuccessAuditAsync(MarkScrapContext context, CancellationToken cancellationToken)
    {
        await this.TryAuditAsync(context.BarCode, ResultValidation.None, GatewayTask.MarkScrap, cancellationToken);
        return Result<MarkScrapContext>.Success(context);
    }

    /// <summary>
    /// Gate OFF or illegal source: persist NOTHING. Record the specific ResultValidation on an audit
    /// TaskGatewayRequest when a cycle exists, then return a value-less failure carrying that code.
    /// </summary>
    private async Task<Result<MarkScrapContext>> AuditIllegalTransitionAsync(MarkScrapCommand request, BarCode barcode, ResultValidation scrapCode, CancellationToken cancellationToken)
    {
        await this.TryAuditAsync(barcode, scrapCode, GatewayTask.MarkScrap, cancellationToken);

        return Result<MarkScrapContext>.WithFailure(
            $"MarkScrap rejected by state machine for BarCode {request.Label}: ({barcode.FlowStatus.Name}, {GatewayTask.MarkScrap.Name}) -> {scrapCode.Name} ({scrapCode.Value})");
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
                "MarkScrap audit write did not land for BarCode {Label} ({Code}): {Error}",
                barcode.Label.Value, code.Name, error),
            cancellationToken);
    }

    /// <summary>
    /// Story 6.4: machine fired above (own/injected); route the resolved part-status write through the
    /// apply seam, then thread the barcode into the pipeline context.
    /// </summary>
    private static Result<MarkScrapContext> ApplyTransition(BarCode barcode, TransitionOutcome transition)
    {
        barcode.ApplyPartStatus(transition.NextPartStatus);
        return Result<MarkScrapContext>.Success(new MarkScrapContext(barcode));
    }

    /// <summary>
    /// Immutable pipeline context threading the resolved barcode through the mark-scrap chain.
    /// </summary>
    private sealed record MarkScrapContext(BarCode BarCode);
}
