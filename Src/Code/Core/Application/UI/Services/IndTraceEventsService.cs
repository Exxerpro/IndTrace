// <copyright file="IndTraceEventsService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UI.Services;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Globalization;
using IndTrace.Application.Models.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Central event management service for the IndTrace manufacturing system.
/// Provides real-time monitoring of PLC communications, machine states, and production events.
/// Implements the Observer pattern for state change notifications and maintains thread-safe collections
/// for tracking gateway requests, responses, controller monitors, and station monitors.
/// </summary>
/// <remarks>
/// This service acts as a singleton and provides centralized state management for:
/// - PLC communication events (requests and responses)
/// - Controller and station monitoring data
/// - Production data and messages
/// - Real-time state change notifications to UI components
///
/// All collections are thread-safe using ConcurrentDictionary implementations.
/// The service automatically filters and sorts data for optimal presentation.
/// </remarks>
public class IndTraceEventsService : IIndTraceEventsService, IObservable<StateChange>
{
    // Private data for monitor
    private readonly FixedSizedStack<string> messages = new();

    /// <summary>
    /// Gets a read-only collection of system messages ordered chronologically.
    /// </summary>
    /// <value>
    /// A collection containing recent system messages with timestamps and machine identifiers.
    /// Messages are automatically managed in a fixed-size stack to prevent memory growth.
    /// </value>
    public IReadOnlyCollection<string> Messages => this.messages.ToReadOnlyCollection();

    // Define dictionaries as ConcurrentDictionary to ensure thread-safe operations
    private readonly ConcurrentDictionary<int, TaskGatewayResponseDto> responseEvents = new();

    private readonly ConcurrentDictionary<int, TaskGatewayRequest> requestEvents = new();

    private readonly ConcurrentDictionary<int, ControllerMonitor> controllerMonitors = new();
    private readonly ConcurrentDictionary<int, StationMonitor> stationMonitors = new();

    // #120 F2 / #126 C11: the ConnectionLost delegate registered per controller id. With the
    // update-in-place design the canonical monitor instance per id is permanent, so this map now acts
    // as the once-per-id subscription gate (TryAdd) — exactly one live handler per PLC id, ever.
    private readonly ConcurrentDictionary<int, EventHandler> connectionLostHandlers = new();

    // ----- #120 F3: versioned snapshot caches for the read-only dictionary views -----
    //
    // INVARIANT (do not break): EVERY mutation of a backing map — including in-place mutation of
    // an entry's filter/sort-relevant fields — MUST bump that map's version counter. All
    // backing-map writes therefore go through the small Set*/TryAdd*/AddOrUpdate* helpers below
    // (which bump right next to the write so the increment can't be forgotten); in-place mutations
    // that bypass a map write call the matching Bump*Version helper directly. A missed bump is
    // bounded by the TTL: FilterByModel prunes by a 45-minute age window, so snapshots also expire
    // after DefaultSnapshotTtl to keep age-based pruning correct without per-read rebuilds.

    /// <summary>
    /// Default freshness window for cached getter snapshots. Snapshots rebuild when the backing
    /// map's version changes OR when older than this TTL (so FilterByModel's age-based pruning
    /// still advances while no writes arrive).
    /// </summary>
    public static readonly TimeSpan DefaultSnapshotTtl = TimeSpan.FromSeconds(5);

    private readonly TimeSpan snapshotTtl;

    private long responseEventsVersion;
    private long requestEventsVersion;
    private long controllerMonitorsVersion;
    private long stationMonitorsVersion;

    private readonly SnapshotCache<TaskGatewayResponseDto> responseEventsSnapshot = new();
    private readonly SnapshotCache<TaskGatewayRequest> requestEventsSnapshot = new();
    private readonly SnapshotCache<ControllerMonitor> controllerMonitorsSnapshot = new();
    private readonly SnapshotCache<StationMonitor> stationMonitorsSnapshot = new();

