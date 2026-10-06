// <copyright file="MachineStatusTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.MachinesTests;

using IndTrace.Domain.ValueObjects;

/// <summary>
/// Unit tests for MachineStatus - Manufacturing equipment status entity for operational monitoring
/// </summary>
public class MachineStatusTests
{
    /// <summary>
    /// Executes MachineStatus_WhenDefaultParameters_ShouldCreateValidInstance operation.
    /// </summary>
    [Fact]
    public void MachineStatus_WhenDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var machineStatus = new MachineStatus();

        // Assert
        machineStatus.ShouldNotBeNull();
        machineStatus.ShouldBeAssignableTo<IEntityRoot>();
    }
    /// <summary>
    /// Executes MachineId_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var machineStatus = new MachineStatus();
        const int expectedMachineId = 100001;

        // Act
        machineStatus.MachineId = new MachineId(expectedMachineId);

        // Assert
        machineStatus.MachineId.Value.ShouldBe(expectedMachineId);
    }
    /// <summary>
    /// Executes StatusMachineId_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void StatusMachineId_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var machineStatus = new MachineStatus();
        const int expectedStatusMachineId = 2002;

        // Act
        machineStatus.StatusMachineId = expectedStatusMachineId;

        // Assert
        machineStatus.StatusMachineId.ShouldBe(expectedStatusMachineId);
    }
    /// <summary>
    /// Executes BreakDownTime_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void BreakDownTime_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var machineStatus = new MachineStatus();
        const decimal expectedBreakDownTime = 45.75m;

        // Act
        machineStatus.BreakDownTime = expectedBreakDownTime;

        // Assert
        machineStatus.BreakDownTime.ShouldBe(expectedBreakDownTime);
    }
    /// <summary>
    /// Executes UpdatedOn_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void UpdatedOn_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var machineStatus = new MachineStatus();
        var expectedUpdatedOn = new DateTime(2025, 1, 20, 14, 30, 45);

        // Act
        machineStatus.UpdatedOn = expectedUpdatedOn;

        // Assert
        machineStatus.UpdatedOn.ShouldBe(expectedUpdatedOn);
    }
    /// <summary>
    /// Executes MachineStatusProperties_WhenSetWithManufacturingData_ShouldRetainAllValues operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 100, 0.0, "Machine RUNNING")]
    [InlineData(1002, 200, 15.5, "Machine STOPPED")]
    [InlineData(1003, 300, 120.75, "Machine BREAKDOWN")]
    [InlineData(1004, 400, 5.25, "Machine MAINTENANCE")]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

    // Justification: Scenario parameter provides test context documentation

    // Approved By: CLAUDE on 27/08/2025

    public void MachineStatusProperties_WhenSetWithManufacturingData_ShouldRetainAllValues(
        int machineId, int statusMachineId, decimal breakDownTime, string scenario)

