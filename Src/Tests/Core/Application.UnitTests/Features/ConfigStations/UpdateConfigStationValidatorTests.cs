// <copyright file="UpdateConfigStationValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Commands.Update;

namespace Application.UnitTests.Features.ConfigStations;

/// <summary>
/// Unit tests for UpdateConfigStationValidator using FluentValidation testing extensions
/// </summary>
public class UpdateConfigStationValidatorTests
{
    private readonly UpdateConfigStationValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public UpdateConfigStationValidatorTests()
    {
        _validator = new UpdateConfigStationValidator();
    }
    /// <summary>
    /// Executes Constructor_ShouldCreateValidatorInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateValidatorInstance()
    {
        // Arrange & Act
        var validator = new UpdateConfigStationValidator();

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
        result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
    }
    /// <summary>
    /// Executes Validate_WithEmptyOrNullStringProperties_ShouldNotHaveValidationError operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullStringProperties_ShouldNotHaveValidationError(string? value)
    {
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.ConfigId = value!;
        command.Client = value!;
        command.Factorie = value!;
        command.Line = value!;
        command.Machine = value!;
        command.Project = value!;
        command.Version = value!;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        // The UpdateConfigStationValidator only validates MachineId, so string properties should not cause errors
        result.ShouldNotHaveValidationErrorFor(x => x.ConfigId);
        result.ShouldNotHaveValidationErrorFor(x => x.Client);
        result.ShouldNotHaveValidationErrorFor(x => x.Factorie);
        result.ShouldNotHaveValidationErrorFor(x => x.Line);
        result.ShouldNotHaveValidationErrorFor(x => x.Machine);
        result.ShouldNotHaveValidationErrorFor(x => x.Project);
        result.ShouldNotHaveValidationErrorFor(x => x.Version);
    }
    /// <summary>
    /// Executes Validate_WithLongStringValues_ShouldNotHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validate_WithLongStringValues_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern A Fix - Validator has Length() rules with specific limits, use strings within limits instead of 1000 chars
        command.ConfigId = new string('A', 50); // Max 50 chars
        command.Client = new string('B', 100); // Max 100 chars  
        command.Factorie = new string('C', 100); // Max 100 chars
        command.Line = new string('D', 100); // Max 100 chars
        command.Machine = new string('E', 100); // Max 100 chars
        command.Project = new string('F', 200); // Max 200 chars
        command.Version = new string('G', 50); // Max 50 chars

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        // Strings within length limits should not cause errors
        result.ShouldNotHaveValidationErrorFor(x => x.ConfigId);
        result.ShouldNotHaveValidationErrorFor(x => x.Client);
        result.ShouldNotHaveValidationErrorFor(x => x.Factorie);
        result.ShouldNotHaveValidationErrorFor(x => x.Line);
        result.ShouldNotHaveValidationErrorFor(x => x.Machine);
        result.ShouldNotHaveValidationErrorFor(x => x.Project);
        result.ShouldNotHaveValidationErrorFor(x => x.Version);
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
    /// Executes Validate_WithVariousInvalidMachineIds_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1000)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Validate_WithVariousInvalidMachineIds_ShouldHaveValidationError(int machineId)
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
    /// Executes Validate_WithVariousValidMachineIds_ShouldNotHaveValidationError operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(999)]
    [InlineData(int.MaxValue)]
    public void Validate_WithVariousValidMachineIds_ShouldNotHaveValidationError(int machineId)
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
        result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
    }
    /// <summary>
    /// Executes Validate_WithSpecialCharactersInStrings_ShouldNotHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validate_WithSpecialCharactersInStrings_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand();
        command.ConfigId = "Config@#$%";
        command.Client = "Client!@#";
        command.Factorie = "Factory<>?";
        command.Line = "Line[]{}";
        command.Machine = "Machine+=";
        command.Project = "Project~`";
        command.Version = "Ver|\\";

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConfigId);
        result.ShouldNotHaveValidationErrorFor(x => x.Client);
        result.ShouldNotHaveValidationErrorFor(x => x.Factorie);
        result.ShouldNotHaveValidationErrorFor(x => x.Line);
        result.ShouldNotHaveValidationErrorFor(x => x.Machine);
        result.ShouldNotHaveValidationErrorFor(x => x.Project);
        result.ShouldNotHaveValidationErrorFor(x => x.Version);
    }

    private static UpdateConfigStationCommand CreateValidCommand()
    {
        return new UpdateConfigStationCommand
        {
            ConfigId = "UpdatedConfig",
            MachineId = 10000,
            Client = "UpdatedClient",
            Factorie = "UpdatedFactory",
            Line = "UpdatedLine",
            Machine = "UpdatedMachine",
            Project = "UpdatedProject",
            Version = "2.0",
            VersionDate = new DateTime(2023, 9, 1, 11, 0, 0),
            ModifiedDate = DateTime.UtcNow
        };
    }
}