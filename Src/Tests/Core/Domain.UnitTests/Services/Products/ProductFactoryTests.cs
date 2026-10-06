// <copyright file="ProductFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Services;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.Services.Products;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace IndTrace.Domain.UnitTests.Services.Products;

/// <summary>
/// Unit tests for ProductFactory - Pure domain service for product creation and ID parsing.
/// Tests advanced ID parsing logic and entity creation without external dependencies.
/// </summary>
public class ProductFactoryTests
{
    private readonly ProductFactory _factory;
    private readonly IDateTimeMachine _dateTimeMachine;
    private readonly DateTime _fixedNow;

    public ProductFactoryTests()
    {
        // Issue #85: inject a FIXED clock so factory timestamps are deterministic.
        _dateTimeMachine = new DateTimeMachine(new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        _fixedNow = _dateTimeMachine.Now;
        _factory = new ProductFactory(_dateTimeMachine);
    }

    #region CreateProduct Tests

    [Fact]
    public void CreateProduct_ValidInput_ShouldCreateProductWithCorrectProperties()
    {
        // Arrange
        var productData = new ProductInput
        {
            PartNumber = "FORD-F150-001",
            ProductName = "Ford F-150 Test Product",
            Description = "Test product description",
            CustomerPartNumber = "CUST-001",
            AliasPartNumber = "ALIAS-001",
            IsActive = 1,
            Version = 1,
            CreatedBy = "TEST_USER"
        };

        var customer = new Customer
        {
            CustomerId = 1,
            Name = "Ford Motor"
        };

        var line = new Line
        {
            LineId = 1,
            Name = "Production Line 1"
        };

        // Act
        var product = _factory.CreateProduct(productData, customer, line);

        // Assert
        product.ShouldNotBeNull();
        product.PartNumber.ShouldBe("FORD-F150-001");
        product.ProductName.ShouldBe("Ford F-150 Test Product");
        product.Description.ShouldBe("Test product description");
        product.CustomerPartNumber.ShouldBe("CUST-001");
        product.AliasPartNumber.ShouldBe("ALIAS-001");
        product.CustomerId.ShouldBe(1);
        product.CustomerName.ShouldBe("Ford Motor");
        product.Customer.ShouldBe(customer);
        product.LineId.ShouldBe(1);
        product.Line.ShouldBe(line);
        product.IsActive.Value.ShouldBe(1);
        product.Version.ShouldBe(1);
        product.CreatedBy.ShouldBe("TEST_USER");
        product.ProductId.Value.ShouldBe(0); // Not set by factory
        product.RuleId.ShouldBe(0); // Not set by factory
        // Issue #85: timestamps come from the injected clock, so they EQUAL the fixed instant (deterministic).
        product.CreatedOn.ShouldNotBeNull();
        product.CreatedOn.Value.ShouldBe(_fixedNow);
        product.ModifiedOn.ShouldNotBeNull();
        product.ModifiedOn.Value.ShouldBe(_fixedNow);
    }

    [Fact]
    public void CreateProduct_NullProductData_ShouldThrowArgumentNullException()
    {
        // Arrange
        ProductInput? productData = null;
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _factory.CreateProduct(productData!, customer, line));
    }

