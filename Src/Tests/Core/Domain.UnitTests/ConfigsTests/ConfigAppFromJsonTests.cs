// <copyright file="ConfigAppFromJsonTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ConfigsTests;

/// <summary>
/// Unit tests for ConfigAppFromJson - Configuration settings loaded from JSON file for manufacturing systems.
/// Tests property validation, JSON serialization, feature flags, and service configuration scenarios.
/// </summary>
public class ConfigAppFromJsonTests
{
    /// <summary>
    /// Executes ConfigAppFromJson_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void ConfigAppFromJson_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new ConfigAppFromJson();

        // Assert
        instance.ShouldNotBeNull();
        instance.Name.ShouldBe(string.Empty);
        instance.Service.ShouldBe(string.Empty);
        instance.GateWayWorker.ShouldBeFalse();
        instance.Reports.ShouldBeFalse();
        instance.Gp12.ShouldBeFalse();
        instance.LogsPath.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes Name_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("ProductionConfig")]
    [InlineData("TestConfig")]
    [InlineData("DevConfig")]
    [InlineData("")]
    public void Name_WhenSetToValidValues_ShouldReturnCorrectValue(string name)
    {
        // Arrange
        var config = new ConfigAppFromJson();

        // Act
        config.Name = name;

        // Assert
        config.Name.ShouldBe(name);
    }
    /// <summary>
    /// Executes GateWayWorker_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="enabled">The enabled.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GateWayWorker_WhenSetToValidValues_ShouldReturnCorrectValue(bool enabled)
    {
        // Arrange
        var config = new ConfigAppFromJson();

        // Act
        config.GateWayWorker = enabled;

        // Assert
        config.GateWayWorker.ShouldBe(enabled);
    }
    /// <summary>
    /// Executes Reports_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="enabled">The enabled.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Reports_WhenSetToValidValues_ShouldReturnCorrectValue(bool enabled)
    {
        // Arrange
        var config = new ConfigAppFromJson();

        // Act
        config.Reports = enabled;

        // Assert
        config.Reports.ShouldBe(enabled);
    }
    /// <summary>
    /// Executes Gp12_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="enabled">The enabled.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Gp12_WhenSetToValidValues_ShouldReturnCorrectValue(bool enabled)
    {
        // Arrange
        var config = new ConfigAppFromJson();

        // Act
        config.Gp12 = enabled;

        // Assert
        config.Gp12.ShouldBe(enabled);
    }
    /// <summary>
    /// Executes ConfigAppFromJson_Properties_WithManufacturingProductionScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigAppFromJson_Properties_WithManufacturingProductionScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var config = new ConfigAppFromJson();

        // Act - Configure for production manufacturing
        config.Name = "ProductionLine_A";
        config.Service = "ManufacturingExecutionSystem";
        config.GateWayWorker = true;
        config.Reports = true;
        config.Gp12 = true;
        config.LogsPath = @"C:\IndTrace\Logs\Production";

        // Assert
        config.Name.ShouldBe("ProductionLine_A");
        config.Service.ShouldBe("ManufacturingExecutionSystem");
        config.GateWayWorker.ShouldBeTrue();
        config.Reports.ShouldBeTrue();
        config.Gp12.ShouldBeTrue();
        config.LogsPath.ShouldBe(@"C:\IndTrace\Logs\Production");
    }
    /// <summary>
    /// Executes ConfigAppFromJson_Properties_WithTestEnvironmentScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigAppFromJson_Properties_WithTestEnvironmentScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var config = new ConfigAppFromJson();

        // Act - Configure for test environment
        config.Name = "TestLine_B";
        config.Service = "TestingService";
        config.GateWayWorker = false;
        config.Reports = false;
        config.Gp12 = false;
        config.LogsPath = @"C:\IndTrace\Logs\Test";

        // Assert
        config.Name.ShouldBe("TestLine_B");
        config.Service.ShouldBe("TestingService");
        config.GateWayWorker.ShouldBeFalse();
        config.Reports.ShouldBeFalse();
        config.Gp12.ShouldBeFalse();
        config.LogsPath.ShouldBe(@"C:\IndTrace\Logs\Test");
    }
    /// <summary>
    /// Executes ConfigAppFromJson_Properties_WithQualityControlScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigAppFromJson_Properties_WithQualityControlScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var config = new ConfigAppFromJson();

        // Act - Configure for quality control
        config.Name = "QualityControl_Station";
        config.Service = "QualityManagementSystem";
        config.GateWayWorker = true;
        config.Reports = true;
        config.Gp12 = false;
        config.LogsPath = @"/var/log/quality";

        // Assert
        config.Name.ShouldBe("QualityControl_Station");
        config.Service.ShouldBe("QualityManagementSystem");
        config.GateWayWorker.ShouldBeTrue();
        config.Reports.ShouldBeTrue();
        config.Gp12.ShouldBeFalse();
        config.LogsPath.ShouldBe(@"/var/log/quality");
    }
}
