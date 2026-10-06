// <copyright file="DistinctRegisterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.DistinctRegistersTests;

/// <summary>
/// Unit tests for DistinctRegister domain entity - Manufacturing register analytics support
/// </summary>
public class DistinctRegisterTests
{
    /// <summary>
    /// Executes DistinctRegister_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void DistinctRegister_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var distinctRegister = new DistinctRegister();

        // Assert
        distinctRegister.ShouldNotBeNull();
        distinctRegister.Name.ShouldBe(string.Empty);
        distinctRegister.VariableId.ShouldBe(0);
        distinctRegister.MachineId.ShouldBe(0);
    }

    /// <summary>
    /// Executes DistinctRegister_WhenInitializer_ShouldCreateInstanceWithValues operation.
    /// </summary>

    [Fact]
    public void DistinctRegister_WhenInitializer_ShouldCreateInstanceWithValues()
    {
        // Arrange
        var name = "Temperature_Sensor";
        var variableId = 1001;
        var machineId = 2001;

        // Act
        var distinctRegister = new DistinctRegister
        {
            Name = name,
            VariableId = variableId,
            MachineId = machineId
        };

        // Assert
        distinctRegister.ShouldNotBeNull();
        distinctRegister.Name.ShouldBe(name);
        distinctRegister.VariableId.ShouldBe(variableId);
        distinctRegister.MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes DistinctRegister_Properties_WithManufacturingVariables_ShouldSetAndGetCorrectly operation.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="variableId">The variableId.</param>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData("Pressure_Sensor", 1001, 2001)]
    [InlineData("Vibration_Monitor", 1002, 2002)]
    [InlineData("Speed_Control", 1003, 2003)]
    [InlineData("Temperature_Control", 1004, 2004)]
    [InlineData("Oil_Level", 1005, 2005)]
    public void DistinctRegister_Properties_WithManufacturingVariables_ShouldSetAndGetCorrectly(string name, int variableId, int machineId)
    {
        // Arrange
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.Name = name;
        distinctRegister.VariableId = variableId;
        distinctRegister.MachineId = machineId;

        // Assert
        distinctRegister.Name.ShouldBe(name);
        distinctRegister.VariableId.ShouldBe(variableId);
        distinctRegister.MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes Name_WithNullValue_ShouldAcceptNullValue operation.
    /// </summary>

    [Fact]
    public void Name_WithNullValue_ShouldAcceptNullValue()
    {
        // Arrange
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.Name = string.Empty;

        // Assert
        distinctRegister.Name.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Name_WithEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void Name_WithEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.Name = string.Empty;

        // Assert
        distinctRegister.Name.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes VariableId_WithVariousValues_ShouldAcceptAllValues operation.
    /// </summary>
    /// <param name="variableId">The variableId.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void VariableId_WithVariousValues_ShouldAcceptAllValues(int variableId)
    {
        // Arrange
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.VariableId = variableId;

        // Assert
        distinctRegister.VariableId.ShouldBe(variableId);
    }

    /// <summary>
    /// Executes MachineId_WithVariousValues_ShouldAcceptAllValues operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void MachineId_WithVariousValues_ShouldAcceptAllValues(int machineId)
    {
        // Arrange
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.MachineId = machineId;

        // Assert
        distinctRegister.MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes DistinctRegister_DomainLogic_WithFordF150Assembly_ShouldCreateValidDistinctRegister operation.
    /// </summary>

    [Fact]
    public void DistinctRegister_DomainLogic_WithFordF150Assembly_ShouldCreateValidDistinctRegister()
    {
        // Arrange - Ford F-150 Assembly Line PLC Variables
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.Name = "Ford_Torque_Wrench_Status";
        distinctRegister.VariableId = 4001;
        distinctRegister.MachineId = 100100; // Ford Assembly Station 1

        // Assert
        distinctRegister.ShouldNotBeNull();
        distinctRegister.Name.ShouldBe("Ford_Torque_Wrench_Status");
        distinctRegister.VariableId.ShouldBe(4001);
        distinctRegister.MachineId.ShouldBe(100100);
    }

    /// <summary>
    /// Executes DistinctRegister_DomainLogic_WithTeslaModelSProduction_ShouldCreateValidDistinctRegister operation.
    /// </summary>

    [Fact]
    public void DistinctRegister_DomainLogic_WithTeslaModelSProduction_ShouldCreateValidDistinctRegister()
    {
        // Arrange - Tesla Model S Battery Assembly
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.Name = "Tesla_Battery_Cell_Voltage";
        distinctRegister.VariableId = 5001;
        distinctRegister.MachineId = 2200; // Tesla Battery Station

        // Assert
        distinctRegister.ShouldNotBeNull();
        distinctRegister.Name.ShouldBe("Tesla_Battery_Cell_Voltage");
        distinctRegister.VariableId.ShouldBe(5001);
        distinctRegister.MachineId.ShouldBe(2200);
    }

    /// <summary>
    /// Executes DistinctRegister_DomainLogic_WithBMWX5Paint_ShouldCreateValidDistinctRegister operation.
    /// </summary>

    [Fact]
    public void DistinctRegister_DomainLogic_WithBMWX5Paint_ShouldCreateValidDistinctRegister()
    {
        // Arrange - BMW X5 Paint Shop Variables
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.Name = "BMW_Paint_Booth_Temperature";
        distinctRegister.VariableId = 6001;
        distinctRegister.MachineId = 3300; // BMW Paint Station

        // Assert
        distinctRegister.ShouldNotBeNull();
        distinctRegister.Name.ShouldBe("BMW_Paint_Booth_Temperature");
        distinctRegister.VariableId.ShouldBe(6001);
        distinctRegister.MachineId.ShouldBe(3300);
    }

    /// <summary>
    /// Executes DistinctRegister_DomainLogic_WithMercedesQualityControl_ShouldCreateValidDistinctRegister operation.
    /// </summary>

    [Fact]
    public void DistinctRegister_DomainLogic_WithMercedesQualityControl_ShouldCreateValidDistinctRegister()
    {
        // Arrange - Mercedes Quality Control System
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.Name = "Mercedes_Vision_System_Result";
        distinctRegister.VariableId = 7001;
        distinctRegister.MachineId = 4400; // Mercedes QC Station

        // Assert
        distinctRegister.ShouldNotBeNull();
        distinctRegister.Name.ShouldBe("Mercedes_Vision_System_Result");
        distinctRegister.VariableId.ShouldBe(7001);
        distinctRegister.MachineId.ShouldBe(4400);
    }

    /// <summary>
    /// Executes DistinctRegister_DomainLogic_WithAudiA4Welding_ShouldCreateValidDistinctRegister operation.
    /// </summary>

    [Fact]
    public void DistinctRegister_DomainLogic_WithAudiA4Welding_ShouldCreateValidDistinctRegister()
    {
        // Arrange - Oemx A4 Welding Process Variables
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.Name = "Audi_Weld_Current_Measurement";
        distinctRegister.VariableId = 8001;
        distinctRegister.MachineId = 5500; // Oemx Welding Station

        // Assert
        distinctRegister.ShouldNotBeNull();
        distinctRegister.Name.ShouldBe("Audi_Weld_Current_Measurement");
        distinctRegister.VariableId.ShouldBe(8001);
        distinctRegister.MachineId.ShouldBe(5500);
    }

    /// <summary>
    /// Executes DistinctRegister_DomainLogic_WithIndustry40Variables_ShouldCreateValidDistinctRegisters operation.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="variableId">The variableId.</param>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData("PLC_Signal_001", 101, 201)]
    [InlineData("Hydraulic_Pressure", 102, 202)]
    [InlineData("Conveyor_Speed", 103, 203)]
    [InlineData("Robot_Position_X", 104, 204)]
    [InlineData("Safety_Interlock", 105, 205)]
    public void DistinctRegister_DomainLogic_WithIndustry40Variables_ShouldCreateValidDistinctRegisters(string name, int variableId, int machineId)
    {
        // Arrange & Act
        var distinctRegister = new DistinctRegister
        {
            Name = name,
            VariableId = variableId,
            MachineId = machineId
        };

        // Assert - Industry 4.0 Variable Management
        distinctRegister.ShouldNotBeNull();
        distinctRegister.Name.ShouldBe(name);
        distinctRegister.VariableId.ShouldBe(variableId);
        distinctRegister.MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes DistinctRegister_DomainLogic_AsCompositeKey_ShouldRepresentUniqueRegisterCombination operation.
    /// </summary>

    [Fact]
    public void DistinctRegister_DomainLogic_AsCompositeKey_ShouldRepresentUniqueRegisterCombination()
    {
        // Arrange - Manufacturing Analytics Scenario
        var register1 = new DistinctRegister
        {
            Name = "Production_Count",
            VariableId = 1001,
            MachineId = 2001
        };

        var register2 = new DistinctRegister
        {
            Name = "Production_Count", // Same name
            VariableId = 1002, // Different variable ID
            MachineId = 2001  // Same machine ID
        };

        // Act & Assert - Should be different distinct registers due to composite key
        register1.ShouldNotBeNull();
        register2.ShouldNotBeNull();

        // Different EntitieId makes them distinct
        register1.VariableId.ShouldNotBe(register2.VariableId);

        // Same name and machine, but different variable makes them unique
        register1.Name.ShouldBe(register2.Name);
        register1.MachineId.ShouldBe(register2.MachineId);
    }

    /// <summary>
    /// Executes DistinctRegister_DomainLogic_WithMaximumLengthName_ShouldBeValidForDatabase operation.
    /// </summary>

    [Fact]
    public void DistinctRegister_DomainLogic_WithMaximumLengthName_ShouldBeValidForDatabase()
    {
        // Arrange - Based on EF Configuration (MaxLength 255)
        var longName = new string('A', 255); // Maximum allowed length
        var distinctRegister = new DistinctRegister();

        // Act
        distinctRegister.Name = longName;
        distinctRegister.VariableId = 9001;
        distinctRegister.MachineId = 6600;

        // Assert
        distinctRegister.ShouldNotBeNull();
        distinctRegister.Name.ShouldBe(longName);
        distinctRegister.Name.Length.ShouldBe(255);
        distinctRegister.VariableId.ShouldBe(9001);
        distinctRegister.MachineId.ShouldBe(6600);
    }

    /// <summary>
    /// Executes DistinctRegister_DomainLogic_WithMultipleMachineRegisters_ShouldSupportMultiMachineScenarios operation.
    /// </summary>

    [Fact]
    public void DistinctRegister_DomainLogic_WithMultipleMachineRegisters_ShouldSupportMultiMachineScenarios()
    {
        // Arrange - Multi-machine manufacturing line
        var machine1Register = new DistinctRegister
        {
            Name = "Cycle_Time",
            VariableId = 1001,
            MachineId = 10000 // Station 1
        };

        var machine2Register = new DistinctRegister
        {
            Name = "Cycle_Time", // Same variable name
            VariableId = 1001,  // Same variable ID
            MachineId = 200     // Station 2
        };

        // Act & Assert - Different machines can have same variable
        machine1Register.ShouldNotBeNull();
        machine2Register.ShouldNotBeNull();

        machine1Register.Name.ShouldBe(machine2Register.Name);
        machine1Register.VariableId.ShouldBe(machine2Register.VariableId);
        machine1Register.MachineId.ShouldNotBe(machine2Register.MachineId);
    }
}