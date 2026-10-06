// <copyright file="ConfigAppTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ConfigsTests;

/// <summary>
/// Unit tests for ConfigApp - Domain entity for application configuration in manufacturing systems.
/// Tests property validation, audit trail functionality, and manufacturing scenarios.
/// </summary>
public class ConfigAppTests
{
    /// <summary>
    /// Executes ConfigApp_ConfigApp_WhenCreatedWithDefaultConstructor_ShouldInitializeAllPropertiesCorrectly operation.
    /// </summary>
    [Fact]
    public void ConfigApp_ConfigApp_WhenCreatedWithDefaultConstructor_ShouldInitializeAllPropertiesCorrectly()
    {
        // Arrange & Act
        var instance = new ConfigApp();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AuditableEntity>();
        instance.ShouldBeAssignableTo<IEntityRoot>();
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Updated expectations for null safety refactoring - string properties initialized to non-null defaults to reduce nulls
        instance.ConfigAppId.ShouldBe(string.Empty); // Property initialized to string.Empty to avoid nulls
        instance.MachineId.ShouldBe(0);
        instance.PlcId.ShouldBe(0);
        instance.Pc.ShouldBe(string.Empty);
        instance.AppId.ShouldBe(0);
        instance.Client.ShouldBe(string.Empty); // Property initialized to string.Empty to avoid nulls
        instance.Factory.ShouldBe(string.Empty); // Property initialized to string.Empty to avoid nulls
        instance.Line.ShouldBe(string.Empty); // Property initialized to string.Empty to avoid nulls
        instance.Project.ShouldBe(string.Empty); // Property initialized to string.Empty to avoid nulls
        instance.Version.ShouldBe(string.Empty); // Property initialized to string.Empty to avoid nulls
    }

    /// <summary>
    /// Executes ConfigApp_Constructor_WhenCreated_ShouldInheritAuditableEntityBehavior operation.
    /// </summary>

    [Fact]
    public void ConfigApp_Constructor_WhenCreated_ShouldInheritAuditableEntityBehavior()
    {
        // Arrange & Act
        var config = new ConfigApp();

        // Assert - AuditableEntity no longer self-stamps; the persistence layer stamps timestamps on save (#29 F6),
        // so a not-yet-persisted entity carries the string.Empty user defaults and null timestamps.
        config.CreatedBy.ShouldBe(string.Empty);
        config.ModifiedBy.ShouldBe(string.Empty);
        config.CreatedOn.ShouldBeNull();
        config.ModifiedOn.ShouldBeNull();
    }

    /// <summary>
    /// Executes ConfigApp_WhenAllPropertiesSetToSpecificValues_ShouldPersistAllChangesAccurately operation.
    /// </summary>

    [Fact]
    public void ConfigApp_WhenAllPropertiesSetToSpecificValues_ShouldPersistAllChangesAccurately()
    {
        // Arrange
        var instance = new ConfigApp();
        string configAppId = "CA-001";
        int machineId = 1;
        int plcId = 2;
        string pc = "PC-03";
        int appId = 4;
        string client = "ClientA";
        string factory = "FactoryA";
        string line = "LineA";
        string project = "ProjectA";
        string version = "1.0.0";

        // Act
        instance.ConfigAppId = configAppId;
        instance.MachineId = machineId;
        instance.PlcId = plcId;
        instance.Pc = pc;
        instance.AppId = appId;
        instance.Client = client;
        instance.Factory = factory;
        instance.Line = line;
        instance.Project = project;
        instance.Version = version;

        // Assert
        instance.ConfigAppId.ShouldBe(configAppId);
        instance.MachineId.ShouldBe(machineId);
        instance.PlcId.ShouldBe(plcId);
        instance.Pc.ShouldBe(pc);
        instance.AppId.ShouldBe(appId);
        instance.Client.ShouldBe(client);
        instance.Factory.ShouldBe(factory);
        instance.Line.ShouldBe(line);
        instance.Project.ShouldBe(project);
        instance.Version.ShouldBe(version);
    }

    /// <summary>
    /// Executes ConfigApp_Properties_WithManufacturingScenarios_ShouldStoreCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1, 100, 200, "PC-01", "Ford Motor Company", "Dearborn Plant", "Assembly Line 1", "F-150 Production", "3.2.1")]
    [InlineData(2, 101, 201, "PC-02", "Tesla Inc", "Fremont Factory", "Model S Line", "Electric Vehicle Assembly", "2.1.0")]
    [InlineData(3, 102, 202, "PC-03", "BMW Group", "Munich Plant", "X5 Line", "SUV Manufacturing", "4.0.5")]
    public void ConfigApp_Properties_WithManufacturingScenarios_ShouldStoreCorrectly(int appId, int machineId, int plcId, string pc,
        string client, string factory, string line, string project, string version)
    {
        // Arrange
        var config = new ConfigApp();

        // Act
        config.AppId = appId;
        config.MachineId = machineId;
        config.PlcId = plcId;
        config.Pc = pc;
        config.Client = client;
        config.Factory = factory;
        config.Line = line;
        config.Project = project;
        config.Version = version;

        // Assert
        config.AppId.ShouldBe(appId);
        config.MachineId.ShouldBe(machineId);
        config.PlcId.ShouldBe(plcId);
        config.Pc.ShouldBe(pc);
        config.Client.ShouldBe(client);
        config.Factory.ShouldBe(factory);
        config.Line.ShouldBe(line);
        config.Project.ShouldBe(project);
        config.Version.ShouldBe(version);
    }

