// <copyright file="MachineDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Unit tests for MachineDto data transfer object.
/// Tests the industrial machine management system for production equipment.
/// </summary>
public class MachineDtoTests
{
    /// <summary>
    /// Executes MachineDto_Constructor_ShouldInitializeWithDefaults operation.
    /// </summary>
    [Fact]
    public void MachineDto_Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var machineDto = new MachineDto();

        // Assert
        machineDto.MachineId.ShouldBe(0);
        machineDto.Name.ShouldBe(string.Empty);
        machineDto.Location.ShouldBe(string.Empty);
        machineDto.Description.ShouldBe(string.Empty);
        machineDto.MachineType.ShouldNotBeNull();
        machineDto.WorkFlowType.ShouldNotBeNull();
        machineDto.IpAddress.ShouldBe(string.Empty);
        machineDto.EnableAppTraceability.ShouldBe(0);
        machineDto.EnableBypassTraceability.ShouldBe(0);
        machineDto.Retry.ShouldBe(1);
        machineDto.ImageName.ShouldBe(string.Empty);
        machineDto.RuleId.ShouldBe(0);
    }
    /// <summary>
    /// Executes MachineDto_Properties_ShouldAllowGetAndSet operation.
    /// </summary>

    [Fact]
    public void MachineDto_Properties_ShouldAllowGetAndSet()
    {
        // Arrange
        const int machineId = 12345;
        const string name = "CNC_Machine_001";
        const string location = "Production_Line_A";
        const string description = "High-precision CNC machining center";
        const string ipAddress = "192.168.1.100";
        const int enableAppTraceability = 1;
        const int enableBypassTraceability = 0;
        const int retry = 3;
        const string imageName = "cnc_machine.png";
        const int ruleId = 9001;

        // Act (#225: init-only properties - construction via object initializer)
        var machineDto = new MachineDto
        {
            MachineId = machineId,
            Name = name,
            Location = location,
            Description = description,
            IpAddress = ipAddress,
            EnableAppTraceability = enableAppTraceability,
            EnableBypassTraceability = enableBypassTraceability,
            Retry = retry,
            ImageName = imageName,
            RuleId = ruleId,
        };

        // Assert
        machineDto.MachineId.ShouldBe(machineId);
        machineDto.Name.ShouldBe(name);
        machineDto.Location.ShouldBe(location);
        machineDto.Description.ShouldBe(description);
        machineDto.IpAddress.ShouldBe(ipAddress);
        machineDto.EnableAppTraceability.ShouldBe(enableAppTraceability);
        machineDto.EnableBypassTraceability.ShouldBe(enableBypassTraceability);
        machineDto.Retry.ShouldBe(retry);
        machineDto.ImageName.ShouldBe(imageName);
        machineDto.RuleId.ShouldBe(ruleId);
        machineDto.IsEnabled.ShouldBeTrue(); // EnableAppTraceability == 1
    }
    /// <summary>
    /// Executes MachineDto_WithIndustrialEquipment_ShouldHandleManufacturingMachines operation.
    /// </summary>

    [Theory]
    [InlineData(1001, "CNC_001", "Line_A", "192.168.1.10", "CNC machining center")]
    [InlineData(2002, "ROBOT_001", "Line_B", "192.168.1.20", "Industrial robot arm")]
    [InlineData(3003, "PRESS_001", "Line_C", "192.168.1.30", "Hydraulic press machine")]
    [InlineData(4004, "CONV_001", "Line_D", "192.168.1.40", "Conveyor belt system")]
    public void MachineDto_WithIndustrialEquipment_ShouldHandleManufacturingMachines(
        int machineId, string name, string location, string ipAddress, string description)
    {
        // Arrange & Act
        var machineDto = new MachineDto
        {
            MachineId = machineId,
            Name = name,
            Location = location,
            IpAddress = ipAddress,
            Description = description,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            Retry = 2
        };

        // Assert
        machineDto.MachineId.ShouldBe(machineId);
        machineDto.Name.ShouldBe(name);
        machineDto.Location.ShouldBe(location);
        machineDto.IpAddress.ShouldBe(ipAddress);
        machineDto.Description.ShouldBe(description);
        machineDto.IsEnabled.ShouldBeTrue();
    }
    /// <summary>
    /// Executes MachineDto_WithDifferentTraceabilitySettings_ShouldCalculateIsEnabled operation.
    /// </summary>

    [Theory]
    [InlineData(1, 0, true, "App traceability enabled")]
    [InlineData(0, 0, true, "Both disabled but Enabled logic")]
    [InlineData(0, 1, false, "Only bypass enabled")]
    [InlineData(1, 1, true, "Both enabled, app takes precedence")]
    public void MachineDto_WithDifferentTraceabilitySettings_ShouldCalculateIsEnabled(
        int enableApp, int enableBypass, bool expectedEnabled, string scenario)
    {

        var logger = XUnitLogger.CreateLogger<MachineDtoTests>();
        logger.LogInformation("Testing scenario: {scenario} with enableApp={enableApp}, enableBypass={enableBypass}, expectedEnabled={expectedEnabled}",
            scenario, enableApp, enableBypass, expectedEnabled);

        // Arrange & Act
        var machineDto = new MachineDto
        {
            EnableAppTraceability = enableApp,
            EnableBypassTraceability = enableBypass
        };

        // Assert
        machineDto.IsEnabled.ShouldBe(expectedEnabled);
        // Each scenario tests different traceability configurations
    }
    /// <summary>
    /// Executes ToDto_WithValidMachine_ShouldCreateCorrectDto operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidMachine_ShouldCreateCorrectDto()
    {
        // Arrange
        var machine = new Machine
        {
            MachineId = new MachineId(99999),
            Name = "Test_Machine",
            Location = "Test_Location",
            Description = "Test machine for validation",
            MachineType = 1, // Assuming enum value
            WorkFlowType = 2, // Assuming enum value
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            Retry = 3,
            RuleId = 5001
        };

        // Act
        var resultWrapper = MachineDto.ToDto(machine);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.MachineId.ShouldBe(99999);
        result.Name.ShouldBe("Test_Machine");
        result.Location.ShouldBe("Test_Location");
        result.Description.ShouldBe("Test machine for validation");
        result.EnableAppTraceability.ShouldBe(1);
        result.EnableBypassTraceability.ShouldBe(0);
        result.Retry.ShouldBe(3);
        result.RuleId.ShouldBe(5001);
        result.IsEnabled.ShouldBeTrue();
    }
    /// <summary>
    /// Executes ToDto_WithNullMachine_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullMachine_ShouldReturnFailureResult()
    {
        // Act
        var result = MachineDto.ToDto(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }
    /// <summary>
    /// Executes ToEntity_WithValidDto_ShouldCreateCorrectEntity operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidDto_ShouldCreateCorrectEntity()
    {
        // Arrange
        var machineDto = new MachineDto
        {
            MachineId = 77777,
            Name = "Converted_Machine",
            Location = "Production_Floor",
            Description = "Machine converted from DTO",
            MachineType = 1, // Use implicit conversion from int
            WorkFlowType = 2, // Use implicit conversion from int
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            Retry = 2,
            RuleId = 6001
        };

        // Act
        var resultWrapper = MachineDto.ToEntity(machineDto);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.MachineId.Value.ShouldBe(77777);
        result.Name.ShouldBe("Converted_Machine");
        result.Location.ShouldBe("Production_Floor");
        result.Description.ShouldBe("Machine converted from DTO");
        result.MachineType.Value.ShouldBe(1);
        result.WorkFlowType.Value.ShouldBe(2);
        result.EnableAppTraceability.ShouldBe(1);
        result.EnableBypassTraceability.ShouldBe(0);
        result.Retry.ShouldBe(2);
        result.RuleId.ShouldBe(6001);
    }
    /// <summary>
    /// Executes ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Act
        var result = MachineDto.ToEntity(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }
    /// <summary>
    /// Executes RoundTrip_MachineToDto_ShouldPreserveData operation.
    /// </summary>

    [Fact]
    public void RoundTrip_MachineToDto_ShouldPreserveData()
    {
        // Arrange
        var originalMachine = new Machine
        {
            MachineId = new MachineId(88888),
            Name = "RoundTrip_Machine",
            Location = "Assembly_Line_C",
            Description = "Round-trip conversion test machine",
            MachineType = 3,
            WorkFlowType = 4,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            Retry = 5,
            RuleId = 7001
        };

        // Act
        var dtoWrapper = MachineDto.ToDto(originalMachine);
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;

        var backToEntityWrapper = MachineDto.ToEntity(dto);

        // Assert
        backToEntityWrapper.IsSuccess.ShouldBeTrue();
        backToEntityWrapper.Value.ShouldNotBeNull();
        var backToEntity = backToEntityWrapper.Value;
        backToEntity.ShouldNotBeNull();
        backToEntity.ShouldNotBeNull();
        backToEntity.ShouldNotBeNull();
        backToEntity.MachineId.ShouldBe(originalMachine.MachineId);
        backToEntity.Name.ShouldBe(originalMachine.Name);
        backToEntity.Location.ShouldBe(originalMachine.Location);
        backToEntity.Description.ShouldBe(originalMachine.Description);
        backToEntity.MachineType.ShouldBe(originalMachine.MachineType);
        backToEntity.WorkFlowType.ShouldBe(originalMachine.WorkFlowType);
        backToEntity.EnableAppTraceability.ShouldBe(originalMachine.EnableAppTraceability);
        backToEntity.EnableBypassTraceability.ShouldBe(originalMachine.EnableBypassTraceability);
        backToEntity.Retry.ShouldBe(originalMachine.Retry);
        backToEntity.RuleId.ShouldBe(originalMachine.RuleId);
    }
    /// <summary>
    /// Executes MachineDto_WithNetworkConfiguration_ShouldHandleIndustrialNetworking operation.
    /// </summary>

    [Fact]
    public void MachineDto_WithNetworkConfiguration_ShouldHandleIndustrialNetworking()
    {
        // Arrange - Industrial network configurations
        var networkMachines = new List<MachineDto>
        {
            new() { MachineId = 100, Name = "CNC_001", IpAddress = "192.168.1.100", Location = "Zone_A" },
            new() { MachineId = 2, Name = "ROBOT_001", IpAddress = "192.168.1.101", Location = "Zone_A" },
            new() { MachineId = 3, Name = "PLC_001", IpAddress = "192.168.1.102", Location = "Zone_B" },
            new() { MachineId = 4, Name = "HMI_001", IpAddress = "192.168.1.103", Location = "Zone_B" }
        };

        // Act & Assert
        networkMachines.ForEach(machine =>
        {
            machine.MachineId.ShouldBeGreaterThan(0);
            machine.Name.ShouldNotBeNullOrEmpty();
            machine.IpAddress.ShouldStartWith("192.168.1.");
            machine.Location.ShouldContain("Zone");
        });

        networkMachines.Count.ShouldBe(4);
        networkMachines.All(m => m.IpAddress.Contains("192.168.1.")).ShouldBeTrue();
    }
    /// <summary>
    /// Executes MachineDto_WithAutomotiveManufacturing_ShouldHandleProductionLine operation.
    /// </summary>

    [Fact]
    public void MachineDto_WithAutomotiveManufacturing_ShouldHandleProductionLine()
    {
        // Arrange - Automotive production line machines
        var automotiveMachines = new List<MachineDto>
        {
            new()
            {
                MachineId = 100001,
                Name = "ENGINE_BLOCK_CNC",
                Location = "ENGINE_LINE",
                Description = "CNC for engine block machining",
                EnableAppTraceability = 1,
                Retry = 3
            },
            new()
            {
                MachineId = 2001,
                Name = "TRANSMISSION_ASSY",
                Location = "TRANSMISSION_LINE",
                Description = "Transmission assembly station",
                EnableAppTraceability = 1,
                Retry = 2
            },
            new()
            {
                MachineId = 3001,
                Name = "FINAL_INSPECTION",
                Location = "QUALITY_CONTROL",
                Description = "Final quality inspection station",
                EnableAppTraceability = 1,
                Retry = 1
            }
        };

        // Act & Assert
        automotiveMachines.All(m => m.EnableAppTraceability == 1).ShouldBeTrue();
        automotiveMachines.All(m => m.IsEnabled).ShouldBeTrue();
        automotiveMachines.All(m => m.Location.Contains("LINE") || m.Location.Contains("CONTROL")).ShouldBeTrue();

        var engineMachine = automotiveMachines.First(m => m.Name.Contains("ENGINE"));
        engineMachine.Description.ShouldContain("engine");
        engineMachine.Retry.ShouldBe(3); // Critical operations need more retries
    }
    /// <summary>
    /// Executes MachineDto_WithReliabilitySettings_ShouldSupportRetryConfiguration operation.
    /// </summary>

    [Fact]
    public void MachineDto_WithReliabilitySettings_ShouldSupportRetryConfiguration()
    {
        // Arrange - Different reliability requirements
        var reliabilityTests = new List<(string MachineName, int RetryCount, string Criticality)>
        {
            ("SAFETY_CRITICAL_PRESS", 5, "Critical safety operations"),
            ("PRECISION_CNC", 3, "High precision requirements"),
            ("CONVEYOR_STANDARD", 2, "Standard operations"),
            ("MONITORING_SENSOR", 1, "Non-critical monitoring")
        };

        // Act & Assert
        foreach (var (machineName, retryCount, criticality) in reliabilityTests)
        {
            var machine = new MachineDto
            {
                Name = machineName,
                Retry = retryCount,
                Description = criticality
            };

            machine.Retry.ShouldBe(retryCount);
            machine.Name.ShouldBe(machineName);

            // Verify retry count aligns with criticality
            if (criticality.Contains("Critical"))
                machine.Retry.ShouldBeGreaterThanOrEqualTo(3);
            else if (criticality.Contains("High"))
                machine.Retry.ShouldBeGreaterThanOrEqualTo(2);
            else if (criticality.Contains("Standard"))
                machine.Retry.ShouldBeGreaterThanOrEqualTo(1);
        }
    }
    /// <summary>
    /// Executes MachineDto_WithTraceabilityCompliance_ShouldSupportRegulatoryRequirements operation.
    /// </summary>

    [Fact]
    public void MachineDto_WithTraceabilityCompliance_ShouldSupportRegulatoryRequirements()
    {
        // Arrange - Regulatory compliance scenarios
        var complianceMachine = new MachineDto
        {
            MachineId = 9999,
            Name = "PHARMACEUTICAL_TABLET_PRESS",
            Location = "CLEANROOM_GRADE_C",
            Description = "FDA-compliant tablet manufacturing press with full traceability",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0, // No bypass for pharmaceutical
            Retry = 5, // High reliability requirement
            RuleId = 21001 // CFR 21 Part 11 compliance rule
        };

        // Act & Assert
        complianceMachine.IsEnabled.ShouldBeTrue();
        complianceMachine.EnableAppTraceability.ShouldBe(1);
        complianceMachine.EnableBypassTraceability.ShouldBe(0);
        complianceMachine.Location.ShouldContain("CLEANROOM");
        complianceMachine.Description.ShouldContain("FDA-compliant");
        complianceMachine.Retry.ShouldBeGreaterThanOrEqualTo(5);
        complianceMachine.RuleId.ShouldBe(21001);
    }
}