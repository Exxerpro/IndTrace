// <copyright file="BarCodeAggregateMemberWriteTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Services;

/// <summary>
/// Aggregation tests (real <see cref="BarCodeAggregateRepository"/> over EF-Core InMemory via
/// <see cref="DependenciesFactory"/>) for the #95 Phase 2 Slice E member-write surface: the staged new-cycle
/// INSERT (<c>BarCode.StageNewCycle</c> — the CycleCreator migration), the staged cycle-status UPDATE
/// (<c>BarCode.StageCycleStatusUpdate</c> — the CancelCycle migration), the consumed-by-the-attempt contract
/// on success AND failure (PR #170), and the two migrated call paths end-to-end (a real
/// <see cref="CycleCreator"/> and a real <see cref="CancelCycleCommandHandler"/> over the real repositories).
/// </summary>
/// <remarks>
/// EF-Core InMemory ignores the <c>rowversion</c> token and the explicit transaction, so this proves the
/// FUNCTIONAL persistence path only; the atomicity/rollback proofs for the member writes live in the real-SQL
/// Integration suite (<c>BarCodeAggregateMemberWriteTests</c> there), mirroring the Chunk 40-D precedent.
/// </remarks>
public class BarCodeAggregateMemberWriteTests : DependenciesFactory
{
    private const int MachineId = 100;

    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeAggregateMemberWriteTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public BarCodeAggregateMemberWriteTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private BarCodeAggregateRepository CreateRepository() =>
        new(DpIndTraceDbContextFactory, XUnitLogger.CreateLogger<BarCodeAggregateRepository>(_outputHelper));

    private async Task SeedBarCodeAsync(int barCodeId, string label, CancellationToken ct)
    {
        var createdOn = DpIDateTimeMachine.Now.AddDays(-1);
        var barCode = BarCode.Create(label, productId: 0, machineId: MachineId, createdOn, createdOn);
        barCode.BarCodeId = new BarCodeId(barCodeId);
        (await DpBarCodeRepository.AddAsync(barCode, ct)).IsSuccess.ShouldBeTrue();
    }

