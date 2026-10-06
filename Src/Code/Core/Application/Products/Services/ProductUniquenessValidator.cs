// <copyright file="ProductUniquenessValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services;

/// <summary>
/// Handles product uniqueness validation against database.
/// Preserves exact error message formats from original handler.
/// </summary>
public class ProductUniquenessValidator : IProductUniquenessValidator
{
    private readonly IRepository<Product> _productRepository;
    private readonly ILogger<ProductUniquenessValidator> _logger;

    public ProductUniquenessValidator(
        IRepository<Product> productRepository,
        ILogger<ProductUniquenessValidator> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates that a product with the given part number does not already exist.
    /// Checks both PartNumber and ProductName for uniqueness.
    /// </summary>
    public async Task<Result> ValidateProductUniquenessAsync(
        string partNumber, 
        string productName, 
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        // Null guards for dependencies
        if (_productRepository is null)
        {
            return Result.WithFailure("Product repository cannot be null.");
        }

        try
        {
            _logger.LogDebug("Validating product uniqueness for PartNumber: {PartNumber}, ProductName: {ProductName}", 
                partNumber, productName);

            // Defensive validation: null/whitespace or minimal length >=3 (resilient service pattern)
            if (string.IsNullOrWhiteSpace(partNumber) || partNumber.Length < 3)
            {
                _logger.LogWarning("PartNumber validation failed - null/whitespace or too short (length: {Length})", 
                    partNumber?.Length ?? 0);
                return Result.WithFailure("PartNumber must be at least 3 characters long.");
            }

            // Create specification matching original handler logic
            var spec = new Specification<Product>(p =>
                p.PartNumber == partNumber || p.ProductName == productName);

            // FAIL CLOSED: use CountAsync, NOT FirstOrDefaultAsync. The repository's FirstOrDefaultAsync surfaces
            // BOTH "no matching row" AND a real infrastructure error as a Result failure, so the previous
            // `IsSuccess` test treated an infra fault as "unique" (fail-open) and let a duplicate through the
            // create-product gate. CountAsync distinguishes the clean case (Success(0) -> unique) from a query
            // error (IsFailure -> refuse) so an unverifiable uniqueness state can never pass as unique.
            var existingCountResult = await _productRepository.CountAsync(spec, cancellationToken)
                .ConfigureAwait(false);

            if (existingCountResult.IsFailure)
            {
                _logger.LogError(
                    "Product uniqueness check could not be verified for PartNumber: {PartNumber}: {Errors}",
                    partNumber,
                    string.Join(", ", existingCountResult.Errors));
                return Result.WithFailure(
                    $"Could not verify product uniqueness for {partNumber}: {string.Join(", ", existingCountResult.Errors)}");
            }

            if (existingCountResult.Value > 0)
            {
                _logger.LogWarning("Product validation failed - product already exists with PartNumber: {PartNumber}", partNumber);

                // EXACT error message format from original handler - critical for compatibility!
                return Result.WithFailure($"Product already exists {partNumber}");
            }

            _logger.LogDebug("Product uniqueness validation successful - product does not exist");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating product uniqueness for PartNumber: {PartNumber}", partNumber);
            return Result.WithFailure($"Exception occurred while validating product uniqueness: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates product uniqueness using ProductInput data.
    /// Convenience method that extracts relevant fields from ProductInput.
    /// </summary>
    public async Task<Result> ValidateProductUniquenessAsync(
        ProductInput productInput, 
        CancellationToken cancellationToken)
    {
        if (productInput is null)
        {
            return Result.WithFailure("ProductInput cannot be null.");
        }

        return await ValidateProductUniquenessAsync(
            productInput.PartNumber, 
            productInput.ProductName, 
            cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if a specific ProductId is available for use.
    /// Used by intelligent ID assignment logic from IProductFactory.
    /// </summary>
    public async Task<bool> IsProductIdAvailableAsync(int productId, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        // Null guard for dependencies
        if (_productRepository is null)
        {
            return false;
        }

        try
        {
            _logger.LogDebug("Checking if ProductId {ProductId} is available for use", productId);

            // FAIL CLOSED: use CountAsync, NOT GetByIdAsync. GetByIdAsync surfaces BOTH "no such row" AND a real
            // infrastructure error as a Result failure, so the previous check treated an infra fault as "id free"
            // (fail-open) and could claim an id that is actually in use. CountAsync distinguishes the clean case
            // (Success(0) -> free) from a query error (IsFailure -> treat as unavailable so the caller falls back
            // to a database-assigned id).
            var spec = new Specification<Product>(p => p.ProductId == new ProductId(productId));

            var countResult = await _productRepository.CountAsync(spec, cancellationToken)
                .ConfigureAwait(false);

            if (countResult.IsFailure)
            {
                _logger.LogWarning(
                    "ProductId {ProductId} availability could not be verified - treating as unavailable: {Errors}",
                    productId,
                    string.Join(", ", countResult.Errors));
                return false; // Fail closed: an unverifiable id is never claimed as free
            }

            // If the product is found, it means the ID is already in use, so we cannot use it
            if (countResult.Value > 0)
            {
                _logger.LogDebug("ProductId {ProductId} is already in use", productId);
                return false; // ID is in use
            }

            // If the product is not found, it means the ID is free to use
            _logger.LogDebug("ProductId {ProductId} is available for use", productId);
            return true; // ID is free
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking ProductId availability for {ProductId}", productId);
            // In case of error, return false to prevent using potentially conflicting ID
            return false;
        }
    }
}