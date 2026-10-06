// <copyright file="GetShiftDetailQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for GetShiftDetailQueryValidator
/// </summary>
public class GetShiftDetailQueryValidatorTests
{
    private readonly GetShiftDetailQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetShiftDetailQueryValidatorTests()
    {
        _validator = new GetShiftDetailQueryValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new GetShiftDetailQueryValidator();

        // Assert
        validator.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Validate_WithValidShiftId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidShiftId_ShouldPass()
    {
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = 50 };

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
    /// Executes Validate_WithShiftIdNotGreaterThanZero_ShouldFail operation.
    /// </summary>
    /// <param name="shiftId">The shiftId.</param>

    [Theory]
    [InlineData(0)]      // At zero boundary (invalid)
    [InlineData(-1)]     // Negative value
    [InlineData(-10)]    // More negative
    public void Validate_WithShiftIdNotGreaterThanZero_ShouldFail(int shiftId)
    {
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = shiftId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetShiftDetailQuery.ShiftId));
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("greater than") || e.ErrorMessage.Contains("'0'"));
    }

    /// <summary>
    /// Executes Validate_WithShiftIdNotLessThanHundred_ShouldFail operation.
    /// </summary>
    /// <param name="shiftId">The shiftId.</param>

    [Theory]
    [InlineData(100)]    // At hundred boundary (invalid)
    [InlineData(101)]    // Just above hundred
    [InlineData(999)]    // Much larger value
    public void Validate_WithShiftIdNotLessThanHundred_ShouldFail(int shiftId)
    {
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = shiftId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetShiftDetailQuery.ShiftId));
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("less than") || e.ErrorMessage.Contains("'100'"));
    }

    /// <summary>
    /// Executes Validate_WithValidShiftIdRange_ShouldPass operation.
    /// </summary>
    /// <param name="shiftId">The shiftId.</param>

    [Theory]
    [InlineData(1)]      // Minimum valid value
    [InlineData(50)]     // Middle value
    [InlineData(99)]     // Maximum valid value
    [InlineData(25)]     // Quarter range
    [InlineData(75)]     // Three-quarter range
    public void Validate_WithValidShiftIdRange_ShouldPass(int shiftId)
    {
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = shiftId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue($"ShiftId {shiftId} should be valid (1-99 range)");
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Validate_WithInvalidShiftIdRange_ShouldFail operation.
    /// </summary>
    /// <param name="shiftId">The shiftId.</param>

    [Theory]
    [InlineData(-100)]   // Very negative
    [InlineData(0)]      // Zero
    [InlineData(100)]    // At upper boundary
    [InlineData(200)]    // Double the boundary
    [InlineData(1000)]   // Much larger
    public void Validate_WithInvalidShiftIdRange_ShouldFail(int shiftId)
    {
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = shiftId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse($"ShiftId {shiftId} should be invalid (outside 1-99 range)");
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetShiftDetailQuery.ShiftId));
    }

    /// <summary>
    /// Executes Validate_WithBoundaryValues_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithBoundaryValues_ShouldWorkCorrectly()
    {
        // Arrange
        var validMinQuery = new GetShiftDetailQuery { ShiftId = 1 };   // Just above lower boundary
        var validMaxQuery = new GetShiftDetailQuery { ShiftId = 99 };  // Just below upper boundary
        var invalidLowQuery = new GetShiftDetailQuery { ShiftId = 0 };  // At lower boundary (invalid)
        var invalidHighQuery = new GetShiftDetailQuery { ShiftId = 100 }; // At upper boundary (invalid)

        // Act
        var validMinResult = _validator.Validate(validMinQuery);
        var validMaxResult = _validator.Validate(validMaxQuery);
        var invalidLowResult = _validator.Validate(invalidLowQuery);
        var invalidHighResult = _validator.Validate(invalidHighQuery);

        // Assert
        validMinResult.IsValid.ShouldBeTrue("ShiftId = 1 should be valid");
        validMaxResult.IsValid.ShouldBeTrue("ShiftId = 99 should be valid");
        invalidLowResult.IsValid.ShouldBeFalse("ShiftId = 0 should be invalid");
        invalidHighResult.IsValid.ShouldBeFalse("ShiftId = 100 should be invalid");
    }

    /// <summary>
    /// Executes Validate_WithDefaultShiftId_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithDefaultShiftId_ShouldFail()
    {
        // Arrange
        var query = new GetShiftDetailQuery(); // Default ShiftId is 0

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetShiftDetailQuery.ShiftId));
    }

    /// <summary>
    /// Executes ValidateAsync_WithValidQuery_ShouldPass operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidQuery_ShouldPass.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidQuery_ShouldPass()
    {
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = 25 };

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
    /// Executes ValidateAsync_WithInvalidQuery_ShouldFail operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithInvalidQuery_ShouldFail.</returns>

    [Fact]
    public async Task ValidateAsync_WithInvalidQuery_ShouldFail()
    {
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = 150 };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShiftId);
    }

    /// <summary>
    /// Executes ValidateAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = 50 };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(query, cts.Token));
    }

    /// <summary>
    /// Executes Validate_WithMemberDataValidShiftIds_ShouldPass operation.
    /// </summary>
    /// <param name="shiftId">The shiftId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetValidShiftIdTestCases))]
    public void Validate_WithMemberDataValidShiftIds_ShouldPass(int shiftId, string description)
    {
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = shiftId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue($"ShiftId {shiftId} ({description}) should be valid");
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes GetValidShiftIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidShiftIdTestCases.</returns>

    public static IEnumerable<object[]> GetValidShiftIdTestCases()
    {
        yield return new object[] { 1, "Minimum valid value" };
        yield return new object[] { 10, "Early shift ID" };
        yield return new object[] { 25, "Quarter range" };
        yield return new object[] { 50, "Middle range" };
        yield return new object[] { 75, "Three-quarter range" };
        yield return new object[] { 90, "Late shift ID" };
        yield return new object[] { 99, "Maximum valid value" };
    }

    /// <summary>
    /// Executes Validate_WithMemberDataInvalidShiftIds_ShouldFail operation.
    /// </summary>
    /// <param name="shiftId">The shiftId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetInvalidShiftIdTestCases))]
    public void Validate_WithMemberDataInvalidShiftIds_ShouldFail(int shiftId, string description)
    {
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: shiftId, description
        _ = shiftId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = new GetShiftDetailQuery { ShiftId = shiftId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse($"ShiftId {shiftId} ({description}) should be invalid");
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetShiftDetailQuery.ShiftId));
    }

    /// <summary>
    /// Executes GetInvalidShiftIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetInvalidShiftIdTestCases.</returns>

    public static IEnumerable<object[]> GetInvalidShiftIdTestCases()
    {
        yield return new object[] { -100, "Very negative value" };
        yield return new object[] { -1, "Just below zero" };
        yield return new object[] { 0, "Zero (at lower boundary)" };
        yield return new object[] { 100, "At upper boundary" };
        yield return new object[] { 101, "Just above upper boundary" };
        yield return new object[] { 500, "Much larger value" };
        yield return new object[] { int.MaxValue, "Maximum integer value" };
        yield return new object[] { int.MinValue, "Minimum integer value" };
    }

    /// <summary>
    /// Executes Validate_WithTypicalShiftScenarios_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithTypicalShiftScenarios_ShouldWorkCorrectly()
    {
        // Arrange - Test typical shift management scenarios
        var scenarios = new[]
        {
            new { ShiftId = 1, Expected = true, Name = "Day shift (first shift)" },
            new { ShiftId = 2, Expected = true, Name = "Evening shift (second shift)" },
            new { ShiftId = 3, Expected = true, Name = "Night shift (third shift)" },
            new { ShiftId = 10, Expected = true, Name = "Weekly schedule shift" },
            new { ShiftId = 31, Expected = true, Name = "Monthly shift rotation" },
            new { ShiftId = 99, Expected = true, Name = "Maximum shifts in system" },
            new { ShiftId = 0, Expected = false, Name = "Invalid default shift" },
            new { ShiftId = 100, Expected = false, Name = "Shift ID exceeding system limit" },
            new { ShiftId = -1, Expected = false, Name = "Invalid negative shift ID" }
        };

        foreach (var scenario in scenarios)
        {
            // Arrange
            var query = new GetShiftDetailQuery { ShiftId = scenario.ShiftId };

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.ShouldBe(scenario.Expected, $"Scenario '{scenario.Name}' should be {(scenario.Expected ? "valid" : "invalid")}");
        }
    }

    /// <summary>
    /// Executes Validate_ShouldNotValidateOtherProperties operation.
    /// </summary>

    [Fact]
    public void Validate_ShouldNotValidateOtherProperties()
    {
        // Arrange - Test that other properties are not validated
        var query = new GetShiftDetailQuery
        {
            ShiftId = 50, // Valid
            TimeRequest = default(DateTime) // Other property with default value
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue("Only ShiftId should be validated, other properties should be ignored");
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Validator_ShouldHaveCorrectRules operation.
    /// </summary>

    [Fact]
    public void Validator_ShouldHaveCorrectRules()
    {
        // Arrange
        var query1 = new GetShiftDetailQuery { ShiftId = 0 };   // Should fail GreaterThan(0)
        var query2 = new GetShiftDetailQuery { ShiftId = 100 }; // Should fail LessThan(100)
        var query3 = new GetShiftDetailQuery { ShiftId = 50 };  // Should pass both rules

        // Act
        var result1 = _validator.Validate(query1);
        var result2 = _validator.Validate(query2);
        var result3 = _validator.Validate(query3);

        // Assert
        result1.IsValid.ShouldBeFalse("Should fail GreaterThan(0) rule");
        result2.IsValid.ShouldBeFalse("Should fail LessThan(100) rule");
        result3.IsValid.ShouldBeTrue("Should pass both GreaterThan(0) and LessThan(100) rules");

        // Verify specific error messages contain expected content
        result1.Errors.ShouldContain(e => e.PropertyName == nameof(GetShiftDetailQuery.ShiftId));
        result2.Errors.ShouldContain(e => e.PropertyName == nameof(GetShiftDetailQuery.ShiftId));
    }
}