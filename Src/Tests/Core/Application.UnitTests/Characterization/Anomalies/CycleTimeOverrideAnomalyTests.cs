// <copyright file="CycleTimeOverrideAnomalyTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Anomalies;

using Application.UnitTests.Features.Cycles;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Commands.UpdateCycles;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Application.Cycles.Services.Strategies;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;

/// <summary>
/// Story 1.3 — Anomaly B (cycle-time-out-of-range FinishedOk → FinishedNok OVERRIDE), RETARGETED in Story 6.3
/// onto the unified <see cref="UpdateCyclesCommandHandler"/> (the legacy handler was deleted in 6.3).
///
/// Pins, AS-IS, the surprising behavior: an "Ok" command whose cycle time falls outside the recipe window is
/// silently DOWNGRADED to FinishedNok and the part marked NOk, and the handler returns a FAILURE — a
/// failure-with-mutated-entities. The override is now produced by the unified OK strategy's
/// <c>Cycle.FinishOk</c> guard (the SAME cycle-time validator wrapped via <c>CycleTimeGuard</c>), so the verdict
/// is byte-identical: recipe is null ? true : (cycleTime &lt;= Minimum || cycleTime &gt;= Maximum) — INCLUSIVE
/// on the failing side.
///
/// KNOWN ANOMALY (analysis §6 / table §4): the cycle-time override forcing FinishedNok on an "Ok" command,
/// pinned AS-IS. Under the unified path the failure now CARRIES the recipe-aware specific code on a non-null
/// value (PartNotValid out-of-range / RecipeNotFound null recipe), the FR4 improvement that shipped in 6.5.
/// </summary>
public class CycleTimeOverrideAnomalyTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;
    private const int CycleId = 777;

    // Recipe window: valid STRICTLY inside (10, 20). Boundaries fail (inclusive-failing).
    private const int RecipeMin = 10;
    private const int RecipeMax = 20;

    private static readonly DateTime FinishNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

    private readonly IShiftService _shiftService = Substitute.For<IShiftService>();
    private readonly IAggregateRepository<BarCode> _aggregateRepository = CycleAggregateRepositoryTestDouble.Passthrough();
    private readonly IRegisterCleaner _registerCleaner = Substitute.For<IRegisterCleaner>();
    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();
    private readonly IFlowStatusCalculator _flowStatusCalculator = new FlowStatusCalculator();

    public CycleTimeOverrideAnomalyTests()
    {
        _dateTime.Now.Returns(FinishNow);

        _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { CyclesOk = 1 })));
        _registerCleaner.CleanRegisters(Arg.Any<IDictionary<string, Register>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime>())
            .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));
    }

    /// <summary>
    /// Builds an immutable load state (Process machine) whose shared tracked entities the OK strategy mutates so
    /// the derived CycleTime is exactly <paramref name="targetCycleTime"/> seconds (Now - StartedOn).
    /// </summary>
    private static (CycleUpdateLoadState Load, Cycle Cycle, BarCode BarCode) BuildLoadState(Recipe? recipe, int targetCycleTime)
    {
        var cycle = new CycleBuilder().Started(PartStatus.Ok)
            .With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(MachineId); c.StartedOn = FinishNow.AddSeconds(-targetCycleTime); }).Build();
        var barCode = new BarCodeBuilder().InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();

        var load = new CycleUpdateLoadState(
            MachineId: MachineId,
            BarCodeId: BarCodeId,
            CycleId: CycleId,
            CyclesOk: 0,
            ShiftId: 0,
            CommandId: 0,
            ResultValidation: ResultValidation.Valid,
            Error: string.Empty,
            Label: "BC-OK",
            PartNumber: "PART",
            Description: "Machine 100",
            LastMachineId: 0,
            NextMachineId: MachineId,
            CycleStatus: CycleStatus.Started,
            FlowStatus: FlowStatus.InProcess,
            PartStatus: PartStatus.Ok,
            MachineType: MachineType.Process,
            WorkFlowType: WorkFlowType.Serial,
            Recipe: recipe!,
            MasterLabel: new MasterLabel(),
            References: new Dictionary<string, Register>(),
            Cycle: cycle,
            BarCode: barCode,
            Product: new Product());

        return (load, cycle, barCode);
    }

    private UpdateCyclesCommandHandler BuildHandler(CycleUpdateLoadState load)
    {
        var provider = Substitute.For<IBarCodeInfoProvider>();
        provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CycleUpdateLoadState>.Success(load)));

        var stationValidator = Substitute.For<IStationValidator>();
        stationValidator.ValidateStation(Arg.Any<int>(), Arg.Any<CycleStatus>(), Arg.Any<CycleUpdateLoadState>())
            .Returns(Result<StationValidationResult>.Success(new StationValidationResult(true, null, ResultValidation.Valid)));

        var factory = Substitute.For<ICycleUpdateStrategyFactory>();
        factory.CreateStrategy(CycleStatus.FinishedOk).Returns(new OkUpdateStrategy(
            _registerCleaner, _aggregateRepository, _shiftService, _flowStatusCalculator, _dateTime, Substitute.For<ILogger<OkUpdateStrategy>>()));

        var commandLogger = Substitute.For<ICommandLogger>();
        commandLogger.CreateCommand(Arg.Any<CycleUpdateLoadState>(), Arg.Any<GatewayTask>(), Arg.Any<string?>())
            .Returns(new TaskGatewayRequest());
        commandLogger.LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        return new UpdateCyclesCommandHandler(
            provider, stationValidator, factory, commandLogger, Substitute.For<ILogger<UpdateCyclesCommandHandler>>());
    }

    private static UpdateCyclesOkCommand OkCommand() =>
        new()
        {
            Command = new TaskGatewayRequest
            {
                MachineId = MachineId,
                BarCode = "BC-OK",
                PartNumber = "PART",
                CycleStatus = CycleStatus.FinishedOk,
                PartStatus = PartStatus.Ok,
                Registers = new Dictionary<string, Register>(),
            },
        };

    /// <summary>
    /// AC3/AC4/AC6 — the cycle-time OVERRIDE forcing cases.
    /// Parametrized over the boundary matrix (inclusive-failing): cycleTime &lt;= Minimum,
    /// cycleTime == Maximum, cycleTime &gt;= Maximum, and recipe == null. In every case an "Ok"
    /// command is forced to cycle FinishedNok, cycle PartStatus NOk, barcode PartStatus NOk, and
    /// the handler returns a FAILURE with message "Cycle time is invalid".
    ///
    /// The failure CARRIES the recipe-aware specific code on a non-null value (CycleTimeGuard authority):
    /// out-of-range with a valid recipe -> PartNotValid(-64); null recipe -> RecipeNotFound(-512).
    /// </summary>
    [Theory]
    [InlineData(RecipeMin, false)]      // cycleTime == Minimum  → invalid (inclusive-failing, <= Min)
    [InlineData(RecipeMin - 5, false)]  // cycleTime <  Minimum  → invalid
    [InlineData(RecipeMax, false)]      // cycleTime == Maximum  → invalid (inclusive-failing, >= Max)
    [InlineData(RecipeMax + 5, false)]  // cycleTime >  Maximum  → invalid
    [InlineData(15, true)]              // recipe == null        → invalid regardless of in-range time
    public async Task UpdateCycleOk_OutOfRange_Overrides_FinishedOk_To_FinishedNok(int cycleTime, bool useNullRecipe)
    {
        // Arrange — Cycle.FinishOk (via CycleTimeGuard) = recipe is null ? true : (cycleTime <= Min || cycleTime >= Max).
        var recipe = useNullRecipe ? null : Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, barCode) = BuildLoadState(recipe, targetCycleTime: cycleTime);

        // Act
        var result = await BuildHandler(load).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — FAILURE result with the override's message and the recipe-aware specific code.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Cycle time is invalid"));

        var expectedCode = useNullRecipe ? ResultValidation.RecipeNotFound : ResultValidation.PartNotValid;
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(expectedCode);

        // Assert — the OVERRIDE is observable on the mutated entities (the real evidence the override ran).
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedNok); // forced down from FinishedOk
        cycle.PartStatus.ShouldBe(PartStatus.NOk);
        barCode.PartStatus.ShouldBe(PartStatus.NOk);
    }

    /// <summary>
    /// AC5/AC6 — CONTROL (non-override) case: an in-range cycle time (Min &lt; t &lt; Max) does
    /// NOT trigger the override — the cycle stays FinishedOk / Ok and the result is SUCCESS.
    /// Proves the override test pins the override specifically, not a constant failure.
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_InRange_Stays_FinishedOk_Ok_Success()
    {
        // Arrange — strictly inside (10, 20).
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, barCode) = BuildLoadState(recipe, targetCycleTime: 15);

        // Act
        var result = await BuildHandler(load).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — SUCCESS, no override.
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        cycle.PartStatus.ShouldBe(PartStatus.Ok);
        barCode.PartStatus.ShouldBe(PartStatus.Ok);
    }
}
