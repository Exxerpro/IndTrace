// <copyright file="AccountServiceRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Monitor.Components.Account;

using IndTrace.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the helpers the Monitor's account pages (Login, Register, Manage, ...) inject. They are internal to the
/// Monitor; without them every account page threw at render time ("no registered service of type
/// IdentityRedirectManager"), so nobody could sign in.
/// </summary>
internal static class AccountServiceRegistration
{
    /// <summary>
    /// Adds the account pages' redirect manager, user accessor and the no-op e-mail sender.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddAccountPageServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Scoped: both wrap per-circuit services (NavigationManager, UserManager).
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<IdentityUserAccessor>();

        // No e-mail delivery is configured; the account pages show confirmation and reset links on screen instead.
        services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        return services;
    }
}
