// <copyright file="OwnedQueryableLeaseTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Data.Sqlite;
using NSubstitute;

namespace IndTrace.Aggregation.BoundedTests.Repository;

/// <summary>
/// #117 (F1) regression tests for the owned-queryable contract: <c>AsQueryableAsync</c>/<c>FromSqlAsync</c>
/// lease a pooled context whose lifetime must span the caller's deferred composition and materialization.
/// Pre-fix the contract returned a bare <c>IQueryable&lt;T&gt;</c> and the leased context was NEVER disposed —
/// every call permanently consumed a pooled context (and its connection), exhausting the pool under sustained
/// Reports load. These tests pin the new guarantee with a lease-counting factory (SQLite relational seam, same
/// as <see cref="AddRangeBulkAsyncRelationalTests"/> so the raw-SQL <c>FromSqlAsync</c> path is real):
/// exactly one context is leased per call, it stays alive through materialization, and disposing the
/// <c>OwnedQueryable</c> is what returns it to the pool.
/// </summary>
public sealed class OwnedQueryableLeaseTests
{
    /// <summary>
    /// Minimal SQLite DDL for the Variables table: exactly the columns the EF model maps for
    /// <see cref="Variable"/> (including the audit columns), mirroring <see cref="AddRangeBulkAsyncRelationalTests"/>.
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
    /// Initializes a new instance of the <see cref="OwnedQueryableLeaseTests"/> class.
    /// </summary>
    /// <param name="output">The xUnit test output helper.</param>
    public OwnedQueryableLeaseTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    /// <summary>
    /// THE F1 defect scenario: after composing and materializing, disposing the lease must return the leased
    /// context to the pool (created == disposed). Pre-fix there was no disposal seam at all — the context
    /// backing the returned queryable could never be released, so ContextsDisposed stayed 0 forever.
    /// </summary>
    [Fact]
    public async Task AsQueryableAsync_DisposeAfterMaterialization_ReturnsLeasedContext()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionWithVariablesTableAsync(ct);
        var factory = new LeaseCountingSqliteFactory(connection);
        var repository = this.CreateRepository(factory);
        (await repository.AddAsync(NewVariable("LeaseProbe1"), ct)).IsSuccess.ShouldBeTrue();

        var createdBefore = factory.ContextsCreated;
        var disposedBefore = factory.ContextsDisposed;

        var leaseResult = await repository.AsQueryableAsync(ct);
        leaseResult.IsSuccess.ShouldBeTrue(leaseResult.Error);
        leaseResult.Value.ShouldNotBeNull();
        var lease = leaseResult.Value;

        // Exactly one context leased, still live while the caller composes and materializes.
        factory.ContextsCreated.ShouldBe(createdBefore + 1);
        factory.ContextsDisposed.ShouldBe(disposedBefore);

        var rows = await lease.Query.Where(v => v.Name == "LeaseProbe1").ToListAsync(ct);
        rows.Count.ShouldBe(1);
        factory.ContextsDisposed.ShouldBe(disposedBefore); // materialization must not kill the lease

        await lease.DisposeAsync();

