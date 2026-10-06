// <copyright file="IHubConnectionContractTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Abstractions;

using HubConnection.Tests.TestDoubles;
using IndTrace.HubConnection.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using Xunit;

/// <summary>
/// Contract tests for IHubConnection interface.
/// These tests define the expected behavior that ALL implementations must satisfy.
/// Following Uncle Bob's Interface Segregation Principle.
/// </summary>
public class IHubConnectionContractTests
{
    [Fact]
    public async Task StartAsync_WhenDisconnected_ShouldConnectSuccessfully()
    {
        // Arrange
        var connection = CreateConnection();
        connection.State.ShouldBe(HubConnectionState.Disconnected);

        // Act
        await connection.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        connection.State.ShouldBe(HubConnectionState.Connected);
        connection.ConnectionId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task StartAsync_WhenAlreadyConnected_ShouldNotThrow()
    {
        // Arrange
        var connection = CreateConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);
        connection.State.ShouldBe(HubConnectionState.Connected);

        // Act & Assert
        await Should.NotThrowAsync(async () => await connection.StartAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task StartAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var connection = CreateConnection();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            async () => await connection.StartAsync(cts.Token));
    }

    [Fact]
    public async Task StopAsync_WhenConnected_ShouldDisconnectSuccessfully()
    {
        // Arrange
        var connection = CreateConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        // Act
        await connection.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        connection.State.ShouldBe(HubConnectionState.Disconnected);
        connection.ConnectionId.ShouldBeNull();
    }

    [Fact]
    public async Task StopAsync_WhenAlreadyDisconnected_ShouldNotThrow()
    {
        // Arrange
        var connection = CreateConnection();

        // Act & Assert
        await Should.NotThrowAsync(async () => await connection.StopAsync());
    }

    [Fact]
    public async Task SendAsync_WhenConnected_ShouldSendMessage()
    {
        // Arrange
        var connection = CreateConnection() as TestHubConnection;
        await connection!.StartAsync(TestContext.Current.CancellationToken);
        var args = new object?[] { "arg1", 123, true };

        // Act
        await connection.SendAsync("TestMethod", args, TestContext.Current.CancellationToken);

        // Assert
        connection.WasMessageSent("TestMethod").ShouldBeTrue();
        connection.GetMessageCount("TestMethod").ShouldBe(1);
        connection.GetLastSentMessageArg<string>("TestMethod", 0).ShouldBe("arg1");
        connection.GetLastSentMessageArg<int>("TestMethod", 1).ShouldBe(123);
        connection.GetLastSentMessageArg<bool>("TestMethod", 2).ShouldBe(true);
    }

