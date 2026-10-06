// <copyright file="PerformanceDataCreatedTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Performances;

/// <summary>
/// Unit tests for PerformanceDataCreated - Notification for manufacturing performance data creation.
/// Tests property validation, notification interface compliance, and performance monitoring scenarios.
/// </summary>
public class PerformanceDataCreatedTests
{
    /// <summary>
    /// Executes Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var instance = new PerformanceDataCreated();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<INotification>();
        instance.PerformanceDataId.ShouldBe(0);
        instance.MachineId.ShouldBe(0);
        instance.PlcId.ShouldBe(0);
        instance.BarCodeId.ShouldBe(0);
        instance.CycleId.ShouldBe(0);
        instance.TimeStamp.ShouldBe(default(DateTime));
        instance.ApplicationFlag.ShouldBe(0);
        instance.EventCounter.ShouldBe(0);
        instance.CurrentTime.ShouldBe(0);
        instance.RunningTime.ShouldBe(0);
        instance.StoppedTime.ShouldBe(0);
        instance.FaultedTime.ShouldBe(0);
        instance.StatusFaultReason.ShouldBe(0);
        instance.TotalProduction.ShouldBe(0.0);
        instance.ProductionOk.ShouldBe(0.0);
        instance.ProductionNoK.ShouldBe(0.0);
        instance.StatusFaultReject.ShouldBe(0);
    }
    /// <summary>
    /// Executes Properties_WithValidManufacturingData_ShouldSetAndReturnCorrectValues operation.
    /// </summary>

    [Theory]
    [InlineData(12345L, 1001, 2001, 3001, 4001)]
    [InlineData(67890L, 1002, 2002, 3002, 4002)]
    [InlineData(11111L, 1003, 2003, 3003, 4003)]
    public void Properties_WithValidManufacturingData_ShouldSetAndReturnCorrectValues(
        long performanceDataId, int machineId, int plcId, int barCodeId, int cycleId)
    {
        // Arrange
        var instance = new PerformanceDataCreated();
        var timestamp = DateTime.UtcNow;

        // Act
        instance.PerformanceDataId = performanceDataId;
        instance.MachineId = machineId;
        instance.PlcId = plcId;
        instance.BarCodeId = barCodeId;
        instance.CycleId = cycleId;
        instance.TimeStamp = timestamp;
        instance.ApplicationFlag = 100;
        instance.EventCounter = 50;
        instance.CurrentTime = 1000;
        instance.RunningTime = 800;
        instance.StoppedTime = 150;
        instance.FaultedTime = 50;
        instance.StatusFaultReason = 0;
        instance.TotalProduction = 95.5;
        instance.ProductionOk = 90.0;
        instance.ProductionNoK = 5.5;
        instance.StatusFaultReject = 0;

        // Assert
        instance.PerformanceDataId.ShouldBe(performanceDataId);
        instance.MachineId.ShouldBe(machineId);
        instance.PlcId.ShouldBe(plcId);
        instance.BarCodeId.ShouldBe(barCodeId);
        instance.CycleId.ShouldBe(cycleId);
        instance.TimeStamp.ShouldBe(timestamp);
        instance.ApplicationFlag.ShouldBe(100);
        instance.EventCounter.ShouldBe(50);
        instance.CurrentTime.ShouldBe(1000);
        instance.RunningTime.ShouldBe(800);
        instance.StoppedTime.ShouldBe(150);
        instance.FaultedTime.ShouldBe(50);
        instance.StatusFaultReason.ShouldBe(0);
        instance.TotalProduction.ShouldBe(95.5);
        instance.ProductionOk.ShouldBe(90.0);
        instance.ProductionNoK.ShouldBe(5.5);
        instance.StatusFaultReject.ShouldBe(0);
    }
    /// <summary>
    /// Executes PerformanceDataCreated_ShouldImplementINotification operation.
    /// </summary>

    [Fact]
    public void PerformanceDataCreated_ShouldImplementINotification()
    {
        // Arrange & Act
        var notification = new PerformanceDataCreated();

        // Assert
        notification.ShouldBeAssignableTo<INotification>();
    }
    /// <summary>
    /// Executes PerformanceDataCreated_WithAutomotiveManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("Ford F-150 Production Line Performance")]
    [InlineData("Tesla Model S Manufacturing Performance")]
    [InlineData("BMW X5 Assembly Line Performance")]
    [InlineData("Mercedes E-Class Production Performance")]
    [InlineData("Oemx A4 Manufacturing Line Performance")]
    public void PerformanceDataCreated_WithAutomotiveManufacturingScenarios_ShouldHandleCorrectly(string scenario)
    {
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Arrange & Act
        var notification = new PerformanceDataCreated
        {
            PerformanceDataId = 98765L,
            MachineId = 2001,
            PlcId = 3001,
            BarCodeId = 4001,
            CycleId = 5001,
            TimeStamp = DateTime.UtcNow,
            TotalProduction = 150.0,
            ProductionOk = 145.0,
            ProductionNoK = 5.0
        };

        // Assert
        notification.ShouldNotBeNull();
        notification.ShouldBeAssignableTo<INotification>();
        notification.TotalProduction.ShouldBe(150.0);
        notification.ProductionOk.ShouldBe(145.0);
        notification.ProductionNoK.ShouldBe(5.0);
    }
    /// <summary>
    /// Executes ToDto_WithValidPerformanceDataCommand_ShouldCreateCorrectNotification operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidPerformanceDataCommand_ShouldCreateCorrectNotification()
    {
        // Arrange
        var command = new PerformanceDataCommand
        {
            PerformanceDataId = 55555L,
            MachineId = 9001,
            PlcId = 8001,
            BarCodeId = 7001,
            CycleId = 6001,
            TimeStamp = DateTime.UtcNow,
            ApplicationFlag = 200,
            EventCounter = 75,
            CurrentTime = 2000,
            RunningTime = 1800,
            StoppedTime = 150,
            FaultedTime = 50,
            StatusFaultReason = 1
        };

        // Act
        var result = PerformanceDataCreated.ToDto(command);

        // Assert
        result.ShouldNotBeNull();
        result.PerformanceDataId.ShouldBe(command.PerformanceDataId);
        result.MachineId.ShouldBe(command.MachineId);
        result.PlcId.ShouldBe(command.PlcId);
        result.BarCodeId.ShouldBe(command.BarCodeId);
        result.CycleId.ShouldBe(command.CycleId);
        result.TimeStamp.ShouldBe(command.TimeStamp);
        result.ApplicationFlag.ShouldBe(command.ApplicationFlag);
        result.EventCounter.ShouldBe(command.EventCounter);
        result.CurrentTime.ShouldBe(command.CurrentTime);
        result.RunningTime.ShouldBe(command.RunningTime);
        result.StoppedTime.ShouldBe(command.StoppedTime);
        result.FaultedTime.ShouldBe(command.FaultedTime);
        result.StatusFaultReason.ShouldBe(command.StatusFaultReason);
    }
    /// <summary>
    /// Executes ToEntity_WithValidPerformanceDataCommand_ShouldCreateCorrectEntity operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidPerformanceDataCommand_ShouldCreateCorrectEntity()
    {
        // Arrange
        var command = new PerformanceDataCommand
        {
            PerformanceDataId = 77777L,
            MachineId = 100234,
            PlcId = 5678,
            BarCodeId = 9012,
            CycleId = 3456,
            TimeStamp = DateTime.UtcNow,
            TotalProduction = 200.0,
            ProductionOk = 190.0,
            ProductionNoK = 10.0
        };

        // Act
        var result = PerformanceDataCreated.ToEntity(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: [ToEntity Pattern] - Updated test to handle Result<T> pattern for Railway-Oriented Programming
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PerformanceDataId.ShouldBe(command.PerformanceDataId);
        result.Value.MachineId.ShouldBe(command.MachineId);
        result.Value.PlcId.ShouldBe(command.PlcId);
        result.Value.BarCodeId.ShouldBe(command.BarCodeId);
        result.Value.CycleId.Value.ShouldBe(command.CycleId);
        result.Value.TimeStamp.ShouldBe(command.TimeStamp);
        result.Value.TotalProduction.ShouldBe(command.TotalProduction);
        result.Value.ProductionOk.ShouldBe(command.ProductionOk);
        result.Value.ProductionNoK.ShouldBe(command.ProductionNoK);
    }
    /// <summary>
    /// Executes Properties_WithVariousTimeValues_ShouldCalculateCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1000, 900, 50, 50)]
    [InlineData(2000, 1800, 100, 100)]
    [InlineData(3000, 2700, 200, 100)]
    [InlineData(4000, 3500, 300, 200)]
    public void Properties_WithVariousTimeValues_ShouldCalculateCorrectly(
        int currentTime, int runningTime, int stoppedTime, int faultedTime)
    {
        // Arrange
        var notification = new PerformanceDataCreated();

        // Act
        notification.CurrentTime = currentTime;
        notification.RunningTime = runningTime;
        notification.StoppedTime = stoppedTime;
        notification.FaultedTime = faultedTime;

        // Assert
        notification.CurrentTime.ShouldBe(currentTime);
        notification.RunningTime.ShouldBe(runningTime);
        notification.StoppedTime.ShouldBe(stoppedTime);
        notification.FaultedTime.ShouldBe(faultedTime);

        // Verify time relationships make sense
        notification.RunningTime.ShouldBeLessThanOrEqualTo(notification.CurrentTime);
        (notification.RunningTime + notification.StoppedTime + notification.FaultedTime)
            .ShouldBeLessThanOrEqualTo(notification.CurrentTime);
    }
    /// <summary>
    /// Executes Properties_WithProductionValues_ShouldValidateCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(100.0, 95.0, 5.0)]
    [InlineData(250.0, 230.0, 20.0)]
    [InlineData(500.0, 475.0, 25.0)]
    [InlineData(1000.0, 950.0, 50.0)]
    public void Properties_WithProductionValues_ShouldValidateCorrectly(
        double totalProduction, double productionOk, double productionNoK)
    {
        // Arrange
        var notification = new PerformanceDataCreated();

        // Act
        notification.TotalProduction = totalProduction;
        notification.ProductionOk = productionOk;
        notification.ProductionNoK = productionNoK;

        // Assert
        notification.TotalProduction.ShouldBe(totalProduction);
        notification.ProductionOk.ShouldBe(productionOk);
        notification.ProductionNoK.ShouldBe(productionNoK);

        // Verify production relationships
        (notification.ProductionOk + notification.ProductionNoK)
            .ShouldBeLessThanOrEqualTo(notification.TotalProduction);
    }
}
