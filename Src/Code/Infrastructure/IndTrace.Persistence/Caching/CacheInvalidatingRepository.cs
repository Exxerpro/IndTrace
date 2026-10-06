// <copyright file="CacheInvalidatingRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace IndTrace.Persistence.Caching;

/// <summary>
/// #116: write-side cache-invalidation decorator over <see cref="IRepository{T}"/>.
/// The read cache (<see cref="FusionCacheService"/>, 1h default TTL) tags every repository entry with its
/// entity type; this decorator closes the write→read staleness gap by invalidating that type tag
/// (via <see cref="ICacheService.RemoveByPatternAsync"/>, whose pattern is the entity type name) after every
/// SUCCESSFUL mutating operation. Reads and queries pass straight through to the inner repository.
/// </summary>
/// <typeparam name="T">The entity type managed by the decorated repository.</typeparam>
/// <remarks>
/// Invalidation failure must never fail the write: by the time invalidation runs the data is already
/// committed, so a stale cache entry (bounded by the TTL) is the lesser evil versus reporting a failed
/// write for data that IS persisted. Failures are logged as warnings and the write's own result is returned.
/// <see cref="CommitAsync"/> deliberately does NOT invalidate: it is a documented honest no-op
/// (each mutating method saves inline), so invalidating there would flush the type cache on calls that
/// changed nothing.
/// </remarks>
public sealed class CacheInvalidatingRepository<T> : IRepository<T> where T : class, IndTrace.Domain.Interfaces.IPersistable
{
    private readonly IRepository<T> inner;
    private readonly ICacheService cache;
    private readonly ILogger<CacheInvalidatingRepository<T>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInvalidatingRepository{T}"/> class.
    /// </summary>
    /// <param name="inner">The repository being decorated.</param>
    /// <param name="cache">The cache service whose type-tagged entries are invalidated on writes.</param>
    /// <param name="logger">The logger.</param>
    public CacheInvalidatingRepository(IRepository<T> inner, ICacheService cache, ILogger<CacheInvalidatingRepository<T>> logger)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<Result<T?>> GetByIdAsync(int id, CancellationToken cancellationToken)
        => this.inner.GetByIdAsync(id, cancellationToken);

    /// <inheritdoc/>
    public Task<Result<T?>> GetByIdsAsync(CancellationToken cancellationToken, params object[] ids)
        => this.inner.GetByIdsAsync(cancellationToken, ids);

    /// <inheritdoc/>
    public Task<Result<IEnumerable<T>>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        => this.inner.ListAsync(spec, cancellationToken);

    /// <inheritdoc/>
    public Task<Result<IEnumerable<T>>> ListAsync(CancellationToken cancellationToken)
        => this.inner.ListAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<Result<T?>> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        => this.inner.FirstOrDefaultAsync(spec, cancellationToken);

    /// <inheritdoc/>
    public Task<Result<T?>> FirstOrDefaultAsync(CancellationToken cancellationToken)
        => this.inner.FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<Result<int>> AddAsync(T entity, CancellationToken cancellationToken)
    {
        var result = await this.inner.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await this.InvalidateTypeEntriesAsync(nameof(this.AddAsync)).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<int>> AddRangeBulkAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        var result = await this.inner.AddRangeBulkAsync(entities, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await this.InvalidateTypeEntriesAsync(nameof(this.AddRangeBulkAsync)).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<int>> AddAsync(T entity, int id, string tableName, CancellationToken cancellationToken)
    {
        var result = await this.inner.AddAsync(entity, id, tableName, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await this.InvalidateTypeEntriesAsync(nameof(this.AddAsync)).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<int>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        var result = await this.inner.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await this.InvalidateTypeEntriesAsync(nameof(this.AddRangeAsync)).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result> UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        var result = await this.inner.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await this.InvalidateTypeEntriesAsync(nameof(this.UpdateAsync)).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        var result = await this.inner.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await this.InvalidateTypeEntriesAsync(nameof(this.DeleteAsync)).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Pure passthrough, NO invalidation: the inner repository's <c>CommitAsync</c> is a documented honest
    /// no-op (every mutating method saves inline and already invalidated here), so it returns success on
    /// every call — invalidating on it would flush the type cache even when nothing changed.
    /// </remarks>
    public Task<Result> CommitAsync(CancellationToken cancellationToken)
        => this.inner.CommitAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<Result<int>> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        => this.inner.CountAsync(spec, cancellationToken);

    /// <inheritdoc/>
    public Task<Result<OwnedQueryable<T>>> AsQueryableAsync(CancellationToken cancellationToken)
        => this.inner.AsQueryableAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<Result<OwnedQueryable<T>>> AsQueryableAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        => this.inner.AsQueryableAsync(spec, cancellationToken);

    /// <inheritdoc/>
    public Task<Result<OwnedQueryable<T>>> FromSqlAsync(FormattableString sql, CancellationToken cancellationToken)
        => this.inner.FromSqlAsync(sql, cancellationToken);

    /// <inheritdoc/>
    public Task<Result> DetachAsync(T entity, CancellationToken cancellationToken)
        => this.inner.DetachAsync(entity, cancellationToken);

    /// <inheritdoc/>
    public Task<Result> ApplyNoTrackingAsync(CancellationToken cancellationToken)
        => this.inner.ApplyNoTrackingAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<Result> ApplyTrackingAsync(CancellationToken cancellationToken)
        => this.inner.ApplyTrackingAsync(cancellationToken);

    /// <summary>
    /// Best-effort invalidation of every cache entry tagged with this entity type. Runs with
    /// <see cref="CancellationToken.None"/> on purpose: the write is already durable when this executes,
    /// so a caller-side cancellation must not be able to skip the invalidation and strand stale reads.
    /// Never throws — failures are logged and swallowed (see class remarks).
    /// </summary>
    /// <param name="operation">The mutating operation that succeeded (for logging).</param>
    private async Task InvalidateTypeEntriesAsync(string operation)
    {
        try
        {
            var removed = await this.cache.RemoveByPatternAsync(typeof(T).Name, CancellationToken.None).ConfigureAwait(false);
            this.logger.LogDebug(
                "Cache invalidation after {Operation}<{EntityType}> completed (result {Removed})",
                operation,
                typeof(T).Name,
                removed);
        }
        catch (Exception ex)
        {
            // Data is committed; a failed invalidation must not turn a successful write into a failure.
            // Worst case the read cache stays stale until its TTL expires.
            this.logger.LogWarning(
                ex,
                "Cache invalidation after {Operation}<{EntityType}> FAILED — cached reads may serve stale data until TTL expiry",
                operation,
                typeof(T).Name);
        }
    }
}
