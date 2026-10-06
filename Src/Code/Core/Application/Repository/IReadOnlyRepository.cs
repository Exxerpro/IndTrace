// <copyright file="IReadOnlyRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

using System.Security.Principal;

/// <summary>
/// Defines a read-only repository interface for accessing entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of entity.</typeparam>
public interface IReadOnlyRepository<T> where T : class, IndTrace.Domain.Interfaces.IPersistable
{
    /// <summary>
    /// Gets an entity by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the entity if found; otherwise, null.</returns>
    Task<Result<T?>> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an entity by its composite identifiers asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="ids">The ordered key values that compose the entity primary key.</param>
    Task<Result<T?>> GetByIdsAsync(CancellationToken cancellationToken, params object[] ids);

    /// <summary>
    /// Lists entities matching the given specification asynchronously.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the list of entities.</returns>
    Task<Result<IEnumerable<T>>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all entities asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the list of entities.</returns>
    Task<Result<IEnumerable<T>>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the first entity matching the given specification asynchronously, or null if none found.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the first entity if found; otherwise, null.</returns>
    Task<Result<T?>> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the first entity asynchronously, or null if none found.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the first entity if found; otherwise, null.</returns>
    Task<Result<T?>> FirstOrDefaultAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Counts the number of entities matching the given specification asynchronously.
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the count of entities.</returns>
    Task<Result<int>> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Leases an <see cref="OwnedQueryable{T}"/> for the entity type so callers can compose read-side
    /// projections that are translated to the data store. The query is read-only; it provides no insert,
    /// update, or delete capability. #117 (F1): the lease OWNS the backing pooled context; on success the
    /// caller MUST <c>await using</c> the value and keep it alive until the composed query has been
    /// materialized — disposing the lease is the only way the context returns to the pool. Failure results
    /// never carry a lease.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the owned queryable lease.</returns>
    Task<Result<OwnedQueryable<T>>> AsQueryableAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Leases an <see cref="OwnedQueryable{T}"/> for the entity type, filtered by the specified specification,
    /// so callers can compose read-side projections that are translated to the data store. The query is
    /// read-only; it provides no insert, update, or delete capability. #117 (F1): the lease OWNS the backing
    /// pooled context; on success the caller MUST <c>await using</c> the value until materialization completes
    /// (see <see cref="AsQueryableAsync(CancellationToken)"/>).
    /// </summary>
    /// <param name="spec">The specification to filter entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the owned queryable lease.</returns>
    Task<Result<OwnedQueryable<T>>> AsQueryableAsync(ISpecification<T> spec, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a COMPOSABLE <see cref="IQueryable{T}"/> rooted at a parameterized raw-SQL query. The interpolated
    /// <paramref name="sql"/> is bound as SQL parameters (never string-concatenated), so callers MUST pass values
    /// through the interpolation holes rather than building the SQL by concatenation. #229 (Slice B): this is the
    /// read-side seam for query shapes an EF value converter cannot translate (e.g. a substring <c>LIKE</c> or a
    /// correlated <c>EXISTS</c> over another table); the returned queryable composes with further LINQ
    /// (Where/Select/Distinct/ToListAsync) exactly like <see cref="AsQueryableAsync(CancellationToken)"/>.
    /// Results are ALWAYS no-tracking, consistent with read-only repository policy. #117 (F1): the lease OWNS the
    /// backing pooled context; on success the caller MUST <c>await using</c> the value and keep it alive until the
    /// composed query has been materialized. Failure results never carry a lease.
    /// </summary>
    /// <param name="sql">A parameterized (interpolated) SQL statement whose columns cover the entity's mapped properties.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the raw-SQL-rooted owned queryable lease.</returns>
    Task<Result<OwnedQueryable<T>>> FromSqlAsync(FormattableString sql, CancellationToken cancellationToken);
}

