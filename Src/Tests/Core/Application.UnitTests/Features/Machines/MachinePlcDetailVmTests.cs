// <copyright file="MachinePlcDetailVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Comprehensive unit tests for MachinePlcDetailVm - Machine-PLC relationship view model
/// </summary>
public class MachinePlcDetailVmTests
{
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>
    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Act
        var viewModel = new MachinePlcDetailVm();

        // Assert
        viewModel.ShouldNotBeNull();
        viewModel.MachineId.ShouldBe(0);
        viewModel.PlcId.ShouldBe(0);
        viewModel.IsActive.ShouldBe(0);
    }
    /// <summary>
    /// Executes Should_SetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var viewModel = new MachinePlcDetailVm();
        const int expectedMachineId = 10001;
        const int expectedPlcId = 201;
        const int expectedIsActive = 1;

        // Act
        viewModel.MachineId = expectedMachineId;
        viewModel.PlcId = expectedPlcId;
        viewModel.IsActive = expectedIsActive;

        // Assert
        viewModel.MachineId.ShouldBe(expectedMachineId);
        viewModel.PlcId.ShouldBe(expectedPlcId);
        viewModel.IsActive.ShouldBe(expectedIsActive);
    }
    /// <summary>
    /// Executes Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(101, 201, 1, "Fanuc Robot to Siemens S7-1500 PLC")]
    [InlineData(102, 202, 1, "ABB Robot to Allen-Bradley ControlLogix")]
    [InlineData(103, 203, 1, "Cognex Vision to Schneider Modicon M580")]
    [InlineData(104, 204, 0, "CNC Machine to Omron NJ-Series (Inactive)")]
    public void Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided(
        int machineId, int plcId, int isActive, string description)
    {

        var logger = XUnitLogger.CreateLogger<MachinePlcDetailVmTests>();
        logger.LogInformation("Testing scenario: {description} with machineId={machineId}, plcId={plcId}, isActive={isActive}",
            description, machineId, plcId, isActive);

        // Arrange
        var viewModel = new MachinePlcDetailVm();

        // Act
        viewModel.MachineId = machineId;
        viewModel.PlcId = plcId;
        viewModel.IsActive = isActive;

        // Assert
        viewModel.MachineId.ShouldBe(machineId);
        viewModel.PlcId.ShouldBe(plcId);
        viewModel.IsActive.ShouldBe(isActive);
    }
    /// <summary>
    /// Executes ToDto_WithValidMachinePlc_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidMachinePlc_ShouldMapAllProperties()
    {
        // Arrange
        var machinePlc = new MachinePlc(101, 201, 1);

        // Act
        var resultWrapper = MachinePlcDetailVm.ToDto(machinePlc);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.MachineId.ShouldBe(101);
        result.PlcId.ShouldBe(201);
        result.IsActive.ShouldBe(1);
    }
    /// <summary>
    /// Executes ToDto_WithNullMachinePlc_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullMachinePlc_ShouldReturnFailureResult()
    {
        // Arrange
        MachinePlc? nullMachinePlc = null!;

        // Act
        var result = MachinePlcDetailVm.ToDto(nullMachinePlc!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("MachinePlc source cannot be null");
    }
    /// <summary>
    /// Executes ToEntity_WithValidViewModel_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidViewModel_ShouldMapAllProperties()
    {
        // Arrange
        var viewModel = new MachinePlcDetailVm
        {
            MachineId = 10001,
            PlcId = 201,
            IsActive = 1
        };

        // Act
        var resultWrapper = MachinePlcDetailVm.ToEntity(viewModel);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.MachineId.Value.ShouldBe(10001);
        result.PlcId.ShouldBe(201);
        result.IsActive.Value.ShouldBe(1);
    }
    /// <summary>
    /// Executes ToEntity_WithNullViewModel_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullViewModel_ShouldReturnFailureResult()
    {
        // Arrange
        MachinePlcDetailVm? nullViewModel = null!;

        // Act
        var result = MachinePlcDetailVm.ToEntity(nullViewModel!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("MachinePlcDetailVm source cannot be null");
    }
    /// <summary>
    /// Executes Should_PerformRoundTripConversion_When_ConvertingBetweenEntityAndDto operation.
    /// </summary>

    [Fact]
    public void Should_PerformRoundTripConversion_When_ConvertingBetweenEntityAndDto()
    {
        // Arrange
        var originalEntity = new MachinePlc(501, 601, 1);

        // Act
        var dtoWrapper = MachinePlcDetailVm.ToDto(originalEntity);
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;

        var convertedEntityWrapper = MachinePlcDetailVm.ToEntity(dto);
        convertedEntityWrapper.IsSuccess.ShouldBeTrue();
        convertedEntityWrapper.Value.ShouldNotBeNull();
        var convertedEntity = convertedEntityWrapper.Value;
        convertedEntity.ShouldNotBeNull();

        // Assert
        convertedEntity.ShouldNotBeNull();
        convertedEntity.MachineId.ShouldBe(originalEntity.MachineId);
        convertedEntity.PlcId.ShouldBe(originalEntity.PlcId);
        convertedEntity.IsActive.Value.ShouldBe(originalEntity.IsActive.Value);
    }
}
