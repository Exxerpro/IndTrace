// <copyright file="BarCodeAggregateRepositoryRetryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Domain.Routing;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Services;

/// <summary>
/// #117 (F5) retry-idempotency tests for <see cref="BarCodeAggregateRepository.SaveAsync"/>: the lambda
/// passed to <c>CreateExecutionStrategy().ExecuteAsync</c> must be safe to RE-EXECUTE after a transient
/// fault. A <see cref="TestRetryingExecutionStrategy"/> (real EF retry mechanics, test-only fault
/// classification) really re-runs the lambda after a <see cref="FailNextSaveChangesInterceptor"/> fails the
/// first attempt, and the save must still complete exactly once — no duplicate registers/markers, no
/// tracker-state corruption carried between attempts.
/// </summary>
/// <remarks>
/// HARNESS LIMIT (stated per the #117 test plan): the InMemory provider cannot roll back a flushed attempt,
/// so the fault is injected at the SavingChanges interception point — the attempt fails BEFORE anything
/// reaches the store, which is the only failure point InMemory can recover from on a retry. The
/// flushed-then-rolled-back re-staging hazards (accepted change tracker, assigned identity keys) are real-SQL
/// concerns NOT reachable in this harness — they are covered by the #126 C4 integration proof
/// (<c>Integration.Tests.BarCodes.BarCodeAggregateCommitRetryTests</c>), which faults the COMMIT after a real
/// flush on live SQL.
/// </remarks>
public class BarCodeAggregateRepositoryRetryTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 910001;
    private const int CycleId = 910002;

    private readonly ITestOutputHelper _outputHelper;
    private readonly IFlowStatusCalculator _flowStatusCalculator = new FlowStatusCalculator();
    private readonly IDateTimeMachine _clock = new DateTimeMachine();

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeAggregateRepositoryRetryTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public BarCodeAggregateRepositoryRetryTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    // A linear graph that contains the processing machine (100 -> 200 -> 0).
    private static ProductionGraph Graph()
    {
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(MachineId, 200, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(200, 0, WorkFlowType.From(WorkFlowType.Serial | WorkFlowType.Final)),
        ];
        var result = ProductionGraph.Create(transitions);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// A transient fault on the first save attempt is retried by the execution strategy and the retried
    /// attempt persists the completion EXACTLY ONCE: the save succeeds, the strategy really ran two attempts,
    /// and a verification read shows one register, one completion marker and the cycle FinishedOk — no
    /// duplicates and no tracker-state poison from the failed attempt.
    /// </summary>
    [Fact]
    public async Task SaveAsync_TransientFaultOnFirstAttempt_RetriesAndPersistsExactlyOnce()
    {
        // Arrange — a retry-simulating factory: shared InMemory database + armable fault injector + a REAL
        // retrying strategy resolved through the repository's CreateExecutionStrategy() seam.
        var ct = TestContext.Current.CancellationToken;
        var interceptor = new FailNextSaveChangesInterceptor();
        var factory = new RetrySimulatingInMemoryContextFactory(interceptor);
        var repository = new BarCodeAggregateRepository(
            factory, XUnitLogger.CreateLogger<BarCodeAggregateRepository>(_outputHelper));

        var startedOn = _clock.Now.AddDays(-1);
        await SeedBarCodeWithStartedCycleAsync(factory, startedOn, ct);

        var recipe = Recipe.Create(0, 0, 1, 200_000, 3, 5, 1).Value.ShouldNotBeNull();
        var registers = new List<Register>
        {
            Register.Create("R1", string.Empty, MachineId, 0, CycleId, "v", "int", 1, _clock.Now).Value.ShouldNotBeNull(),
        };

        var loaded = await repository.LoadAsync(BarCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();
        var cycle = root.LoadedCycles.Single(c => c.CycleId == new CycleId(CycleId));

        var applied = root.CompleteOkCycle(
            cycle, MachineId, MachineType.Final, recipe, registers, root.LoadedCycles, _flowStatusCalculator, Graph(), _clock);
        applied.IsSuccess.ShouldBeTrue();

        // Act — arm ONE transient fault: the first save attempt fails before reaching the store, the strategy
        // must re-execute the lambda, and the second attempt must stage from a clean slate and succeed.
        interceptor.FailuresRemaining = 1;
        var attemptsBefore = interceptor.SavingChangesCalls;
        var saved = await repository.SaveAsync(root, ct);

        // Assert — the save succeeded AND a retry really happened (one failed + one successful attempt).
        saved.IsSuccess.ShouldBeTrue(saved.Error);
        interceptor.FailuresRemaining.ShouldBe(0);
        (interceptor.SavingChangesCalls - attemptsBefore).ShouldBe(2);

        // Assert — exactly-once persistence: one register, one completion marker, the cycle FinishedOk.
        await using var verifyCtx = await factory.CreateDbContextAsync(ct);
        (await verifyCtx.Set<Register>().CountAsync(r => r.CycleId == new CycleId(CycleId), ct)).ShouldBe(1);
        (await verifyCtx.Set<CycleCompletion>().CountAsync(m => m.CycleId == new CycleId(CycleId), ct)).ShouldBe(1);
        var persistedCycle = await verifyCtx.Set<Cycle>().SingleAsync(c => c.CycleId == new CycleId(CycleId), ct);
        persistedCycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
    }

    private static async Task SeedBarCodeWithStartedCycleAsync(
        RetrySimulatingInMemoryContextFactory factory, DateTime startedOn, CancellationToken ct)
    {
        await using var ctx = factory.CreateEfDbContext();

        var barCode = BarCode.Create("AGG-117-F5", productId: 0, machineId: MachineId, createdOn: startedOn, modifiedOn: startedOn);
        barCode.BarCodeId = new BarCodeId(BarCodeId);
        ctx.Set<BarCode>().Add(barCode);

        var cycle = Cycle.CreateStarted(MachineId, BarCodeId, cyclesOk: 0, startedOn, finishedOn: startedOn);
        cycle.CycleId = new CycleId(CycleId);
        ctx.Set<Cycle>().Add(cycle);

        await ctx.SaveChangesAsync(ct);
    }
}
