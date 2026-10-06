// <copyright file="GetReportsFilterInfoQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetReportsList.FiltersInfo;

/// <summary>
/// Represents the GetReportsFilterInfoQuery.
/// </summary>
public class GetReportsFilterInfoQuery(bool isMaster, DateTime startDate, DateTime endDate)
    : IMonitorRequest<ReportsFilterInfoVm>
{
    public bool IsMaster { get; set; } = isMaster;

    public DateTime StartDate { get; set; } = startDate;

    public DateTime EndDate { get; set; } = endDate;
}