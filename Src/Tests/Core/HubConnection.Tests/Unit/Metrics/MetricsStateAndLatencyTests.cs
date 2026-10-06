// <copyright file="MetricsStateAndLatencyTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Metrics;

using IndTrace.HubConnection.Metrics;
using Shouldly;
using Xunit;

/// <summary>
/// Regression tests for metrics truthfulness (issue #106): snapshots hardcoded
/// State = "Connected" regardless of reality, and latency statistics were computed by a racy
/// fire-and-forget Task.Run per sample. Snapshots must report the live state (or "Unknown"),
/// and latency stats must be deterministic immediately after recording.
/// </summary>
public class MetricsStateAndLatencyTests
{
    [Fact]
    public void GetSnapshot_Should_Report_State_From_Provider()
    {
        // Arrange - pre-fix this reported "Connected" no matter what the provider said.
        var metrics = new HubConnectionMetrics("conn-1", () => "Disconnected");

        // Act
        var snapshot = metrics.GetSnapshot();

        // Assert
        snapshot.State.ShouldBe("Disconnected");
        snapshot.ConnectionId.ShouldBe("conn-1");
    }

    [Fact]
    public void GetSnapshot_Should_Report_Unknown_When_No_Provider()
    {
        // Arrange - without a live state source the truthful answer is "Unknown", not "Connected".
        var metrics = new HubConnectionMetrics("conn-2");

        // Act
        var snapshot = metrics.GetSnapshot();

        // Assert
        snapshot.State.ShouldBe("Unknown");
    }

    [Fact]
    public void GetSnapshot_Should_Report_Unknown_When_Provider_Throws()
    {
        // Arrange
        var metrics = new HubConnectionMetrics("conn-3", () => throw new InvalidOperationException("provider broke"));
        metrics.RecordMessageSent(100);

        // Act
        var snapshot = metrics.GetSnapshot();

        // Assert - a failing provider degrades the state only, not the whole snapshot.
        snapshot.State.ShouldBe("Unknown");
        snapshot.MessagesSent.ShouldBe(1);
    }

    [Fact]
    public void Latency_Statistics_Should_Be_Deterministic_Immediately_After_Recording()
    {
        // Arrange
        var metrics = new HubConnectionMetrics("conn-4");

        // Act - pre-fix each Record fired a background Task.Run, so an immediate read raced it.
        metrics.RecordMessageLatency(5);
        metrics.RecordMessageLatency(5);
        metrics.RecordMessageLatency(5);

        // Assert - stats are computed lazily+synchronously on read: no background race window.
        metrics.AverageLatencyMs.ShouldBe(5.0);
        metrics.P95LatencyMs.ShouldBe(5.0);
        metrics.P99LatencyMs.ShouldBe(5.0);
    }

    [Fact]
    public void Latency_Statistics_Should_Update_Deterministically_Across_Batches()
    {
        // Arrange
        var metrics = new HubConnectionMetrics("conn-5");

        metrics.RecordMessageLatency(10);
        metrics.AverageLatencyMs.ShouldBe(10.0);

        // Act - a later sample re-dirties the buffer and the next read recomputes synchronously.
        metrics.RecordMessageLatency(20);

        // Assert
        metrics.AverageLatencyMs.ShouldBe(15.0);
        metrics.P99LatencyMs.ShouldBe(20.0);
    }
}
