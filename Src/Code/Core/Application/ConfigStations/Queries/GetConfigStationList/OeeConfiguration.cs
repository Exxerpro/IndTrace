// <copyright file="OeeConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Queries.GetConfigStationList;

/// <summary>
/// Represents the OEE (Overall Equipment Effectiveness) configuration for the application or a specific station.
/// </summary>
public class OeeConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether OEE is enabled globally.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a dictionary indicating OEE enablement by machine ID.
    /// </summary>
    public Dictionary<int, bool> EnabledByMachine { get; set; } = [];
}