// <copyright file="SimulatedControllerRx.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Plc;

using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using IndTrace.Application.Performance.Request.Command.Create;

/// <summary>
/// In-memory PLC controller for the community edition and for demos/rigs without equipment. It holds the
/// PLC's tag image in memory and behaves like a well-behaved PLC: a <see cref="SimulateCommandAsync"/> writes
/// the part data, then raises <see cref="CommandChanged"/> exactly when the command value changes (the
/// OnChange notification semantics of a real driver), and the gateway reads the same data back through
/// <see cref="UploadCommandDataFromController"/>. It never touches a network.
/// </summary>
/// <remarks>
/// The controller configuration is validated like a real driver validates it (reference, register and the four
/// event tags must be configured), so a mis-configured PLC fails setup in simulation exactly as it would on
/// the line.
/// </remarks>
public sealed class SimulatedControllerRx : IIndTraceControllerRx, IDisposable
{
    private const int RequiredEventTagCount = 4;
    private const string PartStatusPlcTag = "PartStatusPlc";
    private const string CycleStatusPlcTag = "CycleStatusPlc";
    private const string CommandFeedbackTag = "CommandFeedback";

    private readonly ILogger logger;
    private readonly IDateTimeMachine dateTimeMachine;
    private readonly Subject<IIndTraceControllerRx> commandChanged = new();
    private readonly Subject<IIndTraceControllerRx> heartBeatChanged = new();
    private readonly CompositeDisposable subscriptions = [];
    private readonly Dictionary<string, object> tags = new(StringComparer.Ordinal);
    private readonly Lock tagsLock = new();
    private int notificationsCreated;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedControllerRx"/> class.
    /// </summary>
    /// <param name="logger">The logger the controller reports through.</param>
    /// <param name="plcDetails">The PLC configuration being simulated.</param>
    /// <param name="dateTimeMachine">The deterministic time source.</param>
    public SimulatedControllerRx(ILogger logger, PlcDto plcDetails, IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(plcDetails);
        ArgumentNullException.ThrowIfNull(dateTimeMachine);

        this.logger = logger;
        this.dateTimeMachine = dateTimeMachine;
        this.PlcDetails = plcDetails;
        this.IsOeeEnabled = plcDetails.HasOeeEnabled;
        this.PlcId = plcDetails.PlcId;
        this.MachineId = plcDetails.MachineId;
        this.Name = plcDetails.Name;
    }

    /// <summary>
    /// Gets or sets the scheduler driving the simulated heartbeat. Internal so tests can use a virtual clock.
    /// </summary>
    internal IScheduler HeartBeatScheduler { get; set; } = DefaultScheduler.Instance;

