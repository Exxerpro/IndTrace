// <copyright file="MonitorCommandHandlerRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Commands.CancelCycle;
using IndTrace.Application.BarCodes.Commands.MarkInvalid;
using IndTrace.Application.BarCodes.Commands.MarkScrap;

namespace Application.UnitTests.Commands;

/// <summary>
/// DI-resolution guard for the four-argument (bare) construction of the five webapp monitor COMMAND handlers
/// (Reject / Restore / MarkInvalid / MarkScrap / Cancel).
///
/// Background: <c>IMonitorRequestDispatcher</c> resolves these handlers with
/// <see cref="ServiceProviderServiceExtensions.GetRequiredService{T}"/> at runtime when a webapp button is
/// pressed. They were registered ONLY in the test composition root
/// (<c>Aggregation.BoundedTests/Services/ServiceRegistration.cs:1203-1247</c>) and NOT in the production
/// Monitor host, so the buttons would throw
/// "No service for type ... IMonitorRequestHandler&lt;RejectBarCodeCommand,...&gt;". This test reproduces the
/// four required Scoped leaf deps + the AddScoped factory shape and asserts all five handlers resolve - from
/// the root provider AND from a created scope - under <c>ValidateScopes</c>/<c>ValidateOnBuild</c> (the
/// Development default that surfaces lifetime mismatches).
///
/// NOTE (audit finding D4, FR5): the PRODUCTION Monitor host (<c>IndTrace.Monitor/Program.cs</c>) no longer
/// uses this bare 4-arg shape. It now calls <c>AddMonitorWebappLifecycleAudit</c>
/// (<c>IndTrace.Persistence.Services</c>), which builds the handlers with the 6-arg ctor so they resolve the
/// DECORATED IItemStateMachine + the EF FlowTransitionLogSink and append one FlowTransitionLog row per action.
/// That audited graph is guarded by <c>MonitorWebappLifecycleAuditRegistrationTests</c> in
/// Aggregation.BoundedTests (which can see the Persistence-layer EF sink; this Application-layer project
/// cannot). This test remains a valid guard for the handlers' legacy positional 4-arg fallback path
/// (IItemStateMachine?, IOptions&lt;StateMachineRoutingOptions&gt;? both unregistered -&gt; internal
/// `new ItemStateMachine()` + default routing), which the golden-master characterization tests still rely on.
/// </summary>
public class MonitorCommandHandlerRegistrationTests
{
    /// <summary>
    /// Reproduces the bare 4-arg construction of the five webapp monitor command handlers: mocked Scoped leaf
    /// deps (the repositories + clock) plus five AddScoped factory registrations. No state machine engine is
    /// registered, so the handlers fall back to their internal `new ItemStateMachine()` + default routing — the
    /// legacy positional path the golden-master characterization tests exercise. (The production Monitor host
    /// now uses the audited 6-arg wiring via AddMonitorWebappLifecycleAudit; see this class's remarks.)
    /// </summary>
    private static IServiceCollection BuildMonitorHostHandlerServices()
    {
        var services = new ServiceCollection();

        // Scoped leaf deps - the four required ctor dependencies of every handler. Scoped to mirror the
        // production lifetimes (AddRepositories(Scoped) + AddScoped<IDateTimeMachine>) so scoped resolution
        // exercises the same lifetime graph the Monitor host has.
        services.AddScoped(_ => Substitute.For<IRepository<BarCode>>());
        services.AddScoped(_ => Substitute.For<IRepository<TaskGatewayRequest>>());
        services.AddScoped(_ => Substitute.For<IReadOnlyRepository<Cycle>>());
        services.AddScoped(_ => Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>());
        services.AddScoped(_ => Substitute.For<IDateTimeMachine>());

        // The system under test: the EXACT five registrations IndTrace.Monitor/Program.cs performs.
        services.AddScoped<IMonitorRequestHandler<RejectBarCodeCommand, BarCodeRejectedView>>(sp =>
            new RejectBarCodeCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IDateTimeMachine>()));