    private async Task SeedInProcessBarCodeAsync(int barCodeId, string label, CancellationToken ct)
    {
        var barCode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(barCodeId);
                b.MachineId = new MachineId(MachineId);
                b.Label = BarCodeLabel.FromPersisted(label);
            })
            .Build();
        (await DpBarCodeRepository.AddAsync(barCode, ct)).IsSuccess.ShouldBeTrue();
    }

    private async Task SeedStartedCycleAsync(int cycleId, int barCodeId, CancellationToken ct)
    {
        var startedOn = DpIDateTimeMachine.Now.AddHours(-1);
        var cycle = Cycle.CreateStarted(MachineId, barCodeId, cyclesOk: 0, startedOn, finishedOn: startedOn);
        cycle.CycleId = new CycleId(cycleId);
        (await DpCycleRepository.AddAsync(cycle, ct)).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// The Slice E INSERT round-trip: load the (windowed) aggregate, stage a brand-new cycle, save through
    /// the single-flush path — the row persists with a store identity, the staged set is consumed after the
    /// durable commit, and a fresh windowed load folds the new member back onto the root.
    /// </summary>
    [Fact]
    public async Task LoadStageNewCycleSave_PersistsInsert_AndConsumesStagedSet()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int barCodeId = 910001;
        await SeedBarCodeAsync(barCodeId, "AGG-95E-INSERT", ct);

        var loaded = await repository.LoadAsync(barCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();
        root.LoadedCycles.ShouldBeEmpty();

        var startedOn = DpIDateTimeMachine.Now.AddMinutes(-5);
        var newCycle = Cycle.CreateStarted(MachineId, barCodeId, cyclesOk: 0, startedOn, finishedOn: startedOn);
        root.StageNewCycle(newCycle).IsSuccess.ShouldBeTrue();

        var saved = await repository.SaveAsync(root, ct);
        saved.IsSuccess.ShouldBeTrue();

        // Consumed after the durable commit (clear-after-attempt) + the store identity stamped back.
        root.PendingNewCycles.ShouldBeEmpty();
        newCycle.CycleId.Value.ShouldNotBe(0);

        var reloaded = await repository.LoadAsync(barCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        reloaded.IsSuccess.ShouldBeTrue();
        var reloadedRoot = reloaded.Value.ShouldNotBeNull();
        reloadedRoot.LoadedCycles.Count.ShouldBe(1);
        reloadedRoot.LoadedCycles[0].CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
    }

    /// <summary>
    /// FIELD-FOR-FIELD PARITY REGRESSION (definition-of-done gate): the migrated <see cref="CycleCreator"/>
    /// (aggregate path) persists EXACTLY the cycle row the retired raw-<c>AddAsync</c> implementation did —
    /// same machine/barcode ids, CycleTime 0, TaktTime 0, the request timestamps narrowed via
    /// <c>ToLocalTime().DateTime</c>, the PLC-supplied cycle/part status, CyclesOk 0 — proven by reading the
    /// row back from the store through a fresh context.
    /// </summary>
    [Fact]
    public async Task CycleCreator_AggregatePath_PersistsSameCycleRow_FieldForField()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int barCodeId = 910002;
        await SeedBarCodeAsync(barCodeId, "AGG-95E-CREATOR", ct);

        var creator = new CycleCreator(CreateRepository(), XUnitLogger.CreateLogger<CycleCreator>(_outputHelper));
        var startedOn = new DateTimeOffset(2026, 7, 20, 6, 30, 0, TimeSpan.Zero);
        var finishedOn = new DateTimeOffset(2026, 7, 20, 6, 45, 0, TimeSpan.Zero);
        var request = new CycleCreateRequest(
            MachineId: MachineId,
            BarCodeId: barCodeId,
            CycleStatus: CycleStatus.Started,
            PartStatus: PartStatus.Ok,
            StartedOn: startedOn,
            FinishedOn: finishedOn,
            FlowStatus: FlowStatus.InProcess,
            ModifiedOn: finishedOn);

        var result = await creator.CreateAsync(request, ct);

        result.IsSuccess.ShouldBeTrue();
        var created = result.Value.ShouldNotBeNull();
        created.CycleId.Value.ShouldNotBe(0);

        // Read the PERSISTED row back through a fresh context — the parity evidence is store truth, not the
        // in-memory instance.
        await using var verifyCtx = await DpIndTraceDbContextFactory.CreateDbContextAsync(ct);
        var persistedKey = created.CycleId;
        var persisted = await verifyCtx.Set<Cycle>().AsNoTracking()
            .FirstOrDefaultAsync(c => c.CycleId == persistedKey, ct);
        var row = persisted.ShouldNotBeNull();

        row.MachineId.Value.ShouldBe(MachineId);
        row.BarCodeId.Value.ShouldBe(barCodeId);
        row.CycleTime.ShouldBe(0);
        row.TaktTime.ShouldBe(0);
        row.StartedOn.ShouldBe(startedOn.ToLocalTime().DateTime);
        row.FinishedOn.ShouldBe(finishedOn.ToLocalTime().DateTime);
        row.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        row.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
        row.CyclesOk.ShouldBe(0);
    }

    /// <summary>
    /// The Slice E UPDATE round-trip through the migrated handler: with the <c>EnableCanceledState</c> gate
    /// ON, a legal Cancel on an InProcess barcode with a Started latest cycle persists
    /// <see cref="CycleStatus.Canceled"/> ON THE CYCLE through the aggregate save (member-only — the barcode
    /// root row is not part of the write) and still writes the best-effort success audit exactly as before.
    /// </summary>
    [Fact]
    public async Task CancelCycleHandler_GateOn_CancelsLatestCycleThroughAggregate_AndAudits()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int barCodeId = 910003;
        const int cycleId = 910103;
        const string label = "AGG-95E-CANCEL";
        await SeedInProcessBarCodeAsync(barCodeId, label, ct);
        await SeedStartedCycleAsync(cycleId, barCodeId, ct);

        var handler = new CancelCycleCommandHandler(
            DpBarCodeRepository,
            DpCommandRepository,
            DpRoCycleRepository,
            CreateRepository(),
            DpIDateTimeMachine,
            new ItemStateMachine(),
            Options.Create(new StateMachineRoutingOptions { EnableCanceledState = true }));

        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().CycleStatus.Value.ShouldBe(CycleStatus.Canceled.Value);

        // Store truth: the cycle row is Canceled, and the success audit landed (outside the aggregate
        // transaction, unchanged semantics).
        await using var verifyCtx = await DpIndTraceDbContextFactory.CreateDbContextAsync(ct);
        var cycleKey = new CycleId(cycleId);
        var persisted = await verifyCtx.Set<Cycle>().AsNoTracking()
            .FirstOrDefaultAsync(c => c.CycleId == cycleKey, ct);
        persisted.ShouldNotBeNull().CycleStatus.Value.ShouldBe(CycleStatus.Canceled.Value);

        var audits = await verifyCtx.Set<TaskGatewayRequest>().AsNoTracking()
            .Where(r => r.CycleId == cycleId)
            .ToListAsync(ct);
        audits.Count.ShouldBe(1);
        audits[0].GatewayTask.Value.ShouldBe(GatewayTask.Cancel.Value);
        audits[0].ResultValidation.Value.ShouldBe(ResultValidation.None.Value);
        audits[0].CycleStatus.Value.ShouldBe(CycleStatus.Canceled.Value);
    }

    /// <summary>
    /// The PR #170 consumed-by-the-attempt contract, mirrored for the NEW Slice E staged sets: a FAILED
    /// member save (a staged status update whose row does not exist in the store) surfaces as a Result
    /// failure — never a throw — and discards BOTH staged sets, so a later save of the same root cannot
    /// double-apply a stale batch.
    /// </summary>
    [Fact]
    public async Task SaveAsync_FailedMemberSave_ConsumesBothStagedSets()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int barCodeId = 910004;
        await SeedBarCodeAsync(barCodeId, "AGG-95E-FAILDISCARD", ct);

        var loaded = await repository.LoadAsync(barCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();

        // A "persisted" cycle whose row was never inserted: the attach-Modified update hits zero store rows
        // and the flush fails (DbUpdateConcurrencyException on InMemory), rolling the whole attempt back.
        var startedOn = DpIDateTimeMachine.Now.AddMinutes(-5);
        var phantom = Cycle.CreateStarted(MachineId, barCodeId, cyclesOk: 0, startedOn, finishedOn: startedOn);
        phantom.CycleId = new CycleId(910999);
        root.StageCycleStatusUpdate(phantom, CycleStatus.Canceled).IsSuccess.ShouldBeTrue();

        var newCycle = Cycle.CreateStarted(MachineId, barCodeId, cyclesOk: 0, startedOn, finishedOn: startedOn);
        root.StageNewCycle(newCycle).IsSuccess.ShouldBeTrue();

        var saved = await repository.SaveAsync(root, ct);

        saved.IsFailure.ShouldBeTrue();

        // Consumed by the failed attempt — nothing left to double-apply; the caller must re-stage to retry.
        root.PendingCycleUpdates.ShouldBeEmpty();
        root.PendingNewCycles.ShouldBeEmpty();
    }

    /// <summary>
    /// The pinned #40 contract is unchanged by Slice E: a save with NOTHING staged at all (no applied
    /// completion, no member writes, no idempotent-resend flag) still fails with the original message.
    /// </summary>
    [Fact]
    public async Task SaveAsync_NothingStaged_StillFailsWithTheOriginalCompletionMessage()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int barCodeId = 910005;
        await SeedBarCodeAsync(barCodeId, "AGG-95E-NOSTAGE", ct);

        var loaded = await repository.LoadAsync(barCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();

        var saved = await repository.SaveAsync(root, ct);

        saved.IsFailure.ShouldBeTrue();
        saved.Errors.ShouldContain(e => e.Contains("No staged cycle completion to save"));
    }
}
