// <copyright file="EfDbContextFactoryAdapter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Interfaces;
using IndTrace.Persistence.DBContext;

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Adapter that wraps EF Core's IDbContextFactory&lt;T&gt; to implement custom IIndTraceDbContextFactory interface.
/// Provides compatibility between EF Core's factory pattern and custom factory interface.
/// </summary>
public class EfDbContextFactoryAdapter : IIndTraceDbContextFactory
{
    private readonly IDbContextFactory<IndTraceDbContext> _efContextFactory;

    public EfDbContextFactoryAdapter(IDbContextFactory<IndTraceDbContext> efContextFactory)
    {
        _efContextFactory = efContextFactory ?? throw new ArgumentNullException(nameof(efContextFactory));
    }

    /// <summary>
    /// Creates a new Entity Framework DbContext instance.
    /// </summary>
    /// <returns>A new DbContext instance.</returns>
    public DbContext CreateEfDbContext()
    {
        return _efContextFactory.CreateDbContext();
    }

    /// <summary>
    /// Asynchronously creates a new IndTrace database context instance.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation, with a new IIndTraceDbContext instance.</returns>
    public Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        var context = _efContextFactory.CreateDbContext();
        return Task.FromResult<IIndTraceDbContext>(context);
    }

    /// <summary>
    /// Synchronously creates a new IndTrace database context instance.
    /// </summary>
    /// <returns>A new IIndTraceDbContext instance.</returns>
    public IIndTraceDbContext CreateDbContext()
    {
        return _efContextFactory.CreateDbContext();
    }

    /// <summary>
    /// Disposes the adapter.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}