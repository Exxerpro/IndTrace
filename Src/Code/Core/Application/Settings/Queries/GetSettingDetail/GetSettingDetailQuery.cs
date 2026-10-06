// <copyright file="GetSettingDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Settings.Queries.GetSettingDetail;

/// <summary>
/// Represents a query to retrieve the details of a specific setting.
/// </summary>
public class GetSettingDetailQuery : IMonitorRequest<SettingDetailVm>
{
    /// <summary>
    /// Gets or sets the unique identifier of the setting to retrieve.
    /// </summary>
    public int SettingId { get; set; }
}
