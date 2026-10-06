// <copyright file="RestoreBarCodeStateMachineGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Webapp;

using Application.UnitTests.TestDoubles;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Issue #175 (epic #174) — golden-master (characterization) extension for the WEBAPP / MONITOR path
/// RESTORE handler: <see cref="RestoreBarCodeCommandHandler"/>. Sibling of
/// <see cref="RestoreBarCodeGoldenMasterTests"/> (which pins the two-mode happy path, label-not-found and
/// the cycle-optional success). This file pins the remaining branches AS-BUILT before the
/// functional-pipeline refactor: the state-machine rejection branch (with AND without a cycle — the
/// rejection audit is written ONLY when a cycle exists), the rejection-audit best-effort drop, the
/// RouteRestore OFF rollback path, the UpdateAsync-failure branch, the EnableRestoredState-ON success via
/// options only, and the success-audit best-effort drop. NO production code is changed by this story.
///
/// The "succeeded without a transition outcome" branch is UNREACHABLE for the same reason documented on
/// <see cref="RejectBarCodeStateMachineGoldenMasterTests"/> (a success <c>Result&lt;TransitionOutcome&gt;</c>
/// with a null Value cannot be constructed with IndQuestResults 1.7.0).
/// </summary>
public class RestoreBarCodeStateMachineGoldenMasterTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);

    /// <summary>
    /// RouteRestore ON (default): firing Restore on a non-Rejected barcode (InProcess) has no table row, so
    /// the machine default-rejects with OperationCancelled (-65536). With a cycle PRESENT the handler writes
    /// ONE rejection-audit row carrying the specific code, persists nothing, and returns the exact failure.
    /// </summary>
    [Fact]
    public async Task Restore_StateMachineRejects_OnNonRejected_WithCycle_AuditWritten()
    {
        // Arrange
        const string label = "BC-RESTORE-SM-INPROCESS";
        var barcode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(910)).Build();
        var (handler, barCodeRepository, requestRepository, _, clock, _, audits) = BuildHandler(barcode, cycle);

        // Act
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — exact failure message string.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Restore rejected by state machine for BarCode {label}: (InProcess, RestorePartAsync) -> OperationCancelled (-65536)");

        // Nothing persisted; the rejection audit carries the specific code + the cycle fields.
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess); // unchanged
        audits.Count.ShouldBe(1);
        await requestRepository.Received(1).AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        var audit = audits[0];
        audit.ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
        audit.GatewayTask.ShouldBe(GatewayTask.RestorePartAsync);
        audit.FlowStatus.ShouldBe(FlowStatus.InProcess);
        audit.MachineId.ShouldBe(5);
        audit.BarCodeId.ShouldBe(42);
        audit.CycleId.ShouldBe(910);
        audit.CycleStatus.ShouldBe(CycleStatus.Started);
        audit.TimeStamp.ShouldBe(clock.Now.ToLocalTime());
    }

    /// <summary>
    /// Same state-machine rejection WITHOUT a cycle: the rejection audit is SKIPPED entirely (no AddAsync),
    /// but the failure outcome is byte-identical — the audit is conditional on the cycle, not the failure.
    /// </summary>
    [Fact]
    public async Task Restore_StateMachineRejects_OnNonRejected_WithoutCycle_AuditSkipped()
    {
        // Arrange
        const string label = "BC-RESTORE-SM-NOCYCLE";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepository, requestRepository, _, _, _, _) = BuildHandler(barcode, cycle: null);

        // Act
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — same failure, no audit row attempted.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Restore rejected by state machine for BarCode {label}: (InProcess, RestorePartAsync) -> OperationCancelled (-65536)");
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The rejection-audit write is BEST-EFFORT: when AddAsync fails the handler logs
    /// "Restore rejection-audit write did not land" at Error and still returns the SAME state-machine failure.
    /// </summary>
    [Fact]
    public async Task Restore_RejectionAuditDrop_LogsError_FailureOutcomeUnchanged()
    {
        // Arrange
        const string label = "BC-RESTORE-SM-AUDITDROP";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(911)).Build();
        var (handler, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Restore rejected by state machine for BarCode {label}: (InProcess, RestorePartAsync) -> OperationCancelled (-65536)");
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Restore rejection-audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// RouteRestore OFF (zero-redeploy rollback): the machine is NEVER fired and the legacy unconditional
    /// inline mutation applies FlowStatus.InProcess regardless of the source state (here: Finished). The
    /// operation succeeds, persists, and audits ResultValidation.None.
    /// </summary>
    [Fact]
    public async Task Restore_RouteRestoreOff_NeverFiresMachine_WritesInProcessUnconditionally()
    {
        // Arrange — a rejecting machine that must never be consulted.
        const string label = "BC-RESTORE-FLAGOFF";
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure("must not be called"));
        var options = Options.Create(new StateMachineRoutingOptions { RouteRestore = false });
        var barcode = new BarCodeBuilder().Finished(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().FinishedOk(PartStatus.None).With(c => c.CycleId = new CycleId(912)).Build();
        var (handler, barCodeRepository, _, _, clock, _, audits) = BuildHandler(barcode, cycle, machine, options);

        // Act
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — flag OFF: no fire, unconditional InProcess write (even from Finished), audit with None.
        machine.DidNotReceive().Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>());
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barcode.ModifiedOn.ShouldBe(clock.Now.ToLocalTime());
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.None);
        audits[0].GatewayTask.ShouldBe(GatewayTask.RestorePartAsync);
        audits[0].FlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// A failed barcode UpdateAsync FAILS the operation (issue #65): the handler logs
    /// "Failed to persist Restore status" at Error, returns WithFailure(updateResult.Errors), and neither
    /// the cycle lookup nor the success audit is ever attempted (both sit after the persist).
    /// </summary>
    [Fact]
    public async Task Restore_BarCodeUpdateFails_PropagatesErrors_LogsError_NoAudit()
    {
        // Arrange — legal restore (Rejected) whose persist fails.
        const string label = "BC-RESTORE-PERSISTFAIL";
        var barcode = new BarCodeBuilder().Rejected(PartStatus.Rejected).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, _, requestRepository, cycleRepository, _, logger, _) = BuildHandler(barcode, cycle: null, barcodeUpdateFails: true);

        // Act
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — errors propagated verbatim; the post-persist audit pipeline never runs.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Restore status persist failed");
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Failed to persist Restore status").ShouldBeTrue();
        await cycleRepository.DidNotReceive().FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// EnableRestoredState ON via OPTIONS ONLY (no decorator/sink — the plain DI shape): a legal restore
    /// from Rejected resolves the gated computed target FlowStatus.Restored (16), persists it, and the
    /// success audit snapshots the RESTORED flow with ResultValidation.None.
    /// </summary>
    [Fact]
    public async Task Restore_GateOn_EnableRestoredState_WritesRestored_AuditsRestoredFlow()
    {
        // Arrange
        const string label = "BC-RESTORE-GATEON-OPTIONS";
        var options = Options.Create(new StateMachineRoutingOptions { EnableRestoredState = true });
        var barcode = new BarCodeBuilder().Rejected(PartStatus.Rejected).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(913)).Build();
        var (handler, barCodeRepository, _, _, _, _, audits) = BuildHandler(barcode, cycle, routingOptions: options);

        // Act
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — gate ON resolves Restored (16), persisted; the audit snapshots the new flow.
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.Restored);
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().FlowStatus.ShouldBe(FlowStatus.Restored);
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.None);
        audits[0].FlowStatus.ShouldBe(FlowStatus.Restored);
        audits[0].GatewayTask.ShouldBe(GatewayTask.RestorePartAsync);
    }

    /// <summary>
    /// The SUCCESS audit is BEST-EFFORT: a failed AddAsync after a persisted legal restore logs
    /// "Restore success-audit write did not land" at Error and the handler STILL returns the success view.
    /// </summary>
    [Fact]
    public async Task Restore_SuccessAuditDrop_LogsError_SuccessOutcomeUnchanged()
    {
        // Arrange
        const string label = "BC-RESTORE-SUCCESS-AUDITDROP";
        var barcode = new BarCodeBuilder().Rejected(PartStatus.Rejected).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(914)).Build();
        var (handler, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — success unchanged (as-built default gate OFF: Rejected -> InProcess).
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().FlowStatus.ShouldBe(FlowStatus.InProcess);
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Restore success-audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// Builds a <see cref="RestoreBarCodeCommandHandler"/> with NSubstitute-mocked repositories, an
    /// always-injected capturing <see cref="TestLogger{T}"/> and (optionally) an injected machine/options.
    /// Passing null machine/options reproduces the DI defaults (as-built engine, default-ON routing,
    /// EnableRestoredState OFF).
    /// </summary>
    private static (RestoreBarCodeCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo, IReadOnlyRepository<Cycle> CycleRepo, IDateTimeMachine Clock, TestLogger<RestoreBarCodeCommandHandler> Logger, List<TaskGatewayRequest> Audits) BuildHandler(
        BarCode? barcode,
        Cycle? cycle = null,
        IItemStateMachine? machine = null,
        IOptions<StateMachineRoutingOptions>? routingOptions = null,
        bool auditWriteFails = false,
        bool barcodeUpdateFails = false)
    {
        var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var logger = new TestLogger<RestoreBarCodeCommandHandler>();
        var audits = new List<TaskGatewayRequest>();

        dateTimeMachine.Now.Returns(FixedNow);

        barCodeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barcode)));
        barCodeRepository.UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(barcodeUpdateFails
                ? Result.WithFailure("Restore status persist failed")
                : Result.Success()));

        cycleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle?>.Success(cycle)));

        requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                audits.Add(ci.Arg<TaskGatewayRequest>());
                return Task.FromResult(auditWriteFails
                    ? Result<int>.WithFailure("audit write failed")
                    : Result<int>.Success(1));
            });

        var handler = new RestoreBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, machine, routingOptions, logger);
        return (handler, barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, logger, audits);
    }
}
