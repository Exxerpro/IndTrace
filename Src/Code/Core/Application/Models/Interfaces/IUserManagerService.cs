// <copyright file="IUserManagerService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Interfaces;

/// <summary>
/// Provides user management operations for IndTrace application users.
/// </summary>
/// <typeparam name="TUser">The type of user managed by this service.</typeparam>
public interface IUserManagerService<TUser>
    where TUser : IIndTraceApplicationUser
{
    /// <summary>
    /// Creates a new user with the specified password.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="password">The password for the new user.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<IUserManagerResult> CreateAsync(TUser user, string password);

    /// <summary>
    /// Updates the specified user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<IUserManagerResult> UpdateAsync(TUser user);

    /// <summary>
    /// Deletes the specified user.
    /// </summary>
    /// <param name="user">The user to delete.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<IUserManagerResult> DeleteAsync(TUser user);

    /// <summary>
    /// Finds a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<TUser> FindByIdAsync(string userId);

    /// <summary>
    /// Finds a user by their username.
    /// </summary>
    /// <param name="userName">The username of the user.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<TUser> FindByNameAsync(string userName);

    /// <summary>
    /// Adds the specified user to a role.
    /// </summary>
    /// <param name="user">The user to add to the role.</param>
    /// <param name="role">The role to add the user to.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<IUserManagerResult> AddToRoleAsync(TUser user, string role);

    /// <summary>
    /// Removes the specified user from a role.
    /// </summary>
    /// <param name="user">The user to remove from the role.</param>
    /// <param name="role">The role to remove the user from.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<IUserManagerResult> RemoveFromRoleAsync(TUser user, string role);

    /// <summary>
    /// Changes the password for the specified user.
    /// </summary>
    /// <param name="user">The user whose password will be changed.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<IUserManagerResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword);

    /// <summary>
    /// Checks if the specified password is valid for the user.
    /// </summary>
    /// <param name="user">The user to check the password for.</param>
    /// <param name="password">The password to check.</param>
    /// <returns>True if the password is valid; otherwise, false.</returns>
    Task<bool> CheckPasswordAsync(TUser user, string password);

    /// <summary>
    /// Confirms the email address of the specified user using a token.
    /// </summary>
    /// <param name="user">The user whose email will be confirmed.</param>
    /// <param name="token">The confirmation token.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<IUserManagerResult> ConfirmEmailAsync(TUser user, string token);

    /// <summary>
    /// Gets the currently authenticated user.
    /// </summary>
    /// <returns>The current user.</returns>
    Task<IIndTraceApplicationUser> CurrentUser();
}