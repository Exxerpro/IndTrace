// <copyright file="IRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

/// <summary>
/// Defines a generic repository interface for data access operations on entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public interface IRepository<T> where T : class, IndTrace.Domain.Interfaces.IPersistable
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the entity if found, or an error message.</returns>
    Task<Result<T?>> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an entity by its composite identifiers.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <param name="ids">The ordered key values that compose the entity primary key.</param>
    /// <returns>A result containing the entity if found, or an error message.</returns>
    Task<Result<T?>> GetByIdsAsync(CancellationToken cancellationToken, params object[] ids);

    /// <summary>
    /// Retrieves a list of entities matching the specified specification.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the list of entities.</returns>
    Task<Result<IEnumerable<T>>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the list of entities.</returns>
    Task<Result<IEnumerable<T>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the first entity matching the specified specification, or null if none found.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the entity or null.</returns>
    Task<Result<T?>> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the first entity of type <typeparamref name="T"/>, or null if none found.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the entity or null.</returns>
    Task<Result<T?>> FirstOrDefaultAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the number of affected rows.</returns>
    Task<Result<int>> AddAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a collection of entities to the data store as a single batch. #117 (F2): implementations must go
    /// through the EF save pipeline (audit stamping, value converters, column mappings) — never a raw bulk copy.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the number of affected rows.</returns>
    Task<Result<int>> AddRangeBulkAsync(IEnumerable<T> entities, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new entity to the data store with a specified table name and identifier.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="id">The identifier for the entity.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the number of affected rows.</returns>
    Task<Result<int>> AddAsync(T entity, int id, string tableName, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a collection of entities to the data store.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the number of affected rows.</returns>
    Task<Result<int>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing entity in the data store.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<Result> UpdateAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an entity from the data store.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<Result> DeleteAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Commits all changes to the data store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<Result> CommitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Counts the number of entities matching the specified specification.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the count of entities.</returns>
    Task<Result<int>> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken);

    /// <summary>
    /// Leases an <see cref="OwnedQueryable{T}"/> for the entity type. #117 (F1): the lease OWNS the backing
    /// pooled context; on success the caller MUST <c>await using</c> the value and keep it alive until the
    /// composed query has been materialized — disposing the lease is the only way the context returns to the
    /// pool. Failure results never carry a lease.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the owned queryable lease.</returns>
    Task<Result<OwnedQueryable<T>>> AsQueryableAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Leases an <see cref="OwnedQueryable{T}"/> for the entity type, filtered by the specified specification.
    /// #117 (F1): the lease OWNS the backing pooled context; on success the caller MUST <c>await using</c> the
    /// value until materialization completes (see <see cref="AsQueryableAsync(CancellationToken)"/>).
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the owned queryable lease.</returns>
    Task<Result<OwnedQueryable<T>>> AsQueryableAsync(ISpecification<T> spec, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a COMPOSABLE <see cref="IQueryable{T}"/> rooted at a parameterized raw-SQL query. The interpolated
    /// <paramref name="sql"/> is bound as SQL parameters (never string-concatenated), so callers MUST pass values
    /// through the interpolation holes rather than building the SQL by concatenation. Story 27.2b-2 (#27/F4): this is
    /// the server-side seam for query shapes an EF value converter cannot translate (e.g. a label-substring
    /// <c>LIKE</c> over the value-converted <c>BarCode.Label</c>); the returned queryable composes with further LINQ
    /// (Where/OrderBy/ToListAsync) exactly like <see cref="AsQueryableAsync(CancellationToken)"/>. #117 (F1): the
    /// lease OWNS the backing pooled context; on success the caller MUST <c>await using</c> the value until
    /// materialization completes.
    /// </summary>
    /// <param name="sql">A parameterized (interpolated) SQL statement whose columns cover the entity's mapped properties.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the raw-SQL-rooted owned queryable lease.</returns>
    Task<Result<OwnedQueryable<T>>> FromSqlAsync(FormattableString sql, CancellationToken cancellationToken);

    /// <summary>
    /// Detaches the specified entity from the context.
    /// </summary>
    /// <param name="entity">The entity to detach.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<Result> DetachAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Applies no-tracking behavior to the context.
    /// </summary>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<Result> ApplyNoTrackingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Applies tracking behavior to the context.
    /// </summary>
    /// <returns>A result indicating the outcome of the operation.</returns>
    Task<Result> ApplyTrackingAsync(CancellationToken cancellationToken);
}
