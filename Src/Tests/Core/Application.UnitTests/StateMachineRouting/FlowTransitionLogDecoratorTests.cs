// <copyright file="FlowTransitionLogDecoratorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.StateMachine;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Story 3.5 — unit tests for the <see cref="LoggingItemStateMachineDecorator"/>. Proves it appends exactly ONE
/// additive <see cref="FlowTransitionLog"/> row per fire (success AND rejection) with the correct
/// From/To/Trigger/Path/MachineId/ResultValidation/TimeStamp, returns the inner <c>Result</c> verbatim, honors
/// the <c>LogTransitions</c> flag (OFF = no append, zero transition impact), and is best-effort (a throwing or
/// failing sink never flips the transition's success/failure outcome).
/// </summary>
public class FlowTransitionLogDecoratorTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 19, 8, 0, 0, DateTimeKind.Local);

    private static Recipe ValidRecipe() => Recipe.Create(0, 0, 0, 216000, 3, 5, 1).Value.ShouldNotBeNull();

    private static TransitionContext FullContext(MachineType machineType, CycleStatus cycleStatus, PartStatus partStatus) =>
        new(machineType, cycleStatus, partStatus, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    /// <summary>
    /// A capturing sink that records every appended row and reports success.
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

    private sealed class ThrowingSink : IFlowTransitionLogSink
    {
        public int Calls { get; private set; }

        public Result Append(FlowTransitionLog log)
        {
            this.Calls++;
            throw new InvalidOperationException("boom — simulated log-write failure.");
        }
    }

    private sealed class FailingSink : IFlowTransitionLogSink
    {
        public int Calls { get; private set; }

        public Result Append(FlowTransitionLog log)
        {
            this.Calls++;
            return Result.WithFailure("simulated append failure.");
        }
    }

    private sealed class StubPath(TransitionPath path) : ITransitionPathContext
    {
        public TransitionPath Current => path;

        public void Set(TransitionPath p)
        {
        }
    }

    private static IDateTimeMachine Clock()
    {
        var clock = Substitute.For<IDateTimeMachine>();
        clock.Now.Returns(FixedNow);
        return clock;
    }

    private static LoggingItemStateMachineDecorator Build(
        IFlowTransitionLogSink sink,
        TransitionPath path,
        bool logTransitions = true,
        IItemStateMachine? inner = null) =>
        new(
            inner ?? new ItemStateMachine(),
            sink,
            new StubPath(path),
            Clock(),
            Substitute.For<ILogger<LoggingItemStateMachineDecorator>>(),
            Options.Create(new StateMachineRoutingOptions { LogTransitions = logTransitions }));

    /// <summary>
    /// AC2/AC4 — every successful PLC-path Fire appends exactly one row with the realized transition and Path=Plc.
    /// </summary>
    [Theory]
    [InlineData(0, 4, 1)]    // (None, CreateBarCodeAsync) -> Created
    [InlineData(1, 16, 2)]   // (Created, CreateCycleAsync) -> InProcess
    [InlineData(2, 32, 2)]   // (InProcess, UpdateCycleOkAsync) -> InProcess (D2: non-final OK, now machine-routed)
    [InlineData(2, 64, 2)]   // (InProcess, UpdateCycleNotOkAsync) -> InProcess
    [InlineData(2, 128, 4)]  // (InProcess, EndOfProcessAsync) -> Finished
    public void Fire_SuccessfulPlcTrigger_AppendsOneRow_WithRealizedTransition_AndPlcPath(int fromValue, int triggerValue, int expectedToValue)
    {
        // Arrange
        var sink = new CapturingSink();
        var decorator = Build(sink, TransitionPath.Plc);
        var barcode = new BarCodeBuilder().AtState(FlowStatus.FromValue(fromValue), PartStatus.None)
            .With(b => { b.BarCodeId = new BarCodeId(7); b.MachineId = new MachineId(11); }).Build();
        GatewayTask trigger = triggerValue;

        // Act
        var result = decorator.Fire(barcode, trigger, FullContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok));

        // Assert — inner result returned verbatim, exactly one row, realized transition, Path=Plc.
        result.IsSuccess.ShouldBeTrue();
        sink.Rows.Count.ShouldBe(1);
        var row = sink.Rows[0];
        row.From.Value.ShouldBe(fromValue);
        row.To.Value.ShouldBe(expectedToValue);
        row.Trigger.Value.ShouldBe(triggerValue);
        row.Path.ShouldBe(TransitionPath.Plc);
        row.MachineId.ShouldBe(11);
        row.BarCodeId.ShouldBe(7);
        row.ResultValidation.Value.ShouldBe(ResultValidation.Valid.Value);
        row.TimeStamp.ShouldBe(FixedNow.ToLocalTime());
    }

    /// <summary>
    /// D2 / FR5 — a Final-machine UpdateCycleOk fire (FinishedOk) resolves the computed To=Finished and appends
    /// exactly one row with Path=Plc. Pins the newly machine-routed OK trigger's Final-promotion log row.
    /// </summary>
    [Fact]
    public void Fire_UpdateCycleOk_FinalMachine_AppendsOneRow_ToFinished_AndPlcPath()
    {
        // Arrange — Final && FinishedOk so ResolveNextFlowStatus computes Finished (mirrors FlowStatusCalculator).
        var sink = new CapturingSink();
        var decorator = Build(sink, TransitionPath.Plc);
        var barcode = new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.None)
            .With(b => { b.BarCodeId = new BarCodeId(7); b.MachineId = new MachineId(11); }).Build();

        // Act
        var result = decorator.Fire(barcode, GatewayTask.UpdateCycleOkAsync, FullContext(MachineType.Final, CycleStatus.FinishedOk, PartStatus.Ok));

        // Assert — exactly one row, computed To=Finished, Path=Plc, Valid.
        result.IsSuccess.ShouldBeTrue();
        sink.Rows.Count.ShouldBe(1);
        var row = sink.Rows[0];
        row.From.Value.ShouldBe(FlowStatus.InProcess.Value);
        row.To.Value.ShouldBe(FlowStatus.Finished.Value);
        row.Trigger.Value.ShouldBe(GatewayTask.UpdateCycleOkAsync.Value);
        row.Path.ShouldBe(TransitionPath.Plc);
        row.ResultValidation.Value.ShouldBe(ResultValidation.Valid.Value);
    }

    /// <summary>
    /// AC2/AC4 — webapp Reject/Restore successful fires append one row with Path=Webapp.
    /// </summary>
    [Theory]
    [InlineData(2, 256, 32)] // (InProcess, RejectPartAsync) -> Rejected
    [InlineData(32, 1024, 2)] // (Rejected, RestorePartAsync=1024) -> InProcess
    public void Fire_SuccessfulWebappTrigger_AppendsOneRow_WithWebappPath(int fromValue, int triggerValue, int expectedToValue)
    {
        var sink = new CapturingSink();
        var decorator = Build(sink, TransitionPath.Webapp);
        var barcode = new BarCodeBuilder().AtState(FlowStatus.FromValue(fromValue), PartStatus.None).Build();
        GatewayTask trigger = triggerValue;

        var result = decorator.Fire(barcode, trigger, FullContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok));

        result.IsSuccess.ShouldBeTrue();
        sink.Rows.Count.ShouldBe(1);
        sink.Rows[0].To.Value.ShouldBe(expectedToValue);
        sink.Rows[0].Path.ShouldBe(TransitionPath.Webapp);
    }

    /// <summary>
    /// AC3 — a rejected Fire still appends exactly one row, with To = From (no advance) and the specific negative code.
    /// </summary>
    [Theory]
    [InlineData(32, 256)] // (Rejected, RejectPartAsync) - illegal
    [InlineData(2, 1024)]  // (InProcess, RestorePartAsync=1024) - illegal
    [InlineData(4, 16)]   // (Finished, CreateCycleAsync) - illegal
    public void Fire_RejectedTrigger_AppendsOneRow_WithNoAdvance_AndNegativeCode(int fromValue, int triggerValue)
    {
        var sink = new CapturingSink();
        var decorator = Build(sink, TransitionPath.Webapp);
        var barcode = new BarCodeBuilder().AtState(FlowStatus.FromValue(fromValue), PartStatus.None).Build();
        GatewayTask trigger = triggerValue;

        var result = decorator.Fire(barcode, trigger, FullContext(MachineType.Process, CycleStatus.Started, PartStatus.None));

        result.IsSuccess.ShouldBeFalse();
        sink.Rows.Count.ShouldBe(1);
        var row = sink.Rows[0];
        row.From.Value.ShouldBe(fromValue);
        row.To.Value.ShouldBe(fromValue); // no advance
        row.ResultValidation.Value.ShouldBeLessThan(0); // specific negative code
    }

    /// <summary>
    /// AC8 — flag OFF skips the append with zero transition impact (inner result still returned verbatim).
    /// </summary>
    [Fact]
    public void Fire_WhenLogTransitionsOff_SkipsAppend_ButReturnsSameResult()
    {
        var sink = new CapturingSink();
        var decorator = Build(sink, TransitionPath.Plc, logTransitions: false);
        var barcode = new BarCodeBuilder().AtState(FlowStatus.Created, PartStatus.None).Build();

        var result = decorator.Fire(barcode, GatewayTask.CreateCycleAsync, FullContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.ShouldBe(FlowStatus.InProcess);
        sink.Rows.Count.ShouldBe(0); // no append
    }

    /// <summary>
    /// AC5 — a THROWING sink does not change the transition's success outcome (best-effort, swallowed).
    /// </summary>
    [Fact]
    public void Fire_WhenSinkThrows_DoesNotFlipSuccessResult()
    {
        var sink = new ThrowingSink();
        var decorator = Build(sink, TransitionPath.Plc);
        var barcode = new BarCodeBuilder().AtState(FlowStatus.Created, PartStatus.None).Build();

        var result = decorator.Fire(barcode, GatewayTask.CreateCycleAsync, FullContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok));

        sink.Calls.ShouldBe(1);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// AC5 — a FAILING sink (returns failure) does not change the transition's failure outcome on a reject.
    /// </summary>
    [Fact]
    public void Fire_WhenSinkFails_DoesNotFlipRejectResult()
    {
        var sink = new FailingSink();
        var decorator = Build(sink, TransitionPath.Webapp);
        var barcode = new BarCodeBuilder().AtState(FlowStatus.Rejected, PartStatus.None).Build();

        // (Rejected, RejectPartAsync) is illegal -> the transition fails; the failing sink must not change that.
        var result = decorator.Fire(barcode, GatewayTask.RejectPartAsync, FullContext(MachineType.Process, CycleStatus.Started, PartStatus.None));

        sink.Calls.ShouldBe(1);
        result.IsSuccess.ShouldBeFalse();
    }
}
