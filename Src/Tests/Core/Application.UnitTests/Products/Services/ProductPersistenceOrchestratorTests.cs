// <copyright file="ProductPersistenceOrchestratorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Domain.Services.Products;
using Meziantou.Extensions.Logging.Xunit;

namespace IndTrace.Application.UnitTests.Products.Services;

/// <summary>
/// Unit tests for ProductPersistenceOrchestrator - Critical intelligent ID assignment logic.
/// Tests the sophisticated persistence strategy with 4-parameter vs 2-parameter AddAsync patterns.
/// </summary>
public class ProductPersistenceOrchestratorTests
{
    private readonly IRepository<Product> _mockProductRepository;
    private readonly IProductFactory _mockProductFactory;
    private readonly IProductUniquenessValidator _mockUniquenessValidator;
    private readonly ILogger<ProductPersistenceOrchestrator> _mockLogger;
    private readonly IDateTimeMachine _mockDateTimeMachine;
    private readonly DateTime _fixedNow;
    private readonly ProductPersistenceOrchestrator _orchestrator;

    public ProductPersistenceOrchestratorTests(ITestOutputHelper output)
    {
        _mockProductRepository = Substitute.For<IRepository<Product>>();
        _mockProductFactory = Substitute.For<IProductFactory>();
        _mockUniquenessValidator = Substitute.For<IProductUniquenessValidator>();
        _mockLogger = XUnitLogger.CreateLogger<ProductPersistenceOrchestrator>(output);
        // Issue #85: use a FIXED clock so audit timestamps are deterministic (a bare Substitute would
        // yield DateTime.MinValue, which the audit base coerces to null).
        _mockDateTimeMachine = new DateTimeMachine(new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        _fixedNow = _mockDateTimeMachine.Now;

        _orchestrator = new ProductPersistenceOrchestrator(
            _mockProductRepository,
            _mockProductFactory,
            _mockUniquenessValidator,
            _mockLogger,
            _mockDateTimeMachine);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new ProductPersistenceOrchestrator(null!, _mockProductFactory, _mockUniquenessValidator, _mockLogger, _mockDateTimeMachine));
    }

