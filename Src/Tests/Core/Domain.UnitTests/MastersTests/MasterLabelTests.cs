// <copyright file="MasterLabelTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.MastersTests;

/// <summary>
/// Unit tests for MasterLabel domain entity
/// </summary>
public class MasterLabelTests
{
    /// <summary>
    /// Executes MasterLabel_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void MasterLabel_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        var masterLabelId = 1;
        var masterLabelCode = "ML-001";
        var description = "Test Master Label";

        // Act
        var masterLabel = new MasterLabel
        {
            MasterLabelId = masterLabelId,
            MasterLabelCode = masterLabelCode,
            Description = description
        };

        // Assert
        masterLabel.ShouldNotBeNull();
        masterLabel.MasterLabelId.ShouldBe(masterLabelId);
        masterLabel.MasterLabelCode.ShouldBe(masterLabelCode);
        masterLabel.Description.ShouldBe(description);
    }
    /// <summary>
    /// Executes MasterLabel_WithDefaultConstructor_ShouldInitializeToExpectedDefaults operation.
    /// </summary>

    [Fact]
    public void MasterLabel_WithDefaultConstructor_ShouldInitializeToExpectedDefaults()
    {
        // Arrange & Act
        var masterLabel = new MasterLabel();

        // Assert
        masterLabel.ShouldNotBeNull();
        masterLabel.MasterLabelId.ShouldBe(0);
        masterLabel.MasterLabelCode.ShouldBe(string.Empty);
        masterLabel.Description.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes MasterLabel_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void MasterLabel_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var masterLabel = new MasterLabel();
        var masterLabelId = 2;
        var masterLabelCode = "ML-002";
        var description = "Updated Master Label";

        // Act
        masterLabel.MasterLabelId = masterLabelId;
        masterLabel.MasterLabelCode = masterLabelCode;
        masterLabel.Description = description;

        // Assert
        masterLabel.MasterLabelId.ShouldBe(masterLabelId);
        masterLabel.MasterLabelCode.ShouldBe(masterLabelCode);
        masterLabel.Description.ShouldBe(description);
    }
    /// <summary>
    /// Executes MasterLabelProperties_WhenSetToNull_ShouldHandleNullValues operation.
    /// </summary>

    [Fact]
    public void MasterLabelProperties_WhenSetToNull_ShouldHandleNullValues()
    {
        // Arrange
        var masterLabel = new MasterLabel
        {
            MasterLabelCode = "TEST",
            Description = "Test Description"
        };

        // Act
        masterLabel.MasterLabelCode = null!;
        masterLabel.Description = null!;

        // Assert
        masterLabel.MasterLabelCode.ShouldBeNull();
        masterLabel.Description.ShouldBeNull();
    }
    /// <summary>
    /// Executes MasterLabelId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void MasterLabelId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var masterLabel = new MasterLabel();

        // Act
        masterLabel.MasterLabelId = 0;

        // Assert
        masterLabel.MasterLabelId.ShouldBe(0);
    }
    /// <summary>
    /// Executes MasterLabelId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void MasterLabelId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var masterLabel = new MasterLabel();

        // Act
        masterLabel.MasterLabelId = -1;

        // Assert
        masterLabel.MasterLabelId.ShouldBe(-1);
    }
    /// <summary>
    /// Executes MasterLabelCode_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void MasterLabelCode_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var masterLabel = new MasterLabel();

        // Act
        masterLabel.MasterLabelCode = string.Empty;

        // Assert
        masterLabel.MasterLabelCode.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes Description_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void Description_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var masterLabel = new MasterLabel();

        // Act
        masterLabel.Description = string.Empty;

        // Assert
        masterLabel.Description.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes MasterLabel_WhenMasterLabelIsCreated_ShouldHaveDefaultValues operation.
    /// </summary>

    [Fact]
    public void MasterLabel_WhenMasterLabelIsCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var masterLabel = new MasterLabel();

        // Assert
        masterLabel.ShouldNotBeNull();
        masterLabel.MasterLabelId.ShouldBe(0);
        masterLabel.MasterLabelCode.ShouldBe(string.Empty);
        masterLabel.Description.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes MasterLabel_WhenMasterLabelIsConfigured_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void MasterLabel_WhenMasterLabelIsConfigured_ShouldBeValid()
    {
        // Arrange
        var masterLabel = new MasterLabel
        {
            MasterLabelId = 1,
            MasterLabelCode = "ML-PROD-001",
            Description = "Production Master Label"
        };

        // Act & Assert
        masterLabel.ShouldNotBeNull();
        masterLabel.MasterLabelId.ShouldBe(1);
        masterLabel.MasterLabelCode.ShouldBe("ML-PROD-001");
        masterLabel.Description.ShouldBe("Production Master Label");
    }
    /// <summary>
    /// Executes MasterLabel_WhenMasterLabelHasLargeId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void MasterLabel_WhenMasterLabelHasLargeId_ShouldBeValid()
    {
        // Arrange
        var masterLabel = new MasterLabel
        {
            MasterLabelId = 999999,
            MasterLabelCode = "ML-LARGE-001",
            Description = "Large ID Master Label"
        };

        // Act & Assert
        masterLabel.ShouldNotBeNull();
        masterLabel.MasterLabelId.ShouldBe(999999);
        masterLabel.MasterLabelCode.ShouldBe("ML-LARGE-001");
        masterLabel.Description.ShouldBe("Large ID Master Label");
    }
    /// <summary>
    /// Executes MasterLabel_WhenMasterLabelHasNegativeId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void MasterLabel_WhenMasterLabelHasNegativeId_ShouldBeValid()
    {
        // Arrange
        var masterLabel = new MasterLabel
        {
            MasterLabelId = -1,
            MasterLabelCode = "ML-NEG-001",
            Description = "Negative ID Master Label"
        };

        // Act & Assert
        masterLabel.ShouldNotBeNull();
        masterLabel.MasterLabelId.ShouldBe(-1);
        masterLabel.MasterLabelCode.ShouldBe("ML-NEG-001");
        masterLabel.Description.ShouldBe("Negative ID Master Label");
    }
    /// <summary>
    /// Executes MasterLabel_WhenMasterLabelHasEmptyCode_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void MasterLabel_WhenMasterLabelHasEmptyCode_ShouldBeValid()
    {
        // Arrange
        var masterLabel = new MasterLabel
        {
            MasterLabelId = 1,
            MasterLabelCode = string.Empty,
            Description = "Empty Code Master Label"
        };

        // Act & Assert
        masterLabel.ShouldNotBeNull();
        masterLabel.MasterLabelId.ShouldBe(1);
        masterLabel.MasterLabelCode.ShouldBe(string.Empty);
        masterLabel.Description.ShouldBe("Empty Code Master Label");
    }
    /// <summary>
    /// Executes MasterLabel_WhenMasterLabelHasEmptyDescription_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void MasterLabel_WhenMasterLabelHasEmptyDescription_ShouldBeValid()
    {
        // Arrange
        var masterLabel = new MasterLabel
        {
            MasterLabelId = 1,
            MasterLabelCode = "ML-EMPTY-DESC",
            Description = string.Empty
        };

        // Act & Assert
        masterLabel.ShouldNotBeNull();
        masterLabel.MasterLabelId.ShouldBe(1);
        masterLabel.MasterLabelCode.ShouldBe("ML-EMPTY-DESC");
        masterLabel.Description.ShouldBe(string.Empty);
    }
}