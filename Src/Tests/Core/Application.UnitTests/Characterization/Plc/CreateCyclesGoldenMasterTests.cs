// <copyright file="CreateCyclesGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Application.UnitTests.TestDoubles;
using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Validation;
using IndTrace.Application.Gateway.Auditing;
using IndTrace.Application.StateMachine;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.Characterization.Plc;

/// <summary>
/// Golden characterization tests (issue #175, epic #174) that pin the CURRENT observable behavior of
/// <see cref="CreateCyclesCommandHandler"/> — every failure branch plus the success path — BEFORE the
/// functional-pipeline refactor. Each failure branch is pinned under BOTH <c>SpecificDiagnostics</c> ON
/// (a value-CARRYING failure whose DTO holds the branch's specific negative <see cref="ResultValidation"/>)
/// and OFF (a value-LESS failure so the transport's legacy generic <c>-1</c> collapse re-applies). Audit /
/// persistence side effects (which collaborators were written) and log side effects (level + message
/// fragment) are pinned per branch — read from the running code, not from comments.
/// </summary>
public class CreateCyclesGoldenMasterTests
{
    private const int SnapshotMachineId = 1;
    private const int RequestMachineId = 2;
    private const int BarCodeId = 10;

    private static readonly DateTime FixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);

    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();
    private readonly IBarCodeDetailsLoader _loader = Substitute.For<IBarCodeDetailsLoader>();
    private readonly IStationValidator _stationValidator = Substitute.For<IStationValidator>();
    private readonly ICycleLimitPolicy _cycleLimitPolicy = Substitute.For<ICycleLimitPolicy>();
    private readonly ICycleCreator _cycleCreator = Substitute.For<ICycleCreator>();
    private readonly IGatewayAuditFactory _auditFactory = Substitute.For<IGatewayAuditFactory>();
    private readonly TestLogger<CreateCyclesCommandHandler> _logger = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCyclesGoldenMasterTests"/> class with every
    /// collaborator stubbed GREEN (happy path); individual tests re-stub the single collaborator whose
    /// failure branch they pin.
    /// </summary>
    public CreateCyclesGoldenMasterTests()
    {
        _dateTime.Now.Returns(FixedNow);

        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(BuildSnapshot())));

        _stationValidator.ValidateCanStartCycles(Arg.Any<IBarCodeResult>()).Returns(Result.Success());

        _cycleLimitPolicy.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.Success(new CycleLimitDecision(true, "Allowed", ResultValidation.Valid)));

        _cycleCreator.CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle>.Success(
                new CycleBuilder().Started(PartStatus.Ok).With(c => c.CycleId = new CycleId(99)).Build())));

        _auditFactory.CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TaskGatewayRequest>.Success(new TaskGatewayRequest())));
    }

    /// <summary>
    /// Builds the default clean load snapshot: Created barcode on a Process station, no load-carried error.
    /// Tests derive branch-specific snapshots via <c>with</c> expressions.
    /// </summary>
    private static BarCodeSnapshot BuildSnapshot() => new()
    {
        MachineId = SnapshotMachineId,
        NextMachineId = RequestMachineId,
        BarCodeId = BarCodeId,
        CycleId = 0,
        CycleStatus = CycleStatus.NotStarted,
        FlowStatus = FlowStatus.Created,
        PartStatus = PartStatus.Ok,
        MachineType = MachineType.Process,
        WorkFlowType = WorkFlowType.Initial,
        Label = "BC-CREATE-CYCLE",
        PartNumber = "PART",
        ResultValidation = ResultValidation.Valid,
        Cycles = new List<Cycle>(),
        Recipe = Recipe.Create(0, 0, 0, 216000, 10, 3, 1).Value.ShouldNotBeNull(),
        BarCode = new BarCodeBuilder().Created(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(SnapshotMachineId); }).Build(),
        References = new Dictionary<string, Register>(),
    };

    private void StubSnapshot(BarCodeSnapshot snapshot) =>
        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));

    private CreateCyclesCommandHandler BuildHandler(bool specificDiagnostics = true) =>
        new(
            _logger,
            _dateTime,
            _loader,
            _stationValidator,
            _cycleLimitPolicy,
            _cycleCreator,
            _auditFactory,
            null,
            Options.Create(new StateMachineRoutingOptions { SpecificDiagnostics = specificDiagnostics }));

    private static CreateCyclesCommand Command() => new()
    {
        Command = new TaskGatewayRequest
        {
            BarCode = "BC-CREATE-CYCLE",
            MachineId = RequestMachineId,
            PartNumber = "PART",
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
        },
    };

    private async Task AssertNoWritesAsync()
    {
        await _cycleCreator.DidNotReceive().CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>());
        await _auditFactory.DidNotReceive().CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // Success path — writes, projection tuple, and the two LogInformation entries.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Success path: the cycle create carries the barcode status write in the SAME request (#114 chunk B —
    /// one transactional aggregate save inside the creator), then the gateway audit is written (with trigger
    /// CreateCycleAsync); the projected DTO carries InProcess / Started / Ok with the CREATED cycle riding on
    /// <c>Cycle</c> while the <c>CycleId</c> scalar stays the LOAD-TIME snapshot value; the handler emits the
    /// start and retrieve LogInformation entries.
    /// </summary>
    [Fact]
    public async Task CreateCycle_HappyPath_WritesAll_ProjectsTuple_LogsStartAndRetrieve()
    {
        // Act
        var result = await BuildHandler().ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert — success + §7 projection tuple.
        result.IsSuccess.ShouldBeTrue();
        var dto = result.Value.ShouldNotBeNull();
        dto.FlowStatus.ShouldBe(FlowStatus.InProcess);   // Created -> InProcess (RouteCreateCycle ON)
        dto.CycleStatus.ShouldBe(CycleStatus.Started);   // mirrors the request
        dto.PartStatus.ShouldBe(PartStatus.Ok);
        dto.ResultValidation.ShouldBe(ResultValidation.Valid);
        dto.Cycle.CycleId.Value.ShouldBe(99);            // created cycle rides on the Cycle entity...
        dto.CycleId.ShouldBe(0);                         // ...while the CycleId scalar stays load-time (as-built)

        // Assert — the atomic create+barcode write landed (ONE request carrying both), audit carries the
        // CreateCycleAsync trigger. #114 chunk B: the former separate barcode-update write is folded into the
        // cycle-create request's FlowStatus/ModifiedOn payload.
        await _cycleCreator.Received(1).CreateAsync(
            Arg.Is<CycleCreateRequest>(r =>
                r.CycleStatus == CycleStatus.Started
                && r.BarCodeId == BarCodeId
                && r.FlowStatus == FlowStatus.InProcess
                && r.MachineId == RequestMachineId),
            Arg.Any<CancellationToken>());
        await _auditFactory.Received(1).CreateAuditEntryAsync(
            Arg.Is<GatewayAuditRequest>(r => r.GatewayTask == GatewayTask.CreateCycleAsync && r.CycleId == 99),
            Arg.Any<CancellationToken>());

        // Assert — log side effects: the start + retrieve informational entries, no errors/warnings.
        _logger.HasMessage("Starting cycle creation process").ShouldBeTrue();
        _logger.HasMessage("Retrieved barcode information").ShouldBeTrue();
        _logger.GetLogCount(LogLevel.Information).ShouldBe(2);
        _logger.HasLogLevel(LogLevel.Error).ShouldBeFalse();
        _logger.HasLogLevel(LogLevel.Warning).ShouldBeFalse();
    }

    // ----------------------------------------------------------------------------------
    // Pre-try guards — value-less regardless of the SpecificDiagnostics flag.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A cancelled token short-circuits BEFORE the try block: value-less "Operation was canceled." failure
    /// under BOTH flag states, no load, no writes, and NO log entries (the start log comes after the check).
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCycle_CancelledToken_ValuelessFailure_NoLoadNoWritesNoLogs(bool specificDiagnostics)
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), cts.Token);

        // Assert — value-less on both flag states (the guard precedes the diagnostics mechanic).
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldContain(e => e.Contains("Operation was canceled"));
        await _loader.DidNotReceive().LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>());
        await AssertNoWritesAsync();
        _logger.LogEntries.Count.ShouldBe(0);
    }

    // ----------------------------------------------------------------------------------
    // Load failure / null load — InfrastructureFailure(-262144), no projection available.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A FAILED load Result (exception/cancellation on the read pipeline — NOT a genuine not-found) maps to
    /// InfrastructureFailure(-262144) built WITHOUT a barcode projection: ON carries the diagnostic DTO
    /// stamped with the request's MachineId, OFF is value-less. LogError "Failed to retrieve barcode
    /// information"; no writes on either flag state.
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task CreateCycle_LoadFailureOrNull_InfrastructureFailure_NoWrites(bool specificDiagnostics, bool nullLoad)
    {
        // Arrange — failed Result vs null-value "success": both take the same guard branch (the null-value
        // shape enters via the `Value is null` half: as-built, such a Result reports IsSuccess == false AND
        // IsFailure == false simultaneously).
        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(nullLoad
                ? new Result<BarCodeSnapshot>(true, new List<string>())
                : Result<BarCodeSnapshot>.WithFailure("read pipeline exploded")));

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Failed to retrieve barcode information"));
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.InfrastructureFailure);
            dto.MachineId.ShouldBe(RequestMachineId); // built from the request, not a snapshot
            dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await AssertNoWritesAsync();
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Failed to retrieve barcode information").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // Station validation failure — InvalidMachine(-4096) on the snapshot projection.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Station-cannot-start-cycles maps to InvalidMachine(-4096): ON carries the barcode PROJECTION promoted
    /// to the code (snapshot scalars survive on the DTO), OFF is value-less. LogWarning "Only process
    /// stations can start cycles"; no writes on either flag state.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCycle_StationValidationFails_InvalidMachine_NoWrites(bool specificDiagnostics)
    {
        // Arrange
        _stationValidator.ValidateCanStartCycles(Arg.Any<IBarCodeResult>())
            .Returns(Result.WithFailure("Only process stations can start cycles."));

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Only process stations can start cycles"));
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.InvalidMachine);
            dto.BarCodeId.ShouldBe(BarCodeId);          // projection carries the snapshot scalars
            dto.MachineId.ShouldBe(SnapshotMachineId);
            dto.Label.ShouldBe("BC-CREATE-CYCLE");
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await AssertNoWritesAsync();
        _logger.HasLogLevel(LogLevel.Warning).ShouldBeTrue();
        _logger.HasMessage("Only process stations can start cycles").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // Cycle-limit policy — evaluation failure (PartRejected) and denial (decision's own code).
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A cycle-limit policy EVALUATION failure (the policy could not run) maps to PartRejected(-2048).
    /// LogError "Cycle limit policy evaluation failed"; no writes.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCycle_CycleLimitEvaluationFails_PartRejected_NoWrites(bool specificDiagnostics)
    {
        // Arrange
        _cycleLimitPolicy.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.WithFailure("Cycle limit evaluation failed"));

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.PartRejected);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await AssertNoWritesAsync();
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Cycle limit policy evaluation failed").ShouldBeTrue();
    }

    /// <summary>
    /// A cycle-limit DENIAL (policy evaluated, decision.IsAllowed == false) publishes the DECISION'S OWN
    /// ValidationResult code — not a fixed constant — and the error is the decision's Reason. LogWarning
    /// "Cycle limit policy rejected creation"; no writes.
    /// </summary>
    [Theory]
    [InlineData(nameof(ResultValidation.PartRejected), true)]
    [InlineData(nameof(ResultValidation.WorkFlowNotValid), true)]
    [InlineData(nameof(ResultValidation.PartRejected), false)]
    public async Task CreateCycle_CycleLimitDenied_CarriesDecisionCode_NoWrites(string codeName, bool specificDiagnostics)
    {
        // Arrange — the decision's code must flow through verbatim.
        var decisionCode = EnumModel.FromName<ResultValidation>(codeName);
        _cycleLimitPolicy.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.Success(new CycleLimitDecision(false, "Maximum cycles reached", decisionCode)));

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Maximum cycles reached"));
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(decisionCode);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await AssertNoWritesAsync();
        _logger.HasLogLevel(LogLevel.Warning).ShouldBeTrue();
        _logger.HasMessage("Cycle limit policy rejected creation").ShouldBeTrue();
    }

    /// <summary>
    /// A Restored barcode SKIPS the cycle-limit policy entirely (the policy is never evaluated) and the
    /// create proceeds to success.
    /// </summary>
    [Fact]
    public async Task CreateCycle_RestoredFlowStatus_SkipsCycleLimitPolicy_Succeeds()
    {
        // Arrange
        StubSnapshot(BuildSnapshot() with { FlowStatus = FlowStatus.Restored });

        // Act
        var result = await BuildHandler().ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _cycleLimitPolicy.DidNotReceive().EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>());
        await _cycleCreator.Received(1).CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // #59 load-carried validation error — refuse-to-persist short-circuit (NO writes).
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// #59 traceability integrity: a load-carried validation error (snapshot.Error non-empty) short-circuits
    /// BEFORE any cycle / barcode / audit write — NOTHING is persisted — and the DTO surfaces the snapshot's
    /// OWN specific load-carried code (not a collapsed generic). LogWarning "refusing to persist".
    /// </summary>
    [Theory]
    [InlineData(nameof(ResultValidation.DestinationNotValid), true)]
    [InlineData(nameof(ResultValidation.WorkFlowNotValid), true)]
    [InlineData(nameof(ResultValidation.BarCodeNotFound), true)]
    [InlineData(nameof(ResultValidation.DestinationNotValid), false)]
    public async Task CreateCycle_LoadCarriedError_RefusesToPersist_CarriesSnapshotCode(string codeName, bool specificDiagnostics)
    {
        // Arrange — loader SUCCEEDS but the snapshot carries the soft-reject code + error.
        var loadCarriedCode = EnumModel.FromName<ResultValidation>(codeName);
        StubSnapshot(BuildSnapshot() with
        {
            ResultValidation = loadCarriedCode,
            Error = "Load-time arrival gate refused the part",
        });

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert — failure carrying the snapshot's specific code; error is the snapshot's Error verbatim.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Load-time arrival gate refused the part"));
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(loadCarriedCode);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        // The #59 pin: the refused part leaves NO phantom traceability record.
        await AssertNoWritesAsync();
        _logger.HasLogLevel(LogLevel.Warning).ShouldBeTrue();
        _logger.HasMessage("refusing to persist").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // Cycle-creation step failures — CycleNotFound(-16).
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A failed cycle create maps to CycleNotFound(-16); the audit is NOT attempted. #114 chunk B: this
    /// branch now ALSO covers a barcode-status write failure — the barcode UPDATE rides the creator's single
    /// transactional save, so a failed save rolls back BOTH rows and surfaces here (zero rows persisted;
    /// pre-fix the same fault left a committed orphan Started cycle and surfaced -131072).
    /// LogError "Failed to create cycle".
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCycle_CycleCreateFails_CycleNotFound_NoDownstreamWrites(bool specificDiagnostics)
    {
        // Arrange
        _cycleCreator.CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle>.WithFailure("cycle insert blew up")));

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.CycleNotFound);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await _auditFactory.DidNotReceive().CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>());
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Failed to create cycle").ShouldBeTrue();
    }

    /// <summary>
    /// A NULL created cycle reaches the dedicated null guard: CycleNotFound(-16) with the
    /// "Created cycle is null" message, LogError "Created cycle is null", no downstream writes.
    /// DISCOVERED as-built quirk making this reachable: a Result constructed as success-with-null-value
    /// reports IsSuccess == false AND IsFailure == false simultaneously, so the IsFailure guard passes and
    /// the null-value guard fires.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCycle_CreatedCycleNull_CycleNotFound(bool specificDiagnostics)
    {
        // Arrange — a "success" carrying a null value (the only shape that reaches the null guard).
        _cycleCreator.CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new Result<Cycle>(true, new List<string>())));

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert — the dedicated null guard fires.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Created cycle is null"));
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.CycleNotFound);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await _auditFactory.DidNotReceive().CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>());
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Created cycle is null").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // Post-create persistence failure — ExceptionResultValidation(-131072) on the audit.
    // #114 chunk B GOLDEN INVERSION: the former CreateCycle_BarCodeUpdateFails_ExceptionCode_
    // CycleAlreadyWritten golden pinned the DEFECT itself (a separate barcode-update
    // auto-commit failing AFTER the committed cycle — the orphan a PLC retry duplicated).
    // The barcode write now rides the creator's single transactional save, so that partial
    // write shape is IMPOSSIBLE: a barcode-write failure rolls the cycle back too and
    // surfaces through the cycle-create branch as CycleNotFound(-16), pinned by
    // CreateCycle_CycleCreateFails_CycleNotFound_NoDownstreamWrites above.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A failed gateway-audit write maps to ExceptionResultValidation(-131072) — UNCHANGED semantics: the
    /// cycle AND barcode writes already committed (now atomically, in the creator's ONE transaction) before
    /// this branch fires. LogError "Failed to create gateway audit entry".
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCycle_AuditFails_ExceptionCode_CycleAndBarCodeAlreadyWritten(bool specificDiagnostics)
    {
        // Arrange
        _auditFactory.CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TaskGatewayRequest>.WithFailure("audit insert blew up")));

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.ExceptionResultValidation);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        // The upstream ATOMIC write (cycle + barcode in one save) already happened before this branch.
        await _cycleCreator.Received(1).CreateAsync(
            Arg.Is<CycleCreateRequest>(r => r.FlowStatus == FlowStatus.InProcess),
            Arg.Any<CancellationToken>());
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Failed to create gateway audit entry").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // Unhandled exception — catch block, ExceptionResultValidation(-131072).
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// An unhandled exception anywhere in processing is caught: the failure message is
    /// "Operation finished with an exception {message}"; ON carries a BARE diagnostic DTO (no projection)
    /// stamped ExceptionResultValidation with the request's MachineId, OFF is value-less.
    /// LogError "Unhandled exception in CreateCyclesCommandHandler".
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCycle_UnhandledException_ExceptionResultValidation(bool specificDiagnostics)
    {
        // Arrange — the loader throws.
        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<Result<BarCodeSnapshot>>>(_ => throw new InvalidOperationException("boom"));

        // Act
        var result = await BuildHandler(specificDiagnostics).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Operation finished with an exception boom"));
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.ExceptionResultValidation);
            dto.MachineId.ShouldBe(RequestMachineId);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await AssertNoWritesAsync();
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("Unhandled exception in CreateCyclesCommandHandler").ShouldBeTrue();
    }
}
