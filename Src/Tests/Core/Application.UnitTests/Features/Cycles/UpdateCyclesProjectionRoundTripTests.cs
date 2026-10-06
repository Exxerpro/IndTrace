// <copyright file="UpdateCyclesProjectionRoundTripTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Commands.UpdateCycles;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Application.Cycles.Services.Strategies;
using IndTrace.Domain.Services.Interfaces;

/// <summary>
/// Story 6.5 (Task 4) — §7 PLC numeric/tag projection round-trip through the FULL unified
/// <see cref="UpdateCyclesCommandHandler"/> after the stateful god-object was retired from the cycle-update path.
/// The handler now loads an immutable <see cref="CycleUpdateLoadState"/>, decides on a
/// <see cref="CycleUpdateContext"/>, and projects via <c>CycleUpdateProjection.ToResponse</c> (a real static call,
/// NOT a mock). These tests pin the projected <c>TaskGatewayResponse</c> numeric tags
/// (CycleStatus / FlowStatus / PartStatus / CyclesOk) which are the FROZEN PLC contract
/// (state-machine-analysis.md §7) and MUST stay byte-equal to the retired god-object path.
///
/// FAITHFUL to real behavior (unlike the pre-Task-4 version, which rigged a substitute god-object's getters):
/// the OK path performs NO status echo, so the projection surfaces the LOAD-TIME scalars
/// (Started / InProcess / Ok) even though the DECIDE step mutates the cycle/barcode ENTITIES to
/// FinishedOk / Finished. The NOT-OK path echoes the DERIVED entity status (FinishedNok / NOk / calculated flow).
/// </summary>
public class UpdateCyclesProjectionRoundTripTests
{
    private const int MachineId = 100;
    private const int CycleId = 777;
    private const int BarCodeId = 555;
    private const int RecipeMin = 10;
    private const int RecipeMax = 60;

    private static readonly DateTime FinishNow = new(2026, 1, 1, 0, 0, 30, DateTimeKind.Local);

    private readonly IShiftService _shiftService = Substitute.For<IShiftService>();
    private readonly IAggregateRepository<BarCode> _aggregateRepository = CycleAggregateRepositoryTestDouble.Passthrough();
    private readonly IRegisterCleaner _registerCleaner = Substitute.For<IRegisterCleaner>();
    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();
    private readonly IFlowStatusCalculator _flowStatusCalculator = Substitute.For<IFlowStatusCalculator>();

