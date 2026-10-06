// <copyright file="TaskGatewayResponseDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

/// <summary>
/// Immutable §7 wire/response DTO for a gateway task. Story 32.C2 (#32/F9) splits the former mutable
/// <see cref="TaskGatewayResponse"/> god-object into this wire type (the object every §7 producer builds and every
/// consumer — the SignalR Hub payload, the PLC <see cref="References"/> routing carrier, and the monitor VMs —
/// reads) versus the slim persisted <see cref="TaskGatewayResponse"/> entity. The two roles are now different CLR
/// types, so a persistence-side edit can never silently reshape the life-critical PLC payload.
/// </summary>
/// <remarks>
/// <para>
/// Every member is <c>init</c>-only — there are no setters and no fluent mutators, so mutation is expressed with
/// non-destructive <c>with</c> expressions. The six smart-enum members default to <c>.None</c> at construction,
/// which is exactly what the retired <c>EnsureIsValidToRenderAndPersist</c> null-coalesce did at render time; the
/// enums are therefore never null and the guard is gone.
/// </para>
/// <para>
/// <see cref="References"/> and <see cref="Parameters"/> stay <see cref="IDictionary{TKey, TValue}"/> (matching the
/// pre-split wire shape and <c>IIndTraceControllerRx.References</c>) so the SignalR JSON and the PLC download path
/// are byte-identical; immutability is provided by the <c>init</c>-only reference, and register rebuilds go through
/// <see cref="ReferenceStamper"/>.
/// </para>
/// </remarks>
public sealed record TaskGatewayResponseDto : IMonitorFilter
{
    /// <summary>Gets the execution time for the task (wire-only).</summary>
    public TimeSpan ExecutionTime { get; init; }

    /// <summary>Gets the unique identifier for the persisted response row (default on the wire).</summary>
    public int ResponseId { get; init; }

    /// <summary>Gets the machine identifier.</summary>
    public int MachineId { get; init; }

    /// <summary>Gets the PLC identifier (wire-only).</summary>
    public int PlcId { get; init; }

    /// <summary>Gets the barcode identifier.</summary>
    public int BarCodeId { get; init; }

    /// <summary>Gets the cycle identifier.</summary>
    public int CycleId { get; init; }

    /// <summary>Gets the number of successful cycles.</summary>
    public int CyclesOk { get; init; }

    /// <summary>Gets the shift identifier.</summary>
    public int ShiftId { get; init; }

    /// <summary>Gets the command identifier.</summary>
    public int CommandId { get; init; }

    /// <summary>Gets the name of the response (wire-only, <see cref="IMonitorFilter"/>).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the part number associated with the response (<see cref="IMonitorFilter"/>).</summary>
    public string PartNumber { get; init; } = string.Empty;

    /// <summary>Gets the description of the response (wire-only).</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the label for the barcode.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Gets the error message for the response.</summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>Gets the last machine identifier / routing scalar.</summary>
    public int LastMachineId { get; init; }

    /// <summary>Gets the next machine identifier / routing scalar.</summary>
    public int NextMachineId { get; init; }

    /// <summary>Gets the cycle status for the response.</summary>
    public CycleStatus CycleStatus { get; init; } = CycleStatus.None;

    /// <summary>Gets the machine type for the response (wire-only).</summary>
    public MachineType MachineType { get; init; } = MachineType.None;

    /// <summary>Gets the part status for the response.</summary>
    public PartStatus PartStatus { get; init; } = PartStatus.None;

    /// <summary>Gets the flow status for the response.</summary>
    public FlowStatus FlowStatus { get; init; } = FlowStatus.None;

    /// <summary>Gets the result validation status for the response.</summary>
    public ResultValidation ResultValidation { get; init; } = ResultValidation.None;

    /// <summary>Gets the request task name (wire-only).</summary>
    public string RequestTask { get; init; } = string.Empty;

    /// <summary>Gets the workflow type for the response (wire-only).</summary>
    public WorkFlowType WorkFlowType { get; init; } = WorkFlowType.None;

    /// <summary>Gets the recipe associated with the response (wire-only).</summary>
    public Recipe Recipe { get; init; } = new();

    /// <summary>Gets the cycle associated with the response (wire-only).</summary>
    public Cycle Cycle { get; init; } = new();

    /// <summary>
    /// Gets the barcode entity associated with the response, or <c>null</c> when no part is scanned
    /// (Story 27.2b-2 — the absent state is a null reference, not a placeholder empty-label BarCode). Wire-only.
    /// </summary>
    public BarCode? BarCode { get; init; }

    /// <summary>Gets the master label entity associated with the response (wire-only).</summary>
    public MasterLabel MasterLabel { get; init; } = new();

    /// <summary>Gets the timestamp for the response (<see cref="IMonitorFilter"/>).</summary>
    public DateTime TimeStamp { get; init; } = new DateTimeMachine().Now;

