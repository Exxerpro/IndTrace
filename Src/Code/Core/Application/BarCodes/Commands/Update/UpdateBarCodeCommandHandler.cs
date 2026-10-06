// <copyright file="UpdateBarCodeCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Update;

using IndQuestResults.Operations;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Handles the update of existing barcodes within the IndTrace manufacturing system.
/// </summary>
/// <remarks>
/// This handler processes barcode updates which typically occur when a part moves through
/// different stations in the manufacturing workflow. It manages cycle completion,
/// status updates, and flow progression according to manufacturing business rules.
/// <para>
/// Issue #33 (Chunk 5) — cut the last WRITE handler (the EndOfProcess/ReadBarCode update path) off the mutable
/// god-object <c>IBarCodeResult.GetBarCodeDetails</c> onto the stateless <see cref="IBarCodeDetailsLoader"/> +
/// immutable <see cref="BarCodeSnapshot"/> + pure <see cref="BarCodeResultProjection"/>. The §7 PLC surface is
/// preserved byte-identically: the railway keeps gating on <c>snapshot.ResultValidation == Valid</c> (the loader
/// returns <c>Success(snapshot)</c> even on a validation failure — the snapshot carries the specific negative
/// <c>ResultValidation</c> + <c>Error</c>), NOT on <c>Result.IsSuccess</c>. The created cycle is threaded
/// immutably (<c>info with { Cycle = entity }</c>) instead of <c>SetCycle</c>; the SHARED tracked
/// <c>info.BarCode</c> is still mutated IN PLACE in <see cref="UpdateBarcodeState"/> (same persistence side effect
/// + ordering — safe because the repository opens a fresh pooled DbContext per op with <c>AsNoTracking</c>, so each
/// dispatch owns its entity graph); and the final <c>ToDto</c> becomes <see cref="BarCodeResultProjection.ToResponse"/>,
/// byte-equal. The god-object and <c>IBarCodeResult</c> are left intact (their retirement is Chunk 6).
/// </para>
/// <para>
/// Issue #114 (chunk C) — the former two-transaction saga (cycle INSERT via <c>IRepository&lt;Cycle&gt;.AddAsync</c>,
/// barcode Flow/Part status UPDATE via a separate <c>IRepository&lt;BarCode&gt;.UpdateAsync</c> auto-commit) is
/// collapsed to ONE transactional <see cref="IAggregateRepository{TRoot}"/> save: the new FinishedOk cycle is
/// staged on the loaded root (<c>BarCode.StageNewCycle</c>, the chunk A/#95 Slice E path) and the barcode
/// status write is staged via <c>BarCode.StageStatusWrite</c> (the chunk B seam), then both flush in one
/// explicit transaction. A barcode-half failure therefore rolls the cycle INSERT back too — the PLC "failed"
/// response can no longer coexist with a committed FinishedOk cycle for a retry to duplicate. The §7 response
/// is unchanged for every still-possible scenario.
/// </para>
/// </remarks>
public class UpdateBarCodeCommandHandler(
    IDateTimeMachine dateTimeMachine,
    IAggregateRepository<BarCode> barCodeAggregateRepository,
    IBarCodeDetailsLoader barCodeDetailsLoader,
    IItemStateMachine? stateMachine = null,
    IOptions<StateMachineRoutingOptions>? routingOptions = null) : IGatewayRequestHandler<UpdateBarCodeCommand, TaskGatewayResponseDto>
{
    // Story 3.2: fall back to the as-built engine and default-ON routing when not injected (legacy
    // positional construction in characterization tests), so behavior is identical to DI.
    private readonly IItemStateMachine machine = stateMachine ?? new ItemStateMachine();
    private readonly StateMachineRoutingOptions routing = routingOptions?.Value ?? new StateMachineRoutingOptions();

    /// <summary>
    /// Processes the update barcode command asynchronously.
    /// </summary>
    /// <param name="cmd">The update barcode command containing the request details.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> with a <see cref="TaskGatewayResponseDto"/> if successful,
    /// or error information if the operation fails.
    /// </returns>
    /// <remarks>
    /// This method performs the following operations:
    /// 1. Validates the barcode and retrieves its current details
    /// 2. Creates a new cycle to track the update operation
    /// 3. Updates the barcode status to reflect the new state
    /// 4. Returns a comprehensive response with updated information.
    /// </remarks>
    public async Task<Result<TaskGatewayResponseDto>> ProcessAsync(UpdateBarCodeCommand cmd, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<TaskGatewayResponseDto>.WithFailure("Operation was canceled.");
        }

        // Story 3.2: resolve the converged EndOfProcess outcome ONCE; carried through the railway via a local
        // (no mutable handler state — the handler is registered as a singleton). Null = legacy/OFF path.
        TransitionOutcome? outcome = null;

        try
        {
            // #181: the former post-chain `if (result.IsFailure && SpecificDiagnostics)` funnel is dissolved into
            // the terminal ThenRecover, and the Valid-gate ternary into EnsureOrFault — same gate, same messages.
            return await Result.Success(cmd)
                .ValidateNotNull(c => (c, nameof(cmd)))
                .ThenAsync(c => GetBarcodeInfo(c.Command, cancellationToken))
                .EnsureOrFault(
                    info => info.ResultValidation == ResultValidation.Valid,
                    info => Result<BarCodeSnapshot>.WithFailure(info.Error ?? "Barcode validation failed."))
                .ThenDo(info => { outcome = this.ResolveEndOfProcessOutcome(info); })
                .ThenAsync(info => CreateNewCycle(info, outcome, cancellationToken))
                .ThenAsync(info => UpdateBarcodeState(cmd.Command, info, outcome, cancellationToken))
                .ThenMap(info =>
                {
                    var dto = BarCodeResultProjection.ToResponse(info);
                    return ReferenceStamper.Apply(dto).ValueOr(dto);
                })
                .ThenRecover(errors => Task.FromResult(this.ShapeFailureResponse(errors, cmd.Command.MachineId)));
        }
        catch (Exception ex)
        {
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 row 11] - unhandled exception maps to the specific
            //ExceptionResultValidation(-131072) on a non-null value so the PLC tag carries the real reason.
            if (this.routing.SpecificDiagnostics)
            {
                var diagnostic = PlcFailureDiagnostics.BuildDiagnosticResponse(ResultValidation.ExceptionResultValidation, cmd.Command.MachineId);
                return Result<TaskGatewayResponseDto>.WithFailure($"Exception occurred: {ex.Message}", diagnostic);
            }

            return Result<TaskGatewayResponseDto>.WithFailure($"Exception occurred: {ex.Message}");
        }
    }

    //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 AC1-AC3/AC8] - when the upstream barcode lookup or
    //validation FAILS (BarCodeNotFound), classify the failure into the specific negative ResultValidation
    //and return a value-carrying failure so the precise code survives the publish path. Success is
    //untouched (byte-equal with Story 3.2: a machine reject still silently falls back to the legacy inline
    //projection, so a Valid barcode that cannot legally EndOfProcess remains a SUCCESS — only WHICH
    //negative code a genuine failure carries changes, per the story's "only the value changes" mandate).
    private Result<TaskGatewayResponseDto> ShapeFailureResponse(IEnumerable<string> errors, int machineId)
    {
        var error = errors.ToList();
        if (this.routing.SpecificDiagnostics)
        {
            var code = PlcFailureDiagnostics.Classify(error.FirstOrDefault());
            var diagnostic = PlcFailureDiagnostics.BuildDiagnosticResponse(code, machineId);
            return Result<TaskGatewayResponseDto>.WithFailure(error, diagnostic);
        }

        return Result<TaskGatewayResponseDto>.WithFailure(error);
    }

    /// <summary>
    /// Story 3.2 (AC1): fires <see cref="GatewayTask.EndOfProcessAsync"/> through the machine so the converged
    /// outcome (Finished/FinishedOk/Ok) — not inline literals — drives persistence AND projection. When the flag
    /// is OFF or the machine rejects (e.g. a barcode not in InProcess), the legacy inline path is kept (returns
    /// null), preserving byte-equal success behavior from Story 3.2.
    /// </summary>
    private TransitionOutcome? ResolveEndOfProcessOutcome(BarCodeSnapshot info)
    {
        if (!this.routing.RouteEndOfProcess)
        {
            return null;
        }

        // Story 27.2b-2: snapshot.BarCode is nullable ("no part" = absent). This EndOfProcess resolution only runs
        // on the found path; an absent barcode yields no outcome (byte-equal — the god-object never reached here).
        if (info.BarCode is null)
        {
            return null;
        }

        var transitionContext = new TransitionContext(
            info.MachineType,
            CycleStatus.FinishedOk,
            PartStatus.Ok,
            CycleTime: 0,
            Recipe: info.Recipe,
            MachineFound: true);

        var fired = this.machine.Fire(info.BarCode, GatewayTask.EndOfProcessAsync, transitionContext);
        return fired is { IsSuccess: true, Value: not null } ? fired.Value : null;
    }

    private async Task<Result<BarCodeSnapshot>> GetBarcodeInfo(TaskGatewayRequest request, CancellationToken cancellationToken)
    {
        var detailsRequest = new BarCodeDetailsRequest(request.MachineId, request.BarCode, request.PartNumber);

        // #33 Chunk 5: the loader returns Success(snapshot) even on a validation failure (the snapshot carries the
        // specific negative ResultValidation + Error); only a null/exception load yields a failed Result — that is
        // the equivalent of the god-object path's "GetBarCodeDetails returned nothing" branch, preserving the exact
        // "Failed to retrieve barcode details." wrapper. ToResult converts a success-with-null load into the
        // wrapper failure; MapError collapses a failed load's errors onto the same wrapper, as before.
        const string loadFailure = "Failed to retrieve barcode details.";
        return (await barCodeDetailsLoader.LoadAsync(detailsRequest, cancellationToken)
                .ToResult(loadFailure)
                .ConfigureAwait(false))
            .MapError(_ => [loadFailure]);
    }

    private async Task<Result<BarCodeSnapshot>> CreateNewCycle(BarCodeSnapshot info, TransitionOutcome? outcome, CancellationToken cancellationToken)
    {
        // Story 27.2b-2: reached only on the found path; RequireValue resolves the nullable snapshot.BarCode
        // for the compiler, failing the railway with the same wrapper message as before.
        return await Task.FromResult(Result.Success(info.BarCode))
            .RequireValue("Failed to retrieve barcode details.")
            .ThenAsync(barCode =>
            {
                var entity = new Cycle
                {
                    MachineId = barCode.MachineId,
                    BarCodeId = new BarCodeId(info.BarCodeId),
                    StartedOn = dateTimeMachine.Now.ToLocalTime(),
                    FinishedOn = dateTimeMachine.Now.ToLocalTime(),
                    CycleTime = 0,
                    CyclesOk = 0,
                };

                // Story 6.4: the cycle status/part status come from the converged machine outcome (ON) or the legacy
                // literals (OFF). Apply them through the trusted create/apply seam (setters now private set).
                entity.ApplyCycleAndPartStatus(
                    outcome?.NextCycleStatus ?? CycleStatus.FinishedOk,
                    outcome?.NextPartStatus ?? PartStatus.Ok);

                // #114 chunk C: the cycle is STAGED on the loaded root (the chunk A / #95 Slice E path), not
                // persisted here — the single transactional aggregate save in UpdateBarcodeState flushes it
                // together with the staged barcode status write, so neither half can commit without the other.
                // The store-generated CycleId lands on this instance only after the durable commit (never the
                // rows-affected count — the PR #182 id-vs-count bug class stays fixed by construction).
                var staged = barCode.StageNewCycle(entity);
                if (staged is null || staged.IsFailure)
                {
                    return Task.FromResult(Result<BarCodeSnapshot>.WithFailure(
                        staged?.Errors ?? ["Failed to stage the new cycle on the barcode aggregate."]));
                }

                // #33 Chunk 5: thread the created cycle immutably onto the snapshot (was info.SetCycle(entity)); byte-equal —
                // the projection later reads info.Cycle exactly as the god-object's ToDto read the SetCycle-mutated value.
                return Task.FromResult(Result<BarCodeSnapshot>.Success(info with { Cycle = entity }));
            });
    }

    private async Task<Result<BarCodeSnapshot>> UpdateBarcodeState(TaskGatewayRequest request, BarCodeSnapshot info, TransitionOutcome? outcome, CancellationToken cancellationToken)
    {
        // Story 27.2b-2: reached only on the found path; RequireValue resolves the nullable snapshot.BarCode
        // for the compiler, failing the railway with the same wrapper message as before.
        return await Task.FromResult(Result.Success(info.BarCode))
            .RequireValue("Failed to retrieve barcode details.")
            .ThenAsync(async barCode =>
            {
                // Story 3.2 (AC1): persisted truth comes from the converged machine outcome (Finished/Ok) when the
                // flag is ON; the inline literals remain the fallback for the OFF/rollback path.
                // #114 chunk C: the flow/part/machine/modified fields are applied through the chunk B
                // BarCode.StageStatusWrite seam (which routes through the same trusted ApplyFlowAndPartStatus
                // apply path — byte-equal values) and FLAGGED for the single transactional save below, instead
                // of a separate IRepository<BarCode>.UpdateAsync auto-commit.
                var stagedStatus = barCode.StageStatusWrite(
                    (outcome?.NextFlowStatus ?? FlowStatus.Finished).Value,
                    (outcome?.NextPartStatus ?? PartStatus.Ok).Value,
                    request.MachineId,
                    dateTimeMachine.Now.ToLocalTime());
                if (stagedStatus is null || stagedStatus.IsFailure)
                {
                    return Result<BarCodeSnapshot>.WithFailure(
                        stagedStatus?.Errors ?? ["Failed to stage the barcode status write on the aggregate."]);
                }

                // Story 3.2 (AC2/AC3): make the PLC projection a READ-ONLY VIEW of the SAME outcome we persist, so the
                // request's CycleStatus/PartStatus/FlowStatus equal the persisted values — exactly one result. When the
                // flag is OFF, the projection keeps the legacy divergent EndOfProcess/NOk set by SetStatusEndOfProcess.
                if (outcome is not null)
                {
                    request.ProjectOutcome(outcome.NextFlowStatus, outcome.NextCycleStatus, outcome.NextPartStatus, outcome.Result);
                }

                // Story 5.1 (FR6): persist the converged barcode UNCONDITIONALLY. The routing flag governs WHICH
                // values were staged above (converged vs fallback literals), not WHETHER they persist.
                // #114 chunk C: ONE transactional save persists the staged FinishedOk cycle INSERT (staged in
                // CreateNewCycle) AND the barcode root status UPDATE in a single explicit transaction — a failure
                // of either half rolls back both, so the former split state (committed cycle + unfinished barcode)
                // is impossible and a PLC retry can no longer duplicate the cycle. A persistence failure fails the
                // operation (wired into the Result railway, not swallowed) and is surfaced via the existing
                // Story 3.3 diagnostics/exception handling in ProcessAsync.
                var saved = await barCodeAggregateRepository.SaveAsync(barCode, cancellationToken).ConfigureAwait(false);
                if (saved is null || saved.IsFailure)
                {
                    return Result<BarCodeSnapshot>.WithFailure(
                        saved?.Errors ?? ["The barcode aggregate save returned no result."]);
                }

                return Result<BarCodeSnapshot>.Success(info);
            });
    }

    /// <summary>
    /// Attempts to reset the command handler to its initial state.
    /// </summary>
    /// <returns>Always returns <c>true</c> indicating successful reset.</returns>
    /// <remarks>
    /// This implementation always succeeds as the handler is stateless and doesn't require cleanup.
    /// </remarks>
    public bool TryReset()
    {
        return true;
    }
}
