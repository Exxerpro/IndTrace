// <copyright file="ProductRoutingRepositoryRetryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Domain.Routing;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Products.Services;

/// <summary>
/// #117 (F5) retry-idempotency tests for <see cref="ProductRoutingRepository.SaveAsync"/>: the two-flush
/// lambda passed to <c>CreateExecutionStrategy().ExecuteAsync</c> must be safe to RE-EXECUTE after a
/// transient fault. A <see cref="TestRetryingExecutionStrategy"/> (real EF retry mechanics, test-only fault
/// classification) really re-runs the lambda after a <see cref="FailNextSaveChangesInterceptor"/> fails the
/// first attempt, and the whole-route replace must still land exactly once — the new route, no duplicated
/// nodes/edges, no leftovers.
/// </summary>
/// <remarks>
/// HARNESS LIMIT (stated per the #117 test plan): the InMemory provider cannot roll back a flushed attempt,
/// so the fault is injected at the SavingChanges interception point of the FIRST flush — the attempt fails
/// BEFORE anything reaches the store, which is the only failure point InMemory can recover from on a retry.
/// The flushed-then-rolled-back re-staging hazards (delete batch accepted by the tracker, identity keys
/// assigned to the insert batch) are real-SQL concerns NOT reachable in this harness — they are covered by
/// the #126 C4 integration proof (<c>Integration.Tests.Routing.ProductRoutingCommitRetryTests</c>), which
/// faults the COMMIT after both real flushes on live SQL.
/// </remarks>
public class ProductRoutingRepositoryRetryTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IDateTimeMachine _clock = new DateTimeMachine();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRoutingRepositoryRetryTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public ProductRoutingRepositoryRetryTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    /// <summary>
    /// A transient fault on the first attempt of a whole-route replace is retried by the execution strategy
    /// and the retried attempt persists the replacement EXACTLY ONCE: the save succeeds, the strategy really
    /// re-executed the lambda, and a fresh load shows precisely the new four-node route — the delete batch
    /// and insert batch were re-staged coherently, with no duplicates and no old-route leftovers.
    /// </summary>
    [Fact]
    public async Task SaveAsync_TransientFaultOnFirstAttempt_RetriesAndReplacesExactlyOnce()
    {
        // Arrange — a retry-simulating factory: shared InMemory database + armable fault injector + a REAL
        // retrying strategy resolved through the repository's CreateExecutionStrategy() seam.
        var ct = TestContext.Current.CancellationToken;
        const int productId = 9601;
        var interceptor = new FailNextSaveChangesInterceptor();
        var factory = new RetrySimulatingInMemoryContextFactory(interceptor);
        var repository = new ProductRoutingRepository(
            factory, XUnitLogger.CreateLogger<ProductRoutingRepository>(_outputHelper));

        // Seed an existing route (unarmed) so the replace exercises BOTH batches: delete AND insert.
        var first = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        first.IsSuccess.ShouldBeTrue();
        var firstRouting = first.Value.ShouldNotBeNull();
        firstRouting.ReplaceWith(new[] { 100, 400, 500 }, ruleNumber: 2005, authoredBy: "issue117-TEST", _clock)
            .IsSuccess.ShouldBeTrue();
        (await repository.SaveAsync(firstRouting, ct)).IsSuccess.ShouldBeTrue();

        // Reload: the loaded rows become the delete set, the new route the insert set.
        var second = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        second.IsSuccess.ShouldBeTrue();
        var secondRouting = second.Value.ShouldNotBeNull();
        secondRouting.DeletedNodes.Count.ShouldBe(3);
        secondRouting.ReplaceWith(new[] { 100, 400, 500, 600 }, ruleNumber: 2005, authoredBy: "issue117-TEST", _clock)
            .IsSuccess.ShouldBeTrue();

        // Act — arm ONE transient fault: the first attempt fails at its first flush (before the store), the
        // strategy must re-execute the lambda, and the retried attempt must re-stage both batches cleanly.
        interceptor.FailuresRemaining = 1;
        var attemptsBefore = interceptor.SavingChangesCalls;
        var saved = await repository.SaveAsync(secondRouting, ct);

        // Assert — the save succeeded AND a retry really happened: 3 flush attempts total = 1 failed
        // (attempt one's delete flush) + 2 successful (attempt two's delete flush and insert flush).
        saved.IsSuccess.ShouldBeTrue(saved.Error);
        interceptor.FailuresRemaining.ShouldBe(0);
        (interceptor.SavingChangesCalls - attemptsBefore).ShouldBe(3);

        // Assert — exactly the new route survives: 4 nodes / 3 edges, no duplicates, no old-route leftovers.
        var reloaded = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        reloaded.IsSuccess.ShouldBeTrue();
        var reloadedRouting = reloaded.Value.ShouldNotBeNull();

        var nodes = reloadedRouting.DeletedNodes.OrderBy(n => n.MachineId).ToList();
        nodes.Count.ShouldBe(4);
        nodes.Select(n => n.MachineId.Value).ShouldBe([100, 400, 500, 600]);

        var edges = reloadedRouting.DeletedEdges.OrderBy(e => e.LastMachineId).ToList();
        edges.Count.ShouldBe(3);
        edges.ShouldContain(e => e.LastMachineId == new MachineId(500) && e.NextMachineId == new MachineId(600));
    }
}
