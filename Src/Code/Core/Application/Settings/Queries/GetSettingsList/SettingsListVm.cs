// <copyright file="SettingsListVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Settings.Queries.GetSettingsList;

using IndTrace.Application.Settings.DTO;

/// <summary>
/// Represents the SettingsListVm.
/// </summary>
public class SettingsListVm
{
    /// <summary>
    /// Gets or sets the list of Setting DTOs.
    /// </summary>
    public IList<SettingDto> Settings { get; set; } = [];

    /// <summary>
    /// Gets or sets the Count.
    /// </summary>
    public int Count { get; set; }
}