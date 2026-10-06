// <copyright file="HubConnectionFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Implementations;

using HubConnection.Tests.TestDoubles;
using IndTrace.Application.Models.Services;
using IndTrace.HubConnection.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

/// <summary>
/// Unit tests for IHubConnectionFactory interface behavior.
/// Tests factory creation logic using test doubles to avoid SignalR dependencies.
/// Validates factory contracts without real network connections.
/// </summary>
public class HubConnectionFactoryTests
{
    private readonly TestHubConnectionFactory _factory;

    public HubConnectionFactoryTests()
    {
        _factory = new TestHubConnectionFactory();
    }

    [Fact]
    public void TestFactory_InitialState_ShouldBeEmpty()
    {
        // Assert
        _factory.CreateCallCount.ShouldBe(0);
        _factory.CreatedConnections.Count.ShouldBe(0);
    }

    [Fact]
    public async Task CreateAsync_WithValidOptions_ShouldReturnConnection()
    {
        // Act
        var connection = await _factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert
        connection.ShouldNotBeNull();
        connection.ShouldBeAssignableTo<IHubConnection>();
        connection.State.ShouldBe(HubConnectionState.Disconnected);
        connection.ConnectionId.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnTestHubConnection()
    {
        // Act
        var connection = await _factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert
        connection.ShouldBeOfType<TestHubConnection>();
    }

    [Fact]
    public async Task CreateAsync_WithFactoryException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test factory error");
        _factory.WithException(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _factory.CreateAsync(TestContext.Current.CancellationToken));
        exception.ShouldBe(expectedException);
    }

    [Fact]
    public async Task CreateAsync_MultipleCalls_ShouldReturnDifferentInstances()
    {
        // Act
        var connection1 = await _factory.CreateAsync(TestContext.Current.CancellationToken);
        var connection2 = await _factory.CreateAsync(TestContext.Current.CancellationToken);
        var connection3 = await _factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert
        connection1.ShouldNotBeSameAs(connection2);
        connection2.ShouldNotBeSameAs(connection3);
        connection1.ShouldNotBeSameAs(connection3);
    }

    [Fact]
    public async Task CreateAsync_ShouldTrackCreatedConnections()
    {
        // Act
        var connection1 = await _factory.CreateAsync(TestContext.Current.CancellationToken);
        var connection2 = await _factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert
        _factory.CreateCallCount.ShouldBe(2);
        _factory.CreatedConnections.Count.ShouldBe(2);
        _factory.CreatedConnections.ShouldContain(connection1 as TestHubConnection);
        _factory.CreatedConnections.ShouldContain(connection2 as TestHubConnection);
    }

    [Fact]
    public async Task CreateAsync_WithCustomCreator_ShouldUseProvidedLogic()
    {
        // Arrange
        var customConnection = new TestHubConnection { State = HubConnectionState.Connecting };
        _factory.WithCustomCreator(() => customConnection);

        // Act
        var connection = await _factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert
        connection.ShouldBeSameAs(customConnection);
        connection.State.ShouldBe(HubConnectionState.Connecting);
    }

    [Fact]
    public async Task CreateAsync_WithCancellation_ShouldRespectToken()
    {
        // Arrange
        _factory.WithCreationDelay(TimeSpan.FromSeconds(5));
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            async () => await _factory.CreateAsync(cts.Token));
    }

    [Fact]
    public async Task CreateAsync_ConnectionShouldNotBeStarted()
    {
        // Act
        var connection = await _factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert - Factory creates but does NOT start the connection
        connection.State.ShouldBe(HubConnectionState.Disconnected);

        // Verify we can start it manually
        await connection.StartAsync(TestContext.Current.CancellationToken);
        connection.State.ShouldBe(HubConnectionState.Connected);
    }

    [Fact]
    public async Task Factory_Reset_ShouldClearState()
    {
        // Arrange
        await _factory.CreateAsync(TestContext.Current.CancellationToken);
        await _factory.CreateAsync(TestContext.Current.CancellationToken);
        _factory.CreateCallCount.ShouldBe(2);

        // Act
        _factory.Reset();

        // Assert
        _factory.CreateCallCount.ShouldBe(0);
        _factory.CreatedConnections.Count.ShouldBe(0);

        // Should work normally after reset
        var newConnection = await _factory.CreateAsync(TestContext.Current.CancellationToken);
        newConnection.ShouldNotBeNull();
        _factory.CreateCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task CreateAsync_WithDelayedCreation_ShouldEventuallySucceed()
    {
        // Arrange
        _factory.WithCreationDelay(TimeSpan.FromMilliseconds(50));
        var startTime = DateTimeOffset.UtcNow;

        // Act
        var connection = await _factory.CreateAsync(TestContext.Current.CancellationToken);
        var endTime = DateTimeOffset.UtcNow;

        // Assert
        connection.ShouldNotBeNull();
        (endTime - startTime).ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(45));
    }

    [Fact]
    public async Task Factory_ShouldBeThreadSafe()
    {
        // Arrange
        var connections = new List<IHubConnection>();
        var exceptions = new List<Exception>();
        var tasks = new List<Task>();

        // Act - Create connections from multiple threads
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var connection = await _factory.CreateAsync(TestContext.Current.CancellationToken);
                    lock (connections)
                    {
                        connections.Add(connection);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks.ToArray());

        // Assert
        exceptions.ShouldBeEmpty();
        connections.Count.ShouldBe(10);

        // All connections should be different instances
        for (int i = 0; i < connections.Count; i++)
        {
            for (int j = i + 1; j < connections.Count; j++)
            {
                connections[i].ShouldNotBeSameAs(connections[j]);
            }
        }
    }
}