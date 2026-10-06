// <copyright file="IIndTraceApplicationUser.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

/// <summary>
/// Represents an application user in the IndTrace system.
/// </summary>
public interface IIndTraceApplicationUser
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the normalized username.
    /// </summary>
    string? NormalizedUserName { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    string? Email { get; set; }

    /// <summary>
    /// Gets or sets the normalized email address.
    /// </summary>
    string? NormalizedEmail { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email is confirmed.
    /// </summary>
    bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    string? PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the security stamp.
    /// </summary>
    string? SecurityStamp { get; set; }

    /// <summary>
    /// Gets or sets the concurrency stamp.
    /// </summary>
    string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the phone number is confirmed.
    /// </summary>
    bool PhoneNumberConfirmed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled.
    /// </summary>
    bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Gets or sets the lockout end date and time.
    /// </summary>
    DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether lockout is enabled.
    /// </summary>
    bool LockoutEnabled { get; set; }

    /// <summary>
    /// Gets or sets the number of failed access attempts.
    /// </summary>
    int AccessFailedCount { get; set; }

    /// <summary>
    /// Returns a string representation of the user.
    /// </summary>
    /// <returns>A string that represents the user.</returns>
    string ToString();
}