// <copyright file="GetBarCodesListQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeList;

/// <summary>
/// Represents the GetBarCodesListQuery.
/// </summary>
public class GetBarCodesListQuery : IMonitorRequest<BarCodesListVm>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodesListQuery"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetBarCodesListQuery()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodesListQuery"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="isMaster">The isMaster.</param>
    /// <param name="startDate">The startDate.</param>
    /// <param name="endDate">The endDate.</param>
    public GetBarCodesListQuery(bool isMaster, DateTime startDate, DateTime endDate)
    {
        this.IsMaster = isMaster;
        this.StartDate = startDate;
        this.EndDate = endDate;
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the IsMaster.
    /// </summary>
    public bool IsMaster { get; set; }

    /// <summary>
    /// Gets or sets the StartDate.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the EndDate.
    /// </summary>
    public DateTime EndDate { get; set; }
}