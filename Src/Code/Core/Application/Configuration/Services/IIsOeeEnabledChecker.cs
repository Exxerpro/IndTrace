// <copyright file="IIsOeeEnabledChecker.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Configuration.Services;

/// <summary>
/// Interface for checking OEE (Overall Equipment Effectiveness) feature enablement for machines.
/// </summary>
/// <remarks>
/// This service determines which machines have OEE capabilities enabled and provides
/// configuration information for OEE data collection and monitoring.
/// </remarks>
public interface IIsOeeEnabledChecker
{
    /// <summary>
    /// Checks OEE feature enablement status for the specified machine IDs.
    /// </summary>
    /// <param name="machineIds">The list of machine IDs to check for OEE enablement.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A Result containing OeeConfiguration with enablement status for each machine.</returns>
    /// <remarks>
    /// The returned OeeConfiguration contains:
    /// - Global OEE enablement flag
    /// - Per-machine enablement status dictionary
    /// - Any additional OEE configuration parameters.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when machineIds is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    Task<Result<OeeConfiguration>> CheckOeeFeatureByMachineIdsAsync(List<int> machineIds, CancellationToken cancellationToken);
}