// <copyright file="SignalRHubConnectionAdapterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Implementations;

using HubConnection.Tests.TestDoubles;
using IndTrace.HubConnection.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using Xunit;

/// <summary>
/// Unit tests for IHubConnection interface behavior.
/// Tests that implementations fulfill the IHubConnection contract.
/// Uses test doubles to validate interface behavior - proper I²TDD!
/// </summary>
public class SignalRHubConnectionAdapterTests
{
    private readonly IHubConnection _connection;

    public SignalRHubConnectionAdapterTests()
    {
        _connection = new TestHubConnection();
    }

    [Fact]
    public void InitialState_ShouldBeDisconnected()
    {
        // Act & Assert
        _connection.State.ShouldBe(HubConnectionState.Disconnected);
        _connection.ConnectionId.ShouldBeNull();
    }

    [Fact]
    public async Task StartAsync_ShouldChangeStateToConnected()
    {
        // Arrange
        _connection.State.ShouldBe(HubConnectionState.Disconnected);

        // Act
        await _connection.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        _connection.State.ShouldBe(HubConnectionState.Connected);
        _connection.ConnectionId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task StopAsync_ShouldChangeStateToDisconnected()
    {
        // Arrange
        await _connection.StartAsync(TestContext.Current.CancellationToken);
        _connection.State.ShouldBe(HubConnectionState.Connected);

        // Act
        await _connection.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        _connection.State.ShouldBe(HubConnectionState.Disconnected);
        _connection.ConnectionId.ShouldBeNull();
    }

    [Fact]
    public async Task StartAsync_WhenAlreadyConnected_ShouldNotThrow()
    {
        // Arrange
        await _connection.StartAsync(TestContext.Current.CancellationToken);
        _connection.State.ShouldBe(HubConnectionState.Connected);

        // Act & Assert
        await Should.NotThrowAsync(async () => await _connection.StartAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task StopAsync_WhenAlreadyDisconnected_ShouldNotThrow()
    {
        // Arrange
        _connection.State.ShouldBe(HubConnectionState.Disconnected);

        // Act & Assert
        await Should.NotThrowAsync(async () => await _connection.StopAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SendAsync_WhenConnected_ShouldTrackMessage()
    {
        // Arrange
        var connection = _connection as TestHubConnection;
        await _connection.StartAsync(TestContext.Current.CancellationToken);
        const string methodName = "TestMethod";
        var args = new object?[] { "arg1", 123, true };

        // Act
        await _connection.SendAsync(methodName, args, TestContext.Current.CancellationToken);

        // Assert
        connection!.WasMessageSent(methodName).ShouldBeTrue();
        connection.GetMessageCount(methodName).ShouldBe(1);
    }

    [Fact]
    public async Task InvokeAsync_WhenConnected_ShouldReturnResult()
    {
        // Arrange
        await _connection.StartAsync(TestContext.Current.CancellationToken);
        const string methodName = "GetData";
        var args = new object?[] { "param1" };

        // Act
        var result = await _connection.InvokeAsync<string>(methodName, args, TestContext.Current.CancellationToken);

        // Assert - TestHubConnection returns default(T)
        result.ShouldBeNull();
    }

    [Fact]
    public async Task On_SingleParameter_ShouldRegisterHandler()
    {
        // Arrange
        var connection = _connection as TestHubConnection;
        var receivedMessage = string.Empty;
        const string methodName = "TestMessage";

        // Act
        using var registration = _connection.On<string>(methodName, message =>
        {
            receivedMessage = message;
            return Task.CompletedTask;
        });

        await connection!.SimulateMessage(methodName, "Hello World", TestContext.Current.CancellationToken);

        // Assert
        receivedMessage.ShouldBe("Hello World");
    }

    [Fact]
    public async Task On_TwoParameters_ShouldRegisterHandler()
    {
        // Arrange
        var connection = _connection as TestHubConnection;
        var receivedValues = (string.Empty, 0);
        const string methodName = "TestMessage";

        // Act
        using var registration = _connection.On<string, int>(methodName, (msg, num) =>
        {
            receivedValues = (msg, num);
            return Task.CompletedTask;
        });

        await connection!.SimulateMessage(methodName, "Test", 42, TestContext.Current.CancellationToken);

        // Assert
        receivedValues.ShouldBe(("Test", 42));
    }

    [Fact]
    public async Task DisposeAsync_ShouldCleanupResources()
    {
        // Arrange
        await _connection.StartAsync(TestContext.Current.CancellationToken);
        _connection.State.ShouldBe(HubConnectionState.Connected);

        // Act
        await _connection.DisposeAsync();

        // Assert
        _connection.State.ShouldBe(HubConnectionState.Disconnected);
        _connection.ConnectionId.ShouldBeNull();
    }

    [Fact]
    public void Events_Reconnecting_ShouldFireWhenReconnecting()
    {
        // Arrange
        var connection = _connection as TestHubConnection;
        var eventFired = false;
        Exception? receivedException = null;
        var testException = new Exception("Test reconnecting");

        _connection.Reconnecting += exception =>
        {
            eventFired = true;
            receivedException = exception;
            return Task.CompletedTask;
        };

        // Act
        connection!.SimulateReconnecting(testException);

        // Assert
        eventFired.ShouldBeTrue();
        receivedException.ShouldBe(testException);
        _connection.State.ShouldBe(HubConnectionState.Reconnecting);
    }

    [Fact]
    public void Events_Reconnected_ShouldFireWhenReconnected()
    {
        // Arrange
        var connection = _connection as TestHubConnection;
        var eventFired = false;
        string? receivedConnectionId = null;

        _connection.Reconnected += connectionId =>
        {
            eventFired = true;
            receivedConnectionId = connectionId;
            return Task.CompletedTask;
        };

        // Act
        connection!.SimulateReconnected();

        // Assert
        eventFired.ShouldBeTrue();
        receivedConnectionId.ShouldNotBeNullOrEmpty();
        _connection.State.ShouldBe(HubConnectionState.Connected);
    }

    [Fact]
    public void Events_Closed_ShouldFireWhenConnectionClosed()
    {
        // Arrange
        var connection = _connection as TestHubConnection;
        var eventFired = false;
        Exception? receivedException = null;
        var testException = new Exception("Connection closed");

        _connection.Closed += exception =>
        {
            eventFired = true;
            receivedException = exception;
            return Task.CompletedTask;
        };

        // Act
        connection!.SimulateConnectionClosed(testException);

        // Assert
        eventFired.ShouldBeTrue();
        receivedException.ShouldBe(testException);
        _connection.State.ShouldBe(HubConnectionState.Disconnected);
    }

    [Fact]
    public async Task Operations_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        await _connection.DisposeAsync();

        // Act & Assert - Should throw when trying to use disposed connection
        await Should.ThrowAsync<ObjectDisposedException>(
            async () => await _connection.StartAsync(TestContext.Current.CancellationToken));
    }
}