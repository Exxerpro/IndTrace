// <copyright file="GetProductoDetailQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;

namespace Application.UnitTests.Features.Products;

/// <summary>
/// Comprehensive unit tests for GetProductoDetailQueryValidator.
/// Tests ProductId validation rules: GreaterThan(0) and LessThan(100).
/// </summary>
public class GetProductoDetailQueryValidatorTests
{
    private readonly GetProductoDetailQueryValidator _validator = null!;

    /// <summary>
    /// Initializes a new instance of the GetProductoDetailQueryValidatorTests class.
    /// </summary>
    public GetProductoDetailQueryValidatorTests()
    {
        _validator = new GetProductoDetailQueryValidator();
    }

    // Constructor Tests

    /// <summary>
    /// Tests that the validator can be instantiated successfully.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new GetProductoDetailQueryValidator();

        // Assert
        validator.ShouldNotBeNull();
    }

    // ProductId Validation Tests

    /// <summary>
    /// Tests that validation passes with valid ProductId values within the acceptable range.
    /// </summary>
    [Theory]
    [InlineData(1, "Minimum valid ProductId")]
    [InlineData(50, "Mid-range ProductId")]
    [InlineData(99, "Maximum valid ProductId")]
    [InlineData(25, "Quarter range ProductId")]
    [InlineData(75, "Three-quarter range ProductId")]
    public void Validate_WithValidProductId_ShouldPass(int productId, string description)
    {
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = new GetProductDetailQuery { ProductId = productId };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation fails when ProductId is not greater than 0.
    /// </summary>
    [Theory]
    [InlineData(0, "Zero ProductId")]
    [InlineData(-1, "Negative ProductId")]
    [InlineData(-10, "Large negative ProductId")]
    [InlineData(-999, "Very large negative ProductId")]
    public void Validate_WithProductIdNotGreaterThanZero_ShouldFail(int productId, string description)
    {
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = new GetProductDetailQuery { ProductId = productId };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    /// <summary>
    /// Tests that validation fails when ProductId is not less than 100.
    /// </summary>
    [Theory]
    [InlineData(100, "Boundary ProductId at 100")]
    [InlineData(101, "ProductId just over limit")]
    [InlineData(150, "ProductId moderately over limit")]
    [InlineData(999, "Large ProductId")]
    [InlineData(int.MaxValue, "Maximum integer ProductId")]
    public void Validate_WithProductIdNotLessThan100_ShouldFail(int productId, string description)
    {
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: productId, description
        _ = productId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = new GetProductDetailQuery { ProductId = productId };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    /// <summary>
    /// Tests boundary values for ProductId validation.
    /// </summary>
    [Fact]
    public void Validate_WithBoundaryValues_ShouldWorkCorrectly()
    {
        // Test cases for boundary analysis
        var testCases = new[]
        {
            new { ProductId = 0, ExpectedValid = false, Description = "Lower boundary (invalid)" },
            new { ProductId = 1, ExpectedValid = true, Description = "Just above lower boundary (valid)" },
            new { ProductId = 99, ExpectedValid = true, Description = "Just below upper boundary (valid)" },
            new { ProductId = 5080, ExpectedValid = false, Description = "Upper boundary (invalid)" }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var query = new GetProductDetailQuery { ProductId = testCase.ProductId };

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.ShouldBe(testCase.ExpectedValid,
                $"ProductId {testCase.ProductId} - {testCase.Description}");
        }
    }

    // Industrial Manufacturing Scenarios

    /// <summary>
    /// Tests ProductId validation with industrial manufacturing scenarios.
    /// Provides comprehensive test cases using MemberData pattern.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetIndustrialProductIdTestCases))]
    public void Validate_WithIndustrialProductIdScenarios_ShouldWorkCorrectly(
        int productId, bool expectedValid, string scenario)
    {
        // Arrange
        var query = new GetProductDetailQuery { ProductId = productId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBe(expectedValid,
            $"Industrial scenario: {scenario} - ProductId: {productId}");
    }

    /// <summary>
    /// Provides industrial manufacturing test cases for ProductId validation.
    /// Covers typical manufacturing product identification scenarios.
    /// </summary>
    public static IEnumerable<object[]> GetIndustrialProductIdTestCases()
    {
        return new List<object[]>
        {
            // Valid manufacturing product IDs
            new object[] { 1, true, "Single product line starter ID" },
            new object[] { 10, true, "Standard assembly line product ID" },
            new object[] { 25, true, "Automotive component product ID" },
            new object[] { 42, true, "Electronics module product ID" },
            new object[] { 67, true, "Heavy machinery part product ID" },
            new object[] { 88, true, "Quality control sample product ID" },
            new object[] { 99, true, "Maximum production line product ID" },

            // Invalid manufacturing product IDs - too low
            new object[] { 0, false, "Uninitialized product ID" },
            new object[] { -1, false, "Error state product ID" },
            new object[] { -99, false, "System error product ID" },

            // Invalid manufacturing product IDs - too high
            new object[] { 100, false, "Legacy system overflow product ID" },
            new object[] { 150, false, "Out of range assembly product ID" },
            new object[] { 999, false, "Enterprise system product ID" },

            // Edge cases for manufacturing
            new object[] { 1, true, "First production run product" },
            new object[] { 99, true, "Last valid production slot" },
            new object[] { 50, true, "Mid-production line product" }
        };
    }

    // Async Validation Tests

    /// <summary>
    /// Tests that async validation works correctly with valid ProductId.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidProductId_ShouldPass()
    {
        // Arrange
        var query = new GetProductDetailQuery { ProductId = 50 };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that async validation works correctly with invalid ProductId.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithInvalidProductId_ShouldFail()
    {
        // Arrange
        var query = new GetProductDetailQuery { ProductId = 0 };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    /// <summary>
    /// Tests that async validation respects cancellation tokens.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var query = new GetProductDetailQuery { ProductId = 50 };
        using var cts = new CancellationTokenSource();
        // Cancel the token immediately to simulate cancellation
        // Is a non blocking operation so we can cancel right away
        // To test the behavior on cancelation
        // the SUT must not to trow on coded owned byt this repo, but this code is
        // code from Fluen Validation
        // we don 't have
        // and we don't have to teste either
#pragma warning disable
        cts.Cancel();
#pragma  warning restore
        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(query, cts.Token));
    }

    // Multiple Validation Consistency Tests

    /// <summary>
    /// Tests that multiple validation calls with the same data produce consistent results.
    /// </summary>
    [Fact]
    public void Validate_MultipleCallsWithSameData_ShouldBeConsistent()
    {
        // Arrange
        var query = new GetProductDetailQuery { ProductId = 42 };

        // Act
        var result1 = _validator.Validate(query);
        var result2 = _validator.Validate(query);
        var result3 = _validator.Validate(query);

        // Assert
        result1.IsValid.ShouldBe(result2.IsValid);
        result2.IsValid.ShouldBe(result3.IsValid);
        result1.Errors.Count().ShouldBe(result2.Errors.Count);
        result2.Errors.Count().ShouldBe(result3.Errors.Count);
    }

    /// <summary>
    /// Tests validation with different ProductId values in sequence.
    /// </summary>
    [Fact]
    public void Validate_WithSequentialProductIds_ShouldValidateIndependently()
    {
        // Arrange & Act & Assert
        var testSequence = new[] { 1, 50, 99, 0, 100, 25 };
        var expectedResults = new[] { true, true, true, false, false, true };

        for (int i = 0; i < testSequence.Length; i++)
        {
            var query = new GetProductDetailQuery { ProductId = testSequence[i] };
            var result = _validator.Validate(query);

            result.IsValid.ShouldBe(expectedResults[i],
                $"ProductId {testSequence[i]} at index {i}");
        }
    }

    // Edge Case Tests

    /// <summary>
    /// Tests validation with extreme ProductId values and edge cases.
    /// </summary>
    [Theory]
    [InlineData(int.MinValue, false, "Minimum integer value")]
    [InlineData(-1000000, false, "Large negative value")]
    [InlineData(1000000, false, "Large positive value")]
    [InlineData(int.MaxValue, false, "Maximum integer value")]
    public void Validate_WithExtremeProductIdValues_ShouldHandleCorrectly(
        int productId, bool expectedValid, string description)
    {
        // Arrange
        var query = new GetProductDetailQuery { ProductId = productId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBe(expectedValid, $"Extreme value test: {description}");
    }

    /// <summary>
    /// Tests that validation handles null query gracefully.
    /// </summary>
    // MARKED FOR DELETION - Null input validation test no longer needed for Railway-Oriented Programming
    // [Fact]
    // public void Validate_WithNullQuery_ShouldThrowException()
    // {
    //     // Arrange
    //     GetProductDetailQuery query = null!;
    //
    //     // Act & Assert
    //     Should.Throw<ArgumentNullException>(() => _validator.Validate(query));
    // }

    // Manufacturing Range Validation Tests

    /// <summary>
    /// Tests that the ProductId range (1-99) aligns with manufacturing constraints.
    /// Validates the business rule that product IDs must fit within legacy system limits.
    /// </summary>
    [Fact]
    public void Validate_ProductIdRange_ShouldAlignWithManufacturingConstraints()
    {
        // Test the entire valid range to ensure no gaps
        for (int productId = 1; productId <= 99; productId++)
        {
            // Arrange
            var query = new GetProductDetailQuery { ProductId = productId };

            // Act
            //[Fix]
            //CLAUDE
            //Date: 22/08/2025
            //Reason: Pattern A Fix - Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    /// <summary>
    /// Tests that ProductId values outside manufacturing range are consistently rejected.
    /// </summary>
    [Fact]
    public void Validate_ProductIdOutsideRange_ShouldBeConsistentlyRejected()
    {
        // Test values outside the valid range
        var invalidIds = new[] { 0, -1, -50, 100, 101, 150, 200, 999 };

        foreach (var productId in invalidIds)
        {
            // Arrange
            var query = new GetProductDetailQuery { ProductId = productId };

            // Act
            //[Fix]
            //CLAUDE
            //Date: 22/08/2025
            //Reason: Pattern A Fix - Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(query);

            // Assert
            //[Fix]
            //CLAUDE
            //Date: 22/08/2025
            //Reason: Pattern A Fix - Updated to use FluentValidation.TestHelper pattern for modern validation testing
            result.ShouldHaveValidationErrorFor(x => x.ProductId);
        }
    }
}