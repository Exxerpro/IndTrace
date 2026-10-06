// <copyright file="BarCodeMarkedScrapView.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.MarkScrap;

/// <summary>
/// Represents the BarCodeMarkedScrapView.
/// </summary>
public class BarCodeMarkedScrapView
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
    /// Gets or sets the FlowStatus.
    /// </summary>
    public FlowStatus FlowStatus { get; set; }

    /// <summary>
    /// Gets or sets the PartStatus.
    /// </summary>
    public PartStatus PartStatus { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeMarkedScrapView"/> class.
    /// </summary>
    public BarCodeMarkedScrapView()
    {
        this.Label = string.Empty;
        this.FlowStatus = FlowStatus.None;
        this.PartStatus = PartStatus.None;
    }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<BarCodeMarkedScrapView> ToDto(BarCode src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCodeMarkedScrapView>.WithFailure("BarCode source cannot be null");
        }

        return IndQuestResults.Result<BarCodeMarkedScrapView>.Success(new BarCodeMarkedScrapView
        {
            MachineId = src.MachineId.Value,
            BarCodeId = src.BarCodeId.Value,
            Label = src.Label.Value,
            FlowStatus = EnumModel.FromValue<FlowStatus>(src.FlowStatus),
            PartStatus = EnumModel.FromValue<PartStatus>(src.PartStatus),
        });
    }
}
