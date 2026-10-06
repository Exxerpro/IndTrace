// <copyright file="CreateConfigStationValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Commands.Create;

namespace Application.UnitTests.Features.ConfigStations;

/// <summary>
/// Unit tests for CreateConfigStationValidator using FluentValidation testing extensions
/// </summary>
public class CreateConfigStationValidatorTests
{
    private readonly CreateConfigStationValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateConfigStationValidatorTests()
    {
        _validator = new CreateConfigStationValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateValidatorInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateValidatorInstance()
    {
        // Arrange & Act
        var validator = new CreateConfigStationValidator();

        // Assert
        validator.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Validate_WithValidCommand_ShouldNotHaveValidationErrors operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithInvalidConfigId_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="configId">The configId.</param>

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidConfigId_ShouldHaveValidationError(string? configId)
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
        command.ConfigId = configId!;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfigId);
    }

    /// <summary>
    /// Executes Validate_WithInvalidConfigIdLength_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="configId">The configId.</param>

    [Theory]
    [InlineData("123456789")]  // Exactly 9 characters - too short
    [InlineData("12345678901")] // Exactly 11 characters - too long
    [InlineData("123456789012345")] // Much too long
    public void Validate_WithInvalidConfigIdLength_ShouldHaveValidationError(string configId)
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
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfigId);
    }

    /// <summary>
    /// Executes Validate_WithExactlyTenCharacters_ShouldNotHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validate_WithExactlyTenCharacters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.ConfigId = "1234567890"; // Exactly 10 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConfigId);
    }

    /// <summary>
    /// Executes Validate_WithInvalidMachineId_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Validate_WithInvalidMachineId_ShouldHaveValidationError(int machineId)
    {
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.MachineId = machineId;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
    }

    /// <summary>
    /// Executes Validate_WithValidMachineId_ShouldNotHaveValidationError operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public void Validate_WithValidMachineId_ShouldNotHaveValidationError(int machineId)
    {
        // Arrange
        var command = CreateValidCommand();
        command.MachineId = machineId;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
    }

    /// <summary>
    /// Executes Validate_WithEmptyOrNullStringProperties_ShouldHandleGracefully operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullStringProperties_ShouldHandleGracefully(string? value)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Client = value!;
        command.Factorie = value!;
        command.Line = value!;
        command.Machine = value!;
        command.Project = value!;
        command.Version = value!;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        // Since the validator only checks ConfigId length, other properties should not cause validation errors
        result.ShouldNotHaveValidationErrorFor(x => x.Machine);
        // unless additional rules are added
        result.ShouldHaveValidationErrorFor(x => x.Client);
        result.ShouldHaveValidationErrorFor(x => x.Factorie);
        result.ShouldHaveValidationErrorFor(x => x.Line);
        result.ShouldHaveValidationErrorFor(x => x.Project);
        result.ShouldHaveValidationErrorFor(x => x.Version);
    }

    /// <summary>
    /// Executes Validate_WithFutureDates_ShouldNotHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validate_WithFutureDates_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.VersionDate = DateTime.UtcNow.AddDays(1);
        command.ModifiedDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VersionDate);
        result.ShouldNotHaveValidationErrorFor(x => x.ModifiedDate);
    }

    /// <summary>
    /// Executes Validate_WithPastDates_ShouldNotHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validate_WithPastDates_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.VersionDate = DateTime.UtcNow.AddDays(-1);
        command.ModifiedDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VersionDate);
        result.ShouldNotHaveValidationErrorFor(x => x.ModifiedDate);
    }

    /// <summary>
    /// Executes Validate_WithMinimumDateTime_ShouldNotHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validate_WithMinimumDateTime_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.VersionDate = DateTime.MinValue;
        command.ModifiedDate = DateTime.MinValue;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VersionDate);
        result.ShouldNotHaveValidationErrorFor(x => x.ModifiedDate);
    }

    /// <summary>
    /// Executes Validate_WithMaximumDateTime_ShouldNotHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validate_WithMaximumDateTime_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.VersionDate = DateTime.MaxValue;
        command.ModifiedDate = DateTime.MaxValue;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VersionDate);
        result.ShouldNotHaveValidationErrorFor(x => x.ModifiedDate);
    }

    /// <summary>
    /// Executes Validate_WithVariousValidConfigIdFormats_ShouldNotHaveValidationError operation.
    /// </summary>
    /// <param name="configId">The configId.</param>

    [Theory]
    [InlineData("IndTrace01")]  // 10 characters, alphanumeric
    [InlineData("ABCDEFGHIJ")]  // 10 characters, all letters
    [InlineData("1234567890")]  // 10 characters, all numbers
    [InlineData("Test_ID123")]  // 10 characters, with underscore
    [InlineData("Test ID-01")]  // 10 characters, with space and dash
    public void Validate_WithVariousValidConfigIdFormats_ShouldNotHaveValidationError(string configId)
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
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConfigId);
    }

    private static CreateConfigStationCommand CreateValidCommand()
    {
        return new CreateConfigStationCommand
        {
            ConfigId = "TestConfig", // Exactly 10 characters
            MachineId = 10000,
            Client = "TestClient",
            Factorie = "TestFactory",
            Line = "TestLine",
            Machine = "TestMachine",
            Project = "TestProject",
            Version = "1.0",
            VersionDate = new DateTime(2023, 8, 31, 10, 46, 18),
            ModifiedDate = new DateTime(2023, 8, 31, 10, 46, 18)
        };
    }
}