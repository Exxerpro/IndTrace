// <copyright file="OeeService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Oee.Services;

using IndTrace.Application.Oee.Commands;
using IndTrace.Domain.Constants;

/// <summary>
/// Service for OEE (Overall Equipment Effectiveness) calculations and data retrieval.
/// </summary>
public class OeeService : IOeeService
{
    private readonly ICommandHandler<CalculateOeeCommand, OeeMetrics> calculateOeeHandler;
    private readonly IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse> getHistoryHandler;
    private readonly IOeeRepository oeeRepository;
    private readonly ILogger<OeeService> logger;
    private readonly IDateTimeMachine dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="OeeService"/> class with required dependencies.
    /// </summary>
    /// <param name="calculateOeeHandler">Handler for OEE calculations.</param>
    /// <param name="getHistoryHandler">Handler for OEE history retrieval.</param>
    /// <param name="oeeRepository">Repository for OEE data persistence.</param>
    /// <param name="logger">Logger for service operations.</param>
    /// <param name="dateTimeMachine">The date time machine to use for timestamp operations. Defaults to new DateTimeMachine() if null.</param>
    public OeeService(
        ICommandHandler<CalculateOeeCommand, OeeMetrics> calculateOeeHandler,
        IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse> getHistoryHandler,
        IOeeRepository oeeRepository,
        ILogger<OeeService> logger,
        IDateTimeMachine? dateTimeMachine = null)
    {
        this.calculateOeeHandler = calculateOeeHandler;
        this.getHistoryHandler = getHistoryHandler;
        this.oeeRepository = oeeRepository;
        this.logger = logger;
        this.dateTimeMachine = dateTimeMachine ?? new DateTimeMachine();
    }

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
    public async Task<Result<OeeMetrics>> CalculateOeeAsync(
        int machineId,
        double totalTimeMinutes,
        double downtimeMinutes,
        double idealCycleTimeSeconds,
        int totalCount,
        int defectCount,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<OeeMetrics>.WithFailure("Operation cancelled.");
        }

        this.logger.LogDebug("Service: Calculating OEE for Machine {MachineId}", machineId);

        var command = new CalculateOeeCommand
        {
            MachineId = machineId,
            TotalTimeMinutes = totalTimeMinutes,
            DowntimeMinutes = downtimeMinutes,
            IdealCycleTimeSeconds = idealCycleTimeSeconds,
            TotalCount = totalCount,
            DefectCount = defectCount,
            Timestamp = this.dateTimeMachine.UtcNow,
        };

        var metrics = await this.calculateOeeHandler.ProcessAsync(command, cancellationToken);

        // #128: a FAILED metrics result must surface as a failure, not be masked as a measured 0% OEE.
        if (metrics.IsFailure)
        {
            this.logger.LogError("OEE calculation failed for Machine {MachineId}: {@Errors}", machineId, metrics.Errors);
            return metrics;
        }

