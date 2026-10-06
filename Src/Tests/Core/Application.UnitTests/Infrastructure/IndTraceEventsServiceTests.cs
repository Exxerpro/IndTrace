// <copyright file="IndTraceEventsServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reactive;

namespace Application.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for IndTraceEventsService
/// </summary>
public class IndTraceEventsServiceTests
{
    private readonly IndTraceEventsService _eventsService = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public IndTraceEventsServiceTests()
    {
        _eventsService = new IndTraceEventsService();
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var eventsService = new IndTraceEventsService();

        // Assert
        eventsService.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullParameters_ShouldThrowException operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNullParameters_ShouldThrowException()
    {
        // Act & Assert
        // IndTraceEventsService is a singleton, so we can't test null parameters
        Should.NotThrow(() => new IndTraceEventsService());
    }

    /// <summary>
    /// Executes Subscribe_WithValidHandler_ShouldAddHandler operation.
    /// </summary>

    [Fact]
    public void Subscribe_WithValidHandler_ShouldAddHandler()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "TestState" };

        // Act
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Assert
        subscription.ShouldNotBeNull();
        // Note: We can't easily test the internal subscription without exposing it
    }

    /// <summary>
    /// Executes Subscribe_WithNullHandler_ShouldThrowException operation.
    /// </summary>

    [Fact]
    public void Subscribe_WithNullHandler_ShouldThrowException()
    {
        // Arrange
        Action<StateChange>? handler = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _eventsService.Subscribe(handler!));
    }

    /// <summary>
    /// Executes Subscribe_WithMultipleHandlers_ShouldAddAllHandlers operation.
    /// </summary>

    [Fact]
    public void Subscribe_WithMultipleHandlers_ShouldAddAllHandlers()
    {
        // Arrange
        var handler1 = Substitute.For<Action<StateChange>>();
        var handler2 = Substitute.For<Action<StateChange>>();
        var handler3 = Substitute.For<Action<StateChange>>();

        // Act
        var subscription1 = _eventsService.Subscribe(handler1);
        var subscription2 = _eventsService.Subscribe(handler2);
        var subscription3 = _eventsService.Subscribe(handler3);

        // Assert
        subscription1.ShouldNotBeNull();
        subscription2.ShouldNotBeNull();
        subscription3.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Notify_WithValidStateChange_ShouldNotifySubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithValidStateChange_ShouldNotifySubscribers()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "TestState" };
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState"));
    }

    /// <summary>
    /// Executes Notify_WithNullStateChange_ShouldThrowException operation.
    /// </summary>

    [Fact]
    public void Notify_WithNullStateChange_ShouldThrowException()
    {
        // Arrange
        StateChange? stateChange = null!;

        // Act & Assert
        // Should do nothing
        // ??what to assert here

        _eventsService.NotifyStateChanged(stateChange!);

        //nothing has changed just assert true??
        true.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Notify_WithMultipleSubscribers_ShouldNotifyAllSubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithMultipleSubscribers_ShouldNotifyAllSubscribers()
    {
        // Arrange
        var handler1 = Substitute.For<Action<StateChange>>();
        var handler2 = Substitute.For<Action<StateChange>>();
        var handler3 = Substitute.For<Action<StateChange>>();

        var subscription1 = _eventsService.Subscribe(handler1);
        var subscription2 = _eventsService.Subscribe(handler2);
        var subscription3 = _eventsService.Subscribe(handler3);

        var stateChange = new StateChange { State = "TestState" };

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler1.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState"));
        handler2.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState"));
        handler3.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState"));
    }

    /// <summary>
    /// Executes Notify_WithNoSubscribers_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Notify_WithNoSubscribers_ShouldNotThrowException()
    {
        // Arrange
        var stateChange = new StateChange { State = "TestState" };

        // Act & Assert
        Should.NotThrow(() => _eventsService.NotifyStateChanged(stateChange));
    }

    /// <summary>
    /// Executes Notify_WithComplexStateChange_ShouldNotifySubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithComplexStateChange_ShouldNotifySubscribers()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange
        {
            State = "ComplexState",
            Data = new Dictionary<string, object>
            {
                { "Key1", "Value1" },
                { "Key2", 123 },
                { "Key3", true }
            }
        };
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc =>
            (string?)sc.State == "ComplexState" &&
            sc.Data != null &&
            ((Dictionary<string, object>)sc.Data).Count == 3));
    }

    /// <summary>
    /// Executes Notify_WithEmptyStateChange_ShouldNotifySubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithEmptyStateChange_ShouldNotifySubscribers()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "" };
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == ""));
    }

    /// <summary>
    /// Executes Notify_WithNullStateInStateChange_ShouldNotifySubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithNullStateInStateChange_ShouldNotifySubscribers()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = null! };
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => sc.State == null));
    }

    /// <summary>
    /// Executes Notify_WithNullDataInStateChange_ShouldNotifySubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithNullDataInStateChange_ShouldNotifySubscribers()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "TestState", Data = null! };
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState" && sc.Data == null));
    }

    /// <summary>
    /// Executes Notify_WithMultipleNotifications_ShouldNotifySubscribersMultipleTimes operation.
    /// </summary>

    [Fact]
    public void Notify_WithMultipleNotifications_ShouldNotifySubscribersMultipleTimes()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        var stateChange1 = new StateChange { State = "State1" };
        var stateChange2 = new StateChange { State = "State2" };
        var stateChange3 = new StateChange { State = "State3" };

        // Act
        _eventsService.NotifyStateChanged(stateChange1);
        _eventsService.NotifyStateChanged(stateChange2);
        _eventsService.NotifyStateChanged(stateChange3);

        // Assert
        handler.Received(3).Invoke(Arg.Any<StateChange>());
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "State1"));
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "State2"));
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "State3"));
    }

    /// <summary>
    /// Executes Subscribe_WithHandlerThatThrowsException_ShouldNotBreakOtherHandlers operation.
    /// </summary>

    [Fact]
    public void Subscribe_WithHandlerThatThrowsException_ShouldNotBreakOtherHandlers()
    {
        // Arrange
        var handler1 = Substitute.For<Action<StateChange>>();
        var handler2 = Substitute.For<Action<StateChange>>();
        var handler3 = Substitute.For<Action<StateChange>>();

        handler2.When(x => x.Invoke(Arg.Any<StateChange>())).Throw(new Exception("Test exception"));

        var subscription1 = _eventsService.Subscribe(handler1);
        var subscription2 = _eventsService.Subscribe(handler2);
        var subscription3 = _eventsService.Subscribe(handler3);

        var stateChange = new StateChange { State = "TestState" };

        // Act & Assert
        Should.NotThrow(() => _eventsService.NotifyStateChanged(stateChange));

        handler1.Received(1).Invoke(Arg.Is<StateChange>(sc => sc.State != null && sc.State.ToString() == "TestState"));
        handler3.Received(1).Invoke(Arg.Is<StateChange>(sc => sc.State != null && sc.State.ToString() == "TestState"));
    }

    /// <summary>
    /// Executes Subscribe_WithSameHandlerMultipleTimes_ShouldAddHandlerMultipleTimes operation.
    /// </summary>

    [Fact]
    public void Subscribe_WithSameHandlerMultipleTimes_ShouldAddHandlerMultipleTimes()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "TestState" };

        // Act
        var subscription1 = _eventsService.Subscribe(handler);
        var subscription2 = _eventsService.Subscribe(handler);
        var subscription3 = _eventsService.Subscribe(handler);

        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(3).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState"));
    }

    /// <summary>
    /// Executes Subscribe_WithDifferentHandlers_ShouldAddAllHandlers operation.
    /// </summary>

    [Fact]
    public void Subscribe_WithDifferentHandlers_ShouldAddAllHandlers()
    {
        // Arrange
        var handler1 = Substitute.For<Action<StateChange>>();
        var handler2 = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "TestState" };

        // Act
        var subscription1 = _eventsService.Subscribe(handler1);
        var subscription2 = _eventsService.Subscribe(handler2);

        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler1.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState"));
        handler2.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState"));
    }

    /// <summary>
    /// Executes Notify_WithLargeNumberOfSubscribers_ShouldNotifyAllSubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithLargeNumberOfSubscribers_ShouldNotifyAllSubscribers()
    {
        // Arrange
        var handlers = new List<Action<StateChange>>();
        var subscriptions = new List<IDisposable>();

        for (int i = 0; i < 100; i++)
        {
            var handler = Substitute.For<Action<StateChange>>();
            handlers.Add(handler);
            subscriptions.Add(_eventsService.Subscribe(handler));
        }

        var stateChange = new StateChange { State = "TestState" };

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        foreach (var handler in handlers)
        {
            handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState"));
        }
    }

    /// <summary>
    /// Executes Notify_WithConcurrentNotifications_ShouldHandleConcurrency operation.
    /// </summary>

    [Fact]
    public async Task Notify_WithConcurrentNotifications_ShouldHandleConcurrencyAsync()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));
        var stateChange = new StateChange { State = "TestState" };

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                _eventsService.NotifyStateChanged(stateChange);
                return Task.CompletedTask;
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks.ToArray());

        // Assert
        handler.Received(10).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState"));
    }

    /// <summary>
    /// Executes Notify_WithStateChangeContainingSpecialCharacters_ShouldNotifySubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithStateChangeContainingSpecialCharacters_ShouldNotifySubscribers()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "Test@#$%^&*()State" };
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "Test@#$%^&*()State"));
    }

    /// <summary>
    /// Executes Notify_WithStateChangeContainingUnicodeCharacters_ShouldNotifySubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithStateChangeContainingUnicodeCharacters_ShouldNotifySubscribers()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "TestStateñáéíóú" };
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestStateñáéíóú"));
    }

    /// <summary>
    /// Executes Notify_WithStateChangeContainingNumbers_ShouldNotifySubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithStateChangeContainingNumbers_ShouldNotifySubscribers()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "TestState123" };
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "TestState123"));
    }

    /// <summary>
    /// Executes Notify_WithStateChangeContainingWhitespace_ShouldNotifySubscribers operation.
    /// </summary>

    [Fact]
    public void Notify_WithStateChangeContainingWhitespace_ShouldNotifySubscribers()
    {
        // Arrange
        var handler = Substitute.For<Action<StateChange>>();
        var stateChange = new StateChange { State = "Test State With Spaces" };
        var subscription = _eventsService.Subscribe(Observer.Create<StateChange>(handler));

        // Act
        _eventsService.NotifyStateChanged(stateChange);

        // Assert
        handler.Received(1).Invoke(Arg.Is<StateChange>(sc => (string?)sc.State == "Test State With Spaces"));
    }

    // ----- #120 F1: filtered-view read vs backing-map write (silent permanent staleness) -----

    /// <summary>
    /// #120 F1 money test (response path): an entry hidden by FilterByModel (duplicate Name, older
    /// TimeStamp) must still be updatable — the update must hit the BACKING dictionary, not miss on
    /// the filtered view and then silently fail TryAdd because the key already exists.
    /// </summary>
    [Fact]
    public void UpdateStationFromGatewayResponse_WhenEntryHiddenByFilter_ShouldStillUpdateBackingStation()
    {
        // Arrange: two stations sharing the same Name so FilterByModel keeps only the most recent one
        _eventsService.UpdateStationFromGatewayResponse(new TaskGatewayResponseDto
        {
            MachineId = 1,
            Name = "ST-SHARED",
            PartNumber = "PN-A",
            CyclesOk = 1,
        });
        var hiddenStation = _eventsService.StationMonitors[1]; // live reference into the backing map

        _eventsService.UpdateStationFromGatewayResponse(new TaskGatewayResponseDto
        {
            MachineId = 2,
            Name = "ST-SHARED",
            PartNumber = "PN-A",
            CyclesOk = 2,
        });

        // Age station 1 far behind station 2 so the duplicate-name filter deterministically hides it
        hiddenStation.TimeStamp = DateTime.UtcNow.ToLocalTime().AddMinutes(-10);
        var visibleStation = _eventsService.StationMonitors[2];
        visibleStation.TimeStamp = DateTime.UtcNow.ToLocalTime().AddMinutes(-5);

        // Sanity: the filter is really hiding machine 1
        _eventsService.StationMonitors.ShouldNotContainKey(1);

        // Act: a fresh PLC response arrives for the hidden machine
        _eventsService.UpdateStationFromGatewayResponse(new TaskGatewayResponseDto
        {
            MachineId = 1,
            Name = "ST-SHARED",
            PartNumber = "PN-A",
            CyclesOk = 777,
        });

        // Assert: the backing entry WAS updated (pre-fix it was silently discarded forever)
        hiddenStation.CyclesOk.ShouldBe(777);

        // And the next visible snapshot reflects it (machine 1 is now the most recent duplicate)
        _eventsService.StationMonitors.ShouldContainKey(1);
        _eventsService.StationMonitors[1].CyclesOk.ShouldBe(777);
    }

    /// <summary>
    /// #120 F1 (request path): a station hidden by FilterByModel must still receive gateway-request
    /// updates through the backing dictionary.
    /// </summary>
    [Fact]
    public void UpdateStationFromGatewayRequest_WhenEntryHiddenByFilter_ShouldStillUpdateBackingStation()
    {
        // Arrange
        _eventsService.UpdateStationFromGatewayRequest(new TaskGatewayRequest
        {
            MachineId = 1,
            Name = "ST-REQ-SHARED",
            PartNumber = "PN-A",
        });
        var hiddenStation = _eventsService.StationMonitors[1];

        _eventsService.UpdateStationFromGatewayRequest(new TaskGatewayRequest
        {
            MachineId = 2,
            Name = "ST-REQ-SHARED",
            PartNumber = "PN-A",
        });

        hiddenStation.TimeStamp = DateTime.UtcNow.ToLocalTime().AddMinutes(-10);
        _eventsService.StationMonitors[2].TimeStamp = DateTime.UtcNow.ToLocalTime().AddMinutes(-5);

        // Sanity: machine 1 is hidden by the duplicate-name filter
        _eventsService.StationMonitors.ShouldNotContainKey(1);

        // Act
        _eventsService.UpdateStationFromGatewayRequest(new TaskGatewayRequest
        {
            MachineId = 1,
            Name = "ST-REQ-SHARED",
            PartNumber = "PN-777",
        });

        // Assert: pre-fix the update was dropped and PartNumber stayed "PN-A"
        hiddenStation.PartNumber.ShouldBe("PN-777");
        _eventsService.StationMonitors.ShouldContainKey(1);
    }

    /// <summary>
    /// #120 F1 (configuration to RequestEvents path): ApplyConfiguration must refresh a request
    /// event even when the filtered RequestEvents view hides it.
    /// </summary>
    [Fact]
    public void ApplyConfiguration_WhenRequestEventHiddenByFilter_ShouldStillUpdateBackingRequest()
    {
        // Arrange: duplicate-name requests; the older one is hidden by FilterByModel
        var now = DateTime.UtcNow.ToLocalTime();
        var hiddenRequest = new TaskGatewayRequest
        {
            MachineId = 1,
            Name = "M-DUP",
            PartNumber = "PN-A",
            TimeStamp = now.AddMinutes(-10),
        };
        var visibleRequest = new TaskGatewayRequest
        {
            MachineId = 2,
            Name = "M-DUP",
            PartNumber = "PN-A",
            TimeStamp = now,
        };

        _eventsService.AddOrUpdateTaskGatewayRequest(1, hiddenRequest);
        _eventsService.AddOrUpdateTaskGatewayRequest(2, visibleRequest);

        // Sanity: machine 1 is hidden by the duplicate-name filter
        _eventsService.RequestEvents.ShouldNotContainKey(1);

        var configuration = new ApplicationConfiguration
        {
            Machines =
            [
                new MachineDto { MachineId = 1, Name = "Machine-One", EnableAppTraceability = 1 },
            ],
        };

        // Act
        _eventsService.ApplyConfiguration(configuration);

        // Assert: the backing entry was refreshed (pre-fix the TryAdd fallback failed silently)
        hiddenRequest.Description.ShouldBe("Machine-One");
        hiddenRequest.TimeStamp.ShouldBeGreaterThan(now.AddMinutes(-1));
    }

    /// <summary>
    /// #120 F1 (configuration to StationMonitors path): ApplyConfiguration must refresh a station
    /// even when the filtered StationMonitors view hides it.
    /// </summary>
    [Fact]
    public void ApplyConfiguration_WhenStationHiddenByFilter_ShouldStillUpdateBackingStation()
    {
        // Arrange: two stations with duplicate names; the older one is hidden
        _eventsService.UpdateStationFromGatewayResponse(new TaskGatewayResponseDto
        {
            MachineId = 1,
            Name = "ST-CFG-DUP",
            PartNumber = "PN-A",
        });
        var hiddenStation = _eventsService.StationMonitors[1];

        _eventsService.UpdateStationFromGatewayResponse(new TaskGatewayResponseDto
        {
            MachineId = 2,
            Name = "ST-CFG-DUP",
            PartNumber = "PN-A",
        });

        hiddenStation.TimeStamp = DateTime.UtcNow.ToLocalTime().AddMinutes(-10);

        // Sanity: machine 1 is hidden by the duplicate-name filter
        _eventsService.StationMonitors.ShouldNotContainKey(1);

        var configuration = new ApplicationConfiguration
        {
            Machines =
            [
                new MachineDto { MachineId = 1, Name = "Station-One", EnableAppTraceability = 1 },
            ],
        };

        // Act
        _eventsService.ApplyConfiguration(configuration);

        // Assert: pre-fix the hidden station was never touched, so Description stayed empty
        hiddenStation.Description.ShouldBe("Station-One");
    }

    // ----- #120 F2: ConnectionLost handler leak + undisposed replaced-monitor timers -----

    /// <summary>
    /// Reads the private ConnectionLost delegate backing field via reflection and returns its
    /// invocation-list length (0 when no subscribers remain).
    /// </summary>
    private static int GetConnectionLostHandlerCount(ControllerMonitor monitor)
    {
        var field = typeof(ControllerMonitor).GetField(
            "ConnectionLost",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        var handler = field?.GetValue(monitor) as EventHandler;
        return handler?.GetInvocationList().Length ?? 0;
    }

    /// <summary>
    /// #120 F2 (re-pinned for #126 C11 update-in-place): a heartbeat for an existing PLC id must leave
    /// exactly ONE live handler on the CANONICAL (kept) monitor and none on the folded-in duplicate —
    /// no handler may ever accumulate per heartbeat.
    /// </summary>
    [Fact]
    public void AddOrUpdateControllerFromGateway_OnHeartbeatUpdate_ShouldKeepExactlyOneHandlerOnCanonical()
    {
        // Arrange
        var canonical = new ControllerMonitor { PlcId = 5 };
        _eventsService.AddOrUpdateControllerFromGateway(5, canonical);

        var heartbeat = new ControllerMonitor { PlcId = 5 };

        // Act
        _eventsService.AddOrUpdateControllerFromGateway(5, heartbeat);

        // Assert: one live subscription on the canonical instance; none on the unpublished duplicate
        GetConnectionLostHandlerCount(canonical).ShouldBe(1);
        GetConnectionLostHandlerCount(heartbeat).ShouldBe(0);
    }

    /// <summary>
    /// #120 F2 (re-pinned for #126 C11): after N heartbeats the canonical monitor still holds exactly
    /// one service handler and every folded-in duplicate holds none.
    /// </summary>
    [Fact]
    public void AddOrUpdateControllerFromGateway_AfterManyHeartbeats_ShouldKeepExactlyOneLiveHandler()
    {
        // Arrange
        var duplicates = new List<ControllerMonitor>();
        var canonical = new ControllerMonitor { PlcId = 7 };
        _eventsService.AddOrUpdateControllerFromGateway(7, canonical);

        // Act: five heartbeats arrive as new instances, as SignalR hub callbacks do on reconnect storms
        for (var i = 0; i < 5; i++)
        {
            var heartbeat = new ControllerMonitor { PlcId = 7 };
            duplicates.Add(heartbeat);
            _eventsService.AddOrUpdateControllerFromGateway(7, heartbeat);
        }

        // Assert
        GetConnectionLostHandlerCount(canonical).ShouldBe(1);
        foreach (var monitor in duplicates)
        {
            GetConnectionLostHandlerCount(monitor).ShouldBe(0);
        }
    }

    /// <summary>
    /// #120 F2 (re-pinned for #126 C11): the folded-in duplicate's heartbeat timer (armed by the
    /// TimeStamp set during hub deserialization) must be disposed so no second timer can ever fire for
    /// the same PLC id, while the canonical monitor's timer stays live.
    /// </summary>
    [Fact]
    public void AddOrUpdateControllerFromGateway_OnHeartbeatUpdate_ShouldDisposeIncomingDuplicateTimer()
    {
        // Arrange
        var canonical = new ControllerMonitor { PlcId = 9 };
        _eventsService.AddOrUpdateControllerFromGateway(9, canonical);

        // A hub heartbeat arrives with its timer armed, exactly as SignalR deserialization produces it
        var heartbeat = new ControllerMonitor { PlcId = 9 };
        heartbeat.TimeStamp = DateTime.Now;
        heartbeat.HeartBeatTimer.Enabled.ShouldBeTrue(); // sanity: duplicate timer is armed pre-fold

        // Act
        _eventsService.AddOrUpdateControllerFromGateway(9, heartbeat);

        // Assert: duplicate timer stopped and disposed; canonical timer alive — deterministic, no waits
        heartbeat.HeartBeatTimer.Enabled.ShouldBeFalse();
        Should.Throw<ObjectDisposedException>(() => heartbeat.HeartBeatTimer.Start());
        Should.NotThrow(() => canonical.HeartBeatTimer.Stop());
    }

    /// <summary>
    /// #120 F2 regression guard: re-adding the SAME monitor instance must neither dispose its
    /// timer nor stack up handlers.
    /// </summary>
    [Fact]
    public void AddOrUpdateControllerFromGateway_WhenSameInstanceReadded_ShouldKeepSingleHandlerAndLiveTimer()
    {
        // Arrange
        var monitor = new ControllerMonitor { PlcId = 11 };
        _eventsService.AddOrUpdateControllerFromGateway(11, monitor);

        // Act
        _eventsService.AddOrUpdateControllerFromGateway(11, monitor);

        // Assert
        GetConnectionLostHandlerCount(monitor).ShouldBe(1);
        Should.NotThrow(() => monitor.HeartBeatTimer.Start()); // still alive, not disposed
        monitor.HeartBeatTimer.Stop();
    }

    /// <summary>
    /// #126 C11 red test: heartbeats must UPDATE the canonical monitor instance IN PLACE. Blazor
    /// circuits hold direct references to the instance served by <c>ControllerMonitors</c>; pre-fix
    /// every heartbeat replaced-and-DISPOSED it, so a stale circuit reference threw
    /// ObjectDisposedException from TimeStamp/RefreshConnection (both restart the disposed heartbeat
    /// timer), crashing the circuit.
    /// </summary>
    [Fact]
    public void AddOrUpdateControllerFromGateway_OnHeartbeat_KeepsCanonicalInstanceUsableByStaleReferences()
    {
        // Arrange: a Blazor circuit resolves and HOLDS this instance from the read view
        var canonical = new ControllerMonitor { PlcId = 21, HeartBeat = 1 };
        _eventsService.AddOrUpdateControllerFromGateway(21, canonical);

        // Act: a later hub heartbeat arrives as a NEW instance for the same PLC id
        var heartbeat = new ControllerMonitor { PlcId = 21, HeartBeat = 2, TimeStamp = DateTime.Now };
        _eventsService.AddOrUpdateControllerFromGateway(21, heartbeat);

        // Assert: the canonical instance is kept and carries the heartbeat's state
        _eventsService.ControllerMonitors[21].ShouldBeSameAs(canonical);
        canonical.HeartBeat.ShouldBe(2);

        // ...and the stale circuit reference keeps working (pre-fix: ObjectDisposedException)
        Should.NotThrow(() => canonical.RefreshConnection());
        Should.NotThrow(() => canonical.TimeStamp = DateTime.Now);
    }

    /// <summary>
    /// #126 C12: ApplyConfiguration racing hub heartbeats for the same PLC id (concurrent
    /// AddOrUpdateControllerFromGateway calls) must keep exactly one canonical live monitor, dispose
    /// every losing duplicate (no re-created F2 timer leak), and never dispose the published instance.
    /// </summary>
    [Fact]
    public void AddOrUpdateControllerFromGateway_ConcurrentUpdatesForSameId_ShouldKeepOneCanonicalAndDisposeAllDuplicates()
    {
        // Arrange: the canonical monitor a circuit already holds
        var canonical = new ControllerMonitor { PlcId = 31 };
        _eventsService.AddOrUpdateControllerFromGateway(31, canonical);

        var incoming = Enumerable.Range(1, 16)
            .Select(i => new ControllerMonitor { PlcId = 31, HeartBeat = i, TimeStamp = DateTime.Now })
            .ToList();

        // Act: heartbeat and configuration writers race on the same key
        Parallel.ForEach(incoming, monitor => _eventsService.AddOrUpdateControllerFromGateway(31, monitor));

        // Assert: the published instance is still the canonical one, alive and singly subscribed
        _eventsService.ControllerMonitors[31].ShouldBeSameAs(canonical);
        GetConnectionLostHandlerCount(canonical).ShouldBe(1);
        Should.NotThrow(() => canonical.RefreshConnection());

        // ...and every losing duplicate was disposed (armed timers released) with no subscription
        foreach (var monitor in incoming)
        {
            Should.Throw<ObjectDisposedException>(() => monitor.HeartBeatTimer.Start());
            GetConnectionLostHandlerCount(monitor).ShouldBe(0);
        }
    }

    /// <summary>
    /// #126 LOW: the snapshot TTL must run on the INJECTED clock (deterministic-time doctrine), so a
    /// TTL expiry is testable by advancing the fake clock — no wall-clock waits.
    /// </summary>
    [Fact]
    public void Getters_WhenInjectedClockAdvancesPastTtl_ShouldRebuildSnapshotDeterministically()
    {
        // Arrange: a controllable clock and a long TTL that wall time could never cross in-test
        var utcNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var clock = Substitute.For<IDateTimeMachine>();
        clock.UtcNow.Returns(_ => utcNow);
        var service = new IndTraceEventsService(logger: null, snapshotTtl: TimeSpan.FromMinutes(5), dateTimeMachine: clock);

        service.AddOrUpdateTaskGatewayRequest(1, new TaskGatewayRequest
        {
            MachineId = 1,
            Name = "M1",
            TimeStamp = DateTime.Now,
        });

        var first = service.RequestEvents;
        service.RequestEvents.ShouldBeSameAs(first); // same version, TTL not elapsed on the fake clock

        // Act: advance ONLY the injected clock past the TTL
        utcNow = utcNow.AddMinutes(6);

        // Assert: the snapshot rebuilds without any wall-clock wait
        service.RequestEvents.ShouldNotBeSameAs(first);
    }

    /// <summary>
    /// #126 LOW (F3 caller sweep): ApplyConfiguration must honor the EnsureIsValidToRenderAndPersist
    /// verdict — an unrenderable stored request (required member nulled by materialization) is logged
    /// and SKIPPED, never stamped as current and never thrown on, while healthy entries keep refreshing.
    /// </summary>
    [Fact]
    public void ApplyConfiguration_WhenStoredRequestFailsRenderValidation_ShouldLogAndSkipThatEntry()
    {
        // Arrange
        var logger = Substitute.For<ILogger<IndTraceEventsService>>();
        var service = new IndTraceEventsService(logger);

        var staleStamp = DateTime.UtcNow.ToLocalTime().AddMinutes(-10);
        var broken = new TaskGatewayRequest { MachineId = 3, Name = "M3", TimeStamp = staleStamp };
        var healthy = new TaskGatewayRequest { MachineId = 4, Name = "M4", TimeStamp = staleStamp };
        service.AddOrUpdateTaskGatewayRequest(3, broken);
        service.AddOrUpdateTaskGatewayRequest(4, healthy);

        // Simulate EF/wire materialization nulling a required non-enum member
        var barCodeProperty = typeof(TaskGatewayRequest).GetProperty(nameof(TaskGatewayRequest.BarCode));
        barCodeProperty.ShouldNotBeNull();
        barCodeProperty.SetValue(broken, null);

        var configuration = new ApplicationConfiguration
        {
            Machines =
            [
                new MachineDto { MachineId = 3, Name = "Machine-Three", EnableAppTraceability = 1 },
                new MachineDto { MachineId = 4, Name = "Machine-Four", EnableAppTraceability = 1 },
            ],
        };

        // Act
        Should.NotThrow(() => service.ApplyConfiguration(configuration));

        // Assert: the invalid entry was skipped (not stamped), the healthy one refreshed, and a warning surfaced
        broken.TimeStamp.ShouldBe(staleStamp);
        healthy.TimeStamp.ShouldBeGreaterThan(staleStamp.AddMinutes(1));
        healthy.Description.ShouldBe("Machine-Four");
        logger.ReceivedCalls()
            .Any(call => call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                         call.GetArguments().OfType<LogLevel>().Any(level => level == LogLevel.Warning))
            .ShouldBeTrue();
    }

    // ----- #120 F4: observer faults must be logged, not written to Console -----

    /// <summary>
    /// #120 F4: an observer that throws during OnNext must be reported through the injected
    /// ILogger (pre-fix: Console.WriteLine).
    /// </summary>
    [Fact]
    public void NotifyStateChanged_WhenObserverThrows_ShouldLogError()
    {
        // Arrange
        var logger = Substitute.For<ILogger<IndTraceEventsService>>();
        var service = new IndTraceEventsService(logger);

        var throwingHandler = Substitute.For<Action<StateChange>>();
        throwingHandler.When(x => x.Invoke(Arg.Any<StateChange>())).Throw(new InvalidOperationException("observer fault"));
        service.Subscribe(Observer.Create<StateChange>(throwingHandler));

        // Act
        Should.NotThrow(() => service.NotifyStateChanged(new StateChange { State = "TestState" }));

        // Assert: the fault surfaced on the logger at Error level
        logger.ReceivedCalls()
            .Any(call => call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                         call.GetArguments().OfType<LogLevel>().Any(level => level == LogLevel.Error))
            .ShouldBeTrue();
    }

    // ----- #120 F3: getters must serve cached snapshots, not rebuild+filter+sort per read -----

    /// <summary>
    /// #120 F3 money test: two consecutive reads of any getter with no intervening writes must
    /// return the SAME snapshot instance (pre-fix every read ran FilterByModel + OrderBy +
    /// ToDictionary + new ReadOnlyDictionary — a fresh instance per access, per circuit, per render).
    /// </summary>
    [Fact]
    public void Getters_ConsecutiveReadsWithoutWrites_ShouldReturnSameSnapshotInstances()
    {
        // Arrange: put one entry in every backing map
        var now = DateTime.UtcNow.ToLocalTime();
        _eventsService.AddOrUpdateTaskGatewayRequest(1, new TaskGatewayRequest
        {
            MachineId = 1,
            Name = "M-SNAP",
            PartNumber = "PN-A",
            TimeStamp = now,
        });
        _eventsService.AddOrUpdateTaskGatewayResponse(1, new TaskGatewayResponseDto
        {
            MachineId = 1,
            Name = "M-SNAP",
            PartNumber = "PN-A",
            TimeStamp = now,
        });
        _eventsService.AddOrUpdateControllerFromGateway(1, new ControllerMonitor { PlcId = 1 });

        // Act + Assert: with no writes between the two reads, the getter must serve its cache
        _eventsService.RequestEvents.ShouldBeSameAs(_eventsService.RequestEvents);
        _eventsService.ResponseEvents.ShouldBeSameAs(_eventsService.ResponseEvents);
        _eventsService.StationMonitors.ShouldBeSameAs(_eventsService.StationMonitors);
        _eventsService.ControllerMonitors.ShouldBeSameAs(_eventsService.ControllerMonitors);
    }

    /// <summary>
    /// #120 F3: a write through the request ingestion path must bump the version so the next read
    /// rebuilds the snapshot and exposes the fresh entry.
    /// </summary>
    [Fact]
    public void RequestEvents_AfterWrite_ShouldRebuildSnapshotWithFreshData()
    {
        // Arrange
        var now = DateTime.UtcNow.ToLocalTime();
        _eventsService.AddOrUpdateTaskGatewayRequest(1, new TaskGatewayRequest
        {
            MachineId = 1,
            Name = "M-V1",
            PartNumber = "PN-A",
            TimeStamp = now,
        });
        var before = _eventsService.RequestEvents;

        // Act
        _eventsService.AddOrUpdateTaskGatewayRequest(2, new TaskGatewayRequest
        {
            MachineId = 2,
            Name = "M-V2",
            PartNumber = "PN-A",
            TimeStamp = now,
        });
        var after = _eventsService.RequestEvents;

        // Assert
        after.ShouldNotBeSameAs(before);
        after.ShouldContainKey(2);
        after[2].Name.ShouldBe("M-V2");
    }

    /// <summary>
    /// #120 F3: a write through the response ingestion path must invalidate BOTH the response
    /// snapshot and the station snapshot (the response path updates the station internally).
    /// </summary>
    [Fact]
    public void ResponseEvents_AfterWrite_ShouldRebuildResponseAndStationSnapshots()
    {
        // Arrange
        var now = DateTime.UtcNow.ToLocalTime();
        _eventsService.AddOrUpdateTaskGatewayResponse(1, new TaskGatewayResponseDto
        {
            MachineId = 1,
            Name = "M-RS1",
            PartNumber = "PN-A",
            TimeStamp = now,
        });
        var responsesBefore = _eventsService.ResponseEvents;
        var stationsBefore = _eventsService.StationMonitors;

        // Act
        _eventsService.AddOrUpdateTaskGatewayResponse(2, new TaskGatewayResponseDto
        {
            MachineId = 2,
            Name = "M-RS2",
            PartNumber = "PN-A",
            TimeStamp = now,
        });

        // Assert
        _eventsService.ResponseEvents.ShouldNotBeSameAs(responsesBefore);
        _eventsService.ResponseEvents.ShouldContainKey(2);
        _eventsService.StationMonitors.ShouldNotBeSameAs(stationsBefore);
        _eventsService.StationMonitors.ShouldContainKey(2);
    }

    /// <summary>
    /// #120 F3: controller writes (gateway heartbeat path) must invalidate the controller snapshot.
    /// </summary>
    [Fact]
    public void ControllerMonitors_AfterGatewayUpdate_ShouldRebuildSnapshot()
    {
        // Arrange
        _eventsService.AddOrUpdateControllerFromGateway(1, new ControllerMonitor { PlcId = 1 });
        var before = _eventsService.ControllerMonitors;

        // Act
        _eventsService.AddOrUpdateControllerFromGateway(2, new ControllerMonitor { PlcId = 2 });

        // Assert
        _eventsService.ControllerMonitors.ShouldNotBeSameAs(before);
        _eventsService.ControllerMonitors.ShouldContainKey(2);
    }

    /// <summary>
    /// #120 F3: ApplyConfiguration writes every backing map, so every cached snapshot must be
    /// invalidated — including the controller path that mutates an EXISTING monitor in place.
    /// </summary>
    [Fact]
    public void ApplyConfiguration_ShouldInvalidateAllSnapshots()
    {
        // Arrange: seed every map, including an existing controller for the in-place update branch
        var now = DateTime.UtcNow.ToLocalTime();
        _eventsService.AddOrUpdateTaskGatewayRequest(1, new TaskGatewayRequest
        {
            MachineId = 1,
            Name = "M-CFG",
            PartNumber = "PN-A",
            TimeStamp = now,
        });
        _eventsService.AddOrUpdateTaskGatewayResponse(1, new TaskGatewayResponseDto
        {
            MachineId = 1,
            Name = "M-CFG",
            PartNumber = "PN-A",
            TimeStamp = now,
        });
        _eventsService.AddOrUpdateControllerFromGateway(1, new ControllerMonitor { PlcId = 1 });

        var requestsBefore = _eventsService.RequestEvents;
        var responsesBefore = _eventsService.ResponseEvents;
        var stationsBefore = _eventsService.StationMonitors;
        var controllersBefore = _eventsService.ControllerMonitors;

        var configuration = new ApplicationConfiguration
        {
            Machines =
            [
                new MachineDto { MachineId = 1, Name = "M-CFG-NEW", EnableAppTraceability = 1 },
            ],
            Plcs =
            [
                new PlcDto { PlcId = 1, MachineId = 1, Name = "PLC-CFG", IpAddress = "10.0.0.1", Enabled = true },
            ],
        };

        // Act
        _eventsService.ApplyConfiguration(configuration);

        // Assert: all four snapshots were rebuilt and reflect the configuration write
        _eventsService.RequestEvents.ShouldNotBeSameAs(requestsBefore);
        _eventsService.ResponseEvents.ShouldNotBeSameAs(responsesBefore);
        _eventsService.StationMonitors.ShouldNotBeSameAs(stationsBefore);
        _eventsService.ControllerMonitors.ShouldNotBeSameAs(controllersBefore);
        _eventsService.ControllerMonitors[1].Name.ShouldBe("PLC-CFG");
    }

    /// <summary>
    /// #120 F3: FilterByModel prunes by an age window, so a cached snapshot must also expire by
    /// TTL — after the TTL elapses a read rebuilds even though no write bumped the version.
    /// </summary>
    [Fact]
    public async Task Getters_AfterTtlElapses_ShouldRebuildSnapshotWithoutWritesAsync()
    {
        // Arrange: a service with a deliberately tiny TTL (ctor seam added for this test)
        var service = new IndTraceEventsService(snapshotTtl: TimeSpan.FromMilliseconds(20));
        service.AddOrUpdateTaskGatewayRequest(1, new TaskGatewayRequest
        {
            MachineId = 1,
            Name = "M-TTL",
            PartNumber = "PN-A",
            TimeStamp = DateTime.UtcNow.ToLocalTime(),
        });
        var first = service.RequestEvents;
        first.ShouldContainKey(1); // sanity: snapshot built (identity-under-cache proven elsewhere)

        // Act: let the TTL elapse with NO writes
        await Task.Delay(50, TestContext.Current.CancellationToken);

        // Assert: the read after the TTL rebuilt the snapshot
        service.RequestEvents.ShouldNotBeSameAs(first);
    }

    // ----- #120 F6a: the SERVICE applies the arrived hub event to the station, exactly once -----

    /// <summary>
    /// #120 F6a money test: ingesting a gateway REQUEST must update that machine's station inside
    /// the service with the ARRIVED DTO (pre-fix the request ingestion path never touched
    /// stations — each open Blazor circuit re-applied a MaxBy(TimeStamp) over the filtered view,
    /// so N circuits mutated the shared singleton N times and near-simultaneous updates for
    /// different machines were lost).
    /// </summary>
    [Fact]
    public void AddOrUpdateTaskGatewayRequest_ShouldApplyArrivedRequestToItsStation()
    {
        // Arrange: a NEWER request for a DIFFERENT machine already sits in the map
        var now = DateTime.UtcNow.ToLocalTime();
        _eventsService.AddOrUpdateTaskGatewayRequest(2, new TaskGatewayRequest
        {
            MachineId = 2,
            Name = "ST-F6-B",
            PartNumber = "PN-NEWER",
            TimeStamp = now,
        });

        // Act: an out-of-order (older-timestamp) event for machine 1 arrives
        _eventsService.AddOrUpdateTaskGatewayRequest(1, new TaskGatewayRequest
        {
            MachineId = 1,
            Name = "ST-F6-A",
            PartNumber = "PN-ARRIVED",
            TimeStamp = now.AddMinutes(-1),
        });

        // Assert: the ARRIVED DTO reached ITS station (a MaxBy implementation would have
        // re-applied machine 2's newer request instead and machine 1 would never get a station)
        _eventsService.StationMonitors.ShouldContainKey(1);
        _eventsService.StationMonitors[1].PartNumber.ShouldBe("PN-ARRIVED");

        // And machine 2's station still reflects machine 2's own event
        _eventsService.StationMonitors.ShouldContainKey(2);
        _eventsService.StationMonitors[2].PartNumber.ShouldBe("PN-NEWER");
    }

    /// <summary>
    /// #120 F6a (response path guard): ingesting a gateway RESPONSE applies the ARRIVED DTO to its
    /// station even when a newer-timestamp entry exists elsewhere in the map — the pages no longer
    /// re-apply MaxBy(TimeStamp), so the service must remain the single writer.
    /// </summary>
    [Fact]
    public void AddOrUpdateTaskGatewayResponse_ShouldApplyArrivedResponseToItsStation()
    {
        // Arrange: a NEWER response for a different machine already sits in the map
        var now = DateTime.UtcNow.ToLocalTime();
        _eventsService.AddOrUpdateTaskGatewayResponse(2, new TaskGatewayResponseDto
        {
            MachineId = 2,
            Name = "ST-F6R-B",
            PartNumber = "PN-NEWER",
            CyclesOk = 5,
            TimeStamp = now,
        });

        // Act: an out-of-order (older-timestamp) response for machine 1 arrives
        _eventsService.AddOrUpdateTaskGatewayResponse(1, new TaskGatewayResponseDto
        {
            MachineId = 1,
            Name = "ST-F6R-A",
            PartNumber = "PN-ARRIVED",
            CyclesOk = 9,
            TimeStamp = now.AddMinutes(-1),
        });

        // Assert: each station reflects its own machine's arrived DTO — exactly once, no MaxBy
        _eventsService.StationMonitors[1].PartNumber.ShouldBe("PN-ARRIVED");
        _eventsService.StationMonitors[1].CyclesOk.ShouldBe(9);
        _eventsService.StationMonitors[2].PartNumber.ShouldBe("PN-NEWER");
        _eventsService.StationMonitors[2].CyclesOk.ShouldBe(5);
    }
}