        services.AddScoped<IMonitorRequestHandler<RestoreBarCodeCommand, BarCodeRestoredView>>(sp =>
            new RestoreBarCodeCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IDateTimeMachine>()));

        services.AddScoped<IMonitorRequestHandler<MarkInvalidCommand, BarCodeMarkedInvalidView>>(sp =>
            new MarkInvalidCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IDateTimeMachine>()));

        services.AddScoped<IMonitorRequestHandler<MarkScrapCommand, BarCodeMarkedScrapView>>(sp =>
            new MarkScrapCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IDateTimeMachine>()));

        services.AddScoped<IMonitorRequestHandler<CancelCycleCommand, CycleCanceledView>>(sp =>
            new CancelCycleCommandHandler(
                sp.GetRequiredService<IRepository<BarCode>>(),
                sp.GetRequiredService<IRepository<TaskGatewayRequest>>(),
                sp.GetRequiredService<IReadOnlyRepository<Cycle>>(),
                sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>(),
                sp.GetRequiredService<IDateTimeMachine>()));

        return services;
    }

    /// <summary>
    /// Proves the production Monitor-host registration graph resolves all five webapp monitor command handlers
    /// without throwing - the failure the dispatcher would otherwise hit on the first button press. Resolves
    /// inside a created scope (the Scoped handlers + their Scoped repo deps must resolve under a scope) with the
    /// root provider built under <c>ValidateScopes</c>/<c>ValidateOnBuild</c>.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void MonitorHostRegistrations_ResolveAllFiveWebappCommandHandlers()
    {
        // Arrange
        var services = BuildMonitorHostHandlerServices();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });

        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // Act & Assert - every handler the IMonitorRequestDispatcher resolves must be present.
        Should.NotThrow(() =>
            sp.GetRequiredService<IMonitorRequestHandler<RejectBarCodeCommand, BarCodeRejectedView>>());
        Should.NotThrow(() =>
            sp.GetRequiredService<IMonitorRequestHandler<RestoreBarCodeCommand, BarCodeRestoredView>>());
        Should.NotThrow(() =>
            sp.GetRequiredService<IMonitorRequestHandler<MarkInvalidCommand, BarCodeMarkedInvalidView>>());
        Should.NotThrow(() =>
            sp.GetRequiredService<IMonitorRequestHandler<MarkScrapCommand, BarCodeMarkedScrapView>>());
        Should.NotThrow(() =>
            sp.GetRequiredService<IMonitorRequestHandler<CancelCycleCommand, CycleCanceledView>>());

        sp.GetRequiredService<IMonitorRequestHandler<RejectBarCodeCommand, BarCodeRejectedView>>()
            .ShouldNotBeNull();
        sp.GetRequiredService<IMonitorRequestHandler<CancelCycleCommand, CycleCanceledView>>()
            .ShouldNotBeNull();
    }

    /// <summary>
    /// Proves the guard genuinely covers the gap: a Monitor-host service collection that registers the leaf
    /// deps but OMITS the five handler registrations (the pre-fix state) fails to resolve them - exactly the
    /// "No service for type ... IMonitorRequestHandler&lt;RejectBarCodeCommand,...&gt;" the webapp Reject button
    /// would hit before this fix.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void WithoutHandlerRegistrations_RejectHandlerDoesNotResolve()
    {
        // Arrange - leaf deps only, no handler registrations (the pre-fix Monitor host).
        var services = new ServiceCollection();
        services.AddScoped(_ => Substitute.For<IRepository<BarCode>>());
        services.AddScoped(_ => Substitute.For<IRepository<TaskGatewayRequest>>());
        services.AddScoped(_ => Substitute.For<IReadOnlyRepository<Cycle>>());
        services.AddScoped(_ => Substitute.For<IDateTimeMachine>());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => scope.ServiceProvider
            .GetRequiredService<IMonitorRequestHandler<RejectBarCodeCommand, BarCodeRejectedView>>());
    }
}
