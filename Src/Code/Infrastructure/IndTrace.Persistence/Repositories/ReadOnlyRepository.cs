// <copyright file="ReadOnlyRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Data.Common;
using IndTrace.Domain.Diagnostics;
using IndTrace.Domain.Models;
using IndTrace.Persistence.Caching;
using IndTrace.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IndTrace.Persistence.Repositories;

/// <summary>
/// Provides a read-only repository for querying entities with caching and specification support.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
/// <summary>
/// Read-only repository implementation for entity operations with caching.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ReadOnlyRepository{T}"/> class.
/// </remarks>
/// <param name="contextFactory">The database context factory.</param>
/// <param name="cache">The cache.</param>
/// <param name="logger">The logger.</param>
public class ReadOnlyRepository<T>(
    IIndTraceDbContextFactory contextFactory,
    ICacheService cache,
    ILogger<ReadOnlyRepository<T>> logger,
    [ServiceKey] string serviceKey = "",
    IOptions<CacheToggleOptions>? toggleOptions = null
) : IReadOnlyRepository<T> where T : class, IndTrace.Domain.Interfaces.IPersistable
{
    private readonly IIndTraceDbContextFactory contextFactory = contextFactory;
    private readonly ILogger<ReadOnlyRepository<T>> logger = logger;
    private readonly ICacheService cache = cache;
    private readonly bool _cacheEnabled = true;
    private readonly string _key = serviceKey;
    private readonly IOptions<CacheToggleOptions>? _toggleOptions = toggleOptions;

    // #211: mutable aggregate types (roots + write-enforced members) are NEVER cacheable — the FusionCache
    // memory level returns entries by reference, aliasing one shared entity instance across executions.
    private static readonly bool TypeIsCacheable = CacheableTypePolicy.IsCacheable(typeof(T));

    private bool EffectiveCacheEnabled => TypeIsCacheable && _cacheEnabled && (_toggleOptions?.Value.Enabled != false);

    /// <summary>
    /// Gets an entity by its ID, using cache if available.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing the entity or an error.</returns>
    public async Task<Result<T?>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<T?>.WithFailure("Operation was canceled.");
        try
        {
            if (!EffectiveCacheEnabled)
            {
                logger.LogDebug("ReadOnlyRepository: Cache disabled, retrieving from repository");

                return await this.GetByIdFromDbAsync(id, cancellationToken).ConfigureAwait(false);
            }

            var cacheKey = CacheKeyBuilderReadOnlyRepos.BuildKey<T>("GetById", id);
            logger.LogDebug("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey}", typeof(T).Name, nameof(GetByIdAsync), cacheKey);

            var result = await this.cache.GetOrSetAsync<Result<T?>>(
                cacheKey,
                async ct => await this.GetByIdFromDbAsync(id, ct).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            var finalResult = result ?? Result<T?>.WithFailure("Failed to retrieve data from cache");

            if (finalResult.IsFailure)
            {
                logger.LogError("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey} Error={Error}", typeof(T).Name, nameof(GetByIdAsync), cacheKey, result?.Error);
            }
            return finalResult;
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<T?>(ex, nameof(this.GetByIdAsync));
        }
    }

    /// <summary>
    /// Gets an entity by its ID directly from the database.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing the entity or an error.</returns>
    private async Task<Result<T?>> GetByIdFromDbAsync(int id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<T?>.WithFailure("Operation was canceled.");
        try
        {
            await using IIndTraceDbContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            // Story 35.D1: resolve the raw int id to the key's MODEL type (identity for plain-int keys; via the value
            // converter for a strongly-typed key such as BarCode.BarCodeId) so FindAsync matches.
            var keyValue = RepositoryKeyResolver.ResolveKeyValue(context.Model, typeof(T), id);
            T? entity = await context.Set<T>().FindAsync(new object[] { keyValue }, cancellationToken).ConfigureAwait(false);

            if (entity is not null)
            {
                context.Entry(entity).State = EntityState.Detached; // Ensure no tracking
            }

            return entity is not null
                ? Result<T?>.Success(entity)
                : Result<T?>.WithFailure($"Entity of type {typeof(T).Name} with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<T?>(ex, nameof(this.GetByIdFromDbAsync));
        }
    }

    /// <summary>
    /// Lists entities matching a specification, using cache if available.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{IEnumerable{T}}"/> containing the entities or an error.</returns>
    public async Task<Result<IEnumerable<T>>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken)
    {
        if (spec is null)
            return Result<IEnumerable<T>>.WithFailure("spec cannot be null.");
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<T>>.WithFailure("Operation was canceled.");
        }
        this.logger.LogDebug("ReadOnlyRepository: {Operation}<{Entity}> CacheEnabled={CacheEnabled} ServiceKey={ServiceKey}", nameof(ListAsync), typeof(T).Name, EffectiveCacheEnabled, _key);
        try
        {
            if (!EffectiveCacheEnabled)
            {
                logger.LogDebug("ReadOnlyRepository: Cache disabled, retrieving from repository");

                return await this.ListFromDbAsync(spec, cancellationToken).ConfigureAwait(false);
            }

            var cacheKey = CacheKeyBuilderReadOnlyRepos.BuildKey<T>("ListAsync", spec);
            logger.LogDebug("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey}", typeof(T).Name, nameof(ListAsync), cacheKey);
            var result = await this.cache.GetOrSetAsync<Result<IEnumerable<T>>>(
                cacheKey,
                async ct => await this.ListFromDbAsync(spec, ct).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            var finalResult = result ?? Result<IEnumerable<T>>.WithFailure("Failed to retrieve data from cache");
            if (finalResult.IsFailure)
                logger.LogError("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey} Error={Error}", typeof(T).Name, nameof(ListAsync), cacheKey, result?.Error);
            return finalResult;
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<IEnumerable<T>>(ex, nameof(this.ListAsync));
        }
    }

    /// <summary>
    /// Lists entities matching a specification directly from the database.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{IEnumerable{T}}"/> containing the entities or an error.</returns>
    private async Task<Result<IEnumerable<T>>> ListFromDbAsync(ISpecification<T> spec, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<IEnumerable<T>>.WithFailure("Operation was canceled.");
        try
        {
            await using IIndTraceDbContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            IQueryable<T> query = this.ApplySpecification(spec, context);
            List<T> result = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return Result<IEnumerable<T>>.Success(result);
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<IEnumerable<T>>(ex, nameof(this.ListFromDbAsync));
        }
    }

    /// <summary>
    /// Gets the first entity matching a specification, using cache if available.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing the entity or an error.</returns>
    public async Task<Result<T?>> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken)
    {
        if (spec is null)
        {
            return Result<T?>.WithFailure("spec cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<T?>.WithFailure("Operation was canceled.");
        }
        this.logger.LogDebug("ReadOnlyRepository: {Operation}<{Entity}> CacheEnabled={CacheEnabled} ServiceKey={ServiceKey}", nameof(FirstOrDefaultAsync), typeof(T).Name, EffectiveCacheEnabled, _key);
        try
        {
            if (!EffectiveCacheEnabled)
            {
                logger.LogDebug("ReadOnlyRepository: Cache disabled, retrieving from repository");

                return await this.FirstOrDefaultFromDbAsync(spec, cancellationToken).ConfigureAwait(false);
            }
            var cacheKey = CacheKeyBuilderReadOnlyRepos.BuildKey<T>("FirstOrDefault", spec);

            logger.LogDebug("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey}", typeof(T).Name, nameof(FirstOrDefaultAsync), cacheKey);
            var result = await this.cache.GetOrSetAsync<Result<T?>>(
                cacheKey,
                async ct => await this.FirstOrDefaultFromDbAsync(spec, ct).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            var finalResult = result ?? Result<T?>.WithFailure("Failed to retrieve data from cache");
            if (finalResult.IsFailure)
            {
                logger.LogError("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey} Error={Error}", typeof(T).Name, nameof(FirstOrDefaultAsync), cacheKey, result?.Error);
            }
            return finalResult;
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<T?>(ex, nameof(this.FirstOrDefaultAsync));
        }
    }

    /// <summary>
    /// Gets the first entity matching a specification directly from the database.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing the entity or an error.</returns>
    private async Task<Result<T?>> FirstOrDefaultFromDbAsync(ISpecification<T> spec, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<T?>.WithFailure("Operation was canceled.");
        await using IIndTraceDbContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        T? entity = await this.ApplySpecification(spec, context)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return entity is not null
            ? Result<T?>.Success(entity)
            : Result<T?>.WithFailure(RepositoryFailures.NotFoundSentinel + ".");
    }

    /// <summary>
    /// Lists all entities, using cache if available.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{IEnumerable{T}}"/> containing the entities or an error.</returns>
    public async Task<Result<IEnumerable<T>>> ListAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<T>>.WithFailure("Operation was canceled.");
        }
        this.logger.LogDebug("ReadOnlyRepository: {Operation}<{Entity}> CacheEnabled={CacheEnabled} ServiceKey={ServiceKey}", nameof(ListAsync), typeof(T).Name, EffectiveCacheEnabled, _key);
        try
        {
            if (!EffectiveCacheEnabled)
            {
                logger.LogDebug("ReadOnlyRepository: Cache disabled, retrieving from repository");

                return await this.ListAsyncFromDbAsync(cancellationToken).ConfigureAwait(false);
            }

            var cacheKey = CacheKeyBuilderReadOnlyRepos.BuildKey<T>("ListAsync");

            logger.LogDebug("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey}", typeof(T).Name, nameof(ListAsync), cacheKey);

            var result = await this.cache.GetOrSetAsync<Result<IEnumerable<T>>>(
                cacheKey,
                async ct => await this.ListAsyncFromDbAsync(ct).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            var finalResult = result ?? Result<IEnumerable<T>>.WithFailure("Failed to retrieve data from cache");
            if (finalResult.IsFailure)
            {
                logger.LogError("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey} Error={Error}", typeof(T).Name, nameof(ListAsync), cacheKey, result?.Error);
            }
            return finalResult;
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<IEnumerable<T>>(ex, nameof(this.ListAsync));
        }
    }

    /// <summary>
    /// Lists all entities directly from the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{IEnumerable{T}}"/> containing the entities or an error.</returns>
    private async Task<Result<IEnumerable<T>>> ListAsyncFromDbAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<IEnumerable<T>>.WithFailure("Operation was canceled.");
        try
        {
            await using IIndTraceDbContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            List<T> entities = await context.Set<T>().AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
            return Result<IEnumerable<T>>.Success(entities);
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<IEnumerable<T>>(ex, nameof(this.ListAsyncFromDbAsync));
        }
    }

    /// <summary>
    /// Gets the first entity, using cache if available.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing the entity or an error.</returns>
    public async Task<Result<T?>> FirstOrDefaultAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<T?>.WithFailure("Operation was canceled.");
        this.logger.LogDebug("ReadOnlyRepository: {Operation}<{Entity}> CacheEnabled={CacheEnabled} ServiceKey={ServiceKey}", nameof(FirstOrDefaultAsync), typeof(T).Name, EffectiveCacheEnabled, _key);
        try
        {
            if (!EffectiveCacheEnabled)
            {
                logger.LogDebug("ReadOnlyRepository: Cache disabled, retrieving from repository");

                return await this.FirstOrDefaultFromDbAsync(cancellationToken).ConfigureAwait(false);
            }

            var cacheKey = CacheKeyBuilderReadOnlyRepos.BuildKey<T>("FirstOrDefault");
            logger.LogDebug("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey}", typeof(T).Name, nameof(FirstOrDefaultAsync), cacheKey);

            var result = await this.cache.GetOrSetAsync<Result<T?>>(
                cacheKey,
                async ct => await this.FirstOrDefaultFromDbAsync(ct).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            var finalResult = result ?? Result<T?>.WithFailure("Failed to retrieve data from cache");
            if (finalResult.IsFailure)
                logger.LogError("ReadOnlyRepository: Entity={Entity} Operation={Operation} CacheKey={CacheKey} Error={Error}", typeof(T).Name, nameof(FirstOrDefaultAsync), cacheKey, result?.Error);
            return finalResult;
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<T?>(ex, nameof(this.FirstOrDefaultAsync));
        }
    }

    /// <summary>
    /// Gets the first entity directly from the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing the entity or an error.</returns>
    private async Task<Result<T?>> FirstOrDefaultFromDbAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<T?>.WithFailure("Operation was canceled.");
        }
        try
        {
            await using IIndTraceDbContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            T? entity = await context.Set<T>().AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return entity is not null
                ? Result<T?>.Success(entity)
                : Result<T?>.WithFailure("No entity found.");
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<T?>(ex, nameof(this.FirstOrDefaultAsync));
        }
    }

    /// <summary>
    /// Counts entities matching a specification, using cache if available.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{int}"/> containing the count or an error.</returns>
    public async Task<Result<int>> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        if (spec is null)
        {
            return Result<int>.WithFailure("spec cannot be null.");
        }
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<int>.WithFailure("Operation was canceled.");
        }
        if (this.logger.IsEnabled(LogLevel.Debug))
        {
            this.logger.LogDebug("ReadOnlyRepository :: {Operation}<{Entity}> CacheEnabled={CacheEnabled} ServiceKey={ServiceKey}", nameof(CountAsync), typeof(T).Name, EffectiveCacheEnabled, _key);
        }
        try
        {
            if (!EffectiveCacheEnabled)
            {
                if (this.logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Repository Readonly :: Cache disabele retrieving from the repository");
                }

                return await this.CountAsyncFromDbAsync(spec, cancellationToken).ConfigureAwait(false);
            }
            var cacheKey = CacheKeyBuilderReadOnlyRepos.BuildKey<T>("CountAsync", spec);
            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Repository Readonly :: for entity {entity} operation {operation} Using Cache Key: {CacheKey}", typeof(T).Name, nameof(CountAsync), cacheKey);
            }

            var result = await this.cache.GetOrSetAsync<Result<int>>(
                cacheKey,
                async ct => await this.CountAsyncFromDbAsync(spec, ct).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            var finalResult = result ?? Result<int>.WithFailure("Failed to retrieve data from cache");
            if (finalResult.IsFailure && this.logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogError("Repository Readonly :: for entity {entity} operation {operation} Using Cache Key: {CacheKey} resulted on error {error}", typeof(T).Name, nameof(CountAsync), cacheKey, result?.Error);
            }
            return finalResult;
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<int>(ex, nameof(this.CountAsync));
        }
    }

    /// <summary>
    /// Gets an entity by its composite identifiers, using cache if available.
    /// </summary>
    public async Task<Result<T?>> GetByIdsAsync(CancellationToken cancellationToken, params object[] ids)
    {
        if (ids is null || ids.Length == 0)
        {
            return Result<T?>.WithFailure("ids cannot be null or empty.");
        }
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<T?>.WithFailure("Operation was canceled.");
        }
        if (this.logger.IsEnabled(LogLevel.Debug))
        {
            this.logger.LogDebug("ReadOnlyRepository :: {Operation}<{Entity}> CacheEnabled={CacheEnabled} ServiceKey={ServiceKey}", nameof(GetByIdsAsync), typeof(T).Name, EffectiveCacheEnabled, _key);
        }
        try
        {
            if (!EffectiveCacheEnabled)
            {
                if (this.logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Repository Readonly :: Cache disabled retrieving from the repository");
                }

                return await this.GetByIdsFromDbAsync(ids, cancellationToken).ConfigureAwait(false);
            }

            var cacheKey = CacheKeyBuilderReadOnlyRepos.BuildKey<T>("GetByIds", ids);
            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Repository Readonly :: for entity {entity } operation {operation} Using Cache Key: {CacheKey}", typeof(T).Name, nameof(GetByIdsAsync), cacheKey);
            }

            var result = await this.cache.GetOrSetAsync<Result<T?>>(cacheKey,
                async ct => await this.GetByIdsFromDbAsync(ids, ct).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var finalResult = result ?? Result<T?>.WithFailure("Failed to retrieve data from cache");
            if (finalResult.IsFailure && this.logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogError("Repository Readonly :: for entity {entity } operation {operation} Using Cache Key: {CacheKey} resulted on error {error}", typeof(T).Name, nameof(GetByIdsAsync), cacheKey, result?.Error);
            }
            return finalResult;
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<T?>(ex, nameof(this.GetByIdsAsync));
        }
    }

    private async Task<Result<T?>> GetByIdsFromDbAsync(object[] ids, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<T?>.WithFailure("Operation was canceled.");
        }
        try
        {
            await using IIndTraceDbContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var entity = await context.Set<T>().FindAsync(ids, cancellationToken).ConfigureAwait(false);
            if (entity is not null)
            {
                context.Entry(entity).State = EntityState.Detached;
            }

            return entity is not null ? Result<T?>.Success(entity) : Result<T?>.WithFailure("Entity not found.");
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<T?>(ex, nameof(this.GetByIdsFromDbAsync));
        }
    }

    /// <summary>
    /// Counts entities matching a specification directly from the database.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{int}"/> containing the count or an error.</returns>
    private async Task<Result<int>> CountAsyncFromDbAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<int>.WithFailure("Operation was canceled.");
        }
        try
        {
            await using IIndTraceDbContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            int count = await this.ApplySpecification(spec, context).CountAsync(cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            return this.ClassifyAndLogReadFailure<int>(ex, nameof(this.CountAsyncFromDbAsync));
        }
    }

    /// <summary>
    /// Leases a read-only <see cref="OwnedQueryable{T}"/> for the entity type.
    /// The query is always no-tracking, consistent with read-only repository policy. #117 (F1): the lease OWNS
    /// the pooled context backing the queryable — disposing the lease (the caller's <c>await using</c>) is the
    /// only way that context returns to the pool; a failure result never carries (or leaks) a lease.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing the owned queryable lease or an error.</returns>
    public async Task<Result<OwnedQueryable<T>>> AsQueryableAsync(CancellationToken cancellationToken)
    {
        const string methodName = nameof(this.AsQueryableAsync);
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<OwnedQueryable<T>>.WithFailure("Operation was canceled.");
        }

        IIndTraceDbContext? context = null;
        try
        {
            context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            return Result<OwnedQueryable<T>>.Success(new OwnedQueryable<T>(context.Set<T>().AsNoTracking(), context));
        }
        catch (Exception ex)
        {
            // A failure Result must never leak the leased context.
            if (context is not null)
            {
                await context.DisposeAsync().ConfigureAwait(false);
            }

            return this.ClassifyAndLogReadFailure<OwnedQueryable<T>>(ex, methodName);
        }
    }

    /// <summary>
    /// Leases a read-only <see cref="OwnedQueryable{T}"/> for the entity type, filtered by the specified specification.
    /// The query is always no-tracking, consistent with read-only repository policy. #117 (F1): the lease OWNS
    /// the pooled context backing the queryable; see <see cref="AsQueryableAsync(CancellationToken)"/>.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing the owned queryable lease or an error.</returns>
    public async Task<Result<OwnedQueryable<T>>> AsQueryableAsync(ISpecification<T> spec, CancellationToken cancellationToken)
    {
        const string methodName = nameof(this.AsQueryableAsync);
        if (spec is null)
        {
            return Result<OwnedQueryable<T>>.WithFailure("spec cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<OwnedQueryable<T>>.WithFailure("Operation was canceled.");
        }

        IIndTraceDbContext? context = null;
        try
        {
            context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            return Result<OwnedQueryable<T>>.Success(new OwnedQueryable<T>(this.ApplySpecification(spec, context), context));
        }
        catch (Exception ex)
        {
            // A failure Result must never leak the leased context.
            if (context is not null)
            {
                await context.DisposeAsync().ConfigureAwait(false);
            }

            return this.ClassifyAndLogReadFailure<OwnedQueryable<T>>(ex, methodName);
        }
    }

    /// <summary>
    /// Returns a composable <see cref="IQueryable{T}"/> rooted at a parameterized raw-SQL query. #229 (Slice B):
    /// the read-side seam for query shapes an EF value converter cannot translate (e.g. a substring <c>LIKE</c>
    /// or a correlated <c>EXISTS</c> over another table). The interpolated <paramref name="sql"/> is threaded
    /// through <c>FromSql(FormattableString)</c>, which binds every interpolation hole as a SQL parameter (no
    /// string concatenation / SQL injection), and the result composes with further LINQ. Results are ALWAYS
    /// no-tracking, consistent with read-only repository policy. #117 (F1): the lease OWNS the pooled context
    /// backing the queryable — disposing the lease (the caller's <c>await using</c>) is the only way that context
    /// returns to the pool; a failure result never carries (or leaks) a lease.
    /// </summary>
    /// <param name="sql">A parameterized (interpolated) SQL statement whose columns cover the entity's mapped properties.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> containing the raw-SQL-rooted owned queryable lease or an error.</returns>
    public async Task<Result<OwnedQueryable<T>>> FromSqlAsync(FormattableString sql, CancellationToken cancellationToken)
    {
        const string methodName = nameof(this.FromSqlAsync);
        if (sql is null)
        {
            return Result<OwnedQueryable<T>>.WithFailure("sql cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<OwnedQueryable<T>>.WithFailure("Operation was canceled.");
        }

        IIndTraceDbContext? context = null;
        try
        {
            context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            return Result<OwnedQueryable<T>>.Success(
                new OwnedQueryable<T>(context.Set<T>().FromSql(sql).AsNoTracking(), context));
        }
        catch (Exception ex)
        {
            // A failure Result must never leak the leased context.
            if (context is not null)
            {
                await context.DisposeAsync().ConfigureAwait(false);
            }

            return this.ClassifyAndLogReadFailure<OwnedQueryable<T>>(ex, methodName);
        }
    }

    /// <summary>
    /// Classifies a caught exception from a read path and produces the corresponding failure result.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A database / infrastructure fault — keyed on <see cref="DbException"/>, the base of
    /// <c>Microsoft.Data.SqlClient.SqlException</c> (for example a missing SQL column such as
    /// <c>CreatedBy</c> on a schema restored from an older backup) — is logged at <see cref="LogLevel.Error"/>
    /// with its real detail and returned as an <see cref="InfrastructureFault"/>-marked failure. That marker
    /// keeps the fault category DISTINCT so no consumer can mistake it for an ordinary "not found" / empty
    /// result. Any other (genuinely unexpected) exception preserves the previous behavior: logged and
    /// returned as a plain message failure.
    /// </para>
    /// </remarks>
    /// <typeparam name="TResult">The result value type of the calling read method.</typeparam>
    /// <param name="ex">The caught exception.</param>
    /// <param name="operationName">The name of the read operation, for diagnostics.</param>
    /// <returns>A failed <see cref="Result{TResult}"/> classified by fault category.</returns>
    private Result<TResult> ClassifyAndLogReadFailure<TResult>(Exception ex, string operationName)
    {
        if (ex is DbException dbException)
        {
            this.logger.LogError(
                dbException,
                "ReadOnlyRepository: INFRASTRUCTURE/DATABASE fault in {Operation} for entity {EntityType}. This is a schema/infrastructure fault, NOT a not-found result. Detail: {Detail}",
                operationName,
                typeof(T).Name,
                dbException.Message);

            return Result<TResult>.WithFailure(
                InfrastructureFault.Compose($"{operationName}<{typeof(T).Name}>: {dbException.Message}"));
        }

        this.logger.LogError(ex, "ReadOnlyRepository: Error in {Operation} for entity {EntityType}", operationName, typeof(T).Name);
        return Result<TResult>.WithFailure(ex.Message);
    }

    /// <summary>
    /// Applies a specification to the entity set, including filters, includes, ordering, and paging.
    /// Always returns a no-tracking query as per read-only policy.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <param name="context">The database context.</param>
    /// <returns>An <see cref="IQueryable{T}"/> filtered and shaped according to the specification.</returns>
    private IQueryable<T> ApplySpecification(ISpecification<T> spec, IIndTraceDbContext context)
    {
        IQueryable<T> query = context.Set<T>();

        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }

        if (spec.Includes is not null && spec.Includes.Count > 0)
        {
            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        }

        if (spec.IncludeStrings is not null && spec.IncludeStrings.Count > 0)
        {
            query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));
        }

        // Apply ordering and secondary ordering when present
        IOrderedQueryable<T>? ordered = null;
        if (spec.OrderBy is not null)
        {
            ordered = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending is not null)
        {
            ordered = query.OrderByDescending(spec.OrderByDescending);
        }

        if (ordered is not null)
        {
            if (spec.ThenBy is not null)
            {
                ordered = ordered.ThenBy(spec.ThenBy);
            }

            if (spec.ThenByDescending is not null)
            {
                ordered = ordered.ThenByDescending(spec.ThenByDescending);
            }

            query = ordered;
        }

        // F7 (#117): apply Skip and Take independently (matching Repository<T>) — a Take-only
        // specification must not silently materialize the whole table.
        if (spec.Skip.HasValue)
        {
            query = query.Skip(spec.Skip.Value);
        }

        if (spec.Take.HasValue)
        {
            query = query.Take(spec.Take.Value);
        }

        return query.AsNoTracking(); // Always enforce no-tracking
    }

}