    /// <summary>
    /// Gets a read-only dictionary of PLC gateway response events, filtered and ordered by machine ID.
    /// </summary>
    /// <value>
    /// A dictionary where keys are machine IDs and values are gateway response events containing
    /// production cycle results, validation statuses, and processing outcomes.
    /// Served from a versioned snapshot cache (#120 F3): rebuilt only on write or TTL expiry.
    /// </value>
    public IReadOnlyDictionary<int, TaskGatewayResponseDto> ResponseEvents =>
        this.responseEventsSnapshot.GetOrRebuild(
            Interlocked.Read(ref this.responseEventsVersion),
            this.snapshotTtl,
            this.dateTimeMachine.UtcNow,
            () => new ReadOnlyDictionary<int, TaskGatewayResponseDto>(this.responseEvents
                .FilterByModel()
                .OrderBy(k => k.Key)
                .ToDictionary(pair => pair.Key, pair => pair.Value)));

    /// <summary>
    /// Gets a read-only dictionary of PLC gateway request events, filtered and ordered by machine ID.
    /// </summary>
    /// <value>
    /// A dictionary where keys are machine IDs and values are gateway request events containing
    /// commands sent to PLCs for production operations, barcode processing, and cycle control.
    /// Served from a versioned snapshot cache (#120 F3): rebuilt only on write or TTL expiry.
    /// </value>
    public IReadOnlyDictionary<int, TaskGatewayRequest> RequestEvents =>
        this.requestEventsSnapshot.GetOrRebuild(
            Interlocked.Read(ref this.requestEventsVersion),
            this.snapshotTtl,
            this.dateTimeMachine.UtcNow,
            () => new ReadOnlyDictionary<int, TaskGatewayRequest>(this.requestEvents
                .FilterByModel()
                .OrderBy(k => k.Key)
                .ToDictionary(pair => pair.Key, pair => pair.Value)));

    // Filter out entries with MachineId <= 0 and sort by Label, then by Key
    // This ensures that the dictionary is always in a consistent state and ready for use.
    // The filtering and sorting are done in a thread-safe manner using LINQ.

    /// <summary>
    /// Gets a read-only dictionary of PLC controller monitors, filtered to exclude invalid entries and sorted by label and key.
    /// </summary>
    /// <value>
    /// A dictionary where keys are PLC IDs and values are controller monitor objects containing
    /// connection status, heartbeat information, IP addresses, and communication health data.
    /// Only includes controllers with valid IDs (> 0) and sorts alphabetically by label.
    /// Served from a versioned snapshot cache (#120 F3): rebuilt only on write or TTL expiry.
    /// </value>
    public IReadOnlyDictionary<int, ControllerMonitor> ControllerMonitors =>
        this.controllerMonitorsSnapshot.GetOrRebuild(
            Interlocked.Read(ref this.controllerMonitorsVersion),
            this.snapshotTtl,
            this.dateTimeMachine.UtcNow,
            () => new ReadOnlyDictionary<int, ControllerMonitor>(this.controllerMonitors
                .Where(kvp => kvp.Key > 0)
                .OrderBy(kvp => string.IsNullOrWhiteSpace(kvp.Value.Label)) // Empty/null labels last
                .ThenBy(kvp => kvp.Value.Label) // Alphabetical within label
                .ThenBy(kvp => kvp.Key) // MachineId/Key sort
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));

    /// <summary>
    /// Gets a read-only dictionary of production station monitors, filtered and ordered by machine ID.
    /// </summary>
    /// <value>
    /// A dictionary where keys are machine IDs and values are station monitor objects containing
    /// production status, cycle information, part processing data, and operational state.
    /// Served from a versioned snapshot cache (#120 F3): rebuilt only on write or TTL expiry.
    /// </value>
    public IReadOnlyDictionary<int, StationMonitor> StationMonitors =>
        this.stationMonitorsSnapshot.GetOrRebuild(
            Interlocked.Read(ref this.stationMonitorsVersion),
            this.snapshotTtl,
            this.dateTimeMachine.UtcNow,
            () => new ReadOnlyDictionary<int, StationMonitor>(this.stationMonitors
                .FilterByModel()
                .OrderBy(k => k.Key)
                .ToDictionary(pair => pair.Key, pair => pair.Value)));

