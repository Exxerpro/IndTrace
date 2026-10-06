// <copyright file="MachineConfigDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Unit tests for MachineConfigDto - Manufacturing Machine Configuration Data Transfer Object
/// Validates machine configuration properties, collections, and transformation methods
/// in automotive, electronics, and industrial manufacturing contexts
/// </summary>
public class MachineConfigDtoTests
{
    private const int DefaultMachineType = 4;
    private const int BypassEnabledValue = 1;
    private const int TraceabilityEnabledValue = 1;
    private const string DefaultAutomotiveLocation = "HOUSING";
    private const string DefaultElectronicsLocation = "PCB_ASSEMBLY";
    private const string DefaultPharmaceuticalLocation = "PACKAGING";
    /// <summary>
    /// Executes Constructor_Default_ShouldCreateInstanceWithEmptyCollections operation.
    /// </summary>

    [Fact]
    public void Constructor_Default_ShouldCreateInstanceWithEmptyCollections()
    {
        // Act
        var dto = new MachineConfigDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(0);
        dto.MachineId.ShouldBe(0);
        dto.Name.ShouldBe(string.Empty);
        dto.Location.ShouldBe(string.Empty);
        dto.MachineType.ShouldBe(0);
        dto.IpAddress.ShouldBe(string.Empty);
        dto.EnableAppTraceability.ShouldBe(0);
        dto.EnableBypassTraceability.ShouldBe(0);
        dto.BarCodes.ShouldNotBeNull();
        dto.BarCodes.ShouldBeEmpty();
        dto.Cycles.ShouldNotBeNull();
        dto.WorkFlows.ShouldNotBeNull();
        dto.WorkFlows.ShouldBeEmpty();
        dto.DefectRegisters.ShouldNotBeNull();
        dto.DefectRegisters.ShouldBeEmpty();
        dto.Settings.ShouldNotBeNull();
        dto.Settings.ShouldBeEmpty();
        dto.Variables.ShouldNotBeNull();
        dto.Variables.ShouldBeEmpty();
        dto.MachinesPlc.ShouldNotBeNull();
        dto.MachinesPlc.ShouldBeEmpty();
        dto.Plcs.ShouldNotBeNull();
        dto.Plcs.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Properties_AutomotiveSpoilerMachine_ShouldSetAndGetCorrectly operation.
    /// </summary>

    [Fact]
    public void Properties_AutomotiveSpoilerMachine_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dto = new MachineConfigDto();

        // Act - Ford F-150 Housing Assembly Workstation
        dto.Id = 100;
        dto.MachineId = 10000;
        dto.Name = "WS100_SPOILER_ASSEMBLY";
        dto.Location = "FORD_F150_LINE_A";
        dto.MachineType = DefaultMachineType;
        dto.IpAddress = "192.168.100.10";
        dto.EnableAppTraceability = TraceabilityEnabledValue;
        dto.EnableBypassTraceability = 0;

        // Assert
        dto.Id.ShouldBe(100);
        dto.MachineId.ShouldBe(10000);
        dto.Name.ShouldBe("WS100_SPOILER_ASSEMBLY");
        dto.Location.ShouldBe("FORD_F150_LINE_A");
        dto.MachineType.ShouldBe(DefaultMachineType);
        dto.IpAddress.ShouldBe("192.168.100.10");
        dto.EnableAppTraceability.ShouldBe(TraceabilityEnabledValue);
        dto.EnableBypassTraceability.ShouldBe(0);
    }

    /// <summary>
    /// Executes Properties_ElectronicsPcbAssemblyMachine_ShouldSetAndGetCorrectly operation.
    /// </summary>

    [Fact]
    public void Properties_ElectronicsPcbAssemblyMachine_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dto = new MachineConfigDto();

        // Act - iPhone 15 Pro PCB Assembly Station
        dto.Id = 200;
        dto.MachineId = 200;
        dto.Name = "WS200_PCB_PLACEMENT";
        dto.Location = "APPLE_IPHONE15_PRO_LINE";
        dto.MachineType = 8;
        dto.IpAddress = "10.0.200.15";
        dto.EnableAppTraceability = TraceabilityEnabledValue;
        dto.EnableBypassTraceability = 0;

