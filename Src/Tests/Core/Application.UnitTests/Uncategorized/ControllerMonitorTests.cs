// <copyright file="ControllerMonitorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Unit tests for ControllerMonitor
/// </summary>
public class ControllerMonitorTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new ControllerMonitor();

        // Assert
        instance.ShouldNotBeNull();
        instance.PartNumber.ShouldBe("");
        instance.Description.ShouldBe(string.Empty);
        instance.Label.ShouldBe("");
        instance.HeartBeat.ShouldBe(0);
        instance.CyclesOk.ShouldBe(0);
        instance.Parameters.ShouldNotBeNull();
        instance.HeartBeatTimer.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNameParameter_ShouldCreateInstanceWithName operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNameParameter_ShouldCreateInstanceWithName()
    {
        // Arrange & Act - Siemens S7-1500 PLC
        var instance = new ControllerMonitor("Siemens S7-1500 PLC");

        // Assert
        instance.ShouldNotBeNull();
        instance.Name.ShouldBe("Siemens S7-1500 PLC");
        instance.HeartBeatTimer.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithPlcIdAndMachineId_ShouldCreateInstanceWithIds operation.
    /// </summary>

    [Fact]
    public void Constructor_WithPlcIdAndMachineId_ShouldCreateInstanceWithIds()
    {
        // Arrange & Act - Ford F-150 Production Line
        var instance = new ControllerMonitor(plcId: 10, machineId: 101);

        // Assert
        instance.ShouldNotBeNull();
        instance.PlcId.ShouldBe(10);
        instance.MachineId.ShouldBe(101);
        instance.HeartBeatTimer.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNameAndTimeStamp_ShouldCreateInstanceWithBoth operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNameAndTimeStamp_ShouldCreateInstanceWithBoth()
    {
        // Arrange
        var timestamp = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local);

        // Act - ABB IRC5 Robot Controller
        var instance = new ControllerMonitor("ABB IRC5 Robot Controller", timestamp);

        // Assert
        instance.ShouldNotBeNull();
        instance.Name.ShouldBe("ABB IRC5 Robot Controller");
        instance.TimeStamp.ShouldBe(timestamp);
        instance.HeartBeatTimer.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new ControllerMonitor();

        // Act - Fanuc Focas CNC Controller
        instance.PlcId = 15;
        instance.MachineId = 201;
        instance.PartNumber = "CNC-Spindle-Control-V2";
        instance.Description = "Fanuc 31i-Model B CNC Control";
        instance.Label = "FANUC-CNC-201";
        instance.HeartBeat = 42;
        instance.CyclesOk = 1250;
        instance.IpAddress = "192.168.1.15";
        instance.Name = "Fanuc 31i-Model B";

        // Assert
        instance.PlcId.ShouldBe(15);
        instance.MachineId.ShouldBe(201);
        instance.PartNumber.ShouldBe("CNC-Spindle-Control-V2");
        instance.Description.ShouldBe("Fanuc 31i-Model B CNC Control");
        instance.Label.ShouldBe("FANUC-CNC-201");
        instance.HeartBeat.ShouldBe(42);
        instance.CyclesOk.ShouldBe(1250);
        instance.IpAddress.ShouldBe("192.168.1.15");
        instance.Name.ShouldBe("Fanuc 31i-Model B");
    }

    /// <summary>
    /// Executes Properties_WithManufacturingScenarios_ShouldReturnCorrectValues operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="machineId">The machineId.</param>
    /// <param name="ipAddress">The ipAddress.</param>
    /// <param name="name">The name.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(10, 101, "192.168.1.10", "Siemens S7-1500", "Ford F-150 welding")]
    [InlineData(20, 201, "192.168.1.20", "Allen-Bradley CompactLogix", "Tesla Model Y assembly")]
    [InlineData(30, 301, "192.168.1.30", "Mitsubishi FX5U", "iPhone PCB inspection")]
    [InlineData(40, 401, "192.168.1.40", "Schneider Modicon M580", "Pfizer vaccine filling")]
    public void Properties_WithManufacturingScenarios_ShouldReturnCorrectValues(int plcId, int machineId, string ipAddress, string name, string scenario)
    {
        // Using parameters: plcId, machineId, ipAddress, name, scenario
        _ = plcId; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, machineId, ipAddress, name, scenario
        _ = plcId; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, machineId, ipAddress, name, scenario
        _ = plcId; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, machineId, ipAddress, name, scenario
        _ = plcId; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, machineId, ipAddress, name, scenario
        _ = plcId; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var instance = new ControllerMonitor();

        // Act
        instance.PlcId = plcId;
        instance.MachineId = machineId;
        instance.IpAddress = ipAddress;
        instance.Name = name;

        // Assert
        instance.PlcId.ShouldBe(plcId);
        instance.MachineId.ShouldBe(machineId);
        instance.IpAddress.ShouldBe(ipAddress);
        instance.Name.ShouldBe(name);
    }

    /// <summary>
    /// Executes Parameters_WhenPopulated_ShouldContainExpectedKeyValuePairs operation.
    /// </summary>

    [Fact]
    public void Parameters_WhenPopulated_ShouldContainExpectedKeyValuePairs()
    {
        // Arrange
        var instance = new ControllerMonitor();

        // Act - Siemens PLC Parameters for Robot Control
        instance.Parameters.Add("RobotSpeed", "75");
        instance.Parameters.Add("SafetyZone", "Active");
        instance.Parameters.Add("ToolOffset", "12.5");
        instance.Parameters.Add("CycleTime", "45.2");

        // Assert
        instance.Parameters.Count.ShouldBe(4);
        instance.Parameters["RobotSpeed"].ShouldBe("75");
        instance.Parameters["SafetyZone"].ShouldBe("Active");
        instance.Parameters["ToolOffset"].ShouldBe("12.5");
        instance.Parameters["CycleTime"].ShouldBe("45.2");
    }

    /// <summary>
    /// Executes TimeStamp_WhenSet_ShouldUpdateConnectionState operation.
    /// </summary>

    [Fact]
    public void TimeStamp_WhenSet_ShouldUpdateConnectionState()
    {
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: [CLUSTER B] - Fix timestamp to be recent for manufacturing monitoring. Use DateTime.Now to ensure proper connection state evaluation
        var mockDateTimeMachine = Substitute.For<IDateTimeMachine>();
        var now = DateTime.Now;
        mockDateTimeMachine.Now.Returns(now);

        var instance = new ControllerMonitor(mockDateTimeMachine);
        var newTimeStamp = now; // Current timestamp

        // Act
        instance.TimeStamp = newTimeStamp;

        // Assert
        instance.TimeStamp.ShouldBe(newTimeStamp);
        instance.IsConnected.ShouldBeTrue();
        instance.IsConnectionAlive.ShouldBeTrue();
    }

    /// <summary>
    /// Executes IsConnected_WhenTimeStampIsRecent_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void IsConnected_WhenTimeStampIsRecent_ShouldReturnTrue()
    {
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: [CLUSTER B] - Fix timestamp to be recent for manufacturing monitoring. Use DateTime.Now instead of fixed 2025-01-01 to ensure it's within heartbeat interval
        var mockDateTimeMachine = Substitute.For<IDateTimeMachine>();
        var now = DateTime.Now;
        mockDateTimeMachine.Now.Returns(now);

        var instance = new ControllerMonitor(mockDateTimeMachine);

        // Act
        instance.TimeStamp = now.AddSeconds(-5); // Recent timestamp (5 seconds ago)

        // Assert
        instance.IsConnected.ShouldBeTrue();
        instance.IsConnectionAlive.ShouldBeTrue();
    }

    /// <summary>
    /// Executes IsConnected_WhenTimeStampIsOld_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void IsConnected_WhenTimeStampIsOld_ShouldReturnFalse()
    {
        // Arrange
        var instance = new ControllerMonitor();

        // Act
        instance.TimeStamp = new DateTime(2025, 1, 1, 9, 59, 45, DateTimeKind.Local); // Old timestamp beyond timeout

        // Assert - After heartbeat timeout
        // Note: Connection state depends on timer processing which cannot be reliably tested with Thread.Sleep
        // This test verifies the timeout logic indirectly through timestamp comparison
        instance.IsConnected.ShouldBeFalse();
    }

    /// <summary>
    /// Executes RefreshConnection_WhenCalled_ShouldUpdateTimestamp operation.
    /// </summary>

    [Fact]
    public void RefreshConnection_WhenCalled_ShouldUpdateTimestamp()
    {
        // Arrange
        var instance = new ControllerMonitor();
        var originalTime = instance.TimeStamp;

        // Act
        instance.RefreshConnection();

        // Assert
        instance.TimeStamp.ShouldBeGreaterThan(originalTime.AddSeconds(-15));
        instance.HeartBeatTimer.Enabled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Equals_WithSamePlcId_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void Equals_WithSamePlcId_ShouldReturnTrue()
    {
        // Arrange
        var controller1 = new ControllerMonitor { PlcId = 10 };
        var controller2 = new ControllerMonitor { PlcId = 10 };

        // Act & Assert
        controller1.Equals(controller2).ShouldBeTrue();
        (controller1 == controller2).ShouldBeFalse(); // Reference equality
    }

    /// <summary>
    /// Executes Equals_WithDifferentPlcId_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void Equals_WithDifferentPlcId_ShouldReturnFalse()
    {
        // Arrange
        var controller1 = new ControllerMonitor { PlcId = 10 };
        var controller2 = new ControllerMonitor { PlcId = 20 };

        // Act & Assert
        controller1.Equals(controller2).ShouldBeFalse();
    }

    /// <summary>
    /// Executes GetHashCode_WithSamePlcId_ShouldReturnSameHash operation.
    /// </summary>

    [Fact]
    public void GetHashCode_WithSamePlcId_ShouldReturnSameHash()
    {
        // Arrange
        var controller1 = new ControllerMonitor { PlcId = 10 };
        var controller2 = new ControllerMonitor { PlcId = 10 };

        // Act & Assert
        controller1.GetHashCode().ShouldBe(controller2.GetHashCode());
    }

    /// <summary>
    /// Executes CompareTo_WithDifferentPlcIds_ShouldReturnCorrectComparison operation.
    /// </summary>

    [Fact]
    public void CompareTo_WithDifferentPlcIds_ShouldReturnCorrectComparison()
    {
        // Arrange
        var controller1 = new ControllerMonitor { PlcId = 10 };
        var controller2 = new ControllerMonitor { PlcId = 20 };

        // Act & Assert
        ((IComparable<ControllerMonitor>)controller1).CompareTo(controller2).ShouldBeLessThan(0);
        ((IComparable<ControllerMonitor>)controller2).CompareTo(controller1).ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes MapTo_WithValidPlcDto_ShouldReturnCorrectControllerMonitor operation.
    /// </summary>

    [Fact]
    public void MapTo_WithValidPlcDto_ShouldReturnCorrectControllerMonitor()
    {
        // Arrange - Rockwell Automation PLC
        var plcDto = new PlcDto
        {
            PlcId = 25,
            MachineId = 10005,
            Name = "Rockwell ControlLogix 5580",
            IpAddress = "192.168.1.25",
            PlcType = "ControlLogix L85E"
        };

        // Act
        var resultWrapper = ControllerMonitor.MapTo(plcDto);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.PlcId.ShouldBe(25);
        result.MachineId.ShouldBe(10005);
        result.Name.ShouldBe("Rockwell ControlLogix 5580");
        result.IpAddress.ShouldBe("192.168.1.25");
        result.Description.ShouldBe("ControlLogix L85E");
    }

    /// <summary>
    /// Executes MapTo_WithNullPlcDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void MapTo_WithNullPlcDto_ShouldReturnFailureResult()
    {
        // Arrange
        PlcDto nullDto = null!;

        // Act
        var result = ControllerMonitor.MapTo(nullDto);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("PlcDto source cannot be null");
    }

    /// <summary>
    /// Executes ToEntity_WithValidControllerMonitor_ShouldReturnCorrectPlcDto operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidControllerMonitor_ShouldReturnCorrectPlcDto()
    {
        // Arrange - Schneider Electric PLC
        var controller = new ControllerMonitor
        {
            PlcId = 35,
            MachineId = 205,
            Name = "Schneider Modicon M580",
            IpAddress = "192.168.1.35",
            Description = "M580 CPU 2020"
        };

        // Act
        var resultWrapper = ControllerMonitor.ToEntity(controller);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.PlcId.ShouldBe(35);
        result.MachineId.ShouldBe(205);
        result.Name.ShouldBe("Schneider Modicon M580");
        result.IpAddress.ShouldBe("192.168.1.35");
        result.PlcType.ShouldBe("M580 CPU 2020");
    }

    /// <summary>
    /// Executes ToEntity_WithNullControllerMonitor_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullControllerMonitor_ShouldReturnFailureResult()
    {
        // Arrange
        ControllerMonitor nullController = null!;

        // Act
        var result = ControllerMonitor.ToEntity(nullController);

        // Assert - method returns failure result instead of throwing
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("ControllerMonitor source cannot be null");
    }

    /// <summary>
    /// Executes UpdateReceived_Event_ShouldFireWhenTimeStampIsUpdated operation.
    /// </summary>

    [Fact]
    public void UpdateReceived_Event_ShouldFireWhenTimeStampIsUpdated()
    {
        // Arrange
        var instance = new ControllerMonitor();
        var eventFired = false;
        instance.UpdateReceived += (sender, args) => eventFired = true;

        // Act
        instance.TimeStamp = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local);

        // Assert
        eventFired.ShouldBeTrue();
    }

    /// <summary>
    /// Executes ToString_WhenCalled_ShouldReturnDescriptiveString operation.
    /// </summary>

    [Fact]
    public void ToString_WhenCalled_ShouldReturnDescriptiveString()
    {
        // Arrange
        var instance = new ControllerMonitor
        {
            PlcId = 10,
            MachineId = 10001,
            IpAddress = "192.168.1.10",
            Name = "Test PLC"
        };

        // Act
        var result = instance.ToString();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("PlcId: 10");
        result.ShouldContain("MachineId: 10001");
        result.ShouldContain("IpAddress: 192.168.1.10");
    }

    /// <summary>
    /// Executes HeartBeatTimer_ShouldBeConfiguredCorrectly operation.
    /// </summary>

    [Fact]
    public void HeartBeatTimer_ShouldBeConfiguredCorrectly()
    {
        // Arrange & Act
        var instance = new ControllerMonitor();

        // Assert
        instance.HeartBeatTimer.ShouldNotBeNull();
        instance.HeartBeatTimer.AutoReset.ShouldBeFalse();
        instance.HeartBeatTimer.Interval.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes Instance_WithIndustrialScenario_ShouldMaintainCorrectState operation.
    /// </summary>

    [Fact]
    public void Instance_WithIndustrialScenario_ShouldMaintainCorrectState()
    {
        // Arrange & Act - Complete Coca-Cola Bottling Line Controller
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: [CLUSTER B] - Fix timestamp to be recent for manufacturing monitoring. Use DateTime.Now to ensure IsConnected works for industrial scenarios
        var mockDateTimeMachine = Substitute.For<IDateTimeMachine>();
        var now = DateTime.Now;
        mockDateTimeMachine.Now.Returns(now);

        var instance = new ControllerMonitor(mockDateTimeMachine)
        {
            PlcId = 401,
            MachineId = 401,
            Name = "Bottling Line Controller",
            IpAddress = "192.168.10.401",
            Description = "Coca-Cola Bottling Line #1 Main Controller",
            Label = "CC-ATL-LINE1-PLC",
            PartNumber = "CC-Bottle-Control-V3.2",
            HeartBeat = 156,
            CyclesOk = 12450,
            TimeStamp = now // Use current timestamp for proper connection evaluation
        };

        instance.Parameters.Add("BottlesPM", "18000");
        instance.Parameters.Add("FillLevel", "500ml");
        instance.Parameters.Add("QualityCheck", "Passed");
        instance.Parameters.Add("LineSpeed", "85%");

        // Assert
        instance.PlcId.ShouldBe(401);
        instance.MachineId.ShouldBe(401);
        instance.Name.ShouldBe("Bottling Line Controller");
        instance.IpAddress.ShouldBe("192.168.10.401");
        instance.Description.ShouldBe("Coca-Cola Bottling Line #1 Main Controller");
        instance.Label.ShouldBe("CC-ATL-LINE1-PLC");
        instance.PartNumber.ShouldBe("CC-Bottle-Control-V3.2");
        instance.HeartBeat.ShouldBe(156);
        instance.CyclesOk.ShouldBe(12450);
        instance.Parameters["BottlesPM"].ShouldBe("18000");
        instance.Parameters["LineSpeed"].ShouldBe("85%");
        instance.IsConnected.ShouldBeTrue();
    }

    // ----- #120 F5: ConnectionRecovered semantics (dead -> alive only, raised outside the lock) -----

    /// <summary>
    /// #120 F5: the very first heartbeat transitions the monitor from its initial dead state to
    /// alive, so ConnectionRecovered fires exactly once.
    /// </summary>
    [Fact]
    public void TimeStamp_OnFirstHeartbeat_ShouldRaiseConnectionRecoveredOnce()
    {
        // Arrange: deterministic clock
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var current = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);
        dateTimeMachine.Now.Returns(_ => current);

        var instance = new ControllerMonitor(dateTimeMachine);
        var recoveredCount = 0;
        instance.ConnectionRecovered += (_, _) => recoveredCount++;

        // Act: first heartbeat (initial state is not-connected)
        instance.TimeStamp = current;

        // Assert
        recoveredCount.ShouldBe(1);
    }

    /// <summary>
    /// #120 F5: a routine heartbeat while the connection is already alive must NOT raise
    /// ConnectionRecovered (pre-fix it fired on EVERY heartbeat — inverted semantics).
    /// </summary>
    [Fact]
    public void TimeStamp_OnRoutineHeartbeatWhileAlive_ShouldNotRaiseConnectionRecovered()
    {
        // Arrange: deterministic clock
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var current = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);
        dateTimeMachine.Now.Returns(_ => current);

        var instance = new ControllerMonitor(dateTimeMachine);
        var recoveredCount = 0;
        instance.ConnectionRecovered += (_, _) => recoveredCount++;

        instance.TimeStamp = current; // first heartbeat: dead -> alive, fires once

        // Act: routine heartbeats while the connection is alive
        current = current.AddSeconds(5);
        instance.TimeStamp = current;
        current = current.AddSeconds(5);
        instance.TimeStamp = current;

        // Assert: still exactly one recovery event (pre-fix: one per heartbeat)
        recoveredCount.ShouldBe(1);
    }

    /// <summary>
    /// #120 F5: after the heartbeat window elapses (connection dead), the next heartbeat is a
    /// genuine dead -> alive transition and must raise ConnectionRecovered (pre-fix it did NOT,
    /// because the condition was inverted). The handler must also observe the ALREADY-updated
    /// connected state, proving the event is raised after mutation and outside the lock.
    /// </summary>
    [Fact]
    public void TimeStamp_AfterConnectionDied_ShouldRaiseConnectionRecoveredOnNextHeartbeat()
    {
        // Arrange: deterministic clock
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var current = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);
        dateTimeMachine.Now.Returns(_ => current);

        var instance = new ControllerMonitor(dateTimeMachine);
        var recoveredCount = 0;
        var connectedDuringEvent = false;
        instance.ConnectionRecovered += (_, _) =>
        {
            recoveredCount++;
            connectedDuringEvent = instance.IsConnected; // safe: raised outside the lock, after mutation
        };

        instance.TimeStamp = current; // first heartbeat: dead -> alive (count = 1)

        // Act: silence for 30s (heartbeat window is 12s) -> connection is dead, then a heartbeat arrives
        current = current.AddSeconds(30);
        instance.TimeStamp = current;

        // Assert: genuine recovery detected
        recoveredCount.ShouldBe(2);
        connectedDuringEvent.ShouldBeTrue();
    }

    // ----- #120 F2: IDisposable heartbeat-timer lifecycle -----

    /// <summary>
    /// #120 F2: Dispose must stop and dispose the heartbeat timer so a replaced monitor can never
    /// fire a stale ConnectionLost event.
    /// </summary>
    [Fact]
    public void Dispose_WhenCalled_ShouldStopAndDisposeHeartBeatTimer()
    {
        // Arrange: arm the timer via a heartbeat
        var instance = new ControllerMonitor();
        instance.TimeStamp = DateTime.Now;
        instance.HeartBeatTimer.Enabled.ShouldBeTrue(); // sanity: armed

        // Act
        instance.Dispose();

        // Assert: deterministic disposal evidence, no wall-clock waits
        instance.HeartBeatTimer.Enabled.ShouldBeFalse();
        Should.Throw<ObjectDisposedException>(() => instance.HeartBeatTimer.Start());
    }

    /// <summary>
    /// #120 F2: Dispose must be idempotent (the events service may race a replacement with a
    /// shutdown path).
    /// </summary>
    [Fact]
    public void Dispose_WhenCalledTwice_ShouldBeIdempotent()
    {
        // Arrange
        var instance = new ControllerMonitor();

        // Act & Assert
        Should.NotThrow(() =>
        {
            instance.Dispose();
            instance.Dispose();
        });
    }
}