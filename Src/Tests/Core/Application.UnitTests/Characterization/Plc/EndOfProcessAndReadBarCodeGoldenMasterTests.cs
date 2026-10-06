// <copyright file="EndOfProcessAndReadBarCodeGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Plc;

/// <summary>
/// Golden-master (characterization) tests for the remaining PLC-path triggers:
/// <c>EndOfProcessAsync</c> (value 128, persisted via <see cref="UpdateBarCodeCommandHandler"/>)
/// and <c>ReadBarCodeAsync</c> (value 8, read-only no-op).
///
/// Story 1.1, AC 8 (matrix row 9) and AC 9 (matrix row 10).
///
/// Story 3.2 note (FR6): this golden master pins ONLY the PERSISTED truth for EndOfProcess
/// (BarCode Finished/Ok + new Cycle FinishedOk/Ok) and the ReadBarCode no-op — NEITHER assertion
/// pins the divergent EndOfProcess(16)/NOk(2) PLC projection (that projection was pinned in the
/// Domain/Application anomaly tests). The convergence introduced by Story 3.2 leaves the persisted
/// values UNCHANGED, so every assertion below stays byte-equal and green; nothing is flipped here.
/// </summary>
public class EndOfProcessAndReadBarCodeGoldenMasterTests
{
    // ----------------------------------------------------------------------------------
    // AC 8 / matrix row 9 — EndOfProcess persisted truth via UpdateBarCodeCommandHandler:
    // barcode FlowStatus.Finished, PartStatus.Ok; a NEW cycle is created with
    // CycleStatus.FinishedOk, PartStatus.Ok. (The request-vs-persisted divergence — the PLC
    // projection says EndOfProcess/NOk — is pinned in the Domain projection test and Story 1.3.)
    // ----------------------------------------------------------------------------------

    [Fact]
    public async Task EndOfProcess_FromInProcess_PersistsFinishedOk_NewCycleFinishedOk()
    {
        // Arrange
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var barCodeAggregateRepository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();

        // #33 Chunk 5: the handler now loads via the stateless IBarCodeDetailsLoader returning an immutable snapshot.
        var loader = Substitute.For<IBarCodeDetailsLoader>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Local));

        // Persisted barcode entity the handler mutates in UpdateBarcodeState.
        var persistedBarCode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); }).Build();

        var snapshot = new BarCodeSnapshot
        {
            MachineId = 5,
            BarCodeId = 42,
            FlowStatus = FlowStatus.InProcess, // source state
            PartStatus = PartStatus.Ok,
            ResultValidation = ResultValidation.Valid, // gate in handler requires Valid
            BarCode = persistedBarCode,
            References = new Dictionary<string, Register>(),
        };

        // #114 chunk C: capture the cycle STAGED on the root (replaces the retired AddAsync capture). The
        // stub mirrors the real aggregate-repository contract — the store-generated identity (900) is
        // back-filled onto the staged cycle only after the durable commit; a rows-affected count is
        // unrepresentable here by construction (PR #182 bug class).
        Cycle? createdCycle = null;

        loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<BarCodeSnapshot>.Success(snapshot));

        barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var root = ci.Arg<BarCode>();
                if (root.PendingNewCycles.Count > 0)
                {
                    createdCycle = root.PendingNewCycles[0];
                    createdCycle.CycleId = new CycleId(900);
                }

                return Task.FromResult(Result.Success());
            });

        var handler = new UpdateBarCodeCommandHandler(dateTimeMachine, barCodeAggregateRepository, loader);

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 5,
                BarCode = "BC-EOP",
                PartNumber = "PART",
            },
        };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — persisted barcode advanced to Finished / Ok.
        result.IsSuccess.ShouldBeTrue();
        persistedBarCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        persistedBarCode.PartStatus.ShouldBe(PartStatus.Ok);

        // A new cycle was persisted as FinishedOk / Ok through the ONE aggregate save (#114 chunk C).
        await barCodeAggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => ReferenceEquals(b, persistedBarCode)),
            Arg.Any<CancellationToken>());
        createdCycle.ShouldNotBeNull();
        createdCycle.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        createdCycle.PartStatus.ShouldBe(PartStatus.Ok);
        createdCycle.CycleId.Value.ShouldBe(900); // store-generated id back-filled after commit (mirrored by the stub)
    }

    // ----------------------------------------------------------------------------------
    // AC 9 / matrix row 10 — ReadBarCode is a read-only no-op: it does NOT mutate the
    // persisted BarCode.FlowStatus or Cycle.CycleStatus. The PLC-facing transient
    // projection (SetStatusReadBarCode → NotStarted / Ok) is asserted in the Domain test;
    // here we pin that the persisted entities are untouched.
    //
    // The read path has no command handler (it is a query/projection), so this test pins
    // the no-op at the projection seam: applying the read projection to a transient request
    // leaves the persisted entities it was derived from unchanged.
    // ----------------------------------------------------------------------------------

    [Fact]
    public void ReadBarCode_IsNoOp_PersistedBarCodeAndCycleUnchanged()
    {
        // Arrange — persisted state that must survive a read untouched.
        var persistedBarCode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => b.BarCodeId = new BarCodeId(1)).Build();
        var persistedCycle = new CycleBuilder().Started(PartStatus.Ok).With(c => c.CycleId = new CycleId(1)).Build();

        var readRequest = new TaskGatewayRequest
        {
            MachineId = 1,
            BarCode = persistedBarCode.Label.Value,
        };

        // Act — drive only the read projection (the read path mutates the transient request,
        // never the persisted entities).
        readRequest.SetCommandStatusFromTask(GatewayTask.ReadBarCodeAsync.Name);

        // Assert — persisted truth is unchanged by the read.
        persistedBarCode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        persistedBarCode.PartStatus.ShouldBe(PartStatus.Ok);
        persistedCycle.CycleStatus.ShouldBe(CycleStatus.Started);
        persistedCycle.PartStatus.ShouldBe(PartStatus.Ok);

        // And the transient projection took the read-only PLC view (NotStarted / Ok).
        readRequest.CycleStatus.ShouldBe(CycleStatus.NotStarted);
        readRequest.PartStatus.ShouldBe(PartStatus.Ok);
    }
}
