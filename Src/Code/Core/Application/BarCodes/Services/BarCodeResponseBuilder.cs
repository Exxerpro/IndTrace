// <copyright file="BarCodeResponseBuilder.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Builds final TaskGatewayResponseDto with all related data for barcode creation.
/// Handles the construction of the complete response object with proper data mapping.
/// </summary>
public class BarCodeResponseBuilder : IBarCodeResponseBuilder
{
    private readonly ILogger<BarCodeResponseBuilder> _logger;

    public BarCodeResponseBuilder(ILogger<BarCodeResponseBuilder> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Builds a complete TaskGatewayResponseDto using all the created entities and data.
    /// Maps all properties according to the established response structure using fluent builder pattern.
    /// </summary>
    /// <param name="barCode">The created BarCode entity</param>
    /// <param name="cycle">The created Cycle entity</param>
    /// <param name="machine">The validated Machine entity</param>
    /// <param name="references">The dictionary of reference variable registers</param>
    /// <param name="partNumber">The original part number from the request</param>
    /// <returns>Complete TaskGatewayResponseDto ready for client consumption</returns>
    public TaskGatewayResponseDto BuildResponse(BarCode barCode, Cycle cycle, Machine machine, 
        Dictionary<string, Register> references, string partNumber)
    {
        // Input validation
        if (barCode is null) throw new ArgumentNullException(nameof(barCode));
        if (cycle is null) throw new ArgumentNullException(nameof(cycle));
        if (machine is null) throw new ArgumentNullException(nameof(machine));
        if (references is null) throw new ArgumentNullException(nameof(references));
        if (string.IsNullOrWhiteSpace(partNumber)) throw new ArgumentException("Part number cannot be null or empty", nameof(partNumber));

        try
        {
            _logger.LogDebug("Building response for BarCodeId={BarCodeId}, CycleId={CycleId}, MachineId={MachineId}",
                barCode.BarCodeId.Value, cycle.CycleId.Value, machine.MachineId.Value);

            // #32 C2: build the immutable wire DTO directly (WithBarCode's derived Label rule inlined).
            // Story 35.D2 Cluster 5 (#35): the §7 DTO MachineId/LastMachineId/NextMachineId stay int; fed via .Value.
            var response = new TaskGatewayResponseDto
            {
                MachineId = machine.MachineId.Value,
                Description = machine.Name ?? "Unknown Machine",
                Name = machine.Name ?? "Unknown Machine",
                BarCodeId = barCode.BarCodeId.Value,
                CycleId = cycle.CycleId.Value,
                CyclesOk = cycle.CyclesOk,
                ResultValidation = ResultValidation.Valid,
                PartNumber = partNumber,
                LastMachineId = machine.MachineId.Value,
                NextMachineId = machine.MachineId.Value,
                CycleStatus = cycle.CycleStatus,
                FlowStatus = barCode.FlowStatus,
                PartStatus = barCode.PartStatus,
                MachineType = machine.MachineType,
                WorkFlowType = machine.WorkFlowType,
                Cycle = cycle,
                BarCode = barCode,
                Label = barCode.Label.Value,
                References = references,
            };

            _logger.LogDebug("Response built successfully with {ReferenceCount} references", references.Count);

            // Apply reference values to complete the response (existing business logic)
            var applyResult = ReferenceStamper.Apply(response);
            if (applyResult.IsFailure || applyResult.Value is null)
            {
                _logger.LogError("Failed to apply reference values to response: {Errors}",
                    string.Join(", ", applyResult.Errors ?? []));

                // Return response even if reference application fails (graceful degradation)
                // This matches the existing error handling pattern
            }
            else
            {
                response = applyResult.Value;
                _logger.LogDebug("Reference values applied successfully to response");
            }

            _logger.LogInformation("TaskGatewayResponseDto created successfully for BarCode={BarCodeLabel}", barCode.Label.Value);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building response for BarCodeId={BarCodeId}", barCode.BarCodeId.Value);
            
            // Return minimal response to prevent complete failure
            return CreateMinimalResponse(barCode, cycle, machine, partNumber);
        }
    }

    /// <summary>
    /// Creates a minimal response in case of errors during response building.
    /// Ensures graceful degradation when response building fails.
    /// </summary>
    private TaskGatewayResponseDto CreateMinimalResponse(BarCode barCode, Cycle cycle, Machine machine, string partNumber)
    {
        try
        {
            _logger.LogWarning("Creating minimal response due to error in full response building");
            
            return new TaskGatewayResponseDto
            {
                MachineId = machine.MachineId.Value,
                BarCodeId = barCode.BarCodeId.Value,
                CycleId = cycle.CycleId.Value,
                ResultValidation = ResultValidation.Valid,
                PartNumber = partNumber,
                CycleStatus = cycle.CycleStatus,
                FlowStatus = barCode.FlowStatus,
                PartStatus = barCode.PartStatus,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create even minimal response");
            
            // Absolute fallback - return empty response
            return new TaskGatewayResponseDto();
        }
    }
}