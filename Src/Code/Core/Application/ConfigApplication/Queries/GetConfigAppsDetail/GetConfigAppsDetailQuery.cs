// <copyright file="GetConfigAppsDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Queries.GetConfigAppsDetail;

/// <summary>
/// Represents the GetConfigAppsDetailQuery.
/// </summary>
public class GetConfigAppsDetailQuery : IMonitorRequest<ConfigAppDto>
{
    /// <summary>
    /// Gets or sets the RegisterId.
    /// </summary>
    public int Id { get; set; }
}