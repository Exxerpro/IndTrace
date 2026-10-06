// <copyright file="GetShiftDetailQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Queries.GetShiftDetail;

/// <summary>
/// Represents the GetShiftDetailQueryHandler.
/// </summary>
public class GetShiftDetailQueryHandler : IMonitorRequestHandler<GetShiftDetailQuery, ShiftDetailVm>
{
    // The repository signals "FirstOrDefault matched nothing" as a failure carrying this sentinel
    // (Repository.cs / ReadOnlyRepository.cs). It is the ONE failure that means "empty", not
    // "infrastructure error" — any other failure is a genuine repository error and is propagated.
    private const string RepositoryNotFoundSentinel = RepositoryFailures.NotFoundSentinel;

    private readonly IRepository<Shift> repository;
    private readonly ILogger<GetShiftDetailQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetShiftDetailQueryHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="logger">The logger.</param>
    public GetShiftDetailQueryHandler(IRepository<Shift> repository, ILogger<GetShiftDetailQueryHandler> logger)
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
    public async Task<Result<ShiftDetailVm>> ProcessAsync(GetShiftDetailQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<ShiftDetailVm>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ShiftDetailVm>.WithFailure("Operation was canceled.");
        }

        try
        {
            if (request.ShiftId <= 0)
            {
                this.logger.LogError("Invalid ShiftId: {ShiftId}", request.ShiftId);
                return Result<ShiftDetailVm>.WithFailure("ShiftId must be greater than 0");
            }

            var specification = new Specification<Shift>(p => p.ShiftId == new IndTrace.Domain.ValueObjects.ShiftId(request.ShiftId));
            var getResult = await this.repository.FirstOrDefaultAsync(specification, cancellationToken).ConfigureAwait(false);
            if (getResult.IsFailure && !IsNotFound(getResult.Errors))
            {
                this.logger.LogError("Failed to retrieve Shifts: {Errors}", string.Join(", ", getResult.Errors ?? []));
                return Result<ShiftDetailVm>.WithFailure(getResult.Errors);
            }

            var shift = getResult.Value;
            if (shift == null)
            {
                this.logger.LogError("Shift not found: {ShiftId}", request.ShiftId);
                return Result<ShiftDetailVm>.WithFailure($"Shift not found {request.ShiftId}");
            }

            return ShiftDetailVm.ToDto(shift);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in GetShiftDetailQueryHandler");
            return Result<ShiftDetailVm>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }

    /// <summary>
    /// Distinguishes the repository's "no rows matched" sentinel failure from a genuine infrastructure failure.
    /// </summary>
    /// <param name="errors">The errors carried by the failed repository result.</param>
    /// <returns>True when the failure only signals that no entity matched the specification.</returns>
    private static bool IsNotFound(IEnumerable<string>? errors) =>
        errors is not null && errors.Any(e => e is not null && e.Contains(RepositoryNotFoundSentinel, StringComparison.Ordinal));
}
