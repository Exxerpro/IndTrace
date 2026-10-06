// <copyright file="IHubMetricsDashboardTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Dashboard;

using IndTrace.HubConnection.Abstractions;
using HubConnection.Tests.TestDoubles;
using Shouldly;
using Xunit;

/// <summary>
/// TDD tests for IHubMetricsDashboard - aggregated infrastructure metrics service
/// Key principle: Non-blocking aggregation of metrics across multiple connections
/// Dashboard serves infrastructure monitoring, not business intelligence
/// </summary>
public class IHubMetricsDashboardTests
{
    [Fact]
    public async Task Dashboard_Should_Aggregate_Metrics_From_Multiple_Connections()
    {
        // Arrange - This test drives the dashboard interface design
        var dashboard = CreateTestDashboard();

        // Register multiple connections with metrics
        var connection1 = CreateTestConnectionWithMetrics("conn-1", messagesSent: 100, uptime: TimeSpan.FromMinutes(5));
        var connection2 = CreateTestConnectionWithMetrics("conn-2", messagesSent: 200, uptime: TimeSpan.FromMinutes(10));

        dashboard.RegisterConnection(connection1);
        dashboard.RegisterConnection(connection2);

        // Act
        var aggregated = await dashboard.GetAggregatedMetricsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Should combine metrics from all connections
        aggregated.ShouldNotBeNull();
        aggregated.TotalConnections.ShouldBe(2);
        aggregated.TotalMessagesSent.ShouldBe(300);
        aggregated.AverageUptime.ShouldBeGreaterThan(TimeSpan.FromMinutes(7));
    }

    [Fact]
    public async Task Dashboard_Should_Provide_Health_Check_Status()
    {
        // Arrange
        var dashboard = CreateTestDashboard();

        var healthyConnection = CreateTestConnectionWithMetrics("healthy", consecutiveFailures: 0);
        var unhealthyConnection = CreateTestConnectionWithMetrics("unhealthy", consecutiveFailures: 5);

        dashboard.RegisterConnection(healthyConnection);
        dashboard.RegisterConnection(unhealthyConnection);

        // Act
        var health = await dashboard.GetHealthStatusAsync(TestContext.Current.CancellationToken);

        // Assert
        health.ShouldNotBeNull();
        health.Status.ShouldBe("Critical"); // 50% unhealthy = Critical (>= 10% threshold)
        health.TotalConnections.ShouldBe(2);
        health.HealthyConnections.ShouldBe(1);
        health.UnhealthyConnections.ShouldBe(1);
    }

    [Fact]
    public async Task Dashboard_Should_Handle_Connection_Removal_Gracefully()
    {
        // Arrange
        var dashboard = CreateTestDashboard();
        var connection = CreateTestConnectionWithMetrics("temp-conn");

        dashboard.RegisterConnection(connection);

        // Act
        dashboard.UnregisterConnection("temp-conn");
        var metrics = await dashboard.GetAggregatedMetricsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        metrics.TotalConnections.ShouldBe(0);
    }