#pragma warning restore xUnit1026
    {
        // Arrange
        var machineStatus = new MachineStatus();
        var updatedOn = DateTime.Now;

        // Act
        machineStatus.MachineId = new MachineId(machineId);
        machineStatus.StatusMachineId = statusMachineId;
        machineStatus.BreakDownTime = breakDownTime;
        machineStatus.UpdatedOn = updatedOn;

        // Assert
        machineStatus.MachineId.Value.ShouldBe(machineId);
        machineStatus.StatusMachineId.ShouldBe(statusMachineId);
        machineStatus.BreakDownTime.ShouldBe(breakDownTime);
        machineStatus.UpdatedOn.ShouldBe(updatedOn);
    }
    /// <summary>
    /// Executes CompleteStatusRecord_WhenPopulated_ShouldMaintainAllProperties operation.
    /// </summary>

    [Fact]
    public void CompleteStatusRecord_WhenPopulated_ShouldMaintainAllProperties()
    {
        // Arrange
        var machineStatus = new MachineStatus();
        const int machineId = 5001;
        const int statusMachineId = 501;
        const decimal breakDownTime = 75.50m;
        var updatedOn = new DateTime(2025, 1, 20, 16, 45, 30);

        // Act
        machineStatus.MachineId = new MachineId(machineId);
        machineStatus.StatusMachineId = statusMachineId;
        machineStatus.BreakDownTime = breakDownTime;
        machineStatus.UpdatedOn = updatedOn;

        // Assert
        machineStatus.MachineId.Value.ShouldBe(machineId);
        machineStatus.StatusMachineId.ShouldBe(statusMachineId);
        machineStatus.BreakDownTime.ShouldBe(breakDownTime);
        machineStatus.UpdatedOn.ShouldBe(updatedOn);
    }
    /// <summary>
    /// Executes MachineId_WhenSetToAnyValue_ShouldBeAccepted operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999999)]
    public void MachineId_WhenSetToAnyValue_ShouldBeAccepted(int value)
    {
        // Arrange
        var machineStatus = new MachineStatus();

        // Act
        machineStatus.MachineId = new MachineId(value);

        // Assert
        machineStatus.MachineId.Value.ShouldBe(value);
    }
    /// <summary>
    /// Executes StatusMachineId_WhenSetToAnyValue_ShouldBeAccepted operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999999)]
    public void StatusMachineId_WhenSetToAnyValue_ShouldBeAccepted(int value)
    {
        // Arrange
        var machineStatus = new MachineStatus();

        // Act
        machineStatus.StatusMachineId = value;

        // Assert
        machineStatus.StatusMachineId.ShouldBe(value);
    }
    /// <summary>
    /// Executes BreakDownTime_WhenSetToAnyValue_ShouldBeAccepted operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.5)]
    [InlineData(999999.99)]
    public void BreakDownTime_WhenSetToAnyValue_ShouldBeAccepted(decimal value)
    {
        // Arrange
        var machineStatus = new MachineStatus();

        // Act
        machineStatus.BreakDownTime = value;

        // Assert
        machineStatus.BreakDownTime.ShouldBe(value);
    }
    /// <summary>
    /// Executes ZeroBreakDownTime_ShouldIndicateNormalOperation operation.
    /// </summary>

    [Fact]
    public void ZeroBreakDownTime_ShouldIndicateNormalOperation()
    {
        // Arrange
        var machineStatus = new MachineStatus();

        // Act
        machineStatus.BreakDownTime = 0.0m;

        // Assert
        machineStatus.BreakDownTime.ShouldBe(0.0m);
    }
    /// <summary>
    /// Executes PositiveBreakDownTime_ShouldIndicateMachineDowntime operation.
    /// </summary>

    [Fact]
    public void PositiveBreakDownTime_ShouldIndicateMachineDowntime()
    {
        // Arrange
        var machineStatus = new MachineStatus();
        const decimal downtime = 180.25m; // 3 hours and 15 minutes

        // Act
        machineStatus.BreakDownTime = downtime;

        // Assert
        machineStatus.BreakDownTime.ShouldBe(downtime);
        machineStatus.BreakDownTime.ShouldBeGreaterThan(0);
    }
    /// <summary>
    /// Executes UpdatedOn_WhenSetToCurrentTime_ShouldAcceptTimestamp operation.
    /// </summary>

    [Fact]
    public void UpdatedOn_WhenSetToCurrentTime_ShouldAcceptTimestamp()
    {
        // Arrange
        var machineStatus = new MachineStatus();
        var currentTime = DateTime.Now;

        // Act
        machineStatus.UpdatedOn = currentTime;

        // Assert
        machineStatus.UpdatedOn.ShouldBe(currentTime);
    }
    /// <summary>
    /// Executes UpdatedOn_WhenSetToMinValue_ShouldAcceptValue operation.
    /// </summary>

    [Fact]
    public void UpdatedOn_WhenSetToMinValue_ShouldAcceptValue()
    {
        // Arrange
        var machineStatus = new MachineStatus();

        // Act
        machineStatus.UpdatedOn = DateTime.MinValue;

        // Assert
        machineStatus.UpdatedOn.ShouldBe(DateTime.MinValue);
    }
    /// <summary>
    /// Executes UpdatedOn_WhenSetToMaxValue_ShouldAcceptValue operation.
    /// </summary>

    [Fact]
    public void UpdatedOn_WhenSetToMaxValue_ShouldAcceptValue()
    {
        // Arrange
        var machineStatus = new MachineStatus();

        // Act
        machineStatus.UpdatedOn = DateTime.MaxValue;

        // Assert
        machineStatus.UpdatedOn.ShouldBe(DateTime.MaxValue);
    }
    /// <summary>
    /// Executes PropertyRoundTrip_WhenSetAndGet_ShouldMaintainValues operation.
    /// </summary>

    [Fact]
    public void PropertyRoundTrip_WhenSetAndGet_ShouldMaintainValues()
    {
        // Arrange
        var machineStatus = new MachineStatus();
        const int testMachineId = 9999;
        const int testStatusMachineId = 8888;
        const decimal testBreakDownTime = 42.42m;
        var testUpdatedOn = new DateTime(2025, 12, 31, 23, 59, 59);

        // Act
        machineStatus.MachineId = new MachineId(testMachineId);
        machineStatus.StatusMachineId = testStatusMachineId;
        machineStatus.BreakDownTime = testBreakDownTime;
        machineStatus.UpdatedOn = testUpdatedOn;

        // Assert
        machineStatus.MachineId.Value.ShouldBe(testMachineId);
        machineStatus.StatusMachineId.ShouldBe(testStatusMachineId);
        machineStatus.BreakDownTime.ShouldBe(testBreakDownTime);
        machineStatus.UpdatedOn.ShouldBe(testUpdatedOn);
    }
    /// <summary>
    /// Executes ManufacturingStatusScenarios_WithDifferentOperationalStates_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 101, 0.0, "RUNNING - Normal Operation")]
    [InlineData(1002, 102, 30.5, "STOPPED - Planned Maintenance")]
    [InlineData(1003, 103, 120.0, "BREAKDOWN - Emergency Stop")]
    [InlineData(1004, 104, 45.25, "MAINTENANCE - Scheduled Service")]
    [InlineData(1005, 105, 5.75, "SETUP - Tool Change")]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    // Justification: operationalState parameter provides test context documentation  
    // Approved By: CLAUDE on 27/08/2025
    public void ManufacturingStatusScenarios_WithDifferentOperationalStates_ShouldHandleCorrectly(
        int machineId, int statusMachineId, decimal breakDownTime, string operationalState)
