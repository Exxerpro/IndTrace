// <copyright file="CyclesCompositionRoot.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles;

using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Services.Strategies;

/// <summary>
/// Configures dependency injection for cycle-related services.
/// </summary>
public static class CyclesCompositionRoot
{
    /// <summary>
    /// Adds cycle services to the service collection with transparent substitution support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="useRefactoredHandlers">Whether to use the refactored handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCycleServices(
        this IServiceCollection services,
        bool useRefactoredHandlers = false)
    {
        // Core application services
        services.AddScoped<IBarCodeInfoProvider, BarCodeInfoProvider>();
        services.AddScoped<IStationValidator, StationValidator>();
        services.AddScoped<IRegisterCleaner, RegisterCleaner>();

        // #40 Chunk 40-E: the cycle-OK/NotOk persistence tail is now the BarCode aggregate operation-scoped unit of
        // work (IAggregateRepository<BarCode>, registered in the persistence composition root). The retired
        // IPersistenceOrchestrator (three separate commits) is no longer registered — the strategies consume the
        // aggregate repository directly (LoadAsync → CompleteOk/NotOkCycle → SaveAsync).
        services.AddScoped<ICommandLogger, CommandLogger>();
        
        // Domain services (singleton for stateless business rules)
        ServiceCollectionServiceExtensions.AddSingleton<ICycleTimeValidator, CycleTimeValidator>(services);
        ServiceCollectionServiceExtensions.AddSingleton<IFlowStatusCalculator, FlowStatusCalculator>(services);
        
        // Strategy implementations
        services.AddScoped<OkUpdateStrategy>();
        services.AddScoped<NotOkUpdateStrategy>();
        
        // Strategy factory
        services.AddScoped<ICycleUpdateStrategyFactory, CycleUpdateStrategyFactory>();
        
        // Handler registration. Production passes useRefactoredHandlers:true (Program.cs): the unified
        // UpdateCyclesCommandHandler serves BOTH cycle-update gateway commands (via the name-preserving
        // .Refactored.cs adapters). The legacy inline handlers were retired in Story 6.3, so when the flag is
        // false NO cycle-update handler is registered — CycleUpdateHandlerRegistrationTests pins both sides.
        if (useRefactoredHandlers)
        {
            // Use new unified handler for both commands
            services.AddScoped<UpdateCyclesCommandHandler>();

            // Register as both command handlers
            services.AddScoped<IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>>(
                provider => provider.GetRequiredService<UpdateCyclesCommandHandler>());

            services.AddScoped<IGatewayRequestHandler<UpdateCyclesNotOkCommand, TaskGatewayResponseDto>>(
                provider => provider.GetRequiredService<UpdateCyclesCommandHandler>());
        }

        return services;
    }
}