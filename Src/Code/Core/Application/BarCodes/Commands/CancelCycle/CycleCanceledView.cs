// <copyright file="CycleCanceledView.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.CancelCycle;

/// <summary>
/// Represents the CycleCanceledView.
/// </summary>
public class CycleCanceledView
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
    /// Gets or sets the CycleId.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the CycleStatus.
    /// </summary>
    public CycleStatus CycleStatus { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleCanceledView"/> class.
    /// </summary>
    public CycleCanceledView()
    {
        this.CycleStatus = CycleStatus.None;
    }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<CycleCanceledView> ToDto(Cycle src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<CycleCanceledView>.WithFailure("Cycle source cannot be null");
        }

        return IndQuestResults.Result<CycleCanceledView>.Success(new CycleCanceledView
        {
            MachineId = src.MachineId.Value,
            BarCodeId = src.BarCodeId.Value,
            CycleId = src.CycleId.Value,
            CycleStatus = EnumModel.FromValue<CycleStatus>(src.CycleStatus),
        });
    }
}
