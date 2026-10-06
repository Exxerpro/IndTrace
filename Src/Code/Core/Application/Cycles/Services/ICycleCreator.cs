// <copyright file="ICycleCreator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services;

/// <summary>
/// Creates and persists new cycles with proper timestamps.
/// Based on CreateCyclesCommandHandler cycle creation logic.
/// </summary>
public interface ICycleCreator
{
    /// <summary>
    /// Creates and persists a new cycle with deterministic timestamps.
    /// </summary>
    /// <param name="request">Request containing cycle creation parameters.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing the created cycle or failure reasons.</returns>
    Task<Result<Cycle>> CreateAsync(
        CycleCreateRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Request for creating a new cycle. #114 chunk B: the request also carries the barcode ROOT status write
/// (<paramref name="FlowStatus"/> + <paramref name="ModifiedOn"/> alongside the shared machine/part fields) so
/// the creator can apply it to the loaded aggregate root and persist the cycle INSERT and the barcode UPDATE
/// in ONE transactional save — replacing the retired separate <c>BarCodeUpdater</c> auto-commit whose failure
/// left an orphan Started cycle.
/// </summary>
/// <param name="MachineId">The machine ID where the cycle is executed (also stamped on the barcode).</param>
/// <param name="BarCodeId">The barcode ID associated with the cycle.</param>
/// <param name="CycleStatus">The initial cycle status.</param>
/// <param name="PartStatus">The initial part status (also copied onto the barcode).</param>
/// <param name="StartedOn">When the cycle started.</param>
/// <param name="FinishedOn">When the cycle finished.</param>
/// <param name="FlowStatus">The resolved flow status to copy onto the barcode root.</param>
/// <param name="ModifiedOn">The barcode modification timestamp.</param>
public sealed record CycleCreateRequest(
    int MachineId,
    int BarCodeId,
    CycleStatus CycleStatus,
    PartStatus PartStatus,
    DateTimeOffset StartedOn,
    DateTimeOffset FinishedOn,
    FlowStatus FlowStatus,
    DateTimeOffset ModifiedOn);