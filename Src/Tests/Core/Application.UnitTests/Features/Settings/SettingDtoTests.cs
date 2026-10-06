// <copyright file="SettingDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for SettingDto data transfer object.
/// Tests the industrial configuration management system for machine settings.
/// </summary>
public class SettingDtoTests
{
    /// <summary>
    /// Executes SettingDto_Constructor_ShouldInitializeWithDefaults operation.
    /// </summary>
    [Fact]
    public void SettingDto_Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var settingDto = new SettingDto();

        // Assert
        settingDto.SettingId.ShouldBe(0);
        settingDto.MachineId.ShouldBe(0);
        settingDto.Config.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes SettingDto_Properties_ShouldAllowGetAndSet operation.
    /// </summary>

    [Fact]
    public void SettingDto_Properties_ShouldAllowGetAndSet()
    {
        // Arrange
        var settingDto = new SettingDto();
        const int settingId = 12345;
        const int machineId = 9876;
        const string config = "{\"temperature\":75,\"pressure\":150,\"vibration_threshold\":0.5}";

        // Act
        settingDto.SettingId = settingId;
        settingDto.MachineId = machineId;
        settingDto.Config = config;

        // Assert
        settingDto.SettingId.ShouldBe(settingId);
        settingDto.MachineId.ShouldBe(machineId);
        settingDto.Config.ShouldBe(config);
    }

    /// <summary>
    /// Executes SettingDto_WithIndustrialConfigurations_ShouldHandleManufacturingSettings operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 5001, "{\"line_speed\":120,\"quality_check\":true}")]
    [InlineData(2002, 6002, "{\"temperature_setpoint\":85,\"pressure_limit\":200}")]
    [InlineData(3003, 7003, "{\"cycle_time\":45,\"tool_wear_limit\":1000}")]
    [InlineData(4004, 8004, "{\"conveyor_speed\":1.5,\"reject_threshold\":2}")]
    public void SettingDto_WithIndustrialConfigurations_ShouldHandleManufacturingSettings(
        int settingId, int machineId, string config)
    {
        // Arrange & Act
        var settingDto = new SettingDto
        {
            SettingId = settingId,
            MachineId = machineId,
            Config = config
        };

        // Assert
        settingDto.SettingId.ShouldBe(settingId);
        settingDto.MachineId.ShouldBe(machineId);
        settingDto.Config.ShouldBe(config);
    }

    /// <summary>
    /// Executes ToDto_WithValidSetting_ShouldCreateCorrectDto operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidSetting_ShouldCreateCorrectDto()
    {
        // Arrange
        var setting = new Setting
        {
            SettingId = 99999,
            MachineId = new MachineId(100001),
            Config = "{\"oee_target\":0.85,\"downtime_threshold\":300}"
        };

        // Act
        var resultWrapper = SettingDto.ToDto(setting);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.SettingId.ShouldBe(99999);
        result.MachineId.ShouldBe(100001);
        result.Config.ShouldBe("{\"oee_target\":0.85,\"downtime_threshold\":300}");
    }

