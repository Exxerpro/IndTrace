// <copyright file="ReportsListQueryComposer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.GetReportsList.GetList;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;

namespace IndTrace.Application.BarCodes.Queries.Composers;

/// <summary>
/// Implementation of IReportsListQueryComposer providing comprehensive query composition for reports list.
/// Extracted from GetReportsListMonitorQueryHandler to eliminate query complexity from handlers.
/// Implements industrial safety patterns with Result&lt;T&gt;, defensive validation, and performance monitoring.
/// </summary>
public class ReportsListQueryComposer : IReportsListQueryComposer
{
    private readonly ILogger<ReportsListQueryComposer> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsListQueryComposer"/> class.
    /// Follows CLAUDE.md null safety patterns with defensive validation.
    /// </summary>
    /// <param name="logger">Logger for recording operations and performance metrics.</param>
    public ReportsListQueryComposer(ILogger<ReportsListQueryComposer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Composes filtered barcode query based on request criteria with industrial safety patterns.
    /// Replicates exact filtering logic from GetReportsListMonitorQueryHandler for compatibility.
    /// </summary>
    /// <param name="baseQuery">Base barcode queryable.</param>
    /// <param name="request">Filter request with validated parameters.</param>
    /// <param name="supportQueries">Supporting entity queryables.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>Result containing composed query or detailed failure information.</returns>
    public async Task<Result<IQueryable<BarCode>>> ComposeAsync(
        IQueryable<BarCode> baseQuery,
        GetReportsListQuery request,
        ReportsSupportQueries supportQueries,
        CancellationToken cancellationToken)
    {
        // CLAUDE.md compliance: Early cancellation check for industrial safety
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IQueryable<BarCode>>.WithFailure(["Operation was canceled."]);
        }
        
        // Defensive validation for null parameters
        if (baseQuery is null)
        {
            _logger.LogError("Base query cannot be null");
            return Result<IQueryable<BarCode>>.WithFailure(["baseQuery cannot be null."]);
        }
        
        if (request is null)
        {
            _logger.LogError("Request cannot be null");
            return Result<IQueryable<BarCode>>.WithFailure(["request cannot be null."]);
        }
        
        if (supportQueries is null)
        {
            _logger.LogError("Support queries cannot be null");
            return Result<IQueryable<BarCode>>.WithFailure(["supportQueries cannot be null."]);
        }
        
        try
        {
            var query = baseQuery;
            var appliedFilters = new List<string>();

            _logger.LogInformation("Starting query composition with filters - IsMaster: {IsMaster}, DateRange: {StartDate} to {EndDate}",
                request.IsMaster, request.StartDate, request.EndDate);

            // Story 27.2b-2 (#27/F4): the product-model filter is a label SUBSTRING (SQL LIKE). BarCode.Label is a
            // value-converted BarCodeLabel value object, and an EF value converter maps only the WHOLE property
            // (b.Label == vo / IN) — a member-access substring (b.Label.Value.Contains(model)) does NOT translate.
            // The substring is therefore applied SERVER-SIDE as a parameterized FromSql LIKE at the barcode base-query
            // ROOT by the handler (GetReportsListMonitorQueryHandler.GetBaseQueryablesAsync) BEFORE this composer runs —
            // FromSql lives in EF.Relational (Infrastructure), outside this Application-layer composer. The base query
            // reaching here already carries that LIKE, and every filter below composes onto it server-side (byte-
            // equivalent to the pre-2b-2 WHERE Label LIKE '%model%'). This composer only enforces the customer/empty-
            // model gates in ApplyProductFilterAsync below.

            // Apply master label filtering.
            // #119 (F6, PO-FLAG — behavior deliberately unchanged): the IsMaster branch applies NO date bound.
            // It narrows only by master-label membership, so it scans the full BarCodes table regardless of the
            // request's StartDate/EndDate — a known unbounded scan that grows with the ledger. Left as-is
            // pending a PO ruling (#119) on whether master reports should honor the date window; bounding it
            // here would silently change master-report content.
            if (request.IsMaster)
            {
                var masterLabelResult = await ApplyMasterLabelFilterAsync(query, supportQueries.MasterLabels, cancellationToken)
                    .ConfigureAwait(false);
                    
                if (masterLabelResult.IsFailure)
                {
                    return Result<IQueryable<BarCode>>.WithFailure(masterLabelResult.Errors);
                }
                
                query = masterLabelResult.Value;
                appliedFilters.Add("MasterLabel");
            }
            else
            {
                // Apply date range filtering
                query = query.Where(e => e.ModifiedOn >= request.StartDate && e.ModifiedOn <= request.EndDate);
                appliedFilters.Add($"DateRange({request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd})");
            }
            
            // Apply product filtering
            if (request.FilterByProduct)
            {
                if (query is null)
                {
                    _logger.LogError("Query is null during product filtering");
                    return Result<IQueryable<BarCode>>.WithFailure(["Query became null during filtering"]);
                }
                
                var productFilterResult = await ApplyProductFilterAsync(query, request, supportQueries, cancellationToken)
                    .ConfigureAwait(false);
                    
                if (productFilterResult.IsFailure)
                {
                    return Result<IQueryable<BarCode>>.WithFailure(productFilterResult.Errors);
                }
                
                query = productFilterResult.Value;
                appliedFilters.Add($"Product({request.Model})");
            }
            
            // Apply state filtering
            if (request.FilterByState)
            {
                if (query is null)
                {
                    _logger.LogError("Query is null during state filtering");
                    return Result<IQueryable<BarCode>>.WithFailure(["Query became null during filtering"]);
                }
                
                var stateFilterResult = ApplyStateFilter(query, request.State);
                if (stateFilterResult.IsFailure)
                {
                    return Result<IQueryable<BarCode>>.WithFailure(stateFilterResult.Errors);
                }
                
                query = stateFilterResult.Value;
                appliedFilters.Add($"State({request.State})");
            }
            
            // Apply shift filtering
            if (request.FilterByShift)
            {
                if (query is null)
                {
                    _logger.LogError("Query is null during shift filtering");
                    return Result<IQueryable<BarCode>>.WithFailure(["Query became null during filtering"]);
                }
                
                query = ApplyShiftFilter(query, request.Shift);
                appliedFilters.Add($"Shift({request.Shift})");
            }
            
            // Apply line filtering
            if (request.FilterByLine)
            {
                if (query is null)
                {
                    _logger.LogError("Query is null during line filtering");
                    return Result<IQueryable<BarCode>>.WithFailure(["Query became null during filtering"]);
                }
                
                var lineFilterResult = await ApplyLineFilterAsync(query, request.Line, supportQueries.Lines, supportQueries.Products, cancellationToken)
                    .ConfigureAwait(false);
                    
                if (lineFilterResult.IsFailure)
                {
                    return Result<IQueryable<BarCode>>.WithFailure(lineFilterResult.Errors);
                }
                
                query = lineFilterResult.Value;
                appliedFilters.Add($"Line({request.Line})");
            }
            
            if (query is null)
            {
                _logger.LogError("Final query is null");
                return Result<IQueryable<BarCode>>.WithFailure(["Final query is null"]);
            }
            
            _logger.LogInformation("Query composition completed with filters: {AppliedFilters}", 
                string.Join(", ", appliedFilters));
            
            return Result<IQueryable<BarCode>>.Success(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during query composition");
            return Result<IQueryable<BarCode>>.WithFailure([$"Query composition failed: {ex.Message}"]);
        }
    }
    
    /// <summary>
    /// Applies master label filtering to the base query.
    /// Filters barcodes to only include those with labels matching master label codes.
    /// </summary>
    private async Task<Result<IQueryable<BarCode>>> ApplyMasterLabelFilterAsync(
        IQueryable<BarCode> query, 
        IQueryable<MasterLabel> masterLabels, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (masterLabels is null)
            {
                _logger.LogWarning("Master labels queryable is null, returning original query");
                return Result<IQueryable<BarCode>>.Success(query);
            }
            
            var masterLabelCodes = await masterLabels
                .Select(e => e.MasterLabelCode)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Story 27.2b-2: BarCode.Label is a value-converted VO; EF translates a VO-to-VO IN (Label IN (...)) but
            // NOT b.Label.Value. Materialize the master codes as BarCodeLabel values so the converter renders the IN.
            var masterLabelVos = masterLabelCodes
                .Where(code => !string.IsNullOrEmpty(code))
                .Select(BarCodeLabel.FromPersisted)
                .ToList();

            var filteredQuery = query.Where(e => masterLabelVos.Contains(e.Label));
            
            _logger.LogDebug("Applied master label filter with {LabelCount} master labels", masterLabelCodes.Count);
            return Result<IQueryable<BarCode>>.Success(filteredQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying master label filter");
            return Result<IQueryable<BarCode>>.WithFailure([$"Master label filter failed: {ex.Message}"]);
        }
    }
    
    /// <summary>
    /// Applies product filtering to the base query.
    /// Supports both customer-specific product filtering and general model filtering.
    /// </summary>
    private async Task<Result<IQueryable<BarCode>>> ApplyProductFilterAsync(
        IQueryable<BarCode> query,
        GetReportsListQuery request,
        ReportsSupportQueries supportQueries,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Model))
            {
                _logger.LogInformation("Product filter requested but no model specified");
                return Result<IQueryable<BarCode>>.Success(query.Where(e => false)); // Return empty result
            }
            
