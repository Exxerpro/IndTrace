// <copyright file="EndOfProcessDivergencePersistedTruthTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.StateMachine;

namespace Application.UnitTests.Characterization.Anomalies;

/// <summary>
/// Story 1.3 — Anomaly A (EndOfProcess request-vs-persisted divergence), now CONVERGED in Story 3.2 (FR6).
///
/// BEFORE (Story 1.3, AS-BUILT divergence — characterized but intentionally NOT fixed):
///   - PERSISTED truth: BarCode Finished/Ok + a NEW Cycle FinishedOk/Ok (UpdateBarCodeCommandHandler).
///   - PLC PROJECTION : EndOfProcess(16)/NOk(2) (TaskGatewayRequest.SetStatusEndOfProcess).
///   - The two representations DISAGREED — the database said one thing, the PLC heard another.
///
/// AFTER (Story 3.2, FR6 — convergence ON by default): the handler routes EndOfProcessAsync through
/// <see cref="IItemStateMachine"/> and projects the SAME machine outcome it persists onto the PLC-facing
/// <c>TaskGatewayRequest</c>. The persisted tuple and the projected tuple are now IDENTICAL
/// (Finished / FinishedOk / Ok) — exactly ONE result. The legacy divergent path remains reachable with
/// the <c>RouteEndOfProcess</c> flag OFF (rollback), pinned by the flag-OFF handler test.
/// </summary>
public class EndOfProcessDivergencePersistedTruthTests
{
    /// <summary>
    /// AC3/AC4 — the canonical CONVERGENCE test (was the divergence test in Story 1.3).
    /// Drives <see cref="UpdateBarCodeCommandHandler.ProcessAsync"/> with a Valid in-process barcode and the
    /// converge flag ON (default), then asserts the persisted BarCode/Cycle values EQUAL the PLC-projected
    /// <c>TaskGatewayRequest</c> values — there is exactly one result, not two.
    /// </summary>
    [Fact]
    public async Task EndOfProcess_Projection_Converges_With_PersistedTruth()
    {
        // Arrange — drive UpdateBarCodeCommandHandler with a Valid barcode (converge flag default ON).
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var barCodeAggregateRepository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();

        // #33 Chunk 5: the handler now loads via the stateless IBarCodeDetailsLoader returning an immutable snapshot.
        var loader = Substitute.For<IBarCodeDetailsLoader>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Local));

        // Source state before EndOfProcess persistence.
        var persistedBarCode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); }).Build();

        var snapshot = new BarCodeSnapshot
        {
            MachineId = 5,
            BarCodeId = 42,
            FlowStatus = FlowStatus.InProcess,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Final,
            ResultValidation = ResultValidation.Valid, // handler gate requires Valid (UpdateBarcodeCommandHandler.cs)
            BarCode = persistedBarCode,
            References = new Dictionary<string, Register>(),
        };

        // #114 chunk C: capture the new cycle STAGED on the root at save time (replaces the AddAsync capture).
        Cycle? createdCycle = null;
        barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var root = ci.Arg<BarCode>();
                if (root.PendingNewCycles.Count > 0)
                {
                    createdCycle = root.PendingNewCycles[0];
                }

                return Task.FromResult(Result.Success());
            });

        loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<BarCodeSnapshot>.Success(snapshot));

        // Default ctor => machine = new ItemStateMachine(), routing default-ON (RouteEndOfProcess = true).
        var handler = new UpdateBarCodeCommandHandler(dateTimeMachine, barCodeAggregateRepository, loader);

        // The projection rides on the command's TaskGatewayRequest (the same object the gateway mutated).
        var projection = new TaskGatewayRequest { MachineId = 5, BarCode = "BC-EOP", PartNumber = "PART" };
        var command = new UpdateBarCodeCommand { Command = projection };

        // Act — persist the EndOfProcess effect (converge ON).
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — persisted truth: BarCode Finished / Ok, new Cycle FinishedOk / Ok (unchanged from Story 1.3).
        result.IsSuccess.ShouldBeTrue();
        persistedBarCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        persistedBarCode.PartStatus.ShouldBe(PartStatus.Ok);
        createdCycle.ShouldNotBeNull();
        createdCycle!.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        createdCycle.PartStatus.ShouldBe(PartStatus.Ok);

        // Assert — THE CONVERGENCE (AC3, was the divergence in Story 1.3): projection == persisted truth.
        // AFTER: projection no longer says EndOfProcess/NOk; it is a read-only VIEW of the machine outcome.
        projection.FlowStatus.ShouldBe(persistedBarCode.FlowStatus);   // Finished == Finished
        projection.CycleStatus.ShouldBe(createdCycle.CycleStatus);     // FinishedOk == FinishedOk
        projection.PartStatus.ShouldBe(createdCycle.PartStatus);       // Ok == Ok
        projection.PartStatus.ShouldBe(persistedBarCode.PartStatus);   // Ok == Ok (barcode)

        // BEFORE/AFTER contrast — the OLD divergent dead-state value (EndOfProcess) is NO LONGER projected.
        projection.CycleStatus.ShouldNotBe(CycleStatus.EndOfProcess); // was EndOfProcess(16) BEFORE; FinishedOk AFTER
        projection.PartStatus.ShouldNotBe(PartStatus.NOk);            // was NOk(2) BEFORE; Ok AFTER
    }

    /// <summary>
    /// AC6 — flag OFF restores the prior divergent behavior (rollback). The handler keeps the legacy inline
    /// persisted truth (Finished/Ok + new Cycle FinishedOk/Ok) and does NOT project onto the request, so the
    /// legacy <see cref="TaskGatewayRequest.SetStatusEndOfProcess"/> projection (EndOfProcess/NOk) still
    /// disagrees with the persisted truth — exactly the Story 1.3 divergence, reachable on demand.
    /// </summary>
    [Fact]
    public async Task EndOfProcess_WhenFlagOff_RestoresLegacyDivergence()
    {
        // Arrange — legacy PLC projection (set by the gateway executor before dispatch).
        var projection = new TaskGatewayRequest { MachineId = 5, BarCode = "BC-EOP", PartNumber = "PART" };
        projection.SetCommandStatusFromTask(GatewayTask.EndOfProcessAsync.Name); // EndOfProcess(16)/NOk(2)
        projection.CycleStatus.ShouldBe(CycleStatus.EndOfProcess);
        projection.PartStatus.ShouldBe(PartStatus.NOk);

        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var barCodeAggregateRepository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();

        // #33 Chunk 5: the handler now loads via the stateless IBarCodeDetailsLoader returning an immutable snapshot.
        var loader = Substitute.For<IBarCodeDetailsLoader>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Local));

        var persistedBarCode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); }).Build();

        var snapshot = new BarCodeSnapshot
        {
            MachineId = 5,
            BarCodeId = 42,
            FlowStatus = FlowStatus.InProcess,
            PartStatus = PartStatus.Ok,
            ResultValidation = ResultValidation.Valid,
            BarCode = persistedBarCode,
            References = new Dictionary<string, Register>(),
        };

        // #114 chunk C: capture the new cycle STAGED on the root at save time (replaces the AddAsync capture).
        // Story 5.1: the handler persists the barcode UNCONDITIONALLY (flag governs WHICH values, not WHETHER
        // they persist) — the single aggregate save covers the flag-OFF rollback path too.
        Cycle? createdCycle = null;
        barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var root = ci.Arg<BarCode>();
                if (root.PendingNewCycles.Count > 0)
                {
                    createdCycle = root.PendingNewCycles[0];
                }

                return Task.FromResult(Result.Success());
            });

        loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<BarCodeSnapshot>.Success(snapshot));

        var routingOff = Microsoft.Extensions.Options.Options.Create(
            new IndTrace.Application.StateMachine.StateMachineRoutingOptions { RouteEndOfProcess = false });
        var handler = new UpdateBarCodeCommandHandler(dateTimeMachine, barCodeAggregateRepository, loader, new ItemStateMachine(), routingOff);

        var command = new UpdateBarCodeCommand { Command = projection };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — persisted truth is still the legacy inline Finished/Ok + FinishedOk/Ok.
        result.IsSuccess.ShouldBeTrue();
        persistedBarCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        persistedBarCode.PartStatus.ShouldBe(PartStatus.Ok);
        createdCycle.ShouldNotBeNull();
        createdCycle!.CycleStatus.ShouldBe(CycleStatus.FinishedOk);

        // Assert — the legacy DIVERGENCE is restored: the projection was NOT overwritten by the outcome.
        projection.CycleStatus.ShouldBe(CycleStatus.EndOfProcess); // still EndOfProcess(16)
        projection.PartStatus.ShouldBe(PartStatus.NOk);            // still NOk(2)
        projection.CycleStatus.ShouldNotBe(createdCycle.CycleStatus); // EndOfProcess != FinishedOk (diverges)
        projection.PartStatus.ShouldNotBe(persistedBarCode.PartStatus); // NOk != Ok (diverges)
    }
}