    /// <summary>
    /// Gets the current production data containing aggregated manufacturing metrics and status information.
    /// </summary>
    /// <value>
    /// The production data object containing overall production statistics, performance indicators,
    /// and operational metrics across all monitored machines and stations.
    /// </value>
    public ProductionData ProductionData { get; private set; } = new();

    private readonly List<IObserver<StateChange>> observers = [];
    private Lock observersLock = new Lock();

    /// <summary>
    /// Logger for observer faults and diagnostic events. Never null: defaults to
    /// <see cref="NullLogger{T}"/> so parameterless construction (tests) stays valid while the
    /// DI container injects the real logger through the concrete-type singleton registration.
    /// </summary>
    private readonly ILogger<IndTraceEventsService> logger;

    /// <summary>
    /// Deterministic clock for the snapshot TTL (#126 LOW: the SnapshotCache read <c>DateTime.UtcNow</c>
    /// directly, against the deterministic-time doctrine). Never null: defaults to a real
    /// <see cref="DateTimeMachine"/> so parameterless construction stays valid.
    /// </summary>
    private readonly IDateTimeMachine dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndTraceEventsService"/> class.
    /// </summary>
    /// <param name="logger">Optional logger; a <see cref="NullLogger{T}"/> is used when omitted.</param>
    /// <param name="snapshotTtl">
    /// Optional freshness window for the cached getter snapshots (#120 F3); defaults to
    /// <see cref="DefaultSnapshotTtl"/>. Exposed for deterministic TTL tests.
    /// </param>
    /// <param name="dateTimeMachine">
    /// Optional deterministic clock for the snapshot TTL; a real <see cref="DateTimeMachine"/> is used
    /// when omitted.
    /// </param>
    public IndTraceEventsService(ILogger<IndTraceEventsService>? logger = null, TimeSpan? snapshotTtl = null, IDateTimeMachine? dateTimeMachine = null)
    {
        this.logger = logger ?? NullLogger<IndTraceEventsService>.Instance;
        this.snapshotTtl = snapshotTtl ?? DefaultSnapshotTtl;
        this.dateTimeMachine = dateTimeMachine ?? new DateTimeMachine();
    }

    /// <summary>
    /// Gets or sets the current application configuration used for initializing monitoring systems.
    /// </summary>
    private ApplicationConfiguration? ApplicationConfiguration { get; set; }

    /// <summary>
    /// Applies the specified application configuration to initialize and update all monitoring systems.
    /// </summary>
    /// <param name="configuration">The application configuration containing machine, PLC, and system settings.</param>
    /// <remarks>
    /// This method updates all internal collections and monitoring systems based on the provided configuration:
    /// - Initializes request and response event tracking for enabled machines
    /// - Sets up station monitoring for production equipment
    /// - Configures controller monitoring for PLCs
    /// - Initializes production data structures
    ///
    /// If a null configuration is provided, the method returns without making changes.
    /// </remarks>
    public void ApplyConfiguration(ApplicationConfiguration configuration)
    {
        if (configuration is null)
        {
            return; // Only apply configuration if it is not null
        }

        this.ApplicationConfiguration = configuration;

        this.UpdateRequestEventsFromConfiguration(configuration);

        this.UpdateResponseEventsFromConfiguration(configuration);

        this.UpdateStationsFromConfiguration(configuration);

        this.UpdateControllersFromConfiguration(configuration);

        this.InitializeProductInfoFromConfiguration(configuration);
    }

    /// <summary>
    /// Adds a timestamped message to the system message stack and notifies observers of the state change.
    /// </summary>
    /// <param name="user">The identifier of the user or machine that generated the message.</param>
    /// <param name="message">The message content to be logged.</param>
    /// <remarks>
    /// Messages are automatically formatted with timestamp and machine identifier.
    /// The message stack has a fixed size to prevent memory growth.
    /// State change notifications are sent to all registered observers.
    /// </remarks>
    public void PushMessage(string user, string message)
    {
        var encodedMsg =
            $"Message received at {DateTime.Now.ToLocalTime().ToString(CultureInfo.InvariantCulture)} fromMachine: {user}: {message}";
        this.messages.Push(encodedMsg);

        // Notify state change
        this.UpdateState(new StateChange
        {
            PropertyName = nameof(this.Messages),
            State = encodedMsg,
        });
    }

