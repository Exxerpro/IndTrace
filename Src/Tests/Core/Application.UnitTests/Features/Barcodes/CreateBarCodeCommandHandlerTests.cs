// <copyright file="CreateBarCodeCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.BarCodes.Commands.Create;
using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for CreateBarCodeCommandHandler
/// </summary>
public class CreateBarCodeCommandHandlerTests
{
    private readonly IReadOnlyRepository<Rule> _ruleRepository;
    private readonly IAggregateRepository<BarCode> _barCodeAggregateRepository;
    private readonly IReadOnlyRepository<IndTrace.Domain.Entities.Machine> _machineRepository;
    private readonly IReadOnlyRepository<IndTrace.Domain.Entities.Product> _productRepository;
    private readonly IReadOnlyRepository<IndTrace.Domain.Entities.Variable> _variableRepository;
    private readonly IRepository<TaskGatewayRequest> _requestRepository;
    private readonly IShiftService _shiftService;
    private readonly IDateTimeMachine _dateTimeMachine;
    private readonly IMasterLabelService _masterLabelService;
    private readonly IBarCodeService _barCodeService;
    private readonly ILogger<CreateBarCodeCommandHandler> _logger;
    private readonly CreateBarCodeCommandHandler _handler;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateBarCodeCommandHandlerTests()
    {
        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: [ARCHITECTURAL REFACTOR] - Add IMasterLabelService mock and update constructor for new service pattern
        //[Fix]
        //CLAUDE
        //Date: 03/08/2026
        //Reason: [#114 chunk A] - the raw IRepository<BarCode>/IReadOnlyRepository<Cycle> AddAsync pair is retired;
        //        the handler persists the barcode + Started cycle atomically via IAggregateRepository<BarCode>.
        _ruleRepository = Substitute.For<IReadOnlyRepository<Rule>>();
        _barCodeAggregateRepository = Substitute.For<IAggregateRepository<BarCode>>();
        _machineRepository = Substitute.For<IReadOnlyRepository<IndTrace.Domain.Entities.Machine>>();
        _productRepository = Substitute.For<IReadOnlyRepository<IndTrace.Domain.Entities.Product>>();
        _variableRepository = Substitute.For<IReadOnlyRepository<IndTrace.Domain.Entities.Variable>>();
        _requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        _shiftService = Substitute.For<IShiftService>();
        _dateTimeMachine = Substitute.For<IDateTimeMachine>();
        _masterLabelService = Substitute.For<IMasterLabelService>();
        _barCodeService = Substitute.For<IBarCodeService>();
        _logger = XUnitLogger.CreateLogger<CreateBarCodeCommandHandler>();

        _handler = new CreateBarCodeCommandHandler(
            _ruleRepository,
            _barCodeAggregateRepository,
            _machineRepository,
            _productRepository,
            _variableRepository,
            _requestRepository,
            _shiftService,
            _dateTimeMachine,
            _masterLabelService,
            _barCodeService,
            _logger);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new CreateBarCodeCommandHandler(
            _ruleRepository,
            _barCodeAggregateRepository,
            _machineRepository,
            _productRepository,
            _variableRepository,
            _requestRepository,
            _shiftService,
            _dateTimeMachine,
            _masterLabelService,
            _barCodeService,
            _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullParameters_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullParameters_ShouldThrowException()
    //     {
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new CreateBarCodeCommandHandler(
    //             null!,
    //             _cycleRepository,
    //             _machineRepository,
    //             _masterLabelRepository,
    //             _barCodeRepository,
    //             _productRepository,
    //             _variableRepository,
    //             _requestRepository,
    //             _cache,
    //             _shiftService,
    //             _dateTimeMachine));
    //     }
    /// <summary>
    /// Executes Process_WithInvalidMachineId_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WithInvalidMachineId_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WithInvalidMachineId_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 0,
                PartNumber = "TEST123"
            }
        };

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
        //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 FIX 4] - a malformed machine id ("Machine 0 number
        //invalid") is the Create machine-validity failure -> MachineNotFound(-8), NOT the unrelated
        //InvalidMachine(-4096) (MachineFinalGuard's code). The persisted audit row now carries MachineNotFound.
        await _requestRepository.Received(1).AddAsync(Arg.Is<TaskGatewayRequest>(r =>
            r.ResultValidation == ResultValidation.MachineNotFound), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithValidRequest_ShouldCreateBarCodeSuccessfully operation.
    /// </summary>
    /// <returns>The result of Process_WithValidRequest_ShouldCreateBarCodeSuccessfully.</returns>

    [Fact]
    public async Task Process_WithValidRequest_ShouldCreateBarCodeSuccessfully()
    {
        // Arrange
        var machineId = 1;
        var partNumber = "TEST123";
        var currentTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local);

        Console.WriteLine($"DEBUG: Starting test with MachineId={machineId}, PartNumber={partNumber}");

        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber
            }
        };

