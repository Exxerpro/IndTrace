// <copyright file="IndTraceUser.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

/// <summary>
/// Represents a user entity with system access and identification information.
/// </summary>
public class IndTraceUser : AuditableEntity, IEntityRoot, IIndTraceUser
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the username for system authentication.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}