    /// <summary>
    /// Adds or updates a PLC gateway request event and notifies observers of the state change.
    /// </summary>
    /// <param name="id">The unique identifier (typically machine ID) for the request event.</param>
    /// <param name="requestEvent">The gateway request event containing PLC command details.</param>
    /// <remarks>
    /// This method maintains a thread-safe collection of gateway requests for monitoring PLC communications.
    /// State change notifications are automatically sent to registered observers when the collection is updated.
    /// </remarks>
    public void AddOrUpdateTaskGatewayRequest(int id, TaskGatewayRequest requestEvent)
    {
        this.SetRequestEvent(id, requestEvent);

        // #120 F6a: apply the ARRIVED request to its station exactly once, here in the service.
        // (Previously every open Blazor circuit re-applied MaxBy(TimeStamp) over the filtered view
        // in its OnNext — N circuits mutated the shared singleton N times and near-simultaneous
        // updates for different machines were lost to the single max-timestamp winner.)
        this.UpdateStationFromGatewayRequest(requestEvent);

        // Notify state change
        this.UpdateState(new StateChange
        {
            PropertyName = nameof(this.RequestEvents),
            State = requestEvent,
        });
    }

    /// <summary>
    /// Adds or updates a PLC gateway response event, updates associated station monitors, and notifies observers.
    /// </summary>
    /// <param name="id">The unique identifier (typically machine ID) for the response event.</param>
    /// <param name="responseEvent">The gateway response event containing PLC processing results.</param>
    /// <remarks>
    /// This method not only updates the response collection but also automatically updates the corresponding
    /// station monitor based on the response data. State change notifications are sent to all registered observers.
    /// </remarks>
    public void AddOrUpdateTaskGatewayResponse(int id, TaskGatewayResponseDto responseEvent)
    {
        this.SetResponseEvent(id, responseEvent);

        // #120 F6a: apply the ARRIVED response to its station exactly once, here in the service
        // (the pages' OnNext MaxBy re-application has been removed — see AddOrUpdateTaskGatewayRequest).
        this.UpdateStationFromGatewayResponse(responseEvent);

        // Notify state change
        this.UpdateState(new StateChange
        {
            PropertyName = nameof(this.ResponseEvents),
            State = responseEvent,
        });
    }

    private void UpdateRequestEventsFromConfiguration(ApplicationConfiguration configuration)
    {
        if (configuration == null)
        {
            return;
        }

        var currentTime = DateTime.UtcNow.ToLocalTime();

        foreach (var machine in configuration.Machines.Where(m => m.IsEnabled && m.MachineId > 0))
        {
            // #120 F1: read/update the BACKING map, never the filtered RequestEvents view.
            // FilterByModel can hide an entry whose key still exists, so a filtered-view miss
            // followed by TryAdd fails silently and the entry goes permanently stale.
            // AddOrUpdate keeps the read-modify-write atomic against hub-callback threads.
            this.AddOrUpdateRequestEvent(
                machine.MachineId,
                _ =>
                {
                    // Add new entry with default values
                    var newRequest = new TaskGatewayRequest
                    {
                        MachineId = machine.MachineId,
                        TimeStamp = currentTime,
                        Name = machine.Name,
                        CycleStatus = CycleStatus.None,
                        PartStatus = PartStatus.None,
                        PartNumber = string.Empty,
                        GatewayTask = GatewayTask.None,
                        BarCode = string.Empty,
                    };

                    // #126 LOW (F3 caller sweep): the verdict is now meaningful. A freshly constructed
                    // default has every required member non-null, so a false here can only mean the
                    // defaults regressed — log it loudly rather than discard it silently.
                    if (!newRequest.EnsureIsValidToRenderAndPersist())
                    {
                        this.logger.LogWarning("Default TaskGatewayRequest for MachineId {MachineId} failed render/persist validation at construction", machine.MachineId);
                    }

                    return newRequest;
                },
                (_, request) =>
                {
                    // #126 LOW (F3 caller sweep): honor the verdict — a stored request whose required
                    // members were nulled (EF materialization / wire deserialization) is unrenderable and
                    // unpersistable; log and SKIP the refresh instead of stamping it as current (idiom of
                    // the other F3 callers: log + skip, never throw).
                    if (!request.EnsureIsValidToRenderAndPersist())
                    {
                        this.logger.LogWarning("Stored TaskGatewayRequest for MachineId {MachineId} failed render/persist validation; configuration refresh skipped for this entry", machine.MachineId);
                        return request;
                    }

                    // Update existing entry (you can add specific logic here if needed)
                    request.TimeStamp = currentTime;
                    request.Description = machine.Name;
                    return request;
                });
        }
    }

