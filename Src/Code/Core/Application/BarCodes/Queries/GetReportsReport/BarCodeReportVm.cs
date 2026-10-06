// <copyright file="BarCodeReportVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetReportsReport;

/// <summary>
/// Per-barcode view model for the Reports Excel export. #229 (Slice A): slimmed to EXACTLY the surface the only
/// consumer (<c>ExportService.ExportToExcel</c>) reads — MachineId, BarCodeId, Label plus the per-barcode cycle
/// and register views. The 11 former fields (CycleId, ProductId, LastMachineId, NextMachineId, CycleStatus,
/// FlowStatus, PartStatus, MachineType, WorkFlowType, CreatedOn, ModifiedOn) were dead on this path and are
/// removed, together with the entity mappers (ToDto/ToDtoList/ToEntity) — the handler now builds these view
/// models directly from projected, untracked rows.
/// </summary>
public class BarCodeReportVm
{
    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the BarCodeId.
    /// </summary>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the Label.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public List<CycleView> Cycles { get; set; }

    /// <summary>
    /// Gets or sets the Registers.
    /// </summary>
    public List<RegisterView> Registers { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeReportVm"/> class.
    /// </summary>
    public BarCodeReportVm()
    {
        this.Label = string.Empty;
        this.Cycles = [];
        this.Registers = [];
    }
}
