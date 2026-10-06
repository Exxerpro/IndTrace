// <copyright file="ConfigStation.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

/// <summary>
/// Represents the configuration for a station, including client, plant, line, and project details.
/// </summary>
public class ConfigStation : AuditableEntity, IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigStation"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ConfigStation()
    {
        this.ConfigAppId = string.Empty;
        this.Client = string.Empty;
        this.Factory = string.Empty;
        this.Line = string.Empty;
        this.Project = string.Empty;
        this.Version = string.Empty;
    }

    /// <summary>
    /// Gets or sets the application configuration identifier.
    /// </summary>
    public string ConfigAppId { get; set; }

    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public int AppId { get; set; }

    /// <summary>
    /// Gets or sets the client name.
    /// </summary>
    public string Client { get; set; }

    /// <summary>
    /// Gets or sets the plant name.
    /// </summary>
    public string Factory { get; set; }

    /// <summary>
    /// Gets or sets the line name.
    /// </summary>
    public string Line { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Project { get; set; }

    /// <summary>
    /// Gets or sets the version of the configuration.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the date of the version.
    /// </summary>
    public DateTime VersionDate { get; set; }

    /// <summary>
    /// Gets or sets the date the configuration was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public int PlcId { get; set; }
    public int Pc { get; set; }

    /// <summary>
    /// Returns a string representation of the station configuration.
    /// </summary>
    /// <returns>A string containing the config app ID, cliente, planta, and linea.</returns>
    public override string ToString() => $"Station {this.ConfigAppId}: {this.Client}/{this.Factory}/{this.Line}";
}