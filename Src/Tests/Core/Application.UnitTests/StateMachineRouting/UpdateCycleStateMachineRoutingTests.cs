// <copyright file="UpdateCycleStateMachineRoutingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.StateMachineRouting;

using Application.UnitTests.Features.Cycles;
using IndTrace.Application.Cycles.Commands.UpdateCycles;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Application.Cycles.Services.Strategies;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// D2 / FR1+FR5 (docs/architecture/state-machine/d2-cycle-path-routing-design.md) — proves the unified
/// <see cref="UpdateCyclesCommandHandler"/> ROUTES the two cycle-update triggers through the injected
/// <see cref="IItemStateMachine"/> when the per-target routing flag is ON, and skips the fire entirely when OFF.
/// Specifically:
/// <list type="bullet">
/// <item>flag-ON fires <see cref="GatewayTask.UpdateCycleOkAsync"/> / <see cref="GatewayTask.UpdateCycleNotOkAsync"/>
/// (assertion via an NSubstitute spy delegating to the real engine) and the strategy still runs / persists;</item>
/// <item>an out-of-order trigger on a NON-InProcess barcode is TABLE-REJECTED before the strategy runs — the
/// strategy's <c>ExecuteAsync</c> is never invoked (nothing persisted) and the handler returns a value-carrying
/// failure with the machine's specific code;</item>
/// <item>flag-OFF skips the fire (the spy records nothing) and the strategy runs exactly as today.</item>
/// </list>
/// Persisted/wire byte-equality across both flag states is pinned by <c>UpdateCyclesGoldenMasterTests</c> /
/// <c>CycleTimeOverrideAnomalyTests</c> (which run flag default-ON) and stays unchanged.
/// </summary>
public class UpdateCycleStateMachineRoutingTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;
    private const int CycleId = 777;

    private static readonly DateTime FinishNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

    private static IOptions<StateMachineRoutingOptions> RoutingOn() =>
        Options.Create(new StateMachineRoutingOptions());

    private static IOptions<StateMachineRoutingOptions> RoutingOff() =>
        Options.Create(new StateMachineRoutingOptions
        {
            RouteUpdateCycleOk = false,
            RouteUpdateCycleNotOk = false,
        });

    /// <summary>A spy machine that records every Fire trigger and delegates to the real engine.</summary>
    private static IItemStateMachine SpyMachine(out List<GatewayTask> firedTriggers)
    {
        var real = new ItemStateMachine();
        var captured = new List<GatewayTask>();
        firedTriggers = captured;

        var spy = Substitute.For<IItemStateMachine>();
        spy.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(call =>
            {
                captured.Add((GatewayTask)call[1]);
                return real.Fire((BarCode)call[0], (GatewayTask)call[1], (TransitionContext)call[2]);
            });
        return spy;
    }

    private static CycleUpdateLoadState BuildLoadState(BarCode barCode, Cycle cycle, Recipe? recipe, MachineType machineType) =>
        new(
            MachineId: MachineId,
            BarCodeId: BarCodeId,
            CycleId: CycleId,
            CyclesOk: 0,
            ShiftId: 0,
            CommandId: 0,
            ResultValidation: ResultValidation.Valid,
            Error: string.Empty,
            Label: "BC",
            PartNumber: "PART",
            Description: "Machine 100",
            LastMachineId: 0,
            NextMachineId: MachineId,
            CycleStatus: CycleStatus.Started,
            FlowStatus: barCode.FlowStatus,
            PartStatus: PartStatus.Ok,
            MachineType: machineType,
            WorkFlowType: WorkFlowType.Serial,
            Recipe: recipe!,
            MasterLabel: new MasterLabel(),
            References: new Dictionary<string, Register>(),
            Cycle: cycle,
            BarCode: barCode,
            Product: new Product());

    /// <summary>
    /// Builds a handler over the real OK/NotOk strategies (so flag-ON runs persist) with the spy machine.
    /// </summary>
    private static UpdateCyclesCommandHandler BuildHandler(
        CycleUpdateLoadState load,
        IItemStateMachine machine,
        IOptions<StateMachineRoutingOptions> routing)
    {
        var provider = Substitute.For<IBarCodeInfoProvider>();
        provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CycleUpdateLoadState>.Success(load)));

        var stationValidator = Substitute.For<IStationValidator>();
        stationValidator.ValidateStation(Arg.Any<int>(), Arg.Any<CycleStatus>(), Arg.Any<CycleUpdateLoadState>())
            .Returns(Result<StationValidationResult>.Success(new StationValidationResult(true, null, ResultValidation.Valid)));

        var dateTime = Substitute.For<IDateTimeMachine>();
        dateTime.Now.Returns(FinishNow);
        var shiftService = Substitute.For<IShiftService>();
        shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { CyclesOk = 1 })));
        var registerCleaner = Substitute.For<IRegisterCleaner>();
        registerCleaner.CleanRegisters(Arg.Any<IDictionary<string, Register>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime>())
            .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));
        var persistence = CycleAggregateRepositoryTestDouble.Passthrough();
        var flowStatusCalculator = new FlowStatusCalculator();

        var factory = Substitute.For<ICycleUpdateStrategyFactory>();
        factory.CreateStrategy(CycleStatus.FinishedOk).Returns(new OkUpdateStrategy(
            registerCleaner, persistence, shiftService, flowStatusCalculator, dateTime, Substitute.For<ILogger<OkUpdateStrategy>>()));
        factory.CreateStrategy(CycleStatus.FinishedNok).Returns(new NotOkUpdateStrategy(
            registerCleaner, persistence, shiftService, flowStatusCalculator, dateTime, Substitute.For<ILogger<NotOkUpdateStrategy>>()));

        var commandLogger = Substitute.For<ICommandLogger>();
        commandLogger.CreateCommand(Arg.Any<CycleUpdateLoadState>(), Arg.Any<GatewayTask>(), Arg.Any<string?>())
            .Returns(new TaskGatewayRequest());
        commandLogger.LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        return new UpdateCyclesCommandHandler(
            provider, stationValidator, factory, commandLogger,
            Substitute.For<ILogger<UpdateCyclesCommandHandler>>(), machine, routing);
    }

    /// <summary>
    /// Builds a handler whose strategy factory returns a SPY strategy, so the test can assert the strategy was (or
    /// was NOT) executed. The spy strategy persists nothing.
    /// </summary>
    private static UpdateCyclesCommandHandler BuildHandlerWithSpyStrategy(
        CycleUpdateLoadState load,
        IItemStateMachine machine,
        IOptions<StateMachineRoutingOptions> routing,
        out ICycleUpdateStrategy spyStrategy)
    {
        var provider = Substitute.For<IBarCodeInfoProvider>();
        provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CycleUpdateLoadState>.Success(load)));

        var stationValidator = Substitute.For<IStationValidator>();
        stationValidator.ValidateStation(Arg.Any<int>(), Arg.Any<CycleStatus>(), Arg.Any<CycleUpdateLoadState>())
            .Returns(Result<StationValidationResult>.Success(new StationValidationResult(true, null, ResultValidation.Valid)));

        spyStrategy = Substitute.For<ICycleUpdateStrategy>();
        var updatedBarCode = load.BarCode.ShouldNotBeNull();
        spyStrategy.ExecuteAsync(Arg.Any<IUpdateCycleCommand>(), Arg.Any<CycleUpdateContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CycleUpdateResult>.Success(
                new CycleUpdateResult(load.Cycle, updatedBarCode, 0, 0, new ShiftInfo(0, 0)))));

        var factory = Substitute.For<ICycleUpdateStrategyFactory>();
        factory.CreateStrategy(Arg.Any<CycleStatus>()).Returns(spyStrategy);

        var commandLogger = Substitute.For<ICommandLogger>();
        commandLogger.CreateCommand(Arg.Any<CycleUpdateLoadState>(), Arg.Any<GatewayTask>(), Arg.Any<string?>())
            .Returns(new TaskGatewayRequest());
        commandLogger.LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        return new UpdateCyclesCommandHandler(
            provider, stationValidator, factory, commandLogger,
            Substitute.For<ILogger<UpdateCyclesCommandHandler>>(), machine, routing);
    }

    private static (Cycle Cycle, BarCode BarCode) Entities(FlowStatus flowStatus, int targetCycleTime = 15)
    {
        var cycle = new CycleBuilder().Started(PartStatus.Ok)
            .With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(MachineId); c.StartedOn = FinishNow.AddSeconds(-targetCycleTime); }).Build();
        var barCode = new BarCodeBuilder().AtState(flowStatus, PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();
        return (cycle, barCode);
    }

    private static UpdateCyclesOkCommand OkCommand() =>
        new() { Command = new TaskGatewayRequest { MachineId = MachineId, BarCode = "BC-OK", PartNumber = "PART", CycleStatus = CycleStatus.FinishedOk, PartStatus = PartStatus.Ok, Registers = new Dictionary<string, Register>() } };

    private static UpdateCyclesNotOkCommand NotOkCommand() =>
        new() { Command = new TaskGatewayRequest { MachineId = MachineId, BarCode = "BC-NOK", PartNumber = "PART", CycleStatus = CycleStatus.FinishedNok, PartStatus = PartStatus.NOk, Registers = new Dictionary<string, Register>() } };

    // -------------------------------------------------------------------------------------------------
    // (a) flag-ON fires the machine with the correct trigger
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public async Task UpdateCycleOk_WhenFlagOn_FiresMachine_WithUpdateCycleOkTrigger()
    {
        var spy = SpyMachine(out var fired);
        var recipe = Recipe.Create(0, 0, 10, 20, 3, 5, 1).Value.ShouldNotBeNull();
        var (cycle, barCode) = Entities(FlowStatus.InProcess);
        var load = BuildLoadState(barCode, cycle, recipe, MachineType.Process);

        var result = await BuildHandler(load, spy, RoutingOn()).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldContain(GatewayTask.UpdateCycleOkAsync);
    }

    [Fact]
    public async Task UpdateCycleNotOk_WhenFlagOn_FiresMachine_WithUpdateCycleNotOkTrigger()
    {
        var spy = SpyMachine(out var fired);
        var recipe = Recipe.Create(0, 0, 10, 20, 3, 5, 1).Value.ShouldNotBeNull();
        var (cycle, barCode) = Entities(FlowStatus.InProcess);
        var load = BuildLoadState(barCode, cycle, recipe, MachineType.Process);

        var result = await BuildHandler(load, spy, RoutingOn()).ProcessAsync(NotOkCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldContain(GatewayTask.UpdateCycleNotOkAsync);
    }

    // -------------------------------------------------------------------------------------------------
    // (b) out-of-order trigger on a non-InProcess barcode -> TABLE-REJECT, nothing persisted
    // -------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(true)]  // OK trigger
    [InlineData(false)] // NotOk trigger
    public async Task UpdateCycle_WhenBarcodeNotInProcess_TableRejects_AndStrategyNeverRuns(bool okTrigger)
    {
        var spy = SpyMachine(out var fired);
        var recipe = Recipe.Create(0, 0, 10, 20, 3, 5, 1).Value.ShouldNotBeNull();

        // Finished barcode: (Finished, UpdateCycleOk/NotOk) is absent from the §4 table -> default-reject.
        var (cycle, barCode) = Entities(FlowStatus.Finished);
        var load = BuildLoadState(barCode, cycle, recipe, MachineType.Process);

        var handler = BuildHandlerWithSpyStrategy(load, spy, RoutingOn(), out var spyStrategy);

        var result = okTrigger
            ? await handler.ProcessAsync(OkCommand(), TestContext.Current.CancellationToken)
            : await handler.ProcessAsync(NotOkCommand(), TestContext.Current.CancellationToken);

        // The machine WAS fired (the gate), it table-rejected, and the handler returned a value-carrying failure.
        fired.ShouldContain(okTrigger ? GatewayTask.UpdateCycleOkAsync : GatewayTask.UpdateCycleNotOkAsync);
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.Value.ShouldBe(ResultValidation.OperationCancelled.Value);

        // NOTHING persisted: the strategy was never even created/executed (reject happened pre-strategy).
        await spyStrategy.DidNotReceive().ExecuteAsync(
            Arg.Any<IUpdateCycleCommand>(), Arg.Any<CycleUpdateContext>(), Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------------------------------
    // (c) flag-OFF skips the fire -> behavior preserved (strategy still runs)
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public async Task UpdateCycleOk_WhenFlagOff_DoesNotFireMachine_StrategyStillRuns()
    {
        var spy = SpyMachine(out var fired);
        var recipe = Recipe.Create(0, 0, 10, 20, 3, 5, 1).Value.ShouldNotBeNull();
        var (cycle, barCode) = Entities(FlowStatus.InProcess);
        var load = BuildLoadState(barCode, cycle, recipe, MachineType.Process);

        var result = await BuildHandler(load, spy, RoutingOff()).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldBeEmpty();
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedOk); // strategy ran and mutated as today
    }

    [Fact]
    public async Task UpdateCycle_WhenFlagOff_BarcodeNotInProcess_DoesNotTableReject_StrategyRuns()
    {
        // With routing OFF the machine never fires, so the out-of-order gate does NOT bite — exactly today's
        // strategy-only behavior (the station validator, not the machine, is the only gate when OFF).
        var spy = SpyMachine(out var fired);
        var recipe = Recipe.Create(0, 0, 10, 20, 3, 5, 1).Value.ShouldNotBeNull();
        var (cycle, barCode) = Entities(FlowStatus.Finished);
        var load = BuildLoadState(barCode, cycle, recipe, MachineType.Process);

        var handler = BuildHandlerWithSpyStrategy(load, spy, RoutingOff(), out var spyStrategy);

        var result = await handler.ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldBeEmpty();
        await spyStrategy.Received(1).ExecuteAsync(
            Arg.Any<IUpdateCycleCommand>(), Arg.Any<CycleUpdateContext>(), Arg.Any<CancellationToken>());
    }
}
