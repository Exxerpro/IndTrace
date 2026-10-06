// <copyright file="ConnectionStatus.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents per-machine connection status — mutable operational state for connectivity monitoring,
/// owned by the Machine aggregate (issue #95). Not a static lookup / smart-enum twin.
/// </summary>
public class ConnectionStatus : AuditableEntity, IEntityRoot
{
    /// <summary>
    /// Gets or sets the machine identifier. Story 35.D2 Cluster 5 (#35): retyped to the strongly-typed
    /// <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>).
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the status code of the connection.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the status was last modified.
    /// </summary>
    public new DateTime ModifiedOn { get; set; }
}