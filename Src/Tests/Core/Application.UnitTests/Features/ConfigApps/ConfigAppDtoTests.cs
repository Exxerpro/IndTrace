// <copyright file="ConfigAppDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigApps;

/// <summary>
/// Unit tests for ConfigAppDto data transfer object.
/// Tests the industrial application configuration management system.
/// </summary>
public class ConfigAppDtoTests
{
    /// <summary>
    /// Executes ConfigAppDto_Constructor_ShouldInitializeWithDefaults operation.
    /// </summary>
    [Fact]
    public void ConfigAppDto_Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var configAppDto = new ConfigAppDto();

        // Assert
        configAppDto.ConfigAppId.ShouldBe(string.Empty);
        configAppDto.MachineId.ShouldBe(0);
        configAppDto.PlcId.ShouldBe(0);
        configAppDto.Pc.ShouldBe(string.Empty);
        configAppDto.AppId.ShouldBe(0);
        configAppDto.Client.ShouldBe(string.Empty);
        configAppDto.Factory.ShouldBe(string.Empty);
        configAppDto.Line.ShouldBe(string.Empty);
        configAppDto.Project.ShouldBe(string.Empty);
        configAppDto.Version.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes ConfigAppDto_Properties_ShouldAllowGetAndSet operation.
    /// </summary>

    [Fact]
    public void ConfigAppDto_Properties_ShouldAllowGetAndSet()
    {
        // Arrange
        var configAppDto = new ConfigAppDto();

        // Act & Assert
        configAppDto.ConfigAppId = "CONFIG_APP_001";
        configAppDto.ConfigAppId.ShouldBe("CONFIG_APP_001");

        configAppDto.MachineId = 5001;
        configAppDto.MachineId.ShouldBe(5001);

        configAppDto.PlcId = 2001;
        configAppDto.PlcId.ShouldBe(2001);

        configAppDto.Pc = "1001";
        configAppDto.Pc.ShouldBe("1001");

        configAppDto.AppId = 3001;
        configAppDto.AppId.ShouldBe(3001);

        configAppDto.Client = "AUTOMOTIVE_CLIENT";
        configAppDto.Client.ShouldBe("AUTOMOTIVE_CLIENT");

        configAppDto.Factory = "PLANT_VALENCIA";
        configAppDto.Factory.ShouldBe("PLANT_VALENCIA");

        configAppDto.Line = "ASSEMBLY_LINE_01";
        configAppDto.Line.ShouldBe("ASSEMBLY_LINE_01");

        configAppDto.Project = "ENGINE_PRODUCTION";
        configAppDto.Project.ShouldBe("ENGINE_PRODUCTION");

        configAppDto.Version = "V2.1.0";
        configAppDto.Version.ShouldBe("V2.1.0");
    }

    /// <summary>
    /// Executes ConfigAppDto_WithIndustrialScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData("CONFIG_AUTOMOTIVE_01", 1001, 2001, "Valencia Plant")]
    [InlineData("CONFIG_ELECTRONICS_02", 1002, 2002, "Barcelona Factory")]
    [InlineData("CONFIG_PHARMA_03", 1003, 2003, "Madrid Production")]
    [InlineData("CONFIG_FOOD_04", 1004, 2004, "Seville Manufacturing")]
    public void ConfigAppDto_WithIndustrialScenarios_ShouldHandleCorrectly(
        string configAppId, int machineId, int plcId, string factory)
    {
        // Arrange
        var configAppDto = new ConfigAppDto();

        // Act
        configAppDto.ConfigAppId = configAppId;
        configAppDto.MachineId = machineId;
        configAppDto.PlcId = plcId;
        configAppDto.Factory = factory;

        // Assert
        configAppDto.ConfigAppId.ShouldBe(configAppId);
        configAppDto.MachineId.ShouldBe(machineId);
        configAppDto.PlcId.ShouldBe(plcId);
        configAppDto.Factory.ShouldBe(factory);
    }

