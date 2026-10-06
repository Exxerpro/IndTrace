// <copyright file="MachineConfigVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.ViewModels;

/// <summary>
/// Unit tests for MachineConfigVm
/// </summary>
public class MachineConfigVmTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new MachineConfigVm();

        // Assert
        instance.ShouldNotBeNull();
        instance.Machines.ShouldNotBeNull();
        instance.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new MachineConfigVm();
        var machineConfigs = new List<MachineConfigDto>();

        // Act
        instance.Machines = machineConfigs;
        instance.Count = 5;

        // Assert
        instance.Machines.ShouldBe(machineConfigs);
        instance.Count.ShouldBe(5);
    }

    /// <summary>
    /// Executes Maquinas_WhenInitializedWithAutomotiveEquipment_ShouldContainCorrectMachines operation.
    /// </summary>

    [Fact]
    public void Maquinas_WhenInitializedWithAutomotiveEquipment_ShouldContainCorrectMachines()
    {
        // Arrange
        var instance = new MachineConfigVm();
        var fordWeldingCell = new MachineConfigDto
        {
            Id = 1,
            MachineId = 10001,
            Name = "Robotic Welding Cell #1",
            Location = "Ford Rouge Plant - Body Shop",
            MachineType = 8, // Process
            IpAddress = "192.168.1.100",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var fanucRobot = new MachineConfigDto
        {
            Id = 2,
            MachineId = 10002,
            Name = "Fanuc R-2000iC/210F",
            Location = "Ford Rouge Plant - Assembly Line 1",
            MachineType = 8, // Process
            IpAddress = "192.168.1.101",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        instance.Machines.Add(fordWeldingCell);
        instance.Machines.Add(fanucRobot);
        instance.Count = instance.Machines.Count;

        // Assert
        instance.Machines.Count.ShouldBe(2);
        instance.Count.ShouldBe(2);
        instance.Machines.ShouldContain(m => m.Name == "Robotic Welding Cell #1");
        instance.Machines.ShouldContain(m => m.Name == "Fanuc R-2000iC/210F");
    }

    /// <summary>
    /// Executes Maquinas_WhenInitializedWithElectronicsEquipment_ShouldContainCorrectMachines operation.
    /// </summary>

    [Fact]
    public void Maquinas_WhenInitializedWithElectronicsEquipment_ShouldContainCorrectMachines()
    {
        // Arrange
        var instance = new MachineConfigVm();
        var smtPickPlace = new MachineConfigDto
        {
            Id = 3,
            MachineId = 201,
            Name = "SMT Pick & Place Machine",
            Location = "Apple Foxconn - PCB Assembly",
            MachineType = 8, // Process
            IpAddress = "192.168.2.100",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var aoiInspection = new MachineConfigDto
        {
            Id = 4,
            MachineId = 202,
            Name = "Cognex Vision System",
            Location = "Apple Foxconn - Quality Control",
            MachineType = 32, // Inspection
            IpAddress = "192.168.2.101",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        instance.Machines.Add(smtPickPlace);
        instance.Machines.Add(aoiInspection);
        instance.Count = instance.Machines.Count;

        // Assert
        instance.Machines.Count.ShouldBe(2);
        instance.Count.ShouldBe(2);
        instance.Machines.ShouldContain(m => m.MachineType == 8);  // Process
        instance.Machines.ShouldContain(m => m.MachineType == 32); // Inspection
    }

    /// <summary>
    /// Executes Count_WhenSetToValidValue_ShouldReturnExpectedValue operation.
    /// </summary>
    /// <param name="count">The count.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(50)]
    public void Count_WhenSetToValidValue_ShouldReturnExpectedValue(int count)
    {
        // Using parameters: count
        _ = count; // xUnit1026 fix
        // Using parameters: count
        _ = count; // xUnit1026 fix
        // Using parameters: count
        _ = count; // xUnit1026 fix
        // Using parameters: count
        _ = count; // xUnit1026 fix
        // Using parameters: count
        _ = count; // xUnit1026 fix
        // Arrange
        var instance = new MachineConfigVm();

        // Act
        instance.Count = count;

        // Assert
        instance.Count.ShouldBe(count);
    }

    /// <summary>
    /// Executes Maquinas_WhenClearedAndRepopulated_ShouldReflectChanges operation.
    /// </summary>

    [Fact]
    public void Maquinas_WhenClearedAndRepopulated_ShouldReflectChanges()
    {
        // Arrange
        var instance = new MachineConfigVm();
        var initialMachine = new MachineConfigDto { Id = 1, MachineId = 10001 };
        instance.Machines.Add(initialMachine);

        // Act - Clear and add new machines (Pharmaceutical Manufacturing)
        instance.Machines.Clear();
        var tabletPress = new MachineConfigDto
        {
            Id = 5,
            MachineId = 301,
            Name = "Pharmaceutical Tablet Press",
            Location = "Pfizer - Tablet Manufacturing",
            MachineType = 8, // Process
            IpAddress = "192.168.3.100",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var fillingMachine = new MachineConfigDto
        {
            Id = 6,
            MachineId = 302,
            Name = "Vaccine Filling Machine",
            Location = "Pfizer - Vial Filling Line",
            MachineType = 8, // Process
            IpAddress = "192.168.3.101",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        instance.Machines.Add(tabletPress);
        instance.Machines.Add(fillingMachine);
        instance.Count = instance.Machines.Count;

        // Assert
        instance.Machines.Count.ShouldBe(2);
        instance.Count.ShouldBe(2);
        instance.Machines.ShouldNotContain(m => m.Name == "Initial Machine");
        instance.Machines.ShouldContain(m => m.Name == "Pharmaceutical Tablet Press");
        instance.Machines.ShouldContain(m => m.Name == "Vaccine Filling Machine");
    }

    /// <summary>
    /// Executes Maquinas_WhenContainingComplexCollections_ShouldMaintainRelationships operation.
    /// </summary>

    [Fact]
    public void Maquinas_WhenContainingComplexCollections_ShouldMaintainRelationships()
    {
        // Arrange
        var instance = new MachineConfigVm();
        var cncMachine = new MachineConfigDto
        {
            Id = 7,
            MachineId = 401,
            Name = "Haas VF-4SS CNC",
            Location = "Ford Engine Plant - Machining",
            MachineType = 8, // Process
            IpAddress = "192.168.4.100",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            Variables =
            [
                new() { Name="SpindleSpeed", VariableId = 1, Address = "DB1.DBW0" },
                new() { Name="FeedRate", VariableId = 2, Address = "DB1.DBW2" }
            ],
            WorkFlows =
            [
                new() { WorkFlowId = 1 }
            ]
        };

        // Act
        instance.Machines.Add(cncMachine);
        instance.Count = instance.Machines.Count;

        // Assert
        instance.Machines.Count.ShouldBe(1);
        instance.Count.ShouldBe(1);
        var machine = instance.Machines.First();
        machine.Variables.Count.ShouldBe(2);
        machine.WorkFlows.Count.ShouldBe(1);
        machine.Variables.ShouldContain(v => v.Name == "SpindleSpeed");
        machine.WorkFlows.Count.ShouldBe(1);
    }

    /// <summary>
    /// Executes Instance_WhenFullyConfiguredForProductionLine_ShouldRepresentCompleteSystem operation.
    /// </summary>

    [Fact]
    public void Instance_WhenFullyConfiguredForProductionLine_ShouldRepresentCompleteSystem()
    {
        // Arrange & Act - Complete Ford F-150 Assembly Line Configuration
        var instance = new MachineConfigVm
        {
            Machines =
            [
                new() { Id = 1, MachineId = 10000, MachineType = 2 }, // Initial
                new() { Id = 2, MachineId = 10001, MachineType = 8 }, // Process
                new() { Id = 3, MachineId = 10002, MachineType = 32 }, // Inspection
                new() { Id = 4, MachineId = 10003, MachineType = 16 } // Final
            ]
        };
        instance.Count = instance.Machines.Count;

        // Assert
        instance.Machines.Count.ShouldBe(4);
        instance.Count.ShouldBe(4);
        instance.Machines.ShouldContain(m => m.MachineType == 2);  // Initial
        instance.Machines.ShouldContain(m => m.MachineType == 8);  // Process
        instance.Machines.ShouldContain(m => m.MachineType == 32); // Inspection
        instance.Machines.ShouldContain(m => m.MachineType == 16); // Final
    }
}