    private void UpdateResponseEventsFromConfiguration(ApplicationConfiguration configuration)
    {
        if (configuration == null)
        {
            return;
        }

        var currentTime = DateTime.UtcNow.ToLocalTime();

        foreach (var machine in configuration.Machines.Where(m => m.IsEnabled && m.MachineId > 0))
        {
            if (this.responseEvents.TryGetValue(machine.MachineId, out var response))
            {
                // #32 C2: immutable wire DTO — evolve with `with` and write the new instance back to the backing map.
                this.SetResponseEvent(machine.MachineId, response with { TimeStamp = currentTime, Name = machine.Name });
            }
            else
            {
                // Add new entry with default values (enums default to .None at construction).
                var newResponse = new TaskGatewayResponseDto
                {
                    MachineId = machine.MachineId,
                    TimeStamp = currentTime,
                    Name = machine.Name,
                    CycleStatus = CycleStatus.None,
                    ResultValidation = ResultValidation.None,
                    Label = string.Empty,
                    PartNumber = string.Empty,
                    CyclesOk = 0,
                    NextMachineId = 0,
                    LastMachineId = 0,
                };
                this.TryAddResponseEvent(newResponse.MachineId, newResponse);
            }
        }
    }

    private void UpdateStationsFromConfiguration(ApplicationConfiguration configuration)
    {
        if (configuration == null)
        {
            return;
        }

        var currentTime = DateTime.UtcNow.ToLocalTime();

        foreach (var machine in configuration.Machines.Where(m => m.IsEnabled && m.MachineId > 0))
        {
            // #120 F1: same pattern as the request path — the filtered StationMonitors view is
            // display-only; a hidden-but-present key must still be updated in the backing map.
            this.AddOrUpdateStationMonitorEntry(
                machine.MachineId,
                _ => new StationMonitor
                {
                    IsEnabled = true,
                    MachineId = machine.MachineId,
                    TimeStamp = currentTime,
                    Name = machine.Name,
                },
                (_, station) =>
                {
                    station.IsEnabled = true;
                    station.TimeStamp = currentTime;
                    station.Description = machine.Name;
                    return station;
                });
        }
    }

    private void UpdateControllersFromConfiguration(ApplicationConfiguration configuration)
    {
        if (configuration is null)
        {
            return;
        }

        var currentTime = DateTime.UtcNow.ToLocalTime();

        foreach (var plc in configuration.Plcs.Where(p => p.Enabled && p.PlcId > 0))
        {
            // #120 F3: read the BACKING map (not the cached ControllerMonitors snapshot view) and
            // bump the version after the in-place mutation — the mutation changes sort/display
            // fields without writing a new map entry.
            if (this.controllerMonitors.TryGetValue(plc.PlcId, out var controller))
            {
                this.UpdateControllerFromConfiguration(controller, plc, currentTime);
                this.BumpControllerMonitorsVersion();
            }
            else
            {
                this.AddControllerFromConfiguration(plc, currentTime);
            }
        }
    }

    private void InitializeProductInfoFromConfiguration(ApplicationConfiguration applicationConfiguration)
    {
        this.ProductionData = new ProductionData();
    }

