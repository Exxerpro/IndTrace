// <copyright file="DashboardEvictionUnregistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Dashboard;

using HubConnection.Tests.Extensions;
using HubConnection.Tests.TestDoubles;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Dashboard;
using IndTrace.HubConnection.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using Xunit;

/// <summary>
/// Regression tests for the dashboard corpse roster (issue #106 follow-up): connections register
/// with the dashboard right after CreateAsync — BEFORE StartAsync, under a synthetic pending key —
/// but the eviction+dispose path in EnsureHubConnectionIsValid never unregistered them. Every
/// eviction cycle therefore rooted the disposed connection (adapter, inner HubConnection, latency
/// buffer) in the dashboard forever and monotonically inflated TotalConnections.
/// </summary>
// These tests share HubConnectionInterfaceExtensions' STATIC connection cache (and call
// ClearConnectionCache, which wipes it for everyone), so classes touching that cache must
// never run in parallel with each other: same collection => serialized by xUnit.
[Collection("HubConnectionStaticCache")]
public class DashboardEvictionUnregistrationTests
{
    /// <summary>
    /// An evicted+disposed connection must vanish from the dashboard: the eviction path in
    /// EnsureHubConnectionIsValid must unregister the corpse it disposes.
    /// </summary>
    [Fact]
    public async Task EnsureHubConnectionIsValid_Eviction_Should_Unregister_Disposed_Connection_From_Dashboard()
    {
        // Arrange - the production registration site is the metrics factory decorator, which
        // registers every created connection under a pending-* key (ConnectionId is still null).
        HubConnectionInterfaceExtensions.ClearConnectionCache();
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var dashboard = new HubMetricsDashboard();
        var innerFactory = new TestHubConnectionFactory()
            .WithCustomCreator(() => new FailingTestHubConnection
            {
                State = HubConnectionState.Disconnected,
                ShouldFailOnStart = true,
                StartException = new ObjectDisposedException("connection"),
            });
        var factory = new MetricsRegisteringHubConnectionFactory(innerFactory, dashboard);

        IHubConnection? hubConnection = null;

        // Act - each call creates+registers a connection whose start fails as disposed, so the
        // eviction path evicts and disposes it; its dashboard registration must die with it.
        await hubConnection.EnsureHubConnectionIsValid(factory, logger, cancellationToken);
        await hubConnection.EnsureHubConnectionIsValid(factory, logger, cancellationToken);

        // Assert - both connections were evicted and disposed...
        innerFactory.CreateCallCount.ShouldBe(2);
        innerFactory.CreatedConnections[0].IsDisposed.ShouldBeTrue();
        innerFactory.CreatedConnections[1].IsDisposed.ShouldBeTrue();

        // ...so the dashboard must report ZERO connections. Pre-fix both corpses stayed
        // registered under unreachable pending-* keys and TotalConnections was 2.
        var aggregated = await dashboard.GetAggregatedMetricsAsync(cancellationToken: cancellationToken);
        aggregated.TotalConnections.ShouldBe(0);
    }

    /// <summary>
    /// Registration then unregistration by instance must leave the roster empty even when the
    /// connection was registered under the synthetic pending key (null ConnectionId), which no
    /// string-keyed removal could ever match.
    /// </summary>
    [Fact]
    public async Task UnregisterConnection_ByInstance_Should_Remove_Pending_Registration()
    {
        // Arrange - a freshly created (not yet started) connection has no ConnectionId yet.
        var dashboard = new HubMetricsDashboard();
        var pendingConnection = new TestHubConnection
        {
            ConnectionId = null
        };
        dashboard.RegisterConnection(pendingConnection);

        // Act - pre-fix only UnregisterConnection(string) existed and could never match pending-*.
        dashboard.UnregisterConnection(pendingConnection);
        var aggregated = await dashboard.GetAggregatedMetricsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        aggregated.TotalConnections.ShouldBe(0);
    }

    /// <summary>
    /// Instance-identity removal must also work for a connection registered under its real
    /// ConnectionId, so the same eviction call is correct regardless of registration timing.
    /// </summary>
    [Fact]
    public async Task UnregisterConnection_ByInstance_Should_Remove_Registration_Under_Real_ConnectionId()
    {
        // Arrange
        var dashboard = new HubMetricsDashboard();
        var startedConnection = new TestHubConnection
        {
            ConnectionId = "real-connection-id"
        };
        dashboard.RegisterConnection(startedConnection);

        // Act
        dashboard.UnregisterConnection(startedConnection);
        var aggregated = await dashboard.GetAggregatedMetricsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        aggregated.TotalConnections.ShouldBe(0);
    }

    /// <summary>
    /// Instance-identity removal must only remove the matching instance, never other registrations.
    /// </summary>
    [Fact]
    public async Task UnregisterConnection_ByInstance_Should_Not_Remove_Other_Connections()
    {
        // Arrange
        var dashboard = new HubMetricsDashboard();
        var evictee = new TestHubConnection { ConnectionId = null };
        var survivor = new TestHubConnection { ConnectionId = null };
        dashboard.RegisterConnection(evictee);
        dashboard.RegisterConnection(survivor);

        // Act
        dashboard.UnregisterConnection(evictee);
        var aggregated = await dashboard.GetAggregatedMetricsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        aggregated.TotalConnections.ShouldBe(1);
    }

    /// <summary>
    /// A null instance is skipped safely, matching the exception-safe dashboard contract.
    /// </summary>
    [Fact]
    public void UnregisterConnection_ByInstance_Should_Ignore_Null_Connection()
    {
        // Arrange
        var dashboard = new HubMetricsDashboard();

        // Act & Assert
        Should.NotThrow(() => dashboard.UnregisterConnection((IHubConnection?)null));
    }
}
