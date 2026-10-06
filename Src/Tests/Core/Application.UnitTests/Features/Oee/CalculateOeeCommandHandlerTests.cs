// <copyright file="CalculateOeeCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using NSubstitute.ExceptionExtensions;

namespace Application.UnitTests.Features.Oee;

/// <summary>
/// Unit tests for CalculateOeeCommandHandler
/// </summary>
public class CalculateOeeCommandHandlerTests
{
    private readonly IOeeCalculationService _mockOeeCalculationService = null!;
    private readonly IValidator<CalculateOeeCommand> _mockValidator = null!;
    private readonly ILogger<CalculateOeeCommandHandler> _logger = null!;
    private readonly CalculateOeeCommandHandler _handler = null!;
    private readonly ITestOutputHelper _output = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="output">The output.</param>

    public CalculateOeeCommandHandlerTests(ITestOutputHelper output)
    {
        _mockOeeCalculationService = Substitute.For<IOeeCalculationService>();
        _mockValidator = Substitute.For<IValidator<CalculateOeeCommand>>();

        _output = output;
        _logger = XUnitLogger.CreateLogger<CalculateOeeCommandHandler>();

        _handler = new CalculateOeeCommandHandler(
            _mockOeeCalculationService,
            _mockValidator,
            _logger);
    }

    /// <summary>
    /// Executes Constructor_Should_ThrowArgumentNullException_When_OeeCalculationServiceIsNull operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_Should_ThrowArgumentNullException_When_OeeCalculationServiceIsNull()
    //     {
    //         // Arrange & Act & Assert
    //         Should.Throw<ArgumentNullException>(() =>
    //             new CalculateOeeCommandHandler(null, _mockValidator, _logger))
    //             .ParamName.ShouldBe("oeeCalculationService");
    //     }
    /// <summary>
    /// Executes Constructor_Should_ThrowArgumentNullException_When_ValidatorIsNull operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_Should_ThrowArgumentNullException_When_ValidatorIsNull()
    //     {
    //         // Arrange & Act & Assert
    //         Should.Throw<ArgumentNullException>(() =>
    //             new CalculateOeeCommandHandler(_mockOeeCalculationService, null, _logger))
    //             .ParamName.ShouldBe("validator");
    //     }
    /// <summary>
    /// Executes Constructor_Should_ThrowArgumentNullException_When_LoggerIsNull operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_Should_ThrowArgumentNullException_When_LoggerIsNull()
    //     {
    //         // Arrange & Act & Assert
    //         Should.Throw<ArgumentNullException>(() =>
    //             new CalculateOeeCommandHandler(_mockOeeCalculationService, _mockValidator, null))
    //             .ParamName.ShouldBe("logger");
    //     }
    /// <summary>
    /// Executes ProcessAsync_Should_ReturnOeeMetrics_When_CommandIsValid operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_Should_ReturnOeeMetrics_When_CommandIsValid.</returns>

    [Fact]
    public async Task ProcessAsync_Should_ReturnOeeMetrics_When_CommandIsValid()
    {
        // Arrange
        var command = CreateValidCommand();
        var expectedMetrics = new OeeMetrics(0.85m, 0.90m, 0.95m);

        _mockValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockOeeCalculationService.CalculateOee(
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<int>(),
            Arg.Any<int>())
            .Returns(expectedMetrics);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var resultValue = result.Value;
        resultValue.ShouldNotBeNull();
        resultValue.ShouldNotBeNull();
        resultValue.ShouldNotBeNull();
        resultValue.ShouldBe(expectedMetrics);

        await _mockValidator.Received(1).ValidateAsync(command, Arg.Any<CancellationToken>());
        _mockOeeCalculationService.Received(1).CalculateOee(
            TimeSpan.FromMinutes(command.TotalTimeMinutes),
            TimeSpan.FromMinutes(command.DowntimeMinutes),
            TimeSpan.FromSeconds(command.IdealCycleTimeSeconds),
            command.TotalCount,
            command.DefectCount);
    }

    /// <summary>
    /// Executes ProcessAsync_Should_ThrowArgumentException_When_ValidationFails operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_Should_ThrowArgumentException_When_ValidationFails.</returns>

