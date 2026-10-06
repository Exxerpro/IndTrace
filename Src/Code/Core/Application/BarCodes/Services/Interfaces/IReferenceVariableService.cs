// <copyright file="IReferenceVariableService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services.Interfaces;

/// <summary>
/// Handles reference variable collection and processing for barcode responses.
/// Manages the lookup and processing of reference variables specific to ReferenceTags group.
/// </summary>
public interface IReferenceVariableService
{
    /// <summary>
    /// Retrieves and processes reference variables for the given machine and cycle.
    /// Only includes variables from the ReferenceTags group as per business rules.
    /// </summary>
    /// <param name="machineId">The machine identifier</param>
    /// <param name="cycleId">The cycle identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Success with dictionary of variable names to Register values, Failure if processing fails</returns>
    Task<Result<Dictionary<string, Register>>> GetReferenceRegistersAsync(
        int machineId, int cycleId, CancellationToken cancellationToken);
}