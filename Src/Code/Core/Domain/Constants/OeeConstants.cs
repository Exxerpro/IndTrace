// <copyright file="OeeConstants.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Constants;

/// <summary>
/// Contains all OEE-related constants and thresholds based on industry standards.
/// Eliminates magic numbers and provides self-documenting business rules.
/// </summary>
public static class OeeConstants
{
    // Direct constants needed by domain services

    /// <summary>World-class OEE performance level (85% and above).</summary>
    public const decimal WorldClassOeeThreshold = 0.85m;

    /// <summary>Good OEE performance level (65% to 85%).</summary>
    public const decimal GoodOeeThreshold = 0.65m;

    /// <summary>Fair OEE performance level (40% to 65%).</summary>
    public const decimal FairOeeThreshold = 0.40m;

    /// <summary>Poor OEE performance level (below 40%).</summary>
    public const decimal PoorOeeThreshold = 0.0m;

    /// <summary>Conversion factor from decimal to percentage (0.85 becomes 85%).</summary>
    public const decimal PercentageConversion = 100.0m;

    /// <summary>Minimum allowed value for OEE metrics (0.0).</summary>
    public const decimal MinMetricValue = 0.0m;

    /// <summary>Maximum allowed value for OEE metrics (1.0 = 100%).</summary>
    public const decimal MaxMetricValue = 1.0m;

    /// <summary>
    /// OEE performance level thresholds based on industry benchmarks.
    /// </summary>
    public static class PerformanceThresholds
    {
        /// <summary>World-class OEE performance level (85% and above).</summary>
        public const double WorldClassOeeThreshold = 0.85;

        /// <summary>Good OEE performance level (65% to 85%).</summary>
        public const double GoodOeeThreshold = 0.65;

        /// <summary>Fair OEE performance level (40% to 65%).</summary>
        public const double FairOeeThreshold = 0.40;

        /// <summary>Maximum theoretical OEE value (100%).</summary>
        public const double MaxOeeValue = 1.0;

        /// <summary>Minimum OEE value (0%).</summary>
        public const double MinOeeValue = 0.0;

        /// <summary>Threshold for very low availability warning (50%).</summary>
        public const double VeryLowAvailabilityThreshold = 0.5;

        /// <summary>Threshold for very low quality warning (70%).</summary>
        public const double VeryLowQualityThreshold = 0.7;
    }

    /// <summary>
    /// Individual metric thresholds for availability, performance, and quality.
    /// </summary>
    public static class MetricThresholds
    {
        /// <summary>Maximum availability value (100%).</summary>
        public const double MaxAvailability = 1.0;

        /// <summary>Minimum availability value (0%).</summary>
        public const double MinAvailability = 0.0;

        /// <summary>Maximum normal performance value (100%).</summary>
        public const double MaxNormalPerformance = 1.0;

        /// <summary>Maximum allowable performance value (150% - allows overperformance).</summary>
        public const double MaxAllowablePerformance = 1.5;

        /// <summary>Minimum performance value (0%).</summary>
        public const double MinPerformance = 0.0;

        /// <summary>Maximum quality value (100%).</summary>
        public const double MaxQuality = 1.0;

        /// <summary>Minimum quality value (0%).</summary>
        public const double MinQuality = 0.0;
    }

    /// <summary>
    /// Calculation-related constants for OEE formulas.
    /// </summary>
    public static class Calculation
    {
        /// <summary>Decimal precision for OEE calculations (6 decimal places).</summary>
        public const int DecimalPrecision = 6;

        /// <summary>Default fallback value for ratios when denominator is zero.</summary>
        public const double DefaultFallbackRatio = 0.0;

        /// <summary>Maximum counter reset value for simulation data.</summary>
        public const int MaxCounterValue = 120;

        /// <summary>Simulation counter increment step.</summary>
        public const int CounterIncrementStep = 1;
    }

    /// <summary>
    /// Validation thresholds for data quality checks.
    /// </summary>
    public static class Validation
    {
        /// <summary>Maximum acceptable cycle time in seconds (24 hours).</summary>
        public const double MaxAcceptableCycleTime = 86400;

        /// <summary>Maximum acceptable production time in seconds (1 week).</summary>
        public const double MaxAcceptableProductionTime = 604800;

        /// <summary>Minimum meaningful production count.</summary>
        public const int MinMeaningfulProduction = 1;

        /// <summary>Maximum reasonable production count per cycle.</summary>
        public const int MaxReasonableProductionCount = 10000;
    }

    /// <summary>
    /// Time-related constants for OEE calculations.
    /// </summary>
    public static class Time
    {
        /// <summary>Seconds in one hour.</summary>
        public const int SecondsPerHour = 3600;

        /// <summary>Hours in one day.</summary>
        public const int HoursPerDay = 24;

        /// <summary>Seconds in one day.</summary>
        public const int SecondsPerDay = SecondsPerHour * HoursPerDay;

        /// <summary>Days in one week.</summary>
        public const int DaysPerWeek = 7;

        /// <summary>Seconds in one week.</summary>
        public const int SecondsPerWeek = SecondsPerDay * DaysPerWeek;
    }
}