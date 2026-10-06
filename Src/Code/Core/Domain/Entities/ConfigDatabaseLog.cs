// <copyright file="ConfigDatabaseLog.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a database configuration log entity for audit and monitoring purposes.
/// </summary>
public class ConfigDatabaseLog : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigDatabaseLog"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ConfigDatabaseLog()
    {
        this.DatabaseUser = string.Empty;
        this.Event = string.Empty;
        this.Schema = string.Empty;
        this.Object = string.Empty;
        this.Tsql = string.Empty;
        this.XmlEvent = string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the database log entry.
    /// </summary>
    public int DatabaseLogId { get; set; }

    /// <summary>
    /// Gets or sets the time the event was posted.
    /// </summary>
    public DateTime PostTime { get; set; }

    /// <summary>
    /// Gets or sets the database user associated with the event.
    /// </summary>
    public string DatabaseUser { get; set; }

    /// <summary>
    /// Gets or sets the event type or description.
    /// </summary>
    public string Event { get; set; }

    /// <summary>
    /// Gets or sets the schema involved in the event.
    /// </summary>
    public string Schema { get; set; }

    /// <summary>
    /// Gets or sets the object involved in the event.
    /// </summary>
    public string Object { get; set; }

    /// <summary>
    /// Gets or sets the T-SQL statement executed during the event.
    /// </summary>
    public string Tsql { get; set; }

    /// <summary>
    /// Gets or sets the XML event data.
    /// </summary>
    public string XmlEvent { get; set; }

    /// <summary>
    /// Returns a string representation of the database log entry.
    /// </summary>
    /// <returns>A string containing the database log ID, event, and post time.</returns>
    public override string ToString() => $"DatabaseLog {this.DatabaseLogId}: {this.Event} at {this.PostTime}";
}
