// <copyright file="BarCodeAggregateRepositoryTests.cs" company="Exxerpro Solutions SA de CV">
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
/// Aggregation tests (real <see cref="BarCodeAggregateRepository"/> over EF-Core InMemory via
/// <see cref="DependenciesFactory"/>) for the #40 Chunk 40-C operation-scoped unit of work: a functional
/// round-trip of <c>LoadAsync(ForMachineWindow) → CompleteOkCycle → SaveAsync → LoadAsync</c>.
/// </summary>
/// <remarks>
/// EF-Core InMemory ignores the <c>rowversion</c> concurrency token, the <c>UNIQUE(CycleId)</c> constraint AND
/// the explicit transaction (it returns a no-op transaction), so this proves the FUNCTIONAL persistence path
/// only — the aggregate loads its machine-windowed cycles, applies an OK completion, and the single-flush
/// <see cref="BarCodeAggregateRepository.SaveAsync"/> persists the in-place cycle/barcode update, the appended
/// registers and the completion marker. The atomicity / concurrency (rowversion) / idempotency
/// (<c>UNIQUE(CycleId)</c>) proofs are Chunk 40-D on real SQL Server.
/// </remarks>
public class BarCodeAggregateRepositoryTests : DependenciesFactory
{
    private const int MachineId = 100;
    private const int BarCodeId = 900001;
    private const int CycleId = 900002;

    private readonly ITestOutputHelper _outputHelper;
    private readonly IFlowStatusCalculator _flowStatusCalculator = new FlowStatusCalculator();

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeAggregateRepositoryTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public BarCodeAggregateRepositoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private BarCodeAggregateRepository CreateRepository() =>
        new(DpIndTraceDbContextFactory, XUnitLogger.CreateLogger<BarCodeAggregateRepository>(_outputHelper));

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
    /// Loading the barcode aggregate windowed to machine 100, applying an OK cycle completion, and saving it
    /// persists the in-place cycle/barcode update, the appended register and the idempotency completion marker;
    /// a fresh windowed load then shows the cycle FinishedOk and a single <c>CycleCompletion</c> row exists.
    /// </summary>
    [Fact]
    public async Task LoadWindowedApplyOkSave_RoundTripsCompletionAndMarker()
    {
        // Arrange
        await Initialization;

        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        var clock = DpIDateTimeMachine;

        // A cycle started well in the past so its cycle time is comfortably inside the wide recipe window,
        // regardless of the clock's DateTimeKind — this apply must resolve FinishedOk deterministically.
        var startedOn = clock.Now.AddDays(-1);
        await SeedBarCodeWithStartedCycleAsync(startedOn, ct);

        // A recipe whose cycle-time window [1, 200000] surely contains the ~1-day cycle time, and MaxCyclesOk = 3
        // so the single loaded Started cycle (0 FinishedOk) does not trip the rework cap.
        var recipe = Recipe.Create(0, 0, 1, 200_000, 3, 5, 1).Value.ShouldNotBeNull();
        var registers = new List<Register>
        {
            Register.Create("R1", string.Empty, MachineId, 0, CycleId, "v", "int", 1, clock.Now).Value.ShouldNotBeNull(),
        };

        // Act — load the machine-windowed aggregate (M2 complete cycle set), apply an OK completion, and save.
        var loaded = await repository.LoadAsync(BarCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();

        // The window is a machine filter with NO paging: exactly the barcode's machine-100 cycles are loaded.
        root.LoadedCycles.Count.ShouldBe(1);
        var cycle = root.LoadedCycles.Single(c => c.CycleId == new CycleId(CycleId));

        var applied = root.CompleteOkCycle(
            cycle, MachineId, MachineType.Final, recipe, registers, root.LoadedCycles, _flowStatusCalculator, Graph(), clock);
        applied.IsSuccess.ShouldBeTrue();

        var saved = await repository.SaveAsync(root, ct);
        saved.IsSuccess.ShouldBeTrue();

        // Assert — a fresh windowed load shows the cycle completed FinishedOk.
        var reloaded = await repository.LoadAsync(BarCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        reloaded.IsSuccess.ShouldBeTrue();
        var reloadedRoot = reloaded.Value.ShouldNotBeNull();
        var reloadedCycle = reloadedRoot.LoadedCycles.Single(c => c.CycleId == new CycleId(CycleId));
        reloadedCycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        reloadedRoot.MachineId.Value.ShouldBe(MachineId);

        // Assert — exactly one completion marker row exists for the completed cycle, and the register appended.
        await using var verifyCtx = await DpIndTraceDbContextFactory.CreateDbContextAsync(ct);
        var markerCount = await verifyCtx.Set<CycleCompletion>()
            .CountAsync(m => m.CycleId == new CycleId(CycleId), ct);
        markerCount.ShouldBe(1);

        var registerCount = await verifyCtx.Set<Register>()
            .CountAsync(r => r.CycleId == new CycleId(CycleId), ct);
        registerCount.ShouldBe(1);
    }

    private async Task SeedBarCodeWithStartedCycleAsync(DateTime startedOn, CancellationToken ct)
    {
        var barCode = BarCode.Create("AGG-40C-001", productId: 0, machineId: MachineId, createdOn: startedOn, modifiedOn: startedOn);
        barCode.BarCodeId = new BarCodeId(BarCodeId);
        (await DpBarCodeRepository.AddAsync(barCode, ct)).IsSuccess.ShouldBeTrue();

        var cycle = Cycle.CreateStarted(MachineId, BarCodeId, cyclesOk: 0, startedOn, finishedOn: startedOn);
        cycle.CycleId = new CycleId(CycleId);
        (await DpCycleRepository.AddAsync(cycle, ct)).IsSuccess.ShouldBeTrue();
    }
}
