// <copyright file="ProductValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Services;
using IndTrace.Domain.Services.Products;
using Shouldly;
using Xunit;

namespace IndTrace.Domain.UnitTests.Services.Products;

/// <summary>
/// Unit tests for ProductValidator - Pure domain service without external dependencies.
/// Tests business rule validation logic for product data integrity.
/// </summary>
public class ProductValidatorTests
{
    private readonly ProductValidator _validator;

    public ProductValidatorTests()
    {
        _validator = new ProductValidator();
    }

    #region ValidateProductData Tests

    [Fact]
    public void ValidateProductData_ValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var productInput = new ProductInput
        {
            PartNumber = "FORD-F150-001",
            ProductName = "Ford F-150 Test Product",
            CustomerId = 1,
            CustomerName = "Ford Motor",
            LineId = 1,
            Description = "Test product description",
            IsActive = 1,
            Version = 1,
            CreatedBy = "TEST_USER"
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateProductData_NullInput_ShouldReturnFailure()
    {
        // Arrange
        ProductInput? productInput = null;

        // Act
        var result = _validator.ValidateProductData(productInput!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("ProductInput cannot be null.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateProductData_InvalidPartNumber_ShouldReturnFailure(string? partNumber)
    {
        // Arrange
        var productInput = new ProductInput
        {
            PartNumber = partNumber ?? string.Empty,
            ProductName = "Valid Product Name",
            CustomerId = 1,
            CustomerName = "Valid Customer",
            LineId = 1
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("PartNumber is required and cannot be empty.");
    }

    [Fact]
    public void ValidateProductData_PartNumberTooLong_ShouldReturnFailure()
    {
        // Arrange
        var longPartNumber = new string('A', 81); // 101 characters//updates to 81 bussines rule is 80 max chars
        var productInput = new ProductInput
        {
            PartNumber = longPartNumber,
            ProductName = "Valid Product Name",
            CustomerId = 1,
            CustomerName = "Valid Customer",
            LineId = 1
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("PartNumber cannot exceed 80 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateProductData_InvalidProductName_ShouldReturnFailure(string? productName)
    {
        // Arrange
        var productInput = new ProductInput
        {
            PartNumber = "VALID-PART-001",
            ProductName = productName ?? string.Empty,
            CustomerId = 1,
            CustomerName = "Valid Customer",
            LineId = 1
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("ProductName is required and cannot be empty.");
    }

    [Fact]
    public void ValidateProductData_ProductNameTooLong_ShouldReturnFailure()
    {
        // Arrange
        var longProductName = new string('A', 81); // 81 characters — one over the nvarchar(80) column limit
        var productInput = new ProductInput
        {
            PartNumber = "VALID-PART-001",
            ProductName = longProductName,
            CustomerId = 1,
            CustomerName = "Valid Customer",
            LineId = 1
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("ProductName cannot exceed 80 characters.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateProductData_InvalidCustomerId_ShouldReturnFailure(int customerId)
    {
        // Arrange
        var productInput = new ProductInput
        {
            PartNumber = "VALID-PART-001",
            ProductName = "Valid Product Name",
            CustomerId = customerId,
            CustomerName = "Valid Customer",
            LineId = 1
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("CustomerId must be greater than 0.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateProductData_InvalidLineId_ShouldReturnFailure(int lineId)
    {
        // Arrange
        var productInput = new ProductInput
        {
            PartNumber = "VALID-PART-001",
            ProductName = "Valid Product Name",
            CustomerId = 1,
            CustomerName = "Valid Customer",
            LineId = lineId
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("LineId must be greater than 0.");
    }

    [Fact]
    public void ValidateProductData_DescriptionTooLong_ShouldReturnFailure()
    {
        // Arrange
        var longDescription = new string('A', 501); // 501 characters
        var productInput = new ProductInput
        {
            PartNumber = "VALID-PART-001",
            ProductName = "Valid Product Name",
            CustomerId = 1,
            CustomerName = "Valid Customer",
            LineId = 1,
            Description = longDescription
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Description cannot exceed 500 characters.");
    }

    [Fact]
    public void ValidateProductData_CustomerNameTooLong_ShouldReturnFailure()
    {
        // Arrange
        // CustomerName is optional in ValidateProductData, but has length limits when provided
        var longCustomerName = new string('A', 101); // 101 characters
        var productInput = new ProductInput
        {
            PartNumber = "VALID-PART-001",
            ProductName = "Valid Product Name",
            CustomerId = 1,
            CustomerName = longCustomerName,
            LineId = 1
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("CustomerName cannot exceed 100 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateProductData_OptionalCustomerName_ShouldReturnSuccess(string? customerName)
    {
        // Arrange
        // CustomerName is optional in ValidateProductData for dual resolution logic
        var productInput = new ProductInput
        {
            PartNumber = "VALID-PART-001",
            ProductName = "Valid Product Name",
            CustomerId = 1,
            CustomerName = customerName ?? string.Empty,
            LineId = 1
        };

        // Act
        var result = _validator.ValidateProductData(productInput);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    #endregion ValidateProductData Tests

    #region ValidatePartNumberFormat Tests

    [Theory]
    [InlineData("FORD-F150-001")]
    [InlineData("TESLA-MODEL-Y")]
    [InlineData("BMW-X5-2024")]
    [InlineData("ABC123")]
    public void ValidatePartNumberFormat_ValidFormats_ShouldReturnSuccess(string partNumber)
    {
        // Act
        var result = _validator.ValidatePartNumberFormat(partNumber);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidatePartNumberFormat_NullOrEmpty_ShouldReturnFailure(string? partNumber)
    {
        // Act
        var result = _validator.ValidatePartNumberFormat(partNumber ?? string.Empty);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("PartNumber cannot be null, empty, or whitespace.");
    }

    [Theory]
    [InlineData("AB")] // Too short
    [InlineData("A")]  // Too short
    public void ValidatePartNumberFormat_TooShort_ShouldReturnFailure(string partNumber)
    {
        // Act
        var result = _validator.ValidatePartNumberFormat(partNumber);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("PartNumber must be at least 3 characters long.");
    }

    [Fact]
    public void ValidatePartNumberFormat_TooLong_ShouldReturnFailure()
    {
        // Arrange
        var longPartNumber = new string('A', 81); // 81 characters

        // Act
        var result = _validator.ValidatePartNumberFormat(longPartNumber);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("PartNumber cannot exceed 80 characters.");
    }

    [Theory]
    [InlineData("FORD@F150")] // Invalid character @
    [InlineData("FORD#F150")] // Invalid character #
    [InlineData("FORD F150")] // Space not allowed
    [InlineData("FORD.F150")] // Period not allowed
    public void ValidatePartNumberFormat_InvalidCharacters_ShouldReturnFailure(string partNumber)
    {
        // Act
        var result = _validator.ValidatePartNumberFormat(partNumber);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("PartNumber can only contain letters, numbers, and hyphens.");
    }

    [Theory]
    [InlineData("-FORD-F150")] // Starts with hyphen
    [InlineData("FORD-F150-")] // Ends with hyphen
    public void ValidatePartNumberFormat_InvalidHyphenPlacement_ShouldReturnFailure(string partNumber)
    {
        // Act
        var result = _validator.ValidatePartNumberFormat(partNumber);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("PartNumber cannot start or end with a hyphen.");
    }

    [Fact]
    public void ValidatePartNumberFormat_ConsecutiveHyphens_ShouldReturnFailure()
    {
        // Arrange
        var partNumber = "FORD--F150"; // Double hyphen

        // Act
        var result = _validator.ValidatePartNumberFormat(partNumber);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("PartNumber cannot contain consecutive hyphens.");
    }

    #endregion ValidatePartNumberFormat Tests

    #region ValidateCustomerData Tests

    [Fact]
    public void ValidateCustomerData_ValidData_ShouldReturnSuccess()
    {
        // Act
        var result = _validator.ValidateCustomerData(1, "Ford Motor");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateCustomerData_InvalidCustomerId_ShouldReturnFailure(int customerId)
    {
        // Act
        var result = _validator.ValidateCustomerData(customerId, "Valid Customer");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("CustomerId must be greater than 0.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateCustomerData_InvalidCustomerName_ShouldReturnFailure(string? customerName)
    {
        // Act
        var result = _validator.ValidateCustomerData(1, customerName ?? string.Empty);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("CustomerName is required when validating customer data.");
    }

    [Theory]
    [InlineData("TBD")]
    [InlineData("TODO")]
    [InlineData("UNKNOWN")]
    [InlineData("N/A")]
    [InlineData("NA")]
    [InlineData("tbd")] // Case insensitive
    public void ValidateCustomerData_PlaceholderNames_ShouldReturnFailure(string customerName)
    {
        // Act
        var result = _validator.ValidateCustomerData(1, customerName);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"CustomerName '{customerName}' is not a valid customer name.");
    }

    #endregion ValidateCustomerData Tests

    #region ValidateLineAssignment Tests

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public void ValidateLineAssignment_ValidLineId_ShouldReturnSuccess(int lineId)
    {
        // Act
        var result = _validator.ValidateLineAssignment(lineId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateLineAssignment_InvalidLineId_ShouldReturnFailure(int lineId)
    {
        // Act
        var result = _validator.ValidateLineAssignment(lineId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("LineId must be greater than 0.");
    }

    [Fact]
    public void ValidateLineAssignment_LineIdTooLarge_ShouldReturnFailure()
    {
        // Arrange
        const int lineId = 10000; // Exceeds maximum of 9999

        // Act
        var result = _validator.ValidateLineAssignment(lineId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("LineId exceeds maximum allowed value of 9999.");
    }

    #endregion ValidateLineAssignment Tests
}