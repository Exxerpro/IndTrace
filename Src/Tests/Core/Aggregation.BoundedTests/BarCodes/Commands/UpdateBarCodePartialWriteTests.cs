// <copyright file="UpdateBarCodePartialWriteTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.BarCodes.Commands.Update;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Repositories;

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Commands;

/// <summary>
/// #114 chunk C behavioral regression (real repositories over EF InMemory — no mocks): the EndOfProcess
/// update saga persists the new FinishedOk cycle INSERT and the barcode Finished/Ok status UPDATE through
/// ONE <see cref="BarCodeAggregateRepository.SaveAsync"/>. Pre-fix the handler committed the cycle via a
/// separate <c>IRepository&lt;Cycle&gt;.AddAsync</c> BEFORE the failable barcode update: a failure there told
/// the PLC "failed" while the FinishedOk cycle stayed committed, so a PLC retry duplicated it.
/// </summary>
/// <remarks>
/// EF-Core InMemory ignores the <c>rowversion</c> token and the explicit transaction, so these tests prove
/// the FUNCTIONAL single-save path (both rows written by one dispatch; a failed barcode half fails the WHOLE
/// save as one Result and consumes the staged sets); the true rollback/atomicity proof (zero FinishedOk cycle
/// rows leak) lives in the real-SQL Integration suite (<c>UpdateBarCodeAtomicPersistTests</c>), mirroring the
/// chunk A/B precedent.
/// </remarks>
public class UpdateBarCodePartialWriteTests : DependenciesFactory
{
    private const int MachineId = 100;

    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBarCodePartialWriteTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public UpdateBarCodePartialWriteTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private BarCodeAggregateRepository CreateRepository() =>
        new(DpIndTraceDbContextFactory, XUnitLogger.CreateLogger<BarCodeAggregateRepository>(_outputHelper));

    /// <summary>
    /// The folded write, store truth: ONE handler dispatch persists BOTH the new FinishedOk/Ok cycle row AND
    /// the barcode root's Finished/Ok status fields — no separate cycle AddAsync / barcode UpdateAsync is
    /// involved anywhere (the handler has no such dependency any more). Read back through a fresh context.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task UpdateBarCode_PersistsCycleAndBarcodeStatus_InOneAggregateSave()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;

        // The known-good seeded part the sibling UpdateBarCodesCommandTests drive (machine 100).
        const string label = "L1AL100003232372501";
        const string partNumber = "L100003";

        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var sut = new UpdateBarCodeCommandHandler(DpDateTimeMachine, DpBarCodeAggregateRepository, DpBarCodeDetailsLoader);
        var command = new UpdateBarCodeCommand();
        command.WithData(TaskGatewayRequest.Create(MachineId, label, partNumber, PartStatus.Ok, CycleStatus.Started));

        // Act — ONE dispatch = ONE aggregate save carrying both halves.
        var result = await sut.ProcessAsync(command, ct);

        result.IsSuccess.ShouldBeTrue($"the update must succeed: {string.Join("; ", result.Errors ?? [])}");
        var response = result.Value.ShouldNotBeNull();
        var cycleId = response.Cycle.ShouldNotBeNull().CycleId.Value;
        cycleId.ShouldNotBe(0, "the store-generated CycleId must be stamped onto the staged cycle after the commit.");

        // Store truth via a fresh context: the FinishedOk cycle row landed …
        await using var verifyCtx = await DpIndTraceDbContextFactory.CreateDbContextAsync(ct);
        var cycleKey = new CycleId(cycleId);
        var cycleRow = await verifyCtx.Set<Cycle>().AsNoTracking()
            .FirstOrDefaultAsync(c => c.CycleId == cycleKey, ct);
        var persistedCycle = cycleRow.ShouldNotBeNull();
        persistedCycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        persistedCycle.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);

        // … AND the barcode ROOT row carries the folded Finished/Ok status write in the same save.
        var barCodeKey = new BarCodeId(response.BarCodeId);
        var barCodeRow = await verifyCtx.Set<BarCode>().AsNoTracking()
            .FirstOrDefaultAsync(b => b.BarCodeId == barCodeKey, ct);
        var persistedBarCode = barCodeRow.ShouldNotBeNull();
        persistedBarCode.FlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
        persistedBarCode.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
        persistedBarCode.MachineId.Value.ShouldBe(MachineId);
    }

    /// <summary>
    /// The orphan-cycle regression at the persistence seam, with the CHUNK C staged shape (FinishedOk cycle +
    /// Finished/Ok status write): when the single save FAILS at the barcode-update half (the root row was
    /// deleted between load and save, so the attach-Modified UPDATE hits zero store rows), the caller gets
    /// ONE <see cref="Result"/> failure covering BOTH writes — the PLC "failed" answer can no longer coexist
    /// with an independently committed FinishedOk cycle — and the staged sets (including the status-write
    /// flag) are consumed by the attempt. NOTE: the EF-InMemory provider's transaction is a no-op, so the
    /// ZERO-cycle-rows ground truth (the real rollback) cannot be proven here; it is proven on live SQL by
    /// <c>UpdateBarCodeAtomicPersistTests</c> in the Integration suite (chunk B precedent).
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task SaveAsync_BarcodeHalfFails_FailsWholeSave_AndConsumesStagedWrites()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int barCodeId = 914301;

        var createdOn = DpIDateTimeMachine.Now.AddDays(-1);
        var barCode = BarCode.Create("AGG-114C-POISON", productId: 0, machineId: MachineId, createdOn, createdOn);
        barCode.BarCodeId = new BarCodeId(barCodeId);
        (await DpBarCodeRepository.AddAsync(barCode, ct)).IsSuccess.ShouldBeTrue();

        var loaded = await repository.LoadAsync(barCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();

        // Stage the exact chunk C shape: a FinishedOk/Ok cycle + the Finished/Ok status write.
        var startedOn = DpIDateTimeMachine.Now.AddMinutes(-5);
        var newCycle = Cycle.CreateStarted(MachineId, barCodeId, cyclesOk: 0, startedOn, finishedOn: startedOn);
        newCycle.ApplyCycleAndPartStatus(CycleStatus.FinishedOk, PartStatus.Ok);
        root.StageNewCycle(newCycle).IsSuccess.ShouldBeTrue();
        root.StageStatusWrite(FlowStatus.Finished.Value, PartStatus.Ok.Value, MachineId, startedOn)
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
        // FinishedOk cycle INSERT no longer reports success independently of the barcode UPDATE.
        var saved = await repository.SaveAsync(root, ct);
        saved.IsFailure.ShouldBeTrue();

        // … and consume the staged sets (PR #170 contract), including the status-write flag, so a retry
        // cannot double-apply a stale batch.
        root.PendingNewCycles.ShouldBeEmpty();
        root.HasPendingStatusWrite.ShouldBeFalse();
    }
}
