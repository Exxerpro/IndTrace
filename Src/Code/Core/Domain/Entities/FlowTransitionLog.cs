// <copyright file="FlowTransitionLog.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;

/// <summary>
/// Additive, append-only diagnostic record of EVERY attempted item lifecycle transition (Story 3.5,
/// PRD FR5 / CR2). Exactly one row is appended for each <c>IItemStateMachine.Fire(...)</c> — success OR
/// rejection — capturing the realized (or non-) transition.
///
/// This is a PARALLEL, finer-grained record alongside the existing audit <see cref="TaskGatewayRequest"/>;
/// it replaces nothing. The table is purely additive and unread by the transition logic, so it is ignorable
/// on rollback and is NEVER written to a PLC reference tag (CR4/NFR1).
/// </summary>
public class FlowTransitionLog : IEntityRoot
{
    /// <summary>
    /// Gets or sets the surrogate primary key for the transition-log row.
    /// </summary>
    public int FlowTransitionLogId { get; set; }

    /// <summary>
    /// Gets or sets the SOURCE <see cref="Enum.FlowStatus"/> the item was in when the trigger fired.
    /// </summary>
    public FlowStatus From { get; set; } = FlowStatus.None;

    /// <summary>
    /// Gets or sets the RESULTING <see cref="Enum.FlowStatus"/>. For a rejected fire this equals
    /// <see cref="From"/> (no advance occurred).
    /// </summary>
    public FlowStatus To { get; set; } = FlowStatus.None;

    /// <summary>
    /// Gets or sets the SOURCE <see cref="Enum.CycleStatus"/> context for the fire (diagnostic only).
    /// </summary>
    public CycleStatus FromCycleStatus { get; set; } = CycleStatus.None;

    /// <summary>
    /// Gets or sets the <see cref="GatewayTask"/> trigger that was fired.
    /// </summary>
    public GatewayTask Trigger { get; set; } = GatewayTask.None;

    /// <summary>
    /// Gets or sets the ORIGINATING <see cref="TransitionPath"/> (PLC vs Webapp) of the fire.
    /// </summary>
    public TransitionPath Path { get; set; } = TransitionPath.Unknown;

    /// <summary>
    /// Gets or sets the machine identifier of the item that fired the trigger.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the barcode identifier of the item that fired the trigger (for the AC6 diagnostic query).
    /// </summary>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the cycle identifier associated with the fire (0 when not applicable).
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Enum.ResultValidation"/> outcome code. For a success this is the resolved
    /// outcome's code; for a rejection it carries the SPECIFIC negative code (ties to Story 3.3/3.4 codes).
    /// </summary>
    public ResultValidation ResultValidation { get; set; } = ResultValidation.None;

    /// <summary>
    /// Gets or sets the timestamp at which the fire was logged.
    /// </summary>
    public DateTime TimeStamp { get; set; }
}
