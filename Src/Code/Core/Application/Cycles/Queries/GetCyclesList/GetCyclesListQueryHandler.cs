// <copyright file="GetCyclesListQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Queries.GetCyclesList;

/// <summary>
/// Handles queries for retrieving lists of production cycles with optional filtering.
/// </summary>
/// <remarks>
/// This handler provides access to cycle data for monitoring and reporting purposes,
/// supporting both filtered views by barcode ID and general cycle listings with pagination.
/// It's essential for production tracking, performance analysis, and cycle audit trails.
/// </remarks>
public class GetCyclesListQueryHandler : IMonitorRequestHandler<GetCyclesListQuery, CyclesListVm>
{
    private readonly IReadOnlyRepository<Cycle> repository;
    private readonly ILogger<GetCyclesListQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCyclesListQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">Repository for accessing cycle data.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public GetCyclesListQueryHandler(IReadOnlyRepository<Cycle> repository, ILogger<GetCyclesListQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the cycles list query and returns filtered cycle data.
    /// </summary>
    /// <param name="request">The query containing optional filtering criteria.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the cycles list view model with cycle data and count information.</returns>
    /// <remarks>
    /// This method applies the following filtering logic:
    /// - If a specific barcode ID is provided, returns all cycles for that barcode
    /// - If no ID is provided, returns the most recent 250 cycles ordered by cycle ID
    /// This ensures both targeted analysis and general production monitoring capabilities.
    /// </remarks>
    public async Task<Result<CyclesListVm>> ProcessAsync(GetCyclesListQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<CyclesListVm>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<CyclesListVm>.WithFailure("Operation was canceled.");
        }

        const int MaxPageSize = 250;
        try
        {
            ISpecification<Cycle> spec;
            int effectivePageSize;
            int skip = 0;

            // #128 (Chunk B): the same filter predicate roots BOTH the paged page-payload spec and the
            // unpaged count spec, so Count reports the TOTAL matching rows instead of the page size.
            Expression<Func<Cycle, bool>> filter;

            if (request.Id > 0)
            {
                // Filter by barcode, order by CycleId descending, limit to MaxPageSize
                effectivePageSize = Math.Min(request.PageSize > 0 ? request.PageSize : MaxPageSize, MaxPageSize);
                var barCodeIdKey = new BarCodeId(request.Id);
                filter = c => c.BarCodeId == barCodeIdKey;
                spec = new Specification<Cycle>(filter)
                    .AddOrderByDescending(c => c.CycleId)
                    .ApplyPaging(0, effectivePageSize);
            }
            else
            {
                // Use pagination, clamp page size
                effectivePageSize = Math.Min(request.PageSize > 0 ? request.PageSize : 50, MaxPageSize);
                int page = request.Page > 0 ? request.Page : 1;
                skip = (page - 1) * effectivePageSize;
                filter = c => true;
                spec = new Specification<Cycle>(filter)
                    .AddOrderByDescending(c => c.CycleId)
                    .ApplyPaging(skip, effectivePageSize);
            }

            var getResult = await this.repository.ListAsync(spec, cancellationToken).ConfigureAwait(false);
            if (getResult.IsFailure)
            {
                this.logger.LogError("Failed to retrieve Cycles: {Errors}", string.Join(", ", getResult.Errors ?? []));
                return Result<CyclesListVm>.WithFailure(getResult.Errors);
            }

            // #128 (Chunk B): total matching rows for the SAME filter, WITHOUT paging. Fail CLOSED per the #124
            // CountAsync convention — an unverifiable count is a Result failure, never silently reported as the
            // page size.
            var countSpec = new Specification<Cycle>(filter);
            var countResult = await this.repository.CountAsync(countSpec, cancellationToken).ConfigureAwait(false);
            if (countResult.IsFailure)
            {
                this.logger.LogError("Failed to count Cycles: {Errors}", string.Join(", ", countResult.Errors ?? []));
                return Result<CyclesListVm>.WithFailure(countResult.Errors);
            }

            var cycles = getResult.Value?.ToList() ?? [];
            var cycleResult = CyclesDto.ToDtoList(cycles);
            if (cycleResult.IsFailure || cycleResult.Value is null)
            {
                return Result<CyclesListVm>.WithFailure(cycleResult.Errors);
            }

            var cycleView = cycleResult.Value.ToList();
            var vm = new CyclesListVm
            {
                Cycles = cycleView,
                Count = countResult.Value,
            };

            return Result<CyclesListVm>.Success(vm);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in GetCyclesListQueryHandler");
            return Result<CyclesListVm>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}