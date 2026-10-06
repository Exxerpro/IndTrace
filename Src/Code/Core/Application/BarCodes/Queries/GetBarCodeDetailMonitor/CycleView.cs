// <copyright file="CycleView.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetailMonitor;

/// <summary>
/// Represents the CycleView.
/// </summary>
public class CycleView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CycleView"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public CycleView()
    {
        this.CycleStatus = CycleStatus.None;
        this.PartStatus = PartStatus.None;
    }

    /// <summary>
    /// Gets or sets the CycleId.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the BarCodeId.
    /// </summary>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the CycleStatus.
    /// </summary>
    public CycleStatus CycleStatus { get; set; }

    /// <summary>
    /// Gets or sets the CyclesOk.
    /// </summary>
    public int CyclesOk { get; set; }

    /// <summary>
    /// Gets or sets the PartStatus.
    /// </summary>
    public PartStatus PartStatus { get; set; }

    /// <summary>
    /// Gets or sets the CycleTime.
    /// </summary>
    public int CycleTime { get; set; }

    /// <summary>
    /// Gets or sets the TaktTime.
    /// </summary>
    public int TaktTime { get; set; }

    /// <summary>
    /// Gets or sets the StartedOn.
    /// </summary>
    public DateTime StartedOn { get; set; }

    /// <summary>
    /// Gets or sets the FinishedOn.
    /// </summary>
    public DateTime FinishedOn { get; set; }

    /// <summary>
    /// Converts a <see cref="Cycle"/> entity into a <see cref="CycleView"/> using functional Result semantics.
    /// </summary>
    /// <param name="src">The source <see cref="Cycle"/> entity.</param>
    /// <returns>A Result with the mapped <see cref="CycleView"/>, or a failure when <paramref name="src"/> is null.</returns>
    public static IndQuestResults.Result<CycleView> ToDto(Cycle src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<CycleView>.WithFailure("Cycle source cannot be null");
        }

        return IndQuestResults.Result<CycleView>.Success(new CycleView
        {
            CycleId = src.CycleId.Value,
            MachineId = src.MachineId.Value,
            BarCodeId = src.BarCodeId.Value,
            CycleStatus = EnumModel.FromValue<CycleStatus>(src.CycleStatus),
            PartStatus = EnumModel.FromValue<PartStatus>(src.PartStatus),
            CycleTime = src.CycleTime,
            TaktTime = src.TaktTime,
            StartedOn = src.StartedOn,
            FinishedOn = src.FinishedOn,
        });
    }

    /// <summary>
    /// Converts a collection of <see cref="Cycle"/> entities into a list of <see cref="CycleView"/>.
    /// </summary>
    /// <param name="src">Source collection of <see cref="Cycle"/> entities.</param>
    /// <returns>A Result with the mapped list of <see cref="CycleView"/>, or a failure when <paramref name="src"/> is null.</returns>
    public static IndQuestResults.Result<List<CycleView>> ToDtoList(IEnumerable<Cycle> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<List<CycleView>>.WithFailure("Cycle collection cannot be null");
        }

        var list = src.Select(s => new CycleView
        {
            CycleId = s.CycleId.Value,
            MachineId = s.MachineId.Value,
            BarCodeId = s.BarCodeId.Value,
            CycleStatus = EnumModel.FromValue<CycleStatus>(s.CycleStatus),
            PartStatus = EnumModel.FromValue<PartStatus>(s.PartStatus),
            CycleTime = s.CycleTime,
            TaktTime = s.TaktTime,
            StartedOn = s.StartedOn,
            FinishedOn = s.FinishedOn,
        }).ToList();
        return IndQuestResults.Result<List<CycleView>>.Success(list);
    }

    /// <summary>
    /// Converts a <see cref="CycleView"/> back to a <see cref="Cycle"/> entity using functional Result semantics.
    /// </summary>
    /// <param name="src">The source <see cref="CycleView"/>.</param>
    /// <returns>A Result with the mapped <see cref="Cycle"/>, or a failure when <paramref name="src"/> is null.</returns>
    public static IndQuestResults.Result<Cycle> ToEntity(CycleView src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<Cycle>.WithFailure("CycleView source cannot be null");
        }

        var entity = new Cycle
        {
            CycleId = new CycleId(src.CycleId),
            MachineId = new MachineId(src.MachineId),
            BarCodeId = new BarCodeId(src.BarCodeId),
            CycleTime = src.CycleTime,
            TaktTime = src.TaktTime,
            StartedOn = src.StartedOn,
            FinishedOn = src.FinishedOn,
        };

        // Story 6.4: the status setters are now private set; apply the source values via the trusted seam.
        entity.ApplyCycleAndPartStatus(src.CycleStatus.Value, src.PartStatus.Value);
        return IndQuestResults.Result<Cycle>.Success(entity);
    }
}