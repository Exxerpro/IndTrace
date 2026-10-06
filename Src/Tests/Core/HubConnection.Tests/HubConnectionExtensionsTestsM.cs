// <copyright file="HubConnectionExtensionsTestsM.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests;

using HubConnection.Tests.TestDoubles;
using IndTrace.HubConnection.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

/// <summary>
/// Tests for HubConnection extension methods using I²TDD approach with test doubles.
/// </summary>
// These tests share HubConnectionInterfaceExtensions' STATIC connection cache (and call
// ClearConnectionCache, which wipes it for everyone), so classes touching that cache must
// never run in parallel with each other: same collection => serialized by xUnit.
[Collection("HubConnectionStaticCache")]
public class HubConnectionExtensionsTestsM
{
    /// <summary>
    /// Executes EnsureHubConnectionIsValid_ShouldInitializeConnection_WhenConnectionIsNull operation.
    /// </summary>
    /// <returns>The result of EnsureHubConnectionIsValid_ShouldInitializeConnection_WhenConnectionIsNull.</returns>
    [Fact]
    public async Task EnsureHubConnectionIsValid_ShouldInitializeConnection_WhenConnectionIsNull()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var connectionFactory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        IHubConnection? hubConnection = null;

        // Act
        var result = await hubConnection.EnsureHubConnectionIsValid(connectionFactory, logger, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        connectionFactory.CreateCallCount.ShouldBe(1);
    }

    /// <summary>
    /// Executes EnsureHubConnectionIsValid_ShouldStartConnection_WhenDisconnected operation.
    /// </summary>
    /// <returns>The result of EnsureHubConnectionIsValid_ShouldStartConnection_WhenDisconnected.</returns>
    [Fact]
    public async Task EnsureHubConnectionIsValid_ShouldStartConnection_WhenDisconnected()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var connectionFactory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Disconnected
        };

        // Act
        var result = await hubConnection.EnsureHubConnectionIsValid(connectionFactory, logger, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.State.ShouldBe(HubConnectionState.Connected);
        connectionFactory.CreateCallCount.ShouldBe(0); // Should not create new connection
    }

    /// <summary>
    /// Executes EnsureHubConnectionIsValid_ShouldNotReinitialize_WhenAlreadyConnected operation.
    /// </summary>
    /// <returns>The result of EnsureHubConnectionIsValid_ShouldNotReinitialize_WhenAlreadyConnected.</returns>
    [Fact]
    public async Task EnsureHubConnectionIsValid_ShouldNotReinitialize_WhenAlreadyConnected()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var connectionFactory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Connected,
            ConnectionId = "test-connection-id"
        };

        // Act
        var result = await hubConnection.EnsureHubConnectionIsValid(connectionFactory, logger, cancellationToken);

        // Assert
        result.ShouldBeSameAs(hubConnection);
        connectionFactory.CreateCallCount.ShouldBe(0); // Should not create new connection
        result.ShouldNotBeNull();
        result.State.ShouldBe(HubConnectionState.Connected); // Should remain connected
    }

    /// <summary>
    /// Executes EnsureHubConnectionIsValid_ShouldLogError_WhenExceptionOccurs operation.
    /// </summary>
    /// <returns>The result of EnsureHubConnectionIsValid_ShouldLogError_WhenExceptionOccurs.</returns>
    [Fact]
    public async Task EnsureHubConnectionIsValid_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var connectionFactory = new TestHubConnectionFactory()
            .WithException(new InvalidOperationException("Connection error"));
        var cancellationToken = TestContext.Current.CancellationToken;

        IHubConnection? hubConnection = null;

        // Act
        var result = await hubConnection.EnsureHubConnectionIsValid(connectionFactory, logger, cancellationToken);

        // Assert
        result.ShouldBeNull();
        connectionFactory.CreateCallCount.ShouldBe(1);
        logger.LogEntries.ShouldContain(entry => entry.LogLevel == LogLevel.Error);
    }

    //Step 3: Simulate Failures and Race Conditions
    //To simulate failures and race conditions:

    //Simulate Failures:

    //Use the When method in NSubstitute to throw exceptions during method calls, as shown in the last test case above.
    //Simulate Concurrent Access:

    //You can create multiple tasks to simulate concurrent access and ensure the Lock API is working as expected.
    //Example test for concurrency:
    //csharp
    //Copy code
    /// <summary>
    /// Executes EnsureHubConnectionIsValid_ShouldHandleConcurrentAccess operation.
    /// </summary>
    /// <returns>The result of EnsureHubConnectionIsValid_ShouldHandleConcurrentAccess.</returns>
    [Fact]
    public async Task EnsureHubConnectionIsValid_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var logger = new TestLogger<object>();
        var connectionFactory = new TestHubConnectionFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        var hubConnection = new TestHubConnection
        {
            State = HubConnectionState.Disconnected
        };

        var tasks = Enumerable.Range(0, 10).Select(_ =>
            Task.Run(() => hubConnection.EnsureHubConnectionIsValid(connectionFactory, logger, cancellationToken), cancellationToken));

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.All(r => r is not null).ShouldBeTrue();
        hubConnection.State.ShouldBe(HubConnectionState.Connected);
        connectionFactory.CreateCallCount.ShouldBe(0); // Should not create new connections
    }
}