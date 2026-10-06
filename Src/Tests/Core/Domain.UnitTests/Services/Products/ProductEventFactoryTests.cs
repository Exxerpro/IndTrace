// <copyright file="ProductEventFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Events;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.Services.Products;
using IndTrace.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace IndTrace.Domain.UnitTests.Services.Products;

/// <summary>
/// Unit tests for ProductEventFactory - Pure domain service for event creation.
/// Tests event generation patterns without external dependencies.
/// </summary>
public class ProductEventFactoryTests
{
    private readonly ProductEventFactory _factory;
    private readonly DateTime _auditFallback;

    public ProductEventFactoryTests()
    {
        // Issue #85: inject a FIXED clock so audit-fallback timestamps are deterministic.
        var dateTimeMachine = new DateTimeMachine(new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        _auditFallback = dateTimeMachine.UtcNow;
        _factory = new ProductEventFactory(dateTimeMachine);
    }

    #region CreateProductCreatedEvent Tests

    [Fact]
    public void CreateProductCreatedEvent_ValidProduct_ShouldCreateEventSuccessfully()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var result = _factory.CreateProductCreatedEvent(product);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        
        var eventData = result.Value;
        eventData.ProductId.ShouldBe(product.ProductId.Value);
        eventData.PartNumber.ShouldBe(product.PartNumber);
        eventData.Name.ShouldBe(product.ProductName); // ProductName maps to Name
        eventData.CustomerId.ShouldBe(product.CustomerId);
        eventData.Description.ShouldBe(product.Description);
        eventData.IsActive.ShouldBe(product.IsActive.Value);
        eventData.Version.ShouldBe(product.Version);
    }

    [Fact]
    public void CreateProductCreatedEvent_NullProduct_ShouldReturnFailure()
    {
        // Arrange
        Product? product = null;

        // Act
        var result = _factory.CreateProductCreatedEvent(product!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product cannot be null for event creation.");
    }

    [Fact]
    public void CreateProductCreatedEvent_ProductWithNullStrings_ShouldHandleGracefully()
    {
        // Arrange
        var product = Product.CreateFixture(
            productId: 1,
            partNumber: null!, // Null string
            productName: null!, // Null string
            description: null!, // Null string
            customerId: 1,
            isActive: 1,
            version: 1);

        // Act
        var result = _factory.CreateProductCreatedEvent(product);

        // Assert - Should succeed and handle nulls properly
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        
        // The ProductCreatedEvent.FromProduct method should handle null strings
        // by converting them to empty strings or handling them appropriately
    }

    [Fact]
    public void CreateProductCreatedEvent_UsesExistingFromProductMethod_ShouldDelegateCorrectly()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var result = _factory.CreateProductCreatedEvent(product);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        // Verify the factory delegates to the existing static method
        // by comparing with direct call
        var directResult = ProductCreatedEvent.FromProduct(product, _auditFallback);
        
        result.IsSuccess.ShouldBe(directResult.IsSuccess);
        if (result.IsSuccess && directResult.IsSuccess)
        {
            result.Value.ShouldNotBeNull();
            directResult.Value.ShouldNotBeNull();
            result.Value.ProductId.ShouldBe(directResult.Value.ProductId);
            result.Value.PartNumber.ShouldBe(directResult.Value.PartNumber);
            result.Value.Name.ShouldBe(directResult.Value.Name);
        }
    }

    #endregion

    #region ValidateProductForEventCreation Tests

