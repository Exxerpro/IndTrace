// <copyright file="BarCodeRestoredView.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Restore;

/// <summary>
/// Represents the BarCodeRestoredView.
/// </summary>
public class BarCodeRestoredView
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
    /// Initializes a new instance of the <see cref="BarCodeRestoredView"/> class.
    /// </summary>
    public BarCodeRestoredView()
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
    public static IndQuestResults.Result<BarCodeRestoredView> ToDto(BarCode src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCodeRestoredView>.WithFailure("BarCode source cannot be null");
        }

        return IndQuestResults.Result<BarCodeRestoredView>.Success(new BarCodeRestoredView
        {
            MachineId = src.MachineId.Value,
            BarCodeId = src.BarCodeId.Value,
            Label = src.Label.Value,
            FlowStatus = EnumModel.FromValue<FlowStatus>(src.FlowStatus),
            PartStatus = EnumModel.FromValue<PartStatus>(src.PartStatus),
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<BarCode> ToEntity(BarCodeRestoredView src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCode>.WithFailure("BarCodeRestoredView source cannot be null");
        }

        var entity = new BarCode
        {
            MachineId = new MachineId(src.MachineId),
            BarCodeId = new BarCodeId(src.BarCodeId),
            Label = BarCodeLabel.FromPersisted(src.Label ?? string.Empty),
        };

        // Story 6.4: the status setters are now private set; apply the source values via the trusted seam.
        entity.ApplyFlowAndPartStatus(src.FlowStatus.Value, src.PartStatus.Value);
        return IndQuestResults.Result<BarCode>.Success(entity);
    }
}