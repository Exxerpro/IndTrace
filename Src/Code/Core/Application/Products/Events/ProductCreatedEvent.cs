// <copyright file="ProductCreatedEvent.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Models.Interfaces;
using IndTrace.Application.Notifications.Models;

namespace IndTrace.Application.Products.Events;

/// <summary>
/// Domain event raised when a Product is successfully created.
/// Pure POCO event containing product data for downstream consumers.
/// </summary>
public record ProductCreatedEvent : INotification
{
    public int ProductId { get; init; }
    public string PartNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty; // Maps from ProductName
    public string Description { get; init; } = string.Empty;
    public string CustomerPartNumber { get; init; } = string.Empty;
    public string AliasPartNumber { get; init; } = string.Empty;
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int LineId { get; init; }
    public int IsActive { get; init; }
    public int Version { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedOn { get; init; }
    public string ModifiedBy { get; init; } = string.Empty;
    public DateTime ModifiedOn { get; init; }
    public int RuleId { get; init; }

    /// <summary>
    /// Creates a ProductCreatedEvent from a Product entity.
    /// Handles property name mapping (ProductName → Name) and null safety.
    /// </summary>
    /// <param name="product">Product entity to create event from</param>
    /// <param name="auditFallback">Deterministic timestamp supplied by the caller (which owns the injected
    /// <c>IDateTimeMachine</c>) used only when the product's <c>CreatedOn</c>/<c>ModifiedOn</c> are null.
    /// Issue #85: the event no longer fabricates its own <c>DateTime.UtcNow</c> — the clock is read by the caller.</param>
    /// <returns>Result containing ProductCreatedEvent or failure</returns>
    public static Result<ProductCreatedEvent> FromProduct(Product product, DateTime auditFallback)
    {
        if (product is null)
        {
            return Result<ProductCreatedEvent>.WithFailure("Product cannot be null for event creation.");
        }

        var eventData = new ProductCreatedEvent
        {
            ProductId = product.ProductId.Value,
            PartNumber = product.PartNumber ?? string.Empty,
            Name = product.ProductName ?? string.Empty, // Key mapping: ProductName → Name
            Description = product.Description ?? string.Empty,
            CustomerPartNumber = product.CustomerPartNumber ?? string.Empty,
            AliasPartNumber = product.AliasPartNumber ?? string.Empty,
            CustomerId = product.CustomerId,
            CustomerName = product.CustomerName ?? string.Empty,
            LineId = product.LineId,
            IsActive = product.IsActive.Value,
            Version = product.Version,
            CreatedBy = product.CreatedBy ?? string.Empty,
            CreatedOn = product.CreatedOn ?? auditFallback,
            ModifiedBy = product.ModifiedBy ?? string.Empty,
            ModifiedOn = product.ModifiedOn ?? auditFallback,
            RuleId = product.RuleId
        };

        return Result<ProductCreatedEvent>.Success(eventData);
    }

    /// <summary>
    /// Creates a ProductCreatedEvent from a Product entity.
    /// Handles property name mapping (ProductName → Name) and null safety.
    /// </summary>
    /// <param name="productCreated">Product entity to create event from</param>
    /// <returns>Result containing ProductCreatedEvent or failure</returns>
    public static Result<Product> ToProduct(ProductCreatedEvent productCreated)
    {
        if (productCreated is null)
        {
            return Result<Product>.WithFailure("Product cannot be null for event creation.");
        }

        // Story 2.1 (#26): construct through the guarded Product.Create factory. ProductId (identity) and
        // audit fields are applied after construction. For all valid data this is byte-identical; a null
        // identity string now surfaces as a graceful failure Result rather than a malformed entity.
        var createResult = Product.Create(
            productCreated.PartNumber ?? string.Empty,
            productCreated.Name ?? string.Empty, // Key mapping: ProductName → Name

            // Normalize the event int back onto the tri-state entity status (positive -> Active),
            // avoiding the implicit (ActiveStatus)int cast that would yield Invalid for values >= 2.
            productCreated.IsActive > 0 ? ActiveStatus.Active : (productCreated.IsActive < 0 ? ActiveStatus.Inactive : ActiveStatus.None),
            productCreated.Version,
            productCreated.CustomerPartNumber ?? string.Empty,
            productCreated.AliasPartNumber ?? string.Empty,
            productCreated.Description ?? string.Empty,
            productCreated.CustomerId,
            productCreated.CustomerName ?? string.Empty,
            productCreated.LineId,
            productCreated.RuleId);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        var eventData = createResult.Value;
        if (eventData is null)
        {
            return Result<Product>.WithFailure("Product construction produced a null entity.");
        }

        eventData.ProductId = new ProductId(productCreated.ProductId);
        eventData.CreatedBy = productCreated.CreatedBy ?? string.Empty;
        eventData.CreatedOn = productCreated.CreatedOn;
        eventData.ModifiedBy = productCreated.ModifiedBy ?? string.Empty;
        eventData.ModifiedOn = productCreated.ModifiedOn;

        return Result<Product>.Success(eventData);
    }
    /// <summary>
    /// Handler for ProductCreatedEvent.
    /// </summary>
    public class ProductCreatedHandler : INotificationHandler<ProductCreatedEvent>
    {
        private INotificationService notificationService;

        /// <summary>
        /// Constructor with dependency injection for notification service.
        /// </summary>
        /// <param name="notificationService"></param>
        public ProductCreatedHandler(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        /// <summary>
        /// Processes the ProductCreatedEvent.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result> Process(ProductCreatedEvent @event, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.WithFailure("Operation was canceled.");
            }

            // Create a notification message using the factory method
            var message = MessageDto.CreateMessage<ProductCreatedEvent>(@event);

            var result = await notificationService.SendAsync(message, cancellationToken);
            return result;
        }

        /// <summary>
        /// Processes the ProductCreatedEvent.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        Task<Result> INotificationHandler<ProductCreatedEvent>.Process(ProductCreatedEvent @event, CancellationToken cancellationToken)
        {
            return Process(@event, cancellationToken);
        }
    }
}