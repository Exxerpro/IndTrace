// <copyright file="RegisterDataFilter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;

namespace IndTrace.Application.BarCodes.Queries.Filters;

/// <summary>
/// Implementation of IRegisterDataFilter providing register-based barcode filtering.
/// Extracted from GetReportsListMonitorQueryHandler to eliminate filtering complexity from handlers.
/// Implements industrial safety patterns with Result&lt;T&gt;, defensive validation, and performance monitoring.
/// #229 (Slice B): filtering now rides ONE projected SQL query (a correlated register EXISTS rooted at Cycles),
/// so the filter needs only the read-only cycle repository.
/// </summary>
public class RegisterDataFilter : IRegisterDataFilter
{
    private readonly IReadOnlyRepository<Cycle> _cycleRepository;
    private readonly ILogger<RegisterDataFilter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterDataFilter"/> class.
    /// Follows CLAUDE.md null safety patterns with defensive validation.
    /// </summary>
    /// <param name="cycleRepository">Repository for accessing cycle data (carries the raw-SQL query seam).</param>
    /// <param name="logger">Logger for recording operations and performance metrics.</param>
    public RegisterDataFilter(
        IReadOnlyRepository<Cycle> cycleRepository,
        ILogger<RegisterDataFilter> logger)
    {
        _cycleRepository = cycleRepository ?? throw new ArgumentNullException(nameof(cycleRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Gets barcode IDs matching register search criteria with industrial safety patterns.
    /// Delegates to existing repository method and converts result to HashSet for efficient lookup.
    /// </summary>
    /// <param name="registerSearch">Register search criteria.</param>
    /// <param name="windowStart">The inclusive start of the report's date window bounding the register ledger scan, or null for the content-preserving unbounded scan (#126 review C10, IsMaster path — #119 F6 PO ruling pending).</param>
    /// <param name="windowEnd">The inclusive end of the report's date window bounding the register ledger scan, or null for the content-preserving unbounded scan.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>Result containing matching barcode IDs or detailed failure information.</returns>
    public async Task<Result<HashSet<int>>> GetMatchingBarCodeIdsAsync(
        string registerSearch,
        DateTime? windowStart,
        DateTime? windowEnd,
        CancellationToken cancellationToken)
    {
        // CLAUDE.md compliance: Early cancellation check for industrial safety
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<HashSet<int>>.WithFailure(["Operation was canceled."]);
        }
        
        // Defensive validation for null or empty search criteria
        if (string.IsNullOrWhiteSpace(registerSearch))
        {
            _logger.LogWarning("Register search criteria is null or empty");
            return Result<HashSet<int>>.Success(new HashSet<int>());
        }
        
        try
        {
            var sw = Stopwatch.StartNew();
            
            _logger.LogInformation("Starting register data filtering for search: {RegisterSearch}", registerSearch);

            // #229 (Slice B): ONE projected SQL round-trip (correlated register EXISTS rooted at Cycles,
            // projected to distinct owning barcode ids server-side) replaces the former 3-hop entity chain.
            var barCodeIdsResult = await _cycleRepository
                .GetBarCodeIdsByRegisterDataAsync(registerSearch, windowStart, windowEnd, cancellationToken)
                .ConfigureAwait(false);

            if (barCodeIdsResult.IsFailure)
            {
                _logger.LogError("Failed to get barcode ids by register data: {Errors}",
                    string.Join(", ", barCodeIdsResult.Errors ?? []));
                return Result<HashSet<int>>.WithFailure(barCodeIdsResult.Errors);
            }

            var barCodeIds = barCodeIdsResult.Value ?? new HashSet<int>();
            
            sw.Stop();
            
            _logger.LogInformation(
                "Register data filtering completed: {MatchingCount} matching barcodes found in {ElapsedMs}ms for search: {RegisterSearch}",
                barCodeIds.Count, sw.ElapsedMilliseconds, registerSearch);
            
            if (!barCodeIds.Any())
            {
                _logger.LogInformation("No barcodes found matching register search criteria: {RegisterSearch}", registerSearch);
            }
            
            return Result<HashSet<int>>.Success(barCodeIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during register data filtering for search: {RegisterSearch}", registerSearch);
            return Result<HashSet<int>>.WithFailure([$"Register filtering failed: {ex.Message}"]);
        }
    }
}