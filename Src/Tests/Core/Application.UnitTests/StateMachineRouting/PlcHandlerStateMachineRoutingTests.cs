// <copyright file="PlcHandlerStateMachineRoutingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Validation;
using IndTrace.Application.Gateway.Auditing;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Story 3.1 — proves the four PLC create/cycle handlers DELEGATE their FlowStatus/CycleStatus/PartStatus
/// decision to an injected <see cref="IItemStateMachine"/> with the correct <see cref="GatewayTask"/> trigger
/// (assertion (b), via an NSubstitute spy), and that flipping the per-handler feature flag OFF reverts to the
/// legacy inline path (AC6). Persisted-state parity (assertion (a)) is pinned by the existing
/// <c>CreateHandlersGoldenMasterTests</c>/<c>UpdateCyclesGoldenMasterTests</c>, which run with the routing
/// flag default-ON and stay byte-equal.
/// </summary>
public class PlcHandlerStateMachineRoutingTests
{
    private static IOptions<StateMachineRoutingOptions> RoutingOn() =>
        Options.Create(new StateMachineRoutingOptions());

    private static IOptions<StateMachineRoutingOptions> RoutingOff() =>
        Options.Create(new StateMachineRoutingOptions
        {
            RouteCreateBarCode = false,
            RouteCreateCycle = false,
        });

    /// <summary>
    /// A spy machine that records every Fire call and delegates to the real engine so outcomes stay correct.
    /// </summary>
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

