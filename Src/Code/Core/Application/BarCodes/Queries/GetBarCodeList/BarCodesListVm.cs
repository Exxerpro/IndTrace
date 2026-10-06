// <copyright file="BarCodesListVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeList;

/// <summary>
/// Represents the BarCodesListVm.
/// </summary>
public class BarCodesListVm
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodesListVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public BarCodesListVm()
    {
        this.BarCodes = [];
        this.Shift = new Shift(new DateTimeMachine());
    }

    /// <summary>
    /// Gets or sets the BarCodes.
    /// </summary>
    public IList<BarCodeDto> BarCodes { get; set; }

    /// <summary>
    /// Gets or sets the Count.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the Shift.
    /// </summary>
    public Shift Shift { get; set; }

    /// <summary>
    /// Gets or sets the ProductionByShift.
    /// </summary>
    public int ProductionByShift { get; set; }
}