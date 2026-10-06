// <copyright file="MonitorWebappLifecycleAuditRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.StateMachine;

using System;
using System.Threading;
using System.Threading.Tasks;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.StateMachine;
using IndTrace.Persistence.Interfaces;
using IndTrace.Persistence.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

/// <summary>
/// D4 (audit finding, FR5) — DI composition guard proving the production Monitor (Blazor) webapp host now
/// AUDITS every manual lifecycle action. Builds the EXACT graph
/// <see cref="MonitorWebappLifecycleAuditRegistration.AddMonitorWebappLifecycleAudit"/> registers (the same
/// extension <c>IndTrace.Monitor/Program.cs</c> calls) over the minimal prerequisite fakes the Monitor host
/// already provides (Scoped repositories + Scoped <see cref="IDateTimeMachine"/> + Scoped
/// <see cref="IIndTraceDbContextFactory"/> + logging), then asserts:
/// <list type="bullet">
/// <item>(a) <see cref="IItemStateMachine"/> resolves to the <see cref="LoggingItemStateMachineDecorator"/>
/// (the decorated machine that appends the FlowTransitionLog row), NOT a bare <see cref="ItemStateMachine"/>;</item>
/// <item>(b) <see cref="IFlowTransitionLogSink"/> resolves to the EF <see cref="FlowTransitionLogSink"/>,
/// NOT the <see cref="NullFlowTransitionLogSink"/> no-op fallback;</item>
/// <item>(c) each of the five webapp <see cref="IMonitorRequestHandler{TCommand, TResponse}"/> resolves;</item>
/// <item>(d) building the provider with <c>ValidateScopes:true</c> + <c>ValidateOnBuild:true</c> does NOT throw —
/// proving the Scoped state-machine chain captures NO Scoped dependency in a Singleton (no captive dependency).</item>
/// </list>
/// </summary>
public class MonitorWebappLifecycleAuditRegistrationTests
{
    // A throwaway InMemory IIndTraceDbContextFactory — the audit chain's EF sink depends on it. The test only
    // RESOLVES the graph (it does not fire), so the store is never written; a real factory keeps the resolution
    // honest (the sink constructs against the real dependency, not a mock that hides ctor changes).
    private sealed class InMemoryContextFactory : IIndTraceDbContextFactory, IDisposable
    {
        private readonly string databaseName = Guid.NewGuid().ToString();

        private IndTrace.Persistence.DBContext.IndTraceDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<IndTrace.Persistence.DBContext.IndTraceDbContext>()
                .UseInMemoryDatabase(this.databaseName)
                .Options;
            return new IndTrace.Persistence.DBContext.IndTraceDbContext(options);
        }

        public DbContext CreateEfDbContext() => this.NewContext();

        public Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IIndTraceDbContext>(this.NewContext());

        public IIndTraceDbContext CreateDbContext() => this.NewContext();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public void Dispose()
        {
        }
    }

    // Builds the minimal Monitor-host prerequisites + the system under test (the audit extension). Lifetimes
    // mirror the production Monitor host: AddRepositories(Scoped), AddScoped<IDateTimeMachine>,
    // AddScoped<IIndTraceDbContextFactory> (via AddIndTracePersistence).
    private static ServiceCollection BuildMonitorAuditServices(InMemoryContextFactory factory)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddScoped(_ => Substitute.For<IRepository<BarCode>>());
        services.AddScoped(_ => Substitute.For<IRepository<TaskGatewayRequest>>());
        // #114 chunk C: the five webapp handlers now take the READ-ONLY cycle repository (Cycle writes
        // belong to IAggregateRepository<BarCode>).
        services.AddScoped(_ => Substitute.For<IReadOnlyRepository<Cycle>>());

        // #95 Slice E: CancelCycleCommandHandler now takes the BarCode aggregate UoW (the production Monitor
        // host registers it via CommonServiceRegistration at repository lifetime).
        services.AddScoped(_ => Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>());
        services.AddScoped<IDateTimeMachine>(_ => new DateTimeMachine());
        services.AddScoped<IIndTraceDbContextFactory>(_ => factory);

        // System under test: the EXACT wiring IndTrace.Monitor/Program.cs now performs.
        services.AddMonitorWebappLifecycleAudit();

        return services;
    }

    /// <summary>
    /// (a)+(b)+(d): the audit chain resolves to the DECORATED machine + the EF sink, and the provider builds
    /// under ValidateScopes/ValidateOnBuild (no captive dependency from the Scoped chain).
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void MonitorAuditWiring_ResolvesDecoratedMachineAndEfSink_UnderScopeValidation()
    {
        // Arrange
        using var factory = new InMemoryContextFactory();
        var services = BuildMonitorAuditServices(factory);

        // Act — ValidateOnBuild + ValidateScopes is the trap that surfaces a captive Scoped-in-Singleton.
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });

        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // Assert (a): IItemStateMachine is the logging decorator, not the bare engine — so a fire appends a row.
        var machine = sp.GetRequiredService<IItemStateMachine>();
        machine.ShouldBeOfType<LoggingItemStateMachineDecorator>();

        // Assert (b): the sink is the EF-backed implementation that wins over the TryAdd no-op fallback.
        var sink = sp.GetRequiredService<IFlowTransitionLogSink>();
        sink.ShouldBeOfType<FlowTransitionLogSink>();
    }

    /// <summary>
    /// (c): every one of the five webapp lifecycle command handlers resolves through the audited graph.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void MonitorAuditWiring_ResolvesAllFiveWebappLifecycleHandlers()
    {
        // Arrange
        using var factory = new InMemoryContextFactory();
        var services = BuildMonitorAuditServices(factory);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });

        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // Act & Assert
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
    }
}
