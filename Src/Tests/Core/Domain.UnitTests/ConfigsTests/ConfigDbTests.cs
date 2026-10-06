// <copyright file="ConfigDbTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ConfigsTests;

/// <summary>
/// Unit tests for ConfigDb - Database configuration information entity for system versioning.
/// Tests property validation, database version tracking, and configuration management scenarios.
/// </summary>
public class ConfigDbTests
{
    /// <summary>
    /// Executes ConfigDb_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void ConfigDb_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new ConfigDb();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IEntityRoot>();
        instance.SystemInformationId.ShouldBe(0);
        instance.DatabaseVersion.ShouldBe(string.Empty); // Refactored to use string.Empty (safer than null)
        instance.VersionDate.ShouldBe(default(DateTime));
        instance.ModifiedDate.ShouldBe(default(DateTime));
    }
    /// <summary>
    /// Executes ConfigDb_Constructor_WhenCreated_ShouldImplementIEntityRoot operation.
    /// </summary>

    [Fact]
    public void ConfigDb_Constructor_WhenCreated_ShouldImplementIEntityRoot()
    {
        // Arrange & Act
        var configDb = new ConfigDb();

        // Assert
        configDb.ShouldBeAssignableTo<IEntityRoot>();
    }
    /// <summary>
    /// Executes SystemInformationId_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="systemId">The systemId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    [InlineData(0)]
    public void SystemInformationId_WhenSetToValidValues_ShouldReturnCorrectValue(int systemId)
    {
        // Arrange
        var configDb = new ConfigDb();

        // Act
        configDb.SystemInformationId = systemId;

        // Assert
        configDb.SystemInformationId.ShouldBe(systemId);
    }
    /// <summary>
    /// Executes DatabaseVersion_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("2.1.3")]
    [InlineData("10.5.2-beta")]
    [InlineData("2025.06.15.1")]
    [InlineData("v3.2.1-manufacturing-stable")]
    public void DatabaseVersion_WhenSetToValidValues_ShouldReturnCorrectValue(string version)
    {
        // Arrange
        var configDb = new ConfigDb();

        // Act
        configDb.DatabaseVersion = version;

        // Assert
        configDb.DatabaseVersion.ShouldBe(version);
    }
    /// <summary>
    /// Executes VersionDate_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="dateTimeString">The dateTimeString.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("2025-06-15 08:00:00")]
    [InlineData("2025-06-15 16:30:00")]
    [InlineData("2025-12-31 23:59:59")]
    public void VersionDate_WhenSetToValidValues_ShouldReturnCorrectValue(string dateTimeString)
    {
        // Arrange
        var configDb = new ConfigDb();
        var versionDate = DateTime.Parse(dateTimeString);

        // Act
        configDb.VersionDate = versionDate;

        // Assert
        configDb.VersionDate.ShouldBe(versionDate);
    }
    /// <summary>
    /// Executes ModifiedDate_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="dateTimeString">The dateTimeString.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("2025-06-15 10:30:00")]
    [InlineData("2025-06-15 18:45:00")]
    [InlineData("2025-01-01 00:00:00")]
    public void ModifiedDate_WhenSetToValidValues_ShouldReturnCorrectValue(string dateTimeString)
    {
        // Arrange
        var configDb = new ConfigDb();
        var modifiedDate = DateTime.Parse(dateTimeString);

        // Act
        configDb.ModifiedDate = modifiedDate;

        // Assert
        configDb.ModifiedDate.ShouldBe(modifiedDate);
    }
    /// <summary>
    /// Executes ConfigDb_Properties_WithProductionDatabaseScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigDb_Properties_WithProductionDatabaseScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var configDb = new ConfigDb();
        var versionDate = new DateTime(2025, 6, 15, 8, 0, 0);
        var modifiedDate = new DateTime(2025, 6, 15, 10, 30, 0);

        // Act - Configure for production database
        configDb.SystemInformationId = 1;
        configDb.DatabaseVersion = "3.2.1-production";
        configDb.VersionDate = versionDate;
        configDb.ModifiedDate = modifiedDate;

        // Assert
        configDb.SystemInformationId.ShouldBe(1);
        configDb.DatabaseVersion.ShouldBe("3.2.1-production");
        configDb.VersionDate.ShouldBe(versionDate);
        configDb.ModifiedDate.ShouldBe(modifiedDate);
        configDb.ModifiedDate.ShouldBeGreaterThan(configDb.VersionDate);
    }
    /// <summary>
    /// Executes ConfigDb_Properties_WithTestDatabaseScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigDb_Properties_WithTestDatabaseScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var configDb = new ConfigDb();
        var versionDate = new DateTime(2025, 6, 15, 14, 0, 0);
        var modifiedDate = new DateTime(2025, 6, 15, 16, 15, 0);

        // Act - Configure for test database
        configDb.SystemInformationId = 2;
        configDb.DatabaseVersion = "3.3.0-beta";
        configDb.VersionDate = versionDate;
        configDb.ModifiedDate = modifiedDate;

        // Assert
        configDb.SystemInformationId.ShouldBe(2);
        configDb.DatabaseVersion.ShouldBe("3.3.0-beta");
        configDb.VersionDate.ShouldBe(versionDate);
        configDb.ModifiedDate.ShouldBe(modifiedDate);
        configDb.ModifiedDate.ShouldBeGreaterThan(configDb.VersionDate);
    }
    /// <summary>
    /// Executes ConfigDb_Properties_WithManufacturingDatabaseScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigDb_Properties_WithManufacturingDatabaseScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var configDb = new ConfigDb();
        var versionDate = new DateTime(2025, 1, 1, 0, 0, 0);
        var modifiedDate = new DateTime(2025, 6, 15, 12, 0, 0);

        // Act - Configure for manufacturing database
        configDb.SystemInformationId = 100;
        configDb.DatabaseVersion = "2025.01.01-manufacturing-stable";
        configDb.VersionDate = versionDate;
        configDb.ModifiedDate = modifiedDate;

        // Assert
        configDb.SystemInformationId.ShouldBe(100);
        configDb.DatabaseVersion.ShouldBe("2025.01.01-manufacturing-stable");
        configDb.VersionDate.ShouldBe(versionDate);
        configDb.ModifiedDate.ShouldBe(modifiedDate);
        configDb.ModifiedDate.ShouldBeGreaterThan(configDb.VersionDate);
    }
    /// <summary>
    /// Executes DatabaseVersion_WithPlantSpecificVersions_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("Ford-Plant-v1.0.0")]
    [InlineData("Tesla-Fremont-v2.1.3")]
    [InlineData("BMW-Munich-v3.0.0-hotfix")]
    public void DatabaseVersion_WithPlantSpecificVersions_ShouldStoreCorrectly(string version)
    {
        // Arrange
        var configDb = new ConfigDb();

        // Act
        configDb.DatabaseVersion = version;

        // Assert
        configDb.DatabaseVersion.ShouldBe(version);
    }
    /// <summary>
    /// Executes DatabaseVersion_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("VeryLongDatabaseVersionStringThatExceedsTypicalVersionNamingConventionsForManufacturingSystemsAndEnterpriseApplications")]
    public void DatabaseVersion_WithEdgeCaseValues_ShouldStoreCorrectly(string? value)
    {
        // Arrange
        var configDb = new ConfigDb();

        // Act
        configDb.DatabaseVersion = value!;

        // Assert
        configDb.DatabaseVersion.ShouldBe(value);
    }
    /// <summary>
    /// Executes ConfigDb_DateProperties_WithSequentialDates_ShouldMaintainTemporalOrder operation.
    /// </summary>

    [Fact]
    public void ConfigDb_DateProperties_WithSequentialDates_ShouldMaintainTemporalOrder()
    {
        // Arrange
        var configDb = new ConfigDb();
        var baseDate = new DateTime(2025, 6, 15, 8, 0, 0);
        var versionDate = baseDate;
        var modifiedDate = baseDate.AddHours(2);

        // Act
        configDb.VersionDate = versionDate;
        configDb.ModifiedDate = modifiedDate;

        // Assert
        configDb.VersionDate.ShouldBe(versionDate);
        configDb.ModifiedDate.ShouldBe(modifiedDate);
        configDb.ModifiedDate.ShouldBeGreaterThan(configDb.VersionDate);
    }
    /// <summary>
    /// Executes EntityInterface_ShouldImplementCorrectly operation.
    /// </summary>

    [Fact]
    public void EntityInterface_ShouldImplementCorrectly()
    {
        // Arrange & Act
        var configDb = new ConfigDb();

        // Assert - Verify it implements required domain interface
        configDb.ShouldBeAssignableTo<IEntityRoot>();
    }
    /// <summary>
    /// Executes SystemInformationId_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void SystemInformationId_WithEdgeCaseValues_ShouldStoreCorrectly(int value)
    {
        // Arrange
        var configDb = new ConfigDb();

        // Act
        configDb.SystemInformationId = value;

        // Assert
        configDb.SystemInformationId.ShouldBe(value);
    }
}
