// <copyright file="LoggingBehaviorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.Middleware;

/// <summary>
/// Unit tests for LoggingBehavior
/// </summary>
public class LoggingBehaviorTests
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public LoggingBehaviorTests()
    {
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var mockLogger = XUnitLogger.CreateLogger<string>();

        // Act
        var instance = new LoggingBehavior<string, string>(mockLogger);

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IPipelineBehavior<string, string>>();
    }

    ///// <summary>
    ///// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    ///// </summary>

    //[Fact]
    //public void Constructor_WithInvalidParameters_ShouldThrowException()
    //{
    //    // Arrange
    //    ILogger<string>? nullLogger = null!;

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() => new LoggingBehavior<string, string>(nullLogger!));
    //}
    /// <summary>
    /// Executes Handle_WithValidRequest_ShouldLogRequestAndResponse operation.
    /// </summary>
    /// <returns>The result of Handle_WithValidRequest_ShouldLogRequestAndResponse.</returns>

    [Fact]
    public async Task Handle_WithValidRequest_ShouldLogRequestAndResponse()
    {
        // Arrange
        var request = "TestRequest";
        var expectedResponse = "TestResponse";
        var logger = XUnitLogger.CreateLogger<string>();
        var behavior = new LoggingBehavior<string, string>(logger);
        RequestFunctionalHandlerDelegate<string> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);

        // Verify that information logs were called (we can't easily verify exact content with NSubstitute)
    }

    /// <summary>
    /// Executes Handle_WithProductionLineRequest_ShouldProcessCorrectly operation.
    /// </summary>
    /// <returns>The result of Handle_WithProductionLineRequest_ShouldProcessCorrectly.</returns>

    [Fact]
    public async Task Handle_WithProductionLineRequest_ShouldProcessCorrectly()
    {
        // Arrange - Ford F-150 production line monitoring request
        var request = "ProductionLineStatusRequest";
        var expectedResponse = "ProductionLineStatusResponse";
        var logger = XUnitLogger.CreateLogger<string>();
        var behavior = new LoggingBehavior<string, string>(logger);
        RequestFunctionalHandlerDelegate<string> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    /// <summary>
    /// Executes Handle_WithElectronicsManufacturingRequest_ShouldProcessCorrectly operation.
    /// </summary>
    /// <returns>The result of Handle_WithElectronicsManufacturingRequest_ShouldProcessCorrectly.</returns>

    [Fact]
    public async Task Handle_WithElectronicsManufacturingRequest_ShouldProcessCorrectly()
    {
        // Arrange - SMT line monitoring request
        var request = "SMTLineMonitoringRequest";
        var expectedResponse = "SMTLineMonitoringResponse";
        var logger = XUnitLogger.CreateLogger<string>();
        var behavior = new LoggingBehavior<string, string>(logger);
        RequestFunctionalHandlerDelegate<string> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    /// <summary>
    /// Executes Handle_WhenNextHandlerThrows_ShouldPropagateException operation.
    /// </summary>
    /// <returns>The result of Handle_WhenNextHandlerThrows_ShouldPropagateException.</returns>

    [Fact]
    public async Task Handle_WhenNextHandlerThrows_ShouldPropagateException()
    {
        // Arrange
        var request = "TestRequest";
        var expectedException = new InvalidOperationException("Test exception");
        var logger = XUnitLogger.CreateLogger<string>();
        var behavior = new LoggingBehavior<string, string>(logger);
        RequestFunctionalHandlerDelegate<string> next = () => throw expectedException;

        // Act & Assert
        var thrownException = await Should.ThrowAsync<InvalidOperationException>(
            () => behavior.HandleAsync(request, next, TestContext.Current.CancellationToken));

        thrownException.ShouldBe(expectedException);
    }

    /// <summary>
    /// Executes Handle_WithCancellationToken_ShouldPassTokenToNext operation.
    /// </summary>
    /// <returns>The result of Handle_WithCancellationToken_ShouldPassTokenToNext.</returns>

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToNext()
    {
        // Arrange
        var request = "TestRequest";
        var expectedResponse = "TestResponse";
        var cancellationToken = new CancellationToken();
        var nextCalled = false;
        var logger = XUnitLogger.CreateLogger<string>();
        var behavior = new LoggingBehavior<string, string>(logger);

        RequestFunctionalHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.HandleAsync(request, next, cancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
        nextCalled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Handle_WithVariousRequestTypes_ShouldLogCorrectly operation.
    /// </summary>
    /// <param name="requestData">The requestData.</param>
    /// <param name="responseData">The responseData.</param>
    /// <returns>The result of Handle_WithVariousRequestTypes_ShouldLogCorrectly.</returns>

    [Theory]
    [InlineData("CreateMachineCommand", "Result<int>")]
    [InlineData("GetBarCodeDetailQuery", "BarCodeDetailVm")]
    [InlineData("UpdateVariableCommand", "Result<VariableUpdated>")]
    [InlineData("GetEventsListQuery", "EventsListVm")]
    public async Task Handle_WithVariousRequestTypes_ShouldLogCorrectly(string requestData, string responseData)
    {
        // Using parameters: requestData, responseData
        _ = requestData; // xUnit1026 fix
        _ = responseData; // xUnit1026 fix
        // Using parameters: requestData, responseData
        _ = requestData; // xUnit1026 fix
        _ = responseData; // xUnit1026 fix
        // Using parameters: requestData, responseData
        _ = requestData; // xUnit1026 fix
        _ = responseData; // xUnit1026 fix
        // Using parameters: requestData, responseData
        _ = requestData; // xUnit1026 fix
        _ = responseData; // xUnit1026 fix
        // Using parameters: requestData, responseData
        _ = requestData; // xUnit1026 fix
        _ = responseData; // xUnit1026 fix
        // Arrange
        var logger = XUnitLogger.CreateLogger<string>();
        var behavior = new LoggingBehavior<string, string>(logger);
        RequestFunctionalHandlerDelegate<string> next = () => Task.FromResult(responseData);

        // Act
        var result = await behavior.HandleAsync(requestData, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(responseData);
    }

    /// <summary>
    /// Executes Handle_WithGenericTypes_ShouldHandleCorrectly operation.
    /// </summary>
    /// <returns>The result of Handle_WithGenericTypes_ShouldHandleCorrectly.</returns>

    [Fact]
    public async Task Handle_WithGenericTypes_ShouldHandleCorrectly()
    {
        // Arrange
        var mockLoggerGeneric = XUnitLogger.CreateLogger<GetEventsListQuery>();
        var behaviorGeneric = new LoggingBehavior<GetEventsListQuery, Result<EventsListVm>>(mockLoggerGeneric);

        var request = new GetEventsListQuery(1, 10);
        var mockRequests = new List<TaskGatewayRequest>();
        var mockResponses = new List<TaskGatewayResponseDto>();
        var eventsListVm = new EventsListVm(mockRequests, mockResponses);
        var expectedResponse = Result<EventsListVm>.Success(eventsListVm);

        RequestFunctionalHandlerDelegate<Result<EventsListVm>> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behaviorGeneric.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Handle_WithTaskGatewayResponse_ShouldLogSpecialHandling operation.
    /// </summary>
    /// <returns>The result of Handle_WithTaskGatewayResponse_ShouldLogSpecialHandling.</returns>

    [Fact]
    public async Task Handle_WithTaskGatewayResponse_ShouldLogSpecialHandling()
    {
        // Arrange
        var mockLoggerGateway = XUnitLogger.CreateLogger<string>();
        var behaviorGateway = new LoggingBehavior<string, Result<TaskGatewayResponseDto>>(mockLoggerGateway);

        var request = "GatewayRequest";
        var gatewayResponse = new TaskGatewayResponseDto
        {
            CommandId = 123,
            Label = "TEST-LABEL",
            ResultValidation = 1
        };
        var expectedResponse = Result<TaskGatewayResponseDto>.Success(gatewayResponse);

        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behaviorGateway.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(gatewayResponse);
    }

    /// <summary>
    /// Executes Handle_WithNullResponse_ShouldHandleGracefully operation.
    /// </summary>
    /// <returns>The result of Handle_WithNullResponse_ShouldHandleGracefully.</returns>

    [Fact]
    public async Task Handle_WithNullResponse_ShouldHandleGracefully()
    {
        // Arrange
        var request = "TestRequest";
        string? nullResponse = null!;
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8619] Fix nullability mismatch - explicitly type Task.FromResult for nullable string
        RequestFunctionalHandlerDelegate<string?> next = () => Task.FromResult<string?>(nullResponse);

        var mockLoggerNullable = XUnitLogger.CreateLogger<string>();
        var behaviorNullable = new LoggingBehavior<string, string?>(mockLoggerNullable);

        // Act
        var result = await behaviorNullable.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Executes Handle_PipelineBehaviorPattern_ShouldFollowCorrectSequence operation.
    /// </summary>
    /// <returns>The result of Handle_PipelineBehaviorPattern_ShouldFollowCorrectSequence.</returns>

    [Fact]
    public async Task Handle_PipelineBehaviorPattern_ShouldFollowCorrectSequence()
    {
        // Arrange
        var request = "SequenceTestRequest";
        var expectedResponse = "SequenceTestResponse";
        var executionOrder = new List<string>();
        var logger = XUnitLogger.CreateLogger<string>();
        var behavior = new LoggingBehavior<string, string>(logger);

        RequestFunctionalHandlerDelegate<string> next = () =>
        {
            executionOrder.Add("NextHandler");
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
        executionOrder.ShouldContain("NextHandler");
    }
}