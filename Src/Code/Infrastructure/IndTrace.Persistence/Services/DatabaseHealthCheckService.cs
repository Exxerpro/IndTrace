// <copyright file="DatabaseHealthCheckService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using IndTrace.Application.Models.Interfaces;
using IndTrace.Domain.Interfaces;
using IndTrace.Persistence.Interfaces;

namespace IndTrace.Persistence.Services;
/// <summary>
/// Represents the DatabaseHealthCheckService.
/// </summary>

public class DatabaseHealthCheckService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<DatabaseHealthCheckService> logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHealthCheckService"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="serviceProvider">The serviceProvider.</param>
    /// <param name="logger">The logger.</param>

    public DatabaseHealthCheckService(IServiceProvider serviceProvider, ILogger<DatabaseHealthCheckService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }
    /// <summary>
    /// Executes StartAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of StartAsync.</returns>

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = this.serviceProvider.CreateScope())
        {
            try
            {
                // #107: acquire a short-lived context from the pooled factory instead of resolving a
                // registered IIndTraceDbContext — no host needs to pin a shared context in DI for this probe.
                var contextFactory = scope.ServiceProvider.GetRequiredService<IIndTraceDbContextFactory>();
                await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Checking database connection...");
                var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
                if (!canConnect)
                {
                    throw new Exception("Database connection failed.");
                }
                this.logger.LogInformation("Database connection successful.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Database connection failed.");
                throw new Exception("Database connection failed.", ex);
            }
        }
    }
    /// <summary>
    /// Executes StopAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of StopAsync.</returns>

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

