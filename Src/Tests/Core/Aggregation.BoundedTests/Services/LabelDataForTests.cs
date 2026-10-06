// <copyright file="LabelDataForTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Services;
/// <summary>
/// Represents the LabelDataForTests.
/// </summary>

public class LabelDataForTests(
    string label,
    int machineId,
    int lastMachineId,
    int nextMachineId,
    string partStatus,
    string flowStatus,
    string cycleStatus,
    int barCodeId,
    int cycleId,
    string machineType)
{
    public string Label { get; set; } = label;
    public int MachineId { get; set; } = machineId;
    public int LastMachineId { get; set; } = lastMachineId;
    public int NextMachineId { get; set; } = nextMachineId;
    public PartStatus PartStatus { get; set; } = EnumModel.FromName<PartStatus>(partStatus);
    public FlowStatus FlowStatus { get; set; } = EnumModel.FromName<FlowStatus>(flowStatus);
    public CycleStatus CycleStatus { get; set; } = EnumModel.FromName<CycleStatus>(cycleStatus);
    public MachineType MachineType { get; set; } = EnumModel.FromName<MachineType>(machineType);

    public Result Result { get; set; } = new();
    public int BarCodeId { get; set; } = barCodeId;
    public int CycleId { get; set; } = cycleId;

    public bool Equals(TaskGatewayResponseDto taskGatewayInfo)
    {

        {
            taskGatewayInfo.MachineId.ShouldBe(MachineId);
            taskGatewayInfo.LastMachineId.ShouldBe(LastMachineId);
            taskGatewayInfo.NextMachineId.ShouldBe(NextMachineId);
            taskGatewayInfo.BarCodeId.ShouldBe(BarCodeId);
            PartStatus.ShouldBe(taskGatewayInfo.PartStatus);
        }

        return
            (MachineId == taskGatewayInfo.MachineId) &&
            (LastMachineId == taskGatewayInfo.LastMachineId) &&
            (NextMachineId == taskGatewayInfo.NextMachineId) &&
            (BarCodeId == taskGatewayInfo.BarCodeId) &&
            (Equals(PartStatus, taskGatewayInfo.PartStatus));
    }

    public bool Equals(BarCodeResult barCodeInfo)
    {

        {
            barCodeInfo.MachineId.ShouldBe(MachineId);
            barCodeInfo.LastMachineId.ShouldBe(LastMachineId);
            barCodeInfo.NextMachineId.ShouldBe(NextMachineId);
            barCodeInfo.BarCodeId.ShouldBe(BarCodeId);
            PartStatus.ShouldBe(barCodeInfo.PartStatus);
        }

        return
            (MachineId == barCodeInfo.MachineId) &&
            (LastMachineId == barCodeInfo.LastMachineId) &&
            (NextMachineId == barCodeInfo.NextMachineId) &&
            (BarCodeId == barCodeInfo.BarCodeId) &&
            (Equals(PartStatus, barCodeInfo.PartStatus));
    }
}