    /// <summary>
    /// Executes ConfigAppId_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="configId">The configId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("IndTrace L1A")]
    [InlineData("IndTrace L2B")]
    [InlineData("QC_Station_01")]
    [InlineData("")]
    public void ConfigAppId_WhenSetToValidValues_ShouldReturnCorrectValue(string configId)
    {
        // Arrange
        var config = new ConfigApp();

        // Act
        config.ConfigAppId = configId;

        // Assert
        config.ConfigAppId.ShouldBe(configId);
    }

    /// <summary>
    /// Executes MachineId_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(999)]
    [InlineData(0)]
    public void MachineId_WhenSetToValidValues_ShouldReturnCorrectValue(int machineId)
    {
        // Arrange
        var config = new ConfigApp();

        // Act
        config.MachineId = machineId;

        // Assert
        config.MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes PlcId_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(999)]
    [InlineData(0)]
    public void PlcId_WhenSetToValidValues_ShouldReturnCorrectValue(int plcId)
    {
        // Arrange
        var config = new ConfigApp();

        // Act
        config.PlcId = plcId;

        // Assert
        config.PlcId.ShouldBe(plcId);
    }

    /// <summary>
    /// Executes ConfigApp_AuditProperties_WithManufacturingWorkflow_ShouldTrackChangesCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigApp_AuditProperties_WithManufacturingWorkflow_ShouldTrackChangesCorrectly()
    {
        // Arrange
        var config = new ConfigApp();
        var creationDate = new DateTime(2025, 6, 15, 8, 0, 0);
        var modificationDate = new DateTime(2025, 6, 15, 14, 30, 0);

        // Act - Simulate manufacturing configuration creation and update
        config.CreatedBy = "SystemAdmin";
        config.CreatedOn = creationDate;
        config.ModifiedBy = "ProductionManager";
        config.ModifiedOn = modificationDate;

        // Set configuration properties
        config.ConfigAppId = "IndTrace_Prod_L1";
        config.Client = "Ford Motor Company";
        config.Factory = "River Rouge Plant";
        config.Line = "F-150 Final Assembly";
        config.Project = "Next Generation F-150";
        config.Version = "2025.1.0";

        // Assert
        config.CreatedBy.ShouldBe("SystemAdmin");
        config.ModifiedBy.ShouldBe("ProductionManager");
        config.CreatedOn.ShouldBe(creationDate);
        config.ModifiedOn.ShouldBe(modificationDate);
        config.ConfigAppId.ShouldBe("IndTrace_Prod_L1");
        config.Client.ShouldBe("Ford Motor Company");
        config.Factory.ShouldBe("River Rouge Plant");
        config.Line.ShouldBe("F-150 Final Assembly");
        config.Project.ShouldBe("Next Generation F-150");
        config.Version.ShouldBe("2025.1.0");
    }

    /// <summary>
    /// Executes Client_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Very Long Client Name That Might Exceed Database Field Limits In Some Manufacturing Systems")]
    public void Client_WithEdgeCaseValues_ShouldStoreCorrectly(string? value)
    {
        // Arrange
        var config = new ConfigApp();

        // Act
        config.Client = value!;

        // Assert
        config.Client.ShouldBe(value);
    }

    /// <summary>
    /// Executes Version_WithVariousFormats_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1.0.0")]
    [InlineData("v2025.06.15.1")]
    [InlineData("PROD-STABLE-20250615")]
    public void Version_WithVariousFormats_ShouldStoreCorrectly(string? value)
    {
        // Arrange
        var config = new ConfigApp();

        // Act
        config.Version = value!;

        // Assert
        config.Version.ShouldBe(value);
    }

    /// <summary>
    /// Executes EntityInterfaces_ShouldImplementCorrectly operation.
    /// </summary>

    [Fact]
    public void EntityInterfaces_ShouldImplementCorrectly()
    {
        // Arrange & Act
        var config = new ConfigApp();

        // Assert - Verify it implements required domain interfaces
        config.ShouldBeAssignableTo<IEntityRoot>();
        config.ShouldBeAssignableTo<AuditableEntity>();
    }
}