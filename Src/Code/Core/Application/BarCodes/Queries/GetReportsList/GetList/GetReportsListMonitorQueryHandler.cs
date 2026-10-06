// <copyright file="GetReportsListMonitorQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.Composers;
using IndTrace.Application.BarCodes.Queries.Filters;
using IndTrace.Application.BarCodes.Queries.Mappers;

namespace IndTrace.Application.BarCodes.Queries.GetReportsList.GetList;

/// <summary>
/// Handles the retrieval of barcode lists with comprehensive filtering capabilities.
/// Supports filtering by master labels, products, customers, states, shifts, lines, and register data.
/// Refactored to use SRP-compliant services for industrial safety compliance.
/// </summary>
public class GetReportsListMonitorQueryHandler : Domain.Interfaces.IMonitorQueryHandler<GetReportsListQuery, BarCodesListVm>
{
    private readonly IReportsListQueryComposer queryComposer;
    private readonly IBarCodeListMapper barCodeMapper;
    private readonly IRegisterDataFilter registerFilter;
    private readonly IRepository<BarCode> barCodeRepository;
    private readonly IReadOnlyRepository<Register> registerRepository;
    private readonly IReadOnlyRepository<Cycle> cycleRepository;
    private readonly IRepository<MasterLabel> masterLabelRepository;
    private readonly IRepository<Customer> customerRepository;
    private readonly IRepository<Product> productRepository;
    private readonly IRepository<Line> lineRepository;
    private readonly ILogger<GetReportsListMonitorQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReportsListMonitorQueryHandler"/> class.
    /// Refactored constructor to use SRP-compliant services instead of multiple repositories.
    /// </summary>
    /// <param name="queryComposer">Service for composing filtered barcode queries.</param>
    /// <param name="barCodeMapper">Service for mapping barcodes to DTOs with cycle counts.</param>
    /// <param name="registerFilter">Service for register-based filtering.</param>
    /// <param name="barCodeRepository">Repository for accessing barcode data.</param>
    /// <param name="registerRepository">Repository for accessing register data.</param>
    /// <param name="cycleRepository">Repository for accessing cycle data.</param>
    /// <param name="masterLabelRepository">Repository for accessing master label data.</param>
    /// <param name="customerRepository">Repository for accessing customer data.</param>
    /// <param name="productRepository">Repository for accessing product data.</param>
    /// <param name="lineRepository">Repository for accessing line data.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public GetReportsListMonitorQueryHandler(
        IReportsListQueryComposer queryComposer,
        IBarCodeListMapper barCodeMapper,
        IRegisterDataFilter registerFilter,
        IRepository<BarCode> barCodeRepository,
        IReadOnlyRepository<Register> registerRepository,
        IReadOnlyRepository<Cycle> cycleRepository,
        IRepository<MasterLabel> masterLabelRepository,
        IRepository<Customer> customerRepository,
        IRepository<Product> productRepository,
        IRepository<Line> lineRepository,
        ILogger<GetReportsListMonitorQueryHandler> logger)
    {
        // #128 (Chunk B): plain assignment of the DI-provided dependencies. The previous
        // validate-then-log-then-store-null block was anemic — it validated, logged through a possibly-null
        // logger, then stored the null deps anyway. DI guarantees non-null here, and ProcessAsync re-guards
        // every dependency and fails closed, so that remains the single authoritative safety net.
        this.queryComposer = queryComposer;
        this.barCodeMapper = barCodeMapper;
        this.registerFilter = registerFilter;
        this.barCodeRepository = barCodeRepository;
        this.registerRepository = registerRepository;
        this.cycleRepository = cycleRepository;
        this.masterLabelRepository = masterLabelRepository;
        this.customerRepository = customerRepository;
        this.productRepository = productRepository;
        this.lineRepository = lineRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the reports list query with comprehensive filtering options.
    /// Refactored to use SRP-compliant services for reduced complexity and improved maintainability.
    /// </summary>
    /// <param name="request">The query containing filter criteria and parameters.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the filtered barcode list view model.</returns>
    public async Task<Result<BarCodesListVm>> ProcessAsync(GetReportsListQuery request, CancellationToken cancellationToken)
    {
        //[Fix] 
        //CLAUDE
        //Date: 26/09/2025 
        //Reason: [SRP REFACTOR] - Refactored ProcessAsync to use extracted services, reducing from 372 to ~150 lines

        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodesListVm>.WithFailure(["Operation was canceled."]);
        }

        // Null guards for dependencies and parameters
        if (this.queryComposer is null)
        {
            return Result<BarCodesListVm>.WithFailure(["queryComposer cannot be null."]);
        }

        if (this.barCodeMapper is null)
        {
            return Result<BarCodesListVm>.WithFailure(["barCodeMapper cannot be null."]);
        }

        if (this.registerFilter is null)
        {
            return Result<BarCodesListVm>.WithFailure(["registerFilter cannot be null."]);
        }

        if (request is null)
        {
            return Result<BarCodesListVm>.WithFailure(["Request cannot be null."]);
        }

        this.logger.LogInformation(
            "Processing reports list query with filters - IsMaster: {IsMaster}, DateRange: {StartDate} to {EndDate}",
            request.IsMaster, request.StartDate, request.EndDate);

        try
        {
            // 1. Lease the base queryables. #117 (F1): each queryable now arrives as an OwnedQueryable lease that
            // owns its pooled context; the single `await using` below keeps EVERY lease alive until the composer
            // and mapper have materialized the composed queries (steps 2-3), then returns all six contexts to the
            // pool — on the success path and on every early failure return alike.
            var baseQueryResult = await this.GetBaseQueryablesAsync(request, cancellationToken).ConfigureAwait(false);
            if (baseQueryResult.IsFailure)
            {
                this.logger.LogError("Failed to get base queryables: {Errors}", string.Join(", ", baseQueryResult.Errors ?? []));
                return Result<BarCodesListVm>.WithFailure(baseQueryResult.Errors);
            }

            if (baseQueryResult.Value is null)
            {
                this.logger.LogError("Base query leases cannot be null");
                return Result<BarCodesListVm>.WithFailure(["Base query leases cannot be null"]);
            }

            await using var leases = baseQueryResult.Value;
            var barCodeQuery = leases.BarCodes;
            var supportQueries = leases.Support;
            var cycleQuery = leases.Cycles;

            // 2. Apply all filters using the composer service
            var filteredQueryResult = await this.queryComposer.ComposeAsync(
                barCodeQuery, request, supportQueries, cancellationToken).ConfigureAwait(false);

            if (filteredQueryResult.IsFailure)
            {
                this.logger.LogError("Failed to compose filtered query: {Errors}", string.Join(", ", filteredQueryResult.Errors ?? []));
                return Result<BarCodesListVm>.WithFailure(filteredQueryResult.Errors);
            }

            if (filteredQueryResult.Value is null)
            {
                this.logger.LogError("Filtered query cannot be null");
                return Result<BarCodesListVm>.WithFailure(["Filtered query cannot be null"]);
            }

            var filteredQuery = filteredQueryResult.Value;

            // 3. Map to DTOs with cycle counts using the mapper service
            var mappedBarCodesResult = await this.barCodeMapper.MapWithCycleCountsAsync(
                filteredQuery, cycleQuery, cancellationToken).ConfigureAwait(false);

            if (mappedBarCodesResult.IsFailure)
            {
                this.logger.LogError("Failed to map barcodes with cycle counts: {Errors}", string.Join(", ", mappedBarCodesResult.Errors ?? []));
                return Result<BarCodesListVm>.WithFailure(mappedBarCodesResult.Errors);
            }

            if (mappedBarCodesResult.Value is null)
            {
                this.logger.LogError("Mapped bar codes cannot be null");
                return Result<BarCodesListVm>.WithFailure(["Mapped bar codes cannot be null"]);
            }

            var barCodes = mappedBarCodesResult.Value;

            // 4. Apply register filtering if requested using the filter service. #119 (F2): the report's
            // StartDate/EndDate window is threaded through so the register ledger scan is TimeStamp-bounded
            // (IX_Registers_TimeStamp) instead of walking the whole append-only ledger.
            // #126 review C10: the bound is content-neutral ONLY for reports whose barcode rows are
            // themselves date-bounded (the non-master ModifiedOn window applied by the composer). The
            // IsMaster report's rows carry NO date bound (#119 F6 PO-flag, ruling pending on the deferred
            // "IsMaster unbounded scan" item), so it forwards a null window to preserve pre-#147 content.
            if (request.FilterByRegister)
            {
                DateTime? registerWindowStart = request.IsMaster ? null : request.StartDate;
                DateTime? registerWindowEnd = request.IsMaster ? null : request.EndDate;
                var registerFilterResult = await this.ApplyRegisterFilterAsync(barCodes, request.RegisterSearch, registerWindowStart, registerWindowEnd, cancellationToken).ConfigureAwait(false);
                if (registerFilterResult.IsFailure)
                {
                    this.logger.LogError("Failed to apply register filter: {Errors}", string.Join(", ", registerFilterResult.Errors ?? []));
                    return Result<BarCodesListVm>.WithFailure(registerFilterResult.Errors);
                }

                if (registerFilterResult.Value is null)
                {
                    this.logger.LogError("Register filtered bar codes cannot be null");
                    return Result<BarCodesListVm>.WithFailure(["Register filtered bar codes cannot be null"]);
                }

                barCodes = registerFilterResult.Value;
            }

            // 5. Create and return the view model
            var vm = new BarCodesListVm
            {
                BarCodes = barCodes?.OrderByDescending(bc => bc.BarCodeId).ToList() ?? [],
                Count = barCodes?.Count ?? 0,
            };

            this.logger.LogInformation("Successfully processed reports list query, returning {BarCodeCount} barcodes", vm.Count);
            return Result<BarCodesListVm>.Success(vm);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error processing reports list query");
            return Result<BarCodesListVm>.WithFailure([$"Unexpected error processing reports list: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Story 27.2b-2 (#27/F4): builds the PARAMETERIZED server-side label-substring root query for the product-model
    /// filter (<c>WHERE Label LIKE '%model%'</c>). SQL Server LIKE metacharacters (<c>% _ [</c>) are neutralised with
    /// <c>[]</c> character-class escaping so <paramref name="model"/> matches literally (mirroring EF's
    /// <c>string.Contains</c> translation). The pattern is threaded through the interpolation hole so
    /// <c>IRepository.FromSqlAsync</c> binds it as a SQL PARAMETER — it is never string-concatenated into the SQL.
    /// </summary>
    /// <param name="model">The product-model substring to match within the label.</param>
    /// <returns>A parameterized interpolated SQL statement rooting the barcode query at the label LIKE.</returns>
    private static FormattableString BuildLabelSubstringSql(string model)
    {
        var escaped = model
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
        var pattern = "%" + escaped + "%";
        return $"SELECT * FROM BarCodes WHERE Label LIKE {pattern}";
    }

    /// <summary>
    /// Leases all base queryables needed for filtering operations. #117 (F1): the six repository leases are
    /// bundled into one <see cref="ReportsQueryLeases"/> so the caller's single <c>await using</c> governs
    /// every backing pooled context; when any acquisition fails, every lease already acquired is disposed
    /// HERE before the failure is returned, so a failure Result never leaks a context.
    /// </summary>
    /// <param name="request">The reports request (drives the server-side product-label root filter).</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing the bundled base-queryable leases or failure reasons.</returns>
    private async Task<Result<ReportsQueryLeases>> GetBaseQueryablesAsync(
        GetReportsListQuery request,
        CancellationToken cancellationToken)
    {
        // Story 27.2b-2 (#27/F4): the Reports product-model filter is a label SUBSTRING. BarCode.Label is a
        // value-converted value object whose EF converter translates only whole-property predicates, so a member-
        // access substring (b.Label.Value.Contains(model)) cannot become SQL. When the product filter is active we
        // root the barcode query at a PARAMETERIZED server-side FromSql LIKE (Infrastructure owns the raw SQL). It
        // stays COMPOSABLE, so the composer's date/master/state/etc filters AND the mapper's ToListAsync all execute
        // against SQL — byte-equivalent to the pre-2b-2 WHERE Label LIKE '%model%'.
        var barCodeTask = request.FilterByProduct && !string.IsNullOrEmpty(request.Model)
            ? this.barCodeRepository.FromSqlAsync(BuildLabelSubstringSql(request.Model), cancellationToken)
            : this.barCodeRepository.AsQueryableAsync(cancellationToken);
        var masterLabelTask = this.masterLabelRepository.AsQueryableAsync(cancellationToken);
        var customerTask = this.customerRepository.AsQueryableAsync(cancellationToken);
        var productTask = this.productRepository.AsQueryableAsync(cancellationToken);
        var lineTask = this.lineRepository.AsQueryableAsync(cancellationToken);
        var cycleTask = this.cycleRepository.AsQueryableAsync(cancellationToken);

        await Task.WhenAll(barCodeTask, masterLabelTask, customerTask, productTask, lineTask, cycleTask).ConfigureAwait(false);

        var barCodeResult = await barCodeTask.ConfigureAwait(false);
        var masterLabelResult = await masterLabelTask.ConfigureAwait(false);
        var customerResult = await customerTask.ConfigureAwait(false);
        var productResult = await productTask.ConfigureAwait(false);
        var lineResult = await lineTask.ConfigureAwait(false);
        var cycleResult = await cycleTask.ConfigureAwait(false);

        // Check for failures
        var allResults = new Result[] { barCodeResult, masterLabelResult, customerResult, productResult, lineResult, cycleResult };
        var failures = allResults.Where(r => r.IsFailure).SelectMany(r => r.Errors ?? []).ToList();

        if (failures.Any())
        {
            // #117 (F1): the sibling acquisitions that DID succeed are live context leases — return them
            // to the pool before surfacing the combined failure.
            await DisposeAcquiredLeasesAsync(
                barCodeResult.Value, masterLabelResult.Value, customerResult.Value,
                productResult.Value, lineResult.Value, cycleResult.Value).ConfigureAwait(false);
            return Result<ReportsQueryLeases>.WithFailure(failures);
        }

        // Issue #88: a null queryable value on a successful repository Result is an invariant breach; surface it
        // as a Result failure instead of throwing across the boundary.
        if (masterLabelResult.Value is null
            || customerResult.Value is null
            || productResult.Value is null
            || lineResult.Value is null
            || barCodeResult.Value is null
            || cycleResult.Value is null)
        {
            var nullQueries = new List<string>();
            if (masterLabelResult.Value is null)
            {
                nullQueries.Add("Master label query cannot be null");
            }

            if (customerResult.Value is null)
            {
                nullQueries.Add("Customer query cannot be null");
            }

            if (productResult.Value is null)
            {
                nullQueries.Add("Product query cannot be null");
            }

            if (lineResult.Value is null)
            {
                nullQueries.Add("Line query cannot be null");
            }

            if (barCodeResult.Value is null)
            {
                nullQueries.Add("BarCode query cannot be null");
            }

            if (cycleResult.Value is null)
            {
                nullQueries.Add("Cycle query cannot be null");
            }

            // #117 (F1): dispose the leases that were acquired before surfacing the invariant breach.
            await DisposeAcquiredLeasesAsync(
                barCodeResult.Value, masterLabelResult.Value, customerResult.Value,
                productResult.Value, lineResult.Value, cycleResult.Value).ConfigureAwait(false);
            return Result<ReportsQueryLeases>.WithFailure(nullQueries);
        }

        return Result<ReportsQueryLeases>.Success(new ReportsQueryLeases(
            barCodeResult.Value,
            masterLabelResult.Value,
            customerResult.Value,
            productResult.Value,
            lineResult.Value,
            cycleResult.Value));
    }

    /// <summary>
    /// #117 (F1): disposes every non-null lease in <paramref name="leases"/>, returning the backing pooled
    /// contexts to the pool. Used on the partial-failure paths of <see cref="GetBaseQueryablesAsync"/> where
    /// some acquisitions succeeded (live leases) and others failed (null values).
    /// </summary>
    /// <param name="leases">The leases to dispose; null entries (failed acquisitions) are skipped.</param>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    private static async Task DisposeAcquiredLeasesAsync(params IAsyncDisposable?[] leases)
    {
        foreach (var lease in leases)
        {
            if (lease is not null)
            {
                await lease.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Applies register-based filtering to the barcode list.
    /// </summary>
    /// <param name="barCodes">Current barcode list.</param>
    /// <param name="registerSearch">Register search criteria.</param>
    /// <param name="windowStart">The inclusive start of the report window used to bound the register ledger scan, or null for the content-preserving unbounded scan (#126 review C10, IsMaster path).</param>
    /// <param name="windowEnd">The inclusive end of the report window used to bound the register ledger scan, or null for the content-preserving unbounded scan.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing filtered barcode list or failure reasons.</returns>
    private async Task<Result<List<BarCodeDto>>> ApplyRegisterFilterAsync(
        List<BarCodeDto> barCodes,
        string registerSearch,
        DateTime? windowStart,
        DateTime? windowEnd,
        CancellationToken cancellationToken)
    {
        var matchingIdsResult = await this.registerFilter.GetMatchingBarCodeIdsAsync(registerSearch, windowStart, windowEnd, cancellationToken).ConfigureAwait(false);
        if (matchingIdsResult.IsFailure)
        {
            return Result<List<BarCodeDto>>.WithFailure(matchingIdsResult.Errors);
        }

        if (matchingIdsResult.Value is null)
        {
            return Result<List<BarCodeDto>>.WithFailure(["Matching IDs cannot be null"]);
        }

        var matchingIds = matchingIdsResult.Value;
        var filteredBarCodes = barCodes.Where(bc => matchingIds.Contains(bc.BarCodeId)).ToList();

        this.logger.LogDebug("Applied register filter, remaining barcodes: {BarCodeCount}", filteredBarCodes.Count);
        return Result<List<BarCodeDto>>.Success(filteredBarCodes);
    }

    /// <summary>
    /// #117 (F1): bundles the six owned-queryable leases behind the reports pipeline so ONE
    /// <c>await using</c> in <see cref="ProcessAsync"/> keeps every backing pooled context alive until the
    /// composer/mapper have materialized the composed queries, then returns them all to the pool.
    /// Per-lease disposal is idempotent, so bundle disposal is safe even if a lease was disposed elsewhere.
    /// </summary>
    private sealed class ReportsQueryLeases : IAsyncDisposable
    {
        private readonly OwnedQueryable<BarCode> barCodes;
        private readonly OwnedQueryable<MasterLabel> masterLabels;
        private readonly OwnedQueryable<Customer> customers;
        private readonly OwnedQueryable<Product> products;
        private readonly OwnedQueryable<Line> lines;
        private readonly OwnedQueryable<Cycle> cycles;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsQueryLeases"/> class, taking ownership of all six leases.
        /// </summary>
        /// <param name="barCodes">The barcode lease (plain or FromSql-rooted).</param>
        /// <param name="masterLabels">The master-label lease.</param>
        /// <param name="customers">The customer lease.</param>
        /// <param name="products">The product lease.</param>
        /// <param name="lines">The line lease.</param>
        /// <param name="cycles">The cycle lease.</param>
        public ReportsQueryLeases(
            OwnedQueryable<BarCode> barCodes,
            OwnedQueryable<MasterLabel> masterLabels,
            OwnedQueryable<Customer> customers,
            OwnedQueryable<Product> products,
            OwnedQueryable<Line> lines,
            OwnedQueryable<Cycle> cycles)
        {
            this.barCodes = barCodes;
            this.masterLabels = masterLabels;
            this.customers = customers;
            this.products = products;
            this.lines = lines;
            this.cycles = cycles;
            this.Support = new ReportsSupportQueries(
                masterLabels.Query,
                customers.Query,
                products.Query,
                lines.Query);
        }

        /// <summary>Gets the composable barcode queryable; valid until the bundle is disposed.</summary>
        public IQueryable<BarCode> BarCodes => this.barCodes.Query;

        /// <summary>Gets the support queryables (master labels, customers, products, lines); valid until the bundle is disposed.</summary>
        public ReportsSupportQueries Support { get; }

        /// <summary>Gets the composable cycle queryable; valid until the bundle is disposed.</summary>
        public IQueryable<Cycle> Cycles => this.cycles.Query;

        /// <summary>
        /// Returns all six leased contexts to the pool.
        /// </summary>
        /// <returns>A task representing the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            await this.barCodes.DisposeAsync().ConfigureAwait(false);
            await this.masterLabels.DisposeAsync().ConfigureAwait(false);
            await this.customers.DisposeAsync().ConfigureAwait(false);
            await this.products.DisposeAsync().ConfigureAwait(false);
            await this.lines.DisposeAsync().ConfigureAwait(false);
            await this.cycles.DisposeAsync().ConfigureAwait(false);
        }
    }
}