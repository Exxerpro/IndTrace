// <copyright file="MonitorProductAuthoringRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Products;

using IndTrace.Application.Configuration;
using IndTrace.Dependencies.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

/// <summary>
/// E11.4-4b (issue #94) — DI composition guard proving the production Monitor (Blazor) webapp host can now
/// resolve the whole <c>Add product</c> flow end-to-end. Builds the EXACT graph the Monitor host builds — the
/// real prod <c>AddRepositories(Scoped)</c> + <c>AddReadOnlyRepositories(Scoped)</c> + the system-under-test
/// <see cref="MonitorProductAuthoringRegistration.AddMonitorProductAuthoring"/> over the minimal prerequisites
/// the host already provides (a Scoped <see cref="IIndTraceDbContextFactory"/> + <see cref="ICacheService"/> for
/// the repository leaves, a Scoped <see cref="IDateTimeMachine"/>, <c>IOptions&lt;RoutingAuthoringOptions&gt;</c>
/// bound OFF-by-default, logging, and the <see cref="IMonitorRequestDispatcher"/>) — then asserts:
/// <list type="bullet">
/// <item>(a) building the provider with <c>ValidateScopes:true</c> + <c>ValidateOnBuild:true</c> does NOT throw —
/// proving no captive dependency (Scoped-in-Singleton) and no missing registration anywhere in the graph;</item>
/// <item>(b) the <c>IMonitorRequestHandler&lt;CreateProductCommand, ProductCreatedEvent&gt;</c> the dispatcher
/// looks up resolves to the concrete <see cref="CreateProductCommandHandler"/> with its full eleven-dependency
/// SRP graph;</item>
/// <item>(c) the <see cref="IMonitorRequestDispatcher"/> (the entry point the Blazor page calls) resolves too;</item>
/// <item>(d) the negative case — WITHOUT the extension the handler does NOT resolve — proving this guard genuinely
/// covers the gap (the <c>No service for type ... IMonitorRequestHandler&lt;CreateProductCommand,...&gt;</c> the
/// add-product page hit before this fix).</item>
/// </list>
/// This is the liveness deliverable: it fails if the create-product graph is ever unwired or mis-scoped again.
/// </summary>
public class MonitorProductAuthoringRegistrationTests
{
    // A throwaway InMemory IIndTraceDbContextFactory — the repository leaves depend on it. The test only RESOLVES
    // the graph (it does not run a command), so the store is never written; a real factory keeps the resolution
    // honest (the repositories construct against the real dependency, not a mock that could hide a ctor change).
    private sealed class InMemoryContextFactory : IIndTraceDbContextFactory, IDisposable
    {
        private readonly string databaseName = Guid.NewGuid().ToString();

        private IndTraceDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<IndTraceDbContext>()
                .UseInMemoryDatabase(this.databaseName)
                .Options;
            return new IndTraceDbContext(options);
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

    // Builds the Monitor-host prerequisites + the system under test (the product-authoring extension). Lifetimes
    // mirror the production Monitor host: AddRepositories(Scoped) + AddReadOnlyRepositories(Scoped),
    // AddScoped<IDateTimeMachine>, AddScoped<IIndTraceDbContextFactory> (via AddIndTracePersistence), and
    // IOptions<RoutingAuthoringOptions> bound OFF-by-default (fail-closed authoring) via AddCommonServices.
    private static ServiceCollection BuildMonitorProductAuthoringServices(InMemoryContextFactory factory)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        // Repository leaves the create-product graph consumes. ICacheService is only resolved (never called) by the
        // read-only repositories during resolution, so a substitute keeps the graph honest without a real cache.
        services.AddScoped<IIndTraceDbContextFactory>(_ => factory);
        services.AddSingleton(_ => Substitute.For<ICacheService>());

        services.AddScoped<IDateTimeMachine, DateTimeMachine>();

        // Fail-closed authoring: bound with defaults (Enabled=false) exactly as AddCommonServices binds it in prod.
        services.Configure<RoutingAuthoringOptions>(_ => { });

        // The Blazor page's dispatch entry point. Resolving it proves the wire the add-product page actually calls.
        services.AddScoped<IMonitorRequestDispatcher, MonitorRequestDispatcher>();

