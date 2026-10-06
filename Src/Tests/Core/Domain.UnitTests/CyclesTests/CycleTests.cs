// <copyright file="CycleTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.CyclesTests;

using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;
using IndTrace.TestData.Builders;

/// <summary>
/// Unit tests for Cycle domain entity
/// </summary>
public class CycleTests
{
    /// <summary>
    /// Executes Cycle_WhenCreatedWithCompleteValidData_ShouldSetAllPropertiesCorrectly operation.
    /// </summary>
    [Fact]
    public void Cycle_WhenCreatedWithCompleteValidData_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var cycleId = 1;
        var machineId = 1;
        var barCodeId = 1;
        var cycleStatus = CycleStatus.Started;
        var partStatus = PartStatus.Ok;
        var cycleTime = 120;
        var taktTime = 100;
        var cyclesOk = 5;

        // Act
        var cycle = new CycleBuilder()
            .Started(partStatus)
            .With(c =>
            {
                c.CycleId = new CycleId(cycleId);
                c.MachineId = new MachineId(machineId);
                c.BarCodeId = new BarCodeId(barCodeId);
                c.CycleTime = cycleTime;
                c.TaktTime = taktTime;
                c.CyclesOk = cyclesOk;
            })
            .Build();

        // Assert
        cycle.ShouldNotBe(default);
        cycle.CycleId.Value.ShouldBe(cycleId);
        cycle.MachineId.Value.ShouldBe(machineId);
        cycle.BarCodeId.Value.ShouldBe(barCodeId);
        cycle.CycleStatus.ShouldBe(cycleStatus);
        cycle.PartStatus.ShouldBe(partStatus);
        cycle.CycleTime.ShouldBe(cycleTime);
        cycle.TaktTime.ShouldBe(taktTime);
        cycle.CyclesOk.ShouldBe(cyclesOk);
    }
    /// <summary>
    /// Executes Cycle_WhenCreatedWithoutParameters_ShouldInitializeWithDefaultValues operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCreatedWithoutParameters_ShouldInitializeWithDefaultValues()
    {
        // Act
        var cycle = new Cycle();

        // Assert
        cycle.ShouldNotBeNull();
        cycle.CycleId.Value.ShouldBe(0);
        cycle.MachineId.Value.ShouldBe(0);
        cycle.BarCodeId.Value.ShouldBe(0);
        cycle.CycleStatus.ShouldBe(CycleStatus.None);
        cycle.CyclesOk.ShouldBe(0);
        cycle.PartStatus.ShouldBe(PartStatus.None);
        cycle.CycleTime.ShouldBe(0);
        cycle.TaktTime.ShouldBe(0);
        cycle.StartedOn.ShouldBe(default);
        cycle.FinishedOn.ShouldBe(default);
    }
    /// <summary>
    /// Executes Cycle_WhenAllPropertiesUpdated_ShouldPersistAllChangesWithTimestamps operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenAllPropertiesUpdated_ShouldPersistAllChangesWithTimestamps()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var finish = now.AddMinutes(1);

        // Act (Story 6.4: status setters are private set; reach the (FinishedOk, Restored) pair via the builder)
        var cycle = new CycleBuilder()
            .AtState(CycleStatus.FinishedOk, PartStatus.Restored)
            .With(c =>
            {
                c.CycleId = new CycleId(1);
                c.MachineId = new MachineId(2);
                c.BarCodeId = new BarCodeId(3);
                c.CyclesOk = 5;
                c.CycleTime = 7;
                c.TaktTime = 8;
                c.StartedOn = now;
                c.FinishedOn = finish;
            })
            .Build();

        // Assert
        cycle.CycleId.Value.ShouldBe(1);
        cycle.MachineId.Value.ShouldBe(2);
        cycle.BarCodeId.Value.ShouldBe(3);
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        cycle.CyclesOk.ShouldBe(5);
        cycle.PartStatus.ShouldBe(PartStatus.Restored);
        cycle.CycleTime.ShouldBe(7);
        cycle.TaktTime.ShouldBe(8);
        cycle.StartedOn.ShouldBe(now);
        cycle.FinishedOn.ShouldBe(finish);
    }
    /// <summary>
    /// Executes Cycle_WithNegativeAndLargeValues_WithEdgeCases_ShouldHandleWithoutErrors operation.
    /// </summary>

    [Fact]
    public void Cycle_WithNegativeAndLargeValues_WithEdgeCases_ShouldHandleWithoutErrors()
    {
        // Arrange
        var cycle = new CycleBuilder()
            .AtState(CycleStatus.Invalid, PartStatus.Invalid)
            .With(c =>
            {
                c.CycleId = new CycleId(-1);
                c.MachineId = new MachineId(int.MaxValue);
                c.BarCodeId = new BarCodeId(-2);
                c.CyclesOk = int.MaxValue;
                c.CycleTime = int.MaxValue;
                c.TaktTime = int.MinValue;
                c.StartedOn = DateTime.MinValue;
                c.FinishedOn = DateTime.MaxValue;
            })
            .Build();

        // Act & Assert
        cycle.CycleId.Value.ShouldBe(-1);
        cycle.MachineId.Value.ShouldBe(int.MaxValue);
        cycle.BarCodeId.Value.ShouldBe(-2);
        cycle.CycleStatus.ShouldBe(CycleStatus.Invalid);
        cycle.CyclesOk.ShouldBe(int.MaxValue);
        cycle.PartStatus.ShouldBe(PartStatus.Invalid);
        cycle.CycleTime.ShouldBe(int.MaxValue);
        cycle.TaktTime.ShouldBe(int.MinValue);
        cycle.StartedOn.ShouldBe(DateTime.MinValue);
        cycle.FinishedOn.ShouldBe(DateTime.MaxValue);
    }
    /// <summary>
    /// Executes Cycle_Cycle_WhenCreatedWithDefaultConstructor_ShouldHaveExpectedInitialState operation.
    /// </summary>

    [Fact]
    public void Cycle_Cycle_WhenCreatedWithDefaultConstructor_ShouldHaveExpectedInitialState()
    {
        // Arrange & Act
        var cycle = new Cycle();

        // Assert
        cycle.ShouldNotBe(default);
        cycle.CycleId.Value.ShouldBe(0);
        cycle.MachineId.Value.ShouldBe(0);
        cycle.BarCodeId.Value.ShouldBe(0);
        cycle.CycleStatus.Value.ShouldBe(0);
        cycle.PartStatus.Value.ShouldBe(0);
        cycle.CycleTime.ShouldBe(0);
        cycle.TaktTime.ShouldBe(0);
        cycle.CyclesOk.ShouldBe(0);
    }
    /// <summary>
    /// Executes Cycle_WhenPropertiesSetToSpecificValues_ShouldReturnExactValues operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenPropertiesSetToSpecificValues_ShouldReturnExactValues()
    {
        // Act (Story 6.4: status setters are private set; reach the (FinishedOk=4, Ok=1) pair via the builder)
        var cycle = new CycleBuilder()
            .AtState(CycleStatus.FinishedOk, PartStatus.Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(123);
                c.MachineId = new MachineId(456);
                c.BarCodeId = new BarCodeId(789);
                c.CycleTime = 150;
                c.TaktTime = 125;
                c.CyclesOk = 10;
                c.StartedOn = DateTime.Now;
                c.FinishedOn = DateTime.Now.AddMinutes(2);
            })
            .Build();

        // Assert
        cycle.CycleId.Value.ShouldBe(123);
        cycle.MachineId.Value.ShouldBe(456);
        cycle.BarCodeId.Value.ShouldBe(789);
        cycle.CycleStatus.Value.ShouldBe(4);
        cycle.PartStatus.Value.ShouldBe(1);
        cycle.CycleTime.ShouldBe(150);
        cycle.TaktTime.ShouldBe(125);
        cycle.CyclesOk.ShouldBe(10);
        cycle.StartedOn.ShouldNotBe(default);
        cycle.FinishedOn.ShouldNotBe(default);
    }
    /// <summary>
    /// Executes CycleId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void CycleId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.CycleId = new CycleId(0);

        // Assert
        cycle.CycleId.Value.ShouldBe(0);
    }
    /// <summary>
    /// Executes CycleId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void CycleId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.CycleId = new CycleId(-1);

        // Assert
        cycle.CycleId.Value.ShouldBe(-1);
    }
    /// <summary>
    /// Executes MachineId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.MachineId = new MachineId(0);

        // Assert
        cycle.MachineId.Value.ShouldBe(0);
    }
    /// <summary>
    /// Executes MachineId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.MachineId = new MachineId(-1);

        // Assert
        cycle.MachineId.Value.ShouldBe(-1);
    }
    /// <summary>
    /// Executes BarCodeId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void BarCodeId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.BarCodeId = new BarCodeId(0);

        // Assert
        cycle.BarCodeId.Value.ShouldBe(0);
    }
    /// <summary>
    /// Executes BarCodeId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void BarCodeId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.BarCodeId = new BarCodeId(-1);

        // Assert
        cycle.BarCodeId.Value.ShouldBe(-1);
    }
    /// <summary>
    /// Executes CycleTime_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void CycleTime_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.CycleTime = 0;

        // Assert
        cycle.CycleTime.ShouldBe(0);
    }
    /// <summary>
    /// Executes CycleTime_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void CycleTime_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.CycleTime = -1;

        // Assert
        cycle.CycleTime.ShouldBe(-1);
    }
    /// <summary>
    /// Executes TaktTime_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void TaktTime_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.TaktTime = 0;

        // Assert
        cycle.TaktTime.ShouldBe(0);
    }
    /// <summary>
    /// Executes TaktTime_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void TaktTime_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.TaktTime = -1;

        // Assert
        cycle.TaktTime.ShouldBe(-1);
    }
    /// <summary>
    /// Executes CyclesOk_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void CyclesOk_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.CyclesOk = 0;

        // Assert
        cycle.CyclesOk.ShouldBe(0);
    }
    /// <summary>
    /// Executes CyclesOk_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void CyclesOk_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var cycle = new Cycle();

        // Act
        cycle.CyclesOk = -1;

        // Assert
        cycle.CyclesOk.ShouldBe(-1);
    }
    /// <summary>
    /// Executes StartedOn_WhenSet_ShouldStoreDateTime operation.
    /// </summary>

    [Fact]
    public void StartedOn_WhenSet_ShouldStoreDateTime()
    {
        // Arrange
        var cycle = new Cycle();
        var expectedDateTime = DateTime.Now;

        // Act
        cycle.StartedOn = expectedDateTime;

        // Assert
        cycle.StartedOn.ShouldBe(expectedDateTime);
    }
    /// <summary>
    /// Executes FinishedOn_WhenSet_ShouldStoreDateTime operation.
    /// </summary>

    [Fact]
    public void FinishedOn_WhenSet_ShouldStoreDateTime()
    {
        // Arrange
        var cycle = new Cycle();
        var expectedDateTime = DateTime.Now;

        // Act
        cycle.FinishedOn = expectedDateTime;

        // Assert
        cycle.FinishedOn.ShouldBe(expectedDateTime);
    }
    /// <summary>
    /// Executes CycleStatus_WhenSetToNone_ShouldAcceptNone operation.
    /// </summary>

    [Fact]
    public void CycleStatus_WhenSetToNone_ShouldAcceptNone()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var cycle = new CycleBuilder().AtState(CycleStatus.None, PartStatus.None).Build();

        // Assert
        cycle.CycleStatus.Value.ShouldBe(0);
    }
    /// <summary>
    /// Executes CycleStatus_WhenSetToStarted_ShouldAcceptStarted operation.
    /// </summary>

    [Fact]
    public void CycleStatus_WhenSetToStarted_ShouldAcceptStarted()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var cycle = new CycleBuilder().AtState(CycleStatus.Started, PartStatus.None).Build();

        // Assert
        cycle.CycleStatus.Value.ShouldBe(2);
    }
    /// <summary>
    /// Executes CycleStatus_WhenSetToFinishedOk_ShouldAcceptFinishedOk operation.
    /// </summary>

    [Fact]
    public void CycleStatus_WhenSetToFinishedOk_ShouldAcceptFinishedOk()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var cycle = new CycleBuilder().AtState(CycleStatus.FinishedOk, PartStatus.None).Build();

        // Assert
        cycle.CycleStatus.Value.ShouldBe(4);
    }
    /// <summary>
    /// Executes CycleStatus_WhenSetToFinishedNok_ShouldAcceptFinishedNok operation.
    /// </summary>

    [Fact]
    public void CycleStatus_WhenSetToFinishedNok_ShouldAcceptFinishedNok()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var cycle = new CycleBuilder().AtState(CycleStatus.FinishedNok, PartStatus.None).Build();

        // Assert
        cycle.CycleStatus.Value.ShouldBe(8);
    }
    /// <summary>
    /// Executes PartStatus_WhenSetToNone_ShouldAcceptNone operation.
    /// </summary>

    [Fact]
    public void PartStatus_WhenSetToNone_ShouldAcceptNone()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var cycle = new CycleBuilder().AtState(CycleStatus.None, PartStatus.None).Build();

        // Assert
        cycle.PartStatus.Value.ShouldBe(0);
    }
    /// <summary>
    /// Executes PartStatus_WhenSetToOk_ShouldAcceptOk operation.
    /// </summary>

    [Fact]
    public void PartStatus_WhenSetToOk_ShouldAcceptOk()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var cycle = new CycleBuilder().AtState(CycleStatus.None, PartStatus.Ok).Build();

        // Assert
        cycle.PartStatus.Value.ShouldBe(1);
    }
    /// <summary>
    /// Executes PartStatus_WhenSetToNok_ShouldAcceptNok operation.
    /// </summary>

    [Fact]
    public void PartStatus_WhenSetToNok_ShouldAcceptNok()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var cycle = new CycleBuilder().AtState(CycleStatus.None, PartStatus.NOk).Build();

        // Assert
        cycle.PartStatus.Value.ShouldBe(2);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleStatusSetToStarted_ShouldRetainStartedStatusValue operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleStatusSetToStarted_ShouldRetainStartedStatusValue()
    {
        // Arrange
        var cycle = new CycleBuilder().Started().Build(); // Started

        // Act & Assert
        cycle.CycleStatus.Value.ShouldBe(2);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleStatusSetToFinishedOk_ShouldRetainFinishedOkStatusValue operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleStatusSetToFinishedOk_ShouldRetainFinishedOkStatusValue()
    {
        // Arrange
        var cycle = new CycleBuilder().FinishedOk().Build(); // FinishedOk

        // Act & Assert
        cycle.CycleStatus.Value.ShouldBe(4);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleStatusSetToFinishedNok_ShouldRetainFinishedNokStatusValue operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleStatusSetToFinishedNok_ShouldRetainFinishedNokStatusValue()
    {
        // Arrange
        var cycle = new CycleBuilder().FinishedNok().Build(); // FinishedNok

        // Act & Assert
        cycle.CycleStatus.Value.ShouldBe(8);
    }
    /// <summary>
    /// Executes Cycle_WhenPartStatusSetToOk_ShouldRetainOkPartStatusValue operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenPartStatusSetToOk_ShouldRetainOkPartStatusValue()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.None, PartStatus.Ok).Build(); // Ok

        // Act & Assert
        cycle.PartStatus.Value.ShouldBe(1);
    }
    /// <summary>
    /// Executes Cycle_WhenPartStatusSetToNok_ShouldRetainNokPartStatusValue operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenPartStatusSetToNok_ShouldRetainNokPartStatusValue()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.None, PartStatus.NOk).Build(); // NOk

        // Act & Assert
        cycle.PartStatus.Value.ShouldBe(2);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleHasTimestamps_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleHasTimestamps_ShouldBeValid()
    {
        // Arrange
        var cycle = new Cycle
        {
            StartedOn = DateTime.Now,
            FinishedOn = DateTime.Now.AddMinutes(2)
        };

        // Act & Assert
        cycle.StartedOn.ShouldNotBe(default);
        cycle.FinishedOn.ShouldNotBe(default);
        cycle.FinishedOn.ShouldBeGreaterThan(cycle.StartedOn);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleIsNew_ShouldHaveDefaultValues operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleIsNew_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var cycle = new Cycle();

        // Assert
        cycle.CycleId.Value.ShouldBe(0);
        cycle.MachineId.Value.ShouldBe(0);
        cycle.BarCodeId.Value.ShouldBe(0);
        cycle.CycleStatus.Value.ShouldBe(0);
        cycle.PartStatus.Value.ShouldBe(0);
        cycle.CycleTime.ShouldBe(0);
        cycle.TaktTime.ShouldBe(0);
        cycle.CyclesOk.ShouldBe(0);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleIsFullyConfigured_ShouldBeComplete operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleIsFullyConfigured_ShouldBeComplete()
    {
        // Arrange
        var cycle = new CycleBuilder()
            .FinishedOk(PartStatus.Ok) // FinishedOk / Ok
            .With(c =>
            {
                c.CycleId = new CycleId(123);
                c.MachineId = new MachineId(456);
                c.BarCodeId = new BarCodeId(789);
                c.CycleTime = 150;
                c.TaktTime = 125;
                c.CyclesOk = 10;
                c.StartedOn = DateTime.Now;
                c.FinishedOn = DateTime.Now.AddMinutes(2);
            })
            .Build();

        // Act & Assert
        cycle.CycleId.Value.ShouldBe(123);
        cycle.MachineId.Value.ShouldBe(456);
        cycle.BarCodeId.Value.ShouldBe(789);
        cycle.CycleStatus.Value.ShouldBe(4);
        cycle.PartStatus.Value.ShouldBe(1);
        cycle.CycleTime.ShouldBe(150);
        cycle.TaktTime.ShouldBe(125);
        cycle.CyclesOk.ShouldBe(10);
        cycle.StartedOn.ShouldNotBe(default);
        cycle.FinishedOn.ShouldNotBe(default);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleTimeIsGreaterThanTaktTime_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleTimeIsGreaterThanTaktTime_ShouldBeValid()
    {
        // Arrange
        var cycle = new Cycle
        {
            CycleTime = 150,
            TaktTime = 100
        };

        // Act & Assert
        cycle.CycleTime.ShouldBe(150);
        cycle.TaktTime.ShouldBe(100);
        cycle.CycleTime.ShouldBeGreaterThan(cycle.TaktTime);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleTimeIsLessThanTaktTime_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleTimeIsLessThanTaktTime_ShouldBeValid()
    {
        // Arrange
        var cycle = new Cycle
        {
            CycleTime = 80,
            TaktTime = 100
        };

        // Act & Assert
        cycle.CycleTime.ShouldBe(80);
        cycle.TaktTime.ShouldBe(100);
        cycle.CycleTime.ShouldBeLessThan(cycle.TaktTime);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleTimeEqualsTaktTime_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleTimeEqualsTaktTime_ShouldBeValid()
    {
        // Arrange
        var cycle = new Cycle
        {
            CycleTime = 100,
            TaktTime = 100
        };

        // Act & Assert
        cycle.CycleTime.ShouldBe(100);
        cycle.TaktTime.ShouldBe(100);
        cycle.CycleTime.ShouldBe(cycle.TaktTime);
    }
    /// <summary>
    /// Executes Cycle_WhenCyclesOkIsPositive_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCyclesOkIsPositive_ShouldBeValid()
    {
        // Arrange
        var cycle = new Cycle { CyclesOk = 5 };

        // Act & Assert
        cycle.CyclesOk.ShouldBe(5);
        cycle.CyclesOk.ShouldBeGreaterThan(0);
    }
    /// <summary>
    /// Executes Cycle_WhenCyclesOkIsZero_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCyclesOkIsZero_ShouldBeValid()
    {
        // Arrange
        var cycle = new Cycle { CyclesOk = 0 };

        // Act & Assert
        cycle.CyclesOk.ShouldBe(0);
    }
    /// <summary>
    /// Executes Cycle_WhenCyclesOkIsNegative_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCyclesOkIsNegative_ShouldBeValid()
    {
        // Arrange
        var cycle = new Cycle { CyclesOk = -1 };

        // Act & Assert
        cycle.CyclesOk.ShouldBe(-1);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleHasLargeValues_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleHasLargeValues_ShouldBeValid()
    {
        // Arrange
        var cycle = new Cycle
        {
            CycleId = new CycleId(int.MaxValue),
            MachineId = new MachineId(int.MaxValue),
            BarCodeId = new BarCodeId(int.MaxValue),
            CycleTime = int.MaxValue,
            TaktTime = int.MaxValue,
            CyclesOk = int.MaxValue
        };

        // Act & Assert
        cycle.CycleId.Value.ShouldBe(int.MaxValue);
        cycle.MachineId.Value.ShouldBe(int.MaxValue);
        cycle.BarCodeId.Value.ShouldBe(int.MaxValue);
        cycle.CycleTime.ShouldBe(int.MaxValue);
        cycle.TaktTime.ShouldBe(int.MaxValue);
        cycle.CyclesOk.ShouldBe(int.MaxValue);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleIsStartedWithOkPart_ShouldBeValidConfiguration operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleIsStartedWithOkPart_ShouldBeValidConfiguration()
    {
        // Arrange
        var cycle = new CycleBuilder().Started(PartStatus.Ok).Build(); // Started / Ok

        // Act & Assert
        cycle.CycleStatus.Value.ShouldBe(2);
        cycle.PartStatus.Value.ShouldBe(1);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleIsFinishedOkWithOkPart_ShouldBeValidConfiguration operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleIsFinishedOkWithOkPart_ShouldBeValidConfiguration()
    {
        // Arrange
        var cycle = new CycleBuilder().FinishedOk(PartStatus.Ok).Build(); // FinishedOk / Ok

        // Act & Assert
        cycle.CycleStatus.Value.ShouldBe(4);
        cycle.PartStatus.Value.ShouldBe(1);
    }
    /// <summary>
    /// Executes Cycle_WhenCycleIsFinishedNokWithNokPart_ShouldBeValidConfiguration operation.
    /// </summary>

    [Fact]
    public void Cycle_WhenCycleIsFinishedNokWithNokPart_ShouldBeValidConfiguration()
    {
        // Arrange
        var cycle = new CycleBuilder().FinishedNok(PartStatus.NOk).Build(); // FinishedNok / NOk

        // Act & Assert
        cycle.CycleStatus.Value.ShouldBe(8);
        cycle.PartStatus.Value.ShouldBe(2);
    }
    /// <summary>
    /// Executes Cycle_Cycle_Properties_ShouldSetAndGetCorrectly operation.
    /// </summary>

    [Fact]
    public void Cycle_Cycle_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var finishedTime = now.AddSeconds(30);

        // Act (Story 6.4: status setters are private set; reach the (Started=2, Ok=1) pair via the builder)
        var cycle = new CycleBuilder()
            .AtState(CycleStatus.Started, PartStatus.Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(1);
                c.MachineId = new MachineId(1000);
                c.BarCodeId = new BarCodeId(100);
                c.CyclesOk = 5;
                c.CycleTime = 30000;
                c.TaktTime = 35000;
                c.StartedOn = now;
                c.FinishedOn = finishedTime;
            })
            .Build();

        // Assert
        cycle.CycleId.Value.ShouldBe(1);
        cycle.MachineId.Value.ShouldBe(1000);
        cycle.BarCodeId.Value.ShouldBe(100);
        cycle.CycleStatus.Value.ShouldBe(2);
        cycle.CyclesOk.ShouldBe(5);
        cycle.PartStatus.Value.ShouldBe(1);
        cycle.CycleTime.ShouldBe(30000);
        cycle.TaktTime.ShouldBe(35000);
        cycle.StartedOn.ShouldBe(now);
        cycle.FinishedOn.ShouldBe(finishedTime);
    }
}