// <copyright file="AddRangeBulkAsyncRelationalTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Data.Sqlite;

namespace IndTrace.Aggregation.BoundedTests.Repository;

/// <summary>
/// #117 (F2) regression tests for <see cref="Repository{T}.AddRangeBulkAsync"/> on a RELATIONAL provider.
/// Pre-fix the method streamed raw CLR property values to <c>SqlBulkCopy</c> via a reflected
/// <c>DataTable</c>, bypassing <c>IndTraceDbContext.SaveChangesAsync</c> entirely — so the audit stamper
/// never ran (NULL/default <c>CreatedBy</c>/<c>CreatedOn</c>/<c>ModifiedOn</c>), EF value converters
/// (smart enums / value objects) and <c>HasColumnName</c> mappings were not applied, and the copy was
/// hard-coupled to SQL Server (<c>(SqlConnection)</c> cast). The InMemory test provider was special-cased
/// onto the safe EF path, so this defect was invisible to every InMemory-backed test; SQLite (relational
/// in-memory, same seam as <c>SaveChangesRetrySemanticsTests</c>) exercises the REAL non-InMemory branch.
/// </summary>
public sealed class AddRangeBulkAsyncRelationalTests
{
    /// <summary>
    /// Minimal SQLite DDL for the Variables table: exactly the columns the EF model maps for
    /// <see cref="Variable"/> (including the audit columns), so the production INSERT works while the rest
    /// of the huge SQL Server model does not need to be created on SQLite.
    /// </summary>
    private const string VariablesTableDdl = """
        CREATE TABLE "Variables" (
            "VariableId" INTEGER NOT NULL CONSTRAINT "PK_Variables" PRIMARY KEY AUTOINCREMENT,
            "MachineId" INTEGER NOT NULL,
            "PlcId" INTEGER NOT NULL,
            "Name" TEXT NOT NULL,
            "Description" TEXT NOT NULL,
            "Alias" TEXT NOT NULL,
            "Address" TEXT NOT NULL,
            "NetType" TEXT NOT NULL,
            "Length" INTEGER NOT NULL,
            "IsActive" INTEGER NOT NULL,
            "Direction" INTEGER NOT NULL,
            "VariableGroupId" INTEGER NOT NULL,
            "CreatedBy" TEXT NOT NULL,
            "ModifiedBy" TEXT NOT NULL,
            "CreatedOn" TEXT NOT NULL,
            "ModifiedOn" TEXT NULL
        );
        """;

    private readonly ITestOutputHelper output;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRangeBulkAsyncRelationalTests"/> class.
    /// </summary>
    /// <param name="output">The xUnit test output helper.</param>
    public AddRangeBulkAsyncRelationalTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    /// <summary>
    /// A SQLite-backed <see cref="IIndTraceDbContextFactory"/> over ONE shared in-memory connection, so the
    /// write through one context is visible to the read through the next (like the pooled production
    /// factory). Every context carries the deterministic test user service so the audit stamper's output is
    /// assertable.
    /// </summary>
    private sealed class SqliteContextFactory : IIndTraceDbContextFactory
    {
        private readonly SqliteConnection connection;

        public SqliteContextFactory(SqliteConnection connection)
        {
            this.connection = connection;
        }

        public DbContext CreateEfDbContext() => this.NewContext();

        public Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IIndTraceDbContext>(this.NewContext());

