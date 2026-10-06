// <copyright file="CacheWriteInvalidation.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace IndTrace.Persistence.Caching;

/// <summary>
/// #116: shared best-effort write-side cache invalidation for repositories whose unit of work touches one or
/// more entity types (the aggregate repositories and the append-only repository decorator). Invalidates the
/// type tag of every touched entity type via <see cref="ICacheService.RemoveByPatternAsync"/> so cached reads
/// never outlive a committed write WITHIN THE SAME PROCESS. The guarantee is per-process: the FusionCache
/// instance is process-local (no distributed level/backplane), so a PLC-gateway write does not invalidate a
/// Monitor process's cached reads — those stay stale until TTL expiry (cross-process gap tracked on #129).
/// </summary>
/// <remarks>
/// Semantics match <see cref="CacheInvalidatingRepository{T}"/>: invalidation runs AFTER the write is durable,
/// with <see cref="CancellationToken.None"/> (caller-side cancellation must not strand stale reads), and a
/// failure is logged as a warning and swallowed — it must never turn a committed write into a failure. Each
/// type is invalidated independently so one failing type does not skip the rest.
/// </remarks>
internal static class CacheWriteInvalidation
{
    /// <summary>
    /// Invalidates the cached entries of every given entity type. A <see langword="null"/>
    /// <paramref name="cache"/> (composition root without an <see cref="ICacheService"/>) is a no-op.
    /// Never throws.
    /// </summary>
    /// <param name="cache">The cache service, or <see langword="null"/> when the host registers none.</param>
    /// <param name="logger">The calling repository's logger.</param>
    /// <param name="operation">The mutating operation that succeeded (for logging).</param>
    /// <param name="entityTypeNames">The entity type names the committed write touched.</param>
    internal static async Task InvalidateTypesAsync(
        ICacheService? cache,
        ILogger logger,
        string operation,
        params string[] entityTypeNames)
    {
        if (cache is null)
        {
            return;
        }

        foreach (var entityTypeName in entityTypeNames)
        {
            try
            {
                var removed = await cache.RemoveByPatternAsync(entityTypeName, CancellationToken.None).ConfigureAwait(false);
                logger.LogDebug(
                    "Cache invalidation after {Operation} completed for {EntityType} (result {Removed})",
                    operation,
                    entityTypeName,
                    removed);
            }
            catch (Exception ex)
            {
                // Data is committed; a failed invalidation must not turn a successful write into a failure.
                // Worst case the read cache stays stale until its TTL expires.
                logger.LogWarning(
                    ex,
                    "Cache invalidation after {Operation} FAILED for {EntityType} — cached reads may serve stale data until TTL expiry",
                    operation,
                    entityTypeName);
            }
        }
    }
}
