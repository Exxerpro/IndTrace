// <copyright file="IAppendOnlyRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

/// <summary>
/// Defines an append-only repository interface for entities of type <typeparamref name="T"/>.
/// Exposes insert operations exclusively; no update or delete surface is available, which makes
/// write-once / audit-style entities (e.g. <c>Register</c>) immune to in-process mutation or removal.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public interface IAppendOnlyRepository<T> where T : class, IndTrace.Domain.Interfaces.IPersistable
{
    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the number of affected rows.</returns>
    Task<Result<int>> AddAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a collection of entities to the data store.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the number of affected rows.</returns>
    Task<Result<int>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a collection of entities to the data store as a single batch. #117 (F2): implementations must go
    /// through the EF save pipeline (audit stamping, value converters, column mappings) — never a raw bulk copy.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the number of affected rows.</returns>
    Task<Result<int>> AddRangeBulkAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
}
