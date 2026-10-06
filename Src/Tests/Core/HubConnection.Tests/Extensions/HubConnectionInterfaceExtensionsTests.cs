// <copyright file="HubConnectionInterfaceExtensionsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Extensions;

using HubConnection.Tests.TestDoubles;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

/// <summary>
/// I²TDD tests for HubConnectionInterfaceExtensions following industrial safety standards.
/// Tests cover connection validation, resilience patterns, and thread safety.
/// Based on migration patterns from docs/HubInventory.md
/// </summary>
// These tests share HubConnectionInterfaceExtensions' STATIC connection cache (and call
// ClearConnectionCache, which wipes it for everyone), so classes touching that cache must
// never run in parallel with each other: same collection => serialized by xUnit.
[Collection("HubConnectionStaticCache")]
public class HubConnectionInterfaceExtensionsTests
{
    private readonly DateTimeMachine dateTimeMachine = new();

    /// <summary>
    /// Contract test ensuring EnsureHubConnectionIsValid creates and starts connection when null.
    /// RED test from HubInventory.md documentation.
    /// </summary>
    [Fact]
    public async Task EnsureHubConnectionIsValid_Creates_And_Starts_When_Disconnected()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        IHubConnection? hubConnection = null;

        // Act
        var result = await hubConnection.EnsureHubConnectionIsValid(factory, logger, cancellationToken);

        // Assert - Following I²TDD contract verification
        result.ShouldNotBeNull();
        result!.State.ShouldBe(HubConnectionState.Connected);
        result.ConnectionId.ShouldNotBeNullOrEmpty();

        // Verify factory called exactly once
        factory.CreateCallCount.ShouldBe(1);
        factory.CreatedConnections.Count.ShouldBe(1);