    /// <summary>
    /// Gets or sets the simulated heartbeat period.
    /// </summary>
    internal TimeSpan HeartBeatPeriod { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the pause between writing the part data and raising the command, mirroring the settle
    /// time a real PLC program leaves between the data block and the command edge.
    /// </summary>
    internal TimeSpan CommandSettleDelay { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <inheritdoc/>
    public PlcDto PlcDetails { get; }

    /// <inheritdoc/>
    public bool IsOeeEnabled { get; }

    /// <inheritdoc/>
    public string BarCode { get; private set; } = string.Empty;

    /// <inheritdoc/>
    public short Command { get; private set; }

    /// <inheritdoc/>
    public IObservable<IIndTraceControllerRx> CommandChanged => this.commandChanged.AsObservable();

    /// <inheritdoc/>
    public IObservable<IIndTraceControllerRx> HeartBeatChanged => this.heartBeatChanged.AsObservable();

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    public bool IsInitialized { get; private set; }

    /// <inheritdoc/>
    public int MachineId { get; private set; }

    /// <inheritdoc/>
    public string PartNumber { get; private set; } = string.Empty;

    /// <inheritdoc/>
    public bool Configured { get; private set; }

    /// <inheritdoc/>
    public short HeartBeat { get; private set; }

    /// <inheritdoc/>
    public PartStatus PartStatus { get; private set; } = PartStatus.None;

    /// <inheritdoc/>
    public CycleStatus CycleStatus { get; private set; } = CycleStatus.None;

    /// <inheritdoc/>
    public string Name { get; private set; }

    /// <inheritdoc/>
    public int PlcId { get; private set; }

    /// <inheritdoc/>
    public int CyclesOk { get; private set; }

    /// <inheritdoc/>
    public IDictionary<string, Register> References { get; set; } = new Dictionary<string, Register>();

    /// <inheritdoc/>
    public IDictionary<string, Register> Registers { get; set; } = new Dictionary<string, Register>();

    /// <inheritdoc/>
    public bool Retry { get; set; }

    /// <summary>
    /// Gets the last command feedback value the gateway wrote.
    /// </summary>
    public short CommandFeedback { get; private set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        this.subscriptions.Dispose();
        this.commandChanged.OnCompleted();
        this.heartBeatChanged.OnCompleted();
        this.commandChanged.Dispose();
        this.heartBeatChanged.Dispose();
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"PlcId: {this.PlcId} (simulated) MachineId: {this.MachineId} IsConnected: {this.IsConnected} IsInitialized: {this.IsInitialized} Configured: {this.Configured}";

    /// <inheritdoc/>
    public Task<bool> SetUpAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(false);
        }

        this.Configured = false;
        this.IsInitialized = false;

        var variables = this.PlcDetails.Variables;
        if (variables is null || this.PlcDetails.VariablesGroups is null)
        {
            this.logger.LogError("Simulated PLC {PlcId}: configuration is missing Variables or VariablesGroups.", this.PlcId);
            return Task.FromResult(false);
        }

        var eventTagCount = variables.Count(kvp => kvp.Value.VariableGroupId == TagsGroups.EventTags);
        if (eventTagCount != RequiredEventTagCount)
        {
            this.logger.LogError(
                "Simulated PLC {PlcId}: the gateway needs exactly {Required} event tags, found {Found}.",
                this.PlcId,
                RequiredEventTagCount,
                eventTagCount);
            return Task.FromResult(false);
        }

        var references = this.BuildRegisterTags(variables, TagsGroups.ReferenceTags);
        var registers = this.BuildRegisterTags(variables, TagsGroups.RegisterTags);
        if (references is null || registers is null)
        {
            return Task.FromResult(false);
        }

        this.References = references;
        this.Registers = registers;

        lock (this.tagsLock)
        {
            this.tags.Clear();
            foreach (var variable in variables.Values)
            {
                this.tags[variable.Name] = DefaultValueFor(variable.NetType);
            }

            this.tags[nameof(this.PlcId)] = (short)this.PlcId;
        }

        this.Configured = true;
        this.logger.LogInformation("Simulated PLC {PlcId} ({Name}) configured with {Count} tags.", this.PlcId, this.Name, variables.Count);
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<bool> ValidateThatTheTagExistOnTheController(CancellationToken cancellationToken)
    {
        // Every configured tag exists in the in-memory image by construction (SetUpAsync seeds it).
        return Task.FromResult(!cancellationToken.IsCancellationRequested && this.Configured);
    }

    /// <inheritdoc/>
    public Task<bool> ConnectAndCreateNotificationsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || !this.Configured || this.disposed)
        {
            return Task.FromResult(false);
        }

        this.IsConnected = true;
        this.IsInitialized = true;

        // Wire the heartbeat exactly once, however many times the gateway reconnects.
        if (Interlocked.CompareExchange(ref this.notificationsCreated, 1, 0) == 0)
        {
            this.subscriptions.Add(
                Observable.Interval(this.HeartBeatPeriod, this.HeartBeatScheduler)
                    .Subscribe(_ =>
                    {
                        this.HeartBeat = (short)(this.HeartBeat == short.MaxValue ? 0 : this.HeartBeat + 1);
                        this.heartBeatChanged.OnNext(this);
                    }));
        }

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<int> SetPlcIdAsync(short value, CancellationToken cancellationToken)
    {
        this.WriteTag(nameof(this.PlcId), value);
        return Task.FromResult(this.PlcId);
    }

    /// <inheritdoc/>
    public Task<int> GetPlcIdAsync(CancellationToken cancellationToken)
    {
        this.PlcId = this.ReadTag<short>(nameof(this.PlcId));
        return Task.FromResult(this.PlcId);
    }

    /// <inheritdoc/>
    public Task<short> ReadShortTagAsync(string tagName, CancellationToken cancellationToken) =>
        Task.FromResult(this.ReadTag<short>(tagName));

    /// <inheritdoc/>
    public Task<string> ReadStringTagAsync(string tagName, CancellationToken cancellationToken) =>
        Task.FromResult(this.ReadTag<string>(tagName));

    /// <inheritdoc/>
    public Task<string> SetFeedBackAsync(short value, CancellationToken cancellationToken)
    {
        this.CommandFeedback = value;
        this.WriteTag(CommandFeedbackTag, value);
        return Task.FromResult(string.Empty);
    }

    /// <inheritdoc/>
    public Task<string> SetBarCodeAsync(string value, CancellationToken cancellationToken)
    {
        this.BarCode = value ?? string.Empty;
        this.WriteTag(nameof(this.BarCode), this.BarCode);
        return Task.FromResult(this.BarCode);
    }

    /// <inheritdoc/>
    public Task<string> ResetCommandAsync(CancellationToken cancellationToken)
    {
        this.SetCommand(0);
        return Task.FromResult(string.Empty);
    }

    /// <inheritdoc/>
    public async Task<string> SimulateCommandAsync(SimulatedCommand command, CancellationToken cancellationToken)
    {
        if (command is null || cancellationToken.IsCancellationRequested || this.disposed)
        {
            return string.Empty;
        }

        this.PartNumber = command.PartNumber ?? string.Empty;
        this.BarCode = command.BarCode ?? string.Empty;
        this.PartStatus = command.PartStatus;
        this.CycleStatus = command.CycleStatus;

        this.WriteTag(nameof(this.PartNumber), this.PartNumber);
        this.WriteTag(nameof(this.BarCode), this.BarCode);
        this.WriteTag(PartStatusPlcTag, command.PartStatusPlc);
        this.WriteTag(CycleStatusPlcTag, command.CycleStatusPlc);
        this.WriteTag(nameof(this.PartStatus), command.PartStatus);
        this.WriteTag(nameof(this.CycleStatus), command.CycleStatus);

        this.logger.LogInformation(
            "Simulated PLC {PlcId}: part {PartNumber} / {BarCode} written; raising command {Command}.",
            this.PlcId,
            this.PartNumber,
            this.BarCode,
            command.Command);

        await Task.Delay(this.CommandSettleDelay, cancellationToken).ConfigureAwait(false);

        this.SetCommand(command.Command);
        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<SimulatedCommand> ReadStartUp(CancellationToken cancellationToken)
    {
        var command = new SimulatedCommand
        {
            MachineId = this.MachineId,
            PartNumber = this.PartNumber,
            BarCode = this.BarCode,
            Command = 8,
            PartStatus = this.PartStatus,
            CycleStatus = this.CycleStatus,
            PartStatusPlc = (short)this.PartStatus.Value,
            CycleStatusPlc = (short)this.CycleStatus.Value,
        };

        await this.SimulateCommandAsync(command, cancellationToken).ConfigureAwait(false);
        return command;
    }

    /// <inheritdoc/>
    public Task<Result> DownloadReferencesBulkAsync(CancellationToken cancellationToken) =>
        Task.FromResult(cancellationToken.IsCancellationRequested ? Result.WithFailure("Operation was cancelled.") : Result.Success());

    /// <inheritdoc/>
    public Task<Result> ReadRegistersBulkAsync(CancellationToken cancellationToken) =>
        Task.FromResult(cancellationToken.IsCancellationRequested ? Result.WithFailure("Operation was cancelled.") : Result.Success());

    /// <inheritdoc/>
    public Task<Result<DataFromPlc>> UploadCommandDataFromController(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Result<DataFromPlc>.WithFailure("Operation was cancelled."));
        }

        var data = new DataFromPlc
        {
            MachineId = this.MachineId,
            Command = this.Command,
            BarCode = this.BarCode,
            PartNumber = this.PartNumber,
            WatchDogTime = WatchDog.Enable,
        };

        return Task.FromResult(Result<DataFromPlc>.Success(data));
    }