    /// <summary>Gets the dictionary of references — the §7 numeric routing carrier (wire-only).</summary>
    public IDictionary<string, Register> References { get; init; } = new Dictionary<string, Register>();

    /// <summary>Gets the dictionary of parameters for the response (wire-only).</summary>
    public IDictionary<string, string> Parameters { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Projects the read-path <see cref="IBarCodeResult"/> onto an immutable wire DTO. Replaces the retired
    /// <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c>: same field map and the same
    /// <c>?? string.Empty</c> / empty-dictionary guards, with enum-defaulting folded into construction. Null-guards
    /// via <see cref="Result{T}"/> instead of throwing (IndTrace doctrine).
    /// </summary>
    /// <param name="src">The source barcode result.</param>
    /// <returns>A success result carrying the projected DTO, or a failure when <paramref name="src"/> is null.</returns>
    public static Result<TaskGatewayResponseDto> From(IBarCodeResult src)
    {
        if (src is null)
        {
            return Result<TaskGatewayResponseDto>.WithFailure($"Parameter '{nameof(src)}' cannot be null");
        }

        return Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto
        {
            MachineId = src.MachineId,
            BarCodeId = src.BarCodeId,
            CycleId = src.CycleId,
            CyclesOk = src.CyclesOk,
            ShiftId = src.ShiftId,
            CommandId = src.CommandId,
            ResultValidation = src.ResultValidation,
            Error = src.Error ?? string.Empty,
            Label = src.Label ?? string.Empty,
            PartNumber = src.PartNumber ?? string.Empty,
            Description = src.Description ?? string.Empty,
            LastMachineId = src.LastMachineId,
            NextMachineId = src.NextMachineId,
            CycleStatus = src.CycleStatus,
            FlowStatus = src.FlowStatus,
            PartStatus = src.PartStatus,
            MachineType = src.MachineType,
            WorkFlowType = src.WorkFlowType,
            Recipe = src.Recipe,
            Cycle = src.Cycle,
            BarCode = src.BarCode,
            MasterLabel = src.MasterLabel,
            References = src.References ?? new Dictionary<string, Register>(),
        });
    }

    /// <summary>
    /// Projects a <see cref="TaskGatewayRequest"/> onto an immutable wire DTO (the narrow request projection).
    /// Replaces the retired <c>TaskGatewayResponse.ToDto(TaskGatewayRequest)</c>: copies only
    /// MachineId/BarCodeId/CycleId/CommandId/PartNumber/Description/CycleStatus/FlowStatus/PartStatus/MachineType;
    /// every other member takes its construction default.
    /// </summary>
    /// <param name="src">The source gateway request.</param>
    /// <returns>A success result carrying the projected DTO, or a failure when <paramref name="src"/> is null.</returns>
    public static Result<TaskGatewayResponseDto> From(TaskGatewayRequest src)
    {
        if (src is null)
        {
            return Result<TaskGatewayResponseDto>.WithFailure($"Parameter '{nameof(src)}' cannot be null");
        }

        return Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto
        {
            MachineId = src.MachineId,
            BarCodeId = src.BarCodeId,
            CycleId = src.CycleId,
            CommandId = src.CommandId,
            PartNumber = src.PartNumber,
            Description = src.Description,
            CycleStatus = src.CycleStatus,
            FlowStatus = src.FlowStatus,
            PartStatus = src.PartStatus,
            MachineType = src.MachineType,
        });
    }

    /// <summary>
    /// Returns a string representation of the response (mirrors the retired god-object formatting for log parity).
    /// </summary>
    /// <returns>A string representation of the response.</returns>
    public override string ToString()
    {
        string newline = Environment.NewLine;
        return $"BarCode: {this.Label}{newline}" +
               $"Result: {this.ResultValidation.DisplayName}{newline}" +
               $"Machine ID: {this.MachineId}{newline}" +
               $"Machine Name: {this.Name}{newline}" +
               $"BarCode ID: {this.BarCodeId}{newline}" +
               $"Cycle ID: {this.CycleId}{newline}" +
               $"Cycles OK: {this.CyclesOk}{newline}" +
               $"Last Machine ID: {this.LastMachineId}{newline}" +
               $"Next Machine ID: {this.NextMachineId}{newline}" +
               $"Cycle Status: {this.CycleStatus.DisplayName}{newline}" +
               $"Flow Status: {this.FlowStatus.DisplayName}{newline}" +
               $"Part Status: {this.PartStatus.DisplayName}{newline}" +
               $"Machine Type: {this.MachineType.DisplayName}{newline}" +
               $"WorkFlow Type: {this.WorkFlowType.DisplayName}{newline}" +
               $"Description: {this.Description}{newline}" +
               $"Execution Time: {this.ExecutionTime}{newline}";
    }
}
