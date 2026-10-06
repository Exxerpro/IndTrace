// <copyright file="CreateMachinePlcCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.MachinesPlcs.Commands.Create;
using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Comprehensive unit tests for CreateMachinePlcCommand - Manufacturing machine-PLC relationship command
/// Tests cover automotive, electronics, pharmaceutical, aerospace machine-PLC automation scenarios
/// </summary>
public class CreateMachinePlcCommandTests
{
    /// <summary>
    /// Executes Should_CreateInstance_When_Instantiated operation.
    /// </summary>
    [Fact]
    public void Should_CreateInstance_When_Instantiated()
    {
        // Arrange & Act
        var command = new CreateMachinePlcCommand();

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeAssignableTo<IMonitorRequest<MachinePlcCreated>>();
    }

    /// <summary>
    /// Executes Should_ImplementIMonitorRequestInterface_When_Instantiated operation.
    /// </summary>

    [Fact]
    public void Should_ImplementIMonitorRequestInterface_When_Instantiated()
    {
        // Arrange & Act
        var command = new CreateMachinePlcCommand();

        // Assert
        command.ShouldBeAssignableTo<IMonitorRequest<MachinePlcCreated>>();
        typeof(IMonitorRequest<MachinePlcCreated>).IsAssignableFrom(typeof(CreateMachinePlcCommand)).ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_SetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand();
        var machineId = 1001;
        var machine = new Machine
        {
            MachineId = new MachineId(100001),
            Name = "Ford F-150 Robotic Welding Cell",
            Location = "Dearborn Assembly Plant"
        };
        var plcId = 2001;
        var plc = Plc.CreateFixture(plcId: 2001, machineId: 0, enabled: ActiveStatus.None, name: "Siemens S7-1500 PLC", ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Act
        command.MachineId = machineId;
        command.Machine = machine;
        command.PlCsId = plcId;
        command.Plc = plc;

        // Assert
        command.MachineId.ShouldBe(machineId);
        command.Machine.ShouldBe(machine);
        command.Machine.MachineId.Value.ShouldBe(100001);
        command.Machine.Name.ShouldBe("Ford F-150 Robotic Welding Cell");
        command.PlCsId.ShouldBe(plcId);
        command.Plc.ShouldBe(plc);
        command.Plc.PlcId.ShouldBe(2001);
        command.Plc.Name.ShouldBe("Siemens S7-1500 PLC");
    }

    /// <summary>
    /// Executes Should_HandleFordF150AutomotiveManufacturingMachinePlcRelationship_When_FordRoboticWeldingSetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleFordF150AutomotiveManufacturingMachinePlcRelationship_When_FordRoboticWeldingSetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = 1000001;
        command.Machine = new Machine
        {
            MachineId = new MachineId(1000001),
            Name = "Ford F-150 SuperCrew 4x4 Robotic Welding Cell #1",
            Location = "Ford Dearborn Assembly Plant - Body-in-White Line A - Station 15",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        command.PlCsId = 20001;
        command.Plc = Plc.CreateFixture(plcId: 20001, machineId: 0, enabled: ActiveStatus.None, name: "Siemens S7-1500 Advanced Controller", ipAddress: "192.168.10.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.MachineId.ShouldBe(1000001);
        command.Machine.Name.ShouldBe("Ford F-150 SuperCrew 4x4 Robotic Welding Cell #1");
        command.Machine.Location.ShouldBe("Ford Dearborn Assembly Plant - Body-in-White Line A - Station 15");
        command.PlCsId.ShouldBe(20001);
        command.Plc.Name.ShouldBe("Siemens S7-1500 Advanced Controller");
        command.Plc.IpAddress.ShouldBe("192.168.10.100");
    }

    /// <summary>
    /// Executes Should_HandleTeslaModelYElectricVehicleManufacturingMachinePlcRelationship_When_TeslaBatteryAssemblySetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleTeslaModelYElectricVehicleManufacturingMachinePlcRelationship_When_TeslaBatteryAssemblySetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = 20002;
        command.Machine = new Machine
        {
            MachineId = new MachineId(20002),
            Name = "Tesla Model Y 4680 Battery Pack Assembly Robot",
            Location = "Tesla Gigafactory Berlin-Brandenburg - Battery Line B - Station 8",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        command.PlCsId = 30002;
        command.Plc = Plc.CreateFixture(plcId: 30002, machineId: 0, enabled: ActiveStatus.None, name: "Allen-Bradley ControlLogix 5580", ipAddress: "192.168.20.200", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.MachineId.ShouldBe(20002);
        command.Machine.Name.ShouldBe("Tesla Model Y 4680 Battery Pack Assembly Robot");
        command.Machine.Location.ShouldBe("Tesla Gigafactory Berlin-Brandenburg - Battery Line B - Station 8");
        command.PlCsId.ShouldBe(30002);
        command.Plc.Name.ShouldBe("Allen-Bradley ControlLogix 5580");
        command.Plc.IpAddress.ShouldBe("192.168.20.200");
    }

    /// <summary>
    /// Executes Should_HandleAppleIPhoneElectronicsManufacturingMachinePlcRelationship_When_ApplePcbAssemblySetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleAppleIPhoneElectronicsManufacturingMachinePlcRelationship_When_ApplePcbAssemblySetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = 30003;
        command.Machine = new Machine
        {
            MachineId = new MachineId(30003),
            Name = "Apple iPhone 15 Pro Max A17 Pro PCB SMT Line",
            Location = "Apple Park Manufacturing Facility - Cupertino - PCB Assembly Line C",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        command.PlCsId = 40003;
        command.Plc = Plc.CreateFixture(plcId: 40003, machineId: 0, enabled: ActiveStatus.None, name: "Beckhoff TwinCAT CX2020", ipAddress: "192.168.30.300", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.MachineId.ShouldBe(30003);
        command.Machine.Name.ShouldBe("Apple iPhone 15 Pro Max A17 Pro PCB SMT Line");
        command.Machine.Location.ShouldBe("Apple Park Manufacturing Facility - Cupertino - PCB Assembly Line C");
        command.PlCsId.ShouldBe(40003);
        command.Plc.Name.ShouldBe("Beckhoff TwinCAT CX2020");
        command.Plc.IpAddress.ShouldBe("192.168.30.300");
    }

    /// <summary>
    /// Executes Should_HandlePfizerVaccinePharmaceuticalManufacturingMachinePlcRelationship_When_PfizerFillFinishSetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandlePfizerVaccinePharmaceuticalManufacturingMachinePlcRelationship_When_PfizerFillFinishSetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = 40004;
        command.Machine = new Machine
        {
            MachineId = new MachineId(40004),
            Name = "Pfizer COVID-19 mRNA Vaccine Fill-Finish Station",
            Location = "Pfizer Kalamazoo Manufacturing Site - GMP Cleanroom Line 1",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        command.PlCsId = 50004;
        command.Plc = Plc.CreateFixture(plcId: 50004, machineId: 0, enabled: ActiveStatus.None, name: "Schneider Electric Modicon M580", ipAddress: "192.168.40.400", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.MachineId.ShouldBe(40004);
        command.Machine.Name.ShouldBe("Pfizer COVID-19 mRNA Vaccine Fill-Finish Station");
        command.Machine.Location.ShouldBe("Pfizer Kalamazoo Manufacturing Site - GMP Cleanroom Line 1");
        command.PlCsId.ShouldBe(50004);
        command.Plc.Name.ShouldBe("Schneider Electric Modicon M580");
        command.Plc.IpAddress.ShouldBe("192.168.40.400");
    }

    /// <summary>
    /// Executes Should_HandleBoeingAerospaceManufacturingMachinePlcRelationship_When_BoeingWingDrillingSetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleBoeingAerospaceManufacturingMachinePlcRelationship_When_BoeingWingDrillingSetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = 50005;
        command.Machine = new Machine
        {
            MachineId = new MachineId(50005),
            Name = "Boeing 777X Composite Wing Automated Drilling Station",
            Location = "Boeing Everett Factory - Wing Assembly Building - Line B - Station 12",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        command.PlCsId = 60005;
        command.Plc = Plc.CreateFixture(plcId: 60005, machineId: 0, enabled: ActiveStatus.None, name: "GE Fanuc RX3i PACSystems", ipAddress: "192.168.50.500", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.MachineId.ShouldBe(50005);
        command.Machine.Name.ShouldBe("Boeing 777X Composite Wing Automated Drilling Station");
        command.Machine.Location.ShouldBe("Boeing Everett Factory - Wing Assembly Building - Line B - Station 12");
        command.PlCsId.ShouldBe(60005);
        command.Plc.Name.ShouldBe("GE Fanuc RX3i PACSystems");
        command.Plc.IpAddress.ShouldBe("192.168.50.500");
    }

    /// <summary>
    /// Executes Should_HandleSpecializedIndustryMachinePlcRelationships_When_NicheMachineControllerPairsProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <param name="machineName">The machineName.</param>
    /// <param name="machineLocation">The machineLocation.</param>
    /// <param name="plcName">The plcName.</param>
    /// <param name="plcIpAddress">The plcIpAddress.</param>
    /// <param name="industryDescription">The industryDescription.</param>

    [Theory]
    [InlineData(60001, 70001, "Caterpillar 797F Mining Truck Engine Assembly", "Caterpillar Peoria Manufacturing", "Mitsubishi MELSEC iQ-R Series", "192.168.60.600", "Heavy Equipment Manufacturing")]
    [InlineData(70002, 80002, "John Deere S790 Combine Harvester Assembly", "John Deere Waterloo Operations", "Omron Sysmac NJ Series", "192.168.70.700", "Agricultural Equipment Manufacturing")]
    [InlineData(80003, 90003, "Coca-Cola Classic Bottling Machine", "Coca-Cola Atlanta Bottling Plant", "Siemens S7-1200 Compact", "192.168.80.800", "Food & Beverage Manufacturing")]
    [InlineData(90004, 100004, "Medtronic Pacemaker Assembly Station", "Medtronic Minneapolis Cardiac", "Allen-Bradley CompactLogix 5370", "192.168.90.900", "Medical Device Manufacturing")]
    [InlineData(100005, 110005, "Lockheed F-35 Engine Bay Assembly", "Lockheed Martin Fort Worth", "Bosch Rexroth IndraControl XM22", "192.168.100.1000", "Defense Manufacturing")]
    public void Should_HandleSpecializedIndustryMachinePlcRelationships_When_NicheMachineControllerPairsProvided(int machineId, int plcId, string machineName, string machineLocation, string plcName, string plcIpAddress, string industryDescription)
    {
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = machineId;
        command.Machine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = machineName,
            Location = machineLocation,
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        command.PlCsId = plcId;
        command.Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: plcName, ipAddress: plcIpAddress, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.MachineId.ShouldBe(machineId);
        command.Machine.Name.ShouldBe(machineName);
        command.Machine.Location.ShouldBe(machineLocation);
        command.PlCsId.ShouldBe(plcId);
        command.Plc.Name.ShouldBe(plcName);
        command.Plc.IpAddress.ShouldBe(plcIpAddress);
    }

    /// <summary>
    /// Executes Should_HandleInternationalMachinePlcRelationships_When_GlobalAutomationSystemsProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <param name="machineName">The machineName.</param>
    /// <param name="machineLocation">The machineLocation.</param>
    /// <param name="plcName">The plcName.</param>
    /// <param name="plcIpAddress">The plcIpAddress.</param>
    /// <param name="regionDescription">The regionDescription.</param>

    [Theory]
    [InlineData(110001, 120001, "BMW X5 Body Welding Station", "BMW Spartanburg Manufacturing", "Siemens S7-1500F Safety", "192.168.110.1100", "German Automotive Manufacturing")]
    [InlineData(120002, 130002, "Samsung Galaxy S24 Display Assembly", "Samsung Giheung Semiconductor Fab", "LS Electric XGK-CPUA", "192.168.120.1200", "South Korean Electronics Manufacturing")]
    [InlineData(130003, 140003, "Novo Nordisk Insulin Pen Assembly", "Novo Nordisk Kalundborg Production", "Danfoss FC-051P VLT Drive", "192.168.130.1300", "Danish Pharmaceutical Manufacturing")]
    [InlineData(140004, 150004, "Airbus A350 Fuselage Assembly", "Airbus Toulouse Final Assembly", "Schneider Electric M241 Logic Controller", "192.168.140.1400", "European Aerospace Manufacturing")]
    [InlineData(150005, 160005, "Rolls-Royce Trent Engine Manufacturing", "Rolls-Royce Derby Engine Manufacturing", "ABB AC500-eCo PLC", "192.168.150.1500", "UK Aerospace Manufacturing")]
    public void Should_HandleInternationalMachinePlcRelationships_When_GlobalAutomationSystemsProvided(int machineId, int plcId, string machineName, string machineLocation, string plcName, string plcIpAddress, string regionDescription)
    {
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName, plcIpAddress, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = plcIpAddress; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = machineId;
        command.Machine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = machineName,
            Location = machineLocation,
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        command.PlCsId = plcId;
        command.Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: plcName, ipAddress: plcIpAddress, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.MachineId.ShouldBe(machineId);
        command.Machine.Name.ShouldBe(machineName);
        command.PlCsId.ShouldBe(plcId);
        command.Plc.Name.ShouldBe(plcName);
        command.Plc.IpAddress.ShouldBe(plcIpAddress);
    }

    /// <summary>
    /// Executes Should_HandleDifferentPlcTypes_When_VariousIndustrialControllersProvided operation.
    /// </summary>
    /// <param name="plcName">The plcName.</param>
    /// <param name="applicationDescription">The applicationDescription.</param>

    [Theory]
    [InlineData("Siemens S7-1500", "Automotive welding and assembly")]
    [InlineData("Allen-Bradley ControlLogix", "High-speed electronics assembly")]
    [InlineData("Schneider Electric Modicon", "Pharmaceutical and medical device manufacturing")]
    [InlineData("Mitsubishi MELSEC iQ-R", "Heavy machinery and mining equipment")]
    [InlineData("Omron Sysmac NJ", "Food processing and packaging")]
    [InlineData("Beckhoff TwinCAT", "Precision motion control and robotics")]
    public void Should_HandleDifferentPlcTypes_When_VariousIndustrialControllersProvided(string plcName, string applicationDescription)
    {
        // Using parameters: plcName, applicationDescription
        _ = plcName; // xUnit1026 fix
        _ = applicationDescription; // xUnit1026 fix
        // Using parameters: plcName, applicationDescription
        _ = plcName; // xUnit1026 fix
        _ = applicationDescription; // xUnit1026 fix
        // Using parameters: plcName, applicationDescription
        _ = plcName; // xUnit1026 fix
        _ = applicationDescription; // xUnit1026 fix
        // Using parameters: plcName, applicationDescription
        _ = plcName; // xUnit1026 fix
        _ = applicationDescription; // xUnit1026 fix
        // Using parameters: plcName, applicationDescription
        _ = plcName; // xUnit1026 fix
        _ = applicationDescription; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = 100001;
        command.Machine = new Machine
        {
            MachineId = new MachineId(100001),
            Name = "Industrial Manufacturing Station",
            Location = "Test Manufacturing Facility"
        };
        command.PlCsId = 2001;
        command.Plc = Plc.CreateFixture(plcId: 2001, machineId: 0, enabled: ActiveStatus.None, name: plcName, ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.Plc.Name.ShouldBe(plcName);
    }

    /// <summary>
    /// Executes Should_HandleDifferentNetworkConfigurations_When_VariousIpAddressesProvided operation.
    /// </summary>
    /// <param name="ipAddress">The ipAddress.</param>
    /// <param name="networkDescription">The networkDescription.</param>

    [Theory]
    [InlineData("192.168.1.100", "Standard factory network")]
    [InlineData("10.0.1.50", "Corporate network segment")]
    [InlineData("172.16.10.200", "Private manufacturing network")]
    [InlineData("192.168.100.250", "High-security cleanroom network")]
    [InlineData("10.10.10.10", "Dedicated automation network")]
    public void Should_HandleDifferentNetworkConfigurations_When_VariousIpAddressesProvided(string ipAddress, string networkDescription)
    {
        // Using parameters: ipAddress, networkDescription
        _ = ipAddress; // xUnit1026 fix
        _ = networkDescription; // xUnit1026 fix
        // Using parameters: ipAddress, networkDescription
        _ = ipAddress; // xUnit1026 fix
        _ = networkDescription; // xUnit1026 fix
        // Using parameters: ipAddress, networkDescription
        _ = ipAddress; // xUnit1026 fix
        _ = networkDescription; // xUnit1026 fix
        // Using parameters: ipAddress, networkDescription
        _ = ipAddress; // xUnit1026 fix
        _ = networkDescription; // xUnit1026 fix
        // Using parameters: ipAddress, networkDescription
        _ = ipAddress; // xUnit1026 fix
        _ = networkDescription; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = 100001;
        command.Machine = new Machine { MachineId = new MachineId(100001), Name = "Test Machine" };
        command.PlCsId = 2001;
        command.Plc = Plc.CreateFixture(plcId: 2001, machineId: 0, enabled: ActiveStatus.None, name: "Network Test PLC", ipAddress: ipAddress, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.Plc.IpAddress.ShouldBe(ipAddress);
    }

    /// <summary>
    /// Executes Should_HandleEdgeCaseMachineIds_When_SpecialValuesProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(0, "Zero machine ID")]
    [InlineData(-1, "Negative machine ID")]
    [InlineData(999999, "Large machine ID")]
    [InlineData(int.MaxValue, "Maximum integer machine ID")]
    public void Should_HandleEdgeCaseMachineIds_When_SpecialValuesProvided(int machineId, string scenario)
    {
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = machineId;
        command.Machine = new Machine { MachineId = new MachineId(machineId), Name = "Edge Case Machine" };

        // Assert
        command.MachineId.ShouldBe(machineId);
        command.Machine.MachineId.Value.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes Should_HandleEdgeCasePlcIds_When_SpecialValuesProvided operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(0, "Zero PLC ID")]
    [InlineData(-1, "Negative PLC ID")]
    [InlineData(999999, "Large PLC ID")]
    [InlineData(int.MaxValue, "Maximum integer PLC ID")]
    public void Should_HandleEdgeCasePlcIds_When_SpecialValuesProvided(int plcId, string scenario)
    {
        // Using parameters: plcId, scenario
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, scenario
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, scenario
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, scenario
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, scenario
        _ = plcId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.PlCsId = plcId;
        command.Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: "Edge Case PLC", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.PlCsId.ShouldBe(plcId);
        command.Plc.PlcId.ShouldBe(plcId);
    }

    /// <summary>
    /// Executes Should_HandleNullMachineAndPlcObjects_When_DefaultValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleNullMachineAndPlcObjects_When_DefaultValuesProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Assert - Check default object values
        command.Machine.ShouldNotBeNull();
        command.Plc.ShouldNotBeNull();

        // Act - Set to null
        command.Machine = null!;
        command.Plc = null!;

        // Assert - Check null values
        command.Machine.ShouldBeNull();
        command.Plc.ShouldBeNull();
    }

    /// <summary>
    /// Executes Should_HandleConcurrentPropertyUpdates_When_MultipleThreadsUpdateMachinePlcProperties operation.
    /// </summary>

    [Fact]
    public async Task Should_HandleConcurrentPropertyUpdates_When_MultipleThreadsUpdateMachinePlcProperties()
    {
        // Arrange
        var command = new CreateMachinePlcCommand();
        var tasks = new List<Task>();

        // Act
        for (int i = 1; i <= 10; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                command.MachineId = threadId * 1000;
                command.Machine = new Machine
                {
                    MachineId = new MachineId(threadId * 1000),
                    Name = $"Concurrent Machine {threadId}"
                };
                command.PlCsId = threadId * 2000;
                command.Plc = Plc.CreateFixture(plcId: threadId * 2000, machineId: 0, enabled: ActiveStatus.None, name: $"Concurrent PLC {threadId}", ipAddress: $"192.168.{threadId}.{threadId}", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);
                return Task.FromResult(command);
            }
                ));
        }

        await Task.WhenAll(tasks.ToArray());

        // Assert
        command.MachineId.ShouldBeGreaterThan(0);
        command.Machine.ShouldNotBeNull();
        command.Machine.Name.ShouldNotBeNull();
        command.Machine.Name.ShouldStartWith("Concurrent Machine");
        command.PlCsId.ShouldBeGreaterThan(0);
        command.Plc.ShouldNotBeNull();
        command.Plc.Name.ShouldNotBeNull();
        command.Plc.Name.ShouldStartWith("Concurrent PLC");
    }

    /// <summary>
    /// Executes Should_MaintainPropertyIndependence_When_MultipleMachinePlcCommandInstancesCreated operation.
    /// </summary>

    [Fact]
    public void Should_MaintainPropertyIndependence_When_MultipleMachinePlcCommandInstancesCreated()
    {
        // Arrange & Act
        var command1 = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Ford Welding Robot" },
            PlCsId = 2001,
            Plc = Plc.CreateFixture(plcId: 2001, machineId: 0, enabled: ActiveStatus.None, name: "Siemens S7-1500", ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        var command2 = new CreateMachinePlcCommand
        {
            MachineId = 2002,
            Machine = new Machine { MachineId = new MachineId(2002), Name = "Tesla Battery Robot" },
            PlCsId = 3002,
            Plc = Plc.CreateFixture(plcId: 3002, machineId: 0, enabled: ActiveStatus.None, name: "Allen-Bradley ControlLogix", ipAddress: "192.168.2.200", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        var command3 = new CreateMachinePlcCommand
        {
            MachineId = 3003,
            Machine = new Machine { MachineId = new MachineId(3003), Name = "Apple PCB SMT Line" },
            PlCsId = 4003,
            Plc = Plc.CreateFixture(plcId: 4003, machineId: 0, enabled: ActiveStatus.None, name: "Beckhoff TwinCAT", ipAddress: "192.168.3.300", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Assert
        command1.MachineId.ShouldBe(100001);
        command1.Machine.Name.ShouldBe("Ford Welding Robot");
        command1.PlCsId.ShouldBe(2001);
        command1.Plc.Name.ShouldBe("Siemens S7-1500");

        command2.MachineId.ShouldBe(2002);
        command2.Machine.Name.ShouldBe("Tesla Battery Robot");
        command2.PlCsId.ShouldBe(3002);
        command2.Plc.Name.ShouldBe("Allen-Bradley ControlLogix");

        command3.MachineId.ShouldBe(3003);
        command3.Machine.Name.ShouldBe("Apple PCB SMT Line");
        command3.PlCsId.ShouldBe(4003);
        command3.Plc.Name.ShouldBe("Beckhoff TwinCAT");
    }

    /// <summary>
    /// Executes Should_HandleAdditionalGlobalMachinePlcRelationships_When_WorldwideAutomationSystemsProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plcId">The plcId.</param>
    /// <param name="machineName">The machineName.</param>
    /// <param name="machineLocation">The machineLocation.</param>
    /// <param name="plcName">The plcName.</param>

    [Theory]
    [InlineData(160001, 170001, "Honda Civic Engine Assembly Station", "Honda Marysville Auto Plant", "Keyence KV-8000 Series")]
    [InlineData(170002, 180002, "Volkswagen ID.4 Battery Assembly", "Volkswagen Wolfsburg Main Plant", "Wago PFC200 Controller")]
    [InlineData(180003, 190003, "Sony PlayStation 5 SoC Fabrication", "Sony Kumamoto Semiconductor Fab", "Yokogawa STARDOM FCJ Controller")]
    [InlineData(190004, 200004, "Roche Oncology Drug Production", "Roche Basel Pharmaceutical", "B&R X20 System")]
    [InlineData(200005, 210005, "GE9X Turbofan Engine Assembly", "General Electric Lynn Manufacturing", "Honeywell Experion PKS")]
    public void Should_HandleAdditionalGlobalMachinePlcRelationships_When_WorldwideAutomationSystemsProvided(int machineId, int plcId, string machineName, string machineLocation, string plcName)
    {
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: machineId, plcId, machineName, machineLocation, plcName
        _ = machineId; // xUnit1026 fix
        _ = plcId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = machineLocation; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = machineId;
        command.Machine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = machineName,
            Location = machineLocation,
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        command.PlCsId = plcId;
        command.Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: plcName, ipAddress: $"192.168.{machineId % 255}.{plcId % 255}", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.MachineId.ShouldBe(machineId);
        command.Machine.Name.ShouldBe(machineName);
        command.Machine.Location.ShouldBe(machineLocation);
        command.PlCsId.ShouldBe(plcId);
        command.Plc.Name.ShouldBe(plcName);
        command.Plc.IpAddress.ShouldBe($"192.168.{machineId % 255}.{plcId % 255}");
    }

    /// <summary>
    /// Executes Should_HandleComplexManufacturingAutomationScenario_When_FullMachinePlcRelationshipProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleComplexManufacturingAutomationScenario_When_FullMachinePlcRelationshipProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand();

        // Act
        command.MachineId = 999999;
        command.Machine = new Machine
        {
            MachineId = new MachineId(999999),
            Name = "Advanced Multi-Stage Manufacturing Cell with AI-Driven Quality Control and Predictive Maintenance",
            Location = "Industry 4.0 Smart Factory - Building Alpha - Level 5 - Zone Omega - Line Delta - Station Ultimate",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 1
        };
        command.PlCsId = 888888;
        command.Plc = Plc.CreateFixture(plcId: 888888, machineId: 0, enabled: ActiveStatus.None, name: "Next-Gen Industrial IoT Edge Computing Controller with 5G Connectivity", ipAddress: "192.168.255.254", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        // Assert
        command.MachineId.ShouldBe(999999);
        command.Machine.Name.ShouldBe("Advanced Multi-Stage Manufacturing Cell with AI-Driven Quality Control and Predictive Maintenance");
        command.Machine.Location.ShouldBe("Industry 4.0 Smart Factory - Building Alpha - Level 5 - Zone Omega - Line Delta - Station Ultimate");
        command.PlCsId.ShouldBe(888888);
        command.Plc.Name.ShouldBe("Next-Gen Industrial IoT Edge Computing Controller with 5G Connectivity");
        command.Plc.IpAddress.ShouldBe("192.168.255.254");
    }
}