            // Customer-specific product filtering
            if (request.FilterByCustomer)
            {
                if (supportQueries.Customers is null || supportQueries.Products is null)
                {
                    _logger.LogError("Customer or product queryables are null for customer product filtering");
                    return Result<IQueryable<BarCode>>.WithFailure(["Customer or product data not available for filtering"]);
                }
                
                var productNames = await supportQueries.Customers
                    .Where(e => e.Name == request.CustomerSearch)
                    .SelectMany(c => supportQueries.Products
                        .Where(p => p.CustomerId == c.CustomerId)
                        .Select(p => p.ProductName))
                    .Where(p => p.Contains(request.Model))
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                
                if (!productNames.Any())
                {
                    _logger.LogInformation("No matching products found for customer: {Customer}, model: {Model}",
                        request.CustomerSearch, request.Model);
                    return Result<IQueryable<BarCode>>.Success(query.Where(e => false)); // Return empty result
                }
                
                // Story 27.2b-2: the label-substring narrowing is applied SERVER-SIDE at the query root
                // (ComposeAsync -> ReRootWithLabelSubstring). This branch now only enforces the customer gate;
                // the query already carries the parameterized LIKE.
                _logger.LogDebug("Applied customer product filter gate: {Customer}, {Model}", request.CustomerSearch, request.Model);
                return Result<IQueryable<BarCode>>.Success(query);
            }

