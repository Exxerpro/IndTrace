// <copyright file="OeeConfigurationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Configuration;

/// <summary>
/// Unit tests for OeeConfiguration
/// </summary>
public class OeeConfigurationTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var configuration = new OeeConfiguration();

        // Assert
        configuration.ShouldNotBeNull();
        configuration.Enabled.ShouldBeFalse(); // Default value should be false
        configuration.EnabledByMachine.ShouldNotBeNull();
        configuration.EnabledByMachine.ShouldBeEmpty(); // Default collection should be empty
    }
    /// <summary>
    /// Executes Enabled_WhenSetToTrue_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void Enabled_WhenSetToTrue_ShouldReturnTrue()
    {
        // Arrange
        var configuration = new OeeConfiguration();

        // Act
        configuration.Enabled = true;

        // Assert
        configuration.Enabled.ShouldBeTrue();
    }
    /// <summary>
    /// Executes Enabled_WhenSetToFalse_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void Enabled_WhenSetToFalse_ShouldReturnFalse()
    {
        // Arrange
        var configuration = new OeeConfiguration();

        // Act
        configuration.Enabled = false;

        // Assert
        configuration.Enabled.ShouldBeFalse();
    }
    /// <summary>
    /// Executes EnabledByMachine_WhenPopulatedWithManufacturingMachines_ShouldReturnCorrectMappings operation.
    /// </summary>

    [Fact]
    public void EnabledByMachine_WhenPopulatedWithManufacturingMachines_ShouldReturnCorrectMappings()
    {
        // Arrange - Ford F-150 production line with multiple machines
        var configuration = new OeeConfiguration();
        var machineOeeSettings = new Dictionary<int, bool>
        {
            { 1501, true },  // Robotic Welding Cell #1 - OEE enabled
            { 1502, true },  // Paint Booth Station - OEE enabled  
            { 1503, false }, // Manual Assembly Station - OEE disabled
            { 1504, true },  // Final Inspection Station - OEE enabled
            { 1505, false }  // Packaging Station - OEE disabled
        };

        // Act
        configuration.EnabledByMachine = machineOeeSettings;

        // Assert
        configuration.EnabledByMachine.Count.ShouldBe(5);
        configuration.EnabledByMachine[1501].ShouldBeTrue();  // Welding Cell
        configuration.EnabledByMachine[1502].ShouldBeTrue();  // Paint Booth
        configuration.EnabledByMachine[1503].ShouldBeFalse(); // Manual Assembly
        configuration.EnabledByMachine[1504].ShouldBeTrue();  // Inspection
        configuration.EnabledByMachine[1505].ShouldBeFalse(); // Packaging
    }
    /// <summary>
    /// Executes Enabled_WithDifferentGlobalSettings_ShouldSetCorrectly operation.
    /// </summary>
    /// <param name="enabled">The enabled.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(true, "Global OEE enabled for entire production line")]
    [InlineData(false, "Global OEE disabled for maintenance mode")]
    public void Enabled_WithDifferentGlobalSettings_ShouldSetCorrectly(bool enabled, string description)
    {
        // Using parameters: enabled, description
        _ = enabled; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: enabled, description
        _ = enabled; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: enabled, description
        _ = enabled; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: enabled, description
        _ = enabled; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: enabled, description
        _ = enabled; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var configuration = new OeeConfiguration();

        // Act
        configuration.Enabled = enabled;

        // Assert
        configuration.Enabled.ShouldBe(enabled);
    }
    /// <summary>
    /// Executes OeeConfiguration_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void OeeConfiguration_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Ford F-150 production line OEE configuration
        var configuration = new OeeConfiguration
        {
            Enabled = true, // Global OEE enabled
            EnabledByMachine = new Dictionary<int, bool>
            {
                { 1501, true },  // Fanuc R-2000iC Welding Robot - High-value equipment, OEE critical
                { 1502, true },  // ABB Paint Robot - Quality-critical process, OEE enabled
                { 1503, false }, // Manual Bolt Tightening - Manual process, OEE not applicable
                { 1504, true },  // Cognex Vision Inspection - Automated QC, OEE enabled
                { 1505, true }   // Final Assembly Robot - End-of-line automation, OEE enabled
            }
        };

        // Act & Assert
        configuration.Enabled.ShouldBeTrue();
        configuration.EnabledByMachine.Count.ShouldBe(5);
        configuration.EnabledByMachine.Where(kvp => kvp.Value).Count().ShouldBe(4); // 4 machines with OEE enabled
        configuration.EnabledByMachine.Where(kvp => !kvp.Value).Count().ShouldBe(1); // 1 machine with OEE disabled
    }
    /// <summary>
    /// Executes OeeConfiguration_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void OeeConfiguration_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - iPhone PCB assembly line OEE configuration
        var configuration = new OeeConfiguration
        {
            Enabled = true, // Global OEE monitoring for high-volume production
            EnabledByMachine = new Dictionary<int, bool>
            {
                { 3301, true },  // Fuji NXT SMT Pick & Place - Critical bottleneck, OEE essential
                { 3302, true },  // Reflow Oven - Thermal process control, OEE monitoring
                { 3303, true },  // AOI Inspection System - Quality gate, OEE tracking important
                { 3304, false }, // Manual Rework Station - Manual process, OEE not applicable
                { 3305, true }   // ICT Testing Equipment - Final test, OEE critical for throughput
            }
        };

        // Act & Assert
        configuration.Enabled.ShouldBeTrue();
        configuration.EnabledByMachine.Count.ShouldBe(5);
        configuration.EnabledByMachine[3301].ShouldBeTrue(); // SMT machine - critical for OEE
        configuration.EnabledByMachine[3304].ShouldBeFalse(); // Manual station - OEE not applicable
    }
    /// <summary>
    /// Executes OeeConfiguration_WithPharmaceuticalManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void OeeConfiguration_WithPharmaceuticalManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Vaccine production OEE configuration with GMP compliance considerations
        var configuration = new OeeConfiguration
        {
            Enabled = true, // OEE critical for regulatory compliance and efficiency
            EnabledByMachine = new Dictionary<int, bool>
            {
                { 4401, true },  // Bosch GKF 1500 Filling Machine - Critical process, OEE required
                { 4402, true },  // Serialization Equipment - Regulatory requirement, OEE tracking
                { 4403, false }, // Manual Quality Inspection - Human-dependent, OEE not applicable
                { 4404, true },  // Automated Packaging Line - High-speed operation, OEE essential
                { 4405, true }   // Cold Chain Storage Robot - Temperature-critical, OEE monitoring
            }
        };

        // Act & Assert
        configuration.Enabled.ShouldBeTrue();
        configuration.EnabledByMachine.Count.ShouldBe(5);
        configuration.EnabledByMachine[4401].ShouldBeTrue(); // Filling machine - GMP critical
        configuration.EnabledByMachine[4403].ShouldBeFalse(); // Manual inspection - not automated
    }
    /// <summary>
    /// Executes EnabledByMachine_WhenModifiedAfterInitialization_ShouldReflectChanges operation.
    /// </summary>

    [Fact]
    public void EnabledByMachine_WhenModifiedAfterInitialization_ShouldReflectChanges()
    {
        // Arrange
        var configuration = new OeeConfiguration
        {
            EnabledByMachine = new Dictionary<int, bool>
            {
                { 1501, false }, // Initially disabled
                { 1502, true }   // Initially enabled
            }
        };

        // Act - Modify machine OEE settings (e.g., after equipment upgrade)
        configuration.EnabledByMachine[1501] = true;  // Enable OEE after automation upgrade
        configuration.EnabledByMachine[1502] = false; // Disable OEE for maintenance
        configuration.EnabledByMachine.Add(1503, true); // Add new machine with OEE enabled

        // Assert
        configuration.EnabledByMachine[1501].ShouldBeTrue();  // Now enabled
        configuration.EnabledByMachine[1502].ShouldBeFalse(); // Now disabled
        configuration.EnabledByMachine[1503].ShouldBeTrue();  // Newly added
        configuration.EnabledByMachine.Count.ShouldBe(3);
    }
    /// <summary>
    /// Executes EnabledByMachine_WithVariousIndustrialEquipment_ShouldSetCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="oeeEnabled">The oeeEnabled.</param>
    /// <param name="equipmentDescription">The equipmentDescription.</param>

    [Theory]
    [InlineData(1501, true, "Fanuc Welding Robot")]
    [InlineData(2801, true, "Tesla Battery Assembly Robot")]
    [InlineData(3301, true, "iPhone SMT Pick & Place")]
    [InlineData(4401, true, "Vaccine Filling Machine")]
    [InlineData(5501, false, "Manual Bottling Inspection")]
    [InlineData(6601, true, "CNC Machining Center")]
    [InlineData(7701, true, "Boeing Fuselage Assembly")]
    public void EnabledByMachine_WithVariousIndustrialEquipment_ShouldSetCorrectly(int machineId, bool oeeEnabled, string equipmentDescription)
    {
        // Using parameters: machineId, oeeEnabled, equipmentDescription
        _ = machineId; // xUnit1026 fix
        _ = oeeEnabled; // xUnit1026 fix
        _ = equipmentDescription; // xUnit1026 fix
        // Using parameters: machineId, oeeEnabled, equipmentDescription
        _ = machineId; // xUnit1026 fix
        _ = oeeEnabled; // xUnit1026 fix
        _ = equipmentDescription; // xUnit1026 fix
        // Using parameters: machineId, oeeEnabled, equipmentDescription
        _ = machineId; // xUnit1026 fix
        _ = oeeEnabled; // xUnit1026 fix
        _ = equipmentDescription; // xUnit1026 fix
        // Using parameters: machineId, oeeEnabled, equipmentDescription
        _ = machineId; // xUnit1026 fix
        _ = oeeEnabled; // xUnit1026 fix
        _ = equipmentDescription; // xUnit1026 fix
        // Using parameters: machineId, oeeEnabled, equipmentDescription
        _ = machineId; // xUnit1026 fix
        _ = oeeEnabled; // xUnit1026 fix
        _ = equipmentDescription; // xUnit1026 fix
        // Arrange
        var configuration = new OeeConfiguration();

        // Act
        configuration.EnabledByMachine[machineId] = oeeEnabled;

        // Assert
        configuration.EnabledByMachine[machineId].ShouldBe(oeeEnabled);
        configuration.EnabledByMachine.ContainsKey(machineId).ShouldBeTrue();
    }
    /// <summary>
    /// Executes OeeConfiguration_WithWorldClassManufacturingTargets_ShouldEnableStrategicMachines operation.
    /// </summary>

    [Fact]
    public void OeeConfiguration_WithWorldClassManufacturingTargets_ShouldEnableStrategicMachines()
    {
        // Arrange - World-class manufacturing focusing on bottleneck and high-value equipment
        var configuration = new OeeConfiguration
        {
            Enabled = true, // Global OEE for continuous improvement
            EnabledByMachine = new Dictionary<int, bool>
            {
                // High-value automated equipment (OEE critical)
                { 1501, true },  // $2M Robotic Welding Cell - ROI dependent on utilization
                { 2801, true },  // $1.5M Battery Assembly Robot - Bottleneck operation
                { 3301, true },  // $800K SMT Pick & Place - High-speed critical process
                { 4401, true },  // $1.2M Pharmaceutical Filling Line - Regulatory compliance
                { 7701, true },  // $3M CNC Machining Center - Precision manufacturing
                
                // Manual or low-automation processes (OEE less critical)
                { 1510, false }, // Manual Material Handling - Human-paced
                { 2810, false }, // Manual Quality Inspection - Skill-dependent
                { 3310, false }, // Manual Rework Station - Variable demand
                { 4410, false }, // Manual Labeling - Low automation
                { 7710, false }  // Manual Assembly - Craft work
            }
        };

        // Act & Assert
        configuration.Enabled.ShouldBeTrue();
        configuration.EnabledByMachine.Count.ShouldBe(10);

        // High-value automated equipment should have OEE enabled
        var automatedMachines = configuration.EnabledByMachine.Where(kvp => kvp.Value).ToList();
        automatedMachines.Count.ShouldBe(5);
        automatedMachines.ShouldAllBe(kvp => kvp.Key.ToString().EndsWith("01")); // Pattern: automated machines end in "01"

        // Manual processes should have OEE disabled
        var manualProcesses = configuration.EnabledByMachine.Where(kvp => !kvp.Value).ToList();
        manualProcesses.Count.ShouldBe(5);
        manualProcesses.ShouldAllBe(kvp => kvp.Key.ToString().EndsWith("10")); // Pattern: manual processes end in "10"
    }
    /// <summary>
    /// Executes OeeConfiguration_PropertyRoundTrip_ShouldMaintainValues operation.
    /// </summary>

    [Fact]
    public void OeeConfiguration_PropertyRoundTrip_ShouldMaintainValues()
    {
        // Arrange
        var configuration = new OeeConfiguration();
        var originalEnabled = true;
        var originalMachineSettings = new Dictionary<int, bool>
        {
            { 1001, true },
            { 1002, false },
            { 1003, true }
        };

        // Act
        configuration.Enabled = originalEnabled;
        configuration.EnabledByMachine = originalMachineSettings;

        // Assert
        configuration.Enabled.ShouldBe(originalEnabled);
        configuration.EnabledByMachine.ShouldBe(originalMachineSettings);
        configuration.EnabledByMachine.Count.ShouldBe(3);
    }
    /// <summary>
    /// Executes EnabledByMachine_WhenSetToNull_ShouldAcceptNullValue operation.
    /// </summary>

    [Fact]
    public void EnabledByMachine_WhenSetToNull_ShouldAcceptNullValue()
    {
        // Arrange
        var configuration = new OeeConfiguration();

        // Act
        configuration.EnabledByMachine = null!;

        // Assert
        configuration.EnabledByMachine.ShouldBeNull();
    }
    /// <summary>
    /// Executes OeeConfiguration_WithEmptyMachineList_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void OeeConfiguration_WithEmptyMachineList_ShouldHandleGracefully()
    {
        // Arrange & Act
        var configuration = new OeeConfiguration
        {
            Enabled = true,
            EnabledByMachine = [] // Empty but not null
        };

        // Assert
        configuration.Enabled.ShouldBeTrue();
        configuration.EnabledByMachine.ShouldNotBeNull();
        configuration.EnabledByMachine.ShouldBeEmpty();
        configuration.EnabledByMachine.Count.ShouldBe(0);
    }
    /// <summary>
    /// Executes OeeConfiguration_WithLargeScaleManufacturingPlant_ShouldHandleMultipleMachines operation.
    /// </summary>

    [Fact]
    public void OeeConfiguration_WithLargeScaleManufacturingPlant_ShouldHandleMultipleMachines()
    {
        // Arrange - Large automotive plant with 50+ machines
        var configuration = new OeeConfiguration { Enabled = true };
        var largePlantMachines = new Dictionary<int, bool>();

        // Simulate large plant with mixed OEE requirements
        for (int i = 1; i <= 50; i++)
        {
            // Critical automated processes (every 3rd machine)
            bool isAutomated = i % 3 == 0;
            largePlantMachines.Add(1000 + i, isAutomated);
        }

        // Act
        configuration.EnabledByMachine = largePlantMachines;

        // Assert
        configuration.EnabledByMachine.Count.ShouldBe(50);
        var automatedCount = configuration.EnabledByMachine.Where(kvp => kvp.Value).Count();
        var manualCount = configuration.EnabledByMachine.Where(kvp => !kvp.Value).Count();

        automatedCount.ShouldBe(16); // Every 3rd machine (50/3 ≈ 16)
        manualCount.ShouldBe(34);    // Remaining machines
        (automatedCount + manualCount).ShouldBe(50);
    }
}
