// <copyright file="OeeCalculationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services.OEEServices;

using IndTrace.Domain.Constants;

/// <summary>
/// Implements the OEE (Overall Equipment Effectiveness) calculation service with industry-standard formulas.
/// </summary>
/// <remarks>
/// Initializes a new instance of the OeeCalculationService.
/// </remarks>
/// <param name="logger">Logger for diagnostic information.</param>
/// <summary>
/// Service for calculating OEE (Overall Equipment Effectiveness) metrics.
/// </summary>
/// <param name="logger">Logger for diagnostic information.</param>
public class OeeCalculationService : IOeeCalculationService
{
    private readonly ILogger<OeeCalculationService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OeeCalculationService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public OeeCalculationService(ILogger<OeeCalculationService> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Calculates OEE metrics based on operational data.
    /// </summary>
    /// <param name="totalTime">Total scheduled production time.</param>
    /// <param name="downtime">Total downtime.</param>
    /// <param name="idealCycleTime">Ideal cycle time per unit.</param>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="defectCount">Number of defective units.</param>
    /// <returns>A Result containing the calculated OEE metrics or validation errors.</returns>
    public Result<OeeMetrics> CalculateOee(
        TimeSpan totalTime,
        TimeSpan downtime,
        TimeSpan idealCycleTime,
        int totalCount,
        int defectCount)
    {
        var validationErrors = GetValidationErrors(totalTime, downtime, idealCycleTime, totalCount, defectCount);
        if (validationErrors.Count != 0)
        {
            this.logger.LogError("Validation failed for OEE calculation: {Errors}", string.Join(", ", validationErrors));
            return Result<OeeMetrics>.WithFailure(validationErrors);
        }

        this.logger.LogDebug(
            "Calculating OEE: TotalTime={TotalTime}, Downtime={Downtime}, IdealCycleTime={IdealCycleTime}, TotalCount={TotalCount}, DefectCount={DefectCount}",
            totalTime, downtime, idealCycleTime, totalCount, defectCount);

        var availability = this.CalculateAvailability(totalTime, downtime);
        var runTime = totalTime - downtime;
        var performance = this.CalculatePerformance(idealCycleTime, totalCount, runTime);
        var quality = this.CalculateQuality(totalCount, defectCount);

        var oeeMetrics = new OeeMetrics(availability, performance, quality);

        this.logger.LogInformation(
            "OEE calculated: Availability={Availability:P2}, Performance={Performance:P2}, Quality={Quality:P2}, OEE={Oee:P2}",
            oeeMetrics.Availability, oeeMetrics.Performance, oeeMetrics.Quality, oeeMetrics.Oee);

        return Result<OeeMetrics>.Success(oeeMetrics);
    }

    /// <summary>
    /// Calculates availability component of OEE.
    /// </summary>
    /// <param name="totalTime">Total scheduled production time.</param>
    /// <param name="downtime">Total downtime.</param>
    /// <returns>Availability as a decimal (0.0 to 1.0).</returns>
    public decimal CalculateAvailability(TimeSpan totalTime, TimeSpan downtime)
    {
        if (totalTime <= TimeSpan.Zero)
        {
            this.logger.LogWarning("Total time is zero or negative: {TotalTime}", totalTime);
            return OeeConstants.MinMetricValue;
        }

        if (downtime < TimeSpan.Zero)
        {
            this.logger.LogWarning("Downtime is negative: {Downtime}", downtime);
            downtime = TimeSpan.Zero;
        }

        if (downtime >= totalTime)
        {
            this.logger.LogWarning("Downtime exceeds total time: Downtime={Downtime}, TotalTime={TotalTime}", downtime, totalTime);
            return OeeConstants.MinMetricValue;
        }

        var runTime = totalTime - downtime;
        var availability = (decimal)runTime.TotalMinutes / (decimal)totalTime.TotalMinutes;

        this.logger.LogDebug(
            "Availability calculated: RunTime={RunTime}, TotalTime={TotalTime}, Availability={Availability:P2}",
            runTime, totalTime, availability);

        return Math.Max(OeeConstants.MinMetricValue, Math.Min(OeeConstants.MaxMetricValue, availability));
    }

    /// <summary>
    /// Calculates performance component of OEE.
    /// </summary>
    /// <param name="idealCycleTime">Ideal cycle time per unit.</param>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="runTime">Actual run time (total time - downtime).</param>
    /// <returns>Performance as a decimal (0.0 to 1.0).</returns>
    public decimal CalculatePerformance(TimeSpan idealCycleTime, int totalCount, TimeSpan runTime)
    {
        if (idealCycleTime <= TimeSpan.Zero)
        {
            this.logger.LogWarning("Ideal cycle time is zero or negative: {IdealCycleTime}", idealCycleTime);
            return OeeConstants.MinMetricValue;
        }

        if (totalCount < 0)
        {
            this.logger.LogWarning("Total count is negative: {TotalCount}", totalCount);
            return OeeConstants.MinMetricValue;
        }

        if (runTime <= TimeSpan.Zero)
        {
            this.logger.LogWarning("Run time is zero or negative: {RunTime}", runTime);
            return OeeConstants.MinMetricValue;
        }

        if (totalCount == 0)
        {
            this.logger.LogDebug("No units produced, performance is zero");
            return OeeConstants.MinMetricValue;
        }

        var idealProductionTime = TimeSpan.FromMilliseconds(idealCycleTime.TotalMilliseconds * totalCount);
        var performance = (decimal)idealProductionTime.TotalMinutes / (decimal)runTime.TotalMinutes;

        this.logger.LogDebug(
            "Performance calculated: IdealProductionTime={IdealProductionTime}, RunTime={RunTime}, Performance={Performance:P2}",
            idealProductionTime, runTime, performance);

        return Math.Max(OeeConstants.MinMetricValue, Math.Min(OeeConstants.MaxMetricValue, performance));
    }

    /// <summary>
    /// Calculates quality component of OEE.
    /// </summary>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="defectCount">Number of defective units.</param>
    /// <returns>Quality as a decimal (0.0 to 1.0).</returns>
    public decimal CalculateQuality(int totalCount, int defectCount)
    {
        if (totalCount < 0)
        {
            this.logger.LogWarning("Total count is negative: {TotalCount}", totalCount);
            return OeeConstants.MinMetricValue;
        }

        if (defectCount < 0)
        {
            this.logger.LogWarning("Defect count is negative: {DefectCount}", defectCount);
            defectCount = 0;
        }

        if (totalCount == 0)
        {
            this.logger.LogDebug("No units produced, quality is considered perfect");
            return OeeConstants.MaxMetricValue;
        }

        if (defectCount > totalCount)
        {
            this.logger.LogWarning("Defect count exceeds total count: DefectCount={DefectCount}, TotalCount={TotalCount}", defectCount, totalCount);
            return OeeConstants.MinMetricValue;
        }

        var goodCount = totalCount - defectCount;
        var quality = (decimal)goodCount / totalCount;

        this.logger.LogDebug(
            "Quality calculated: GoodCount={GoodCount}, TotalCount={TotalCount}, Quality={Quality:P2}",
            goodCount, totalCount, quality);

        return Math.Max(OeeConstants.MinMetricValue, Math.Min(OeeConstants.MaxMetricValue, quality));
    }

    /// <summary>
    /// Validates the input parameters for OEE calculation.
    /// </summary>
    /// <param name="totalTime">Total scheduled production time.</param>
    /// <param name="downtime">Total downtime.</param>
    /// <param name="idealCycleTime">Ideal cycle time per unit.</param>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="defectCount">Number of defective units.</param>
    /// <returns>True if all parameters are valid, false otherwise.</returns>
    public bool ValidateInputs(
        TimeSpan totalTime,
        TimeSpan downtime,
        TimeSpan idealCycleTime,
        int totalCount,
        int defectCount)
    {
        return GetValidationErrors(totalTime, downtime, idealCycleTime, totalCount, defectCount).Count == 0;
    }

    /// <summary>
    /// Gets validation errors for the input parameters for OEE calculation.
    /// </summary>
    /// <param name="totalTime">Total scheduled production time.</param>
    /// <param name="downtime">Total downtime.</param>
    /// <param name="idealCycleTime">Ideal cycle time per unit.</param>
    /// <param name="totalCount">Total units produced.</param>
    /// <param name="defectCount">Number of defective units.</param>
    /// <returns>List of validation error messages.</returns>
    private static List<string> GetValidationErrors(
        TimeSpan totalTime,
        TimeSpan downtime,
        TimeSpan idealCycleTime,
        int totalCount,
        int defectCount)
    {
        var errors = new List<string>();

        if (totalTime <= TimeSpan.Zero)
        {
            errors.Add($"Total time must be positive: {totalTime}");
        }

        if (downtime < TimeSpan.Zero)
        {
            errors.Add($"Downtime cannot be negative: {downtime}");
        }

        if (downtime > totalTime)
        {
            errors.Add($"Downtime cannot exceed total time: Downtime={downtime}, TotalTime={totalTime}");
        }

        if (idealCycleTime <= TimeSpan.Zero)
        {
            errors.Add($"Ideal cycle time must be positive: {idealCycleTime}");
        }

        if (totalCount < 0)
        {
            errors.Add($"Total count cannot be negative: {totalCount}");
        }

        if (defectCount < 0)
        {
            errors.Add($"Defect count cannot be negative: {defectCount}");
        }

        if (defectCount > totalCount)
        {
            errors.Add($"Defect count cannot exceed total count: DefectCount={defectCount}, TotalCount={totalCount}");
        }

        return errors;
    }
}