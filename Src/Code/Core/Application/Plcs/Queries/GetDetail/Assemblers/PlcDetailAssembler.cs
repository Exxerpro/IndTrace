// <copyright file="PlcDetailAssembler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Plcs.Queries.GetDetail.DataLoaders;

namespace IndTrace.Application.Plcs.Queries.GetDetail.Assemblers;

/// <summary>
/// Assembles PlcDto from loaded context with comprehensive business rules.
/// Implements CLAUDE.md patterns: Result-T, null safety, industrial logging.
/// Eliminates magic numbers by using VariableGroupIds constants.
/// </summary>
public class PlcDetailAssembler : IPlcDetailAssembler
{
    private readonly ILogger<PlcDetailAssembler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlcDetailAssembler"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public PlcDetailAssembler(ILogger<PlcDetailAssembler> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Assembles complete PLC detail view model.
    /// Preserves original business logic including variable deduplication and register transformation.
    /// </summary>
    /// <param name="context">Loaded PLC detail context.</param>
    /// <returns>Result containing assembled PLC DTO or failure reasons.</returns>
    public Result<PlcDto> AssembleDetail(PlcDetailContext context)
    {
        // 1. Parameter validation
        if (context is null)
        {
            this.logger.LogError("PlcDetailContext cannot be null");
            return Result<PlcDto>.WithFailure(["Context cannot be null."]);
        }

        if (context.Plc is null)
        {
            this.logger.LogError("PLC in context cannot be null");
            return Result<PlcDto>.WithFailure(["PLC cannot be null."]);
        }

        try
        {
            using var activity = this.logger.BeginScope("AssemblePlcDetail PlcId: {PlcId}", context.Plc.PlcId);
            var stopwatch = Stopwatch.StartNew();

            this.logger.LogInformation(
                "Starting assembly for PLC: {PlcId}, {MachinePlcCount} machine-PLCs, {VariableCount} variables",
                context.Plc.PlcId, context.MachinePlcs.Count, context.Variables.Count);

            // 2. Convert PLC to DTO using existing pattern
            var vmResult = PlcDto.ToDto(context.Plc);
            if (vmResult.IsFailure || vmResult.Value is null)
            {
                this.logger.LogWarning("Failed to convert PLC to DTO: {Errors}", 
                    string.Join(", ", vmResult.Errors ?? []));
                return Result<PlcDto>.WithFailure(vmResult.Errors);
            }

            var vm = vmResult.Value;

            // 3. Calculate minimum MachineId (preserving original logic)
            var machineIds = context.MachinePlcs
                .Select(mp => mp.MachineId.Value)
                .ToList();

            vm.MachineId = machineIds.Count > 0 ? machineIds.Min() : 0;

            // 4. Assign machines
            vm.Machines = context.Machines.ToList();

            // 5. Deduplicate variables by Name (preserving original GroupBy logic)
            // A GroupBy group is never empty, so First() yields the non-null representative directly — giving a
            // Dictionary<string, Variable> with non-null values and removing the need for a null-forgiving cast.
            var deduplicatedVariables = context.Variables
                .GroupBy(v => v.Name)
                .ToDictionary(
                    group => group.Key,
                    group => group.First());

            vm.Variables = deduplicatedVariables;

            // 6. Build variable groups dictionary
            // #128: dedup first-wins (matching the variable dedup above) so a duplicate VariableGroupName no
            // longer throws ArgumentException and fails the whole PLC-detail view.
            vm.VariablesGroups = context.VariableGroups
                .GroupBy(vg => vg.VariableGroupName)
                .ToDictionary(group => group.Key, group => group.First());

            // 7. Filter and transform variables to registers (VariableGroupId = 128)
            var registerVariables = deduplicatedVariables
                .Where(kvp => kvp.Value != null && kvp.Value.VariableGroupId == VariableGroupIds.Registers)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // #39: register maps are now built through the guarded Register.Create factory and surface a Result.
            var registersResult = TransformToRegisters(registerVariables);
            if (registersResult.IsFailure || registersResult.Value is null)
            {
                return Result<PlcDto>.WithFailure(registersResult.Error);
            }

            vm.Registers = registersResult.Value;

            // 8. Filter and transform variables to references (VariableGroupId = 256)
            var referenceVariables = deduplicatedVariables
                .Where(kvp => kvp.Value != null && kvp.Value.VariableGroupId == VariableGroupIds.References)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var referencesResult = TransformToRegisters(referenceVariables);
            if (referencesResult.IsFailure || referencesResult.Value is null)
            {
                return Result<PlcDto>.WithFailure(referencesResult.Error);
            }

            vm.References = referencesResult.Value;

            stopwatch.Stop();

            this.logger.LogInformation("Assembled PlcDto for PlcId {PlcId} in {ElapsedMs}ms", 
                context.Plc.PlcId, stopwatch.ElapsedMilliseconds);

            return Result<PlcDto>.Success(vm);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to assemble PlcDto for PlcId {PlcId}", context.Plc.PlcId);
            return Result<PlcDto>.WithFailure([$"Assembly failed: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Transforms variables dictionary to registers dictionary.
    /// Preserves original Register entity creation logic.
    /// </summary>
    /// <param name="variables">Dictionary of variables to transform.</param>
    /// <returns>A Result carrying a dictionary of registers keyed by variable name, or a failure.</returns>
    private static Result<Dictionary<string, Register>> TransformToRegisters(
        Dictionary<string, Variable> variables)
    {
        // #39: route each register through the guarded Register.Create factory. RegisterId (surrogate identity)
        // is assigned after construction (mapped from VariableId, as before). Null map values are skipped (the
        // callers pre-filter them out), which also removes the former null-forgiving access; for the filtered,
        // non-null input the produced map is byte-identical.
        var registers = new Dictionary<string, Register>();
        foreach (var kvp in variables)
        {
            var variable = kvp.Value;
            if (variable is null)
            {
                continue;
            }

            var registerResult = Register.Create(
                name: variable.Name,
                description: variable.Description,
                machineId: variable.MachineId,
                variableId: variable.VariableId,
                cycleId: 0,

                // #43: Variable.Value dropped (always empty in DB, overwritten downstream); string.Empty is byte-equivalent.
                value: string.Empty,
                dataType: variable.NetType,
                statusValueId: 0,
                timeStamp: default,
                registerId: variable.VariableId); // #39: RegisterId is immutable; carry it via the factory.

            if (registerResult.IsFailure || registerResult.Value is null)
            {
                return Result<Dictionary<string, Register>>.WithFailure(registerResult.Error);
            }

            var register = registerResult.Value;
            registers.Add(kvp.Key, register);
        }

        return Result<Dictionary<string, Register>>.Success(registers);
    }
}