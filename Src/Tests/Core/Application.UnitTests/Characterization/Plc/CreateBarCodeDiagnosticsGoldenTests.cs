// <copyright file="CreateBarCodeDiagnosticsGoldenTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Plc;

using Application.UnitTests.TestDoubles;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Golden characterization tests (issue #175, epic #174) that pin the CURRENT observable behavior of the
/// failure branches, <see cref="StateMachineRoutingOptions.SpecificDiagnostics"/> ON/OFF split, failure- and
/// success-audit writes, state-machine gate and log side effects of <see cref="CreateBarCodeCommandHandler"/>
/// BEFORE the functional-pipeline refactor. Extends the sibling <see cref="CreateHandlersGoldenMasterTests"/>
/// (success tuple) and the Features suite (ON-flag error strings) without rewriting them.
///
/// KEY AS-BUILT ASYMMETRY PINNED HERE: the failure-audit row ALWAYS carries the SPECIFIC classified
/// <see cref="ResultValidation"/> code — even when <c>SpecificDiagnostics</c> is OFF and the returned wire
/// failure is value-less (legacy <c>-1</c> collapse). The flag gates only the RETURNED diagnostic value,
/// never the audit.
/// </summary>
public class CreateBarCodeDiagnosticsGoldenTests
{
    private const int MachineId = 77;
    private const int ProductId = 7;
    private const string PartNumber = "PART01";

    // Failure-step selectors for the classification theory.
    private const string StepInvalidMachineId = "InvalidMachineId";
    private const string StepMachineMissing = "MachineMissing";
    private const string StepProductMissing = "ProductMissing";
    private const string StepRuleMissing = "RuleMissing";
    private const string StepMasterLabels = "MasterLabels";
    private const string StepReferencesEmpty = "ReferencesEmpty";
    private const string StepUnmappedShift = "UnmappedShift";

    /// <summary>
    /// Frozen, valid rule JSON fixture (mirrors the sibling golden master) so the inline label generator
    /// produces a label without touching the database.
    /// </summary>
    private const string RuleJsonFixture =
        @"{""ruleId"": ""R001"",""ruleFunction"": [""lineIdentifier"", ""fixedPart"", ""partNumber""]," +
        @"""components"": {""lineIdentifier"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""WS""}," +
        @"""fixedPart"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""100""}," +
        @"""partNumber"": {""action"": ""string"",""origin"": ""program"",""lengthMin"": 6,""lengthMax"": 9}}}";

    private static readonly DateTime FixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);

    private readonly IReadOnlyRepository<Rule> _ruleRepository = Substitute.For<IReadOnlyRepository<Rule>>();
    private readonly IAggregateRepository<BarCode> _barCodeAggregateRepository = Substitute.For<IAggregateRepository<BarCode>>();
    private readonly IReadOnlyRepository<Machine> _machineRepository = Substitute.For<IReadOnlyRepository<Machine>>();
    private readonly IReadOnlyRepository<Product> _productRepository = Substitute.For<IReadOnlyRepository<Product>>();
    private readonly IReadOnlyRepository<Variable> _variableRepository = Substitute.For<IReadOnlyRepository<Variable>>();
    private readonly IRepository<TaskGatewayRequest> _requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
    private readonly IShiftService _shiftService = Substitute.For<IShiftService>();
    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();
    private readonly IMasterLabelService _masterLabelService = Substitute.For<IMasterLabelService>();
    private readonly IBarCodeService _barCodeService = Substitute.For<IBarCodeService>();
    private readonly TestLogger<CreateBarCodeCommandHandler> _logger = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBarCodeDiagnosticsGoldenTests"/> class with every
    /// collaborator stubbed GREEN (full success pipeline, EF-style identity back-fill mirrored); individual
    /// tests re-stub the single collaborator whose branch they pin.
    /// </summary>
    public CreateBarCodeDiagnosticsGoldenTests()
    {
        _dateTime.Now.Returns(FixedNow);

        var machine = new Machine { MachineId = new MachineId(MachineId), Name = "Printer-77", MachineType = MachineType.Printer, WorkFlowType = WorkFlowType.Initial };
        _machineRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Machine?>.Success(machine)));

        var product = Product.CreateFixture(productId: ProductId, partNumber: PartNumber);
        _productRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product?>.Success(product)));

        var rule = new Rule { RuleId = 1, MachineId = new MachineId(MachineId), ProductId = new ProductId(ProductId), IsActive = true, Version = 1, RuleJson = RuleJsonFixture };
        _ruleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Rule>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Rule?>.Success(rule)));

        _masterLabelService.GetMasterLabelByPartNumberAsync(PartNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<List<string>>.Success(new List<string>())));
        _barCodeService.GetConsecutiveByBarCodeLabelAsync(PartNumber, Arg.Any<List<string>>(), Arg.Any<Rule?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        // Mirror the real aggregate-repository contract (#114): the atomic SaveAsync back-fills the
        // store-generated identities onto the root and its staged Started cycle after the durable commit
        // (BarCodeId 101 / CycleId 201, matching the retired per-entity AddAsync back-fill pin from PR #182).
        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var root = ci.Arg<BarCode>();
                root.BarCodeId = new BarCodeId(101);
                foreach (var stagedCycle in root.PendingNewCycles)
                {
                    stagedCycle.CycleId = new CycleId(201);
                    stagedCycle.BarCodeId = root.BarCodeId;
                }

                return Task.FromResult(Result.Success());
            });

        _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(MachineId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { CyclesOk = 3 })));

        var refVariable = new Variable
        {
            VariableId = 1,
            MachineId = MachineId,
            Name = "RefTag1",
            IsActive = 1,
            VariableGroupId = TagsGroups.ReferenceTags.Value,
        };
        _variableRepository.ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable> { refVariable })));

        _requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));
    }

    private CreateBarCodeCommandHandler BuildHandler(
        IItemStateMachine? machine = null,
        IOptions<StateMachineRoutingOptions>? options = null) =>
        new(_ruleRepository, _barCodeAggregateRepository, _machineRepository, _productRepository,
            _variableRepository, _requestRepository, _shiftService, _dateTime, _masterLabelService, _barCodeService,
            _logger, machine, options);

    private static IOptions<StateMachineRoutingOptions> DiagnosticsOff() =>
        Options.Create(new StateMachineRoutingOptions { SpecificDiagnostics = false });

    private static CreateBarCodeCommand Command(int machineId = MachineId) => new()
    {
        Command = new TaskGatewayRequest
        {
            MachineId = machineId,
            PartNumber = PartNumber,
            BarCode = string.Empty,
        },
    };

    private void ArrangeFailure(string step)
    {
        switch (step)
        {
            case StepMachineMissing:
                _machineRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result<Machine?>.Success((Machine?)null)));
                break;
            case StepProductMissing:
                _productRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result<Product?>.Success((Product?)null)));
                break;
            case StepRuleMissing:
                _ruleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Rule>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result<Rule?>.Success((Rule?)null)));
                break;
            case StepMasterLabels:
                _masterLabelService.GetMasterLabelByPartNumberAsync(PartNumber, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result<List<string>>.WithFailure("master label backend offline")));
                break;
            case StepReferencesEmpty:
                _variableRepository.ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable>())));
                break;
            case StepUnmappedShift:
                _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(MachineId, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result<ShiftCreatedEvent>.WithFailure("Shift lookup exploded")));
                break;
            default:
                break; // StepInvalidMachineId is arranged via the command's MachineId.
        }
    }

    // ----------------------------------------------------------------------------------
    // Cancellation — short-circuits before the pipeline, the audit and any log entry.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A pre-cancelled token returns the value-less "Operation was canceled." failure BEFORE the railway:
    /// no lookups, NO failure-audit row, no log entries.
    /// </summary>
    [Fact]
    public async Task CreateBarCode_CancelledToken_ValuelessFailure_NoAuditNoLogs()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await BuildHandler().ProcessAsync(Command(), cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldContain("Operation was canceled.");
        await _machineRepository.DidNotReceive().FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>());
        await _requestRepository.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());
        _logger.LogEntries.Count.ShouldBe(0);
    }

    // ----------------------------------------------------------------------------------
    // Per-step classification × SpecificDiagnostics ON/OFF — the central pin.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Pins, per failure step: the exact first error message; the classified code on the returned
    /// value-carrying DTO when <c>SpecificDiagnostics</c> is ON versus the VALUE-LESS failure when OFF;
    /// that the failure-audit row ALWAYS carries the SPECIFIC code (flag not consulted for the audit) with
    /// <c>GatewayTask.CreateBarCodeAsync</c> and the joined errors in <c>Comment</c>; and the single
    /// "CreateBarCode failed for machine … (Code)" LogError. Unmapped messages fall back to Invalid(-1).
    /// </summary>
    [Theory]
    [InlineData(StepInvalidMachineId, "Machine 0 number invalid", nameof(ResultValidation.MachineNotFound), true)]
    [InlineData(StepInvalidMachineId, "Machine 0 number invalid", nameof(ResultValidation.MachineNotFound), false)]
    [InlineData(StepMachineMissing, "Machine 77 does not exist or cannot create labels.", nameof(ResultValidation.MachineNotFound), true)]
    [InlineData(StepMachineMissing, "Machine 77 does not exist or cannot create labels.", nameof(ResultValidation.MachineNotFound), false)]
    [InlineData(StepProductMissing, "Product for PART01 does not exist.", nameof(ResultValidation.ProductNotFound), true)]
    [InlineData(StepProductMissing, "Product for PART01 does not exist.", nameof(ResultValidation.ProductNotFound), false)]
    [InlineData(StepRuleMissing, "Rule for Machine 77 does not exist.", nameof(ResultValidation.RuleNotFound), true)]
    [InlineData(StepRuleMissing, "Rule for Machine 77 does not exist.", nameof(ResultValidation.RuleNotFound), false)]
    [InlineData(StepMasterLabels, "Failed to retrieve master labels.", nameof(ResultValidation.ReferencesNotFound), true)]
    [InlineData(StepMasterLabels, "Failed to retrieve master labels.", nameof(ResultValidation.ReferencesNotFound), false)]
    [InlineData(StepReferencesEmpty, "References for PART01 not found.", nameof(ResultValidation.ReferencesNotFound), true)]
    [InlineData(StepReferencesEmpty, "References for PART01 not found.", nameof(ResultValidation.ReferencesNotFound), false)]
    [InlineData(StepUnmappedShift, "Shift lookup exploded", nameof(ResultValidation.Invalid), true)]
    [InlineData(StepUnmappedShift, "Shift lookup exploded", nameof(ResultValidation.Invalid), false)]
    public async Task CreateBarCode_FailureStep_ClassifiesCode_AuditAlwaysSpecific_ValueGatedByFlag(
        string step, string expectedError, string expectedCodeName, bool specificDiagnostics)
    {
        // Arrange
        ArrangeFailure(step);
        var expectedCode = EnumModel.FromName<ResultValidation>(expectedCodeName);
        var command = Command(string.Equals(step, StepInvalidMachineId, StringComparison.Ordinal) ? 0 : MachineId);
        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(options: options).ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — wire failure: value-carrying diagnostic under ON, value-less under OFF.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(expectedError);
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(expectedCode);
            dto.MachineId.ShouldBe(command.Command.MachineId);
            dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        // Audit: the SPECIFIC code lands regardless of the flag (the asymmetry the refactor must preserve).
        await _requestRepository.Received(1).AddAsync(
            Arg.Is<TaskGatewayRequest>(r =>
                r.ResultValidation == expectedCode
                && r.GatewayTask == GatewayTask.CreateBarCodeAsync
                && r.MachineId == command.Command.MachineId
                && r.TimeStamp == FixedNow
                && (r.Comment ?? string.Empty).Contains(expectedError)),
            Arg.Any<CancellationToken>());

        // Log: exactly the failure LogError naming the classified code.
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("CreateBarCode failed for machine").ShouldBeTrue();
        _logger.HasMessage($"({expectedCodeName})").ShouldBeTrue();
    }

    /// <summary>
    /// #114 chunk A INVERTS the old partial-persistence pin: the late "References for … not found." refusal
    /// now runs BEFORE any persistence, so a failed create leaves ZERO barcode/cycle writes (only the
    /// failure-audit row). The wire failure itself is byte-identical to the pre-#114 shape.
    /// </summary>
    [Fact]
    public async Task CreateBarCode_ReferencesEmpty_FailsWithZeroBarcodeAndCyclePersistence()
    {
        // Arrange
        ArrangeFailure(StepReferencesEmpty);

        // Act
        var result = await BuildHandler().ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert — failure, and the ONE persistence seam (the atomic aggregate save) never fired.
        result.IsFailure.ShouldBeTrue();
        await _barCodeAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // Audit-drop branches — a discarded audit write never flips the wire outcome.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A FAILED failure-audit write leaves the handler outcome unchanged (still the same value-carrying
    /// diagnostic failure) and logs the dedicated "failure-audit write did not land" LogError.
    /// </summary>
    [Fact]
    public async Task CreateBarCode_FailureAuditDropped_OutcomeUnchanged_LogsError()
    {
        // Arrange
        ArrangeFailure(StepProductMissing);
        _requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.WithFailure("audit db down")));

        // Act
        var result = await BuildHandler().ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product for PART01 does not exist.");
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.ProductNotFound);
        _logger.HasMessage("CreateBarCode failure-audit write did not land for machine").ShouldBeTrue();
    }

    /// <summary>
    /// The success path writes ONE success-audit row carrying <c>ResultValidation.Valid</c>,
    /// <c>GatewayTask.CreateBarCodeAsync</c>, the back-filled BarCodeId/CycleId and the part number.
    /// </summary>
    [Fact]
    public async Task CreateBarCode_Success_WritesValidAuditWithBackFilledIds()
    {
        // Act
        var result = await BuildHandler().ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.BarCodeId.ShouldBe(101);
        result.Value.CycleId.ShouldBe(201);
        await _requestRepository.Received(1).AddAsync(
            Arg.Is<TaskGatewayRequest>(r =>
                r.ResultValidation == ResultValidation.Valid
                && r.GatewayTask == GatewayTask.CreateBarCodeAsync
                && r.MachineId == MachineId
                && r.BarCodeId == 101
                && r.CycleId == 201
                && r.PartNumber == PartNumber
                && r.TimeStamp == FixedNow),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A FAILED success-audit write leaves the success outcome unchanged (the PLC still sees the created
    /// barcode/cycle) and logs the dedicated "success-audit write did not land" LogError.
    /// </summary>
    [Fact]
    public async Task CreateBarCode_SuccessAuditDropped_StillSuccess_LogsError()
    {
        // Arrange
        _requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.WithFailure("audit db down")));

        // Act
        var result = await BuildHandler().ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.Valid);
        _logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        _logger.HasMessage("CreateBarCode success-audit write did not land for machine").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // RouteCreateBarCode gate — machine fired once (ON), skipped (OFF), reject falls back.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// With <c>RouteCreateBarCode</c> OFF the state machine is NEVER fired and the legacy literals
    /// (Created / Started / Ok) are persisted and projected.
    /// </summary>
    [Fact]
    public async Task CreateBarCode_RouteCreateBarCodeOff_MachineNotFired_LegacyLiterals()
    {
        // Arrange
        var machine = Substitute.For<IItemStateMachine>();
        var options = Options.Create(new StateMachineRoutingOptions { RouteCreateBarCode = false });

        // Act
        var result = await BuildHandler(machine, options).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        machine.DidNotReceive().Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>());
        result.Value.ShouldNotBeNull();
        result.Value.FlowStatus.ShouldBe(FlowStatus.Created);
        result.Value.CycleStatus.ShouldBe(CycleStatus.Started);
        result.Value.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// With routing ON, the machine is fired EXACTLY ONCE with <c>CreateBarCodeAsync</c> on a throwaway
    /// None-state probe (label "CreateBarCodeProbe" — never the persisted barcode) with the fail-closed
    /// lookup flags set true, and the SUCCESS outcome drives BOTH the persisted barcode
    /// (NextFlowStatus/NextPartStatus) and the persisted cycle (NextCycleStatus/NextPartStatus).
    /// </summary>
    [Fact]
    public async Task CreateBarCode_RoutingOn_ProbeFiredOnce_OutcomeDrivesPersistedStatuses()
    {
        // Arrange — a machine outcome distinct from the legacy literals in every slot.
        var machine = Substitute.For<IItemStateMachine>();
        BarCode? probe = null;
        TransitionContext? firedContext = null;
        machine.Fire(
                Arg.Do<BarCode>(b => probe = b),
                Arg.Any<GatewayTask>(),
                Arg.Do<TransitionContext>(c => firedContext = c))
            .Returns(Result<TransitionOutcome>.Success(
                new TransitionOutcome(FlowStatus.InProcess, CycleStatus.FinishedOk, PartStatus.NOk, ResultValidation.Valid)));

        BarCode? persisted = null;
        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                persisted = ci.Arg<BarCode>();
                ci.Arg<BarCode>().BarCodeId = new BarCodeId(101);
                return Task.FromResult(Result.Success());
            });

        // Act
        var result = await BuildHandler(machine).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert — one fire, on the probe, with the fail-closed flags explicitly true.
        result.IsSuccess.ShouldBeTrue();
        machine.Received(1).Fire(Arg.Any<BarCode>(), GatewayTask.CreateBarCodeAsync, Arg.Any<TransitionContext>());
        probe.ShouldNotBeNull().Label.Value.ShouldBe("CreateBarCodeProbe");
        var context = firedContext.ShouldNotBeNull();
        context.MachineFound.ShouldBeTrue();
        context.ProductFound.ShouldBeTrue();
        context.RuleFound.ShouldBeTrue();
        context.CycleTime.ShouldBe(0);
        context.Recipe.ShouldBeNull();

        // The persisted barcode is NOT the probe and carries the machine outcome.
        var added = persisted.ShouldNotBeNull();
        ReferenceEquals(added, probe).ShouldBeFalse();
        added.FlowStatus.ShouldBe(FlowStatus.InProcess);
        added.PartStatus.ShouldBe(PartStatus.NOk);

        // #114: the Started cycle rides the SAME atomic save, staged on the root.
        var stagedCycle = added.PendingNewCycles.ShouldHaveSingleItem();
        stagedCycle.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        stagedCycle.PartStatus.ShouldBe(PartStatus.NOk);
        result.Value.ShouldNotBeNull();
        result.Value.FlowStatus.ShouldBe(FlowStatus.InProcess);
        result.Value.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        result.Value.PartStatus.ShouldBe(PartStatus.NOk);
    }

    /// <summary>
    /// A state-machine REJECT is swallowed: the legacy literals (Created / Started / Ok) are preserved, the
    /// create still SUCCEEDS, and a single LogWarning records the rejection.
    /// </summary>
    [Fact]
    public async Task CreateBarCode_MachineRejects_LegacyLiteralsPreserved_StillSuccess_LogsWarning()
    {
        // Arrange
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure("Illegal transition"));

        // Act
        var result = await BuildHandler(machine).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.FlowStatus.ShouldBe(FlowStatus.Created);
        result.Value.CycleStatus.ShouldBe(CycleStatus.Started);
        result.Value.PartStatus.ShouldBe(PartStatus.Ok);
        _logger.HasLogLevel(LogLevel.Warning).ShouldBeTrue();
        _logger.HasMessage("state machine rejected CreateBarCodeAsync").ShouldBeTrue();
    }

    // ----------------------------------------------------------------------------------
    // IResettable.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// <see cref="CreateBarCodeCommandHandler.TryReset"/> (the <c>IResettable</c> pooling seam) always
    /// returns <see langword="true"/> — the handler holds no per-request state to clear.
    /// </summary>
    [Fact]
    public void CreateBarCode_TryReset_ReturnsTrue()
    {
        BuildHandler().TryReset().ShouldBeTrue();
    }
}
