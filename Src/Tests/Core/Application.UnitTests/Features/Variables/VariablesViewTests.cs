// <copyright file="VariablesViewTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Unit tests for VariablesView
/// </summary>
public class VariablesViewTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new VariablesView();

        // Assert
        instance.ShouldNotBeNull();
        instance.VariableId.ShouldBe(0);
        instance.MachineId.ShouldBe(0);
        instance.PlcId.ShouldBe(0);
        instance.Name.ShouldBe(string.Empty);
        instance.Description.ShouldBe(string.Empty);
        instance.Alias.ShouldBe(string.Empty);
        instance.Address.ShouldBe(string.Empty);
        instance.NetType.ShouldBe(string.Empty);
        instance.Length.ShouldBe(0);
        instance.IsActive.ShouldBe(0);
        instance.Direction.ShouldBe(0);
        instance.VariableGroupId.ShouldBe(0);
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange - Siemens S7-1500 PLC variable for robotic welding cell
        var instance = new VariablesView();
        const int expectedVariableId = 1501;
        const int expectedMachineId = 10001;
        const int expectedPlcId = 15;
        const string expectedName = "WeldingCellCycleStart";
        const string expectedDescription = "Cycle start signal for robotic welding cell";
        const string expectedAlias = "WELD_START";
        const string expectedAddress = "DB1.DBX0.0";
        const string expectedNetType = "BOOL";
        const int expectedLength = 1;
        const int expectedIsActive = 1;
        const int expectedDirection = 1;
        const int expectedVariableGroupId = 10;

        // Act
        instance.VariableId = expectedVariableId;
        instance.MachineId = expectedMachineId;
        instance.PlcId = expectedPlcId;
        instance.Name = expectedName;
        instance.Description = expectedDescription;
        instance.Alias = expectedAlias;
        instance.Address = expectedAddress;
        instance.NetType = expectedNetType;
        instance.Length = expectedLength;
        instance.IsActive = expectedIsActive;
        instance.Direction = expectedDirection;
        instance.VariableGroupId = expectedVariableGroupId;

        // Assert
        instance.VariableId.ShouldBe(expectedVariableId);
        instance.MachineId.ShouldBe(expectedMachineId);
        instance.PlcId.ShouldBe(expectedPlcId);
        instance.Name.ShouldBe(expectedName);
        instance.Description.ShouldBe(expectedDescription);
        instance.Alias.ShouldBe(expectedAlias);
        instance.Address.ShouldBe(expectedAddress);
        instance.NetType.ShouldBe(expectedNetType);
        instance.Length.ShouldBe(expectedLength);
        instance.IsActive.ShouldBe(expectedIsActive);
        instance.Direction.ShouldBe(expectedDirection);
        instance.VariableGroupId.ShouldBe(expectedVariableGroupId);
    }

    /// <summary>
    /// Executes Properties_WithVariousManufacturingScenarios_ShouldRetainValues operation.
    /// </summary>

    [Theory]
    [InlineData(2801, 201, 28, "ABB IRC5", "TemperatureSensor", "TEMP_01", "AI1", "REAL", 4)]
    [InlineData(3301, 301, 33, "Fanuc 31i-B", "CycleCounter", "CYC_CNT", "R100", "INT", 2)]
    [InlineData(4401, 401, 44, "Mitsubishi FX5U", "QualityStatus", "QUAL_OK", "M100", "BOOL", 1)]
    [InlineData(5501, 501, 55, "Schneider M580", "ProductionSpeed", "PROD_SPD", "MW200", "WORD", 2)]
    public void Properties_WithVariousManufacturingScenarios_ShouldRetainValues(
        int variableId, int machineId, int plcId, string name, string description, string alias, string address, string netType, int length)
    {
        // Arrange
        var instance = new VariablesView();

        // Act
        instance.VariableId = variableId;
        instance.MachineId = machineId;
        instance.PlcId = plcId;
        instance.Name = name;
        instance.Description = description;
        instance.Alias = alias;
        instance.Address = address;
        instance.NetType = netType;
        instance.Length = length;

        // Assert
        instance.VariableId.ShouldBe(variableId);
        instance.MachineId.ShouldBe(machineId);
        instance.PlcId.ShouldBe(plcId);
        instance.Name.ShouldBe(name);
        instance.Description.ShouldBe(description);
        instance.Alias.ShouldBe(alias);
        instance.Address.ShouldBe(address);
        instance.NetType.ShouldBe(netType);
        instance.Length.ShouldBe(length);
    }

    /// <summary>
    /// Executes ToDto_WithValidVariable_ShouldCreateCorrectVariablesView operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidVariable_ShouldCreateCorrectVariablesView()
    {
        // Arrange - Ford F-150 production line variable
        var variable = new Variable
        {
            VariableId = 1501,
            MachineId = 10001,
            PlcId = 15,
            Name = "CycleStatusPlc",
            Description = "Cycle status from PLC",
            Alias = "CYC_STAT",
            Address = "DB1.DBW10",
            NetType = "INT",
            Length = 2,
            IsActive = 1,
            Direction = 0,
            VariableGroupId = 10,
        };

        // Act
        var resultWrapper = VariablesView.ToDto(variable);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.VariableId.ShouldBe(1501);
        result.MachineId.ShouldBe(10001);
        result.PlcId.ShouldBe(15);
        result.Name.ShouldBe("CycleStatusPlc");
        result.Description.ShouldBe("Cycle status from PLC");
        result.Alias.ShouldBe("CYC_STAT");
        result.Address.ShouldBe("DB1.DBW10");
        result.NetType.ShouldBe("INT");
        result.Length.ShouldBe(2);
        result.IsActive.ShouldBe(1);
        result.Direction.ShouldBe(0);
        result.VariableGroupId.ShouldBe(10);
    }

    /// <summary>
    /// Executes ToDto_WithNullVariable_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullVariable_ShouldReturnFailureResult()
    {
        // Arrange
        Variable nullVariable = null!;

        // Act
        var result = VariablesView.ToDto(nullVariable);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Variable source cannot be null");
    }

    /// <summary>
    /// Executes ToDtoList_WithValidVariableList_ShouldCreateCorrectVariablesViewList operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithValidVariableList_ShouldCreateCorrectVariablesViewList()
    {
        // Arrange - Multiple manufacturing variables
        var variables = new List<Variable>
        {
            new() { VariableId = 1, Name = "CycleStatusPlc", MachineId = 10001 },
            new() { VariableId = 2, Name = "PartStatusPlc", MachineId = 10002 },
            new() { VariableId = 3, Name = "TemperatureSensor", MachineId = 10003 }
        };

        // Act
        var resultWrapper = VariablesView.ToDtoList(variables);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.Count().ShouldBe(3);

        var resultList = result.ToList();
        resultList[0].Name.ShouldBe("CycleStatusPlc");
        resultList[1].Name.ShouldBe("PartStatusPlc");
        resultList[2].Name.ShouldBe("TemperatureSensor");
    }

    /// <summary>
    /// Executes ToDtoList_WithNullVariableList_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithNullVariableList_ShouldReturnFailureResult()
    {
        // Arrange
        IEnumerable<Variable> nullVariableList = null!;

        // Act
        var result = VariablesView.ToDtoList(nullVariableList);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Variable collection cannot be null");
    }

    /// <summary>
    /// Executes ToEntity_WithValidVariablesView_ShouldCreateCorrectVariable operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidVariablesView_ShouldCreateCorrectVariable()
    {
        // Arrange - Electronics manufacturing scenario
        var variablesView = new VariablesView
        {
            VariableId = 8801,
            MachineId = 880,
            PlcId = 88,
            Name = "PCB_InspectionResult",
            Description = "PCB quality inspection result",
            Alias = "PCB_QUAL",
            Address = "DM1000",
            NetType = "DINT",
            Length = 4,
            IsActive = 1,
            Direction = 0,
            VariableGroupId = 88,
        };

        // Act
        var resultWrapper = VariablesView.ToEntity(variablesView);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.VariableId.ShouldBe(8801);
        result.MachineId.ShouldBe(880);
        result.PlcId.ShouldBe(88);
        result.Name.ShouldBe("PCB_InspectionResult");
        result.Description.ShouldBe("PCB quality inspection result");
        result.Alias.ShouldBe("PCB_QUAL");
        result.Address.ShouldBe("DM1000");
        result.NetType.ShouldBe("DINT");
        result.Length.ShouldBe(4);
        result.IsActive.Value.ShouldBe(1);
        result.Direction.ShouldBe(0);
        result.VariableGroupId.ShouldBe(88);
    }

    /// <summary>
    /// Executes ToEntity_WithNullVariablesView_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullVariablesView_ShouldReturnFailureResult()
    {
        // Arrange
        VariablesView nullVariablesView = null!;

        // Act
        var result = VariablesView.ToEntity(nullVariablesView);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("VariablesView source cannot be null");
    }

    /// <summary>
    /// Executes Properties_WithPharmaceuticalManufacturingScenario_ShouldHandleComplexConfiguration operation.
    /// </summary>

    [Fact]
    public void Properties_WithPharmaceuticalManufacturingScenario_ShouldHandleComplexConfiguration()
    {
        // Arrange - Pharmaceutical tablet production with Siemens S7-1518F
        var instance = new VariablesView();

        // Act - cGMP compliant pharmaceutical manufacturing
        instance.VariableId = 9901;
        instance.MachineId = 990;
        instance.PlcId = 99;
        instance.Name = "TabletWeightSensor";
        instance.Description = "Tablet weight measurement for quality control";
        instance.Alias = "TAB_WEIGHT";
        instance.Address = "DB10.DBD100";
        instance.NetType = "REAL";
        instance.Length = 4;
        instance.IsActive = 1;
        instance.Direction = 0; // Read-only for safety
        instance.VariableGroupId = 99;

        // Assert
        instance.VariableId.ShouldBe(9901);
        instance.MachineId.ShouldBe(990);
        instance.PlcId.ShouldBe(99);
        instance.Name.ShouldBe("TabletWeightSensor");
        instance.Description.ShouldBe("Tablet weight measurement for quality control");
        instance.Alias.ShouldBe("TAB_WEIGHT");
        instance.Address.ShouldBe("DB10.DBD100");
        instance.NetType.ShouldBe("REAL");
        instance.Length.ShouldBe(4);
        instance.IsActive.ShouldBe(1);
        instance.Direction.ShouldBe(0);
        instance.VariableGroupId.ShouldBe(99);
    }

    /// <summary>
    /// Executes RoundTrip_Conversion_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void RoundTrip_Conversion_ShouldMaintainDataIntegrity()
    {
        // Arrange - Original Variable entity
        var originalVariable = new Variable
        {
            VariableId = 7701,
            MachineId = 770,
            PlcId = 77,
            Name = "MachineTypePlc",
            Description = "Machine type indicator",
            Alias = "MACH_TYPE",
            Address = "DB5.DBW50",
            NetType = "INT",
            Length = 2,
            IsActive = 1,
            Direction = 0,
            VariableGroupId = 77,
        };

        // Act - Convert to DTO and back to Entity
        var dtoWrapper = VariablesView.ToDto(originalVariable);
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;

        var convertedVariableWrapper = VariablesView.ToEntity(dto);
        convertedVariableWrapper.IsSuccess.ShouldBeTrue();
        convertedVariableWrapper.Value.ShouldNotBeNull();
        var convertedVariable = convertedVariableWrapper.Value;
        convertedVariable.ShouldNotBeNull();

        // Assert - Verify data integrity
        convertedVariable.VariableId.ShouldBe(originalVariable.VariableId);
        convertedVariable.MachineId.ShouldBe(originalVariable.MachineId);
        convertedVariable.PlcId.ShouldBe(originalVariable.PlcId);
        convertedVariable.Name.ShouldBe(originalVariable.Name);
        convertedVariable.Description.ShouldBe(originalVariable.Description);
        convertedVariable.Alias.ShouldBe(originalVariable.Alias);
        convertedVariable.Address.ShouldBe(originalVariable.Address);
        convertedVariable.NetType.ShouldBe(originalVariable.NetType);
        convertedVariable.Length.ShouldBe(originalVariable.Length);
        convertedVariable.IsActive.Value.ShouldBe(originalVariable.IsActive.Value);
        convertedVariable.Direction.ShouldBe(originalVariable.Direction);
        convertedVariable.VariableGroupId.ShouldBe(originalVariable.VariableGroupId);
    }
}
