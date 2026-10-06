// <copyright file="CacheToggleCacheService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using Microsoft.Extensions.Logging;

namespace IndTrace.Persistence.Caching;

/// <summary>
/// Decorator that can disable caching transparently based on configuration.
/// </summary>
public class CacheToggleCacheService : ICacheService
{
    private readonly ICacheService inner;
    private readonly CacheToggleOptions options;
    private readonly ILogger<CacheToggleCacheService> logger;

    public CacheToggleCacheService(ICacheService inner, CacheToggleOptions options, ILogger<CacheToggleCacheService> logger)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        this.options = options ?? new CacheToggleOptions();
        this.logger = logger;
    }

    private bool Disabled => options?.Enabled == false;

    public async Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (Disabled)
        {
            // Bypass cache entirely
            try { return await factory(cancellationToken).ConfigureAwait(false); }
            catch (Exception ex) { logger.LogError(ex, "Cache disabled: factory failed for key {Key}", key); return null; }
        }
        return await inner.GetOrSetAsync<T>(key, factory, expiration, cancellationToken).ConfigureAwait(false);
    }

    // #116: removals ALWAYS forward to the inner service, even while the toggle is OFF. Entries cached
    // while the toggle was ON may still be alive; short-circuiting a removal here would strand them and
    // serve stale data the moment the toggle is flipped back ON. Forwarding when nothing is cached is a
    // cheap no-op, so always-forward is strictly safer.
    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return await inner.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        return await inner.RemoveByPatternAsync(pattern, cancellationToken).ConfigureAwait(false);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (Disabled) return; // no-op
        await inner.SetAsync(key, value, expiration, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (Disabled) return null; // always a miss when disabled
        return await inner.GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (Disabled) return false;
        return await inner.ExistsAsync(key, cancellationToken).ConfigureAwait(false);
    }
}

