// <copyright file="GatewayWorkerManager.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Gateway.Gateway;

using IndTrace.Gateway.Exceptions;

/// <summary>
/// Represents the GatewayWorkerManager.
/// </summary>
public class GatewayWorkerManager(ILogger<GatewayWorkerManager> logger,
    DateTimeMachine dateTimeMachine,
    IndTraceConfigurationService configService,
    IServiceProvider serviceProvider,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    private readonly ILogger<GatewayWorkerManager> logger = logger;
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly IHostApplicationLifetime applicationLifetime = applicationLifetime;
    private IndTraceConfigurationService configService = configService;
    private readonly DateTimeMachine dateTimeMachine = dateTimeMachine;
    private readonly Dictionary<int, GatewayWorker> gatewayWorkers = []; // Keeps track of workers
    private readonly Dictionary<int, Task> runningWorkers = []; // Keeps track of workers
    private readonly Dictionary<int, IServiceScope> workerScopes = []; // DI scope owned per worker for its lifetime

    // Per-line fault registry. A worker whose long-running tracking loop dies is recorded here (loudly, via
    // LogCritical) instead of taking the whole plant down — one line's fault never stops tracking the others.
    private readonly ConcurrentDictionary<int, Exception> faultedWorkers = new();

    private readonly CancellationTokenSource cts = new(); // Global cancellation for cleanup

    /// <summary>
    /// Gets the workers whose long-running tracking loop has terminated on an unrecoverable fault, keyed by
    /// PLC id. Exposed for a health probe / monitor and for tests — a non-empty entry means that one line is
    /// unhealthy while the rest keep tracking.
    /// </summary>
    internal IReadOnlyDictionary<int, Exception> FaultedWorkers => this.faultedWorkers;

    /// <summary>
    /// Returns <see langword="true"/> while the worker for <paramref name="plcId"/> has not faulted.
    /// </summary>
    internal bool IsWorkerHealthy(int plcId) => !this.faultedWorkers.ContainsKey(plcId);

    /// <summary>
    /// Gets the started workers keyed by PLC id. Test observation seam (mirrors <see cref="FaultedWorkers"/>)
    /// so the host-simulation-flag wiring of <see cref="StartWorkerAsync"/> is verifiable.
    /// </summary>
    internal IReadOnlyDictionary<int, GatewayWorker> Workers => this.gatewayWorkers;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Gateway Worker Manager started.");

        List<int> plcs = [];

        try
        {
            this.logger.LogInformation("Communications Worker GetConfigAppDetails at : {time}", this.dateTimeMachine.Now.ToLocalTime());
            var resultConfiguration = await this.configService.GetConfigurationAsync(true, stoppingToken).ConfigureAwait(false);
            if (resultConfiguration.Value is null)
            {
                // Total, non-transient configuration failure: with no configuration there is nothing to
                // track on ANY line. That is genuinely plant-wide, so stopping the whole gateway is correct
                // here (unlike a single line's transient hiccup, which must never escalate).
                this.logger.LogCritical("PLC configuration is null — the gateway has nothing to track. Stopping the application.");
                this.applicationLifetime.StopApplication();
                return;
            }

            plcs = resultConfiguration.Value.Plcs.Select(e => e.PlcId).ToList();
            if (plcs.Count == 0)
            {
                this.logger.LogCritical("PLC list is empty — the gateway has nothing to track. Stopping the application.");
                this.applicationLifetime.StopApplication();
                return;
            }

            // Validate the PLC list have no tags on the same PLC with the same Address
            // For this purpose, we will use the IndTraceConfigurationService to get the configuration
            // and check if there are any duplicate addresses in the PLC list
            // Using Command, Barcode, PartNumber to discover duplicates
            this.DiscoverDuplicatedTags(resultConfiguration.Value);
            this.logger.LogInformation("Communications Worker GetConfigAppDetails at : {time}", this.dateTimeMachine.Now.ToLocalTime());
        }
        catch (Exception ex)
        {
            this.logger.LogError("Communications Worker GetConfigAppDetails at {ex}", ex);
            this.logger.LogCritical("exiting application");
            throw;
        }

        // Start a worker for each client. One PLC failing to start must not abort the whole manager and
        // strand the remaining lines untracked — log it loudly and keep going.
        foreach (var plc in plcs)
        {
            try
            {
                await this.StartWorkerAsync(plc, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogCritical(ex, "Failed to start worker for PLC {PlcId}; continuing with remaining PLCs.", plc);
            }
        }

        this.logger.LogInformation("All {Count} workers initialized successfully: [{Ids}]", this.gatewayWorkers.Count,
            string.Join(", ", this.gatewayWorkers.Keys));

        // Interactive keyboard simulation is a developer aid only. When stdin is redirected (Windows service,
        // container, systemd) Console.KeyAvailable throws, so it must never run in a hosted context.
        var consoleSimulationEnabled = !Console.IsInputRedirected;
        if (!consoleSimulationEnabled)
        {
            this.logger.LogInformation("Console input is redirected; keyboard command simulation is disabled.");
        }

        // Keep running, listen for additional clients (if needed)
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);

            if (!consoleSimulationEnabled)
            {
                continue;
            }

            var commands = await this.WaitForSimulationCommands(TimeSpan.FromMilliseconds(500), stoppingToken).ConfigureAwait(false);
            await this.SimulateEventFromKeyboard(commands, stoppingToken).ConfigureAwait(false);
        }

        this.logger.LogInformation("Gateway Worker Manager stopping.");
    }

    internal async Task StartWorkerAsync(int plcId, CancellationToken cancellationToken)
    {
        if (this.runningWorkers.ContainsKey(plcId))
        {
            this.logger.LogWarning("Worker for {PlcId} is already running", plcId);
            return;
        }

        this.logger.LogInformation("Starting worker for {PlcId}", plcId);

        // The scope is owned by the worker for its entire (process-lifetime) run and is disposed in StopAsync.
        // Disposing it here (the old `using`) left the long-lived worker holding captive, already-disposed
        // dependencies — the #33 corruption family.
        var scope = this.serviceProvider.CreateScope();

        try
        {
            var hubConnection = await scope.ServiceProvider.GetRequiredService<IHubConnectionFactory>()
                .CreateAsync(cancellationToken).ConfigureAwait(false);

            // Host-level simulation switch (issue #101 follow-up). GetService (not GetRequiredService) keeps
            // the composition fail-safe: a host that never registered the options runs with simulation OFF —
            // the real-PLC path — instead of failing to start its workers.
            var simulationOptions = scope.ServiceProvider.GetService<GatewaySimulationOptions>();

            var worker = new GatewayWorker(
                plcId,
                scope.ServiceProvider.GetRequiredService<ILogger<GatewayWorker>>(),
                scope.ServiceProvider.GetRequiredService<IGatewayCommandDispatcher>(),
                scope.ServiceProvider.GetRequiredService<IndTraceConfigurationService>(),
                scope.ServiceProvider.GetRequiredService<IHubConnectionFactory>(),
                scope.ServiceProvider.GetRequiredService<DateTimeMachine>(),
                hubConnection,
                scope.ServiceProvider.GetRequiredService<IPlcControllerFactory>())
            {
                PlcData = new PlcDto { PlcId = plcId },
                SimulationEnabled = simulationOptions?.EnableSimulation ?? false,
            };

            // Subscribe to the event (bind it to the existing SimulateCommand method)
            worker.OnCommandReceived += async (command) => await worker.SimulateCommand(command, cancellationToken).ConfigureAwait(false);

            // Start the worker. worker.StartAsync launches the BackgroundService and returns as soon as the
            // tracking loop hits its first await — so the returned Task only ever reflects *startup*, not the
            // long-running loop. Observing it (the old bug) meant a transient boot/hub blip was the only thing
            // that could ever fault it, while a genuine fault deep in the loop went unseen. We therefore await
            // startup here and then observe the REAL execution task (worker.ExecuteTask).
            this.logger.LogInformation("Worker task for PLC {PlcId} starting.", plcId);
            await worker.StartAsync(this.cts.Token).ConfigureAwait(false);

            var executionTask = worker.ExecuteTask;
            if (executionTask is null)
            {
                // BackgroundService always exposes ExecuteTask after StartAsync, but never assume in a
                // life-critical path: if it is missing the line is not actually being tracked.
                this.logger.LogCritical("Worker for PLC {PlcId} did not expose an execution task after start; the line is NOT being tracked.", plcId);
            }
            else
            {
                this.logger.LogInformation("Worker for PLC {PlcId} started Ok; observing its execution loop.", plcId);

                // Observe the real tracking loop. A fault here means that production line silently stopped
                // being tracked — surface it loudly, per line, without taking the rest of the plant down.
                _ = this.ObserveWorkerTask(plcId, executionTask);
                this.runningWorkers[plcId] = executionTask;
            }

            this.gatewayWorkers[plcId] = worker;
            this.workerScopes[plcId] = scope;
        }
        catch (Exception ex)
        {
            // A startup fault for this line (hub/config/DI resolution or the worker's StartAsync) is recorded
            // per-line and surfaced loudly — that line is UNHEALTHY, but the rest of the plant keeps tracking
            // (IndTrace TRACKS, it does not CONTROL). We do NOT rethrow: one line failing to start must never
            // strand the others, and this keeps a startup fault symmetric with a loop fault (ObserveWorkerTask).
            this.faultedWorkers[plcId] = ex;
            this.logger.LogCritical(
                ex,
                "Worker for PLC {PlcId} FAILED TO START and is NOT tracking its line. That line is now UNHEALTHY; the remaining lines keep tracking. Operator/monitor intervention required — the gateway is NOT taking the whole plant down for one line.",
                plcId);

            // Startup failed before the worker took ownership of the scope: dispose it so nothing leaks.
            scope.Dispose();
        }
    }

    /// <summary>
    /// Observes a worker's long-running execution task so a fault can never pass silently. On fault the
    /// manager records the line as unhealthy and logs critically (per-line observability). It deliberately
    /// does NOT stop the whole application: IndTrace TRACKS, it does not CONTROL, so one line's fault must
    /// never stop tracking the remaining lines. The transient conditions a healthy gateway sees constantly
    /// (hub or PLC briefly unreachable) are retried inside the loop and never reach here. Returns the
    /// continuation for test observation.
    /// </summary>
    internal Task ObserveWorkerTask(int plcId, Task executionTask)
    {
        return executionTask.ContinueWith(
            faulted =>
            {
                var error = faulted.Exception;
                if (error is not null)
                {
                    this.faultedWorkers[plcId] = error;
                }

                this.logger.LogCritical(
                    error,
                    "Worker for PLC {PlcId} FAULTED and stopped tracking its line. That line is now UNHEALTHY; the remaining lines keep tracking. Operator/monitor intervention required — the gateway is NOT taking the whole plant down for one line.",
                    plcId);
            },
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Stopping all workers...");
        await this.cts.CancelAsync().ConfigureAwait(false);

        // Stop each worker so it unsubscribes and disposes its hub connection (this must run for every
        // worker, including ones whose task already completed).
        foreach (var (plcId, worker) in this.gatewayWorkers)
        {
            try
            {
                await worker.StopAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error stopping worker for PLC {PlcId}.", plcId);
            }
        }

        try
        {
            await Task.WhenAll(this.runningWorkers.Values).ConfigureAwait(false);
            this.logger.LogInformation("All workers stopped cleanly.");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error while stopping one or more workers.");
        }

        // Dispose the per-worker DI scopes we held for the workers' lifetime.
        foreach (var (plcId, scope) in this.workerScopes)
        {
            try
            {
                scope.Dispose();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error disposing DI scope for PLC {PlcId}.", plcId);
            }
        }

        this.runningWorkers.Clear();
        this.gatewayWorkers.Clear();
        this.workerScopes.Clear();
        this.faultedWorkers.Clear();
        this.cts.Dispose();

        this.logger.LogInformation("All workers stopped.");
    }

    private async Task<List<string>> WaitForSimulationCommands(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var collectedLines = new List<string>();

        // Defensive guard: never touch Console.KeyAvailable when stdin is redirected (it throws).
        if (Console.IsInputRedirected)
        {
            return collectedLines;
        }

        var startTime = this.dateTimeMachine.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            if ((this.dateTimeMachine.UtcNow - startTime) > timeout)
            {
                break;
            }

            if (Console.KeyAvailable)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    // _logger.LogInformation("Empty input received — ending input collection.");
                    break;
                }

                collectedLines.Add(line);
            }

            await Task.Delay(100, cancellationToken).ConfigureAwait(false); // Polling delay
        }

        return collectedLines;
    }

    private async Task SimulateEventFromKeyboard(List<string> inputLines, CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        // Step 1: Parse all valid Request lines
        var commands = SimulatedCommand.ParseCommands(inputLines, this.logger);

        if (commands.Count == 0)
        {
            this.logger.LogTrace("No valid commands found.");
            return;
        }

        // Step 2: ProcessAsync each valid Request to its worker in parallel
        var simulationTasks = new List<Task>();

        foreach (var command in commands)
        {
            if (this.gatewayWorkers.TryGetValue(command.MachineId, out GatewayWorker? worker))
            {
                simulationTasks.Add(worker.TriggerCommand(command));
            }
            else
            {
                this.logger.LogWarning("No worker found for MachineId {MachineId}", command.MachineId);
            }
        }

        await Task.WhenAll(simulationTasks).ConfigureAwait(false);
    }

    private void DiscoverDuplicatedTags(ApplicationConfiguration configuration)
    {
        if (configuration?.Plcs == null)
        {
            throw new ArgumentNullException(nameof(configuration), "PLC configuration is missing.");
        }

        var criticalKeys = new[] { "PartNumber", "BarCode", "PlcId", "HeartBeat", "Command" };
        var errors = new List<PlcConfigurationException>();

        var plcGroupsByIp = configuration.Plcs
            .GroupBy(p => p.IpAddress)
            .Where(g => g.Count() > 1);

        foreach (var group in plcGroupsByIp)
        {
            foreach (var key in criticalKeys)
            {
                var addressGroups = group
                    .Where(plc => plc.Variables.ContainsKey(key))
                    .GroupBy(plc => plc.Variables[key].Address);

                foreach (var addressGroup in addressGroups)
                {
                    if (addressGroup.Count() > 1)
                    {
                        var ip = group.Key;
                        var duplicateAddress = addressGroup.Key;
                        var affectedPlcs = string.Join(", ", addressGroup.Select(p => p.PlcId));

                        var message = $"FATAL CONFIGURATION ERROR: PLCs with IP '{ip}' share address '{duplicateAddress}' for key '{key}'. Affected PLCs: {affectedPlcs}.";

                        this.logger.LogCritical("{Message} Application will terminate.", message);

                        errors.Add(new PlcConfigurationException(ip, key, duplicateAddress, message));
                    }
                }
            }
        }

        if (errors.Any())
        {
            var combinedMessage = string.Join(Environment.NewLine, errors.Select(e => e.Message));
            throw new AggregateException("PLC configuration errors found:\n" + combinedMessage, errors);
        }
    }
}
