// <copyright file="ProductCreatedEventTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Events;
using IndTrace.Application.Models.Interfaces;
using IndTrace.Application.Notifications.Models;

namespace Application.UnitTests.Features.Products;

/// <summary>
/// Unit tests for ProductCreatedEvent
/// </summary>
public class ProductCreatedEventTests
{
    // Issue #85: FromProduct no longer reads DateTime.UtcNow; the caller supplies a deterministic
    // audit fallback used when the product's audit fields are null. Tests pass this fixed instant.
    private static readonly DateTime AuditFallback = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Executes Constructor_WithDefaultConstructor_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultConstructor_ShouldCreateInstanceWithDefaultValues()
    {
        // Act
        var productEvent = new ProductCreatedEvent();

        // Assert
        productEvent.ShouldNotBeNull();
        productEvent.ProductId.ShouldBe(0);
        productEvent.CustomerId.ShouldBe(0);
        productEvent.PartNumber.ShouldBe(string.Empty);
        productEvent.Name.ShouldBe(string.Empty);
        productEvent.IsActive.ShouldBe(0);
        productEvent.Version.ShouldBe(0);
        //productEvent.PartNumberCustomer.ShouldBe(string.Empty);
        //productEvent.AliasNoParte.ShouldBe(string.Empty);
        productEvent.Description.ShouldBeNullOrEmpty();
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var productEvent = new ProductCreatedEvent
        {
            ProductId = 12345,
            CustomerId = 100,
            PartNumber = "PART-ABC-123",
            Name = "Industrial Widget",
            IsActive = 1,
            Version = 2,
            CustomerPartNumber = "CUST-PART-456",
            AliasPartNumber = "ALIAS-789",
            Description = "High-precision industrial component"
        };

        // Assert
        productEvent.ProductId.ShouldBe(12345);
        productEvent.CustomerId.ShouldBe(100);
        productEvent.PartNumber.ShouldBe("PART-ABC-123");
        productEvent.Name.ShouldBe("Industrial Widget");
        productEvent.IsActive.ShouldBe(1);
        productEvent.Version.ShouldBe(2);
        productEvent.CustomerPartNumber.ShouldBe("CUST-PART-456");
        productEvent.AliasPartNumber.ShouldBe("ALIAS-789");
        productEvent.Description.ShouldBe("High-precision industrial component");
    }

    /// <summary>
    /// Executes ProductCreatedEvent_ShouldImplementINotification operation.
    /// </summary>

    [Fact]
    public void ProductCreatedEvent_ShouldImplementINotification()
    {
        // Arrange & Act
        var productEvent = new ProductCreatedEvent();

        // Assert
        productEvent.ShouldBeAssignableTo<INotification>();
    }

    // FromProduct Static Method Tests
    /// <summary>
    /// Executes FromProduct_WithNullProduct_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void FromProduct_WithNullProduct_ShouldReturnFailureResult()
    {
        // Arrange
        Product? nullProduct = null!;

        // Act
        var result = ProductCreatedEvent.FromProduct(nullProduct!, AuditFallback);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Product cannot be null for event creation.");
    }

    /// <summary>
    /// Executes FromProduct_WithValidProduct_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void FromProduct_WithValidProduct_ShouldMapAllProperties()
    {
        // Arrange
        var product = Product.CreateFixture(
            productId: 12345,
            customerId: 100,
            partNumber: "PART-ABC-123",
            productName: "Industrial Widget",
            isActive: 1,
            version: 2,
            customerPartNumber: "CUST-PART-456",
            aliasPartNumber: "ALIAS-789",
            description: "High-precision industrial component");

        // Act
        var productEventWrapper = ProductCreatedEvent.FromProduct(product, AuditFallback);

        // Assert
        productEventWrapper.IsSuccess.ShouldBeTrue();
        productEventWrapper.Value.ShouldNotBeNull();
        var productEvent = productEventWrapper.Value;
        productEvent.ShouldNotBeNull();
        productEvent.ShouldNotBeNull();
        productEvent.ShouldNotBeNull();
        productEvent.ProductId.ShouldBe(12345);
        productEvent.CustomerId.ShouldBe(100);
        productEvent.PartNumber.ShouldBe("PART-ABC-123");
        productEvent.Name.ShouldBe("Industrial Widget");
        productEvent.IsActive.ShouldBe(1);
        productEvent.Version.ShouldBe(2);
        productEvent.CustomerPartNumber.ShouldBe("CUST-PART-456");
        productEvent.AliasPartNumber.ShouldBe("ALIAS-789");
        productEvent.Description.ShouldBe("High-precision industrial component");
    }

