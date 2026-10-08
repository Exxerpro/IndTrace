// <copyright file="IndTraceEventsServiceRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Monitor.Tests;

using IndTrace.Application.UI.Services;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Guards the Monitor host's registration of the Singleton <see cref="IndTraceEventsService"/>. The host registers
/// <see cref="IDateTimeMachine"/> as Scoped, so a type-based Singleton registration lets DI inject the Scoped clock
/// into the Singleton (a captive dependency). <c>ValidateScopes</c> (on in Development) then aborted Monitor
/// start-up with "Cannot consume scoped service IDateTimeMachine from singleton IndTraceEventsService".
/// </summary>
public sealed class IndTraceEventsServiceRegistrationTests
{
    private static readonly ServiceProviderOptions StrictValidation = new() { ValidateScopes = true, ValidateOnBuild = true };

    private static ServiceCollection MonitorLikeServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDateTimeMachine, DateTimeMachine>(); // as in the Monitor host
        return services;
    }

    [Fact]
    public void AddIndTraceEventsService_WithScopedClock_BuildsUnderScopeValidation()
    {
        // Arrange
        var services = MonitorLikeServices();
        services.AddIndTraceEventsService();

        // Act
        using var provider = services.BuildServiceProvider(StrictValidation);

        // Assert — the Singleton resolves from the root provider and is a single process-wide instance.
        var first = provider.GetRequiredService<IndTraceEventsService>();
        provider.GetRequiredService<IndTraceEventsService>().ShouldBeSameAs(first);
    }

    [Fact]
    public void TypeBasedSingletonRegistration_WithScopedClock_IsRejectedAsCaptiveDependency()
    {
        // Arrange — the former registration; documents why the explicit factory is required.
        var services = MonitorLikeServices();
        services.AddSingleton<IndTraceEventsService>();

        // Act / Assert
        Should.Throw<AggregateException>(() => services.BuildServiceProvider(StrictValidation))
            .Message.ShouldContain(nameof(IDateTimeMachine));
    }
}
