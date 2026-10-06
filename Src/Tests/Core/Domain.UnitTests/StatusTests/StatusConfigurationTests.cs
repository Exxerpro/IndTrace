// <copyright file="StatusConfigurationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StatusTests;

/// <summary>
/// Unit tests for StatusConfiguration
/// </summary>
public class StatusConfigurationTests
{
    /// <summary>
    /// Executes StatusConfiguration_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void StatusConfiguration_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new StatusConfiguration();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IEntityRoot>();
    }
    /// <summary>
    /// Executes StatusConfiguration_WhenDefaultValues_ShouldInitializePropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void StatusConfiguration_WhenDefaultValues_ShouldInitializePropertiesCorrectly()
    {
        // Arrange & Act
        var config = new StatusConfiguration();

        // Assert
        config.MachineId.Value.ShouldBe(default(int));
        config.Status.ShouldBe(default(int));
        config.Message.ShouldBe(string.Empty);
        config.ModifiedOn.ShouldBe(default(DateTime));
    }
    /// <summary>
    /// Executes StatusConfigurationProperties_WhenSetToValidValues_ShouldReturnCorrectValues operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="status">The status.</param>
    /// <param name="message">The message.</param>

    [Theory]
    [InlineData(1001, 1, "Machine Running")]
    [InlineData(1002, 2, "Machine Stopped")]
    [InlineData(1003, 3, "Machine Error")]
    [InlineData(1004, 4, "Maintenance Mode")]
    [InlineData(1005, 0, "Unknown Status")]
    public void StatusConfigurationProperties_WhenSetToValidValues_ShouldReturnCorrectValues(int machineId, int status, string message)
    {
        // Arrange
        var config = new StatusConfiguration();
        var modifiedDate = DateTime.Now;

        // Act
        config.MachineId = new MachineId(machineId);
        config.Status = status;
        config.Message = message;
        config.ModifiedOn = modifiedDate;

        // Assert
        config.MachineId.Value.ShouldBe(machineId);
        config.Status.ShouldBe(status);
        config.Message.ShouldBe(message);
        config.ModifiedOn.ShouldBe(modifiedDate);
    }
    /// <summary>
    /// Executes ManufacturingStatusConfiguration_WithDifferentMachines_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="status">The status.</param>
    /// <param name="message">The message.</param>

    [Theory]
    [InlineData(2001, 1, "Ford F-150 Assembly Line - Running")]
    [InlineData(2002, 2, "Tesla Model S Battery Station - Stopped")]
    [InlineData(2003, 3, "BMW X5 Welding Robot - Error State")]
    [InlineData(2004, 4, "Mercedes GLE Paint Booth - Maintenance")]
    [InlineData(2005, 0, "Oemx A4 Quality Check - Idle")]
    public void ManufacturingStatusConfiguration_WithDifferentMachines_ShouldHandleCorrectly(int machineId, int status, string message)
    {
        // Arrange
        var config = new StatusConfiguration();
        var timestamp = DateTime.UtcNow;

        // Act
        config.MachineId = new MachineId(machineId);
        config.Status = status;
        config.Message = message;
        config.ModifiedOn = timestamp;

        // Assert
        config.ShouldNotBeNull();
        config.MachineId.Value.ShouldBe(machineId);
        config.Status.ShouldBe(status);
        config.Message.ShouldBe(message);
        config.ModifiedOn.ShouldBe(timestamp);

        // Verify IEntityRoot compliance (#95 Phase 2.1: per-machine mutable state, no longer ILookupEntity)
        config.ShouldBeAssignableTo<IEntityRoot>();
    }
    /// <summary>
    /// Executes StatusConfiguration_AsEntityRoot_ShouldImplementInterface operation.
    /// </summary>

    [Fact]
    public void StatusConfiguration_AsEntityRoot_ShouldImplementInterface()
    {
        // Arrange & Act
        var config = new StatusConfiguration();

        // Assert
        config.ShouldBeAssignableTo<IEntityRoot>();
        config.ShouldNotBeAssignableTo<ILookupEntity>();
    }
}