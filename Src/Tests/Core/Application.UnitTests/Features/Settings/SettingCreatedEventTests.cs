// <copyright file="SettingCreatedEventTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for SettingCreatedEvent - Notification event for setting creation in manufacturing systems.
/// Tests property validation, notification interface compliance, and manufacturing configuration scenarios.
/// </summary>
public class SettingCreatedEventTests
{
    /// <summary>
    /// Executes Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var instance = new SettingCreatedEvent();

        // Assert
        instance.ShouldNotBeNull();
        instance.SettingId.ShouldBe(0);
        instance.MachineId.ShouldBe(0);
        instance.Setting.ShouldBe("");
        instance.Machine.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<INotification>();
    }

    /// <summary>
    /// Executes Properties_WithValidManufacturingSettings_ShouldSetAndReturnCorrectValues operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 2001, "ProductionLineSpeed=85")]
    [InlineData(1002, 2002, "QualityThreshold=95")]
    [InlineData(1003, 2003, "TemperatureLimit=180")]
    [InlineData(1004, 2004, "PressureMax=250")]
    [InlineData(1005, 2005, "VibrationLevel=Low")]
    public void Properties_WithValidManufacturingSettings_ShouldSetAndReturnCorrectValues(
        int settingId, int machineId, string setting)
    {
        // Arrange
        var settingCreated = new SettingCreatedEvent();
        var machine = new Machine { MachineId = new MachineId(machineId), Name = $"Machine-{machineId}" };

        // Act
        settingCreated.SettingId = settingId;
        settingCreated.MachineId = machineId;
        settingCreated.Setting = setting;
        settingCreated.Machine = machine;

        // Assert
        settingCreated.SettingId.ShouldBe(settingId);
        settingCreated.MachineId.ShouldBe(machineId);
        settingCreated.Setting.ShouldBe(setting);
        settingCreated.Machine.ShouldBe(machine);
        settingCreated.Machine.MachineId.Value.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes SettingCreated_ShouldImplementINotification operation.
    /// </summary>

    [Fact]
    public void SettingCreated_ShouldImplementINotification()
    {
        // Arrange & Act
        var settingCreated = new SettingCreatedEvent();

        // Assert
        settingCreated.ShouldBeAssignableTo<INotification>();
    }

    /// <summary>
    /// Executes Properties_WithAutomotiveManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(3001, 4001, "Ford-F150-EngineSpeed=2500")]
    [InlineData(3002, 4002, "Tesla-ModelS-BatteryTemp=45")]
    [InlineData(3003, 4003, "BMW-X5-TransmissionPressure=180")]
    [InlineData(3004, 4004, "Mercedes-CClass-BrakeForce=850")]
    [InlineData(3005, 4005, "Oemx-A4-FuelInjection=25")]
    public void Properties_WithAutomotiveManufacturingScenarios_ShouldHandleCorrectly(
        int settingId, int machineId, string setting)
    {
        // Arrange
        var settingCreated = new SettingCreatedEvent();
        var machine = new Machine { MachineId = new MachineId(machineId), Name = $"Automotive-Machine-{machineId}", Location = "Production Line A" };

        // Act
        settingCreated.SettingId = settingId;
        settingCreated.MachineId = machineId;
        settingCreated.Setting = setting;
        settingCreated.Machine = machine;

        // Assert
        settingCreated.SettingId.ShouldBe(settingId);
        settingCreated.MachineId.ShouldBe(machineId);
        settingCreated.Setting.ShouldBe(setting);
        settingCreated.Machine.Location.ShouldBe("Production Line A");
    }

    /// <summary>
    /// Executes Setting_WithDifferentConfigurationFormats_ShouldAcceptVariousFormats operation.
    /// </summary>

    [Theory]
    [InlineData(5001, 6001, "JSON={\"temperature\":75,\"pressure\":150}", "Complex JSON configuration")]
    [InlineData(5002, 6002, "XML=<config><speed>100</speed></config>", "XML-based configuration")]
    [InlineData(5003, 6003, "CSV=speed,100,temp,75,pressure,150", "CSV format configuration")]
    [InlineData(5004, 6004, "Key1=Value1;Key2=Value2;Key3=Value3", "Key-value pair configuration")]
    [InlineData(5005, 6005, "BINARY=01001100010101", "Binary configuration data")]
    public void Setting_WithDifferentConfigurationFormats_ShouldAcceptVariousFormats(
        int settingId, int machineId, string setting, string description)
    {
        var logger = XUnitLogger.CreateLogger<SettingCreatedEventTests>();
        logger.LogInformation("Testing scenario: {description} with settingId={settingId}, machineId={machineId}, setting={setting}",
            description, settingId, machineId, setting);

        // Arrange
        var settingCreated = new SettingCreatedEvent();

        // Act
        settingCreated.SettingId = settingId;
        settingCreated.MachineId = machineId;
        settingCreated.Setting = setting;

        // Assert
        settingCreated.SettingId.ShouldBe(settingId);
        settingCreated.MachineId.ShouldBe(machineId);
        settingCreated.Setting.ShouldBe(setting);
    }

    /// <summary>
    /// Executes SettingId_WithEdgeValues_ShouldStoreValue operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(0, "Zero setting ID")]
    [InlineData(-1, "Negative setting ID")]
    [InlineData(int.MinValue, "Minimum integer boundary")]
    [InlineData(int.MaxValue, "Maximum integer boundary")]
    public void SettingId_WithEdgeValues_ShouldStoreValue(int settingId, string description)
    {
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var settingCreated = new SettingCreatedEvent();

        // Act
        settingCreated.SettingId = settingId;

        // Assert
        settingCreated.SettingId.ShouldBe(settingId);
    }

    /// <summary>
    /// Executes Setting_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="setting">The setting.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("", "Empty setting value")]
    [InlineData("   ", "Whitespace setting value")]
    [InlineData("VeryLongSettingValueThatExceedsTypicalConfigurationLengthsForManufacturingSystemsConfigurationParameters", "Very long setting value")]
    [InlineData("Special!@#$%^&*()Characters", "Special characters in setting")]
    public void Setting_WithEdgeCaseValues_ShouldStoreCorrectly(string setting, string description)
    {
        // Using parameters: setting, description
        _ = setting; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: setting, description
        _ = setting; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: setting, description
        _ = setting; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: setting, description
        _ = setting; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: setting, description
        _ = setting; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var settingCreated = new SettingCreatedEvent();

        // Act
        settingCreated.Setting = setting;

        // Assert
        settingCreated.Setting.ShouldBe(setting);
    }

    /// <summary>
    /// Executes SettingCreated_WithCompleteManufacturingScenario_ShouldSetAllPropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void SettingCreated_WithCompleteManufacturingScenario_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var settingId = 7001;
        var machineId = 8001;
        var setting = "QualityControlThreshold=98.5;OEE=85;CycleTime=4500";
        var machine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "QC-Machine-001",
            Location = "Quality Control Bay",
            MachineType = MachineType.Inspection.Value
        };

        // Act
        var settingCreated = new SettingCreatedEvent
        {
            SettingId = settingId,
            MachineId = machineId,
            Setting = setting,
            Machine = machine
        };

        // Assert
        settingCreated.SettingId.ShouldBe(settingId);
        settingCreated.MachineId.ShouldBe(machineId);
        settingCreated.Setting.ShouldBe(setting);
        settingCreated.Machine.ShouldBe(machine);
        settingCreated.Machine.Name.ShouldBe("QC-Machine-001");
        settingCreated.Machine.Location.ShouldBe("Quality Control Bay");
        settingCreated.Machine.MachineType.ShouldBe(MachineType.Inspection);
        settingCreated.ShouldBeAssignableTo<INotification>();
    }

    /// <summary>
    /// Executes Machine_WhenSetToNull_ShouldAllowNullValue operation.
    /// </summary>

    [Fact]
    public void Machine_WhenSetToNull_ShouldAllowNullValue()
    {
        // Arrange
        var settingCreated = new SettingCreatedEvent();

        // Act
        settingCreated.Machine = null!;

        // Assert
        settingCreated.Machine.ShouldBeNull();
    }

    /// <summary>
    /// Executes SettingCreated_WithSpecializedManufacturingSettings_ShouldHandleIndustrialScenarios operation.
    /// </summary>

    [Theory]
    [InlineData(9001, 10001, "HeavyIndustry-PressForce=2500", "Heavy industrial machinery setting")]
    [InlineData(9002, 10002, "Precision-ToleranceLevel=0.001", "Precision manufacturing setting")]
    [InlineData(9003, 10003, "Automated-RobotSpeed=150", "Automated assembly setting")]
    [InlineData(9004, 10004, "QualityInspection-AccuracyThreshold=99.9", "Quality inspection setting")]
    [InlineData(9005, 10005, "Packaging-SealPressure=175", "Packaging machinery setting")]
    public void SettingCreated_WithSpecializedManufacturingSettings_ShouldHandleIndustrialScenarios(
        int settingId, int machineId, string setting, string description)
    {
        var logger = XUnitLogger.CreateLogger<SettingCreatedEventTests>();
        logger.LogInformation("Testing scenario: {description} with settingId={settingId}, machineId={machineId}, setting={setting}",
            description, settingId, machineId, setting);

        // Arrange & Act
        var settingCreated = new SettingCreatedEvent
        {
            SettingId = settingId,
            MachineId = machineId,
            Setting = setting
        };

        // Assert
        settingCreated.SettingId.ShouldBe(settingId);
        settingCreated.MachineId.ShouldBe(machineId);
        settingCreated.Setting.ShouldBe(setting);
        settingCreated.ShouldBeAssignableTo<INotification>();
    }
}