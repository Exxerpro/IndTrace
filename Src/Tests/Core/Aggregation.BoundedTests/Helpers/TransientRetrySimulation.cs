// <copyright file="TransientRetrySimulation.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace IndTrace.Aggregation.BoundedTests.Helpers;

/// <summary>
/// #117 (F5/F6a) retry-semantics test double: the ONE exception type
/// <see cref="TestRetryingExecutionStrategy"/> treats as transient. Real transient faults are SQL Server
/// <c>SqlException</c>s the test providers (InMemory/SQLite) can never produce, so the tests inject THIS
/// exception from a <see cref="FailNextSaveChangesInterceptor"/> instead and retry on it specifically —
/// any other exception still propagates un-retried, exactly like the production strategy.
/// </summary>
public sealed class TestTransientException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestTransientException"/> class.
    /// </summary>
    public TestTransientException()
        : base("Simulated transient database fault (#117 retry-semantics tests).")
    {
    }
}

/// <summary>
/// #117 (F5/F6a) fault injector: throws <see cref="TestTransientException"/> from <c>SavingChanges</c> while
/// <see cref="FailuresRemaining"/> is positive (decrementing per throw), and counts every <c>SaveChanges</c>
/// attempt so a test can prove a retry actually happened. Throwing from the SavingChanges interception point
/// fails the attempt BEFORE anything reaches the store — the only failure point the non-transactional test
/// providers can recover from on a retry.
/// </summary>
public sealed class FailNextSaveChangesInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Gets or sets the number of upcoming <c>SaveChanges</c> attempts that will fail with a
    /// <see cref="TestTransientException"/>. Arm with 1 to fail exactly the next attempt.
    /// </summary>
    public int FailuresRemaining { get; set; }

    /// <summary>
    /// Gets the total number of <c>SaveChanges</c> attempts observed (failed and successful).
    /// </summary>
    public int SavingChangesCalls { get; private set; }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        this.CountAndMaybeThrow();
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        this.CountAndMaybeThrow();
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CountAndMaybeThrow()
    {
        this.SavingChangesCalls++;
        if (this.FailuresRemaining > 0)
        {
            this.FailuresRemaining--;
            throw new TestTransientException();
        }
    }
}

/// <summary>
/// #126 (C3/C4) POST-FLUSH fault injector: lets <c>SaveChanges</c> succeed and then throws
/// <see cref="TestTransientException"/> from the transaction COMMIT interception point while
/// <see cref="FailuresRemaining"/> is positive (decrementing per throw). This models the real transient
/// window the SavingChanges-time injector cannot reach: the flush has already written (and, pre-fix,
/// ACCEPTED) the tracked changes inside the transaction, and the fault strikes between the flush and the
/// durable commit — so the rolled-back attempt leaves whatever tracker/instance state the flush produced.
/// Counts every commit attempt so a test can prove the strategy really re-ran the retriable unit.
/// </summary>
public sealed class FailNextTransactionCommitInterceptor : DbTransactionInterceptor
{
    /// <summary>
    /// Gets or sets the number of upcoming commit attempts that will fail with a
    /// <see cref="TestTransientException"/>. Arm with 1 to fail exactly the next commit.
    /// </summary>
    public int FailuresRemaining { get; set; }

    /// <summary>
    /// Gets the total number of commit attempts observed (failed and successful).
    /// </summary>
    public int CommitAttempts { get; private set; }

