// <copyright file="FusionCacheService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace IndTrace.Persistence.Caching;

/// <summary>
/// FusionCache implementation of ICacheService.
/// Provides Redis-like caching operations using FusionCache.
/// </summary>
public class FusionCacheService : ICacheService
{
    private readonly IFusionCache _fusionCache;
    private readonly ILogger<FusionCacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="FusionCacheService"/> class.
    /// </summary>
    /// <param name="fusionCache">The FusionCache instance.</param>
    /// <param name="logger">The logger.</param>
    public FusionCacheService(IFusionCache fusionCache, ILogger<FusionCacheService> logger)
    {
        _fusionCache = fusionCache ?? throw new ArgumentNullException(nameof(fusionCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogError("Cache key cannot be null or empty");
            return null;
        }

        if (factory is null)
        {
            _logger.LogError("Factory function cannot be null");
            return null;
        }

        try
        {
            // #116: tag the entry with its entity type (derived from the |Type:{X}| key segment) so a write
            // can invalidate every cached read of that type via RemoveByPatternAsync -> RemoveByTagAsync.
            // Keys without a type segment are stored untagged, exactly as before.
            var typeTag = CacheKeyBuilderReadOnlyRepos.TryGetTypeTag(key);

            return await _fusionCache.GetOrSetAsync<T?>(
                key,
                async (ctx, ct) =>
                {
                    var value = await factory(ct).ConfigureAwait(false);

                    // #61: never cache a failed/None Result. Negative-caching a "not found" for an entity that
                    // was just created would serve a phantom miss for the whole TTL (wrong-product / wrong-rule
                    // / missing-references risk). Skip the write for this call; the value is still returned to
                    // the caller. Success values keep their normal duration/fail-safe behaviour.
                    // #187: a SUCCESSFUL Result<T> holding an EMPTY collection is the collection-shaped
                    // "not found" (ListAsync on an empty table) and is skipped for the same reason — caching it
                    // hides rows seeded out-of-band until a service restart.
                    // #194: a SUCCESSFUL Result<int> of 0 under a CountAsync-scoped key is the scalar sibling
                    // (count-shaped "not found") and is skipped for the same reason — a cached Success(0) makes
                    // existence probes answer "does not exist" for the whole TTL.
                    if (IsFailedResult(value) || IsEmptyCollectionSuccessResult(value) || IsZeroCountSuccessResult(key, value))
                    {
                        ctx.Options.SetDurationZero();
                        ctx.Options.SetSkipMemoryCacheWrite(true);
                        ctx.Options.SetSkipDistributedCacheWrite(true, true);
                    }

                    return value;
                },
                options => options
                    .SetDuration(expiration ?? _defaultExpiration)
                    .SetFailSafe(true),
                //.SetFactorySoftTimeout(TimeSpan.FromMilliseconds(100))
                //  .SetFactoryHardTimeout(TimeSpan.FromSeconds(2)),
                tags: typeTag is null ? null : [typeTag],
                token: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrSetAsync for key {Key}", key);
            // With fail-safe enabled, this should rarely happen
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogError("Cache key cannot be null or empty");
            return false;
        }

        try
        {
            await _fusionCache.RemoveAsync(key, token: cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key {Key}", key);
            return false;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// #116: implemented via FusionCache tag invalidation (available since FusionCache 2.0; this repo ships
    /// 2.6.0). The <paramref name="pattern"/> is the ENTITY TYPE NAME (e.g. <c>nameof(Machine)</c>): every
    /// entry stored through <see cref="GetOrSetAsync{T}"/>/<see cref="SetAsync{T}"/> whose key carries a
    /// <c>|Type:{EntityType}|</c> segment (all repository keys do — see
    /// <see cref="CacheKeyBuilderReadOnlyRepos"/>) is tagged <c>Type:{EntityType}</c>, and this call issues
    /// <c>RemoveByTagAsync</c> for that tag. Wildcard/regex key patterns remain unsupported — FusionCache has
    /// no pattern-based key removal; for a single exact key use <see cref="RemoveAsync"/>.
    /// FusionCache does not report a per-key removal count, so this returns 1 when the tag invalidation was
    /// issued successfully and 0 on invalid input or failure.
    /// </remarks>
    public async Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            _logger.LogError("Pattern cannot be null or empty");
            return 0;
        }

        try
        {
            var tag = CacheKeyBuilderReadOnlyRepos.BuildTypeTag(pattern);
            await _fusionCache.RemoveByTagAsync(tag, token: cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Invalidated cache entries tagged {Tag}", tag);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries for entity type {Pattern}", pattern);
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogError("Cache key cannot be null or empty");
            return;
        }

        if (value is null)
        {
            _logger.LogWarning("Attempting to cache null value for key {Key}", key);
            return;
        }

        try
        {
            // #116: same type-tagging as GetOrSetAsync so directly-set entries are also invalidated on writes.
            var typeTag = CacheKeyBuilderReadOnlyRepos.TryGetTypeTag(key);

            await _fusionCache.SetAsync(
                key,
                value,
                options => options.SetDuration(expiration ?? _defaultExpiration),
                tags: typeTag is null ? null : [typeTag],
                token: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value for key {Key}", key);
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogError("Cache key cannot be null or empty");
            return null;
        }

        try
        {
            var result = await _fusionCache.TryGetAsync<T>(key, token: cancellationToken).ConfigureAwait(false);
            return result.HasValue ? result.Value : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value for key {Key}", key);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogError("Cache key cannot be null or empty");
            return false;
        }

        try
        {
            var result = await _fusionCache.TryGetAsync<object>(key, token: cancellationToken).ConfigureAwait(false);
            return result.HasValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence for key {Key}", key);
            return false;
        }
    }

    // #61: cached values on the repository path are always IndQuestResults Result<...> instances. A failed
    // Result must never be persisted, so this reflects the closed Result<T> shape and reads IsSuccess without
    // this cache having to know the concrete generic argument. Non-Result values are treated as cacheable.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, System.Reflection.PropertyInfo?> IsSuccessAccessors = new();

    private static bool IsFailedResult(object? value)
    {
        if (value is null)
        {
            return false;
        }

        // #116: the non-generic IndQuestResults.Result (sealed, no inheritance relation to Result<T>) carries
        // the same failure semantics — a failed one must never be negative-cached either. It is matched by a
        // direct type check; the reflection path below only handles the open-generic Result<T> shape.
        if (value is Result nonGenericResult)
        {
            return nonGenericResult.IsFailure;
        }

        var type = value.GetType();
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
        {
            return false;
        }

        var accessor = IsSuccessAccessors.GetOrAdd(type, static t => t.GetProperty(nameof(Result<object>.IsSuccess)));
        if (accessor is null)
        {
            return false;
        }

        return accessor.GetValue(value) is bool isSuccess && !isSuccess;
    }

    // #187: cached Value accessors per closed Result<T> type, mirroring IsSuccessAccessors above.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, System.Reflection.PropertyInfo?> ValueAccessors = new();

    /// <summary>
    /// #187: detects a SUCCESSFUL <c>Result&lt;T&gt;</c> whose Value is an empty materialized collection —
    /// the collection-shaped "not found" (e.g. <c>ListAsync</c> on an empty table). Such a value must not be
    /// negative-cached: an empty list stored for the whole TTL hides rows seeded out-of-band until a service
    /// restart. Only the non-generic <see cref="System.Collections.ICollection"/> <c>Count</c> is inspected —
    /// lazy <see cref="System.Collections.IEnumerable"/> sequences are never enumerated (no materialization),
    /// and strings are never treated as collections. Failed Results are handled by
    /// <see cref="IsFailedResult"/>; non-Result and non-collection values are cacheable as before.
    /// </summary>
    /// <param name="value">The factory-produced value about to be written to the cache.</param>
    /// <returns><c>true</c> when the value is a successful <c>Result&lt;T&gt;</c> holding an empty collection.</returns>
    private static bool IsEmptyCollectionSuccessResult(object? value)
    {
        if (value is null)
        {
            return false;
        }

        var type = value.GetType();
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
        {
            return false;
        }

        var isSuccessAccessor = IsSuccessAccessors.GetOrAdd(type, static t => t.GetProperty(nameof(Result<object>.IsSuccess)));
        if (isSuccessAccessor is null || isSuccessAccessor.GetValue(value) is not bool isSuccess || !isSuccess)
        {
            return false;
        }

        var valueAccessor = ValueAccessors.GetOrAdd(type, static t => t.GetProperty(nameof(Result<object>.Value)));
        if (valueAccessor is null)
        {
            return false;
        }

        var innerValue = valueAccessor.GetValue(value);
        return innerValue is not string && innerValue is System.Collections.ICollection { Count: 0 };
    }

    /// <summary>
    /// #194: detects a SUCCESSFUL <c>Result&lt;int&gt;</c> whose Value is 0 under a CountAsync-scoped cache
    /// key — the count-shaped "not found", scalar sibling of the #187 empty-collection case. Such a value
    /// must not be negative-cached: existence probes built on <c>CountAsync</c> would answer "does not
    /// exist" for the whole TTL after rows are seeded out-of-band. Unlike the reflection-based predicates
    /// above (which must handle the open-generic <c>Result&lt;T&gt;</c>), this is a closed-type pattern
    /// match. The check is scoped by key via
    /// <see cref="CacheKeyBuilderReadOnlyRepos.IsOperationScoped"/> so a legitimate 0 under any other
    /// operation's key stays cached; non-zero counts and failed Results are untouched
    /// (failures are handled by <see cref="IsFailedResult"/>).
    /// </summary>
    /// <param name="key">The cache key the value is about to be written under.</param>
    /// <param name="value">The factory-produced value about to be written to the cache.</param>
    /// <returns><c>true</c> when the value is a successful zero <c>Result&lt;int&gt;</c> under a
    /// CountAsync-scoped key.</returns>
    private static bool IsZeroCountSuccessResult(string key, object? value)
        => value is Result<int> { IsSuccess: true, Value: 0 }
            && CacheKeyBuilderReadOnlyRepos.IsOperationScoped(key, "CountAsync");
}
