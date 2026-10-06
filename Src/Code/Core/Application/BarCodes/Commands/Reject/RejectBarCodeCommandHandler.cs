// <copyright file="RejectBarCodeCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Reject;

using IndTrace.Application.Models.Extensions;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Handles the rejection of barcodes by updating their flow status and logging the rejection operation.
/// </summary>
/// <remarks>
/// This handler orchestrates the barcode rejection process including:
/// - Validating the barcode exists
/// - Updating the barcode flow status to rejected
/// - Finding the associated cycle for logging
/// - Creating a gateway request for the rejection operation
/// - Returning a rejection view with the updated barcode information.
/// </remarks>
public class RejectBarCodeCommandHandler(
    IRepository<BarCode> barCodeRepository,
    IRepository<TaskGatewayRequest> repositoryCommand,
    IReadOnlyRepository<Cycle> repositoryCycles,
    IDateTimeMachine dateTimeMachine,
    IItemStateMachine? stateMachine = null,
    IOptions<StateMachineRoutingOptions>? routingOptions = null,
    ILogger<RejectBarCodeCommandHandler>? logger = null)
    : IMonitorRequestHandler<RejectBarCodeCommand, BarCodeRejectedView>
{
    // Story 3.4: when no machine/options are injected (legacy positional construction in the Epic-1 webapp
    // golden masters), fall back to the as-built engine and default-ON routing so behavior matches DI (CR1).
    private readonly IItemStateMachine machine = stateMachine ?? new ItemStateMachine();
    private readonly StateMachineRoutingOptions routing = routingOptions?.Value ?? new StateMachineRoutingOptions();

    /// <summary>
    /// Processes the barcode rejection command asynchronously.
    /// </summary>
    /// <param name="request">The reject barcode command containing the barcode label to reject.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> with a <see cref="BarCodeRejectedView"/> if successful,
    /// or error information if the operation fails.
    /// </returns>
    /// <remarks>
    /// This method performs the following steps:
    /// 1. Locates the barcode by its label
    /// 2. Updates the barcode flow status to rejected
    /// 3. Updates the modification timestamp
    /// 4. Finds the most recent cycle associated with the barcode
    /// 5. Creates a gateway request to log the rejection operation
    /// 6. Returns a rejection view with the updated barcode data.
    /// </remarks>
    //[Fix]
    //CLAUDE
    //Date: 18/06/2026
    //Reason: [Result<T> convention] - The railway refactor regressed two behaviors the tests and
    //        the rest of the handlers (e.g. CreateCyclesCommandHandler) guarantee: (1) a cycle is
    //        mandatory for rejection — a missing/null/failed cycle must fail, not silently succeed;
    //        (2) a thrown side-effect must surface as "Operation finished with an exception {msg}",
    //        not the library's generic railway-wrapper text. Restored the imperative try/catch +
    //        explicit IsFailure-checks shape used across the codebase.
    //        #177 (epic #174): re-railwayed as a single fluent chain over the #176 building blocks,
    //        PRESERVING both regressed behaviors byte-identically: the chain uses only no-catch
    //        combinators (exceptions still surface via the outer catch with the exact message above),
    //        and RequireFound keeps the cycle mandatory (missing/null/failed cycle fails).
    public async Task<Result<BarCodeRejectedView>> ProcessAsync(RejectBarCodeCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeRejectedView>.WithFailure("Operation was canceled.");
        }

        if (request is null)
        {
            return Result<BarCodeRejectedView>.WithFailure("request cannot be null.");
        }

        try
        {
            return await FindBarCodeAsync(request, cancellationToken)
                .BindAsync((barcode, ct) => this.FindRequiredCycleAsync(request, barcode, ct), cancellationToken)
                .BindAsync((context, ct) => this.ResolveTransitionAsync(request, context, ct), cancellationToken)
                .BindAsync((context, ct) => this.PersistStatusAsync(request, context, ct), cancellationToken)
                .BindAsync((context, ct) => this.WriteSuccessAuditAsync(request, context, ct), cancellationToken)
                .BindAsync((context, _) => Task.FromResult(BarCodeRejectedView.ToDto(context.BarCode)), cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<BarCodeRejectedView>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }

    /// <summary>
    /// Pipeline step 1: locates the barcode by its label. Both a repository failure and a
    /// successful-but-null lookup collapse to the single as-built "BarCode not found" message.
    /// </summary>
    private async Task<Result<BarCode>> FindBarCodeAsync(RejectBarCodeCommand request, CancellationToken cancellationToken)
    {
        var labelVo = BarCodeLabel.FromPersisted(request.Label);
        var barCodeSpec = new Specification<BarCode>(b => b.Label.Equals(labelVo));
        return await RequireFound(barCodeRepository.FirstOrDefaultAsync(barCodeSpec, cancellationToken), $"BarCode not found {request.Label}");
    }

    /// <summary>
    /// Pipeline step 2: the most recent cycle is REQUIRED for rejection — a missing/null/failed cycle
    /// fails the operation (never silently succeeds; see the ProcessAsync [Fix] provenance).
    /// </summary>
    private async Task<Result<RejectContext>> FindRequiredCycleAsync(RejectBarCodeCommand request, BarCode barcode, CancellationToken cancellationToken)
    {
        var cycleSpec = new Specification<Cycle>(c => c.BarCodeId == barcode.BarCodeId)
            .AddOrderByDescending(b => b.CycleId)
            .ApplyPaging(0, 1);
        var cycleResult = await RequireFound(repositoryCycles.FirstOrDefaultAsync(cycleSpec, cancellationToken), $"Cycles for BarCode {request.Label} not found");
        return cycleResult.Bind(cycle => Result<RejectContext>.Success(new RejectContext(barcode, cycle)));
    }

    /// <summary>
    /// Pipeline step 3 — Story 3.4 (AC1/AC4): resolve the reject transition through the machine BEFORE any
    /// mutation or persistence. A LEGAL reject (from InProcess/Finished) resolves to Rejected; an ILLEGAL
    /// reject (e.g. on an already-Rejected item) is default-rejected by the table with a specific
    /// ResultValidation (OperationCancelled). On rejection nothing is persisted (no UpdateAsync) —
    /// the audit records the specific code instead of None. The flag OFF restores the prior
    /// unconditional inline mutation (AC7 rollback).
    /// </summary>
    private async Task<Result<RejectContext>> ResolveTransitionAsync(RejectBarCodeCommand request, RejectContext context, CancellationToken cancellationToken)
    {
        if (!this.routing.RouteReject)
        {
            // Flag OFF (AC7 rollback): legacy unconditional inline mutation, via the trusted apply seam.
            context.BarCode.ApplyFlowStatus(FlowStatus.Rejected);
            return Result<RejectContext>.Success(context);
        }

        // BarCodeGuard requires BarCodeFound: true (the pipeline resolved the barcode by label upstream).
        // Mirror the item's current cycle/part status into the context (the machine echoes them).
        var transitionContext = new TransitionContext(
            MachineType.Process,
            context.Cycle.CycleStatus,
            context.BarCode.PartStatus,
            CycleTime: 0,
            Recipe: null,
            BarCodeFound: true);

        // Story 6.4: the handler still fires its OWN (possibly injected/spy) machine here; only the
        // resolved field-write is routed through the guarded apply seam (setter now private set).
        var outcome = this.machine.Fire(context.BarCode, GatewayTask.RejectPartAsync, transitionContext);

        // The failure fork must read the SPECIFIC code carried on the FAILED outcome's value
        // (outcome.Value?.Result) — a failure-value carry the success-biased combinators cannot express,
        // so the three-way fork stays an explicit pattern match.
        return await (outcome switch
        {
            { IsSuccess: true, Value: { } transition } => Task.FromResult(ApplyTransition(context, transition)),
            { IsSuccess: true } => Task.FromResult(Result<RejectContext>.WithFailure(
                $"Reject for BarCode {request.Label} succeeded without a transition outcome.")),
            _ => this.AuditIllegalTransitionAsync(request, context, outcome.Value?.Result ?? ResultValidation.OperationCancelled, cancellationToken),
        });
    }

    /// <summary>
    /// Pipeline step 4 — #65 (traceability integrity): the barcode status write was previously discarded, so a
    /// failed persist reported a successful rejection to the UI/caller. Propagate the failure — a rejection
    /// whose status did not land must NOT report success.
    /// </summary>
    private async Task<Result<RejectContext>> PersistStatusAsync(RejectBarCodeCommand request, RejectContext context, CancellationToken cancellationToken)
    {
        context.BarCode.ModifiedOn = dateTimeMachine.Now.ToLocalTime();

        var updateResult = await barCodeRepository.UpdateAsync(context.BarCode, cancellationToken);
        if (updateResult is { IsFailure: true })
        {
            logger?.LogError(
                "Failed to persist Reject status for BarCode {Label}: {Error}",
                request.Label, updateResult.Errors?.FirstOrDefault());
            return Result<RejectContext>.WithFailure(updateResult.Errors);
        }

        return Result<RejectContext>.Success(context);
    }

    /// <summary>
    /// Pipeline step 5: creates the gateway request that logs the (legal) rejection operation.
    /// #65: best-effort success audit — keep the successful-rejection view byte-identical, but a dropped
    /// audit row must not be silent. Log-and-flag the discarded Result.
    /// </summary>
    private async Task<Result<RejectContext>> WriteSuccessAuditAsync(RejectBarCodeCommand request, RejectContext context, CancellationToken cancellationToken)
    {
        var command = new TaskGatewayRequest
        {
            MachineId = context.BarCode.MachineId.Value,
            BarCodeId = context.BarCode.BarCodeId.Value,
            PartStatus = context.BarCode.PartStatus,
            FlowStatus = context.BarCode.FlowStatus,
            ResultValidation = ResultValidation.None,
            GatewayTask = GatewayTask.RejectPartAsync,
            TimeStamp = dateTimeMachine.Now.ToLocalTime(),
            CycleId = context.Cycle.CycleId.Value,
            CycleStatus = context.Cycle.CycleStatus,
        };

        var commandAuditResult = await repositoryCommand.AddAsync(command, cancellationToken);
        if (commandAuditResult is { IsFailure: true })
        {
            logger?.LogError(
                "Reject success-audit write did not land for BarCode {Label}: {Error}",
                request.Label, commandAuditResult.Errors?.FirstOrDefault());
        }

        return Result<RejectContext>.Success(context);
    }

    /// <summary>
    /// Illegal source state: persist NOTHING (no UpdateAsync). Record the rejection's SPECIFIC
    /// ResultValidation on the audit TaskGatewayRequest, then return a value-less failure
    /// carrying that code in the message (AC3/AC4).
    /// #65: this rejection path already returns a SPECIFIC failure (the state-machine code); do
    /// not clobber it by propagating an audit-write fault. Log-and-flag the dropped audit row.
    /// </summary>
    private async Task<Result<RejectContext>> AuditIllegalTransitionAsync(RejectBarCodeCommand request, RejectContext context, ResultValidation rejectCode, CancellationToken cancellationToken)
    {
        var rejectAudit = new TaskGatewayRequest
        {
            MachineId = context.BarCode.MachineId.Value,
            BarCodeId = context.BarCode.BarCodeId.Value,
            PartStatus = context.BarCode.PartStatus,
            FlowStatus = context.BarCode.FlowStatus,
            ResultValidation = rejectCode,
            GatewayTask = GatewayTask.RejectPartAsync,
            TimeStamp = dateTimeMachine.Now.ToLocalTime(),
            CycleId = context.Cycle.CycleId.Value,
            CycleStatus = context.Cycle.CycleStatus,
        };
        var rejectAuditResult = await repositoryCommand.AddAsync(rejectAudit, cancellationToken);
        if (rejectAuditResult is { IsFailure: true })
        {
            logger?.LogError(
                "Reject rejection-audit write did not land for BarCode {Label}: {Error}",
                request.Label, rejectAuditResult.Errors?.FirstOrDefault());
        }

        return Result<RejectContext>.WithFailure(
            $"Reject rejected by state machine for BarCode {request.Label}: ({context.BarCode.FlowStatus.Name}, {GatewayTask.RejectPartAsync.Name}) -> {rejectCode.Name} ({rejectCode.Value})");
    }

    /// <summary>
    /// Applies the machine-resolved flow status through the trusted apply seam (Story 6.4) and
    /// forwards the pipeline context unchanged.
    /// </summary>
    private static Result<RejectContext> ApplyTransition(RejectContext context, TransitionOutcome transition)
    {
        context.BarCode.ApplyFlowStatus(transition.NextFlowStatus);
        return Result<RejectContext>.Success(context);
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

    /// <summary>
    /// Immutable pipeline context threading the resolved barcode and its mandatory most-recent cycle
    /// through the rejection chain.
    /// </summary>
    private sealed record RejectContext(BarCode BarCode, Cycle Cycle);
}
