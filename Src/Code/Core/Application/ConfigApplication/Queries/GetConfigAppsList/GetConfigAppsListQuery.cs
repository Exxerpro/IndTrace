// <copyright file="GetConfigAppsListQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Queries.GetConfigAppsList;

/// <summary>
/// Represents the GetConfigAppsListQuery.
/// </summary>
public class GetConfigAppsListQuery : IMonitorRequest<ConfigAppsListVm>
{
    /// <summary>
    /// Gets or sets the RegisterId.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConfigAppsListQuery"/> class.
    /// </summary>
    public GetConfigAppsListQuery()
    {
        this.Id = string.Empty;
    }
}