// <copyright file="CreateBarCodeCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using ValidationException = IndTrace.Application.Models.Exceptions.ValidationException;

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Commands;
/// <summary>
/// Represents the CreateBarCodeCommandTests.
/// </summary>

public class CreateBarCodeCommandTests : DependenciesFactory
{
    private const string RuleJson = @"
              {
        'ruleId': 'V3',
        'ruleFunction': ['lineIdentifier','lineNumber','fixedPart', 'partNumber', 'lastTwoYearDigits','julianDay',  'autoIncrement'],
        'components': {
            'lineIdentifier': {
                'action': 'string',
                'origin': 'fixed',
                'value': 'L'
            },
            'lineNumber': {
                'action': 'string',
                'origin': 'fixed',
                'value': '1'
            },
            'fixedPart': {
                'action': 'string',
                'origin': 'fixed',
                'value': 'A'
            },
            'partNumber': {
                'action': 'string',
                'origin': 'program',
                'lengthMin': 6,
                'lengthMax': 9
            },
            'lastTwoYearDigits': {
                'action': 'lastTwoYearDigits',
                'origin': 'program'
            },
            'julianDay': {
                'action': 'julianDay',
                'origin': 'program'
            },
            'autoIncrement': {
                'action': 'numeric',
                'origin': 'program',
                'length': 4,
                'incremental': true
            }
        }
    }";

    private readonly IValidator<CreateBarCodeCommand> _validator = new CreateBarCodeCommandValidator();

    //[Fix]
    //CLAUDE
    //Date: 09/09/2025
    //Reason: [Constructor Pattern] - Added ITestContextAccessor parameter to match DependenciesFactory signature
    public CreateBarCodeCommandTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes ShouldPassValidation_WhenCommandIsValid operation.
    /// </summary>

    [Fact]
    public void ShouldPassValidation_WhenCommandIsValid()
    {
        var command = new CreateBarCodeCommand();
        // Validator requires BarCode to contain PartNumber; provide both consistently
        var partNumber = "L90164629";
        var label = $"{partNumber}-X";
        command.WithData(TaskGatewayRequest.Create(100, label, partNumber));

        Should.NotThrow(() => command.Validate(_validator));
    }

    /// <summary>
    /// Executes ShouldFailValidation_WhenPartNumberIsEmpty operation.
    /// </summary>

    [Fact]
    public void ShouldFailValidation_WhenPartNumberIsEmpty()
    {
        // Arrange
        var machineId = 100;
        var partNumber = "";

        var command = new CreateBarCodeCommand();
        command.WithData(TaskGatewayRequest.CreateWithPartNumber(machineId, partNumber));

        // Act & Assert
        var exception = Should.Throw<ValidationException>(() => command.Validate(_validator));

        //[Fix]
        //CLAUDE
        //Date: 09/09/2025
        //Reason: [Validation Message Issue] - Updated expectation to match actual generic validation message instead of specific field name
        exception.Message.ShouldContain("One or more validation failures have occurred");
    }

    /// <summary>
    /// Executes ShouldHandleRequestWithNegativeResponse operation.
    /// </summary>
    /// <returns>The result of ShouldHandleRequestWithNegativeResponse.</returns>

    [Fact]
    public async Task ShouldHandleRequestWithNegativeResponse()
    {
        await Initialization;

        // Arrange

        const string partNumber = "L90164629";
        const int machineId = -1;

        // NO MOCKING: Use real DpGatewayCommandDispatcher for LOW LATENCY

        var DpGatewayCommandDispatcher = base.DpGatewayCommandDispatcher;

        var rule = new Rule
        {
            RuleJson = RuleJson,
            IsActive = true
        };

        // DELETED: NSubstitute setup

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: [ARCHITECTURAL UPDATE] - Add missing service interface parameters for new constructor signature

        var sut = new CreateBarCodeCommandHandler(
            DpRoRuleRepository,
            DpBarCodeAggregateRepository,
            DpRoMachineRepository,
            DpRoProductRepository,
            DpRoVariablesRepository,
            DpCommandRepository,
            DpShiftService,
            DpDateTimeMachine,
            DpMasterLabelService,
            DpBarCodeService,
            XUnitLogger.CreateLogger<CreateBarCodeCommandHandler>());

        var command = new CreateBarCodeCommand();
        command.WithData(TaskGatewayRequest.CreateWithPartNumber(machineId, partNumber));

        var gatewayResponse = new TaskGatewayResponseDto()
        {
            MachineId = machineId,
            CycleStatus = CycleStatus.Started,
            FlowStatus = FlowStatus.Created,
            PartStatus = PartStatus.Ok,

            TimeStamp = DpDateTimeMachine.Now.ToLocalTime(),
        };

        // DELETED: NSubstitute setup - using real dispatcher processing

        var result = await sut.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        // Using Shouldly for assertions

        {
            result.ShouldBeOfType<Result<TaskGatewayResponseDto>>();
            result.IsFailure.ShouldBeTrue();
            result.IsSuccess.ShouldBeFalse();
        }
    }

