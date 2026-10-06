// <copyright file="IRegisterService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services;

/// <summary>
/// Service interface for Register-related operations, replacing RegisterRepositoryExtensions.
/// Enables proper dependency injection and testability.
/// </summary>
public interface IRegisterService
{
    /// <summary>
    /// Gets a list of registers grouped by machine for the specified cycle IDs and active variables.
    /// </summary>
    /// <param name="cycleIdList">The list of cycle IDs to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A list of registers grouped by machine, or a failure result if not found.</returns>
    Task<Result<List<Register>>> GetRegistersGroupedByMachineAsync(
        List<int> cycleIdList,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of register views with variable information for the specified cycle IDs.
    /// </summary>
    /// <param name="cycleIdList">The list of cycle IDs to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A list of register views, or a failure result if not found.</returns>
    Task<Result<List<RegisterView>>> GetRegistersWithVariablesAsync(
        List<int> cycleIdList,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of register views by cycle ID list, grouped by machine and variable.
    /// </summary>
    /// <param name="cycleIdList">The list of cycle IDs to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A list of register views, or a failure result if not found.</returns>
    Task<Result<List<RegisterView>>> GetRegisterByCycleIdListAsync(
        List<int> cycleIdList,
        CancellationToken cancellationToken);
}