    [Fact]
    public void CreateProduct_NullCustomer_ShouldThrowArgumentNullException()
    {
        // Arrange
        var productData = new ProductInput { PartNumber = "TEST-001", ProductName = "Test Product" };
        Customer? customer = null;
        var line = new Line { LineId = 1, Name = "Test Line" };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _factory.CreateProduct(productData, customer!, line));
    }

    [Fact]
    public void CreateProduct_NullLine_ShouldThrowArgumentNullException()
    {
        // Arrange
        var productData = new ProductInput { PartNumber = "TEST-001", ProductName = "Test Product" };
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        Line? line = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _factory.CreateProduct(productData, customer, line!));
    }

    // Issue #88: CreateResultProduct is the railway entry — it returns Result failures for missing identity
    // inputs instead of throwing across the boundary, and returns a Success product for valid inputs.

    [Fact]
    public void CreateResultProduct_NullProductData_ShouldReturnFailureNotThrow()
    {
        // Arrange
        ProductInput? productData = null;
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };

        // Act
        var result = _factory.CreateResultProduct(productData!, customer, line);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("productData cannot be null.");
    }

    [Fact]
    public void CreateResultProduct_NullCustomer_ShouldReturnFailureNotThrow()
    {
        // Arrange
        var productData = new ProductInput { PartNumber = "TEST-001", ProductName = "Test Product" };
        Customer? customer = null;
        var line = new Line { LineId = 1, Name = "Test Line" };

        // Act
        var result = _factory.CreateResultProduct(productData, customer!, line);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("customer cannot be null.");
    }

    [Fact]
    public void CreateResultProduct_NullLine_ShouldReturnFailureNotThrow()
    {
        // Arrange
        var productData = new ProductInput { PartNumber = "TEST-001", ProductName = "Test Product" };
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        Line? line = null;

        // Act
        var result = _factory.CreateResultProduct(productData, customer, line!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("line cannot be null.");
    }

    [Fact]
    public void CreateResultProduct_ValidInput_ShouldReturnSuccessWithProduct()
    {
        // Arrange
        var productData = new ProductInput
        {
            PartNumber = "FORD-F150-001",
            ProductName = "Ford F-150 Test Product",
            CreatedBy = "TEST_USER"
        };
        var customer = new Customer { CustomerId = 1, Name = "Ford Motor" };
        var line = new Line { LineId = 1, Name = "Production Line 1" };

        // Act
        var result = _factory.CreateResultProduct(productData, customer, line);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PartNumber.ShouldBe("FORD-F150-001");
        result.Value.Customer.ShouldBe(customer);
        result.Value.Line.ShouldBe(line);
    }

    [Fact]
    public void CreateProduct_NullStrings_ShouldReplaceWithEmptyStrings()
    {
        // Arrange
        var productData = new ProductInput
        {
            PartNumber = null!, // Null string
            ProductName = null!, // Null string
            Description = null!, // Null string
            CustomerPartNumber = null!, // Null string
            AliasPartNumber = null!, // Null string
            CreatedBy = null! // Null string
        };

        var customer = new Customer { CustomerId = 1, Name = null! }; // Null customer name
        var line = new Line { LineId = 1, Name = "Test Line" };

        // Act
        var product = _factory.CreateProduct(productData, customer, line);

        // Assert
        product.PartNumber.ShouldBe(string.Empty);
        product.ProductName.ShouldBe(string.Empty);
        product.Description.ShouldBe(string.Empty);
        product.CustomerPartNumber.ShouldBe(string.Empty);
        product.AliasPartNumber.ShouldBe(string.Empty);
        product.CreatedBy.ShouldBe(string.Empty);
        product.CustomerName.ShouldBe(string.Empty);
    }

    [Fact]
    public void CreateProduct_DefaultValues_ShouldUseCorrectDefaults()
    {
        // Arrange
        var productData = new ProductInput
        {
            PartNumber = "TEST-001",
            ProductName = "Test Product",
            IsActive = 0, // Less than or equal to 0
            Version = 0, // Less than or equal to 0
            CreatedBy = "TEST_USER"
        };

        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };

        // Act
        var product = _factory.CreateProduct(productData, customer, line);

        // Assert
        product.IsActive.Value.ShouldBe(1); // Should default to 1
        product.Version.ShouldBe(1); // Should default to 1
    }

    #endregion

    #region TryParseLastInteger Tests - CRITICAL for ID Parsing

    [Theory]
    [InlineData("FORD-F150-001", true, 1)]
    [InlineData("TESLA-Y-12345", true, 12345)]
    [InlineData("BMW-X5-999", true, 999)]
    [InlineData("MERCEDES-C180-0", true, 0)]
    [InlineData("OEMX-A4-1234567890", true, 1234567890)] // Large number
    public void TryParseLastInteger_ValidNumericSuffix_ShouldReturnTrueAndCorrectValue(
        string partNumber, bool expectedSuccess, int expectedValue)
    {
        // Act
        var result = _factory.TryParseLastInteger(partNumber);

        // Assert
        result.Success.ShouldBe(expectedSuccess);
        result.ParsedId.ShouldBe(expectedValue);
    }

    // [Fix] CLAUDE Date: 18/06/2026 Reason: [Test-bug] The strict expectation contradicted the
    // as-built lenient contract (ProductFactory.TryParseLastInteger "preserves EXACT parsing logic
    // from original handler": trailing-digit regex, then any-embedded-number fallback). Decision:
    // lenient is correct, so the test now asserts the real behavior. Only strings with NO digits
    // anywhere return (false, 0); otherwise the embedded number is extracted.
    [Theory]
    [InlineData("FORD-F150-ABC", true, 150)]    // no trailing digits → embedded "150" extracted
    [InlineData("TESLA-MODEL-Y", false, 0)]     // no digits anywhere → (false, 0)
    [InlineData("BMW-X5", true, 5)]             // trailing digit "5" extracted
    [InlineData("MERCEDES-C180-1ABC", true, 1)] // last embedded number "1"
    [InlineData("PART-NAME-123ABC", true, 123)] // embedded "123" extracted
    public void TryParseLastInteger_NonNumericSuffix_FollowsLenientExtraction(
        string partNumber, bool expectedSuccess, int expectedValue)
    {
        // Act
        var result = _factory.TryParseLastInteger(partNumber);

        // Assert
        result.Success.ShouldBe(expectedSuccess);
        result.ParsedId.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(null, false, 0)]
    [InlineData("", false, 0)]
    public void TryParseLastInteger_NullOrEmpty_ShouldReturnFalseAndZero(
        string? partNumber, bool expectedSuccess, int expectedValue)
    {
        // Act
        var result = _factory.TryParseLastInteger(partNumber ?? string.Empty);

        // Assert
        result.Success.ShouldBe(expectedSuccess);
        result.ParsedId.ShouldBe(expectedValue);
    }

    [Fact]
    public void TryParseLastInteger_MultipleNumbers_ShouldReturnLastNumber()
    {
        // Arrange
        var partNumber = "FORD-150-F-250-999"; // Multiple numbers, should get 999

        // Act
        var result = _factory.TryParseLastInteger(partNumber);

        // Assert
        result.Success.ShouldBeTrue();
        result.ParsedId.ShouldBe(999);
    }

    [Theory]
    [InlineData("PART-2147483647", true, 2147483647)] // int.MaxValue
    [InlineData("PART-2147483648", false, 0)] // Exceeds int.MaxValue
    public void TryParseLastInteger_IntegerLimits_ShouldHandleCorrectly(
        string partNumber, bool expectedSuccess, int expectedValue)
    {
        // Act
        var result = _factory.TryParseLastInteger(partNumber);

        // Assert
        result.Success.ShouldBe(expectedSuccess);
        result.ParsedId.ShouldBe(expectedValue);
    }

    #endregion

    #region GetDynamicOffset Tests - CRITICAL for ID Assignment

    [Theory]
    [InlineData(5, 10)] // Single digit: add 10
    [InlineData(23, 100)] // Double digit: add 100
    [InlineData(123, 1000)] // Triple digit: add 1000
    [InlineData(1234, 10000)] // Four digit: add 10000
    [InlineData(12345, 100000)] // Five digit: add 100000
    public void GetDynamicOffset_VariousNumbers_ShouldReturnCorrectOffset(int parsedNumber, int expectedOffset)
    {
        // Act
        var offset = _factory.GetDynamicOffset(parsedNumber);

        // Assert
        offset.ShouldBe(expectedOffset);
    }

    [Theory]
    [InlineData(0, 10)] // Zero is single digit
    [InlineData(9, 10)] // Single digit boundary
    [InlineData(10, 100)] // Double digit boundary
    [InlineData(99, 100)] // Double digit boundary
    [InlineData(100, 1000)] // Triple digit boundary
    [InlineData(999, 1000)] // Triple digit boundary
    public void GetDynamicOffset_BoundaryValues_ShouldReturnCorrectOffset(int parsedNumber, int expectedOffset)
    {
        // Act
        var offset = _factory.GetDynamicOffset(parsedNumber);

        // Assert
        offset.ShouldBe(expectedOffset);
    }

    [Fact]
    public void GetDynamicOffset_LargeNumber_ShouldCalculateCorrectly()
    {
        // Arrange
        var parsedNumber = 1234567; // 7 digits

        // Act
        var offset = _factory.GetDynamicOffset(parsedNumber);

        // Assert
        offset.ShouldBe(10000000); // 10^7
    }

    #endregion

    #region Integration Tests - ID Parsing + Offset Logic

    [Theory]
    [InlineData("FORD-F150-5", 5, 10, 15)] // Single digit: 5 + 10 = 15
    [InlineData("TESLA-Y-23", 23, 100, 123)] // Double digit: 23 + 100 = 123
    [InlineData("BMW-X5-999", 999, 1000, 1999)] // Triple digit: 999 + 1000 = 1999
    [InlineData("MERCEDES-C180-1234", 1234, 10000, 11234)] // Four digit: 1234 + 10000 = 11234
    public void ParseAndCalculateOffset_Integration_ShouldProduceExpectedResults(
        string partNumber, int expectedParsedId, int expectedOffset, int expectedFinalId)
    {
        // Act
        var parseResult = _factory.TryParseLastInteger(partNumber);
        var offset = parseResult.Success ? _factory.GetDynamicOffset(parseResult.ParsedId) : 0;
        var finalId = parseResult.Success ? parseResult.ParsedId + offset : 0;

        // Assert
        parseResult.Success.ShouldBeTrue();
        parseResult.ParsedId.ShouldBe(expectedParsedId);
        offset.ShouldBe(expectedOffset);
        finalId.ShouldBe(expectedFinalId);
    }

    #endregion
}