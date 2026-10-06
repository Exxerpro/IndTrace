// <copyright file="GetReportsFilterInfoMonitorQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.Builders;

namespace IndTrace.Application.BarCodes.Queries.GetReportsList.FiltersInfo;

/// <summary>
/// Handles retrieval of filter information for reports list functionality.
/// Refactored to use SRP-compliant services for industrial safety compliance.
/// </summary>
public class GetReportsFilterInfoMonitorQueryHandler : IMonitorQueryHandler<GetReportsFilterInfoQuery, ReportsFilterInfoVm>
{
    private readonly IReportsFilterInfoBuilder filterInfoBuilder;
    private readonly ILogger<GetReportsFilterInfoMonitorQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReportsFilterInfoMonitorQueryHandler"/> class.
    /// Refactored to use SRP-compliant filter info builder service.
    /// </summary>
    /// <param name="filterInfoBuilder">Service for building comprehensive filter information.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public GetReportsFilterInfoMonitorQueryHandler(
        IReportsFilterInfoBuilder filterInfoBuilder,
        ILogger<GetReportsFilterInfoMonitorQueryHandler> logger)
    {
        //[Fix] 
        //CLAUDE
        //Date: 26/09/2025 
        //Reason: [SRP REFACTOR] - Updated constructor to use extracted IReportsFilterInfoBuilder service
        
        this.filterInfoBuilder = filterInfoBuilder ?? throw new ArgumentNullException(nameof(filterInfoBuilder));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the reports filter info query using the SRP-compliant builder service.
    /// Refactored to delegate responsibility to IReportsFilterInfoBuilder for improved maintainability.
    /// </summary>
    /// <param name="request">The filter info query request.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing filter information or failure reasons.</returns>
    public async Task<Result<ReportsFilterInfoVm>> ProcessAsync(GetReportsFilterInfoQuery request, CancellationToken cancellationToken)
    {
        //[Fix] 
        //CLAUDE
        //Date: 26/09/2025 
        //Reason: [SRP REFACTOR] - Refactored ProcessAsync to use IReportsFilterInfoBuilder service, reducing from 103 to ~80 lines

        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ReportsFilterInfoVm>.WithFailure(["Operation was canceled."]);
        }

        // Null guards for dependencies and parameters
        if (this.filterInfoBuilder is null)
        {
            return Result<ReportsFilterInfoVm>.WithFailure(["filterInfoBuilder cannot be null."]);
        }

        if (request is null)
        {
            return Result<ReportsFilterInfoVm>.WithFailure(["Request cannot be null."]);
        }

        this.logger.LogInformation("Processing reports filter info query");

        try
        {
            // Delegate filter information building to the specialized service
            var filterInfoResult = await this.filterInfoBuilder.BuildAsync(cancellationToken).ConfigureAwait(false);

            if (filterInfoResult.IsFailure)
            {
                this.logger.LogError("Failed to build filter information: {Errors}", string.Join(", ", filterInfoResult.Errors ?? []));
                return Result<ReportsFilterInfoVm>.WithFailure(filterInfoResult.Errors);
            }

            if (filterInfoResult.Value is null)
            {
                this.logger.LogError("Filter info result cannot be null");
                return Result<ReportsFilterInfoVm>.WithFailure(["Filter info result cannot be null"]);
            }

            this.logger.LogInformation("Successfully processed reports filter info query");
            return Result<ReportsFilterInfoVm>.Success(filterInfoResult.Value);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error processing reports filter info query");
            return Result<ReportsFilterInfoVm>.WithFailure([$"Unexpected error processing filter info: {ex.Message}"]);
        }
    }
}