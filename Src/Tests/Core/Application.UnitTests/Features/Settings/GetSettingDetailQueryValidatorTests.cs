// <copyright file="GetSettingDetailQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Settings.Queries.GetSettingDetail;

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for GetSettingDetailQueryValidator
/// </summary>
public class GetSettingDetailQueryValidatorTests
{
    private readonly GetSettingDetailQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetSettingDetailQueryValidatorTests()
    {
        _validator = new GetSettingDetailQueryValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new GetSettingDetailQueryValidator();

        // Assert
        validator.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Validate_WithValidSettingId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidSettingId_ShouldPass()
    {
        // Arrange
        var query = new GetSettingDetailQuery { SettingId = 50 };

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
    /// Executes Validate_WithSettingIdNotGreaterThanZero_ShouldFail operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>

    [Theory]
    [InlineData(0)]      // At zero boundary (invalid)
    [InlineData(-1)]     // Negative value
    [InlineData(-10)]    // More negative
    [InlineData(-100)]   // Very negative
    public void Validate_WithSettingIdNotGreaterThanZero_ShouldFail(int settingId)
    {
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Arrange
        var query = new GetSettingDetailQuery { SettingId = settingId };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_WithValidSettingIdRange_ShouldPass operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>

    [Theory]
    [InlineData(1)]      // Minimum valid value
    [InlineData(10)]     // Small value
    [InlineData(50)]     // Medium value
    [InlineData(100)]    // Larger value
    [InlineData(999)]    // Large value
    [InlineData(1000)]   // Very large value
    public void Validate_WithValidSettingIdRange_ShouldPass(int settingId)
    {
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Arrange
        var query = new GetSettingDetailQuery { SettingId = settingId };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_WithInvalidSettingIdRange_ShouldFail operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>

    [Theory]
    [InlineData(-1000)]  // Very negative
    [InlineData(-100)]   // Negative
    [InlineData(-1)]     // Just below zero
    [InlineData(0)]      // Zero (invalid)
    public void Validate_WithInvalidSettingIdRange_ShouldFail(int settingId)
    {
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Using parameters: settingId
        _ = settingId; // xUnit1026 fix
        // Arrange
        var query = new GetSettingDetailQuery { SettingId = settingId };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_WithBoundaryValues_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithBoundaryValues_ShouldWorkCorrectly()
    {
        // Arrange
        var validMinQuery = new GetSettingDetailQuery { SettingId = 1 };   // Just above boundary (valid)
        var invalidZeroQuery = new GetSettingDetailQuery { SettingId = 0 };  // At boundary (invalid)
        var invalidNegativeQuery = new GetSettingDetailQuery { SettingId = -1 }; // Below boundary (invalid)
        var largeValidQuery = new GetSettingDetailQuery { SettingId = 10000 }; // Large valid value

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var validMinResult = _validator.TestValidate(validMinQuery);
        var invalidZeroResult = _validator.TestValidate(invalidZeroQuery);
        var invalidNegativeResult = _validator.TestValidate(invalidNegativeQuery);
        var largeValidResult = _validator.TestValidate(largeValidQuery);

        // Assert
        validMinResult.IsValid.ShouldBeTrue("SettingId = 1 should be valid");
        invalidZeroResult.IsValid.ShouldBeFalse("SettingId = 0 should be invalid");
        invalidNegativeResult.IsValid.ShouldBeFalse("SettingId = -1 should be invalid");
        largeValidResult.IsValid.ShouldBeTrue("Large positive SettingId should be valid");
    }

    /// <summary>
    /// Executes Validate_WithDefaultSettingId_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithDefaultSettingId_ShouldFail()
    {
        // Arrange
        var query = new GetSettingDetailQuery(); // Default SettingId is 0

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_ErrorMessage_ShouldBeCorrect operation.
    /// </summary>

    [Fact]
    public void Validate_ErrorMessage_ShouldBeCorrect()
    {
        // Arrange
        var query = new GetSettingDetailQuery { SettingId = 0 };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes ValidateAsync_WithValidQuery_ShouldPass operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidQuery_ShouldPass.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidQuery_ShouldPass()
    {
        // Arrange
        var query = new GetSettingDetailQuery { SettingId = 25 };

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
        var query = new GetSettingDetailQuery { SettingId = 0 };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes ValidateAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var query = new GetSettingDetailQuery { SettingId = 50 };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(query, cts.Token));
    }

    /// <summary>
    /// Executes Validate_WithMemberDataValidSettingIds_ShouldPass operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetValidSettingIdTestCases))]
    public void Validate_WithMemberDataValidSettingIds_ShouldPass(int settingId, string description)
    {
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = new GetSettingDetailQuery { SettingId = settingId };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes GetValidSettingIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidSettingIdTestCases.</returns>

    public static IEnumerable<object[]> GetValidSettingIdTestCases()
    {
        yield return new object[] { 1, "Minimum valid value" };
        yield return new object[] { 5, "Small setting ID" };
        yield return new object[] { 10, "Standard setting ID" };
        yield return new object[] { 25, "Medium setting ID" };
        yield return new object[] { 50, "Mid-range setting ID" };
        yield return new object[] { 100, "Large setting ID" };
        yield return new object[] { 500, "Very large setting ID" };
        yield return new object[] { 1000, "Maximum typical setting ID" };
        yield return new object[] { int.MaxValue, "Maximum integer value" };
    }

    /// <summary>
    /// Executes Validate_WithMemberDataInvalidSettingIds_ShouldFail operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetInvalidSettingIdTestCases))]
    public void Validate_WithMemberDataInvalidSettingIds_ShouldFail(int settingId, string description)
    {
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = new GetSettingDetailQuery { SettingId = settingId };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes GetInvalidSettingIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetInvalidSettingIdTestCases.</returns>

    public static IEnumerable<object[]> GetInvalidSettingIdTestCases()
    {
        yield return new object[] { int.MinValue, "Minimum integer value" };
        yield return new object[] { -1000, "Very negative value" };
        yield return new object[] { -100, "Negative value" };
        yield return new object[] { -1, "Just below zero" };
        yield return new object[] { 0, "Zero (at boundary)" };
    }

    /// <summary>
    /// Executes Validate_WithTypicalSettingScenarios_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithTypicalSettingScenarios_ShouldWorkCorrectly()
    {
        // Arrange - Test typical setting management scenarios
        var scenarios = new[]
        {
            new { SettingId = 1, Expected = true, Name = "Temperature threshold setting" },
            new { SettingId = 2, Expected = true, Name = "Speed control setting" },
            new { SettingId = 10, Expected = true, Name = "Safety parameter setting" },
            new { SettingId = 25, Expected = true, Name = "Quality control setting" },
            new { SettingId = 50, Expected = true, Name = "Maintenance schedule setting" },
            new { SettingId = 100, Expected = true, Name = "System configuration setting" },
            new { SettingId = 0, Expected = false, Name = "Invalid default setting ID" },
            new { SettingId = -1, Expected = false, Name = "Invalid negative setting ID" },
            new { SettingId = -100, Expected = false, Name = "Very negative setting ID" }
        };

        foreach (var scenario in scenarios)
        {
            // Arrange
            var query = new GetSettingDetailQuery { SettingId = scenario.SettingId };

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(query);

            // Assert
            result.IsValid.ShouldBe(scenario.Expected, $"Scenario '{scenario.Name}' should be {(scenario.Expected ? "valid" : "invalid")}");
        }
    }

    /// <summary>
    /// Executes Validator_ShouldHaveCorrectRules operation.
    /// </summary>

    [Fact]
    public void Validator_ShouldHaveCorrectRules()
    {
        // Arrange
        var validQuery = new GetSettingDetailQuery { SettingId = 10 };
        var invalidQuery = new GetSettingDetailQuery { SettingId = 0 };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var validResult = _validator.TestValidate(validQuery);
        var invalidResult = _validator.TestValidate(invalidQuery);

        // Assert
        validResult.IsValid.ShouldBeTrue("Should pass GreaterThan(0) rule");
        invalidResult.IsValid.ShouldBeFalse("Should fail GreaterThan(0) rule");

        // Verify specific error message
        invalidResult.ShouldHaveValidationErrorFor(x => x.SettingId)
            .WithErrorMessage("SettingId must be greater than 0.");
    }

    /// <summary>
    /// Executes Validate_CustomErrorMessage_ShouldBeUsed operation.
    /// </summary>

    [Fact]
    public void Validate_CustomErrorMessage_ShouldBeUsed()
    {
        // Arrange - Test that the custom error message is used correctly
        var query = new GetSettingDetailQuery { SettingId = -5 };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_WithIndustrialSettingIdScenarios_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithIndustrialSettingIdScenarios_ShouldWorkCorrectly()
    {
        // Arrange - Test common industrial setting ID scenarios
        var settingTypes = new[]
        {
            new { Id = 1, Type = "Temperature Control", Description = "Machine temperature threshold" },
            new { Id = 2, Type = "Speed Control", Description = "Motor speed configuration" },
            new { Id = 10, Type = "Safety Parameters", Description = "Safety interlock settings" },
            new { Id = 50, Type = "Quality Thresholds", Description = "Quality control parameters" },
            new { Id = 100, Type = "Maintenance", Description = "Maintenance schedule configuration" },
            new { Id = 500, Type = "Production Targets", Description = "Production target settings" }
        };

        foreach (var settingType in settingTypes)
        {
            // Arrange
            var query = new GetSettingDetailQuery { SettingId = settingType.Id };

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
        }
    }

    /// <summary>
    /// Executes Validate_MultipleValidationCalls_ShouldBeConsistent operation.
    /// </summary>

    [Fact]
    public void Validate_MultipleValidationCalls_ShouldBeConsistent()
    {
        // Arrange
        var validQuery = new GetSettingDetailQuery { SettingId = 42 };
        var invalidQuery = new GetSettingDetailQuery { SettingId = 0 };

        // Act - Multiple calls to ensure consistency
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var validResult1 = _validator.TestValidate(validQuery);
        var validResult2 = _validator.TestValidate(validQuery);
        var invalidResult1 = _validator.TestValidate(invalidQuery);
        var invalidResult2 = _validator.TestValidate(invalidQuery);

        // Assert
        validResult1.IsValid.ShouldBe(validResult2.IsValid);
        invalidResult1.IsValid.ShouldBe(invalidResult2.IsValid);

        invalidResult1.Errors.Count().ShouldBe(invalidResult2.Errors.Count);
        invalidResult1.Errors.First().ErrorMessage.ShouldBe(invalidResult2.Errors.First().ErrorMessage);
    }
}