// <copyright file="IOeeService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Oee.Services;

using IndTrace.Application.Oee.Commands;

/// <summary>
/// Defines the contract for OEE service operations providing a clean abstraction for UI components.
/// </summary>
public interface IOeeService
{
    /// <summary>
    /// Calculates OEE metrics for a machine based on operational data.
    /// </summary>
    /// <param name="machineId">Machine identifier.</param>
    /// <param name="totalTimeMinutes">Total scheduled production time in minutes.</param>
    /// <param name="downtimeMinutes">Total downtime in minutes.</param>
    /// <param name="idealCycleTimeSeconds">Ideal cycle time per unit in seconds.</param>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="defectCount">Number of defective units.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result carrying the calculated OEE metrics, or a failure propagating the underlying error.</returns>
    Task<Result<OeeMetrics>> CalculateOeeAsync(
        int machineId,
        double totalTimeMinutes,
        double downtimeMinutes,
        double idealCycleTimeSeconds,
        int totalCount,
        int defectCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves OEE history for machines within a specified date range.
    /// </summary>
    /// <param name="machineId">Machine identifier (optional, null means all machines).</param>
    /// <param name="startDate">Start date for the history range.</param>
    /// <param name="endDate">End date for the history range.</param>
    /// <param name="minPerformanceLevel">Minimum performance level filter (optional).</param>
    /// <param name="pageNumber">Page number for pagination (1-based).</param>
    /// <param name="pageSize">Page size for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OEE history response with pagination information.</returns>
    Task<Result<GetOeeHistoryResponse>> GetOeeHistoryAsync(
        int? machineId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        OeePerformanceLevel? minPerformanceLevel = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest OEE metrics for a specific machine.
    /// </summary>
    /// <param name="machineId">Machine identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest OEE record for the machine, or null if not found.</returns>
    Task<OeeHistoryRecord?> GetLatestOeeAsync(
        int machineId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves OEE summary statistics for machines within a date range.
    /// </summary>
    /// <param name="machineIds">Machine identifiers (optional, null means all machines).</param>
    /// <param name="startDate">Start date for the summary range (optional, defaults to 30 days ago).</param>
    /// <param name="endDate">End date for the summary range (optional, defaults to now).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OEE summary statistics.</returns>
    Task<IEnumerable<OeeSummaryRecord>> GetOeeSummaryAsync(
        IEnumerable<int>? machineIds = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates OEE calculation parameters before processing.
    /// </summary>
    /// <param name="machineId">Machine identifier.</param>
    /// <param name="totalTimeMinutes">Total scheduled production time in minutes.</param>
    /// <param name="downtimeMinutes">Total downtime in minutes.</param>
    /// <param name="idealCycleTimeSeconds">Ideal cycle time per unit in seconds.</param>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="defectCount">Number of defective units.</param>
    /// <returns>Validation result with error messages if invalid.</returns>
    Task<(bool IsValid, IEnumerable<string> Errors)> ValidateOeeParametersAsync(
        int machineId,
        double totalTimeMinutes,
        double downtimeMinutes,
        double idealCycleTimeSeconds,
        int totalCount,
        int defectCount);

    /// <summary>
    /// Gets the performance level thresholds for display purposes.
    /// </summary>
    /// <returns>Dictionary of performance levels and their threshold values.</returns>
    Dictionary<OeePerformanceLevel, decimal> GetPerformanceLevelThresholds();

    /// <summary>
    /// Formats OEE metrics for display in the UI.
    /// </summary>
    /// <param name="metrics">OEE metrics to format.</param>
    /// <param name="includePercentageSymbol">Whether to include % symbol.</param>
    /// <param name="decimalPlaces">Number of decimal places.</param>
    /// <returns>Formatted metrics for display.</returns>
    OeeDisplayMetrics FormatMetricsForDisplay(
        OeeMetrics metrics,
        bool includePercentageSymbol = true,
        int decimalPlaces = 2);
}

/// <summary>
/// Represents OEE metrics formatted for display in the UI.
/// </summary>
public record OeeDisplayMetrics
{
    /// <summary>
    /// Gets the formatted availability string.
    /// </summary>
    public string Availability { get; init; } = string.Empty;

    /// <summary>
    /// Gets the formatted performance string.
    /// </summary>
    public string Performance { get; init; } = string.Empty;

    /// <summary>
    /// Gets the formatted quality string.
    /// </summary>
    public string Quality { get; init; } = string.Empty;

    /// <summary>
    /// Gets the formatted OEE string.
    /// </summary>
    public string Oee { get; init; } = string.Empty;

    /// <summary>
    /// Gets the performance level description.
    /// </summary>
    public string PerformanceLevel { get; init; } = string.Empty;

    /// <summary>
    /// Gets the CSS class for styling based on performance level.
    /// </summary>
    public string PerformanceLevelCssClass { get; init; } = string.Empty;

    /// <summary>
    /// Gets the raw metrics for calculations.
    /// </summary>
    public OeeMetrics RawMetrics { get; init; } = new(0, 0, 0);
}