    [Fact]
    public async Task ProcessAsync_Should_ThrowArgumentException_When_ValidationFails()
    {
        // Arrange
        var command = CreateValidCommand();
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("MachineId", "Machine ID is required"),
            new ValidationFailure("TotalTimeMinutes", "Total time must be greater than zero")
        });

        _mockValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(validationResult);

        // Act & Assert
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();

        //exception.Message.ShouldContain("Invalid command:");
        //exception.Message.ShouldContain("Machine ID is required");
        //exception.Message.ShouldContain("Total time must be greater than zero");

        await _mockValidator.Received(1).ValidateAsync(command, Arg.Any<CancellationToken>());
        _mockOeeCalculationService.DidNotReceive().CalculateOee(
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<int>(),
            Arg.Any<int>());
    }

    /// <summary>
    /// Executes ProcessAsync_Should_PropagateException_When_OeeCalculationServiceThrows operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_Should_PropagateException_When_OeeCalculationServiceThrows.</returns>

    [Fact]
    public async Task ProcessAsync_Should_PropagateException_When_OeeCalculationServiceThrows()
    {
        // Arrange
        var command = CreateValidCommand();
        var expectedException = new InvalidOperationException("Calculation service error");

        _mockValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockOeeCalculationService.CalculateOee(
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<int>(),
            Arg.Any<int>())
            .Throws(expectedException);

        // Act & Assert
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();

        await _mockValidator.Received(1).ValidateAsync(command, Arg.Any<CancellationToken>());
        _mockOeeCalculationService.Received(1).CalculateOee(
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<int>(),
            Arg.Any<int>());
    }

    /// <summary>
    /// Executes ProcessAsync_Should_PassCorrectParametersToCalculationService operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_Should_PassCorrectParametersToCalculationService.</returns>

    [Fact]
    public async Task ProcessAsync_Should_PassCorrectParametersToCalculationService()
    {
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 42,
            TotalTimeMinutes = 480.0, // 8 hours
            DowntimeMinutes = 60.0,   // 1 hour
            IdealCycleTimeSeconds = 30.0, // 30 seconds
            TotalCount = 100,
            DefectCount = 5,
            Timestamp = DateTime.UtcNow
        };

        var expectedMetrics = new OeeMetrics(0.75m, 0.85m, 0.95m);

        _mockValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockOeeCalculationService.CalculateOee(
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<int>(),
            Arg.Any<int>())
            .Returns(expectedMetrics);

        // Act
        await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        _mockOeeCalculationService.Received(1).CalculateOee(
            TimeSpan.FromMinutes(480.0),
            TimeSpan.FromMinutes(60.0),
            TimeSpan.FromSeconds(30.0),
            100,
            5);
    }

    /// <summary>
    /// Executes ProcessAsync_Should_UseCancellationToken_WhenProvided operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_Should_UseCancellationToken_WhenProvided.</returns>

    [Fact]
    public async Task ProcessAsync_Should_UseCancellationToken_WhenProvided()
    {
        // Arrange
        var command = CreateValidCommand();
        var cancellationToken = TestContext.Current.CancellationToken;
        var expectedMetrics = new OeeMetrics(0.80m, 0.90m, 0.89m);

        _mockValidator.ValidateAsync(command, cancellationToken)
            .Returns(new ValidationResult());

        _mockOeeCalculationService.CalculateOee(
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<int>(),
            Arg.Any<int>())
            .Returns(expectedMetrics);

        // Act
        var result = await _handler.ProcessAsync(command, cancellationToken);
        result.Value.ShouldNotBeNull();
        var resultValue = result.Value;
        resultValue.ShouldNotBeNull();
        resultValue.ShouldNotBeNull();

        // Assert
        resultValue.ShouldBe(expectedMetrics);
        await _mockValidator.Received(1).ValidateAsync(command, cancellationToken);
    }

    /// <summary>
    /// Executes ProcessAsync_Should_HandleVariousValidInputs operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_Should_HandleVariousValidInputs.</returns>

    [Theory]
    [MemberData(nameof(GetValidCommandTestCases))]
    public async Task ProcessAsync_Should_HandleVariousValidInputs(
        int machineId,
        double totalTimeMinutes,
        double downtimeMinutes,
        double idealCycleTimeSeconds,
        int totalCount,
        int defectCount)
    {
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = machineId,
            TotalTimeMinutes = totalTimeMinutes,
            DowntimeMinutes = downtimeMinutes,
            IdealCycleTimeSeconds = idealCycleTimeSeconds,
            TotalCount = totalCount,
            DefectCount = defectCount,
            Timestamp = DateTime.UtcNow
        };

        var expectedMetrics = new OeeMetrics(0.80m, 0.90m, 0.89m);

        _mockValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockOeeCalculationService.CalculateOee(
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<int>(),
            Arg.Any<int>())
            .Returns(expectedMetrics);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();
        var resultValue = result.Value;
        resultValue.ShouldNotBeNull();

        // Assert
        resultValue.ShouldNotBeNull();
        resultValue.ShouldBe(expectedMetrics);
    }

    /// <summary>
    /// Executes GetValidCommandTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidCommandTestCases.</returns>

    public static IEnumerable<object[]> GetValidCommandTestCases()
    {
        yield return new object[] { 1, 480.0, 0.0, 30.0, 100, 0 }; // Perfect production
        yield return new object[] { 5, 480.0, 60.0, 25.5, 150, 10 }; // Normal production with downtime and defects
        yield return new object[] { 10, 960.0, 120.0, 15.0, 500, 25 }; // Long shift
        yield return new object[] { 99, 240.0, 30.0, 45.0, 50, 2 }; // Short shift
        yield return new object[] { 1000, 600.0, 90.0, 60.0, 200, 15 }; // High machine ID
    }

    private static CalculateOeeCommand CreateValidCommand()
    {
        return new CalculateOeeCommand
        {
            MachineId = 100,
            TotalTimeMinutes = 480.0,
            DowntimeMinutes = 30.0,
            IdealCycleTimeSeconds = 25.0,
            TotalCount = 100,
            DefectCount = 5,
            Timestamp = DateTime.UtcNow
        };
    }
}