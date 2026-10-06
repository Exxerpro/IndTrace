// <copyright file="OeeServiceCalculationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.CodeDom;
using IndTrace.Application.Oee.Services;
using NSubstitute.ExceptionExtensions;

namespace Application.UnitTests.Features.Oee.Services;

/// <summary>
/// Calculation tests for OeeService focusing on core OEE calculation scenarios
/// </summary>
public class OeeServiceCalculationTests : IDisposable
{
    private readonly ICommandHandler<CalculateOeeCommand, OeeMetrics> _calculateOeeHandler = null!;
    private readonly IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse> _getHistoryHandler = null!;
    private readonly IOeeRepository _oeeRepository = null!;
    private readonly ILogger<OeeService> _logger = null!;
    private readonly OeeService _service = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public OeeServiceCalculationTests()
    {
        _calculateOeeHandler = Substitute.For<ICommandHandler<CalculateOeeCommand, OeeMetrics>>();
        _getHistoryHandler = Substitute.For<IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse>>();
        _oeeRepository = Substitute.For<IOeeRepository>();
        _logger = XUnitLogger.CreateLogger<OeeService>();

        _service = new OeeService(
            _calculateOeeHandler,
            _getHistoryHandler,
            _oeeRepository,
            _logger);
    }

    /// <summary>
    /// Executes Should_CalculateOee_When_ValidParametersProvided operation.
    /// </summary>
    /// <returns>The result of Should_CalculateOee_When_ValidParametersProvided.</returns>

