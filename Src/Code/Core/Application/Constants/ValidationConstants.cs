// <copyright file="ValidationConstants.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Constants;

/// <summary>
/// Contains validation-related constants for OEE application layer.
/// Eliminates magic strings and provides consistent error messaging.
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Error message templates for OEE validation.
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>Error message for null input parameters.</summary>
        public const string InputsCannotBeNull = "Inputs cannot be null";

        /// <summary>Error message for invalid OEE range.</summary>
        public const string OeeValueOutOfRange = "OEE value must be between 0.0 and 1.0";

        /// <summary>Error message for invalid availability range.</summary>
        public const string AvailabilityOutOfRange = "Availability must be between 0.0 and 1.0";

        /// <summary>Error message for invalid performance range.</summary>
        public const string PerformanceOutOfRange = "Performance must be between 0.0 and 1.5";

        /// <summary>Error message for invalid quality range.</summary>
        public const string QualityOutOfRange = "Quality must be between 0.0 and 1.0";

        /// <summary>Error message for negative production values.</summary>
        public const string NegativeProductionValues = "Production values cannot be negative";

        /// <summary>Error message for negative time values.</summary>
        public const string NegativeTimeValues = "Time values cannot be negative";

        /// <summary>Error message for invalid machine identifier.</summary>
        public const string InvalidMachineId = "Machine identifier must be greater than zero";

        /// <summary>Error message for invalid PLC identifier.</summary>
        public const string InvalidPlcId = "PLC identifier must be greater than zero";

        /// <summary>Error message for missing required data.</summary>
        public const string MissingRequiredData = "Required data fields are missing or invalid";
    }

    /// <summary>
    /// Warning message templates for OEE validation.
    /// </summary>
    public static class WarningMessages
    {
        /// <summary>Warning for performance over 100%.</summary>
        public const string PerformanceOverHundredPercent = "Performance exceeds 100% - verify cycle time calculations";

        /// <summary>Warning for very low availability.</summary>
        public const string VeryLowAvailability = "Availability below 50% - check for equipment issues";

        /// <summary>Warning for very low quality.</summary>
        public const string VeryLowQuality = "Quality below 70% - check for process issues";

        /// <summary>Warning for inconsistent data.</summary>
        public const string InconsistentData = "Data consistency issues detected - verify input data";

        /// <summary>Warning for zero production.</summary>
        public const string ZeroProduction = "No production recorded - verify data collection";

        /// <summary>Warning for excessive downtime.</summary>
        public const string ExcessiveDowntime = "Downtime exceeds normal operating parameters";

        /// <summary>Warning for missing cycle time data.</summary>
        public const string MissingCycleTimeData = "Cycle time data missing or zero - using fallback calculations";

        /// <summary>Warning for data age issues.</summary>
        public const string StaleData = "Data is older than recommended for real-time analysis";
    }

    /// <summary>
    /// Validation rule constants.
    /// </summary>
    public static class Rules
    {
        /// <summary>Maximum allowed string length for machine names.</summary>
        public const int MaxMachineNameLength = 50;

        /// <summary>Maximum allowed string length for part numbers.</summary>
        public const int MaxPartNumberLength = 100;

        /// <summary>Maximum allowed string length for labels.</summary>
        public const int MaxLabelLength = 200;

        /// <summary>Minimum required data points for trend analysis.</summary>
        public const int MinDataPointsForTrends = 5;

        /// <summary>Maximum data points for efficient processing.</summary>
        public const int MaxDataPointsPerQuery = 10000;

        /// <summary>Maximum age of data for real-time calculations (in hours).</summary>
        public const int MaxDataAgeHours = 24;

        /// <summary>Minimum machine ID value.</summary>
        public const int MinMachineId = 1;

        /// <summary>Minimum PLC ID value.</summary>
        public const int MinPlcId = 1;

        /// <summary>Maximum reasonable machine ID value.</summary>
        public const int MaxMachineId = 99999;

        /// <summary>Maximum reasonable PLC ID value.</summary>
        public const int MaxPlcId = 99999;
    }

    /// <summary>
    /// Success and informational message templates.
    /// </summary>
    public static class InfoMessages
    {
        /// <summary>Success message for OEE calculation.</summary>
        public const string OeeCalculationSuccess = "OEE calculation completed successfully";

        /// <summary>Info message for data processing.</summary>
        public const string DataProcessingComplete = "Data processing completed";

        /// <summary>Info message for validation passed.</summary>
        public const string ValidationPassed = "All validation checks passed";

        /// <summary>Info message for world-class performance.</summary>
        public const string WorldClassPerformance = "World-class OEE performance achieved";

        /// <summary>Info message for good performance.</summary>
        public const string GoodPerformance = "Good OEE performance level maintained";
    }

    /// <summary>
    /// Field name constants for consistent validation reporting.
    /// </summary>
    public static class FieldNames
    {
        /// <summary>OEE field name.</summary>
        public const string Oee = "OEE";

        /// <summary>Availability field name.</summary>
        public const string Availability = "Availability";

        /// <summary>Performance field name.</summary>
        public const string Performance = "Performance";

        /// <summary>Quality field name.</summary>
        public const string Quality = "Quality";

        /// <summary>Machine ID field name.</summary>
        public const string MachineId = "MachineId";

        /// <summary>PLC ID field name.</summary>
        public const string PlcId = "PlcId";

        /// <summary>Total production field name.</summary>
        public const string TotalProduction = "TotalProduction";

        /// <summary>Production OK field name.</summary>
        public const string ProductionOk = "ProductionOk";

        /// <summary>Production NOK field name.</summary>
        public const string ProductionNoK = "ProductionNoK";

        /// <summary>Running time field name.</summary>
        public const string RunningTime = "RunningTime";

        /// <summary>Planned production time field name.</summary>
        public const string PlannedProductionTime = "PlannedProductionTime";

        /// <summary>Standard cycle time field name.</summary>
        public const string StandardCycleTime = "StandardCycleTime";

        /// <summary>Actual cycle time field name.</summary>
        public const string ActualCycleTime = "ActualCycleTime";
    }

    // Direct constants required by OEE validation (for backward compatibility)

    /// <summary>Machine ID is required for OEE calculations.</summary>
    public const string MachineIdRequired = "Machine ID is required for OEE calculations";

    /// <summary>Total time must be greater than zero.</summary>
    public const string TotalTimeRequired = "Total time must be greater than zero";

    /// <summary>Downtime cannot be negative.</summary>
    public const string DowntimeNonNegative = "Downtime cannot be negative";

    /// <summary>Ideal cycle time must be greater than zero.</summary>
    public const string IdealCycleTimeRequired = "Ideal cycle time must be greater than zero";

    /// <summary>Total count cannot be negative.</summary>
    public const string TotalCountNonNegative = "Total count cannot be negative";

    /// <summary>Defect count cannot be negative.</summary>
    public const string DefectCountNonNegative = "Defect count cannot be negative";

    /// <summary>Downtime cannot exceed total time.</summary>
    public const string DowntimeExceedsTotalTime = "Downtime cannot exceed total time";

    /// <summary>Defect count cannot exceed total count.</summary>
    public const string DefectCountExceedsTotalCount = "Defect count cannot exceed total count";

    /// <summary>Timestamp is required for OEE calculations.</summary>
    public const string TimestampRequired = "Timestamp is required for OEE calculations";

    /// <summary>Start date is required.</summary>
    public const string StartDateRequired = "Start date is required";

    /// <summary>End date is required.</summary>
    public const string EndDateRequired = "End date is required";

    /// <summary>End date must be after start date.</summary>
    public const string EndDateMustBeAfterStartDate = "End date must be after start date";

    /// <summary>Date range is too large (maximum 90 days).</summary>
    public const string DateRangeTooLarge = "Date range is too large (maximum 90 days)";

    /// <summary>Page number must be 1 or greater.</summary>
    public const string PageNumberInvalid = "Page number must be 1 or greater";

    /// <summary>Page size must be between 1 and 1000.</summary>
    public const string PageSizeInvalid = "Page size must be between 1 and 1000";
}