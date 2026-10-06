// <copyright file="ProductEventFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Events;

/// <summary>
/// Product event creation. Uses the injected deterministic clock (issue #85) only to supply an audit fallback
/// timestamp to <see cref="ProductCreatedEvent.FromProduct"/> when a product's audit fields are null.
/// </summary>
public class ProductEventFactory : IProductEventFactory
{
    private readonly IDateTimeMachine _dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductEventFactory"/> class.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic time source used as the audit fallback when a product's
    /// <c>CreatedOn</c>/<c>ModifiedOn</c> are null — never <c>DateTime.UtcNow</c>.</param>
    public ProductEventFactory(IDateTimeMachine dateTimeMachine)
    {
        _dateTimeMachine = dateTimeMachine ?? throw new ArgumentNullException(nameof(dateTimeMachine));
    }

    /// <summary>
    /// Creates a ProductCreatedEvent from a Product entity.
    /// Leverages existing ProductCreatedEvent.FromProduct method for consistency.
    /// </summary>
    public Result<ProductCreatedEvent> CreateProductCreatedEvent(Product product)
    {
        if (product is null)
        {
            return Result<ProductCreatedEvent>.WithFailure("Product cannot be null for event creation.");
        }

        // Use the existing static factory method which already handles:
        // - Null checks
        // - Property mapping (ProductName → Name)
        // - String null safety (null → string.Empty)
        // - All required transformations
        // The audit fallback is sourced from the injected clock (never DateTime.UtcNow).
        return ProductCreatedEvent.FromProduct(product, _dateTimeMachine.UtcNow);
    }

    /// <summary>
    /// Validates product entity readiness for event creation.
    /// Ensures product has all required data before creating events.
    /// </summary>
    public Result ValidateProductForEventCreation(Product product)
    {
        if (product is null)
        {
            return Result.WithFailure("Product cannot be null.");
        }

        var errors = new List<string>();

        // ProductId must be assigned (greater than 0)
        if (product.ProductId.Value <= 0)
        {
            errors.Add("ProductId must be assigned and greater than 0 before creating events.");
        }

        // Essential properties validation
        if (string.IsNullOrWhiteSpace(product.PartNumber))
        {
            errors.Add("Product PartNumber is required for event creation.");
        }

        if (string.IsNullOrWhiteSpace(product.ProductName))
        {
            errors.Add("Product ProductName is required for event creation.");
        }

        // CustomerId must be valid
        if (product.CustomerId <= 0)
        {
            errors.Add("Product must have a valid CustomerId for event creation.");
        }

        // Ensure product is in a valid state
        if (product.IsActive.Value < 0)
        {
            errors.Add("Product IsActive status must be valid (0 or greater).");
        }

        return errors.Count > 0 
            ? Result.WithFailure(errors) 
            : Result.Success();
    }

    /// <summary>
    /// Creates event payload with enhanced metadata.
    /// Validates product state before creating events to ensure data integrity.
    /// </summary>
    public Result<ProductCreatedEvent> CreateEnhancedProductCreatedEvent(Product product, ProductCreationContext context)
    {
        if (product is null)
        {
            return Result<ProductCreatedEvent>.WithFailure("Product cannot be null for enhanced event creation.");
        }

        if (context is null)
        {
            return Result<ProductCreatedEvent>.WithFailure("ProductCreationContext cannot be null.");
        }

        // Validate product state before creating events
        var validationResult = ValidateProductForEventCreation(product);
        if (validationResult.IsFailure)
        {
            return Result<ProductCreatedEvent>.WithFailure(validationResult.Errors);
        }

        // Create event after validation passes
        var eventResult = CreateProductCreatedEvent(product);
        
        if (eventResult.IsFailure)
        {
            return eventResult;
        }

        // Future: Enrich event with context metadata
        // eventResult.Value.Metadata = context.AdditionalMetadata;
        // eventResult.Value.CreatedBy = context.CreatedBy;
        // eventResult.Value.Source = context.Source;

        return eventResult;
    }
}