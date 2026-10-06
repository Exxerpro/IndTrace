// <copyright file="GetRegistersListQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Registers.Queries.GetRegisterList;

/// <summary>
/// Represents the GetRegistersListQueryHandler.
/// </summary>
public class GetRegistersListQueryHandler(
    IRepository<Variable> variableRepository,
    IReadOnlyRepository<Register> registerRepository)
    : IMonitorRequestHandler<GetRegistersListQuery, IEnumerable<RegisterDto>>
{
    /// <inheritdoc/>
    public async Task<Result<IEnumerable<RegisterDto>>> ProcessAsync(GetRegistersListQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<IEnumerable<RegisterDto>>.WithFailure(["Request cannot be null."]);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<RegisterDto>>.WithFailure(["Operation was canceled."]);
        }

        try
        {
            var variablesIdsResult = await this.GetVariablesIdFromRequest(request, cancellationToken).ConfigureAwait(false);

            // #119 (F1): repository faults propagate as failures — pre-fix they were coerced to an
            // empty id set, which the Monitor Metrics page rendered as "no data" on a DB outage.
            if (variablesIdsResult.IsFailure || variablesIdsResult.Value is null)
            {
                return Result<IEnumerable<RegisterDto>>.WithFailure(variablesIdsResult.Errors ?? ["Failed to resolve variable ids for the request."]);
            }

            var variablesIds = variablesIdsResult.Value;

            if (!variablesIds.Any())
            {
                return Result<IEnumerable<RegisterDto>>.WithFailure(["No valid variable IDs or names provided."]);
            }

            var result = await this.GetRegisterOnRequestSpec(request, cancellationToken, variablesIds).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RegisterDto>>.WithFailure([ex.Message]);
        }
    }

    private async Task<Result<IEnumerable<int>>> GetVariablesIdFromRequest(GetRegistersListQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<int> variablesIds = [];

        if (request.VariablesId is not null && request.VariablesId.Any())
        {
            // Case where variable IDs are provided, no need to fetch them
            variablesIds = request.VariablesId;
        }

        if (request.RegistersName is null || !request.RegistersName.Any())
        {
            return Result<IEnumerable<int>>.Success(variablesIds);
        }

        // Specification to filter registers by name and machine ID (IX_Registers_Name_MachineId).
        var registerSpec = new Specification<Register>(
            r => request.RegistersName.Contains(r.Name)
                 && request.MachineId.Contains(r.MachineId))
            .ApplyNoTracking();

        // #119 (F1): the Registers table is an append-only ledger that grows forever — resolving ids
        // must be a server-side projection (SELECT DISTINCT VariableID), never a full-entity
        // materialization of every matching ledger row.
        var registerIdsLease = await registerRepository.AsQueryableAsync(registerSpec, cancellationToken).ConfigureAwait(false);

        if (registerIdsLease.IsFailure || registerIdsLease.Value is null)
        {
            return Result<IEnumerable<int>>.WithFailure(registerIdsLease.Errors ?? ["Failed to lease a Registers queryable."]);
        }

        // #117 (F1): the lease owns the pooled context backing the queryable; hold it until the
        // projection materializes, then return the context to the pool.
        List<int> registerIds;
        await using (var lease = registerIdsLease.Value)
        {
            registerIds = lease.Query
                .Select(r => r.VariableId)
                .Distinct()
                .ToList();
        }

        // Specification to filter variables by name and machine ID
        var variablesSpec = new Specification<Variable>(
            v => request.RegistersName.Contains(v.Name)
                 && request.MachineId.Contains(v.MachineId));

        // Fetch variable IDs
        var variables = await variableRepository.ListAsync(variablesSpec, cancellationToken).ConfigureAwait(false);

        if (variables.IsFailure || variables.Value is null)
        {
            return Result<IEnumerable<int>>.WithFailure(variables.Errors ?? ["Failed to query variables for the request."]);
        }

        var variableIds = variables.Value.Select(v => v.VariableId).ToList();

        // Union of register IDs and variable IDs, eliminating duplicates
        var combinedIds = registerIds.Union(variableIds).Union(variablesIds).Distinct();

        return Result<IEnumerable<int>>.Success(combinedIds);
    }

    private async Task<Result<IEnumerable<RegisterDto>>> GetRegisterOnRequestSpec(GetRegistersListQuery request, CancellationToken cancellationToken,
        IEnumerable<int> variablesIds)
    {
        var spec = new Specification<Register>(r =>
            variablesIds.Contains(r.VariableId) &&
            request.MachineId.Contains(r.MachineId) &&
            r.TimeStamp >= request.StartDate &&
            r.TimeStamp <= request.EndDate);

        // Retrieve the registers with the matched EntitieId, MachineId list, and CreatedOn range
        var registers = await registerRepository.ListAsync(
            spec,
            cancellationToken).ConfigureAwait(false);

        // #119 (F1): surface the repository's own errors instead of masking a DB fault as "not found".
        if (registers.IsFailure)
        {
            return Result<IEnumerable<RegisterDto>>.WithFailure(registers.Errors ?? ["No registers found for the specified criteria."]);
        }

        // Map the result to RegisterDto
        var registerDtos = RegisterDto.ToDtoList(registers.Value ?? []);
        if (registerDtos.IsFailure || registerDtos.Value is null)
        {
            return Result<IEnumerable<RegisterDto>>.WithFailure(registerDtos.Errors);
        }

        return Result<IEnumerable<RegisterDto>>.Success(registerDtos.Value);
    }
}
