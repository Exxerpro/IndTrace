// <copyright file="BarCodeQueries.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries;

/// <summary>
/// Pure query composition helpers for BarCode operations
/// No execution, no side effects, deterministic ordering
/// </summary>
/// <remarks>
/// This static class provides composable query helpers for BarCode entities.
/// All methods are pure functions that return modified IQueryable instances
/// without executing queries or causing side effects.
/// 
/// Industrial safety compliance:
/// - Defensive null checks on all parameters
/// - Consistent ordering for deterministic results
/// - ArgumentException for invalid parameters
/// - No database execution - pure composition only
/// </remarks>
public static class BarCodeQueries
{
    /// <summary>
    /// Filters barcodes by ProductId with consistent ordering
    /// </summary>
    /// <param name="source">The source queryable to filter. Cannot be null.</param>
    /// <param name="productId">The product ID to filter by. Must be greater than 0.</param>
    /// <returns>A filtered and ordered queryable of BarCode entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentException">Thrown when productId is less than or equal to 0.</exception>
    /// <remarks>
    /// Applies the following transformations:
    /// 1. Filters by ProductId equality
    /// 2. Orders by CreatedOn ascending
    /// 3. Then orders by BarCodeId ascending for deterministic sorting
    /// 
    /// This method does not execute the query - it only modifies the query expression tree.
    /// </remarks>
    public static IQueryable<BarCode> ForProduct(
        IQueryable<BarCode> source,
        int productId)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        if (productId <= 0)
            throw new ArgumentException("ProductId must be greater than 0", nameof(productId));
        
        return source
            .Where(b => b.ProductId == new ProductId(productId))
            .OrderBy(b => b.CreatedOn)
            .ThenBy(b => b.BarCodeId);
    }
    
    /// <summary>
    /// Filters barcodes by exact label match
    /// </summary>
    /// <param name="source">The source queryable to filter. Cannot be null.</param>
    /// <param name="label">The label to filter by. Cannot be null, empty, or whitespace.</param>
    /// <returns>A filtered queryable of BarCode entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentException">Thrown when label is null, empty, or whitespace.</exception>
    /// <remarks>
    /// Performs exact string matching on the Label property.
    /// Industrial manufacturing requires precise label matching for traceability.
    /// 
    /// This method does not execute the query - it only modifies the query expression tree.
    /// </remarks>
    public static IQueryable<BarCode> ForLabel(
        IQueryable<BarCode> source,
        string label)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be empty", nameof(label));

        // Story 27.2b-2: VO-to-VO equality translates through the value converter (b.Label.Value == string does not).
        var labelVo = BarCodeLabel.FromPersisted(label);
        return source.Where(b => b.Label.Equals(labelVo));
    }
    
    /// <summary>
    /// Filters barcodes by MachineId with consistent ordering
    /// </summary>
    /// <param name="source">The source queryable to filter. Cannot be null.</param>
    /// <param name="machineId">The machine ID to filter by. Must be greater than 0.</param>
    /// <returns>A filtered and ordered queryable of BarCode entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentException">Thrown when machineId is less than or equal to 0.</exception>
    /// <remarks>
    /// Applies the following transformations:
    /// 1. Filters by MachineId equality
    /// 2. Orders by CreatedOn ascending for chronological processing
    /// 3. Then orders by BarCodeId ascending for deterministic sorting
    /// 
    /// This method does not execute the query - it only modifies the query expression tree.
    /// </remarks>
    public static IQueryable<BarCode> ForMachine(
        IQueryable<BarCode> source,
        int machineId)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        if (machineId <= 0)
            throw new ArgumentException("MachineId must be greater than 0", nameof(machineId));
        
        return source
            .Where(b => b.MachineId == new MachineId(machineId))
            .OrderBy(b => b.CreatedOn)
            .ThenBy(b => b.BarCodeId);
    }
    
    /// <summary>
    /// Filters barcodes by date range with consistent ordering
    /// </summary>
    /// <param name="source">The source queryable to filter. Cannot be null.</param>
    /// <param name="startDate">The start date (inclusive).</param>
    /// <param name="endDate">The end date (inclusive).</param>
    /// <returns>A filtered and ordered queryable of BarCode entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentException">Thrown when startDate is greater than endDate.</exception>
    /// <remarks>
    /// Filters by CreatedOn date within the specified range (inclusive).
    /// Orders results chronologically for manufacturing traceability.
    /// 
    /// This method does not execute the query - it only modifies the query expression tree.
    /// </remarks>
    public static IQueryable<BarCode> ForDateRange(
        IQueryable<BarCode> source,
        DateTime startDate,
        DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        if (startDate > endDate)
            throw new ArgumentException("StartDate cannot be greater than EndDate", nameof(startDate));
        
        return source
            .Where(b => b.CreatedOn >= startDate && b.CreatedOn <= endDate)
            .OrderBy(b => b.CreatedOn)
            .ThenBy(b => b.BarCodeId);
    }
}