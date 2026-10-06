// <copyright file="SqlServerTestDatabaseHelper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Models.Interfaces;
using IndTrace.Persistence.DBContext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IndTrace.Persistence.Testing;
/// <summary>
/// Represents the SqlServerTestDatabaseHelper.
/// </summary>

public class SqlServerTestDatabaseHelper : ITestDatabaseHelper
{
    private readonly IndTraceDbContext context;
    private readonly ILogger<SqlServerTestDatabaseHelper> logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerTestDatabaseHelper"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="logger">The logger.</param>

    public SqlServerTestDatabaseHelper(IndTraceDbContext context, ILogger<SqlServerTestDatabaseHelper> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>
    /// Executes ResetDatabaseAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ResetDatabaseAsync.</returns>

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (!await this.IsValidSnapshotAsync("IndTraceData_Snapshot", cancellationToken))
        {
            this.logger.LogWarning("Snapshot 'IndTraceData_Snapshot' is not valid. Skipping restore.");
            return;
        }

        this.logger.LogInformation("Restoring database snapshot...");
        await this.context.Database.ExecuteSqlRawAsync("RESTORE DATABASE IndTraceData FROM DATABASE_SNAPSHOT = 'IndTraceData_Snapshot'", cancellationToken);
    }

    /// <summary>
    /// Executes SeedTestDataAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of SeedTestDataAsync.</returns>

    public Task SeedTestDataAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Seeding additional test data (stubbed).");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes EnsureDatabaseCreatedAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of EnsureDatabaseCreatedAsync.</returns>

    public async Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default)
    {
        await this.context.Database.EnsureCreatedAsync(cancellationToken);
    }

    /// <summary>
    /// Executes ApplyMigrationAsync operation.
    /// </summary>
    /// <param name="migrationName">The migrationName.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ApplyMigrationAsync.</returns>

    public async Task ApplyMigrationAsync(string? migrationName = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(migrationName))
        {
            await this.context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            var migrator = this.context.Database.GetService<IMigrator>();
            await migrator.MigrateAsync(migrationName, cancellationToken);
        }
    }

    /// <summary>
    /// Executes RevertMigrationAsync operation.
    /// </summary>
    /// <param name="migrationName">The migrationName.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of RevertMigrationAsync.</returns>

    public Task RevertMigrationAsync(string? migrationName = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes CreateSnapshotAsync operation.
    /// </summary>
    /// <param name="snapshotName">The snapshotName.</param>
    /// <param name="dbName">The dbName.</param>
    /// <param name="filePath">The filePath.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of CreateSnapshotAsync.</returns>

    public async Task CreateSnapshotAsync(string snapshotName, string dbName, string filePath, CancellationToken cancellationToken = default)
    {
        var sql = $"CREATE DATABASE {snapshotName} ON (NAME = '{dbName}', FILENAME = '{filePath}') AS SNAPSHOT OF {dbName}";
        await this.context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Executes RestoreSnapshotAsync operation.
    /// </summary>
    /// <param name="snapshotName">The snapshotName.</param>
    /// <param name="dbName">The dbName.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of RestoreSnapshotAsync.</returns>

    public async Task RestoreSnapshotAsync(string snapshotName, string dbName, CancellationToken cancellationToken = default)
    {
        if (!await this.IsValidSnapshotAsync(snapshotName, cancellationToken))
        {
            this.logger.LogWarning("Snapshot '{snapshotName}' is not valid. Skipping restore.", snapshotName);
            return;
        }

        var sql = $"RESTORE DATABASE {dbName} FROM DATABASE_SNAPSHOT = '{snapshotName}'";
        await this.context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Executes DropSnapshotAsync operation.
    /// </summary>
    /// <param name="snapshotName">The snapshotName.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of DropSnapshotAsync.</returns>

    public async Task DropSnapshotAsync(string snapshotName, CancellationToken cancellationToken = default)
    {
        if (!await this.IsValidSnapshotAsync(snapshotName, cancellationToken))
        {
            this.logger.LogWarning("Database '{snapshotName}' is not recognized as a snapshot or does not exist.", snapshotName);
            return;
        }

        this.logger.LogInformation("Dropping snapshot database '{snapshotName}'...", snapshotName);
        await this.context.Database.ExecuteSqlAsync($"DROP DATABASE [{snapshotName}]", cancellationToken);
    }

    /// <summary>
    /// Executes IsSnapshotPresentAsync operation.
    /// </summary>
    /// <param name="snapshotName">The snapshotName.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of IsSnapshotPresentAsync.</returns>

    public async Task<bool> IsSnapshotPresentAsync(string snapshotName, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT COUNT(*) FROM sys.databases WHERE name = @snapshotName";
        var result = await this.context.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@snapshotName", snapshotName));
        return result > 0;
    }

    /// <summary>
    /// Executes GetDatabaseFilePathAsync operation.
    /// </summary>
    /// <param name="dbName">The dbName.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of GetDatabaseFilePathAsync.</returns>

    public async Task<string> GetDatabaseFilePathAsync(string dbName, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT TOP 1 physical_name FROM sys.master_files WHERE database_id = DB_ID(@dbName) AND type_desc = 'ROWS'";
        var connection = this.context.Database.GetDbConnection();

        await connection.OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@dbName", dbName));

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result?.ToString() ?? throw new InvalidOperationException("Database file path not found.");
    }

    private async Task<bool> IsValidSnapshotAsync(string snapshotName, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM sys.databases d
            JOIN sys.database_files df ON d.database_id = df.database_id
            WHERE d.name = @snapshotName AND d.source_database_id IS NOT NULL";

        var connection = this.context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@snapshotName", snapshotName));

        var scalarResult = await command.ExecuteScalarAsync(cancellationToken);
        var result = scalarResult is int intResult ? intResult : 0;
        return result > 0;
    }
}