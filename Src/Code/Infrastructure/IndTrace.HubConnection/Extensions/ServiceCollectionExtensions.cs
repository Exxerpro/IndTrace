// <copyright file="ServiceCollectionExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Extensions;

using IndTrace.Application.Models.Services;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Dashboard;
using IndTrace.HubConnection.Implementations;
using IndTrace.HubConnection.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Hub connection options, <see cref="IHubMetricsDashboard"/>, and the <see cref="IHubConnectionFactory"/>.
    /// </summary>
    public static IServiceCollection AddHubConnectionAbstractions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HubMonitorOptions>(configuration.GetSection(nameof(HubMonitorOptions)));
        // Options validation centralized here so hosts don't need to register it
        services.AddSingleton<IValidateOptions<HubMonitorOptions>, HubMonitorOptionsValidator>();
        // Dashboard singleton for metrics aggregation
        services.AddSingleton<IHubMetricsDashboard, HubMetricsDashboard>();

        // Register base factory and decorate with metrics registration
        services.AddSingleton<HubConnectionFactory>();
        services.AddSingleton<IHubConnectionFactory>(sp =>
        {
            var baseFactory = sp.GetRequiredService<HubConnectionFactory>();
            var dashboard = sp.GetRequiredService<IHubMetricsDashboard>();
            return new MetricsRegisteringHubConnectionFactory(baseFactory, dashboard);
        });
        return services;
    }
}