    [Fact]
    public void Constructor_NullProductFactory_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new ProductPersistenceOrchestrator(_mockProductRepository, null!, _mockUniquenessValidator, _mockLogger, _mockDateTimeMachine));
    }

    [Fact]
    public void Constructor_NullUniquenessValidator_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new ProductPersistenceOrchestrator(_mockProductRepository, _mockProductFactory, null!, _mockLogger, _mockDateTimeMachine));
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new ProductPersistenceOrchestrator(_mockProductRepository, _mockProductFactory, _mockUniquenessValidator, null!, _mockDateTimeMachine));
    }

    // Issue #85: the injected clock is a required dependency and must be null-guarded.
    [Fact]
    public void Constructor_NullDateTimeMachine_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new ProductPersistenceOrchestrator(_mockProductRepository, _mockProductFactory, _mockUniquenessValidator, _mockLogger, null!));
    }

    #endregion Constructor Tests

    #region ValidateProductForPersistence Tests

    [Fact]
    public void ValidateProductForPersistence_ValidProduct_ShouldReturnSuccess()
    {
        // Arrange
        var product = CreateValidProductForPersistence();

        // Act
        var result = _orchestrator.ValidateProductForPersistence(product);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateProductForPersistence_NullProduct_ShouldReturnFailure()
    {
        // Act
        var result = _orchestrator.ValidateProductForPersistence(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product cannot be null for persistence validation.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateProductForPersistence_InvalidPartNumber_ShouldReturnFailure(string partNumber)
    {
        // Arrange
        var product = CreateValidProductForPersistence(partNumber: partNumber);

        // Act
        var result = _orchestrator.ValidateProductForPersistence(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product PartNumber is required for persistence.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateProductForPersistence_InvalidProductName_ShouldReturnFailure(string productName)
    {
        // Arrange
        var product = CreateValidProductForPersistence(productName: productName);

        // Act
        var result = _orchestrator.ValidateProductForPersistence(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product ProductName is required for persistence.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateProductForPersistence_InvalidCustomerId_ShouldReturnFailure(int customerId)
    {
        // Arrange
        var product = CreateValidProductForPersistence(customerId: customerId);

        // Act
        var result = _orchestrator.ValidateProductForPersistence(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product must have a valid CustomerId for persistence.");
    }

    [Fact]
    public void ValidateProductForPersistence_NullCustomer_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProductForPersistence();

#pragma warning disable cs8604 //testing null response
        product.Customer = null!;
#pragma warning restore cs8625 //testing null response
        // Act
        var result = _orchestrator.ValidateProductForPersistence(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product must have a Customer entity for persistence.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateProductForPersistence_InvalidLineId_ShouldReturnFailure(int lineId)
    {
        // Arrange
        var product = CreateValidProductForPersistence(lineId: lineId);

        // Act
        var result = _orchestrator.ValidateProductForPersistence(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product must have a valid LineId for persistence.");
    }

    [Fact]
    public void ValidateProductForPersistence_NullLine_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProductForPersistence();
#pragma warning disable cs8625 //testing null response
        product.Line = null!;
#pragma warning restore cs8625 //testing null response
        // Act
        var result = _orchestrator.ValidateProductForPersistence(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product must have a Line entity for persistence.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateProductForPersistence_InvalidCreatedBy_ShouldReturnFailure(string createdBy)
    {
        // Arrange
        var product = CreateValidProductForPersistence();
        product.CreatedBy = createdBy;

        // Act
        var result = _orchestrator.ValidateProductForPersistence(product);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product CreatedBy is required for persistence.");
    }

    #endregion ValidateProductForPersistence Tests

    #region UpdateProductWithIntelligentStrategyAsync Tests

    [Fact]
    public async Task UpdateProductWithIntelligentStrategyAsync_ValidProduct_ShouldUpdateSuccessfully()
    {
        // Arrange
        var product = CreateValidProductForPersistence();
        var productInput = CreateTestProductInput("FORD-F150-001");

        _mockProductRepository
            .UpdateAsync(product, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));

        // Act
        var result = await _orchestrator.UpdateProductWithIntelligentStrategyAsync(product, productInput, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(product);

        // Verify audit fields were updated
        product.ModifiedBy.ShouldBe("TEST_USER");
        // Issue #85: ModifiedOn is stamped from the injected clock, so it EQUALS the fixed instant (deterministic).
        product.ModifiedOn.ShouldNotBeNull();
        product.ModifiedOn.Value.ShouldBe(_fixedNow);

        await _mockProductRepository
            .Received(1)
            .UpdateAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProductWithIntelligentStrategyAsync_NullProduct_ShouldReturnFailure()
    {
        // Arrange
        var productInput = CreateTestProductInput("FORD-F150-001");

        // Act
        var result = await _orchestrator.UpdateProductWithIntelligentStrategyAsync(null!, productInput, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product cannot be null for update.");
    }

    [Fact]
    public async Task UpdateProductWithIntelligentStrategyAsync_RepositoryFailure_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProductForPersistence();
        var productInput = CreateTestProductInput("FORD-F150-001");

        _mockProductRepository
            .UpdateAsync(product, Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Update failed"));

        // Act
        var result = await _orchestrator.UpdateProductWithIntelligentStrategyAsync(product, productInput, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Update failed");
    }

    #endregion UpdateProductWithIntelligentStrategyAsync Tests

    #region Helper Methods

    private ProductInput CreateTestProductInput(string partNumber)
    {
        return new ProductInput
        {
            PartNumber = partNumber,
            ProductName = $"Product {partNumber}",
            CustomerId = 1,
            LineId = 1,
            CreatedBy = "TEST_USER"
        };
    }

    private Product CreateValidProductForPersistence(
        string partNumber = "FORD-F150-001",
        string productName = "Ford F-150 Test Product",
        int customerId = 1,
        int lineId = 1)
    {
        var product = Product.CreateFixture(
            productId: 1,
            partNumber: partNumber,
            productName: productName,
            customerId: customerId,
            lineId: lineId);
        product.Customer = new Customer { CustomerId = 1, Name = "Ford Motor" };
        product.Line = new Line { LineId = 1, Name = "Production Line 1" };
        product.CreatedBy = "TEST_USER";
        product.CreatedOn = DateTime.Now;
        product.ModifiedBy = "TEST_USER";
        product.ModifiedOn = DateTime.Now;
        return product;
    }

    #endregion Helper Methods
}
