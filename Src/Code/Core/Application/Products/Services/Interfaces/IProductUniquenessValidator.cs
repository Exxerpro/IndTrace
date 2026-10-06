// <copyright file="IProductUniquenessValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services.Interfaces;

/// <summary>
/// Handles product uniqueness validation against database.
/// Application service - orchestrates database checks for product existence.
/// </summary>
public interface IProductUniquenessValidator
{
    /// <summary>
    /// Validates that a product with the given part number does not already exist.
    /// Checks both PartNumber and ProductName for uniqueness.
    /// </summary>
    /// <param name="partNumber">Part number to check for uniqueness</param>
    /// <param name="productName">Product name to check for uniqueness</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Success if product is unique, failure if product already exists</returns>
    /// <remarks>
    /// Database Query Logic:
    /// - Checks both PartNumber and ProductName fields
    /// - Uses Specification pattern for flexible queries
    /// - Returns specific error messages for business context
    /// - Preserves exact error message format: "Product already exists {partNumber}"
    /// </remarks>
    Task<Result> ValidateProductUniquenessAsync(string partNumber, string productName, CancellationToken cancellationToken);

    /// <summary>
    /// Validates product uniqueness using ProductInput data.
    /// Convenience method that extracts relevant fields from ProductInput.
    /// </summary>
    /// <param name="productInput">Product input data containing part number and name</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Success if product is unique, failure if product already exists</returns>
    Task<Result> ValidateProductUniquenessAsync(ProductInput productInput, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a specific ProductId is available for use.
    /// Used by intelligent ID assignment logic from IProductFactory.
    /// </summary>
    /// <param name="productId">ProductId to check for availability</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>True if ProductId is available, false if already in use</returns>
    /// <remarks>
    /// Supporting ID Assignment Logic:
    /// - Used by advanced PartNumber parsing logic
    /// - Enables dynamic offset calculation
    /// - Prevents ID conflicts during intelligent assignment
    /// - Returns boolean for simple availability check
    /// </remarks>
    Task<bool> IsProductIdAvailableAsync(int productId, CancellationToken cancellationToken);
}