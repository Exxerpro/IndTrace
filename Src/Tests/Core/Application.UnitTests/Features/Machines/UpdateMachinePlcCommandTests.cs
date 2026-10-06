// <copyright file="UpdateMachinePlcCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Unit tests for UpdateMachinePlcCommand
/// </summary>
public class UpdateMachinePlcCommandTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var command = new UpdateMachinePlcCommand();

        // Assert
        command.ShouldNotBeNull();
        command.MachineId.ShouldBe(0);
        command.PlcId.ShouldBe(0);
        command.IsActive.ShouldBeNull();
        command.NewMachineId.ShouldBeNull();
        command.NewPlcId.ShouldBeNull();
    }
    /// <summary>
    /// Executes Properties_WhenSetWithValidValues_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithValidValues_ShouldReturnCorrectValues()
    {
        // Arrange - Siemens S7-1500 PLC connection update for Ford F-150 welding station
        var command = new UpdateMachinePlcCommand();
        const int expectedMachineId = 100501;
        const int expectedPlcId = 101;
        const int expectedIsActive = 1;
        const int expectedNewMachineId = 100502;
        const int expectedNewPlcId = 102;

        // Act
        command.MachineId = expectedMachineId;
        command.PlcId = expectedPlcId;
        command.IsActive = expectedIsActive;
        command.NewMachineId = expectedNewMachineId;
        command.NewPlcId = expectedNewPlcId;

        // Assert
        command.MachineId.ShouldBe(expectedMachineId);
        command.PlcId.ShouldBe(expectedPlcId);
        command.IsActive.ShouldBe(expectedIsActive);
        command.NewMachineId.ShouldBe(expectedNewMachineId);
        command.NewPlcId.ShouldBe(expectedNewPlcId);
    }
    /// <summary>
    /// Executes Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <param name="isActive">The isActive.</param>
    /// <param name="newMachineId">The newMachineId.</param>
    /// <param name="newPlcId">The newPlcId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1501, 101, 1, 1502, 102, "Ford F-150 Welding Cell PLC Update")]
    [InlineData(2801, 201, 1, 2802, 202, "Tesla Model S Battery Assembly PLC Upgrade")]
    [InlineData(3301, 301, 0, 3302, 302, "iPhone PCB SMT Line PLC Reconfiguration")]
    [InlineData(4401, 401, 1, 4402, 402, "Pfizer Vaccine Filling Station PLC Migration")]
    [InlineData(5501, 501, 1, 5502, 502, "Coca-Cola Bottling Line PLC Replacement")]
    public void Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly(int machineId, int plcId, int isActive, int newMachineId, int newPlcId, string scenario)
    {
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, scenario
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand();

        // Act
        command.MachineId = machineId;
        command.PlcId = plcId;
        command.IsActive = isActive;
        command.NewMachineId = newMachineId;
        command.NewPlcId = newPlcId;

        // Assert
        command.MachineId.ShouldBe(machineId);
        command.PlcId.ShouldBe(plcId);
        command.IsActive.ShouldBe(isActive);
        command.NewMachineId.ShouldBe(newMachineId);
        command.NewPlcId.ShouldBe(newPlcId);
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void UpdateMachinePlcCommand_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Ford F-150 robotic welding cell PLC connection update
        var command = new UpdateMachinePlcCommand
        {
            MachineId = 100501, // Robotic Welding Cell #1
            PlcId = 101,      // Siemens S7-1516
            IsActive = 1,     // Active connection
            NewMachineId = 100502, // Upgraded Welding Cell #2
            NewPlcId = 102       // New Siemens S7-1518
        };

        // Act & Assert
        command.MachineId.ShouldBe(100501);
        command.PlcId.ShouldBe(101);
        command.IsActive.ShouldBe(1);
        command.NewMachineId.ShouldBe(100502);
        command.NewPlcId.ShouldBe(102);
        command.ShouldBeAssignableTo<IMonitorRequest<MachinePlcDetailVm>>();
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void UpdateMachinePlcCommand_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - iPhone PCB SMT line PLC configuration update
        var command = new UpdateMachinePlcCommand
        {
            MachineId = 3301, // SMT Pick & Place Machine
            PlcId = 301,      // Mitsubishi Q-Series
            IsActive = 1,     // Active production
            NewMachineId = 3302, // Upgraded SMT Machine
            NewPlcId = 302       // New Mitsubishi iQ-R Series
        };

        // Act & Assert
        command.MachineId.ShouldBe(3301);
        command.PlcId.ShouldBe(301);
        command.IsActive.ShouldBe(1);
        command.NewMachineId.ShouldBe(3302);
        command.NewPlcId.ShouldBe(302);
        command.ShouldBeAssignableTo<IMonitorRequest<MachinePlcDetailVm>>();
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_WithPharmaceuticalManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void UpdateMachinePlcCommand_WithPharmaceuticalManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Pfizer vaccine production filling station PLC update
        var command = new UpdateMachinePlcCommand
        {
            MachineId = 4401, // Vaccine Filling Station
            PlcId = 401,      // Schneider Modicon M580
            IsActive = 1,     // Active GMP production
            NewMachineId = 4402, // New FDA-compliant Filling Station
            NewPlcId = 402       // Upgraded Modicon M580
        };

        // Act & Assert
        command.MachineId.ShouldBe(4401);
        command.PlcId.ShouldBe(401);
        command.IsActive.ShouldBe(1);
        command.NewMachineId.ShouldBe(4402);
        command.NewPlcId.ShouldBe(402);
        command.ShouldBeAssignableTo<IMonitorRequest<MachinePlcDetailVm>>();
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_AsIMonitorRequest_ShouldImplementInterface operation.
    /// </summary>

    [Fact]
    public void UpdateMachinePlcCommand_AsIMonitorRequest_ShouldImplementInterface()
    {
        // Arrange & Act
        var command = new UpdateMachinePlcCommand();

        // Assert
        command.ShouldBeAssignableTo<IMonitorRequest<MachinePlcDetailVm>>();
    }
    /// <summary>
    /// Executes MachineId_WhenSetToDifferentValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="setValue">The setValue.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(999, 999)]
    [InlineData(-1, -1)]
    public void MachineId_WhenSetToDifferentValues_ShouldReturnCorrectValue(int setValue, int expectedValue)
    {
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand();

        // Act
        command.MachineId = setValue;

        // Assert
        command.MachineId.ShouldBe(expectedValue);
    }
    /// <summary>
    /// Executes PlcId_WhenSetToValidPlcIds_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="setValue">The setValue.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(101, 101)] // Siemens S7-1516
    [InlineData(201, 201)] // Allen-Bradley ControlLogix
    [InlineData(301, 301)] // Mitsubishi Q-Series
    [InlineData(401, 401)] // Schneider Modicon M580
    [InlineData(501, 501)] // ABB AC500
    public void PlcId_WhenSetToValidPlcIds_ShouldReturnCorrectValue(int setValue, int expectedValue)
    {
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Using parameters: setValue, expectedValue
        _ = setValue; // xUnit1026 fix
        _ = expectedValue; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand();

        // Act
        command.PlcId = setValue;

        // Assert
        command.PlcId.ShouldBe(expectedValue);
    }
    /// <summary>
    /// Executes IsActive_WhenSetToValidValues_ShouldIndicateConnectionStatus operation.
    /// </summary>
    /// <param name="setValue">The setValue.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(0, "Inactive")]
    [InlineData(1, "Active")]
    [InlineData(null, "Not specified")]
    public void IsActive_WhenSetToValidValues_ShouldIndicateConnectionStatus(int? setValue, string description)
    {
        // Using parameters: setValue, description
        _ = setValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: setValue, description
        _ = setValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: setValue, description
        _ = setValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: setValue, description
        _ = setValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: setValue, description
        _ = setValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand();

        // Act
        command.IsActive = setValue;

        // Assert
        command.IsActive.ShouldBe(setValue);
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_WithNullOptionalValues_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void UpdateMachinePlcCommand_WithNullOptionalValues_ShouldHandleGracefully()
    {
        // Arrange
        var command = new UpdateMachinePlcCommand();

        // Act
        command.MachineId = 100501;
        command.PlcId = 101;
        command.IsActive = null!;
        command.NewMachineId = null!;
        command.NewPlcId = null!;

        // Assert
        command.MachineId.ShouldBe(100501);
        command.PlcId.ShouldBe(101);
        command.IsActive.ShouldBeNull();
        command.NewMachineId.ShouldBeNull();
        command.NewPlcId.ShouldBeNull();
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_WithMigrationScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void UpdateMachinePlcCommand_WithMigrationScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Industrial 4.0 upgrade: Legacy PLC to modern PLC migration
        var command = new UpdateMachinePlcCommand
        {
            MachineId = 100501,     // Legacy Welding Cell
            PlcId = 101,          // Old Siemens S7-300
            IsActive = 0,         // Deactivating old connection
            NewMachineId = 100501,  // Same machine, different PLC
            NewPlcId = 201        // New Siemens S7-1500 with OPC-UA
        };

        // Act & Assert
        command.MachineId.ShouldBe(100501);
        command.PlcId.ShouldBe(101);
        command.IsActive.ShouldBe(0);
        command.NewMachineId.ShouldBe(100501);
        command.NewPlcId.ShouldBe(201);
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_WithProductionLineReconfiguration_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void UpdateMachinePlcCommand_WithProductionLineReconfiguration_ShouldHandleCorrectly()
    {
        // Arrange - Production line reorganization: Machine reassignment
        var command = new UpdateMachinePlcCommand
        {
            MachineId = 2801,     // Tesla Battery Assembly Station A
            PlcId = 201,          // Allen-Bradley ControlLogix
            IsActive = 1,         // Active transfer
            NewMachineId = 2802,  // Tesla Battery Assembly Station B
            NewPlcId = 201        // Same PLC, different machine assignment
        };

        // Act & Assert
        command.MachineId.ShouldBe(2801);
        command.PlcId.ShouldBe(201);
        command.IsActive.ShouldBe(1);
        command.NewMachineId.ShouldBe(2802);
        command.NewPlcId.ShouldBe(201);
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_WithEdgeCaseScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <param name="isActive">The isActive.</param>
    /// <param name="newMachineId">The newMachineId.</param>
    /// <param name="newPlcId">The newPlcId.</param>

    [Theory]
    [InlineData(1501, 101, 1, 1502, 102)]
    [InlineData(2801, 201, 0, 2802, 202)]
    [InlineData(3301, 301, 1, null, 302)]
    [InlineData(4401, 401, null, 4402, null)]
    public void UpdateMachinePlcCommand_WithEdgeCaseScenarios_ShouldHandleCorrectly(int machineId, int plcId, int? isActive, int? newMachineId, int? newPlcId)
    {
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand();

        // Act
        command.MachineId = machineId;
        command.PlcId = plcId;
        command.IsActive = isActive;
        command.NewMachineId = newMachineId;
        command.NewPlcId = newPlcId;

        // Assert
        command.MachineId.ShouldBe(machineId);
        command.PlcId.ShouldBe(plcId);
        command.IsActive.ShouldBe(isActive);
        command.NewMachineId.ShouldBe(newMachineId);
        command.NewPlcId.ShouldBe(newPlcId);
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_PropertyRoundTrip_ShouldMaintainValues operation.
    /// </summary>

    [Fact]
    public void UpdateMachinePlcCommand_PropertyRoundTrip_ShouldMaintainValues()
    {
        // Arrange
        var command = new UpdateMachinePlcCommand();
        const int originalMachineId = 6601;
        const int originalPlcId = 601;
        const int originalIsActive = 1;
        const int originalNewMachineId = 6602;
        const int originalNewPlcId = 602;

        // Act
        command.MachineId = originalMachineId;
        command.PlcId = originalPlcId;
        command.IsActive = originalIsActive;
        command.NewMachineId = originalNewMachineId;
        command.NewPlcId = originalNewPlcId;

        // Assert
        command.MachineId.ShouldBe(originalMachineId);
        command.PlcId.ShouldBe(originalPlcId);
        command.IsActive.ShouldBe(originalIsActive);
        command.NewMachineId.ShouldBe(originalNewMachineId);
        command.NewPlcId.ShouldBe(originalNewPlcId);
    }
    /// <summary>
    /// Executes UpdateMachinePlcCommand_WithManufacturingIndustryScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <param name="isActive">The isActive.</param>
    /// <param name="newMachineId">The newMachineId.</param>
    /// <param name="newPlcId">The newPlcId.</param>
    /// <param name="industry">The industry.</param>
    /// <param name="equipment">The equipment.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(ManufacturingPlcUpdateScenarios))]
    public void UpdateMachinePlcCommand_WithManufacturingIndustryScenarios_ShouldHandleCorrectly(int machineId, int plcId, int isActive, int newMachineId, int newPlcId, string industry, string equipment, string description)
    {
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, industry, equipment, description
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = industry; // xUnit1026 fix
        _ = equipment; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, industry, equipment, description
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = industry; // xUnit1026 fix
        _ = equipment; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, industry, equipment, description
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = industry; // xUnit1026 fix
        _ = equipment; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, industry, equipment, description
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = industry; // xUnit1026 fix
        _ = equipment; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, plcId, isActive, newMachineId, newPlcId, industry, equipment, description
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = isActive; // xUnit1026 fix
        _ = newMachineId; // xUnit1026 fix
        _ = newPlcId; // xUnit1026 fix
        _ = industry; // xUnit1026 fix
        _ = equipment; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var command = new UpdateMachinePlcCommand();

        // Act
        command.MachineId = machineId;
        command.PlcId = plcId;
        command.IsActive = isActive;
        command.NewMachineId = newMachineId;
        command.NewPlcId = newPlcId;

        // Assert
        command.MachineId.ShouldBe(machineId);
        command.PlcId.ShouldBe(plcId);
        command.IsActive.ShouldBe(isActive);
        command.NewMachineId.ShouldBe(newMachineId);
        command.NewPlcId.ShouldBe(newPlcId);
        command.ShouldBeAssignableTo<IMonitorRequest<MachinePlcDetailVm>>();
    }
    /// <summary>
    /// Executes ManufacturingPlcUpdateScenarios operation.
    /// </summary>
    /// <returns>The result of ManufacturingPlcUpdateScenarios.</returns>

    public static IEnumerable<object[]> ManufacturingPlcUpdateScenarios()
    {
        yield return new object[] { 1501, 101, 1, 1502, 102, "Automotive", "Siemens S7-1516", "Ford F-150 Welding Cell PLC Upgrade" };
        yield return new object[] { 2801, 201, 1, 2802, 202, "Automotive", "Allen-Bradley ControlLogix", "Tesla Model S Battery Assembly PLC Migration" };
        yield return new object[] { 3301, 301, 0, 3302, 302, "Electronics", "Mitsubishi Q-Series", "iPhone PCB SMT Line PLC Reconfiguration" };
        yield return new object[] { 4401, 401, 1, 4402, 402, "Pharmaceutical", "Schneider M580", "Pfizer Vaccine Filling Station PLC Update" };
        yield return new object[] { 5501, 501, 1, 5502, 502, "Food & Beverage", "ABB AC500", "Coca-Cola Bottling Line PLC Replacement" };
        yield return new object[] { 6601, 601, 1, 6602, 602, "Robotics", "Omron NJ-Series", "Fanuc Robot Welding Cell PLC Integration" };
        yield return new object[] { 7701, 701, 1, 7702, 702, "Aerospace", "Fanuc 31i-Model B", "Boeing 777 CNC Machining Center PLC Upgrade" };
        yield return new object[] { 8801, 801, 0, 8802, 802, "Semiconductor", "Siemens S7-1500", "Intel Wafer Processing PLC Maintenance" };
        yield return new object[] { 9901, 901, 1, 9902, 902, "Chemical", "Schneider Modicon", "DuPont Reactor Control PLC Modernization" };
    }
}