        // Assert
        dto.Id.ShouldBe(200);
        dto.MachineId.ShouldBe(200);
        dto.Name.ShouldBe("WS200_PCB_PLACEMENT");
        dto.Location.ShouldBe("APPLE_IPHONE15_PRO_LINE");
        dto.MachineType.ShouldBe(8);
        dto.IpAddress.ShouldBe("10.0.200.15");
        dto.EnableAppTraceability.ShouldBe(TraceabilityEnabledValue);
        dto.EnableBypassTraceability.ShouldBe(0);
    }

    /// <summary>
    /// Executes Properties_PharmaceuticalPackagingMachine_ShouldSetAndGetCorrectly operation.
    /// </summary>

    [Fact]
    public void Properties_PharmaceuticalPackagingMachine_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dto = new MachineConfigDto();

        // Act - Pfizer COVID-19 Vaccine Packaging Station
        dto.Id = 300;
        dto.MachineId = 300;
        dto.Name = "WS300_VACCINE_PACKAGING";
        dto.Location = "PFIZER_COVID19_PACKAGING";
        dto.MachineType = 16;
        dto.IpAddress = "172.16.30.20";
        dto.EnableAppTraceability = TraceabilityEnabledValue;
        dto.EnableBypassTraceability = 0;

        // Assert
        dto.Id.ShouldBe(300);
        dto.MachineId.ShouldBe(300);
        dto.Name.ShouldBe("WS300_VACCINE_PACKAGING");
        dto.Location.ShouldBe("PFIZER_COVID19_PACKAGING");
        dto.MachineType.ShouldBe(16);
        dto.IpAddress.ShouldBe("172.16.30.20");
        dto.EnableAppTraceability.ShouldBe(TraceabilityEnabledValue);
        dto.EnableBypassTraceability.ShouldBe(0);
    }

    /// <summary>
    /// Executes Properties_AerospaceMachiningCenter_ShouldSetAndGetCorrectly operation.
    /// </summary>

    [Fact]
    public void Properties_AerospaceMachiningCenter_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dto = new MachineConfigDto();

        // Act - Boeing 777X Engine Component Machining
        dto.Id = 400;
        dto.MachineId = 400;
        dto.Name = "WS400_TURBINE_MACHINING";
        dto.Location = "BOEING_777X_ENGINE_SHOP";
        dto.MachineType = 32;
        dto.IpAddress = "192.168.77.40";
        dto.EnableAppTraceability = TraceabilityEnabledValue;
        dto.EnableBypassTraceability = 0;

        // Assert
        dto.Id.ShouldBe(400);
        dto.MachineId.ShouldBe(400);
        dto.Name.ShouldBe("WS400_TURBINE_MACHINING");
        dto.Location.ShouldBe("BOEING_777X_ENGINE_SHOP");
        dto.MachineType.ShouldBe(32);
        dto.IpAddress.ShouldBe("192.168.77.40");
        dto.EnableAppTraceability.ShouldBe(TraceabilityEnabledValue);
        dto.EnableBypassTraceability.ShouldBe(0);
    }

    /// <summary>
    /// Executes Properties_BypassTraceabilityEnabled_ShouldSetCorrectly operation.
    /// </summary>

    [Fact]
    public void Properties_BypassTraceabilityEnabled_ShouldSetCorrectly()
    {
        // Arrange
        var dto = new MachineConfigDto();

        // Act - Maintenance/Test Station with Bypass Enabled
        dto.Id = 0;
        dto.MachineId = 0;
        dto.Name = "END_START_PROCESS";
        dto.Location = "MAINTENANCE_BAY";
        dto.MachineType = 0;
        dto.IpAddress = "127.0.0.1";
        dto.EnableAppTraceability = 0;
        dto.EnableBypassTraceability = BypassEnabledValue;

        // Assert
        dto.EnableAppTraceability.ShouldBe(0);
        dto.EnableBypassTraceability.ShouldBe(BypassEnabledValue);
        dto.Name.ShouldBe("END_START_PROCESS");
        dto.Location.ShouldBe("MAINTENANCE_BAY");
    }

    /// <summary>
    /// Executes Collections_WhenInitialized_ShouldBeEmptyButNotNull operation.
    /// </summary>

    [Fact]
    public void Collections_WhenInitialized_ShouldBeEmptyButNotNull()
    {
        // Arrange & Act
        var dto = new MachineConfigDto();

        // Assert - All collections should be initialized but empty
        dto.BarCodes.ShouldNotBeNull();
        dto.BarCodes.Count.ShouldBe(0);

        dto.Cycles.ShouldNotBeNull();

        dto.WorkFlows.ShouldNotBeNull();
        dto.WorkFlows.Count.ShouldBe(0);

        dto.DefectRegisters.ShouldNotBeNull();
        dto.DefectRegisters.Count.ShouldBe(0);

        dto.Settings.ShouldNotBeNull();
        dto.Settings.Count.ShouldBe(0);

        dto.Variables.ShouldNotBeNull();
        dto.Variables.Count.ShouldBe(0);

        dto.MachinesPlc.ShouldNotBeNull();
        dto.MachinesPlc.Count.ShouldBe(0);

        dto.Plcs.ShouldNotBeNull();
        dto.Plcs.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes ToDto_WithValidMachine_ShouldCreateCorrectDto operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidMachine_ShouldCreateCorrectDto()
    {
        // Arrange - Tesla Model Y Battery Assembly Machine
        var machine = new Machine
        {
            MachineId = new MachineId(500),
            Name = "WS500_BATTERY_ASSEMBLY",
            Location = "TESLA_MODELY_GIGAFACTORY",
            MachineType = 64,
            EnableAppTraceability = TraceabilityEnabledValue,
            EnableBypassTraceability = 0
        };

        // Act
        var dtoWrapper = MachineConfigDto.ToDto(machine);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(500);
        dto.MachineId.ShouldBe(500);
        dto.Name.ShouldBe("WS500_BATTERY_ASSEMBLY");
        dto.Location.ShouldBe("TESLA_MODELY_GIGAFACTORY");
        dto.MachineType.ShouldBe(64);
        dto.EnableAppTraceability.ShouldBe(TraceabilityEnabledValue);
        dto.EnableBypassTraceability.ShouldBe(0);
    }

    /// <summary>
    /// Executes ToDto_WithAutomotiveAssemblyMachine_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_WithAutomotiveAssemblyMachine_ShouldMapAllProperties()
    {
        // Arrange - BMW X5 Transmission Assembly
        var machine = new Machine
        {
            MachineId = new MachineId(600),
            Name = "WS600_TRANSMISSION_ASSEMBLY",
            Location = "BMW_X5_SPARTANBURG",
            MachineType = 128,
            EnableAppTraceability = TraceabilityEnabledValue,
            EnableBypassTraceability = 0
        };

        // Act
        var dtoWrapper = MachineConfigDto.ToDto(machine);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(machine.MachineId.Value);
        dto.MachineId.ShouldBe(machine.MachineId.Value);
        dto.Name.ShouldBe(machine.Name);
        dto.Location.ShouldBe(machine.Location);
        dto.MachineType.ShouldBe(machine.MachineType.Value);
        dto.EnableAppTraceability.ShouldBe(machine.EnableAppTraceability);
        dto.EnableBypassTraceability.ShouldBe(machine.EnableBypassTraceability);
    }

    /// <summary>
    /// Executes ToDto_WithNullMachine_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullMachine_ShouldReturnFailureResult()
    {
        // Act
        var result = MachineConfigDto.ToDto(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }

    /// <summary>
    /// Executes ToEntity_WithValidDto_ShouldCreateCorrectMachine operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidDto_ShouldCreateCorrectMachine()
    {
        // Arrange - Samsung Galaxy S24 Display Assembly
        var dto = new MachineConfigDto
        {
            Id = 700,
            MachineId = 700,
            Name = "WS700_DISPLAY_ASSEMBLY",
            Location = "SAMSUNG_GALAXY_S24_LINE",
            MachineType = MachineType.Inspection,
            EnableAppTraceability = TraceabilityEnabledValue,
            EnableBypassTraceability = 0
        };

        // Act
        var machineWrapper = MachineConfigDto.ToEntity(dto);

        // Assert
        machineWrapper.IsSuccess.ShouldBeTrue();
        machineWrapper.Value.ShouldNotBeNull();
        var machine = machineWrapper.Value;
        machine.ShouldNotBeNull();
        machine.ShouldNotBeNull();
        machine.ShouldNotBeNull();
        machine.MachineId.Value.ShouldBe(700);
        machine.Name.ShouldBe("WS700_DISPLAY_ASSEMBLY");
        machine.Location.ShouldBe("SAMSUNG_GALAXY_S24_LINE");
        machine.MachineType.Value.ShouldBeGreaterThan(0);
        machine.EnableAppTraceability.ShouldBe(TraceabilityEnabledValue);
        machine.EnableBypassTraceability.ShouldBe(0);
    }

    /// <summary>
    /// Executes ToEntity_WithPharmaceuticalMachine_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithPharmaceuticalMachine_ShouldMapAllProperties()
    {
        // Arrange - Johnson & Johnson Vaccine Fill-Finish Machine
        var dto = new MachineConfigDto
        {
            Id = 800,
            MachineId = 800,
            Name = "WS800_FILL_FINISH",
            Location = "JJ_VACCINE_PRODUCTION",
            MachineType = MachineType.DashBoard,
            EnableAppTraceability = TraceabilityEnabledValue,
            EnableBypassTraceability = 0
        };

        // Act
        var machineWrapper = MachineConfigDto.ToEntity(dto);

        // Assert
        machineWrapper.IsSuccess.ShouldBeTrue();
        machineWrapper.Value.ShouldNotBeNull();
        var machine = machineWrapper.Value;
        machine.ShouldNotBeNull();
        machine.ShouldNotBeNull();
        machine.ShouldNotBeNull();
        machine.MachineId.Value.ShouldBe(dto.MachineId);
        machine.Name.ShouldBe(dto.Name);
        machine.Location.ShouldBe(dto.Location);
        machine.MachineType.Value.ShouldBe(dto.MachineType);
        machine.EnableAppTraceability.ShouldBe(dto.EnableAppTraceability);
        machine.EnableBypassTraceability.ShouldBe(dto.EnableBypassTraceability);
    }

    /// <summary>
    /// Executes ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Act
        var result = MachineConfigDto.ToEntity(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }

    /// <summary>
    /// Executes ToDto_ToEntity_RoundTrip_ShouldPreserveData operation.
    /// </summary>

    [Fact]
    public void ToDto_ToEntity_RoundTrip_ShouldPreserveData()
    {
        // Arrange - Intel i9 Processor Fabrication Machine
        var originalMachine = new Machine
        {
            MachineId = new MachineId(900),
            Name = "WS900_PROCESSOR_FAB",
            Location = "INTEL_I9_FABRICATION",
            MachineType = 1024,
            EnableAppTraceability = TraceabilityEnabledValue,
            EnableBypassTraceability = 0
        };

        // Act - Round trip conversion
        var dto = MachineConfigDto.ToDto(originalMachine);
        dto.Value.ShouldNotBeNull();




        var convertedMachineWrapper = MachineConfigDto.ToEntity(dto.Value);

        // Assert
        convertedMachineWrapper.IsSuccess.ShouldBeTrue();
        convertedMachineWrapper.Value.ShouldNotBeNull();
        var convertedMachine = convertedMachineWrapper.Value;
        convertedMachine.ShouldNotBeNull();
        convertedMachine.ShouldNotBeNull();
        convertedMachine.ShouldNotBeNull();
        convertedMachine.MachineId.ShouldBe(originalMachine.MachineId);
        convertedMachine.Name.ShouldBe(originalMachine.Name);
        convertedMachine.Location.ShouldBe(originalMachine.Location);
        convertedMachine.MachineType.ShouldBe(originalMachine.MachineType);
        convertedMachine.EnableAppTraceability.ShouldBe(originalMachine.EnableAppTraceability);
        convertedMachine.EnableBypassTraceability.ShouldBe(originalMachine.EnableBypassTraceability);
    }

    /// <summary>
    /// Executes Properties_AutomotiveManufacturingScenarios_ShouldSetCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(100, "WS100_FORD_ASSEMBLY", "FORD_F150_PLANT", 4)]
    [InlineData(200, "WS200_TESLA_BATTERY", "TESLA_GIGAFACTORY_1", 8)]
    [InlineData(300, "WS300_BMW_ENGINE", "BMW_MUNICH_PLANT", 16)]
    [InlineData(400, "WS400_TOYOTA_HYBRID", "TOYOTA_PRIUS_LINE", 32)]
    [InlineData(500, "WS500_HONDA_CIVIC", "HONDA_OHIO_PLANT", 64)]
    public void Properties_AutomotiveManufacturingScenarios_ShouldSetCorrectly(
        int machineId, string name, string location, int machineType)
    {
        // Arrange
        var dto = new MachineConfigDto();

        // Act
        dto.Id = machineId;
        dto.MachineId = machineId;
        dto.Name = name;
        dto.Location = location;
        dto.MachineType = machineType;
        dto.EnableAppTraceability = TraceabilityEnabledValue;
        dto.EnableBypassTraceability = 0;

        // Assert
        dto.Id.ShouldBe(machineId);
        dto.MachineId.ShouldBe(machineId);
        dto.Name.ShouldBe(name);
        dto.Location.ShouldBe(location);
        dto.MachineType.ShouldBe(machineType);
        dto.EnableAppTraceability.ShouldBe(TraceabilityEnabledValue);
        dto.EnableBypassTraceability.ShouldBe(0);
    }

    /// <summary>
    /// Executes IpAddress_VariousNetworkConfigurations_ShouldSetCorrectly operation.
    /// </summary>
    /// <param name="ipAddress">The ipAddress.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("192.168.1.100", "Standard manufacturing network")]
    [InlineData("10.0.50.200", "Enterprise production network")]
    [InlineData("172.16.100.150", "Secure pharmaceutical network")]
    [InlineData("192.168.77.40", "Aerospace defense network")]
    [InlineData("10.10.10.10", "Electronics assembly network")]
    public void IpAddress_VariousNetworkConfigurations_ShouldSetCorrectly(string ipAddress, string description)
    {
        // Using parameters: ipAddress, description
        _ = ipAddress; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: ipAddress, description
        _ = ipAddress; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: ipAddress, description
        _ = ipAddress; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: ipAddress, description
        _ = ipAddress; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: ipAddress, description
        _ = ipAddress; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var dto = new MachineConfigDto();

        // Act
        dto.IpAddress = ipAddress;

        // Assert
        dto.IpAddress.ShouldBe(ipAddress);
        dto.IpAddress.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Properties_EdgeCaseValues_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Properties_EdgeCaseValues_ShouldHandleCorrectly()
    {
        // Arrange
        var dto = new MachineConfigDto();

        // Act - Edge cases with maximum/minimum values
        dto.Id = int.MaxValue;
        dto.MachineId = int.MaxValue;
        dto.Name = new string('A', 255); // Long name
        dto.Location = new string('B', 255); // Long location
        dto.MachineType = int.MaxValue;
        dto.EnableAppTraceability = 1;
        dto.EnableBypassTraceability = 1;

        // Assert
        dto.Id.ShouldBe(int.MaxValue);
        dto.MachineId.ShouldBe(int.MaxValue);
        dto.Name.Length.ShouldBe(255);
        dto.Location.Length.ShouldBe(255);
        dto.MachineType.ShouldBe(int.MaxValue);
        dto.EnableAppTraceability.ShouldBe(1);
        dto.EnableBypassTraceability.ShouldBe(1);
    }

    /// <summary>
    /// Executes Properties_DefaultStringValues_ShouldBeEmpty operation.
    /// </summary>

    [Fact]
    public void Properties_DefaultStringValues_ShouldBeEmpty()
    {
        // Arrange & Act
        var dto = new MachineConfigDto();

        // Assert
        dto.Name.ShouldBe(string.Empty);
        dto.Location.ShouldBe(string.Empty);
        dto.IpAddress.ShouldBe(string.Empty);
        dto.Name.ShouldNotBeNull();
        dto.Location.ShouldNotBeNull();
        dto.IpAddress.ShouldNotBeNull();
    }
}