        public IIndTraceDbContext CreateDbContext() => this.NewContext();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        private IndTraceDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<IndTraceDbContext>()
                .UseSqlite(this.connection)
                .Options;
            var context = new IndTraceDbContext(options);
            context.SetTestingInterfaces(new TesterUserService(), new DateTimeMachine());
            return context;
        }
    }

    private static async Task<SqliteConnection> OpenConnectionWithVariablesTableAsync(CancellationToken ct)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = VariablesTableDdl;
        await command.ExecuteNonQueryAsync(ct);

        return connection;
    }

    private static Variable NewVariable(string name, ActiveStatus isActive)
    {
        var created = Variable.Create(
            machineId: 9401,
            plcId: 1,
            name: name,
            description: $"#117 F2 bulk-path variable {name}",
            alias: name,
            address: $"DB100.{name}",
            netType: "Int",
            length: 2,
            isActive: isActive,
            direction: 1,
            variableGroupId: 1);
        created.IsSuccess.ShouldBeTrue(created.Error);
        created.Value.ShouldNotBeNull();
        return created.Value;
    }

    private Repository<Variable> CreateRepository(SqliteContextFactory factory) =>
        new(factory, XUnitLogger.CreateLogger<Repository<Variable>>(this.output));

    /// <summary>
    /// THE F2 defect scenario: on a relational (non-InMemory) provider, <c>AddRangeBulkAsync</c> must run
    /// through the EF save pipeline so the audit stamper stamps <c>CreatedBy</c>/<c>CreatedOn</c> (and
    /// <c>ModifiedOn</c> equal at birth). Pre-fix the SqlBulkCopy path never invoked
    /// <c>SaveChangesAsync</c>, so no audit column was ever stamped (and the SQL Server-only connection
    /// cast made every other relational provider fail outright).
    /// </summary>
    [Fact]
    public async Task AddRangeBulkAsync_OnRelationalProvider_StampsCreationAudit()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionWithVariablesTableAsync(ct);
        var factory = new SqliteContextFactory(connection);
        var repository = this.CreateRepository(factory);

        var variables = new[] { NewVariable("BulkAudit1", ActiveStatus.Active), NewVariable("BulkAudit2", ActiveStatus.Active) };

        // Act
        var result = await repository.AddRangeBulkAsync(variables, ct);

        // Assert — the batch succeeded and every row carries a stamped creation audit.
        result.IsSuccess.ShouldBeTrue(result.Error);
        result.Value.ShouldBe(variables.Length);

        await using var readContext = factory.CreateEfDbContext();
        var persisted = await readContext.Set<Variable>().AsNoTracking().OrderBy(v => v.Name).ToListAsync(ct);
        persisted.Count.ShouldBe(variables.Length);
        foreach (var variable in persisted)
        {
            variable.CreatedBy.ShouldBe("Admin");
            variable.CreatedOn.ShouldNotBeNull();
            variable.ModifiedOn.ShouldNotBeNull();
            variable.ModifiedOn.ShouldBe(variable.CreatedOn); // created == modified at birth
        }
    }

    /// <summary>
    /// F2 companion: the EF value converter and <c>HasColumnName</c> mappings must be applied on the bulk
    /// path — the <see cref="ActiveStatus"/> smart enum must persist as its int column value and
    /// round-trip back. Pre-fix the reflected DataTable carried the raw CLR <see cref="ActiveStatus"/>
    /// object (no converter), so any non-scalar property either failed the copy or wrote wrong bytes.
    /// </summary>
    [Fact]
    public async Task AddRangeBulkAsync_OnRelationalProvider_AppliesValueConverter()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionWithVariablesTableAsync(ct);
        var factory = new SqliteContextFactory(connection);
        var repository = this.CreateRepository(factory);

        // Act
        var result = await repository.AddRangeBulkAsync([NewVariable("BulkConverter", ActiveStatus.Active)], ct);

        // Assert — the stored column holds the CONVERTED int value, and EF materializes it back to Active.
        result.IsSuccess.ShouldBeTrue(result.Error);

        await using (var rawCommand = connection.CreateCommand())
        {
            rawCommand.CommandText = "SELECT \"IsActive\" FROM \"Variables\" WHERE \"Name\" = 'BulkConverter'";
            var rawValue = await rawCommand.ExecuteScalarAsync(ct);
            rawValue.ShouldNotBeNull();
            Convert.ToInt32(rawValue, CultureInfo.InvariantCulture).ShouldBe(ActiveStatus.Active.Value);
        }

        await using var readContext = factory.CreateEfDbContext();
        var persisted = await readContext.Set<Variable>().AsNoTracking().SingleAsync(v => v.Name == "BulkConverter", ct);
        persisted.IsActive.ShouldBe(ActiveStatus.Active);
    }
}
