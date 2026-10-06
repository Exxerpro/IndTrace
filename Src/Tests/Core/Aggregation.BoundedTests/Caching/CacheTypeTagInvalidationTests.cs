// <copyright file="CacheTypeTagInvalidationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Caching;
using NSubstitute;
using ZiggyCreatures.Caching.Fusion;

namespace Application.AgregationTests.Caching;

/// <summary>
/// #116 unit tests for the cache type-tag plumbing:
/// <list type="bullet">
/// <item>the <see cref="CacheKeyBuilderReadOnlyRepos"/> tag-derivation helpers (single source of key-format truth),</item>
/// <item><see cref="FusionCacheService.RemoveByPatternAsync"/> actually removing the matching type's entries
/// while entries of OTHER types survive (real in-memory FusionCache, no mocks), and</item>
/// <item><see cref="CacheToggleCacheService"/> ALWAYS forwarding removals to the inner service, even while
/// the kill-switch is OFF (entries cached while it was ON may still be alive).</item>
/// </list>
/// </summary>
public sealed class CacheTypeTagInvalidationTests
{
    private readonly ITestOutputHelper output;

    public CacheTypeTagInvalidationTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private FusionCacheService CreateRealCacheService()
    {
        var fusionCache = new FusionCache(new FusionCacheOptions());
        return new FusionCacheService(fusionCache, XUnitLogger.CreateLogger<FusionCacheService>(this.output));
    }

    [Fact]
    public void BuildTypeTag_ProducesTypePrefixedTag()
    {
        CacheKeyBuilderReadOnlyRepos.BuildTypeTag(nameof(Machine)).ShouldBe("Type:Machine");
    }

    [Fact]
    public void TryGetTypeTag_KeyBuiltByTheBuilder_ReturnsItsTypeTag()
    {
        // The helper and BuildKey share the same "|Type:{X}|" format — round-trip must agree.
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<Machine>("GetById", 5);

        CacheKeyBuilderReadOnlyRepos.TryGetTypeTag(key).ShouldBe(CacheKeyBuilderReadOnlyRepos.BuildTypeTag(nameof(Machine)));
    }

    [Theory]
    [InlineData("GetById|Type:Machine|MachineId:5", "Type:Machine")]
    [InlineData("qa-partition:GetById|Type:Line|LineId:9", "Type:Line")] // partition prefix tolerated
    [InlineData("ListAsync|Type:Cycle|Spec:abc123", "Type:Cycle")]
    public void TryGetTypeTag_KeyWithTypeSegment_ReturnsTag(string key, string expectedTag)
    {
        CacheKeyBuilderReadOnlyRepos.TryGetTypeTag(key).ShouldBe(expectedTag);
    }

    [Theory]
    [InlineData("GetById|MachineId:5")] // no type segment
    [InlineData("some-ad-hoc-key")] // ad-hoc key not built by the builder
    [InlineData("GetById|Type:|MachineId:5")] // empty type name is not a usable tag
    [InlineData("")]
    [InlineData("   ")]
    public void TryGetTypeTag_KeyWithoutUsableTypeSegment_ReturnsNull(string key)
    {
        CacheKeyBuilderReadOnlyRepos.TryGetTypeTag(key).ShouldBeNull();
    }

    /// <summary>
    /// THE tag-invalidation contract: set entries for two entity types, invalidate ONE type name, and prove
    /// the matching type's entry is gone (factory re-runs) while the other type's entry survives (factory
    /// does not re-run).
    /// </summary>
    [Fact]
    public async Task RemoveByPatternAsync_RemovesOnlyTheMatchingTypesEntries()
    {
        // Arrange — one real cache, one tagged entry per entity type.
        var service = this.CreateRealCacheService();
        var cancellationToken = TestContext.Current.CancellationToken;
        var machineKey = CacheKeyBuilderReadOnlyRepos.BuildKey<Machine>("GetById", 1);
        var lineKey = CacheKeyBuilderReadOnlyRepos.BuildKey<Line>("GetById", 1);
        var machineFactoryCalls = 0;
        var lineFactoryCalls = 0;

        Task<Result<string>> MachineFactory(CancellationToken _)
        {
            machineFactoryCalls++;
            return Task.FromResult(Result<string>.Success("machine"));
        }

        Task<Result<string>> LineFactory(CancellationToken _)
        {
            lineFactoryCalls++;
            return Task.FromResult(Result<string>.Success("line"));
        }

        await service.GetOrSetAsync<Result<string>>(machineKey, MachineFactory, cancellationToken: cancellationToken);
        await service.GetOrSetAsync<Result<string>>(lineKey, LineFactory, cancellationToken: cancellationToken);

        // Act — invalidate ONLY the Machine type.
        var removed = await service.RemoveByPatternAsync(nameof(Machine), cancellationToken);

        // Assert — invalidation was issued...
        removed.ShouldBeGreaterThan(0, "pre-#116 this was a documented no-op that always returned 0");

        // ...the Machine entry is gone (factory re-runs on the next call)...
        await service.GetOrSetAsync<Result<string>>(machineKey, MachineFactory, cancellationToken: cancellationToken);
        machineFactoryCalls.ShouldBe(2, "the Machine entry must have been invalidated by its type tag");

        // ...and the Line entry SURVIVED (factory did not re-run).
        await service.GetOrSetAsync<Result<string>>(lineKey, LineFactory, cancellationToken: cancellationToken);
        lineFactoryCalls.ShouldBe(1, "entries of OTHER entity types must survive the invalidation");
    }

    [Fact]
    public async Task RemoveByPatternAsync_EmptyPattern_ReturnsZero()
    {
        var service = this.CreateRealCacheService();

        var removed = await service.RemoveByPatternAsync(string.Empty, TestContext.Current.CancellationToken);

        removed.ShouldBe(0);
    }

    /// <summary>
    /// #116: with the kill-switch OFF the toggle decorator must still FORWARD removals — entries cached
    /// while the toggle was ON may still be alive, and short-circuiting here would strand them.
    /// </summary>
    [Fact]
    public async Task CacheToggle_WhenDisabled_StillForwardsRemovals()
    {
        // Arrange
        var inner = Substitute.For<ICacheService>();
        inner.RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        inner.RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(1);
        var sut = new CacheToggleCacheService(
            inner,
            new CacheToggleOptions { Enabled = false },
            XUnitLogger.CreateLogger<CacheToggleCacheService>(this.output));
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await sut.RemoveAsync("some-key", cancellationToken);
        await sut.RemoveByPatternAsync(nameof(Machine), cancellationToken);

        // Assert — both removals reached the inner service despite the toggle being OFF.
        await inner.Received(1).RemoveAsync("some-key", Arg.Any<CancellationToken>());
        await inner.Received(1).RemoveByPatternAsync(nameof(Machine), Arg.Any<CancellationToken>());
    }
}
