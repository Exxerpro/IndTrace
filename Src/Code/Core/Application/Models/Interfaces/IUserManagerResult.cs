// <copyright file="IUserManagerResult.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Interfaces;

/// <summary>
/// Represents the result of a user management operation.
/// </summary>
public interface IUserManagerResult
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    bool Succeeded { get; }

    /// <summary>
    /// Gets the collection of error messages, if any.
    /// </summary>
    IEnumerable<string> Errors { get; }
}