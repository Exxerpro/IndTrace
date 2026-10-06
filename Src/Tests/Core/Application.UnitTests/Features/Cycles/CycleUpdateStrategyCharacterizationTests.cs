// <copyright file="CycleUpdateStrategyCharacterizationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Application.Cycles.Services.Strategies;
using IndTrace.Domain.Services.Interfaces;

/// <summary>
/// Story 6.2 — characterization (regression net) for the two LIVE cycle-update strategies
/// (<see cref="OkUpdateStrategy"/> / <see cref="NotOkUpdateStrategy"/>). Pins, AS-IS, the persisted
/// (cycle, barCode) tuple, the FlowStatusCalculator arguments and the returned <see cref="Result{T}"/>
/// shape for every command shape Story 6.2 touches. These assertions MUST stay byte-identical before and
/// after the de-anemization refactor (route through Cycle.FinishOk/FinishNok + BarCode.MarkPartNok); only the
/// way the verdict is DRIVEN changes (a real <see cref="Recipe"/> + cycle-time window instead of a mocked
/// <see cref="ICycleTimeValidator"/>), never the observed outcome.
/// </summary>
public class CycleUpdateStrategyCharacterizationTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;
    private const int CycleId = 777;

    // Recipe window: valid STRICTLY inside (Min, Max). CycleTimeValidator rule is
    // cycleTime >= 0 && cycleTime > Min && cycleTime < Max.
    private const int RecipeMin = 10;
    private const int RecipeMax = 60;

    // CycleTime is computed by the strategy as (FinishedOn - StartedOn).TotalSeconds. FinishedOn comes from
    // IDateTimeMachine.Now.ToLocalTime(); StartedOn is seeded on the cycle. Pick StartedOn so the resulting
    // CycleTime lands inside / outside the recipe window deterministically.
    private static readonly DateTime FinishNow = new(2026, 1, 1, 0, 0, 30, DateTimeKind.Local); // ToLocalTime() no-op

    private readonly IShiftService _shiftService = Substitute.For<IShiftService>();
    private readonly IAggregateRepository<BarCode> _aggregateRepository;
    private readonly IRegisterCleaner _registerCleaner = Substitute.For<IRegisterCleaner>();
    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();
    private readonly IFlowStatusCalculator _flowStatusCalculator = Substitute.For<IFlowStatusCalculator>();

    // Captured persisted tuple.
    private Cycle? _persistedCycle;
    private BarCode? _persistedBarCode;

    public CycleUpdateStrategyCharacterizationTests()
    {
        _dateTime.Now.Returns(FinishNow);

        _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { ShiftId = 9, CyclesOk = 3 })));

        _registerCleaner.CleanRegisters(
                Arg.Any<IDictionary<string, Register>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<DateTime>())
            .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));

        // The calculator is a substitute: it returns a sentinel FlowStatus so we can assert it was assigned
        // verbatim onto barCode.FlowStatus AND assert the (machineType, cycleStatus, partStatus) it received.
        _flowStatusCalculator.Calculate(Arg.Any<MachineType>(), Arg.Any<CycleStatus>(), Arg.Any<PartStatus>())
            .Returns(FlowStatus.Finished);

        // #40 Chunk 40-E: capture the persisted (cycle, barCode) tuple off the aggregate SaveAsync — the strategy
        // passes the SAME mutated barcode (with AppliedCycle == the mutated cycle) it did to the retired
        // PersistenceOrchestrator, so the captured tuple is byte-identical.
        _aggregateRepository = CycleAggregateRepositoryTestDouble.Capturing(root =>
        {
            _persistedBarCode = root;
            _persistedCycle = root.AppliedCycle;
        });
    }

    private static Recipe ValidRecipe() => Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();

    private static IUpdateCycleCommand Command(PartStatus partStatus, CycleStatus cycleStatus)
    {
        var command = Substitute.For<IUpdateCycleCommand>();
        command.MachineId.Returns(MachineId);
        command.BarCode.Returns("TEST-001");
        command.PartNumber.Returns("PN-1");
        command.PartStatus.Returns(partStatus);
        command.CycleStatus.Returns(cycleStatus);
        command.Registers.Returns(new Dictionary<string, Register>());
        return command;
    }

    private static CycleUpdateContext BarCodeInfo(DateTime startedOn, Recipe? recipe)
    {
        var cycle = new CycleBuilder()
            .Started(PartStatus.Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(CycleId);
                c.MachineId = new MachineId(MachineId);
                c.StartedOn = startedOn;
            })
            .Build();

        var barCode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(BarCodeId);
                b.MachineId = new MachineId(MachineId);
            })
            .Build();

        // Story 6.5: the DECIDE step now consumes an immutable CycleUpdateContext, not the god-object.
        return new CycleUpdateContext(cycle, barCode, MachineType.Final, recipe!);
    }

    private OkUpdateStrategy CreateOkStrategy() =>
        new(
            _registerCleaner,
            _aggregateRepository,
            _shiftService,
            _flowStatusCalculator,
            _dateTime,
            Substitute.For<ILogger<OkUpdateStrategy>>());

    private NotOkUpdateStrategy CreateNotOkStrategy() =>
        new(
            _registerCleaner,
            _aggregateRepository,
            _shiftService,
            _flowStatusCalculator,
            _dateTime,
            Substitute.For<ILogger<NotOkUpdateStrategy>>());

    /// <summary>
    /// Ok, in-range: persisted cycle = FinishedOk/Ok; barCode.PartStatus left UNCHANGED (Ok success does NOT
    /// demote); barCode.FlowStatus = calculator result; calculator called with (Final, FinishedOk, Ok); Success.
    /// </summary>
    [Fact]
    public async Task OkStrategy_InRange_PersistsFinishedOk_AndReturnsSuccess()
    {
        // Arrange: CycleTime = 30s, recipe window (10, 60) -> in range.
        var info = BarCodeInfo(FinishNow.AddSeconds(-30), ValidRecipe());
        var strategy = CreateOkStrategy();

        // Act
        var result = await strategy.ExecuteAsync(Command(PartStatus.Ok, CycleStatus.FinishedOk), info, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _persistedCycle!.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        _persistedCycle!.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
        _persistedBarCode!.PartStatus.Value.ShouldBe(PartStatus.Ok.Value); // UNCHANGED on Ok success
        _persistedBarCode!.FlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
        _flowStatusCalculator.Received(1).Calculate(MachineType.Final, CycleStatus.FinishedOk, PartStatus.Ok);
    }

    /// <summary>
    /// Ok, out-of-range: persisted cycle = FinishedNok/NOk; barCode.PartStatus = NOk; calculator called with
    /// (Final, FinishedNok, NOk); returns WithFailure("Cycle time is invalid", result).
    /// </summary>
    [Fact]
    public async Task OkStrategy_OutOfRange_OverridesToFinishedNok_AndReturnsCycleTimeInvalid()
    {
        // Arrange: CycleTime = 5s, recipe window (10, 60) -> below minimum -> out of range.
        var info = BarCodeInfo(FinishNow.AddSeconds(-5), ValidRecipe());
        var strategy = CreateOkStrategy();

        // Act
        var result = await strategy.ExecuteAsync(Command(PartStatus.Ok, CycleStatus.FinishedOk), info, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.First().ShouldBe("Cycle time is invalid");
        result.Value.ShouldNotBeNull();
        _persistedCycle!.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        _persistedCycle!.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        _persistedBarCode!.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        _flowStatusCalculator.Received(1).Calculate(MachineType.Final, CycleStatus.FinishedNok, PartStatus.NOk);
    }

    /// <summary>
    /// Ok, null recipe: same override as out-of-range (FinishedNok/NOk; barcode NOk; Cycle time is invalid).
    /// </summary>
    [Fact]
    public async Task OkStrategy_NullRecipe_OverridesToFinishedNok_AndReturnsCycleTimeInvalid()
    {
        // Arrange: CycleTime in-range, but recipe is null -> forced override.
        var info = BarCodeInfo(FinishNow.AddSeconds(-30), recipe: null);
        var strategy = CreateOkStrategy();

        // Act
        var result = await strategy.ExecuteAsync(Command(PartStatus.Ok, CycleStatus.FinishedOk), info, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.First().ShouldBe("Cycle time is invalid");
        result.Value.ShouldNotBeNull();
        _persistedCycle!.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        _persistedCycle!.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        _persistedBarCode!.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        _flowStatusCalculator.Received(1).Calculate(MachineType.Final, CycleStatus.FinishedNok, PartStatus.NOk);
    }

    /// <summary>
    /// NotOk: persisted cycle = FinishedNok/NOk; barCode.PartStatus = NOk; barCode.FlowStatus = calculator
    /// result; returns Success.
    /// </summary>
    [Fact]
    public async Task NotOkStrategy_PersistsFinishedNok_AndReturnsSuccess()
    {
        // Arrange
        var info = BarCodeInfo(FinishNow.AddSeconds(-30), ValidRecipe());
        var strategy = CreateNotOkStrategy();

        // Act
        var result = await strategy.ExecuteAsync(Command(PartStatus.NOk, CycleStatus.FinishedNok), info, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _persistedCycle!.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        _persistedCycle!.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        _persistedBarCode!.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        _persistedBarCode!.FlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
        _flowStatusCalculator.Received(1).Calculate(MachineType.Final, CycleStatus.FinishedNok, PartStatus.NOk);
    }

    /// <summary>
    /// Story 6.2 (M1) / Story 6.5: on the NotOk path the status surfaced to the PLC must be the DERIVED cycle
    /// status (FinishedNok), not the raw command.CycleStatus. The echo (UpdateBarCodeInformationOnCycle) moved
    /// out of the strategy and into the handler in Story 6.5, so this strategy-level test now pins the DERIVED
    /// values the handler echoes: drive a MISMATCHED command (CycleStatus.FinishedOk) and assert the result
    /// entities carry FinishedNok/NOk (the values the handler replays onto the god-object).
    /// </summary>
    [Fact]
    public async Task NotOkStrategy_MismatchedCommandCycleStatus_DerivesFinishedNok()
    {
        // Arrange
        var context = BarCodeInfo(FinishNow.AddSeconds(-30), ValidRecipe());
        var strategy = CreateNotOkStrategy();

        // Act: command carries FinishedOk (mismatched with the NotOk trigger).
        var result = await strategy.ExecuteAsync(Command(PartStatus.Ok, CycleStatus.FinishedOk), context, CancellationToken.None);

        // Assert: persisted AND result-carried status are the derived FinishedNok/NOk, not the command's
        // FinishedOk/Ok. These are exactly the values the handler echoes via UpdateBarCodeInformationOnCycle.
        result.IsSuccess.ShouldBeTrue();
        _persistedCycle!.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        result.Value.ShouldNotBeNull().UpdatedCycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        result.Value.ShouldNotBeNull().UpdatedBarCode.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
    }
}
