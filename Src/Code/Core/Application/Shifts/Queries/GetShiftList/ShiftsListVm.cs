// <copyright file="ShiftsListVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Queries.GetShiftList;

/// <summary>
/// Represents the ShiftsListVm.
/// </summary>
public class ShiftsListVm
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShiftsListVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ShiftsListVm()
    {
        this.Shifts = [];
    }

    /// <summary>
    /// Gets or sets the Shifts.
    /// </summary>
    public IList<ShiftsDto> Shifts { get; set; }

    /// <summary>
    /// Gets or sets the Count.
    /// </summary>
    public int Count { get; set; }
}