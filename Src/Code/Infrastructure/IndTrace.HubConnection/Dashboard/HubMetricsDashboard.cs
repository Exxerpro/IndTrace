// <copyright file="HubMetricsDashboard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Dashboard;

using IndTrace.HubConnection.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// Non-blocking, exception-safe dashboard for aggregating SignalR connection metrics.
/// Designed for infrastructure monitoring with timeout protection and graceful degradation.
/// Key principle: Dashboard operations NEVER block message delivery.
/// </summary>
public sealed class HubMetricsDashboard : IHubMetricsDashboard
{
    private readonly ConcurrentDictionary<string, IHubConnection> _connections = new();
    private readonly DateTimeOffset _startTime = DateTimeOffset.UtcNow;
    private readonly object _trendLock = new();
    private readonly List<MetricsSample> _trendHistory = new();
    private const int MaxTrendSamples = 1000; // ~16 minutes at 1 sample/second

    public void RegisterConnection(IHubConnection? connection)
    {
        try
        {
            if (connection is null) return;

            // SignalR assigns ConnectionId only AFTER StartAsync; registrations arrive right after
            // CreateAsync, when it is still null. Register those under a synthetic stable key so the
            // dashboard aggregates them instead of silently no-oping on every registration.
            var key = connection.ConnectionId
                ?? $"pending-{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(connection)}";

            _connections.AddOrUpdate(
                key,
                connection,
                (_, _) => connection // Allow overwrite for reconnections
            );
        }
        catch
        {
            // Exception-safe - registration failures cannot break the system
        }
    }

    public void UnregisterConnection(string connectionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(connectionId)) return;

