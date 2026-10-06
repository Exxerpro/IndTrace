// <copyright file="QuestDbConstants.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Infrastructure.Constants;

/// <summary>
/// Contains all QuestDB-related constants for table names, columns, and queries.
/// Eliminates magic strings in database operations and provides consistent naming.
/// </summary>
public static class QuestDbConstants
{
    /// <summary>
    /// QuestDB table names for OEE data storage.
    /// </summary>
    public static class TableNames
    {
        /// <summary>Table for storing KPI OEE results.</summary>
        public const string KpiOee = "KpiOee";

        /// <summary>Table for storing raw performance data.</summary>
        public const string PerformanceData = "PerformanceData";

        /// <summary>Table for storing OEE calculation results.</summary>
        public const string OeeResults = "OeeResults";
    }

    /// <summary>
    /// Column names for OEE-related tables.
    /// </summary>
    public static class ColumnNames
    {
        /// <summary>OEE data identifier column.</summary>
        public const string OeeDataId = "OEEDataId";

        /// <summary>Availability metric column.</summary>
        public const string Availability = "Availability";

        /// <summary>Performance metric column.</summary>
        public const string Performance = "Performance";

        /// <summary>Quality metric column.</summary>
        public const string Quality = "Quality";

        /// <summary>Overall OEE value column.</summary>
        public const string Oee = "OEE";

        /// <summary>Timestamp column for time-series data.</summary>
        public const string Timestamp = "timestamp";

        /// <summary>Machine identifier column.</summary>
        public const string MachineId = "MachineId";

        /// <summary>PLC identifier column.</summary>
        public const string PlcId = "PlcId";

        /// <summary>Performance data identifier column.</summary>
        public const string PerformanceDataId = "PerformanceDataId";

        /// <summary>BarCode identifier column.</summary>
        public const string BarCodeId = "BarCodeId";

        /// <summary>Cycle identifier column.</summary>
        public const string CycleId = "CycleId";

        /// <summary>Application flag column.</summary>
        public const string ApplicationFlag = "ApplicationFlag";

        /// <summary>Event counter column.</summary>
        public const string EventCounter = "EventCounter";

        /// <summary>Current time column.</summary>
        public const string CurrentTime = "CurrentTime";

        /// <summary>Running time column.</summary>
        public const string RunningTime = "RunningTime";

        /// <summary>Stopped time column.</summary>
        public const string StoppedTime = "StoppedTime";

        /// <summary>Faulted time column.</summary>
        public const string FaultedTime = "FaultedTime";

        /// <summary>Status fault reason column.</summary>
        public const string StatusFaultReason = "StatusFaultReason";

        /// <summary>Total production column.</summary>
        public const string TotalProduction = "TotalProduction";

        /// <summary>Production OK column.</summary>
        public const string ProductionOk = "ProductionOk";

        /// <summary>Production NOK column.</summary>
        public const string ProductionNoK = "ProductionNoK";

        /// <summary>Status fault reject column.</summary>
        public const string StatusFaultReject = "StatusFaultReject";

        /// <summary>Reject event counter column.</summary>
        public const string RejectEventCounter = "RejectEventCounter";

        /// <summary>Status reject column.</summary>
        public const string StatusReject = "StatusReject";

        /// <summary>Reject quantity units column.</summary>
        public const string RejectQuantityUnits = "RejectQuantityUnits";

        /// <summary>Standard cycle time column.</summary>
        public const string StandardCycleTime = "StandardCycleTime";

        /// <summary>Actual cycle time column.</summary>
        public const string ActualCycleTime = "ActualCycleTime";

        /// <summary>Planned production time column.</summary>
        public const string PlanedProductionTime = "PlanedProductionTime";
    }

    /// <summary>
    /// Common SQL queries for OEE data retrieval.
    /// </summary>
    public static class Queries
    {
        /// <summary>Basic query to select all OEE results.</summary>
        public const string SelectAllOeeResults = "SELECT * FROM OeeResults";

        /// <summary>QueryAsync template for OEE results by machine ID.</summary>
        public const string SelectOeeResultsByMachine = "SELECT * FROM OeeResults WHERE MachineId = ?";

        /// <summary>QueryAsync template for OEE results within date range.</summary>
        public const string SelectOeeResultsByDateRange = "SELECT * FROM OeeResults WHERE timestamp BETWEEN ? AND ?";

        /// <summary>QueryAsync template for latest OEE results.</summary>
        public const string SelectLatestOeeResults = "SELECT * FROM OeeResults ORDER BY timestamp DESC LIMIT ?";
    }

    /// <summary>
    /// Connection and configuration constants.
    /// </summary>
    public static class Connection
    {
        /// <summary>Default QuestDB HTTP address.</summary>
        public const string DefaultHttpAddress = "http::addr=localhost:9000;";

        /// <summary>Default connection timeout in seconds.</summary>
        public const int DefaultTimeoutSeconds = 10;

        /// <summary>Maximum retry attempts for failed operations.</summary>
        public const int MaxRetryAttempts = 3;

        /// <summary>Default retry delay in milliseconds.</summary>
        public const int DefaultRetryDelayMs = 1000;

        /// <summary>Default batch size for bulk operations.</summary>
        public const int DefaultBatchSize = 1000;
    }

    /// <summary>
    /// HTTP endpoint paths for QuestDB REST API.
    /// </summary>
    public static class EndpointPaths
    {
        /// <summary>QueryAsync execution endpoint.</summary>
        public const string QueryExecution = "/exec";

        /// <summary>Import endpoint for data ingestion.</summary>
        public const string Import = "/imp";

        /// <summary>Health check endpoint.</summary>
        public const string Health = "/status";
    }

    /// <summary>
    /// Parameter names for query building.
    /// </summary>
    public static class QueryParameters
    {
        /// <summary>QueryAsync parameter name.</summary>
        public const string Query = "query";

        /// <summary>Limit parameter name.</summary>
        public const string Limit = "limit";

        /// <summary>Count parameter name.</summary>
        public const string Count = "count";

        /// <summary>Format parameter name.</summary>
        public const string Format = "fmt";
    }

    /// <summary>
    /// Response format constants.
    /// </summary>
    public static class ResponseFormats
    {
        /// <summary>JSON response format.</summary>
        public const string Json = "json";

        /// <summary>CSV response format.</summary>
        public const string Csv = "csv";

        /// <summary>Tab-separated values format.</summary>
        public const string Tsv = "tab";
    }
}