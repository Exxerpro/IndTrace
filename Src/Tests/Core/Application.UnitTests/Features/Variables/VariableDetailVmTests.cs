// <copyright file="VariableDetailVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Comprehensive unit tests for VariableDetailVm - PLC Variable configuration view model
/// </summary>
public class VariableDetailVmTests
{
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>
    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Act
        var viewModel = new VariableDetailVm();

        // Assert
        viewModel.ShouldNotBeNull();
        viewModel.VariableId.ShouldBe(0);
        viewModel.MachineId.ShouldBe(0);
        viewModel.PlcId.ShouldBe(0);
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - VariableDetailVm initializes string properties with = null!, not string.Empty
        viewModel.Name.ShouldBe(string.Empty);
        viewModel.Address.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes Should_SetAllIntegerProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAllIntegerProperties_When_ValidValuesProvided()
    {
        // Arrange
        var viewModel = new VariableDetailVm();
        const int expectedVariableId = 1501;
        const int expectedMachineId = 10001;
        const int expectedPlcId = 201;

        // Act
        viewModel.VariableId = expectedVariableId;
        viewModel.MachineId = expectedMachineId;
        viewModel.PlcId = expectedPlcId;

        // Assert
        viewModel.VariableId.ShouldBe(expectedVariableId);
        viewModel.MachineId.ShouldBe(expectedMachineId);
        viewModel.PlcId.ShouldBe(expectedPlcId);
    }
    /// <summary>
    /// Executes Should_HandleManufacturingScenarios_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(1501, 101, 201, "M001_CycleStart", "DB1.DBX0.0", "Ford F-150 welding station")]
    [InlineData(3301, 201, 301, "M002_PickPlaceStatus", "DB2.DBX1.0", "iPhone PCB assembly line")]
    [InlineData(4401, 301, 401, "M003_VialCount", "DB3.DBW0", "Pfizer vaccine production")]
    public void Should_HandleManufacturingScenarios_When_ValidDataProvided(
        int variableId, int machineId, int plcId, string name, string address, string description)
    {
        // Arrange
        description.ShouldNotBeNull(); // Validates test description parameter

        var viewModel = new VariableDetailVm();

        // Act
        viewModel.VariableId = variableId;
        viewModel.MachineId = machineId;
        viewModel.PlcId = plcId;
        viewModel.Name = name;
        viewModel.Address = address;

        // Assert
        viewModel.VariableId.ShouldBe(variableId);
        viewModel.MachineId.ShouldBe(machineId);
        viewModel.PlcId.ShouldBe(plcId);
        viewModel.Name.ShouldBe(name);
        viewModel.Address.ShouldBe(address);
    }
    /// <summary>
    /// Executes ToDto_Should_ConvertEntityToDto_When_ValidEntityProvided operation.
    /// </summary>

    [Fact]
    public void ToDto_Should_ConvertEntityToDto_When_ValidEntityProvided()
    {
        // Arrange - Fanuc robotic welding cell variable
        var variable = new Variable
        {
            VariableId = 1501,
            MachineId = 10001,
            PlcId = 201,
            Name = "M001_RobotReady",
            Address = "DB1.DBX0.1",
            Alias = "FanucRobotReady",
            NetType = "System.Boolean"
        };

        // Act
        var dtoWrapper = VariableDetailVm.ToDto(variable);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.VariableId.ShouldBe(variable.VariableId);
        dto.MachineId.ShouldBe(variable.MachineId);
        dto.PlcId.ShouldBe(variable.PlcId);
        dto.Name.ShouldBe(variable.Name);
        dto.Address.ShouldBe(variable.Address);
    }
    /// <summary>
    /// Executes ToDto_Should_ReturnFailureResult_When_NullEntityProvided operation.
    /// </summary>

    [Fact]
    public void ToDto_Should_ReturnFailureResult_When_NullEntityProvided()
    {
        // Arrange
        Variable? nullVariable = null!;

        // Act
        var result = VariableDetailVm.ToDto(nullVariable!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Variable source cannot be null");
    }
    /// <summary>
    /// Executes ToEntity_Should_ConvertDtoToEntity_When_ValidDtoProvided operation.
    /// </summary>

    [Fact]
    public void ToEntity_Should_ConvertDtoToEntity_When_ValidDtoProvided()
    {
        // Arrange - ABB paint robot variable
        var dto = new VariableDetailVm
        {
            VariableId = 2501,
            MachineId = 201,
            PlcId = 301,
            Name = "M002_PaintGunPosition",
            Address = "DB2.DBD0",
            Alias = "ABBPaintGunPos",
            NetType = "System.Single"
        };

        // Act
        var entityWrapper = VariableDetailVm.ToEntity(dto);

        // Assert
        entityWrapper.IsSuccess.ShouldBeTrue();
        entityWrapper.Value.ShouldNotBeNull();
        var entity = entityWrapper.Value;
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.VariableId.ShouldBe(dto.VariableId);
        entity.MachineId.ShouldBe(dto.MachineId);
        entity.PlcId.ShouldBe(dto.PlcId);
        entity.Name.ShouldBe(dto.Name);
        entity.Address.ShouldBe(dto.Address);
    }
    /// <summary>
    /// Executes ToEntity_Should_ReturnFailureResult_When_NullDtoProvided operation.
    /// </summary>

    [Fact]
    public void ToEntity_Should_ReturnFailureResult_When_NullDtoProvided()
    {
        // Arrange
        VariableDetailVm? nullDto = null!;

        // Act
        var result = VariableDetailVm.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("VariableDetailVm source cannot be null");
    }
}