            // General model filtering — the label LIKE is already applied at the query root (server-side).
            _logger.LogDebug("Applied product model filter (server-side LIKE at root): {Model}", request.Model);
            return Result<IQueryable<BarCode>>.Success(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying product filter");
            return Result<IQueryable<BarCode>>.WithFailure([$"Product filter failed: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Applies state filtering to the base query.
    /// Converts state name to enum value for precise filtering.
    /// </summary>
    private Result<IQueryable<BarCode>> ApplyStateFilter(IQueryable<BarCode> query, string? state)
    {
        // F7 (#80): trim before FromName. EnumModel.FromName is exact-match, so a whitespace-padded valid name
        // (e.g. " Created ") would otherwise resolve to the Invalid sentinel and be wrongly failed-loud below as
        // unrecognized. Trim first so a padded valid state is accepted; a genuinely unrecognized name still fails loud.
        var stateValue = string.IsNullOrWhiteSpace(state) ? "None" : state.Trim();

        FlowStatus stateInt;
        try
        {
            stateInt = EnumModel.FromName<FlowStatus>(stateValue);
        }
        catch (Exception ex)
        {
            // Backstop: FromName is not expected to throw for FlowStatus (see the Invalid-sentinel guard
            // below), but if a future provider does, still fail loud rather than drop the filter.
            _logger.LogWarning(ex, "Unparseable flow status filter: {State}", stateValue);
            return Result<IQueryable<BarCode>>.WithFailure(
                [$"Invalid state filter '{stateValue}': not a recognized FlowStatus."]);
        }

        // FAIL LOUD on an unrecognized name. EnumModel.FromName does NOT throw for an unknown name — it
        // resolves ANY unrecognized string to the FlowStatus.Invalid sentinel (value 8). Left unchecked, a
        // typo'd state ("Createed") would silently narrow to the Invalid bucket and return zero rows, quietly
        // misreporting the data. Treat "resolved to Invalid while the caller did not literally ask for Invalid"
        // as an invalid filter and surface it, so the caller/UI corrects the query instead of trusting a wrong
        // (empty) narrowing. An explicit "Invalid" request is still honored.
        var invalidSentinel = FlowStatus.Invalid;
        if (stateInt.Value == invalidSentinel.Value
            && !string.Equals(stateValue, invalidSentinel.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Unrecognized flow status filter: {State} (resolved to the Invalid sentinel).", stateValue);
            return Result<IQueryable<BarCode>>.WithFailure(
                [$"Invalid state filter '{stateValue}': not a recognized FlowStatus."]);
        }

        var filteredQuery = query.Where(e => e.FlowStatus == stateInt);
        _logger.LogDebug("Applied state filter: {State} (value: {StateInt})", stateValue, stateInt);
        return Result<IQueryable<BarCode>>.Success(filteredQuery);
    }
    
    /// <summary>
    /// Applies shift filtering to the base query.
    /// Filters based on time ranges for different shifts.
    /// </summary>
    private IQueryable<BarCode> ApplyShiftFilter(IQueryable<BarCode> query, int shift)
    {
        // Canonical shift boundaries (ShiftDetectionRuleExecutor): First 07:00-15:00, Second 15:00-23:00,
        // Third 23:00-07:00 (spans midnight). These three windows PARTITION the full 24h with no gap and no
        // lost sliver. The previous bounds lost two spans of barcodes:
        //   * 06:00-08:00 belonged to NO shift (first started at 08:00, third ended at 06:00);
        //   * the final ~3.6s of the day (23:59:56.4-24:00) was dropped by the `<= 23.999h` upper bound.
        // Shift 3 now needs no upper bound: TimeOfDay is always < 24h, so `>= 23h` captures the whole late
        // window including the final sliver, and `< 7h` captures the early-morning half.
        var filteredQuery = query.Where(e =>
            (shift == 1 && e.CreatedOn.TimeOfDay >= TimeSpan.FromHours(7) && e.CreatedOn.TimeOfDay < TimeSpan.FromHours(15)) ||
            (shift == 2 && e.CreatedOn.TimeOfDay >= TimeSpan.FromHours(15) && e.CreatedOn.TimeOfDay < TimeSpan.FromHours(23)) ||
            (shift == 3 && (e.CreatedOn.TimeOfDay >= TimeSpan.FromHours(23) || e.CreatedOn.TimeOfDay < TimeSpan.FromHours(7))));

        _logger.LogDebug("Applied shift filter: {Shift}", shift);
        return filteredQuery;
    }
    
    /// <summary>
    /// Applies line filtering to the base query. A <see cref="BarCode"/> carries no LineId; it belongs to a
    /// line through its <see cref="Product"/> (<c>Product.LineId</c>), so this narrows to the products on the
    /// requested line and filters barcodes by those product ids. A requested line that does not exist yields
    /// NO rows (never every line's rows — that was the pre-fix data leak).
    /// </summary>
    private async Task<Result<IQueryable<BarCode>>> ApplyLineFilterAsync(
        IQueryable<BarCode> query,
        string? lineName,
        IQueryable<Line> lines,
        IQueryable<Product> products,
        CancellationToken cancellationToken)
    {
        try
        {
            if (lines is null || products is null)
            {
                _logger.LogError("Lines or products queryable is null for line filtering");
                return Result<IQueryable<BarCode>>.WithFailure(["Line or product data not available for line filtering."]);
            }

            var line = await lines
                .FirstOrDefaultAsync(e => e.Name == lineName, cancellationToken)
                .ConfigureAwait(false);

            if (line is null)
            {
                // FAIL SAFE, not open: a requested line filter that resolves to nothing must return an EMPTY
                // result set, never the unfiltered query (which would leak every line's barcodes as if they
                // belonged to the requested line).
                _logger.LogWarning("Line filter requested but line not found: {LineName}; returning empty result.", lineName);
                return Result<IQueryable<BarCode>>.Success(query.Where(e => false));
            }

            // BarCode.ProductId and Product.ProductId are the same value-converted ProductId struct, so a
            // whole-property IN (Contains) translates through the converter (same pattern as the master-label
            // filter above); a member-access on .Value would not translate.
            var lineProductIds = await products
                .Where(p => p.LineId == line.LineId)
                .Select(p => p.ProductId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var filteredQuery = query.Where(e => lineProductIds.Contains(e.ProductId));

            _logger.LogDebug("Applied line filter: {LineName} (ID: {LineId}) over {ProductCount} product(s).",
                line.Name, line.LineId, lineProductIds.Count);
            return Result<IQueryable<BarCode>>.Success(filteredQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying line filter");
            return Result<IQueryable<BarCode>>.WithFailure([$"Line filter failed: {ex.Message}"]);
        }
    }
}