// <copyright file="GetOeeHistoryPerformanceQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Oee.Queries;

using IndTrace.Application.Mediator;

/// <summary>
/// Handles the retrieval of OEE history data.
/// </summary>
public class GetOeeHistoryPerformanceQueryHandler : IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse>
{
    private readonly IOeeRepository oeeRepository;
    private readonly IValidator<GetOeeHistoryQuery> validator;
    private readonly ILogger<GetOeeHistoryPerformanceQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOeeHistoryPerformanceQueryHandler"/> class.
    /// Initializes a new instance of the GetOeeHistoryPerformanceQueryHandler.
    /// </summary>
    /// <param name="oeeRepository">Repository for OEE data access.</param>
    /// <param name="validator">Validator for the query.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    /// <summary>
    /// Initializes a new instance of the GetOeeHistoryPerformanceQueryHandler class.
    /// </summary>
    /// <param name="oeeRepository">The OEE repository.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="logger">The logger.</param>
    public GetOeeHistoryPerformanceQueryHandler(
        IOeeRepository oeeRepository,
        IValidator<GetOeeHistoryQuery> validator,
        ILogger<GetOeeHistoryPerformanceQueryHandler> logger)
    {
        this.oeeRepository = oeeRepository;
        this.validator = validator;
        this.logger = logger;
    }

    // TODO
    // ABR
    // REFACTOR TO RESULT OF T PATTERN THIS FORM IT DOESN HAVE TO ALLOWED BY DEFINITION
    // WE HAVE TO IMPLEMENT SOMETHING TO AVOID THIS PATTERNS

    /// <summary>
    /// Handles the OEE history query.
    /// </summary>
    /// <param name="query">The OEE history query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The OEE history response.</returns>
    public async Task<Result<GetOeeHistoryResponse>> HandleAsync(
        GetOeeHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            this.logger.LogInformation("Cancellation requested for OEE history query");
            return Result<GetOeeHistoryResponse>.WithFailure("Cancellation requested");
        }

        this.logger.LogInformation(
            "Processing OEE history query for Machine {MachineId} from {StartDate} to {EndDate}",
            query.MachineId, query.StartDate, query.EndDate);

        try
        {
            // Validate the query
            var validationResult = await this.validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                this.logger.LogError("Validation failed for OEE history query: {Errors}", string.Join(", ", errors));
                return Result<GetOeeHistoryResponse>.WithFailure(errors);
            }

            this.logger.LogInformation("OEE history query validation passed for Machine {MachineId}", query.MachineId);

            // Retrieve OEE history from repository
            var (records, totalCount) = await this.oeeRepository.GetOeeHistoryAsync(
                query.MachineId,
                query.StartDate,
                query.EndDate,
                query.MinPerformanceLevel,
                query.PageNumber,
                query.PageSize,
                cancellationToken);

            var response = new GetOeeHistoryResponse
            {
                Records = records,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
            };

            this.logger.LogInformation(
                "OEE history query completed: {RecordCount} records returned, {TotalCount} total records",
                records.Count(), totalCount);

            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while retrieving OEE history");
            return Result<GetOeeHistoryResponse>.WithFailure(ex.Message);
        }
    }
}