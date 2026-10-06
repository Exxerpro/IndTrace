// <copyright file="AuditableEntityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.AuditableEntitiesTests;

/// <summary>
/// Unit tests for AuditableEntity - Domain model for audit tracking in manufacturing systems.
/// Tests property validation, date handling, and audit field behavior.
/// </summary>
public class AuditableEntityTests
{
    /// <summary>
    /// Executes AuditableEntity_WhenCreated_ShouldInitializeWithDefaultAuditFields operation.
    /// </summary>
    [Fact]
    public void AuditableEntity_WhenCreated_ShouldInitializeWithDefaultAuditFields()
    {
        // Arrange & Act
        var instance = new AuditableEntity();

        // Assert
        // Audit timestamps are stamped by the persistence layer on save (#29 F6); a not-yet-persisted
        // entity therefore carries the string.Empty user defaults and no timestamps.
        instance.ShouldNotBeNull();
        instance.CreatedBy.ShouldBe(string.Empty);
        instance.ModifiedBy.ShouldBe(string.Empty);
        instance.CreatedOn.ShouldBeNull();
        instance.ModifiedOn.ShouldBeNull();
    }
    /// <summary>
    /// Executes AuditableEntity_Constructor_WhenCreated_ShouldInitializeDefaultValues operation.
    /// </summary>

    [Fact]
    public void AuditableEntity_Constructor_WhenCreated_ShouldInitializeDefaultValues()
    {
        // Arrange & Act
        var instance = new AuditableEntity();

        // Assert
        // The entity no longer self-stamps in its constructor; timestamps stay null until persistence stamps them.
        instance.CreatedBy.ShouldBe(string.Empty);
        instance.ModifiedBy.ShouldBe(string.Empty);
        instance.CreatedOn.ShouldBeNull();
        instance.ModifiedOn.ShouldBeNull();
    }
    /// <summary>
    /// Executes CreatedBy_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="createdBy">The createdBy.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("TestUser")]
    [InlineData("AutomationSystem")]
    [InlineData("ManufacturingSystem")]
    [InlineData("QualityControl")]
    [InlineData("ProductionManager")]
    public void CreatedBy_WhenSetToValidValues_ShouldReturnCorrectValue(string createdBy)
    {
        // Arrange
        var instance = new AuditableEntity();

        // Act
        instance.CreatedBy = createdBy;

        // Assert
        instance.CreatedBy.ShouldBe(createdBy);
    }
    /// <summary>
    /// Executes ModifiedBy_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="modifiedBy">The modifiedBy.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("ModifiedUser")]
    [InlineData("SystemUpdate")]
    [InlineData("QualityInspector")]
    [InlineData("MaintenanceTeam")]
    [InlineData("ProductionSupervisor")]
    public void ModifiedBy_WhenSetToValidValues_ShouldReturnCorrectValue(string modifiedBy)
    {
        // Arrange
        var instance = new AuditableEntity();

        // Act
        instance.ModifiedBy = modifiedBy;

        // Assert
        instance.ModifiedBy.ShouldBe(modifiedBy);
    }
    /// <summary>
    /// Executes CreatedOn_WhenSetToValidDate_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void CreatedOn_WhenSetToValidDate_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new AuditableEntity();
        var testDate = new DateTime(2025, 6, 15, 10, 30, 45);

        // Act
        instance.CreatedOn = testDate;

