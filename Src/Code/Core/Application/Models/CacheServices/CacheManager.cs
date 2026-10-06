// <copyright file="CacheManager.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.CacheServices;

/// <summary>
/// Represents the CacheManager.
/// </summary>
/// <typeparam name="T">The type of the cached value.</typeparam>
/// <param name="cacheDuration">How long a refreshed value stays valid.</param>
/// <param name="dateTimeMachine">The deterministic clock used for cache-age checks; defaults to the system-backed <see cref="DateTimeMachine"/> when omitted (#126 LOW sweep — no raw <see cref="DateTime.UtcNow"/>).</param>
public class CacheManager<T>(TimeSpan cacheDuration, IDateTimeMachine? dateTimeMachine = null)
{
    private T? cachedData;
    private DateTime? cacheTimestamp;
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private readonly IDateTimeMachine clock = dateTimeMachine ?? new DateTimeMachine();

    /// <summary>
    /// Gets the cached data if valid, or refreshes it using the provided function.
    /// </summary>
    /// <param name="refreshFunc">The function to refresh the data if the cache is invalid or expired.</param>
    /// <param name="forceRefresh">Whether to force a refresh of the data, even if the cache is valid.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="logger">An optional logger for logging cache operations.</param>
    /// <returns>The cached or refreshed data.</returns>
    /// <remarks>
    /// This overload caches whatever the factory returns. For Result-shaped factories use the
    /// <see cref="GetOrRefreshAsync(Func{Task{Result{T}}}, bool, CancellationToken, ILogger?)"/> overload,
    /// which refuses to cache failures (#116 fail-loud).
    /// </remarks>
    public async Task<T?> GetOrRefreshAsync(
        Func<Task<T>> refreshFunc,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default,
        ILogger? logger = default)
    {
        await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.cachedData != null && !forceRefresh && this.cacheTimestamp.HasValue && this.clock.UtcNow - this.cacheTimestamp.Value <= cacheDuration)
            {
                if (logger is not null)
                {
                    logger.LogInformation("Returning cached data");
                }

                return this.cachedData;
            }

            this.cachedData = await refreshFunc().ConfigureAwait(false);
            this.cacheTimestamp = this.clock.UtcNow;

            if (logger is not null)
            {
                logger.LogInformation("Returning refreshed data");
            }

            return this.cachedData;
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    /// <summary>
    /// Gets the cached data if valid, or refreshes it using the provided Result-shaped factory.
    /// </summary>
    /// <param name="refreshFunc">The Result-shaped function that produces fresh data.</param>
    /// <param name="forceRefresh">Whether to force a refresh of the data, even if the cache is valid.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="logger">An optional logger for logging cache operations.</param>
    /// <returns>A successful Result carrying the cached or refreshed data, or the factory's failure.</returns>
    /// <remarks>
    /// #116 fail-loud: a FAILED refresh Result is returned to the caller but NEVER cached — the previous
    /// cache state (data and timestamp) is left untouched, so the next call retries the factory instead
    /// of serving a fabricated value for the whole cache duration.
    /// </remarks>
    public async Task<Result<T>> GetOrRefreshAsync(
        Func<Task<Result<T>>> refreshFunc,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default,
        ILogger? logger = default)
    {
        if (refreshFunc is null)
        {
            return Result<T>.WithFailure("Refresh function must not be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<T>.WithFailure("Operation cancelled.");
        }

        await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.cachedData != null && !forceRefresh && this.cacheTimestamp.HasValue && this.clock.UtcNow - this.cacheTimestamp.Value <= cacheDuration)
            {
                if (logger is not null)
                {
                    logger.LogInformation("Returning cached data");
                }

                return Result<T>.Success(this.cachedData);
            }

            var refreshed = await refreshFunc().ConfigureAwait(false);
            if (refreshed.IsFailure || refreshed.Value is null)
            {
                if (logger is not null)
                {
                    logger.LogError("Cache refresh failed; previous cache state kept so the next call retries: {@Errors}", refreshed.Errors);
                }

                return refreshed.IsFailure
                    ? refreshed
                    : Result<T>.WithFailure("Refresh function returned a successful Result without a value.");
            }

            this.cachedData = refreshed.Value;
            this.cacheTimestamp = this.clock.UtcNow;

            if (logger is not null)
            {
                logger.LogInformation("Returning refreshed data");
            }

            return refreshed;
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    /// <summary>
    /// Invalidates the current cache, clearing the cached data and timestamp.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the cache has been cleared.</returns>
    /// <remarks>
    /// #116: acquires the same semaphore as the GetOrRefreshAsync overloads, so an invalidation can no
    /// longer interleave with a concurrent refresh and leave torn cachedData/cacheTimestamp state.
    /// </remarks>
    public async Task InvalidateCacheAsync(CancellationToken cancellationToken = default)
    {
        await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            this.cachedData = default;
            this.cacheTimestamp = null;
        }
        finally
        {
            this.semaphore.Release();
        }
    }
}
