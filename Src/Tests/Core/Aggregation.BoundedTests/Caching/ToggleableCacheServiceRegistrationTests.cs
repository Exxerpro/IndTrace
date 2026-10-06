// <copyright file="ToggleableCacheServiceRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Dependencies.Utilities;
using IndTrace.Persistence.Caching;
using Microsoft.Extensions.Configuration;

namespace Application.AgregationTests.Caching;

/// <summary>
/// Regression tests for #216: the Communications PLC-gateway host registered <c>ICacheService</c> raw
/// (<c>AddSingleton&lt;ICacheService, FusionCacheService&gt;()</c>) in its own composition root, so the
/// "Caching:Toggle" kill-switch decorator introduced by #116 was never wired there and
/// <c>Caching:Toggle:Enabled=false</c> was silently ignored.
///
/// The fix extracts the toggle-decorator wiring into the ONE shared
/// <c>CacheServiceRegistration.AddToggleableCacheService</c> extension, used by BOTH
/// <c>CommonServiceRegistration.AddCommonServices</c> (Monitor) and the Communications <c>Program.cs</c>.
/// These tests pin that shared extension so the idiom cannot drift again.
/// </summary>
public sealed class ToggleableCacheServiceRegistrationTests
{
    private static IConfiguration BuildConfiguration(params KeyValuePair<string, string?>[] entries)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(entries)
            .Build();
    }

    private static ServiceProvider BuildProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Act (registration under test) — the ONE shared cache wiring both production hosts must use.
        services.AddToggleableCacheService(configuration);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// (a) The resolved <c>ICacheService</c> is the <see cref="CacheToggleCacheService"/> kill-switch
    /// decorator — a raw <see cref="FusionCacheService"/> here is the exact #216 Communications bug.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void AddToggleableCacheService_ResolvesCacheToggleDecorator_NotRawFusionCacheService()
    {
        // Arrange
        var configuration = BuildConfiguration();

        // Act
        using var provider = BuildProvider(configuration);
        var cacheService = provider.GetRequiredService<ICacheService>();

        // Assert — the kill-switch decorator is the public cache surface.
        cacheService.ShouldBeOfType<CacheToggleCacheService>();
    }

    /// <summary>
    /// (b) With <c>Caching:Toggle:Enabled=false</c> cache operations are bypassed: a Set is a no-op, a Get is
    /// always a miss, and Exists reports false. Pre-fix the Communications host cached regardless.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public async Task AddToggleableCacheService_ToggleDisabled_BypassesCacheOperations()
    {
        // Arrange — the kill-switch flipped OFF.
        var configuration = BuildConfiguration(
            new KeyValuePair<string, string?>("Caching:Toggle:Enabled", "false"));
        await using var provider = BuildProvider(configuration);
        var cacheService = provider.GetRequiredService<ICacheService>();
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act — write then read the same key.
        await cacheService.SetAsync("toggle-off-key", "cached-value", expiration: null, cancellationToken);
        var cached = await cacheService.GetAsync<string>("toggle-off-key", cancellationToken);
        var exists = await cacheService.ExistsAsync("toggle-off-key", cancellationToken);

        // Assert — nothing was cached; every read is a miss while the toggle is OFF.
        cached.ShouldBeNull();
        exists.ShouldBeFalse();
    }

    /// <summary>
    /// (c) With NO "Caching:Toggle" configuration section at all, caching behaves ENABLED
    /// (<c>CacheToggleOptions.Enabled</c> defaults to true) — default behavior stays byte-identical to the
    /// pre-fix raw registration.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    public async Task AddToggleableCacheService_NoToggleSection_CachingBehavesEnabled()
    {
        // Arrange — empty configuration: no Caching section anywhere.
        var configuration = BuildConfiguration();
        await using var provider = BuildProvider(configuration);
        var cacheService = provider.GetRequiredService<ICacheService>();
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act — write then read the same key.
        await cacheService.SetAsync("toggle-default-key", "cached-value", expiration: null, cancellationToken);
        var cached = await cacheService.GetAsync<string>("toggle-default-key", cancellationToken);
        var exists = await cacheService.ExistsAsync("toggle-default-key", cancellationToken);

        // Assert — the value round-trips: default (absent) config keeps the cache ON.
        cached.ShouldBe("cached-value");
        exists.ShouldBeTrue();
    }
}
