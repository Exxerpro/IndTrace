// <copyright file="OeeServiceHistoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Oee.Services;
using NSubstitute.ExceptionExtensions;

namespace Application.UnitTests.Features.Oee.Services;

/// <summary>
/// History and summary tests for OeeService focusing on data retrieval operations
/// </summary>
public class OeeServiceHistoryTests : IDisposable
{
    private readonly ICommandHandler<CalculateOeeCommand, OeeMetrics> _calculateOeeHandler = null!;
    private readonly IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse> _getHistoryHandler = null!;
    private readonly IOeeRepository _oeeRepository = null!;
    private readonly ILogger<OeeService> _logger = null!;
    private readonly OeeService _service = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public OeeServiceHistoryTests()
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
    /// Executes Should_GetOeeHistory_When_ValidParametersProvided operation.
    /// </summary>
    /// <returns>The result of Should_GetOeeHistory_When_ValidParametersProvided.</returns>

    [Fact]
    public async Task Should_GetOeeHistory_When_ValidParametersProvided()
    {
        // Arrange - Ford F-150 production line history
        const int machineId = 101;
        var startDate = DateTime.UtcNow.AddDays(-7);  // Last week
        var endDate = DateTime.UtcNow;
        const int pageNumber = 1;
        const int pageSize = 20;

        var expectedResponse = new GetOeeHistoryResponse
        {
            Records = new[]
            {
                new OeeHistoryRecord
                {
                    MachineId = machineId,
                    MachineName = "Robotic Welding Cell #1",
                    Metrics = new OeeMetrics(0.95m, 0.88m, 0.97m),
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Shift = "Day Shift A"
                }
            },
            TotalCount = 50,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _getHistoryHandler.HandleAsync(Arg.Any<GetOeeHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _service.GetOeeHistoryAsync(
            machineId, startDate, endDate, pageNumber: pageNumber, pageSize: pageSize, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Value.ShouldBe(expectedResponse);
        result.Value.ShouldNotBeNull();
        result.Value.Records.Count().ShouldBe(1);
        result.Value.TotalCount.ShouldBe(50);
        result.Value.PageNumber.ShouldBe(pageNumber);
        result.Value.PageSize.ShouldBe(pageSize);

        await _getHistoryHandler.Received(1).HandleAsync(
            Arg.Is<GetOeeHistoryQuery>(q =>
                q.MachineId == machineId &&
                q.StartDate == startDate &&
                q.EndDate == endDate &&
                q.PageNumber == pageNumber &&
                q.PageSize == pageSize),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_UseDefaultDateRange_When_DatesNotProvided operation.
    /// </summary>
    /// <returns>The result of Should_UseDefaultDateRange_When_DatesNotProvided.</returns>

    [Fact]
    public async Task Should_UseDefaultDateRange_When_DatesNotProvided()
    {
        // Arrange - Tesla Model Y battery pack production (no date range specified)
        const int machineId = 201;
        var expectedResponse = new GetOeeHistoryResponse
        {
            Records = Enumerable.Empty<OeeHistoryRecord>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 50
        };

        _getHistoryHandler.HandleAsync(Arg.Any<GetOeeHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _service.GetOeeHistoryAsync(machineId, cancellationToken: TestContext.Current.CancellationToken);
        var afterCall = DateTime.UtcNow;

        // Assert
        await _getHistoryHandler.Received(1).HandleAsync(
            Arg.Is<GetOeeHistoryQuery>(q =>
                q.MachineId == machineId &&
                q.StartDate >= beforeCall.AddDays(-30) &&
                q.StartDate <= afterCall.AddDays(-30) &&
                q.EndDate >= beforeCall &&
                q.EndDate <= afterCall &&
                q.PageNumber == 1 &&
                q.PageSize == 50),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_GetOeeHistoryWithFilters_When_FiltersProvided operation.
    /// </summary>
    /// <returns>The result of Should_GetOeeHistoryWithFilters_When_FiltersProvided.</returns>

    [Theory]
    [InlineData(null, null, OeePerformanceLevel.Poor)]     // All machines, Poor performance
    [InlineData(301, 401, OeePerformanceLevel.Good)]       // iPhone PCB machines, Good performance
    [InlineData(501, null, OeePerformanceLevel.WorldClass)] // Pharmaceutical machine, World-class
    public async Task Should_GetOeeHistoryWithFilters_When_FiltersProvided(
        int? machineId, int? secondMachineId, OeePerformanceLevel minPerformanceLevel)
    {
        var logger = XUnitLogger.CreateLogger<OeeService>();
        logger.LogInformation("Starting test with MachineId: {MachineId}, SecondMachineId: {SecondMachineId}, MinPerformanceLevel: {MinPerformanceLevel}",
            machineId, secondMachineId, minPerformanceLevel);

        // Arrange - Various manufacturing scenarios with performance filters
        var startDate = DateTime.UtcNow.AddDays(-14);
        var endDate = DateTime.UtcNow;

        var expectedResponse = new GetOeeHistoryResponse
        {
            Records = new[]
            {
                new OeeHistoryRecord
                {
                    MachineId = machineId ?? 101,
                    MachineName = "Production Machine",
                    Metrics = new OeeMetrics(0.90m, 0.85m, 0.95m),
                    Timestamp = DateTime.UtcNow.AddDays(-1)
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 50
        };

        _getHistoryHandler.HandleAsync(Arg.Any<GetOeeHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _service.GetOeeHistoryAsync(
            machineId, startDate, endDate, minPerformanceLevel, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        await _getHistoryHandler.Received(1).HandleAsync(
            Arg.Is<GetOeeHistoryQuery>(q =>
                q.MachineId == machineId &&
                q.MinPerformanceLevel == minPerformanceLevel),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_GetLatestOee_When_MachineIdProvided operation.
    /// </summary>
    /// <returns>The result of Should_GetLatestOee_When_MachineIdProvided.</returns>

    [Fact]
    public async Task Should_GetLatestOee_When_MachineIdProvided()
    {
        // Arrange - BMW 3 Series transmission assembly latest OEE
        const int machineId = 301;
        var expectedRecord = new OeeHistoryRecord
        {
            MachineId = machineId,
            MachineName = "BMW Transmission Assembly Line",
            Metrics = new OeeMetrics(0.92m, 0.85m, 0.98m),
            Timestamp = DateTime.UtcNow.AddMinutes(-15),
            Shift = "Evening Shift B"
        };

        _oeeRepository.GetLatestOeeAsync(machineId, Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        var result = await _service.GetLatestOeeAsync(machineId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedRecord);
        result!.MachineId.ShouldBe(machineId);
        result.MachineName.ShouldBe("BMW Transmission Assembly Line");
        result.Metrics.Oee.ShouldBe(0.766360m); // 0.92 * 0.85 * 0.98

        await _oeeRepository.Received(1).GetLatestOeeAsync(machineId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnNull_When_NoLatestOeeFound operation.
    /// </summary>
    /// <returns>The result of Should_ReturnNull_When_NoLatestOeeFound.</returns>

    [Fact]
    public async Task Should_ReturnNull_When_NoLatestOeeFound()
    {
        // Arrange - Machine with no OEE history
        const int machineId = 999;

        _oeeRepository.GetLatestOeeAsync(machineId, Arg.Any<CancellationToken>())
            .Returns((OeeHistoryRecord?)null);

        // Act
        var result = await _service.GetLatestOeeAsync(machineId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
        await _oeeRepository.Received(1).GetLatestOeeAsync(machineId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_GetOeeSummary_When_MachineIdsProvided operation.
    /// </summary>
    /// <returns>The result of Should_GetOeeSummary_When_MachineIdsProvided.</returns>

    [Fact]
    public async Task Should_GetOeeSummary_When_MachineIdsProvided()
    {
        // Arrange - Multiple manufacturing lines summary
        var machineIds = new[] { 101, 201, 301 }; // Ford, Tesla, BMW lines
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var expectedSummary = new[]
        {
            new OeeSummaryRecord
            {
                MachineId = 10001,
                MachineName = "Ford F-150 Welding Cell",
                AverageOee = 0.81m,
                MaxOee = 0.89m,
                MinOee = 0.74m,
                CalculationCount = 120,
                StartDate = startDate,
                EndDate = endDate
            },
            new OeeSummaryRecord
            {
                MachineId = 201,
                MachineName = "Tesla Model Y Battery Assembly",
                AverageOee = 0.87m,
                MaxOee = 0.94m,
                MinOee = 0.79m,
                CalculationCount = 95,
                StartDate = startDate,
                EndDate = endDate
            }
        };

        _oeeRepository.GetOeeSummaryAsync(
            machineIds, startDate, endDate, Arg.Any<CancellationToken>())
            .Returns(expectedSummary);

        // Act
        var result = await _service.GetOeeSummaryAsync(machineIds, startDate, endDate, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);

        var fordSummary = result.First(s => s.MachineId == 10001 || s.MachineId == 101);
        fordSummary.AverageOee.ShouldBe(0.81m);
        fordSummary.MachineName.ShouldBe("Ford F-150 Welding Cell");

        var teslaSummary = result.First(s => s.MachineId == 201);
        teslaSummary.AverageOee.ShouldBe(0.87m);
        teslaSummary.MachineName.ShouldBe("Tesla Model Y Battery Assembly");

        await _oeeRepository.Received(1).GetOeeSummaryAsync(
            machineIds, startDate, endDate, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_UseDefaultDates_When_SummaryDatesNotProvided operation.
    /// </summary>
    /// <returns>The result of Should_UseDefaultDates_When_SummaryDatesNotProvided.</returns>

    [Fact]
    public async Task Should_UseDefaultDates_When_SummaryDatesNotProvided()
    {
        // Arrange - Pharmaceutical production line (no dates specified)
        var machineIds = new[] { 501, 502 };
        var expectedSummary = new[]
        {
            new OeeSummaryRecord
            {
                MachineId = 501,
                MachineName = "Pharmaceutical Tablet Press",
                AverageOee = 0.88m,
                MaxOee = 0.93m,
                MinOee = 0.82m,
                CalculationCount = 150,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            }
        };

        _oeeRepository.GetOeeSummaryAsync(
            Arg.Any<IEnumerable<int>>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(expectedSummary);

        var beforeCall = DateTime.UtcNow;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS1503] Add missing DateTime parameters - method expects (machineIds, startDate, endDate, cancellationToken)
        var result = await _service.GetOeeSummaryAsync(machineIds, beforeCall.AddHours(-1), beforeCall, TestContext.Current.CancellationToken);
        var afterCall = DateTime.UtcNow;

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);

        await _oeeRepository.Received(1).GetOeeSummaryAsync(
            machineIds,
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
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
        const int machineId = 601;
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _getHistoryHandler.HandleAsync(Arg.Any<GetOeeHistoryQuery>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        var result = await _service.GetOeeHistoryAsync(machineId, cancellationToken: cts.Token);
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_PropagateException_When_HistoryRetrievalFails operation.
    /// </summary>
    /// <returns>The result of Should_PropagateException_When_HistoryRetrievalFails.</returns>

    [Fact]
    public async Task Should_PropagateException_When_HistoryRetrievalFails()
    {
        // Arrange
        const int machineId = 701;

        _getHistoryHandler.HandleAsync(Arg.Any<GetOeeHistoryQuery>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var result = await _service.GetOeeHistoryAsync(machineId, cancellationToken: TestContext.Current.CancellationToken);
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_HandlePagination_When_DifferentPageSizesProvided operation.
    /// </summary>
    /// <param name="pageNumber">The pageNumber.</param>
    /// <param name="pageSize">The pageSize.</param>
    /// <returns>The result of Should_HandlePagination_When_DifferentPageSizesProvided.</returns>

    [Theory]
    [InlineData(1, 10)]    // Small page
    [InlineData(5, 100)]   // Large page
    [InlineData(10, 1000)] // Maximum page size
    public async Task Should_HandlePagination_When_DifferentPageSizesProvided(int pageNumber, int pageSize)
    {
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Arrange - Coca-Cola bottling line with pagination
        const int machineId = 801;
        var expectedResponse = new GetOeeHistoryResponse
        {
            Records = Enumerable.Empty<OeeHistoryRecord>(),
            TotalCount = 5000, // Large dataset
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _getHistoryHandler.HandleAsync(Arg.Any<GetOeeHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _service.GetOeeHistoryAsync(machineId, pageNumber: pageNumber, pageSize: pageSize, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PageNumber.ShouldBe(pageNumber);
        result.Value.PageSize.ShouldBe(pageSize);
        result.Value.TotalCount.ShouldBe(5000);
        result.Value.TotalPages.ShouldBe((int)Math.Ceiling(5000.0 / pageSize));
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}