    /// <inheritdoc/>
    public override InterceptionResult TransactionCommitting(
        System.Data.Common.DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
    {
        this.CountAndMaybeThrow();
        return base.TransactionCommitting(transaction, eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult> TransactionCommittingAsync(
        System.Data.Common.DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        this.CountAndMaybeThrow();
        return base.TransactionCommittingAsync(transaction, eventData, result, cancellationToken);
    }

    private void CountAndMaybeThrow()
    {
        this.CommitAttempts++;
        if (this.FailuresRemaining > 0)
        {
            this.FailuresRemaining--;
            throw new TestTransientException();
        }
    }
}

/// <summary>
/// #117 (F5/F6a) retrying execution strategy for tests: retries the retriable unit on
/// <see cref="TestTransientException"/> only (up to 2 retries, negligible delay). Deriving from EF's
/// <see cref="ExecutionStrategy"/> base gives the REAL production retry mechanics — including the
/// suspended-strategy bookkeeping that permits the user-initiated transaction inside the retriable unit —
/// with only the transient-fault classification swapped for the injectable test exception.
/// </summary>
public sealed class TestRetryingExecutionStrategy : ExecutionStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestRetryingExecutionStrategy"/> class.
    /// </summary>
    /// <param name="dependencies">The EF execution-strategy dependencies.</param>
    public TestRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : base(dependencies, maxRetryCount: 2, maxRetryDelay: TimeSpan.FromMilliseconds(10))
    {
    }

    /// <inheritdoc/>
    protected override bool ShouldRetryOn(Exception exception) => exception is TestTransientException;
}

/// <summary>
/// Factory that makes <see cref="TestRetryingExecutionStrategy"/> the context's strategy — wire it with
/// <c>optionsBuilder.ReplaceService&lt;IExecutionStrategyFactory, TestRetryingExecutionStrategyFactory&gt;()</c>
/// so the production code's <c>Database.CreateExecutionStrategy()</c> seam picks it up unchanged.
/// </summary>
public sealed class TestRetryingExecutionStrategyFactory : IExecutionStrategyFactory
{
    private readonly ExecutionStrategyDependencies dependencies;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestRetryingExecutionStrategyFactory"/> class.
    /// </summary>
    /// <param name="dependencies">The EF execution-strategy dependencies (injected by EF's internal provider).</param>
    public TestRetryingExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
    {
        this.dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
    }

    /// <inheritdoc/>
    public IExecutionStrategy Create() => new TestRetryingExecutionStrategy(this.dependencies);
}

/// <summary>
/// A throwaway InMemory <see cref="IIndTraceDbContextFactory"/> (same seam as
/// <c>CacheInvalidationOnWriteTests.InMemoryContextFactory</c>) whose contexts additionally carry the #117
/// retry-simulation services: every context shares ONE named InMemory database, runs the given
/// <see cref="FailNextSaveChangesInterceptor"/>, and resolves <see cref="TestRetryingExecutionStrategy"/>
/// from <c>Database.CreateExecutionStrategy()</c> so the aggregate repositories' retriable lambdas really
/// re-execute after an injected transient fault.
/// </summary>
public sealed class RetrySimulatingInMemoryContextFactory : IIndTraceDbContextFactory
{
    private readonly string databaseName = Guid.NewGuid().ToString();
    private readonly FailNextSaveChangesInterceptor interceptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetrySimulatingInMemoryContextFactory"/> class.
    /// </summary>
    /// <param name="interceptor">The armable transient-fault injector shared by every created context.</param>
    public RetrySimulatingInMemoryContextFactory(FailNextSaveChangesInterceptor interceptor)
    {
        this.interceptor = interceptor ?? throw new ArgumentNullException(nameof(interceptor));
    }

    /// <inheritdoc/>
    public DbContext CreateEfDbContext() => this.NewContext();

    /// <inheritdoc/>
    public Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IIndTraceDbContext>(this.NewContext());

    /// <inheritdoc/>
    public IIndTraceDbContext CreateDbContext() => this.NewContext();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private IndTraceDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseInMemoryDatabase(this.databaseName)
            .AddInterceptors(this.interceptor)
            .ReplaceService<IExecutionStrategyFactory, TestRetryingExecutionStrategyFactory>()

            // #41: the aggregate repositories ALWAYS open a real transaction; InMemory cannot honour it and by
            // default THROWS TransactionIgnoredWarning — ignore it so InMemory returns the no-op transaction.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new IndTraceDbContext(options);
    }
}
