// <copyright file="ProductionGraphCache.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

using System.Collections.Concurrent;

/// <summary>
/// An immutable per-ProductId routing cache entry (#83): the validated <see cref="ProductionGraph"/> a product's
/// routing topology builds to (never <c>null</c> for a cached entry — only the SUCCESS-path graph is cached),
/// paired with the reconstructed magic-0 <c>LastMachineId</c>-keyed workflow lookup built from that same graph.
/// </summary>
/// <remarks>
/// <para>
/// Both members are built ONCE (via <see cref="ProductionGraph.Create"/> and
/// <c>BarCodeResult.BuildWorkflowLookupFromGraph</c>). The graph aggregate is inherently immutable and stays
/// shared; the lookup dictionary is NOT immutable, so the entry isolates it (#217): the constructor takes
/// ownership of a shallow COPY of the supplied lookup (the builder's instance never aliases the cache), and
/// <see cref="Lookup"/> serves a fresh shallow snapshot per read (no consumer ever holds the cached instance,
/// so no dispatch can corrupt what later dispatches see). The dictionaries are tens of entries, so the per-read
/// copy is trivial on the hot path.
/// </para>
/// </remarks>
public sealed record ProductionGraphCacheEntry
{
    // #217: the entry's OWN copy of the lookup — never the instance the builder supplied, never handed out.
    private readonly Dictionary<int, WorkFlow> lookup;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionGraphCacheEntry"/> class, taking ownership of a
    /// shallow copy of <paramref name="lookup"/> (#217 — the caller's instance never aliases the cache).
    /// </summary>
    /// <param name="graph">The validated production graph for the product (the success-path graph; never null here).</param>
    /// <param name="lookup">The reconstructed magic-0 workflow lookup built from <paramref name="graph"/>.</param>
    /// <param name="version">
    /// #224: the routing version (see <see cref="IProductRoutingVersionProbe"/>) probed BEFORE the rows this
    /// entry was built from were read — the freshness stamp a later probe compares against.
    /// </param>
    public ProductionGraphCacheEntry(ProductionGraph? graph, Dictionary<int, WorkFlow> lookup, ulong version)
    {
        this.Graph = graph;
        this.lookup = new Dictionary<int, WorkFlow>(lookup);
        this.Version = version;
    }

    /// <summary>
    /// Gets the validated production graph for the product. The graph is immutable, so the single instance is
    /// safely shared by every consumer.
    /// </summary>
    public ProductionGraph? Graph { get; }

    /// <summary>
    /// Gets a fresh shallow snapshot of the reconstructed magic-0 workflow lookup. Every read returns a NEW
    /// dictionary (#217), so mutating a served instance can never corrupt the cached routing other dispatches see.
    /// </summary>
    public Dictionary<int, WorkFlow> Lookup => new(this.lookup);

    /// <summary>
    /// Gets the #224 freshness stamp: the routing version probed BEFORE the rows this entry was built from
    /// were read. A consumer serves this entry only when a fresh probe returns the SAME version; any committed
    /// routing edit (in this process or any other) bumps the rows' rowversions, so the next probe mismatches
    /// and the consumer rebuilds from the current rows instead of serving this stale entry.
    /// </summary>
    public ulong Version { get; }
}

/// <summary>
/// A per-ProductId cache of a product's validated routing <see cref="ProductionGraph"/> and its reconstructed
/// workflow lookup (#83). The routing topology of a product is STATIC within a run — routing authoring is gated
/// off and unwired in production — so the expensive <see cref="ProductionGraph.Create"/> structural validation
/// (O(V²·E)) and the two routing table reads that feed it are performed once per product and reused on every
/// subsequent barcode read, byte-identically.
/// </summary>
/// <remarks>
/// <para>
/// The cache is a process-wide singleton shared across the per-scope <c>BarCodeResult</c> instances, so it must be
/// thread-safe. <see cref="Invalidate(int)"/> / <see cref="Clear"/> are the in-process invalidation seam: the
/// routing write path (<c>ProductRoutingRepository.SaveAsync</c>) calls <see cref="Invalidate(int)"/> for the
/// edited product (#219) so the SAVING process drops its cached topology immediately.
/// </para>
/// <para>
/// #224 — cross-process staleness is closed by VERSIONING, not by invalidation: each entry carries the routing
/// <see cref="ProductionGraphCacheEntry.Version"/> it was built at, and consumers probe the current version
/// (<see cref="IProductRoutingVersionProbe"/>) before serving, rebuilding on any mismatch. An edit committed by
/// another host (routing authored in Monitor while the Communications gateway validates arrivals) therefore
/// invalidates itself on the next read — no cross-process invalidation transport is needed, and the in-process
/// <see cref="Invalidate(int)"/> remains only an immediate fast path for the saving host.
/// </para>
/// </remarks>
public interface IProductionGraphCache
{
    /// <summary>
    /// Returns the cached entry for <paramref name="productId"/>, or <see langword="null"/> on a miss.
    /// </summary>
    /// <param name="productId">The product whose cached routing is sought.</param>
    /// <returns>The cached entry, or <see langword="null"/> when the product is not cached.</returns>
    ProductionGraphCacheEntry? Get(int productId);

    /// <summary>
    /// Stores (or replaces) the cached routing entry for <paramref name="productId"/>.
    /// </summary>
    /// <param name="productId">The product being cached.</param>
    /// <param name="entry">The immutable routing entry to cache.</param>
    void Set(int productId, ProductionGraphCacheEntry entry);

    /// <summary>
    /// Removes any cached routing for <paramref name="productId"/> (the authoring invalidation seam).
    /// </summary>
    /// <param name="productId">The product whose cached routing is dropped.</param>
    void Invalidate(int productId);

    /// <summary>
    /// Removes all cached routing entries.
    /// </summary>
    void Clear();
}

/// <summary>
/// Thread-safe <see cref="IProductionGraphCache"/> backed by a <see cref="ConcurrentDictionary{TKey, TValue}"/>.
/// A double build under a cache-miss race is harmless: the two builds are byte-identical (a pure function of the
/// same product routing rows), so a last-writer-wins <see cref="Set(int, ProductionGraphCacheEntry)"/> never
/// changes the served value.
/// </summary>
public sealed class ProductionGraphCache : IProductionGraphCache
{
    private readonly ConcurrentDictionary<int, ProductionGraphCacheEntry> entries = new();

    /// <inheritdoc/>
    public ProductionGraphCacheEntry? Get(int productId) =>
        this.entries.TryGetValue(productId, out var entry) ? entry : null;

    /// <inheritdoc/>
    public void Set(int productId, ProductionGraphCacheEntry entry) =>
        this.entries[productId] = entry;

    /// <inheritdoc/>
    public void Invalidate(int productId) => this.entries.TryRemove(productId, out _);

    /// <inheritdoc/>
    public void Clear() => this.entries.Clear();
}
