// <copyright file="RestoreBarCodeSemanticsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.BarCodes;

using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Config;
using IndTrace.Domain.ValueObjects;
using Microsoft.Extensions.Options;

/// <summary>
/// Story 4.2 — focused handler tests for the WEBAPP/monitor Restore semantics gate. Pin the two-mode
/// behavior of <see cref="RestoreBarCodeCommandHandler"/>: with the <c>EnableRestoredState</c> gate ON a legal
/// restore resolves <c>Rejected → Restored (16)</c> and the Story 3.5 decorator appends a
/// <see cref="FlowTransitionLog"/> audit row; with the gate OFF (default) the as-built <c>Rejected → InProcess
/// (2)</c> behavior is unchanged (regression-equivalent). Domain reachability is covered separately in
/// <c>CompletenessGatingTests</c> — not duplicated here.
/// </summary>
public class RestoreBarCodeSemanticsTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 20, 8, 0, 0, DateTimeKind.Local);

    /// <summary>
    /// Gate ON ⇒ handler writes <see cref="FlowStatus.Restored"/> (16) and the decorator audits the transition
    /// with one row capturing From=Rejected(32), To=Restored(16), Trigger=RestorePartAsync, Path=Webapp.
    /// </summary>
    [Fact]
    public async Task Restore_GateOn_WritesRestored_AndAppendsAuditRow()
    {
        // Arrange
        const string label = "BC-SEM-ON";
        var barcode = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected)
            .With(b => { b.BarCodeId = new BarCodeId(7); b.MachineId = new MachineId(3); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();

        var sink = new CapturingSink();
        var (handler, barCodeRepository, _) = BuildHandler(barcode, cycle: null, enableRestoredState: true, sink: sink);

        // Act
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.Restored);
        barcode.FlowStatus.Value.ShouldBe(16);
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());

        sink.Rows.Count.ShouldBe(1);
        var row = sink.Rows[0];
        row.From.Value.ShouldBe(32); // Rejected
        row.To.Value.ShouldBe(16);   // Restored
        row.Trigger.ShouldBe(GatewayTask.RestorePartAsync);
        row.Path.ShouldBe(TransitionPath.Webapp);
    }

    /// <summary>
    /// Gate OFF (default) ⇒ handler keeps the as-built <see cref="FlowStatus.InProcess"/> (2) behavior — no
    /// semantic change, regression-equivalent to today.
    /// </summary>
    [Fact]
    public async Task Restore_GateOff_WritesInProcess_RegressionEquivalent()
    {
        // Arrange
        const string label = "BC-SEM-OFF";
        var barcode = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected)
            .With(b => { b.BarCodeId = new BarCodeId(8); b.MachineId = new MachineId(4); b.Label = BarCodeLabel.FromPersisted(label); })
            .Build();

        var sink = new CapturingSink();
        var (handler, barCodeRepository, _) = BuildHandler(barcode, cycle: null, enableRestoredState: false, sink: sink);

        // Act
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Assert — gate OFF resolves the as-built InProcess (NOT Restored).
        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barcode.FlowStatus.Value.ShouldBe(2);
        barcode.FlowStatus.ShouldNotBe(FlowStatus.Restored);
        await barCodeRepository.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());

        // The audit row records the realized as-built transition To=InProcess(2).
        sink.Rows.Count.ShouldBe(1);
        sink.Rows[0].To.Value.ShouldBe(2);
    }

    private static (RestoreBarCodeCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IDateTimeMachine Clock) BuildHandler(
        BarCode? barcode,
        Cycle? cycle,
        bool enableRestoredState,
        CapturingSink sink)
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

        var routingOptions = Options.Create(new StateMachineRoutingOptions { EnableRestoredState = enableRestoredState });

        IItemStateMachine machine = new LoggingItemStateMachineDecorator(
            new ItemStateMachine(),
            sink,
            new WebappPathStub(),
            dateTimeMachine,
            Substitute.For<ILogger<LoggingItemStateMachineDecorator>>(),
            routingOptions);

        var handler = new RestoreBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, machine, routingOptions);
        return (handler, barCodeRepository, dateTimeMachine);
    }

    private sealed class CapturingSink : IFlowTransitionLogSink
    {
        public List<FlowTransitionLog> Rows { get; } = [];

        public Result Append(FlowTransitionLog log)
        {
            this.Rows.Add(log);
            return Result.Success();
        }
    }

    private sealed class WebappPathStub : ITransitionPathContext
    {
        public TransitionPath Current => TransitionPath.Webapp;

        public void Set(TransitionPath path)
        {
        }
    }
}
