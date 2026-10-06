// <copyright file="TaskGatewayRequest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using System.Xml.Linq;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a request for a gateway task, including machine, barcode, part, status, and command information.
/// </summary>
/// <remarks>
/// Aggregate root (#95 Phase 2, ratified boundary map): its sole member is the 1:1
/// <c>TaskGatewayResponse</c> (linked by <c>CommandId</c>). The response write currently stays on a direct
/// repository inside <c>GatewayPersistenceBehavior</c> under the §7 record-first waiver (see
/// <c>AggregateWriteEnforcementTests.WaivedBoundaryPairs</c>); the aggregate unit of work is Slice F,
/// deferred on the #95 plan. (<see cref="IAggregateRoot"/> derives from <see cref="IEntityRoot"/> :
/// <c>IPersistable</c>, so repository access stays valid.)
/// </remarks>
public class TaskGatewayRequest : IMonitorFilter, IAggregateRoot
{
    // Constructors (before properties)
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID and part number.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="partNumber">The part number.</param>
    public TaskGatewayRequest(int machineId, string partNumber)
    {
        this.MachineId = machineId;
        this.PartNumber = partNumber;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayRequest"/> class.
    /// </summary>
    /// <param name="gatewayTask">The gateway task.</param>
    public TaskGatewayRequest(GatewayTask gatewayTask)
    {
        this.GatewayTask = gatewayTask;
    }

    public TaskGatewayRequest()
    {
    }

    /// <summary>
    /// Gets or sets the command identifier.
    /// </summary>
    public int CommandId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the name of the request.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp for the request.
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets the part number associated with the request.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the request.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the barcode identifier associated with the request.
    /// </summary>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the cycle identifier associated with the request.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the cycle status for the request.
    /// </summary>
    public CycleStatus CycleStatus { get; set; } = CycleStatus.None;

    /// <summary>
    /// Gets or sets the machine type for the request.
    /// </summary>
    public MachineType MachineType { get; set; } = MachineType.None;

    /// <summary>
    /// Gets or sets the part status for the request.
    /// </summary>
    public PartStatus PartStatus { get; set; } = PartStatus.None;

    /// <summary>
    /// Gets or sets the flow status for the request.
    /// </summary>
    public FlowStatus FlowStatus { get; set; } = FlowStatus.None;

    /// <summary>
    /// Gets or sets the result validation status for the request.
    /// </summary>
    public ResultValidation ResultValidation { get; set; } = ResultValidation.None;

    /// <summary>
    /// Gets or sets the gateway task for the request.
    /// </summary>
    public GatewayTask GatewayTask { get; set; } = GatewayTask.None;

    /// <summary>
    /// Gets or sets the request task name.
    /// </summary>
    public string RequestTask { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dictionary of registers associated with the request.
    /// </summary>
    public IDictionary<string, Register> Registers { get; set; } = new Dictionary<string, Register>();

    /// <summary>
    /// Gets or sets additional comments for the request.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the watchdog timer status.
    /// </summary>
    public WatchDog WatchDogTime { get; set; } = WatchDog.Enable;

    /// <summary>
    /// Gets or sets the barcode value for the request.
    /// </summary>
    public string BarCode { get; set; } = string.Empty; // Set by EF or by builder on runtime, consumer must check for string.Empty before accessing.

    /// <summary>
    /// Gets or sets the product identifier associated with the request.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request has been completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the event status for the request.
    /// </summary>
    public string EventStatus { get; set; } = string.Empty; // Set by EF or by builder on runtime, consumer must check for string.Empty before accessing.

    /// <summary>
    /// Gets or sets the status color for the request.
    /// </summary>
    public string StatusColor { get; set; } = string.Empty; // Set by EF or by builder on runtime, consumer must check for string.Empty before accessing.

    /// <summary>
    /// Gets or sets the error message for the request.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dictionary of parameters for the request.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = [];
    // Static factory methods (before instance methods)
    /// <summary>
    /// Sets the command status based on the specified task name.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    public void SetCommandStatusFromTask(string taskName)
    {
        switch (taskName)
        {
            case var name when name == GatewayTask.ReadBarCodeAsync.Name:
                this.SetStatusReadBarCode();
                break;

            case var name when name == GatewayTask.CreateBarCodeAsync.Name:
                this.SetStatusCreateBarCode();
                break;

            case var name when name == GatewayTask.CreateCycleAsync.Name:
                this.SetStatusCreateCycle();
                break;

            case var name when name == GatewayTask.UpdateCycleOkAsync.Name:
                this.SetStatusUpdateCycleOk();
                break;

            case var name when name == GatewayTask.UpdateCycleNotOkAsync.Name:
                this.SetStatusUpdateCycleNotOk();
                break;

            case var name when name == GatewayTask.RejectPartAsync.Name:
                this.SetStatusRejectPart();
                break;

            case var name when name == GatewayTask.EndOfProcessAsync.Name:
                this.SetStatusEndOfProcess();
                break;
        }
    }

    /// <summary>
    /// Sets the status for reading a barcode.
    /// </summary>
    public void SetStatusReadBarCode()
    {
        this.CycleStatus = CycleStatus.NotStarted;
        this.PartStatus = PartStatus.Ok;
    }

    /// <summary>
    /// Sets the status for creating a barcode.
    /// </summary>
    public void SetStatusCreateBarCode()
    {
        this.CycleStatus = CycleStatus.NotStarted;
        this.PartStatus = PartStatus.Ok;
    }

    /// <summary>
    /// Sets the status for creating a cycle.
    /// </summary>
    public void SetStatusCreateCycle()
    {
        this.CycleStatus = CycleStatus.Started;
        this.PartStatus = PartStatus.Ok;
    }

    /// <summary>
    /// Sets the status for updating a cycle as OK.
    /// </summary>
    public void SetStatusUpdateCycleOk()
    {
        this.CycleStatus = CycleStatus.FinishedOk;
        this.PartStatus = PartStatus.Ok;
    }

    /// <summary>
    /// Sets the status for updating a cycle as not OK.
    /// </summary>
    public void SetStatusUpdateCycleNotOk()
    {
        this.CycleStatus = CycleStatus.FinishedNok;
        this.PartStatus = PartStatus.NOk;
    }

    /// <summary>
    /// Sets the status for rejecting a part.
    /// </summary>
    public void SetStatusRejectPart()
    {
        this.CycleStatus = CycleStatus.Rejected;
        this.PartStatus = PartStatus.Rejected;
    }

    /// <summary>
    /// Sets the status for the end of the process.
    /// </summary>
    /// <remarks>
    /// Story 3.2 (FR6): this is the LEGACY divergent projection (EndOfProcess(16)/NOk(2)) that disagrees
    /// with the persisted truth (Finished/FinishedOk/Ok). It remains the reachable path when the
    /// <c>RouteEndOfProcess</c> feature flag is OFF (rollback). When the flag is ON, the
    /// <c>UpdateBarCodeCommandHandler</c> overwrites this projection with the converged machine outcome via
    /// <see cref="ProjectOutcome(FlowStatus, CycleStatus, PartStatus, ResultValidation)"/>.
    /// </remarks>
    public void SetStatusEndOfProcess()
    {
        this.CycleStatus = CycleStatus.EndOfProcess;
        this.PartStatus = PartStatus.NOk;
    }

    /// <summary>
    /// Story 3.2 (AC2/AC3): read-only projection of a state-machine <c>TransitionOutcome</c> onto the
    /// PLC-facing request, so the projection becomes a VIEW of the persisted outcome rather than an
    /// independent assertion. Used by <c>UpdateBarCodeCommandHandler</c> for the converged EndOfProcess
    /// path (Finished/FinishedOk/Ok); makes the projected tuple equal the persisted tuple — exactly one result.
    /// </summary>
    /// <param name="flowStatus">The resolved flow status from the machine outcome.</param>
    /// <param name="cycleStatus">The resolved cycle status from the machine outcome.</param>
    /// <param name="partStatus">The resolved part status from the machine outcome.</param>
    /// <param name="resultValidation">The resolved result-validation diagnostic from the machine outcome.</param>
    public void ProjectOutcome(FlowStatus flowStatus, CycleStatus cycleStatus, PartStatus partStatus, ResultValidation resultValidation)
    {
        this.FlowStatus = flowStatus;
        this.CycleStatus = cycleStatus;
        this.PartStatus = partStatus;
        this.ResultValidation = resultValidation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, part number, cycle status, and part status.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="cycleStatus">The cycle status.</param>
    /// <param name="partStatus">The part status.</param>
    public TaskGatewayRequest(int machineId, string partNumber, CycleStatus cycleStatus, PartStatus partStatus)
    {
        this.MachineId = machineId;
        this.PartNumber = partNumber;
        this.CycleStatus = cycleStatus;
        this.PartStatus = partStatus;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, part number, and timestamp.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="timeStamp">The timestamp.</param>
    public TaskGatewayRequest(int machineId, string partNumber, DateTime timeStamp)
    {
        this.MachineId = machineId;
        this.PartNumber = partNumber;
        this.TimeStamp = timeStamp;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, barcode, part number, part status, and cycle status.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCode">The barcode.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="partStatus">The part status.</param>
    /// <param name="cycleStatus">The cycle status.</param>
    public TaskGatewayRequest(int machineId, string barCode, string partNumber, PartStatus partStatus, CycleStatus cycleStatus)
    {
        this.MachineId = machineId;
        this.BarCode = barCode;
        this.PartNumber = partNumber;
        this.CycleStatus = cycleStatus;
        this.PartStatus = partStatus;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, part number, and cycle status.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="cycleStatus">The cycle status.</param>
    public TaskGatewayRequest(int machineId, string partNumber, CycleStatus cycleStatus)
    {
        this.MachineId = machineId;
        this.PartNumber = partNumber;
        this.CycleStatus = cycleStatus;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, part number, and barcode.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="barCode">The barcode.</param>
    public TaskGatewayRequest(int machineId, string partNumber, string barCode)
    {
        this.MachineId = machineId;
        this.PartNumber = partNumber;
        this.BarCode = barCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, part number, barcode, and part status.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="barCode">The barcode.</param>
    /// <param name="partStatus">The part status.</param>
    public TaskGatewayRequest(int machineId, string partNumber, string barCode, PartStatus partStatus)
    {
        this.MachineId = machineId;
        this.PartNumber = partNumber;
        this.BarCode = barCode;
        this.PartStatus = partStatus;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, part number, and machine type.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="machineType">The machine type.</param>
    public TaskGatewayRequest(int machineId, string partNumber, MachineType machineType)
    {
        this.MachineId = machineId;
        this.PartNumber = partNumber;
        this.MachineType = machineType;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TaskGatewayRequest"/> class from PLC data.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCode">The barcode.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="cycleStatus">The cycle status.</param>
    /// <param name="partStatus">The part status.</param>
    /// <param name="gatewayTask">The gateway task.</param>
    /// <param name="timeStamp">The timestamp.</param>
    /// <returns>A new instance of the <see cref="TaskGatewayRequest"/> class.</returns>
    public static TaskGatewayRequest FromPlc(int machineId, string barCode, string partNumber,
            CycleStatus cycleStatus, PartStatus partStatus,
            GatewayTask gatewayTask, DateTime timeStamp)
    {
        return new TaskGatewayRequest(gatewayTask)
        {
            MachineId = machineId,
            BarCode = barCode,
            PartNumber = partNumber,
            CycleStatus = cycleStatus,
            PartStatus = partStatus,
            GatewayTask = gatewayTask,
            RequestTask = gatewayTask.Name,
            TimeStamp = timeStamp,
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TaskGatewayRequest"/> class from PLC data.
    /// </summary>
    /// <param name="dataPlc">The PLC data.</param>
    /// <param name="name">The name of the request.</param>
    /// <param name="gatewayTask">The gateway task.</param>
    /// <param name="timeStamp">The timestamp.</param>
    /// <returns>A new instance of the <see cref="TaskGatewayRequest"/> class.</returns>
    public static TaskGatewayRequest FromPlc(DataFromPlc dataPlc, string name, GatewayTask gatewayTask, DateTime timeStamp)
    {
        return new TaskGatewayRequest
        {
            Name = name,
            MachineId = dataPlc.MachineId,
            BarCode = dataPlc.BarCode,
            PartNumber = dataPlc.PartNumber,
            CycleStatus = dataPlc.CycleStatus,
            PartStatus = dataPlc.PartStatus,
            WatchDogTime = dataPlc.WatchDogTime,
            GatewayTask = gatewayTask,
            RequestTask = gatewayTask.Name,
            TimeStamp = timeStamp,
        };
    }
    /// <summary>
    /// Creates a new instance of the <see cref="TaskGatewayRequest"/> class.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barcode">The barcode.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="partStatus">The part status.</param>
    /// <param name="cycleStatus">The cycle status.</param>
    /// <param name="registers">The dictionary of registers.</param>
    /// <returns>A new instance of the <see cref="TaskGatewayRequest"/> class.</returns>
    public static TaskGatewayRequest Create(
        int machineId,
        string barcode,
        string partNumber,
        PartStatus partStatus,
        CycleStatus cycleStatus,
        Dictionary<string, Register> registers)
    {
        return new TaskGatewayRequest
        {
            MachineId = machineId,
            BarCode = barcode,
            PartNumber = partNumber,
            PartStatus = partStatus,
            CycleStatus = cycleStatus,
            Registers = registers,
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID and part number.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="partNumber">The part number.</param>
    /// <returns>A new instance of the <see cref="TaskGatewayRequest"/> class.</returns>
    public static TaskGatewayRequest CreateWithPartNumber(int machineId, string partNumber)
    {
        return new TaskGatewayRequest(machineId, partNumber, string.Empty);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID and barcode.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCode">The barcode.</param>
    /// <returns>A new instance of the <see cref="TaskGatewayRequest"/> class.</returns>
    public static TaskGatewayRequest CreateWithLabel(int machineId, string barCode)
    {
        return new TaskGatewayRequest(machineId, string.Empty, barCode);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, barcode, and part number.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCode">The barcode.</param>
    /// <param name="partNumber">The part number.</param>
    /// <returns>A new instance of the <see cref="TaskGatewayRequest"/> class.</returns>
    public static TaskGatewayRequest Create(int machineId, string barCode, string partNumber)
    {
        return new TaskGatewayRequest(machineId, partNumber, barCode);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, barcode, part number, and part status.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCode">The barcode.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="partStatus">The part status.</param>
    /// <returns>A new instance of the <see cref="TaskGatewayRequest"/> class.</returns>
    public static TaskGatewayRequest Create(int machineId, string barCode, string partNumber, PartStatus partStatus)
    {
        return new TaskGatewayRequest(machineId, partNumber, barCode, partStatus);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TaskGatewayRequest"/> class with the specified machine ID, barcode, part number, part status, and cycle status.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCode">The barcode.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="partStatus">The part status.</param>
    /// <param name="cycleStatus">The cycle status.</param>
    /// <returns>A new instance of the <see cref="TaskGatewayRequest"/> class.</returns>
    public static TaskGatewayRequest Create(int machineId, string barCode, string partNumber, PartStatus partStatus, CycleStatus cycleStatus)
    {
        return new TaskGatewayRequest(machineId, barCode, partNumber, partStatus, cycleStatus);
    }

    /// <summary>
    /// Converts the specified <see cref="TaskGatewayRequest"/> instance to a wire <see cref="TaskGatewayResponseDto"/>.
    /// </summary>
    /// <param name="src">The source request.</param>
    /// <returns>A new instance of the <see cref="TaskGatewayResponseDto"/> class.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the source request is null.</exception>
    public static TaskGatewayResponseDto ToResponse(TaskGatewayRequest src)
    {
        ArgumentNullException.ThrowIfNull(src);
        return new TaskGatewayResponseDto
        {
            ExecutionTime = TimeSpan.Zero, // Set as needed
            ResponseId = 0, // Set as needed
            MachineId = src.MachineId,
            BarCodeId = src.BarCodeId,
            CycleId = src.CycleId,

            CommandId = src.CommandId,
            Name = src.Name,
            PartNumber = src.PartNumber,
            Description = src.Description,
            Label = src.BarCode,
            Error = src.Error,

            CycleStatus = src.CycleStatus,
            MachineType = src.MachineType,
            PartStatus = src.PartStatus,
            FlowStatus = src.FlowStatus,
            ResultValidation = src.ResultValidation,
            RequestTask = src.RequestTask,

            TimeStamp = src.TimeStamp,
            References = src.Registers,
            Parameters = src.Parameters,
        };
    }

    // Story 27.2b-2 (#27/F4): the dead `FromResponse(TaskGatewayResponse)` factory was removed here. It had ZERO
    // production callers (only a class-diagram reference), and the 2b-2 retype had introduced an UNPINNED absent-
    // barcode byte-drift on it (`src.BarCode?.Label.Value ?? src.Label`). Rather than leave latent drift on dead
    // code, the method was deleted outright. If a Response->Request projection is ever needed again, reintroduce it
    // with a golden-master pinning the absent-barcode case.

    // Instance methods (after static methods)
    /// <summary>
    /// Ensures the request is valid for rendering and persistence: null smart-enum members are defaulted
    /// to their <c>.None</c> sentinel (tolerated wire semantics), and required non-enum members are
    /// verified fail-loud.
    /// </summary>
    /// <remarks>
    /// Issue #126 (F3), PO decision 2026-07-15: this method previously returned <see langword="true"/>
    /// unconditionally, masking EF-materialization nulls in required members behind the enum coalescing.
    /// It now returns <see langword="false"/> when the request is genuinely unrenderable/unpersistable:
    /// <list type="bullet">
    /// <item><description><see cref="Comment"/> — backs the NOT NULL <c>Comment</c> column on
    /// <c>TaskGatewayRequests</c> (<c>TaskRequestEventConfiguration</c>: required, max 120).</description></item>
    /// <item><description><see cref="BarCode"/> — projected onto the wire response <c>Label</c>, a NOT NULL
    /// column on <c>TaskGatewayResponses</c> (required, max 100).</description></item>
    /// <item><description><see cref="PartNumber"/> — projected onto the wire response <c>PartNumber</c>,
    /// NOT NULL (required, max 50).</description></item>
    /// <item><description><see cref="Error"/> — projected onto the wire response <c>Error</c>, NOT NULL
    /// (required, max 240).</description></item>
    /// </list>
    /// The six smart-enum members keep the pre-existing null-to-None defaulting and never fail the verdict;
    /// the defaulting is applied even when the verdict is <see langword="false"/>. The §7 success-path
    /// emission is unchanged: a request with all required members present behaves exactly as before.
    /// </remarks>
    /// <returns>True if the request is renderable and persistable; otherwise, false.</returns>
    public bool EnsureIsValidToRenderAndPersist()
    {
        this.FlowStatus ??= FlowStatus.None;
        this.ResultValidation ??= ResultValidation.None;

        this.CycleStatus ??= CycleStatus.None;
        this.PartStatus ??= PartStatus.None;
        this.GatewayTask ??= GatewayTask.None;
        this.MachineType ??= MachineType.None;

        // The members below are declared non-nullable but can be nulled by EF materialization or wire
        // deserialization; a null here makes the SQL insert (request row) or the wire projection
        // (response row) throw, so the verdict must be false — callers branch on it.
        return this.BarCode is not null
            && this.PartNumber is not null
            && this.Comment is not null
            && this.Error is not null;
    }

    /// <summary>
    /// Marks the request as successful and sets the barcode.
    /// </summary>
    /// <param name="barCode">The barcode.</param>
    public void Success(string barCode)
    {
        this.IsEnabled = true;
        this.EventStatus = "Success";
        this.StatusColor = "Success";
        this.BarCode = barCode;
    }

    /// <summary>
    /// Marks the request as failed and sets the barcode.
    /// </summary>
    /// <param name="barCode">The barcode.</param>
    public void Failure(string barCode)
    {
        this.IsEnabled = true;
        this.EventStatus = "WithFailure";
        this.StatusColor = "WithFailure";
        this.BarCode = barCode;
    }

    /// <summary>
    /// Displau a nice string representation.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{this.MachineId} + {this.Name} ";
    }
}
