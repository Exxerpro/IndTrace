// <copyright file="IShiftService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Services;

using IndTrace.Application.Shifts.Commands.Create;

/// <summary>
/// Defines a contract for shift management operations including creation and retrieval of shifts and cycles.
/// </summary>
public interface IShiftService
{
    /// <summary>
    /// Creates a new shift or retrieves an existing shift for the specified machine, and updates cycle information.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the shift creation or retrieval.</returns>
    Task<Result<ShiftCreatedEvent>> CreateOrRetrieveShiftAndCyclesOkAsync(int machineId, CancellationToken cancellationToken);
}