#pragma warning restore xUnit1026
    {
        // Arrange
        var machineStatus = new MachineStatus();
        var timestamp = new DateTime(2025, 1, 20, 14, 0, 0);

        // Act
        machineStatus.MachineId = new MachineId(machineId);
        machineStatus.StatusMachineId = statusMachineId;
        machineStatus.BreakDownTime = breakDownTime;
        machineStatus.UpdatedOn = timestamp;

        // Assert
        machineStatus.MachineId.Value.ShouldBe(machineId);
        machineStatus.StatusMachineId.ShouldBe(statusMachineId);
        machineStatus.BreakDownTime.ShouldBe(breakDownTime);
        machineStatus.UpdatedOn.ShouldBe(timestamp);
        machineStatus.ShouldBeAssignableTo<IEntityRoot>();
    }
    /// <summary>
    /// Executes ProductionLineMonitoring_WithMultipleMachineStatuses_ShouldTrackEffectively operation.
    /// </summary>

    [Fact]
    public void ProductionLineMonitoring_WithMultipleMachineStatuses_ShouldTrackEffectively()
    {
        // Arrange
        var weldingRobot = new MachineStatus
        {
            MachineId = new MachineId(2001),
            StatusMachineId = 201,
            BreakDownTime = 0.0m,
            UpdatedOn = DateTime.Now.AddMinutes(-5)
        };

        var paintingBooth = new MachineStatus
        {
            MachineId = new MachineId(2002),
            StatusMachineId = 202,
            BreakDownTime = 25.5m,
            UpdatedOn = DateTime.Now.AddMinutes(-10)
        };

        var assemblyLine = new MachineStatus
        {
            MachineId = new MachineId(2003),
            StatusMachineId = 203,
            BreakDownTime = 120.75m,
            UpdatedOn = DateTime.Now.AddHours(-2)
        };

        // Act & Assert - Verify production line status tracking
        weldingRobot.MachineId.Value.ShouldBe(2001);
        weldingRobot.BreakDownTime.ShouldBe(0.0m); // Running normally

        paintingBooth.MachineId.Value.ShouldBe(2002);
        paintingBooth.BreakDownTime.ShouldBe(25.5m); // Minor downtime

        assemblyLine.MachineId.Value.ShouldBe(2003);
        assemblyLine.BreakDownTime.ShouldBe(120.75m); // Major breakdown

        // Verify breakdown time progression
        weldingRobot.BreakDownTime.ShouldBeLessThan(paintingBooth.BreakDownTime);
        paintingBooth.BreakDownTime.ShouldBeLessThan(assemblyLine.BreakDownTime);
    }
    /// <summary>
    /// Executes QualityControlIntegration_WithInspectionMachineStatus_ShouldSupportMonitoring operation.
    /// </summary>

    [Fact]
    public void QualityControlIntegration_WithInspectionMachineStatus_ShouldSupportMonitoring()
    {
        // Arrange
        var qualityInspectionStation = new MachineStatus
        {
            MachineId = new MachineId(3001),
            StatusMachineId = 301,
            BreakDownTime = 0.0m,
            UpdatedOn = DateTime.Now
        };

        // Act & Assert - Verify quality control station monitoring
        qualityInspectionStation.MachineId.Value.ShouldBe(3001);
        qualityInspectionStation.StatusMachineId.ShouldBe(301);
        qualityInspectionStation.BreakDownTime.ShouldBe(0.0m);
        qualityInspectionStation.UpdatedOn.ShouldBeInRange(
            DateTime.Now.AddSeconds(-5),
            DateTime.Now.AddSeconds(5));
    }
    /// <summary>
    /// Executes EmergencyStopScenario_WithHighBreakdownTime_ShouldIndicateCriticalIssue operation.
    /// </summary>

    [Fact]
    public void EmergencyStopScenario_WithHighBreakdownTime_ShouldIndicateCriticalIssue()
    {
        // Arrange
        var emergencyStoppedMachine = new MachineStatus();
        const decimal criticalDowntime = 480.0m; // 8 hours emergency stop
        var emergencyTimestamp = new DateTime(2025, 1, 20, 8, 0, 0);

        // Act
        emergencyStoppedMachine.MachineId = new MachineId(4001);
        emergencyStoppedMachine.StatusMachineId = 999; // Emergency status code
        emergencyStoppedMachine.BreakDownTime = criticalDowntime;
        emergencyStoppedMachine.UpdatedOn = emergencyTimestamp;

        // Assert - Verify emergency stop scenario handling
        emergencyStoppedMachine.BreakDownTime.ShouldBe(criticalDowntime);
        emergencyStoppedMachine.BreakDownTime.ShouldBeGreaterThan(240.0m); // > 4 hours = critical
        emergencyStoppedMachine.StatusMachineId.ShouldBe(999);
        emergencyStoppedMachine.UpdatedOn.ShouldBe(emergencyTimestamp);
    }
    /// <summary>
    /// Executes MaintenanceWindow_WithScheduledDowntime_ShouldTrackAccurately operation.
    /// </summary>

    [Fact]
    public void MaintenanceWindow_WithScheduledDowntime_ShouldTrackAccurately()
    {
        // Arrange
        var maintenanceMachine = new MachineStatus();
        const decimal scheduledMaintenanceTime = 60.0m; // 1 hour maintenance
        var maintenanceStart = new DateTime(2025, 1, 20, 18, 0, 0);

        // Act
        maintenanceMachine.MachineId = new MachineId(5001);
        maintenanceMachine.StatusMachineId = 500; // Maintenance status
        maintenanceMachine.BreakDownTime = scheduledMaintenanceTime;
        maintenanceMachine.UpdatedOn = maintenanceStart;

        // Assert - Verify maintenance window tracking
        maintenanceMachine.BreakDownTime.ShouldBe(scheduledMaintenanceTime);
        maintenanceMachine.StatusMachineId.ShouldBe(500);
        maintenanceMachine.UpdatedOn.ShouldBe(maintenanceStart);

        // Verify maintenance time is reasonable (not emergency level)
        maintenanceMachine.BreakDownTime.ShouldBeLessThan(240.0m); // < 4 hours
    }
    /// <summary>
    /// #95 Phase 2.1: MachineStatus is per-machine MUTABLE STATE — an IEntityRoot member of the Machine
    /// aggregate, no longer an ILookupEntity (reclassified in PR #167).
    /// </summary>

    [Fact]
    public void EntityRootMarker_Implementation_ShouldBeCorrect()
    {
        // Arrange & Act
        var machineStatus = new MachineStatus();

        // Assert
        machineStatus.ShouldBeAssignableTo<IEntityRoot>();
        machineStatus.ShouldNotBeAssignableTo<ILookupEntity>();
    }
    /// <summary>
    /// Executes TimestampAccuracy_WhenSetWithPrecision_ShouldMaintainAccuracy operation.
    /// </summary>

    [Fact]
    public void TimestampAccuracy_WhenSetWithPrecision_ShouldMaintainAccuracy()
    {
        // Arrange
        var machineStatus = new MachineStatus();
        var preciseTimestamp = new DateTime(2025, 1, 20, 14, 30, 45, 123);

        // Act
        machineStatus.UpdatedOn = preciseTimestamp;

        // Assert
        machineStatus.UpdatedOn.ShouldBe(preciseTimestamp);
        machineStatus.UpdatedOn.Year.ShouldBe(2025);
        machineStatus.UpdatedOn.Month.ShouldBe(1);
        machineStatus.UpdatedOn.Day.ShouldBe(20);
        machineStatus.UpdatedOn.Hour.ShouldBe(14);
        machineStatus.UpdatedOn.Minute.ShouldBe(30);
        machineStatus.UpdatedOn.Second.ShouldBe(45);
        machineStatus.UpdatedOn.Millisecond.ShouldBe(123);
    }
    /// <summary>
    /// Executes IndustryStandardOEE_WithBreakdownTracking_ShouldSupportCalculations operation.
    /// </summary>

    [Fact]
    public void IndustryStandardOEE_WithBreakdownTracking_ShouldSupportCalculations()
    {
        // Arrange - Create scenarios for OEE calculation support
        var highPerformanceMachine = new MachineStatus
        {
            MachineId = new MachineId(6001),
            StatusMachineId = 10000, // Running status
            BreakDownTime = 0.0m,  // No downtime
            UpdatedOn = DateTime.Now
        };

        var averagePerformanceMachine = new MachineStatus
        {
            MachineId = new MachineId(6002),
            StatusMachineId = 200, // Intermittent status
            BreakDownTime = 30.0m, // 30 minutes downtime
            UpdatedOn = DateTime.Now.AddMinutes(-30)
        };

        // Act & Assert - Verify OEE supporting data structure
        highPerformanceMachine.BreakDownTime.ShouldBe(0.0m);
        averagePerformanceMachine.BreakDownTime.ShouldBe(30.0m);

        // Verify timestamp tracking for availability calculations
        highPerformanceMachine.UpdatedOn.ShouldBeGreaterThan(
            averagePerformanceMachine.UpdatedOn);

        // Verify different performance levels can be tracked
        averagePerformanceMachine.BreakDownTime.ShouldBeGreaterThan(
            highPerformanceMachine.BreakDownTime);
    }
}
