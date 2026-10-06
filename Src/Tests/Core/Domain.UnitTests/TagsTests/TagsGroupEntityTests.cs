// <copyright file="TagsGroupEntityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.TagsTests;

/// <summary>
/// Unit tests for TagsGroupEntity domain entity
/// </summary>
public class TagsGroupEntityTests
{
    /// <summary>
    /// Executes TagsGroupEntity_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void TagsGroupEntity_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        var id = 1;
        var name = "Test Tags Group";
        var displayName = "Test Display Name";

        // Act
        var tagsGroup = new TagsGroupEntity(id, name, displayName);

        // Assert
        tagsGroup.ShouldNotBeNull();
        tagsGroup.Id.ShouldBe(id);
        tagsGroup.Name.ShouldBe(name);
        tagsGroup.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes TagsGroupEntity_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void TagsGroupEntity_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, "Initial Name", "Initial Display");
        var id = 2;
        var name = "Updated Tags Group";
        var displayName = "Updated Display Name";

        // Act
        tagsGroup.Id = id;
        tagsGroup.Name = name;
        tagsGroup.DisplayName = displayName;

        // Assert
        tagsGroup.Id.ShouldBe(id);
        tagsGroup.Name.ShouldBe(name);
        tagsGroup.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes TagsGroupEntityProperties_WhenSetToNull_ShouldHandleNullValues operation.
    /// </summary>

    [Fact]
    public void TagsGroupEntityProperties_WhenSetToNull_ShouldHandleNullValues()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, "TEST", "TEST DISPLAY");

        // Act
        tagsGroup.Name = null!;
        tagsGroup.DisplayName = null!;

        // Assert
        tagsGroup.Name.ShouldBeNull();
        tagsGroup.DisplayName.ShouldBeNull();
    }
    /// <summary>
    /// Executes Id_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void Id_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, "Test", "Test");

        // Act
        tagsGroup.Id = 0;

        // Assert
        tagsGroup.Id.ShouldBe(0);
    }
    /// <summary>
    /// Executes Id_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void Id_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, "Test", "Test");

        // Act
        tagsGroup.Id = -1;

        // Assert
        tagsGroup.Id.ShouldBe(-1);
    }
    /// <summary>
    /// Executes Name_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void Name_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, "Test", "Test");

        // Act
        tagsGroup.Name = string.Empty;

        // Assert
        tagsGroup.Name.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes DisplayName_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void DisplayName_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, "Test", "Test");

        // Act
        tagsGroup.DisplayName = string.Empty;

        // Assert
        tagsGroup.DisplayName.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes TagsGroupEntity_WhenTagsGroupIsConfigured_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void TagsGroupEntity_WhenTagsGroupIsConfigured_ShouldBeValid()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, "Production Tags Group", "Production Display Name");

        // Act & Assert
        tagsGroup.ShouldNotBeNull();
        tagsGroup.Id.ShouldBe(1);
        tagsGroup.Name.ShouldBe("Production Tags Group");
        tagsGroup.DisplayName.ShouldBe("Production Display Name");
    }
    /// <summary>
    /// Executes TagsGroupEntity_WhenTagsGroupHasLargeId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void TagsGroupEntity_WhenTagsGroupHasLargeId_ShouldBeValid()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(999999, "Large ID Tags Group", "Large ID Display Name");

        // Act & Assert
        tagsGroup.ShouldNotBeNull();
        tagsGroup.Id.ShouldBe(999999);
        tagsGroup.Name.ShouldBe("Large ID Tags Group");
        tagsGroup.DisplayName.ShouldBe("Large ID Display Name");
    }
    /// <summary>
    /// Executes TagsGroupEntity_WhenTagsGroupHasNegativeId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void TagsGroupEntity_WhenTagsGroupHasNegativeId_ShouldBeValid()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(-1, "Negative ID Tags Group", "Negative ID Display Name");

        // Act & Assert
        tagsGroup.ShouldNotBeNull();
        tagsGroup.Id.ShouldBe(-1);
        tagsGroup.Name.ShouldBe("Negative ID Tags Group");
        tagsGroup.DisplayName.ShouldBe("Negative ID Display Name");
    }
    /// <summary>
    /// Executes TagsGroupEntity_WhenTagsGroupHasEmptyName_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void TagsGroupEntity_WhenTagsGroupHasEmptyName_ShouldBeValid()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, string.Empty, "Empty Name Display");

        // Act & Assert
        tagsGroup.ShouldNotBeNull();
        tagsGroup.Id.ShouldBe(1);
        tagsGroup.Name.ShouldBe(string.Empty);
        tagsGroup.DisplayName.ShouldBe("Empty Name Display");
    }
    /// <summary>
    /// Executes TagsGroupEntity_WhenTagsGroupHasEmptyDisplayName_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void TagsGroupEntity_WhenTagsGroupHasEmptyDisplayName_ShouldBeValid()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, "Empty Display Name", string.Empty);

        // Act & Assert
        tagsGroup.ShouldNotBeNull();
        tagsGroup.Id.ShouldBe(1);
        tagsGroup.Name.ShouldBe("Empty Display Name");
        tagsGroup.DisplayName.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes Deconstruct_WhenQueried_ShouldReturnExpectedData operation.
    /// </summary>

    [Fact]
    public void Deconstruct_WhenQueried_ShouldReturnExpectedData()
    {
        // Arrange
        var tagsGroup = new TagsGroupEntity(1, "Test Name", "Test Display");

        // Act
        var (id, name, displayName) = tagsGroup;

        // Assert
        id.ShouldBe(1);
        name.ShouldBe("Test Name");
        displayName.ShouldBe("Test Display");
    }
}