        var machine = new IndTrace.Domain.Entities.Machine { MachineId = new MachineId(machineId), Name = "TestMachine", MachineType = MachineType.Process };
        var product = IndTrace.Domain.Entities.Product.CreateFixture(productId: 1, partNumber: partNumber);
        var rule = new Rule { RuleId = 1, RuleJson = "{\"ruleFunction\":[\"lineIdentifier\",\"partNumber\",\"autoIncrement\"],\"components\":{\"lineIdentifier\":{\"action\":\"string\",\"origin\":\"fixed\",\"value\":\"L\"},\"partNumber\":{\"action\":\"string\",\"origin\":\"program\",\"lengthMin\":6,\"lengthMax\":9},\"autoIncrement\":{\"action\":\"numeric\",\"origin\":\"program\",\"length\":4,\"incremental\":true}}}" };
        var consecutive = 1;
        var masterLabels = new List<MasterLabel>()
        {
            new MasterLabel() { Description = "New Master", MasterLabelId = 1 },
            new MasterLabel() { Description = "New Master", MasterLabelId = 2 }
        };
        var variables = new List<IndTrace.Domain.Entities.Variable>
         {
             new IndTrace.Domain.Entities.Variable
             {
                 VariableId = 1,
                 Name = "TestVariable1",
                 MachineId = machineId,
                 IsActive = 1,
                 VariableGroupId = TagsGroups.ReferenceTags.Value
             },
             new IndTrace.Domain.Entities.Variable
             {
                 VariableId = 2,
                 Name = "TestVariable2",
                 MachineId = machineId,
                 IsActive = 1,
                 VariableGroupId = TagsGroups.ReferenceTags.Value
             }
         };
        var barcode = new BarCode() { BarCodeId = new BarCodeId(consecutive), Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        Console.WriteLine($"DEBUG: Setting up mocks");

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Machine>>(), Arg.Any<CancellationToken>()).Returns(Result<IndTrace.Domain.Entities.Machine?>.Success(machine));
        _productRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Product>>(), Arg.Any<CancellationToken>()).Returns(Result<IndTrace.Domain.Entities.Product?>.Success(product));
        _ruleRepository.FirstOrDefaultAsync(Arg.Any<Specification<Rule>>(), Arg.Any<CancellationToken>()).Returns(Result<Rule?>.Success(rule));

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: [COMPILATION ERROR FIX] - Replace _masterLabelRepository with _masterLabelService mock after architectural refactor
        _masterLabelService
            .GetMasterLabelByPartNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<string>>.Success(new List<string> { "MASTER_LABEL_1", "MASTER_LABEL_2" }));

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: [ARCHITECTURAL CLEANUP] - Add mock for IBarCodeService GetConsecutiveByBarCodeLabelAsync
        _barCodeService
            .GetConsecutiveByBarCodeLabelAsync(Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<Rule?>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(consecutive));
        _variableRepository.ListAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Variable>>(), Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<IndTrace.Domain.Entities.Variable>>.Success(variables));

        _dateTimeMachine.Now.Returns(currentTime);

        // #114: the barcode + Started cycle pair is persisted through the ONE atomic aggregate save.
        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Mock the 2-parameter AddAsync for request repository
        _requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));

        // Mock shift service
        _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent()));

        _logger.LogInformation("DEBUG: About to call ProcessAsync");

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        _logger.LogInformation("DEBUG: ProcessAsync completed, result.IsSuccess={IsSuccess}", result.IsSuccess);
        if (!result.IsSuccess)
        {
            _logger.LogError("DEBUG: Result.Errors = {Errors}", string.Join(", ", result.Errors ?? []));
        }

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(machineId);
        result.Value.PartNumber.ShouldBe(partNumber);
        result.Value.ResultValidation.ShouldBe(ResultValidation.Valid);

        _logger.LogInformation("DEBUG: Verifying repository calls");

        // #114: ONE atomic save carrying the barcode root AND its staged Started cycle.
        await _barCodeAggregateRepository.Received(1).SaveAsync(Arg.Is<BarCode>(b =>
            b.MachineId.Value == machineId &&
            b.ProductId == product.ProductId &&
            b.PendingNewCycles.Count == 1 &&
            b.PendingNewCycles[0].MachineId.Value == machineId), Arg.Any<CancellationToken>());

        await _requestRepository.Received(1).AddAsync(Arg.Is<TaskGatewayRequest>(r =>
            r.ResultValidation == ResultValidation.Valid), Arg.Any<CancellationToken>());

        _logger.LogInformation("DEBUG: Test completed successfully");
    }

    /// <summary>
    /// Executes Process_WithMissingMachine_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WithMissingMachine_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WithMissingMachine_ShouldReturnFailure()
    {
        // Arrange
        var machineId = 999;
        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = "TEST123"
            }
        };

        //[Fix]
        //CLAUDE
        //Date: 18/06/2026
        //Reason: [Not-found path] - A missing machine is a successful read returning null, not a repo
        //        failure. RequireValue emits the handler's custom diagnostic only on success-but-null;
        //        stubbing WithFailure suppressed it. Exercise the real not-found path.
        _machineRepository.FirstOrDefaultAsync(
            Arg.Any<Specification<IndTrace.Domain.Entities.Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IndTrace.Domain.Entities.Machine?>.Success((IndTrace.Domain.Entities.Machine?)null));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Machine 999 does not exist or cannot create labels.");
        await _requestRepository.Received(1).AddAsync(Arg.Is<TaskGatewayRequest>(r =>
            r.ResultValidation == ResultValidation.MachineNotFound), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithMissingProduct_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WithMissingProduct_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WithMissingProduct_ShouldReturnFailure()
    {
        // Arrange
        var machineId = 1;
        var partNumber = "INVALID123";
        var machine = new IndTrace.Domain.Entities.Machine { MachineId = new MachineId(machineId), Name = "TestMachine" };

        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber
            }
        };

        //[Fix]
        //CLAUDE
        //Date: 18/06/2026
        //Reason: [Not-found path] - A missing product is a successful read returning null, not a repo
        //        failure; exercise the real not-found path so the handler's custom diagnostic surfaces.
        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Machine>>(), Arg.Any<CancellationToken>()).Returns(Result<IndTrace.Domain.Entities.Machine?>.Success(machine));
        _productRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Product>>(), Arg.Any<CancellationToken>()).Returns(Result<IndTrace.Domain.Entities.Product?>.Success((IndTrace.Domain.Entities.Product?)null));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product for INVALID123 does not exist.");
        await _requestRepository.Received(1).AddAsync(Arg.Is<TaskGatewayRequest>(r =>
            r.ResultValidation == ResultValidation.ProductNotFound), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithMissingRule_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WithMissingRule_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WithMissingRule_ShouldReturnFailure()
    {
        // Arrange
        var machineId = 1;
        var partNumber = "TEST123";
        var machine = new IndTrace.Domain.Entities.Machine { MachineId = new MachineId(machineId), Name = "TestMachine" };
        var product = IndTrace.Domain.Entities.Product.CreateFixture(productId: 1, partNumber: partNumber);

        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber
            }
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Machine>>(), Arg.Any<CancellationToken>()).Returns(Result<IndTrace.Domain.Entities.Machine?>.Success(machine));
        _productRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Product>>(), Arg.Any<CancellationToken>()).Returns(Result<IndTrace.Domain.Entities.Product?>.Success(product));
        //[Fix]
        //CLAUDE
        //Date: 18/06/2026
        //Reason: [Not-found path] - A missing rule is a successful read returning null, not a repo
        //        failure; exercise the real not-found path so the handler's custom diagnostic surfaces.
        _ruleRepository.FirstOrDefaultAsync(Arg.Any<Specification<Rule>>(), Arg.Any<CancellationToken>()).Returns(Result<Rule?>.Success((Rule?)null));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Rule for Machine 1 does not exist.");
        await _requestRepository.Received(1).AddAsync(Arg.Is<TaskGatewayRequest>(r =>
            r.ResultValidation == ResultValidation.RuleNotFound), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes ProcessAsync_WhenAddFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WhenAddFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task ProcessAsync_WhenAddFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateBarCodeCommand();

        command.Command = new TaskGatewayRequest
        {
            MachineId = 100,
            PartNumber = "TEST123"
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IndTrace.Domain.Entities.Machine?>.Success(new IndTrace.Domain.Entities.Machine { MachineId = new MachineId(100), Name = "TestMachine", MachineType = MachineType.Process }));

        _productRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Product>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IndTrace.Domain.Entities.Product?>.Success(IndTrace.Domain.Entities.Product.CreateFixture(productId: 1, partNumber: "TEST123")));

        _ruleRepository.FirstOrDefaultAsync(Arg.Any<Specification<Rule>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Rule?>.WithFailure("Database connection failed"));

        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Database connection failed"));

        //[TODO]
        // SETUP THE COMPLETE PIPILENE TO TRIGGER THE FAILURE AT ADDING BARCODE STEP
        // ADD UNIT TEST TO TRIGGER THE FAILURE AT EACH STEP OF THE PIPELINE TO ENSURE FULL COVERAGE

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    // #114 chunk A regression: a failure at the SHIFT stage must leave ZERO barcode/cycle persistence —
    // pre-fix the handler persisted both rows BEFORE calling the shift service, so a shift failure left
    // phantom traceability rows (burned consecutive; the PLC retry minted a second label).
    private void SetupHappyPathUntilPersistence()
    {
        var machine = new IndTrace.Domain.Entities.Machine { MachineId = new MachineId(1), Name = "TestMachine", MachineType = MachineType.Printer };
        var product = IndTrace.Domain.Entities.Product.CreateFixture(productId: 1, partNumber: "TEST123");
        var rule = new Rule { RuleId = 1, RuleJson = "{\"ruleFunction\":[\"lineIdentifier\",\"partNumber\",\"autoIncrement\"],\"components\":{\"lineIdentifier\":{\"action\":\"string\",\"origin\":\"fixed\",\"value\":\"L\"},\"partNumber\":{\"action\":\"string\",\"origin\":\"program\",\"lengthMin\":6,\"lengthMax\":9},\"autoIncrement\":{\"action\":\"numeric\",\"origin\":\"program\",\"length\":4,\"incremental\":true}}}" };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IndTrace.Domain.Entities.Machine?>.Success(machine));
        _productRepository.FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Product>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IndTrace.Domain.Entities.Product?>.Success(product));
        _ruleRepository.FirstOrDefaultAsync(Arg.Any<Specification<Rule>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Rule?>.Success(rule));
        _masterLabelService.GetMasterLabelByPartNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<string>>.Success(new List<string> { "MASTER_LABEL_1" }));
        _barCodeService.GetConsecutiveByBarCodeLabelAsync(Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<Rule?>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Local));
        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
    }

    private static CreateBarCodeCommand ValidCommand() => new()
    {
        Command = new TaskGatewayRequest { MachineId = 1, PartNumber = "TEST123" }
    };

    /// <summary>
    /// #114 chunk A: when the shift service fails, NO barcode and NO cycle may be persisted (the failable
    /// lookups must run BEFORE any persistence). Pre-fix this test is RED: both rows were already committed.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task ProcessAsync_WhenShiftServiceFails_ShouldNotPersistBarcodeOrCycle()
    {
        // Arrange
        SetupHappyPathUntilPersistence();
        _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<ShiftCreatedEvent>.WithFailure("Shift service unavailable"));
        _variableRepository.ListAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<IndTrace.Domain.Entities.Variable>>.Success(new List<IndTrace.Domain.Entities.Variable>
            {
                new() { VariableId = 1, Name = "RefTag1", MachineId = 1, IsActive = 1, VariableGroupId = TagsGroups.ReferenceTags.Value },
            }));

        // Act
        var result = await _handler.ProcessAsync(ValidCommand(), TestContext.Current.CancellationToken);

        // Assert — the PLC sees the failure AND the database keeps zero barcode/cycle rows: the ONE
        // persistence seam (the atomic aggregate save) was never invoked.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Shift service unavailable");
        await _barCodeAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #114 chunk A: when the reference-variables lookup comes back empty (the "References ... not found."
    /// refusal), NO barcode and NO cycle may be persisted. Pre-fix this test is RED: both rows were already
    /// committed before the variables lookup ran.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task ProcessAsync_WhenReferencesLookupEmpty_ShouldNotPersistBarcodeOrCycle()
    {
        // Arrange
        SetupHappyPathUntilPersistence();
        _shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { CyclesOk = 3 }));
        _variableRepository.ListAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<IndTrace.Domain.Entities.Variable>>.Success(new List<IndTrace.Domain.Entities.Variable>()));

        // Act
        var result = await _handler.ProcessAsync(ValidCommand(), TestContext.Current.CancellationToken);

        // Assert — the frozen §7 refusal is unchanged AND the database keeps zero barcode/cycle rows: the
        // ONE persistence seam (the atomic aggregate save) was never invoked.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("References for TEST123 not found.");
        await _barCodeAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes ProcessAsync_WhenCommitFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WhenCommitFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task ProcessAsync_WhenCommitFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateBarCodeCommand();
        command.Command = new TaskGatewayRequest
        {
            MachineId = -1, // Invalid machine ID to trigger failure
            PartNumber = "TEST123"
        };

        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: PATTERN C Fix - Test actual handler failure path: invalid machine ID scenario
        _requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Machine -1 number invalid");
    }
}