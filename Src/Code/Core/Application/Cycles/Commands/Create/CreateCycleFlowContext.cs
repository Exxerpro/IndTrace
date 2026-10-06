// <copyright file="CreateCycleFlowContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.Cycles.Policies;

namespace IndTrace.Application.Cycles.Commands.Create;

/// <summary>
/// Issue #176 — immutable accumulating context for the create-cycle gateway flow
/// (<see cref="CreateCyclesCommandHandler"/>). Each pipeline step (#178 will rewire the handler onto it) reads
/// the state accumulated so far and returns a NEW instance via a <c>with</c> expression — mirroring the
/// <c>BarCodeDetailContext</c> precedent — so no step ever mutates shared state. Members are nullable
/// (or default) until the step that produces them has run.
/// </summary>
/// <remarks>
/// The members mirror EXACTLY the state the as-built handler threads through its branches: the incoming §7
/// request, the immutable load snapshot, the station-validation and cycle-limit outcomes, the state-machine
/// resolved status triple (Story 3.1 <c>ResolveCreateCycleStatuses</c>), the created cycle, and the final
/// projected §7 response. Purely additive — the handler is not modified by this record's introduction.
/// </remarks>
public sealed record CreateCycleFlowContext
{
    /// <summary>Gets the incoming §7 gateway request (<c>cmd.Command</c>), carrying MachineId/BarCode/PartNumber.</summary>
    public TaskGatewayRequest? Request { get; init; }

    /// <summary>Gets the immutable load snapshot produced by <c>IBarCodeDetailsLoader.LoadAsync</c>.</summary>
    public BarCodeSnapshot? Snapshot { get; init; }

    /// <summary>
    /// Gets a value indicating whether the station validator accepted the machine
    /// (<c>IStationValidator.ValidateCanStartCycles</c> succeeded — only process stations may start cycles).
    /// </summary>
    public bool StationValidated { get; init; }

    /// <summary>
    /// Gets the cycle-limit policy decision (<c>ICycleLimitPolicy.EvaluateCycleLimits</c>), or null when the
    /// gate was skipped (Restored barcodes) or has not run yet.
    /// </summary>
    public CycleLimitDecision? CycleLimitDecision { get; init; }

    /// <summary>Gets the state-machine-resolved next flow status (Story 3.1 <c>ResolveCreateCycleStatuses</c>).</summary>
    public FlowStatus? ResolvedFlowStatus { get; init; }

    /// <summary>Gets the state-machine-resolved next cycle status (mirrors the request when the machine rejects).</summary>
    public CycleStatus? ResolvedCycleStatus { get; init; }

    /// <summary>Gets the state-machine-resolved next part status (mirrors the request when the machine rejects).</summary>
    public PartStatus? ResolvedPartStatus { get; init; }

    /// <summary>Gets the persisted cycle produced by <c>ICycleCreator.CreateAsync</c>.</summary>
    public Cycle? CreatedCycle { get; init; }

    /// <summary>Gets the final projected §7 response (<c>BarCodeResultProjection.ToCreateResponse</c> + reference stamp).</summary>
    public TaskGatewayResponseDto? Response { get; init; }
}
