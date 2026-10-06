// <copyright file="IHubConnectionMetrics.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Abstractions;

/// <summary>
/// Non-blocking metrics collection for SignalR HubConnection infrastructure monitoring.
/// Key principle: Message delivery is FIRST CITIZEN, stats collection is SECOND.
/// If congestion occurs, stats are forgotten (acceptable loss).
/// </summary>
public interface IHubConnectionMetrics
{
    // Connection Health - Thread-safe properties
    DateTimeOffset? ConnectedAt { get; }
    DateTimeOffset? LastActivityAt { get; }
    TimeSpan Uptime { get; }
    int ReconnectionCount { get; }

    // Message Statistics - Lock-free counters using Interlocked
    long MessagesSent { get; }
    long MessagesReceived { get; }
    long BytesSent { get; }
    long BytesReceived { get; }

    // Performance Metrics - Circular buffer, no allocations
    double AverageLatencyMs { get; }
    double P95LatencyMs { get; }
    double P99LatencyMs { get; }

    // Connection Quality
    int ConsecutiveFailures { get; }
    DateTimeOffset? LastFailureAt { get; }
    string? LastFailureReason { get; }

    // Recording Methods - Fire-and-forget, exception-safe
    void RecordMessageSent(long bytes);
    void RecordMessageReceived(long bytes);
    void RecordMessageLatency(double latencyMs);
    void RecordConnectionStarted(DateTimeOffset connectedAt);
    void RecordReconnection();
    void RecordFailure(Exception exception);
    void RecordSuccess(); // Reset consecutive failures

    // Snapshot for Dashboard - Immutable, cacheable
    HubMetricsSnapshot GetSnapshot();
}

/// <summary>
/// Immutable snapshot of hub connection metrics for dashboard consumption.
/// Designed for JSON serialization and caching.
/// </summary>
public record HubMetricsSnapshot
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public string ConnectionId { get; init; } = string.Empty;
    public string State { get; init; } = "Unknown";
    public long MessagesSent { get; init; }
    public long MessagesReceived { get; init; }
    public double MessagesPerSecond { get; init; }
    public double AverageLatencyMs { get; init; }
    public double P95LatencyMs { get; init; }
    public double P99LatencyMs { get; init; }
    public TimeSpan Uptime { get; init; }
    public int ReconnectionCount { get; init; }
    public int ConsecutiveFailures { get; init; }
    public DateTimeOffset? LastFailureAt { get; init; }
    public string? LastFailureReason { get; init; }
    public long BytesSent { get; init; }
    public long BytesReceived { get; init; }
}
