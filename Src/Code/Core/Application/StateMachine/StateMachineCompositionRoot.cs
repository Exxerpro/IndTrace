// <copyright file="StateMachineCompositionRoot.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Registers the Epic-2 <see cref="IItemStateMachine"/> engine and the Story 3.1 per-handler routing
/// flags (<see cref="StateMachineRoutingOptions"/>) into DI. The four PLC create/cycle handlers resolve
/// the machine through constructor injection (CR1: the <c>IGatewayRequestHandler</c> interface and the
/// <c>GatewayCommandDispatcher</c> handler-map shape are unchanged).
/// </summary>
public static class StateMachineCompositionRoot
{
    /// <summary>
    /// Adds <see cref="IItemStateMachine"/> (singleton; the engine is stateless) and binds
    /// <see cref="StateMachineRoutingOptions"/> from the <c>StateMachineRouting</c> configuration section.
    /// Unset keys keep their defaults (all flags ON), so a partial section flips only the named triggers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration providing the routing-flag section (optional).</param>
    /// <param name="lifetime">
    /// Lifetime for the engine, decorator and path-context registrations. Defaults to <see cref="ServiceLifetime.Singleton"/>
    /// (the engine is stateless), which the PLC Gateway host relies on (its <see cref="IDateTimeMachine"/> +
    /// <see cref="IFlowTransitionLogSink"/> are also Singleton). Hosts whose <see cref="IDateTimeMachine"/> /
    /// sink are <see cref="ServiceLifetime.Scoped"/> (e.g. the Monitor webapp host) MUST pass
    /// <see cref="ServiceLifetime.Scoped"/> so the Singleton decorator does not capture a Scoped dependency
    /// (captive-dependency bug; surfaced by <c>ValidateScopes:true</c>).
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddStateMachineRouting(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        // Story 3.5: the real engine is registered as the concrete type; IItemStateMachine resolves to the
        // logging decorator that wraps it (appends one additive FlowTransitionLog per fire, success OR reject).
        // The decorator returns the inner result verbatim and is best-effort, so handlers are unchanged.
        //
        // D4 (audit finding): the lifetime is parameterized so the Monitor webapp host can register the whole
        // chain Scoped (its IDateTimeMachine + EF sink are Scoped). The default stays Singleton so the gateway
        // host's registration is byte-identical to before.
        services.Add(new ServiceDescriptor(typeof(ItemStateMachine), typeof(ItemStateMachine), lifetime));
        services.Add(new ServiceDescriptor(typeof(ITransitionPathContext), typeof(TransitionPathContext), lifetime));

        // Best-effort sink fallback: the infrastructure (EF-backed) sink wins when the host registers it
        // BEFORE this call; otherwise the no-op sink keeps the decorator resolvable (tests / no-persistence).
        // The fallback matches the requested lifetime so a Scoped chain does not capture a Singleton no-op sink
        // (harmless) nor, more importantly, a Singleton chain capture a Scoped one.
        services.TryAdd(new ServiceDescriptor(typeof(IFlowTransitionLogSink), typeof(NullFlowTransitionLogSink), lifetime));
        services.Add(new ServiceDescriptor(typeof(IItemStateMachine), provider => new LoggingItemStateMachineDecorator(
            provider.GetRequiredService<ItemStateMachine>(),
            provider.GetRequiredService<IFlowTransitionLogSink>(),
            provider.GetRequiredService<ITransitionPathContext>(),
            provider.GetRequiredService<IDateTimeMachine>(),
            provider.GetRequiredService<ILogger<LoggingItemStateMachineDecorator>>(),
            provider.GetService<IOptions<StateMachineRoutingOptions>>()),
            lifetime));

        var section = configuration?.GetSection(StateMachineRoutingOptions.SectionName);
        services.Configure<StateMachineRoutingOptions>(options =>
        {
            if (section is null || !section.Exists())
            {
                return;
            }

            options.RouteCreateBarCode = ReadFlag(section, nameof(StateMachineRoutingOptions.RouteCreateBarCode), options.RouteCreateBarCode);
            options.RouteCreateCycle = ReadFlag(section, nameof(StateMachineRoutingOptions.RouteCreateCycle), options.RouteCreateCycle);
            options.RouteEndOfProcess = ReadFlag(section, nameof(StateMachineRoutingOptions.RouteEndOfProcess), options.RouteEndOfProcess);
            options.RouteUpdateCycleOk = ReadFlag(section, nameof(StateMachineRoutingOptions.RouteUpdateCycleOk), options.RouteUpdateCycleOk);
            options.RouteUpdateCycleNotOk = ReadFlag(section, nameof(StateMachineRoutingOptions.RouteUpdateCycleNotOk), options.RouteUpdateCycleNotOk);
            options.SpecificDiagnostics = ReadFlag(section, nameof(StateMachineRoutingOptions.SpecificDiagnostics), options.SpecificDiagnostics);
            options.RouteReject = ReadFlag(section, nameof(StateMachineRoutingOptions.RouteReject), options.RouteReject);
            options.RouteRestore = ReadFlag(section, nameof(StateMachineRoutingOptions.RouteRestore), options.RouteRestore);
            options.EnableRestoredState = ReadFlag(section, nameof(StateMachineRoutingOptions.EnableRestoredState), options.EnableRestoredState);
            options.EnableInvalidState = ReadFlag(section, nameof(StateMachineRoutingOptions.EnableInvalidState), options.EnableInvalidState);
            options.EnableScrapState = ReadFlag(section, nameof(StateMachineRoutingOptions.EnableScrapState), options.EnableScrapState);
            options.EnableCanceledState = ReadFlag(section, nameof(StateMachineRoutingOptions.EnableCanceledState), options.EnableCanceledState);
            options.LogTransitions = ReadFlag(section, nameof(StateMachineRoutingOptions.LogTransitions), options.LogTransitions);
        });

        return services;
    }

    private static bool ReadFlag(IConfigurationSection section, string key, bool fallback)
    {
        var raw = section[key];
        return bool.TryParse(raw, out var parsed) ? parsed : fallback;
    }
}
