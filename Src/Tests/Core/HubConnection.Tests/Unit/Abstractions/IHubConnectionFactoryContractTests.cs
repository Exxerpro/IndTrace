// <copyright file="IHubConnectionFactoryContractTests.cs" company="Exxerpro Solutions SA de CV">
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
/// Contract tests for IHubConnectionFactory interface.
/// Defines expected behavior for ALL factory implementations.
/// Following Single Responsibility Principle - factory ONLY creates connections.
/// </summary>
public class IHubConnectionFactoryContractTests
{
    [Fact]
    public async Task CreateAsync_ShouldReturnDisconnectedConnection()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var connection = await factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert
        connection.ShouldNotBeNull();
        connection.State.ShouldBe(HubConnectionState.Disconnected);
        connection.ConnectionId.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAsync_MultipleCalls_ShouldReturnDifferentInstances()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var connection1 = await factory.CreateAsync(TestContext.Current.CancellationToken);
        var connection2 = await factory.CreateAsync(TestContext.Current.CancellationToken);
        var connection3 = await factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert
        connection1.ShouldNotBeSameAs(connection2);
        connection2.ShouldNotBeSameAs(connection3);
        connection1.ShouldNotBeSameAs(connection3);
    }

    [Fact]
    public async Task CreateAsync_WithCancellation_ShouldRespectToken()
    {
        // Arrange
        var factory = CreateFactory() as TestHubConnectionFactory;
        factory!.WithCreationDelay(TimeSpan.FromSeconds(5));

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            async () => await factory.CreateAsync(cts.Token));
    }

    [Fact]
    public async Task CreateAsync_ShouldNotAutoStartConnection()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var connection = await factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert - Connection should be created but NOT started
        connection.State.ShouldBe(HubConnectionState.Disconnected);

        // Verify we can start it manually
        await connection.StartAsync(TestContext.Current.CancellationToken);
        connection.State.ShouldBe(HubConnectionState.Connected);
    }

    [Fact]
    public async Task CreateAsync_CreatedConnections_ShouldBeIndependent()
    {
        // Arrange
        var factory = CreateFactory();
        var connection1 = await factory.CreateAsync(TestContext.Current.CancellationToken);
        var connection2 = await factory.CreateAsync(TestContext.Current.CancellationToken);

        // Act - Start only connection1
        await connection1.StartAsync(TestContext.Current.CancellationToken);

        // Assert - connection2 should remain disconnected
        connection1.State.ShouldBe(HubConnectionState.Connected);
        connection2.State.ShouldBe(HubConnectionState.Disconnected);

        // Act - Stop connection1, start connection2
        await connection1.StopAsync(TestContext.Current.CancellationToken);
        await connection2.StartAsync(TestContext.Current.CancellationToken);

        // Assert - States are independent
        connection1.State.ShouldBe(HubConnectionState.Disconnected);
        connection2.State.ShouldBe(HubConnectionState.Connected);
    }

    [Fact]
    public async Task CreateAsync_WhenFactoryFails_ShouldPropagateException()
    {
        // Arrange
        var factory = CreateFactory() as TestHubConnectionFactory;
        var expectedException = new InvalidOperationException("Configuration error");
        factory!.WithException(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await factory.CreateAsync(TestContext.Current.CancellationToken));
        exception.ShouldBe(expectedException);
    }

    [Fact]
    public async Task Factory_ShouldTrackCreatedConnections()
    {
        // Arrange
        var factory = CreateFactory() as TestHubConnectionFactory;
        factory!.CreateCallCount.ShouldBe(0);
        factory.CreatedConnections.Count.ShouldBe(0);

        // Act
        var connection1 = await factory.CreateAsync(TestContext.Current.CancellationToken);
        var connection2 = await factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert
        factory.CreateCallCount.ShouldBe(2);
        factory.CreatedConnections.Count.ShouldBe(2);
        factory.CreatedConnections.ShouldContain(connection1 as TestHubConnection);
        factory.CreatedConnections.ShouldContain(connection2 as TestHubConnection);
    }

    [Fact]
    public async Task Factory_WithCustomCreator_ShouldUseProvidedLogic()
    {
        // Arrange
        var factory = CreateFactory() as TestHubConnectionFactory;
        var customConnection = new TestHubConnection
        {
            State = HubConnectionState.Connecting // Non-standard initial state
        };

        factory!.WithCustomCreator(() => customConnection);

        // Act
        var connection = await factory.CreateAsync(TestContext.Current.CancellationToken);

        // Assert
        connection.ShouldBeSameAs(customConnection);
        connection.State.ShouldBe(HubConnectionState.Connecting);
    }

    [Fact]
    public async Task Factory_Reset_ShouldClearState()
    {
        // Arrange
        var factory = CreateFactory() as TestHubConnectionFactory;
        await factory!.CreateAsync(TestContext.Current.CancellationToken);
        await factory.CreateAsync(TestContext.Current.CancellationToken);

        factory.CreateCallCount.ShouldBe(2);
        factory.CreatedConnections.Count.ShouldBe(2);

        // Act
        factory.Reset();

        // Assert
        factory.CreateCallCount.ShouldBe(0);
        factory.CreatedConnections.Count.ShouldBe(0);

        // Should work normally after reset
        var newConnection = await factory.CreateAsync(TestContext.Current.CancellationToken);
        newConnection.ShouldNotBeNull();
        factory.CreateCallCount.ShouldBe(1);
    }

    private static IHubConnectionFactory CreateFactory() => new TestHubConnectionFactory();
}