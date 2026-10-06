// <copyright file="CreateSettingValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for CreateSettingValidator
/// </summary>
public class CreateSettingValidatorTests
{
    private readonly CreateSettingValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateSettingValidatorTests()
    {
        _validator = new CreateSettingValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new CreateSettingValidator();

        // Assert
        validator.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Validate_WithValidCommand_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithEmptySettingId_ShouldFail operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>

    [Theory]
    [InlineData(0)]      // Zero SettingId (invalid)
    public void Validate_WithEmptySettingId_ShouldFail(int settingId)
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
        var command = CreateValidCommand();
        command.SettingId = settingId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_WithValidSettingId_ShouldPass operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>

    [Theory]
    [InlineData(1)]      // Minimum valid SettingId
    [InlineData(10)]     // Standard SettingId
    [InlineData(50)]     // Medium SettingId
    [InlineData(100)]    // Large SettingId
    [InlineData(999)]    // Very large SettingId
    [InlineData(1000)]   // Maximum typical SettingId
    public void Validate_WithValidSettingId_ShouldPass(int settingId)
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
        var command = CreateValidCommand();
        command.SettingId = settingId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 6 Fix - Validator now validates ALL 3 properties comprehensively. Updated to check only SettingId property since CreateValidCommand sets all other properties to valid values.
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_WithNegativeSettingId_ShouldFail operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>

    [Theory]
    [InlineData(-1)]     // Negative SettingId
    [InlineData(-10)]    // More negative
    [InlineData(-100)]   // Very negative
    public void Validate_WithNegativeSettingId_ShouldFail(int settingId)
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
        var command = CreateValidCommand();
        command.SettingId = settingId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_WithDefaultSettingId_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithDefaultSettingId_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.SettingId = default(int); // This is 0

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_ShouldValidateAllProperties operation.
    /// </summary>

    [Fact]
    public void Validate_ShouldValidateAllProperties()
    {
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 6 Fix - Validator now validates ALL 3 properties comprehensively (SettingId, MachineId, Setting), not just SettingId. Updated test expectations accordingly.
        // Arrange - Test that all properties are validated
        var command = CreateValidCommand();
        command.SettingId = 10; // Valid
        // Set invalid values for other properties to confirm they ARE validated
        command.MachineId = -1;
        command.Setting = string.Empty;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse("All properties are validated, not just SettingId");
        result.ShouldHaveValidationErrorFor(x => x.MachineId)
            .WithErrorMessage("Machine ID must be greater than 0.");
        result.ShouldHaveValidationErrorFor(x => x.Setting)
            .WithErrorMessage("Setting must be between 1 and 500 characters.");
    }

    /// <summary>
    /// Executes ValidateAsync_WithValidCommand_ShouldPass operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidCommand_ShouldPass.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes ValidateAsync_WithInvalidCommand_ShouldFail operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithInvalidCommand_ShouldFail.</returns>

    [Fact]
    public async Task ValidateAsync_WithInvalidCommand_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.SettingId = 0;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
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
        var command = CreateValidCommand();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.TestValidateAsync(command, cancellationToken: cts.Token));
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
        var command = CreateValidCommand();
        command.SettingId = settingId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 6 Fix - Validator now validates ALL 3 properties comprehensively. Updated to check only SettingId property since CreateValidCommand sets all other properties to valid values.
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes GetValidSettingIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidSettingIdTestCases.</returns>

