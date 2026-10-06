// <copyright file="UpdateCyclesGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Plc;

using Application.UnitTests.Features.Cycles;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Commands.UpdateCycles;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Application.Cycles.Services.Strategies;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;

/// <summary>
/// Golden-master (characterization) tests for the PLC-path cycle-update use case, RETARGETED in Story 6.3 onto
/// the unified <see cref="UpdateCyclesCommandHandler"/> (the sole live cycle-update path after the 6.5
/// production flip; the legacy inline handlers were deleted in 6.3). The handler loads an immutable
/// <see cref="CycleUpdateLoadState"/>, DECIDES via the real <see cref="OkUpdateStrategy"/> /
/// <see cref="NotOkUpdateStrategy"/> over the shared tracked entities, and projects the §7 PLC response.
///
/// Story 1.1, AC 5, 6, 7: pins the AS-BUILT persisted <c>(FlowStatus, CycleStatus, PartStatus, Result)</c>
/// tuples for the in-range and out-of-range (cycle-time override) paths, plus the inclusive-failing cycle-time
/// boundaries. These tuples are the FROZEN §7 PLC contract and are byte-equal to the retired legacy handler
/// (the unified OK strategy wraps the SAME cycle-time guard via <c>Cycle.FinishOk</c>; the real
/// <see cref="FlowStatusCalculator"/> reproduces the Final &amp;&amp; FinishedOk -&gt; Finished rule).
///
/// Mocking notes: the persisted truth lives on the <see cref="Cycle"/> / <see cref="BarCode"/> entities carried
/// by the load state — the strategy mutates them in place (they are the SAME references the loader snapshots),
/// so the persisted tuple is asserted on those entities while success/failure is asserted on the result.
///
/// KNOWN DIVERGENCE pinned here (Story 6.3, user-confirmed 2026-06-21): the legacy NotOk path returned a FAILURE
/// for an out-of-range cycle time; the unified NotOk strategy intentionally DISREGARDS the cycle-time anomaly and
/// SUCCEEDS (it always finishes FinishedNok / NOk). The persisted entity tuple is unchanged; only the result is
/// now Success. See <c>UpdateCycleNotOk_OutOfRange_DisregardsCycleTimeAnomaly_Success</c>.
/// </summary>
public class UpdateCyclesGoldenMasterTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;
    private const int CycleId = 777;

    // Recipe window used across the theory: valid strictly inside (10, 20).
    private const int RecipeMin = 10;
    private const int RecipeMax = 20;

    private static readonly DateTime FinishNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

    private readonly IShiftService _shiftService = Substitute.For<IShiftService>();
    private readonly IAggregateRepository<BarCode> _aggregateRepository = CycleAggregateRepositoryTestDouble.Passthrough();
    private readonly IRegisterCleaner _registerCleaner = Substitute.For<IRegisterCleaner>();
    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();

    // The REAL flow-status calculator — reproduces the legacy Final && FinishedOk -> Finished rule.
    private readonly IFlowStatusCalculator _flowStatusCalculator = new FlowStatusCalculator();

    public UpdateCyclesGoldenMasterTests()
    {
        // Fixed clock so the derived cycle time is deterministic.
        _dateTime.Now.Returns(FinishNow);

        _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { CyclesOk = 1 })));
        _registerCleaner.CleanRegisters(Arg.Any<IDictionary<string, Register>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime>())
            .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));
    }

    /// <summary>
    /// Builds an immutable <see cref="CycleUpdateLoadState"/> whose shared tracked Cycle/BarCode the DECIDE step
    /// mutates in place. The transient cycle's <c>StartedOn</c> is set so the strategy derives a CycleTime of
    /// exactly <paramref name="targetCycleTime"/> seconds (Now - StartedOn). Source state mirrors the legacy
    /// golden master: cycle Started/Ok, barcode InProcess/Ok.
    /// </summary>
    private static (CycleUpdateLoadState Load, Cycle Cycle, BarCode BarCode) BuildLoadState(
        MachineType machineType, Recipe? recipe, int targetCycleTime)
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
            Label: "BC",
            PartNumber: "PART",
            Description: "Machine 100",
            LastMachineId: 0,
            NextMachineId: MachineId,
            CycleStatus: CycleStatus.Started,
            FlowStatus: FlowStatus.InProcess,
            PartStatus: PartStatus.Ok,
            MachineType: machineType,
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
        factory.CreateStrategy(CycleStatus.FinishedNok).Returns(new NotOkUpdateStrategy(
            _registerCleaner, _aggregateRepository, _shiftService, _flowStatusCalculator, _dateTime, Substitute.For<ILogger<NotOkUpdateStrategy>>()));

        var commandLogger = Substitute.For<ICommandLogger>();
        commandLogger.CreateCommand(Arg.Any<CycleUpdateLoadState>(), Arg.Any<GatewayTask>(), Arg.Any<string?>())
            .Returns(new TaskGatewayRequest());
        commandLogger.LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        return new UpdateCyclesCommandHandler(
            provider, stationValidator, factory, commandLogger, Substitute.For<ILogger<UpdateCyclesCommandHandler>>());
    }

    private static UpdateCyclesOkCommand OkCommand(CycleStatus cycleStatus, PartStatus partStatus) =>
        new()
        {
            Command = new TaskGatewayRequest
            {
                MachineId = MachineId,
                BarCode = "BC-OK",
                PartNumber = "PART",
                CycleStatus = cycleStatus,
                PartStatus = partStatus,
                Registers = new Dictionary<string, Register>(),
            },
        };

    private static UpdateCyclesNotOkCommand NotOkCommand(CycleStatus cycleStatus, PartStatus partStatus) =>
        new()
        {
            Command = new TaskGatewayRequest
            {
                MachineId = MachineId,
                BarCode = "BC-NOK",
                PartNumber = "PART",
                CycleStatus = cycleStatus,
                PartStatus = partStatus,
                Registers = new Dictionary<string, Register>(),
            },
        };

    // ----------------------------------------------------------------------------------
    // AC 5 — UpdateCycleOk, time IN RANGE. Matrix rows 3 (non-final) and 4 (Final).
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// AC 5 / matrix row 3 — UpdateCycleOk, in range, non-final station:
    /// cycle becomes FinishedOk / Ok, barcode stays InProcess, result is success.
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_InRange_NonFinal_PersistsFinishedOk_BarcodeInProcess_Success()
    {
        // Arrange — Process machine, cycle time strictly inside (10, 20).
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, barCode) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: 15);

        // Act
        var result = await BuildHandler(load).ProcessAsync(OkCommand(CycleStatus.FinishedOk, PartStatus.Ok), TestContext.Current.CancellationToken);

        // Assert — persisted truth on the entities the strategy mutated.
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        cycle.PartStatus.ShouldBe(PartStatus.Ok);
        barCode.FlowStatus.ShouldBe(FlowStatus.InProcess); // not Final → unchanged
    }

    /// <summary>
    /// AC 5 / matrix row 4 — UpdateCycleOk, in range, Final station:
    /// cycle FinishedOk / Ok, barcode advances to Finished, result is success.
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_InRange_Final_PersistsFinished_Success()
    {
        // Arrange — Final machine, cycle time strictly inside (10, 20).
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, barCode) = BuildLoadState(MachineType.Final, recipe, targetCycleTime: 15);

        // Act
        var result = await BuildHandler(load).ProcessAsync(OkCommand(CycleStatus.FinishedOk, PartStatus.Ok), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        cycle.PartStatus.ShouldBe(PartStatus.Ok);
        barCode.FlowStatus.ShouldBe(FlowStatus.Finished); // Final && FinishedOk → Finished
    }

    // ----------------------------------------------------------------------------------
    // AC 6 — UpdateCycleOk, time OUT OF RANGE → cycle-time override fires.
    // Matrix rows 5 (out of range) and 6 (null recipe). Includes inclusive-failing
    // boundary cases (== Minimum, == Maximum) and in-range boundaries (Min+1, Max-1).
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// AC 6 / matrix rows 5 and 6 — UpdateCycleOk cycle-time override.
    /// Out-of-range OR null recipe forces cycle FinishedNok, cycle PartStatus.NOk,
    /// barcode PartStatus.NOk, AND returns a FAILURE result carrying a populated DTO with
    /// the message "Cycle time is invalid" and the recipe-aware negative code.
    ///
    /// Byte-equal to the legacy handler: the unified OK strategy's <c>Cycle.FinishOk</c> wraps the SAME
    /// cycle-time guard; on rejection it forces FinishedNok/NOk on the entity before failing, and the handler
    /// promotes the recipe-aware code (PartNotValid out-of-range / RecipeNotFound null recipe).
    ///
    /// Boundaries are inclusive on the failing side (cycleTime &lt;= Min || &gt;= Max).
    /// </summary>
    [Theory]
    [InlineData(RecipeMin, false)]      // == Minimum → invalid (inclusive-failing boundary)
    [InlineData(RecipeMax, false)]      // == Maximum → invalid (inclusive-failing boundary)
    [InlineData(RecipeMin - 5, false)]  // below window → invalid
    [InlineData(RecipeMax + 5, false)]  // above window → invalid
    [InlineData(15, true)]              // null recipe → invalid regardless of time
    public async Task UpdateCycleOk_OutOfRange_OverrideFires_FinishedNok_FailureWithDto(int cycleTime, bool useNullRecipe)
    {
        // Arrange
        var recipe = useNullRecipe ? null : Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, barCode) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: cycleTime);

        // Act
        var result = await BuildHandler(load).ProcessAsync(OkCommand(CycleStatus.FinishedOk, PartStatus.Ok), TestContext.Current.CancellationToken);

        // Assert — failure result.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Cycle time is invalid"));

        // The recipe-aware specific code rides on the value-carrying failure (CycleTimeGuard authority):
        //   - OUT OF RANGE with a VALID recipe -> PartNotValid(-64)
        //   - NULL recipe                       -> RecipeNotFound(-512)
        var expectedCode = useNullRecipe ? ResultValidation.RecipeNotFound : ResultValidation.PartNotValid;
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(expectedCode);

        // The override IS observable on the persisted entities (the real evidence the override ran).
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedNok);
        cycle.PartStatus.ShouldBe(PartStatus.NOk);
        barCode.PartStatus.ShouldBe(PartStatus.NOk);
    }

    /// <summary>
    /// AC 5 — in-range boundary verification: Min+1 and Max-1 are strictly inside the
    /// window and must NOT trigger the override (cycle stays FinishedOk / Ok, success).
    /// </summary>
    [Theory]
    [InlineData(RecipeMin + 1)] // just above minimum → valid
    [InlineData(RecipeMax - 1)] // just below maximum → valid
    public async Task UpdateCycleOk_InRangeBoundary_NoOverride_Success(int cycleTime)
    {
        // Arrange
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, _) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: cycleTime);

        // Act
        var result = await BuildHandler(load).ProcessAsync(OkCommand(CycleStatus.FinishedOk, PartStatus.Ok), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        cycle.PartStatus.ShouldBe(PartStatus.Ok);
    }

    // ----------------------------------------------------------------------------------
    // AC 7 — UpdateCycleNotOk. Matrix rows 7 (non-final) and 8 (out of range).
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// AC 7 / matrix row 7 — UpdateCycleNotOk, in range, non-final:
    /// cycle becomes FinishedNok / NOk, barcode stays InProcess, result is success.
    /// </summary>
    [Fact]
    public async Task UpdateCycleNotOk_InRange_NonFinal_PersistsFinishedNok_BarcodeInProcess_Success()
    {
        // Arrange — request carries FinishedNok / NOk; cycle time in range so NO cycle-time anomaly.
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, barCode) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: 15);

        // Act
        var result = await BuildHandler(load).ProcessAsync(NotOkCommand(CycleStatus.FinishedNok, PartStatus.NOk), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedNok);
        cycle.PartStatus.ShouldBe(PartStatus.NOk); // NotOk always finishes NOk
        barCode.FlowStatus.ShouldBe(FlowStatus.InProcess); // NotOk path never advances to Finished
    }

    /// <summary>
    /// AC 7 / matrix row 8 — UpdateCycleNotOk, out of range.
    ///
    /// KNOWN DIVERGENCE (Story 6.3, user-confirmed 2026-06-21): the legacy handler returned a FAILURE here (the
    /// cycle-time override fired on the NotOk path too). The unified NotOk strategy intentionally DISREGARDS the
    /// cycle-time anomaly and SUCCEEDS — it unconditionally finishes FinishedNok / NOk via <c>Cycle.FinishNok</c>.
    /// The PERSISTED entity tuple is identical to the legacy override (cycle FinishedNok / NOk, barcode NOk,
    /// barcode FlowStatus InProcess); only the result is now Success instead of a failure-with-DTO. This is the
    /// already-shipped 6.5 behavior, NOT a regression introduced by 6.3.
    ///
    /// APPROVED PLC-FEEDBACK CHANGE (state-machine-analysis.md §7 "Approved feedback change #1",
    /// docs/architecture/plc/plc-feedback-change-notice.md). The OLD wire result for this case made the PLC read
    /// the generic -1 (legacy Failure with RV ≥ 0 → ControllerExtensions.SetErrorReferences). The NEW result is
    /// Success, so the transport leaves the real FinishedNok(8)/NOk(2) tuple and a NON-NEGATIVE ResultValidation
    /// on the wire — "operation OK, part is NOk" instead of generic -1. The boundary that the old wire (-1) is no
    /// longer reachable here is pinned by <see cref="UpdateCycleNotOk_AnyCycleTime_PlcWireNeverGenericMinusOne"/>.
    /// </summary>
    [Fact]
    public async Task UpdateCycleNotOk_OutOfRange_DisregardsCycleTimeAnomaly_Success()
    {
        // Arrange — cycle time at the maximum boundary (inclusive-failing for the OK path).
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, barCode) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: RecipeMax);

        // Act
        var result = await BuildHandler(load).ProcessAsync(NotOkCommand(CycleStatus.FinishedNok, PartStatus.NOk), TestContext.Current.CancellationToken);

        // Assert — the NotOk path succeeds (cycle-time anomaly disregarded), but the persisted tuple is the same
        // FinishedNok / NOk override as the legacy handler.
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedNok);
        cycle.PartStatus.ShouldBe(PartStatus.NOk);
        barCode.PartStatus.ShouldBe(PartStatus.NOk);
        barCode.FlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// APPROVED PLC-FEEDBACK CHANGE — wire-contract guard (state-machine-analysis.md §7 "Approved feedback
    /// change #1"; docs/architecture/plc/plc-feedback-change-notice.md).
    ///
    /// Pins the boundary the legacy golden master lost when it was retargeted onto the unified handler: for a
    /// NotOk command the PLC must NEVER read the generic -1 for a cycle-time anomaly, regardless of cycle time or
    /// recipe presence. The transport (<c>ControllerExtensions.PublishResultToPlc</c>) writes -1 only when the
    /// result is a failure with <c>ResultValidation ≥ 0</c> OR the projected value is null. This test asserts the
    /// projected wire payload (<c>result.Value</c>) is a SUCCESS carrying the real FinishedNok(8)/NOk(2) tuple and
    /// a NON-NEGATIVE ResultValidation — so SetErrorReferences cannot fire and the old -1 cannot reach the PLC.
    /// If a future change reintroduces a failure (or a negative RV) on the NotOk anomaly path, this test fails.
    /// </summary>
    [Theory]
    [InlineData(15, false)]             // in-range NotOk
    [InlineData(RecipeMin, false)]      // == minimum (legacy: -1) → now real tuple
    [InlineData(RecipeMax, false)]      // == maximum (legacy: -1) → now real tuple
    [InlineData(RecipeMax + 5, false)]  // above window (legacy: -1) → now real tuple
    [InlineData(15, true)]              // null recipe (legacy: -1) → now real tuple
    public async Task UpdateCycleNotOk_AnyCycleTime_PlcWireNeverGenericMinusOne(int cycleTime, bool useNullRecipe)
    {
        // Arrange
        var recipe = useNullRecipe ? null : Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, _, _) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: cycleTime);

        // Act
        var result = await BuildHandler(load).ProcessAsync(NotOkCommand(CycleStatus.FinishedNok, PartStatus.NOk), TestContext.Current.CancellationToken);

        // Assert — the projected wire payload keeps the PLC off the generic -1.
        result.IsSuccess.ShouldBeTrue();                                 // not a failure → SetErrorReferences cannot fire
        var value = result.Value.ShouldNotBeNull();                      // not null → SetErrorReferences cannot fire
        value.ResultValidation.Value.ShouldBeGreaterThanOrEqualTo(0);    // negative=failure convention: this case is non-negative
        value.CycleStatus.ShouldBe(CycleStatus.FinishedNok);             // PLC reads 8, not -1
        value.PartStatus.ShouldBe(PartStatus.NOk);                       // PLC reads 2, not -1
    }

    // ----------------------------------------------------------------------------------
    // Unhandled-exception path (Story 3.3 AC8 / SpecificDiagnostics). Retained on the unified handler in
    // Story 6.3 — the equivalent legacy-handler test was removed with the deleted legacy class. An exception
    // anywhere in processing is caught, classified through PlcFailureDiagnostics ("Exception occurred: …" →
    // ExceptionResultValidation), and surfaced as a value-CARRYING failure so the PLC learns a specific code
    // instead of the transport's generic -1.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// When the load step throws, the unified handler returns a FAILURE whose DTO carries the specific
    /// <see cref="ResultValidation.ExceptionResultValidation"/> code (not a value-less generic failure).
    /// </summary>
    [Fact]
    public async Task UpdateCycle_WhenProcessingThrows_FailsWithExceptionResultValidationDto()
    {
        // Arrange — provider throws; everything else would have succeeded.
        var provider = Substitute.For<IBarCodeInfoProvider>();
        provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<Result<CycleUpdateLoadState>>>(_ => throw new InvalidOperationException("boom"));

        var handler = new UpdateCyclesCommandHandler(
            provider,
            Substitute.For<IStationValidator>(),
            Substitute.For<ICycleUpdateStrategyFactory>(),
            Substitute.For<ICommandLogger>(),
            Substitute.For<ILogger<UpdateCyclesCommandHandler>>());

        // Act
        var result = await handler.ProcessAsync(OkCommand(CycleStatus.FinishedOk, PartStatus.Ok), TestContext.Current.CancellationToken);

        // Assert — value-carrying failure with the specific exception code on the §7 projection.
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.ExceptionResultValidation);
    }
}
