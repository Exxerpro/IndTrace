// <copyright file="MarkScrapGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Webapp;

using Application.UnitTests.TestDoubles;
using IndTrace.Application.BarCodes.Commands.MarkScrap;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Issue #175 (epic #174) — golden-master (characterization) tests for the WEBAPP / MONITOR path
/// MARK-SCRAP handler: <see cref="MarkScrapCommandHandler"/>. No golden tests existed for this handler;
/// this file pins EVERY branch AS-BUILT before the functional-pipeline refactor. Same shape as MarkInvalid
/// but the resolved PART status (Scrap, 512) is persisted via the part seam (the FlowStatus is unchanged)
/// and the view is a <see cref="BarCodeMarkedScrapView"/>. Gated by EnableScrapState; the PartScrappableGuard
/// additionally rejects already-Scrap/Rejected parts even with the gate ON. There is NO Route* flag for
/// MarkScrap — the machine is ALWAYS fired. NO production code is changed by this story.
///
/// The "succeeded without a transition outcome" branch is UNREACHABLE for the same reason documented on
/// <see cref="RejectBarCodeStateMachineGoldenMasterTests"/> (a success <c>Result&lt;TransitionOutcome&gt;</c>
/// with a null Value cannot be constructed with IndQuestResults 1.7.0).
/// </summary>
public class MarkScrapGoldenMasterTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);

    private static IOptions<StateMachineRoutingOptions> GateOn() =>
        Options.Create(new StateMachineRoutingOptions { EnableScrapState = true });

    /// <summary>
    /// Label not found: FAILURE "BarCode not found {label}" and no mutation, audit, or cycle lookup.
    /// </summary>
    [Fact]
    public async Task MarkScrap_LabelNotFound_Failure_NoMutation()
    {
        // Arrange
        const string label = "BC-SCRAP-MISSING";
        var (handler, barCodeRepository, requestRepository, cycleRepository, _, _, _) = BuildHandler(barcode: null);

        // Act
        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"BarCode not found {label}");
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        await cycleRepository.DidNotReceive().FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Gate OFF (DEFAULT, fail-closed): even a would-be-legal MarkScrap (InProcess, scrappable Ok part) is
    /// rejected by the CompletenessGateGuard with OperationCancelled (-65536). With a cycle present the
    /// rejection audit IS written (TryAuditAsync) carrying the specific code; nothing is persisted.
    /// </summary>
    [Fact]
    public async Task MarkScrap_GateOffDefault_Rejects_WithCycle_AuditsOperationCancelled()
    {
        // Arrange — default construction: no options => EnableScrapState false => gate OFF.
        const string label = "BC-SCRAP-GATEOFF";
        var barcode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(940)).Build();
        var (handler, barCodeRepository, requestRepository, _, clock, _, audits) = BuildHandler(barcode, cycle);

        // Act
        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — exact failure message string (the golden contract of this branch).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"MarkScrap rejected by state machine for BarCode {label}: (InProcess, MarkScrap) -> OperationCancelled (-65536)");

        // Nothing persisted; part untouched.
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        barcode.PartStatus.ShouldBe(PartStatus.Ok);

        // Rejection audit via TryAuditAsync (cycle exists).
        audits.Count.ShouldBe(1);
        await requestRepository.Received(1).AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        var audit = audits[0];
        audit.ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
        audit.GatewayTask.ShouldBe(GatewayTask.MarkScrap);
        audit.FlowStatus.ShouldBe(FlowStatus.InProcess);
        audit.PartStatus.ShouldBe(PartStatus.Ok);
        audit.MachineId.ShouldBe(5);
        audit.BarCodeId.ShouldBe(42);
        audit.CycleId.ShouldBe(940);
        audit.CycleStatus.ShouldBe(CycleStatus.Started);
        audit.TimeStamp.ShouldBe(clock.Now.ToLocalTime());
    }

    /// <summary>
    /// Gate ON but the part is ALREADY Scrap: the PartScrappableGuard rejects with OperationCancelled even
    /// with the gate enabled; the part stays Scrap and nothing is persisted.
    /// </summary>
    [Fact]
    public async Task MarkScrap_GateOn_AlreadyScrap_GuardRejects()
    {
        // Arrange
        const string label = "BC-SCRAP-ALREADY";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Scrap).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(941)).Build();
        var (handler, barCodeRepository, _, _, _, _, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn());

        // Act
        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"MarkScrap rejected by state machine for BarCode {label}: (InProcess, MarkScrap) -> OperationCancelled (-65536)");
        barcode.PartStatus.ShouldBe(PartStatus.Scrap); // unchanged
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
    }

    /// <summary>
    /// Rejection WITHOUT a cycle: TryAuditAsync silently skips the audit (no AddAsync, no log), while the
    /// failure outcome stays byte-identical.
    /// </summary>
    [Fact]
    public async Task MarkScrap_Rejection_WithoutCycle_AuditSkipped()
    {
        // Arrange
        const string label = "BC-SCRAP-NOCYCLE";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, _, requestRepository, _, _, logger, _) = BuildHandler(barcode, cycle: null);

        // Act
        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"MarkScrap rejected by state machine for BarCode {label}: (InProcess, MarkScrap) -> OperationCancelled (-65536)");
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        logger.GetLogCount(LogLevel.Error).ShouldBe(0);
    }

    /// <summary>
    /// The TryAuditAsync write is BEST-EFFORT: when AddAsync fails on the rejection path the handler logs
    /// "MarkScrap audit write did not land" at Error and still returns the SAME machine failure.
    /// </summary>
    [Fact]
    public async Task MarkScrap_RejectionAuditDrop_LogsError_FailureOutcomeUnchanged()
    {
        // Arrange
        const string label = "BC-SCRAP-AUDITDROP";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(942)).Build();
        var (handler, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"MarkScrap rejected by state machine for BarCode {label}: (InProcess, MarkScrap) -> OperationCancelled (-65536)");
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("MarkScrap audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// Gate ON, legal MarkScrap from InProcess: PartStatus.Scrap (512) is persisted on the barcode (the
    /// FlowStatus stays InProcess) with a refreshed ModifiedOn; the success audit carries
    /// ResultValidation.None + GatewayTask.MarkScrap with the POST-mutation Scrap part; the view projects
    /// the scrapped barcode.
    /// </summary>
    [Fact]
    public async Task MarkScrap_GateOn_Success_FromInProcess_WritesScrapPart_AuditsNone_ReturnsView()
    {
        // Arrange
        const string label = "BC-SCRAP-SUCCESS";
        var barcode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(943)).Build();
        var (handler, barCodeRepository, _, _, clock, _, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn());

        // Act
        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — persisted truth: PART status changed, FLOW status untouched.
        result.IsSuccess.ShouldBeTrue();
        barcode.PartStatus.ShouldBe(PartStatus.Scrap);
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barcode.ModifiedOn.ShouldBe(clock.Now.ToLocalTime());
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());

        // Success audit: None, post-mutation Scrap part.
        audits.Count.ShouldBe(1);
        var audit = audits[0];
        audit.ResultValidation.ShouldBe(ResultValidation.None);
        audit.GatewayTask.ShouldBe(GatewayTask.MarkScrap);
        audit.PartStatus.ShouldBe(PartStatus.Scrap);
        audit.FlowStatus.ShouldBe(FlowStatus.InProcess);
        audit.CycleId.ShouldBe(943);

        // View DTO.
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ShouldBeOfType<BarCodeMarkedScrapView>();
        result.Value.PartStatus.ShouldBe(PartStatus.Scrap);
        result.Value.FlowStatus.ShouldBe(FlowStatus.InProcess);
        result.Value.Label.ShouldBe(label);
        result.Value.BarCodeId.ShouldBe(42);
        result.Value.MachineId.ShouldBe(5);
    }

    /// <summary>
    /// Gate ON, legal MarkScrap from Finished (the second table row): the part becomes Scrap while the
    /// Finished flow status is preserved.
    /// </summary>
    [Fact]
    public async Task MarkScrap_GateOn_Success_FromFinished_FlowUnchanged()
    {
        // Arrange
        const string label = "BC-SCRAP-FINISHED";
        var barcode = new BarCodeBuilder().Finished(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().FinishedOk().With(c => c.CycleId = new CycleId(944)).Build();
        var (handler, barCodeRepository, _, _, _, _, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn());

        // Act
        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barcode.PartStatus.ShouldBe(PartStatus.Scrap);
        barcode.FlowStatus.ShouldBe(FlowStatus.Finished);
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.None);
        audits[0].FlowStatus.ShouldBe(FlowStatus.Finished);
    }

    /// <summary>
    /// A failed barcode UpdateAsync FAILS the operation with the repository's errors; TryAuditAsync is never
    /// reached (no cycle lookup, no audit) and — AS-BUILT — nothing is logged for the persist failure.
    /// </summary>
    [Fact]
    public async Task MarkScrap_GateOn_UpdateFails_PropagatesErrors_NoAuditAttempt()
    {
        // Arrange
        const string label = "BC-SCRAP-PERSISTFAIL";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(945)).Build();
        var (handler, _, requestRepository, cycleRepository, _, logger, _) = BuildHandler(barcode, cycle, routingOptions: GateOn(), barcodeUpdateFails: true);

        // Act
        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — errors propagated verbatim; the in-memory mutation had already happened.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("MarkScrap status persist failed");
        barcode.PartStatus.ShouldBe(PartStatus.Scrap); // in-memory mutation before the failed persist
        await cycleRepository.DidNotReceive().FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        logger.GetLogCount(LogLevel.Error).ShouldBe(0); // AS-BUILT: no log on the persist failure
    }

    /// <summary>
    /// The SUCCESS audit is BEST-EFFORT: a failed AddAsync after the barcode persisted Scrap logs
    /// "MarkScrap audit write did not land" at Error and the handler STILL returns the success view.
    /// </summary>
    [Fact]
    public async Task MarkScrap_SuccessAuditDrop_LogsError_SuccessOutcomeUnchanged()
    {
        // Arrange
        const string label = "BC-SCRAP-SUCCESS-AUDITDROP";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(946)).Build();
        var (handler, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn(), auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().PartStatus.ShouldBe(PartStatus.Scrap);
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("MarkScrap audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// Builds a <see cref="MarkScrapCommandHandler"/> with NSubstitute-mocked repositories, an
    /// always-injected capturing <see cref="TestLogger{T}"/> and (optionally) an injected machine/options.
    /// Passing null machine/options reproduces the DI defaults (as-built engine, EnableScrapState OFF).
    /// </summary>
    private static (MarkScrapCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo, IReadOnlyRepository<Cycle> CycleRepo, IDateTimeMachine Clock, TestLogger<MarkScrapCommandHandler> Logger, List<TaskGatewayRequest> Audits) BuildHandler(
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
        var logger = new TestLogger<MarkScrapCommandHandler>();
        var audits = new List<TaskGatewayRequest>();

        dateTimeMachine.Now.Returns(FixedNow);

        barCodeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barcode)));
        barCodeRepository.UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(barcodeUpdateFails
                ? Result.WithFailure("MarkScrap status persist failed")
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

        var handler = new MarkScrapCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, machine, routingOptions, logger);
        return (handler, barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, logger, audits);
    }
}
