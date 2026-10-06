// <copyright file="CancelCycleGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Webapp;

using Application.UnitTests.TestDoubles;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.BarCodes.Commands.CancelCycle;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Issue #175 (epic #174) — golden-master (characterization) tests for the WEBAPP / MONITOR path CANCEL
/// handler: <see cref="CancelCycleCommandHandler"/>. No golden tests existed for this handler; this file
/// pins EVERY branch AS-BUILT before the functional-pipeline refactor: barcode not found, mandatory cycle,
/// the machine rejection (gate OFF default / non-Started cycle / illegal source / value-less fallback), the
/// cycle-persist failure, the EnableCanceledState-ON success (status persisted ON THE CYCLE, view from the
/// cycle) and both best-effort audit drops. Unlike Reject/Restore there is NO Route* flag for Cancel — the
/// machine is ALWAYS fired. NO production code is changed by this story.
///
/// #95 Slice E updated the WRITE pins only: the resolved status is staged on the loaded BarCode root
/// (<see cref="BarCode.StageCycleStatusUpdate"/>) and persisted via
/// <see cref="IAggregateRepository{TRoot}.SaveAsync"/>; the raw <c>IRepository&lt;Cycle&gt;.UpdateAsync</c>
/// is never called. One original pin is retired by the aggregate cross-guard: the success fixture's cycle
/// must now carry the barcode's own id (43 → 42) — the MachineId 7-vs-5 mismatch still pins the view's
/// cycle provenance. Every read, machine, audit and message pin is unchanged.
///
/// The "succeeded without a transition outcome" branch is UNREACHABLE for the same reason documented on
/// <see cref="RejectBarCodeStateMachineGoldenMasterTests"/> (a success <c>Result&lt;TransitionOutcome&gt;</c>
/// with a null Value cannot be constructed with IndQuestResults 1.7.0).
/// </summary>
public class CancelCycleGoldenMasterTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);

    private static IOptions<StateMachineRoutingOptions> GateOn() =>
        Options.Create(new StateMachineRoutingOptions { EnableCanceledState = true });

    /// <summary>
    /// Label not found: FAILURE "BarCode not found {label}", the cycle lookup never runs and nothing is
    /// mutated or audited.
    /// </summary>
    [Fact]
    public async Task Cancel_LabelNotFound_Failure_NoCycleLookup_NoMutation()
    {
        // Arrange
        const string label = "BC-CANCEL-MISSING";
        var (handler, _, requestRepository, cycleRepository, aggregateRepository, _, _, _) = BuildHandler(barcode: null);

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"BarCode not found {label}");
        await cycleRepository.DidNotReceive().FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>());
        await aggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The most recent cycle is MANDATORY for Cancel (mirrors the Reject pattern): when no cycle exists the
    /// operation fails "Cycles for BarCode {label} not found" before the machine is ever consulted.
    /// </summary>
    [Fact]
    public async Task Cancel_NoCycle_Failure_CycleMandatory()
    {
        // Arrange
        const string label = "BC-CANCEL-NOCYCLE";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, _, requestRepository, _, aggregateRepository, _, _, _) = BuildHandler(barcode, cycle: null);

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Cycles for BarCode {label} not found");
        await aggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Gate OFF (DEFAULT, fail-closed): even a would-be-legal Cancel (InProcess barcode, Started cycle) is
    /// rejected by the CompletenessGateGuard with OperationCancelled (-65536). The machine IS fired (no
    /// Route* guard for Cancel), the cancel-audit records the specific code, and NOTHING is persisted.
    /// </summary>
    [Fact]
    public async Task Cancel_GateOffDefault_MachineRejects_AuditsOperationCancelled_NothingPersisted()
    {
        // Arrange — default construction: no options => EnableCanceledState false => gate OFF.
        const string label = "BC-CANCEL-GATEOFF";
        var barcode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(920)).Build();
        var (handler, _, requestRepository, _, aggregateRepository, clock, _, audits) = BuildHandler(barcode, cycle);

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — exact failure message string (the golden contract of this branch).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Cancel rejected by state machine for BarCode {label}: (InProcess, Cancel) -> OperationCancelled (-65536)");

        // Nothing persisted; the cycle status is untouched.
        await aggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        cycle.CycleStatus.ShouldBe(CycleStatus.Started);
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);

        // Cancel-audit row: specific code, Cancel trigger, pre-mutation snapshot.
        audits.Count.ShouldBe(1);
        await requestRepository.Received(1).AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        var audit = audits[0];
        audit.ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
        audit.GatewayTask.ShouldBe(GatewayTask.Cancel);
        audit.FlowStatus.ShouldBe(FlowStatus.InProcess);
        audit.PartStatus.ShouldBe(PartStatus.Ok);
        audit.MachineId.ShouldBe(5);
        audit.BarCodeId.ShouldBe(42);
        audit.CycleId.ShouldBe(920);
        audit.CycleStatus.ShouldBe(CycleStatus.Started);
        audit.TimeStamp.ShouldBe(clock.Now.ToLocalTime());
    }

    /// <summary>
    /// Gate ON but the latest cycle is NOT Started (FinishedOk): the CycleStartedGuard rejects with
    /// OperationCancelled; the audit snapshots the real (FinishedOk) cycle status; nothing is persisted.
    /// </summary>
    [Fact]
    public async Task Cancel_GateOn_CycleNotStarted_Rejected()
    {
        // Arrange
        const string label = "BC-CANCEL-NOTSTARTED";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().FinishedOk().With(c => c.CycleId = new CycleId(921)).Build();
        var (handler, _, _, _, aggregateRepository, _, _, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn());

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Cancel rejected by state machine for BarCode {label}: (InProcess, Cancel) -> OperationCancelled (-65536)");
        await aggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
        audits[0].CycleStatus.ShouldBe(CycleStatus.FinishedOk);
    }

    /// <summary>
    /// Gate ON but the barcode is NOT InProcess (Finished): no (Finished, Cancel) table row exists, so the
    /// machine default-rejects; the message pins the Finished source state.
    /// </summary>
    [Fact]
    public async Task Cancel_GateOn_NonInProcessSource_TableRejected()
    {
        // Arrange
        const string label = "BC-CANCEL-FINISHED";
        var barcode = new BarCodeBuilder().Finished(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(922)).Build();
        var (handler, _, _, _, aggregateRepository, _, _, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn());

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Cancel rejected by state machine for BarCode {label}: (Finished, Cancel) -> OperationCancelled (-65536)");
        await aggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
    }

    /// <summary>
    /// A VALUE-LESS machine failure (outcome.Value null) falls back to OperationCancelled (-65536) in both
    /// the message and the audit — and proves the machine is always fired with the Cancel trigger.
    /// </summary>
    [Fact]
    public async Task Cancel_InjectedMachine_ValuelessFailure_FallsBackToOperationCancelled()
    {
        // Arrange
        const string label = "BC-CANCEL-VALUELESS";
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure("machine rejected without outcome"));
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(923)).Build();
        var (handler, _, _, _, _, _, _, audits) = BuildHandler(barcode, cycle, machine);

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Cancel rejected by state machine for BarCode {label}: (InProcess, Cancel) -> OperationCancelled (-65536)");
        machine.Received(1).Fire(Arg.Any<BarCode>(), GatewayTask.Cancel, Arg.Any<TransitionContext>());
        audits.Count.ShouldBe(1);
        audits[0].ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
    }

    /// <summary>
    /// Gate ON, legal Cancel (InProcess barcode, Started cycle): the resolved Canceled status is persisted
    /// ON THE CYCLE (never the barcode), the success audit carries ResultValidation.None + GatewayTask.Cancel
    /// with the POST-mutation Canceled cycle status, and the returned CycleCanceledView is built from the
    /// CYCLE's ids (not the barcode's).
    /// </summary>
    [Fact]
    public async Task Cancel_GateOn_Success_PersistsCanceledOnCycle_AuditsNone_ReturnsView()
    {
        // Arrange — the cycle's MachineId deliberately differs from the barcode's to pin the view's source.
        // (#95 Slice E retired the mismatched-BarCodeId pin: StageCycleStatusUpdate's cross-guard refuses a
        // cycle that targets another barcode, so the fixture now carries the barcode's own id 42.)
        const string label = "BC-CANCEL-SUCCESS";
        var barcode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();
        var cycle = new CycleBuilder()
            .Started(PartStatus.None)
            .With(c => { c.CycleId = new CycleId(924); c.MachineId = new MachineId(7); c.BarCodeId = new BarCodeId(42); })
            .Build();
        var (handler, barCodeRepository, _, cycleRepository, aggregateRepository, _, _, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn());

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — the CYCLE carries the resolved status; the write is the aggregate save of the loaded root
        // (the staged cycle stays visible on the mock — proof it was staged); the raw repos are never written.
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.ShouldBe(CycleStatus.Canceled);
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        await aggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => ReferenceEquals(b, barcode)), Arg.Any<CancellationToken>());
        barcode.PendingCycleUpdates.ShouldContain(cycle);

        // #114 chunk C: a raw cycle write is STRUCTURALLY impossible now — the handler takes
        // IReadOnlyRepository<Cycle>, which has no write member to assert against.
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());

        // Success audit: None + Cancel trigger, POST-mutation Canceled cycle status, barcode ids for M/BC.
        audits.Count.ShouldBe(1);
        var audit = audits[0];
        audit.ResultValidation.ShouldBe(ResultValidation.None);
        audit.GatewayTask.ShouldBe(GatewayTask.Cancel);
        audit.CycleStatus.ShouldBe(CycleStatus.Canceled);
        audit.CycleId.ShouldBe(924);
        audit.MachineId.ShouldBe(5); // from the BARCODE
        audit.BarCodeId.ShouldBe(42); // from the BARCODE
        audit.FlowStatus.ShouldBe(FlowStatus.InProcess);

        // View DTO is projected from the CYCLE.
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ShouldBeOfType<CycleCanceledView>();
        result.Value.CycleId.ShouldBe(924);
        result.Value.MachineId.ShouldBe(7); // from the CYCLE, not the barcode's 5 — the surviving provenance pin
        result.Value.BarCodeId.ShouldBe(42); // now equal by the cross-guard; provenance is pinned by MachineId
        result.Value.CycleStatus.ShouldBe(CycleStatus.Canceled);
    }

    /// <summary>
    /// A failed aggregate SaveAsync FAILS the operation with the repository's errors; the success audit is
    /// never attempted and — unlike Reject/Restore — the Cancel handler logs NOTHING for a persist failure.
    /// </summary>
    [Fact]
    public async Task Cancel_GateOn_CycleUpdateFails_PropagatesErrors_NoSuccessAudit_NoLog()
    {
        // Arrange
        const string label = "BC-CANCEL-PERSISTFAIL";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(925)).Build();
        var (handler, _, requestRepository, _, _, _, logger, _) = BuildHandler(barcode, cycle, routingOptions: GateOn(), cycleUpdateFails: true);

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — errors propagated verbatim; the cycle entity WAS mutated in memory (staged on the root)
        // before the failed save.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Cancel status persist failed");
        cycle.CycleStatus.ShouldBe(CycleStatus.Canceled); // in-memory mutation happened before SaveAsync
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        logger.GetLogCount(LogLevel.Error).ShouldBe(0); // AS-BUILT: no log on the cancel persist failure
    }

    /// <summary>
    /// The rejection-audit write is BEST-EFFORT: when AddAsync fails, the handler logs
    /// "Cancel rejection-audit write did not land" at Error and still returns the SAME machine failure.
    /// </summary>
    [Fact]
    public async Task Cancel_RejectionAuditDrop_LogsError_FailureOutcomeUnchanged()
    {
        // Arrange — gate OFF rejection + failing audit write.
        const string label = "BC-CANCEL-AUDITDROP";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(926)).Build();
        var (handler, _, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Cancel rejected by state machine for BarCode {label}: (InProcess, Cancel) -> OperationCancelled (-65536)");
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Cancel rejection-audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// The SUCCESS audit is BEST-EFFORT: a failed AddAsync after the cycle persisted Canceled logs
    /// "Cancel success-audit write did not land" at Error and the handler STILL returns the success view.
    /// </summary>
    [Fact]
    public async Task Cancel_SuccessAuditDrop_LogsError_SuccessOutcomeUnchanged()
    {
        // Arrange
        const string label = "BC-CANCEL-SUCCESS-AUDITDROP";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(927)).Build();
        var (handler, _, _, _, _, _, logger, audits) = BuildHandler(barcode, cycle, routingOptions: GateOn(), auditWriteFails: true);

        // Act
        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().CycleStatus.ShouldBe(CycleStatus.Canceled);
        audits.Count.ShouldBe(1); // the write was attempted
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Cancel success-audit write did not land").ShouldBeTrue();
    }

    /// <summary>
    /// Builds a <see cref="CancelCycleCommandHandler"/> with NSubstitute-mocked repositories, an
    /// always-injected capturing <see cref="TestLogger{T}"/> and (optionally) an injected machine/options.
    /// Passing null machine/options reproduces the DI defaults (as-built engine, EnableCanceledState OFF).
    /// </summary>
    private static (CancelCycleCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo, IReadOnlyRepository<Cycle> CycleRepo, IAggregateRepository<BarCode> AggregateRepo, IDateTimeMachine Clock, TestLogger<CancelCycleCommandHandler> Logger, List<TaskGatewayRequest> Audits) BuildHandler(
        BarCode? barcode,
        Cycle? cycle = null,
        IItemStateMachine? machine = null,
        IOptions<StateMachineRoutingOptions>? routingOptions = null,
        bool auditWriteFails = false,
        bool cycleUpdateFails = false)
    {
        var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var aggregateRepository = Substitute.For<IAggregateRepository<BarCode>>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var logger = new TestLogger<CancelCycleCommandHandler>();
        var audits = new List<TaskGatewayRequest>();

        dateTimeMachine.Now.Returns(FixedNow);

        barCodeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barcode)));

        cycleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle?>.Success(cycle)));

        // #95 Slice E: the cycle-status WRITE goes through the aggregate save; cycleUpdateFails now fails
        // SaveAsync (the raw IReadOnlyRepository<Cycle>.UpdateAsync is never called by the handler).
        aggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(cycleUpdateFails
                ? Result.WithFailure("Cancel status persist failed")
                : Result.Success()));

        requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                audits.Add(ci.Arg<TaskGatewayRequest>());
                return Task.FromResult(auditWriteFails
                    ? Result<int>.WithFailure("audit write failed")
                    : Result<int>.Success(1));
            });

        var handler = new CancelCycleCommandHandler(barCodeRepository, requestRepository, cycleRepository, aggregateRepository, dateTimeMachine, machine, routingOptions, logger);
        return (handler, barCodeRepository, requestRepository, cycleRepository, aggregateRepository, dateTimeMachine, logger, audits);
    }
}