    [Fact]
    public async Task Should_CalculateOee_When_ValidParametersProvided()
    {
        // Arrange - Ford F-150 engine block machining operation
        const int machineId = 101; // Robotic Welding Cell #1
        const double totalTimeMinutes = 480.0; // 8-hour shift
        const double downtimeMinutes = 24.0;   // 30 minutes maintenance downtime
        const double idealCycleTimeSeconds = 120.0; // 2 minutes per engine block
        const int totalCount = 230;            // Actual production
        const int defectCount = 5;             // 5 defective parts

        var expectedMetrics = new OeeMetrics(0.95m, 0.88m, 0.97m); // World-class performance

        _calculateOeeHandler.ProcessAsync(Arg.Any<CalculateOeeCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedMetrics);

        // Act
        var result = await _service.CalculateOeeAsync(
            machineId, totalTimeMinutes, downtimeMinutes,
            idealCycleTimeSeconds, totalCount, defectCount, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedMetrics);

        await _calculateOeeHandler.Received(1).ProcessAsync(
            Arg.Is<CalculateOeeCommand>(cmd =>
                cmd.MachineId == machineId &&
                cmd.TotalTimeMinutes == totalTimeMinutes &&
                cmd.DowntimeMinutes == downtimeMinutes &&
                cmd.IdealCycleTimeSeconds == idealCycleTimeSeconds &&
                cmd.TotalCount == totalCount &&
                cmd.DefectCount == defectCount),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_CalculateOee_When_DifferentManufacturingScenarios operation.
    /// </summary>
    /// <returns>The result of Should_CalculateOee_When_DifferentManufacturingScenarios.</returns>

    [Theory]
    [InlineData(480.0, 24.0, 120.0, 230, 5)]   // Ford F-150: Good automotive performance
    [InlineData(600.0, 15.0, 180.0, 200, 2)]   // Tesla Model Y: Excellent EV performance
    [InlineData(420.0, 60.0, 90.0, 280, 12)]   // iPhone PCB: High-volume electronics
    [InlineData(360.0, 45.0, 240.0, 90, 3)]    // Pharmaceutical: FDA-regulated precision
    public async Task Should_CalculateOee_When_DifferentManufacturingScenarios(
        double totalTimeMinutes, double downtimeMinutes, double idealCycleTimeSeconds,
        int totalCount, int defectCount)
    {
        // Arrange - Various manufacturing contexts
        const int machineId = 201;
        var expectedMetrics = new OeeMetrics(0.85m, 0.80m, 0.95m);

        _calculateOeeHandler.ProcessAsync(Arg.Any<CalculateOeeCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedMetrics);

        // Act
        var result = await _service.CalculateOeeAsync(
            machineId, totalTimeMinutes, downtimeMinutes,
            idealCycleTimeSeconds, totalCount, defectCount, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedMetrics);
    }

    /// <summary>
    /// Executes Should_StoreOeeCalculation_When_CalculationSucceeds operation.
    /// </summary>
    /// <returns>The result of Should_StoreOeeCalculation_When_CalculationSucceeds.</returns>

    [Fact]
    public async Task Should_StoreOeeCalculation_When_CalculationSucceeds()
    {
        // Arrange - BMW 3 Series transmission assembly
        const int machineId = 301;
        var expectedMetrics = new OeeMetrics(0.92m, 0.85m, 0.98m);
        var calculationTime = DateTime.UtcNow;

        _calculateOeeHandler.ProcessAsync(Arg.Any<CalculateOeeCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedMetrics);

        // Act
        var result = await _service.CalculateOeeAsync(
            machineId, 480.0, 36.0, 150.0, 192, 3, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedMetrics);

        await _oeeRepository.Received(1).StoreOeeCalculationAsync(
            machineId,
            expectedMetrics,
            Arg.Any<DateTime>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ContinueCalculation_When_StorageFails operation.
    /// </summary>
    /// <returns>The result of Should_ContinueCalculation_When_StorageFails.</returns>

    [Fact]
    public async Task Should_ContinueCalculation_When_StorageFails()
    {
        // Arrange - Storage failure should not fail the calculation
        const int machineId = 401;
        var expectedMetrics = new OeeMetrics(0.88m, 0.82m, 0.94m);

        _calculateOeeHandler.ProcessAsync(Arg.Any<CalculateOeeCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedMetrics);
#pragma warning disable
        _oeeRepository.StoreOeeCalculationAsync(
            Arg.Any<int>(), Arg.Any<OeeMetrics>(), Arg.Any<DateTime>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Database connection failed"));
#pragma warning restore
        // Act & Assert - Should not throw
        var result = await _service.CalculateOeeAsync(
            machineId, 480.0, 30.0, 140.0, 205, 8, cancellationToken: TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedMetrics);
    }

    /// <summary>
    /// Executes Should_PropagateException_When_CalculationFails operation.
    /// </summary>
    /// <returns>The result of Should_PropagateException_When_CalculationFails.</returns>

    [Fact]
    public async Task Should_PropagateException_When_CalculationFails()
    {
        // Arrange
        const int machineId = 501;
#pragma warning disable
        _calculateOeeHandler.ProcessAsync(Arg.Any<CalculateOeeCommand>(), Arg.Any<CancellationToken>())
            .Throws(new ArgumentException("Invalid calculation parameters"));
#pragma watning restore
        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            () => _service.CalculateOeeAsync(machineId, 480.0, 30.0, 120.0, 200, 5));

        exception.Message.ShouldBe("Invalid calculation parameters");
    }

    /// <summary>
    /// Executes Should_UseCorrectTimestamp_When_CalculatingOee operation.
    /// </summary>
    /// <returns>The result of Should_UseCorrectTimestamp_When_CalculatingOee.</returns>

    [Fact]
    public async Task Should_UseCorrectTimestamp_When_CalculatingOee()
    {
        // Arrange
        const int machineId = 601;
        var expectedMetrics = new OeeMetrics(0.90m, 0.87m, 0.96m);
        var timestampBefore = DateTime.UtcNow;

        _calculateOeeHandler.ProcessAsync(Arg.Any<CalculateOeeCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedMetrics);

        // Act
        await _service.CalculateOeeAsync(machineId, 480.0, 20.0, 130.0, 220, 4, cancellationToken: TestContext.Current.CancellationToken);
        var timestampAfter = DateTime.UtcNow;

        // Assert
        await _calculateOeeHandler.Received(1).ProcessAsync(
            Arg.Is<CalculateOeeCommand>(cmd =>
                cmd.Timestamp >= timestampBefore &&
                cmd.Timestamp <= timestampAfter),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleCancellation_When_CancellationRequested operation.
    /// </summary>
    /// <returns>The result of Should_HandleCancellation_When_CancellationRequested.</returns>

    [Fact]
    public async Task Should_HandleCancellation_When_CancellationRequested()
    {
        // Arrange
        const int machineId = 701;
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // #128: the service now performs an early cancellation check and returns a failure Result before the
        // handler is ever invoked — a cancelled request must not be masked as a measured OEE value.
        var result = await _service.CalculateOeeAsync(
            machineId, 480.0, 25.0, 125.0, 190, 6, cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        await _calculateOeeHandler.DidNotReceive().ProcessAsync(
            Arg.Any<CalculateOeeCommand>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_CalculateCorrectPerformanceLevel_When_DifferentOeeValues operation.
    /// </summary>
    /// <returns>The result of Should_CalculateCorrectPerformanceLevel_When_DifferentOeeValues.</returns>

    [Theory]
    [InlineData(0.95, 0.88, 0.97, "81.09%", "Good")]          // Ford F-150: Good performance
    [InlineData(0.98, 0.92, 0.99, "89.26%", "World Class")]   // Tesla Model Y: World-class
    [InlineData(0.85, 0.75, 0.90, "57.38%", "Fair")]          // Typical manufacturing: Fair
    [InlineData(0.61, 0.65, 0.85, "33.70%", "Poor")]          // Problematic line: Poor
    public async Task Should_CalculateCorrectPerformanceLevel_When_DifferentOeeValues(
        decimal availability, decimal performance, decimal quality,
        string expectedOeePercent, string expectedLevel)
    {
        // Arrange
        const int machineId = 801;
        var metrics = new OeeMetrics(availability, performance, quality);

        _calculateOeeHandler.ProcessAsync(Arg.Any<CalculateOeeCommand>(), Arg.Any<CancellationToken>())
            .Returns(metrics);

        // Act
        var result = await _service.CalculateOeeAsync(machineId, 480.0, 30.0, 120.0, 200, 10, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var resultValue = result.Value;
        resultValue.ShouldNotBeNull();
        var displayMetrics = _service.FormatMetricsForDisplay(resultValue);

        // Assert
        displayMetrics.Oee.ShouldBe(expectedOeePercent);
        displayMetrics.PerformanceLevel.ShouldBe(expectedLevel);
    }

    /// <summary>
    /// #128: a FAILED underlying metrics result must surface as a Result failure, not be masked as a
    /// measured 0% OEE value.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_MetricsCalculationResultFails.</returns>
    [Fact]
    public async Task Should_ReturnFailure_When_MetricsCalculationResultFails()
    {
        // Arrange
        const int machineId = 901;

        _calculateOeeHandler.ProcessAsync(Arg.Any<CalculateOeeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<OeeMetrics>.WithFailure("Calculation failed"));

        // Act
        var result = await _service.CalculateOeeAsync(
            machineId, 480.0, 30.0, 120.0, 200, 5, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - the failure is propagated, NOT masked as a 0% OeeMetrics value.
        result.IsFailure.ShouldBeTrue();

        // A failed calculation must never be stored.
        await _oeeRepository.DidNotReceive().StoreOeeCalculationAsync(
            Arg.Any<int>(), Arg.Any<OeeMetrics>(), Arg.Any<DateTime>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}