    /// <summary>
    /// Executes ConfigAppDto_ToDto_WithValidEntity_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigAppDto_ToDto_WithValidEntity_ShouldConvertCorrectly()
    {
        // Arrange
        var configApp = new ConfigApp
        {
            ConfigAppId = "INDUSTRIAL_CONFIG_001",
            MachineId = 7001,
            PlcId = 3001,
            Pc = "1001",
            AppId = 5001,
            Client = "AUTOMOTIVE_TIER1",
            Factory = "SMART_FACTORY_VALENCIA",
            Line = "HYBRID_ASSEMBLY_LINE",
            Project = "ELECTRIC_VEHICLE_PROJECT",
            Version = "V3.2.1"
        };

        // Act
        var resultWrapper = ConfigAppDto.ToDto(configApp);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ConfigAppId.ShouldBe("INDUSTRIAL_CONFIG_001");
        result.MachineId.ShouldBe(7001);
        result.PlcId.ShouldBe(3001);
        result.Pc.ShouldBe("1001");
        result.AppId.ShouldBe(5001);
        result.Client.ShouldBe("AUTOMOTIVE_TIER1");
        result.Factory.ShouldBe("SMART_FACTORY_VALENCIA");
        result.Line.ShouldBe("HYBRID_ASSEMBLY_LINE");
        result.Project.ShouldBe("ELECTRIC_VEHICLE_PROJECT");
        result.Version.ShouldBe("V3.2.1");
    }

    /// <summary>
    /// Executes ConfigAppDto_ToEntity_WithValidDto_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigAppDto_ToEntity_WithValidDto_ShouldConvertCorrectly()
    {
        // Arrange
        var configAppDto = new ConfigAppDto
        {
            ConfigAppId = "PHARMA_CONFIG_002",
            MachineId = 8002,
            PlcId = 4002,
            Pc = "2002",
            AppId = 6002,
            Client = "PHARMACEUTICAL_CORP",
            Factory = "GMP_FACILITY_MADRID",
            Line = "STERILE_PACKAGING_LINE",
            Project = "VACCINE_PRODUCTION",
            Version = "V1.5.3"
        };

        // Act
        var resultWrapper = ConfigAppDto.ToEntity(configAppDto);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ConfigAppId.ShouldBe("PHARMA_CONFIG_002");
        result.MachineId.ShouldBe(8002);
        result.PlcId.ShouldBe(4002);
        result.Pc.ShouldBe("2002");
        result.AppId.ShouldBe(6002);
        result.Client.ShouldBe("PHARMACEUTICAL_CORP");
        result.Factory.ShouldBe("GMP_FACILITY_MADRID");
        result.Line.ShouldBe("STERILE_PACKAGING_LINE");
        result.Project.ShouldBe("VACCINE_PRODUCTION");
        result.Version.ShouldBe("V1.5.3");
    }

    /// <summary>
    /// Executes ConfigAppDto_RoundTripConversion_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void ConfigAppDto_RoundTripConversion_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var originalConfigApp = new ConfigApp
        {
            ConfigAppId = "FOOD_BEVERAGE_CONFIG",
            MachineId = 9003,
            PlcId = 5003,
            Pc = "3003",
            AppId = 7003,
            Client = "FOOD_PROCESSING_LTD",
            Factory = "HACCP_CERTIFIED_PLANT",
            Line = "BOTTLING_LINE_A",
            Project = "CARBONATED_DRINKS",
            Version = "V2.0.0"
        };

        // Act
        var dtoWrapper = ConfigAppDto.ToDto(originalConfigApp);
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;

        var convertedEntityWrapper = ConfigAppDto.ToEntity(dto);

