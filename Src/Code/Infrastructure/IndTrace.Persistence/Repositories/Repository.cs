// <copyright file="Repository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Models;
using IndTrace.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IndTrace.Persistence.Repositories
{
    /// <summary>
    /// Generic repository implementation providing data access operations for entities.
    /// Implements the Repository pattern with specification pattern for flexible querying.
    /// </summary>
    /// <typeparam name="T">The entity type that this repository manages.</typeparam>
    /// <summary>
    /// Generic repository implementation for entity operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class Repository<T> : IRepository<T>, IReadOnlyRepository<T>, IAppendOnlyRepository<T> where T : class, IndTrace.Domain.Interfaces.IPersistable
    {
        private const string DatabaseContextIsNotActive = "Database context is not active.";
        private readonly IIndTraceDbContextFactory contextFactory;
        private readonly ILogger<Repository<T>> logger;
        private readonly string key = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="contextFactory">The database context factory.</param>
        /// <param name="logger">The logger.</param>
        public Repository(IIndTraceDbContextFactory contextFactory, ILogger<Repository<T>> logger, [ServiceKey] string key = "")
        {
            this.contextFactory = contextFactory;
            this.logger = logger;
            this.key = key;
        }

        private bool InvalidContext(IIndTraceDbContext? context, string methodName)
        {
            if (context is null)
            {
                this.logger.LogError("Context is null in {MethodName} for entity type {EntityType}.", methodName, typeof(T).Name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Applies the given specification to the DbSet, returning a filtered IQueryable.
        /// </summary>
        /// <param name="specification">The specification to apply.</param>
        /// <param name="context">The database context.</param>
        /// <returns>An IQueryable filtered and shaped according to the specification.</returns>
        private IQueryable<T> ApplySpecification(ISpecification<T> specification, IIndTraceDbContext context)
        {
            var query = context.Set<T>().AsQueryable();

            // Apply criteria
            if (specification.Criteria is not null)
                query = query.Where(specification.Criteria);

            // Apply includes (expression-based)
            if (specification.Includes is not null)
            {
                foreach (var include in specification.Includes)
                {
                    query = query.Include(include);
                }
            }

            // Apply includes (string-based)
            if (specification.IncludeStrings is not null)
            {
                foreach (var includeString in specification.IncludeStrings)
                {
                    query = query.Include(includeString);
                }
            }

            // Apply ordering (primary + secondary)
            IOrderedQueryable<T>? ordered = null;
            if (specification.OrderBy is not null)
            {
                ordered = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending is not null)
            {
                ordered = query.OrderByDescending(specification.OrderByDescending);
            }

            if (ordered is not null)
            {
                if (specification.ThenBy is not null)
                {
                    ordered = ordered.ThenBy(specification.ThenBy);
                }

                if (specification.ThenByDescending is not null)
                {
                    ordered = ordered.ThenByDescending(specification.ThenByDescending);
                }

                query = ordered;
            }

            // Apply paging
            if (specification.Skip.HasValue)
                query = query.Skip(specification.Skip.Value);
            if (specification.Take.HasValue)
                query = query.Take(specification.Take.Value);

            // Apply tracking
            if (!specification.IsTracking)
            {
                query = query.AsNoTracking();
            }
            else
            {
                query = query.AsTracking();
            }

            return query;
        }

        /// <summary>
        /// Retrieves an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A result containing the entity if found, or an error message.</returns>
        public async Task<Result<T?>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.GetByIdAsync);
            if (cancellationToken.IsCancellationRequested)
                return Result<T?>.WithFailure("Operation was canceled.");
            try
            {
                if (this.contextFactory is null)
                    return Result<T?>.WithFailure("Context factory is not initialized");

                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<T?>.WithFailure(DatabaseContextIsNotActive);

                // Story 35.D1: resolve the raw int id to the key's MODEL type (identity for plain-int keys; via the
                // value converter for a strongly-typed key such as BarCode.BarCodeId) so FindAsync matches.
                var keyValue = RepositoryKeyResolver.ResolveKeyValue(context.Model, typeof(T), id);
                var entity = await context.Set<T>().FindAsync(new object[] { keyValue }, cancellationToken).ConfigureAwait(false);

                return entity is not null
                    ? Result<T?>.Success(entity)
                    : Result<T?>.WithFailure($"Entity of type {typeof(T).Name} with ID {id} not found.");
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result<T?>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves an entity by its composite identifiers.
        /// </summary>
        public async Task<Result<T?>> GetByIdsAsync(CancellationToken cancellationToken, params object[] ids)
        {
            if (ids is null || ids.Length == 0)
                return Result<T?>.WithFailure("ids cannot be null or empty.");
            try
            {
                await using IIndTraceDbContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (InvalidContext(context, nameof(GetByIdsAsync)))
                    return Result<T?>.WithFailure(DatabaseContextIsNotActive);

                var entity = await context.Set<T>().FindAsync(ids, cancellationToken).ConfigureAwait(false);
                return entity is not null ? Result<T?>.Success(entity) : Result<T?>.WithFailure("Entity not found.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", nameof(GetByIdsAsync), typeof(T).Name);
                return Result<T?>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of entities that match the given specification.
        /// </summary>
        /// <param name="spec">The specification defining filtering, ordering, and inclusion criteria.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Result containing the list of matching entities or failure if error occurred.</returns>
        public async Task<Result<IEnumerable<T>>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.ListAsync);
            if (spec is null)
                return Result<IEnumerable<T>>.WithFailure("spec cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result<IEnumerable<T>>.WithFailure("Operation was canceled.");
            try
            {
                if (this.contextFactory is null)
                    return Result<IEnumerable<T>>.WithFailure("Context factory is not initialized");

                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<IEnumerable<T>>.WithFailure(DatabaseContextIsNotActive);

                // P0-4 (#64): honour spec.IsTracking. ApplySpecification already applies AsNoTracking()/AsTracking()
                // per the specification; forcing .AsNoTracking() here silently overrode a caller's IsTracking = true.
                var entities = await this.ApplySpecification(spec, context).ToListAsync(cancellationToken).ConfigureAwait(false);
                return Result<IEnumerable<T>>.Success(entities);
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result<IEnumerable<T>>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all entities of type T from the repository.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Result containing all entities or failure if error occurred.</returns>
        public async Task<Result<IEnumerable<T>>> ListAsync(CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.ListAsync);
            if (cancellationToken.IsCancellationRequested)
                return Result<IEnumerable<T>>.WithFailure("Operation was canceled.");
            try
            {
                if (this.contextFactory is null)
                    return Result<IEnumerable<T>>.WithFailure("Context factory is not initialized");

                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<IEnumerable<T>>.WithFailure(DatabaseContextIsNotActive);

                var entities = await context.Set<T>().AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
                return Result<IEnumerable<T>>.Success(entities);
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result<IEnumerable<T>>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the first entity that matches the given specification, or null if none found.
        /// </summary>
        /// <param name="spec">The specification defining filtering, ordering, and inclusion criteria.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Result containing the first matching entity or null if none found.</returns>
        public async Task<Result<T?>> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.FirstOrDefaultAsync);
            if (spec is null)
                return Result<T?>.WithFailure("spec cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result<T?>.WithFailure("Operation was canceled.");
            try
            {
                if (this.contextFactory is null)
                    return Result<T?>.WithFailure("Context factory is not initialized");

                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<T?>.WithFailure(DatabaseContextIsNotActive);

                var entity = await this.ApplySpecification(spec, context).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
                return entity is not null ? Result<T?>.Success(entity) : Result<T?>.WithFailure(RepositoryFailures.NotFoundSentinel + ".");
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result<T?>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the first entity of type <typeparamref name="T"/>, or null if none found.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A result containing the entity or null.</returns>
        public async Task<Result<T?>> FirstOrDefaultAsync(CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.FirstOrDefaultAsync);
            if (cancellationToken.IsCancellationRequested)
                return Result<T?>.WithFailure("Operation was canceled.");
            try
            {
                if (this.contextFactory is null)
                    return Result<T?>.WithFailure("Context factory is not initialized");

                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<T?>.WithFailure(DatabaseContextIsNotActive);

                var entity = await context.Set<T>().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
                return entity != null ? Result<T?>.Success(entity) : Result<T?>.WithFailure("No entity found.");
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result<T?>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Result containing the number of entities added or failure if error occurred.</returns>
        public async Task<Result<int>> AddAsync(T entity, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.AddAsync);
            if (entity is null)
                return Result<int>.WithFailure("entity cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result<int>.WithFailure("Operation was canceled.");
            try
            {
                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<int>.WithFailure(DatabaseContextIsNotActive);

                await context.Set<T>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
                var result = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result<int>.Success(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result<int>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Adds a collection of entities to the data store as a single change-tracked batch
        /// (one <c>AddRange</c> + one <c>SaveChangesAsync</c>).
        /// </summary>
        /// <remarks>
        /// #117 (F2): this method previously streamed raw CLR property values to <c>SqlBulkCopy</c> through a
        /// reflected <c>DataTable</c>, bypassing the EF save pipeline entirely — the audit stamper
        /// (<c>IndTraceDbContext.SaveChangesAsync</c>) never stamped CreatedBy/CreatedOn/ModifiedOn, EF value
        /// converters (smart enums / value objects) and <c>HasColumnName</c> mappings were not applied, the copy
        /// enlisted in no transaction, and the <c>SqlConnection</c> cast hard-coupled it to SQL Server. Routing
        /// through the change tracker restores all of those guarantees (a single-batch SaveChanges is
        /// transactional by default) — a deliberate trade of raw bulk-copy speed for correctness, acceptable at
        /// this method's real call volumes (recipe authoring: hundreds to low thousands of rows). The same path
        /// works on every provider, so the former InMemory special-case collapsed into the one code path.
        /// </remarks>
        /// <param name="entities">The entities to add.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A result containing the number of affected rows.</returns>
        public async Task<Result<int>> AddRangeBulkAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
        {
            if (entities is null)
                return Result<int>.WithFailure("entities cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result<int>.WithFailure("Operation was canceled.");

            return await this.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a collection of entities to the data store.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A result containing the number of affected rows.</returns>
        public async Task<Result<int>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.AddRangeAsync);
            if (entities is null)
                return Result<int>.WithFailure("entities cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result<int>.WithFailure("Operation was canceled.");
            try
            {
                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<int>.WithFailure(DatabaseContextIsNotActive);

                await context.Set<T>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
                var result = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result<int>.Success(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result<int>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Adds a new entity to the data store with a specified table name and identifier.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="id">The identifier for the entity.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A result containing the number of affected rows.</returns>
        public async Task<Result<int>> AddAsync(T entity, int id, string tableName, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.AddAsync);
            if (entity is null)
                return Result<int>.WithFailure("entity cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result<int>.WithFailure("Operation was canceled.");
            try
            {
                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<int>.WithFailure(DatabaseContextIsNotActive);

                //[Fix] 
                //CLAUDE
                //Date: 23/09/2025 
                //Reason: [CRITICAL: ID Validation Required] - Products don't have identity, must validate ID > 0
                // Restore critical validation removed by previous agent - Products need manual ID validation
                if (id <= 0 || string.IsNullOrWhiteSpace(tableName))
                {
                    this.logger.LogWarning("Repository: Invalid parameters in {MethodName} for entity type {EntityType}.", methodName, typeof(T).Name);
                    return Result<int>.WithFailure($"Invalid id or table name in {methodName} for entity type {typeof(T).Name}.");
                }

                // If provider is not relational, fall back to regular SaveChanges (no tableName overload)
                var isRelational = context.Database.IsRelational();

                await context.Set<T>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
                if (!isRelational)
                {
                    var saved = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return Result<int>.Success(saved);
                }

                var result = await context.SaveChangesAsync(tableName, cancellationToken).ConfigureAwait(false);
                return result ?? Result<int>.WithFailure("Failed to save changes.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result<int>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing entity in the repository.
        /// </summary>
        /// <param name="entity">The entity with updated values.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Result indicating success or failure of the update operation.</returns>
        public async Task<Result> UpdateAsync(T entity, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.UpdateAsync);
            if (entity is null)
                return Result.WithFailure("entity cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result.WithFailure("Operation was canceled.");
            try
            {
                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result.WithFailure(DatabaseContextIsNotActive);

                return await EntityUpdateHelper<T>.UpdateAsync(context, entity, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Deletes an entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Result indicating success or failure of the delete operation.</returns>
        public async Task<Result> DeleteAsync(T entity, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.DeleteAsync);
            if (entity is null)
                return Result.WithFailure("entity cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result.WithFailure("Operation was canceled.");
            try
            {
                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result.WithFailure(DatabaseContextIsNotActive);

                context.Set<T>().Remove(entity);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Compatibility no-op: this repository has no deferred unit of work to commit.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see cref="Result.Success"/> once the cancellation token is honoured. This is <b>not</b> a commit gate.
        /// </returns>
        /// <remarks>
        /// <para>
        /// P0-4 (#64) finding: <see cref="Repository{T}"/> is <b>stateless per operation</b>. Every mutating method
        /// (<see cref="AddAsync(T, CancellationToken)"/>, <see cref="UpdateAsync"/>, <see cref="DeleteAsync"/>, …)
        /// creates its own pooled <see cref="IIndTraceDbContext"/>, calls <c>SaveChangesAsync</c> inline, and disposes
        /// that context before returning. There is no shared tracked context that accumulates changes between calls, so
        /// there is nothing for a deferred <c>Commit</c> to flush.
        /// </para>
        /// <para>
        /// The previous implementation created a <b>fresh</b> context and checked
        /// <c>ChangeTracker.HasChanges()</c> on it — always <see langword="false"/> on a brand-new context — and so
        /// returned <see cref="Result.Success"/> unconditionally without ever committing anything, while looking like a
        /// real commit gate to callers (e.g. <c>CreateWorkFlowCommandHandler</c>). That deceptive "commit that never
        /// commits" is removed here.
        /// </para>
        /// <para>
        /// This method is retained only to satisfy the <c>IRepository</c> contract and existing call sites that pair
        /// <c>AddAsync</c>/<c>UpdateAsync</c> with a <c>CommitAsync</c>. Because the save already happened inline, this
        /// is an honest, documented no-op. <b>Callers must not rely on it as a transaction boundary.</b> Multi-entity
        /// atomicity is provided by the dedicated transactional repositories (e.g. <c>BarCodeAggregateRepository</c>,
        /// <c>ProductRoutingRepository</c>) or <c>IIndTraceDbContext.SaveChangesAsync(tableName, …)</c>, not by this seam.
        /// </para>
        /// </remarks>
        public Task<Result> CommitAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Result.WithFailure("Operation was canceled."));

            // No shared unit of work to flush: saves happen inline in each mutating method. See <remarks>.
            return Task.FromResult(Result.Success());
        }

        /// <summary>
        /// Counts the number of entities matching the specified specification.
        /// </summary>
        /// <param name="spec">The specification to filter entities.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A result containing the count of entities.</returns>
        public async Task<Result<int>> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.CountAsync);
            if (spec is null)
                return Result<int>.WithFailure("spec cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result<int>.WithFailure("Operation was canceled.");
            try
            {
                await using var context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<int>.WithFailure(DatabaseContextIsNotActive);

                var query = this.ApplySpecification(spec, context);
                var count = await query.CountAsync(cancellationToken).ConfigureAwait(false);
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Repository: Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);
                return Result<int>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Leases an <see cref="OwnedQueryable{T}"/> for the entity type. #117 (F1): the returned lease OWNS the
        /// pooled context backing the queryable — disposing the lease (the caller's <c>await using</c>) is the only
        /// way that context returns to the pool. This closes the P0-4 (#64) follow-up: the previous contract handed
        /// out a bare <see cref="IQueryable{T}"/> whose backing pooled context was never disposed, permanently
        /// consuming a pooled context (and its connection) per call.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A result containing the owned queryable lease.</returns>
        public async Task<Result<OwnedQueryable<T>>> AsQueryableAsync(CancellationToken cancellationToken = default)
        {
            const string methodName = nameof(this.AsQueryableAsync);
            if (cancellationToken.IsCancellationRequested)
                return Result<OwnedQueryable<T>>.WithFailure("Operation was canceled.");
            IIndTraceDbContext? context = null;
            try
            {
                context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<OwnedQueryable<T>>.WithFailure(DatabaseContextIsNotActive);

                return Result<OwnedQueryable<T>>.Success(new OwnedQueryable<T>(context.Set<T>().AsQueryable(), context));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);

                // A failure Result must never leak the leased context.
                if (context is not null)
                    await context.DisposeAsync().ConfigureAwait(false);
                return Result<OwnedQueryable<T>>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Leases an <see cref="OwnedQueryable{T}"/> for the entity type, filtered by the specified specification.
        /// #117 (F1): the returned lease OWNS the pooled context backing the queryable; see the parameterless
        /// <see cref="AsQueryableAsync(CancellationToken)"/> overload for the full ownership-contract rationale.
        /// </summary>
        /// <param name="spec">The specification to filter entities.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A result containing the owned queryable lease.</returns>
        public async Task<Result<OwnedQueryable<T>>> AsQueryableAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.AsQueryableAsync);
            if (spec is null)
                return Result<OwnedQueryable<T>>.WithFailure("spec cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result<OwnedQueryable<T>>.WithFailure("Operation was canceled.");
            IIndTraceDbContext? context = null;
            try
            {
                context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<OwnedQueryable<T>>.WithFailure(DatabaseContextIsNotActive);

                return Result<OwnedQueryable<T>>.Success(new OwnedQueryable<T>(this.ApplySpecification(spec, context), context));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);

                // A failure Result must never leak the leased context.
                if (context is not null)
                    await context.DisposeAsync().ConfigureAwait(false);
                return Result<OwnedQueryable<T>>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Returns a composable <see cref="IQueryable{T}"/> rooted at a parameterized raw-SQL query. Story 27.2b-2
        /// (#27/F4): the server-side seam for query shapes an EF value converter cannot translate (e.g. a label-
        /// substring <c>LIKE</c> over the value-converted <c>BarCode.Label</c>). The interpolated <paramref name="sql"/>
        /// is threaded through <c>FromSql(FormattableString)</c>, which binds every interpolation hole as a SQL
        /// parameter (no string concatenation / SQL injection), and the result composes with further LINQ.
        /// </summary>
        /// <param name="sql">A parameterized (interpolated) SQL statement whose columns cover the entity's mapped properties.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A result containing the raw-SQL-rooted owned queryable lease.</returns>
        public async Task<Result<OwnedQueryable<T>>> FromSqlAsync(FormattableString sql, CancellationToken cancellationToken)
        {
            const string methodName = nameof(this.FromSqlAsync);
            if (sql is null)
                return Result<OwnedQueryable<T>>.WithFailure("sql cannot be null.");
            if (cancellationToken.IsCancellationRequested)
                return Result<OwnedQueryable<T>>.WithFailure("Operation was canceled.");
            IIndTraceDbContext? context = null;
            try
            {
                // #117 (F1): the raw-SQL-rooted queryable is composed and enumerated lazily by the caller, so the
                // backing pooled context must outlive this method — the OwnedQueryable lease now carries that
                // ownership, and the caller's `await using` returns the context to the pool.
                context = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                if (this.InvalidContext(context, methodName))
                    return Result<OwnedQueryable<T>>.WithFailure(DatabaseContextIsNotActive);

                return Result<OwnedQueryable<T>>.Success(new OwnedQueryable<T>(context.Set<T>().FromSql(sql), context));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in {MethodName} for entity type {EntityType}", methodName, typeof(T).Name);

                // A failure Result must never leak the leased context.
                if (context is not null)
                    await context.DisposeAsync().ConfigureAwait(false);
                return Result<OwnedQueryable<T>>.WithFailure(ex.Message);
            }
        }

        /// <summary>
        /// Idempotent no-op under the stateless-per-operation design: the entity is not tracked by any live context.
        /// </summary>
        /// <param name="entity">The entity to detach.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A failure if the entity is null or the operation is cancelled; otherwise success (the post-condition
        /// "entity is not tracked" already holds).
        /// </returns>
        /// <remarks>
        /// P0-4 (#64) finding: the previous implementation created a <b>fresh</b> pooled context and set
        /// <c>context.Entry(entity).State = Detached</c> on it. That context never tracked <paramref name="entity"/>
        /// (the entity was materialised/persisted by a different, already-disposed context), so the call had no effect
        /// and additionally leaked the pooled context. Because every mutating method here uses its own context and
        /// disposes it, a caller-held entity is inherently untracked — detach is a no-op whose post-condition already
        /// holds. No context is created; the cancellation and null guards are preserved.
        /// </remarks>
        public Task<Result> DetachAsync(T entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                return Task.FromResult(Result.WithFailure("entity cannot be null."));
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Result.WithFailure("Operation was canceled."));

            // Nothing to detach: no shared/live context tracks this entity. See <remarks>.
            return Task.FromResult(Result.Success());
        }

        /// <summary>
        /// Unsupported in the stateless-per-operation repository: tracking is controlled per query via
        /// <see cref="ISpecification{T}.IsTracking"/>, not by a repository-wide mode toggle.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A failure result; there is no shared context on which a tracking mode could persist.</returns>
        /// <remarks>
        /// P0-4 (#64) finding: the previous implementation set <c>QueryTrackingBehavior</c> on a <b>fresh</b> pooled
        /// context that was immediately discarded, so the setting never affected any subsequent query and the context
        /// leaked. Rather than leave a Success no-op that pretends to work, this now fails loud. Callers that need
        /// no-tracking reads must pass a specification with <c>IsTracking = false</c>.
        /// </remarks>
        public Task<Result> ApplyNoTrackingAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Result.WithFailure("Operation was canceled."));

            return Task.FromResult(Result.WithFailure(
                "ApplyNoTrackingAsync is not supported: this repository is stateless per operation and has no shared context to configure. Use a specification with IsTracking = false instead."));
        }

        /// <summary>
        /// Unsupported in the stateless-per-operation repository: tracking is controlled per query via
        /// <see cref="ISpecification{T}.IsTracking"/>, not by a repository-wide mode toggle.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A failure result; there is no shared context on which a tracking mode could persist.</returns>
        /// <remarks>
        /// P0-4 (#64) finding: see <see cref="ApplyNoTrackingAsync"/>. Callers that need tracked reads must pass a
        /// specification with <c>IsTracking = true</c>.
        /// </remarks>
        public Task<Result> ApplyTrackingAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Result.WithFailure("Operation was canceled."));

            return Task.FromResult(Result.WithFailure(
                "ApplyTrackingAsync is not supported: this repository is stateless per operation and has no shared context to configure. Use a specification with IsTracking = true instead."));
        }

        /// <summary>
        /// Disposes the repository asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync() => await Task.CompletedTask;
    }
}