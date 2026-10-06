// <copyright file="ConfigAppFromJson.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

/// <summary>
/// Represents configuration settings loaded from a JSON file, including service and feature flags.
/// </summary>
public class ConfigAppFromJson
{
    /// <summary>
    /// Gets or sets the name of the configuration.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service name associated with the configuration.
    /// </summary>
    public string Service { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the gateway worker is enabled.
    /// </summary>
    public bool GateWayWorker { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether reports are enabled.
    /// </summary>
    public bool Reports { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether GP12 is enabled.
    /// </summary>
    public bool Gp12 { get; set; }

    /// <summary>
    /// Gets or sets the path to the logs directory.
    /// </summary>
    public string LogsPath { get; set; } = string.Empty;
}