        // Assert
        instance.CreatedOn.ShouldBe(testDate);
    }
    /// <summary>
    /// Executes ModifiedOn_WhenSetToValidDate_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void ModifiedOn_WhenSetToValidDate_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new AuditableEntity();
        var testDate = new DateTime(2025, 6, 15, 14, 20, 30);

        // Act
        instance.ModifiedOn = testDate;

        // Assert
        instance.ModifiedOn.ShouldBe(testDate);
    }
    /// <summary>
    /// Executes CreatedOn_WhenSetToDateBefore2000_ShouldSetToNull operation.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="day">The day.</param>

    [Theory]
    [InlineData(1999, 12, 31)]
    [InlineData(1950, 1, 1)]
    [InlineData(1800, 5, 15)]
    public void CreatedOn_WhenSetToDateBefore2000_ShouldSetToNull(int year, int month, int day)
    {
        // Arrange
        var instance = new AuditableEntity();
        var invalidDate = new DateTime(year, month, day);

        // Act
        instance.CreatedOn = invalidDate;

        // Assert
        instance.CreatedOn.ShouldBeNull();
    }
    /// <summary>
    /// Executes ModifiedOn_WhenSetToDateBefore2000_ShouldSetToNull operation.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="day">The day.</param>

    [Theory]
    [InlineData(1999, 12, 31)]
    [InlineData(1990, 6, 15)]
    [InlineData(1850, 3, 10)]
    public void ModifiedOn_WhenSetToDateBefore2000_ShouldSetToNull(int year, int month, int day)
    {
        // Arrange
        var instance = new AuditableEntity();
        var invalidDate = new DateTime(year, month, day);

        // Act
        instance.ModifiedOn = invalidDate;

        // Assert
        instance.ModifiedOn.ShouldBeNull();
    }
    /// <summary>
    /// Executes CreatedOn_WhenSetToNull_ShouldReturnNull operation.
    /// </summary>

    [Fact]
    public void CreatedOn_WhenSetToNull_ShouldReturnNull()
    {
        // Arrange
        var instance = new AuditableEntity();

        // Act
        instance.CreatedOn = null;

        // Assert
        instance.CreatedOn.ShouldBeNull();
    }
    /// <summary>
    /// Executes ModifiedOn_WhenSetToNull_ShouldReturnNull operation.
    /// </summary>

    [Fact]
    public void ModifiedOn_WhenSetToNull_ShouldReturnNull()
    {
        // Arrange
        var instance = new AuditableEntity();

        // Act
        instance.ModifiedOn = null;

        // Assert
        instance.ModifiedOn.ShouldBeNull();
    }
    /// <summary>
    /// Executes AuditableEntity_Properties_WithManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="day">The day.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(2025, 6, 15, "Manufacturing Cycle Audit")]
    [InlineData(2024, 12, 25, "Quality Control Audit")]
    [InlineData(2023, 3, 10, "Production Line Audit")]
    [InlineData(2026, 9, 5, "Equipment Maintenance Audit")]
    public void AuditableEntity_Properties_WithManufacturingScenarios_ShouldHandleCorrectly(int year, int month, int day, string scenario)
    {
        // Arrange
        var instance = new AuditableEntity();
        var testDate = new DateTime(year, month, day);
        var userId = $"User_{scenario.Replace(" ", "")}";

        // Act
        instance.CreatedBy = userId;
        instance.ModifiedBy = userId;
        instance.CreatedOn = testDate;
        instance.ModifiedOn = testDate;

        // Assert
        instance.CreatedBy.ShouldBe(userId);
        instance.ModifiedBy.ShouldBe(userId);
        instance.CreatedOn.ShouldBe(testDate);
        instance.ModifiedOn.ShouldBe(testDate);
    }
    /// <summary>
    /// Executes AuditFields_WhenUsedInManufacturingWorkflow_ShouldTrackChangesCorrectly operation.
    /// </summary>

    [Fact]
    public void AuditFields_WhenUsedInManufacturingWorkflow_ShouldTrackChangesCorrectly()
    {
        // Arrange
        var instance = new AuditableEntity();
        var initialCreationDate = new DateTime(2025, 6, 15, 8, 0, 0);
        var modificationDate = new DateTime(2025, 6, 15, 14, 30, 0);

        // Act - Simulate manufacturing process audit trail
        instance.CreatedBy = "ProductionSystem";
        instance.CreatedOn = initialCreationDate;

        // Simulate modification during quality control
        instance.ModifiedBy = "QualityControlSystem";
        instance.ModifiedOn = modificationDate;

        // Assert
        instance.CreatedBy.ShouldBe("ProductionSystem");
        instance.ModifiedBy.ShouldBe("QualityControlSystem");
        instance.CreatedOn.ShouldBe(initialCreationDate);
        instance.ModifiedOn.ShouldBe(modificationDate);
        modificationDate.ShouldBeGreaterThan(initialCreationDate);
    }
    /// <summary>
    /// Executes CreatedBy_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("")]
    [InlineData("SingleChar")]
    [InlineData("VeryLongUserNameForManufacturingSystemWithExtendedIdentification")]
    public void CreatedBy_WithEdgeCaseValues_ShouldStoreCorrectly(string value)
    {
        // Arrange
        var instance = new AuditableEntity();

        // Act
        instance.CreatedBy = value;

        // Assert
        instance.CreatedBy.ShouldBe(value);
    }
    /// <summary>
    /// Executes ModifiedBy_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("")]
    [InlineData("X")]
    [InlineData("AnotherVeryLongUserNameForQualityControlAndInspectionProcess")]
    public void ModifiedBy_WithEdgeCaseValues_ShouldStoreCorrectly(string value)
    {
        // Arrange
        var instance = new AuditableEntity();

        // Act
        instance.ModifiedBy = value;

        // Assert
        instance.ModifiedBy.ShouldBe(value);
    }
}