    /// <inheritdoc/>
    public Task<Result<PerformanceDataCommand>> ReadPerformanceDataFromPlcAsync(CancellationToken cancellationToken)
    {
        var performance = new PerformanceDataCommand
        {
            MachineId = this.MachineId,
            PlcId = this.PlcId,
        };

        return Task.FromResult(cancellationToken.IsCancellationRequested
            ? Result<PerformanceDataCommand>.WithFailure("Operation was cancelled.", performance)
            : Result<PerformanceDataCommand>.Success(performance));
    }

    /// <inheritdoc/>
    public Task<ControllerMonitor> GetPlcMonitorAsync(CancellationToken cancellationToken)
    {
        var monitor = new ControllerMonitor
        {
            PlcId = this.PlcId,
            MachineId = this.PlcDetails.MachineId,
            Description = this.PlcDetails.Name,
            Name = this.Name,
            HeartBeat = this.HeartBeat,
            IpAddress = this.PlcDetails.IpAddress,
            PartNumber = this.PartNumber,
            Label = this.BarCode,
            CyclesOk = this.CyclesOk,
            TimeStamp = this.dateTimeMachine.Now.ToLocalTime(),
        };

        return Task.FromResult(monitor);
    }

    private static object DefaultValueFor(string? netType) => netType switch
    {
        "System.String" => string.Empty,
        "System.Int16" => (short)0,
        "System.Int32" => 0,
        "System.Boolean" => false,
        "System.Single" => 0f,
        _ => string.Empty,
    };

