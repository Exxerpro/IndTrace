// <copyright file="MachinePlcTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.MachinesTests;

/// <summary>
/// Unit tests for MachinePlc domain entity. Story 26.A2 (#26): the value scalars (MachineId/PlcId/IsActive) are now
/// <c>private set</c>, so construction routes through the public parameterised constructor (or
/// <c>MachinePlc.CreateFixture</c>) instead of raw object-initializer / post-construction property assignments. A
/// bare <c>new MachinePlc()</c> parameterless constructor seeds IsActive = Active by design and is still exercised.
/// </summary>
public class MachinePlcTests
{
    /// <summary>
    /// Executes MachinePlc_Constructor_Default_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void MachinePlc_Constructor_Default_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc();

        // Assert
        machinePlc.ShouldNotBeNull();
        machinePlc.MachineId.Value.ShouldBe(0);
        machinePlc.PlcId.ShouldBe(0);
        machinePlc.IsActive.Value.ShouldBe(1);
    }

    /// <summary>
    /// Executes MachinePlc_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        var machineId = 100;
        var plcId = 200;
        var isActive = 1;

        // Act
        var machinePlc = new MachinePlc(machineId, plcId, isActive);

        // Assert
        machinePlc.ShouldNotBeNull();
        machinePlc.MachineId.Value.ShouldBe(machineId);
        machinePlc.PlcId.ShouldBe(plcId);
        machinePlc.IsActive.Value.ShouldBe(isActive);
    }

    /// <summary>
    /// Executes MachinePlc_WhenPropertiesAssigned_ShouldMaintainAllValues operation. Story 26.A2 (#26): the value
    /// scalars are seeded through the parameterised constructor (private set).
    /// </summary>
    [Fact]
    public void MachinePlc_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var machineId = 150;
        var plcId = 250;
        var isActive = 1;

        // Act
        var machinePlc = new MachinePlc(machineId, plcId, isActive);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(machineId);
        machinePlc.PlcId.ShouldBe(plcId);
        machinePlc.IsActive.Value.ShouldBe(isActive);
    }

    /// <summary>
    /// Executes MachinePlcProperties_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>
    [Fact]
    public void MachinePlcProperties_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(0, 0, 0);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(0);
        machinePlc.PlcId.ShouldBe(0);
        machinePlc.IsActive.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes MachinePlcProperties_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>
    [Fact]
    public void MachinePlcProperties_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(-1, -1, 1);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(-1);
        machinePlc.PlcId.ShouldBe(-1);
        machinePlc.IsActive.Value.ShouldBe(1);
    }

    /// <summary>
    /// Executes MachinePlcProperties_WhenSetToLargeValues_ShouldAcceptLargeValues operation.
    /// </summary>
    [Fact]
    public void MachinePlcProperties_WhenSetToLargeValues_ShouldAcceptLargeValues()
    {
        // Arrange
        var largeValue = int.MaxValue;

        // Act
        var machinePlc = new MachinePlc(largeValue, largeValue, 1);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(largeValue);
        machinePlc.PlcId.ShouldBe(largeValue);
        machinePlc.IsActive.Value.ShouldBe(1);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcIsCreated_ShouldHaveDefaultValues operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcIsCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc();

        // Assert - Verify business logic defaults
        machinePlc.MachineId.Value.ShouldBe(0, "Machine ID should default to 0");
        machinePlc.PlcId.ShouldBe(0, "PLC ID should default to 0");
        machinePlc.IsActive.Value.ShouldBe(1, "IsActive should default to 1 (active)");
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcIsConfigured_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcIsConfigured_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(100, 100, 1);

        // Assert
        machinePlc.ShouldNotBeNull();
        machinePlc.MachineId.Value.ShouldBe(100);
        machinePlc.PlcId.ShouldBe(100);
        machinePlc.IsActive.Value.ShouldBe(1);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcIsActive_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcIsActive_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(0, 0, 1);

        // Assert
        machinePlc.IsActive.Value.ShouldBe(1);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcIsInactive_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcIsInactive_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(0, 0, 0);

        // Assert
        machinePlc.IsActive.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasSameMachineAndPlcIds_ShouldBeValid operation. Byte-equal to the former
    /// object initializer without IsActive: the parameterless constructor seeded IsActive = Active.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasSameMachineAndPlcIds_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(10000, 100, ActiveStatus.Active);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(10000);
        machinePlc.PlcId.ShouldBe(100);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasDifferentMachineAndPlcIds_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasDifferentMachineAndPlcIds_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(10000, 200, ActiveStatus.Active);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(10000);
        machinePlc.PlcId.ShouldBe(200);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasZeroMachineId_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasZeroMachineId_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(0, 100, ActiveStatus.Active);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(0);
        machinePlc.PlcId.ShouldBe(100);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasZeroPlcId_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasZeroPlcId_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(10000, 0, ActiveStatus.Active);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(10000);
        machinePlc.PlcId.ShouldBe(0);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasNegativeMachineId_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasNegativeMachineId_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(-1, 100, ActiveStatus.Active);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(-1);
        machinePlc.PlcId.ShouldBe(100);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasNegativePlcId_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasNegativePlcId_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(10000, -1, ActiveStatus.Active);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(10000);
        machinePlc.PlcId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasLargeMachineId_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasLargeMachineId_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(int.MaxValue, 100, ActiveStatus.Active);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(int.MaxValue);
        machinePlc.PlcId.ShouldBe(100);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasLargePlcId_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasLargePlcId_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(10000, int.MaxValue, ActiveStatus.Active);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(10000);
        machinePlc.PlcId.ShouldBe(int.MaxValue);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasLargeIsActive_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasLargeIsActive_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(10000, 200, 1);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(10000);
        machinePlc.PlcId.ShouldBe(200);
        machinePlc.IsActive.Value.ShouldBe(1);
    }

    /// <summary>
    /// Executes MachinePlc_WhenMachinePlcHasNegativeIsActive_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenMachinePlcHasNegativeIsActive_ShouldBeValid()
    {
        // Arrange & Act
        var machinePlc = new MachinePlc(10000, 200, 0);

        // Assert
        machinePlc.MachineId.Value.ShouldBe(10000);
        machinePlc.PlcId.ShouldBe(200);
        machinePlc.IsActive.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes MachinePlc_WhenValidParameters_ShouldSetAllProperties operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenValidParameters_ShouldSetAllProperties()
    {
        // Arrange
        var machineId = 500;
        var plcId = 600;
        var isActive = 1;

        // Act
        var machinePlc = new MachinePlc(machineId, plcId, isActive);

        // Assert
        machinePlc.ShouldNotBeNull();
        machinePlc.MachineId.Value.ShouldBe(machineId);
        machinePlc.PlcId.ShouldBe(plcId);
        machinePlc.IsActive.Value.ShouldBe(isActive);
    }

    /// <summary>
    /// Executes MachinePlc_WhenZeroParameters_ShouldSetAllProperties operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenZeroParameters_ShouldSetAllProperties()
    {
        // Arrange
        var machineId = 0;
        var plcId = 0;
        var isActive = 0;

        // Act
        var machinePlc = new MachinePlc(machineId, plcId, isActive);

        // Assert
        machinePlc.ShouldNotBeNull();
        machinePlc.MachineId.Value.ShouldBe(machineId);
        machinePlc.PlcId.ShouldBe(plcId);
        machinePlc.IsActive.Value.ShouldBe(isActive);
    }

    /// <summary>
    /// Executes MachinePlc_WhenNegativeParameters_ShouldSetAllProperties operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenNegativeParameters_ShouldSetAllProperties()
    {
        // Arrange
        var machineId = -1;
        var plcId = -2;
        var isActive = 1;

        // Act
        var machinePlc = new MachinePlc(machineId, plcId, isActive);

        // Assert
        machinePlc.ShouldNotBeNull();
        machinePlc.MachineId.Value.ShouldBe(machineId);
        machinePlc.PlcId.ShouldBe(plcId);
        machinePlc.IsActive.Value.ShouldBe(isActive);
    }

    /// <summary>
    /// Executes MachinePlc_WhenLargeParameters_ShouldSetAllProperties operation.
    /// </summary>
    [Fact]
    public void MachinePlc_WhenLargeParameters_ShouldSetAllProperties()
    {
        // Arrange
        var machineId = int.MaxValue;
        var plcId = int.MaxValue - 1;
        var isActive = 0;

        // Act
        var machinePlc = new MachinePlc(machineId, plcId, isActive);

        // Assert
        machinePlc.ShouldNotBeNull();
        machinePlc.MachineId.Value.ShouldBe(machineId);
        machinePlc.PlcId.ShouldBe(plcId);
        machinePlc.IsActive.Value.ShouldBe(isActive);
    }
}
