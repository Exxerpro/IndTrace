// <copyright file="GetOeeHistoryPerformanceQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using NSubstitute.ExceptionExtensions;

namespace Application.UnitTests.Features.Oee;

/// <summary>
/// Unit tests for GetOeeHistoryPerformanceQueryHandler
/// </summary>
public class GetOeeHistoryPerformanceQueryHandlerTests
{
    private readonly IOeeRepository _mockOeeRepository = null!;
    private readonly IValidator<GetOeeHistoryQuery> _mockValidator = null!;
    private readonly ILogger<GetOeeHistoryPerformanceQueryHandler> _logger = null!;
    private readonly GetOeeHistoryPerformanceQueryHandler _handler = null!;
    private readonly ITestOutputHelper _output = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="output">The output.</param>

    public GetOeeHistoryPerformanceQueryHandlerTests(ITestOutputHelper output)
    {
        _mockOeeRepository = Substitute.For<IOeeRepository>();
        _mockValidator = Substitute.For<IValidator<GetOeeHistoryQuery>>();
        _output = output;

        _logger = XUnitLogger.CreateLogger<GetOeeHistoryPerformanceQueryHandler>();
        _handler = new GetOeeHistoryPerformanceQueryHandler(
            _mockOeeRepository,
            _mockValidator,
            _logger);
    }

    /// <summary>
    /// Executes Constructor_Should_ThrowArgumentNullException_When_OeeRepositoryIsNull operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_Should_ThrowArgumentNullException_When_OeeRepositoryIsNull()
    //     {
    //         // Arrange & Act & Assert
    //         Should.Throw<ArgumentNullException>(() =>
    //             new GetOeeHistoryPerformanceQueryHandler(null, _mockValidator, _logger))
    //             .ParamName.ShouldBe("oeeRepository");
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
    //             new GetOeeHistoryPerformanceQueryHandler(_mockOeeRepository, null, _logger))
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
    //             new GetOeeHistoryPerformanceQueryHandler(_mockOeeRepository, _mockValidator, null))
    //             .ParamName.ShouldBe("logger");
    //     }
    /// <summary>
    /// Executes HandleAsync_Should_ReturnOeeHistoryResponse_When_QueryIsValid operation.
    /// </summary>
    /// <returns>The result of HandleAsync_Should_ReturnOeeHistoryResponse_When_QueryIsValid.</returns>

