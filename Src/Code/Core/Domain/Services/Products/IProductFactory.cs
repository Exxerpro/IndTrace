// <copyright file="IProductFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;

namespace IndTrace.Domain.Services.Products;

/// <summary>
/// Pure product creation logic without external dependencies.
/// Handles entity creation and advanced ID parsing logic from PartNumber.
/// Domain service - contains zero dependencies, pure business logic only.
/// </summary>
public interface IProductFactory
{
    /// <summary>
    /// Creates a Product entity with intelligent ID assignment.
    /// Implements advanced PartNumber parsing with dynamic offset calculation.
    /// </summary>
    /// <param name="productData">Core product information including PartNumber for ID parsing</param>
    /// <param name="customer">Associated customer entity</param>
    /// <param name="line">Associated production line entity</param>
    /// <returns>Product entity with intelligently assigned ProductId based on PartNumber parsing</returns>
    /// <remarks>
    /// Advanced ID Logic:
    /// - Parses numeric suffix from PartNumber (e.g., "FORD-F150-001" → 1)
    /// - Applies dynamic offset based on number width (1 → 11, 23 → 123, 456 → 1456)
    /// - Preserves visual comparison while avoiding ID conflicts
    /// - Falls back to auto-generated ID if parsing fails
    /// </remarks>
    Product CreateProduct(ProductInput productData, Customer customer, Line line);

    /// <summary>
    /// Creates a Product entity with intelligent ID assignment.
    /// Implements advanced PartNumber parsing with dynamic offset calculation.
    /// </summary>
    /// <param name="productData">Core product information including PartNumber for ID parsing</param>
    /// <param name="customer">Associated customer entity</param>
    /// <param name="line">Associated production line entity</param>
    /// <returns>Product entity with intelligently assigned ProductId based on PartNumber parsing</returns>
    /// <remarks>
    /// Advanced ID Logic:
    /// - Parses numeric suffix from PartNumber (e.g., "FORD-F150-001" → 1)
    /// - Applies dynamic offset based on number width (1 → 11, 23 → 123, 456 → 1456)
    /// - Preserves visual comparison while avoiding ID conflicts
    /// - Falls back to auto-generated ID if parsing fails
    /// </remarks>
    Result<Product> CreateResultProduct(ProductInput productData, Customer customer, Line line);

    /// <summary>
    /// Attempts to parse the last integer from a part number string.
    /// Core business logic for intelligent ProductId assignment.
    /// </summary>
    /// <param name="partNumber">The part number string to parse</param>
    /// <returns>Tuple with success flag and parsed integer value</returns>
    /// <remarks>
    /// Uses regex pattern @"\d+$" to find last numeric sequence.
    /// Examples: "FORD-F150-001" → (true, 1), "TESLA-Y-ABC" → (false, 0)
    /// </remarks>
    (bool Success, int ParsedId) TryParseLastInteger(string partNumber);

    /// <summary>
    /// Calculates dynamic offset based on the width of the parsed number.
    /// Maintains visual comparison while avoiding conflicts.
    /// </summary>
    /// <param name="parsedNumber">The parsed number from PartNumber</param>
    /// <returns>Dynamic offset for ProductId assignment</returns>
    /// <remarks>
    /// Offset Logic:
    /// - Single digit: +10 (5 → 15)
    /// - Double digit: +100 (23 → 123)
    /// - Triple digit: +1000 (456 → 1456)
    /// - Formula: Math.Pow(10, numberWidth)
    /// </remarks>
    int GetDynamicOffset(int parsedNumber);
}

// ProductInput is defined in ProductInput.cs - removed duplicate definition