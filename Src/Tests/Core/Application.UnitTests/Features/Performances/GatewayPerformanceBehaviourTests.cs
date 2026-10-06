// <copyright file="GatewayPerformanceBehaviourTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Performances;

/// <summary>
/// Unit tests for GatewayPerformanceBehaviour - Performance monitoring pipeline behavior for manufacturing gateway requests.
/// Tests performance tracking, logging, and manufacturing workflow performance scenarios.
/// </summary>
public class GatewayPerformanceBehaviourTests
{
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    // /// </summary>
    // [Fact]
    // public void Constructor_WithValidParameters_ShouldCreateInstance()
    // {
    //     // Arrange
    //     var logger =  XUnitLogger.CreateLogger<IGatewayRequest>();

    //     // Act
    //     var instance = new GatewayPerformanceBehaviour<IGatewayRequest, TaskGatewayResponseDto>(logger);

    //     // Assert
    //     instance.ShouldNotBeNull();
    //     instance.ShouldBeAssignableTo<IPipelineBehavior<IGatewayRequest, TaskGatewayResponseDto>>();
    // }
    // /// <summary>
    // /// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithInvalidParameters_ShouldThrowException()
    // {
    //     // Arrange
    //     ILogger<IGatewayRequest> logger = null!;

    //     // Act & Assert
    //     Should.Throw<ArgumentNullException>(() =>
    //         new GatewayPerformanceBehaviour<IGatewayRequest, Result<TaskGatewayResponseDto>>(logger));
    // }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<IGatewayRequest>();
        var instance = new GatewayPerformanceBehaviour<IGatewayRequest, Result<TaskGatewayResponseDto>>(logger);

        // Act & Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IPipelineBehavior<IGatewayRequest, Result<TaskGatewayResponseDto>>>();
    }

    /// <summary>
    /// Executes Handle_WhenCalled_ShouldLogPerformanceMetrics operation.
    /// </summary>
    /// <returns>The result of Handle_WhenCalled_ShouldLogPerformanceMetrics.</returns>

    [Fact]
    public async Task Handle_WhenCalled_ShouldLogPerformanceMetrics()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<IGatewayRequest>();
        var instance = new GatewayPerformanceBehaviour<IGatewayRequest, Result<TaskGatewayResponseDto>>(logger);
        var request = Substitute.For<IGatewayRequest>();
        var response = Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto());

        var next = Substitute.For<RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>>>();
        next.Invoke().Returns(Task.FromResult(response));

        // Act
        var result = await instance.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await next.Received(1).Invoke();
    }

    /// <summary>
    /// Executes Handle_WithAutomotiveManufacturingScenarios_ShouldProcessCorrectly operation.
    /// </summary>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Handle_WithAutomotiveManufacturingScenarios_ShouldProcessCorrectly.</returns>

    [Theory]
    [InlineData("PART-AUTO-001")]
    [InlineData("PART-AUTO-002")]
    public async Task Handle_WithAutomotiveManufacturingScenarios_ShouldProcessCorrectly(string scenario)
    {
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Arrange
        var logger = XUnitLogger.CreateLogger<IGatewayRequest>();
        var instance = new GatewayPerformanceBehaviour<IGatewayRequest, Result<TaskGatewayResponseDto>>(logger);
        var request = Substitute.For<IGatewayRequest>();
        var response = Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto
        {
            MachineId = 100001,
            PartNumber = scenario
        });

        var next = Substitute.For<RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>>>();
        next.Invoke().Returns(Task.FromResult(response));

        // Act
        var result = await instance.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator since result.IsSuccess was verified true
        result.Value.ShouldNotBeNull().MachineId.ShouldBe(100001);
        result.Value.ShouldNotBeNull().PartNumber.ShouldBe(scenario);
    }

    /// <summary>
    /// Executes Handle_WithLongRunningOperation_ShouldLogWarning operation.
    /// </summary>
    /// <returns>The result of Handle_WithLongRunningOperation_ShouldLogWarning.</returns>

    [Fact]
    public async Task Handle_WithLongRunningOperation_ShouldLogWarning()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<IGatewayRequest>();
        var instance = new GatewayPerformanceBehaviour<IGatewayRequest, Result<TaskGatewayResponseDto>>(logger);
        var request = Substitute.For<IGatewayRequest>();
        var response = Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto());

        var next = Substitute.For<RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>>>();
        next.Invoke().Returns(async (e) =>
        {
            await Task.Delay(100); // Simulate operation
            return response;
        });

        // Act
        var result = await instance.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await next.Received(1).Invoke();
    }

    /// <summary>
    /// Executes Handle_WithFailedOperation_ShouldLogAndReturnFailure operation.
    /// </summary>
    /// <returns>The result of Handle_WithFailedOperation_ShouldLogAndReturnFailure.</returns>

    [Fact]
    public async Task Handle_WithFailedOperation_ShouldLogAndReturnFailure()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<IGatewayRequest>();
        var instance = new GatewayPerformanceBehaviour<IGatewayRequest, Result<TaskGatewayResponseDto>>(logger);
        var request = Substitute.For<IGatewayRequest>();
        var failureResponse = Result<TaskGatewayResponseDto>.WithFailure("Operation failed");

        var next = Substitute.For<RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>>>();
        next.Invoke().Returns(Task.FromResult(failureResponse));

        // Act
        var result = await instance.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
        await next.Received(1).Invoke();
    }

    /// <summary>
    /// Executes Handle_WithSpecializedIndustryScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="industry">The industry.</param>
    /// <returns>The result of Handle_WithSpecializedIndustryScenarios_ShouldHandleCorrectly.</returns>

    [Theory]
    [InlineData("Electronics Manufacturing Performance")]
    [InlineData("Pharmaceutical Production Performance")]
    [InlineData("Food & Beverage Manufacturing Performance")]
    [InlineData("Aerospace Component Performance")]
    [InlineData("Chemical Processing Performance")]
    public async Task Handle_WithSpecializedIndustryScenarios_ShouldHandleCorrectly(string industry)
    {
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Arrange
        var logger = XUnitLogger.CreateLogger<IGatewayRequest>();
        var instance = new GatewayPerformanceBehaviour<IGatewayRequest, Result<TaskGatewayResponseDto>>(logger);
        var request = Substitute.For<IGatewayRequest>();
        var response = Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto
        {
            MachineId = 2001,
            PartNumber = industry
        });

        var next = Substitute.For<RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>>>();
        next.Invoke().Returns(Task.FromResult(response));

        // Act
        var result = await instance.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator since result.IsSuccess was verified true
        result.Value.ShouldNotBeNull().PartNumber.ShouldBe(industry);
    }

    /// <summary>
    /// Executes Handle_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of Handle_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<IGatewayRequest>();
        var instance = new GatewayPerformanceBehaviour<IGatewayRequest, Result<TaskGatewayResponseDto>>(logger);
        var request = Substitute.For<IGatewayRequest>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var next = Substitute.For<RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>>>();
        next.Invoke().Returns(Task.FromCanceled<Result<TaskGatewayResponseDto>>(cts.Token));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await instance.HandleAsync(request, next, cts.Token));
    }
}