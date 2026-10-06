// <copyright file="HubMonitorWorkerRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Monitor.Tests;

using IndTrace.Application.UI.Services;
using IndTrace.Monitor.Tests.TestDoubles;
using IndTrace.Monitor.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using static IndTrace.HubConnection.Contracts.HubMethods;

/// <summary>
/// Regression tests for issue #71 (P0-11): SignalR handlers must be registered exactly once,
/// not re-appended on every 60s watchdog pass. SignalR's <c>On(...)</c> and the connection events
/// append, so re-registering per pass stacks duplicate handlers without bound.
/// </summary>
public sealed class HubMonitorWorkerRegistrationTests
{
    private static HubMonitorWorker CreateWorker(SingleConnectionHubConnectionFactory factory) =>
        new(factory, new IndTraceEventsService(), NullLogger<HubMonitorWorker>.Instance);

    [Fact]
    public async Task SetupAndStartHubConnection_AcrossManyWatchdogPasses_RegistersHubEventHandlersExactlyOnce()
    {
        // Arrange
        var factory = new SingleConnectionHubConnectionFactory();
        var worker = CreateWorker(factory);

        // Act — simulate many watchdog iterations against the memoized connection.
        for (var pass = 0; pass < 5; pass++)
        {
            await worker.SetupAndStartHubConnectionAsync(Xunit.TestContext.Current.CancellationToken);
        }

        // Assert — the connection is built once and each hub event handler is attached exactly once,
        // independent of the number of passes. Before the fix this would be 4 x 5 = 20 registrations.
        factory.CreateCallCount.ShouldBe(1);
        factory.Connection.TotalOnRegistrations.ShouldBe(4);
        factory.Connection.OnRegistrationCountFor(BroadcastMessageToClients).ShouldBe(1);
        factory.Connection.OnRegistrationCountFor(BroadcastHeartbeatSignal).ShouldBe(1);
        factory.Connection.OnRegistrationCountFor(BroadcastTaskGatewayRequest).ShouldBe(1);
        factory.Connection.OnRegistrationCountFor(BroadcastTaskGatewayResponse).ShouldBe(1);
    }

    [Fact]
    public async Task SetupAndStartHubConnection_AcrossManyWatchdogPasses_SubscribesConnectionStateHandlersExactlyOnce()
    {
        // Arrange
        var factory = new SingleConnectionHubConnectionFactory();
        var worker = CreateWorker(factory);

        // Act
        for (var pass = 0; pass < 5; pass++)
        {
            await worker.SetupAndStartHubConnectionAsync(Xunit.TestContext.Current.CancellationToken);
        }

        // Assert — Closed/Reconnected are attached once at connection creation, not per pass.
        factory.Connection.ClosedSubscriptions.ShouldBe(1);
        factory.Connection.ReconnectedSubscriptions.ShouldBe(1);
    }

    [Fact]
    public async Task SetupAndStartHubConnection_SinglePass_RegistersEveryHandlerOnce()
    {
        // Arrange
        var factory = new SingleConnectionHubConnectionFactory();
        var worker = CreateWorker(factory);

        // Act — a single pass must still wire up every handler exactly once (no regression the other way).
        await worker.SetupAndStartHubConnectionAsync(Xunit.TestContext.Current.CancellationToken);

        // Assert
        factory.CreateCallCount.ShouldBe(1);
        factory.Connection.TotalOnRegistrations.ShouldBe(4);
        factory.Connection.ClosedSubscriptions.ShouldBe(1);
        factory.Connection.ReconnectedSubscriptions.ShouldBe(1);
    }
}
