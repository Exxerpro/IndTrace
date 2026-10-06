// <copyright file="PerformanceIndicator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

/// <summary>
/// Calculates and represents Overall Equipment Effectiveness (OEE) metrics including availability, performance, and quality indicators.
/// </summary>
/// <param name="productionData">The production data containing runtime metrics and production counts.</param>
/// <param name="capacity">The production capacity used for performance calculations.</param>
public class PerformanceIndicator(ProductionData productionData, double capacity)
{
    private readonly ProductionData productionData = productionData ?? throw new ArgumentNullException(nameof(productionData));

    /// <summary>
    /// Gets the Overall Equipment Effectiveness (OEE) percentage.
    /// </summary>
    public double Oee => this.CalculatePercentage(this.CalculateOee);

    /// <summary>
    /// Gets the availability percentage of the machine.
    /// </summary>
    public double Availability => this.CalculatePercentage(this.CalculateAvailability);

    /// <summary>
    /// Gets the performance percentage of the machine.
    /// </summary>
    public double Performance => this.CalculatePercentage(this.CalculatePerformance);

    /// <summary>
    /// Gets the quality percentage of the machine output.
    /// </summary>
    public double Quality => this.CalculatePercentage(this.CalculateQuality);

    /// <summary>
    /// Calculates the overall OEE value by multiplying availability, performance, and quality percentages.
    /// </summary>
    /// <returns>The OEE value as a decimal percentage.</returns>
    private double CalculateOee()
    {
        var availability = this.CalculateAvailability();
        var performance = this.CalculatePerformance();
        var quality = this.CalculateQuality();

        return availability * performance * quality / 10_000;
    }

    /// <summary>
    /// Calculates the availability percentage based on running time and stopping time.
    /// </summary>
    /// <returns>The availability percentage or 0 if no time data is available.</returns>
    private double CalculateAvailability()
    {
        return this.productionData.RunningTime + this.productionData.StoppingTime > 0
            ? (this.productionData.RunningTime / (this.productionData.RunningTime + this.productionData.StoppingTime)) * 100
            : 0;
    }

    /// <summary>
    /// Calculates the performance percentage based on produced pieces relative to capacity and running time.
    /// </summary>
    /// <returns>The performance percentage, or 0 if no running time is available or the capacity is not positive (a misconfigured capacity must never fabricate perfect performance).</returns>
    private double CalculatePerformance()
    {
        return this.productionData.RunningTime > 0 && capacity > 0
            ? (this.productionData.ProducedPieces / (this.productionData.RunningTime * capacity)) * 100
            : 0;
    }

    /// <summary>
    /// Calculates the quality percentage based on the ratio of good pieces to total pieces produced.
    /// </summary>
    /// <returns>The quality percentage or 0 if no pieces were produced.</returns>
    private double CalculateQuality()
    {
        return this.productionData.ProducedPieces > 0
            ? ((this.productionData.ProducedPieces - this.productionData.RejectedPieces) / this.productionData.ProducedPieces) * 100
            : 0;
    }

    /// <summary>
    /// Applies percentage range validation to ensure the result is within 0-100% bounds.
    /// </summary>
    /// <param name="calculationMethod">The calculation method to execute and validate.</param>
    /// <returns>The validated percentage value within the 0-100 range.</returns>
    private double CalculatePercentage(Func<double> calculationMethod)
    {
        return this.EnsurePercentageRange(calculationMethod());
    }

    /// <summary>
    /// Ensures the percentage value is within the valid range of 0 to 100.
    /// </summary>
    /// <param name="value">The percentage value to validate.</param>
    /// <returns>The value clamped to the 0-100 range.</returns>
    private double EnsurePercentageRange(double value)
    {
        return Math.Max(0, Math.Min(value, 100));
    }
}