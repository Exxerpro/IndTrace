// <copyright file="IdentityUserAccessor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.AspNetCore.Identity;
using IndTrace.Identity.Data;

namespace IndTrace.Monitor.Components.Account;

/// <summary>
/// Provides access to the current user with automatic redirect handling for invalid users.
/// </summary>
internal sealed class IdentityUserAccessor(UserManager<ApplicationUser> userManager, IdentityRedirectManager redirectManager)
{
    /// <summary>
    /// Gets the current user from the HTTP context, redirecting to an error page if the user is not found.
    /// </summary>
    /// <param name="context">The HTTP context containing the user information.</param>
    /// <returns>The current application user.</returns>
    public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}
