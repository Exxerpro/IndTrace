// <copyright file="IOeeCalculationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Defines the contract for OEE (Overall Equipment Effectiveness) calculation services.
/// </summary>
public interface IOeeCalculationService
{
    /// <summary>
    /// Calculates OEE metrics based on operational data.
    /// </summary>
    /// <param name="totalTime">Total scheduled production time.</param>
    /// <param name="downtime">Total downtime.</param>
    /// <param name="idealCycleTime">Ideal cycle time per unit.</param>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="defectCount">Number of defective units.</param>
    /// <returns>A Result containing the calculated OEE metrics or validation errors.</returns>
    Result<OeeMetrics> CalculateOee(
        TimeSpan totalTime,
        TimeSpan downtime,
        TimeSpan idealCycleTime,
        int totalCount,
        int defectCount);

    /// <summary>
    /// Calculates availability component of OEE.
    /// </summary>
    /// <param name="totalTime">Total scheduled production time.</param>
    /// <param name="downtime">Total downtime.</param>
    /// <returns>Availability as a decimal (0.0 to 1.0).</returns>
    decimal CalculateAvailability(TimeSpan totalTime, TimeSpan downtime);

    /// <summary>
    /// Calculates performance component of OEE.
    /// </summary>
    /// <param name="idealCycleTime">Ideal cycle time per unit.</param>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="runTime">Actual run time (total time - downtime).</param>
    /// <returns>Performance as a decimal (0.0 to 1.0).</returns>
    decimal CalculatePerformance(TimeSpan idealCycleTime, int totalCount, TimeSpan runTime);

    /// <summary>
    /// Calculates quality component of OEE.
    /// </summary>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="defectCount">Number of defective units.</param>
    /// <returns>Quality as a decimal (0.0 to 1.0).</returns>
    decimal CalculateQuality(int totalCount, int defectCount);

    /// <summary>
    /// Validates the input parameters for OEE calculation.
    /// </summary>
    /// <param name="totalTime">Total scheduled production time.</param>
    /// <param name="downtime">Total downtime.</param>
    /// <param name="idealCycleTime">Ideal cycle time per unit.</param>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="defectCount">Number of defective units.</param>
    /// <returns>True if all parameters are valid, false otherwise.</returns>
    bool ValidateInputs(
        TimeSpan totalTime,
        TimeSpan downtime,
        TimeSpan idealCycleTime,
        int totalCount,
        int defectCount);
}