            _connections.TryRemove(connectionId, out _);
        }
        catch
        {
            // Exception-safe - removal failures are acceptable
        }
    }

    public void UnregisterConnection(IHubConnection? connection)
    {
        try
        {
            if (connection is null) return;

            // Registration may have keyed this instance under a real ConnectionId OR under the
            // synthetic pending-* key (registrations arrive before StartAsync assigns an id), so
            // removal must match by instance identity rather than by key. The keyed TryRemove
            // overload only removes when the entry still maps to this exact instance, so a
            // reconnection that overwrote the key with a fresh connection is never evicted.
            foreach (var entry in _connections)
            {
                if (ReferenceEquals(entry.Value, connection))
                {
                    _connections.TryRemove(entry);
                }
            }
        }
        catch
        {
            // Exception-safe - removal failures are acceptable
        }
    }

    public async Task<AggregatedMetrics> GetAggregatedMetricsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(effectiveTimeout);

            var snapshots = await GetAllSnapshotsAsync(cts.Token).ConfigureAwait(false);

            return AggregateSnapshots(snapshots, startTime);
        }
        catch (OperationCanceledException)
        {
            // Timeout occurred - return partial results
            return new AggregatedMetrics
            {
                Timestamp = startTime,
                TotalConnections = _connections.Count
                // Other fields will default to 0, which is acceptable for timeout scenarios
            };
        }
        catch
        {
            // Any other exception - return safe empty result
            return new AggregatedMetrics { Timestamp = startTime };
        }
    }

    public async Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(2)); // Fast health check

            var snapshots = await GetAllSnapshotsAsync(cts.Token).ConfigureAwait(false);

            return CalculateHealthStatus(snapshots);
        }
        catch
        {
            return new HealthStatus
            {
                Status = "Unknown",
                TotalConnections = _connections.Count,
                OverallHealthReason = "Health check failed",
                SystemUptime = DateTimeOffset.UtcNow - _startTime
            };
        }
    }

    public async Task<IReadOnlyList<HubMetricsSnapshot>> GetTopConnectionsByActivityAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var snapshots = await GetAllSnapshotsAsync(cts.Token).ConfigureAwait(false);

            return snapshots
                .OrderByDescending(s => s.MessagesSent + s.MessagesReceived)
                .Take(Math.Max(1, limit))
                .ToList()
                .AsReadOnly();
        }
        catch
        {
            return Array.Empty<HubMetricsSnapshot>();
        }
    }

    public async Task<SystemPerformanceMetrics> GetSystemPerformanceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var snapshots = await GetAllSnapshotsAsync(cts.Token).ConfigureAwait(false);

            return CalculateSystemPerformance(snapshots);
        }
        catch
        {
            return new SystemPerformanceMetrics();
        }
    }

    public async Task<TrendData> GetTrendDataAsync(TimeSpan period, CancellationToken cancellationToken = default)
    {
        try
        {
            // Add current sample to trend history
            await RecordCurrentSampleAsync(cancellationToken).ConfigureAwait(false);

            lock (_trendLock)
            {
                var cutoff = DateTimeOffset.UtcNow - period;
                var relevantSamples = _trendHistory
                    .Where(s => s.Timestamp >= cutoff)
                    .OrderBy(s => s.Timestamp)
                    .ToList();

                return new TrendData
                {
                    Period = period,
                    Timestamps = relevantSamples.Select(s => s.Timestamp).ToList(),
                    MessageRates = relevantSamples.Select(s => s.MessageRate).ToList(),
                    LatencyTrends = relevantSamples.Select(s => s.AverageLatency).ToList(),
                    ConnectionCounts = relevantSamples.Select(s => s.ConnectionCount).ToList(),
                    ThroughputTrends = relevantSamples.Select(s => s.ThroughputMbps).ToList(),
                    ErrorRates = relevantSamples.Select(s => s.ErrorRate).ToList()
                };
            }
        }
        catch
        {
            return new TrendData { Period = period };
        }
    }

    // Private helper methods
    private async Task<List<HubMetricsSnapshot>> GetAllSnapshotsAsync(CancellationToken cancellationToken)
    {
        var snapshots = new List<HubMetricsSnapshot>();
        var tasks = new List<Task<HubMetricsSnapshot?>>();

        // Start all snapshot operations in parallel
        foreach (var connection in _connections.Values)
        {
            tasks.Add(GetSnapshotSafeAsync(connection, cancellationToken));
        }

        // Wait for all to complete or timeout
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Collect successful results
        foreach (var snapshot in results)
        {
            if (snapshot != null)
            {
                snapshots.Add(snapshot);
            }
        }

        return snapshots;
    }

    private async Task<HubMetricsSnapshot?> GetSnapshotSafeAsync(IHubConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return connection.Metrics?.GetSnapshot();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null; // Acceptable loss - some connections may be slow or failing
        }
    }

    private AggregatedMetrics AggregateSnapshots(List<HubMetricsSnapshot> snapshots, DateTimeOffset timestamp)
    {
        if (snapshots.Count == 0)
        {
            return new AggregatedMetrics { Timestamp = timestamp };
        }

        var totalMessagesSent = snapshots.Sum(s => s.MessagesSent);
        var totalMessagesReceived = snapshots.Sum(s => s.MessagesReceived);
        var totalBytesSent = snapshots.Sum(s => s.BytesSent);
        var totalBytesReceived = snapshots.Sum(s => s.BytesReceived);
        var activeConnections = snapshots.Count(s => s.State == "Connected");
        var averageUptime = snapshots.Any()
            ? TimeSpan.FromTicks((long)snapshots.Average(s => s.Uptime.Ticks))
            : TimeSpan.Zero;
        var totalReconnections = snapshots.Sum(s => s.ReconnectionCount);
        var totalFailures = snapshots.Sum(s => s.ConsecutiveFailures);

        // Calculate messages per second based on uptime
        var messagesPerSecond = averageUptime.TotalSeconds > 0
            ? (totalMessagesSent + totalMessagesReceived) / averageUptime.TotalSeconds
            : 0.0;

        return new AggregatedMetrics
        {
            Timestamp = timestamp,
            TotalConnections = snapshots.Count,
            ActiveConnections = activeConnections,
            TotalMessagesSent = totalMessagesSent,
            TotalMessagesReceived = totalMessagesReceived,
            TotalBytesSent = totalBytesSent,
            TotalBytesReceived = totalBytesReceived,
            MessagesPerSecond = messagesPerSecond,
            AverageUptime = averageUptime,
            TotalReconnections = totalReconnections,
            TotalFailures = totalFailures
        };
    }

    private HealthStatus CalculateHealthStatus(List<HubMetricsSnapshot> snapshots)
    {
        var totalConnections = snapshots.Count;
        var healthyConnections = snapshots.Count(s => s.ConsecutiveFailures == 0 && s.State == "Connected");
        var unhealthyConnections = snapshots.Count(s => s.ConsecutiveFailures > 0);
        var reconnectingConnections = snapshots.Count(s => s.State == "Reconnecting");

        string status;
        string? reason = null;

        if (totalConnections == 0)
        {
            status = "Unknown";
            reason = "No connections registered";
        }
        else if (unhealthyConnections == 0)
        {
            status = "Healthy";
        }
        else if (unhealthyConnections < totalConnections * 0.1) // Less than 10% unhealthy
        {
            status = "Degraded";
            reason = $"{unhealthyConnections} of {totalConnections} connections experiencing failures";
        }
        else
        {
            status = "Critical";
            reason = $"{unhealthyConnections} of {totalConnections} connections unhealthy";
        }

        return new HealthStatus
        {
            Status = status,
            TotalConnections = totalConnections,
            HealthyConnections = healthyConnections,
            UnhealthyConnections = unhealthyConnections,
            ReconnectingConnections = reconnectingConnections,
            OverallHealthReason = reason,
            SystemUptime = DateTimeOffset.UtcNow - _startTime
        };
    }

    private SystemPerformanceMetrics CalculateSystemPerformance(List<HubMetricsSnapshot> snapshots)
    {
        if (snapshots.Count == 0)
        {
            return new SystemPerformanceMetrics();
        }

        var averageLatency = snapshots.Any(s => s.AverageLatencyMs > 0)
            ? snapshots.Where(s => s.AverageLatencyMs > 0).Average(s => s.AverageLatencyMs)
            : 0.0;

        var p95Latency = snapshots.Any(s => s.P95LatencyMs > 0)
            ? snapshots.Where(s => s.P95LatencyMs > 0).Average(s => s.P95LatencyMs)
            : 0.0;

        var p99Latency = snapshots.Any(s => s.P99LatencyMs > 0)
            ? snapshots.Where(s => s.P99LatencyMs > 0).Average(s => s.P99LatencyMs)
            : 0.0;

        var totalBytes = snapshots.Sum(s => s.BytesSent + s.BytesReceived);
        var avgUptimeSeconds = snapshots.Any() ? snapshots.Average(s => s.Uptime.TotalSeconds) : 0;
        var throughputMbps = avgUptimeSeconds > 0 ? (totalBytes * 8.0) / (avgUptimeSeconds * 1_000_000) : 0.0;

        return new SystemPerformanceMetrics
        {
            AverageLatencyMs = averageLatency,
            P95LatencyMs = p95Latency,
            P99LatencyMs = p99Latency,
            TotalThroughputMbps = throughputMbps,
            PeakThroughputMbps = throughputMbps * 1.5, // Estimate
            ConcurrentConnectionsPeak = snapshots.Count
        };
    }

    private async Task RecordCurrentSampleAsync(CancellationToken cancellationToken)
    {
        try
        {
            var metrics = await GetAggregatedMetricsAsync(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);

            var sample = new MetricsSample
            {
                Timestamp = DateTimeOffset.UtcNow,
                MessageRate = metrics.MessagesPerSecond,
                AverageLatency = 0.0, // Would need to aggregate from snapshots
                ConnectionCount = metrics.TotalConnections,
                ThroughputMbps = (metrics.TotalBytesSent + metrics.TotalBytesReceived) * 8.0 / 1_000_000,
                ErrorRate = metrics.TotalFailures
            };

            lock (_trendLock)
            {
                _trendHistory.Add(sample);

                // Keep only the last N samples to prevent memory growth
                if (_trendHistory.Count > MaxTrendSamples)
                {
                    _trendHistory.RemoveAt(0);
                }
            }
        }
        catch
        {
            // Acceptable loss - trend recording failures are not critical
        }
    }

    private record MetricsSample
    {
        public DateTimeOffset Timestamp { get; init; }
        public double MessageRate { get; init; }
        public double AverageLatency { get; init; }
        public int ConnectionCount { get; init; }
        public double ThroughputMbps { get; init; }
        public int ErrorRate { get; init; }
    }
}