// <copyright file="StatusMonitor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UI.Models;

/// <summary>
/// Represents a status monitor for tracking machine and production status information.
/// </summary>
public class StatusMonitor : IEquatable<StatusMonitor>, IComparable<StatusMonitor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatusMonitor"/> class.
    /// </summary>
    public StatusMonitor()
    {
        this.CycleStatus = CycleStatus.None;
        this.FlowStatus = FlowStatus.None;
        this.MachineType = MachineType.None;
        this.PartStatus = PartStatus.None;
        this.ResultValidation = ResultValidation.None;
        this.WorkFlowType = WorkFlowType.None;
    }

    /// <summary>
    /// Gets or sets the barcode identifier.
    /// </summary>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the cycle identifier.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the cycle status.
    /// </summary>
    public CycleStatus CycleStatus { get; set; }

    /// <summary>
    /// Gets or sets the flow status.
    /// </summary>
    public FlowStatus FlowStatus { get; set; }

    /// <summary>
    /// Gets or sets the label value.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last machine identifier.
    /// </summary>
    public int LastMachineId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the machine type.
    /// </summary>
    public MachineType MachineType { get; set; }

    /// <summary>
    /// Gets or sets the next machine identifier.
    /// </summary>
    public int NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets the part status.
    /// </summary>
    public PartStatus PartStatus { get; set; }

    /// <summary>
    /// Gets or sets the PLC identifier.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the result validation status.
    /// </summary>
    public ResultValidation ResultValidation { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets the workflow type.
    /// </summary>
    public WorkFlowType WorkFlowType { get; set; }

    /// <summary>
    /// Compares the current instance with another status monitor.
    /// </summary>
    /// <param name="other">The other status monitor to compare with.</param>
    /// <returns>A value indicating the relative order of the objects being compared.</returns>
    public int CompareTo(StatusMonitor? other)
    {
        if (other is null)
        {
            return 1;
        }

        return this.PlcId.CompareTo(other.PlcId);
    }

    /// <summary>
    /// Determines whether the specified status monitor is equal to the current status monitor.
    /// </summary>
    /// <param name="other">The status monitor to compare with the current status monitor.</param>
    /// <returns>True if the specified status monitor is equal to the current status monitor; otherwise, false.</returns>
    public bool Equals(StatusMonitor? other)
    {
        return this.PlcId == other?.PlcId;
    }

    /// <summary>
    /// Converts a TaskGatewayResponseDto to a StatusMonitor.
    /// </summary>
    /// <param name="src">The source TaskGatewayResponseDto.</param>
    /// <returns>A StatusMonitor representation of the TaskGatewayResponseDto.</returns>
    public static IndQuestResults.Result<StatusMonitor> ToDto(TaskGatewayResponseDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<StatusMonitor>.WithFailure("TaskGatewayResponseDto source cannot be null");
        }

        return IndQuestResults.Result<StatusMonitor>.Success(new StatusMonitor
        {
            BarCodeId = src.BarCodeId,
            CycleId = src.CycleId,
            CycleStatus = EnumModel.FromValue<CycleStatus>(src.CycleStatus),
            FlowStatus = EnumModel.FromValue<FlowStatus>(src.FlowStatus),
            Label = src.Label,
            LastMachineId = src.LastMachineId,
            MachineId = src.MachineId,
            MachineType = EnumModel.FromValue<MachineType>(src.MachineType),
            NextMachineId = src.NextMachineId,
            PartStatus = EnumModel.FromValue<PartStatus>(src.PartStatus),
            PlcId = src.MachineId,
            ResultValidation = EnumModel.FromValue<ResultValidation>(src.ResultValidation),
            TimeStamp = src.TimeStamp,
            WorkFlowType = EnumModel.FromValue<WorkFlowType>(src.WorkFlowType),
        });
    }

    /// <summary>
    /// Converts a StatusMonitor to a TaskGatewayResponseDto.
    /// </summary>
    /// <param name="src">The source StatusMonitor.</param>
    /// <returns>A TaskGatewayResponseDto representation of the StatusMonitor.</returns>
    public static IndQuestResults.Result<TaskGatewayResponseDto> ToEntity(StatusMonitor src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<TaskGatewayResponseDto>.WithFailure("StatusMonitor source cannot be null");
        }

        return IndQuestResults.Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto
        {
            BarCodeId = src.BarCodeId,
            CycleId = src.CycleId,
            CycleStatus = src.CycleStatus.Value,
            FlowStatus = src.FlowStatus.Value,
            Label = src.Label,
            LastMachineId = src.LastMachineId,
            MachineId = src.MachineId,
            MachineType = src.MachineType.Value,
            NextMachineId = src.NextMachineId,
            PartStatus = src.PartStatus.Value,

            ResultValidation = src.ResultValidation.Value,
            TimeStamp = src.TimeStamp,
            WorkFlowType = src.WorkFlowType.Value,
        });
    }

}