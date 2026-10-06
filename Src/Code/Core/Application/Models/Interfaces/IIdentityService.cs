// <copyright file="IIdentityService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Interfaces;

/// <summary>
/// IIdentityService interface defines the essential methods and properties for managing user identity.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Registers a new user with the given credentials.
    /// </summary>
    /// <param name="username">Username of the user.</param>
    /// <param name="password">Password of the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RegisterAsync(string username, string password);

    /// <summary>
    /// Authenticates a user with the given credentials.
    /// </summary>
    /// <param name="username">Username of the user.</param>
    /// <param name="password">Password of the user.</param>
    /// <returns>A task representing the authentication result.</returns>
    Task<bool> SignInAsync(string username, string password);

    /// <summary>
    /// Signs out the currently authenticated user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SignOutAsync();

    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
    /// <param name="oldPassword">Current password of the user.</param>
    /// <param name="newPassword">New password for the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ChangePasswordAsync(string oldPassword, string newPassword);

    /// <summary>
    /// Assigns a role to the specified user.
    /// </summary>
    /// <param name="username">Username of the user.</param>
    /// <param name="role">Role to be assigned.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AssignRoleAsync(string username, string role);

    /// <summary>
    /// Gets a list of roles for the specified user.
    /// </summary>
    /// <param name="username">Username of the user.</param>
    /// <returns>A task representing the list of roles for the user.</returns>
    Task<IEnumerable<string>> GetRolesAsync(string username);

    // Other identity-related properties and methods can be added as needed.
}