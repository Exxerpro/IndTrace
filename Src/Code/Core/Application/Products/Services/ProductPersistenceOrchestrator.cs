// <copyright file="ProductPersistenceOrchestrator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services;

/// <summary>
/// Orchestrates sophisticated product persistence with intelligent ID assignment.
/// Handles complex persistence strategy from original handler including dual AddAsync patterns.
/// Preserves EXACT persistence logic and ID assignment algorithms.
/// </summary>
public class ProductPersistenceOrchestrator : IProductPersistenceOrchestrator
{
    private readonly IRepository<Product> _productRepository;
    private readonly IProductFactory _productFactory;
    private readonly IProductUniquenessValidator _productUniquenessValidator;
    private readonly ILogger<ProductPersistenceOrchestrator> _logger;
    private readonly IDateTimeMachine _dateTimeMachine;

    public ProductPersistenceOrchestrator(
        IRepository<Product> productRepository,
        IProductFactory productFactory,
        IProductUniquenessValidator productUniquenessValidator,
        ILogger<ProductPersistenceOrchestrator> logger,
        IDateTimeMachine dateTimeMachine)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _productFactory = productFactory ?? throw new ArgumentNullException(nameof(productFactory));
        _productUniquenessValidator = productUniquenessValidator ?? throw new ArgumentNullException(nameof(productUniquenessValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dateTimeMachine = dateTimeMachine ?? throw new ArgumentNullException(nameof(dateTimeMachine));
    }

    /// <summary>
    /// Creates and persists a product using provided parsed id and dynamic offset when available.
    /// Falls back to auto id when the calculated id is not available.
    /// </summary>
    public async Task<Result<Product>> CreateProductWithIntelligentIdAsync(
        Product product,
        int parsedId,
        int dynamicOffset,
        CancellationToken cancellationToken)
    {
        if (product is null)
        {
            return Result<Product>.WithFailure("Product cannot be null.");
        }

        var proposedId = CalculateAdjustedProductId(parsedId, dynamicOffset);
        var isAvailable = await _productUniquenessValidator
            .IsProductIdAvailableAsync(proposedId, cancellationToken)
            .ConfigureAwait(false);

        if (isAvailable)
        {
            return await CreateProductWithSpecificIdAsync(product, proposedId, "Products", cancellationToken)
                .ConfigureAwait(false);
        }

        return await CreateProductWithAutoIdAsync(product, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Persists a product with database-assigned id.
    /// </summary>
    public async Task<Result<Product>> CreateProductWithAutoIdAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        if (product is null)
        {
            return Result<Product>.WithFailure("Product cannot be null.");
        }

        product.ProductId = new ProductId(0);
        var result = await _productRepository.AddAsync(product, cancellationToken).ConfigureAwait(false);
        return result.IsFailure ? Result<Product>.WithFailure(result.Errors) : Result<Product>.Success(product);
    }

    /// <summary>
    /// Persists a product with a specific id into a specific table.
    /// </summary>
    public async Task<Result<Product>> CreateProductWithSpecificIdAsync(
        Product product,
        int productId,
        string tableName,
        CancellationToken cancellationToken)
    {
        if (product is null)
        {
            return Result<Product>.WithFailure("Product cannot be null.");
        }

        product.ProductId = new ProductId(productId);
        var result = await _productRepository
            .AddAsync(product, productId, tableName, cancellationToken)
            .ConfigureAwait(false);

        return result.IsFailure ? Result<Product>.WithFailure(result.Errors) : Result<Product>.Success(product);
    }

    /// <summary>
    /// Compensating delete for a mid-sequence-persisted product whose downstream creation step failed.
    /// Removes the orphan so the pre-persist uniqueness gate no longer blocks a retry.
    /// </summary>
    public async Task<Result> DeleteProductAsync(Product product, CancellationToken cancellationToken)
    {
        if (product is null)
        {
            return Result.WithFailure("Product cannot be null.");
        }

        return await _productRepository.DeleteAsync(product, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Decides the best persistence method based on availability of the calculated id.
    /// </summary>
    public async Task<Result<ProductPersistenceStrategy>> DeterminePersistenceStrategyAsync(
        Product product,
        int parsedId,
        int dynamicOffset,
        CancellationToken cancellationToken)
    {
        if (product is null)
        {
            return Result<ProductPersistenceStrategy>.WithFailure("Product cannot be null.");
        }

        var proposedId = CalculateAdjustedProductId(parsedId, dynamicOffset);
        var isAvailable = await _productUniquenessValidator
            .IsProductIdAvailableAsync(proposedId, cancellationToken)
            .ConfigureAwait(false);

        var method = isAvailable ? PersistenceMethod.IntelligentIdAssignment : PersistenceMethod.AutoGeneratedId;
        var rationale = isAvailable ? "Calculated ProductId available" : "Calculated ProductId unavailable";

        return Result<ProductPersistenceStrategy>.Success(new ProductPersistenceStrategy
        {
            Method = method,
            ProposedProductId = isAvailable ? proposedId : 0,
            Rationale = rationale,
            IsIntelligentAssignment = isAvailable
        });
    }

    /// <summary>
    /// Calculates adjusted product id from parsed suffix and dynamic offset.
    /// </summary>
    public int CalculateAdjustedProductId(int parsedId, int dynamicOffset)
    {
        if (parsedId <= 0)
        {
            return 0;
        }

        return parsedId + dynamicOffset;
    }

    /// <summary>
    /// Validates product readiness for persistence.
    /// Ensures all required data is present before attempting persistence.
    /// </summary>
    public Result ValidateProductForPersistence(Product product)
    {
        if (product is null)
        {
            return Result.WithFailure("Product cannot be null for persistence validation.");
        }

        var errors = new List<string>();

        // Core product validation
        if (string.IsNullOrWhiteSpace(product.PartNumber))
        {
            errors.Add("Product PartNumber is required for persistence.");
        }

        if (string.IsNullOrWhiteSpace(product.ProductName))
        {
            errors.Add("Product ProductName is required for persistence.");
        }

        // Customer relationship validation
        if (product.CustomerId <= 0)
        {
            errors.Add("Product must have a valid CustomerId for persistence.");
        }

        if (product.Customer is null)
        {
            errors.Add("Product must have a Customer entity for persistence.");
        }

        // Line relationship validation
        if (product.LineId <= 0)
        {
            errors.Add("Product must have a valid LineId for persistence.");
        }

        if (product.Line is null)
        {
            errors.Add("Product must have a Line entity for persistence.");
        }

        // Audit field validation
        if (string.IsNullOrWhiteSpace(product.CreatedBy))
        {
            errors.Add("Product CreatedBy is required for persistence.");
        }

        return errors.Count > 0
            ? Result.WithFailure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Updates an existing product using intelligent persistence strategy.
    /// Handles product updates while preserving ID assignment logic.
    /// </summary>
    public async Task<Result<Product>> UpdateProductWithIntelligentStrategyAsync(
        Product product,
        ProductInput productInput,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Product>.WithFailure("Operation was canceled.");
        }

        // Null guards for dependencies and parameters
        if (_productRepository is null)
        {
            return Result<Product>.WithFailure("Product repository cannot be null.");
        }

        if (product is null)
        {
            return Result<Product>.WithFailure("Product cannot be null for update.");
        }

        if (productInput is null)
        {
            return Result<Product>.WithFailure("ProductInput cannot be null for update.");
        }

        try
        {
            _logger.LogDebug("Updating product with intelligent strategy. ProductId: {ProductId}", product.ProductId);

            // Update audit fields
            product.ModifiedBy = productInput.CreatedBy ?? product.ModifiedBy;
            product.ModifiedOn = _dateTimeMachine.Now;

            // Execute update
            var updateResult = await _productRepository.UpdateAsync(product, cancellationToken)
                .ConfigureAwait(false);

            if (updateResult.IsFailure)
            {
                _logger.LogError("Product update failed for ProductId: {ProductId}", product.ProductId);
                return Result<Product>.WithFailure(updateResult.Errors);
            }

            _logger.LogDebug("Product update successful. ProductId: {ProductId}", product.ProductId);
            return Result<Product>.Success(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while updating product ProductId: {ProductId}", product.ProductId);
            return Result<Product>.WithFailure($"Exception occurred while updating product: {ex.Message}");
        }
    }
}
