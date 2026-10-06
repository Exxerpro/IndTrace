// <copyright file="GetVariableListQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Queries.GetVariableList;

using IndTrace.Application.WorkFlows.Queries.GetList;

/// <summary>
/// Represents the GetVariableListQueryHandler.
/// </summary>
public class GetVariableListQueryHandler : IMonitorRequestHandler<GetVariableListQuery, VariableListVm>
{
    private readonly IRepository<Variable> repository;
    private readonly ILogger<GetVariableListQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetVariableListQueryHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="logger">The logger.</param>
    public GetVariableListQueryHandler(IRepository<Variable> repository, ILogger<GetVariableListQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    /// <summary>
    /// Executes ProcessAsync operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ProcessAsync.</returns>
    public async Task<Result<VariableListVm>> ProcessAsync(GetVariableListQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<VariableListVm>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<VariableListVm>.WithFailure("Operation was canceled.");
        }

        try
        {
            var getAllResult = await this.repository.ListAsync(cancellationToken).ConfigureAwait(false);
            if (!getAllResult.IsSuccess)
            {
                this.logger.LogError("Failed to retrieve Variables: {Errors}", string.Join(", ", getAllResult.Errors ?? []));
                return Result<VariableListVm>.WithFailure(getAllResult.Errors);
            }

            var variables = getAllResult.Value ?? [];
            var varResult = VariableDto.ToDtoList(variables);
            if (varResult.IsFailure || varResult.Value is null)
            {
                return Result<VariableListVm>.WithFailure(varResult.Errors);
            }

            var variableDtos = varResult.Value.ToList();

            var vm = new VariableListVm
            {
                VariableList = variableDtos,
                Count = variableDtos.Count,
            };

            return Result<VariableListVm>.Success(vm);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in GetVariableListQueryHandler");
            return Result<VariableListVm>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
