// <copyright file="OeeMetrics.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using IndTrace.Domain.Constants;

/// <summary>
/// Represents the Overall Equipment Effectiveness (OEE) metrics as an immutable value object.
/// </summary>
public record OeeMetrics
{
    /// <summary>
    /// Gets the availability percentage (0.0 to 1.0).
    /// </summary>
    public decimal Availability { get; init; }

    /// <summary>
    /// Gets the performance percentage (0.0 to 1.0).
    /// </summary>
    public decimal Performance { get; init; }

    /// <summary>
    /// Gets the quality percentage (0.0 to 1.0).
    /// </summary>
    public decimal Quality { get; init; }

    /// <summary>
    /// Gets the calculated Overall Equipment Effectiveness (0.0 to 1.0).
    /// </summary>
    public decimal Oee => this.Availability * this.Performance * this.Quality;

    /// <summary>
    /// Gets the performance level classification based on the OEE value.
    /// </summary>
    public OeePerformanceLevel PerformanceLevel => ClassifyLevel(this.Oee);

    /// <summary>
    /// Initializes a new instance of the <see cref="OeeMetrics"/> class (non-throwing). Values are clamped to valid range.
    /// </summary>
    /// <param name="availability">The availability percentage (0.0 to 1.0).</param>
    /// <param name="performance">The performance percentage (0.0 to 1.0).</param>
    /// <param name="quality">The quality percentage (0.0 to 1.0).</param>
    public OeeMetrics(decimal availability, decimal performance, decimal quality)
    {
        this.Availability = ClampMetric(availability);
        this.Performance = ClampMetric(performance);
        this.Quality = ClampMetric(quality);
    }

    /// <summary>
    /// Creates OEE metrics from percentage values (0-100).
    /// </summary>
    /// <param name="availabilityPercent">Availability percentage (0-100).</param>
    /// <param name="performancePercent">Performance percentage (0-100).</param>
    /// <param name="qualityPercent">Quality percentage (0-100).</param>
    /// <returns>A new OeeMetrics instance.</returns>
    public static OeeMetrics FromPercentages(decimal availabilityPercent, decimal performancePercent, decimal qualityPercent)
    {
        return new OeeMetrics(
            availabilityPercent / OeeConstants.PercentageConversion,
            performancePercent / OeeConstants.PercentageConversion,
            qualityPercent / OeeConstants.PercentageConversion);
    }

    /// <summary>
    /// Gets the availability as a percentage (0-100).
    /// </summary>
    public decimal AvailabilityPercent => this.Availability * OeeConstants.PercentageConversion;

    /// <summary>
    /// Gets the performance as a percentage (0-100).
    /// </summary>
    public decimal PerformancePercent => this.Performance * OeeConstants.PercentageConversion;

    /// <summary>
    /// Gets the quality as a percentage (0-100).
    /// </summary>
    public decimal QualityPercent => this.Quality * OeeConstants.PercentageConversion;

    /// <summary>
    /// Gets the OEE as a percentage (0-100).
    /// </summary>
    public decimal OeePercent => this.Oee * OeeConstants.PercentageConversion;

    /// <summary>
    /// Clamps a metric to the valid range without throwing.
    /// </summary>
    private static decimal ClampMetric(decimal value) =>
        value < OeeConstants.MinMetricValue ? OeeConstants.MinMetricValue :
        (value > OeeConstants.MaxMetricValue ? OeeConstants.MaxMetricValue : value);

    /// <summary>
    /// Builds OeeMetrics validating inputs using functional Result pattern.
    /// </summary>
    /// <param name="availability">The availability percentage (0.0 to 1.0).</param>
    /// <param name="performance">The performance percentage (0.0 to 1.0).</param>
    /// <param name="quality">The quality percentage (0.0 to 1.0).</param>
    /// <remarks>
    /// Availability, Performance, and Quality must be between 0.0 and 1.0 (inclusive).
    /// </remarks>
    /// <Returns>A Result containing either a valid OeeMetrics instance or validation errors.</Returns>
    /// <returns></returns>
    public static Result<OeeMetrics> Build(decimal availability, decimal performance, decimal quality)
    {
        var errors = new List<string>();
        if (availability is < OeeConstants.MinMetricValue or > OeeConstants.MaxMetricValue)
        {
            errors.Add($"availability must be between {OeeConstants.MinMetricValue} and {OeeConstants.MaxMetricValue}");
        }

        if (performance is < OeeConstants.MinMetricValue or > OeeConstants.MaxMetricValue)
        {
            errors.Add($"performance must be between {OeeConstants.MinMetricValue} and {OeeConstants.MaxMetricValue}");
        }

        if (quality is < OeeConstants.MinMetricValue or > OeeConstants.MaxMetricValue)
        {
            errors.Add($"quality must be between {OeeConstants.MinMetricValue} and {OeeConstants.MaxMetricValue}");
        }

        return errors.Count > 0
            ? IndQuestResults.Result<OeeMetrics>.WithFailure(errors)
            : IndQuestResults.Result<OeeMetrics>.Success(new OeeMetrics(availability, performance, quality));
    }

    /// <summary>
    /// Classifies an OEE value into its <see cref="OeePerformanceLevel"/> band using the industry-standard
    /// thresholds in <see cref="OeeConstants"/> (World-class &gt;= 0.85, Good &gt;= 0.65, Fair &gt;= 0.40,
    /// otherwise Poor). This is the single source of OEE banding; both the instance
    /// <see cref="PerformanceLevel"/> accessor and entity-level classifiers (for example
    /// <c>KpiOee.ClassifyOeeLevel</c>) delegate here so the bands never diverge.
    /// </summary>
    /// <param name="oeeValue">The OEE value (typically 0.0 to 1.0).</param>
    /// <returns>The corresponding performance level.</returns>
    public static OeePerformanceLevel ClassifyLevel(decimal oeeValue)
    {
        return oeeValue switch
        {
            >= OeeConstants.WorldClassOeeThreshold => OeePerformanceLevel.WorldClass,
            >= OeeConstants.GoodOeeThreshold => OeePerformanceLevel.Good,
            >= OeeConstants.FairOeeThreshold => OeePerformanceLevel.Fair,
            _ => OeePerformanceLevel.Poor,
        };
    }
}