    [Fact]
    public async Task SendAsync_WhenDisconnected_ShouldThrow()
    {
        // Arrange
        var connection = CreateConnection();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            async () => await connection.SendAsync("TestMethod", Array.Empty<object?>(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task InvokeAsync_WhenConnected_ShouldReturnResult()
    {
        // Arrange
        var connection = CreateConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connection.InvokeAsync<string>("GetData", Array.Empty<object?>(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull(); // Test double returns default(T)
    }

    [Fact]
    public async Task InvokeAsync_WhenDisconnected_ShouldThrow()
    {
        // Arrange
        var connection = CreateConnection();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            async () => await connection.InvokeAsync<string>("GetData", Array.Empty<object?>(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task On_SingleParameter_ShouldRegisterHandler()
    {
        // Arrange
        var connection = CreateConnection() as TestHubConnection;
        var receivedMessage = string.Empty;

        // Act
        using var registration = connection!.On<string>("TestMessage", message =>
        {
            receivedMessage = message;
            return Task.CompletedTask;
        });

        await connection.SimulateMessage("TestMessage", "Hello World", TestContext.Current.CancellationToken);

        // Assert
        receivedMessage.ShouldBe("Hello World");
    }

    [Fact]
    public async Task On_TwoParameters_ShouldRegisterHandler()
    {
        // Arrange
        var connection = CreateConnection() as TestHubConnection;
        var receivedValues = (string.Empty, 0);

        // Act
        using var registration = connection!.On<string, int>("TestMessage", (msg, num) =>
        {
            receivedValues = (msg, num);
            return Task.CompletedTask;
        });

        await connection.SimulateMessage("TestMessage", "Test", 42, TestContext.Current.CancellationToken);

        // Assert
        receivedValues.ShouldBe(("Test", 42));
    }

    [Fact]
    public async Task On_DisposedRegistration_ShouldNotReceiveMessages()
    {
        // Arrange
        var connection = CreateConnection() as TestHubConnection;
        var messageCount = 0;

        // Act
        var registration = connection!.On<string>("TestMessage", _ =>
        {
            messageCount++;
            return Task.CompletedTask;
        });

        await connection.SimulateMessage("TestMessage", "First", TestContext.Current.CancellationToken);
        messageCount.ShouldBe(1);

        registration.Dispose();

        await connection.SimulateMessage("TestMessage", "Second", TestContext.Current.CancellationToken);

        // Assert
        messageCount.ShouldBe(1); // Should not receive second message
    }

    [Fact]
    public void Events_Reconnecting_ShouldFireWhenReconnecting()
    {
        // Arrange
        var connection = CreateConnection() as TestHubConnection;
        Exception? receivedException = null;
        var eventFired = false;

        connection!.Reconnecting += error =>
        {
            eventFired = true;
            receivedException = error;
            return Task.CompletedTask;
        };

        // Act
        var testException = new Exception("Test error");
        connection.SimulateReconnecting(testException);

        // Assert
        eventFired.ShouldBeTrue();
        receivedException.ShouldBe(testException);
        connection.State.ShouldBe(HubConnectionState.Reconnecting);
    }

    [Fact]
    public void Events_Reconnected_ShouldFireWhenReconnected()
    {
        // Arrange
        var connection = CreateConnection() as TestHubConnection;
        string? receivedConnectionId = null;
        var eventFired = false;

        connection!.Reconnected += connectionId =>
        {
            eventFired = true;
            receivedConnectionId = connectionId;
            return Task.CompletedTask;
        };

        // Act
        connection.SimulateReconnected();

        // Assert
        eventFired.ShouldBeTrue();
        receivedConnectionId.ShouldNotBeNullOrEmpty();
        connection.State.ShouldBe(HubConnectionState.Connected);
    }

    [Fact]
    public void Events_Closed_ShouldFireWhenConnectionClosed()
    {
        // Arrange
        var connection = CreateConnection() as TestHubConnection;
        Exception? receivedException = null;
        var eventFired = false;

        connection!.Closed += error =>
        {
            eventFired = true;
            receivedException = error;
            return Task.CompletedTask;
        };

        // Act
        var testException = new Exception("Connection lost");
        connection.SimulateConnectionClosed(testException);

        // Assert
        eventFired.ShouldBeTrue();
        receivedException.ShouldBe(testException);
        connection.State.ShouldBe(HubConnectionState.Disconnected);
    }

    [Fact]
    public async Task DisposeAsync_ShouldCleanupResources()
    {
        // Arrange
        var connection = CreateConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        // Act
        await connection.DisposeAsync();

        // Assert
        connection.State.ShouldBe(HubConnectionState.Disconnected);
        connection.ConnectionId.ShouldBeNull();

        // Further operations should throw
        await Should.ThrowAsync<ObjectDisposedException>(
            async () => await connection.StartAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DisposeAsync_MultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var connection = CreateConnection();

        // Act & Assert
        await Should.NotThrowAsync(async () =>
        {
            await connection.DisposeAsync();
            await connection.DisposeAsync();
            await connection.DisposeAsync();
        });
    }

    private static IHubConnection CreateConnection() => new TestHubConnection();
}