// <copyright file="RejectBarCodeGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Webapp;

/// <summary>
/// Golden-master (characterization) tests for the WEBAPP / MONITOR path REJECT handler:
/// <see cref="RejectBarCodeCommandHandler"/> (an <c>IMonitorRequestHandler</c> invoked from
/// <c>BarCodeReject.razor</c>). This path does NOT traverse the PLC command dispatcher
/// (state-machine-analysis.md §2.2); the PLC-path triggers are out of scope (Story 1.1).
///
/// Story 1.2 — AC 3 (matrix row 1), AC 4 (matrix row 2), AC 6 (matrix row 4), AC 7, AC 8.
/// These tests pin the CURRENT as-built behavior; NO production code is changed.
/// </summary>
public class RejectBarCodeGoldenMasterTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);

    // ----------------------------------------------------------------------------------
    // AC 3 / matrix row 1 — Reject from InProcess (found):
    // FlowStatus -> Rejected, ModifiedOn updated, barcode persisted (UpdateAsync),
    // logged TaskGatewayRequest carries GatewayTask.RejectPartAsync + ResultValidation.None,
    // result is success with a BarCodeRejectedView.
    // ----------------------------------------------------------------------------------
    [Fact]
    public async Task Reject_FromInProcess_WritesRejected_LogsRejectPartAsync_ReturnsRejectedView()
    {
        // Arrange
        const string label = "BC-REJECT-INPROCESS";
        var barcode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok) // source state
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();

        var (handler, barCodeRepository, requestRepository, _, dateTimeMachine) = BuildHandler(barcode, cycle: new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(900)).Build());
        TaskGatewayRequest? logged = null;
        requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(ci => { logged = ci.Arg<TaskGatewayRequest>(); return Task.FromResult(Result<int>.Success(1)); });

        var command = new RejectBarCodeCommand { Label = label };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — persisted truth.
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.Rejected);
        barcode.ModifiedOn.ShouldBe(dateTimeMachine.Now.ToLocalTime());
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());

        // Logged gateway request snapshot.
        logged.ShouldNotBeNull();
        logged!.GatewayTask.ShouldBe(GatewayTask.RejectPartAsync);
        logged.ResultValidation.ShouldBe(ResultValidation.None);
        logged.FlowStatus.ShouldBe(FlowStatus.Rejected);

        // View DTO returned on success.
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ShouldBeOfType<BarCodeRejectedView>();
        result.Value.FlowStatus.ShouldBe(FlowStatus.Rejected);
        result.Value.Label.ShouldBe(label);
    }

    // ----------------------------------------------------------------------------------
    // AC 4 / matrix row 2 — Reject from Finished (found):
    // SAME outcome -> FlowStatus.Rejected. There is NO source-state guard today; rejection is
    // unconditional once the label lookup (and the now-mandatory cycle lookup) succeed.
    // Characterized AS-IS; do NOT add a guard.
    // ----------------------------------------------------------------------------------
    [Fact]
    public async Task Reject_FromFinished_WritesRejected_Unconditional_NoSourceStateGuard()
    {
        // Arrange
        const string label = "BC-REJECT-FINISHED";
        var barcode = new BarCodeBuilder()
            .Finished(PartStatus.Ok) // source state (a "terminal" state today)
            .With(b => { b.BarCodeId = new BarCodeId(7); b.MachineId = new MachineId(3); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();

        var (handler, barCodeRepository, _, _, _) = BuildHandler(barcode, cycle: new CycleBuilder().FinishedOk(PartStatus.None).With(c => c.CycleId = new CycleId(901)).Build());
        var command = new RejectBarCodeCommand { Label = label };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — unconditional reject regardless of the (Finished) source state.
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.Rejected);
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // AC 6 / matrix row 4 — Reject when the label is NOT found:
    // returns a FAILURE Result with message "BarCode not found {label}" and performs no mutation
    // (UpdateAsync / AddAsync never called).
    // ----------------------------------------------------------------------------------
    [Fact]
    public async Task Reject_LabelNotFound_ReturnsFailure_NoMutation()
    {
        // Arrange
        const string label = "BC-DOES-NOT-EXIST";
        var (handler, barCodeRepository, requestRepository, _, _) = BuildHandler(barcode: null);
        var command = new RejectBarCodeCommand { Label = label };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"BarCode not found {label}");
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // AC 8 — most-recent-cycle lookup characterization for Reject (AS-BUILT SURPRISE):
    // the analysis doc said Reject "still logs without cycle fields" when no cycle exists, but the
    // current production handler (RejectBarCodeCommandHandler.cs:82-85) treats the cycle as
    // MANDATORY: when no cycle is found the reject FAILS with "Cycles for BarCode {label} not found"
    // and the barcode is NOT mutated. Pinned AS-IS (feeds Story 1.3).
    // ----------------------------------------------------------------------------------
    [Fact]
    public async Task Reject_WhenNoCycle_FailsCycleNotFound_AndDoesNotPersist()
    {
        // Arrange
        const string label = "BC-REJECT-NOCYCLE";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(11); b.MachineId = new MachineId(2); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepository, requestRepository, _, _) = BuildHandler(barcode, cycle: null);
        var command = new RejectBarCodeCommand { Label = label };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — cycle is mandatory for reject in the current build.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Cycles for BarCode {label} not found");
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess); // untouched
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Builds a <see cref="RejectBarCodeCommandHandler"/> with NSubstitute-mocked repositories.
    /// The barcode lookup returns <paramref name="barcode"/> (or null for not-found); the cycle
    /// lookup returns <paramref name="cycle"/> (or null when none).
    /// </summary>
    private static (RejectBarCodeCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo, IReadOnlyRepository<Cycle> CycleRepo, IDateTimeMachine Clock) BuildHandler(BarCode? barcode, Cycle? cycle = null)
    {
        var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();

        dateTimeMachine.Now.Returns(FixedNow);

        barCodeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barcode)));
        barCodeRepository.UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        cycleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle?>.Success(cycle)));

        requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        var handler = new RejectBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine);
        return (handler, barCodeRepository, requestRepository, cycleRepository, dateTimeMachine);
    }
}
