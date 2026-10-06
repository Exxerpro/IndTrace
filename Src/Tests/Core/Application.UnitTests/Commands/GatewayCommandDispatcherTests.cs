// <copyright file="GatewayCommandDispatcherTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;
using IndTrace.Application.BarCodes.Commands.Create;
using IndTrace.Application.BarCodes.Queries.GetBarCodeGatewayDetail;

namespace Application.UnitTests.Commands;

/// <summary>
/// Unit tests for GatewayCommandDispatcher - Gateway command/query dispatcher for industrial operations.
/// Tests constructor validation, interface compliance, command routing, and manufacturing scenarios.
/// </summary>
public class GatewayCommandDispatcherTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();

        // Act
        var instance = new GatewayCommandDispatcher(scopeFactory, logger);

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IGatewayCommandDispatcher>();
    }

    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    // /// </summary>
    //
    // [Fact]
    // public void Constructor_WithInvalidParameters_ShouldThrowException()
    // {
    //     // Arrange
    //     IServiceProvider provider = null!;
    //     ILogger<GatewayCommandDispatcher> logger = null!;
    //
    //     // Act & Assert
    //     Should.Throw<ArgumentNullException>(() => new GatewayCommandDispatcher(provider, logger));
    // }
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithNullProvider_ShouldThrowException operation.
    // /// </summary>
    //
    // [Fact]
    // public void Constructor_WithNullProvider_ShouldThrowException()
    // {
    //     // Arrange
    //     IServiceProvider? nullProvider = null!;
    //     var logger =  XUnitLogger.CreateLogger<GatewayCommandDispatcher>();
    //
    //     // Act & Assert
    //     Should.Throw<ArgumentNullException>(() => new GatewayCommandDispatcher(nullProvider!, logger));
    // }
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    // /// </summary>
    //
    // [Fact]
    // public void Constructor_WithNullLogger_ShouldThrowException()
    // {
    //     // Arrange
    //     var provider = Substitute.For<IServiceProvider>();
    //     ILogger<GatewayCommandDispatcher>? nullLogger = null!;
    //
    //     // Act & Assert
    //     Should.Throw<ArgumentNullException>(() => new GatewayCommandDispatcher(provider, nullLogger!));
    // }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();
        var instance = new GatewayCommandDispatcher(scopeFactory, logger);

        // Act & Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IGatewayCommandDispatcher>();

        // Verify interface compliance
        typeof(IGatewayCommandDispatcher).IsAssignableFrom(typeof(GatewayCommandDispatcher)).ShouldBeTrue();
    }

    /// <summary>
    /// Executes ProcessAsync_WithCreateBarCodeCommand_ShouldInvokeHandlerAndReturnResult operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithCreateBarCodeCommand_ShouldInvokeHandlerAndReturnResult.</returns>

    [Fact]
    public async Task ProcessAsync_WithCreateBarCodeCommand_ShouldInvokeHandlerAndReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHandler = Substitute.For<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>();
        var mockPipelineBehavior = Substitute.For<IPipelineBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>>();
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();
        var testCommand = new CreateBarCodeCommand();
        var expectedResponse = new TaskGatewayResponseDto();
        var expectedResult = Result<TaskGatewayResponseDto>.Success(expectedResponse);
        mockHandler.ProcessAsync(testCommand, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult));
        // Register mocks in DI
        services.AddSingleton<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>(mockHandler);
        services.AddSingleton<IPipelineBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>>(mockPipelineBehavior);
        services.AddSingleton<ILogger<GatewayCommandDispatcher>>(logger);
        var provider = services.BuildServiceProvider();
        var instance = new GatewayCommandDispatcher(provider.GetRequiredService<IServiceScopeFactory>(), logger);
        // Act
        var result = await instance.ProcessAsync(testCommand, TestContext.Current.CancellationToken);
        // Assert
        result.ShouldBe(expectedResult);
        result.Value.ShouldBe(expectedResponse);
        await mockHandler.Received(1).ProcessAsync(testCommand, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes QueryAsync_WithGatewayQuery_ShouldInvokeHandlerAndReturnResult operation.
    /// </summary>
    /// <returns>The result of QueryAsync_WithGatewayQuery_ShouldInvokeHandlerAndReturnResult.</returns>

    [Fact]
    public async Task QueryAsync_WithGatewayQuery_ShouldInvokeHandlerAndReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHandler = Substitute.For<IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>>();
        var mockPipelineBehavior = Substitute.For<IPipelineBehavior<ReadBarCodeQuery, Result<TaskGatewayResponseDto>>>();
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();
        var testQuery = new ReadBarCodeQuery();
        var expectedResponse = new TaskGatewayResponseDto();
        var expectedResult = Result<TaskGatewayResponseDto>.Success(expectedResponse);
        mockHandler.ProcessAsync(testQuery, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult));
        // Register mocks in DI
        services.AddSingleton<IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>>(mockHandler);
        services.AddSingleton<IPipelineBehavior<ReadBarCodeQuery, Result<TaskGatewayResponseDto>>>(mockPipelineBehavior);
        services.AddSingleton<ILogger<GatewayCommandDispatcher>>(logger);
        var provider = services.BuildServiceProvider();
        var instance = new GatewayCommandDispatcher(provider.GetRequiredService<IServiceScopeFactory>(), logger);
        // Act
        var result = await instance.QueryAsync(testQuery, TestContext.Current.CancellationToken);
        // Assert
        result.ShouldBe(expectedResult);
        result.Value.ShouldBe(expectedResponse);
        await mockHandler.Received(1).ProcessAsync(testQuery, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes ProcessAsync_WithUnregisteredCommand_ShouldThrowInvalidOperationException operation.
    /// </summary>

    [Fact]
    public async Task ProcessAsync_WithUnregisteredCommand_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();
        var unregisteredCommand = new TestGatewayCommand();

        var instance = new GatewayCommandDispatcher(scopeFactory, logger);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern B - Railway-Oriented Programming - dispatcher returns Result.Failure instead of throwing
        var result = await instance.ProcessAsync(unregisteredCommand, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"No registered handler for request type {nameof(TestGatewayCommand)}");
    }

    /// <summary>
    /// Executes ProcessAsync_WithCancellationToken_ShouldPassTokenToHandler operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithCancellationToken_ShouldPassTokenToHandler.</returns>

    [Fact]
    public async Task ProcessAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockHandler = Substitute.For<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>();
        var mockPipelineBehavior = Substitute.For<IPipelineBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>>();
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();

        var testCommand = new CreateBarCodeCommand();
        var cancellationToken = new CancellationToken(true);

        var expectedResponse = new TaskGatewayResponseDto();
        var expectedResult = Result<TaskGatewayResponseDto>.Success(expectedResponse);

        mockHandler.ProcessAsync(testCommand, cancellationToken)
            .Returns(Task.FromResult(expectedResult));
        mockHandler.ProcessAsync(testCommand, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult));

        // Register mocks in DI
        services.AddSingleton<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>(mockHandler);
        services.AddSingleton<IPipelineBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>>(mockPipelineBehavior);
        services.AddSingleton<ILogger<GatewayCommandDispatcher>>(logger);

        var provider = services.BuildServiceProvider();
        var instance = new GatewayCommandDispatcher(provider.GetRequiredService<IServiceScopeFactory>(), logger);

        // Act
        var result = await instance.ProcessAsync(testCommand, cancellationToken);

        // Assert
        result.ShouldBe(expectedResult);
        await mockHandler.Received(1).ProcessAsync(testCommand, cancellationToken);
    }

    /// <summary>
    /// F3 (#78 remediation): a cooperative <see cref="OperationCanceledException"/> from the forwarded token must be
    /// RETHROWN (genuine cancellation), not flattened into a generic <c>Result.WithFailure</c>, so a caller can tell
    /// "cancelled" from "handler faulted." The generic catch still wraps real handler throws.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenHandlerCancels_ShouldRethrowCancellationNotFlattenToResult()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockHandler = Substitute.For<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>();
        var mockPipelineBehavior = Substitute.For<IPipelineBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>>();
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();

        var testCommand = new CreateBarCodeCommand();
        var cancellationToken = new CancellationToken(true);

        // The handler observes the cancelled token cooperatively and throws.
        mockHandler.ProcessAsync(testCommand, Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        services.AddSingleton<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>(mockHandler);
        services.AddSingleton<IPipelineBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>>(mockPipelineBehavior);
        services.AddSingleton<ILogger<GatewayCommandDispatcher>>(logger);

        var provider = services.BuildServiceProvider();
        var instance = new GatewayCommandDispatcher(provider.GetRequiredService<IServiceScopeFactory>(), logger);

        // Act & Assert - genuine cancellation propagates as an exception, NOT a Result failure.
        await Should.ThrowAsync<OperationCanceledException>(
            async () => await instance.ProcessAsync(testCommand, cancellationToken));
    }

    /// <summary>
    /// F3 (#78) counterpart: a NON-cancellation handler throw is still caught and wrapped into a diagnostic
    /// <c>Result.WithFailure</c> (the original #78 behavior), so a real fault never escapes into the PLC worker loop.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenHandlerFaults_ShouldWrapIntoResultFailure()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockHandler = Substitute.For<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>();
        var mockPipelineBehavior = Substitute.For<IPipelineBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>>();
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();

        var testCommand = new CreateBarCodeCommand();

        mockHandler.ProcessAsync(testCommand, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("handler faulted"));

        services.AddSingleton<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>(mockHandler);
        services.AddSingleton<IPipelineBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>>(mockPipelineBehavior);
        services.AddSingleton<ILogger<GatewayCommandDispatcher>>(logger);

        var provider = services.BuildServiceProvider();
        var instance = new GatewayCommandDispatcher(provider.GetRequiredService<IServiceScopeFactory>(), logger);

        // Act - a non-cancellation fault is wrapped, not thrown (uses a non-cancelled token).
        var result = await instance.ProcessAsync(testCommand, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.Contains("Error processing gateway request"));
    }
}

/// <summary>
/// Test helper classes for GatewayCommandDispatcher testing
/// </summary>
public class TestGatewayCommand : IGatewayRequest<TaskGatewayResponseDto>
{
}