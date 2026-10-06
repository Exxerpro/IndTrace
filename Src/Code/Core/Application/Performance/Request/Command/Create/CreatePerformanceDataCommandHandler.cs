// <copyright file="CreatePerformanceDataCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Performance.Request.Command.Create;

/// <summary>
/// Handles the creation of performance data commands and manages OEE and KPI OEE registration.
/// </summary>
public class CreatePerformanceDataCommandHandler(
    ILogger<CreatePerformanceDataCommandHandler> logger,
    IRepository<OeeRegister> oeeRegisterRepo,
    IRepository<KpiOee> kpiOeeRepo) : IGatewayRequestHandler<PerformanceDataCommand, TaskGatewayResponseDto>, IResettable // Fixed the incorrect generic usage
{
    /// <summary>
    /// Processes the performance data command and registers OEE and KPI OEE data.
    /// </summary>
    /// <param name="request">The performance data command to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the task gateway response.</returns>
    public async Task<Result<TaskGatewayResponseDto>> ProcessAsync(PerformanceDataCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<TaskGatewayResponseDto>.WithFailure("Request cannot be null");
        }

        // Respect cooperative cancellation without throwing exceptions
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<TaskGatewayResponseDto>.WithFailure("Operation was canceled.");
        }

        try
        {
            var response = TaskGatewayResponseDto.From(request.Command).Value ?? new TaskGatewayResponseDto();

            var performanceData = PerformanceDataCommand.ToEntity(request);

            var register = new OeeRegister();

            var resultCalculateOee = OeeRegister.CalculateOee(register, performanceData);
            if (resultCalculateOee.IsFailure)
            {
                foreach (var error in resultCalculateOee.Errors ?? [])
                {
                    logger.LogError("Error calculating OEE on Machine {MachineId}: {Error}", request.MachineId, error);
                }

                // #126 review C13 (PO decision 2026-07-15): invalid PLC inputs FAIL the OEE calculation —
                // the garbage-derived (zero-clamped) register must NOT be persisted and the failure must
                // reach the caller. The only gateway consumer (GatewayExecutor's fire-and-forget dispatch)
                // logs a failed Result loudly and skips, so a failure can never crash the message loop.
                return Result<TaskGatewayResponseDto>.WithFailure(
                    resultCalculateOee.Errors ?? ["OEE calculation failed for an invalid PLC sample."],
                    response);
            }

            if (resultCalculateOee.HasWarnings)
            {
                // Log warnings with confidence metadata for observability; do not fail the operation
                logger.LogWarning(
                    "OEE calculation for Machine {MachineId} completed with warnings. Confidence={Confidence:0.00}, MissingDataRatio={Missing:0.00}. Warnings: {Warnings}",
                    request.MachineId,
                    resultCalculateOee.Confidence,
                    resultCalculateOee.MissingDataRatio,
                    string.Join(", ", resultCalculateOee.Warnings));
            }

            var kpiOee = OeeRegister.ToKpiOee(register);

            var result2 = await oeeRegisterRepo.AddAsync(register, cancellationToken).ConfigureAwait(false);
            var result3 = await kpiOeeRepo.AddAsync(kpiOee, cancellationToken).ConfigureAwait(false);

            // Failure if any repo add failed or invalid IDs were returned
            var id1 = result2.IsSuccess ? (result2.Value is int v1 ? v1 : 0) : 0;
            var id2 = result3.IsSuccess ? (result3.Value is int v2 ? v2 : 0) : 0;
            var isFailure = result2.IsFailure || result3.IsFailure || id1 <= 0 || id2 <= 0;

            return isFailure
                ? Result<TaskGatewayResponseDto>.WithFailure("Error Adding Performance Data", response)
                : Result<TaskGatewayResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in CreatePerformanceDataCommandHandler");
            return Result<TaskGatewayResponseDto>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }

    /// <summary>
    /// Attempts to reset the state of the command handler.
    /// </summary>
    /// <returns>True if the reset was successful; otherwise, false.</returns>
    public bool TryReset()
    {
        // Reset state for the command handler
        // This is a no-op in this case, as we don't maintain any state in this handler.
        return true;
    }
}
