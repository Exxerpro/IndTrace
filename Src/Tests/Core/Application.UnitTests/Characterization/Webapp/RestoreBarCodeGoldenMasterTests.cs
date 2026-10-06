// <copyright file="RestoreBarCodeGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Webapp;

using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Config;
using Microsoft.Extensions.Options;

/// <summary>
/// Golden-master (characterization) tests for the WEBAPP / MONITOR path RESTORE handler:
/// <see cref="RestoreBarCodeCommandHandler"/> (an <c>IMonitorRequestHandler</c> invoked from
/// <c>BarCodeRestore.razor</c>). This path does NOT traverse the PLC command dispatcher
/// (state-machine-analysis.md §2.2); the PLC-path triggers are out of scope (Story 1.1).
///
/// Story 1.2 — AC 5 (matrix row 3), AC 6 (matrix row 5), AC 7, AC 8.
/// These tests pin the CURRENT as-built behavior; NO production code is changed.
/// </summary>
public class RestoreBarCodeGoldenMasterTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);

    // ----------------------------------------------------------------------------------
    // AC 5 / matrix row 3 — TWO-MODE Restore semantics (Story 4.2 makes this the SINGLE permitted
    // golden-master change in Epic 4, limited to the Restore transition).
    //
    // GATE OFF (default, no EnableRestoredState option): Restore writes FlowStatus.InProcess (2),
    // NOT FlowStatus.Restored (16) — regression-equivalent to the original characterization. This is
    // the unchanged half: the original assertions are preserved verbatim below.
    //
    // GATE ON (CompletenessOptions.EnableRestoredState = true, exercised via the second test
    // Restore_FromRejected_GateOn_WritesRestored_AndAuditsTransition): Restore resolves the gated
    // computed target Rejected → Restored (16) and the Story 3.5 decorator appends a FlowTransitionLog
    // row (From=Rejected(32), To=Restored(16), Trigger=RestorePartAsync, Path=Webapp). So
    // FlowStatus.Restored (16) is no longer dead when the gate is ON (analysis §3.1 / §6 #5 resolved).
    // ----------------------------------------------------------------------------------
    [Fact]
    public async Task Restore_FromRejected_WritesInProcess_NotRestored()
    {
        // Arrange
        const string label = "BC-RESTORE-REJECTED";
        var barcode = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected) // source state
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();

        var (handler, barCodeRepository, requestRepository, _, dateTimeMachine) = BuildHandler(barcode, cycle: new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(900)).Build());
        TaskGatewayRequest? logged = null;
        requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(ci => { logged = ci.Arg<TaskGatewayRequest>(); return Task.FromResult(Result<int>.Success(1)); });

        var command = new RestoreBarCodeCommand { Label = label };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — the anomaly: Restore writes InProcess, NOT Restored.
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barcode.FlowStatus.ShouldNotBe(FlowStatus.Restored); // FlowStatus.Restored (16) is defined-but-dead
        barcode.ModifiedOn.ShouldBe(dateTimeMachine.Now.ToLocalTime());
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());

        // Logged gateway request snapshot — GatewayTask.RestorePartAsync resolves as a Domain symbol
        // (Story 4.3: Domain GatewayTask.cs defines RestorePartAsync = 1024, off the PLC bus), so we assert the enum reference
        // the handler uses (RestoreBarCodeCommandHandler.cs:79) directly.
        logged.ShouldNotBeNull();
        logged!.GatewayTask.ShouldBe(GatewayTask.RestorePartAsync);
        logged.ResultValidation.ShouldBe(ResultValidation.None);
        logged.FlowStatus.ShouldBe(FlowStatus.InProcess);

        // View DTO returned on success.
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ShouldBeOfType<BarCodeRestoredView>();
        result.Value.FlowStatus.ShouldBe(FlowStatus.InProcess);
        result.Value.Label.ShouldBe(label);
    }

    // ----------------------------------------------------------------------------------
    // Story 4.2 — GATE ON half of the two-mode Restore golden master. With
    // EnableRestoredState = true the handler resolves Rejected → Restored (16) (NOT InProcess) and the
    // Story 3.5 LoggingItemStateMachineDecorator appends ONE FlowTransitionLog audit row capturing the
    // realized transition. The handler is built with a DECORATED machine + capturing sink so the audit
    // channel is exercised (the 3.5 best-effort design is reused — see AC5 deviation note in the story).
    // ----------------------------------------------------------------------------------
    [Fact]
    public async Task Restore_FromRejected_GateOn_WritesRestored_AndAuditsTransition()
    {
        // Arrange
        const string label = "BC-RESTORE-REJECTED-ON";
        var barcode = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected) // source state Rejected (32)
            .With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();

        var sink = new CapturingSink();
        var (handler, barCodeRepository, _, _, dateTimeMachine) = BuildHandler(
            barcode,
            cycle: new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(900)).Build(),
            enableRestoredState: true,
            sink: sink);

        var command = new RestoreBarCodeCommand { Label = label };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — gate ON resolves the gated computed target Restored (16), persisted successfully.
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.Restored);
        barcode.FlowStatus.Value.ShouldBe(16);
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());

        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().FlowStatus.ShouldBe(FlowStatus.Restored);

        // Audit — exactly one FlowTransitionLog row recording From=Rejected(32), To=Restored(16),
        // Trigger=RestorePartAsync, Path=Webapp (appended by the Story 3.5 decorator on Fire).
        sink.Rows.Count.ShouldBe(1);
        var row = sink.Rows[0];
        row.From.ShouldBe(FlowStatus.Rejected);
        row.From.Value.ShouldBe(32);
        row.To.ShouldBe(FlowStatus.Restored);
        row.To.Value.ShouldBe(16);
        row.Trigger.ShouldBe(GatewayTask.RestorePartAsync);
        row.Path.ShouldBe(TransitionPath.Webapp);
        row.TimeStamp.ShouldBe(dateTimeMachine.Now.ToLocalTime());
    }

    // ----------------------------------------------------------------------------------
    // AC 6 / matrix row 5 — Restore when the label is NOT found:
    // returns a FAILURE Result with message "BarCode not found {label}" and performs no mutation.
    // ----------------------------------------------------------------------------------
    [Fact]
    public async Task Restore_LabelNotFound_ReturnsFailure_NoMutation()
    {
        // Arrange
        const string label = "BC-DOES-NOT-EXIST";
        var (handler, barCodeRepository, requestRepository, _, _) = BuildHandler(barcode: null);
        var command = new RestoreBarCodeCommand { Label = label };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"BarCode not found {label}");
        await barCodeRepository.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // AC 8 — most-recent-cycle lookup characterization for Restore:
    // when NO cycle exists, the restore STILL SUCCEEDS (the cycle is optional for restore;
    // RestoreBarCodeCommandHandler.cs:70) and the gateway audit log is simply skipped, but the
    // barcode is still advanced to InProcess and persisted.
    // ----------------------------------------------------------------------------------
    [Fact]
    public async Task Restore_WhenNoCycle_StillSucceeds_WritesInProcess_SkipsGatewayAudit()
    {
        // Arrange
        const string label = "BC-RESTORE-NOCYCLE";
        var barcode = new BarCodeBuilder().Rejected(PartStatus.Rejected).With(b => { b.BarCodeId = new BarCodeId(11); b.MachineId = new MachineId(2); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepository, requestRepository, _, _) = BuildHandler(barcode, cycle: null);
        var command = new RestoreBarCodeCommand { Label = label };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — succeeds, barcode advanced to InProcess, persisted, but NO audit logged.
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
        await requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Builds a <see cref="RestoreBarCodeCommandHandler"/> with NSubstitute-mocked repositories.
    /// The barcode lookup returns <paramref name="barcode"/> (or null for not-found); the cycle
    /// lookup returns <paramref name="cycle"/> (or null when none).
    ///
    /// When <paramref name="enableRestoredState"/> is <see langword="true"/> the handler receives routing
    /// options with the Story 4.2 gate ON, so a legal restore resolves Rejected → Restored (16). When a
    /// <paramref name="sink"/> is supplied the handler is built with a Story 3.5
    /// <see cref="LoggingItemStateMachineDecorator"/> over a real <see cref="ItemStateMachine"/> so the
    /// FlowTransitionLog audit channel is exercised; otherwise the legacy positional ctor (default-ON
    /// routing, as-built engine) is used to preserve the existing characterization.
    /// </summary>
    private static (RestoreBarCodeCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo, IReadOnlyRepository<Cycle> CycleRepo, IDateTimeMachine Clock) BuildHandler(
        BarCode? barcode,
        Cycle? cycle = null,
        bool enableRestoredState = false,
        CapturingSink? sink = null)
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

        // Default (gate OFF, no sink): legacy positional construction — preserves the original golden master.
        if (!enableRestoredState && sink is null)
        {
            var legacyHandler = new RestoreBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine);
            return (legacyHandler, barCodeRepository, requestRepository, cycleRepository, dateTimeMachine);
        }

        var routingOptions = Options.Create(new StateMachineRoutingOptions { EnableRestoredState = enableRestoredState });

        // Story 3.5 decorator over the real engine so the FlowTransitionLog audit row is captured for the
        // gate-ON assertion. Path=Webapp mirrors the monitor-path dispatcher's ambient transition path.
        IItemStateMachine machine = sink is null
            ? new ItemStateMachine()
            : new LoggingItemStateMachineDecorator(
                new ItemStateMachine(),
                sink,
                new WebappPathStub(),
                dateTimeMachine,
                Substitute.For<ILogger<LoggingItemStateMachineDecorator>>(),
                routingOptions);

        var handler = new RestoreBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, machine, routingOptions);
        return (handler, barCodeRepository, requestRepository, cycleRepository, dateTimeMachine);
    }

    /// <summary>
    /// A capturing <see cref="IFlowTransitionLogSink"/> that records every appended row and reports success.
    /// Mirrors the sink in <c>FlowTransitionLogDecoratorTests</c> (Story 3.5 best-effort channel).
    /// </summary>
    private sealed class CapturingSink : IFlowTransitionLogSink
    {
        public List<FlowTransitionLog> Rows { get; } = [];

        public Result Append(FlowTransitionLog log)
        {
            this.Rows.Add(log);
            return Result.Success();
        }
    }

    /// <summary>
    /// An ambient transition-path stub fixed to <see cref="TransitionPath.Webapp"/> (the monitor/Restore path).
    /// </summary>
    private sealed class WebappPathStub : ITransitionPathContext
    {
        public TransitionPath Current => TransitionPath.Webapp;

        public void Set(TransitionPath path)
        {
        }
    }
}
