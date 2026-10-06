// <copyright file="SpecificDiagnosticsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Validation;
using IndTrace.Application.Gateway.Auditing;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Story 3.3 — proves each routed PLC handler, on a guard/lookup failure, propagates the SPECIFIC negative
/// <see cref="ResultValidation"/> from the §Dev-Notes table onto a value-CARRYING failure
/// (<c>WithFailure(errors, dto)</c>, NOT the value-less <c>WithFailure(errors)</c>), so the precise code can
/// later survive <c>ControllerExtensions.PublishResultToPlc</c>. Each test contrasts the flag-OFF behavior
/// (value-less failure -> the transport's legacy generic <c>-1</c> collapse) with the flag-ON behavior
/// (value carries the specific code). Reach-the-tag verification lives in the Gateway test project.
/// </summary>
public class SpecificDiagnosticsTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;

    private static IOptions<StateMachineRoutingOptions> DiagnosticsOn() =>
        Options.Create(new StateMachineRoutingOptions());

    private static IOptions<StateMachineRoutingOptions> DiagnosticsOff() =>
        Options.Create(new StateMachineRoutingOptions { SpecificDiagnostics = false });

    // =================================================================================================
    // CreateBarCode — rows 1-4 (Machine / Product / Rule / References)
    // =================================================================================================

    [Fact]
    public async Task CreateBarCode_WhenFlagOn_MachineMissing_PropagatesMachineNotFound() =>
        await AssertCreateBarCodeSpecificCode("machine-missing", ResultValidation.MachineNotFound);

    [Fact]
    public async Task CreateBarCode_WhenFlagOn_ProductMissing_PropagatesProductNotFound() =>
        await AssertCreateBarCodeSpecificCode("product-missing", ResultValidation.ProductNotFound);

    [Fact]
    public async Task CreateBarCode_WhenFlagOn_RuleMissing_PropagatesRuleNotFound() =>
        await AssertCreateBarCodeSpecificCode("rule-missing", ResultValidation.RuleNotFound);

    [Fact]
    public async Task CreateBarCode_WhenFlagOn_ReferencesMissing_PropagatesReferencesNotFound() =>
        await AssertCreateBarCodeSpecificCode("references-missing", ResultValidation.ReferencesNotFound);

    private async Task AssertCreateBarCodeSpecificCode(string scenario, ResultValidation expected)
    {
        var handler = BuildCreateBarCodeHandler(DiagnosticsOn(), scenario);

        var result = await handler.ProcessAsync(CreateBarCodeCommandFor(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldNotBeNull(); // Failure-with-value, NOT WithFailure(errors)
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(expected);
        result.Value.ResultValidation.Value.ShouldBeLessThan(0);
    }

    [Fact]
    public async Task CreateBarCode_WhenFlagOff_FailureCollapsesToValueless()
    {
        var handler = BuildCreateBarCodeHandler(DiagnosticsOff(), "machine-missing");

        var result = await handler.ProcessAsync(CreateBarCodeCommandFor(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull(); // value-less -> transport re-applies generic -1
    }

    // =================================================================================================
    // CreateCycle — row 9 (Station/machine cannot start cycle -> InvalidMachine)
    // =================================================================================================

    [Fact]
    public async Task CreateCycle_WhenFlagOn_StationCannotStart_PropagatesInvalidMachine()
    {
        var handler = BuildCreateCyclesHandler(DiagnosticsOn(), stationCanStart: false);

        var result = await handler.ProcessAsync(CreateCyclesCommandFor(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.InvalidMachine);
    }

    [Fact]
    public async Task CreateCycle_WhenFlagOff_StationCannotStart_CollapsesToValueless()
    {
        var handler = BuildCreateCyclesHandler(DiagnosticsOff(), stationCanStart: false);

        var result = await handler.ProcessAsync(CreateCyclesCommandFor(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    // Story 6.3 — the UpdateCycleOk / UpdateCycleNotOk SpecificDiagnostics rows were deleted: their subject (the
    // legacy LegacyUpdateCyclesOk/NotOkCommandHandler) was retired. The SpecificDiagnostics flag itself stays LIVE
    // (read by CreateBarCode/CreateCycle/EndOfProcess below). Specific-code parity for the unified cycle-update
    // path (BarCodeNotFound, DestinationNotValid, PartNotValid, RecipeNotFound) is pinned by
    // UpdateCyclesProjectionRoundTripTests.

    // =================================================================================================
    // EndOfProcess — row 8 (BarCode lookup fails -> BarCodeNotFound)
    // =================================================================================================

    [Fact]
    public async Task EndOfProcess_WhenFlagOn_BarCodeInvalid_PropagatesBarCodeNotFound()
    {
        var handler = BuildEndOfProcessHandler(DiagnosticsOn(), barCodeValid: false);

        var result = await handler.ProcessAsync(EndOfProcessCommandFor(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.BarCodeNotFound);
    }

    [Fact]
    public async Task EndOfProcess_WhenFlagOff_BarCodeInvalid_CollapsesToValueless()
    {
        var handler = BuildEndOfProcessHandler(DiagnosticsOff(), barCodeValid: false);

        var result = await handler.ProcessAsync(EndOfProcessCommandFor(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    // =================================================================================================
    // Builders
    // =================================================================================================

    private const string RuleJsonFixture =
        @"{""ruleId"": ""R001"",""ruleFunction"": [""lineIdentifier"", ""fixedPart"", ""partNumber""]," +
        @"""components"": {""lineIdentifier"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""WS""}," +
        @"""fixedPart"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""100""}," +
        @"""partNumber"": {""action"": ""string"",""origin"": ""program"",""lengthMin"": 6,""lengthMax"": 9}}}";

    private static CreateBarCodeCommand CreateBarCodeCommandFor() =>
        new() { Command = new TaskGatewayRequest { MachineId = 1, PartNumber = "PART01", BarCode = string.Empty } };

    private static CreateBarCodeCommandHandler BuildCreateBarCodeHandler(IOptions<StateMachineRoutingOptions> routing, string scenario)
    {
        const int machineId = 1;
        const int productId = 7;
        const string partNumber = "PART01";

        var ruleRepository = Substitute.For<IReadOnlyRepository<Rule>>();
        var machineRepository = Substitute.For<IReadOnlyRepository<IndTrace.Domain.Entities.Machine>>();
        var barCodeAggregateRepository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();
        var productRepository = Substitute.For<IReadOnlyRepository<Product>>();
        var variableRepository = Substitute.For<IReadOnlyRepository<Variable>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var shiftService = Substitute.For<IShiftService>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var masterLabelService = Substitute.For<IMasterLabelService>();
        var barCodeService = Substitute.For<IBarCodeService>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local));

        // Machine present unless the scenario removes it.
        var machineEntity = new IndTrace.Domain.Entities.Machine { MachineId = new MachineId(machineId), Name = "Printer-1", MachineType = MachineType.Printer, WorkFlowType = WorkFlowType.Initial };
        machineRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(scenario == "machine-missing" ? Result<IndTrace.Domain.Entities.Machine?>.Success(null) : Result<IndTrace.Domain.Entities.Machine?>.Success(machineEntity)));

        var product = Product.CreateFixture(productId: productId, partNumber: partNumber);
        productRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(scenario == "product-missing" ? Result<Product?>.Success(null) : Result<Product?>.Success(product)));

        var rule = new Rule { RuleId = 1, MachineId = new MachineId(machineId), ProductId = new ProductId(productId), IsActive = true, Version = 1, RuleJson = RuleJsonFixture };
        ruleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Rule>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(scenario == "rule-missing" ? Result<Rule?>.Success(null) : Result<Rule?>.Success(rule)));

        masterLabelService.GetMasterLabelByPartNumberAsync(partNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<List<string>>.Success(new List<string>())));
        barCodeService.GetConsecutiveByBarCodeLabelAsync(partNumber, Arg.Any<List<string>>(), Arg.Any<Rule?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(machineId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { CyclesOk = 0 })));

        // References present unless the scenario removes them.
        var refVariable = new Variable { VariableId = 1, MachineId = machineId, Name = "RefTag1", IsActive = 1, VariableGroupId = TagsGroups.ReferenceTags.Value };
        variableRepository.ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(scenario == "references-missing"
                ? Result<IEnumerable<Variable>>.Success(new List<Variable>())
                : Result<IEnumerable<Variable>>.Success(new List<Variable> { refVariable })));

        requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(0)));

        return new CreateBarCodeCommandHandler(
            ruleRepository, barCodeAggregateRepository, machineRepository, productRepository,
            variableRepository, requestRepository, shiftService, dateTimeMachine, masterLabelService, barCodeService,
            XUnitLogger.CreateLogger<CreateBarCodeCommandHandler>(), new ItemStateMachine(), routing);
    }

    private static CreateCyclesCommand CreateCyclesCommandFor() =>
        new() { Command = new TaskGatewayRequest { BarCode = "BC", MachineId = 2, PartNumber = "PART", CycleStatus = CycleStatus.Started, PartStatus = PartStatus.Ok } };

    private static CreateCyclesCommandHandler BuildCreateCyclesHandler(IOptions<StateMachineRoutingOptions> routing, bool stationCanStart)
    {
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var barCodeDetailsLoader = Substitute.For<IBarCodeDetailsLoader>();
        var stationValidator = Substitute.For<IStationValidator>();
        var cycleLimitPolicy = Substitute.For<ICycleLimitPolicy>();
        var cycleCreator = Substitute.For<ICycleCreator>();
        var gatewayAuditFactory = Substitute.For<IGatewayAuditFactory>();

        // #33 Chunk 4: load an immutable BarCodeSnapshot via IBarCodeDetailsLoader.
        var snapshot = new BarCodeSnapshot
        {
            MachineId = 1,
            NextMachineId = 2,
            BarCodeId = 10,
            FlowStatus = FlowStatus.Created,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Initial,
            PartNumber = "PART",
            ResultValidation = ResultValidation.Valid,
            Cycles = new List<Cycle>(),
            Recipe = Recipe.Create(0, 0, 0, 216000, 10, 3, 1).Value.ShouldNotBeNull(),
            BarCode = new BarCodeBuilder().Created(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(10); b.MachineId = new MachineId(1); }).Build(),
            References = new Dictionary<string, Register>(),
        };

        barCodeDetailsLoader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));

        stationValidator.ValidateCanStartCycles(Arg.Any<IBarCodeResult>())
            .Returns(stationCanStart ? Result.Success() : Result.WithFailure("Only process stations can start cycles."));
        cycleLimitPolicy.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.Success(new CycleLimitDecision(true, "Allowed", ResultValidation.Valid)));
        cycleCreator.CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle>.Success(new CycleBuilder().Started(PartStatus.Ok).With(c => c.CycleId = new CycleId(99)).Build())));
        gatewayAuditFactory.CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TaskGatewayRequest>.Success(new TaskGatewayRequest())));

        return new CreateCyclesCommandHandler(
            XUnitLogger.CreateLogger<CreateCyclesCommandHandler>(), dateTimeMachine, barCodeDetailsLoader,
            stationValidator, cycleLimitPolicy, cycleCreator, gatewayAuditFactory,
            new ItemStateMachine(), routing);
    }

    private static UpdateBarCodeCommand EndOfProcessCommandFor() =>
        new() { Command = new TaskGatewayRequest { MachineId = MachineId, BarCode = "BC-EOP", PartNumber = "PART" } };

    private static UpdateBarCodeCommandHandler BuildEndOfProcessHandler(IOptions<StateMachineRoutingOptions> routing, bool barCodeValid)
    {
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();

        // #114 chunk C: cycle INSERT + barcode status UPDATE ride the ONE aggregate save.
        var barCodeAggregateRepository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();

        // #33 Chunk 5: the handler now loads via the stateless IBarCodeDetailsLoader returning an immutable snapshot.
        var barCodeDetailsLoader = Substitute.For<IBarCodeDetailsLoader>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Local));

        var snapshot = new BarCodeSnapshot
        {
            MachineId = MachineId,
            BarCodeId = BarCodeId,
            FlowStatus = FlowStatus.InProcess,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Final,
            ResultValidation = barCodeValid ? ResultValidation.Valid : ResultValidation.BarCodeNotFound,
            Error = barCodeValid ? string.Empty : "BarCode not found.",
            BarCode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build(),
            References = new Dictionary<string, Register>(),
        };

        barCodeDetailsLoader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));
        barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        return new UpdateBarCodeCommandHandler(dateTimeMachine, barCodeAggregateRepository, barCodeDetailsLoader, new ItemStateMachine(), routing);
    }
}