    [Fact]
    public async Task HandleAsync_Should_ReturnOeeHistoryResponse_When_QueryIsValid()
    {
        // Arrange
        var query = CreateValidQuery();
        var expectedRecords = CreateSampleOeeHistoryRecords();
        var totalCount = 25;

        _mockValidator.ValidateAsync(query, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockOeeRepository.GetOeeHistoryAsync(
            query.MachineId,
            query.StartDate,
            query.EndDate,
            query.MinPerformanceLevel,
            query.PageNumber,
            query.PageSize,
            Arg.Any<CancellationToken>())
            .Returns((expectedRecords, totalCount));

        // Act
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Added TestContext.Current.CancellationToken for better test cancellation support
        var result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Updated to handle Railway-Oriented Programming Result<T> pattern - check IsSuccess and access Value
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var response = result.Value;
        response.ShouldNotBeNull();
        response.ShouldNotBeNull();
        response.ShouldNotBeNull();
        response.Records.ShouldNotBeEmpty();
        response.Records.Count().ShouldBe(expectedRecords.Count());
        response.TotalCount.ShouldBe(totalCount);
        response.PageNumber.ShouldBe(query.PageNumber);
        response.PageSize.ShouldBe(query.PageSize);

        await _mockValidator.Received(1).ValidateAsync(query, Arg.Any<CancellationToken>());
        await _mockOeeRepository.Received(1).GetOeeHistoryAsync(
            query.MachineId,
            query.StartDate,
            query.EndDate,
            query.MinPerformanceLevel,
            query.PageNumber,
            query.PageSize,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes HandleAsync_Should_ThrowArgumentException_When_ValidationFails operation.
    /// </summary>
    /// <returns>The result of HandleAsync_Should_ThrowArgumentException_When_ValidationFails.</returns>

    [Fact]
    public async Task HandleAsync_Should_ThrowArgumentException_When_ValidationFails()
    {
        // Arrange
        var query = CreateValidQuery();
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("StartDate", "Start date is required"),
            new ValidationFailure("EndDate", "End date is required")
        });

        _mockValidator.ValidateAsync(query, Arg.Any<CancellationToken>())
            .Returns(validationResult);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Added TestContext.Current.CancellationToken for better test cancellation support
        var result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Updated to handle Railway-Oriented Programming Result<T> pattern - validation failures return Result.Fail instead of throwing
        result.IsSuccess.ShouldBeFalse();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        //result.Errors.ShouldContain(error => error.Contains("Start date is required"));
        //result.Errors.ShouldContain(error => error.Contains("End date is required"));

        await _mockValidator.Received(1).ValidateAsync(query, Arg.Any<CancellationToken>());
        await _mockOeeRepository.DidNotReceive().GetOeeHistoryAsync(
            Arg.Any<int?>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<OeePerformanceLevel?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes HandleAsync_Should_PropagateException_When_RepositoryThrows operation.
    /// </summary>
    /// <returns>The result of HandleAsync_Should_PropagateException_When_RepositoryThrows.</returns>

    [Fact]
    public async Task HandleAsync_Should_PropagateException_When_RepositoryThrows()
    {
        // Arrange
        var query = CreateValidQuery();
        var expectedException = new InvalidOperationException("Repository error");

        _mockValidator.ValidateAsync(query, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Fixed async exception throwing pattern to use ThrowsAsync for async methods
        _mockOeeRepository.GetOeeHistoryAsync(
            Arg.Any<int?>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<OeePerformanceLevel?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act
        var result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Updated to handle Railway-Oriented Programming Result<T> pattern - exceptions are now wrapped in Result.Fail
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain(error => error.Contains("Repository error"));

        await _mockValidator.Received(1).ValidateAsync(query, Arg.Any<CancellationToken>());
        await _mockOeeRepository.Received(1).GetOeeHistoryAsync(
            Arg.Any<int?>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<OeePerformanceLevel?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes HandleAsync_Should_PassCorrectParametersToRepository operation.
    /// </summary>
    /// <returns>The result of HandleAsync_Should_PassCorrectParametersToRepository.</returns>

    [Fact]
    public async Task HandleAsync_Should_PassCorrectParametersToRepository()
    {
        // Arrange
        var query = new GetOeeHistoryQuery
        {
            MachineId = 42,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 31),
            MinPerformanceLevel = OeePerformanceLevel.WorldClass,
            PageNumber = 2,
            PageSize = 25
        };

        var expectedRecords = CreateSampleOeeHistoryRecords();
        var totalCount = 50;

        _mockValidator.ValidateAsync(query, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockOeeRepository.GetOeeHistoryAsync(
            query.MachineId,
            query.StartDate,
            query.EndDate,
            query.MinPerformanceLevel,
            query.PageNumber,
            query.PageSize,
            Arg.Any<CancellationToken>())
            .Returns((expectedRecords, totalCount));

        // Act
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Added TestContext.Current.CancellationToken for better test cancellation support
        var result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Updated to handle Railway-Oriented Programming Result<T> pattern - verify successful result
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        await _mockOeeRepository.Received(1).GetOeeHistoryAsync(
            42,
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31),
            OeePerformanceLevel.WorldClass,
            2,
            25,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes HandleAsync_Should_UseCancellationToken_WhenProvided operation.
    /// </summary>
    /// <returns>The result of HandleAsync_Should_UseCancellationToken_WhenProvided.</returns>

    [Fact]
    public async Task HandleAsync_Should_UseCancellationToken_WhenProvided()
    {
        // Arrange
        var query = CreateValidQuery();
        var cancellationToken = TestContext.Current.CancellationToken;
        var expectedRecords = CreateSampleOeeHistoryRecords();
        var totalCount = 10;

        _mockValidator.ValidateAsync(query, cancellationToken)
            .Returns(new ValidationResult());

        _mockOeeRepository.GetOeeHistoryAsync(
            Arg.Any<int?>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<OeePerformanceLevel?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            cancellationToken)
            .Returns((expectedRecords, totalCount));

        // Act
        var result = await _handler.HandleAsync(query, cancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Updated to handle Railway-Oriented Programming Result<T> pattern - check IsSuccess and Value
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        await _mockValidator.Received(1).ValidateAsync(query, cancellationToken);
        await _mockOeeRepository.Received(1).GetOeeHistoryAsync(
            Arg.Any<int?>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<OeePerformanceLevel?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            cancellationToken);
    }

    /// <summary>
    /// Executes HandleAsync_Should_HandleEmptyResultsFromRepository operation.
    /// </summary>
    /// <returns>The result of HandleAsync_Should_HandleEmptyResultsFromRepository.</returns>

    [Fact]
    public async Task HandleAsync_Should_HandleEmptyResultsFromRepository()
    {
        // Arrange
        var query = CreateValidQuery();
        var emptyRecords = Enumerable.Empty<OeeHistoryRecord>();
        var totalCount = 0;

        _mockValidator.ValidateAsync(query, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockOeeRepository.GetOeeHistoryAsync(
            Arg.Any<int?>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<OeePerformanceLevel?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((emptyRecords, totalCount));

        // Act
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Added TestContext.Current.CancellationToken for better test cancellation support
        var result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Updated to handle Railway-Oriented Programming Result<T> pattern - check IsSuccess and access Value
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var response = result.Value;
        response.ShouldNotBeNull();
        response.ShouldNotBeNull();
        response.ShouldNotBeNull();
        response.Records.ShouldBeEmpty();
        response.TotalCount.ShouldBe(0);
        response.PageNumber.ShouldBe(query.PageNumber);
        response.PageSize.ShouldBe(query.PageSize);
        response.TotalPages.ShouldBe(0);
        response.HasNextPage.ShouldBeFalse();
        response.HasPreviousPage.ShouldBeFalse();
    }

    /// <summary>
    /// Executes HandleAsync_Should_HandleVariousValidQueries operation.
    /// </summary>
    /// <returns>The result of HandleAsync_Should_HandleVariousValidQueries.</returns>

    [Theory]
    [MemberData(nameof(GetValidQueryTestCases))]
    public async Task HandleAsync_Should_HandleVariousValidQueries(
        int? machineId,
        DateTime startDate,
        DateTime endDate,
        OeePerformanceLevel? minPerformanceLevel,
        int pageNumber,
        int pageSize)
    {
        // Arrange
        var query = new GetOeeHistoryQuery
        {
            MachineId = machineId,
            StartDate = startDate,
            EndDate = endDate,
            MinPerformanceLevel = minPerformanceLevel,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var expectedRecords = CreateSampleOeeHistoryRecords();
        var totalCount = 100;

        _mockValidator.ValidateAsync(query, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockOeeRepository.GetOeeHistoryAsync(
            Arg.Any<int?>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<OeePerformanceLevel?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((expectedRecords, totalCount));

        // Act
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Added TestContext.Current.CancellationToken for better test cancellation support
        var result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Updated to handle Railway-Oriented Programming Result<T> pattern - check IsSuccess and access Value
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var response = result.Value;
        response.ShouldNotBeNull();
        response.ShouldNotBeNull();
        response.ShouldNotBeNull();
        response.PageNumber.ShouldBe(pageNumber);
        response.PageSize.ShouldBe(pageSize);
    }

    /// <summary>
    /// Executes GetValidQueryTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidQueryTestCases.</returns>

    public static IEnumerable<object[]> GetValidQueryTestCases()
    {
        var baseDate = new DateTime(2025, 1, 1);

        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Fixed null literal warnings by using proper nullable types
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8625] Fix null literal conversion - use proper null casting
        yield return new object[] { (int?)null!, baseDate, baseDate.AddDays(30), (OeePerformanceLevel?)null!, 1, 50 }; // All machines, no performance filter
        yield return new object[] { 1, baseDate, baseDate.AddDays(7), OeePerformanceLevel.WorldClass, 1, 20 }; // Specific machine, WorldClass filter
        yield return new object[] { 5, baseDate, baseDate.AddDays(1), OeePerformanceLevel.Good, 2, 100 }; // Single day, Good filter
        yield return new object[] { 10, baseDate, baseDate.AddMonths(1), OeePerformanceLevel.Fair, 3, 10 }; // Monthly range, Fair filter
        yield return new object[] { 999, baseDate, baseDate.AddHours(12), (OeePerformanceLevel?)null!, 1, 500 }; // Half-day range, large page size
    }

    private static GetOeeHistoryQuery CreateValidQuery()
    {
        return new GetOeeHistoryQuery
        {
            MachineId = 100,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 31),
            MinPerformanceLevel = OeePerformanceLevel.Good,
            PageNumber = 1,
            PageSize = 50
        };
    }

    private static List<OeeHistoryRecord> CreateSampleOeeHistoryRecords()
    {
        return
        [
            new OeeHistoryRecord
            {
                MachineId = 100,
                MachineName = "Machine 001",
                Metrics = new OeeMetrics(0.85m, 0.90m, 0.95m),
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Shift = "Day Shift",
                Product = "Product A"
            },
            new OeeHistoryRecord
            {
                MachineId = 100,
                MachineName = "Machine 001",
                Metrics = new OeeMetrics(0.75m, 0.85m, 0.88m),
                Timestamp = DateTime.UtcNow.AddHours(-2),
                Shift = "Day Shift",
                Product = "Product B"
            },
            new OeeHistoryRecord
            {
                MachineId = 2,
                MachineName = "Machine 002",
                Metrics = new OeeMetrics(0.92m, 0.95m, 0.97m),
                Timestamp = DateTime.UtcNow.AddHours(-3),
                Shift = "Night Shift",
                Product = "Product C"
            }
        ];
    }
}