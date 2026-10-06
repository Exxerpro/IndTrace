// <copyright file="GetShiftDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Queries.GetShiftDetail;

/// <summary>
/// Represents the GetShiftDetailQuery.
/// </summary>
public class GetShiftDetailQuery : IMonitorRequest<ShiftDetailVm>
{
    /// <summary>
    /// Gets or sets the TimeRequest.
    /// </summary>
    public DateTime TimeRequest { get; set; }

    /// <summary>
    /// Gets or sets the ShiftId.
    /// </summary>
    public int ShiftId { get; set; }
}