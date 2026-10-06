// <copyright file="ReferenceVariableService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services.Interfaces;
using IndTrace.Domain.Enum;

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Handles reference variable collection and processing for barcode responses.
/// Manages the lookup and processing of reference variables specific to ReferenceTags group.
/// </summary>
public class ReferenceVariableService : IReferenceVariableService
{
    private readonly IReadOnlyRepository<Variable> _variableRepository;
    private readonly ILogger<ReferenceVariableService> _logger;

    public ReferenceVariableService(
        IReadOnlyRepository<Variable> variableRepository,
        ILogger<ReferenceVariableService> logger)
    {
        _variableRepository = variableRepository ?? throw new ArgumentNullException(nameof(variableRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves and processes reference variables for the given machine and cycle.
    /// Only includes variables from the ReferenceTags group as per business rules.
    /// </summary>
    /// <param name="machineId">The machine identifier</param>
    /// <param name="cycleId">The cycle identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Success with dictionary of variable names to Register values, Failure if processing fails</returns>
    public async Task<Result<Dictionary<string, Register>>> GetReferenceRegistersAsync(
        int machineId, int cycleId, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Dictionary<string, Register>>.WithFailure(["Operation was canceled."]);
        }

        // Null guard for dependencies
        if (_variableRepository is null)
        {
            return Result<Dictionary<string, Register>>.WithFailure(["_variableRepository cannot be null."]);
        }

        try
        {
            _logger.LogDebug("Getting reference variables for MachineId={MachineId}, CycleId={CycleId}", machineId, cycleId);

            // Step 1: Get variables from ReferenceTags group for the machine
            var variablesResult = await GetReferenceVariablesAsync(machineId, cancellationToken);
            if (variablesResult.IsFailure || variablesResult.Value is null)
            {
                return Result<Dictionary<string, Register>>.WithFailure(variablesResult.Errors);
            }

            var variables = variablesResult.Value;
            _logger.LogDebug("Retrieved {Count} reference variables for MachineId={MachineId}", variables.Count(), machineId);

            // Step 2: Convert variables to registers dictionary
            var registersResult = ConvertVariablesToRegisters(variables, cycleId);
            if (registersResult.IsFailure || registersResult.Value is null)
            {
                return Result<Dictionary<string, Register>>.WithFailure(registersResult.Errors);
            }

            var registers = registersResult.Value;
            _logger.LogInformation("Successfully created {Count} reference registers for MachineId={MachineId}, CycleId={CycleId}",
                registers.Count, machineId, cycleId);

            return Result<Dictionary<string, Register>>.Success(registers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reference variable processing error for MachineId={MachineId}, CycleId={CycleId}", machineId, cycleId);
            return Result<Dictionary<string, Register>>.WithFailure([$"Reference variable processing failed: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Retrieves active reference variables for the specified machine.
    /// Business rule: Only ReferenceTags group variables are included.
    /// </summary>
    private async Task<Result<IEnumerable<Variable>>> GetReferenceVariablesAsync(int machineId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Looking up reference variables for MachineId={MachineId}", machineId);

        // Critical business rule: Only ReferenceTags group variables
        var specification = new Specification<Variable>(v =>
            v.MachineId == machineId &&
            v.IsActive == ActiveStatus.Active &&
            v.VariableGroupId == TagsGroups.ReferenceTags.Value);

        var variablesResult = await _variableRepository.ListAsync(specification, cancellationToken);

        if (variablesResult.IsFailure || variablesResult.Value is null)
        {
            _logger.LogWarning("Failed to get reference variables for MachineId={MachineId}", machineId);
            return Result<IEnumerable<Variable>>.WithFailure(["References not found"]);
        }

        var variables = variablesResult.Value;
        if (!variables.Any())
        {
            _logger.LogWarning("No reference variables found for MachineId={MachineId}", machineId);
            return Result<IEnumerable<Variable>>.WithFailure(["No reference variables found"]);
        }

        return Result<IEnumerable<Variable>>.Success(variables);
    }

    /// <summary>
    /// Converts variables to registers dictionary with proper mapping.
    /// Maps variable properties to register properties according to business rules.
    /// </summary>
    private Result<Dictionary<string, Register>> ConvertVariablesToRegisters(IEnumerable<Variable> variables, int cycleId)
    {
        try
        {
            _logger.LogDebug("Converting {Count} variables to registers for CycleId={CycleId}", variables.Count(), cycleId);

            // #39: build through the guarded Register.Create factory. RegisterId (surrogate identity) is mapped
            // from VariableId via the factory's registerId parameter (the #39 lockdown made RegisterId immutable,
            // so it can no longer be assigned after construction). `.Add` preserves the original ToDictionary
            // duplicate-key throw, which the surrounding try/catch converts to a failure Result.
            var registers = new Dictionary<string, Register>();
            foreach (var v in variables)
            {
                var registerResult = Register.Create(
                    name: v.Name ?? string.Empty,
                    description: string.Empty,
                    machineId: 0,
                    variableId: v.VariableId,
                    cycleId: cycleId,

                    // #43: ReferenceTags.Value is always empty in the DB and is overwritten downstream with the
                    // runtime routing value, so seeding string.Empty is byte-equivalent. (Variable.Value dropped.)
                    value: string.Empty,

                    // #43: source DataType from the real .NET type (NetType), not the role-tag-polluted NativeType.
                    dataType: v.NetType ?? string.Empty,
                    statusValueId: 1, // Default status value as per existing logic
                    timeStamp: default,
                    registerId: v.VariableId); // Map VariableId to RegisterId

                if (registerResult.IsFailure || registerResult.Value is null)
                {
                    return Result<Dictionary<string, Register>>.WithFailure(registerResult.Error);
                }

                var register = registerResult.Value;
                registers.Add(v.Name ?? string.Empty, register);
            }

            if (registers.Count == 0)
            {
                _logger.LogWarning("No registers created from variables for CycleId={CycleId}", cycleId);
                return Result<Dictionary<string, Register>>.WithFailure(["No references created"]);
            }

            return Result<Dictionary<string, Register>>.Success(registers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting variables to registers for CycleId={CycleId}", cycleId);
            return Result<Dictionary<string, Register>>.WithFailure([$"Variable conversion failed: {ex.Message}"]);
        }
    }
}