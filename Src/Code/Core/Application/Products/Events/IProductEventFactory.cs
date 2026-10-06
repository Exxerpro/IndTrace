// <copyright file="IProductEventFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Events;

/// <summary>
/// Product event creation without external dependencies.
/// Handles ProductCreatedEvent generation from Product entities.
/// Domain service - contains zero dependencies, pure event creation logic only.
/// </summary>
public interface IProductEventFactory
{
    /// <summary>
    /// Creates a ProductCreatedEvent from a Product entity.
    /// Transforms domain entity into event payload for notification.
    /// </summary>
    /// <param name="product">Product entity to create event from</param>
    /// <returns>ProductCreatedEvent containing product information</returns>
    /// <remarks>
    /// Event Creation Rules:
    /// - Maps Product properties to event properties
    /// - Handles property name differences (ProductName → Name)
    /// - Ensures null safety for string properties
    /// - Preserves all essential product data for downstream consumers
    /// </remarks>
    Result<ProductCreatedEvent> CreateProductCreatedEvent(Product product);

    /// <summary>
    /// Validates product entity readiness for event creation.
    /// Ensures product has all required data before creating events.
    /// </summary>
    /// <param name="product">Product entity to validate</param>
    /// <returns>Validation result for event creation readiness</returns>
    /// <remarks>
    /// Validation Rules:
    /// - Product must not be null
    /// - ProductId must be assigned (> 0)
    /// - Essential properties must be populated
    /// - CustomerId must be valid
    /// </remarks>
    Result ValidateProductForEventCreation(Product product);

    /// <summary>
    /// Creates event payload with enhanced metadata.
    /// Includes additional context information for event consumers.
    /// </summary>
    /// <param name="product">Source product entity</param>
    /// <param name="creationContext">Additional context for event creation</param>
    /// <returns>Enhanced ProductCreatedEvent with metadata</returns>
    Result<ProductCreatedEvent> CreateEnhancedProductCreatedEvent(Product product, ProductCreationContext creationContext);
}

/// <summary>
/// Context information for enhanced product event creation.
/// Provides additional metadata for event payload enrichment.
/// </summary>
public record ProductCreationContext
{
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string Source { get; init; } = "CreateProductCommandHandler";
    public string Version { get; init; } = "1.0";
    public Dictionary<string, object> AdditionalMetadata { get; init; } = new();
}