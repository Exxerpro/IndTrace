// <copyright file="RegisterService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services;

/// <summary>
/// Service implementation for Register-related operations, replacing RegisterRepositoryExtensions.
/// Provides proper dependency injection and testability.
/// </summary>
public class RegisterService : IRegisterService
{
    private readonly IReadOnlyRepository<Register> registerRepository;
    private readonly IRepository<Variable> variableRepository;

    public RegisterService(
        IReadOnlyRepository<Register> registerRepository,
        IRepository<Variable> variableRepository)
    {
        this.registerRepository = registerRepository ?? throw new ArgumentNullException(nameof(registerRepository));
        this.variableRepository = variableRepository ?? throw new ArgumentNullException(nameof(variableRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<List<Register>>> GetRegistersGroupedByMachineAsync(
        List<int> cycleIdList,
        CancellationToken cancellationToken)
    {
        var activeVariablesSpec = new Specification<Variable>(v => v.IsActive == ActiveStatus.Active);
        var activeVariables = await this.variableRepository.ListAsync(activeVariablesSpec, cancellationToken).ConfigureAwait(false);

        if (activeVariables.IsFailure || activeVariables.Value is null || !activeVariables.Value.Any())
        {
            return Result<List<Register>>.WithFailure("No active variables found");
        }

        var activeVariableIds = activeVariables.Value.Select(v => v.VariableId).ToList();

        // Story 35.D2 C3: Register.CycleId is the converted CycleId struct — project the int filter list to CycleId
        // keys and compare whole-property so the predicate translates on real SQL (a .Value member-access would not).
        var cycleIdKeys = cycleIdList.Select(id => new CycleId(id)).ToList();
        var spec = new Specification<Register>(p => cycleIdKeys.Contains(p.CycleId) && activeVariableIds.Contains(p.VariableId));
        var registers = await this.registerRepository.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        if (registers.IsFailure || registers.Value is null || !registers.Value.Any())
        {
            return Result<List<Register>>.WithFailure("No registers found for the specified Cycle IDs");
        }

        return Result<List<Register>>.Success(registers.Value.ToList());
    }

    /// <inheritdoc/>
    public async Task<Result<List<RegisterView>>> GetRegistersWithVariablesAsync(
        List<int> cycleIdList,
        CancellationToken cancellationToken)
    {
        var activeVariablesSpec = new Specification<Variable>(v => v.IsActive == ActiveStatus.Active);
        var activeVariablesResult = await this.variableRepository.ListAsync(activeVariablesSpec, cancellationToken).ConfigureAwait(false);

        if (activeVariablesResult.IsFailure || activeVariablesResult.Value is null || !activeVariablesResult.Value.Any())
        {
            return Result<List<RegisterView>>.WithFailure("No active variables found");
        }

        var cycleIdKeys = cycleIdList.Select(id => new CycleId(id)).ToList();
        var spec = new Specification<Register>(r => cycleIdKeys.Contains(r.CycleId));
        var registersResult = await this.registerRepository.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        if (registersResult.IsFailure || registersResult.Value is null || !registersResult.Value.Any())
        {
            return Result<List<RegisterView>>.WithFailure("No registers found for the specified Cycle IDs");
        }

        var registers = registersResult.Value.ToList();
        var activeVariables = activeVariablesResult.Value.ToList();

        var query = from register in registers
                    join variable in activeVariables on register.VariableId equals variable.VariableId into variableJoin
                    from subVariable in variableJoin.DefaultIfEmpty()
                    where subVariable != null && subVariable.IsActive.Value > 0
                    select new { register, subVariable };

        var dict = new Dictionary<KeyRegister, RegisterView>();

        foreach (var g in query)
        {
            var newKey = new KeyRegister()
            {
                CycleId = g.register.CycleId.Value,
                VariableId = g.register.VariableId,
            };

            dict.TryAdd(newKey, new RegisterView
            {
                RegisterId = g.register.RegisterId,
                Name = g.register.Name,
                MachineId = g.subVariable?.MachineId ?? 0,
                VariableId = g.register.VariableId,
                CycleId = g.register.CycleId.Value,
                Value = !string.IsNullOrEmpty(g.register.Value) ? g.register.Value : string.Empty,
                DataType = g.register.DataType,
                StatusValueId = g.register.StatusValueId,
                Description = g.subVariable?.Description ?? string.Empty,
                TimeStamp = g.register.TimeStamp,
            });
        }

        var resultQuery = dict.Values.ToList();
        return Result<List<RegisterView>>.Success(resultQuery);
    }

    /// <inheritdoc/>
    public async Task<Result<List<RegisterView>>> GetRegisterByCycleIdListAsync(
        List<int> cycleIdList,
        CancellationToken cancellationToken)
    {
        // Step 1: Get all active variables
        var activeVariablesSpec = new Specification<Variable>(v => v.IsActive == ActiveStatus.Active);
        var activeVariablesResult = await this.variableRepository.ListAsync(activeVariablesSpec, cancellationToken).ConfigureAwait(false);

        if (activeVariablesResult.IsFailure || activeVariablesResult.Value is null || !activeVariablesResult.Value.Any())
        {
            return Result<List<RegisterView>>.WithFailure("No active variables found.");
        }

        var activeVariables = activeVariablesResult.Value;

        // Step 2: Get all registers related to provided cycle IDs
        var cycleIdKeys = cycleIdList.Select(id => new CycleId(id)).ToList();
        var registersSpec = new Specification<Register>(r => cycleIdKeys.Contains(r.CycleId));
        var registersResult = await this.registerRepository.ListAsync(registersSpec, cancellationToken).ConfigureAwait(false);

        if (registersResult.IsFailure || registersResult.Value is null || !registersResult.Value.Any())
        {
            return Result<List<RegisterView>>.WithFailure("No registers found for the specified Cycle IDs.");
        }

        var registers = registersResult.Value.ToList();

        // Step 3: Create lookup for active variables for faster join
        var activeVariableLookup = activeVariables.ToDictionary(v => v.VariableId);

        // Step 4: Join registers with variables
        var registerViews = new Dictionary<(int MachineId, int VariableId), RegisterView>();

        foreach (var register in registers)
        {
            if (!activeVariableLookup.TryGetValue(register.VariableId, out var variable) || variable.IsActive.Value != ActiveStatus.Active.Value)
            {
                continue; // Skip if variable is not active
            }

            var key = (MachineId: register.MachineId, VariableId: register.VariableId);

            if (!registerViews.ContainsKey(key))
            {
                registerViews[key] = new RegisterView
                {
                    RegisterId = register.RegisterId,
                    Name = register.Name,
                    MachineId = register.MachineId, // <-- Grouping by real machine id
                    VariableId = register.VariableId,
                    CycleId = register.CycleId.Value, // Original cycle id, but grouping is by MachineId
                    Value = register.Value ?? string.Empty,
                    DataType = register.DataType,
                    StatusValueId = register.StatusValueId,
                    Description = variable.Description ?? string.Empty,
                    TimeStamp = register.TimeStamp,
                };
            }
        }

        var resultList = registerViews.Values.ToList();

        return Result<List<RegisterView>>.Success(resultList);
    }
}