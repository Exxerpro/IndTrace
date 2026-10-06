// <copyright file="UpdateCyclesDiagnosticsGoldenTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Plc;

using Application.UnitTests.Features.Cycles;
using Application.UnitTests.TestDoubles;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Commands.UpdateCycles;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Application.Cycles.Services.Strategies;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Golden characterization tests (issue #175, epic #174) that pin the CURRENT observable behavior of the
/// failure branches, diagnostics, state-machine gate, audit side effects and log side effects of
/// <see cref="UpdateCyclesCommandHandler"/> BEFORE the functional-pipeline refactor. Extends the sibling
/// <see cref="UpdateCyclesGoldenMasterTests"/> (which pins the persisted tuple / cycle-time override paths)
/// without rewriting it.
///
/// KEY AS-BUILT FACT PINNED THROUGHOUT: unlike the Create handlers, this handler does NOT consult
/// <see cref="StateMachineRoutingOptions.SpecificDiagnostics"/> — EVERY mapped failure branch returns a
/// value-CARRYING failure DTO under flag ON and flag OFF alike (only the two Route* gate flags are read).
/// The flag-OFF rows below pin exactly that (read from the running code, not from comments).
/// </summary>
public class UpdateCyclesDiagnosticsGoldenTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;
    private const int CycleId = 777;

    // Recipe window used across the tests: valid strictly inside (10, 20).
    private const int RecipeMin = 10;
    private const int RecipeMax = 20;

    private static readonly DateTime FinishNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

    private readonly IShiftService _shiftService = Substitute.For<IShiftService>();
    private readonly IAggregateRepository<BarCode> _aggregateRepository = CycleAggregateRepositoryTestDouble.Passthrough();
    private readonly IRegisterCleaner _registerCleaner = Substitute.For<IRegisterCleaner>();
    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();
    private readonly IFlowStatusCalculator _flowStatusCalculator = new FlowStatusCalculator();

    private readonly IBarCodeInfoProvider _provider = Substitute.For<IBarCodeInfoProvider>();
    private readonly IStationValidator _stationValidator = Substitute.For<IStationValidator>();
    private readonly ICycleUpdateStrategyFactory _factory = Substitute.For<ICycleUpdateStrategyFactory>();
    private readonly ICommandLogger _commandLogger = Substitute.For<ICommandLogger>();
    private readonly TestLogger<UpdateCyclesCommandHandler> _logger = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCyclesDiagnosticsGoldenTests"/> class with every
    /// collaborator stubbed GREEN over a default in-range load state; individual tests re-stub the single
    /// collaborator whose branch they pin.
    /// </summary>
    public UpdateCyclesDiagnosticsGoldenTests()
    {
        _dateTime.Now.Returns(FinishNow);

        _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { CyclesOk = 1 })));
        _registerCleaner.CleanRegisters(Arg.Any<IDictionary<string, Register>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime>())
            .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));

        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, _, _) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: 15);
        StubLoad(load);

        _stationValidator.ValidateStation(Arg.Any<int>(), Arg.Any<CycleStatus>(), Arg.Any<CycleUpdateLoadState>())
            .Returns(Result<StationValidationResult>.Success(new StationValidationResult(true, null, ResultValidation.Valid)));

        _factory.CreateStrategy(CycleStatus.FinishedOk).Returns(new OkUpdateStrategy(
            _registerCleaner, _aggregateRepository, _shiftService, _flowStatusCalculator, _dateTime, Substitute.For<ILogger<OkUpdateStrategy>>()));
        _factory.CreateStrategy(CycleStatus.FinishedNok).Returns(new NotOkUpdateStrategy(
            _registerCleaner, _aggregateRepository, _shiftService, _flowStatusCalculator, _dateTime, Substitute.For<ILogger<NotOkUpdateStrategy>>()));

        _commandLogger.CreateCommand(Arg.Any<CycleUpdateLoadState>(), Arg.Any<GatewayTask>(), Arg.Any<string?>())
            .Returns(new TaskGatewayRequest());
        _commandLogger.LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
    }

    /// <summary>
    /// Builds an immutable load state whose shared tracked Cycle/BarCode the strategies mutate in place,
    /// mirroring the sibling golden master, PLUS a marker References register so tests can pin that failure
    /// DTOs reuse the LOAD state's References dictionary.
    /// </summary>
    private static (CycleUpdateLoadState Load, Cycle Cycle, BarCode BarCode) BuildLoadState(
        MachineType machineType, Recipe recipe, int targetCycleTime)
    {
        var cycle = new CycleBuilder().Started(PartStatus.Ok)
            .With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(MachineId); c.StartedOn = FinishNow.AddSeconds(-targetCycleTime); }).Build();
        var barCode = new BarCodeBuilder().InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();

        var marker = Register.Create(
            name: "Marker",
            description: string.Empty,
            machineId: MachineId,
            variableId: 0,
            cycleId: 0,
            value: string.Empty,
            dataType: string.Empty,
            statusValueId: 0,
            timeStamp: default).Value.ShouldNotBeNull();

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
            Recipe: recipe,
            MasterLabel: new MasterLabel(),
            References: new Dictionary<string, Register> { ["Marker"] = marker },
            Cycle: cycle,
            BarCode: barCode,
            Product: new Product());

        return (load, cycle, barCode);
    }

    private void StubLoad(CycleUpdateLoadState load) =>
        _provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CycleUpdateLoadState>.Success(load)));

    private UpdateCyclesCommandHandler BuildHandler(
        IItemStateMachine? machine = null,
        IOptions<StateMachineRoutingOptions>? options = null) =>
        new(_provider, _stationValidator, _factory, _commandLogger, _logger, machine, options);

    private static IOptions<StateMachineRoutingOptions> DiagnosticsOff() =>
        Options.Create(new StateMachineRoutingOptions { SpecificDiagnostics = false });

    private static UpdateCyclesOkCommand OkCommand() => new()
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

    private static UpdateCyclesNotOkCommand NotOkCommand() => new()
    {
        Command = new TaskGatewayRequest
        {
            MachineId = MachineId,
            BarCode = "BC-NOK",
            PartNumber = "PART",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.NOk,
            Registers = new Dictionary<string, Register>(),
        },
    };

    // ----------------------------------------------------------------------------------
    // Cancellation — short-circuits before the load and before any log entry.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A cancelled token returns the cancelled failure BEFORE the load and BEFORE the processing
    /// LogInformation: value-less failure, no provider call, no log entries.
    /// </summary>
    [Fact]
    public async Task UpdateCycle_CancelledToken_ValuelessFailure_NoLoadNoLogs()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await BuildHandler().ProcessAsync(OkCommand(), cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        await _provider.DidNotReceive().GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _logger.LogEntries.Count.ShouldBe(0);
    }

    // ----------------------------------------------------------------------------------
    // Load failure — message classified through PlcFailureDiagnostics; flag NOT consulted.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A failed load classifies its FIRST error message into the specific code (BarCode not Found ->
    /// BarCodeNotFound(-2); an unmapped message falls back to Invalid(-1)) and carries the DTO under flag ON
    /// AND flag OFF alike (SpecificDiagnostics is not read by this handler). LogError "Failed to get barcode
    /// info"; no strategy, no command log.
    /// </summary>
    [Theory]
    [InlineData("BarCode not Found", nameof(ResultValidation.BarCodeNotFound), true)]
    [InlineData("BarCode not Found", nameof(ResultValidation.BarCodeNotFound), false)]
    [InlineData("some unmapped storage failure", nameof(ResultValidation.Invalid), true)]
    public async Task UpdateCycle_LoadFails_ClassifiesFirstError_CarriesDtoRegardlessOfFlag(
        string loadError, string expectedCodeName, bool specificDiagnostics)
    {
        // Arrange
        var expectedCode = EnumModel.FromName<ResultValidation>(expectedCodeName);
        _provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CycleUpdateLoadState>.WithFailure(loadError)));
        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(options: options).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — value-carrying under BOTH flag states (as-built: flag not consulted).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains(loadError));
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(expectedCode);
        dto.MachineId.ShouldBe(MachineId);

        _factory.DidNotReceive().CreateStrategy(Arg.Any<CycleStatus>());
        await _commandLogger.DidNotReceive().LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Failed to get barcode info").ShouldBeTrue();
    }

    /// <summary>
    /// A NULL-value load "success" enters the same guard branch via its `Value is null` half, with an EMPTY
    /// error list: the classifier sees no message and falls back to Invalid(-1) on the carried DTO.
    /// </summary>
    [Fact]
    public async Task UpdateCycle_LoadSuccessWithNullValue_FailsWithInvalidDto()
    {
        // Arrange — a "success" carrying a null value.
        _provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new Result<CycleUpdateLoadState>(true, new List<string>())));

        // Act
        var result = await BuildHandler().ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.Invalid);
        _logger.HasMessage("Failed to get barcode info").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // Station validation — failed Result, null result, and !CanUpdate (explicit code).
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A FAILED station validation Result classifies its message ("Station cannot update cycles" ->
    /// InvalidMachine(-4096)) and the DTO reuses the LOAD state's References dictionary (marker register
    /// present + a ResultValidation register added). LogError "Station validation failed"; no strategy.
    /// </summary>
    [Fact]
    public async Task UpdateCycle_StationValidationFails_ClassifiesInvalidMachine_CarriesLoadReferences()
    {
        // Arrange
        _stationValidator.ValidateStation(Arg.Any<int>(), Arg.Any<CycleStatus>(), Arg.Any<CycleUpdateLoadState>())
            .Returns(Result<StationValidationResult>.WithFailure("Station cannot update cycles"));

        // Act
        var result = await BuildHandler().ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.InvalidMachine);
        dto.References.ContainsKey("Marker").ShouldBeTrue(); // load.References reused on the diagnostic DTO
        dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();

        _factory.DidNotReceive().CreateStrategy(Arg.Any<CycleStatus>());
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Station validation failed").ShouldBeTrue();
    }

    /// <summary>
    /// A station validation "success" carrying a NULL result reaches the dedicated null guard: the fixed
    /// "Station validation returned null result" message classifies to no arm -> Invalid(-1) on a DTO
    /// carrying the load References. LogError with the same fixed message; no strategy.
    /// </summary>
    [Fact]
    public async Task UpdateCycle_StationValidationNullResult_FailsWithInvalidDto()
    {
        // Arrange — a "success" carrying a null value (reaches the null guard: such a Result reports
        // IsSuccess == false AND IsFailure == false simultaneously, so the IsFailure guard passes).
        _stationValidator.ValidateStation(Arg.Any<int>(), Arg.Any<CycleStatus>(), Arg.Any<CycleUpdateLoadState>())
            .Returns(new Result<StationValidationResult>(true, new List<string>()));

        // Act
        var result = await BuildHandler().ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — the dedicated null guard fires.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Station validation returned null result"));
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.Invalid);
        dto.References.ContainsKey("Marker").ShouldBeTrue();
        _factory.DidNotReceive().CreateStrategy(Arg.Any<CycleStatus>());
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Station validation returned null result").ShouldBeTrue();
    }

    /// <summary>
    /// A station refusal (CanUpdate == false) publishes the validator's OWN code VERBATIM (explicitCode wins
    /// over message classification) under flag ON and OFF alike. LogError "Station cannot update"; the
    /// strategy never runs and nothing is mutated on the tracked cycle.
    /// </summary>
    [Theory]
    [InlineData(nameof(ResultValidation.DestinationNotValid), true)]
    [InlineData(nameof(ResultValidation.WorkFlowNotValid), true)]
    [InlineData(nameof(ResultValidation.DestinationNotValid), false)]
    public async Task UpdateCycle_StationCannotUpdate_CarriesValidatorCodeVerbatim(string codeName, bool specificDiagnostics)
    {
        // Arrange
        var validatorCode = EnumModel.FromName<ResultValidation>(codeName);
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, _) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: 15);
        StubLoad(load);
        _stationValidator.ValidateStation(Arg.Any<int>(), Arg.Any<CycleStatus>(), Arg.Any<CycleUpdateLoadState>())
            .Returns(Result<StationValidationResult>.Success(new StationValidationResult(false, "Cycle belongs to another station", validatorCode)));
        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(options: options).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Cycle belongs to another station"));
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(validatorCode);

        _factory.DidNotReceive().CreateStrategy(Arg.Any<CycleStatus>());
        cycle.CycleStatus.ShouldBe(CycleStatus.Started); // nothing mutated / persisted
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Station cannot update").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // State-machine gate (D2) — table-reject BEFORE the strategy; Route* flags gate the fire.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A gate rejection whose outcome VALUE carries a code publishes THAT code; the failure message pins the
    /// "(FlowStatus, Trigger)" rejection shape; the strategy never runs, nothing is mutated, and the DTO
    /// reuses the load References. LogError "Cycle update rejected by state machine".
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_GateRejects_WithOutcomeValue_CarriesMachineCode_NothingPersisted()
    {
        // Arrange — mocked machine rejects with a specific code on the outcome value.
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure(
                "table reject",
                new TransitionOutcome(FlowStatus.InProcess, CycleStatus.FinishedOk, PartStatus.Ok, ResultValidation.WorkFlowNotValid)));
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, _) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: 15);
        StubLoad(load);

        // Act
        var result = await BuildHandler(machine).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Cycle update rejected by state machine: (InProcess, UpdateCycleOkAsync)"));
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.WorkFlowNotValid);
        dto.References.ContainsKey("Marker").ShouldBeTrue();

        _factory.DidNotReceive().CreateStrategy(Arg.Any<CycleStatus>());
        await _commandLogger.DidNotReceive().LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        cycle.CycleStatus.ShouldBe(CycleStatus.Started); // strategy never mutated the tracked entity
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Cycle update rejected by state machine").ShouldBeTrue();
    }

    /// <summary>
    /// A gate rejection with a NULL outcome value falls back to OperationCancelled(-65536). Pinned on the
    /// NotOk dispatch, which also proves the gate fires with the UpdateCycleNotOkAsync trigger.
    /// </summary>
    [Fact]
    public async Task UpdateCycleNotOk_GateRejects_NullOutcome_FallsBackToOperationCancelled()
    {
        // Arrange — value-less machine failure.
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure("table reject"));

        // Act
        var result = await BuildHandler(machine).ProcessAsync(NotOkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("(InProcess, UpdateCycleNotOkAsync)"));
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.OperationCancelled);

        machine.Received(1).Fire(Arg.Any<BarCode>(), GatewayTask.UpdateCycleNotOkAsync, Arg.Any<TransitionContext>());
        _factory.DidNotReceive().CreateStrategy(Arg.Any<CycleStatus>());
    }

    /// <summary>
    /// With the matching Route* flag OFF the gate fire is SKIPPED entirely (zero-redeploy rollback) and the
    /// update succeeds through the strategy — even with a machine that would reject.
    /// </summary>
    [Theory]
    [InlineData(true)]  // OK dispatch, RouteUpdateCycleOk = false
    [InlineData(false)] // NotOk dispatch, RouteUpdateCycleNotOk = false
    public async Task UpdateCycle_RouteFlagOff_SkipsGateFire_Succeeds(bool okDispatch)
    {
        // Arrange — a rejecting machine that must never be consulted.
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure("table reject"));
        var options = Options.Create(okDispatch
            ? new StateMachineRoutingOptions { RouteUpdateCycleOk = false }
            : new StateMachineRoutingOptions { RouteUpdateCycleNotOk = false });

        // Act
        var result = okDispatch
            ? await BuildHandler(machine, options).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken)
            : await BuildHandler(machine, options).ProcessAsync(NotOkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        machine.DidNotReceive().Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>());
    }

    // ----------------------------------------------------------------------------------
    // Strategy failures — value-carrying (cycle-time override) vs value-less (infra) vs null success.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// The OK cycle-time override (the ONLY value-carrying strategy failure) keeps its recipe-aware code
    /// with SpecificDiagnostics OFF too — pinning that this handler never consults the flag. Out-of-range
    /// with a valid recipe -> PartNotValid(-64) on the §7 projection.
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_CycleTimeOverride_FlagOff_StillCarriesPartNotValidDto()
    {
        // Arrange — cycle time at the inclusive-failing maximum boundary.
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();
        var (load, cycle, _) = BuildLoadState(MachineType.Process, recipe, targetCycleTime: RecipeMax);
        StubLoad(load);

        // Act
        var result = await BuildHandler(options: DiagnosticsOff()).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — identical to flag ON (see sibling golden master): value-carrying PartNotValid failure.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Cycle time is invalid"));
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.PartNotValid);
        cycle.CycleStatus.ShouldBe(CycleStatus.FinishedNok); // override persisted on the entity
    }

    /// <summary>
    /// A value-LESS strategy failure (infra: shift create / register clean / persist) classifies its first
    /// message into the specific code ("Cannot create Shift" -> ShiftInvalid(-32768)) on a diagnostic DTO
    /// carrying the load References. LogError "Strategy execution failed".
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_StrategyInfraFailure_ClassifiesShiftInvalid()
    {
        // Arrange — mocked strategy failing WITHOUT a value.
        var strategy = Substitute.For<ICycleUpdateStrategy>();
        strategy.ExecuteAsync(Arg.Any<IUpdateCycleCommand>(), Arg.Any<CycleUpdateContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CycleUpdateResult>.WithFailure("Cannot create Shift for Machine 100")));
        _factory.CreateStrategy(CycleStatus.FinishedOk).Returns(strategy);

        // Act
        var result = await BuildHandler().ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Cannot create Shift"));
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.ShiftInvalid);
        dto.References.ContainsKey("Marker").ShouldBeTrue();

        await _commandLogger.DidNotReceive().LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Strategy execution failed").ShouldBeTrue();
    }

    /// <summary>
    /// A strategy "success" carrying a NULL result reaches the handler's ONE value-LESS failure branch:
    /// "Strategy returned a null result" with NO DTO — under flag ON and OFF alike. LogError "Strategy
    /// returned success with a null result".
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateCycleOk_StrategySuccessNullResult_ValuelessFailure(bool specificDiagnostics)
    {
        // Arrange — a "success" carrying a null value (reaches the null guard: such a Result reports
        // IsSuccess == false AND IsFailure == false simultaneously, so the IsFailure guard passes).
        var strategy = Substitute.For<ICycleUpdateStrategy>();
        strategy.ExecuteAsync(Arg.Any<IUpdateCycleCommand>(), Arg.Any<CycleUpdateContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new Result<CycleUpdateResult>(true, new List<string>())));
        _factory.CreateStrategy(CycleStatus.FinishedOk).Returns(strategy);
        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(options: options).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — value-LESS on both flag states (as-built asymmetry vs every other branch).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Strategy returned a null result"));
        result.Value.ShouldBeNull();
        await _commandLogger.DidNotReceive().LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Strategy returned success with a null result").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // Unhandled exception — always a value-carrying ExceptionResultValidation DTO.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// An unhandled exception is caught and surfaced as "Exception occurred: {message}" with a DTO carrying
    /// ExceptionResultValidation(-131072) — under flag ON and OFF alike (flag not consulted). LogError
    /// "Exception processing cycle update".
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateCycle_UnhandledException_CarriesExceptionDtoRegardlessOfFlag(bool specificDiagnostics)
    {
        // Arrange — the provider throws.
        _provider.GetCycleUpdateLoadStateAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<Result<CycleUpdateLoadState>>>(_ => throw new InvalidOperationException("boom"));
        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(options: options).ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Exception occurred: boom"));
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.ExceptionResultValidation);
        dto.MachineId.ShouldBe(MachineId);
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Exception processing cycle update").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // Success path — logs, command-log audit write, best-effort drop, NotOk dispatch parity.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// OK success path: the processing and completion LogInformation entries are emitted, the command log
    /// is written exactly once with the UpdateCycleOkAsync trigger, and the FinishedOk strategy was chosen.
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_Success_LogsAndWritesCommandLogWithOkTrigger()
    {
        // Act
        var result = await BuildHandler().ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        _factory.Received(1).CreateStrategy(CycleStatus.FinishedOk);
        _commandLogger.Received(1).CreateCommand(Arg.Any<CycleUpdateLoadState>(), GatewayTask.UpdateCycleOkAsync, Arg.Any<string?>());
        await _commandLogger.Received(1).LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());

        _logger.HasMessage("Processing cycle update").ShouldBeTrue();
        _logger.HasMessage("Cycle update completed successfully").ShouldBeTrue();
        _logger.HasLogLevel(LogLevel.Error).ShouldBeFalse();
    }

    /// <summary>
    /// NotOk dispatch parity: the FinishedNok strategy is chosen and the command log carries the
    /// UpdateCycleNotOkAsync trigger.
    /// </summary>
    [Fact]
    public async Task UpdateCycleNotOk_Success_UsesNokStrategyAndNokTrigger()
    {
        // Act
        var result = await BuildHandler().ProcessAsync(NotOkCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _factory.Received(1).CreateStrategy(CycleStatus.FinishedNok);
        _commandLogger.Received(1).CreateCommand(Arg.Any<CycleUpdateLoadState>(), GatewayTask.UpdateCycleNotOkAsync, Arg.Any<string?>());
    }

    /// <summary>
    /// #65 best-effort command log: a FAILED command-log write does NOT flip the frozen §7 success response —
    /// the result stays Success — and the drop is flagged with LogError "Cycle-update command-log write did
    /// not land".
    /// </summary>
    [Fact]
    public async Task UpdateCycleOk_CommandLogDrop_StillSucceeds_LogsError()
    {
        // Arrange
        _commandLogger.LogCommandAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("command log db down")));

        // Act
        var result = await BuildHandler().ProcessAsync(OkCommand(), TestContext.Current.CancellationToken);

        // Assert — wire response unchanged; drop logged.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Cycle-update command-log write did not land").ShouldBeTrue();
    }
}
