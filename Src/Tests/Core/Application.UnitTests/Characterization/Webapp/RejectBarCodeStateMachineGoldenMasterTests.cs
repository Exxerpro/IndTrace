// <copyright file="RejectBarCodeStateMachineGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
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
/// REJECT handler: <see cref="RejectBarCodeCommandHandler"/>. Sibling of
/// <see cref="RejectBarCodeGoldenMasterTests"/> (which pins the happy paths, label-not-found and the
/// mandatory-cycle branch). This file pins the remaining branches AS-BUILT before the functional-pipeline
/// refactor: the state-machine rejection branch (RouteReject ON), the machine-code carry/fallback, the
/// rejection-audit best-effort drop, the RouteReject OFF rollback path, the UpdateAsync-failure branch and
/// the success-audit best-effort drop. NO production code is changed by this story.
///
/// NOTE (pinned as evidence, not as a test): the "succeeded without a transition outcome" branch
/// (RejectBarCodeCommandHandler, <c>outcome.Value is not { } transition</c>) is UNREACHABLE — IndQuestResults
/// 1.7.0 <c>Result&lt;T&gt;</c> is sealed with non-virtual members and its constructor/factories demote any
/// success carrying a null value to a failure (verified empirically:
/// <c>new Result&lt;string&gt;(true, [], null, null)</c> yields <c>IsSuccess == false</c>), so no
/// <c>IItemStateMachine</c> substitute can produce a success whose <c>Value</c> is null.
/// </summary>
public class RejectBarCodeStateMachineGoldenMasterTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);

    /// <summary>
    /// RouteReject ON (default): firing Reject on an already-Rejected barcode has no table row, so the
    /// machine default-rejects with OperationCancelled (-65536). The handler persists NOTHING (no
    /// UpdateAsync), writes ONE rejection-audit TaskGatewayRequest carrying the SPECIFIC code (not None),
    /// and returns a value-less failure with the exact "(From, Trigger) -&gt; Code (Value)" message.
    /// </summary>
    [Fact]
    public async Task Reject_StateMachineRejects_OnAlreadyRejected_AuditsSpecificCode_NoPersist()
    {
        // Arrange
        const string label = "BC-REJECT-SM-REJECTED";
        var barcode = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected)
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(900)).Build();
        var (handler, barCodeRepository, requestRepository, _, clock, _, audits) = BuildHandler(barcode, cycle);

        // Act
        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — exact failure message string (the golden contract of this branch).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Reject rejected by state machine for BarCode {label}: (Rejected, RejectPartAsync) -> OperationCancelled (-65536)");

        // Nothing persisted on the barcode.
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        barcode.FlowStatus.ShouldBe(FlowStatus.Rejected); // unchanged

        // Rejection-audit row: SPECIFIC code, Reject trigger, current (unchanged) statuses, cycle fields.
        audits.Count.ShouldBe(1);
        await requestRepository.Received(1).AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        var audit = audits[0];
        audit.ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
        audit.GatewayTask.ShouldBe(GatewayTask.RejectPartAsync);
        audit.FlowStatus.ShouldBe(FlowStatus.Rejected);
        audit.PartStatus.ShouldBe(PartStatus.Rejected);
        audit.MachineId.ShouldBe(5);
        audit.BarCodeId.ShouldBe(42);
        audit.CycleId.ShouldBe(900);
        audit.CycleStatus.ShouldBe(CycleStatus.Started);
        audit.TimeStamp.ShouldBe(clock.Now.ToLocalTime());
    }

    /// <summary>
    /// When the injected machine's failure OUTCOME carries a specific code, the handler publishes THAT code
    /// (message and audit), not the OperationCancelled fallback — the rejectCode comes from
    /// <c>outcome.Value?.Result</c>.
    /// </summary>
    [Fact]
    public async Task Reject_StateMachineRejects_CarriesInjectedMachineCode()
    {
        // Arrange — machine rejects with BarCodeNotFound (-2) on the outcome value.
        const string label = "BC-REJECT-SM-CODE";
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure(
                "machine rejected",
                new TransitionOutcome(FlowStatus.InProcess, CycleStatus.None, PartStatus.Ok, ResultValidation.BarCodeNotFound)));
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(901)).Build();
        var (handler, barCodeRepository, _, _, _, _, audits) = BuildHandler(barcode, cycle, machine);

        // Act
        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — the machine's specific code survives to the message AND the audit row.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Reject rejected by state machine for BarCode {label}: (InProcess, RejectPartAsync) -> BarCodeNotFound (-2)");
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.BarCodeNotFound);
        machine.Received(1).Fire(Arg.Any<BarCode>(), GatewayTask.RejectPartAsync, Arg.Any<TransitionContext>());
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When the injected machine fails VALUE-LESS (<c>outcome.Value</c> is null), the rejectCode falls back
    /// to OperationCancelled (-65536) in both the message and the audit row.
    /// </summary>
    [Fact]
    public async Task Reject_StateMachineRejects_ValuelessFailure_FallsBackToOperationCancelled()
    {
        // Arrange
        const string label = "BC-REJECT-SM-VALUELESS";
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure("machine rejected without outcome"));
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(902)).Build();
        var (handler, _, _, _, _, _, audits) = BuildHandler(barcode, cycle, machine);

        // Act
        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Reject rejected by state machine for BarCode {label}: (InProcess, RejectPartAsync) -> OperationCancelled (-65536)");
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
    }

    /// <summary>
    /// The rejection-audit write is BEST-EFFORT: when AddAsync fails, the handler logs
    /// "Reject rejection-audit write did not land" at Error and still returns the SAME state-machine
    /// failure (the audit fault never clobbers the specific rejection outcome).
    /// </summary>
    [Fact]
    public async Task Reject_RejectionAuditDrop_LogsError_FailureOutcomeUnchanged()
    {
        // Arrange — already-Rejected source (machine rejection) + failing audit write.
        const string label = "BC-REJECT-SM-AUDITDROP";
        var barcode = new BarCodeBuilder().Rejected(PartStatus.Rejected).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(903)).Build();
        var (handler, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — outcome unchanged, dropped audit logged at Error.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Reject rejected by state machine for BarCode {label}: (Rejected, RejectPartAsync) -> OperationCancelled (-65536)");
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Reject rejection-audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// RouteReject OFF (zero-redeploy rollback): the machine is NEVER fired and the legacy unconditional
    /// inline mutation applies FlowStatus.Rejected regardless of the source state (here: already Rejected).
    /// The operation succeeds, persists, and audits ResultValidation.None.
    /// </summary>
    [Fact]
    public async Task Reject_RouteRejectOff_NeverFiresMachine_WritesRejectedUnconditionally()
    {
        // Arrange — a rejecting machine that must never be consulted.
        const string label = "BC-REJECT-FLAGOFF";
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure("must not be called"));
        var options = Options.Create(new StateMachineRoutingOptions { RouteReject = false });
        var barcode = new BarCodeBuilder().Rejected(PartStatus.Rejected).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(904)).Build();
        var (handler, barCodeRepository, _, _, clock, _, audits) = BuildHandler(barcode, cycle, machine, options);

        // Act
        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — flag OFF: no fire, unconditional Rejected write, success audit with None.
        machine.DidNotReceive().Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>());
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.Rejected);
        barcode.ModifiedOn.ShouldBe(clock.Now.ToLocalTime());
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.None);
        audits[0].GatewayTask.ShouldBe(GatewayTask.RejectPartAsync);
    }

    /// <summary>
    /// A failed barcode UpdateAsync FAILS the operation (issue #65): the handler logs
    /// "Failed to persist Reject status" at Error, returns WithFailure(updateResult.Errors) and never
    /// writes the success audit.
    /// </summary>
    [Fact]
    public async Task Reject_BarCodeUpdateFails_PropagatesErrors_LogsError_NoAudit()
    {
        // Arrange — legal reject (InProcess) whose persist fails.
        const string label = "BC-REJECT-PERSISTFAIL";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(905)).Build();
        var (handler, _, requestRepository, _, _, logger, _) = BuildHandler(barcode, cycle, barcodeUpdateFails: true);

        // Act
        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — the repository's error text is propagated verbatim; no audit row is written.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Reject status persist failed");
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Failed to persist Reject status").ShouldBeTrue();
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The SUCCESS audit is BEST-EFFORT: a failed AddAsync after a persisted legal reject logs
    /// "Reject success-audit write did not land" at Error and the handler STILL returns the success view.
    /// </summary>
    [Fact]
    public async Task Reject_SuccessAuditDrop_LogsError_SuccessOutcomeUnchanged()
    {
        // Arrange — legal reject whose success-audit write fails.
        const string label = "BC-REJECT-SUCCESS-AUDITDROP";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(906)).Build();
        var (handler, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — success view unchanged despite the dropped audit row.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().FlowStatus.ShouldBe(FlowStatus.Rejected);
        barcode.FlowStatus.ShouldBe(FlowStatus.Rejected);
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Reject success-audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// Builds a <see cref="RejectBarCodeCommandHandler"/> with NSubstitute-mocked repositories, an
    /// always-injected capturing <see cref="TestLogger{T}"/> and (optionally) an injected machine/options.
    /// Passing null machine/options reproduces the DI defaults (as-built engine, default-ON routing).
    /// </summary>
    private static (RejectBarCodeCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo, IReadOnlyRepository<Cycle> CycleRepo, IDateTimeMachine Clock, TestLogger<RejectBarCodeCommandHandler> Logger, List<TaskGatewayRequest> Audits) BuildHandler(
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
        var logger = new TestLogger<RejectBarCodeCommandHandler>();
        var audits = new List<TaskGatewayRequest>();

        dateTimeMachine.Now.Returns(FixedNow);

        barCodeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barcode)));
        barCodeRepository.UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(barcodeUpdateFails
                ? Result.WithFailure("Reject status persist failed")
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

        var handler = new RejectBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, machine, routingOptions, logger);
        return (handler, barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, logger, audits);
    }
}