    /// <summary>
    /// Executes UpdateStationFromGatewayRequest operation.
    /// </summary>
    /// <param name="request">The request.</param>
    public void UpdateStationFromGatewayRequest(TaskGatewayRequest? request)
    {
        if (request is null)
        {
            return;
        }

        var currentTime = DateTime.UtcNow.ToLocalTime();

        // #120 F1: update the BACKING dictionary (the filtered StationMonitors view can hide an
        // existing key; a view miss + TryAdd silently discarded every subsequent PLC request for
        // that machine). AddOrUpdate is atomic against concurrent hub-callback writers.
        this.AddOrUpdateStationMonitorEntry(
            request.MachineId,
            _ => StationMonitor.CreateStationFromGatewayRequest(request, currentTime),
            (_, station) =>
            {
                station.UpdateStationFromGatewayRequest(request, currentTime);
                return station;
            });
    }

    /// <summary>
    /// Executes UpdateStationFromGatewayResponse operation.
    /// </summary>
    /// <param name="response">The response.</param>
    public void UpdateStationFromGatewayResponse(TaskGatewayResponseDto? response)
    {
        if (response is null)
        {
            return;
        }

        var currentTime = DateTime.UtcNow.ToLocalTime();

        // #120 F1: update the BACKING dictionary (see UpdateStationFromGatewayRequest) — a station
        // hidden by FilterByModel must still receive PLC response updates.
        this.AddOrUpdateStationMonitorEntry(
            response.MachineId,
            _ => StationMonitor.CreateStationFromGatewayResponse(response, currentTime),
            (_, station) =>
            {
                station.UpdateStationFromGatewayResponse(response, currentTime);
                return station;
            });
    }

    private void UpdateControllerFromConfiguration(ControllerMonitor controller, PlcDto plc, DateTime currentTime)
    {
        controller.MachineId = plc.MachineId;
        controller.IpAddress = plc.IpAddress;
        controller.Name = plc.Name;
        controller.TimeStamp = currentTime.AddSeconds(-60);
        controller.HeartBeat = -1;
    }

    private void AddControllerFromConfiguration(PlcDto plc, DateTime currentTime)
    {
        var controllerMonitor = new ControllerMonitor
        {
            PlcId = plc.PlcId,
            MachineId = plc.MachineId,
            IpAddress = plc.IpAddress,
            Name = plc.Name,
            TimeStamp = currentTime.AddSeconds(-60),
            HeartBeat = -1,
        };

        this.AddOrUpdateControllerMonitor(controllerMonitor.PlcId, controllerMonitor);
    }

    // Method to add or update a ControllerMonitor instance

    /// <summary>
    /// Executes AddOrUpdateControllerFromGateway operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="heartBeatControllerMonitor">The heartBeatControllerMonitor.</param>
    public void AddOrUpdateControllerFromGateway(int id, ControllerMonitor heartBeatControllerMonitor)
    {
        var canonical = this.AddOrUpdateControllerMonitor(id, heartBeatControllerMonitor);
        if (canonical is null)
        {
            return;
        }

        // #126 C11: notify with the CANONICAL instance — observers must never be handed the folded-in
        // (and now disposed) duplicate.
        this.UpdateState(new StateChange
        {
            PropertyName = nameof(this.ControllerMonitors),
            State = canonical,
        });
    }

