// <copyright file="GetVariableDetailQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Queries.GetVariableDetail;

/// <summary>
/// Handles the retrieval of detailed variable information.
/// Provides comprehensive data about a specific variable entity.
/// </summary>
public class GetVariableDetailQueryHandler : IMonitorRequestHandler<GetVariableDetailQuery, VariableDetailVm>
{
    private readonly IRepository<Variable> repository;
    private readonly ILogger<GetVariableDetailQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetVariableDetailQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">Repository for accessing variable data.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public GetVariableDetailQueryHandler(IRepository<Variable> repository, ILogger<GetVariableDetailQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the variable detail query and returns comprehensive variable information.
    /// </summary>
    /// <param name="request">The query containing the variable ID to retrieve details for.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the detailed variable view model.</returns>
    public async Task<Result<VariableDetailVm>> ProcessAsync(GetVariableDetailQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<VariableDetailVm>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<VariableDetailVm>.WithFailure("Operation was canceled.");
        }

        try
        {
            var getResult = await this.repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                this.logger.LogError("Variable not found: {EntitieId}", request.Id);
                return Result<VariableDetailVm>.WithFailure($"Variable with ID {request.Id} not found");
            }

            var dtoResult = VariableDetailVm.ToDto(getResult.Value);
            if (!dtoResult.IsSuccess)
            {
                this.logger.LogError("Failed to convert variable to DTO: {EntitieId}", request.Id);
                return Result<VariableDetailVm>.WithFailure(dtoResult.Error ?? "Failed to convert variable to DTO");
            }

            if (dtoResult.Value is null)
            {
                this.logger.LogError("DTO conversion returned null value for variable: {EntitieId}", request.Id);
                return Result<VariableDetailVm>.WithFailure("DTO conversion returned null value");
            }

            return Result<VariableDetailVm>.Success(dtoResult.Value);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in GetVariableDetailQueryHandler");
            return Result<VariableDetailVm>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}