// <copyright file="CreateVariableValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Unit tests for CreateVariableValidator
/// </summary>
public class CreateVariableValidatorTests
{
    private readonly CreateVariableValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateVariableValidatorTests()
    {
        _validator = new CreateVariableValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new CreateVariableValidator();

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
    /// Executes Validate_WithEmptyLength_ShouldFail operation.
    /// </summary>
    /// <param name="length">The length.</param>

    [Theory]
    [InlineData(0)]      // Zero length (invalid)
    public void Validate_WithEmptyLength_ShouldFail(int length)
    {
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Length = length;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        //Actual message "'Length' must be greater than '0'." [TODO] we must  a less flaky condition
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Length)
            .WithErrorMessage("'Length' must be greater than '0'.");
    }

    /// <summary>
    /// Executes Validate_WithValidLength_ShouldPass operation.
    /// </summary>
    /// <param name="length">The length.</param>

    [Theory]
    [InlineData(1)]      // Minimum valid length
    [InlineData(4)]      // Standard byte length
    [InlineData(8)]      // Standard long length
    [InlineData(16)]     // String length
    [InlineData(32)]     // Larger string length
    [InlineData(64)]     // Extended string length
    [InlineData(256)]    // Large string length
    [InlineData(1000)]   // Maximum valid length (validator max is 1000)
    public void Validate_WithValidLength_ShouldPass(int length)
    {
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Length = length;

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
        //Reason: Pattern 6 Fix - Validator now validates ALL 12 properties comprehensively, not just Length. Updated to check only Length property since CreateValidCommand sets all other properties to valid values.
        result.ShouldNotHaveValidationErrorFor(x => x.Length);
    }

    /// <summary>
    /// Executes Validate_WithNegativeLength_ShouldFail operation.
    /// </summary>
    /// <param name="length">The length.</param>