    private ControllerMonitor? AddOrUpdateControllerMonitor(int id, ControllerMonitor heartBeatControllerMonitor)
    {
        if (heartBeatControllerMonitor is null)
        {
            return null;
        }

        // #126 C11: UPDATE IN PLACE. Writers are SignalR hub callback threads; readers are Blazor circuit
        // renders that HOLD the monitor instances served by the ControllerMonitors view. The former
        // replace-and-dispose per heartbeat made every stale circuit reference throw
        // ObjectDisposedException from TimeStamp/RefreshConnection (both restart the disposed heartbeat
        // timer), crashing the circuit. The canonical instance per PLC id is therefore permanent:
        // heartbeats and config refreshes fold their state onto it via ControllerMonitor.UpdateFrom.
        //
        // #126 C12: AddOrUpdate makes the read-modify-write atomic per key, closing the
        // TryGetValue/Set/Dispose race that could adopt a stale monitor over a fresh heartbeat or leak an
        // undisposed armed duplicate. Remaining (documented) window: two concurrent heartbeats for the
        // SAME id fold their fields onto the canonical instance concurrently — per-field last-writer-wins,
        // converging on the next heartbeat; no interleaving can dispose a published instance or leak one.
        var adopted = this.AddOrUpdateControllerMonitorEntry(
            id,
            _ => heartBeatControllerMonitor,
            (_, existing) =>
            {
                existing.UpdateFrom(heartBeatControllerMonitor);
                return existing;
            });

        if (!ReferenceEquals(adopted, heartBeatControllerMonitor))
        {
            // The incoming instance was folded into the canonical one and was never published, so no
            // reader can hold it: dispose it here so the heartbeat timer armed during hub
            // deserialization (setting TimeStamp starts it) cannot leak (#120 F2 intent preserved —
            // never more than one live timer per PLC id).
            heartBeatControllerMonitor.Dispose();
        }

        // Subscribe the canonical instance exactly once per id (the instance never changes, so the
        // handler registered at adoption stays valid for the service lifetime). connectionLostHandlers
        // TryAdd is the single-subscription gate under concurrent adopters (#126 C12).
        void OnConnectionLostHandler(object? sender, EventArgs e)
        {
            this.UpdateState(new StateChange
            {
                PropertyName = nameof(this.ControllerMonitors),
                State = adopted,
            });
        }

        EventHandler onConnectionLost = OnConnectionLostHandler;
        if (this.connectionLostHandlers.TryAdd(id, onConnectionLost))
        {
            adopted.ConnectionLost += onConnectionLost;
        }

        return adopted;
    }

    /// <summary>
    /// Executes Subscribe operation.
    /// </summary>
    /// <param name="observer">The observer.</param>
    /// <returns>The result of Subscribe.</returns>
    public IDisposable Subscribe(IObserver<StateChange> observer)
    {
        lock (this.observersLock)
        {
            if (!this.observers.Contains(observer))
            {
                this.observers.Add(observer);
            }
        }

        return new Unsubscriber<StateChange>(this.observers, observer, this.observersLock);
    }

    /// <summary>
    /// Executes NotifyStateChanged operation.
    /// </summary>
    /// <param name="change">The change.</param>
    public void NotifyStateChanged(StateChange change)
    {
        if (change is null)
        {
            return;
        }

        try
        {
            List<IObserver<StateChange>> snapshot;
            lock (this.observersLock)
            {
                snapshot = this.observers.ToList(); // snapshot inside lock
            }

            foreach (var observer in snapshot)
            {
                try
                {
                    observer.OnNext(change);
                }
                catch (Exception e)
                {
                    // #120 F4: an observer fault must never take down the notifying hub-callback
                    // thread, but it must be visible — log it instead of Console.WriteLine.
                    this.logger.LogError(e, "Observer OnNext failed for state change {PropertyName}", change.PropertyName);
                }
            }
        }
        catch (Exception e)
        {
            // #120 F4: fail-loud on the log, fail-safe on the thread (see above).
            this.logger.LogError(e, "State change notification failed for {PropertyName}", change.PropertyName);
        }
    }

    /// <summary>
    /// Executes UpdateState operation.
    /// </summary>
    /// <param name="change">The change.</param>
    public void UpdateState(StateChange change)
    {
        this.NotifyStateChanged(change);
    }

    // ----- #120 F3: backing-map write helpers (write + version bump, always together) -----
    // INVARIANT: no code outside these helpers may write requestEvents / responseEvents /
    // stationMonitors / controllerMonitors; in-place mutations that change filter/sort-relevant
    // fields without a map write must call the matching Bump*Version helper.

    private void SetRequestEvent(int id, TaskGatewayRequest value)
    {
        this.requestEvents[id] = value;
        this.BumpRequestEventsVersion();
    }

    private void AddOrUpdateRequestEvent(
        int id,
        Func<int, TaskGatewayRequest> addFactory,
        Func<int, TaskGatewayRequest, TaskGatewayRequest> updateFactory)
    {
        this.requestEvents.AddOrUpdate(id, addFactory, updateFactory);
        this.BumpRequestEventsVersion();
    }