    private Dictionary<string, Register>? BuildRegisterTags(IDictionary<string, Variable> variables, TagsGroups group)
    {
        var registers = new Dictionary<string, Register>(StringComparer.Ordinal);
        foreach (var variable in variables.Values.Where(v => v.VariableGroupId == group))
        {
            var created = Register.Create(
                name: variable.Name,
                description: string.Empty,
                machineId: 0,
                variableId: variable.VariableId,
                cycleId: 0,
                value: string.Empty,
                dataType: variable.NetType,
                statusValueId: 1,
                timeStamp: default);

            if (created.IsFailure || created.Value is null || !registers.TryAdd(variable.Name, created.Value))
            {
                this.logger.LogError("Simulated PLC {PlcId}: {Group} tag '{Tag}' is invalid or duplicated.", this.PlcId, group.Name, variable.Name);
                return null;
            }
        }

        if (registers.Count == 0)
        {
            this.logger.LogError("Simulated PLC {PlcId}: the gateway needs at least one {Group} tag.", this.PlcId, group.Name);
            return null;
        }

        return registers;
    }

    private void SetCommand(short value)
    {
        var changed = this.Command != value;
        this.Command = value;
        this.WriteTag(nameof(this.Command), value);

        // OnChange semantics: a real driver notifies only on a value change.
        if (changed && !this.disposed)
        {
            this.commandChanged.OnNext(this);
        }
    }

    private void WriteTag(string tagName, object value)
    {
        lock (this.tagsLock)
        {
            this.tags[tagName] = value;
        }
    }

    private T ReadTag<T>(string tagName)
    {
        object? value;
        lock (this.tagsLock)
        {
            if (!this.tags.TryGetValue(tagName, out value))
            {
                // Same contract as a driver: an unknown tag is a failed read, never a fabricated default.
                throw new KeyNotFoundException($"Simulated PLC {this.PlcId}: tag '{tagName}' is not configured.");
            }
        }

        return value is T typed
            ? typed
            : (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
    }
}
