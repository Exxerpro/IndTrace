// <copyright file="ICacheService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

/// <summary>
/// Cache service abstraction for repository pattern.
/// Designed to support Redis-like operations for future flexibility.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache or creates it using the provided factory.
    /// Redis-like GET with automatic SET if not found.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory to create the value if not in cache.</param>
    /// <param name="expiration">Optional expiration time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from cache.
    /// Redis-like DEL operation.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the key was removed, false if not found.</returns>
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached values belonging to one entity type.
    /// #116: the "pattern" is the ENTITY TYPE NAME (e.g. <c>nameof(Machine)</c>, i.e.
    /// <c>typeof(T).Name</c>), not a wildcard/regex — implementations match it against the type tag every
    /// repository cache entry is stored under. Called by the write-side invalidation decorator after every
    /// successful Add/Update/Delete so cached reads never outlive a write WITHIN THE SAME PROCESS: the
    /// backing FusionCache is process-local (no distributed level or backplane), so a write in one process
    /// (e.g. the PLC gateway) does NOT invalidate another process's cache (e.g. Monitor) — reads there stay
    /// stale until TTL expiry. The cross-process gap is tracked on issue #129.
    /// </summary>
    /// <param name="pattern">The entity type name whose cached entries must be invalidated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A positive number when the invalidation was issued successfully, 0 on invalid input or failure
    /// (implementations may not know the exact per-key count).
    /// </returns>
    Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache with optional expiration.
    /// Redis-like SET operation.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Optional expiration time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets a value from cache.
    /// Redis-like GET operation.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached value or null if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Checks if a key exists in cache.
    /// Redis-like EXISTS operation.
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}