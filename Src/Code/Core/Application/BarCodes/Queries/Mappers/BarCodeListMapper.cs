// <copyright file="BarCodeListMapper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.DTO;
using IndTrace.Domain.Entities;

namespace IndTrace.Application.BarCodes.Queries.Mappers;

/// <summary>
/// Implementation of IBarCodeListMapper providing comprehensive mapping for barcode lists with cycle count enrichment.
/// Extracted from GetReportsListMonitorQueryHandler to eliminate mapping complexity from handlers.
/// Implements industrial safety patterns with Result&lt;T&gt;, defensive validation, and performance monitoring.
/// </summary>
public class BarCodeListMapper : IBarCodeListMapper
{
    private readonly ILogger<BarCodeListMapper> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeListMapper"/> class.
    /// Follows CLAUDE.md null safety patterns with defensive validation.
    /// </summary>
    /// <param name="logger">Logger for recording operations and performance metrics.</param>
    public BarCodeListMapper(ILogger<BarCodeListMapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Maps barcode query to DTOs with cycle count enrichment and industrial safety patterns.
    /// Replicates exact mapping and enrichment logic from GetReportsListMonitorQueryHandler for compatibility.
    /// </summary>
    /// <param name="query">Filtered barcode query ready for execution.</param>
    /// <param name="cycleQuery">Cycle queryable for count enrichment.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>Result containing mapped DTOs with cycle counts or detailed failure information.</returns>
    public async Task<Result<List<BarCodeDto>>> MapWithCycleCountsAsync(
        IQueryable<BarCode> query,
        IQueryable<Cycle> cycleQuery,
        CancellationToken cancellationToken)
    {
        // CLAUDE.md compliance: Early cancellation check for industrial safety
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<List<BarCodeDto>>.WithFailure(["Operation was canceled."]);
        }
        
        // Defensive validation for null parameters
        if (query is null)
        {
            _logger.LogError("Barcode query cannot be null");
            return Result<List<BarCodeDto>>.WithFailure(["query cannot be null."]);
        }
        
        if (cycleQuery is null)
        {
            _logger.LogError("Cycle query cannot be null");
            return Result<List<BarCodeDto>>.WithFailure(["cycleQuery cannot be null."]);
        }
        
        try
        {
            var sw = Stopwatch.StartNew();
            
            _logger.LogInformation("Starting barcode mapping with cycle count enrichment");
            
            // Execute the barcode query to get entities
            var barCodeEntities = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            var entityLoadTime = sw.ElapsedMilliseconds;
            
            _logger.LogDebug("Loaded {BarCodeCount} barcode entities in {ElapsedMs}ms", barCodeEntities.Count, entityLoadTime);
            
            // Transform to DTOs using existing proven method
            var barCodesResult = BarCodeDto.ToDtoList(barCodeEntities);
            if (barCodesResult.IsFailure || barCodesResult.Value is null)
            {
                _logger.LogError("Failed to convert barcode entities to DTOs: {Errors}", 
                    string.Join(", ", barCodesResult.Errors ?? []));
                return Result<List<BarCodeDto>>.WithFailure(barCodesResult.Errors);
            }
            
            var barCodes = barCodesResult.Value.ToList();
            var mappingTime = sw.ElapsedMilliseconds - entityLoadTime;
            
            _logger.LogDebug("Mapped to {DtoCount} BarCodeDtos in {ElapsedMs}ms", barCodes.Count, mappingTime);
            
            // Early return if no barcodes to process
            if (!barCodes.Any())
            {
                _logger.LogInformation("No barcodes to enrich with cycle counts");
                return Result<List<BarCodeDto>>.Success(barCodes);
            }
            
            // Get cycle counts for enrichment
            var barCodeIds = barCodes.Select(e => e.BarCodeId).ToList();
            var cycleCountResult = await LoadCycleCountsAsync(cycleQuery, barCodeIds, cancellationToken)
                .ConfigureAwait(false);
                
            if (cycleCountResult.IsFailure)
            {
                _logger.LogError("Failed to load cycle counts: {Errors}", 
                    string.Join(", ", cycleCountResult.Errors ?? []));
                return Result<List<BarCodeDto>>.WithFailure(cycleCountResult.Errors);
            }
            
            var cyclesDictionary = cycleCountResult.Value;
            var cycleLoadTime = sw.ElapsedMilliseconds - entityLoadTime - mappingTime;
            
            // Enrich DTOs with cycle counts using LINQ for immutability
            var enrichedBarCodes = barCodes
                .OrderByDescending(barCode => barCode.BarCodeId)
                .Select(barCode =>
                {
                    var cycleDict = cyclesDictionary ?? new Dictionary<int, int>();
                    barCode.CycleCount = cycleDict.GetValueOrDefault(barCode.BarCodeId, 0);
                    return barCode;
                })
                .ToList();
            
            var enrichmentTime = sw.ElapsedMilliseconds - entityLoadTime - mappingTime - cycleLoadTime;
            sw.Stop();
            
            _logger.LogInformation(
                "Barcode mapping completed: {Count} items in {TotalMs}ms " +
                "(Load: {LoadMs}ms, Map: {MapMs}ms, Cycles: {CycleMs}ms, Enrich: {EnrichMs}ms)",
                enrichedBarCodes.Count, sw.ElapsedMilliseconds, 
                entityLoadTime, mappingTime, cycleLoadTime, enrichmentTime);
            
            return Result<List<BarCodeDto>>.Success(enrichedBarCodes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during barcode mapping with cycle counts");
            return Result<List<BarCodeDto>>.WithFailure([$"Mapping failed: {ex.Message}"]);
        }
    }
    
    /// <summary>
    /// Loads cycle counts for the specified barcode IDs with performance monitoring.
    /// Creates a dictionary for efficient lookup during enrichment.
    /// </summary>
    private async Task<Result<Dictionary<int, int>>> LoadCycleCountsAsync(
        IQueryable<Cycle> cycleQuery,
        List<int> barCodeIds,
        CancellationToken cancellationToken)
    {
        try
        {
            // Story 35.D2 C1: Cycle.BarCodeId is the converted BarCodeId key. Build a typed IN-list (whole-property
            // Contains translates to SQL IN over the int column); GROUP BY / COUNT run server-side, then the grouping
            // key is narrowed to int in memory so the returned dictionary stays keyed by the raw int.
            var barCodeIdKeys = barCodeIds.Select(id => new BarCodeId(id)).ToList();
            var groupedCounts = await cycleQuery
                .Where(e => barCodeIdKeys.Contains(e.BarCodeId))
                .GroupBy(e => e.BarCodeId)
                .Select(e => new { BarCodeId = e.Key, Cycles = e.Count() })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            var cyclesDictionary = groupedCounts.ToDictionary(e => e.BarCodeId.Value, e => e.Cycles);
            
            _logger.LogDebug("Loaded cycle counts for {BarCodeCount} barcodes, {CycleEntryCount} have cycles",
                barCodeIds.Count, cyclesDictionary.Count);
            
            return Result<Dictionary<int, int>>.Success(cyclesDictionary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cycle counts");
            return Result<Dictionary<int, int>>.WithFailure([$"Cycle count loading failed: {ex.Message}"]);
        }
    }
}