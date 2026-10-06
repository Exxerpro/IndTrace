// <copyright file="CreateHandlersGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Validation;
using IndTrace.Application.Gateway.Auditing;
using IndTrace.Application.Services;
using IndTrace.Application.Shifts.Commands.Create;
using IndTrace.Domain.Models;

namespace Application.UnitTests.Characterization.Plc;

/// <summary>
/// Golden-master (characterization) tests for the PLC-path CREATE handlers:
/// <see cref="CreateBarCodeCommandHandler"/> (trigger <c>CreateBarCodeAsync</c>, value 4)
/// and <see cref="CreateCyclesCommandHandler"/> (trigger <c>CreateCycleAsync</c>, value 16).
///
/// Story 1.1, AC 3 and 4 (matrix rows 1 and 2). Pins the AS-BUILT
/// <c>(FlowStatus, CycleStatus, PartStatus, ResultValidation)</c> tuples on the success path.
/// </summary>
public class CreateHandlersGoldenMasterTests
{
    // ----------------------------------------------------------------------------------
    // AC 3 / matrix row 1 — CreateBarCode from FlowStatus.None on a Printer station:
    // new barcode Created, new cycle Started, PartStatus.Ok, ResultValidation.Valid.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A frozen, valid rule JSON fixture (mirrors RuleRawData "R001") so the inline label
    /// generator in the handler produces a label without reaching out to the database.
    /// </summary>
    private const string RuleJsonFixture =
        @"{""ruleId"": ""R001"",""ruleFunction"": [""lineIdentifier"", ""fixedPart"", ""partNumber""]," +
        @"""components"": {""lineIdentifier"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""WS""}," +
        @"""fixedPart"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""100""}," +
        @"""partNumber"": {""action"": ""string"",""origin"": ""program"",""lengthMin"": 6,""lengthMax"": 9}}}";

    [Fact]
    public async Task CreateBarCode_FromNone_OnPrinter_PersistsCreatedStartedOk_Valid()
    {
        // Arrange
        const int machineId = 1;
        const int productId = 7;
        const string partNumber = "PART01";

        var ruleRepository = Substitute.For<IReadOnlyRepository<Rule>>();
        var machineRepository = Substitute.For<IReadOnlyRepository<Machine>>();
        var barCodeAggregateRepository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();
        var productRepository = Substitute.For<IReadOnlyRepository<Product>>();
        var variableRepository = Substitute.For<IReadOnlyRepository<Variable>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var shiftService = Substitute.For<IShiftService>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var masterLabelService = Substitute.For<IMasterLabelService>();
        var barCodeService = Substitute.For<IBarCodeService>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local));

        var machine = new Machine { MachineId = new MachineId(machineId), Name = "Printer-1", MachineType = MachineType.Printer, WorkFlowType = WorkFlowType.Initial };
        machineRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Machine?>.Success(machine)));

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

        // One active reference variable so the references dictionary is non-empty.
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

        var handler = new CreateBarCodeCommandHandler(
            ruleRepository, barCodeAggregateRepository, machineRepository, productRepository,
            variableRepository, requestRepository, shiftService, dateTimeMachine, masterLabelService, barCodeService,
            XUnitLogger.CreateLogger<CreateBarCodeCommandHandler>());

        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber,
                BarCode = string.Empty,
            },
        };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — AC 3: the success-path tuple, read off the response DTO.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().FlowStatus.ShouldBe(FlowStatus.Created);
        result.Value.CycleStatus.ShouldBe(CycleStatus.Started);
        result.Value.PartStatus.ShouldBe(PartStatus.Ok);
        result.Value.ResultValidation.ShouldBe(ResultValidation.Valid);

        // The persisted barcode entity carries the same Created / Ok pin, with its Started cycle staged on
        // the SAME atomic aggregate save (#114).
        await barCodeAggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b =>
                b.FlowStatus == FlowStatus.Created &&
                b.PartStatus == PartStatus.Ok &&
                b.PendingNewCycles.Count == 1 &&
                b.PendingNewCycles[0].CycleStatus == CycleStatus.Started &&
                b.PendingNewCycles[0].PartStatus == PartStatus.Ok),
            Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // AC 4 / matrix row 2 — CreateCycle from FlowStatus.Created on a process station:
    // barcode advances to InProcess; cycle is Started; request CycleStatus/PartStatus flow
    // through; success. Mirrors the existing CreateCyclesCommandHandlerTests style.
    // ----------------------------------------------------------------------------------

    [Fact]
    public async Task CreateCycle_FromCreated_OnProcessStation_AdvancesInProcess_CycleStarted_Success()
    {
        // Arrange
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var barCodeDetailsLoader = Substitute.For<IBarCodeDetailsLoader>();
        var stationValidator = Substitute.For<IStationValidator>();
        var cycleLimitPolicy = Substitute.For<ICycleLimitPolicy>();
        var cycleCreator = Substitute.For<ICycleCreator>();
        var gatewayAuditFactory = Substitute.For<IGatewayAuditFactory>();

        // #33 Chunk 4: the handler loads an immutable BarCodeSnapshot via IBarCodeDetailsLoader. The snapshot
        // carries the same getters the god-object used to; BarCode is in the Created source state so the
        // CreateCycleAsync transition resolves to InProcess (routing default-ON), byte-equal to the as-built tuple.
        var snapshot = new BarCodeSnapshot
        {
            MachineId = 1,
            NextMachineId = 2,
            BarCodeId = 10,
            CycleId = 0,
            CycleStatus = CycleStatus.NotStarted,
            FlowStatus = FlowStatus.Created, // source state
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

        // Capture the cycle-create request to verify the Started status flows through.
        cycleCreator.CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle>.Success(new CycleBuilder().Started(PartStatus.Ok).With(c => c.CycleId = new CycleId(99)).Build())));

        gatewayAuditFactory.CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TaskGatewayRequest>.Success(new TaskGatewayRequest())));

        var handler = new CreateCyclesCommandHandler(
            XUnitLogger.CreateLogger<CreateCyclesCommandHandler>(),
            dateTimeMachine,
            barCodeDetailsLoader,
            stationValidator,
            cycleLimitPolicy,
            cycleCreator,
            gatewayAuditFactory);

        var command = new CreateCyclesCommand
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

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — AC 4: success, a cycle was created with CycleStatus.Started, and the barcode advance
        // toward InProcess rides the SAME cycle-create request (#114 chunk B: one transactional aggregate
        // save inside the creator replaced the separate barcode-update write).
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        await cycleCreator.Received(1).CreateAsync(
            Arg.Is<CycleCreateRequest>(r =>
                r.CycleStatus == CycleStatus.Started && r.FlowStatus == FlowStatus.InProcess),
            Arg.Any<CancellationToken>());
    }
}
