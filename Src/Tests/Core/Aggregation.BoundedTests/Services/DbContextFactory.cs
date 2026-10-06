// <copyright file="DbContextFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IndTrace.Aggregation.BoundedTests.Services;
/// <summary>
/// Represents the DbContextFactory.
/// </summary>

public class DbContextFactoryTests(DbContextTests contextTestsData) : IIndTraceDbContextFactory
{
    /// <summary>
    /// Executes CreateAndLoadDbContext operation.
    /// </summary>
    /// <returns>The result of CreateAndLoadDbContext.</returns>
    public async Task<IIndTraceDbContextFactory> CreateAndLoadDbContext()
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        var currentUserService = new TesterUserService();
        var machineDateTime = new DateTimeMachine();

        var context = new IndTraceDbContext(options);

        context.SetTestingInterfaces(currentUserService, machineDateTime);

        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        await contextTestsData.LoadReferenceDataAsync(context);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (IIndTraceDbContextFactory)context;
    }
    /// <summary>
    /// Executes Destroy operation.
    /// </summary>
    /// <param name="context">The context.</param>

    public static void Destroy(IndTraceDbContext context)
    {
        if (context is null) return;
        context.Database.EnsureDeleted();
        context.Dispose();
    }
    /// <summary>
    /// Executes CreateEfDbContext operation.
    /// </summary>
    /// <returns>The result of CreateEfDbContext.</returns>

    public Microsoft.EntityFrameworkCore.DbContext CreateEfDbContext()
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Executes CreateDbContextAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of CreateDbContextAsync.</returns>

    public async Task<Persistence.Interfaces.IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        return (Persistence.Interfaces.IIndTraceDbContext)await CreateAndLoadDbContext();
    }
    /// <summary>
    /// Executes CreateDbContext operation.
    /// </summary>
    /// <returns>The result of CreateDbContext.</returns>

    public Persistence.Interfaces.IIndTraceDbContext CreateDbContext()
    {
        return (Persistence.Interfaces.IIndTraceDbContext)CreateAndLoadDbContext().GetAwaiter().GetResult();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}