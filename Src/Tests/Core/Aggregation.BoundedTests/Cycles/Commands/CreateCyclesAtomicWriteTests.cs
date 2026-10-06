// <copyright file="CreateCyclesAtomicWriteTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Services;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Repositories;

namespace IndTrace.Aggregation.BoundedTests.Cycles.Commands;

/// <summary>
/// #114 chunk B behavioral regression (real repositories over EF InMemory — no mocks): the create-cycles
/// persistence saga now writes the Started cycle INSERT and the barcode status UPDATE through ONE
/// <see cref="BarCodeAggregateRepository.SaveAsync"/> — the real <see cref="CycleCreator"/> applies the
/// barcode field changes to the loaded aggregate root before its single save. Pre-fix the barcode update was
/// a SEPARATE auto-commit after the committed cycle: a failure there told the PLC "failed" while the Started
/// cycle stayed committed, so a PLC retry duplicated the cycle.
/// </summary>
/// <remarks>
/// EF-Core InMemory ignores the <c>rowversion</c> token and the explicit transaction, so these tests prove
/// the FUNCTIONAL single-save path (both rows written by one <c>SaveAsync</c>; a failed barcode half fails
/// the WHOLE save as one Result); the true rollback/atomicity proof (zero cycle rows leak) for the folded
/// root UPDATE lives in the real-SQL Integration suite (<c>CreateCyclesAtomicPersistTests</c>), mirroring
/// the chunk A precedent.
/// </remarks>
public class CreateCyclesAtomicWriteTests : DependenciesFactory
{
    private const int MachineId = 100;

    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCyclesAtomicWriteTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public CreateCyclesAtomicWriteTests(ITestOutputHelper outputHelper)
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

    private static CycleCreateRequest Request(int barCodeId, DateTimeOffset startedOn, DateTimeOffset modifiedOn) => new(
        MachineId: MachineId,
        BarCodeId: barCodeId,
        CycleStatus: CycleStatus.Started,
        PartStatus: PartStatus.Ok,
        StartedOn: startedOn,
        FinishedOn: startedOn,
        FlowStatus: FlowStatus.InProcess,
        ModifiedOn: modifiedOn);

    /// <summary>
    /// The folded write, store truth: ONE <see cref="CycleCreator.CreateAsync"/> call persists BOTH the
    /// Started cycle row AND the barcode root's status fields (FlowStatus/PartStatus/MachineId/ModifiedOn) —
    /// no separate barcode write is involved anywhere. Read back through a fresh context.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task CycleCreator_PersistsCycleAndBarcodeStatus_InOneAggregateSave()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int barCodeId = 914201;
        await SeedBarCodeAsync(barCodeId, "AGG-114B-FOLD", ct);

        var creator = new CycleCreator(CreateRepository(), XUnitLogger.CreateLogger<CycleCreator>(_outputHelper));
        var startedOn = new DateTimeOffset(2026, 8, 3, 6, 30, 0, TimeSpan.Zero);
        var modifiedOn = new DateTimeOffset(2026, 8, 3, 6, 30, 5, TimeSpan.Zero);

        var result = await creator.CreateAsync(Request(barCodeId, startedOn, modifiedOn), ct);

        result.IsSuccess.ShouldBeTrue();
        var created = result.Value.ShouldNotBeNull();
        created.CycleId.Value.ShouldNotBe(0);

        // Store truth via a fresh context: the cycle row landed …
        await using var verifyCtx = await DpIndTraceDbContextFactory.CreateDbContextAsync(ct);
        var cycleKey = created.CycleId;
        var cycleRow = await verifyCtx.Set<Cycle>().AsNoTracking()
            .FirstOrDefaultAsync(c => c.CycleId == cycleKey, ct);
        cycleRow.ShouldNotBeNull().CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);

        // … AND the barcode ROOT row carries the folded status write (pre-fix a member-only save left the
        // root row untouched and a separate BarCodeUpdater auto-commit wrote it in a second transaction).
        var barCodeKey = new BarCodeId(barCodeId);
        var barCodeRow = await verifyCtx.Set<BarCode>().AsNoTracking()
            .FirstOrDefaultAsync(b => b.BarCodeId == barCodeKey, ct);
        var persisted = barCodeRow.ShouldNotBeNull();
        persisted.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
        persisted.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
        persisted.MachineId.Value.ShouldBe(MachineId);
        persisted.ModifiedOn.ShouldBe(modifiedOn.ToLocalTime().DateTime);
    }

    /// <summary>
    /// The orphan-cycle regression at the persistence seam: when the single save FAILS at the barcode-update
    /// half (the root row cannot be updated — here it was deleted between load and save, so the attach-
    /// Modified update hits zero store rows), the caller gets ONE <see cref="Result"/> failure covering BOTH
    /// writes (pre-fix the cycle half had already committed and reported success on its own), and the staged
    /// sets — including the new status-write flag — are consumed by the attempt. NOTE: the EF-InMemory
    /// provider's transaction is a no-op, so the ZERO-new-cycle-rows ground truth (the real rollback) cannot
    /// be proven here; it is proven on live SQL by <c>CreateCyclesAtomicPersistTests</c> in the Integration
    /// suite, mirroring the chunk A / Chunk 40-D precedent.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task SaveAsync_BarcodeUpdateHalfFails_FailsWholeSave_AndConsumesStagedWrites()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int barCodeId = 914202;
        await SeedBarCodeAsync(barCodeId, "AGG-114B-POISON", ct);

        var loaded = await repository.LoadAsync(barCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();

        var startedOn = DpIDateTimeMachine.Now.AddMinutes(-5);
        var newCycle = Cycle.CreateStarted(MachineId, barCodeId, cyclesOk: 0, startedOn, finishedOn: startedOn);
        root.StageNewCycle(newCycle).IsSuccess.ShouldBeTrue();
        root.StageStatusWrite(FlowStatus.InProcess.Value, PartStatus.Ok.Value, MachineId, startedOn)
            .IsSuccess.ShouldBeTrue();

        // Poison the barcode-update half: delete the root row so the staged root UPDATE hits zero store rows.
        await using (var poisonCtx = await DpIndTraceDbContextFactory.CreateDbContextAsync(ct))
        {
            var barCodeKey = new BarCodeId(barCodeId);
            var row = await poisonCtx.Set<BarCode>().FirstAsync(b => b.BarCodeId == barCodeKey, ct);
            poisonCtx.Set<BarCode>().Remove(row);
            await poisonCtx.SaveChangesAsync(ct);
        }

        // Act — the failed barcode half must refuse the WHOLE save as a Result failure (never a throw): the
        // cycle INSERT no longer reports success independently of the barcode UPDATE.
        var saved = await repository.SaveAsync(root, ct);
        saved.IsFailure.ShouldBeTrue();

        // … and consume the staged sets (PR #170 contract), including the new status-write flag, so a retry
        // cannot double-apply a stale batch.
        root.PendingNewCycles.ShouldBeEmpty();
        root.HasPendingStatusWrite.ShouldBeFalse();
    }
}
