// <copyright file="CacheInvalidatingAppendOnlyRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace IndTrace.Persistence.Caching;

/// <summary>
/// #116: write-side cache-invalidation decorator over <see cref="IAppendOnlyRepository{T}"/> — the
/// append-only sibling of <see cref="CacheInvalidatingRepository{T}"/>. Every SUCCESSFUL append
/// (<see cref="AddAsync"/>/<see cref="AddRangeAsync"/>/<see cref="AddRangeBulkAsync"/>) invalidates the
/// entity type's tagged read-cache entries so cached lists (e.g. Register audit reads) never outlive an
/// append for the TTL — a PER-PROCESS guarantee only: the FusionCache behind it is process-local (no
/// distributed level/backplane), so appends in one process do not invalidate another process's cached
/// reads (cross-process gap tracked on issue #129).
/// </summary>
/// <typeparam name="T">The entity type managed by the decorated repository.</typeparam>
/// <remarks>
/// Invalidation failure must never fail the append: by the time invalidation runs the data is already
/// committed, so a stale cache entry (bounded by the TTL) is the lesser evil versus reporting a failed
/// write for data that IS persisted. Failures are logged as warnings and the append's own result is returned.
/// </remarks>
public sealed class CacheInvalidatingAppendOnlyRepository<T> : IAppendOnlyRepository<T> where T : class, IndTrace.Domain.Interfaces.IPersistable
{
    private readonly IAppendOnlyRepository<T> inner;
    private readonly ICacheService cache;
    private readonly ILogger<CacheInvalidatingAppendOnlyRepository<T>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInvalidatingAppendOnlyRepository{T}"/> class.
    /// </summary>
    /// <param name="inner">The append-only repository being decorated.</param>
    /// <param name="cache">The cache service whose type-tagged entries are invalidated on appends.</param>
    /// <param name="logger">The logger.</param>
    public CacheInvalidatingAppendOnlyRepository(
        IAppendOnlyRepository<T> inner,
        ICacheService cache,
        ILogger<CacheInvalidatingAppendOnlyRepository<T>> logger)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<int>> AddAsync(T entity, CancellationToken cancellationToken)
    {
        var result = await this.inner.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await CacheWriteInvalidation.InvalidateTypesAsync(
                this.cache, this.logger, nameof(this.AddAsync), typeof(T).Name).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<int>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        var result = await this.inner.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await CacheWriteInvalidation.InvalidateTypesAsync(
                this.cache, this.logger, nameof(this.AddRangeAsync), typeof(T).Name).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<int>> AddRangeBulkAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        var result = await this.inner.AddRangeBulkAsync(entities, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await CacheWriteInvalidation.InvalidateTypesAsync(
                this.cache, this.logger, nameof(this.AddRangeBulkAsync), typeof(T).Name).ConfigureAwait(false);
        }

        return result;
    }
}
