// <copyright file="DashboardPendingConnectionRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Dashboard;

using HubConnection.Tests.TestDoubles;
using IndTrace.HubConnection.Abstractions;
using Shouldly;
using Xunit;

/// <summary>
/// Regression tests for dead dashboard metrics (issue #106): connections are registered right
/// after CreateAsync, BEFORE StartAsync, when SignalR's ConnectionId is still null. Pre-fix
/// RegisterConnection silently no-oped on those, so the dashboard aggregated an empty set forever.
/// </summary>
public class DashboardPendingConnectionRegistrationTests
{
    [Fact]
    public async Task RegisterConnection_Should_Register_Connection_With_Null_ConnectionId()
    {
        // Arrange - a freshly created (not yet started) connection has no ConnectionId yet.
        var dashboard = CreateTestDashboard();
        var pendingConnection = new TestHubConnection
        {
            ConnectionId = null
        };

        // Act
        dashboard.RegisterConnection(pendingConnection);
        var aggregated = await dashboard.GetAggregatedMetricsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert - pre-fix registration no-oped and TotalConnections stayed 0.
        aggregated.ShouldNotBeNull();
        aggregated.TotalConnections.ShouldBe(1);
    }

    [Fact]
    public async Task RegisterConnection_Should_Track_Multiple_Pending_Connections_Separately()
    {
        // Arrange - two distinct not-yet-started connections must get distinct synthetic keys.
        var dashboard = CreateTestDashboard();
        var pending1 = new TestHubConnection { ConnectionId = null };
        var pending2 = new TestHubConnection { ConnectionId = null };

        // Act
        dashboard.RegisterConnection(pending1);
        dashboard.RegisterConnection(pending2);
        var aggregated = await dashboard.GetAggregatedMetricsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        aggregated.TotalConnections.ShouldBe(2);
    }

    [Fact]
    public void RegisterConnection_Should_Still_Ignore_Null_Connection()
    {
        // Arrange
        var dashboard = CreateTestDashboard();

        // Act & Assert - only a genuinely null connection is skipped, and never throws.
        Should.NotThrow(() => dashboard.RegisterConnection(null));
    }

    private static IHubMetricsDashboard CreateTestDashboard()
    {
        return new IndTrace.HubConnection.Dashboard.HubMetricsDashboard();
    }
}
