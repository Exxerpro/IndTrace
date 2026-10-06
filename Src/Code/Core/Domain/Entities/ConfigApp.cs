// <copyright file="ConfigApp.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

/// <summary>
/// Represents the configuration settings for an application instance, including machine, PLC, and project details.
/// </summary>
public class ConfigApp : AuditableEntity, IEntityRoot
{
    /// <summary>
    /// Gets or sets the unique identifier for the application configuration.
    /// </summary>
    public string ConfigAppId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the machine identifier associated with the configuration.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the machine Name associated with the configuration.
    /// </summary>
    public string Machine { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PLC identifier associated with the configuration.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the PC identifier associated with the configuration.
    /// </summary>
    public string Pc { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public int AppId { get; set; }

    /// <summary>
    /// Gets or sets the client name for the configuration.
    /// </summary>
    public string Client { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the factory name for the configuration.
    /// </summary>
    public string Factory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line name for the configuration.
    /// </summary>
    public string Line { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name for the configuration.
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the configuration.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}