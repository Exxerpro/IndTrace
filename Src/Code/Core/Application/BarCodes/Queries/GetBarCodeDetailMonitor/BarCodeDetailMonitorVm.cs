// <copyright file="BarCodeDetailMonitorVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetailMonitor;

/// <summary>
/// Represents the BarCodeDetailMonitorVm.
/// </summary>
public class BarCodeDetailMonitorVm
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeDetailMonitorVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public BarCodeDetailMonitorVm()
    {
        this.CycleStatus = CycleStatus.None;
        this.FlowStatus = FlowStatus.None;
        this.PartStatus = PartStatus.None;
        this.MachineType = MachineType.None;
        this.WorkFlowType = WorkFlowType.None;
        this.ResultValidation = ResultValidation.None;
        this.Label = string.Empty;
    }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the BarCodeId.
    /// </summary>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the CycleId.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the LastMachineId.
    /// </summary>
    public int LastMachineId { get; set; }

    /// <summary>
    /// Gets or sets the NextMachineId.
    /// </summary>
    public int NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets the CycleStatus.
    /// </summary>
    public CycleStatus CycleStatus { get; set; }

    /// <summary>
    /// Gets or sets the FlowStatus.
    /// </summary>
    public FlowStatus FlowStatus { get; set; }

    /// <summary>
    /// Gets or sets the PartStatus.
    /// </summary>
    public PartStatus PartStatus { get; set; }

    /// <summary>
    /// Gets or sets the MachineType.
    /// </summary>
    public MachineType MachineType { get; set; }

    /// <summary>
    /// Gets or sets the WorkFlowType.
    /// </summary>
    public WorkFlowType WorkFlowType { get; set; }

    /// <summary>
    /// Gets or sets the ResultValidation.
    /// </summary>
    public ResultValidation ResultValidation { get; set; }

    /// <summary>
    /// Gets or sets the Label.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public List<CycleView> Cycles { get; set; } = [];

    /// <summary>
    /// Gets or sets the Registers.
    /// </summary>
    public List<RegisterView> Registers { get; set; } = [];

    /// <summary>
    /// Gets or sets the StatusMonitor.
    /// </summary>
    public StatusMonitor StatusMonitor { get; set; } = new();

    /// <summary>
    /// Gets or sets the Variables.
    /// </summary>
    public List<VariablesView> Variables { get; set; } = [];

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<BarCodeDetailMonitorVm> ToDto(BarCode src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCodeDetailMonitorVm>.WithFailure("BarCode source cannot be null");
        }

        return IndQuestResults.Result<BarCodeDetailMonitorVm>.Success(new BarCodeDetailMonitorVm
        {
            MachineId = src.MachineId.Value,
            BarCodeId = src.BarCodeId.Value,
            Label = src.Label.Value,
            FlowStatus = EnumModel.FromValue<FlowStatus>(src.FlowStatus),
            PartStatus = EnumModel.FromValue<PartStatus>(src.PartStatus),
            Registers = [],
            Cycles = [],
            StatusMonitor = new StatusMonitor(),
            Variables = [],

            // Only properties available in BarCode are mapped
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<BarCode> ToEntity(BarCodeDetailMonitorVm src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCode>.WithFailure("BarCodeDetailMonitorVm source cannot be null");
        }

        var entity = new BarCode
        {
            MachineId = new MachineId(src.MachineId),
            BarCodeId = new BarCodeId(src.BarCodeId),
            Label = BarCodeLabel.FromPersisted(src.Label),

            // Only properties available in BarCode are mapped
        };

        // Story 6.4: the status setters are now private set; apply the source values via the trusted seam.
        entity.ApplyFlowAndPartStatus((int)src.FlowStatus, (int)src.PartStatus);
        return IndQuestResults.Result<BarCode>.Success(entity);
    }
}