    public UpdateCyclesProjectionRoundTripTests()
    {
        _dateTime.Now.Returns(FinishNow);
        _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { ShiftId = 9, CyclesOk = 3 })));
        _registerCleaner.CleanRegisters(Arg.Any<IDictionary<string, Register>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime>())
            .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));
        _flowStatusCalculator.Calculate(Arg.Any<MachineType>(), Arg.Any<CycleStatus>(), Arg.Any<PartStatus>())
            .Returns(FlowStatus.Finished);
    }

    private static Recipe ValidRecipe() => Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();

    /// <summary>
    /// Builds an immutable <see cref="CycleUpdateLoadState"/> whose scalar status fields are the LOAD-TIME values
    /// (Started / InProcess / Ok) — exactly what the god-object's scalar getters held at load. The Cycle/BarCode
    /// entity references are the SAME tracked instances the DECIDE step mutates in place.
    /// </summary>
    private static (CycleUpdateLoadState Load, Cycle Cycle, BarCode BarCode) BuildLoadState(Recipe? recipe, MachineType machineType)
    {
        var cycle = new CycleBuilder().Started(PartStatus.Ok)
            .With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(MachineId); c.StartedOn = FinishNow.AddSeconds(-30); }).Build();
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
            CycleStatus: CycleStatus.Started,   // LOAD-TIME scalar (god-object AssignWorkflowAndMachineDetails)
            FlowStatus: FlowStatus.InProcess,   // LOAD-TIME scalar
            PartStatus: PartStatus.Ok,          // LOAD-TIME scalar
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

    private static UpdateCyclesOkCommand OkCommand() => new()
    {
        Command = new TaskGatewayRequest
        {
            MachineId = MachineId, BarCode = "BC-OK", PartNumber = "PART",
            CycleStatus = CycleStatus.FinishedOk, PartStatus = PartStatus.Ok,
            Registers = new Dictionary<string, Register>(),
        },
    };

    private static UpdateCyclesNotOkCommand NotOkCommand(CycleStatus cycleStatus, PartStatus partStatus) =>
        new UpdateCyclesNotOkCommand().WithData(new TaskGatewayRequest
        {
            MachineId = MachineId, BarCode = "BC-NOK", PartNumber = "PART",
            CycleStatus = cycleStatus, PartStatus = partStatus,
            Registers = new Dictionary<string, Register>(),
        });

    /// <summary>
    /// OK dispatch: the §7 projection surfaces the LOAD-TIME scalars (Started / InProcess / Ok) — the OK path
    /// performs NO god-object echo — plus the shift's CyclesOk. The cycle/barcode ENTITIES are mutated to
    /// FinishedOk underneath, but the frozen PLC scalar tags keep their load-time values (byte-equal to the
    /// retired god-object path). This is the user-confirmed (2026-06-21) preserve-byte-equal contract.
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_ProjectsFrozenTags_LoadTimeScalarsSurvive()
    {
        // Arrange — Final machine, cycle time 30 in (10, 60).
        var (load, cycle, _) = BuildLoadState(ValidRecipe(), MachineType.Final);
        var handler = BuildHandler(load);

        // Act
        var result = await handler.ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — frozen §7 numeric tags are the LOAD-TIME scalars (no echo on OK).
        result.IsSuccess.ShouldBeTrue();
        var dto = result.Value.ShouldNotBeNull();
        dto.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        dto.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
        dto.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
        dto.CyclesOk.ShouldBe(3);

        // The split: the DECIDE step DID mutate the entity to FinishedOk, but the scalar tag stays load-time.
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        dto.Cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
    }

    /// <summary>
    /// NOT-OK dispatch with a MISMATCHED command CycleStatus (FinishedOk): the §7 projection must surface the
    /// DERIVED FinishedNok / NOk — proving the NOT-OK status echo (now reproduced inside
    /// <c>CycleUpdateProjection.ToResponse</c>) drives the projection identically to the pre-refactor god-object.
    /// </summary>
    [Fact]
    public async Task UpdateCycleNotOk_MismatchedCommand_ProjectsDerivedFinishedNok()
    {
        // Arrange
        var (load, _, _) = BuildLoadState(ValidRecipe(), MachineType.Final);
        var handler = BuildHandler(load);

        // Act — command carries FinishedOk/Ok, but the NotOk trigger derives FinishedNok/NOk.
        var result = await handler.ProcessAsync(NotOkCommand(CycleStatus.FinishedOk, PartStatus.Ok), TestContext.Current.CancellationToken);

        // Assert — projected tags are the derived FinishedNok / NOk (echoed from the result entities), and the
        // calculator-produced FlowStatus.Finished.
        result.IsSuccess.ShouldBeTrue();
        var dto = result.Value.ShouldNotBeNull();
        dto.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        dto.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        dto.FlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
    }

    /// <summary>
    /// Story 6.5 (Task 5) — OK dispatch, cycle time OUT OF RANGE (window (40, 60), derived time 30): the
    /// Cycle.FinishOk guard forces the FinishedNok/NOk override and the strategy returns a FAILURE carrying a
    /// value. The unified handler must reproduce the legacy <c>Diagnose</c> projection: a value-CARRYING failure
    /// whose DTO publishes the recipe-aware <see cref="ResultValidation.PartNotValid"/> (-64) — the negative-code
    /// projection the PLC consumes (user-confirmed 2026-06-21). Mirrors the legacy golden master
    /// <c>UpdateCycleOk_OutOfRange_OverrideFires_FinishedNok_FailureWithDto</c>, now pinned on the unified path.
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_OutOfRange_FailsWithDto_PartNotValid()
    {
        // Arrange — recipe window (40, 60); derived cycle time is 30 -> below minimum -> out of range.
        var (load, _, _) = BuildLoadState(Recipe.Create(0, 0, 40, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull(), MachineType.Final);
        var handler = BuildHandler(load);

        // Act
        var result = await handler.ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — value-carrying failure with the recipe-aware negative code on the DTO + the References tag.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Cycle time is invalid"));
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.PartNotValid);
    }

    /// <summary>
    /// Story 6.5 (Task 5) — OK dispatch, NULL recipe: the same FinishedNok/NOk override fires, but the precise
    /// diagnostic is <see cref="ResultValidation.RecipeNotFound"/> (-512) per the CycleTimeGuard authority. The
    /// unified handler carries the value-bearing failure DTO so the PLC negative-code projection survives. Mirrors
    /// the null-recipe row of the legacy golden master, now pinned on the unified path.
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_NullRecipe_FailsWithDto_RecipeNotFound()
    {
        // Arrange — null recipe forces the override regardless of cycle time.
        var (load, _, _) = BuildLoadState(recipe: null, MachineType.Final);
        var handler = BuildHandler(load);

        // Act
        var result = await handler.ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Cycle time is invalid"));
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.RecipeNotFound);
    }

    /// <summary>
    /// Story 6.5 (Task 5) — a LOAD failure (barcode not found) must reach the PLC as its SPECIFIC code, not the
    /// transport's generic -1 (user-confirmed 2026-06-21). The unified handler classifies the message and returns
    /// a value-CARRYING failure carrying <see cref="ResultValidation.BarCodeNotFound"/>.
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_LoadFailure_BarCodeNotFound_FailsWithDiagnosticCode()
    {
        // Arrange — the loader fails with a "BarCode not Found" message (no snapshot available).
        var provider = Substitute.For<IBarCodeInfoProvider>();
        provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CycleUpdateLoadState>.WithFailure("BarCode not Found BarCodeId: 555")));

        var handler = new UpdateCyclesCommandHandler(
            provider,
            Substitute.For<IStationValidator>(),
            Substitute.For<ICycleUpdateStrategyFactory>(),
            Substitute.For<ICommandLogger>(),
            Substitute.For<ILogger<UpdateCyclesCommandHandler>>());

        // Act
        var result = await handler.ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — value-carrying failure with the specific BarCodeNotFound code.
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.BarCodeNotFound);
    }

    /// <summary>
    /// Story 6.5 (Task 5) — a station REFUSAL ("cycle created on another station") must publish the validator's
    /// own <see cref="ResultValidation.DestinationNotValid"/> code, not a generic -1 (user-confirmed 2026-06-21).
    /// Dispatched via the NOT-OK command to prove the diagnostic surfaces on BOTH paths (the NotOk path otherwise
    /// disregards the cycle-time anomaly but must still report genuine routing failures).
    /// </summary>
    [Fact]
    public async Task UpdateCycleNotOk_DestinationInvalid_FailsWithDestinationNotValidCode()
    {
        // Arrange — loader succeeds, but the station validator refuses with DestinationNotValid.
        var (load, _, _) = BuildLoadState(ValidRecipe(), MachineType.Final);
        var provider = Substitute.For<IBarCodeInfoProvider>();
        provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CycleUpdateLoadState>.Success(load)));

        var validator = Substitute.For<IStationValidator>();
        validator.ValidateStation(Arg.Any<int>(), Arg.Any<CycleStatus>(), Arg.Any<CycleUpdateLoadState>())
            .Returns(Result<StationValidationResult>.Success(
                new StationValidationResult(false, "Cannot update cycles created on another station", ResultValidation.DestinationNotValid)));

        var handler = new UpdateCyclesCommandHandler(
            provider, validator,
            Substitute.For<ICycleUpdateStrategyFactory>(),
            Substitute.For<ICommandLogger>(),
            Substitute.For<ILogger<UpdateCyclesCommandHandler>>());

        // Act
        var result = await handler.ProcessAsync(NotOkCommand(CycleStatus.FinishedNok, PartStatus.NOk), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.DestinationNotValid);
    }
}
