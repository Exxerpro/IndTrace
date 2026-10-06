// <copyright file="GatewayWorker.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Gateway.Gateway;

using IndTrace.Gateway.Interfaces;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Extensions;
using Microsoft.AspNetCore.SignalR;
using static IndTrace.HubConnection.Contracts.HubMethods;

/// <summary>
/// Represents the GatewayWorker.
/// </summary>
/// <param name="id">The PLC id this worker tracks.</param>
/// <param name="logger">The logger.</param>
/// <param name="commandDispatcher">The gateway command dispatcher.</param>
/// <param name="configService">The configuration service.</param>
/// <param name="connectionFactory">The hub connection factory.</param>
/// <param name="dateTimeMachine">The deterministic time source.</param>
/// <param name="hubConnection">The hub connection.</param>
/// <param name="plcControllerFactory">The installed PLC driver's controller factory.</param>
public class GatewayWorker(
    int id,
    ILogger<GatewayWorker> logger,
    IGatewayCommandDispatcher commandDispatcher,
    IndTraceConfigurationService configService,
    IHubConnectionFactory connectionFactory,
    DateTimeMachine dateTimeMachine,
    IHubConnection hubConnection,
    IPlcControllerFactory plcControllerFactory)
    : BackgroundService, IManualStart
{
    public readonly int Id = id;
    private readonly ILogger<GatewayWorker> logger = logger;
    private readonly IndTraceConfigurationService configService = configService;
    private readonly IHubConnectionFactory connectionFactory = connectionFactory;
    private readonly DateTimeMachine dateTimeMachine = dateTimeMachine;
    private readonly IPlcControllerFactory plcControllerFactory = plcControllerFactory;
    private CancellationTokenSource? cts; // Allow individual worker cancellation

    public required PlcDto PlcData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the HOST-LEVEL gateway simulation mode is on for this worker
    /// (source: <see cref="GatewaySimulationOptions"/>, set by the manager at worker construction). When ON,
    /// the PLC configuration handed to the controller is marked simulation-enabled (NoOpPlc, no real PLC I/O)
    /// and console simulation commands may dispatch. Defaults to <see langword="false"/>: real PLC path, and
    /// console simulation input is refused (issue #101 follow-up).
    /// </summary>
    public bool SimulationEnabled { get; set; }

    private IIndTraceControllerRx? controller;

    /// <summary>
    /// Test seam (mirrors the manager's internal observation seams): exposes this worker's active controller
    /// so gating tests can prove console simulation input never touches the controller when simulation mode
    /// is OFF. Production code assigns the controller only via <see cref="SetupCommunicationsPlcs"/>.
    /// </summary>
    internal IIndTraceControllerRx? ActiveController
    {
        get => this.controller;
        set => this.controller = value;
    }
    private string machineName = string.Empty;

    // Define an event that external components can trigger
    public event Func<SimulatedCommand, Task>? OnCommandReceived;

    public Task StartAsync()
    {
        if (this.cts != null)
        {
            this.logger.LogWarning("[{WorkerId}] Worker is already running", this.Id);
            return Task.CompletedTask;
        }

        this.cts = new CancellationTokenSource();
        this.logger.LogInformation("[{WorkerId}] Starting worker", this.Id);

        return Task.Run(() => this.ExecuteAsync(this.cts.Token));
    }

    public Task StopAsync()
    {
        if (this.cts == null)
        {
            this.logger.LogWarning("[{WorkerId}] Worker is not running", this.Id);
            return Task.CompletedTask;
        }

        this.logger.LogInformation("[{WorkerId}] Stopping worker", this.Id);
        this.cts.Cancel();
        this.cts.Dispose();
        this.cts = null;

        this.UnsubscribeFromEvents();

        return Task.CompletedTask;
    }

    private ApplicationConfiguration configApplicationConfiguration = new();
    private DateTime lasTimeStampCommandExecuted = dateTimeMachine.Now.ToLocalTime();

    public bool Configured { get; private set; }

    public bool IsHubConnected => this.hubConnection is not null && this.hubConnection.State == HubConnectionState.Connected;

    /// <summary>
    /// Gets a value indicating whether this worker's tracking loop is still healthy. It flips to
    /// <see langword="false"/> only when the long-running <see cref="ExecuteAsync"/> loop terminates on a
    /// genuinely unrecoverable fault (not a transient hub/PLC blip, which the loop retries). A manager or a
    /// health probe can read this per line without taking the whole plant down.
    /// </summary>
    public bool IsHealthy { get; private set; } = true;

    // #103: 1 = a tracking loop is currently running. Both start paths (IManualStart.StartAsync via Task.Run
    // and BackgroundService.StartAsync via base.StartAsync) funnel into ExecuteAsync; without this guard,
    // invoking both ran TWO concurrent tracking loops against the same PLC. Reset on loop exit so a manual
    // Stop/Start cycle still works.
    private int trackingLoopRunning;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Interlocked.CompareExchange(ref this.trackingLoopRunning, 1, 0) != 0)
        {
            this.logger.LogWarning("[{WorkerId}] Tracking loop is already running; ignoring the duplicate start request (#103 single-start guard).", this.Id);
            return;
        }

        // The tracking loop is designed to run for the whole process lifetime and to swallow+retry every
        // transient condition (hub or PLC briefly unreachable). Only an exception that escapes that retry
        // loop is a genuine, unrecoverable fault — that is the one thing the manager must observe. We keep
        // cancellation (a normal shutdown) strictly separate from a fault so stopping never looks like a
        // crash, and we re-throw a real fault so the observed ExecuteTask actually faults.
        try
        {
            await this.RunTrackingLoopAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            this.logger.LogInformation("[{WorkerId}] Tracking loop stopped on cancellation.", this.Id);
        }
        catch (Exception ex)
        {
            this.IsHealthy = false;
            this.logger.LogCritical(
                ex,
                "[{WorkerId}] Tracking loop terminated by an unrecoverable fault; this line is now UNHEALTHY and has stopped tracking.",
                this.Id);
            throw;
        }
        finally
        {
            Volatile.Write(ref this.trackingLoopRunning, 0);
        }
    }

    private async Task RunTrackingLoopAsync(CancellationToken stoppingToken)
    {
        this.hubConnection = await this.hubConnection.EnsureHubConnectionIsValid(this.connectionFactory, this.logger, stoppingToken).ConfigureAwait(false) ?? this.hubConnection;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                this.Configured = await this.TryConfigureGatewayAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Transient/expected per-iteration failure: log it, tell the hub, and let the loop retry on
                // the next pass. This must never escalate — a briefly-unreachable hub or PLC is business as
                // usual for a tracking gateway.
                var errorMessage = $"{ErrorGatewayExecution}: {ex.Message}";
                this.logger.LogError(ex, "Error executing gateway command - {ErrorConstant}", ErrorExecutingCommand);
                await this.LogErrorToHubAsync(errorMessage, stoppingToken).ConfigureAwait(false);
            }

            if (!this.IsHubConnected)
            {
                this.hubConnection = await this.hubConnection.EnsureHubConnectionIsValid(this.connectionFactory, this.logger, stoppingToken).ConfigureAwait(false) ?? this.hubConnection;
            }

            this.LogInformationEveryFiveMinutes();

            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);

            // if (PlcData.PlcId == 100 && dateTimeMachine.Now.Second == 0)
            // {
            //    var performance = _controller.ReadPerformanceDataCommandFromPlcAsync(stoppingToken);

            // _logger.LogInformation(performance.ToString());
            // }
        }
    }

    public async Task<bool> TryConfigureGatewayAsync(CancellationToken cancellationToken)
    {
        if (this.Configured)
        {
            return true;
        }

        this.logger.LogInformation("Communications Worker PLC {KpiOeeId} running at:  {time}", this.Id, this.dateTimeMachine.Now.ToLocalTime());
        await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", "Starting",
            this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);

        try
        {
            this.logger.LogInformation("Communications Worker  {KpiOeeId} GetConfigAppDetails at : {time}", this.Id, this.dateTimeMachine.Now.ToLocalTime());
            var resultConfiguration = await this.configService.GetConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            if (resultConfiguration.IsSuccess && resultConfiguration.Value is not null)
            {
                this.configApplicationConfiguration = resultConfiguration.Value;
            }
            else
            {
                this.logger.LogError("Error getting configuration details @{Errors}", resultConfiguration.Errors);
            }

            this.logger.LogInformation("Communications Worker SetupCommunications PLC {KpiOeeId} at : {time}", this.Id, this.dateTimeMachine.Now.ToLocalTime());
            var communicationsReady = await this.SetupCommunicationsPlcs(cancellationToken).ConfigureAwait(false);

            this.logger.LogInformation("Communications Worker SubscribeToEvents PLC {id} at: {time}", this.Id, this.dateTimeMachine.Now.ToLocalTime());
            this.SubscribeToEvents(cancellationToken);

            this.logger.LogInformation("Communications Worker {KpiOeeId} ValidateConfigurationIsCorrect at: {time}", this.Id, this.dateTimeMachine.Now.ToLocalTime());

            // #126 C6: the Configured latch is set ONLY when the WHOLE setup succeeded — config data valid
            // AND the controller was created, tag-validated, and adopted. ValidateConfigurationIsCorrect
            // checks config data alone; pre-fix it latched Configured=true even when SetupCommunicationsPlcs
            // failed (e.g. a transient tag-validation failure while the PLC was briefly unreachable at
            // startup), so the 1 Hz configure-retry loop never re-entered and the worker was permanently
            // stuck refusing work until process restart. A failed setup now leaves the latch open so the
            // next loop pass re-runs the full configure+validate sequence.
            this.Configured = communicationsReady && this.ValidateConfigurationIsCorrect();
            if (this.Configured)
            {
                this.logger.LogInformation("PLC {KpiOeeId} Configuration and communication successfully at: {time}", this.Id, this.dateTimeMachine.Now.ToLocalTime());
            }
            else if (!communicationsReady)
            {
                this.logger.LogWarning("PLC {KpiOeeId} communications setup did not complete; the configure-retry loop will re-run the full configure+validate sequence.", this.Id);
            }

            this.logger.LogInformation("Communications Worker {KpiOeeId} Configured at: {time}", this.Id, this.dateTimeMachine.Now.ToLocalTime());

            // Kick off the deferred startup reads through SafeFireAndForget so any exception is observed
            // and logged (a raw Task.Run would leave it unobserved). Guard the controller: if configuration
            // did not produce one, there is nothing to read. #126 C6: also gate on Configured — on a failed
            // setup pass the field may still hold a controller from an earlier pass, and firing the startup
            // reset against it while the worker is (correctly) not configured would be a stray PLC write.
            var startupController = this.controller;
            if (this.Configured && startupController is not null)
            {
                this.SafeFireAndForget(
                    async () =>
                    {
                        var delay = Random.Shared.Next(1000, 5001); // 5001 is exclusive upper bound
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        await startupController.ReadStartUp(cancellationToken).ConfigureAwait(false);
                    },
                    nameof(IIndTraceControllerRx.ReadStartUp));

                this.SafeFireAndForget(
                    async () =>
                    {
                        var delay = Random.Shared.Next(1000, 2001); // 2001 is exclusive upper bound
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        await startupController.ResetCommandAsync(cancellationToken).ConfigureAwait(false);
                    },
                    nameof(IIndTraceControllerRx.ResetCommandAsync));
            }

            return this.Configured;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing command: {ErrorConstant}", ErrorExecutingCommand);
            await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", ErrorGatewayExecution,
                this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
        }

        return false;
    }

    private bool ValidateConfigurationIsCorrect()
    {
        if (this.configApplicationConfiguration?.Plcs is null)
        {
            this.logger.LogError("Configuration or Plcs collection is null for PLC {RecipeId}", this.Id);
            return false;
        }

        // Fail loud when this worker's PlcId is absent — never fall back to an empty stub. Adopting a
        // default PlcDto here would silently clobber the config DTO adopted by SetupCommunicationsPlcs
        // and recreate the empty-IP limp-on mode that wedged the configure-retry loop.
        var configuredPlc = this.configApplicationConfiguration.Plcs.FirstOrDefault(x => x.PlcId == this.Id);
        if (configuredPlc is null)
        {
            this.logger.LogError(" PLC {RecipeId} Configuration and communication failed ", this.Id);
            return false;
        }

        this.PlcData = configuredPlc;

        if (this.Id == this.PlcData.PlcId)
        {
            this.logger.LogInformation("PLC {RecipeId} Configuration and communication Ok ", this.Id);
            return true;
        }
        else
        {
            this.logger.LogError(" PLC {RecipeId} Configuration and communication failed ", this.Id);
            return false;
        }
    }

    /// <summary>
    /// Creates, validates, and adopts this worker's controller from the loaded configuration.
    /// #126 C6: the outcome is no longer fire-and-forget — <see cref="TryConfigureGatewayAsync"/> folds it
    /// into the <see cref="Configured"/> latch so a failed setup (missing PLC/machine config, or a rejected
    /// controller) keeps the configure-retry loop re-entering instead of latching a permanently
    /// "configured" worker that never adopted a controller.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns><see langword="true"/> when a controller was configured and adopted; otherwise <see langword="false"/>.</returns>
    private async Task<bool> SetupCommunicationsPlcs(CancellationToken cancellationToken)
    {
        if (this.configApplicationConfiguration == null)
        {
            throw new InvalidDataException("App Details is null");
        }

        if (this.configApplicationConfiguration.Plcs == null)
        {
            throw new InvalidDataException("Plc Details is null");
        }

        if (this.configApplicationConfiguration.Machines == null)
        {
            throw new InvalidDataException("Machines Details is null");
        }

        // The manager seeds PlcData with an id-only stub (`required` property, no IpAddress/tags), so this
        // adoption must run on EVERY pass. A null-only guard here kept the stub forever: every controller was
        // built with an empty IpAddress, setup threw, and — because the Configured latch short-circuits past
        // ValidateConfigurationIsCorrect (the only other place that reloads PlcData) — the 1 Hz configure-retry
        // loop could never recover. A PlcId genuinely missing from configuration now fails loudly instead of
        // limping on as an empty PlcDto.
        var configuredPlc = this.configApplicationConfiguration.Plcs.FirstOrDefault(x => x.PlcId == this.Id);
        if (configuredPlc is null)
        {
            this.logger.LogCritical("PlcId {RecipeId} not found in configuration", this.Id);
            return false;
        }

        this.PlcData = configuredPlc;

        var machine = this.configApplicationConfiguration.Machines.FirstOrDefault(x => x.MachineId == this.Id);
        if (machine is null)
        {
            this.logger.LogCritical("MachineId {MachineId} not found in configuration", this.Id);
            return false;
        }
        this.machineName = machine.Name;

        // Stamp the host-level simulation switch onto the configuration the controller will consult
        // (PlcDetails.EnableSimulation): this is what makes controller setup pick the explicit NoOpPlc on a
        // simulation rig. With the flag OFF (default) this is a no-op and the real Sharp7Plc path is taken.
        this.ApplyHostSimulationMode(this.PlcData);

        var id = this.PlcData.PlcId;
        var setupResult = await this.SetupControllerAsync(id, this.PlcData, this.dateTimeMachine, cancellationToken).ConfigureAwait(false);

        if (!setupResult.IsSuccess || setupResult.Value is null)
        {
            this.logger.LogError("PLC {RecipeId} Configuration and communication failed ", this.Id);
            await this.LogErrorToHubAsync($"PLC {this.Id} Configuration and communication failed", cancellationToken).ConfigureAwait(false);
            return false;
        }

        var configuredController = setupResult.Value;

        // #103: the configure-retry loop re-enters this method every second until validation passes; each pass
        // allocates a fresh controller (a Sharp7 connection + Rx subscriptions). Dispose the previous instance
        // before replacing it so a repeatedly-failing configuration cannot leak connections unboundedly.
        var previousController = this.controller;
        this.controller = configuredController;
        if (previousController is not null && !ReferenceEquals(previousController, configuredController))
        {
            try
            {
                previousController.Dispose();
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "[{WorkerId}] Failed to dispose the previous controller while re-configuring; continuing with the new controller.", this.Id);
            }
        }

        var message = configuredController.MachineId == id ? $"PlcId {id} Configured" : $"PlcId {id} Error configuring";

        await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", message,
            this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
        await this.HeartBeatChangedHandler(configuredController, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly ConcurrentDictionary<int, DateTime> lastExecutionTimeStamps = new();
    private readonly double minIntervalExecutionTime = 1500;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Communications Worker started at: {DateTime}", this.dateTimeMachine.Now.ToLocalTime());
        this.hubConnection = await this.hubConnection.EnsureHubConnectionIsValid(this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false) ?? this.hubConnection;
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Tear down Rx subscriptions and the heartbeat CTS before dropping the connection so nothing keeps
        // firing against a disposed hub.
        this.UnsubscribeFromEvents();

        if (this.cts is not null)
        {
            await this.cts.CancelAsync().ConfigureAwait(false);
            this.cts.Dispose();
            this.cts = null;
        }

        if (this.hubConnection is not null)
        {
            await this.hubConnection.StopAsync(cancellationToken).ConfigureAwait(false);
            await this.hubConnection.DisposeAsync().ConfigureAwait(false);
        }

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    private void LogInformationEveryFiveMinutes()
    {
        // #126 F2.1 — one snapshot: separate Now reads for Minute and Second can tear across a
        // minute boundary and make the five-minute gate misfire.
        var now = this.dateTimeMachine.Now;
        if (now.Minute % 5 != 0 || now.Second != 0)
        {
            return;
        }

        this.logger.LogInformation("Communications Worker for plc {id} running at: {DateTime}", this.Id, now.ToLocalTime());
        this.logger.LogInformation("Last Request executed at {LasTimeCommandExecuted}", this.lasTimeStampCommandExecuted);
    }

    private async Task LogErrorToHubAsync(string message, CancellationToken cancellationToken)
    {
        if (this.hubConnection is not null)
        {
            await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", message,
                this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task HeartBeatChangedHandler(IIndTraceControllerRx controller, CancellationToken cancellationToken)
    {
        if (!this.IsHubConnected)
        {
            this.hubConnection = await this.hubConnection.EnsureHubConnectionIsValid(this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false) ?? this.hubConnection;
        }

        try
        {
            if (controller is not null)
            {
                var monitor = await controller.GetPlcMonitorAsync(cancellationToken).ConfigureAwait(false);
                await this.hubConnection.TryInvokeAsync(BroadcastHeartbeatSignal, controller.PlcId, monitor, this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (TaskCanceledException)
        {
            // Log task cancellation
            if (controller is not null)
            {
                this.logger.LogWarning("Heartbeat task was canceled for PLC {PlcId}", controller.PlcId);
            }
        }
        catch (HubException ex)
        {
            this.logger.LogError(ex, ErrorWhileSendingHeartbeat);
            if (this.hubConnection is not null)
            {
                await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", ErrorWhileSendingHeartbeat,
                    this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ErrorWhileSendingHeartbeat);
        }
    }

    private IDisposable? heartbeatSubscription;
    private CancellationTokenSource? heartbeatCts;
    private readonly Lock syncLock = new();
    private IDisposable? commandChangedSubscription;
    private bool isCommandHandling;
    private bool commandPending;
    private IHubConnection hubConnection = hubConnection;

    private async Task CommandChangedHandler(IIndTraceControllerRx controller, CancellationToken cancellationToken)
    {
        // #103: the §7 reset/ack write (command tag -> 0) raises its own CommandChanged edge. There is nothing
        // to process for a non-positive command (HandleCommandAsync no-ops on it), so skip it BEFORE latching —
        // otherwise every processed command would be followed by a pointless drain cycle holding the latch.
        if (controller.Command <= 0)
        {
            this.logger.LogDebug("[{WorkerId}] Ignoring command edge with non-positive command value {Command} (reset/idle edge).", this.Id, controller.Command);
            return;
        }

        if (this.ShouldSkipCommandProcessing())
        {
            return;
        }

        try
        {
            this.logger.LogInformation("Handling command {Request} from controller {RecipeId}", controller.Command, this.Id);

            // #103: an edge arriving inside the minimum interval was previously DISCARDED entirely (no latch,
            // no command-tag reset, no result write) — an edge-triggered command silently lost. The debounce
            // intent (suppress duplicate detections of the SAME edge) is preserved by COALESCING: wait out the
            // remainder of the interval and then process once. Duplicate/noise edges self-suppress downstream
            // (a reset command tag reads <= 0 and same-part resends replay idempotently), while a genuinely new
            // command is never dropped without a dispatch and a result write.
            var coalesceDelay = this.TimeUntilAcceptedInterval(this.Id);
            if (coalesceDelay > TimeSpan.Zero)
            {
                this.logger.LogWarning("Command fired too quickly {PlcId}; coalescing — waiting {Delay} before processing so the edge is not lost.", this.Id, coalesceDelay);
                var message = $"Command fired too quickly {this.Id}";
                await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", message,
                    this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
                await Task.Delay(coalesceDelay, cancellationToken).ConfigureAwait(false);
                this.lastExecutionTimeStamps[this.Id] = this.dateTimeMachine.Now.ToLocalTime();
            }

            if (!this.IsHubConnected)
            {
                this.EnsureHubConnectionInBackground(cancellationToken);
            }

            await controller.HandleCommandAsync(commandDispatcher, this.hubConnection, this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            bool reprocessPending;
            lock (this.syncLock)
            {
                this.isCommandHandling = false;
                reprocessPending = this.commandPending;
                this.commandPending = false;
            }

            // A command edge that arrived while we were handling the previous one must not be lost: it was
            // latched in commandPending. Re-run the handler once so the tracked line is not left stale.
            var pendingController = this.controller;
            if (reprocessPending && pendingController is not null && !cancellationToken.IsCancellationRequested)
            {
                this.logger.LogInformation("[{WorkerId}] Re-processing a command edge that arrived during handling.", this.Id);
                this.SafeFireAndForget(() => this.CommandChangedHandler(pendingController, cancellationToken), nameof(this.CommandChangedHandler));
            }
        }
    }

    private bool ShouldSkipCommandProcessing()
    {
        lock (this.syncLock)
        {
            if (this.isCommandHandling)
            {
                // Do not silently drop the edge: latch it so it is re-processed once the current command
                // completes. Losing a command edge would leave the tracked line's state stale.
                this.commandPending = true;
                this.logger.LogWarning("Command already in progress. New command latched for re-processing.");
                return true;
            }
            else
            {
                // #103: STAY SUBSCRIBED while handling. The previous code disposed the CommandChanged
                // subscription here for the whole multi-second HandleCommandAsync; CommandChanged is a plain
                // Subject (no replay), so an edge arriving during handling had NO subscriber and the
                // commandPending latch above was unreachable — genuinely new commands were lost until the PLC
                // watchdog expired. Serialization is enforced by the isCommandHandling latch, not by
                // unsubscribing.
                this.isCommandHandling = true;
                return false;
            }
        }
    }

    // #103: returns the time remaining inside the minimum per-machine execution interval (zero when clear).
    // When the interval is clear the execution timestamp is recorded immediately; a coalescing caller records
    // it after its wait completes.
    private TimeSpan TimeUntilAcceptedInterval(int machineId)
    {
        this.lasTimeStampCommandExecuted = this.dateTimeMachine.Now.ToLocalTime();
        if (this.lastExecutionTimeStamps.TryGetValue(machineId, out var lastExecutionTime))
        {
            var remaining = TimeSpan.FromMilliseconds(this.minIntervalExecutionTime) - (this.lasTimeStampCommandExecuted - lastExecutionTime);
            if (remaining > TimeSpan.Zero)
            {
                this.logger.LogInformation("Command for MachineId {MachineId} was executed too recently. Coalescing execution.", this.Id);
                return remaining;
            }
        }

        this.lastExecutionTimeStamps[machineId] = this.lasTimeStampCommandExecuted;
        return TimeSpan.Zero;
    }

    /// <summary>
    /// Creates the raw controller instance for a PLC. Internal virtual seam (#103) so tests can substitute the
    /// controller and count allocations; production delegates to the AddControllerAsync PLC extension, which
    /// asks the installed driver's <see cref="IPlcControllerFactory"/> for the controller.
    /// </summary>
    /// <param name="value">The PLC configuration to build the controller from.</param>
    /// <param name="dateTimeMachine">The deterministic time source.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The created controller, or the factory's failure.</returns>
    internal virtual Task<Result<IIndTraceControllerRx>> CreateControllerAsync(PlcDto value, DateTimeMachine dateTimeMachine, CancellationToken cancellationToken)
        => value.AddControllerAsync(this.plcControllerFactory, this.logger, this.hubConnection, this.connectionFactory, dateTimeMachine, cancellationToken);

    /// <summary>
    /// Creates and configures the controller for one PLC. #123 F5b (ops-visible change): the boolean
    /// outcomes of <c>ConfigureControllerAsync</c> and <c>ValidateVariablesAsync</c> were previously
    /// DISCARDED, so a PLC with missing tags limped on as "configured" and setup returned Success.
    /// Both are now honored — a false result fails setup loudly (naming the PLC key and the failed step)
    /// and the rejected controller is disposed so the 1 Hz configure-retry loop cannot leak connections.
    /// Internal for the GateWay.Tests seam (InternalsVisibleTo), mirroring <see cref="CreateControllerAsync"/>.
    /// </summary>
    /// <param name="key">The PLC key (PlcId) being configured.</param>
    /// <param name="value">The PLC configuration.</param>
    /// <param name="dateTimeMachine">The deterministic time source.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The configured controller, or a failure naming the PLC and the step that failed.</returns>
    internal async Task<Result<IIndTraceControllerRx>> SetupControllerAsync(int key, PlcDto value, DateTimeMachine dateTimeMachine, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IIndTraceControllerRx>.WithFailure("Operation was cancelled.");
        }

        IIndTraceControllerRx? controller = null;
        try
        {
            var created = await this.CreateControllerAsync(value, dateTimeMachine, cancellationToken).ConfigureAwait(false);
            if (created.IsFailure || created.Value is null)
            {
                // No controller was allocated, so there is nothing to dispose: report the driver's refusal
                // (e.g. no PLC driver installed for a non-simulated PLC) and let the configure-retry loop retry.
                this.logger.LogError("[{WorkerId}] PLC {PlcKey} controller could not be created: {Error}", this.Id, key, created.Error);
                if (this.hubConnection is not null)
                {
                    await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", $"PlcId {key} controller could not be created",
                        this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
                }

                return Result<IIndTraceControllerRx>.WithFailure($"PlcId {key} controller creation failed: {created.Error}");
            }

            controller = created.Value;

            var configured = await controller.ConfigureControllerAsync(key, this.logger, this.hubConnection, this.connectionFactory, cancellationToken).ConfigureAwait(false);
            if (!configured)
            {
                return await this.FailControllerSetupAsync(controller, key, "controller configuration (SetUpAsync)", cancellationToken).ConfigureAwait(false);
            }

            var tagsValid = await controller.ValidateVariablesAsync(key, this.logger, this.hubConnection, this.connectionFactory, cancellationToken).ConfigureAwait(false);
            if (!tagsValid)
            {
                return await this.FailControllerSetupAsync(controller, key, "PLC tag validation (ValidateVariablesAsync)", cancellationToken).ConfigureAwait(false);
            }

            var plcId = await controller.ConnectToControllerAsync((short)key, this.logger, this.hubConnection, this.connectionFactory, cancellationToken).ConfigureAwait(false);

            await this.logger.LogPlcConnectionStatusAsync(key, plcId, this.hubConnection, this.connectionFactory, cancellationToken).ConfigureAwait(false);

            return Result<IIndTraceControllerRx>.Success(controller);
        }
        catch (Exception ex) when (this.plcControllerFactory.IsControllerUnreachable(ex))
        {
            // #126 C7: the controller could not be REACHED — a transient condition, not a configuration or
            // tag error. Report it as a warning naming the retry (the C6 configure-retry loop re-runs the
            // full setup on its next pass) and release the half-configured controller so nothing leaks.
            this.logger.LogWarning(ex, "[{WorkerId}] PLC {PlcKey} controller unreachable during setup — transient; the configure-retry loop will retry.", this.Id, key);

            if (controller is not null)
            {
                try
                {
                    controller.Dispose();
                }
                catch (Exception disposeEx)
                {
                    this.logger.LogWarning(disposeEx, "[{WorkerId}] Failed to dispose a controller rejected while the PLC was unreachable.", this.Id);
                }
            }

            if (this.hubConnection is not null)
            {
                await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", $"PlcId {key} controller unreachable — will retry",
                    this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
            }

            return Result<IIndTraceControllerRx>.WithFailure($"PlcId {key} controller unreachable — will retry: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing command: {ErrorConstant}", ErrorExecutingCommand);

            // #103: a controller that was created but failed configuration must not be abandoned — the retry
            // loop allocates a new one on the next pass, so an undisposed failure here leaks one Sharp7
            // connection per retry second.
            if (controller is not null)
            {
                try
                {
                    controller.Dispose();
                }
                catch (Exception disposeEx)
                {
                    this.logger.LogWarning(disposeEx, "[{WorkerId}] Failed to dispose a partially-configured controller.", this.Id);
                }
            }

            var message = $"PlcId {key} Error configuring";
            if (this.hubConnection is not null)
            {
                await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", message,
                    this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
            }

            return Result<IIndTraceControllerRx>.WithFailure($"PlcId {key} configuration failed: {ex.Message}");
        }
    }

    /// <summary>
    /// #123 F5b failure path shared by both honored setup steps: logs the failed step loudly, disposes the
    /// rejected controller (same leak rationale as the #103 catch path — the retry loop allocates a fresh
    /// one every second), notifies the hub, and returns a failure naming the PLC key and the step.
    /// </summary>
    /// <param name="controller">The controller that failed a setup step.</param>
    /// <param name="key">The PLC key (PlcId) being configured.</param>
    /// <param name="step">The setup step that returned false.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A failed result naming the PLC key and the failed step.</returns>
    private async Task<Result<IIndTraceControllerRx>> FailControllerSetupAsync(
        IIndTraceControllerRx controller,
        int key,
        string step,
        CancellationToken cancellationToken)
    {
        this.logger.LogError("PLC {PlcKey} setup FAILED at step '{Step}'; the controller will not be adopted.", key, step);

        try
        {
            controller.Dispose();
        }
        catch (Exception disposeEx)
        {
            this.logger.LogWarning(disposeEx, "[{WorkerId}] Failed to dispose a controller rejected at setup step '{Step}'.", this.Id, step);
        }

        if (this.hubConnection is not null)
        {
            await this.hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", $"PlcId {key} Error configuring",
                this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
        }

        return Result<IIndTraceControllerRx>.WithFailure($"PlcId {key} configuration failed at step: {step}");
    }

    private void SubscribeToEvents(CancellationToken cancellationToken)
    {
        var localController = this.controller;
        if (localController is null)
        {
            // The configure loop may call this before a controller exists; do not throw, just skip so the
            // loop can retry cleanly on the next pass.
            this.logger.LogWarning("[{WorkerId}] Cannot subscribe to controller events: controller is not configured yet.", this.Id);
            return;
        }

        lock (this.syncLock)
        {
            // Idempotent re-subscribe: dispose any previous subscriptions first so a repeatedly-failing
            // configure loop cannot stack N live subscriptions and dispatch each PLC command N times.
            this.commandChangedSubscription?.Dispose();
            this.commandChangedSubscription = null;

            this.heartbeatSubscription?.Dispose();
            this.heartbeatSubscription = null;

            this.heartbeatCts?.Cancel();
            this.heartbeatCts?.Dispose();
            this.heartbeatCts = null;

            this.commandChangedSubscription = localController.CommandChanged.Subscribe(e =>
                this.SafeFireAndForget(() => this.CommandChangedHandler(e, cancellationToken), nameof(this.CommandChangedHandler)));

            this.heartbeatCts = new CancellationTokenSource();
            var localToken = this.heartbeatCts.Token;

            this.heartbeatSubscription = localController.HeartBeatChanged.Subscribe(e =>
                this.SafeFireAndForget(() => this.HeartBeatChangedHandler(e, localToken), nameof(this.HeartBeatChangedHandler)));
        }
    }

    private void UnsubscribeFromEvents()
    {
        lock (this.syncLock)
        {
            this.commandChangedSubscription?.Dispose();
            this.commandChangedSubscription = null;

            this.heartbeatSubscription?.Dispose();
            this.heartbeatSubscription = null;

            this.heartbeatCts?.Cancel();
            this.heartbeatCts?.Dispose();
            this.heartbeatCts = null;
        }
    }

    // #103: the background hub repair previously DISCARDED the validated/replacement connection returned by
    // EnsureHubConnectionIsValid, so a repaired connection was never adopted by this worker. Keep it. Internal
    // so the repair seam is directly testable.
    internal void EnsureHubConnectionInBackground(CancellationToken cancellationToken)
    {
        _ = Task.Run(
            async () =>
        {
            try
            {
                var repaired = await this.hubConnection.EnsureHubConnectionIsValid(this.connectionFactory, this.logger, cancellationToken).ConfigureAwait(false);
                if (repaired is not null)
                {
                    this.hubConnection = repaired;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Background task failed: EnsureHubConnectionIsValid");
            }
        }, cancellationToken);
    }

    private void SafeFireAndForget(Func<Task> action, string context)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Unhandled exception in {Context}", context);
            }
        });
    }

    /// <summary>
    /// Applies the host-level gateway simulation switch to the PLC configuration this worker is about to hand
    /// to its controller. <c>PlcDto.EnableSimulation</c> has no persisted source (the DTO mapping never sets
    /// it), so this host flag is THE source of the controller's NoOpPlc decision. OFF (the default) leaves the
    /// DTO untouched — the real Sharp7Plc path of issue #101.
    /// </summary>
    /// <param name="plcData">The PLC configuration about to be handed to the controller.</param>
    internal void ApplyHostSimulationMode(PlcDto? plcData)
    {
        if (plcData is null || !this.SimulationEnabled)
        {
            return;
        }

        if (!plcData.EnableSimulation)
        {
            this.logger.LogWarning(
                "[{WorkerId}] Gateway simulation mode is ON ({Section}:{Key}) — PLC {PlcId} will use the explicit NoOpPlc controller; NO real PLC I/O will occur on this line.",
                this.Id,
                GatewaySimulationOptions.SectionName,
                nameof(GatewaySimulationOptions.EnableSimulation),
                plcData.PlcId);
            plcData.EnableSimulation = true;
        }
    }

    public async Task SimulateCommand(SimulatedCommand command, CancellationToken cancellationToken)
    {
        // Life-critical gate (issue #101 follow-up): with the real Sharp7Plc construction path live, console
        // simulation input would write PartNumber/BarCode/PartStatus/Command straight into a REAL plant PLC.
        // Refuse it loudly unless the host explicitly enabled simulation mode; the controller is not touched.
        if (!this.SimulationEnabled)
        {
            this.logger.LogWarning(
                "[{WorkerId}] Console simulation command REFUSED: gateway simulation mode is OFF ({Section}:{Key}). The controller was NOT touched — enable the host flag on a simulation rig to use console commands.",
                this.Id,
                GatewaySimulationOptions.SectionName,
                nameof(GatewaySimulationOptions.EnableSimulation));
            return;
        }

        try
        {
            if (command.MachineId == this.Id)
            {
                var localController = this.controller;
                if (localController is null)
                {
                    this.logger.LogWarning("[{WorkerId}] Cannot simulate command: controller is not configured yet.", this.Id);
                    return;
                }

                this.logger.LogInformation("Simulating Request");
                await localController.SimulateCommandAsync(command, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to parse Request.");
        }
    }

    // Expose a public method to trigger the event
    public async Task TriggerCommand(SimulatedCommand command)
    {
        if (this.OnCommandReceived != null)
        {
            await this.OnCommandReceived.Invoke(command).ConfigureAwait(false);
        }
        else
        {
            this.logger.LogWarning("No handlers registered for SimulateCommand.");
        }
    }

}
