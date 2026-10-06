// <copyright file="GetConfigAppsDetailQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Queries.GetConfigAppsDetail;

/// <summary>
/// Represents the GetConfigAppsDetailQueryHandler.
/// </summary>
public class GetConfigAppsDetailQueryHandler : IMonitorRequestHandler<GetConfigAppsDetailQuery, ConfigAppDto>
{
    private readonly IRepository<Domain.Entities.ConfigApp> repository;
    private readonly ILogger<GetConfigAppsDetailQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConfigAppsDetailQueryHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="logger">The logger.</param>
    public GetConfigAppsDetailQueryHandler(IRepository<Domain.Entities.ConfigApp> repository, ILogger<GetConfigAppsDetailQueryHandler> logger)
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
    public async Task<Result<ConfigAppDto>> ProcessAsync(GetConfigAppsDetailQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<ConfigAppDto>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ConfigAppDto>.WithFailure("Operation was canceled.");
        }

        try
        {
            var result = await this.repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (result is null)
            {
                this.logger.LogError("Repository returned null Result for ConfigApp RegisterId={RegisterId}", request.Id);
                return Result<ConfigAppDto>.WithFailure($"ConfigApp {request.Id} not found");
            }

            if (!result.IsSuccess || result.Value == null)
            {
                this.logger.LogError("Failed to get ConfigApp details: {Errors}", string.Join(", ", result.Errors ?? []));
                return Result<ConfigAppDto>.WithFailure(result.Errors);
            }

            var dto = ConfigAppDto.ToDto(result.Value);
            return dto.IsSuccess && dto.Value is not null
                ? Result<ConfigAppDto>.Success(dto.Value)
                : Result<ConfigAppDto>.WithFailure(dto.Errors);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in GetConfigAppsDetailQueryHandler");
            return Result<ConfigAppDto>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}