// <copyright file="TrySendAsyncTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Extensions;

using HubConnection.Tests.Extensions;
using HubConnection.Tests.TestDoubles;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

/// <summary>
/// Tests for the TrySendAsync extension (issue #106): worker hosts must never die on a
/// disconnected or failing send — a failed send is reported as <c>false</c>, never thrown.
/// </summary>
public class TrySendAsyncTests
{
    /// <summary>
    /// A null connection must be reported as a failed delivery, not a NullReferenceException.
    /// </summary>
    [Fact]
    public async Task TrySendAsync_Should_Return_False_When_Connection_Is_Null()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        IHubConnection? hubConnection = null;

        // Act
        var sent = await hubConnection.TrySendAsync("SendMessage", new object?[] { "hello" }, logger, cancellationToken);

        // Assert
        sent.ShouldBeFalse();
        logger.HasLogLevel(LogLevel.Warning).ShouldBeTrue();
        logger.HasMessage("connection is null").ShouldBeTrue();
    }

    /// <summary>
    /// Sending on a disconnected connection must be skipped and reported as failed — pre-#106 the
    /// raw SendAsync threw and killed the worker host.
    /// </summary>
    [Fact]
    public async Task TrySendAsync_Should_Return_False_When_Disconnected()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Disconnected
        };

        // Act
        var sent = await hubConnection.TrySendAsync("SendMessage", new object?[] { "hello" }, logger, cancellationToken);

        // Assert
        sent.ShouldBeFalse();
        hubConnection.WasMessageSent("SendMessage").ShouldBeFalse();
        logger.HasLogLevel(LogLevel.Warning).ShouldBeTrue();
    }

    /// <summary>
    /// A transport failure during the send must be logged and reported as failed, never thrown.
    /// </summary>
    [Fact]
    public async Task TrySendAsync_Should_Return_False_When_SendAsync_Throws()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var failingConnection = new FailingTestHubConnection
        {
            State = HubConnectionState.Connected,
            ShouldFailOnInvoke = true,
            InvokeException = new InvalidOperationException("Transport failure"),
        };

        // Act
        var sent = await failingConnection.TrySendAsync("SendMessage", new object?[] { "hello" }, logger, cancellationToken);

        // Assert
        sent.ShouldBeFalse();
        logger.HasLogLevel(LogLevel.Error).ShouldBeTrue();
        logger.HasMessage("Error sending hub message SendMessage").ShouldBeTrue();
    }

    /// <summary>
    /// A connected send must succeed and forward method name and arguments exactly as given.
    /// </summary>
    [Fact]
    public async Task TrySendAsync_Should_Send_And_Forward_Args_Exactly_When_Connected()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection"
        };

        // Act
        var sent = await hubConnection.TrySendAsync("SendMessage", new object?[] { "hub-server heartbeat", 42 }, logger, cancellationToken);

        // Assert
        sent.ShouldBeTrue();
        hubConnection.WasMessageSent("SendMessage").ShouldBeTrue();
        hubConnection.GetMessageCount("SendMessage").ShouldBe(1);
        hubConnection.GetLastSentMessageArg<string>("SendMessage", 0).ShouldBe("hub-server heartbeat");
        hubConnection.GetLastSentMessageArg<int>("SendMessage", 1).ShouldBe(42);
    }

    /// <summary>
    /// Cancellation is a control-flow signal, not a delivery failure — it must propagate.
    /// </summary>
    [Fact]
    public async Task TrySendAsync_Should_Propagate_Cancellation()
    {
        // Arrange
        var logger = new TestLogger<object>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected
        };

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            hubConnection.TrySendAsync("SendMessage", new object?[] { "hello" }, logger, cts.Token));
    }

    /// <summary>
    /// An OperationCanceledException thrown by the transport itself must also propagate rather than
    /// being converted into a silent <c>false</c>.
    /// </summary>
    [Fact]
    public async Task TrySendAsync_Should_Propagate_Cancellation_Thrown_By_Transport()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var failingConnection = new FailingTestHubConnection
        {
            State = HubConnectionState.Connected,
            ShouldFailOnInvoke = true,
            InvokeException = new OperationCanceledException("Transport canceled"),
        };

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            failingConnection.TrySendAsync("SendMessage", new object?[] { "hello" }, logger, cancellationToken));
    }
}
