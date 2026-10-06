// <copyright file="CyclesDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Queries.GetCyclesList;

/// <summary>
/// Represents the CyclesDto.
/// </summary>
public class CyclesDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CyclesDto"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public CyclesDto()
    {
        // Story 27.2b-2: "no part scanned" is an ABSENT reference (null), not a placeholder empty-label BarCode.
        this.BarCode = null;
        this.Machine = new Machine();
        this.Product = Product.CreateEmpty();
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
    public int CycleStatus { get; set; }

    /// <summary>
    /// Gets or sets the CyclesOk.
    /// </summary>
    public int CyclesOk { get; set; }

    /// <summary>
    /// Gets or sets the PartStatus.
    /// </summary>
    public int PartStatus { get; set; }

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
    /// Gets or sets the StatusCicloId.
    /// </summary>
    public int StatusCicloId { get; set; }

    /// <summary>
    /// Gets or sets the BarCode, or <c>null</c> when no part is scanned (Story 27.2b-2).
    /// </summary>
    public virtual BarCode? BarCode { get; set; }

    /// <summary>
    /// Gets or sets the Machine.
    /// </summary>
    public virtual Machine Machine { get; set; }

    /// <summary>
    /// Gets or sets the Product.
    /// </summary>
    public virtual Product Product { get; set; }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<CyclesDto> ToDto(Cycle src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<CyclesDto>.WithFailure("Cycle source cannot be null");
        }

        return IndQuestResults.Result<CyclesDto>.Success(new CyclesDto
        {
            CycleId = src.CycleId.Value,
            MachineId = src.MachineId.Value,
            BarCodeId = src.BarCodeId.Value,
            CycleStatus = src.CycleStatus,
            CyclesOk = src.CyclesOk,
            PartStatus = src.PartStatus,
            CycleTime = src.CycleTime,
            TaktTime = src.TaktTime,
            StartedOn = src.StartedOn,
            FinishedOn = src.FinishedOn,

            // StatusCicloId, BarCode, Machine, Product are not mapped from Cycle directly
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<Cycle> ToEntity(CyclesDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<Cycle>.WithFailure("CyclesDto source cannot be null");
        }

        var entity = new Cycle
        {
            CycleId = new CycleId(src.CycleId),
            MachineId = new MachineId(src.MachineId),
            BarCodeId = new BarCodeId(src.BarCodeId),
            CyclesOk = src.CyclesOk,
            CycleTime = src.CycleTime,
            TaktTime = src.TaktTime,
            StartedOn = src.StartedOn,
            FinishedOn = src.FinishedOn,
        };

        // Story 6.4: the status setters are now private set; apply the source values via the trusted seam.
        entity.ApplyCycleAndPartStatus(src.CycleStatus, src.PartStatus);
        return IndQuestResults.Result<Cycle>.Success(entity);
    }

    /// <summary>
    /// Executes ToDtoList operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDtoList.</returns>
    public static IndQuestResults.Result<List<CyclesDto>> ToDtoList(IEnumerable<Cycle> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<List<CyclesDto>>.WithFailure("Cycle collection cannot be null");
        }

        var list = src.Select(s => new CyclesDto
        {
            CycleId = s.CycleId.Value,
            MachineId = s.MachineId.Value,
            BarCodeId = s.BarCodeId.Value,
            CycleStatus = s.CycleStatus,
            CyclesOk = s.CyclesOk,
            PartStatus = s.PartStatus,
            CycleTime = s.CycleTime,
            TaktTime = s.TaktTime,
            StartedOn = s.StartedOn,
            FinishedOn = s.FinishedOn,
        }).ToList();
        return IndQuestResults.Result<List<CyclesDto>>.Success(list);
    }
}