        // Store the calculation result
        try
        {
            if (metrics.Value is not null)
            {
                await this.oeeRepository.StoreOeeCalculationAsync(
                    machineId,
                    metrics.Value,
                    command.Timestamp,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Failed to store OEE calculation result for Machine {MachineId}", machineId);

            // Don't fail the calculation if storage fails
        }

        return metrics;
    }

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
    public async Task<Result<GetOeeHistoryResponse>> GetOeeHistoryAsync(
        int? machineId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        OeePerformanceLevel? minPerformanceLevel = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            // #128: previously this logged and then fell through, running the query anyway. Fail fast instead.
            this.logger.LogInformation("Service: OEE history request cancelled for Machine {MachineId}", machineId);
            return Result<GetOeeHistoryResponse>.WithFailure("Operation cancelled.");
        }

        try
        {
            this.logger.LogDebug("Service: Getting OEE history for Machine {MachineId}", machineId);

            var query = new GetOeeHistoryQuery
            {
                MachineId = machineId,
                StartDate = startDate ?? this.dateTimeMachine.UtcNow.AddDays(-30), // Default to last 30 days
                EndDate = endDate ?? this.dateTimeMachine.UtcNow,
                MinPerformanceLevel = minPerformanceLevel,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };

            return await this.getHistoryHandler.HandleAsync(query, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<GetOeeHistoryResponse>.WithFailure($"Operation was processed whit error {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves the latest OEE metrics for a specific machine.
    /// </summary>
    /// <param name="machineId">Machine identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest OEE record for the machine, or null if not found.</returns>
    public async Task<OeeHistoryRecord?> GetLatestOeeAsync(
        int machineId,
        CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Service: Getting latest OEE for Machine {MachineId}", machineId);

        return await this.oeeRepository.GetLatestOeeAsync(machineId, cancellationToken);
    }

    /// <summary>
    /// Retrieves OEE summary statistics for machines within a date range.
    /// </summary>
    /// <param name="machineIds">Machine identifiers (optional, null means all machines).</param>
    /// <param name="startDate">Start date for the summary range (optional, defaults to 30 days ago).</param>
    /// <param name="endDate">End date for the summary range (optional, defaults to now).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OEE summary statistics.</returns>
    public async Task<IEnumerable<OeeSummaryRecord>> GetOeeSummaryAsync(
        IEnumerable<int>? machineIds = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug(
            "Service: Getting OEE summary for {MachineCount} machines",
            machineIds?.Count() ?? 0);

        return await this.oeeRepository.GetOeeSummaryAsync(
            machineIds,
            startDate ?? this.dateTimeMachine.UtcNow.AddDays(-30),
            endDate ?? this.dateTimeMachine.UtcNow,
            cancellationToken);
    }

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
    public Task<(bool IsValid, IEnumerable<string> Errors)> ValidateOeeParametersAsync(
        int machineId,
        double totalTimeMinutes,
        double downtimeMinutes,
        double idealCycleTimeSeconds,
        int totalCount,
        int defectCount)
    {
        var errors = new List<string>();

        if (machineId <= 0)
        {
            errors.Add("Machine ID must be greater than zero");
        }

        if (totalTimeMinutes <= 0)
        {
            errors.Add("Total time must be greater than zero");
        }

        if (downtimeMinutes < 0)
        {
            errors.Add("Downtime cannot be negative");
        }

        if (downtimeMinutes > totalTimeMinutes)
        {
            errors.Add("Downtime cannot exceed total time");
        }

        if (idealCycleTimeSeconds <= 0)
        {
            errors.Add("Ideal cycle time must be greater than zero");
        }

        if (totalCount < 0)
        {
            errors.Add("Total count cannot be negative");
        }

        if (defectCount < 0)
        {
            errors.Add("Defect count cannot be negative");
        }

        if (defectCount > totalCount)
        {
            errors.Add("Defect count cannot exceed total count");
        }

        return Task.FromResult((errors.Count == 0, errors.AsEnumerable()));
    }

    /// <summary>
    /// Gets the performance level thresholds for display purposes.
    /// </summary>
    /// <returns>Dictionary of performance levels and their threshold values.</returns>
    public Dictionary<OeePerformanceLevel, decimal> GetPerformanceLevelThresholds()
    {
        return new Dictionary<OeePerformanceLevel, decimal>
        {
            { OeePerformanceLevel.Poor, OeeConstants.PoorOeeThreshold },
            { OeePerformanceLevel.Fair, OeeConstants.FairOeeThreshold },
            { OeePerformanceLevel.Good, OeeConstants.GoodOeeThreshold },
            { OeePerformanceLevel.WorldClass, OeeConstants.WorldClassOeeThreshold },
        };
    }

    /// <summary>
    /// Formats OEE metrics for display in the UI.
    /// </summary>
    /// <param name="metrics">OEE metrics to format.</param>
    /// <param name="includePercentageSymbol">Whether to include % symbol.</param>
    /// <param name="decimalPlaces">Number of decimal places.</param>
    /// <returns>Formatted metrics for display.</returns>
    public OeeDisplayMetrics FormatMetricsForDisplay(
        OeeMetrics metrics,
        bool includePercentageSymbol = true,
        int decimalPlaces = 2)
    {
        var suffix = includePercentageSymbol ? "%" : string.Empty;
        var format = $"F{decimalPlaces}";

        var performanceLevelText = metrics.PerformanceLevel switch
        {
            OeePerformanceLevel.Poor => "Poor",
            OeePerformanceLevel.Fair => "Fair",
            OeePerformanceLevel.Good => "Good",
            OeePerformanceLevel.WorldClass => "World Class",
            _ => "Unknown",
        };

        var cssClass = metrics.PerformanceLevel switch
        {
            OeePerformanceLevel.Poor => "oee-poor",
            OeePerformanceLevel.Fair => "oee-fair",
            OeePerformanceLevel.Good => "oee-good",
            OeePerformanceLevel.WorldClass => "oee-world-class",
            _ => "oee-unknown",
        };

        return new OeeDisplayMetrics
        {
            Availability = $"{metrics.AvailabilityPercent.ToString(format)}{suffix}",
            Performance = $"{metrics.PerformancePercent.ToString(format)}{suffix}",
            Quality = $"{metrics.QualityPercent.ToString(format)}{suffix}",
            Oee = $"{metrics.OeePercent.ToString(format)}{suffix}",
            PerformanceLevel = performanceLevelText,
            PerformanceLevelCssClass = cssClass,
            RawMetrics = metrics,
        };
    }
}