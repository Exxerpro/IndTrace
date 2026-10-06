// <copyright file="MarkInvalidGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Webapp;

using Application.UnitTests.TestDoubles;
using IndTrace.Application.BarCodes.Commands.MarkInvalid;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Issue #175 (epic #174) — golden-master (characterization) tests for the WEBAPP / MONITOR path
/// MARK-INVALID handler: <see cref="MarkInvalidCommandHandler"/>. No golden tests existed for this handler;
/// this file pins EVERY branch AS-BUILT before the functional-pipeline refactor: barcode not found, the
/// machine rejection (gate OFF default / illegal source with gate ON), the cycle-conditional TryAuditAsync
/// (audit only when a cycle exists; "MarkInvalid audit write did not land" on a drop), the persist failure,
/// and the EnableInvalidState-ON success. There is NO Route* flag for MarkInvalid — the machine is ALWAYS
/// fired. NO production code is changed by this story.
///
/// The "succeeded without a transition outcome" branch is UNREACHABLE for the same reason documented on
/// <see cref="RejectBarCodeStateMachineGoldenMasterTests"/> (a success <c>Result&lt;TransitionOutcome&gt;</c>
/// with a null Value cannot be constructed with IndQuestResults 1.7.0).
/// </summary>
public class MarkInvalidGoldenMasterTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);

    private static IOptions<StateMachineRoutingOptions> GateOn() =>
        Options.Create(new StateMachineRoutingOptions { EnableInvalidState = true });

    /// <summary>
    /// Label not found: FAILURE "BarCode not found {label}" and no mutation, audit, or cycle lookup.
    /// </summary>
    [Fact]
    public async Task MarkInvalid_LabelNotFound_Failure_NoMutation()
    {
        // Arrange
        const string label = "BC-INVALID-MISSING";
        var (handler, barCodeRepository, requestRepository, cycleRepository, _, _, _) = BuildHandler(barcode: null);

        // Act
        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"BarCode not found {label}");
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        await cycleRepository.DidNotReceive().FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Gate OFF (DEFAULT, fail-closed): even a would-be-legal MarkInvalid (InProcess source) is rejected by
    /// the CompletenessGateGuard with OperationCancelled (-65536). With a cycle present the rejection audit
    /// IS written (TryAuditAsync) carrying the specific code; nothing is persisted.
    /// </summary>
    [Fact]
    public async Task MarkInvalid_GateOffDefault_Rejects_WithCycle_AuditsOperationCancelled()
    {
        // Arrange — default construction: no options => EnableInvalidState false => gate OFF.
        const string label = "BC-INVALID-GATEOFF";
        var barcode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(930)).Build();
        var (handler, barCodeRepository, requestRepository, _, clock, _, audits) = BuildHandler(barcode, cycle);

        // Act
        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — exact failure message string (the golden contract of this branch).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"MarkInvalid rejected by state machine for BarCode {label}: (InProcess, MarkInvalid) -> OperationCancelled (-65536)");

        // Nothing persisted; barcode untouched.
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);

        // Rejection audit via TryAuditAsync (cycle exists): specific code + MarkInvalid trigger.
        audits.Count.ShouldBe(1);
        await requestRepository.Received(1).AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        var audit = audits[0];
        audit.ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
        audit.GatewayTask.ShouldBe(GatewayTask.MarkInvalid);
        audit.FlowStatus.ShouldBe(FlowStatus.InProcess);
        audit.PartStatus.ShouldBe(PartStatus.Ok);
        audit.MachineId.ShouldBe(5);
        audit.BarCodeId.ShouldBe(42);
        audit.CycleId.ShouldBe(930);
        audit.CycleStatus.ShouldBe(CycleStatus.Started);
        audit.TimeStamp.ShouldBe(clock.Now.ToLocalTime());
    }

    /// <summary>
    /// Rejection WITHOUT a cycle: TryAuditAsync silently skips the audit (no AddAsync, no log), while the
    /// failure outcome stays byte-identical.
    /// </summary>
    [Fact]
    public async Task MarkInvalid_Rejection_WithoutCycle_AuditSkipped()
    {
        // Arrange
        const string label = "BC-INVALID-NOCYCLE";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, _, requestRepository, _, _, logger, _) = BuildHandler(barcode, cycle: null);

        // Act
        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"MarkInvalid rejected by state machine for BarCode {label}: (InProcess, MarkInvalid) -> OperationCancelled (-65536)");
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        logger.GetLogCount(LogLevel.Error).ShouldBe(0);
    }

    /// <summary>
    /// Gate ON but the source is TERMINAL (Finished): no (Finished, MarkInvalid) table row exists, so the
    /// machine default-rejects with OperationCancelled even with the gate enabled.
    /// </summary>
    [Fact]
    public async Task MarkInvalid_GateOn_IllegalSource_Finished_TableRejected()
    {
        // Arrange
        const string label = "BC-INVALID-FINISHED";
        var barcode = new BarCodeBuilder().Finished(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().FinishedOk().With(c => c.CycleId = new CycleId(931)).Build();
        var (handler, barCodeRepository, _, _, _, _, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn());

        // Act
        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"MarkInvalid rejected by state machine for BarCode {label}: (Finished, MarkInvalid) -> OperationCancelled (-65536)");
        barcode.FlowStatus.ShouldBe(FlowStatus.Finished);
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
    }

    /// <summary>
    /// The TryAuditAsync write is BEST-EFFORT: when AddAsync fails on the rejection path the handler logs
    /// "MarkInvalid audit write did not land" at Error and still returns the SAME machine failure.
    /// </summary>
    [Fact]
    public async Task MarkInvalid_RejectionAuditDrop_LogsError_FailureOutcomeUnchanged()
    {
        // Arrange
        const string label = "BC-INVALID-AUDITDROP";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(932)).Build();
        var (handler, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"MarkInvalid rejected by state machine for BarCode {label}: (InProcess, MarkInvalid) -> OperationCancelled (-65536)");
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("MarkInvalid audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// Gate ON, legal MarkInvalid from Created: FlowStatus.Invalid (8) is persisted on the barcode with a
    /// refreshed ModifiedOn; the success audit carries ResultValidation.None (NOT the machine's Invalid(-1)
    /// success diagnostic — the handler explicitly audits None) + GatewayTask.MarkInvalid with the
    /// POST-mutation Invalid flow; the view projects the marked barcode.
    /// </summary>
    [Fact]
    public async Task MarkInvalid_GateOn_Success_FromCreated_WritesInvalid_AuditsNone_ReturnsView()
    {
        // Arrange
        const string label = "BC-INVALID-SUCCESS";
        var barcode = new BarCodeBuilder()
            .Created(PartStatus.None)
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(933)).Build();
        var (handler, barCodeRepository, _, _, clock, _, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn());

        // Act
        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — persisted truth.
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.Invalid);
        barcode.ModifiedOn.ShouldBe(clock.Now.ToLocalTime());
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());

        // Success audit: None (not the machine's Invalid(-1) outcome diagnostic), post-mutation flow.
        audits.Count.ShouldBe(1);
        var audit = audits[0];
        audit.ResultValidation.ShouldBe(ResultValidation.None);
        audit.GatewayTask.ShouldBe(GatewayTask.MarkInvalid);
        audit.FlowStatus.ShouldBe(FlowStatus.Invalid);
        audit.CycleId.ShouldBe(933);

        // View DTO.
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ShouldBeOfType<BarCodeMarkedInvalidView>();
        result.Value.FlowStatus.ShouldBe(FlowStatus.Invalid);
        result.Value.Label.ShouldBe(label);
        result.Value.BarCodeId.ShouldBe(42);
        result.Value.MachineId.ShouldBe(5);
    }

    /// <summary>
    /// Gate ON, legal MarkInvalid with NO cycle: the operation still SUCCEEDS (the success audit is simply
    /// skipped by TryAuditAsync) and the barcode is persisted as Invalid.
    /// </summary>
    [Fact]
    public async Task MarkInvalid_GateOn_Success_NoCycle_AuditSkipped_StillSuccess()
    {
        // Arrange
        const string label = "BC-INVALID-SUCCESS-NOCYCLE";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepository, requestRepository, _, _, _, _) = BuildHandler(barcode, cycle: null, routingOptions: GateOn());

        // Act
        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.Invalid);
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A failed barcode UpdateAsync FAILS the operation with the repository's errors; TryAuditAsync is never
    /// reached (no cycle lookup, no audit) and — AS-BUILT — nothing is logged for the persist failure.
    /// </summary>
    [Fact]
    public async Task MarkInvalid_GateOn_UpdateFails_PropagatesErrors_NoAuditAttempt()
    {
        // Arrange
        const string label = "BC-INVALID-PERSISTFAIL";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(934)).Build();
        var (handler, _, requestRepository, cycleRepository, _, logger, _) = BuildHandler(barcode, cycle, routingOptions: GateOn(), barcodeUpdateFails: true);

        // Act
        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — errors propagated verbatim; the in-memory mutation had already happened.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("MarkInvalid status persist failed");
        barcode.FlowStatus.ShouldBe(FlowStatus.Invalid); // in-memory mutation before the failed persist
        await cycleRepository.DidNotReceive().FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        logger.GetLogCount(LogLevel.Error).ShouldBe(0); // AS-BUILT: no log on the persist failure
    }

    /// <summary>
    /// The SUCCESS audit is BEST-EFFORT: a failed AddAsync after the barcode persisted Invalid logs
    /// "MarkInvalid audit write did not land" at Error and the handler STILL returns the success view.
    /// </summary>
    [Fact]
    public async Task MarkInvalid_SuccessAuditDrop_LogsError_SuccessOutcomeUnchanged()
    {
        // Arrange
        const string label = "BC-INVALID-SUCCESS-AUDITDROP";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(935)).Build();
        var (handler, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn(), auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().FlowStatus.ShouldBe(FlowStatus.Invalid);
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("MarkInvalid audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// Builds a <see cref="MarkInvalidCommandHandler"/> with NSubstitute-mocked repositories, an
    /// always-injected capturing <see cref="TestLogger{T}"/> and (optionally) an injected machine/options.
    /// Passing null machine/options reproduces the DI defaults (as-built engine, EnableInvalidState OFF).
    /// </summary>
    private static (MarkInvalidCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo, IReadOnlyRepository<Cycle> CycleRepo, IDateTimeMachine Clock, TestLogger<MarkInvalidCommandHandler> Logger, List<TaskGatewayRequest> Audits) BuildHandler(
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
        var logger = new TestLogger<MarkInvalidCommandHandler>();
        var audits = new List<TaskGatewayRequest>();

        dateTimeMachine.Now.Returns(FixedNow);

        barCodeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barcode)));
        barCodeRepository.UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(barcodeUpdateFails
                ? Result.WithFailure("MarkInvalid status persist failed")
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

        var handler = new MarkInvalidCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, machine, routingOptions, logger);
        return (handler, barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, logger, audits);
    }
}
