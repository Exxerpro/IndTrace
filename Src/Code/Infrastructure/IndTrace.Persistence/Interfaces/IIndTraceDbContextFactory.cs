// <copyright file="IIndTraceDbContextFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore;

namespace IndTrace.Persistence.Interfaces;

/// <summary>
/// Provides methods for creating IndTrace database context instances.
/// </summary>
public interface IIndTraceDbContextFactory : IAsyncDisposable
{
    /// <summary>
    /// Creates a new Entity Framework DbContext instance.
    /// </summary>
    /// <returns>A new DbContext instance.</returns>
    DbContext CreateEfDbContext();

    /// <summary>
    /// Asynchronously creates a new IndTrace database context instance.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation, with a new IIndTraceDbContext instance.</returns>
    Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Synchronously creates a new IndTrace database context instance.
    /// </summary>
    /// <returns>A task representing the synchronous operation, with a new IIndTraceDbContext instance.</returns>
    IIndTraceDbContext CreateDbContext();
}