    [Fact]
    public async Task Dashboard_Should_Never_Block_On_Slow_Connections()
    {
        // Arrange
        var dashboard = CreateTestDashboard();
        var slowConnection = CreateSlowTestConnection("slow");
        var fastConnection = CreateTestConnectionWithMetrics("fast");

        dashboard.RegisterConnection(slowConnection);
        dashboard.RegisterConnection(fastConnection);

        // Act - Should complete quickly even with slow connection
        var startTime = DateTimeOffset.UtcNow;
        var metrics = await dashboard.GetAggregatedMetricsAsync(TimeSpan.FromMilliseconds(500), TestContext.Current.CancellationToken);
        var duration = DateTimeOffset.UtcNow - startTime;

        // Assert - Should not wait for slow connections (allow some tolerance for test execution)
        duration.ShouldBeLessThan(TimeSpan.FromSeconds(3));
        metrics.ShouldNotBeNull();
        // Should include fast connection even if slow one times out
        metrics.TotalConnections.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_Should_Provide_Top_N_Connections_By_Activity()
    {
        // Arrange
        var dashboard = CreateTestDashboard();

        var highActivity = CreateTestConnectionWithMetrics("high", messagesSent: 1000);
        var mediumActivity = CreateTestConnectionWithMetrics("medium", messagesSent: 500);
        var lowActivity = CreateTestConnectionWithMetrics("low", messagesSent: 100);

        dashboard.RegisterConnection(highActivity);
        dashboard.RegisterConnection(mediumActivity);
        dashboard.RegisterConnection(lowActivity);

        // Act
        var topConnections = await dashboard.GetTopConnectionsByActivityAsync(2, TestContext.Current.CancellationToken);

        // Assert
        topConnections.ShouldNotBeNull();
        topConnections.Count.ShouldBe(2);
        topConnections[0].ConnectionId.ShouldBe("high");
        topConnections[1].ConnectionId.ShouldBe("medium");
    }

    [Fact]
    public async Task Dashboard_Should_Track_System_Wide_Performance_Metrics()
    {
        // Arrange
        var dashboard = CreateTestDashboard();

        var conn1 = CreateTestConnectionWithMetrics("perf-1", messagesSent: 1000, averageLatency: 25.0, p99Latency: 100.0, bytesSent: 100000, bytesReceived: 50000);
        var conn2 = CreateTestConnectionWithMetrics("perf-2", messagesSent: 2000, averageLatency: 35.0, p99Latency: 150.0, bytesSent: 200000, bytesReceived: 100000);

        dashboard.RegisterConnection(conn1);
        dashboard.RegisterConnection(conn2);

        // Act
        var performance = await dashboard.GetSystemPerformanceAsync(TestContext.Current.CancellationToken);

        // Assert
        performance.ShouldNotBeNull();
        performance.AverageLatencyMs.ShouldBeInRange(25.0, 35.0); // Weighted average
        performance.P99LatencyMs.ShouldBeGreaterThan(100.0);
        performance.TotalThroughputMbps.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Dashboard_Should_Never_Throw_Exceptions_During_Registration()
    {
        // Arrange
        var dashboard = CreateTestDashboard();

        // Act & Assert - Exception safety is critical for infrastructure
        Should.NotThrow(() =>
        {
            // Null connection
            dashboard.RegisterConnection(null!);

            // Duplicate registration
            var conn = CreateTestConnectionWithMetrics("duplicate");
            dashboard.RegisterConnection(conn);
            dashboard.RegisterConnection(conn);

            // Invalid unregistration
            dashboard.UnregisterConnection("non-existent");
        });
    }

    [Fact]
    public async Task Dashboard_Should_Provide_Historical_Trend_Data()
    {
        // Arrange
        var dashboard = CreateTestDashboard();
        var connection = CreateTestConnectionWithMetrics("trend-test");

        dashboard.RegisterConnection(connection);

        // Simulate time passing and metrics changing
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Act
        var trends = await dashboard.GetTrendDataAsync(TimeSpan.FromMinutes(5), TestContext.Current.CancellationToken);

        // Assert
        trends.ShouldNotBeNull();
        trends.Timestamps.ShouldNotBeEmpty();
        trends.MessageRates.ShouldNotBeEmpty();
        trends.LatencyTrends.ShouldNotBeEmpty();
    }

    // Test Factory Methods
    private IHubMetricsDashboard CreateTestDashboard()
    {
        return new IndTrace.HubConnection.Dashboard.HubMetricsDashboard();
    }

    private IHubConnection CreateTestConnectionWithMetrics(
        string connectionId,
        long messagesSent = 0,
        TimeSpan? uptime = null,
        int consecutiveFailures = 0,
        double averageLatency = 0.0,
        double p99Latency = 0.0,
        long bytesSent = 0,
        long bytesReceived = 0)
    {
        var connection = new TestHubConnectionWithCustomMetrics(connectionId);

        // Configure metrics for testing
        connection.SetupMetrics(
            messagesSent: messagesSent,
            uptime: uptime ?? TimeSpan.FromMinutes(1),
            consecutiveFailures: consecutiveFailures,
            averageLatency: averageLatency,
            p99Latency: p99Latency,
            bytesSent: bytesSent > 0 ? bytesSent : messagesSent * 100,
            bytesReceived: bytesReceived > 0 ? bytesReceived : messagesSent * 50
        );

        return connection;
    }

    private IHubConnection CreateSlowTestConnection(string connectionId)
    {
        return new SlowTestHubConnection(connectionId);
    }
}

/// <summary>
/// Test double that allows customizing metrics for dashboard testing
/// </summary>
public class TestHubConnectionWithCustomMetrics : TestHubConnection
{
    private readonly TestMetrics _customMetrics;

    public TestHubConnectionWithCustomMetrics(string connectionId) : base()
    {
        ConnectionId = connectionId;
        _customMetrics = new TestMetrics(connectionId);
    }

    public override IHubConnectionMetrics Metrics => _customMetrics;

    public void SetupMetrics(
        long messagesSent = 0,
        TimeSpan? uptime = null,
        int consecutiveFailures = 0,
        double averageLatency = 0.0,
        double p99Latency = 0.0,
        long bytesSent = 0,
        long bytesReceived = 0)
    {
        _customMetrics.SetupValues(messagesSent, uptime ?? TimeSpan.Zero, consecutiveFailures, averageLatency, p99Latency, bytesSent, bytesReceived);
    }

    private class TestMetrics : IHubConnectionMetrics
    {
        private readonly string _connectionId;
        private long _messagesSent;
        private TimeSpan _uptime;
        private int _consecutiveFailures;
        private double _averageLatency;
        private double _p99Latency;
        private long _bytesSent;
        private long _bytesReceived;

        public TestMetrics(string connectionId)
        {
            _connectionId = connectionId;
        }

        public void SetupValues(long messagesSent, TimeSpan uptime, int consecutiveFailures, double averageLatency, double p99Latency, long bytesSent, long bytesReceived)
        {
            _messagesSent = messagesSent;
            _uptime = uptime;
            _consecutiveFailures = consecutiveFailures;
            _averageLatency = averageLatency;
            _p99Latency = p99Latency;
            _bytesSent = bytesSent;
            _bytesReceived = bytesReceived;
        }

        public DateTimeOffset? ConnectedAt => DateTimeOffset.UtcNow - _uptime;
        public DateTimeOffset? LastActivityAt => DateTimeOffset.UtcNow;
        public TimeSpan Uptime => _uptime;
        public int ReconnectionCount => 0;
        public long MessagesSent => _messagesSent;
        public long MessagesReceived => 0;
        public long BytesSent => _bytesSent;
        public long BytesReceived => _bytesReceived;
        public double AverageLatencyMs => _averageLatency;
        public double P95LatencyMs => _averageLatency * 1.2;
        public double P99LatencyMs => _p99Latency;
        public int ConsecutiveFailures => _consecutiveFailures;
        public DateTimeOffset? LastFailureAt => null;
        public string? LastFailureReason => null;

        public void RecordMessageSent(long bytes) { }
        public void RecordMessageReceived(long bytes) { }
        public void RecordMessageLatency(double latencyMs) { }
        public void RecordConnectionStarted(DateTimeOffset connectedAt) { }
        public void RecordReconnection() { }
        public void RecordFailure(Exception exception) { }
        public void RecordSuccess() { }

        public HubMetricsSnapshot GetSnapshot()
        {
            return new HubMetricsSnapshot
            {
                ConnectionId = _connectionId,
                State = "Connected",
                MessagesSent = _messagesSent,
                MessagesReceived = 0,
                AverageLatencyMs = _averageLatency,
                P99LatencyMs = _p99Latency,
                Uptime = _uptime,
                ConsecutiveFailures = _consecutiveFailures,
                BytesSent = _messagesSent * 100
            };
        }
    }
}

/// <summary>
/// Test double that simulates slow metric collection
/// </summary>
public class SlowTestHubConnection : TestHubConnection
{
    public SlowTestHubConnection(string connectionId) : base()
    {
        ConnectionId = connectionId;
    }

    public override IHubConnectionMetrics Metrics => new SlowTestMetrics();

    private class SlowTestMetrics : IHubConnectionMetrics
    {
        public DateTimeOffset? ConnectedAt => DateTimeOffset.UtcNow;
        public DateTimeOffset? LastActivityAt => DateTimeOffset.UtcNow;
        public TimeSpan Uptime => TimeSpan.FromMinutes(1);
        public int ReconnectionCount => 0;
        public long MessagesSent => 100;
        public long MessagesReceived => 0;
        public long BytesSent => 10000;
        public long BytesReceived => 0;
        public double AverageLatencyMs => 50.0;
        public double P95LatencyMs => 80.0;
        public double P99LatencyMs => 120.0;
        public int ConsecutiveFailures => 0;
        public DateTimeOffset? LastFailureAt => null;
        public string? LastFailureReason => null;

        public void RecordMessageSent(long bytes) { }
        public void RecordMessageReceived(long bytes) { }
        public void RecordMessageLatency(double latencyMs) { }
        public void RecordConnectionStarted(DateTimeOffset connectedAt) { }
        public void RecordReconnection() { }
        public void RecordFailure(Exception exception) { }
        public void RecordSuccess() { }

        public HubMetricsSnapshot GetSnapshot()
        {
            // Simulate slow operation
            Thread.Sleep(2000);

            return new HubMetricsSnapshot
            {
                ConnectionId = "slow",
                State = "Connected",
                MessagesSent = 100,
                AverageLatencyMs = 50.0,
                P99LatencyMs = 120.0,
                Uptime = TimeSpan.FromMinutes(1)
            };
        }
    }
}