    private void SetResponseEvent(int id, TaskGatewayResponseDto value)
    {
        this.responseEvents[id] = value;
        this.BumpResponseEventsVersion();
    }

    private void TryAddResponseEvent(int id, TaskGatewayResponseDto value)
    {
        if (this.responseEvents.TryAdd(id, value))
        {
            this.BumpResponseEventsVersion();
        }
    }

    private void AddOrUpdateStationMonitorEntry(
        int id,
        Func<int, StationMonitor> addFactory,
        Func<int, StationMonitor, StationMonitor> updateFactory)
    {
        this.stationMonitors.AddOrUpdate(id, addFactory, updateFactory);
        this.BumpStationMonitorsVersion();
    }

    private ControllerMonitor AddOrUpdateControllerMonitorEntry(
        int id,
        Func<int, ControllerMonitor> addFactory,
        Func<int, ControllerMonitor, ControllerMonitor> updateFactory)
    {
        // #126 C12: single atomic read-modify-write per key (the update factory mutates the canonical
        // instance in place and returns it, so ConcurrentDictionary never swaps the published reference).
        var adopted = this.controllerMonitors.AddOrUpdate(id, addFactory, updateFactory);
        this.BumpControllerMonitorsVersion();
        return adopted;
    }

    private void BumpRequestEventsVersion() => Interlocked.Increment(ref this.requestEventsVersion);

    private void BumpResponseEventsVersion() => Interlocked.Increment(ref this.responseEventsVersion);

    private void BumpStationMonitorsVersion() => Interlocked.Increment(ref this.stationMonitorsVersion);

    private void BumpControllerMonitorsVersion() => Interlocked.Increment(ref this.controllerMonitorsVersion);

    /// <summary>
    /// #120 F3: version + TTL gated snapshot cache for one read-only dictionary view.
    /// Readers get the SAME immutable <see cref="ReadOnlyDictionary{TKey,TValue}"/> instance until
    /// a backing write bumps the version or the TTL elapses; the rebuild runs inside the lock so a
    /// torn snapshot can never be observed. The caller reads the version BEFORE the rebuild, so a
    /// concurrent write during the rebuild at worst records a conservative (older) version and
    /// triggers one extra rebuild on the next read — never staleness.
    /// </summary>
    /// <typeparam name="TValue">The dictionary value type of the cached view.</typeparam>
    private sealed class SnapshotCache<TValue>
    {
        private readonly Lock gate = new();
        private long builtVersion = -1;
        private DateTime builtAtUtc = DateTime.MinValue;
        private ReadOnlyDictionary<int, TValue>? snapshot;

        /// <summary>
        /// Returns the cached snapshot when it is still current, otherwise rebuilds it.
        /// </summary>
        /// <param name="currentVersion">The backing map's current version counter.</param>
        /// <param name="ttl">Maximum snapshot age before a rebuild is forced (age-based pruning).</param>
        /// <param name="nowUtc">
        /// The current UTC instant from the service's injected <see cref="IDateTimeMachine"/> (#126 LOW:
        /// no direct <c>DateTime.UtcNow</c> reads — deterministic-time doctrine).
        /// </param>
        /// <param name="build">Builds a fresh immutable snapshot from the backing map.</param>
        /// <returns>The current snapshot instance.</returns>
        public IReadOnlyDictionary<int, TValue> GetOrRebuild(
            long currentVersion,
            TimeSpan ttl,
            DateTime nowUtc,
            Func<ReadOnlyDictionary<int, TValue>> build)
        {
            lock (this.gate)
            {
                if (this.snapshot is not null &&
                    this.builtVersion == currentVersion &&
                    nowUtc - this.builtAtUtc < ttl)
                {
                    return this.snapshot;
                }

                var rebuilt = build();
                this.snapshot = rebuilt;
                this.builtVersion = currentVersion;
                this.builtAtUtc = nowUtc;
                return rebuilt;
            }
        }
    }
}