    /// <summary>
    /// Executes FromProduct_WithMinimalProduct_ShouldMapBasicProperties operation.
    /// </summary>

    [Fact]
    public void FromProduct_WithMinimalProduct_ShouldMapBasicProperties()
    {
        // Arrange
        var product = Product.CreateFixture(
            productId: 1,
            partNumber: "MIN-PART",
            productName: "Minimal Product");

        // Act
        var productEventWrapper = ProductCreatedEvent.FromProduct(product, AuditFallback);

        // Assert
        productEventWrapper.IsSuccess.ShouldBeTrue();
        productEventWrapper.Value.ShouldNotBeNull();
        var productEvent = productEventWrapper.Value;
        productEvent.ShouldNotBeNull();
        productEvent.ShouldNotBeNull();
        productEvent.ShouldNotBeNull();
        productEvent.ProductId.ShouldBe(1);
        productEvent.PartNumber.ShouldBe("MIN-PART");
        productEvent.Name.ShouldBe("Minimal Product");
        productEvent.CustomerId.ShouldBe(0);
        productEvent.IsActive.ShouldBe(0);
        productEvent.Version.ShouldBe(0);
    }

    /// <summary>
    /// Executes FromProduct_WithNullStringProperties_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void FromProduct_WithNullStringProperties_ShouldHandleGracefully()
    {
        // Arrange
        var product = Product.CreateFixture(
            productId: 123,
            partNumber: null!,
            productName: null!,
            customerPartNumber: null!,
            aliasPartNumber: null!,
            description: null!);

        // Act
        var productEventWrapper = ProductCreatedEvent.FromProduct(product, AuditFallback);

        // Assert
        productEventWrapper.IsSuccess.ShouldBeTrue();
        productEventWrapper.Value.ShouldNotBeNull();
        var productEvent = productEventWrapper.Value;
        productEvent.ShouldNotBeNull();
        productEvent.ShouldNotBeNull();
        productEvent.ShouldNotBeNull();
        productEvent.ProductId.ShouldBe(123);
        productEvent.PartNumber.ShouldBe(string.Empty);
        productEvent.Name.ShouldBe(string.Empty);
        productEvent.CustomerId.ShouldBe(0);
        productEvent.IsActive.ShouldBe(0);
        productEvent.Version.ShouldBe(0);
    }

