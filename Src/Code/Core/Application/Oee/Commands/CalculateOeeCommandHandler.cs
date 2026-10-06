// <copyright file="CalculateOeeCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Oee.Commands;

using IndTrace.Application.Mediator;

/// <summary>
/// Handles the calculation of OEE metrics.
/// </summary>
public class CalculateOeeCommandHandler : ICommandHandler<CalculateOeeCommand, OeeMetrics>
{
    private readonly IOeeCalculationService oeeCalculationService;
    private readonly IValidator<CalculateOeeCommand> validator;
    private readonly ILogger<CalculateOeeCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalculateOeeCommandHandler"/> class.
    /// Initializes a new instance of the CalculateOeeCommandHandler.
    /// </summary>
    /// <param name="oeeCalculationService">Service for OEE calculations.</param>
    /// <param name="validator">Validator for the command.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    /// <summary>
    /// Initializes a new instance of the CalculateOeeCommandHandler class.
    /// </summary>
    /// <param name="oeeCalculationService">The OEE calculation service.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="logger">The logger.</param>
    public CalculateOeeCommandHandler(
        IOeeCalculationService oeeCalculationService,
        IValidator<CalculateOeeCommand> validator,
        ILogger<CalculateOeeCommandHandler> logger)
    {
        this.oeeCalculationService = oeeCalculationService;
        this.validator = validator;
        this.logger = logger;
    }

    /// <summary>
    /// Handles the OEE calculation command.
    /// </summary>
    /// <param name="command">The OEE calculation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the calculated OEE metrics or validation errors.</returns>
    public async Task<Result<OeeMetrics>> ProcessAsync(CalculateOeeCommand command, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation(
            "Processing OEE calculation for Machine {MachineId} at {Timestamp}",
            command.MachineId, command.Timestamp);

        // Validate the command
        var validationResult = await this.validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            this.logger.LogError("Validation failed for OEE calculation command: {Errors}", string.Join(", ", errors));
            return Result<OeeMetrics>.WithFailure(errors);
        }

        try
        {
            // Convert command parameters to TimeSpan objects
            var totalTime = TimeSpan.FromMinutes(command.TotalTimeMinutes);
            var downtime = TimeSpan.FromMinutes(command.DowntimeMinutes);
            var idealCycleTime = TimeSpan.FromSeconds(command.IdealCycleTimeSeconds);

            // Calculate OEE metrics
            var oeeResult = this.oeeCalculationService.CalculateOee(
                totalTime,
                downtime,
                idealCycleTime,
                command.TotalCount,
                command.DefectCount);

            if (oeeResult.IsFailure)
            {
                this.logger.LogError(
                    "OEE calculation failed for Machine {MachineId}: {Errors}",
                    command.MachineId, string.Join(", ", oeeResult.Errors));
                return oeeResult;
            }

            this.logger.LogInformation(
                "OEE calculation completed for Machine {MachineId}: OEE={Oee:P2}, Level={Level}",
                command.MachineId, oeeResult.Value?.Oee, oeeResult.Value?.PerformanceLevel);

            return oeeResult;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error occurred while calculating OEE for Machine {MachineId}", command.MachineId);
            return Result<OeeMetrics>.WithFailure($"Unexpected error: {ex.Message}");
        }
    }
}