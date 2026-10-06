// <copyright file="IndTraceUser.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Identity.Users;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using IndTrace.Domain.Interfaces;
using IndTrace.Identity.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents the IndTraceUser.
/// </summary>
public class IndTraceUser : IdentityUser, IIndTraceUser
{
    /// <summary>
    /// Gets or sets the UserId.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the UserName, ensuring non-nullable contract.
    /// </summary>
    public override string? UserName
    {
        get => base.UserName;
        set => base.UserName = value;
    }

    /// <summary>
    /// Gets or sets the UserName as non-nullable for the interface contract.
    /// </summary>
    string IIndTraceUser.UserName
    {
        get => base.UserName ?? string.Empty;
        set => base.UserName = value;
    }
}

/// <summary>
/// Represents the IdentityUserAccessor.
/// </summary>
public class IdentityUserAccessor
{
    private readonly UserManager<IndTraceUser> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityUserAccessor"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="userManager">The userManager.</param>
    public IdentityUserAccessor(UserManager<IndTraceUser> userManager)
    {
        this.userManager = userManager;
    }

    /// <summary>
    /// Executes GetCurrentUserAsync operation.
    /// </summary>
    /// <param name="userId">The userId.</param>
    /// <returns>The result of GetCurrentUserAsync.</returns>
    public async Task<IndTraceUser?> GetCurrentUserAsync(string userId)
    {
        return await this.userManager.FindByIdAsync(userId);
    }

    /// <summary>
    /// Executes FindByIdAsync operation.
    /// </summary>
    /// <param name="userId">The userId.</param>
    /// <returns>The result of FindByIdAsync.</returns>
    public async Task<IndTraceUser?> FindByIdAsync(string userId)
    {
        return await this.userManager.FindByIdAsync(userId);
    }
}

/// <summary>
/// Represents the IdentityRedirectManager.
/// </summary>
public class IdentityRedirectManager
{
    /// <summary>
    /// Executes GetRedirectUrl operation.
    /// </summary>
    /// <param name="returnUrl">The returnUrl.</param>
    /// <returns>The result of GetRedirectUrl.</returns>
    public string GetRedirectUrl(string returnUrl)
    {
        // Logic to determine the redirect URL based on the returnUrl
        return string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
    }
}

/// <summary>
/// A server-side <see cref="AuthenticationStateProvider"/> that surfaces the real authenticated
/// principal established by the Blazor circuit and revalidates the connected user's security stamp
/// every 30 minutes while an interactive circuit is connected.
/// </summary>
/// <remarks>
/// The authenticated principal is provided by the framework base (<see cref="ServerAuthenticationStateProvider"/>)
/// which is fed from the request principal at circuit start; this type must never fabricate its own
/// principal from an empty <see cref="ClaimsPrincipal"/>. Only revalidation logic is overridden here.
/// </remarks>
public sealed class IdentityRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IOptions<IdentityOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityRevalidatingAuthenticationStateProvider"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used by the revalidation loop.</param>
    /// <param name="scopeFactory">The scope factory used to resolve a fresh <see cref="UserManager{TUser}"/> per revalidation.</param>
    /// <param name="options">The configured identity options.</param>
    public IdentityRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> options)
        : base(loggerFactory)
    {
        this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Gets the interval between security-stamp revalidations for a connected circuit.
    /// </summary>
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    /// <summary>
    /// Revalidates the authentication state by comparing the principal's security stamp against the store.
    /// </summary>
    /// <param name="authenticationState">The current authentication state to validate.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the revalidation to complete.</param>
    /// <returns><see langword="true"/> when the security stamp is still valid; otherwise <see langword="false"/>.</returns>
    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (authenticationState is null)
        {
            return false;
        }

        // Resolve the user manager from a fresh scope so revalidation reads current store data.
        await using var scope = this.scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await this.ValidateSecurityStampAsync(userManager, authenticationState.User).ConfigureAwait(false);
    }

    private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal).ConfigureAwait(false);
        if (user is null)
        {
            return false;
        }

        if (!userManager.SupportsUserSecurityStamp)
        {
            return true;
        }

        var principalStamp = principal.FindFirstValue(this.options.Value.ClaimsIdentity.SecurityStampClaimType);
        var userStamp = await userManager.GetSecurityStampAsync(user).ConfigureAwait(false);
        return principalStamp == userStamp;
    }
}

/// <summary>
/// Represents the IdentityNoOpEmailSender.
/// </summary>
public class IdentityNoOpEmailSender<TUser> : IEmailSender
    where TUser : IIndTraceApplicationUser
{
    /// <summary>
    /// Executes SendEmailAsync operation.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="subject">The subject.</param>
    /// <param name="htmlMessage">The htmlMessage.</param>
    /// <returns>The result of SendEmailAsync.</returns>
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        throw new NotImplementedException();
    }
}