    // ToProduct Static Method Tests
    /// <summary>
    /// Executes ToProduct_WithNullProductCreatedEvent_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToProduct_WithNullProductCreatedEvent_ShouldReturnFailureResult()
    {
        // Arrange
        ProductCreatedEvent? nullEvent = null!;

        // Act
        var result = ProductCreatedEvent.ToProduct(nullEvent!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Product cannot be null for event creation.");
    }

    /// <summary>
    /// Executes ToProduct_WithValidProductCreatedEvent_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToProduct_WithValidProductCreatedEvent_ShouldMapAllProperties()
    {
        // Arrange
        var productEvent = new ProductCreatedEvent
        {
            ProductId = 12345,
            CustomerId = 100,
            PartNumber = "PART-ABC-123",
            Name = "Industrial Widget",
            IsActive = 1,
            Version = 2,
            CustomerPartNumber = "CUST-PART-456",
            AliasPartNumber = "ALIAS-789",
            Description = "High-precision industrial component"
        };

        // Act
        var product = ProductCreatedEvent.ToProduct(productEvent);

        // Assert
        product.ShouldNotBeNull();
        var value = product.Value.ShouldNotBeNull();
        value.ProductId.Value.ShouldBe(12345);
        value.CustomerId.ShouldBe(100);
        value.PartNumber.ShouldBe("PART-ABC-123");
        value.ProductName.ShouldBe("Industrial Widget");
        value.IsActive.Value.ShouldBe(1);
        value.Version.ShouldBe(2);
        value.CustomerPartNumber.ShouldBe("CUST-PART-456");
        value.AliasPartNumber.ShouldBe("ALIAS-789");
        value.Description.ShouldBe("High-precision industrial component");
    }

    /// <summary>
    /// Executes ToProduct_WithMinimalProductCreatedEvent_ShouldMapBasicProperties operation.
    /// </summary>

    [Fact]
    public void ToProduct_WithMinimalProductCreatedEvent_ShouldMapBasicProperties()
    {
        // Arrange
        var productEvent = new ProductCreatedEvent
        {
            ProductId = 1,
            PartNumber = "MIN-PART",
            Name = "Minimal Product"
        };

        // Act
        var product = ProductCreatedEvent.ToProduct(productEvent);

        // Assert
        product.ShouldNotBeNull();
        var value = product.Value.ShouldNotBeNull();
        value.ProductId.Value.ShouldBe(1);
        value.PartNumber.ShouldBe("MIN-PART");
        value.ProductName.ShouldBe("Minimal Product");
        value.CustomerId.ShouldBe(0);
        value.IsActive.Value.ShouldBe(0);
        value.Version.ShouldBe(0);
    }

    // Round-trip Conversion Tests
    /// <summary>
    /// Executes FromProduct_ThenToProduct_ShouldPreserveAllProperties operation.
    /// </summary>

    [Fact]
    public void FromProduct_ThenToProduct_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalProduct = Product.CreateFixture(
            productId: 12345,
            customerId: 100,
            partNumber: "ROUND-TRIP-PART",
            productName: "Round Trip Product",
            isActive: 1,
            version: 3,
            customerPartNumber: "RT-CUST-PART",
            aliasPartNumber: "RT-ALIAS",
            description: "Round trip test product");

        // Act
        var productEvent = ProductCreatedEvent.FromProduct(originalProduct, AuditFallback);
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8604] Add null check before using productEvent.Value
        productEvent.Value.ShouldNotBeNull();
        var convertedProduct = ProductCreatedEvent.ToProduct(productEvent.Value);

