// <copyright file="PlcDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Plcs;

/// <summary>
/// Unit tests for PlcDto - Manufacturing PLC (Programmable Logic Controller) Data Transfer Object
/// Tests PLC configuration properties and behavior for industrial automation systems
/// including automotive, electronics, pharmaceutical, and aerospace manufacturing
/// </summary>
public class PlcDtoTests
{
    private const int SiemensS71200PlcId = 100;
    private const int SiemensS71500PlcId = 600;
    private const int FordF150WeldingMachineId = 10000;
    private const int TeslaModelYBatteryMachineId = 500;
    private const int iPhonePcbAssemblyMachineId = 200;
    private const int PfizerVaccinePackagingMachineId = 300;
    private const int BoeingTurbineMachiningId = 400;

    private const string SiemensS71200IpAddress = "192.168.0.100";
    private const string SiemensS71500IpAddress = "192.168.1.100";
    private const string AllenBradleyIpAddress = "192.168.2.100";
    private const string MitsubishiIpAddress = "192.168.3.100";

    private const string SiemensS7Options = "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]";
    private const string AllenBradleyOptions = "[{\"Slot\": 0, \"Path\": \"1,0\"}]";
    /// <summary>
    /// Executes Should_CreatePlcDto_When_DefaultConstructorCalled operation.
    /// </summary>

