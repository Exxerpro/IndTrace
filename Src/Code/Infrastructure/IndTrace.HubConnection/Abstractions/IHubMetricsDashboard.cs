// <copyright file="IHubMetricsDashboard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Abstractions;

/// <summary>
/// Infrastructure dashboard service for aggregating SignalR connection metrics.
/// Provides system-wide monitoring capabilities for DevOps/Infrastructure teams.
/// Key principle: Non-blocking aggregation with timeout protection.
/// </summary>
public interface IHubMetricsDashboard
{
    /// <summary>
    /// Register a connection for metrics tracking.
    /// Exception-safe - never throws, supports duplicate registrations.
    /// </summary>
    void RegisterConnection(IHubConnection? connection);

    /// <summary>
    /// Unregister a connection from metrics tracking.
    /// Exception-safe - never throws, supports non-existent connections.
    /// </summary>
    void UnregisterConnection(string connectionId);

    /// <summary>
    /// Unregister a connection from metrics tracking by instance identity, regardless of the key
    /// it was registered under (a real ConnectionId or the synthetic pending key used for
    /// connections registered before StartAsync assigned one).
    /// Exception-safe - never throws, supports non-registered connections.
    /// </summary>
    void UnregisterConnection(IHubConnection? connection);

    /// <summary>
    /// Get aggregated metrics across all registered connections.
    /// Non-blocking with timeout protection.
    /// </summary>
    Task<AggregatedMetrics> GetAggregatedMetricsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get system health status summary.
    /// Fast operation for health checks and monitoring alerts.
    /// </summary>
    Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top N connections by activity level.
    /// Useful for identifying hotspots and resource usage patterns.
    /// </summary>
    Task<IReadOnlyList<HubMetricsSnapshot>> GetTopConnectionsByActivityAsync(int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get system-wide performance metrics.
    /// Aggregated latency, throughput, and performance indicators.
    /// </summary>
    Task<SystemPerformanceMetrics> GetSystemPerformanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical trend data for time-series monitoring.
    /// Supports monitoring dashboards like Grafana, DataDog, etc.
    /// </summary>
    Task<TrendData> GetTrendDataAsync(TimeSpan period, CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregated metrics across all connections in the system.
/// Immutable snapshot for consistent reporting.
/// </summary>
public record AggregatedMetrics
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public int TotalConnections { get; init; }
    public int ActiveConnections { get; init; }
    public long TotalMessagesSent { get; init; }
    public long TotalMessagesReceived { get; init; }
    public long TotalBytesSent { get; init; }
    public long TotalBytesReceived { get; init; }
    public double MessagesPerSecond { get; init; }
    public TimeSpan AverageUptime { get; init; }
    public int TotalReconnections { get; init; }
    public int TotalFailures { get; init; }
}

/// <summary>
/// System health status summary for monitoring and alerting.
/// </summary>
public record HealthStatus
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public string Status { get; init; } = "Unknown"; // "Healthy", "Degraded", "Critical"
    public int TotalConnections { get; init; }
    public int HealthyConnections { get; init; }
    public int UnhealthyConnections { get; init; }
    public int ReconnectingConnections { get; init; }
    public string? OverallHealthReason { get; init; }
    public TimeSpan SystemUptime { get; init; }
}

/// <summary>
/// System-wide performance metrics for capacity planning and optimization.
/// </summary>
public record SystemPerformanceMetrics
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public double AverageLatencyMs { get; init; }
    public double P95LatencyMs { get; init; }
    public double P99LatencyMs { get; init; }
    public double TotalThroughputMbps { get; init; }
    public double PeakThroughputMbps { get; init; }
    public int ConcurrentConnectionsPeak { get; init; }
    public double CpuUtilizationPercent { get; init; }
    public double MemoryUtilizationPercent { get; init; }
}

/// <summary>
/// Historical trend data for time-series monitoring and analytics.
/// </summary>
public record TrendData
{
    public TimeSpan Period { get; init; }
    public IReadOnlyList<DateTimeOffset> Timestamps { get; init; } = Array.Empty<DateTimeOffset>();
    public IReadOnlyList<double> MessageRates { get; init; } = Array.Empty<double>();
    public IReadOnlyList<double> LatencyTrends { get; init; } = Array.Empty<double>();
    public IReadOnlyList<int> ConnectionCounts { get; init; } = Array.Empty<int>();
    public IReadOnlyList<double> ThroughputTrends { get; init; } = Array.Empty<double>();
    public IReadOnlyList<int> ErrorRates { get; init; } = Array.Empty<int>();
}