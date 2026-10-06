// <copyright file="ProductionExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

/// <summary>
/// Provides extension methods for generating production data with statistical distributions.
/// </summary>
public static class ProductionExtensions
{
    /// <summary>
    /// Generates new production data for a machine within a specified time period.
    /// </summary>
    /// <param name="machine">The performance record of the machine.</param>
    /// <param name="random">The random number generator.</param>
    /// <param name="startTime">The start time of the production period.</param>
    /// <param name="endTime">The end time of the production period.</param>
    /// <returns>New production data with statistically generated values.</returns>
    public static ProductionData GenerateNewData(this PerformanceRecord machine, Random random, DateTime startTime, DateTime endTime)
    {
        double runningFactor = random.SampleWithTendencyToTheRight(); // Generate negative skewed random between 0 and 1
        double defectiveRate = random.SampleWithTendencyToTheLeft(); // Generate positive skewed random between 0 and 1
        double productiveFactor = random.SampleWithTendencyToTheRight(); // Generate negative skewed random between 0 and 1

        var producedPieces = (int)(machine.Capacity * runningFactor * productiveFactor * (endTime - startTime).TotalMinutes);
        var defectivePieces = (int)(producedPieces * defectiveRate);

        var runningTime = (endTime - startTime).TotalMinutes * runningFactor;
        var stoppingTime = (endTime - startTime).TotalMinutes * (1 - runningFactor);

        var productionData = new ProductionData(producedPieces, defectivePieces, runningTime, stoppingTime, startTime, endTime);
        return productionData;
    }

    /// <summary>
    /// Generates historical production data for a machine in 5-minute intervals.
    /// </summary>
    /// <param name="machine">The performance record of the machine.</param>
    /// <param name="random">The random number generator.</param>
    /// <param name="startTime">The start time for historical data generation.</param>
    /// <param name="endTime">The end time for historical data generation.</param>
    public static void GeneratePastData(this PerformanceRecord machine, Random random, DateTime startTime, DateTime endTime)
    {
        var currentTime = startTime;
        while (currentTime < endTime)
        {
            var nextTime = currentTime.AddMinutes(5);
            var productionData = machine.GenerateNewData(random, currentTime, nextTime);
            machine.UpdateData(productionData);
            currentTime = nextTime;
        }
    }
}