        // The lease's disposal — and nothing else — returned the context to the pool.
        factory.ContextsDisposed.ShouldBe(disposedBefore + 1);
        Should.Throw<ObjectDisposedException>(() => lease.Query.ToList());
    }

    /// <summary>
    /// F1 on the specification overload: the spec-filtered queryable rides the same lease contract.
    /// </summary>
    [Fact]
    public async Task AsQueryableAsync_WithSpecification_DisposeReturnsLeasedContext()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionWithVariablesTableAsync(ct);
        var factory = new LeaseCountingSqliteFactory(connection);
        var repository = this.CreateRepository(factory);
        (await repository.AddAsync(NewVariable("LeaseProbeSpec"), ct)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(NewVariable("Unrelated"), ct)).IsSuccess.ShouldBeTrue();

        var createdBefore = factory.ContextsCreated;
        var disposedBefore = factory.ContextsDisposed;
        var spec = new Specification<Variable>(v => v.Name == "LeaseProbeSpec");

        var leaseResult = await repository.AsQueryableAsync(spec, ct);
        leaseResult.IsSuccess.ShouldBeTrue(leaseResult.Error);
        leaseResult.Value.ShouldNotBeNull();

        List<Variable> rows;
        await using (var lease = leaseResult.Value)
        {
            rows = await lease.Query.ToListAsync(ct);
        }

        rows.Count.ShouldBe(1);
        factory.ContextsCreated.ShouldBe(createdBefore + 1);
        factory.ContextsDisposed.ShouldBe(disposedBefore + 1);
    }

    /// <summary>
    /// F1 on the raw-SQL Reports seam (the sanctioned VO-LIKE path, #27/F4): the FromSql-rooted queryable
    /// must stay composable with further LINQ until materialization, and disposing the lease must return the
    /// context. Under sustained Reports load the pre-fix leak on THIS path exhausted the pool.
    /// </summary>
    [Fact]
    public async Task FromSqlAsync_ComposedThenDisposed_ReturnsLeasedContext()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionWithVariablesTableAsync(ct);
        var factory = new LeaseCountingSqliteFactory(connection);
        var repository = this.CreateRepository(factory);
        (await repository.AddAsync(NewVariable("SqlLeaseProbe"), ct)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(NewVariable("SqlOther"), ct)).IsSuccess.ShouldBeTrue();

        var createdBefore = factory.ContextsCreated;
        var disposedBefore = factory.ContextsDisposed;
        var pattern = "SqlLease%";

        var leaseResult = await repository.FromSqlAsync($"SELECT * FROM Variables WHERE Name LIKE {pattern}", ct);
        leaseResult.IsSuccess.ShouldBeTrue(leaseResult.Error);
        leaseResult.Value.ShouldNotBeNull();

        List<Variable> rows;
        await using (var lease = leaseResult.Value)
        {
            // Compose further LINQ over the raw-SQL root, then materialize — the lease spans it all.
            rows = await lease.Query.OrderBy(v => v.Name).ToListAsync(ct);
        }

        rows.Count.ShouldBe(1);
        rows[0].Name.ShouldBe("SqlLeaseProbe");
        factory.ContextsCreated.ShouldBe(createdBefore + 1);
        factory.ContextsDisposed.ShouldBe(disposedBefore + 1);
    }

    /// <summary>
    /// F1 on <c>ReadOnlyRepository</c>: both AsQueryableAsync overloads lease and return exactly one context
    /// each. The cache substitute is irrelevant to these paths (queryable leases are never cached).
    /// </summary>
    [Fact]
    public async Task ReadOnlyRepository_AsQueryableAsync_DisposeReturnsLeasedContext()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionWithVariablesTableAsync(ct);
        var factory = new LeaseCountingSqliteFactory(connection);
        var writer = this.CreateRepository(factory);
        (await writer.AddAsync(NewVariable("RoLeaseProbe"), ct)).IsSuccess.ShouldBeTrue();

        var readOnly = new ReadOnlyRepository<Variable>(
            factory,
            Substitute.For<ICacheService>(),
            XUnitLogger.CreateLogger<ReadOnlyRepository<Variable>>(this.output));

        var createdBefore = factory.ContextsCreated;
        var disposedBefore = factory.ContextsDisposed;

        var plainResult = await readOnly.AsQueryableAsync(ct);
        plainResult.IsSuccess.ShouldBeTrue(plainResult.Error);
        plainResult.Value.ShouldNotBeNull();
        await using (var lease = plainResult.Value)
        {
            (await lease.Query.CountAsync(ct)).ShouldBe(1);
        }

        var spec = new Specification<Variable>(v => v.Name == "RoLeaseProbe");
        var specResult = await readOnly.AsQueryableAsync(spec, ct);
        specResult.IsSuccess.ShouldBeTrue(specResult.Error);
        specResult.Value.ShouldNotBeNull();
        await using (var lease = specResult.Value)
        {
            (await lease.Query.ToListAsync(ct)).Count.ShouldBe(1);
        }

        factory.ContextsCreated.ShouldBe(createdBefore + 2);
        factory.ContextsDisposed.ShouldBe(disposedBefore + 2);
    }

    /// <summary>
    /// F1 hygiene: double-disposing a lease is safe and releases the underlying context exactly once
    /// (the pool must never see a double return).
    /// </summary>
    [Fact]
    public async Task OwnedQueryable_DoubleDispose_ReleasesContextOnce()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionWithVariablesTableAsync(ct);
        var factory = new LeaseCountingSqliteFactory(connection);
        var repository = this.CreateRepository(factory);

        var disposedBefore = factory.ContextsDisposed;
        var leaseResult = await repository.AsQueryableAsync(ct);
        leaseResult.IsSuccess.ShouldBeTrue(leaseResult.Error);
        leaseResult.Value.ShouldNotBeNull();
        var lease = leaseResult.Value;

        await lease.DisposeAsync();
        await lease.DisposeAsync();

        factory.ContextsDisposed.ShouldBe(disposedBefore + 1);
    }

    /// <summary>
    /// F1 guard rail: a pre-lease failure (cancellation) must not lease a context at all — the failure rail
    /// never carries or leaks a lease.
    /// </summary>
    [Fact]
    public async Task AsQueryableAsync_CanceledToken_LeasesNoContext()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionWithVariablesTableAsync(ct);
        var factory = new LeaseCountingSqliteFactory(connection);
        var repository = this.CreateRepository(factory);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var leaseResult = await repository.AsQueryableAsync(cts.Token);

        leaseResult.IsFailure.ShouldBeTrue();
        factory.ContextsCreated.ShouldBe(0);
        factory.ContextsDisposed.ShouldBe(0);
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

    private static Variable NewVariable(string name)
    {
        var created = Variable.Create(
            machineId: 9501,
            plcId: 1,
            name: name,
            description: $"#117 F1 lease-path variable {name}",
            alias: name,
            address: $"DB101.{name}",
            netType: "Int",
            length: 2,
            isActive: ActiveStatus.Active,
            direction: 1,
            variableGroupId: 1);
        created.IsSuccess.ShouldBeTrue(created.Error);
        created.Value.ShouldNotBeNull();
        return created.Value;
    }

    private Repository<Variable> CreateRepository(LeaseCountingSqliteFactory factory) =>
        new(factory, XUnitLogger.CreateLogger<Repository<Variable>>(this.output));

    /// <summary>
    /// A SQLite-backed <see cref="IIndTraceDbContextFactory"/> over ONE shared in-memory connection (same
    /// seam as <see cref="AddRangeBulkAsyncRelationalTests"/>) that counts every context it hands out and
    /// every context that is disposed, so a test can prove leases == returns.
    /// </summary>
    private sealed class LeaseCountingSqliteFactory : IIndTraceDbContextFactory
    {
        private readonly SqliteConnection connection;
        private int contextsCreated;
        private int contextsDisposed;

        public LeaseCountingSqliteFactory(SqliteConnection connection)
        {
            this.connection = connection;
        }

        /// <summary>Gets the number of contexts handed out (leases).</summary>
        public int ContextsCreated => Volatile.Read(ref this.contextsCreated);

        /// <summary>Gets the number of contexts disposed (returns to the pool).</summary>
        public int ContextsDisposed => Volatile.Read(ref this.contextsDisposed);

        public DbContext CreateEfDbContext() => this.NewContext();

        public Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IIndTraceDbContext>(this.NewContext());

        public IIndTraceDbContext CreateDbContext() => this.NewContext();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        private DisposalCountingContext NewContext()
        {
            var options = new DbContextOptionsBuilder<IndTraceDbContext>()
                .UseSqlite(this.connection)
                .Options;
            var context = new DisposalCountingContext(options, this);
            context.SetTestingInterfaces(new TesterUserService(), new DateTimeMachine());
            Interlocked.Increment(ref this.contextsCreated);
            return context;
        }

        /// <summary>
        /// An <see cref="IndTraceDbContext"/> that reports its (first) disposal back to the counting factory,
        /// whether it is disposed asynchronously (the OwnedQueryable path) or synchronously.
        /// </summary>
        private sealed class DisposalCountingContext : IndTraceDbContext
        {
            private readonly LeaseCountingSqliteFactory owner;
            private int reported;

            public DisposalCountingContext(DbContextOptions<IndTraceDbContext> options, LeaseCountingSqliteFactory owner)
                : base(options)
            {
                this.owner = owner;
            }

            public override ValueTask DisposeAsync()
            {
                this.ReportDisposalOnce();
                return base.DisposeAsync();
            }

            public override void Dispose()
            {
                this.ReportDisposalOnce();
                base.Dispose();
            }

            private void ReportDisposalOnce()
            {
                if (Interlocked.Exchange(ref this.reported, 1) == 0)
                {
                    Interlocked.Increment(ref this.owner.contextsDisposed);
                }
            }
        }
    }
}
