// <copyright file="DemoDatabaseSeeder.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.DemoSeed;

/// <summary>
/// Writes a <see cref="DemoDataset"/> into an IndTrace data database. The dataset carries its own keys (PLC ids
/// equal machine ids, tag groups use their catalog values), so on SQL Server identity inserts are switched on per
/// table. Refuses a database that already holds stations, so it never mixes the demo line with real data.
/// </summary>
/// <param name="context">The data database context.</param>
/// <param name="logger">The logger.</param>
public sealed class DemoDatabaseSeeder(IndTraceDbContext context, ILogger<DemoDatabaseSeeder> logger)
{
    /// <summary>
    /// Writes the dataset in one transaction.
    /// </summary>
    /// <param name="dataset">The dataset to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Success, or the reason nothing was written.</returns>
    public async Task<Result> SeedAsync(DemoDataset dataset, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was cancelled.");
        }

        if (dataset is null)
        {
            return Result.WithFailure("A dataset is required.");
        }

        if (await context.Set<Machine>().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return Result.WithFailure("The database already holds stations. Seed an empty database (use --reset to recreate it).");
        }

        var relational = context.Database.IsRelational();
        await using var transaction = relational
            ? await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
            : null;

        foreach (var rows in Batches(dataset.All()))
        {
            var entityType = context.Entry(rows[0]).Metadata;
            var table = $"[{entityType.GetSchema() ?? "dbo"}].[{entityType.GetTableName()}]";
            var identityInsert = relational && entityType.GetProperties().Any(p => p.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.IdentityColumn);

            if (identityInsert)
            {
                await context.Database.ExecuteSqlRawAsync(IdentityInsert(table, on: true), cancellationToken).ConfigureAwait(false);
            }

            context.AddRange(rows);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            context.ChangeTracker.Clear();

            if (identityInsert)
            {
                await context.Database.ExecuteSqlRawAsync(IdentityInsert(table, on: false), cancellationToken).ConfigureAwait(false);
            }

            logger.LogInformation("{Table}: {Count} rows", table, rows.Count);
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }

    // The table name comes from the EF model, never from input; SET IDENTITY_INSERT cannot take a parameter.
    private static string IdentityInsert(string table, bool on) => $"SET IDENTITY_INSERT {table} {(on ? "ON" : "OFF")}";

    // Consecutive rows of the same entity type, in dataset order (which satisfies the foreign keys).
    private static IEnumerable<List<object>> Batches(IEnumerable<object> rows)
    {
        var batch = new List<object>();
        foreach (var row in rows)
        {
            if (batch.Count > 0 && batch[0].GetType() != row.GetType())
            {
                yield return batch;
                batch = [];
            }

            batch.Add(row);
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}
