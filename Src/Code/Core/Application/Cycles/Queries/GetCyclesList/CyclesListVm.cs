// <copyright file="CyclesListVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Queries.GetCyclesList;

/// <summary>
/// Represents the CyclesListVm.
/// </summary>
public class CyclesListVm
{
    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public IList<CyclesDto> Cycles { get; set; } = [];

    /// <summary>
    /// Gets or sets the Count.
    /// </summary>
    public int Count { get; set; }
}