    [Theory]
    [InlineData(-1)]     // Negative length
    [InlineData(-10)]    // More negative
    [InlineData(-100)]   // Very negative
    public void Validate_WithNegativeLength_ShouldFail(int length)
    {
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Length = length;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse($"Negative length {length} should be invalid");
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateVariableCommand.Length));
    }

    /// <summary>
    /// Executes Validate_WithDefaultLength_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithDefaultLength_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Length = default(int); // This is 0

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Length);
    }

    /// <summary>
    /// Executes Validate_ShouldNotValidateOtherProperties operation.
    /// </summary>

    [Fact]
    public void Validate_ShouldValidateAllProperties()
    {
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Validator now validates ALL 12 properties comprehensively, not just Length. Updated test expectations accordingly.
        // Arrange - Test that all properties are validated
        var command = CreateValidCommand();
        command.Length = 10; // Valid
        // Set invalid values for other properties to confirm they ARE validated
        command.MachineId = -1;
        command.Name = string.Empty;
        command.Address = string.Empty;
        command.Type = string.Empty;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse("All properties are validated, not just Length");
        result.ShouldHaveValidationErrorFor(x => x.MachineId)
            .WithErrorMessage("Machine ID must be greater than 0.");
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Variable name must be between 1 and 100 characters.");
        result.ShouldHaveValidationErrorFor(x => x.Address)
            .WithErrorMessage("Address must be between 1 and 50 characters.");
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Type must be between 1 and 20 characters.");
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
        command.Length = 0;

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
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
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.TestValidateAsync(command, cancellationToken: cts.Token));
    }

    /// <summary>
    /// Executes Validate_WithMemberDataValidLengths_ShouldPass operation.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetValidLengthTestCases))]
    public void Validate_WithMemberDataValidLengths_ShouldPass(int length, string description)
    {
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Length = length;

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
        //Reason: Pattern 6 Fix - Validator now validates ALL 12 properties comprehensively, not just Length. Updated to check only Length property since CreateValidCommand sets all other properties to valid values.
        result.ShouldNotHaveValidationErrorFor(x => x.Length);
    }

    /// <summary>
    /// Executes GetValidLengthTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidLengthTestCases.</returns>

    public static IEnumerable<object[]> GetValidLengthTestCases()
    {
        yield return new object[] { 1, "Boolean/byte length" };
        yield return new object[] { 2, "Short integer length" };
        yield return new object[] { 4, "Integer/float length" };
        yield return new object[] { 8, "Long/double length" };
        yield return new object[] { 16, "Small string length" };
        yield return new object[] { 32, "Medium string length" };
        yield return new object[] { 64, "Large string length" };
        yield return new object[] { 128, "Extended string length" };
        yield return new object[] { 256, "Very large string length" };
        yield return new object[] { 512, "Buffer size length" };
        yield return new object[] { 1000, "Maximum allowed length" };
    }

    /// <summary>
    /// Executes Validate_WithMemberDataInvalidLengths_ShouldFail operation.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetInvalidLengthTestCases))]
    public void Validate_WithMemberDataInvalidLengths_ShouldFail(int length, string description)
    {
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: length, description
        _ = length; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Length = length;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse($"Length {length} ({description}) should be invalid");
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateVariableCommand.Length));
    }

    /// <summary>
    /// Executes GetInvalidLengthTestCases operation.
    /// </summary>
    /// <returns>The result of GetInvalidLengthTestCases.</returns>

    public static IEnumerable<object[]> GetInvalidLengthTestCases()
    {
        yield return new object[] { 0, "Zero length (empty)" };
        yield return new object[] { -1, "Negative length" };
        yield return new object[] { -10, "More negative length" };
        yield return new object[] { -100, "Very negative length" };
        yield return new object[] { int.MinValue, "Minimum integer value" };
    }

    /// <summary>
    /// Executes Validate_WithTypicalVariableScenarios_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithTypicalVariableScenarios_ShouldWorkCorrectly()
    {
        // Arrange - Test typical industrial variable scenarios
        var scenarios = new[]
        {
            new { Length = 1, Expected = true, Name = "Boolean/Bit variable" },
            new { Length = 2, Expected = true, Name = "Word variable (16-bit)" },
            new { Length = 4, Expected = true, Name = "DWord variable (32-bit)" },
            new { Length = 8, Expected = true, Name = "LWord variable (64-bit)" },
            new { Length = 20, Expected = true, Name = "String variable (20 chars)" },
            new { Length = 80, Expected = true, Name = "Description string" },
            new { Length = 255, Expected = true, Name = "Maximum short string" },
            new { Length = 0, Expected = false, Name = "Invalid zero length" },
            new { Length = -1, Expected = false, Name = "Invalid negative length" }
        };

        foreach (var scenario in scenarios)
        {
            // Arrange
            var command = CreateValidCommand();
            command.Length = scenario.Length;

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(command);

            // Assert
            result.IsValid.ShouldBe(scenario.Expected, $"Scenario '{scenario.Name}' should be {(scenario.Expected ? "valid" : "invalid")}");
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
        validMinCommand.Length = 1; // Minimum valid

        var invalidZeroCommand = CreateValidCommand();
        invalidZeroCommand.Length = 0; // At boundary (invalid)

        var invalidNegativeCommand = CreateValidCommand();
        invalidNegativeCommand.Length = -1; // Below boundary (invalid)

        var largeValidCommand = CreateValidCommand();
        largeValidCommand.Length = 1000; // Maximum valid value (validator max is 1000)

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
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 6 Fix - Validator now validates ALL 12 properties comprehensively. Updated to use FluentValidation.TestHelper for specific property validation.
        //Actual message ''Length' must be greater than '0'.' [TODO] we must  a less flaky condition
        validMinResult.ShouldNotHaveValidationErrorFor(x => x.Length);
        invalidZeroResult.ShouldHaveValidationErrorFor(x => x.Length).WithErrorMessage("'Length' must be greater than '0'.");
        invalidNegativeResult.ShouldHaveValidationErrorFor(x => x.Length)
            .WithErrorMessage("'Length' must be greater than '0'.");
        largeValidResult.ShouldNotHaveValidationErrorFor(x => x.Length);
    }

    /// <summary>
    /// Executes Validator_ShouldHaveCorrectRules operation.
    /// </summary>

    [Fact]
    public void Validator_ShouldHaveCorrectRules()
    {
        // Arrange
        var validCommand = CreateValidCommand();
        validCommand.Length = 10;

        var invalidCommand = CreateValidCommand();
        invalidCommand.Length = 0;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var validResult = _validator.TestValidate(validCommand);
        var invalidResult = _validator.TestValidate(invalidCommand);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 6 Fix - Validator now validates ALL 12 properties comprehensively. Updated to use FluentValidation.TestHelper for specific property validation.
        validResult.ShouldNotHaveValidationErrorFor(x => x.Length);
        invalidResult.ShouldHaveValidationErrorFor(x => x.Length)
            .WithErrorMessage("'Length' must be greater than '0'.");
    }

    /// <summary>
    /// Executes Validate_WithIndustrialDataTypes_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithIndustrialDataTypes_ShouldWorkCorrectly()
    {
        // Arrange - Test common industrial PLC data type lengths
        var dataTypes = new[]
        {
            new { Type = "BOOL", Length = 1, Description = "Boolean data type" },
            new { Type = "BYTE", Length = 1, Description = "8-bit unsigned integer" },
            new { Type = "WORD", Length = 2, Description = "16-bit unsigned integer" },
            new { Type = "DWORD", Length = 4, Description = "32-bit unsigned integer" },
            new { Type = "LWORD", Length = 8, Description = "64-bit unsigned integer" },
            new { Type = "SINT", Length = 1, Description = "Short integer" },
            new { Type = "INT", Length = 2, Description = "Integer" },
            new { Type = "DINT", Length = 4, Description = "Double integer" },
            new { Type = "LINT", Length = 8, Description = "Long integer" },
            new { Type = "REAL", Length = 4, Description = "32-bit floating point" },
            new { Type = "LREAL", Length = 8, Description = "64-bit floating point" },
            new { Type = "STRING", Length = 256, Description = "String data type" }
        };

        foreach (var dataType in dataTypes)
        {
            // Arrange
            var command = CreateValidCommand();
            command.Length = dataType.Length;
            command.Type = dataType.Type;

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(command);

            // Assert
            result.IsValid.ShouldBeTrue($"PLC data type {dataType.Type} with length {dataType.Length} should be valid");
        }
    }

    /// <summary>
    /// Creates a valid CreateVariableCommand for testing purposes
    /// </summary>
    private static CreateVariableCommand CreateValidCommand()
    {
        return new CreateVariableCommand
        {
            VariableId = 1,
            MachineId = 10000,
            Plc = "PLC1",
            Name = "TestVariable",
            Address = "DB1.DBW0",
            Type = "WORD",
            Length = 2,
            Event = 1,
            Direction = 0,
            VariableGroupId = 1
        };
    }
}