    /// <summary>
    /// Executes ToDto_WithNullSetting_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullSetting_ShouldReturnFailureResult()
    {
        // Act
        var result = SettingDto.ToDto(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes ToEntity_WithValidDto_ShouldCreateCorrectEntity operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidDto_ShouldCreateCorrectEntity()
    {
        // Arrange
        var settingDto = new SettingDto
        {
            SettingId = 77777,
            MachineId = 2002,
            Config = "{\"maintenance_interval\":168,\"lubrication_schedule\":24}"
        };

        // Act
        var resultWrapper = SettingDto.ToEntity(settingDto);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.SettingId.ShouldBe(77777);
        result.MachineId.Value.ShouldBe(2002);
        result.Config.ShouldBe("{\"maintenance_interval\":168,\"lubrication_schedule\":24}");
    }

    /// <summary>
    /// Executes ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Act
        var result = SettingDto.ToEntity(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes RoundTrip_SettingToDto_ShouldPreserveData operation.
    /// </summary>

    [Fact]
    public void RoundTrip_SettingToDto_ShouldPreserveData()
    {
        // Arrange
        var originalSetting = new Setting
        {
            SettingId = 88888,
            MachineId = new MachineId(3003),
            Config = "{\"safety_sensors\":true,\"emergency_stop\":true,\"light_curtain\":enabled}"
        };

        // Act
        var dtoWrapper = SettingDto.ToDto(originalSetting);
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();




        var backToEntityWrapper = SettingDto.ToEntity(dtoWrapper.Value);

        // Assert
        backToEntityWrapper.IsSuccess.ShouldBeTrue();
        backToEntityWrapper.Value.ShouldNotBeNull();
        var backToEntity = backToEntityWrapper.Value;
        backToEntity.ShouldNotBeNull();
        backToEntity.ShouldNotBeNull();
        backToEntity.ShouldNotBeNull();
        backToEntity.SettingId.ShouldBe(originalSetting.SettingId);
        backToEntity.MachineId.ShouldBe(originalSetting.MachineId);
        backToEntity.Config.ShouldBe(originalSetting.Config);
    }

    /// <summary>
    /// Executes ToDtoList_WithValidSettings_ShouldCreateCorrectDtoList operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithValidSettings_ShouldCreateCorrectDtoList()
    {
        // Arrange
        var settings = new List<Setting>
        {
            new() { SettingId = 1, MachineId = new MachineId(10001), Config = "{\"mode\":\"auto\"}" },
            new() { SettingId = 2, MachineId = new MachineId(10002), Config = "{\"mode\":\"manual\"}" },
            new() { SettingId = 3, MachineId = new MachineId(10003), Config = "{\"mode\":\"maintenance\"}" }
        };

        // Act
        var resultWrapper = SettingDto.ToDtoList(settings);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value.ToList();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        result[0].SettingId.ShouldBe(1);
        result[0].MachineId.ShouldBe(10001);
        result[0].Config.ShouldBe("{\"mode\":\"auto\"}");

        result[1].SettingId.ShouldBe(2);
        result[1].MachineId.ShouldBe(10002);
        result[1].Config.ShouldBe("{\"mode\":\"manual\"}");

        result[2].SettingId.ShouldBe(3);
        result[2].MachineId.ShouldBe(10003);
        result[2].Config.ShouldBe("{\"mode\":\"maintenance\"}");
    }

    /// <summary>
    /// Executes ToDtoList_WithNullSettings_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithNullSettings_ShouldReturnFailureResult()
    {
        // Act & Assert
        var result = SettingDto.ToDtoList(null!);
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Setting collection cannot be null");
    }

    /// <summary>
    /// Executes ToDtoList_WithEmptySettings_ShouldReturnEmptyList operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithEmptySettings_ShouldReturnEmptyList()
    {
        // Arrange
        var settings = new List<Setting>();

        // Act
        var resultWrapper = SettingDto.ToDtoList(settings);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value.ToList();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes SettingDto_WithDifferentConfigTypes_ShouldHandleVariousFormats operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 5001, "{\"production_line\":\"A1\",\"shift\":\"morning\"}")]
    [InlineData(2002, 6002, "{\"quality_control\":\"enabled\",\"inspection_level\":\"high\"}")]
    [InlineData(3003, 7003, "{\"predictive_maintenance\":true,\"sensor_monitoring\":\"continuous\"}")]
    [InlineData(4004, 8004, "{\"energy_efficiency\":\"optimized\",\"power_save_mode\":true}")]
    public void SettingDto_WithDifferentConfigTypes_ShouldHandleVariousFormats(
        int settingId, int machineId, string config)
    {
        // Arrange
        var settingDto = new SettingDto();

        // Act
        settingDto.SettingId = settingId;
        settingDto.MachineId = machineId;
        settingDto.Config = config;

        // Assert
        settingDto.SettingId.ShouldBe(settingId);
        settingDto.MachineId.ShouldBe(machineId);
        settingDto.Config.ShouldBe(config);
        // Verify each configuration represents a valid industrial setting format
    }

    /// <summary>
    /// Executes SettingDto_WithComplexJsonConfig_ShouldHandleNestedStructures operation.
    /// </summary>

    [Fact]
    public void SettingDto_WithComplexJsonConfig_ShouldHandleNestedStructures()
    {
        // Arrange
        const string complexConfig = @"{
            ""machine_parameters"": {
                ""temperature"": {""min"": 70, ""max"": 90, ""current"": 75},
                ""pressure"": {""min"": 100, ""max"": 200, ""current"": 150},
                ""vibration"": {""threshold"": 0.5, ""current"": 0.2}
            },
            ""safety_settings"": {
                ""emergency_stop"": true,
                ""light_curtain"": ""enabled"",
                ""access_control"": ""restricted""
            },
            ""production_settings"": {
                ""cycle_time"": 45,
                ""target_output"": 1000,
                ""quality_threshold"": 0.95
            }
        }";

        var settingDto = new SettingDto();

        // Act
        settingDto.SettingId = 9999;
        settingDto.MachineId = 8888;
        settingDto.Config = complexConfig;

        // Assert
        settingDto.SettingId.ShouldBe(9999);
        settingDto.MachineId.ShouldBe(8888);
        settingDto.Config.ShouldBe(complexConfig);
        settingDto.Config.ShouldContain("machine_parameters");
        settingDto.Config.ShouldContain("safety_settings");
        settingDto.Config.ShouldContain("production_settings");
    }

    /// <summary>
    /// Executes SettingDto_WithIndustrialMachineTypes_ShouldSupportVariousEquipment operation.
    /// </summary>

    [Fact]
    public void SettingDto_WithIndustrialMachineTypes_ShouldSupportVariousEquipment()
    {
        // Arrange - Different industrial machine configurations
        var cncMachineConfig = "{\"spindle_speed\":3000,\"feed_rate\":500,\"tool_life\":1000}";
        var robotConfig = "{\"payload\":50,\"reach\":1500,\"precision\":0.1}";
        var conveyorConfig = "{\"speed\":1.2,\"load_capacity\":100,\"auto_sort\":true}";
        var pressConfig = "{\"force\":500000,\"stroke\":200,\"safety_mat\":\"enabled\"}";

        var settings = new List<SettingDto>
        {
            new() { SettingId = 1, MachineId = 100001, Config = cncMachineConfig },
            new() { SettingId = 2, MachineId = 2001, Config = robotConfig },
            new() { SettingId = 3, MachineId = 3001, Config = conveyorConfig },
            new() { SettingId = 4, MachineId = 4001, Config = pressConfig }
        };

        // Act & Assert
        settings.ForEach(setting =>
        {
            setting.SettingId.ShouldBeGreaterThan(0);
            setting.MachineId.ShouldBeGreaterThan(1000);
            setting.Config.ShouldNotBeNullOrEmpty();
            setting.Config.ShouldStartWith("{");
            setting.Config.ShouldEndWith("}");
        });
    }

    /// <summary>
    /// Executes SettingDto_WithLargeScaleManufacturing_ShouldHandleHighVolumeConfigurations operation.
    /// </summary>

    [Fact]
    public void SettingDto_WithLargeScaleManufacturing_ShouldHandleHighVolumeConfigurations()
    {
        // Arrange - High-volume manufacturing settings
        var settingDto = new SettingDto
        {
            SettingId = 999999,
            MachineId = 888888,
            Config = @"{
                ""production_targets"": {
                    ""daily_output"": 50000,
                    ""hourly_rate"": 2083,
                    ""efficiency_target"": 0.92
                },
                ""quality_parameters"": {
                    ""defect_rate_target"": 0.001,
                    ""inspection_frequency"": 100,
                    ""statistical_process_control"": true
                },
                ""maintenance_schedule"": {
                    ""preventive_interval_hours"": 168,
                    ""predictive_monitoring"": true,
                    ""lubrication_points"": [""bearing_1"", ""bearing_2"", ""gear_box""]
                },
                ""energy_management"": {
                    ""power_optimization"": true,
                    ""idle_power_reduction"": 0.7,
                    ""peak_demand_management"": true
                }
            }"
        };

        // Act & Assert
        settingDto.SettingId.ShouldBe(999999);
        settingDto.MachineId.ShouldBe(888888);
        settingDto.Config.ShouldContain("production_targets");
        settingDto.Config.ShouldContain("quality_parameters");
        settingDto.Config.ShouldContain("maintenance_schedule");
        settingDto.Config.ShouldContain("energy_management");
        settingDto.Config.Length.ShouldBeGreaterThan(500); // Complex configuration
    }
}