// <copyright file="ReportsDetailMonitorQrQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetailQrCode;

/// <summary>
/// Represents the ReportsDetailMonitorQrQuery.
/// </summary>
public class ReportsDetailMonitorQrQuery : IMonitorRequest<ReportDetailMonitorVm>
{
    /// <summary>
    /// Gets or sets the BarCode.
    /// </summary>
    public string BarCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BarCodeId.
    /// </summary>
    public int BarCodeId { get; set; }
}