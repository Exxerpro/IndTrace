// <copyright file="IIndTraceUser.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

public interface IIndTraceUser
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    int UserId { get; set; }

    /// <summary>
    /// Gets or sets the username for system authentication.
    /// </summary>
    string UserName { get; set; }
}