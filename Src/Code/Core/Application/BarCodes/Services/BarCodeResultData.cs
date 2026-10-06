// <copyright file="BarCodeResultData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Data transfer object for barcode result data, including machine, barcode, cycle, and validation information.
/// </summary>
public class BarCodeResultData
{
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
    /// Gets or sets the number of cycles marked as OK.
    /// </summary>
    public int CyclesOk { get; set; }

    /// <summary>
    /// Gets or sets the shift identifier.
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// Gets or sets the command identifier.
    /// </summary>
    public int CommandId { get; set; }

    /// <summary>
    /// Gets or sets the result validation status.
    /// </summary>
    public ResultValidation ResultValidation { get; set; } = ResultValidation.None;

    /// <summary>
    /// Gets or sets the error message, if any.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the label associated with the barcode.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the part number.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the last machine.
    /// </summary>
    public int LastMachineId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the next machine.
    /// </summary>
    public int NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets the number of registers saved.
    /// </summary>
    public int RegistersSaved { get; set; }

    /// <summary>
    /// Gets or sets the current shift information.
    /// </summary>
    public Shift Shift { get; set; } = new(new DateTimeMachine());

    /// <summary>
    /// Gets or sets the last shift information.
    /// </summary>
    public Shift LastShift { get; set; } = new(new DateTimeMachine());

    /// <summary>
    /// Gets or sets the product information.
    /// </summary>
    public Product Product { get; set; } = new();

    /// <summary>
    /// Gets or sets the command information.
    /// </summary>
    public TaskGatewayRequest Command { get; set; } = new();

    /// <summary>
    /// Gets or sets the cycle status.
    /// </summary>
    public CycleStatus CycleStatus { get; set; } = CycleStatus.None;

    /// <summary>
    /// Gets or sets the flow status.
    /// </summary>
    public FlowStatus FlowStatus { get; set; } = FlowStatus.None;

    /// <summary>
    /// Gets or sets the part status.
    /// </summary>
    public PartStatus PartStatus { get; set; } = PartStatus.None;

    /// <summary>
    /// Gets or sets the machine type.
    /// </summary>
    public MachineType MachineType { get; set; } = MachineType.None;

    /// <summary>
    /// Gets or sets the workflow type.
    /// </summary>
    public WorkFlowType WorkFlowType { get; set; } = WorkFlowType.None;

    /// <summary>
    /// Gets or sets the recipe information.
    /// </summary>
    public Recipe Recipe { get; set; } = new();

    /// <summary>
    /// Gets or sets the machine information.
    /// </summary>
    public Machine Machine { get; set; } = new();

    /// <summary>
    /// Gets or sets the cycle information.
    /// </summary>
    public Cycle Cycle { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of cycles.
    /// </summary>
    public IEnumerable<Cycle> Cycles { get; set; } = new List<Cycle>();

    /// <summary>
    /// Gets or sets the barcode information, or <c>null</c> when no part is scanned (Story 27.2b-2).
    /// </summary>
    public BarCode? BarCode { get; set; }

    /// <summary>
    /// Gets or sets the master label information.
    /// </summary>
    public MasterLabel MasterLabel { get; set; } = new();

    /// <summary>
    /// Gets or sets the references dictionary.
    /// </summary>
    public IDictionary<string, Register> References { get; set; } = new Dictionary<string, Register>();
}
