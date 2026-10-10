// <copyright file="MigrationsCoverModelTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Keeps the EF Core migrations in step with the model, so <c>Database.MigrateAsync()</c> can create a new
/// database the current code can use (issue #246). A model change without a migration fails here instead of at
/// install time with <c>PendingModelChangesWarning</c>.
/// <para>
/// No database connection is opened: the check compares the migrations' model snapshot with the built model.
/// </para>
/// </summary>
public class MigrationsCoverModelTests
{
    private static IndTraceDbContext NewContext() =>
        new(new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseSqlServer("Server=architecture-model-build-only;Database=none;Trusted_Connection=True;")
            .Options);

    /// <summary>
    /// The latest migration snapshot equals the current model.
    /// </summary>
    [Fact]
    public void Model_HasNoChangesMissingFromTheMigrations()
    {
        using var context = NewContext();

        context.Database.HasPendingModelChanges().ShouldBeFalse(
            "The EF model has changes no migration covers. Add one with: dotnet ef migrations add <Name> " +
            "--project Src/Code/Infrastructure/IndTrace.Persistence/IndTrace.Persistence.csproj --output-dir Migrations");
    }

    /// <summary>
    /// The migrations start from the baseline, which creates the whole schema of a new database.
    /// </summary>
    [Fact]
    public void Migrations_StartFromTheBaseline()
    {
        using var context = NewContext();

        context.Database.GetMigrations().First().ShouldEndWith("_Baseline");
    }
}
