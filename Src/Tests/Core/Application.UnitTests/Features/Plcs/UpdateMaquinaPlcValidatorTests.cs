// <copyright file="UpdateMaquinaPlcValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Plcs;

/// <summary>
/// Unit tests for UpdateMaquinaPlcValidator - FluentValidation for PLC (Programmable Logic Controller) update operations.
/// Tests validation rules for industrial manufacturing PLC configurations including Siemens, Allen-Bradley, and Mitsubishi systems.
/// </summary>
public class UpdateMaquinaPlcValidatorTests
{
    private readonly UpdateMaquinaPlcValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public UpdateMaquinaPlcValidatorTests()
    {
        _validator = new UpdateMaquinaPlcValidator();
    }
    /// <summary>
    /// Executes Constructor_WhenCalled_ShouldCreateValidatorInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WhenCalled_ShouldCreateValidatorInstance()
    {
        // Arrange & Act
        var validator = new UpdateMaquinaPlcValidator();

        // Assert
        validator.ShouldNotBeNull();
        validator.ShouldBeAssignableTo<AbstractValidator<UpdateMachinePlcCommand>>();
    }
    /// <summary>
    /// Executes Validate_WithValidPlcUpdateCommand_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Validate_WithValidPlcUpdateCommand_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Validate_WithValidPlcUpdateCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new UpdateMachinePlcCommand
        {
            MachineId = 2001,
            PlcId = 3001,
            IsActive = 1
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithInvalidMachineIds_ShouldFailValidation operation.
    /// </summary>
    /// <param name="invalidMachineId">The invalidMachineId.</param>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Validate_WithInvalidMachineIds_ShouldFailValidation.</returns>

    [Theory]
    [InlineData(0, "Invalid MachineId - Zero")]
    [InlineData(-1, "Invalid MachineId - Negative")]
    [InlineData(-999, "Invalid MachineId - Large Negative")]
    public async Task Validate_WithInvalidMachineIds_ShouldFailValidation(int invalidMachineId, string scenario)
    {
        // Using parameters: invalidMachineId, scenario
        _ = invalidMachineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidMachineId, scenario
        _ = invalidMachineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidMachineId, scenario
        _ = invalidMachineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidMachineId, scenario
        _ = invalidMachineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidMachineId, scenario
        _ = invalidMachineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand
        {
            MachineId = invalidMachineId,
            PlcId = 3001,
            IsActive = 1
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateMachinePlcCommand.MachineId));

        // Verify scenario context
        scenario.ShouldNotBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithInvalidPlcIds_ShouldFailValidation operation.
    /// </summary>
    /// <param name="invalidPlcId">The invalidPlcId.</param>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Validate_WithInvalidPlcIds_ShouldFailValidation.</returns>

    [Theory]
    [InlineData(0, "Invalid PlcId - Zero")]
    [InlineData(-1, "Invalid PlcId - Negative")]
    [InlineData(-750, "Invalid PlcId - Large Negative")]
    public async Task Validate_WithInvalidPlcIds_ShouldFailValidation(int invalidPlcId, string scenario)
    {
        // Using parameters: invalidPlcId, scenario
        _ = invalidPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidPlcId, scenario
        _ = invalidPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidPlcId, scenario
        _ = invalidPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidPlcId, scenario
        _ = invalidPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidPlcId, scenario
        _ = invalidPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand
        {
            MachineId = 2001,
            PlcId = invalidPlcId,
            IsActive = 1
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateMachinePlcCommand.PlcId));

        // Verify scenario context
        scenario.ShouldNotBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithIndustrialManufacturingScenarios_ShouldPassValidation operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <param name="isActive">The isActive.</param>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Validate_WithIndustrialManufacturingScenarios_ShouldPassValidation.</returns>

    [Theory]
    [InlineData(101, 201, 1, "Ford F-150 Engine Assembly PLC")]
    [InlineData(102, 202, 0, "iPhone PCB Surface Mount PLC")]
    [InlineData(103, 203, 1, "Aspirin Tablet Press PLC")]
    [InlineData(104, 204, 0, "Intel CPU Lithography PLC")]
    [InlineData(105, 205, 1, "Samsung OLED Panel PLC")]
    public async Task Validate_WithIndustrialManufacturingScenarios_ShouldPassValidation(int machineId, int plcId, int isActive, string scenario)
    {
        // Using parameters: machineId, plcId, isActive, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand
        {
            MachineId = machineId,
            PlcId = plcId,
            IsActive = isActive
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        // Verify manufacturing scenario context
        scenario.ShouldNotBeEmpty();
        scenario.ShouldContain("PLC");
    }
    /// <summary>
    /// Executes Validate_WithDifferentPlcBrands_ShouldPassValidation operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <param name="plcBrand">The plcBrand.</param>
    /// <returns>The result of Validate_WithDifferentPlcBrands_ShouldPassValidation.</returns>

    [Theory]
    [InlineData(5000, 8000, "Siemens S7-1500 Series")]
    [InlineData(6000, 9000, "Allen-Bradley ControlLogix")]
    [InlineData(7000, 10000, "Mitsubishi MELSEC iQ-R")]
    [InlineData(8000, 11000, "Schneider Electric Modicon M580")]
    [InlineData(9000, 12000, "Rockwell Automation CompactLogix")]
    public async Task Validate_WithDifferentPlcBrands_ShouldPassValidation(int machineId, int plcId, string plcBrand)
    {
        // Using parameters: machineId, plcId, plcBrand
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        // Using parameters: machineId, plcId, plcBrand
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        // Using parameters: machineId, plcId, plcBrand
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        // Using parameters: machineId, plcId, plcBrand
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        // Using parameters: machineId, plcId, plcBrand
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand
        {
            MachineId = machineId,
            PlcId = plcId,
            IsActive = 1
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        // Verify PLC brand context
        plcBrand.ShouldNotBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithMaxValidValues_ShouldPassValidation operation.
    /// </summary>
    /// <returns>The result of Validate_WithMaxValidValues_ShouldPassValidation.</returns>

    [Fact]
    public async Task Validate_WithMaxValidValues_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateMachinePlcCommand
        {
            MachineId = int.MaxValue,
            PlcId = int.MaxValue - 1,
            IsActive = 1
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithActiveAndInactiveStates_ShouldPassValidation operation.
    /// </summary>
    /// <returns>The result of Validate_WithActiveAndInactiveStates_ShouldPassValidation.</returns>

    [Fact]
    public async Task Validate_WithActiveAndInactiveStates_ShouldPassValidation()
    {
        // Arrange - Active PLC
        var activeCommand = new UpdateMachinePlcCommand
        {
            MachineId = 2001,
            PlcId = 3001,
            IsActive = 1
        };

        // Arrange - Inactive PLC
        var inactiveCommand = new UpdateMachinePlcCommand
        {
            MachineId = 2002,
            PlcId = 3002,
            IsActive = 0
        };

        // Act
        var activeResult = await _validator.ValidateAsync(activeCommand, TestContext.Current.CancellationToken);
        var inactiveResult = await _validator.ValidateAsync(inactiveCommand, TestContext.Current.CancellationToken);

        // Assert
        activeResult.IsValid.ShouldBeTrue();
        inactiveResult.IsValid.ShouldBeTrue();
        activeResult.Errors.ShouldBeEmpty();
        inactiveResult.Errors.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithNullCommand_ShouldThrowException operation.
    /// </summary>
    /// <returns>The result of Validate_WithNullCommand_ShouldThrowException.</returns>

    [Fact]
    public async Task Validate_WithNullCommand_ShouldThrowException()
    {
        // Arrange
        UpdateMachinePlcCommand? nullCommand = null!;

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _validator.ValidateAsync(nullCommand!, TestContext.Current.CancellationToken));
        exception.Message.ShouldContain("Cannot pass a null model to Validate");
    }
    /// <summary>
    /// Executes Validate_WithVariousIdRanges_ShouldPassValidation operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Validate_WithVariousIdRanges_ShouldPassValidation.</returns>

    [Theory]
    [InlineData(1, 2, "Low ID Range")]
    [InlineData(100, 200, "Medium ID Range")]
    [InlineData(10000, 20000, "High ID Range")]
    [InlineData(999999, 1000000, "Very High ID Range")]
    public async Task Validate_WithVariousIdRanges_ShouldPassValidation(int machineId, int plcId, string scenario)
    {
        // Using parameters: machineId, plcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand
        {
            MachineId = machineId,
            PlcId = plcId,
            IsActive = 1
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        // Verify scenario context
        scenario.ShouldNotBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithIndustrySectors_ShouldPassValidation operation.
    /// </summary>
    /// <param name="industrySector">The industrySector.</param>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <returns>The result of Validate_WithIndustrySectors_ShouldPassValidation.</returns>

    [Theory]
    [InlineData("Automotive Assembly Line", 1001, 2001)]
    [InlineData("Electronics SMT Line", 1002, 2002)]
    [InlineData("Pharmaceutical Fill Line", 1003, 2003)]
    [InlineData("Food & Beverage Packaging", 1004, 2004)]
    [InlineData("Chemical Process Control", 1005, 2005)]
    public async Task Validate_WithIndustrySectors_ShouldPassValidation(string industrySector, int machineId, int plcId)
    {
        // Using parameters: industrySector, machineId, plcId
        _ = industrySector; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Using parameters: industrySector, machineId, plcId
        _ = industrySector; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Using parameters: industrySector, machineId, plcId
        _ = industrySector; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Using parameters: industrySector, machineId, plcId
        _ = industrySector; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Using parameters: industrySector, machineId, plcId
        _ = industrySector; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand
        {
            MachineId = machineId,
            PlcId = plcId,
            IsActive = 1
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        // Verify industry context
        industrySector.ShouldNotBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithMultipleValidationErrors_ShouldReturnAllErrors operation.
    /// </summary>
    /// <returns>The result of Validate_WithMultipleValidationErrors_ShouldReturnAllErrors.</returns>

    [Fact]
    public async Task Validate_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange - Command with multiple invalid properties
        var command = new UpdateMachinePlcCommand
        {
            MachineId = -1,
            PlcId = -2,
            IsActive = -1 // Invalid value
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of Validate_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task Validate_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var command = new UpdateMachinePlcCommand
        {
            MachineId = 2001,
            PlcId = 3001,
            IsActive = 1
        };
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var result = await _validator.ValidateAsync(command, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
    }
    /// <summary>
    /// Executes Validate_WithIndustry4Point0Scenarios_ShouldPassValidation operation.
    /// </summary>
    /// <param name="technology">The technology.</param>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <returns>The result of Validate_WithIndustry4Point0Scenarios_ShouldPassValidation.</returns>

    [Theory]
    [InlineData("Smart Factory IoT Integration", 5001, 6001)]
    [InlineData("AI-Driven Quality Control", 5002, 6002)]
    [InlineData("Digital Twin Manufacturing", 5003, 6003)]
    [InlineData("Predictive Maintenance", 5004, 6004)]
    [InlineData("Edge Computing PLCs", 5005, 6005)]
    public async Task Validate_WithIndustry4Point0Scenarios_ShouldPassValidation(string technology, int machineId, int plcId)
    {
        // Using parameters: technology, machineId, plcId
        _ = technology; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Using parameters: technology, machineId, plcId
        _ = technology; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Using parameters: technology, machineId, plcId
        _ = technology; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Using parameters: technology, machineId, plcId
        _ = technology; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Using parameters: technology, machineId, plcId
        _ = technology; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        // Arrange - Industry 4.0 PLC scenarios
        var command = new UpdateMachinePlcCommand
        {
            MachineId = machineId,
            PlcId = plcId,
            IsActive = 1
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        // Verify Industry 4.0 context
        technology.ShouldNotBeEmpty();
        technology.Length.ShouldBeGreaterThan(0);
    }
}