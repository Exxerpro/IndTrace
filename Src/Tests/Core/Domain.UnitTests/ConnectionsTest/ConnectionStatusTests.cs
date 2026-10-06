// <copyright file="ConnectionStatusTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ConnectionsTest;

/// <summary>
/// Unit tests for ConnectionStatus
/// </summary>
public class ConnectionStatusTests
{
    /// <summary>
    /// Executes ConnectionStatus_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void ConnectionStatus_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new ConnectionStatus();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AuditableEntity>();
        instance.ShouldBeAssignableTo<IEntityRoot>();
    }
    /// <summary>
    /// Executes ConnectionStatus_WhenDefaultValues_ShouldInitializePropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void ConnectionStatus_WhenDefaultValues_ShouldInitializePropertiesCorrectly()
    {
        // Arrange & Act
        var connectionStatus = new ConnectionStatus();

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Updated expectations for null safety refactoring - string properties initialized to non-null defaults to reduce nulls
        connectionStatus.MachineId.Value.ShouldBe(default(int));
        connectionStatus.Status.ShouldBe(default(int));
        connectionStatus.Message.ShouldBe(string.Empty); // Property initialized to string.Empty to avoid nulls
        connectionStatus.ModifiedOn.ShouldBe(default(DateTime)); // Overridden in ConnectionStatus

        // Verify AuditableEntity properties (inherited from base class): timestamps are stamped by the
        // persistence layer on save (#29 F6), so the inherited CreatedOn is null on a fresh entity.
        connectionStatus.CreatedOn.ShouldBeNull();
        connectionStatus.CreatedBy.ShouldBe(string.Empty); // AuditableEntity uses string.Empty to avoid nulls
        connectionStatus.ModifiedBy.ShouldBe(string.Empty); // AuditableEntity uses string.Empty to avoid nulls
    }
    /// <summary>
    /// Executes ConnectionStatusProperties_WhenSetToValidValues_ShouldReturnCorrectValues operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="status">The status.</param>
    /// <param name="message">The message.</param>

    [Theory]
    [InlineData(1001, 1, "Connected")]
    [InlineData(1002, 0, "Disconnected")]
    [InlineData(1003, 2, "Timeout")]
    [InlineData(1004, 3, "Error")]
    [InlineData(1005, 4, "Reconnecting")]
    public void ConnectionStatusProperties_WhenSetToValidValues_ShouldReturnCorrectValues(int machineId, int status, string message)
    {
        // Arrange
        var connectionStatus = new ConnectionStatus();
        var modifiedDate = DateTime.Now;

        // Act
        connectionStatus.MachineId = new MachineId(machineId);
        connectionStatus.Status = status;
        connectionStatus.Message = message;
        connectionStatus.ModifiedOn = modifiedDate;

        // Assert
        connectionStatus.MachineId.Value.ShouldBe(machineId);
        connectionStatus.Status.ShouldBe(status);
        connectionStatus.Message.ShouldBe(message);
        connectionStatus.ModifiedOn.ShouldBe(modifiedDate);
    }
    /// <summary>
    /// Executes ManufacturingConnectionMonitoring_WithDifferentMachines_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="status">The status.</param>
    /// <param name="message">The message.</param>

    [Theory]
    [InlineData(2001, 1, "Ford F-150 Assembly Line - PLC Connected")]
    [InlineData(2002, 0, "Tesla Model S Battery Station - PLC Offline")]
    [InlineData(2003, 2, "BMW X5 Welding Robot - Communication Timeout")]
    [InlineData(2004, 3, "Mercedes GLE Paint Booth - Network Error")]
    [InlineData(2005, 4, "Oemx A4 Quality Station - Attempting Reconnection")]
    public void ManufacturingConnectionMonitoring_WithDifferentMachines_ShouldHandleCorrectly(int machineId, int status, string message)
    {
        // Arrange
        var connectionStatus = new ConnectionStatus();
        var timestamp = DateTime.UtcNow;
        var operatorId = "MFG_OPERATOR_001";

        // Act
        connectionStatus.MachineId = new MachineId(machineId);
        connectionStatus.Status = status;
        connectionStatus.Message = message;
        connectionStatus.ModifiedOn = timestamp;
        connectionStatus.CreatedBy = operatorId;
        connectionStatus.CreatedOn = timestamp;

        // Assert
        connectionStatus.ShouldNotBeNull();
        connectionStatus.MachineId.Value.ShouldBe(machineId);
        connectionStatus.Status.ShouldBe(status);
        connectionStatus.Message.ShouldBe(message);
        connectionStatus.ModifiedOn.ShouldBe(timestamp);
        connectionStatus.CreatedBy.ShouldBe(operatorId);
        connectionStatus.CreatedOn.ShouldBe(timestamp);

        // Verify both interface implementations
        connectionStatus.ShouldBeAssignableTo<IEntityRoot>();
        connectionStatus.ShouldBeAssignableTo<AuditableEntity>();
    }
    /// <summary>
    /// Executes ConnectionStatus_AsAuditableEntity_ShouldInheritAuditingCapabilities operation.
    /// </summary>

    [Fact]
    public void ConnectionStatus_AsAuditableEntity_ShouldInheritAuditingCapabilities()
    {
        // Arrange & Act
        var connectionStatus = new ConnectionStatus();

        // Assert
        connectionStatus.ShouldBeAssignableTo<AuditableEntity>();
    }
    /// <summary>
    /// Executes ConnectionStatus_AsEntityRoot_ShouldImplementInterface operation.
    /// </summary>

    [Fact]
    public void ConnectionStatus_AsEntityRoot_ShouldImplementInterface()
    {
        // Arrange & Act
        var connectionStatus = new ConnectionStatus();

        // Assert
        connectionStatus.ShouldBeAssignableTo<IEntityRoot>();
        connectionStatus.ShouldNotBeAssignableTo<ILookupEntity>();
    }
}