// <copyright file="IdentityRevalidatingAuthenticationStateProviderTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Identity.UnitTests;

using System.Security.Claims;
using IndTrace.Identity.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

/// <summary>
/// Regression tests for <see cref="IdentityRevalidatingAuthenticationStateProvider"/>.
/// Prior to the P0-16 fix the provider fabricated its own principal from an empty
/// <see cref="ClaimsPrincipal"/> and blocked on <c>.Result</c>, so it was always anonymous.
/// These tests pin the corrected behaviour: the real circuit-provided principal is surfaced.
/// </summary>
public sealed class IdentityRevalidatingAuthenticationStateProviderTests
{
    private static IdentityRevalidatingAuthenticationStateProvider CreateProvider() =>
        new(
            NullLoggerFactory.Instance,
            Substitute.For<IServiceScopeFactory>(),
            Options.Create(new IdentityOptions()));

    /// <summary>
    /// When the host has established a signed-in principal, the provider surfaces it as authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetAuthenticationStateAsync_ReturnsAuthenticatedPrincipal_ForSignedInUser()
    {
        var provider = CreateProvider();
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "operator-1") },
            authenticationType: "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(principal)));

        var state = await provider.GetAuthenticationStateAsync();

        state.ShouldNotBeNull();
        var isAuthenticated = state.User.Identity?.IsAuthenticated == true;
        isAuthenticated.ShouldBeTrue();
        var name = state.User.Identity?.Name;
        name.ShouldBe("operator-1");
    }

    /// <summary>
    /// When no user has signed in, the provider surfaces an anonymous principal (never a fabricated one).
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetAuthenticationStateAsync_ReturnsAnonymous_WhenNoUserSignedIn()
    {
        var provider = CreateProvider();
        provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));

        var state = await provider.GetAuthenticationStateAsync();

        state.ShouldNotBeNull();
        var isAuthenticated = state.User.Identity?.IsAuthenticated == true;
        isAuthenticated.ShouldBeFalse();
    }

    /// <summary>
    /// The provider's constructor dependencies (logger factory, scope factory, identity options)
    /// are all satisfiable from a standard DI container, proving the graph composes after the
    /// self-referential UserManager registration was removed.
    /// </summary>
    [Fact]
    public void Provider_ComposesFromDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.Configure<IdentityOptions>(_ => { });
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var resolved = scope.ServiceProvider.GetRequiredService<AuthenticationStateProvider>();

        resolved.ShouldBeOfType<IdentityRevalidatingAuthenticationStateProvider>();
    }
}
