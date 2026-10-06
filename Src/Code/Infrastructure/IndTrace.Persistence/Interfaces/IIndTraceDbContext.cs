// <copyright file="IIndTraceDbContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum.LookUpTable;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IndTrace.Persistence.Interfaces;

/// <summary>
/// Represents the IndTrace database context interface for managing entities and database operations.
/// </summary>
public interface IIndTraceDbContext : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    int SaveChanges();

    /// <summary>
    /// Creates a DbSet that can be used to query and save instances of entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <returns>A set for the given entity type.</returns>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Creates a DbSet for lookup table entities that extend EnumModel.
    /// </summary>
    /// <typeparam name="TLookupEntity">Lookup entity type that extends EnumModel.</typeparam>
    /// <returns>A set for the given lookup entity type.</returns>
    DbSet<TLookupEntity> LookupSet<TLookupEntity>() where TLookupEntity : class;

    /// <summary>
    /// Creates a DbSet for app-wide shared objects (configs, app status, health, etc.).
    /// </summary>
    /// <typeparam name="TState">State entity type that implements IAppState.</typeparam>
    /// <returns>A set for the given state entity type.</returns>
    DbSet<TState> SetState<TState>() where TState : class, IAppState, new();

    /// <summary>
    /// Creates a DbSet for aggregate-specific entities that implement aggregate root pattern.
    /// </summary>
    /// <typeparam name="TAggregate">Aggregate entity type that implements IAggregateRoot.</typeparam>
    /// <returns>A set for the given aggregate entity type.</returns>
    DbSet<TAggregate> SetAggregate<TAggregate>() where TAggregate : class, IAggregateRoot;

    /// <summary>
    /// Provides access to change tracking information and operations.
    /// </summary>
    ChangeTracker ChangeTracker { get; }

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously, optionally deferring
    /// change-tracker acceptance. #126 (C3/C4): a flush that runs INSIDE an explicit transaction of a
    /// retriable execution-strategy unit must pass <paramref name="acceptAllChangesOnSuccess"/> =
    /// <see langword="false"/> and call <c>ChangeTracker.AcceptAllChanges()</c> only AFTER the commit
    /// succeeds — otherwise a flushed-then-rolled-back attempt leaves the tracker accepted (and, with the
    /// default overload, the store-generated values written onto the entity instances), so the strategy's
    /// retry silently re-runs against corrupted state.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    /// Whether <c>AcceptAllChanges</c> is called on the tracker when the flush succeeds. Pass
    /// <see langword="false"/> when a later commit can still fail and the unit may be retried.
    /// </param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken);

    /// <summary>
    /// Provides access to database-related information and operations.
    /// </summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// Indicates whether the database supports transactions.
    /// </summary>
    bool SupportsTransactions { get; }

    /// <summary>
    /// Saves changes to the database with a specific table name.
    /// </summary>
    /// <param name="tableName">The name of the table to save to.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task<Result<int>> SaveChangesAsync(string tableName, CancellationToken cancellationToken);

    /// <summary>
    /// The model that backs this context.
    /// </summary>
    IModel Model { get; }

    /// <summary>
    /// Gets an EntityEntry for the given entity providing access to information about the entity and the ability to perform actions on the entity.
    /// </summary>
    /// <param name="entity">The entity to get the entry for.</param>
    /// <returns>The entry for the given entity.</returns>
    EntityEntry Entry(object entity);
}