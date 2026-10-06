// <copyright file="GetShiftsListQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Queries.GetShiftList;

/// <summary>
/// Represents the GetShiftsListQueryHandler.
/// </summary>
public class GetShiftsListQueryHandler : IMonitorRequestHandler<GetShiftsListQuery, ShiftsListVm>
{
    private readonly IRepository<Shift> repository;
    private readonly ILogger<GetShiftsListQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetShiftsListQueryHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="logger">The logger.</param>
    public GetShiftsListQueryHandler(IRepository<Shift> repository, ILogger<GetShiftsListQueryHandler> logger)
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
    public async Task<Result<ShiftsListVm>> ProcessAsync(GetShiftsListQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<ShiftsListVm>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ShiftsListVm>.WithFailure("Operation was canceled.");
        }

        try
        {
            var getAllResult = await this.repository.ListAsync(cancellationToken).ConfigureAwait(false);
            if (!getAllResult.IsSuccess)
            {
                this.logger.LogError("Failed to retrieve Shifts: {Errors}", string.Join(", ", getAllResult.Errors ?? []));
                return Result<ShiftsListVm>.WithFailure(getAllResult.Errors);
            }

            var shifts = getAllResult.Value ?? [];
            var shiftRes = ShiftsDto.ToDtoList(shifts);
            if (shiftRes.IsFailure || shiftRes.Value is null)
            {
                return Result<ShiftsListVm>.WithFailure(shiftRes.Errors);
            }

            var shiftList = shiftRes.Value.ToList();

            var vm = new ShiftsListVm
            {
                Shifts = shiftList,
                Count = shiftList.Count,
            };

            return Result<ShiftsListVm>.Success(vm);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in GetShiftsListQueryHandler");
            return Result<ShiftsListVm>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
