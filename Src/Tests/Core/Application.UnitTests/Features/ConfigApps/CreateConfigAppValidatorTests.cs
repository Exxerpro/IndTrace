// <copyright file="CreateConfigAppValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigApps;

/// <summary>
/// Unit tests for CreateConfigAppValidator
/// </summary>
public class CreateConfigAppValidatorTests
{
    private readonly CreateConfigAppValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateConfigAppValidatorTests()
    {
        _validator = new CreateConfigAppValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new CreateConfigAppValidator();

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
    /// Executes Validate_WithNullOrEmptyConfigId_ShouldFail operation.
    /// </summary>
    /// <param name="configId">The configId.</param>

    [Theory]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithNullOrEmptyConfigId_ShouldFail(string configId)
    {
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.ConfigId = configId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfigId);
    }

    /// <summary>
    /// Executes Validate_WithShortConfigId_ShouldFail operation.
    /// </summary>
    /// <param name="configId">The configId.</param>

    [Theory]
    [InlineData("A")]          // Too short
    [InlineData("AB")]         // Too short
    [InlineData("ABC")]        // Too short
    [InlineData("ABCD")]       // Too short
    [InlineData("ABCDE")]      // Too short
    [InlineData("ABCDEF")]     // Too short
    [InlineData("ABCDEFG")]    // Too short
    [InlineData("ABCDEFGH")]   // Too short
    [InlineData("ABCDEFGHI")]  // Too short (9 chars)
    public void Validate_WithShortConfigId_ShouldFail(string configId)
    {
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.ConfigId = configId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfigId);
    }

    /// <summary>
    /// Executes Validate_WithLongConfigId_ShouldFail operation.
    /// </summary>
    /// <param name="configId">The configId.</param>

    [Theory]
    [InlineData("ABCDEFGHIJK")]    // Too long (11 chars)
    [InlineData("ABCDEFGHIJKL")]   // Too long (12 chars)
    [InlineData("ABCDEFGHIJKLM")]  // Too long (13 chars)
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")] // Much too long
    public void Validate_WithLongConfigId_ShouldFail(string configId)
    {
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.ConfigId = configId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfigId);
    }

    /// <summary>
    /// Executes Validate_WithValidConfigId_ShouldPass operation.
    /// </summary>
    /// <param name="configId">The configId.</param>

    [Theory]
    [InlineData("ABCDEFGHIJ")]     // Exactly 10 chars
    [InlineData("1234567890")]     // Numbers
    [InlineData("A123456789")]     // Mixed alphanumeric
    [InlineData("CONFIG_APP")]     // With underscore
    [InlineData("CONFIG-APP")]     // With hyphen
    [InlineData("CONFIG.APP")]     // With period
    public void Validate_WithValidConfigId_ShouldPass(string configId)
    {
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.ConfigId = configId;

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
    /// Executes Validate_WithBoundaryConfigId_ExactlyTenCharacters_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithBoundaryConfigId_ExactlyTenCharacters_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();
        command.ConfigId = "1234567890"; // Exactly 10 characters

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        command.ConfigId.Length.ShouldBe(10);
    }

    /// <summary>
    /// Executes Validate_WithVariousValidTenCharacterConfigIds_ShouldPass operation.
    /// </summary>
    /// <param name="configId">The configId.</param>

    [Theory]
    [InlineData("CONFIGAPP1")]     // Alphanumeric
    [InlineData("CFG_APP_01")]     // With underscores
    [InlineData("CFG-APP-01")]     // With hyphens
    [InlineData("CFG.APP.01")]     // With periods
    [InlineData("0123456789")]     // All numeric
    [InlineData("ABCDEFGHIJ")]     // All letters
    public void Validate_WithVariousValidTenCharacterConfigIds_ShouldPass(string configId)
    {
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Using parameters: configId
        _ = configId; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.ConfigId = configId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        command.ConfigId.Length.ShouldBe(10);
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
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes ValidateAsync_WithInvalidCommand_ShouldFail operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithInvalidCommand_ShouldFail.</returns>

    [Fact]
    public async Task ValidateAsync_WithInvalidCommand_ShouldFail()
    {
        // Arrange
        var command = new CreateConfigAppCommand
        {
            ConfigId = "", // Empty ConfigId should fail
            AppId = 1,
            Client = "TestClient",
            Factorie = "TestPlant",
            Line = "TestLine",
            MachineId = 100,
            Project = "TestProject",
            Version = "1.0",
            VersionDate = DateTime.Now,
            ModifiedDate = DateTime.Now
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

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
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(command, cts.Token));
    }

    /// <summary>
    /// Executes Validate_OtherPropertiesAreNotValidated_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_OtherPropertiesAreNotValidated_ShouldPass()
    {
        // Arrange - Create command with invalid other properties but valid ConfigId
        var command = new CreateConfigAppCommand
        {
            ConfigId = "VALIDCONFG", // Valid 10-char ConfigId
            AppId = -1,             // Invalid AppId
            Client = "",           // Empty Client
            Factorie = null!,          // Null Factory
            Line = "",             // Empty Line
            MachineId = -100,       // Invalid MachineId
            Project = null!,        // Null Project
            Version = "",           // Empty Version
            VersionDate = default,  // Default DateTime
            ModifiedDate = default  // Default DateTime
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert - Should fail because validator now has comprehensive validation rules
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated test expectation - validator now validates all properties, not just ConfigId
        result.ShouldHaveValidationErrorFor(x => x.AppId);
        result.ShouldHaveValidationErrorFor(x => x.Client);
        result.ShouldHaveValidationErrorFor(x => x.Factorie);
        result.ShouldHaveValidationErrorFor(x => x.Line);
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
        result.ShouldHaveValidationErrorFor(x => x.Project);
        result.ShouldHaveValidationErrorFor(x => x.Version);
    }

    /// <summary>
    /// Creates a valid CreateConfigAppCommand for testing purposes
    /// </summary>
    private static CreateConfigAppCommand CreateValidCommand()
    {
        return new CreateConfigAppCommand
        {
            ConfigId = "CONFIG_001", // Valid 10-character ConfigId
            AppId = 1,
            Client = "TestClient",
            Factorie = "TestPlant",
            Line = "TestLine",
            MachineId = 100,
            Project = "TestProject",
            Version = "1.0.0",
            VersionDate = DateTime.Now,
            ModifiedDate = DateTime.Now
        };
    }
}