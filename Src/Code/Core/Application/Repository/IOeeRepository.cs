// <copyright file="IOeeRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

/// <summary>
/// Defines the contract for OEE data repository operations.
/// </summary>
public interface IOeeRepository
{
    /// <summary>
    /// Retrieves OEE history records based on the specified criteria.
    /// </summary>
    /// <param name="machineId">Machine identifier (optional, null means all machines).</param>
    /// <param name="startDate">Start date for the history range.</param>
    /// <param name="endDate">End date for the history range.</param>
    /// <param name="minPerformanceLevel">Minimum performance level filter (optional).</param>
    /// <param name="pageNumber">Page number for pagination (1-based).</param>
    /// <param name="pageSize">Page size for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the OEE history records and total count.</returns>
    Task<(IEnumerable<OeeHistoryRecord> Records, int TotalCount)> GetOeeHistoryAsync(
        int? machineId,
        DateTime startDate,
        DateTime endDate,
        OeePerformanceLevel? minPerformanceLevel,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores OEE calculation results.
    /// </summary>
    /// <param name="machineId">Machine identifier.</param>
    /// <param name="metrics">OEE metrics to store.</param>
    /// <param name="timestamp">Calculation timestamp.</param>
    /// <param name="shift">Shift information (optional).</param>
    /// <param name="product">Product information (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored record identifier.</returns>
    Task<long> StoreOeeCalculationAsync(
        int machineId,
        OeeMetrics metrics,
        DateTime timestamp,
        string? shift = null,
        string? product = null,
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
    /// <param name="startDate">Start date for the summary range.</param>
    /// <param name="endDate">End date for the summary range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OEE summary statistics.</returns>
    Task<IEnumerable<OeeSummaryRecord>> GetOeeSummaryAsync(
        IEnumerable<int>? machineIds,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes OEE records older than the specified date.
    /// </summary>
    /// <param name="cutoffDate">Date before which records should be deleted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of records deleted.</returns>
    Task<int> DeleteOldRecordsAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents OEE summary statistics for a machine.
/// </summary>
public record OeeSummaryRecord
{
    /// <summary>
    /// Gets the machine identifier.
    /// </summary>
    public int MachineId { get; init; }

    /// <summary>
    /// Gets the machine name.
    /// </summary>
    public string MachineName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the average OEE for the period.
    /// </summary>
    public decimal AverageOee { get; init; }

    /// <summary>
    /// Gets the average availability for the period.
    /// </summary>
    public decimal AverageAvailability { get; init; }

    /// <summary>
    /// Gets the average performance for the period.
    /// </summary>
    public decimal AveragePerformance { get; init; }

    /// <summary>
    /// Gets the average quality for the period.
    /// </summary>
    public decimal AverageQuality { get; init; }

    /// <summary>
    /// Gets the minimum OEE recorded in the period.
    /// </summary>
    public decimal MinOee { get; init; }

    /// <summary>
    /// Gets the maximum OEE recorded in the period.
    /// </summary>
    public decimal MaxOee { get; init; }

    /// <summary>
    /// Gets the number of calculations in the period.
    /// </summary>
    public int CalculationCount { get; init; }

    /// <summary>
    /// Gets the dominant performance level for the period.
    /// </summary>
    public OeePerformanceLevel DominantPerformanceLevel { get; init; }

    /// <summary>
    /// Gets the start date of the summary period.
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// Gets the end date of the summary period.
    /// </summary>
    public DateTime EndDate { get; init; }
}