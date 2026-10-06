// <copyright file="ProductionData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

/// <summary>
/// Represents production data for a specific time period including pieces produced, rejected, and timing information.
/// </summary>
public class ProductionData(
    double producedPieces,
    double rejectedPieces,
    double runningTime,
    double stoppingTime,
    DateTime startTime,
    DateTime endTime)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionData"/> class.
    /// </summary>
    public ProductionData()
        : this(0, 0, 0, 0, DateTime.MaxValue, DateTime.MinValue)
    {
    }

    /// <summary>
    /// Gets the number of pieces produced (non-negative).
    /// </summary>
    public double ProducedPieces { get; private set; } = Math.Max(0, producedPieces);

    /// <summary>
    /// Gets the number of pieces rejected (non-negative).
    /// </summary>
    public double RejectedPieces { get; private set; } = Math.Max(0, rejectedPieces);

    /// <summary>
    /// Gets the running time in minutes (non-negative).
    /// </summary>
    public double RunningTime { get; private set; } = Math.Max(0, runningTime);

    /// <summary>
    /// Gets the stopping time in minutes (non-negative).
    /// </summary>
    public double StoppingTime { get; private set; } = Math.Max(0, stoppingTime);

    /// <summary>
    /// Gets the start time of the production period.
    /// </summary>
    public DateTime StartTime { get; private set; } = startTime;

    /// <summary>
    /// Gets the end time of the production period.
    /// </summary>
    public DateTime EndTime { get; private set; } = endTime;

    /// <summary>
    /// Adds production data to the current instance, combining the values.
    /// </summary>
    /// <param name="productionData">The production data to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when production data is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when production data values are negative.</exception>
    public void AddProductionData(ProductionData productionData)
    {
        if (productionData == null)
        {
            throw new ArgumentNullException(nameof(productionData), "Production data cannot be null.");
        }

        if (productionData.ProducedPieces < 0 || productionData.RejectedPieces < 0 || productionData.RunningTime < 0 || productionData.StoppingTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(productionData), "Production data values cannot be negative.");
        }

        this.ProducedPieces += productionData.ProducedPieces;
        this.RejectedPieces += productionData.RejectedPieces;
        this.RunningTime += productionData.RunningTime;
        this.StoppingTime += productionData.StoppingTime;

        if (productionData.StartTime < this.StartTime)
        {
            this.StartTime = productionData.StartTime;
        }

        if (productionData.EndTime > this.EndTime)
        {
            this.EndTime = productionData.EndTime;
        }
    }

    /// <summary>
    /// Adds two production data instances together.
    /// </summary>
    /// <param name="pd1">The first production data instance.</param>
    /// <param name="pd2">The second production data instance.</param>
    /// <returns>A new production data instance with combined values.</returns>
    public static ProductionData operator +(ProductionData pd1, ProductionData pd2)
    {
        return new ProductionData(
            pd1.ProducedPieces + pd2.ProducedPieces,
            pd1.RejectedPieces + pd2.RejectedPieces,
            pd1.RunningTime + pd2.RunningTime,
            pd1.StoppingTime + pd2.StoppingTime,
            pd1.StartTime < pd2.StartTime ? pd1.StartTime : pd2.StartTime,
            pd1.EndTime > pd2.EndTime ? pd1.EndTime : pd2.EndTime);
    }

    /// <summary>
    /// Returns a string representation of the production data.
    /// </summary>
    /// <returns>A formatted string containing all production data values.</returns>
    public override string ToString()
    {
        return $"StartTime: {this.StartTime}, EndTime: {this.EndTime}, ProducedPieces: {this.ProducedPieces}, RejectedPieces: {this.RejectedPieces}, RunningTime: {this.RunningTime} minutes, StoppingTime: {this.StoppingTime} minutes";
    }

}