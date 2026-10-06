// <copyright file="UpdateCycleFlowContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.UpdateCycles;

using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Issue #176 — immutable accumulating context for the unified cycle-update gateway flow
/// (<see cref="UpdateCyclesCommandHandler"/>, OK and NOT-OK dispatches). Each pipeline step (#179 will rewire
/// the handler onto it) reads the state accumulated so far and returns a NEW instance via a <c>with</c>
/// expression — mirroring the <c>BarCodeDetailContext</c> precedent — so no step ever mutates shared state.
/// Members are nullable (or the EnumModel <c>None</c> default) until the step that produces them has run.
/// </summary>
/// <remarks>
/// The members mirror EXACTLY the state the as-built handler threads through <c>ProcessInternalAsync</c>: the
/// adapted command scalars plus the dispatch-selected target/trigger pair, the immutable
/// <see cref="CycleUpdateLoadState"/>, the station-validation outcome, the D2 state-machine gate outcome, the
/// DECIDE-step context and strategy result, and the final projected §7 response. Purely additive — the handler
/// is not modified by this record's introduction.
/// </remarks>
public sealed record UpdateCycleFlowContext
{
    /// <summary>Gets the requesting machine identifier (<c>IUpdateCycleCommand.MachineId</c>).</summary>
    public int MachineId { get; init; }

    /// <summary>Gets the reported bar code (<c>IUpdateCycleCommand.BarCode</c>).</summary>
    public string? BarCode { get; init; }

    /// <summary>Gets the reported part number (<c>IUpdateCycleCommand.PartNumber</c>).</summary>
    public string? PartNumber { get; init; }

    /// <summary>
    /// Gets the dispatch-selected target cycle status (<see cref="CycleStatus.FinishedOk"/> for the OK dispatch,
    /// <see cref="CycleStatus.FinishedNok"/> for NOT-OK).
    /// </summary>
    public CycleStatus TargetStatus { get; init; } = CycleStatus.None;

    /// <summary>
    /// Gets the dispatch-selected gateway trigger (<see cref="GatewayTask.UpdateCycleOkAsync"/> /
    /// <see cref="GatewayTask.UpdateCycleNotOkAsync"/>).
    /// </summary>
    public GatewayTask Trigger { get; init; } = GatewayTask.None;

    /// <summary>Gets the immutable load snapshot (<c>IBarCodeInfoProvider.GetCycleUpdateLoadStateAsync</c>).</summary>
    public CycleUpdateLoadState? Load { get; init; }

    /// <summary>Gets the station-validation outcome (<c>IStationValidator.ValidateStation</c>).</summary>
    public StationValidationResult? StationValidation { get; init; }

    /// <summary>
    /// Gets the D2 pre-strategy state-machine gate outcome (<c>IItemStateMachine.Fire</c> on the load-time
    /// barcode), or null when the route flag skipped the fire or it has not run yet.
    /// </summary>
    public TransitionOutcome? GateOutcome { get; init; }

    /// <summary>Gets the immutable DECIDE-step context (<see cref="CycleUpdateLoadState.ToDecideContext"/>).</summary>
    public CycleUpdateContext? DecideContext { get; init; }

    /// <summary>Gets the strategy execution result (<c>ICycleUpdateStrategy.ExecuteAsync</c>).</summary>
    public CycleUpdateResult? UpdateResult { get; init; }

    /// <summary>Gets the final projected §7 response (<c>CycleUpdateProjection.ToResponse</c> + reference stamp).</summary>
    public TaskGatewayResponseDto? Response { get; init; }
}
