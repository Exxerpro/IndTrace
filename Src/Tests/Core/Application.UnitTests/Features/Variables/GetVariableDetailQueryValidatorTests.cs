// <copyright file="GetVariableDetailQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Unit tests for GetVariableDetailQueryValidator
/// </summary>
public class GetVariableDetailQueryValidatorTests
{
    //[Fix] 
    //CLAUDE
    //Date: 21/08/2025 
    //Reason: Updated to use FluentValidation.TestHelper pattern and added specific error message assertions

    private readonly GetVariableDetailQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetVariableDetailQueryValidatorTests()
    {
        _validator = new GetVariableDetailQueryValidator();
    }
    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new GetVariableDetailQueryValidator();

        // Assert
        validator.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Validate_WithValidId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidId_ShouldPass()
    {
        // Arrange
        var query = new GetVariableDetailQuery { Id = 50 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Validate_WithIdNotGreaterThanZero_ShouldFail operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
    [InlineData(0)]      // At zero boundary (invalid)
    [InlineData(-1)]     // Negative value
    [InlineData(-10)]    // More negative
    [InlineData(-100)]   // Very negative
    public void Validate_WithIdNotGreaterThanZero_ShouldFail(int id)
    {
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Arrange
        var query = new GetVariableDetailQuery { Id = id };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            //[Fix] 
            //CLAUDE
            //Date: 21/08/2025 
            //Reason: Updated error message expectation to match current validator implementation
            .WithErrorMessage("Variable ID must be greater than 0.");
    }
    /// <summary>
    /// Executes Validate_WithValidIdRange_ShouldPass operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
    [InlineData(1)]      // Minimum valid value
    [InlineData(10)]     // Small value
    [InlineData(50)]     // Medium value
    [InlineData(100)]    // Larger value
    [InlineData(999)]    // Large value
    [InlineData(1000)]   // Very large value
    public void Validate_WithValidIdRange_ShouldPass(int id)
    {
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Arrange
        var query = new GetVariableDetailQuery { Id = id };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Validate_WithInvalidIdRange_ShouldFail operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
    [InlineData(-1000)]  // Very negative
    [InlineData(-100)]   // Negative
    [InlineData(-1)]     // Just below zero
    [InlineData(0)]      // Zero (invalid)
    public void Validate_WithInvalidIdRange_ShouldFail(int id)
    {
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Arrange
        var query = new GetVariableDetailQuery { Id = id };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            //[Fix] 
            //CLAUDE
            //Date: 21/08/2025 
            //Reason: Updated error message expectation to match current validator implementation
            .WithErrorMessage("Variable ID must be greater than 0.");
    }
    /// <summary>
    /// Executes Validate_WithBoundaryValues_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithBoundaryValues_ShouldWorkCorrectly()
    {
        // Arrange
        var validMinQuery = new GetVariableDetailQuery { Id = 1 };   // Just above boundary (valid)
        var invalidZeroQuery = new GetVariableDetailQuery { Id = 0 };  // At boundary (invalid)
        var invalidNegativeQuery = new GetVariableDetailQuery { Id = -1 }; // Below boundary (invalid)
        var largeValidQuery = new GetVariableDetailQuery { Id = 10000 }; // Large valid value

        // Act
        var validMinResult = _validator.TestValidate(validMinQuery);
        var invalidZeroResult = _validator.TestValidate(invalidZeroQuery);
        var invalidNegativeResult = _validator.TestValidate(invalidNegativeQuery);
        var largeValidResult = _validator.TestValidate(largeValidQuery);

        // Assert
        validMinResult.ShouldNotHaveAnyValidationErrors();
        invalidZeroResult.ShouldHaveValidationErrorFor(x => x.Id);
        invalidNegativeResult.ShouldHaveValidationErrorFor(x => x.Id);
        largeValidResult.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Validate_WithDefaultId_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithDefaultId_ShouldFail()
    {
        // Arrange
        var query = new GetVariableDetailQuery(); // Default UserId is 0

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            //[Fix] 
            //CLAUDE
            //Date: 21/08/2025 
            //Reason: Updated error message expectation to match current validator implementation
            .WithErrorMessage("Variable ID must be greater than 0.");
    }
    /// <summary>
    /// Executes Validate_ErrorMessage_ShouldBeCorrect operation.
    /// </summary>

    [Fact]
    public void Validate_ErrorMessage_ShouldBeCorrect()
    {
        // Arrange
        var query = new GetVariableDetailQuery { Id = 0 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            //[Fix] 
            //CLAUDE
            //Date: 21/08/2025 
            //Reason: Updated error message expectation to match current validator implementation
            .WithErrorMessage("Variable ID must be greater than 0.");
    }
    /// <summary>
    /// Executes ValidateAsync_WithValidQuery_ShouldPass operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidQuery_ShouldPass.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidQuery_ShouldPass()
    {
        // Arrange
        var query = new GetVariableDetailQuery { Id = 25 };

        // Act
        var result = await _validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes ValidateAsync_WithInvalidQuery_ShouldFail operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithInvalidQuery_ShouldFail.</returns>

    [Fact]
    public async Task ValidateAsync_WithInvalidQuery_ShouldFail()
    {
        // Arrange
        var query = new GetVariableDetailQuery { Id = 0 };

        // Act
        var result = await _validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Fixed error message expectation - validator returns "Variable ID" not "RecipeId"
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Variable ID must be greater than 0.");
    }
    /// <summary>
    /// Executes ValidateAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var query = new GetVariableDetailQuery { Id = 50 };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(query, cts.Token));
    }
    /// <summary>
    /// Executes Validate_WithMemberDataValidIds_ShouldPass operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetValidIdTestCases))]
    public void Validate_WithMemberDataValidIds_ShouldPass(int id, string description)
    {
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = new GetVariableDetailQuery { Id = id };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes GetValidIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidIdTestCases.</returns>

    public static IEnumerable<object[]> GetValidIdTestCases()
    {
        yield return new object[] { 1, "Minimum valid value" };
        yield return new object[] { 5, "Small variable ID" };
        yield return new object[] { 10, "Standard variable ID" };
        yield return new object[] { 25, "Medium variable ID" };
        yield return new object[] { 50, "Mid-range variable ID" };
        yield return new object[] { 100, "Large variable ID" };
        yield return new object[] { 500, "Very large variable ID" };
        yield return new object[] { 1000, "Maximum typical variable ID" };
        yield return new object[] { int.MaxValue, "Maximum integer value" };
    }
    /// <summary>
    /// Executes Validate_WithMemberDataInvalidIds_ShouldFail operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetInvalidIdTestCases))]
    public void Validate_WithMemberDataInvalidIds_ShouldFail(int id, string description)
    {
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = new GetVariableDetailQuery { Id = id };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            //[Fix] 
            //CLAUDE
            //Date: 21/08/2025 
            //Reason: Updated error message expectation to match current validator implementation
            .WithErrorMessage("Variable ID must be greater than 0.");
    }
    /// <summary>
    /// Executes GetInvalidIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetInvalidIdTestCases.</returns>

    public static IEnumerable<object[]> GetInvalidIdTestCases()
    {
        yield return new object[] { int.MinValue, "Minimum integer value" };
        yield return new object[] { -1000, "Very negative value" };
        yield return new object[] { -100, "Negative value" };
        yield return new object[] { -1, "Just below zero" };
        yield return new object[] { 0, "Zero (at boundary)" };
    }
    /// <summary>
    /// Executes Validate_WithTypicalVariableScenarios_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithTypicalVariableScenarios_ShouldWorkCorrectly()
    {
        // Arrange - Test typical variable management scenarios
        var scenarios = new[]
        {
            new { Id = 1, Expected = true, Name = "First variable in system" },
            new { Id = 10, Expected = true, Name = "Standard variable ID" },
            new { Id = 25, Expected = true, Name = "Mid-range variable ID" },
            new { Id = 100, Expected = true, Name = "High variable ID" },
            new { Id = 999, Expected = true, Name = "Maximum typical variable ID" },
            new { Id = 0, Expected = false, Name = "Invalid default variable ID" },
            new { Id = -1, Expected = false, Name = "Invalid negative variable ID" },
            new { Id = -100, Expected = false, Name = "Very negative variable ID" }
        };

        foreach (var scenario in scenarios)
        {
            // Arrange
            var query = new GetVariableDetailQuery { Id = scenario.Id };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            if (scenario.Expected)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.Id);
            }
        }
    }
    /// <summary>
    /// Executes Validator_ShouldHaveCorrectRules operation.
    /// </summary>

    [Fact]
    public void Validator_ShouldHaveCorrectRules()
    {
        // Arrange
        var validQuery = new GetVariableDetailQuery { Id = 10 };
        var invalidQuery = new GetVariableDetailQuery { Id = 0 };

        // Act
        var validResult = _validator.TestValidate(validQuery);
        var invalidResult = _validator.TestValidate(invalidQuery);

        // Assert
        validResult.ShouldNotHaveAnyValidationErrors();
        invalidResult.ShouldHaveValidationErrorFor(x => x.Id)
            //[Fix] 
            //CLAUDE
            //Date: 21/08/2025 
            //Reason: Updated error message expectation to match current validator implementation
            .WithErrorMessage("Variable ID must be greater than 0.");
    }
    /// <summary>
    /// Executes Validate_CustomErrorMessage_ShouldBeUsed operation.
    /// </summary>

    [Fact]
    public void Validate_CustomErrorMessage_ShouldBeUsed()
    {
        // Arrange - Test that the custom error message is used correctly
        var query = new GetVariableDetailQuery { Id = -5 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            //[Fix] 
            //CLAUDE
            //Date: 21/08/2025 
            //Reason: Updated error message expectation to match current validator implementation
            .WithErrorMessage("Variable ID must be greater than 0.");
    }
    /// <summary>
    /// Executes Validate_WithIndustrialVariableIdScenarios_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithIndustrialVariableIdScenarios_ShouldWorkCorrectly()
    {
        // Arrange - Test common industrial variable ID scenarios
        var variableTypes = new[]
        {
            new { Id = 1, Type = "Temperature Sensor", Description = "Primary temperature variable" },
            new { Id = 2, Type = "Pressure Sensor", Description = "Primary pressure variable" },
            new { Id = 10, Type = "Speed Control", Description = "Motor speed variable" },
            new { Id = 50, Type = "Position Feedback", Description = "Actuator position variable" },
            new { Id = 100, Type = "Status Register", Description = "System status variable" },
            new { Id = 500, Type = "Configuration", Description = "Configuration parameter variable" }
        };

        foreach (var variable in variableTypes)
        {
            // Arrange
            var query = new GetVariableDetailQuery { Id = variable.Id };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
    /// <summary>
    /// Executes Validate_MultipleValidationCalls_ShouldBeConsistent operation.
    /// </summary>

    [Fact]
    public void Validate_MultipleValidationCalls_ShouldBeConsistent()
    {
        // Arrange
        var validQuery = new GetVariableDetailQuery { Id = 42 };
        var invalidQuery = new GetVariableDetailQuery { Id = 0 };

        // Act - Multiple calls to ensure consistency
        var validResult1 = _validator.TestValidate(validQuery);
        var validResult2 = _validator.TestValidate(validQuery);
        var invalidResult1 = _validator.TestValidate(invalidQuery);
        var invalidResult2 = _validator.TestValidate(invalidQuery);

        // Assert
        validResult1.ShouldNotHaveAnyValidationErrors();
        validResult2.ShouldNotHaveAnyValidationErrors();
        invalidResult1.ShouldHaveValidationErrorFor(x => x.Id);
        invalidResult2.ShouldHaveValidationErrorFor(x => x.Id);
    }
}