        // Assert
        var convertedValue = convertedProduct.Value.ShouldNotBeNull();
        convertedValue.ProductId.ShouldBe(originalProduct.ProductId);
        convertedValue.CustomerId.ShouldBe(originalProduct.CustomerId);
        convertedValue.PartNumber.ShouldBe(originalProduct.PartNumber);
        convertedValue.ProductName.ShouldBe(originalProduct.ProductName);
        convertedValue.IsActive.Value.ShouldBe(originalProduct.IsActive.Value);
        convertedValue.Version.ShouldBe(originalProduct.Version);
        convertedValue.CustomerPartNumber.ShouldBe(originalProduct.CustomerPartNumber);
        convertedValue.AliasPartNumber.ShouldBe(originalProduct.AliasPartNumber);
        convertedValue.Description.ShouldBe(originalProduct.Description);
    }

    // Industrial Scenario Tests
    /// <summary>
    /// Executes ProductCreatedEvent_WithIndustrialAutomotiveScenario_ShouldHandleComplexData operation.
    /// </summary>

    [Fact]
    public void ProductCreatedEvent_WithIndustrialAutomotiveScenario_ShouldHandleComplexData()
    {
        // Arrange - Automotive manufacturing scenario
        var productEvent = new ProductCreatedEvent
        {
            ProductId = 501234,
            PartNumber = "AUTO-ENGINE-BLOCK-V8-2024",
            Name = "V8 Engine Block Assembly",
            CustomerId = 1001,
            CustomerPartNumber = "FORD-ENG-V8-2024",
            AliasPartNumber = "F150-ENGINE-BLOCK",
            Description = "High-performance V8 engine block for F-150 series",
            IsActive = 1,
            Version = 5
        };

        // Assert - Verify automotive scenario
        productEvent.PartNumber.ShouldBe("AUTO-ENGINE-BLOCK-V8-2024");
        productEvent.Name.ShouldBe("V8 Engine Block Assembly");
        productEvent.CustomerPartNumber.ShouldBe("FORD-ENG-V8-2024");
        productEvent.AliasPartNumber.ShouldBe("F150-ENGINE-BLOCK");
        productEvent.Description.ShouldContain("F-150 series");
    }

    /// <summary>
    /// Executes ProductCreatedEvent_WithIndustrialElectronicsScenario_ShouldHandleComplexData operation.
    /// </summary>

    [Fact]
    public void ProductCreatedEvent_WithIndustrialElectronicsScenario_ShouldHandleComplexData()
    {
        // Arrange - Electronics manufacturing scenario
        var productEvent = new ProductCreatedEvent
        {
            ProductId = 700456,
            PartNumber = "PCB-MAIN-CTRL-V3.2",
            Name = "Main Control PCB Assembly",
            CustomerId = 2001,
            CustomerPartNumber = "SIEMENS-PCB-CTRL-32",
            AliasPartNumber = "MAIN-BOARD-V3",
            Description = "Primary control board for industrial automation systems",
            IsActive = 1,
            Version = 3
        };

        // Assert - Verify electronics scenario
        productEvent.PartNumber.ShouldBe("PCB-MAIN-CTRL-V3.2");
        productEvent.Name.ShouldBe("Main Control PCB Assembly");
        productEvent.CustomerPartNumber.ShouldBe("SIEMENS-PCB-CTRL-32");
        productEvent.AliasPartNumber.ShouldBe("MAIN-BOARD-V3");
        productEvent.Description.ShouldContain("automation systems");
    }

    // ProductCreatedHandler Tests
    /// <summary>
    /// Executes ProductCreatedHandler_Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void ProductCreatedHandler_Constructor_ShouldCreateInstance()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();

        // Act
        var handler = new ProductCreatedEvent.ProductCreatedHandler(notificationService);

        // Assert
        handler.ShouldNotBeNull();
        handler.ShouldBeAssignableTo<INotificationHandler<ProductCreatedEvent>>();
    }

    /// <summary>
    /// Executes ProductCreatedHandler_Process_ShouldCallNotificationService operation.
    /// </summary>
    /// <returns>The result of ProductCreatedHandler_Process_ShouldCallNotificationService.</returns>

    [Fact]
    public async Task ProductCreatedHandler_Process_ShouldCallNotificationService()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new ProductCreatedEvent.ProductCreatedHandler(notificationService);
        var productEvent = new ProductCreatedEvent
        {
            ProductId = 123,
            PartNumber = "TEST-PART",
            Name = "Test Product"
        };

        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern B Fix - Added mock setup for Railway-Oriented Programming Result<T> pattern
        notificationService.SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await handler.Process(productEvent, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern B Fix - Updated to validate Result<T> pattern
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes ProductCreatedHandler_Process_ShouldRespectCancellationToken operation.
    /// </summary>
    /// <returns>The result of ProductCreatedHandler_Process_ShouldRespectCancellationToken.</returns>

    [Fact]
    public async Task ProductCreatedHandler_Process_ShouldRespectCancellationToken()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new ProductCreatedEvent.ProductCreatedHandler(notificationService);
        var productEvent = new ProductCreatedEvent();
        var cancellationToken = TestContext.Current.CancellationToken;

        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern B Fix - Updated mock setup for Railway-Oriented Programming Result<T> pattern
        notificationService.SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>())
                          .Returns(Result.Success());

        // Act
        var result = await handler.Process(productEvent, cancellationToken);

        // Assert - Should complete without throwing
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern B Fix - Updated to validate Result<T> instead of Task completion
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes ProductCreatedHandler_Process_WithCancelledToken_ShouldHandleGracefully operation.
    /// </summary>
    /// <returns>The result of ProductCreatedHandler_Process_WithCancelledToken_ShouldHandleGracefully.</returns>

    [Fact]
    public async Task ProductCreatedHandler_Process_WithCancelledToken_ShouldHandleGracefully()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new ProductCreatedEvent.ProductCreatedHandler(notificationService);
        var productEvent = new ProductCreatedEvent();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern B Fix - Handler checks for cancellation and returns failure result, no longer throws
        notificationService.SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>())
                          .Returns(Result.Success());

        // Act
        var result = await handler.Process(productEvent, cts.Token);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern B Fix - Handler returns failure result for cancelled token instead of throwing
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation was canceled.");
    }
}