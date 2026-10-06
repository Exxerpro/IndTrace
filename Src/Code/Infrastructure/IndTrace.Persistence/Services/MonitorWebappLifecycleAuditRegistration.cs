// <copyright file="MonitorWebappLifecycleAuditRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Persistence.Services;

using IndTrace.Application.BarCodes.Commands.CancelCycle;
using IndTrace.Application.BarCodes.Commands.MarkInvalid;
using IndTrace.Application.BarCodes.Commands.MarkScrap;
using IndTrace.Application.BarCodes.Commands.Reject;
using IndTrace.Application.BarCodes.Commands.Restore;
using IndTrace.Application.Repository;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// D4 (audit finding, FR5): wires the Monitor (Blazor) webapp host so EVERY manual lifecycle action
/// (Reject / Restore / MarkInvalid / MarkScrap / Cancel) appends one additive <c>FlowTransitionLog</c> row.
///
/// Before D4 the Monitor host registered the five command handlers with their 4-argument ctor, so each
/// handler fell back to an UNDECORATED <c>new ItemStateMachine()</c> and logged nothing. The audit wiring
/// existed only in the PLC Gateway host. This extension closes the gap by:
/// <list type="number">
/// <item>registering the EF-backed <see cref="FlowTransitionLogSink"/> BEFORE state-machine routing so it
/// wins over the no-op <c>NullFlowTransitionLogSink</c> fallback (mirrors the gateway reference pattern);</item>
/// <item>calling <c>AddStateMachineRouting</c> with <see cref="ServiceLifetime.Scoped"/> — the Monitor host's
/// <see cref="IDateTimeMachine"/> and <see cref="IIndTraceDbContextFactory"/> (hence the sink) are Scoped, so
/// a Singleton decorator would be a CAPTIVE dependency; Scoped keeps the whole chain lifetime-consistent and
/// passes <c>ValidateScopes:true</c>;</item>
/// <item>registering the five handler factories with their 6-argument ctor, injecting the resolved (decorated)
/// <see cref="IItemStateMachine"/> + <see cref="IOptions{StateMachineRoutingOptions}"/> so they no longer hit
/// the undecorated internal fallback.</item>
/// </list>
///
/// This changes NO transition OUTCOME (FR8 already holds) — it adds only the audit row. The gateway host is
/// untouched: it keeps calling <c>AddStateMachineRouting</c> with the default Singleton lifetime.
///
/// PREREQUISITES the host must already have registered (the Monitor host does, via <c>AddRepositories</c>,
/// <c>AddScoped&lt;IDateTimeMachine&gt;</c> and <c>AddIndTracePersistence</c>): the three
/// <see cref="IRepository{T}"/> leaves, <see cref="IDateTimeMachine"/>, <see cref="IIndTraceDbContextFactory"/>
/// and logging.
/// </summary>
public static class MonitorWebappLifecycleAuditRegistration
{
    /// <summary>
    /// Registers the Monitor webapp lifecycle-audit chain (EF sink + Scoped state-machine routing + the five
    /// 6-arg handler factories). Idempotent-safe to call once during host composition.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration providing the routing-flag section (optional).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMonitorWebappLifecycleAudit(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // 1. EF-backed sink, registered BEFORE AddStateMachineRouting so it wins over the TryAdd no-op fallback.
        //    Scoped because its IIndTraceDbContextFactory dependency is Scoped in the Monitor host.
        services.AddScoped<IFlowTransitionLogSink, FlowTransitionLogSink>();

        // 2. State-machine routing registered Scoped (NOT the default Singleton) — see class remarks: the
        //    Monitor host's IDateTimeMachine + sink are Scoped, so a Singleton decorator would be captive.
        services.AddStateMachineRouting(configuration, ServiceLifetime.Scoped);

        // 3. The five webapp command handlers, now built with the 6-arg ctor so they resolve the DECORATED
        //    IItemStateMachine (which appends the FlowTransitionLog row) instead of `new ItemStateMachine()`.
        services.AddScoped<IMonitorRequestHandler<RejectBarCodeCommand, BarCodeRejectedView>>(sp =>
            new RejectBarCodeCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IDateTimeMachine>(),
                sp.GetRequiredService<IItemStateMachine>(),
                sp.GetService<IOptions<StateMachineRoutingOptions>>(),
                sp.GetService<ILogger<RejectBarCodeCommandHandler>>()));

        services.AddScoped<IMonitorRequestHandler<RestoreBarCodeCommand, BarCodeRestoredView>>(sp =>
            new RestoreBarCodeCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IDateTimeMachine>(),
                sp.GetRequiredService<IItemStateMachine>(),
                sp.GetService<IOptions<StateMachineRoutingOptions>>(),
                sp.GetService<ILogger<RestoreBarCodeCommandHandler>>()));

        services.AddScoped<IMonitorRequestHandler<MarkInvalidCommand, BarCodeMarkedInvalidView>>(sp =>
            new MarkInvalidCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IDateTimeMachine>(),
                sp.GetRequiredService<IItemStateMachine>(),
                sp.GetService<IOptions<StateMachineRoutingOptions>>(),
                sp.GetService<ILogger<MarkInvalidCommandHandler>>()));

        services.AddScoped<IMonitorRequestHandler<MarkScrapCommand, BarCodeMarkedScrapView>>(sp =>
            new MarkScrapCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IDateTimeMachine>(),
                sp.GetRequiredService<IItemStateMachine>(),
                sp.GetService<IOptions<StateMachineRoutingOptions>>(),
                sp.GetService<ILogger<MarkScrapCommandHandler>>()));

        services.AddScoped<IMonitorRequestHandler<CancelCycleCommand, CycleCanceledView>>(sp =>
            new CancelCycleCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>(),
                sp.GetRequiredService<IDateTimeMachine>(),
                sp.GetRequiredService<IItemStateMachine>(),
                sp.GetService<IOptions<StateMachineRoutingOptions>>(),
                sp.GetService<ILogger<CancelCycleCommandHandler>>()));

        return services;
    }
}
