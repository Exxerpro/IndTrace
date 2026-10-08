// <copyright file="AccountServiceRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Monitor.Tests;

using IndTrace.Identity.Data;
using IndTrace.Monitor.Components.Account;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The Monitor's account pages (Login, Register, Manage, ...) inject its internal redirect manager, user accessor and
/// e-mail sender. None were registered, so every account page threw at render time and nobody could sign in. These
/// tests build the helpers the way the host does, under strict validation, and resolve each one.
/// </summary>
public sealed class AccountServiceRegistrationTests
{
    [Fact]
    public async Task AddAccountPageServices_ResolvesEveryAccountPageHelperUnderStrictValidation()
    {
        // Arrange — the host provides NavigationManager (Blazor) and UserManager<ApplicationUser> (AddIndTraceIdentity).
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<NavigationManager, StubNavigationManager>();
        services.AddScoped(_ => Substitute.For<IUserStore<ApplicationUser>>());
        services.AddIdentityCore<ApplicationUser>();

        // Act
        services.AddAccountPageServices();
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        await using var scope = provider.CreateAsyncScope();

        // Assert
        scope.ServiceProvider.GetService<IdentityRedirectManager>().ShouldNotBeNull();
        scope.ServiceProvider.GetService<IdentityUserAccessor>().ShouldNotBeNull();
        scope.ServiceProvider.GetService<IEmailSender<ApplicationUser>>().ShouldNotBeNull();
    }

    [Fact]
    public void AddAccountPageServices_KeepsTheCircuitBoundHelpersScoped()
    {
        // Arrange — both helpers wrap per-circuit services, so they must not outlive the circuit.
        var services = new ServiceCollection();

        // Act
        services.AddAccountPageServices();

        // Assert
        services.Single(d => d.ServiceType == typeof(IdentityRedirectManager)).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        services.Single(d => d.ServiceType == typeof(IdentityUserAccessor)).Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    private sealed class StubNavigationManager : NavigationManager
    {
        public StubNavigationManager() => this.Initialize("http://localhost/", "http://localhost/");
    }
}