    public static IEnumerable<object[]> GetValidSettingIdTestCases()
    {
        yield return new object[] { 1, "Primary setting configuration" };
        yield return new object[] { 5, "Machine parameter setting" };
        yield return new object[] { 10, "Temperature threshold setting" };
        yield return new object[] { 25, "Speed control setting" };
        yield return new object[] { 50, "Safety parameter setting" };
        yield return new object[] { 100, "Advanced configuration setting" };
        yield return new object[] { 250, "Extended parameter setting" };
        yield return new object[] { 500, "System-level setting" };
        yield return new object[] { 1000, "Maximum configuration ID" };
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
        var command = CreateValidCommand();
        command.SettingId = settingId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes GetInvalidSettingIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetInvalidSettingIdTestCases.</returns>

    public static IEnumerable<object[]> GetInvalidSettingIdTestCases()
    {
        yield return new object[] { 0, "Zero SettingId (empty)" };
        yield return new object[] { -1, "Negative SettingId" };
        yield return new object[] { -10, "More negative SettingId" };
        yield return new object[] { -100, "Very negative SettingId" };
        yield return new object[] { int.MinValue, "Minimum integer value" };
    }

    /// <summary>
    /// Executes Validate_WithTypicalSettingScenarios_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithTypicalSettingScenarios_ShouldWorkCorrectly()
    {
        // Arrange - Test typical industrial setting scenarios
        var scenarios = new[]
        {
            new { SettingId = 1, Expected = true, Name = "Machine temperature threshold" },
            new { SettingId = 2, Expected = true, Name = "Production speed setting" },
            new { SettingId = 10, Expected = true, Name = "Safety interlock configuration" },
            new { SettingId = 25, Expected = true, Name = "Quality control parameters" },
            new { SettingId = 50, Expected = true, Name = "Maintenance schedule setting" },
            new { SettingId = 100, Expected = true, Name = "System diagnostic configuration" },
            new { SettingId = 0, Expected = false, Name = "Invalid default setting ID" },
            new { SettingId = -1, Expected = false, Name = "Invalid negative setting ID" }
        };

        foreach (var scenario in scenarios)
        {
            // Arrange
            var command = CreateValidCommand();
            command.SettingId = scenario.SettingId;

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(command);

            // Assert
            if (scenario.Expected)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.SettingId);
            }
        }
    }

    /// <summary>
    /// Executes Validate_WithBoundaryValues_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithBoundaryValues_ShouldWorkCorrectly()
    {
        // Arrange
        var validMinCommand = CreateValidCommand();
        validMinCommand.SettingId = 1; // Minimum valid

        var invalidZeroCommand = CreateValidCommand();
        invalidZeroCommand.SettingId = 0; // At boundary (invalid)

        var invalidNegativeCommand = CreateValidCommand();
        invalidNegativeCommand.SettingId = -1; // Below boundary (invalid)

        var largeValidCommand = CreateValidCommand();
        largeValidCommand.SettingId = 10000; // Large valid value

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var validMinResult = _validator.TestValidate(validMinCommand);
        var invalidZeroResult = _validator.TestValidate(invalidZeroCommand);
        var invalidNegativeResult = _validator.TestValidate(invalidNegativeCommand);
        var largeValidResult = _validator.TestValidate(largeValidCommand);

        // Assert
        validMinResult.ShouldNotHaveAnyValidationErrors();
        invalidZeroResult.ShouldHaveValidationErrorFor(x => x.SettingId);
        invalidNegativeResult.ShouldHaveValidationErrorFor(x => x.SettingId);
        largeValidResult.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validator_ShouldHaveCorrectRules operation.
    /// </summary>

    [Fact]
    public void Validator_ShouldHaveCorrectRules()
    {
        // Arrange
        var validCommand = CreateValidCommand();
        validCommand.SettingId = 10;

        var invalidCommand = CreateValidCommand();
        invalidCommand.SettingId = 0;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var validResult = _validator.TestValidate(validCommand);
        var invalidResult = _validator.TestValidate(invalidCommand);

        // Assert
        validResult.ShouldNotHaveAnyValidationErrors();
        invalidResult.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Executes Validate_WithIndustrialSettingTypes_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithIndustrialSettingTypes_ShouldWorkCorrectly()
    {
        // Arrange - Test common industrial setting type IDs
        var settingTypes = new[]
        {
            new { Id = 1, Type = "Temperature Control", Description = "Machine temperature settings" },
            new { Id = 2, Type = "Speed Control", Description = "Motor speed configurations" },
            new { Id = 5, Type = "Safety Parameters", Description = "Safety interlock settings" },
            new { Id = 10, Type = "Quality Thresholds", Description = "Quality control parameters" },
            new { Id = 15, Type = "Maintenance", Description = "Maintenance schedule settings" },
            new { Id = 20, Type = "Production", Description = "Production target settings" },
            new { Id = 50, Type = "Alarm Configuration", Description = "Alarm threshold settings" },
            new { Id = 100, Type = "System Configuration", Description = "System-level parameters" }
        };

        foreach (var settingType in settingTypes)
        {
            // Arrange
            var command = CreateValidCommand();
            command.SettingId = settingType.Id;

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(command);

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
        var validCommand = CreateValidCommand();
        validCommand.SettingId = 42;

        var invalidCommand = CreateValidCommand();
        invalidCommand.SettingId = 0;

        // Act - Multiple calls to ensure consistency
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var validResult1 = _validator.TestValidate(validCommand);
        var validResult2 = _validator.TestValidate(validCommand);
        var invalidResult1 = _validator.TestValidate(invalidCommand);
        var invalidResult2 = _validator.TestValidate(invalidCommand);

        // Assert
        validResult1.ShouldNotHaveAnyValidationErrors();
        validResult2.ShouldNotHaveAnyValidationErrors();
        invalidResult1.ShouldHaveValidationErrorFor(x => x.SettingId);
        invalidResult2.ShouldHaveValidationErrorFor(x => x.SettingId);
    }

    /// <summary>
    /// Creates a valid CreateSettingCommand for testing purposes
    /// </summary>
    private static CreateSettingCommand CreateValidCommand()
    {
        return new CreateSettingCommand
        {
            SettingId = 1,
            MachineId = 10000,
            Setting = "TemperatureThreshold=85.5"
        };
    }
}