    // -------------------------------------------------------------------------------------------------
    // CreateBarCode — trigger CreateBarCodeAsync (4)
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public async Task CreateBarCode_WhenFlagOn_DelegatesToMachine_WithCreateBarCodeTrigger()
    {
        var spy = SpyMachine(out var fired);
        var handler = BuildCreateBarCodeHandler(spy, RoutingOn(), out var barCodeAggregateRepo);

        var result = await handler.ProcessAsync(CreateBarCodeCommandFor(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldContain(GatewayTask.CreateBarCodeAsync);

        // Assertion (a): the atomically saved aggregate carries the machine-resolved Created/Started/Ok
        // (golden-master tuple) on the root and its staged Started cycle (#114 single-save seam).
        await barCodeAggregateRepo.Received(1).SaveAsync(
            Arg.Is<BarCode>(b =>
                b.FlowStatus == FlowStatus.Created &&
                b.PartStatus == PartStatus.Ok &&
                b.PendingNewCycles.Count == 1 &&
                b.PendingNewCycles[0].CycleStatus == CycleStatus.Started &&
                b.PendingNewCycles[0].PartStatus == PartStatus.Ok),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBarCode_WhenFlagOff_DoesNotCallMachine_LegacyPath()
    {
        var spy = SpyMachine(out var fired);
        var handler = BuildCreateBarCodeHandler(spy, RoutingOff(), out var barCodeAggregateRepo);

        var result = await handler.ProcessAsync(CreateBarCodeCommandFor(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldBeEmpty();
        await barCodeAggregateRepo.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => b.FlowStatus == FlowStatus.Created && b.PartStatus == PartStatus.Ok),
            Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------------------------------
    // CreateCycle — trigger CreateCycleAsync (16)
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public async Task CreateCycle_WhenFlagOn_DelegatesToMachine_WithCreateCycleTrigger()
    {
        var spy = SpyMachine(out var fired);
        var handler = BuildCreateCyclesHandler(spy, RoutingOn(), out var cycleCreator);

        var result = await handler.ProcessAsync(CreateCyclesCommandFor(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldContain(GatewayTask.CreateCycleAsync);

        // #114 chunk B: the resolved flow status reaches persistence on the cycle-create request (the barcode
        // write rides the creator's single transactional aggregate save).
        await cycleCreator.Received(1).CreateAsync(
            Arg.Is<CycleCreateRequest>(r => r.FlowStatus == FlowStatus.InProcess),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCycle_WhenFlagOff_DoesNotCallMachine_LegacyInProcess()
    {
        var spy = SpyMachine(out var fired);
        var handler = BuildCreateCyclesHandler(spy, RoutingOff(), out var cycleCreator);

        var result = await handler.ProcessAsync(CreateCyclesCommandFor(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldBeEmpty();

        // #114 chunk B: the legacy InProcess reaches persistence on the cycle-create request (the barcode
        // write rides the creator's single transactional aggregate save).
        await cycleCreator.Received(1).CreateAsync(
            Arg.Is<CycleCreateRequest>(r => r.FlowStatus == FlowStatus.InProcess),
            Arg.Any<CancellationToken>());
    }

    // Story 6.3 — the UpdateCycleOk / UpdateCycleNotOk flag-routing tests were deleted: their subject (the legacy
    // LegacyUpdateCyclesOk/NotOkCommandHandler) was retired now that production runs only the unified
    // UpdateCyclesCommandHandler. As-built §7 parity for the unified cycle-update path is pinned by
    // UpdateCyclesGoldenMasterTests / CycleTimeOverrideAnomalyTests / UpdateCyclesProjectionRoundTripTests.

    // =================================================================================================
    // Builders
    // =================================================================================================

    private const string RuleJsonFixture =
        @"{""ruleId"": ""R001"",""ruleFunction"": [""lineIdentifier"", ""fixedPart"", ""partNumber""]," +
        @"""components"": {""lineIdentifier"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""WS""}," +
        @"""fixedPart"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""100""}," +
        @"""partNumber"": {""action"": ""string"",""origin"": ""program"",""lengthMin"": 6,""lengthMax"": 9}}}";

    private static CreateBarCodeCommand CreateBarCodeCommandFor() =>
        new()
        {
            Command = new TaskGatewayRequest { MachineId = 1, PartNumber = "PART01", BarCode = string.Empty },
        };

    private static CreateBarCodeCommandHandler BuildCreateBarCodeHandler(
        IItemStateMachine machine,
        IOptions<StateMachineRoutingOptions> routing,
        out IAggregateRepository<BarCode> barCodeAggregateRepository)
    {
        const int machineId = 1;
        const int productId = 7;
        const string partNumber = "PART01";

        var ruleRepository = Substitute.For<IReadOnlyRepository<Rule>>();
        var machineRepository = Substitute.For<IReadOnlyRepository<IndTrace.Domain.Entities.Machine>>();
        barCodeAggregateRepository = Substitute.For<IAggregateRepository<BarCode>>();
        var productRepository = Substitute.For<IReadOnlyRepository<Product>>();
        var variableRepository = Substitute.For<IReadOnlyRepository<Variable>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var shiftService = Substitute.For<IShiftService>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var masterLabelService = Substitute.For<IMasterLabelService>();
        var barCodeService = Substitute.For<IBarCodeService>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local));

        var machineEntity = new IndTrace.Domain.Entities.Machine { MachineId = new MachineId(machineId), Name = "Printer-1", MachineType = MachineType.Printer, WorkFlowType = WorkFlowType.Initial };
        machineRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IndTrace.Domain.Entities.Machine?>.Success(machineEntity)));

        var product = Product.CreateFixture(productId: productId, partNumber: partNumber);
        productRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product?>.Success(product)));

        var rule = new Rule { RuleId = 1, MachineId = new MachineId(machineId), ProductId = new ProductId(productId), IsActive = true, Version = 1, RuleJson = RuleJsonFixture };
        ruleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Rule>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Rule?>.Success(rule)));

        masterLabelService.GetMasterLabelByPartNumberAsync(partNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<List<string>>.Success(new List<string>())));
        barCodeService.GetConsecutiveByBarCodeLabelAsync(partNumber, Arg.Any<List<string>>(), Arg.Any<Rule?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(machineId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { CyclesOk = 0 })));

        var refVariable = new Variable
        {
            VariableId = 1,
            MachineId = machineId,
            Name = "RefTag1",
            IsActive = 1,
            VariableGroupId = TagsGroups.ReferenceTags.Value,
        };
        variableRepository.ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable> { refVariable })));

        requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(0)));

        return new CreateBarCodeCommandHandler(
            ruleRepository, barCodeAggregateRepository, machineRepository, productRepository,
            variableRepository, requestRepository, shiftService, dateTimeMachine, masterLabelService, barCodeService,
            XUnitLogger.CreateLogger<CreateBarCodeCommandHandler>(), machine, routing);
    }

    private static CreateCyclesCommand CreateCyclesCommandFor() =>
        new()
        {
            Command = new TaskGatewayRequest
            {
                BarCode = "BC-CREATE-CYCLE",
                MachineId = 2,
                PartNumber = "PART",
                CycleStatus = CycleStatus.Started,
                PartStatus = PartStatus.Ok,
                TimeStamp = DateTime.UtcNow,
            },
        };

    private static CreateCyclesCommandHandler BuildCreateCyclesHandler(
        IItemStateMachine machine,
        IOptions<StateMachineRoutingOptions> routing,
        out ICycleCreator cycleCreator)
    {
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var barCodeDetailsLoader = Substitute.For<IBarCodeDetailsLoader>();
        var stationValidator = Substitute.For<IStationValidator>();
        var cycleLimitPolicy = Substitute.For<ICycleLimitPolicy>();
        cycleCreator = Substitute.For<ICycleCreator>();
        var gatewayAuditFactory = Substitute.For<IGatewayAuditFactory>();

        // #33 Chunk 4: load an immutable BarCodeSnapshot via IBarCodeDetailsLoader. BarCode in the Created source
        // state so the CreateCycleAsync transition resolves (not rejected).
        var snapshot = new BarCodeSnapshot
        {
            MachineId = 1,
            NextMachineId = 2,
            BarCodeId = 10,
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
            BarCode = new BarCodeBuilder().Created(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(10); b.MachineId = new MachineId(1); }).Build(),
        };

        barCodeDetailsLoader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));

        stationValidator.ValidateCanStartCycles(Arg.Any<IBarCodeResult>()).Returns(Result.Success());
        cycleLimitPolicy.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.Success(new CycleLimitDecision(true, "Allowed", ResultValidation.Valid)));

        cycleCreator.CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle>.Success(new CycleBuilder().Started(PartStatus.Ok).With(c => c.CycleId = new CycleId(99)).Build())));

        gatewayAuditFactory.CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TaskGatewayRequest>.Success(new TaskGatewayRequest())));

        return new CreateCyclesCommandHandler(
            XUnitLogger.CreateLogger<CreateCyclesCommandHandler>(),
            dateTimeMachine,
            barCodeDetailsLoader,
            stationValidator,
            cycleLimitPolicy,
            cycleCreator,
            gatewayAuditFactory,
            machine,
            routing);
    }

}
