// <copyright file="DomainRegistrations.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Interfaces;
using IndTrace.Application.WorkFlows.Commands.Create;
using IndTrace.Application.Variables.Commands.Create;
using IndTrace.Application.Settings.Commands.Create;

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Simplified domain registration using working patterns from DependenciesFactory
/// Frictionless approach - only register what's actually working
/// </summary>
public static class DomainRegistrations
{
    /// <summary>
    /// Register the working domain handlers - copied from DependenciesFactory working patterns
    /// </summary>
    public static IServiceCollection AddAllDomainHandlers(this IServiceCollection services)
    {
        // Copy exact working registrations from DependenciesFactory lines 448-451
        services.AddScoped<IMonitorRequestHandler<CreateWorkFlowCommand, WorkFlowCreatedEvent>, CreateWorkFlowCommandHandler>();
        services.AddScoped<IMonitorRequestHandler<CreateVariableCommand, VariableCreatedEvent>, CreateVariableCommandHandler>();
        services.AddScoped<IMonitorRequestHandler<CreateSettingCommand, SettingCreatedEvent>, CreateSettingCommandHandler>();
        services.AddScoped<IMonitorRequestHandler<CreateProductCommand, ProductCreatedEvent>, CreateProductCommandHandler>();

        return services;
    }
}