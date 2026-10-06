// <copyright file="ProductionGraphCacheIsolationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;

namespace Application.UnitTests.Characterization.Routing;

/// <summary>
/// Issue #217: the dictionary stored inside a <see cref="ProductionGraphCacheEntry"/> must NEVER be the same
/// instance a consumer holds. The cache is a process-wide singleton serving every PLC dispatch, so a consumer
/// mutating the dictionary it was handed (or the builder mutating the dictionary it cached) must not corrupt
/// what later dispatches see. <see cref="ProductionGraphCache.Get"/> keeps returning the SAME entry instance
/// (pinned by <c>ProductionGraphCacheParityTests.CacheApi_GetSetInvalidateClear_BehaveAsSpecified</c>); the
/// isolation lives in the entry, which owns a private copy and serves snapshots.
/// </summary>
public class ProductionGraphCacheIsolationTests
{
    private const int ProductId = 7;

    private static Dictionary<int, WorkFlow> BuildLookup() =>
        new()
        {
            [100] = new WorkFlow { ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
            [400] = new WorkFlow { ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(0) },
        };

    /// <summary>
    /// Mutating the dictionary handed out by a cached entry must not corrupt what a later consumer sees: the
    /// same shared-cache scenario as two PLC dispatches for one product. Pre-#217 the entry aliased ONE
    /// dictionary to every caller, so the first consumer's <c>Clear()</c> emptied the second consumer's routing.
    /// </summary>
    [Fact]
    public void Get_MutatingTheServedLookup_DoesNotCorruptWhatLaterConsumersSee()
    {
        // Arrange: a cached entry for one product, as the miss path builds it.
        var cache = new ProductionGraphCache();
        cache.Set(ProductId, new ProductionGraphCacheEntry(null, BuildLookup(), version: 0));

        // Act: first consumer mutates the dictionary it was served.
        var firstEntry = cache.Get(ProductId);
        firstEntry.ShouldNotBeNull();
        var firstServed = firstEntry.Lookup;
        firstServed.Clear();
        firstServed[999] = new WorkFlow { ProductId = ProductId, LastMachineId = new MachineId(999), NextMachineId = new MachineId(0) };

        // Assert: a second consumer still sees the ORIGINAL routing entries.
        var secondEntry = cache.Get(ProductId);
        secondEntry.ShouldNotBeNull();
        var secondServed = secondEntry.Lookup;
        secondServed.Count.ShouldBe(2);
        secondServed.ContainsKey(100).ShouldBeTrue();
        secondServed.ContainsKey(400).ShouldBeTrue();
        secondServed.ContainsKey(999).ShouldBeFalse();

        // The two consumers never share an instance.
        ReferenceEquals(firstServed, secondServed).ShouldBeFalse();
    }

    /// <summary>
    /// The entry takes ownership of a COPY at construction: the builder mutating (or reusing) the dictionary it
    /// cached must not reach into the cache. Pre-#217 the miss path cached the very instance it returned to its
    /// caller, so the caller's dictionary WAS the cache.
    /// </summary>
    [Fact]
    public void EntryConstruction_CopiesTheLookup_SoTheBuilderInstanceNeverAliasesTheCache()
    {
        // Arrange: the builder's own dictionary, cached as the miss path does.
        var builderLookup = BuildLookup();
        var cache = new ProductionGraphCache();
        cache.Set(ProductId, new ProductionGraphCacheEntry(null, builderLookup, version: 0));

        // Act: the builder's caller mutates the instance it kept.
        builderLookup.Clear();

        // Assert: the cached routing is intact.
        var served = cache.Get(ProductId);
        served.ShouldNotBeNull();
        served.Lookup.Count.ShouldBe(2);
        served.Lookup.ContainsKey(100).ShouldBeTrue();
        served.Lookup.ContainsKey(400).ShouldBeTrue();
    }
}