        // Verify logging behavior for industrial traceability
        logger.HasLogLevel(LogLevel.Warning).ShouldBeTrue(); // "IHubConnection is null"
        logger.HasLogLevel(LogLevel.Information).ShouldBeTrue(); // "Initializing IHubConnection..."
        logger.HasMessage("IHubConnection is null").ShouldBeTrue();
        logger.HasMessage("Initializing IHubConnection").ShouldBeTrue();
    }

    /// <summary>
    /// Tests EnsureHubConnectionIsValid starts disconnected connection.
    /// Covers the production pattern from GatewayWorker.cs usage.
    /// </summary>
    [Fact]
    public async Task EnsureHubConnectionIsValid_Should_Start_Disconnected_Connection()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Disconnected
        };

        // Act
        var result = await hubConnection.EnsureHubConnectionIsValid(factory, logger, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeSameAs(hubConnection);
        result.State.ShouldBe(HubConnectionState.Connected);
        result.ConnectionId.ShouldNotBeNullOrEmpty();

        // Should not create new connection
        factory.CreateCallCount.ShouldBe(0);

        // Verify warning and start attempt logged
        logger.HasMessage("IHubConnection is not connected").ShouldBeTrue();
        logger.HasMessage("Attempting to start IHubConnection").ShouldBeTrue();
    }

    /// <summary>
    /// Tests EnsureHubConnectionIsValid preserves already connected state.
    /// Optimization pattern from production usage.
    /// </summary>
    [Fact]
    public async Task EnsureHubConnectionIsValid_Should_Preserve_Connected_State()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var originalConnectionId = "existing-connection-123";
        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = originalConnectionId
        };

        // Act
        var result = await hubConnection.EnsureHubConnectionIsValid(factory, logger, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeSameAs(hubConnection);
        result.State.ShouldBe(HubConnectionState.Connected);
        result.ConnectionId.ShouldBe(originalConnectionId); // Unchanged

        // Should not create new connection or attempt start
        factory.CreateCallCount.ShouldBe(0);

        // Should not log warnings for connected state
        logger.HasLogLevel(LogLevel.Warning).ShouldBeFalse();
    }

    /// <summary>
    /// Tests EnsureHubConnectionIsValid handles factory exceptions gracefully.
    /// Critical resilience pattern for industrial environments.
    /// </summary>
    [Fact]
    public async Task EnsureHubConnectionIsValid_Should_Handle_Factory_Exception()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory()
            .WithException(new InvalidOperationException("Factory failure"));
        var cancellationToken = TestContext.Current.CancellationToken;

        IHubConnection? hubConnection = null;

        // Act
        var result = await hubConnection.EnsureHubConnectionIsValid(factory, logger, cancellationToken);

        // Assert - Graceful degradation
        result.ShouldBeNull();
        factory.CreateCallCount.ShouldBe(1);

        // Verify error logging for industrial traceability
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Error establishing SignalR connection").ShouldBeTrue();
    }

    /// <summary>
    /// Tests TryStartHubConnectionAsync on already connected hub.
    /// Performance optimization from HubMonitorWorker.cs pattern.
    /// </summary>
    [Fact]
    public async Task TryStartHubConnectionAsync_Should_Skip_Already_Connected()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var originalConnectionId = "already-connected-456";
        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = originalConnectionId
        };

        // Act
        var result = await hubConnection.TryStartHubConnectionAsync(logger, cancellationToken);

        // Assert
        result.ShouldBeSameAs(hubConnection);
        result.State.ShouldBe(HubConnectionState.Connected);
        result.ConnectionId.ShouldBe(originalConnectionId);

        // Verify optimization logging
        logger.HasMessage("Hub already connected").ShouldBeTrue();
        logger.HasLogLevel(LogLevel.Information).ShouldBeTrue();
    }

    /// <summary>
    /// Tests TryStartHubConnectionAsync successfully starts disconnected hub.
    /// Primary success path from production usage.
    /// </summary>
    [Fact]
    public async Task TryStartHubConnectionAsync_Should_Start_Disconnected_Hub()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Disconnected
        };

        // Act
        var result = await hubConnection.TryStartHubConnectionAsync(logger, cancellationToken);

        // Assert
        result.ShouldBeSameAs(hubConnection);
        result.State.ShouldBe(HubConnectionState.Connected);
        result.ConnectionId.ShouldNotBeNullOrEmpty();

        // Verify success logging
        logger.HasMessage("Attempting to start IHubConnection").ShouldBeTrue();
        logger.HasMessage("Hub connected successfully").ShouldBeTrue();
    }

    /// <summary>
    /// Tests TryStartHubConnectionAsync handles start exceptions with retry scheduling.
    /// Critical resilience behavior for manufacturing environments.
    /// </summary>
    [Fact]
    public async Task TryStartHubConnectionAsync_Should_Handle_Start_Exception_With_Retry()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var failingConnection = new FailingTestHubConnection
        {
            State = HubConnectionState.Disconnected,
            ShouldFailOnStart = true,
            StartException = new InvalidOperationException("Network failure")
        };

        // Act
        var result = await failingConnection.TryStartHubConnectionAsync(logger, cancellationToken);

        // Assert - Connection returned despite failure (for retry handling)
        result.ShouldBeSameAs(failingConnection);
        result.State.ShouldBe(HubConnectionState.Disconnected); // Remains disconnected after failure

        // Verify error logging and retry scheduling
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Error starting IHubConnection").ShouldBeTrue();
        logger.HasMessage("Scheduling reconnect attempt").ShouldBeTrue();
    }

    /// <summary>
    /// Tests TryStartHubConnectionAsync ignores non-disconnected states.
    /// Edge case handling from production scenarios.
    /// </summary>
    [Theory]
    [InlineData(HubConnectionState.Connecting)]
    [InlineData(HubConnectionState.Reconnecting)]
    public async Task TryStartHubConnectionAsync_Should_Ignore_NonDisconnected_States(HubConnectionState state)
    {
        // Arrange
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = state
        };

        // Act
        var result = await hubConnection.TryStartHubConnectionAsync(logger, cancellationToken);

        // Assert
        result.ShouldBeSameAs(hubConnection);
        result.State.ShouldBe(state); // Unchanged

        // Should not attempt to start
        logger.HasMessage("Attempting to start IHubConnection").ShouldBeTrue();
        logger.HasMessage("Hub connected successfully").ShouldBeFalse();
    }

    /// <summary>
    /// Tests concurrent access to EnsureHubConnectionIsValid using semaphore protection.
    /// Thread safety critical for industrial multi-threaded environments.
    /// </summary>
    [Fact]
    public async Task EnsureHubConnectionIsValid_Should_Handle_Concurrent_Access_Safely()
    {
        // Arrange
        HubConnectionInterfaceExtensions.ClearConnectionCache(); // Ensure clean state for test
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        IHubConnection? hubConnection = null;

        // Act - Simulate 10 concurrent calls
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(async () =>
                await hubConnection.EnsureHubConnectionIsValid(factory, logger, cancellationToken),
                cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - Semaphore should prevent race conditions
        results.All(r => r is not null).ShouldBeTrue();

        // Only one connection should be created despite concurrent access
        factory.CreateCallCount.ShouldBe(1);
        factory.CreatedConnections.Count.ShouldBe(1);

        // All results should reference the same connection instance
        var firstConnection = results.First();
        results.All(r => ReferenceEquals(r, firstConnection)).ShouldBeTrue();
    }

    /// <summary>
    /// Tests cancellation handling in EnsureHubConnectionIsValid.
    /// Industrial safety requirement for controlled shutdown.
    /// </summary>
    [Fact]
    public async Task EnsureHubConnectionIsValid_Should_Respect_Cancellation()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory()
            .WithCreationDelay(TimeSpan.FromSeconds(1)); // Slow factory for cancellation test

        using var cts = new CancellationTokenSource();
        IHubConnection? hubConnection = null;

        // Act & Assert
        var task = hubConnection.EnsureHubConnectionIsValid(factory, logger, cts.Token);

        // Cancel after a short delay
        await Task.Delay(10, TestContext.Current.CancellationToken);
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() => task);
    }

    // --- Tests for newly ported extension methods ---

    /// <summary>
    /// Tests TryInvokeAsync with connected hub - success path.
    /// Covers the primary usage pattern from GatewayExecutor.cs.
    /// </summary>
    [Fact]
    public async Task TryInvokeAsync_Should_Invoke_Method_When_Connected()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection"
        };

        // Act
        var result = await hubConnection.TryInvokeAsync("TestMethod", "arg1", "arg2", factory, logger, cancellationToken);

        // Assert
        result.ShouldBeTrue();
        hubConnection.WasMessageSent("TestMethod").ShouldBeTrue();
        hubConnection.GetLastSentMessageArg<string>("TestMethod", 0).ShouldBe("arg1");
        hubConnection.GetLastSentMessageArg<string>("TestMethod", 1).ShouldBe("arg2");

        // Should not create new connection
        factory.CreateCallCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests TryInvokeAsync with disconnected hub - ensures connection before invoke.
    /// Tests the resilience pattern from production usage.
    /// </summary>
    [Fact]
    public async Task TryInvokeAsync_Should_Ensure_Connection_When_Disconnected()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Disconnected
        };

        // Act
        var result = await hubConnection.TryInvokeAsync("TestMethod", "arg1", "arg2", factory, logger, cancellationToken);

        // Assert
        result.ShouldBeTrue();
        hubConnection.State.ShouldBe(HubConnectionState.Connected); // Should be started
        hubConnection.WasMessageSent("TestMethod").ShouldBeTrue();

        // Verify connection validation logging
        logger.HasMessage("Attempting to start IHubConnection").ShouldBeTrue();
    }

    /// <summary>
    /// Tests TryInvokeAsync handles exceptions gracefully.
    /// Critical resilience behavior for manufacturing environments.
    /// </summary>
    [Fact]
    public async Task TryInvokeAsync_Should_Return_False_On_Exception()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var failingConnection = new FailingTestHubConnection
        {
            State = HubConnectionState.Connected,
            ShouldFailOnInvoke = true,
            InvokeException = new InvalidOperationException("Network failure")
        };

        // Act
        var result = await failingConnection.TryInvokeAsync("TestMethod", "arg1", "arg2", factory, logger, cancellationToken);

        // Assert
        result.ShouldBeFalse();
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Error invoking SignalR method TestMethod").ShouldBeTrue();
    }

    /// <summary>
    /// Tests PublishCommandToHubAsync with valid request.
    /// Covers the primary pattern from GatewayExecutor.cs for command publishing.
    /// </summary>
    [Fact]
    public async Task PublishCommandToHubAsync_Should_Publish_Valid_Request()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection"
        };

        var request = new TaskGatewayRequest
        {
            MachineId = 1001,
            PartNumber = "TEST-PART-001",
            BarCode = "BC123456"
        };

        // Act
        var publishResult = await hubConnection.PublishCommandToHubAsync(request, factory, dateTimeMachine, logger, cancellationToken);

        // Assert
        publishResult.IsSuccess.ShouldBeTrue();
        hubConnection.WasMessageSent("BroadcastTaskGatewayRequest").ShouldBeTrue();
        hubConnection.GetLastSentMessageArg<int>("BroadcastTaskGatewayRequest", 0).ShouldBe(1001); // MachineId
        hubConnection.GetLastSentMessageArg<TaskGatewayRequest>("BroadcastTaskGatewayRequest", 1).ShouldNotBeNull();

        // Verify timestamp was set
        request.TimeStamp.ShouldBeGreaterThan(DateTime.MinValue);
    }

    /// <summary>
    /// Tests PublishCommandToHubAsync handles null connection gracefully.
    /// Edge case handling for optional connection scenarios.
    /// </summary>
    [Fact]
    public async Task PublishCommandToHubAsync_Should_Handle_Null_Connection()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        IHubConnection? hubConnection = null;
        var request = new TaskGatewayRequest
        {
            MachineId = 1001,
            PartNumber = "TEST-PART-001"
        };

        // Act & Assert - Should not throw
        await Should.NotThrowAsync(async () =>
            await hubConnection.PublishCommandToHubAsync(request, factory, dateTimeMachine, logger, cancellationToken));

        // Verify timestamp still set
        request.TimeStamp.ShouldBeGreaterThan(DateTime.MinValue);
    }

    /// <summary>
    /// Tests PublishResultsToHubAsync with successful result.
    /// Covers success path from GatewayExecutor.cs result publishing.
    /// </summary>
    [Fact]
    public async Task PublishResultsToHubAsync_Should_Publish_Successful_Result()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection"
        };

        var request = new TaskGatewayRequest { MachineId = 1001, PartNumber = "TEST-PART" };
        var response = new TaskGatewayResponseDto { MachineId = 1001, PartNumber = "TEST-PART" };
        var result = Result<TaskGatewayResponseDto>.Success(response);

        // Act
        var publishResult = await hubConnection.PublishResultsToHubAsync(request, result, factory, dateTimeMachine, logger, cancellationToken);

        // Assert
        publishResult.IsSuccess.ShouldBeTrue();
        hubConnection.WasMessageSent("BroadcastTaskGatewayResponse").ShouldBeTrue();
        hubConnection.GetLastSentMessageArg<int>("BroadcastTaskGatewayResponse", 0).ShouldBe(1001);

        // Verify success logging
        logger.HasLogLevel(LogLevel.Information).ShouldBeTrue();
        logger.HasMessage("Result").ShouldBeTrue();

        // Should not send error messages
        hubConnection.WasMessageSent("BroadcastMessageToClients").ShouldBeFalse();
    }

    /// <summary>
    /// Tests PublishResultsToHubAsync with failed result and error broadcasting.
    /// Critical error handling pattern for industrial traceability.
    /// </summary>
    [Fact]
    public async Task PublishResultsToHubAsync_Should_Publish_Failed_Result_With_Errors()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection"
        };

        var request = new TaskGatewayRequest { MachineId = 1001, PartNumber = "TEST-PART" };
        var response = new TaskGatewayResponseDto { MachineId = 1001, PartNumber = "TEST-PART" };
        var errors = new[] { "Validation failed", "Machine offline" };
        var result = Result<TaskGatewayResponseDto>.WithFailure(errors, response);

        // Act
        await hubConnection.PublishResultsToHubAsync(request, result, factory, dateTimeMachine, logger, cancellationToken);

        // Assert
        hubConnection.WasMessageSent("BroadcastTaskGatewayResponse").ShouldBeTrue();
        hubConnection.GetMessageCount("BroadcastMessageToClients").ShouldBe(2); // One per error

        // Verify error logging
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Request Failed").ShouldBeTrue();

        // Verify individual error messages sent
        hubConnection.WasMessageSent("BroadcastMessageToClients").ShouldBeTrue();
    }

    /// <summary>
    /// Tests PublishResultsToHubAsync with null value failure.
    /// Edge case handling for complete operation failures.
    /// </summary>
    [Fact]
    public async Task PublishResultsToHubAsync_Should_Handle_Null_Value_Failure()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection"
        };

        var request = new TaskGatewayRequest { MachineId = 1001, PartNumber = "TEST-PART" };
        var errors = new[] { "Critical system failure" };
        var result = Result<TaskGatewayResponseDto>.WithFailure(errors, null);

        // Act
        await hubConnection.PublishResultsToHubAsync(request, result, factory, dateTimeMachine, logger, cancellationToken);

        // Assert
        hubConnection.WasMessageSent("BroadcastTaskGatewayResponse").ShouldBeTrue();
        hubConnection.WasMessageSent("BroadcastMessageToClients").ShouldBeTrue();

        // Should create response from request
        var sentResponse = hubConnection.GetLastSentMessageArg<TaskGatewayResponseDto>("BroadcastTaskGatewayResponse", 1);
        sentResponse.ShouldNotBeNull();
        sentResponse.Error.ShouldBe("Request Failed");
    }

    /// <summary>
    /// Tests LogAndSendMessageFromControllerAsync functionality.
    /// Covers the controller messaging pattern from production usage.
    /// </summary>
    [Fact]
    public async Task LogAndSendMessageFromControllerAsync_Should_Log_And_Send_Message()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection"
        };

        var message = "Controller operation completed successfully";

        // Act
        await hubConnection.LogAndSendMessageFromControllerAsync(message, logger, factory, cancellationToken);

        // Assert
        // Verify logging
        logger.HasLogLevel(LogLevel.Information).ShouldBeTrue();
        logger.HasMessage(message).ShouldBeTrue();

        // Verify hub message
        hubConnection.WasMessageSent("BroadcastMessageToClients").ShouldBeTrue();
        hubConnection.GetLastSentMessageArg<string>("BroadcastMessageToClients", 0).ShouldBe("Gateway");
        hubConnection.GetLastSentMessageArg<string>("BroadcastMessageToClients", 1).ShouldBe(message);
    }

    /// <summary>
    /// Tests all extension methods respect cancellation tokens.
    /// Industrial safety requirement for controlled shutdown scenarios.
    /// </summary>
    [Theory]
    [InlineData("TryInvokeAsync")]
    [InlineData("PublishCommandToHubAsync")]
    [InlineData("PublishResultsToHubAsync")]
    [InlineData("LogAndSendMessageFromControllerAsync")]
    public async Task Extension_Methods_Should_Respect_Cancellation(string methodName)
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        using var cts = new CancellationTokenSource();

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected
        };

        // Cancel immediately to test cancellation handling
        cts.Cancel();

        // Act & Assert
        switch (methodName)
        {
            case "TryInvokeAsync":
                await Should.ThrowAsync<OperationCanceledException>(() =>
                    hubConnection.TryInvokeAsync("TestMethod", "arg1", "arg2", factory, logger, cts.Token));
                break;

            case "PublishCommandToHubAsync":
                await Should.ThrowAsync<OperationCanceledException>(() =>
                    hubConnection.PublishCommandToHubAsync(new TaskGatewayRequest(), factory, dateTimeMachine, logger, cts.Token));
                break;

            case "PublishResultsToHubAsync":
                await Should.ThrowAsync<OperationCanceledException>(() =>
                    hubConnection.PublishResultsToHubAsync(new TaskGatewayRequest(),
                        Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto()), factory, dateTimeMachine, logger, cts.Token));
                break;

            case "LogAndSendMessageFromControllerAsync":
                await Should.ThrowAsync<OperationCanceledException>(() =>
                    hubConnection.LogAndSendMessageFromControllerAsync("test", logger, factory, cts.Token));
                break;
        }
    }

    // --- P0-3 (#63) regression tests: a failed broadcast must NOT be reported as delivered ---

    /// <summary>
    /// Regression for #63: TryInvokeAsync used to `return true` on ANY exception for the broadcast
    /// method names ("for integration tests"). A failed send of a broadcast method must return false.
    /// </summary>
    [Theory]
    [InlineData("BroadcastTaskGatewayResponse")]
    [InlineData("BroadcastTaskGatewayRequest")]
    [InlineData("BroadcastMessageToClients")]
    [InlineData("Echo")]
    public async Task TryInvokeAsync_Should_Return_False_For_Broadcast_Methods_On_Exception(string methodName)
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var failingConnection = new FailingTestHubConnection
        {
            State = HubConnectionState.Connected,
            ShouldFailOnInvoke = true,
            InvokeException = new InvalidOperationException("Send failed"),
        };

        // Act
        var result = await failingConnection.TryInvokeAsync(methodName, "arg1", "arg2", factory, logger, cancellationToken);

        // Assert - failed publish is NOT reported as delivered
        result.ShouldBeFalse();
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
    }

    /// <summary>
    /// Regression for #63: a failed hub send must surface as a failed Result from the publish path.
    /// </summary>
    [Fact]
    public async Task PublishCommandToHubAsync_Should_Report_Failure_When_Broadcast_Fails()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var failingConnection = new FailingTestHubConnection
        {
            State = HubConnectionState.Connected,
            ShouldFailOnInvoke = true,
            InvokeException = new InvalidOperationException("Send failed"),
        };

        var request = new TaskGatewayRequest { MachineId = 1001, PartNumber = "PART" };

        // Act
        var result = await failingConnection.PublishCommandToHubAsync(request, factory, dateTimeMachine, logger, cancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// Regression for #63: a failed response broadcast must surface as a failed Result.
    /// </summary>
    [Fact]
    public async Task PublishResultsToHubAsync_Should_Report_Failure_When_Broadcast_Fails()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var failingConnection = new FailingTestHubConnection
        {
            State = HubConnectionState.Connected,
            ShouldFailOnInvoke = true,
            InvokeException = new InvalidOperationException("Send failed"),
        };

        var request = new TaskGatewayRequest { MachineId = 1001, PartNumber = "PART" };
        var response = new TaskGatewayResponseDto { MachineId = 1001, PartNumber = "PART" };
        var result = Result<TaskGatewayResponseDto>.Success(response);

        // Act
        var publishResult = await failingConnection.PublishResultsToHubAsync(request, result, factory, dateTimeMachine, logger, cancellationToken);

        // Assert
        publishResult.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// Regression for #63: a failed controller-message broadcast must surface as a failed Result.
    /// </summary>
    [Fact]
    public async Task LogAndSendMessageFromControllerAsync_Should_Report_Failure_When_Broadcast_Fails()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var failingConnection = new FailingTestHubConnection
        {
            State = HubConnectionState.Connected,
            ShouldFailOnInvoke = true,
            InvokeException = new InvalidOperationException("Send failed"),
        };

        // Act
        var result = await failingConnection.LogAndSendMessageFromControllerAsync("hello", logger, factory, cancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// Regression for #63: wire-DTO timestamps must come from the injected IDateTimeMachine, not
    /// DateTime.Now. Proven with a substituted machine returning a fixed instant.
    /// </summary>
    [Fact]
    public async Task PublishCommandToHubAsync_Should_Stamp_Request_TimeStamp_From_DateTimeMachine()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var fixedNow = new DateTime(2031, 3, 14, 9, 26, 53, DateTimeKind.Local);
        var machine = Substitute.For<IDateTimeMachine>();
        machine.Now.Returns(fixedNow);

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection",
        };

        var request = new TaskGatewayRequest { MachineId = 1001, PartNumber = "PART" };

        // Act
        await hubConnection.PublishCommandToHubAsync(request, factory, machine, logger, cancellationToken);

        // Assert - stamped from the machine, not DateTime.Now
        request.TimeStamp.ShouldBe(fixedNow);
    }

    /// <summary>
    /// Regression for #63: the response wire-DTO timestamp must come from the injected IDateTimeMachine.
    /// </summary>
    [Fact]
    public async Task PublishResultsToHubAsync_Should_Stamp_Response_TimeStamp_From_DateTimeMachine()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var factory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var fixedNow = new DateTime(2031, 3, 14, 9, 26, 53, DateTimeKind.Local);
        var machine = Substitute.For<IDateTimeMachine>();
        machine.Now.Returns(fixedNow);

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection",
        };

        var request = new TaskGatewayRequest { MachineId = 1001, PartNumber = "PART" };
        var response = new TaskGatewayResponseDto { MachineId = 1001, PartNumber = "PART" };
        var result = Result<TaskGatewayResponseDto>.Success(response);

        // Act
        await hubConnection.PublishResultsToHubAsync(request, result, factory, machine, logger, cancellationToken);

        // Assert - stamped from the machine, not DateTime.Now
        var sent = hubConnection.GetLastSentMessageArg<TaskGatewayResponseDto>("BroadcastTaskGatewayResponse", 1);
        sent.ShouldNotBeNull();
        sent.TimeStamp.ShouldBe(fixedNow);
    }
}

/// <summary>
/// Test double that can simulate start and invoke failures for resilience testing.
/// </summary>
public sealed class FailingTestHubConnection : TestHubConnection
{
    public bool ShouldFailOnStart { get; set; }
    public Exception? StartException { get; set; }
    public bool ShouldFailOnInvoke { get; set; }
    public Exception? InvokeException { get; set; }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (ShouldFailOnStart && StartException is not null)
        {
            throw StartException;
        }
        return base.StartAsync(cancellationToken);
    }

    public override Task<T?> InvokeAsync<T>(string methodName, object?[] args, CancellationToken cancellationToken = default) where T : default
    {
        if (ShouldFailOnInvoke && InvokeException is not null)
        {
            throw InvokeException;
        }
        return base.InvokeAsync<T>(methodName, args, cancellationToken);
    }

    public override Task SendAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        if (ShouldFailOnInvoke && InvokeException is not null)
        {
            throw InvokeException;
        }
        return base.SendAsync(methodName, args, cancellationToken);
    }
}