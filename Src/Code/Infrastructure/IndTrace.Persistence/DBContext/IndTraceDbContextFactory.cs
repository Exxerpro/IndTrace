// <copyright file="IndTraceDbContextFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndTrace.Persistence.DBContext;

/// <summary>
/// Factory implementation for creating IndTrace database context instances with connection pooling support.
/// Provides both synchronous and asynchronous context creation for industrial data operations.
/// </summary>
/// <param name="options">The database context options for configuration.</param>
/// <param name="pooledFactory">The pooled factory for efficient context instance management.</param>
public class IndTraceDbContextFactory(DbContextOptions<IndTraceDbContext> options, IDbContextFactory<IndTraceDbContext> pooledFactory) : IIndTraceDbContextFactory, IAsyncDisposable
{
    /// <summary>
    /// Creates a new database context instance synchronously using the pooled factory.
    /// </summary>
    /// <returns>A new IndTrace database context instance.</returns>
    public IIndTraceDbContext CreateDbContext()
    {
        return pooledFactory.CreateDbContext();
    }

    /// <summary>
    /// Creates a new database context instance asynchronously using the pooled factory.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous context creation operation.</returns>
    public Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        // Explicitly cast the IndTraceDbContext to IIndTraceDbContext to resolve the type mismatch
        //
        // // return Task.FromResult<IIndTraceDbContext>(new IndTraceDbContext(options));

        //TODO [VERIFY]
        // ABR CHECK THIS STILL WORK, BECAUSE I DON'T HAVE REGISTERED A CONTEXT FACTORY IN THE PERSISTENCE PROJECT
        // I HAVE A CONTEXT FACTORY ON THE CLIENTS CLASS
        // DbContext registration updated to use AddPooledDbContextFactory for improved performance and thread safety.
        // This allows IDbContextFactory<IndTraceDbContext> to provide pooled DbContext instances.
        // Change applied: 2025-06-12

        return Task.FromResult<IIndTraceDbContext>(pooledFactory.CreateDbContext());
    }

    /// <summary>
    /// Creates a direct Entity Framework DbContext instance for bulk operations that require direct SQL Server access.
    /// Used primarily for SqlBulkCopy operations and performance-critical database interactions.
    /// </summary>
    /// <returns>A direct EF Core DbContext instance.</returns>
    public DbContext CreateEfDbContext()
    {
        return new IndTraceDbContext(options); // Used directly in bulk operations
    }

    public ValueTask DisposeAsync()
    {
        // Dispose pooledFactory if it supports async disposal
        if (pooledFactory is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }
        // Fallback to synchronous disposal if available
        if (pooledFactory is IDisposable disposable)
        {
            disposable.Dispose();
        }
        // Nothing to dispose asynchronously
        return ValueTask.CompletedTask;
    }
}