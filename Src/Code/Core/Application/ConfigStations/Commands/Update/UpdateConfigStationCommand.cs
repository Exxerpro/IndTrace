// <copyright file="UpdateConfigStationCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Commands.Update;

/// <summary>
/// Command to update the configuration of a station.
/// </summary>
public class UpdateConfigStationCommand : IMonitorRequest<ConfigStationUpdated>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateConfigStationCommand"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public UpdateConfigStationCommand()
    {
        this.ConfigId = string.Empty;
        this.Client = string.Empty;
        this.Factorie = string.Empty;
        this.Line = string.Empty;
        this.Machine = string.Empty;
        this.Project = string.Empty;
        this.Version = string.Empty;
    }

    /// <summary>
    /// Gets or sets the configuration identifier.
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the client name.
    /// </summary>
    public string Client { get; set; }

    /// <summary>
    /// Gets or sets the factory name.
    /// </summary>
    public string Factorie { get; set; }

    /// <summary>
    /// Gets or sets the line name.
    /// </summary>
    public string Line { get; set; }

    /// <summary>
    /// Gets or sets the machine name.
    /// </summary>
    public string Machine { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Project { get; set; }

    /// <summary>
    /// Gets or sets the version string.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the date of the version.
    /// </summary>
    public DateTime VersionDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the configuration was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}