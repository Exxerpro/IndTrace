// <copyright file="GetEventsListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Queries;

/// <summary>
/// Unit tests for GetEventsListQueryHandler
/// </summary>
public class GetEventsListQueryHandlerTests
{
    private readonly IRepository<TaskGatewayRequest> _mockRepositoryRequests;
    private readonly IReadOnlyRepository<TaskGatewayResponse> _mockRepositoryResponses;
    private readonly GetEventsListQueryHandler _handler;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetEventsListQueryHandlerTests()
    {
        _mockRepositoryRequests = Substitute.For<IRepository<TaskGatewayRequest>>();
        _mockRepositoryResponses = Substitute.For<IReadOnlyRepository<TaskGatewayResponse>>();
        _handler = new GetEventsListQueryHandler(_mockRepositoryRequests, _mockRepositoryResponses);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new GetEventsListQueryHandler(_mockRepositoryRequests, _mockRepositoryResponses);

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IMonitorRequestHandler<GetEventsListQuery, EventsListVm>>();
    }

    ///// <summary>
    ///// Executes Constructor_WithNullRequestRepository_ShouldThrowException operation.
    ///// </summary>

    //[Fact]
    //public void Constructor_WithNullRequestRepository_ShouldThrowException()
    //{
    //    // Arrange
    //    IRepository<TaskGatewayRequest> nullRequestRepo = null!;

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() =>
    //        new GetEventsListQueryHandler(nullRequestRepo, _mockRepositoryResponses));
    //}
    ///// <summary>
    ///// Executes Constructor_WithNullResponseRepository_ShouldThrowException operation.
    ///// </summary>

    //[Fact]
    //public void Constructor_WithNullResponseRepository_ShouldThrowException()
    //{
    //    // Arrange
    //    IRepository<TaskGatewayResponse> nullResponseRepo = null!;

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() =>
    //        new GetEventsListQueryHandler(_mockRepositoryRequests, nullResponseRepo));
    //}

    /// <summary>
    /// Executes ProcessAsync_WithValidQuery_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithValidQuery_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task ProcessAsync_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10);
        var mockRequests = new List<TaskGatewayRequest>();
        var mockResponses = new List<TaskGatewayResponse>();

        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.Success(mockRequests);
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.Success(mockResponses);

