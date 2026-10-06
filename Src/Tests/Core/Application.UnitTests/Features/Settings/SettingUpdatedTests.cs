// <copyright file="SettingUpdatedTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Settings.Commands.Update;

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for SettingUpdated notification event.
/// Tests industrial equipment setting update notifications for manufacturing systems.
/// </summary>
public class SettingUpdatedTests
{
    /// <summary>
    /// Executes SettingUpdated_Constructor_ShouldCreateInstanceWithDefaults operation.
    /// </summary>
    [Fact]
    public void SettingUpdated_Constructor_ShouldCreateInstanceWithDefaults()
    {
        // Arrange & Act
        var notification = new SettingUpdated();

        // Assert
        notification.ShouldNotBeNull();
        notification.SettingId.ShouldBeNull();
        notification.MaquinaId.ShouldBeNull();
    }
    /// <summary>
    /// Executes SettingUpdated_Properties_ShouldAllowGetAndSet operation.
    /// </summary>

    [Fact]
    public void SettingUpdated_Properties_ShouldAllowGetAndSet()
    {
        // Arrange
        var notification = new SettingUpdated();
        const int settingId = 1001;
        const int machineId = 101;

        // Act
        notification.SettingId = settingId;
        notification.MaquinaId = machineId;

        // Assert
        notification.SettingId.ShouldBe(settingId);
        notification.MaquinaId.ShouldBe(machineId);
    }
    /// <summary>
    /// Executes SettingUpdated_WithManufacturingScenarios_ShouldSetPropertiesCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1, 101, "Ford F-150 Welding Station Update")]
    [InlineData(2, 201, "CNC Machining Center Parameter Change")]
    [InlineData(3, 301, "SMT Pick & Place Configuration Update")]
    [InlineData(4, 401, "AOI Inspection System Calibration")]
    [InlineData(5, 501, "Pharmaceutical Tablet Press Setting")]
    public void SettingUpdated_WithManufacturingScenarios_ShouldSetPropertiesCorrectly(
        int settingId, int machineId, string description)
    {

        var logger = XUnitLogger.CreateLogger<SettingUpdatedTests>();
        logger.LogInformation("Testing scenario: {description} with settingId={settingId}, machineId={machineId}",
            description, settingId, machineId);

        // Arrange & Act
        var notification = new SettingUpdated
        {
            SettingId = settingId,
            MaquinaId = machineId
        };

        // Assert
        notification.SettingId.ShouldBe(settingId);
        notification.MaquinaId.ShouldBe(machineId);
    }
    /// <summary>
    /// Executes SettingUpdated_WithAutomotiveWeldingStation_ShouldTrackSettingUpdate operation.
    /// </summary>

    [Fact]
    public void SettingUpdated_WithAutomotiveWeldingStation_ShouldTrackSettingUpdate()
    {
        // Arrange - Ford F-150 engine block welding robot setting update
        var notification = new SettingUpdated
        {
            SettingId = 1001,
            MaquinaId = 101  // Robotic Welding Cell #1
        };

        // Act & Assert
        notification.SettingId.ShouldBe(1001);
        notification.MaquinaId.ShouldBe(101);
    }
    /// <summary>
    /// Executes SettingUpdated_WithElectronicsManufacturing_ShouldTrackConfigurationChange operation.
    /// </summary>

    [Fact]
    public void SettingUpdated_WithElectronicsManufacturing_ShouldTrackConfigurationChange()
    {
        // Arrange - iPhone PCB SMT Pick & Place machine update
        var notification = new SettingUpdated
        {
            SettingId = 2001,
            MaquinaId = 301  // High-Speed SMT Line
        };

        // Act & Assert
        notification.SettingId.ShouldBe(2001);
        notification.MaquinaId.ShouldBe(301);
    }
    /// <summary>
    /// Executes SettingUpdated_WithPharmaceuticalEquipment_ShouldTrackQualityParameters operation.
    /// </summary>

    [Fact]
    public void SettingUpdated_WithPharmaceuticalEquipment_ShouldTrackQualityParameters()
    {
        // Arrange - FDA-regulated tablet press parameter update
        var notification = new SettingUpdated
        {
            SettingId = 3001,
            MaquinaId = 501  // Rotary Tablet Press
        };

        // Act & Assert
        notification.SettingId.ShouldBe(3001);
        notification.MaquinaId.ShouldBe(501);
    }
    /// <summary>
    /// Executes SettingUpdated_WithNullValues_ShouldAllowNullProperties operation.
    /// </summary>

    [Fact]
    public void SettingUpdated_WithNullValues_ShouldAllowNullProperties()
    {
        // Arrange & Act
        var notification = new SettingUpdated
        {
            SettingId = null!,
            MaquinaId = null
        };

        // Assert
        notification.SettingId.ShouldBeNull();
        notification.MaquinaId.ShouldBeNull();
    }
    /// <summary>
    /// Executes SettingUpdated_WithZeroValues_ShouldAcceptZeroIds operation.
    /// </summary>

    [Fact]
    public void SettingUpdated_WithZeroValues_ShouldAcceptZeroIds()
    {
        // Arrange & Act
        var notification = new SettingUpdated
        {
            SettingId = 0,
            MaquinaId = 0
        };

        // Assert
        notification.SettingId.ShouldBe(0);
        notification.MaquinaId.ShouldBe(0);
    }
    /// <summary>
    /// Executes SettingUpdated_WithLargeIndustrialIds_ShouldHandleEnterpriseScaleIds operation.
    /// </summary>

    [Fact]
    public void SettingUpdated_WithLargeIndustrialIds_ShouldHandleEnterpriseScaleIds()
    {
        // Arrange - Large enterprise manufacturing plant IDs
        var notification = new SettingUpdated
        {
            SettingId = 999999,
            MaquinaId = 888888
        };

        // Act & Assert
        notification.SettingId.ShouldBe(999999);
        notification.MaquinaId.ShouldBe(888888);
    }
    /// <summary>
    /// Executes SettingUpdated_PropertyAssignment_ShouldBeIndependent operation.
    /// </summary>

    [Fact]
    public void SettingUpdated_PropertyAssignment_ShouldBeIndependent()
    {
        // Arrange
        var notification1 = new SettingUpdated { SettingId = 1001, MaquinaId = 101 };
        var notification2 = new SettingUpdated { SettingId = 2002, MaquinaId = 202 };

        // Act & Assert
        notification1.SettingId.ShouldBe(1001);
        notification1.MaquinaId.ShouldBe(101);
        notification2.SettingId.ShouldBe(2002);
        notification2.MaquinaId.ShouldBe(202);
    }
    /// <summary>
    /// Executes SettingUpdated_MultipleInstancesCreation_ShouldBeIndependent operation.
    /// </summary>

    [Fact]
    public void SettingUpdated_MultipleInstancesCreation_ShouldBeIndependent()
    {
        // Arrange & Act
        var automotiveNotification = new SettingUpdated { SettingId = 1001, MaquinaId = 101 };
        var electronicsNotification = new SettingUpdated { SettingId = 2001, MaquinaId = 301 };
        var pharmaceuticalNotification = new SettingUpdated { SettingId = 3001, MaquinaId = 501 };

        // Assert
        automotiveNotification.SettingId.ShouldBe(1001);
        automotiveNotification.MaquinaId.ShouldBe(101);

        electronicsNotification.SettingId.ShouldBe(2001);
        electronicsNotification.MaquinaId.ShouldBe(301);

        pharmaceuticalNotification.SettingId.ShouldBe(3001);
        pharmaceuticalNotification.MaquinaId.ShouldBe(501);
    }
}