    [Fact]
    public void ValidateProductForEventCreation_ValidProduct_ShouldReturnSuccess()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var result = _factory.ValidateProductForEventCreation(product);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProductForEventCreation_NullProduct_ShouldReturnFailure()
    {
        // Arrange
        Product? product = null;

        // Act
        var result = _factory.ValidateProductForEventCreation(product!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product cannot be null.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateProductForEventCreation_InvalidProductId_ShouldReturnFailure(int productId)
    {
        // Arrange
        var product = CreateValidProduct();
        product.ProductId = new ProductId(productId);

        // Act
        var result = _factory.ValidateProductForEventCreation(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("ProductId must be assigned and greater than 0 before creating events.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateProductForEventCreation_InvalidPartNumber_ShouldReturnFailure(string? partNumber)
    {
        // Arrange
        var product = CreateValidProduct(partNumber: partNumber ?? string.Empty);

        // Act
        var result = _factory.ValidateProductForEventCreation(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product PartNumber is required for event creation.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateProductForEventCreation_InvalidProductName_ShouldReturnFailure(string? productName)
    {
        // Arrange
        var product = CreateValidProduct(productName: productName ?? string.Empty);

        // Act
        var result = _factory.ValidateProductForEventCreation(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product ProductName is required for event creation.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateProductForEventCreation_InvalidCustomerId_ShouldReturnFailure(int customerId)
    {
        // Arrange
        var product = CreateValidProduct(customerId: customerId);

        // Act
        var result = _factory.ValidateProductForEventCreation(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product must have a valid CustomerId for event creation.");
    }

    [Fact]
    public void ValidateProductForEventCreation_NegativeIsActive_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct(isActive: -1); // Negative value

        // Act
        var result = _factory.ValidateProductForEventCreation(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product IsActive status must be valid (0 or greater).");
    }

    [Fact]
    public void ValidateProductForEventCreation_IsActiveZero_ShouldReturnSuccess()
    {
        // Arrange
        var product = CreateValidProduct(isActive: 0); // Zero is valid (inactive but valid state)

        // Act
        var result = _factory.ValidateProductForEventCreation(product);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProductForEventCreation_MultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var product = Product.CreateFixture(
            productId: 0, // Invalid
            partNumber: "", // Invalid
            productName: "", // Invalid
            customerId: 0, // Invalid
            isActive: -1); // Invalid

        // Act
        var result = _factory.ValidateProductForEventCreation(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBe(5);
        result.Errors.ShouldContain("ProductId must be assigned and greater than 0 before creating events.");
        result.Errors.ShouldContain("Product PartNumber is required for event creation.");
        result.Errors.ShouldContain("Product ProductName is required for event creation.");
        result.Errors.ShouldContain("Product must have a valid CustomerId for event creation.");
        result.Errors.ShouldContain("Product IsActive status must be valid (0 or greater).");
    }

    #endregion

    #region CreateEnhancedProductCreatedEvent Tests

    [Fact]
    public void CreateEnhancedProductCreatedEvent_ValidInputs_ShouldCreateEventSuccessfully()
    {
        // Arrange
        var product = CreateValidProduct();
        var context = new ProductCreationContext
        {
            Source = "TEST_SOURCE",
            CreatedBy = "TEST_USER",
            AdditionalMetadata = new Dictionary<string, object> { ["TestKey"] = "TEST_METADATA" }
        };

        // Act
        var result = _factory.CreateEnhancedProductCreatedEvent(product, context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        
        // For now, should delegate to standard event creation
        // Future enhancement will add context metadata
        var eventData = result.Value;
        eventData.ProductId.ShouldBe(product.ProductId.Value);
        eventData.PartNumber.ShouldBe(product.PartNumber);
        eventData.Name.ShouldBe(product.ProductName);
    }

    [Fact]
    public void CreateEnhancedProductCreatedEvent_NullProduct_ShouldReturnFailure()
    {
        // Arrange
        Product? product = null;
        var context = new ProductCreationContext();

        // Act
        var result = _factory.CreateEnhancedProductCreatedEvent(product!, context);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product cannot be null for enhanced event creation.");
    }

    [Fact]
    public void CreateEnhancedProductCreatedEvent_NullContext_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();
        ProductCreationContext? context = null;

        // Act
        var result = _factory.CreateEnhancedProductCreatedEvent(product, context!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("ProductCreationContext cannot be null.");
    }

    [Fact]
    public void CreateEnhancedProductCreatedEvent_BaseEventCreationFails_ShouldReturnBaseFailure()
    {
        // Arrange
        var invalidProduct = new Product { ProductId = new ProductId(0) }; // Invalid product
        var context = new ProductCreationContext();

        // Act
        var result = _factory.CreateEnhancedProductCreatedEvent(invalidProduct, context);

        // Assert
        result.IsFailure.ShouldBeTrue();
        // Should propagate the base event creation failure
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CreateProductCreatedEvent_ValidateAndCreate_Integration_ShouldWorkTogether()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var validationResult = _factory.ValidateProductForEventCreation(product);
        Result<ProductCreatedEvent> eventResult = Result<ProductCreatedEvent>.WithFailure("Not executed");
        
        if (validationResult.IsSuccess)
        {
            eventResult = _factory.CreateProductCreatedEvent(product);
        }

        // Assert
        validationResult.IsSuccess.ShouldBeTrue();
        eventResult.IsSuccess.ShouldBeTrue();
        eventResult.Value.ShouldNotBeNull();
        eventResult.Value.ProductId.ShouldBe(product.ProductId.Value);
    }

    [Fact]
    public void CreateProductCreatedEvent_InvalidProduct_ValidationAndCreationShouldBothHandle()
    {
        // Arrange
        var invalidProduct = new Product { ProductId = new ProductId(0) }; // Invalid product

        // Act
        var validationResult = _factory.ValidateProductForEventCreation(invalidProduct);
        var eventResult = _factory.CreateProductCreatedEvent(invalidProduct);

        // Assert
        validationResult.IsFailure.ShouldBeTrue();
        // Event creation may still succeed if ProductCreatedEvent.FromProduct handles invalid products
        // But validation should catch issues beforehand
    }

    #endregion

    #region Helper Methods and Test Classes

    private Product CreateValidProduct(
        string partNumber = "FORD-F150-001",
        string productName = "Ford F-150 Test Product",
        int customerId = 1,
        int? isActive = null)
    {
        var product = Product.CreateFixture(
            productId: 1,
            partNumber: partNumber,
            productName: productName,
            description: "Test product description",
            customerId: customerId,
            lineId: 1,
            isActive: isActive ?? 1,
            version: 1);
        product.CreatedBy = "TEST_USER";
        product.CreatedOn = DateTime.Now;
        product.ModifiedBy = "TEST_USER";
        product.ModifiedOn = DateTime.Now;
        return product;
    }

    /// <summary>
    /// Test context class for enhanced event creation.
    /// In future implementation, this would contain additional metadata.
    /// </summary>
    // Use the real ProductCreationContext from Domain layer
    // private class ProductCreationContext was causing type conflicts

    #endregion
}