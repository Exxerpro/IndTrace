// <copyright file="RulesDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.RulesEngine;

/// <summary>
/// Unit tests for RuleDto data transfer object.
/// Tests the business rules engine system for validating production rules.
/// </summary>
public class RuleDtoTests
{
    /// <summary>
    /// Executes RuleDto_Constructor_ShouldInitializeWithDefaults operation.
    /// </summary>
    [Fact]
    public void RuleDto_Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var ruleDto = new RuleDto();

        // Assert
        ruleDto.RuleId.ShouldBe(0);
        ruleDto.RuleJson.ShouldBe(string.Empty);
        ruleDto.Name.ShouldBe(string.Empty);
        ruleDto.Description.ShouldBe(string.Empty);
        ruleDto.Version.ShouldBe(0);
        ruleDto.IsActive.ShouldBeFalse();
    }
    /// <summary>
    /// Executes RuleDto_Properties_ShouldAllowGetAndSet operation.
    /// </summary>

    [Fact]
    public void RuleDto_Properties_ShouldAllowGetAndSet()
    {
        // Arrange
        var ruleDto = new RuleDto();
        const int ruleId = 12345;
        const string ruleJson = "{\"condition\":\"temperature > 85\",\"action\":\"alert\"}";
        const string name = "Temperature_Monitor_Rule";
        const string description = "Monitors temperature and triggers alerts";
        const int version = 3;
        const bool isActive = true;

        // Act
        ruleDto.RuleId = ruleId;
        ruleDto.RuleJson = ruleJson;
        ruleDto.Name = name;
        ruleDto.Description = description;
        ruleDto.Version = version;
        ruleDto.IsActive = isActive;

        // Assert
        ruleDto.RuleId.ShouldBe(ruleId);
        ruleDto.RuleJson.ShouldBe(ruleJson);
        ruleDto.Name.ShouldBe(name);
        ruleDto.Description.ShouldBe(description);
        ruleDto.Version.ShouldBe(version);
        ruleDto.IsActive.ShouldBe(isActive);
    }
    /// <summary>
    /// Executes RuleDto_WithIndustrialRules_ShouldHandleManufacturingScenarios operation.
    /// </summary>

    [Theory]
    [InlineData(1001, "Quality_Control_Rule", "Validates part dimensions within tolerance", 1, true)]
    [InlineData(2002, "Temperature_Alert_Rule", "Monitors machine temperature thresholds", 2, true)]
    [InlineData(3003, "Cycle_Time_Optimization", "Optimizes production cycle times", 1, false)]
    [InlineData(4004, "Safety_Interlock_Rule", "Ensures safety protocols are followed", 3, true)]
    public void RuleDto_WithIndustrialRules_ShouldHandleManufacturingScenarios(
        int ruleId, string name, string description, int version, bool isActive)
    {
        // Arrange & Act
        var ruleDto = new RuleDto
        {
            RuleId = ruleId,
            Name = name,
            Description = description,
            Version = version,
            IsActive = isActive,
            RuleJson = "{\"type\":\"industrial\",\"domain\":\"manufacturing\"}"
        };

        // Assert
        ruleDto.RuleId.ShouldBe(ruleId);
        ruleDto.Name.ShouldBe(name);
        ruleDto.Description.ShouldBe(description);
        ruleDto.Version.ShouldBe(version);
        ruleDto.IsActive.ShouldBe(isActive);
        ruleDto.RuleJson.ShouldContain("industrial");
    }
    /// <summary>
    /// Executes ToDto_WithValidRule_ShouldCreateCorrectDto operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidRule_ShouldCreateCorrectDto()
    {
        // Arrange
        var rule = new Rule
        {
            RuleId = 99999,
            RuleJson = "{\"condition\":\"pressure > 3000\",\"action\":\"shutdown\"}",
            Name = "Test_Safety_Rule",
            Description = "Test rule for safety validation",
            Version = 2,
            IsActive = true
        };

        // Act
        var resultWrapper = RuleDto.ToDto(rule);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.RuleId.ShouldBe(99999);
        result.RuleJson.ShouldBe("{\"condition\":\"pressure > 3000\",\"action\":\"shutdown\"}");
        result.Name.ShouldBe("Test_Safety_Rule");
        result.Description.ShouldBe("Test rule for safety validation");
        result.Version.ShouldBe(2);
        result.IsActive.ShouldBeTrue();
    }
    /// <summary>
    /// Executes ToDto_WithNullRule_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullRule_ShouldReturnFailureResult()
    {
        // Act
        var result = RuleDto.ToDto(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Rule source cannot be null");
    }
    /// <summary>
    /// Executes ToEntity_WithValidDto_ShouldCreateCorrectEntity operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidDto_ShouldCreateCorrectEntity()
    {
        // Arrange
        var ruleDto = new RuleDto
        {
            RuleId = 77777,
            RuleJson = "{\"condition\":\"vibration > 0.5\",\"action\":\"maintenance\"}",
            Name = "Vibration_Monitor",
            Description = "Monitors equipment vibration levels",
            Version = 1,
            IsActive = true
        };

        // Act
        var resultWrapper = RuleDto.ToEntity(ruleDto);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.RuleId.ShouldBe(77777);
        result.RuleJson.ShouldBe("{\"condition\":\"vibration > 0.5\",\"action\":\"maintenance\"}");
        result.Name.ShouldBe("Vibration_Monitor");
        result.Description.ShouldBe("Monitors equipment vibration levels");
        result.Version.ShouldBe(1);
        result.IsActive.ShouldBeTrue();
    }
    /// <summary>
    /// Executes ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Act
        var result = RuleDto.ToEntity(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("RuleDto source cannot be null");
    }
    /// <summary>
    /// Executes RoundTrip_RuleToDto_ShouldPreserveData operation.
    /// </summary>

    [Fact]
    public void RoundTrip_RuleToDto_ShouldPreserveData()
    {
        // Arrange
        var originalRule = new Rule
        {
            RuleId = 88888,
            RuleJson = "{\"condition\":\"temperature > 90\",\"action\":\"cooldown\"}",
            Name = "Temperature_Control",
            Description = "Controls equipment temperature",
            Version = 4,
            IsActive = false
        };

        // Act
        var dtoWrapper = RuleDto.ToDto(originalRule);
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;

        var backToEntityWrapper = RuleDto.ToEntity(dto);

        // Assert
        backToEntityWrapper.IsSuccess.ShouldBeTrue();
        backToEntityWrapper.Value.ShouldNotBeNull();
        var backToEntity = backToEntityWrapper.Value;
        backToEntity.ShouldNotBeNull();
        backToEntity.ShouldNotBeNull();
        backToEntity.ShouldNotBeNull();
        backToEntity.RuleId.ShouldBe(originalRule.RuleId);
        backToEntity.RuleJson.ShouldBe(originalRule.RuleJson);
        backToEntity.Name.ShouldBe(originalRule.Name);
        backToEntity.Description.ShouldBe(originalRule.Description);
        backToEntity.Version.ShouldBe(originalRule.Version);
        backToEntity.IsActive.ShouldBe(originalRule.IsActive);
    }
    /// <summary>
    /// Executes RuleDto_WithQualityControlRules_ShouldSupportManufacturing operation.
    /// </summary>

    [Fact]
    public void RuleDto_WithQualityControlRules_ShouldSupportManufacturing()
    {
        // Arrange - Quality control rules for manufacturing
        var qualityRules = new List<RuleDto>
        {
            new()
            {
                RuleId = 1001,
                Name = "DIMENSIONAL_TOLERANCE",
                Description = "Validates part dimensions within ±0.1mm tolerance",
                RuleJson = "{\"condition\":\"abs(measured - target) <= 0.1\",\"action\":\"pass\"}"
            },
            new()
            {
                RuleId = 2001,
                Name = "SURFACE_FINISH",
                Description = "Checks surface roughness Ra ≤ 1.6μm",
                RuleJson = "{\"condition\":\"surface_roughness <= 1.6\",\"action\":\"approve\"}"
            },
            new()
            {
                RuleId = 3001,
                Name = "HARDNESS_TEST",
                Description = "Verifies material hardness 58-62 HRC",
                RuleJson = "{\"condition\":\"hardness >= 58 && hardness <= 62\",\"action\":\"accept\"}"
            }
        };

        // Act & Assert
        qualityRules.All(r => r.Name.Contains("_")).ShouldBeTrue();
        qualityRules.All(r => r.RuleJson.Contains("condition")).ShouldBeTrue();
        qualityRules.Count.ShouldBe(3);

        var dimensionalRule = qualityRules.First(r => r.Name.Contains("DIMENSIONAL"));
        dimensionalRule.Description.ShouldContain("±0.1mm");
        dimensionalRule.RuleJson.ShouldContain("0.1");
    }
    /// <summary>
    /// Executes RuleDto_WithProcessMonitoring_ShouldSupportRealTimeControl operation.
    /// </summary>

    [Fact]
    public void RuleDto_WithProcessMonitoring_ShouldSupportRealTimeControl()
    {
        // Arrange - Process monitoring rules
        var processRule = new RuleDto
        {
            RuleId = 5001,
            Name = "REAL_TIME_TEMPERATURE_MONITOR",
            Description = "Continuously monitors CNC spindle temperature during machining",
            RuleJson = @"{
                ""condition"": ""spindle_temp > 85 || coolant_temp > 40"",
                ""action"": ""reduce_speed_or_stop"",
                ""priority"": ""high"",
                ""response_time"": ""immediate""
            }",
            Version = 2,
            IsActive = true
        };

        // Act & Assert
        processRule.IsActive.ShouldBeTrue();
        processRule.Name.ShouldContain("REAL_TIME");
        processRule.RuleJson.ShouldContain("spindle_temp");
        processRule.RuleJson.ShouldContain("coolant_temp");
        processRule.RuleJson.ShouldContain("priority");
        processRule.Description.ShouldContain("CNC");
        processRule.Version.ShouldBe(2);
    }
    /// <summary>
    /// Executes RuleDto_WithPredictiveMaintenance_ShouldOptimizeUptime operation.
    /// </summary>

    [Fact]
    public void RuleDto_WithPredictiveMaintenance_ShouldOptimizeUptime()
    {
        // Arrange - Predictive maintenance rule
        var maintenanceRule = new RuleDto
        {
            RuleId = 6001,
            Name = "BEARING_CONDITION_PREDICTOR",
            Description = "Predicts bearing failure based on vibration patterns and temperature trends",
            RuleJson = @"{
                ""condition"": ""(vibration_rms > 0.3 && temp_trend > 2.5) || operating_hours > 8000"",
                ""action"": ""schedule_maintenance"",
                ""lead_time"": ""72_hours"",
                ""confidence"": ""85_percent""
            }",
            Version = 3,
            IsActive = true
        };

        // Act & Assert
        maintenanceRule.Name.ShouldContain("PREDICTOR");
        maintenanceRule.Description.ShouldContain("bearing failure");
        maintenanceRule.RuleJson.ShouldContain("vibration_rms");
        maintenanceRule.RuleJson.ShouldContain("temp_trend");
        maintenanceRule.RuleJson.ShouldContain("operating_hours");
        maintenanceRule.RuleJson.ShouldContain("schedule_maintenance");
        maintenanceRule.Version.ShouldBe(3);
        maintenanceRule.IsActive.ShouldBeTrue();
    }
    /// <summary>
    /// Executes RuleDto_WithEnergyOptimization_ShouldReduceConsumption operation.
    /// </summary>

    [Fact]
    public void RuleDto_WithEnergyOptimization_ShouldReduceConsumption()
    {
        // Arrange - Energy optimization rule
        var energyRule = new RuleDto
        {
            RuleId = 7001,
            Name = "ADAPTIVE_ENERGY_MANAGEMENT",
            Description = "Optimizes machine power consumption during low-demand periods",
            RuleJson = @"{
                ""condition"": ""production_demand < 70_percent && power_cost > peak_rate"",
                ""action"": ""reduce_non_critical_systems"",
                ""savings_target"": ""15_percent"",
                ""affected_systems"": [""lighting"", ""hvac"", ""auxiliary_pumps""]
            }",
            Version = 1,
            IsActive = true
        };

        // Act & Assert
        energyRule.Name.ShouldContain("ENERGY");
        energyRule.Description.ShouldContain("power consumption");
        energyRule.RuleJson.ShouldContain("production_demand");
        energyRule.RuleJson.ShouldContain("power_cost");
        energyRule.RuleJson.ShouldContain("savings_target");
        energyRule.RuleJson.ShouldContain("affected_systems");
        energyRule.IsActive.ShouldBeTrue();
    }
}