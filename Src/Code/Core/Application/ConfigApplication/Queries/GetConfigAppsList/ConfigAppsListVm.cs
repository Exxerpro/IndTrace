// <copyright file="ConfigAppsListVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Queries.GetConfigAppsList;

/// <summary>
/// Represents the ConfigAppsListVm.
/// </summary>
public class ConfigAppsListVm
{
    /// <summary>
    /// Gets or sets the list of ConfigApp DTOs.
    /// </summary>
    public IList<ConfigAppsDto> ConfigApp { get; set; } = [];

    /// <summary>
    /// Gets or sets the Count.
    /// </summary>
    public int Count { get; set; }
}