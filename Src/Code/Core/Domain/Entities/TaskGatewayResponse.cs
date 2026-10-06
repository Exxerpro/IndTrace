// <copyright file="TaskGatewayResponse.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

/// <summary>
/// Persisted EF row for a gateway task response. Story 32.C2 (#32/F9) split the former mutable god-object into the
/// immutable wire <see cref="TaskGatewayResponseDto"/> (built by every §7 producer and read by every consumer) and
/// this entity, which carries ONLY the 17 columns <c>TaskGatewayResponseConfiguration</c> maps to
/// <c>TaskGatewayResponses</c>. The 12 wire-only members (References, Recipe, Cycle, BarCode, MasterLabel,
/// MachineType, WorkFlowType, Description, ExecutionTime, RequestTask, Parameters, Name, PlcId) live on the DTO only.
/// It is built solely by the down-projection mapper <see cref="TaskGatewayResponsePersistence.ToEntity"/>; the
/// persist step is intentionally lossy but byte-identical to the pre-split stored row.
/// </summary>
public class TaskGatewayResponse : IEntityRoot
{
    private readonly IDateTimeMachine dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayResponse"/> class with an optional date time machine.
    /// </summary>
    /// <param name="dateTimeMachine">The date time machine to use for timestamp defaults. Defaults to a new instance when null.</param>
    public TaskGatewayResponse(IDateTimeMachine? dateTimeMachine = null)
    {
        this.dateTimeMachine = dateTimeMachine ?? new DateTimeMachine();
        this.TimeStamp = this.dateTimeMachine.Now;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskGatewayResponse"/> class.
    /// Private parameterless constructor for EF Core entity materialization.
    /// </summary>
    private TaskGatewayResponse()
    {
        this.dateTimeMachine = new DateTimeMachine();
        this.TimeStamp = this.dateTimeMachine.Now;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the response.
    /// </summary>
    public int ResponseId { get; set; }

    /// <summary>
    /// Gets or sets the command identifier.
    /// </summary>
    public int CommandId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the barcode identifier.
    /// </summary>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the cycle identifier.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the number of successful cycles.
    /// </summary>
    public int CyclesOk { get; set; }

    /// <summary>
    /// Gets or sets the shift identifier.
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// Gets or sets the result validation status for the response.
    /// </summary>
    public ResultValidation ResultValidation { get; set; } = ResultValidation.None;

    /// <summary>
    /// Gets or sets the part number associated with the response.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the label for the barcode.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message for the response.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last machine identifier.
    /// </summary>
    public int LastMachineId { get; set; }

    /// <summary>
    /// Gets or sets the next machine identifier.
    /// </summary>
    public int NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets the cycle status for the response.
    /// </summary>
    public CycleStatus CycleStatus { get; set; } = CycleStatus.None;

    /// <summary>
    /// Gets or sets the flow status for the response.
    /// </summary>
    public FlowStatus FlowStatus { get; set; } = FlowStatus.None;

    /// <summary>
    /// Gets or sets the part status for the response.
    /// </summary>
    public PartStatus PartStatus { get; set; } = PartStatus.None;

    /// <summary>
    /// Gets or sets the timestamp for the response.
    /// </summary>
    public DateTime TimeStamp { get; set; }
}
