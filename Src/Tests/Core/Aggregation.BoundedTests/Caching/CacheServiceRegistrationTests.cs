// <copyright file="CacheServiceRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Dependencies.Utilities;
using IndTrace.Persistence.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Serilog;

namespace Application.AgregationTests.Caching;

/// <summary>
/// Regression tests for #116: the "Caching:Toggle" kill-switch decorator must survive the EXACT registration
/// sequence the production Monitor host performs (<c>AddCommonServices</c> then <c>AddEventsServices</c>).
///
/// Pre-fix, <c>AddEventsServices</c> re-registered <c>ICacheService</c> with a plain <c>AddSingleton</c>, and
/// DI last-wins resolution replaced the <see cref="CacheToggleCacheService"/> decorator (registered by
/// <c>AddCommonServices</c>) with the raw <see cref="FusionCacheService"/> — so flipping the
/// "Caching:Toggle:Enabled" kill-switch could never actually disable the cache in production.
/// </summary>
public sealed class CacheServiceRegistrationTests
{
    private static IConfiguration BuildMinimalConfiguration()
    {
        // AddIndTracePersistence (inside AddCommonServices) fail-louds without a connection string; the string
        // is only stored by the pooled-factory registration, never dialled during this test.
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:IndTraceDbContext"] =
                    "Server=(local);Database=IndTraceRegistrationTest;Integrated Security=true;TrustServerCertificate=true",
                ["Caching:Toggle:Enabled"] = "true",
            })
            .Build();
    }

    private static IHostEnvironment BuildProductionHostEnvironment()
    {
        // Production environment — the exact environment in which the kill-switch regression mattered most.
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);
        environment.ApplicationName.Returns("IndTrace.Monitor");
        environment.ContentRootPath.Returns(AppContext.BaseDirectory);
        return environment;
    }

    /// <summary>
    /// Builds the service collection the way <c>IndTrace.Monitor/Program.cs</c> does — AddCommonServices first,
    /// AddEventsServices second — and asserts the resolved <c>ICacheService</c> is still the
    /// <see cref="CacheToggleCacheService"/> kill-switch decorator, NOT the raw <see cref="FusionCacheService"/>.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void MonitorRegistrationSequence_ResolvesCacheToggleDecorator_NotRawFusionCacheService()
    {
        // Arrange — mirror the Monitor host: configuration, Serilog logger, hosting environment.
        var configuration = BuildMinimalConfiguration();
        Serilog.ILogger logger = new LoggerConfiguration().CreateLogger();
        var environment = BuildProductionHostEnvironment();

        var services = new ServiceCollection();

        // Act — the EXACT prod sequence (Monitor Program.cs lines ~91/~93): common services register the
        // toggle decorator, then the events services registration runs after it.
        services.AddCommonServices(configuration, logger, environment);
        services.AddEventsServices(configuration, logger);

        using var provider = services.BuildServiceProvider();
        var cacheService = provider.GetRequiredService<ICacheService>();

        // Assert — the kill-switch decorator survived; a raw FusionCacheService here means the
        // "Caching:Toggle" kill-switch is unreachable in production.
        cacheService.ShouldBeOfType<CacheToggleCacheService>();
    }

    /// <summary>
    /// Standalone <c>AddEventsServices</c> (no prior <c>AddCommonServices</c> — the Integration host shape must
    /// keep working) still registers a usable <c>ICacheService</c>: TryAdd only defers to an EXISTING
    /// registration, it must not leave the service unregistered when called alone.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    public void StandaloneEventsServices_StillRegistersCacheService()
    {
        // Arrange
        var configuration = BuildMinimalConfiguration();
        Serilog.ILogger logger = new LoggerConfiguration().CreateLogger();

        var services = new ServiceCollection();
        services.AddLogging();

        // Act — AddEventsServices with no prior ICacheService registration.
        services.AddEventsServices(configuration, logger);

        using var provider = services.BuildServiceProvider();
        var cacheService = provider.GetRequiredService<ICacheService>();

        // Assert — the fallback raw registration is preserved for standalone callers.
        cacheService.ShouldBeOfType<FusionCacheService>();
    }

    /// <summary>
    /// #116 write-invalidation wiring: when the composition root registers an <c>ICacheService</c> (as every
    /// production host does), the production <c>AddRepositories</c> helper must resolve <c>IRepository&lt;T&gt;</c>
    /// as the <see cref="CacheInvalidatingRepository{T}"/> decorator so writes invalidate the read cache.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void AddRepositories_WithCacheServiceRegistered_ResolvesCacheInvalidatingDecorator()
    {
        // Arrange — minimal root: repositories + their two constructor deps + a cache service.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IIndTraceDbContextFactory>());
        services.AddSingleton(Substitute.For<ICacheService>());

        // Act
        services.AddRepositories(ServiceLifetime.Scoped);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Machine>>();

        // Assert — writes go through the invalidating decorator.
        repository.ShouldBeOfType<CacheInvalidatingRepository<Machine>>();

        // B2: the append-only Register seam is decorated too (audit appends must invalidate cached lists).
        var appendOnly = scope.ServiceProvider.GetRequiredService<IAppendOnlyRepository<Register>>();
        appendOnly.ShouldBeOfType<CacheInvalidatingAppendOnlyRepository<Register>>();
    }

    /// <summary>
    /// #116 fallback: composition roots WITHOUT an <c>ICacheService</c> (test hosts) must keep resolving the
    /// raw repository unchanged — the decorator is only engaged when there is a cache to invalidate.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    public void AddRepositories_WithoutCacheService_ResolvesRawRepository()
    {
        // Arrange — same root, but NO ICacheService registration.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IIndTraceDbContextFactory>());

        // Act
        services.AddRepositories(ServiceLifetime.Scoped);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Machine>>();

        // Assert — no cache, no decorator (raw repository for both seams).
        repository.ShouldBeOfType<Repository<Machine>>();
        scope.ServiceProvider.GetRequiredService<IAppendOnlyRepository<Register>>()
            .ShouldBeOfType<Repository<Register>>();
    }
}
