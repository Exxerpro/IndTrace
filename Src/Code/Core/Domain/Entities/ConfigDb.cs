// <copyright file="ConfigDb.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents configuration information for the database system, including version and modification dates.
/// </summary>
public class ConfigDb : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigDb"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ConfigDb()
    {
        this.DatabaseVersion = string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the system information.
    /// </summary>
    public int SystemInformationId { get; set; }

    /// <summary>
    /// Gets or sets the version of the database.
    /// </summary>
    public string DatabaseVersion { get; set; }

    /// <summary>
    /// Gets or sets the date of the database version.
    /// </summary>
    public DateTime VersionDate { get; set; }

    /// <summary>
    /// Gets or sets the date the configuration was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Returns a string representation of the database configuration.
    /// </summary>
    /// <returns>A string containing the system information ID, database version, and version date.</returns>
    public override string ToString() => $"Config {this.SystemInformationId}: {this.DatabaseVersion} ({this.VersionDate:yyyy-MM-dd})";
}
