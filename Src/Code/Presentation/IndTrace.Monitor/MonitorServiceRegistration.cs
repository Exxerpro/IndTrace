// <copyright file="MonitorServiceRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Monitor;

using IndTrace.Application.UI.Services;
using IndTrace.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Monitor host registrations that need an explicit lifetime decision, kept out of <c>Program</c> so tests can
/// exercise the exact registration under <c>ValidateScopes</c>/<c>ValidateOnBuild</c>.
/// </summary>
public static class MonitorServiceRegistration
{
    /// <summary>
    /// Registers the process-wide Singleton <see cref="IndTraceEventsService"/> with its own Singleton clock.
    /// </summary>
    /// <remarks>
    /// The Monitor registers <see cref="IndTrace.Domain.Interfaces.IDateTimeMachine"/> as Scoped. A type-based
    /// Singleton registration would let DI inject that Scoped clock into the Singleton — a captive dependency that
    /// <c>ValidateScopes</c> rejects at start-up. Same pattern as <c>IUserOfflineCreationService</c>.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddIndTraceEventsService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(provider => new IndTraceEventsService(
            provider.GetService<ILogger<IndTraceEventsService>>(),
            dateTimeMachine: new DateTimeMachine()));

        return services;
    }
}