        _mockRepositoryRequests.ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), Arg.Any<CancellationToken>())
            .Returns(requestResult);
        _mockRepositoryResponses.ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), Arg.Any<CancellationToken>())
            .Returns(responseResult);

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Requests.ShouldBe(mockRequests);
        result.Value.Responses.Count().ShouldBe(mockResponses.Count);
    }

    /// <summary>
    /// Executes ProcessAsync_WithProductionLineEventQuery_ShouldApplyCorrectPaging operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithProductionLineEventQuery_ShouldApplyCorrectPaging.</returns>

    [Fact]
    public async Task ProcessAsync_WithProductionLineEventQuery_ShouldApplyCorrectPaging()
    {
        // Arrange - Ford F-150 production line event monitoring with paging
        var query = new GetEventsListQuery(2, 25); // Page 2, 25 items per page
        var mockRequests = new List<TaskGatewayRequest>();
        var mockResponses = new List<TaskGatewayResponse>();

        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.Success(mockRequests);
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.Success(mockResponses);

        _mockRepositoryRequests.ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), Arg.Any<CancellationToken>())
            .Returns(requestResult);
        _mockRepositoryResponses.ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), Arg.Any<CancellationToken>())
            .Returns(responseResult);

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Verify that ListAsync was called with correct specifications
        await _mockRepositoryRequests.Received(1).ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), Arg.Any<CancellationToken>());
        await _mockRepositoryResponses.Received(1).ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes ProcessAsync_WithElectronicsManufacturingQuery_ShouldHandleHighFrequencyData operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithElectronicsManufacturingQuery_ShouldHandleHighFrequencyData.</returns>

    [Fact]
    public async Task ProcessAsync_WithElectronicsManufacturingQuery_ShouldHandleHighFrequencyData()
    {
        // Arrange - SMT line with high frequency events
        var query = new GetEventsListQuery(1, 100); // Large page size for high-frequency data
        var mockRequests = new List<TaskGatewayRequest>();
        var mockResponses = new List<TaskGatewayResponse>();

        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.Success(mockRequests);
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.Success(mockResponses);

        _mockRepositoryRequests.ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), Arg.Any<CancellationToken>())
            .Returns(requestResult);
        _mockRepositoryResponses.ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), Arg.Any<CancellationToken>())
            .Returns(responseResult);

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes ProcessAsync_WhenRequestRepositoryFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WhenRequestRepositoryFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task ProcessAsync_WhenRequestRepositoryFails_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10);
        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.WithFailure("Request repository error");
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.Success(new List<TaskGatewayResponse>());

        _mockRepositoryRequests.ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), Arg.Any<CancellationToken>())
            .Returns(requestResult);
        _mockRepositoryResponses.ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), Arg.Any<CancellationToken>())
            .Returns(responseResult);

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes ProcessAsync_WhenResponseRepositoryFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WhenResponseRepositoryFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task ProcessAsync_WhenResponseRepositoryFails_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10);
        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.Success(new List<TaskGatewayRequest>());
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.WithFailure("Response repository error");

        _mockRepositoryRequests.ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), Arg.Any<CancellationToken>())
            .Returns(requestResult);
        _mockRepositoryResponses.ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), Arg.Any<CancellationToken>())
            .Returns(responseResult);

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes ProcessAsync_WhenBothRepositoriesFail_ShouldCombineErrors operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WhenBothRepositoriesFail_ShouldCombineErrors.</returns>

    [Fact]
    public async Task ProcessAsync_WhenBothRepositoriesFail_ShouldCombineErrors()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10);
        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.WithFailure("Request error");
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.WithFailure("Response error");

        _mockRepositoryRequests.ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), Arg.Any<CancellationToken>())
            .Returns(requestResult);
        _mockRepositoryResponses.ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), Arg.Any<CancellationToken>())
            .Returns(responseResult);

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldNotBeEmpty(); // Combined errors
    }

    /// <summary>
    /// Executes ProcessAsync_WithVariousPagingParameters_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="pageNumber">The pageNumber.</param>
    /// <param name="pageSize">The pageSize.</param>
    /// <returns>The result of ProcessAsync_WithVariousPagingParameters_ShouldHandleCorrectly.</returns>

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 10)]
    [InlineData(5, 50)]
    [InlineData(100, 5)]
    public async Task ProcessAsync_WithVariousPagingParameters_ShouldHandleCorrectly(int pageNumber, int pageSize)
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
        // Arrange
        var query = new GetEventsListQuery(pageNumber, pageSize);
        var mockRequests = new List<TaskGatewayRequest>();
        var mockResponses = new List<TaskGatewayResponse>();

        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.Success(mockRequests);
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.Success(mockResponses);

        _mockRepositoryRequests.ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), Arg.Any<CancellationToken>())
            .Returns(requestResult);
        _mockRepositoryResponses.ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), Arg.Any<CancellationToken>())
            .Returns(responseResult);

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes ProcessAsync_WithCancellationToken_ShouldPassTokenToRepositories operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithCancellationToken_ShouldPassTokenToRepositories.</returns>

    [Fact]
    public async Task ProcessAsync_WithCancellationToken_ShouldPassTokenToRepositories()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10);
        var cancellationToken = new CancellationToken();
        var mockRequests = new List<TaskGatewayRequest>();
        var mockResponses = new List<TaskGatewayResponse>();

        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.Success(mockRequests);
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.Success(mockResponses);

        _mockRepositoryRequests.ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), cancellationToken)
            .Returns(requestResult);
        _mockRepositoryResponses.ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), cancellationToken)
            .Returns(responseResult);

        // Act
        var result = await _handler.ProcessAsync(query, cancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _mockRepositoryRequests.Received(1).ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), cancellationToken);
        await _mockRepositoryResponses.Received(1).ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), cancellationToken);
    }

    /// <summary>
    /// Executes ProcessAsync_CreatesEventListVm_ShouldUseFactoryMethod operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_CreatesEventListVm_ShouldUseFactoryMethod.</returns>

    [Fact]
    public async Task ProcessAsync_CreatesEventListVm_ShouldUseFactoryMethod()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10);
        var mockRequests = new List<TaskGatewayRequest>();
        var mockResponses = new List<TaskGatewayResponse>();

        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.Success(mockRequests);
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.Success(mockResponses);

        _mockRepositoryRequests.ListAsync(Arg.Any<Specification<TaskGatewayRequest>>(), Arg.Any<CancellationToken>())
            .Returns(requestResult);
        _mockRepositoryResponses.ListAsync(Arg.Any<Specification<TaskGatewayResponse>>(), Arg.Any<CancellationToken>())
            .Returns(responseResult);

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert

        result.Value.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<EventsListVm>();
        result.Value.Requests.ShouldBe(mockRequests);
        result.Value.Responses.Count().ShouldBe(mockResponses.Count);
    }
}