        // The real production repository registrations (Scoped) — supply every IRepository<>/IReadOnlyRepository<>
        // leaf plus IAggregateRepository<ProductRouting> the graph needs.
        services.AddRepositories(ServiceLifetime.Scoped);
        services.AddReadOnlyRepositories(ServiceLifetime.Scoped);

        // System under test: the EXACT wiring IndTrace.Monitor/Program.cs now performs.
        services.AddMonitorProductAuthoring();

        return services;
    }

    /// <summary>
    /// (a)+(b)+(c): the create-product graph builds under ValidateScopes/ValidateOnBuild (no captive dependency,
    /// no missing registration), the handler resolves to the concrete <see cref="CreateProductCommandHandler"/>
    /// with its full SRP graph, and the dispatcher entry point resolves.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void MonitorProductAuthoringWiring_ResolvesHandlerAndDispatcher_UnderScopeValidation()
    {
        // Arrange
        using var factory = new InMemoryContextFactory();
        var services = BuildMonitorProductAuthoringServices(factory);

        // Act — ValidateOnBuild + ValidateScopes is the trap that surfaces a captive Scoped-in-Singleton or any
        // missing registration in the eleven-dependency graph AT BUILD, not at first request.
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });

        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // Assert (b): the exact closed handler interface the IMonitorRequestDispatcher looks up resolves to the
        // concrete handler with its full graph.
        var handler = sp.GetRequiredService<IMonitorRequestHandler<CreateProductCommand, ProductCreatedEvent>>();
        handler.ShouldBeOfType<CreateProductCommandHandler>();

        // Assert (c): the dispatch entry point the Blazor add-product page calls resolves.
        var dispatcher = sp.GetRequiredService<IMonitorRequestDispatcher>();
        dispatcher.ShouldNotBeNull();
    }

    /// <summary>
    /// (b, focused): every SRP collaborator in the create-product graph resolves individually — a failure here
    /// pinpoints exactly which registration is missing rather than a generic ValidateOnBuild aggregate.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void MonitorProductAuthoringWiring_ResolvesEverySrpCollaborator()
    {
        // Arrange
        using var factory = new InMemoryContextFactory();
        var services = BuildMonitorProductAuthoringServices(factory);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });

        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // Act & Assert — each collaborator the handler injects must resolve.
        Should.NotThrow(() => sp.GetRequiredService<IProductValidator>());
        Should.NotThrow(() => sp.GetRequiredService<IProductFactory>());
        Should.NotThrow(() => sp.GetRequiredService<IProductEventFactory>());
        Should.NotThrow(() => sp.GetRequiredService<IProductUniquenessValidator>());
        Should.NotThrow(() => sp.GetRequiredService<ICustomerLookupService>());
        Should.NotThrow(() => sp.GetRequiredService<ILineLookupService>());
        Should.NotThrow(() => sp.GetRequiredService<IWorkflowOrchestrator>());
        Should.NotThrow(() => sp.GetRequiredService<IRoutingAuthoringService>());
        Should.NotThrow(() => sp.GetRequiredService<IRuleOrchestrator>());
        Should.NotThrow(() => sp.GetRequiredService<IRecipeOrchestrator>());
        Should.NotThrow(() => sp.GetRequiredService<IProductPersistenceOrchestrator>());
    }

    /// <summary>
    /// (d): proves the guard genuinely covers the gap — a Monitor-host service collection that has the repository
    /// leaves but OMITS <c>AddMonitorProductAuthoring</c> (the pre-fix state) fails to resolve the handler, exactly
    /// the "No service for type ... IMonitorRequestHandler&lt;CreateProductCommand, ...&gt;" the add-product page
    /// would hit before this fix.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void WithoutProductAuthoringRegistration_CreateProductHandlerDoesNotResolve()
    {
        // Arrange — prerequisites only, WITHOUT the product-authoring extension (the pre-fix Monitor host).
        using var factory = new InMemoryContextFactory();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IIndTraceDbContextFactory>(_ => factory);
        services.AddSingleton(_ => Substitute.For<ICacheService>());
        services.AddScoped<IDateTimeMachine, DateTimeMachine>();
        services.Configure<RoutingAuthoringOptions>(_ => { });
        services.AddRepositories(ServiceLifetime.Scoped);
        services.AddReadOnlyRepositories(ServiceLifetime.Scoped);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => scope.ServiceProvider
            .GetRequiredService<IMonitorRequestHandler<CreateProductCommand, ProductCreatedEvent>>());
    }
}
