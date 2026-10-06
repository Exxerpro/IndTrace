// <copyright file="IHubConnectionMetricsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Metrics;

using IndTrace.HubConnection.Abstractions;
using Shouldly;
using Xunit;

/// <summary>
/// TDD tests for IHubConnectionMetrics - non-blocking metrics collection
/// Key principle: Message delivery FIRST, stats collection SECOND
/// If congestion → stats are forgotten (acceptable loss)
/// </summary>
public class IHubConnectionMetricsTests
{
    [Fact]
    public void Metrics_Should_Track_Messages_Sent_With_Lock_Free_Counters()
    {
        // Arrange - This test will drive the interface design
        // We need lock-free counters that NEVER block message sending

        // This test will fail until we implement IHubConnectionMetrics
        var metrics = CreateTestMetrics();

        // Act - Simulate concurrent message sending
        metrics.RecordMessageSent(1024); // 1KB message
        metrics.RecordMessageSent(2048); // 2KB message

        // Assert - Metrics should be captured without blocking
        metrics.MessagesSent.ShouldBe(2);
        metrics.BytesSent.ShouldBe(3072);
    }

    [Fact]
    public void Metrics_Should_Track_Connection_Uptime()
    {
        // Arrange
        var metrics = CreateTestMetrics();
        var startTime = DateTimeOffset.UtcNow;

        // Act
        metrics.RecordConnectionStarted(startTime);

        // Assert
        metrics.ConnectedAt.ShouldBe(startTime);
        metrics.Uptime.ShouldBeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task Metrics_Should_Record_Latency_Without_Blocking_Messages()
    {
        // Arrange
        var metrics = CreateTestMetrics();

        // Act - Fire-and-forget latency recording
        metrics.RecordMessageLatency(25.5); // 25.5ms
        metrics.RecordMessageLatency(50.0); // 50ms
        metrics.RecordMessageLatency(15.2); // 15.2ms

        // Wait a moment for fire-and-forget calculation to complete
        // This is acceptable in tests to verify the eventual consistency
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert - Should calculate percentiles from circular buffer
        metrics.AverageLatencyMs.ShouldBeGreaterThan(0);
        metrics.P95LatencyMs.ShouldBeGreaterThan(0);
        metrics.P99LatencyMs.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Metrics_Should_Track_Reconnection_Count()
    {
        // Arrange
        var metrics = CreateTestMetrics();

        // Act
        metrics.RecordReconnection();
        metrics.RecordReconnection();

        // Assert
        metrics.ReconnectionCount.ShouldBe(2);
    }

    [Fact]
    public void Metrics_Should_Record_Failures_With_Timestamps()
    {
        // Arrange
        var metrics = CreateTestMetrics();
        var exception = new InvalidOperationException("Network failure");

        // Act
        metrics.RecordFailure(exception);

        // Assert
        metrics.ConsecutiveFailures.ShouldBe(1);
        metrics.LastFailureAt.ShouldNotBeNull();
        metrics.LastFailureReason.ShouldNotBeNull();
        metrics.LastFailureReason.ShouldContain("Network failure");
    }

    [Fact]
    public void Metrics_Should_Provide_Immutable_Snapshot_For_Dashboard()
    {
        // Arrange
        var metrics = CreateTestMetrics();
        metrics.RecordMessageSent(1024);
        metrics.RecordConnectionStarted(DateTimeOffset.UtcNow.AddMinutes(-5));

        // Act
        var snapshot = metrics.GetSnapshot();

        // Assert - Snapshot should be immutable and cacheable
        snapshot.ShouldNotBeNull();
        snapshot.MessagesSent.ShouldBe(1);
        snapshot.Uptime.ShouldBeGreaterThan(TimeSpan.FromMinutes(4));
        snapshot.ConnectionId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Metrics_Collection_Should_Never_Throw_Exceptions()
    {
        // Arrange
        var metrics = CreateTestMetrics();

        // Act & Assert - Metrics collection must be exception-safe
        // Message delivery is FIRST CITIZEN - metrics cannot break it
        Should.NotThrow(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                metrics.RecordMessageSent(i);
                metrics.RecordMessageLatency(i * 0.1);
            }
        });
    }

    private IHubConnectionMetrics CreateTestMetrics()
    {
        return new IndTrace.HubConnection.Metrics.HubConnectionMetrics("test-connection-123");
    }
}