    [Fact]
    public void Should_CreatePlcDto_When_DefaultConstructorCalled()
    {
        // Arrange & Act
        var plcDto = new PlcDto();

        // Assert
        plcDto.ShouldNotBeNull();
        plcDto.PlcId.ShouldBe(0);
        plcDto.MachineId.ShouldBe(0);
        plcDto.Name.ShouldBe(string.Empty);
        plcDto.PlcType.ShouldBe(string.Empty);
        plcDto.IpAddress.ShouldBe(string.Empty);
        plcDto.PlcBrand.ShouldBe(string.Empty);
        plcDto.Options.ShouldBe(string.Empty);
        plcDto.BrandOwner.ShouldBe(string.Empty);
        plcDto.CommLibrary.ShouldBe(string.Empty);
        plcDto.EnableSimulation.ShouldBeFalse();
        plcDto.Enabled.ShouldBeFalse();
        plcDto.HasOeeEnabled.ShouldBeFalse();
        plcDto.Machines.ShouldNotBeNull();
        plcDto.Machines.ShouldBeEmpty();
        plcDto.VariablesGroups.ShouldNotBeNull();
        plcDto.VariablesGroups.ShouldBeEmpty();
        plcDto.Variables.ShouldNotBeNull();
        plcDto.Variables.ShouldBeEmpty();
        plcDto.Registers.ShouldNotBeNull();
        plcDto.Registers.ShouldBeEmpty();
        plcDto.Perfomances.ShouldNotBeNull();
        plcDto.Perfomances.ShouldBeEmpty();
        plcDto.References.ShouldNotBeNull();
        plcDto.References.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Should_SetAndGetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var plcDto = new PlcDto();
        var machines = new List<Machine> { new() { MachineId = new MachineId(100), Name = "Test Machine" } };
        var variablesGroups = new Dictionary<string, VariablesGroup> { { "Group1", new VariablesGroup() } };
        var variables = new Dictionary<string, Variable> { { "Var1", new Variable() } };
        var registers = new Dictionary<string, Register> { { "Reg1", Register.CreateFixture() } };
        var performances = new Dictionary<string, Register> { { "Perf1", Register.CreateFixture() } };
        var references = new Dictionary<string, Register> { { "Ref1", Register.CreateFixture() } };

        // Act
        plcDto.PlcId = SiemensS71200PlcId;
        plcDto.MachineId = FordF150WeldingMachineId;
        plcDto.Name = "Siemens S7-1200";
        plcDto.PlcType = "S7-1200";
        plcDto.IpAddress = SiemensS71200IpAddress;
        plcDto.PlcBrand = "Siemens";
        plcDto.Options = SiemensS7Options;
        plcDto.BrandOwner = "Siemens AG";
        plcDto.CommLibrary = "S7-Link";
        plcDto.EnableSimulation = true;
        plcDto.Enabled = true;
        plcDto.HasOeeEnabled = true;
        plcDto.Machines = machines;
        plcDto.VariablesGroups = variablesGroups;
        plcDto.Variables = variables;
        plcDto.Registers = registers;
        plcDto.Perfomances = performances;
        plcDto.References = references;

        // Assert
        plcDto.PlcId.ShouldBe(SiemensS71200PlcId);
        plcDto.MachineId.ShouldBe(FordF150WeldingMachineId);
        plcDto.Name.ShouldBe("Siemens S7-1200");
        plcDto.PlcType.ShouldBe("S7-1200");
        plcDto.IpAddress.ShouldBe(SiemensS71200IpAddress);
        plcDto.PlcBrand.ShouldBe("Siemens");
        plcDto.Options.ShouldBe(SiemensS7Options);
        plcDto.BrandOwner.ShouldBe("Siemens AG");
        plcDto.CommLibrary.ShouldBe("S7-Link");
        plcDto.EnableSimulation.ShouldBeTrue();
        plcDto.Enabled.ShouldBeTrue();
        plcDto.HasOeeEnabled.ShouldBeTrue();
        plcDto.Machines.ShouldBe(machines);
        plcDto.VariablesGroups.ShouldBe(variablesGroups);
        plcDto.Variables.ShouldBe(variables);
        plcDto.Registers.ShouldBe(registers);
        plcDto.Perfomances.ShouldBe(performances);
        plcDto.References.ShouldBe(references);
    }
    /// <summary>
    /// Executes Should_HandleDifferentManufacturingPlcConfigurations_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(100, 100, "S7-1200", "192.168.0.100", "Siemens", "Ford F-150 Welding Cell PLC")]
    [InlineData(600, 500, "S7-1500", "192.168.1.100", "Siemens", "Tesla Model Y Battery Assembly PLC")]
    [InlineData(200, 200, "CompactLogix", "192.168.2.100", "Allen-Bradley", "iPhone PCB Assembly PLC")]
    [InlineData(300, 300, "MELSEC Q", "192.168.3.100", "Mitsubishi", "Pfizer Vaccine Packaging PLC")]
    [InlineData(400, 400, "S7-1500", "192.168.1.101", "Siemens", "Boeing Turbine Machining PLC")]
    public void Should_HandleDifferentManufacturingPlcConfigurations_When_ValidDataProvided(
        int plcId, int machineId, string plcType, string ipAddress, string brand, string name)
    {
        // Arrange & Act
        var plcDto = new PlcDto
        {
            PlcId = plcId,
            MachineId = machineId,
            Name = name,
            PlcType = plcType,
            IpAddress = ipAddress,
            PlcBrand = brand,
            BrandOwner = brand == "Siemens" ? "Siemens AG" : brand == "Allen-Bradley" ? "Rockwell Automation" : "Mitsubishi Electric",
            CommLibrary = brand == "Siemens" ? "S7-Link" : brand == "Allen-Bradley" ? "EtherNet/IP" : "MC Protocol",
            Enabled = true
        };

        // Assert
        plcDto.PlcId.ShouldBe(plcId);
        plcDto.MachineId.ShouldBe(machineId);
        plcDto.Name.ShouldBe(name);
        plcDto.PlcType.ShouldBe(plcType);
        plcDto.IpAddress.ShouldBe(ipAddress);
        plcDto.PlcBrand.ShouldBe(brand);
        plcDto.Enabled.ShouldBeTrue();
    }
    /// <summary>
    /// Executes Should_ConvertPlcToDto_When_ValidPlcEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertPlcToDto_When_ValidPlcEntityProvided()
    {
        // Arrange
        var plcEntity = Plc.CreateFixture(plcId: SiemensS71200PlcId, machineId: FordF150WeldingMachineId, enabled: 1, name: "Ford F-150 Welding PLC", ipAddress: SiemensS71200IpAddress, plcType: "S7-1200", plcBrand: "Siemens", options: SiemensS7Options, commLibrary: "S7-Link", brandOwner: "Siemens AG"); // Entity uses int, DTO uses bool

        // Act
        var plcDtoWrapper = PlcDto.ToDto(plcEntity);

        // Assert
        plcDtoWrapper.IsSuccess.ShouldBeTrue();
        plcDtoWrapper.Value.ShouldNotBeNull();
        var plcDto = plcDtoWrapper.Value;
        plcDto.ShouldNotBeNull();
        plcDto.ShouldNotBeNull();
        plcDto.ShouldNotBeNull();
        plcDto.PlcId.ShouldBe(plcEntity.PlcId);
        plcDto.MachineId.ShouldBe(plcEntity.MachineId);
        plcDto.Name.ShouldBe(plcEntity.Name);
        plcDto.PlcType.ShouldBe(plcEntity.PlcType);
        plcDto.IpAddress.ShouldBe(plcEntity.IpAddress);
        plcDto.PlcBrand.ShouldBe(plcEntity.PlcBrand);
        plcDto.Options.ShouldBe(plcEntity.Options);
        plcDto.BrandOwner.ShouldBe(plcEntity.BrandOwner);
        plcDto.CommLibrary.ShouldBe(plcEntity.CommLibrary);
        plcDto.Enabled.ShouldBeTrue(); // 1 converts to true
    }
}