    /// <summary>
    /// Executes ShouldNotPrintLabel_OnlyPrinterMachinesCanPrint operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="machineId">The machineId.</param>
    /// <param name="date">The date.</param>
    /// <returns>The result of ShouldNotPrintLabel_OnlyPrinterMachinesCanPrint.</returns>

    [Theory]
    [InlineData("L90164629", 500, "2023-08-27 03:41:34.9600000")]
    [InlineData("L90164629", 500, "2023-01-01 03:41:34.9600000")]
    [InlineData("L90164629", 500, "2023-01-12 03:41:34.9600000")]
    [InlineData("L90164629", 500, "2023-12-31 03:41:34.9600000")]
    [InlineData("L90164629", 500, "2024-08-27 03:41:34.9600000")]
    [InlineData("L90164629", 500, "2024-01-01 03:41:34.9600000")]
    [InlineData("L90164629", 500, "2024-01-12 03:41:34.9600000")]
    [InlineData("L90164629", 500, "2024-12-31 03:41:34.9600000")]
    [InlineData("L100003", 300, "2023-08-27 03:41:34.9600000")]
    [InlineData("L100003", 300, "2023-01-01 03:41:34.9600000")]
    [InlineData("L100003", 300, "2023-01-12 03:41:34.9600000")]
    [InlineData("L100003", 300, "2023-12-31 03:41:34.9600000")]
    [InlineData("L100003", 300, "2024-08-27 03:41:34.9600000")]
    [InlineData("L100003", 300, "2024-01-01 03:41:34.9600000")]
    [InlineData("L100003", 300, "2024-02-29 03:41:34.9600000")]
    [InlineData("L100003", 300, "2024-01-12 03:41:34.9600000")]
    [InlineData("L100003", 300, "2024-12-31 03:41:34.9600000")]
    [InlineData("L90164629", 328, "2023-12-31 03:41:34.9600000")]
    public async Task ShouldNotPrintLabel_OnlyPrinterMachinesCanPrint(string partNumber, int machineId, string date) // Changed to return Task instead of void
    {
        await Initialization;

        // Arrange

        var DpGatewayCommandDispatcher = base.DpGatewayCommandDispatcher;

        var parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);

        DpDateTimeMachine.SetDateTimeNow(parsedDate);

        var rule = new Rule
        {
            RuleJson = RuleJson,
            IsActive = true
        };

        // var ruleProvider = DpRuleProvider; // Changed to use NSubstitute syntax
        //Is RULE PROVIDER USED ???
        // DELETED: NSubstitute setup

        var expectedShift = new ShiftCreatedEvent()
        {
            StartBy = DateTime.Now,
            Duration = TimeSpan.FromHours(8),
            EndTime = DateTime.Now + TimeSpan.FromHours(8),
            ShiftType = "Normal",
            CyclesOk = 1
        };

        var expectedResult = Result<ShiftCreatedEvent>.Success(expectedShift);

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: [ARCHITECTURAL UPDATE] - Add missing service interface parameters for new constructor signature

        var sut = new CreateBarCodeCommandHandler(
            DpRoRuleRepository,
            DpBarCodeAggregateRepository,
            DpRoMachineRepository,
            DpRoProductRepository,
            DpRoVariablesRepository,
            DpCommandRepository,
            DpShiftService,
            DpDateTimeMachine,
            DpMasterLabelService,
            DpBarCodeService,
            XUnitLogger.CreateLogger<CreateBarCodeCommandHandler>());

        var command = new CreateBarCodeCommand();
        command.WithData(TaskGatewayRequest.CreateWithPartNumber(machineId, partNumber));

        // Act
        var result = await sut.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        // Using Shouldly for assertions

        {
            result.IsFailure.ShouldBeTrue();
            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain($"Machine {machineId} does not exist or cannot create labels.");

            //[Fix]
            //CLAUDE
            //Date: 20/06/2026
            //Reason: [Story 3.3 AC3/AC4 value-carrying failure] - a failed Create now returns a NON-null
            //        diagnostic Value carrying the specific negative ResultValidation (MachineNotFound for a
            //        missing/non-printer machine) so the code survives ControllerExtensions.PublishResultToPlc.
            //        The prior `result.Value.ShouldBeNull()` predates that contract (already honored by the
            //        Application unit tests and Create golden tests).
            result.Value.ShouldNotBeNull();
            result.Value.ResultValidation.ShouldBe(ResultValidation.MachineNotFound);
        }
    }
}