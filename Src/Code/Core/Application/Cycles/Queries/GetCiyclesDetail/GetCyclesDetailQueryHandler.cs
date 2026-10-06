// <copyright file="GetCyclesDetailQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Queries.GetCiyclesDetail;

/// <summary>
/// Represents the GetCyclesDetailQueryHandler.
/// </summary>
public class GetCyclesDetailQueryHandler : IMonitorRequestHandler<GetCyclesDetailQuery, CyclesDetailVm>
{
    private readonly IReadOnlyRepository<Cycle> repository;
    private readonly ILogger<GetCyclesDetailQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCyclesDetailQueryHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="logger">The logger.</param>
    public GetCyclesDetailQueryHandler(IReadOnlyRepository<Cycle> repository, ILogger<GetCyclesDetailQueryHandler> logger)
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
    public async Task<Result<CyclesDetailVm>> ProcessAsync(GetCyclesDetailQuery request, CancellationToken cancellationToken)
    {
        var getResult = await this.repository.GetByIdAsync(request.Id, cancellationToken);
        if (!getResult.IsSuccess || getResult.Value == null)
        {
            this.logger.LogError("Cycle not found: {UserId}", request.Id);
            return Result<CyclesDetailVm>.WithFailure($"Cycle Not Found {request.Id}");
        }

        var dto = CyclesDetailVm.ToDto(getResult.Value);
        if (!dto.IsSuccess || dto.Value is null)
        {
            return Result<CyclesDetailVm>.WithFailure(dto.Errors);
        }

        return Result<CyclesDetailVm>.Success(dto.Value);
    }
}