// <copyright file="VariableListVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Unit tests for VariableListVm
/// </summary>
public class VariableListVmTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new VariableListVm();

        // Assert
        instance.ShouldNotBeNull();
        instance.VariableList.ShouldNotBeNull().ShouldBeEmpty(); // Field is not initialized by default
        instance.Count.ShouldBe(0);
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange - Ford F-150 production line variables
        var instance = new VariableListVm();
        var variableList = new List<VariableDto>
        {
            new() { VariableId = 1501, MachineId = 10001, Name = "CycleStartSignal", NetType = "BOOL" },
            new() { VariableId = 1502, MachineId = 10001, Name = "QualityStatus", NetType = "INT" },
            new() { VariableId = 1503, MachineId = 10002, Name = "TemperatureSensor", NetType = "REAL" }
        };
        const int expectedCount = 3;

        // Act
        instance.VariableList = variableList;
        instance.Count = expectedCount;

        // Assert
        instance.VariableList.ShouldNotBeNull();
        instance.VariableList.Count.ShouldBe(3);
        instance.Count.ShouldBe(expectedCount);
        instance.VariableList.First().Name.ShouldBe("CycleStartSignal");
        instance.VariableList.Last().Name.ShouldBe("TemperatureSensor");
    }
    /// <summary>
    /// Executes Count_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="expectedCount">The expectedCount.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Count_WhenSet_ShouldReturnCorrectValue(int expectedCount)
    {
        // Using parameters: expectedCount
        _ = expectedCount; // xUnit1026 fix
        // Using parameters: expectedCount
        _ = expectedCount; // xUnit1026 fix
        // Using parameters: expectedCount
        _ = expectedCount; // xUnit1026 fix
        // Using parameters: expectedCount
        _ = expectedCount; // xUnit1026 fix
        // Using parameters: expectedCount
        _ = expectedCount; // xUnit1026 fix
        // Arrange
        var instance = new VariableListVm();

        // Act
        instance.Count = expectedCount;

        // Assert
        instance.Count.ShouldBe(expectedCount);
    }
    /// <summary>
    /// Executes VariableList_WithElectronicsManufacturingScenario_ShouldHandleComplexVariables operation.
    /// </summary>

    [Fact]
    public void VariableList_WithElectronicsManufacturingScenario_ShouldHandleComplexVariables()
    {
        // Arrange - iPhone PCB production variables
        var instance = new VariableListVm();
        var electronicsVariables = new List<VariableDto>
        {
            new()
            {
                VariableId = 8801,
                MachineId = 880,
                PlcId = 88,
                Name = "PCB_InspectionResult",
                Description = "PCB quality inspection result",
                Address = "DM1000",
                NetType = "DINT",
                Length = 4
            },
            new()
            {
                VariableId = 8802,
                MachineId = 880,
                PlcId = 88,
                Name = "SMT_PlacementAccuracy",
                Description = "SMT component placement accuracy",
                Address = "DM1004",
                NetType = "REAL",
                Length = 4
            }
        };

        // Act
        instance.VariableList = electronicsVariables;
        instance.Count = electronicsVariables.Count;

        // Assert
        instance.VariableList.ShouldNotBeNull();
        instance.VariableList.Count.ShouldBe(2);
        instance.Count.ShouldBe(2);

        var firstVariable = instance.VariableList.First();
        firstVariable.Name.ShouldBe("PCB_InspectionResult");
        firstVariable.NetType.ShouldBe("DINT");

        var secondVariable = instance.VariableList.Last();
        secondVariable.Name.ShouldBe("SMT_PlacementAccuracy");
        secondVariable.NetType.ShouldBe("REAL");
    }
    /// <summary>
    /// Executes VariableList_WithPharmaceuticalManufacturingScenario_ShouldHandleRegulatedVariables operation.
    /// </summary>

    [Fact]
    public void VariableList_WithPharmaceuticalManufacturingScenario_ShouldHandleRegulatedVariables()
    {
        // Arrange - Pharmaceutical tablet production variables (cGMP compliant)
        var instance = new VariableListVm();
        var pharmaVariables = new List<VariableDto>
        {
            new()
            {
                VariableId = 9901,
                MachineId = 990,
                PlcId = 99,
                Name = "TabletWeightSensor",
                Description = "Tablet weight measurement (mg)",
                Address = "DB10.DBD100",
                NetType = "REAL",
                Length = 4
            },
            new()
            {
                VariableId = 9902,
                MachineId = 990,
                PlcId = 99,
                Name = "CompressionForce",
                Description = "Tablet compression force (kN)",
                Address = "DB10.DBD104",
                NetType = "REAL",
                Length = 4
            },
            new()
            {
                VariableId = 9903,
                MachineId = 990,
                PlcId = 99,
                Name = "QualityApproval",
                Description = "FDA quality approval status",
                Address = "DB10.DBX108.0",
                NetType = "BOOL",
                Length = 1
            }
        };

        // Act
        instance.VariableList = pharmaVariables;
        instance.Count = pharmaVariables.Count;

        // Assert
        instance.VariableList.ShouldNotBeNull();
        instance.VariableList.Count.ShouldBe(3);
        instance.Count.ShouldBe(3);

        var weightSensor = instance.VariableList.FirstOrDefault(v => v.Name == "TabletWeightSensor");
        weightSensor.ShouldNotBeNull();
        weightSensor.Description.ShouldBe("Tablet weight measurement (mg)");

        var qualityApproval = instance.VariableList.FirstOrDefault(v => v.Name == "QualityApproval");
        qualityApproval.ShouldNotBeNull();
        qualityApproval.NetType.ShouldBe("BOOL");
    }
    /// <summary>
    /// Executes VariableList_WithEmptyCollection_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void VariableList_WithEmptyCollection_ShouldHandleGracefully()
    {
        // Arrange
        var instance = new VariableListVm();
        var emptyVariableList = new List<VariableDto>();

        // Act
        instance.VariableList = emptyVariableList;
        instance.Count = 0;

        // Assert
        instance.VariableList.ShouldNotBeNull();
        instance.VariableList.ShouldBeEmpty();
        instance.Count.ShouldBe(0);
    }
    /// <summary>
    /// Executes VariableList_WithNullAssignment_ShouldAcceptNull operation.
    /// </summary>

    [Fact]
    public void VariableList_WithNullAssignment_ShouldAcceptNull()
    {
        // Arrange
        var instance = new VariableListVm();

        // Act
        instance.VariableList = null!;
        instance.Count = 0;

        // Assert
        instance.VariableList.ShouldBeNull();
        instance.Count.ShouldBe(0);
    }
    /// <summary>
    /// Executes VariableList_WithAutomotiveManufacturingScenario_ShouldHandleRoboticsVariables operation.
    /// </summary>

    [Fact]
    public void VariableList_WithAutomotiveManufacturingScenario_ShouldHandleRoboticsVariables()
    {
        // Arrange - Ford F-150 robotic welding cell variables
        var instance = new VariableListVm();
        var automotiveVariables = new List<VariableDto>
        {
            new()
            {
                VariableId = 1501,
                MachineId = 10001,
                PlcId = 15,
                Name = "WeldingCurrent",
                Description = "Robotic welding current (A)",
                Alias = "WELD_CURR",
                Address = "DB1.DBW10",
                NetType = "INT",
                Length = 2
            },
            new()
            {
                VariableId = 1502,
                MachineId = 10001,
                PlcId = 15,
                Name = "WeldingVoltage",
                Description = "Robotic welding voltage (V)",
                Alias = "WELD_VOLT",
                Address = "DB1.DBW12",
                NetType = "INT",
                Length = 2
            },
            new()
            {
                VariableId = 1503,
                MachineId = 10001,
                PlcId = 15,
                Name = "WeldQualityOK",
                Description = "Weld quality approval",
                Alias = "WELD_OK",
                Address = "DB1.DBX14.0",
                NetType = "BOOL",
                Length = 1
            }
        };

        // Act
        instance.VariableList = automotiveVariables;
        instance.Count = automotiveVariables.Count;

        // Assert
        instance.VariableList.ShouldNotBeNull();
        instance.VariableList.Count.ShouldBe(3);
        instance.Count.ShouldBe(3);

        var weldingCurrent = instance.VariableList.FirstOrDefault(v => v.Name == "WeldingCurrent");
        weldingCurrent.ShouldNotBeNull();
        weldingCurrent.Alias.ShouldBe("WELD_CURR");

        var qualityCheck = instance.VariableList.FirstOrDefault(v => v.Name == "WeldQualityOK");
        qualityCheck.ShouldNotBeNull();
        qualityCheck.NetType.ShouldBe("BOOL");
    }
    /// <summary>
    /// Executes Properties_WithMismatchedCountAndCollection_ShouldAllowInconsistency operation.
    /// </summary>

    [Fact]
    public void Properties_WithMismatchedCountAndCollection_ShouldAllowInconsistency()
    {
        // Arrange - Testing that Count property is independent of collection size
        var instance = new VariableListVm();
        var variableList = new List<VariableDto>
        {
            new() { VariableId = 1, Name = "Variable1" },
            new() { VariableId = 2, Name = "Variable2" }
        };

        // Act - Intentionally set mismatched count
        instance.VariableList = variableList;
        instance.Count = 10; // Different from actual collection count

        // Assert - Both properties should retain their set values independently
        instance.VariableList.Count.ShouldBe(2);
        instance.Count.ShouldBe(10);
    }
}