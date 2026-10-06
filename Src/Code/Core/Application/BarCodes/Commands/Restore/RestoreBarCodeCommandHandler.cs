// <copyright file="RestoreBarCodeCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Restore;

using IndTrace.Application.Models.Extensions;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Config;
using Microsoft.Extensions.Options;

/// <summary>
/// Handles the restoration of bar codes by processing RestoreBarCodeCommand requests.
/// Updates bar code status from rejected back to an active state and creates associated gateway command.
/// </summary>
public class RestoreBarCodeCommandHandler(
    IRepository<BarCode> barCodeRepository,
    IRepository<TaskGatewayRequest> repositoryCommand,
    IReadOnlyRepository<Cycle> repositoryCycles,
    IDateTimeMachine dateTimeMachine,
    IItemStateMachine? stateMachine = null,
    IOptions<StateMachineRoutingOptions>? routingOptions = null,
    ILogger<RestoreBarCodeCommandHandler>? logger = null)
    : IMonitorRequestHandler<RestoreBarCodeCommand, BarCodeRestoredView>
{
    // Story 3.4: when no machine/options are injected (legacy positional construction in the Epic-1 webapp
    // golden masters), fall back to the as-built engine and default-ON routing so behavior matches DI (CR1).
    private readonly IItemStateMachine machine = stateMachine ?? new ItemStateMachine();
    private readonly StateMachineRoutingOptions routing = routingOptions?.Value ?? new StateMachineRoutingOptions();

    // Story 4.2: map the Application-layer EnableRestoredState flag onto the Domain completeness gate. OFF
    // (default, fail-closed) ⇒ CompletenessOptions.Disabled ⇒ Fire resolves the as-built Rejected → InProcess.
    // ON ⇒ Fire resolves the gated Rejected → Restored (16) computed target (Story 4.1). Resolving null/absent
    // options ⇒ Disabled keeps every existing positional/golden-master caller behaving exactly as today.
    private readonly CompletenessOptions completeness =
        (routingOptions?.Value?.EnableRestoredState ?? false)
            ? new CompletenessOptions { EnableRestoredState = true }
            : CompletenessOptions.Disabled;

    /// <summary>
    /// Processes a RestoreBarCodeCommand request to restore a bar code from rejected status.
    /// Finds the bar code by label, updates its flow status, and creates a gateway command.
    /// </summary>
    /// <param name="request">The restore bar code command containing the bar code label.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Result containing BarCodeRestoredView with restoration details or failure information.</returns>
    //[Fix]
    //CLAUDE
    //Date: 18/06/2026
    //Reason: [Result<T> convention] - Same railway regression fixed in RejectBarCodeCommandHandler:
    //        the fluent chain emitted the library's generic "Async side effect failed" text instead of
    //        the codebase convention "Operation finished with an exception {msg}". Restored the imperative
    //        try/catch shape. Restore semantics (owner decision 18/06/2026): a rejected part is restored
    //        to the active FlowStatus.InProcess and audited with GatewayTask.RestorePartAsync. The cycle is
    //        OPTIONAL for restore — when no cycle exists (missing or repo failure) the restore still
    //        succeeds and simply skips the gateway audit.
    //        #177 (epic #174): re-railwayed as a single fluent chain over the #176 building blocks,
    //        PRESERVING the exception text byte-identically: the chain uses only no-catch combinators,
    //        so a thrown side-effect still surfaces via the outer catch with the codebase-convention
    //        message. The cycle stays OPTIONAL (the audit steps skip, never fail, when it is absent).
    public async Task<Result<BarCodeRestoredView>> ProcessAsync(RestoreBarCodeCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeRestoredView>.WithFailure("Operation was canceled.");
        }

        if (request is null)
        {
            return Result<BarCodeRestoredView>.WithFailure("request cannot be null.");
        }

        try
        {
            return await FindBarCodeAsync(request, cancellationToken)
                .BindAsync((barcode, ct) => this.ResolveTransitionAsync(request, barcode, ct), cancellationToken)
                .BindAsync((barcode, ct) => this.PersistStatusAsync(request, barcode, ct), cancellationToken)
                .BindAsync((barcode, ct) => this.WriteOptionalSuccessAuditAsync(request, barcode, ct), cancellationToken)
                .BindAsync((barcode, _) => Task.FromResult(BarCodeRestoredView.ToDto(barcode)), cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<BarCodeRestoredView>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }

    /// <summary>
    /// Pipeline step 1: locates the barcode by its label. Both a repository failure and a
    /// successful-but-null lookup collapse to the single as-built "BarCode not found" message.
    /// </summary>
    private async Task<Result<BarCode>> FindBarCodeAsync(RestoreBarCodeCommand request, CancellationToken cancellationToken)
    {
        var labelVo = BarCodeLabel.FromPersisted(request.Label);
        var barCodeSpec = new Specification<BarCode>(b => b.Label.Equals(labelVo));
        return await RequireFound(barCodeRepository.FirstOrDefaultAsync(barCodeSpec, cancellationToken), $"BarCode not found {request.Label}");
    }

    /// <summary>
    /// Pipeline step 2 — Story 3.4 (AC2/AC3) + Story 4.2: resolve the restore transition through the machine
    /// BEFORE any mutation or persistence. A LEGAL restore (from Rejected) resolves the gated computed target:
    /// EnableRestoredState OFF (default) ⇒ InProcess (2); EnableRestoredState ON ⇒ Restored (16),
    /// with the Story 3.5 decorator appending the From=Rejected(32)/To=Restored(16) audit row. An
    /// ILLEGAL restore (on a non-Rejected item) is default-rejected by the table with a specific
    /// ResultValidation (OperationCancelled); on rejection nothing is persisted (no UpdateAsync) and
    /// the audit records the specific code. Flag OFF (RouteRestore) restores the prior unconditional
    /// inline mutation (AC7 rollback).
    /// </summary>
    private async Task<Result<BarCode>> ResolveTransitionAsync(RestoreBarCodeCommand request, BarCode barcode, CancellationToken cancellationToken)
    {
        if (!this.routing.RouteRestore)
        {
            // Flag OFF (AC7 rollback): legacy unconditional inline mutation (Rejected -> InProcess),
            // via the trusted apply seam.
            barcode.ApplyFlowStatus(FlowStatus.InProcess);
            return Result<BarCode>.Success(barcode);
        }

        // BarCodeGuard requires BarCodeFound: true (the pipeline resolved the barcode by label upstream).
        var transitionContext = new TransitionContext(
            MachineType.Process,
            CycleStatus.None,
            barcode.PartStatus,
            CycleTime: 0,
            Recipe: null,
            BarCodeFound: true,
            Completeness: this.completeness);

        // Story 6.4: the handler still fires its OWN (possibly injected/spy) machine here; only the
        // resolved field-write is routed through the guarded apply seam (setter now private set).
        var outcome = this.machine.Fire(barcode, GatewayTask.RestorePartAsync, transitionContext);

        // The failure fork must read the SPECIFIC code carried on the FAILED outcome's value
        // (outcome.Value?.Result) — a failure-value carry the success-biased combinators cannot express,
        // so the three-way fork stays an explicit pattern match.
        return await (outcome switch
        {
            { IsSuccess: true, Value: { } transition } => Task.FromResult(ApplyTransition(barcode, transition)),
            { IsSuccess: true } => Task.FromResult(Result<BarCode>.WithFailure(
                $"Restore for BarCode {request.Label} succeeded without a transition outcome.")),
            _ => this.AuditIllegalTransitionAsync(request, barcode, outcome.Value?.Result ?? ResultValidation.OperationCancelled, cancellationToken),
        });
    }

    /// <summary>
    /// Pipeline step 3 — #65 (traceability integrity): the barcode status write was previously discarded, so a
    /// failed persist reported a successful restore. Propagate the failure — a restore whose status did not
    /// land must NOT report success.
    /// </summary>
    private async Task<Result<BarCode>> PersistStatusAsync(RestoreBarCodeCommand request, BarCode barcode, CancellationToken cancellationToken)
    {
        barcode.ModifiedOn = dateTimeMachine.Now.ToLocalTime();

        var updateResult = await barCodeRepository.UpdateAsync(barcode, cancellationToken);
        if (updateResult is { IsFailure: true })
        {
            logger?.LogError(
                "Failed to persist Restore status for BarCode {Label}: {Error}",
                request.Label, updateResult.Errors?.FirstOrDefault());
            return Result<BarCode>.WithFailure(updateResult.Errors);
        }

        return Result<BarCode>.Success(barcode);
    }

    /// <summary>
    /// Pipeline step 4: the cycle is OPTIONAL for restore. When the most recent cycle exists, log a gateway
    /// request for traceability; when it is missing or the lookup fails, restore still succeeds.
    /// #65: best-effort success audit — keep the successful-restore view byte-identical, but a
    /// dropped audit row must not be silent. Log-and-flag the discarded Result.
    /// </summary>
    private async Task<Result<BarCode>> WriteOptionalSuccessAuditAsync(RestoreBarCodeCommand request, BarCode barcode, CancellationToken cancellationToken)
    {
        var cycleSpec = new Specification<Cycle>(c => c.BarCodeId == barcode.BarCodeId)
            .AddOrderByDescending(b => b.CycleId)
            .ApplyPaging(0, 1);
        var cycleResult = await repositoryCycles.FirstOrDefaultAsync(cycleSpec, cancellationToken);
        var cycle = cycleResult.Value;
        if (cycleResult.IsSuccess && cycle is not null)
        {
            var command = new TaskGatewayRequest
            {
                MachineId = barcode.MachineId.Value,
                BarCodeId = barcode.BarCodeId.Value,
                PartStatus = barcode.PartStatus,
                FlowStatus = barcode.FlowStatus,
                ResultValidation = ResultValidation.None,
                GatewayTask = GatewayTask.RestorePartAsync,
                TimeStamp = dateTimeMachine.Now.ToLocalTime(),
                CycleId = cycle.CycleId.Value,
                CycleStatus = cycle.CycleStatus,
            };

            var commandAuditResult = await repositoryCommand.AddAsync(command, cancellationToken);
            if (commandAuditResult is { IsFailure: true })
            {
                logger?.LogError(
                    "Restore success-audit write did not land for BarCode {Label}: {Error}",
                    request.Label, commandAuditResult.Errors?.FirstOrDefault());
            }
        }

        return Result<BarCode>.Success(barcode);
    }

    /// <summary>
    /// Illegal source state: persist NOTHING (no UpdateAsync). Record the rejection's SPECIFIC
    /// ResultValidation on an audit TaskGatewayRequest when a cycle exists, then return a
    /// value-less failure carrying that code in the message (AC3).
    /// #65: this path already returns a SPECIFIC failure (the state-machine code); do not
    /// clobber it by propagating an audit-write fault. Log-and-flag the dropped audit row.
    /// </summary>
    private async Task<Result<BarCode>> AuditIllegalTransitionAsync(RestoreBarCodeCommand request, BarCode barcode, ResultValidation restoreCode, CancellationToken cancellationToken)
    {
        var rejectCycleSpec = new Specification<Cycle>(c => c.BarCodeId == barcode.BarCodeId)
            .AddOrderByDescending(b => b.CycleId)
            .ApplyPaging(0, 1);
        var rejectCycleResult = await repositoryCycles.FirstOrDefaultAsync(rejectCycleSpec, cancellationToken);
        var rejectCycle = rejectCycleResult.Value;
        if (rejectCycleResult.IsSuccess && rejectCycle is not null)
        {
            var rejectAudit = new TaskGatewayRequest
            {
                MachineId = barcode.MachineId.Value,
                BarCodeId = barcode.BarCodeId.Value,
                PartStatus = barcode.PartStatus,
                FlowStatus = barcode.FlowStatus,
                ResultValidation = restoreCode,
                GatewayTask = GatewayTask.RestorePartAsync,
                TimeStamp = dateTimeMachine.Now.ToLocalTime(),
                CycleId = rejectCycle.CycleId.Value,
                CycleStatus = rejectCycle.CycleStatus,
            };
            var rejectAuditResult = await repositoryCommand.AddAsync(rejectAudit, cancellationToken);
            if (rejectAuditResult is { IsFailure: true })
            {
                logger?.LogError(
                    "Restore rejection-audit write did not land for BarCode {Label}: {Error}",
                    request.Label, rejectAuditResult.Errors?.FirstOrDefault());
            }
        }

        return Result<BarCode>.WithFailure(
            $"Restore rejected by state machine for BarCode {request.Label}: ({barcode.FlowStatus.Name}, {GatewayTask.RestorePartAsync.Name}) -> {restoreCode.Name} ({restoreCode.Value})");
    }

    /// <summary>
    /// Applies the machine-resolved flow status through the trusted apply seam (Story 6.4) and
    /// forwards the barcode down the pipeline unchanged.
    /// </summary>
    private static Result<BarCode> ApplyTransition(BarCode barcode, TransitionOutcome transition)
    {
        barcode.ApplyFlowStatus(transition.NextFlowStatus);
        return Result<BarCode>.Success(barcode);
    }

    /// <summary>
    /// Bridges a repository lookup (<see cref="Result{T}"/> of a nullable entity) into a non-nullable
    /// pipeline value. Deliberately NOT <c>ResultExtensions.RequireValue</c>: the as-built handler collapses
    /// BOTH a repository failure and a successful-but-null value to the single fixed not-found message
    /// (the production repository fails a no-match lookup, and that failure must surface as "not found",
    /// not as the repository's own error text).
    /// </summary>
    private static async Task<Result<TEntity>> RequireFound<TEntity>(Task<Result<TEntity?>> query, string notFoundMessage)
        where TEntity : class
    {
        var result = await query;
        var entity = result.Value;
        return result.IsFailure || entity is null
            ? Result<TEntity>.WithFailure(notFoundMessage)
            : Result<TEntity>.Success(entity);
    }
}
