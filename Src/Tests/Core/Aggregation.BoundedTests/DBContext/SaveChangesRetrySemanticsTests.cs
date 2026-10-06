// <copyright file="SaveChangesRetrySemanticsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace IndTrace.Aggregation.BoundedTests.DBContext;

/// <summary>
/// #117 (F6a) retry-semantics tests for <see cref="IndTraceDbContext.SaveChangesAsync(string, CancellationToken)"/>:
/// a transient fault inside the retriable unit must PROPAGATE to the execution strategy (which retries it),
/// while the method boundary still always returns a <see cref="Result{T}"/> — no exception escapes.
/// Pre-fix, a catch INSIDE the retriable lambda converted the fault to a Result failure, so the strategy never
/// saw an exception and <c>EnableRetryOnFailure</c> was dead code on this path.
/// </summary>
/// <remarks>
/// The method early-returns on non-relational providers, so InMemory cannot exercise it; SQLite (in-memory
/// connection) provides the relational pipeline instead. SQLite cannot execute the SQL Server-only
/// <c>SET IDENTITY_INSERT</c> statements, so a command interceptor suppresses exactly those — everything else
/// (the strategy, the transaction, the flush, the Result boundary) is the real production path. The transient
/// fault is injected via <see cref="FailNextSaveChangesInterceptor"/> and retried by
/// <see cref="TestRetryingExecutionStrategy"/> (real EF retry mechanics, test-only fault classification),
/// resolved through the production <c>Database.CreateExecutionStrategy()</c> seam.
/// </remarks>
public class SaveChangesRetrySemanticsTests
{
    /// <summary>
    /// SQLite cannot run the SQL Server-only <c>SET IDENTITY_INSERT</c> session statements; suppress exactly
    /// those commands (result 0) so the rest of the relational pipeline runs for real.
    /// </summary>
    private sealed class SuppressIdentityInsertCommandInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            return IsIdentityInsert(command) ? InterceptionResult<int>.SuppressWithResult(0) : result;
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            return IsIdentityInsert(command)
                ? ValueTask.FromResult(InterceptionResult<int>.SuppressWithResult(0))
                : base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        private static bool IsIdentityInsert(DbCommand command) =>
            command.CommandText.StartsWith("SET IDENTITY_INSERT", StringComparison.Ordinal);
    }

    /// <summary>
    /// Minimal SQLite DDL for the Variables table (exactly the columns the EF model maps for
    /// <see cref="Variable"/>, including the audit columns) so the #126 C3 post-flush tests can track a REAL
    /// row through the retriable unit without creating the huge SQL Server model on SQLite. Mirrors
    /// <c>AddRangeBulkAsyncRelationalTests</c>.
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

    private static async Task<(IndTraceDbContext Context, FailNextSaveChangesInterceptor Interceptor, SqliteConnection Connection)>
        CreateRelationalRetryContextAsync(CancellationToken ct)
    {
        var (context, saveInterceptor, _, connection) = await CreateRelationalRetryContextWithCommitFaultAsync(ct);
        return (context, saveInterceptor, connection);
    }

    private static async Task<(
        IndTraceDbContext Context,
        FailNextSaveChangesInterceptor SaveInterceptor,
        FailNextTransactionCommitInterceptor CommitInterceptor,
        SqliteConnection Connection)>
        CreateRelationalRetryContextWithCommitFaultAsync(CancellationToken ct)
    {
        var saveInterceptor = new FailNextSaveChangesInterceptor();
        var commitInterceptor = new FailNextTransactionCommitInterceptor();
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(ct);

        await using (var ddl = connection.CreateCommand())
        {
            ddl.CommandText = VariablesTableDdl;
            await ddl.ExecuteNonQueryAsync(ct);
        }

        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(saveInterceptor, commitInterceptor, new SuppressIdentityInsertCommandInterceptor())
            .ReplaceService<IExecutionStrategyFactory, TestRetryingExecutionStrategyFactory>()
            .Options;

        var context = new IndTraceDbContext(options);
        context.SetTestingInterfaces(new TesterUserService(), new DateTimeMachine());
        return (context, saveInterceptor, commitInterceptor, connection);
    }

    private static Variable NewVariable(string name)
    {
        var created = Variable.Create(
            machineId: 9601,
            plcId: 1,
            name: name,
            description: $"#126 C3 post-flush variable {name}",
            alias: name,
            address: $"DB126.{name}",
            netType: "Int",
            length: 2,
            isActive: ActiveStatus.Active,
            direction: 1,
            variableGroupId: 1);
        created.IsSuccess.ShouldBeTrue(created.Error);
        return created.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// THE F6a defect scenario: a transient fault thrown by the flush inside the retriable unit reaches the
    /// strategy — which retries — and the retried attempt succeeds, so the caller receives SUCCESS. Pre-fix,
    /// the in-lambda catch converted the first fault straight into a returned failure and the strategy never
    /// retried (a single attempt, a failed Result).
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_TransientFaultInsideRetriableUnit_IsRetriedToSuccess()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var (context, interceptor, connection) = await CreateRelationalRetryContextAsync(ct);
        await using var contextScope = context;
        await using var connectionScope = connection;

        // Act — arm ONE transient fault; no entities are tracked, so a retried flush is a clean no-op write.
        interceptor.FailuresRemaining = 1;
        var result = await context.SaveChangesAsync("Machines", ct);

        // Assert — the strategy really retried (1 failed + 1 successful attempt) and the caller sees success.
        result.IsSuccess.ShouldBeTrue(result.Error);
        result.Value.ShouldBe(0);
        interceptor.SavingChangesCalls.ShouldBe(2);
    }

    /// <summary>
    /// A fault that survives the whole retry budget still surfaces as the same <see cref="Result{T}"/> failure
    /// carrying the ORIGINAL fault's message (not the strategy's retry-limit wrapper) — and no exception
    /// escapes the method boundary. The strategy ran its full budget: 1 initial attempt + 2 retries.
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_TransientFaultExhaustsRetries_ReturnsOriginalFailureMessage()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var (context, interceptor, connection) = await CreateRelationalRetryContextAsync(ct);
        await using var contextScope = context;
        await using var connectionScope = connection;

        // Act — arm more faults than the retry budget (TestRetryingExecutionStrategy allows 3 attempts).
        interceptor.FailuresRemaining = 5;
        var result = await context.SaveChangesAsync("Machines", ct);

        // Assert — a Result failure with the original transient message; the full retry budget was spent.
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("Simulated transient database fault");
        interceptor.SavingChangesCalls.ShouldBe(3);
    }

    /// <summary>
    /// THE #126 C3 defect scenario: the flush inside the retriable unit SUCCEEDS, then the COMMIT throws a
    /// transient fault. Pre-fix, the flush ran with <c>acceptAllChangesOnSuccess: true</c>, so the failed
    /// attempt ACCEPTED the tracked changes before the rollback undid the INSERT — the retried attempt found
    /// an EMPTY tracker, flushed 0 rows, committed, and reported <c>Success(0)</c> while the row was silently
    /// LOST. Post-fix the tracker keeps the pending changes across the rolled-back attempt (acceptance is
    /// deferred until after the commit), so the retry re-flushes and the row is actually persisted.
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_TransientCommitFaultAfterFlush_RetriedAttemptPersistsTheRow()
    {
        // Arrange — a real Variables table so the retriable unit tracks and flushes an actual row.
        var ct = TestContext.Current.CancellationToken;
        var (context, saveInterceptor, commitInterceptor, connection) = await CreateRelationalRetryContextWithCommitFaultAsync(ct);
        await using var contextScope = context;
        await using var connectionScope = connection;

        context.Set<Variable>().Add(NewVariable("C3-CommitFault"));

        // Act — arm ONE post-flush commit fault: attempt 1 flushes the INSERT, the commit throws, the
        // transaction rolls back, and the strategy re-executes the retriable unit.
        commitInterceptor.FailuresRemaining = 1;
        var result = await context.SaveChangesAsync("Variables", ct);

        // Assert — NEVER Success-with-the-row-absent: the retried attempt re-flushed and persisted the row.
        result.IsSuccess.ShouldBeTrue(result.Error);
        result.Value.ShouldBe(1, "the retried attempt must re-flush the tracked row, not commit an empty tracker.");
        commitInterceptor.CommitAttempts.ShouldBe(2);
        saveInterceptor.SavingChangesCalls.ShouldBe(2, "each attempt must flush the still-pending change.");

        (await context.Set<Variable>().AsNoTracking().CountAsync(v => v.Name == "C3-CommitFault", ct))
            .ShouldBe(1, "the row must actually be present after the reported success.");
    }

    /// <summary>
    /// #126 C3 guard invariant: when the post-flush commit fault survives the WHOLE retry budget, the caller
    /// gets a LOUD failure and the row is absent — never <c>Success</c> disguising the lost write. (Pre-fix
    /// the second attempt would commit an EMPTY tracker and report <c>Success(0)</c>.)
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_CommitFaultExhaustsRetries_FailsLoudly_NeverSilentSuccess()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var (context, _, commitInterceptor, connection) = await CreateRelationalRetryContextWithCommitFaultAsync(ct);
        await using var contextScope = context;
        await using var connectionScope = connection;

        context.Set<Variable>().Add(NewVariable("C3-Exhausted"));

        // Act — arm more commit faults than the retry budget (TestRetryingExecutionStrategy allows 3 attempts).
        commitInterceptor.FailuresRemaining = 5;
        var result = await context.SaveChangesAsync("Variables", ct);

        // Assert — a loud failure carrying the original transient message; no phantom success, no phantom row.
        result.IsFailure.ShouldBeTrue("a commit fault that outlives the retry budget must surface as a failure.");
        result.Error.ShouldContain("Simulated transient database fault");
        (await context.Set<Variable>().AsNoTracking().CountAsync(v => v.Name == "C3-Exhausted", ct))
            .ShouldBe(0, "every attempt rolled back, so no row may exist.");
    }
}