        // Assert
        convertedEntityWrapper.IsSuccess.ShouldBeTrue();
        convertedEntityWrapper.Value.ShouldNotBeNull();
        var convertedEntity = convertedEntityWrapper.Value;
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ConfigAppId.ShouldBe(originalConfigApp.ConfigAppId);
        convertedEntity.MachineId.ShouldBe(originalConfigApp.MachineId);
        convertedEntity.PlcId.ShouldBe(originalConfigApp.PlcId);
        convertedEntity.Pc.ShouldBe(originalConfigApp.Pc);
        convertedEntity.AppId.ShouldBe(originalConfigApp.AppId);
        convertedEntity.Client.ShouldBe(originalConfigApp.Client);
        convertedEntity.Factory.ShouldBe(originalConfigApp.Factory);
        convertedEntity.Line.ShouldBe(originalConfigApp.Line);
        convertedEntity.Project.ShouldBe(originalConfigApp.Project);
        convertedEntity.Version.ShouldBe(originalConfigApp.Version);
    }

    /// <summary>
    /// Executes ConfigAppDto_ToDto_WithNullEntity_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ConfigAppDto_ToDto_WithNullEntity_ShouldReturnFailureResult()
    {
        // Arrange
        ConfigApp? nullConfigApp = null!;

        // Act
        var result = ConfigAppDto.ToDto(nullConfigApp!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("ConfigApp source cannot be null");
    }

    /// <summary>
    /// Executes ConfigAppDto_ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ConfigAppDto_ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Arrange
        ConfigAppDto? nullDto = null!;

        // Act
        var result = ConfigAppDto.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("ConfigAppDto source cannot be null");
    }

    /// <summary>
    /// Executes ConfigAppDto_WithEdgeCaseValues_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData("", 0, 0, "")]
    [InlineData("SINGLE_CHAR", 1, 1, "S")]
    [InlineData("VERY_LONG_CONFIG_APP_IDENTIFIER_WITH_MANY_CHARACTERS", 999999, 888888, "EXTREMELY_LONG_FACTORY_NAME")]
    public void ConfigAppDto_WithEdgeCaseValues_ShouldHandleCorrectly(
        string configAppId, int machineId, int plcId, string factory)
    {
        // Arrange
        var configAppDto = new ConfigAppDto();

        // Act
        configAppDto.ConfigAppId = configAppId;
        configAppDto.MachineId = machineId;
        configAppDto.PlcId = plcId;
        configAppDto.Factory = factory;

        // Assert
        configAppDto.ConfigAppId.ShouldBe(configAppId);
        configAppDto.MachineId.ShouldBe(machineId);
        configAppDto.PlcId.ShouldBe(plcId);
        configAppDto.Factory.ShouldBe(factory);
    }

    /// <summary>
    /// Executes ConfigAppDto_WithComplexIndustrialScenario_ShouldHandleIndustry40Requirements operation.
    /// </summary>

    [Fact]
    public void ConfigAppDto_WithComplexIndustrialScenario_ShouldHandleIndustry40Requirements()
    {
        // Arrange - Industry 4.0 Smart Factory Configuration
        var configAppDto = new ConfigAppDto
        {
            ConfigAppId = "INDUSTRY40_SMART_FACTORY",
            MachineId = 1000001, // IoT-enabled CNC machine
            PlcId = 5001,      // Siemens S7-1500 PLC
            Pc = "2001",       // Edge computing device identifier
            AppId = 8001,      // Manufacturing execution system
            Client = "AEROSPACE_TIER1_SUPPLIER",
            Factory = "DIGITAL_TWIN_ENABLED_PLANT",
            Line = "FLEXIBLE_MANUFACTURING_CELL",
            Project = "AIRCRAFT_COMPONENT_PRECISION",
            Version = "V4.0.0_INDUSTRY40"
        };

        // Act & Assert - Validate Industry 4.0 compliance
        configAppDto.MachineId.ShouldBeGreaterThan(10000); // High-tech machine ID range
        configAppDto.Client.ShouldContain("AEROSPACE");     // Critical industry
        configAppDto.Factory.ShouldContain("DIGITAL_TWIN"); // Advanced factory type
        configAppDto.Version.ShouldContain("INDUSTRY40");   // Modern version tag
        configAppDto.Line.ShouldContain("FLEXIBLE");        // Adaptive manufacturing
    }
}
