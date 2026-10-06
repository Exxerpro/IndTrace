// <copyright file="HubConnectionMetrics.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Metrics;

using IndTrace.HubConnection.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// Lock-free, non-blocking metrics collection for SignalR HubConnection.
/// Key principle: Message delivery is FIRST CITIZEN, stats collection is SECOND.
/// Uses Interlocked operations and fire-and-forget patterns to ensure zero blocking.
/// </summary>
public sealed class HubConnectionMetrics : IHubConnectionMetrics
{
    private readonly string _connectionId;
    private readonly CircularLatencyBuffer _latencyBuffer;
    private readonly Func<string>? _stateProvider;

    // Lock-free counters using Interlocked operations
    private long _messagesSent;
    private long _messagesReceived;
    private long _bytesSent;
    private long _bytesReceived;
    private int _reconnectionCount;
    private int _consecutiveFailures;

    // Thread-safe timestamps using atomic operations
    private long _connectedAtTicks;
    private long _lastActivityAtTicks;
    private long _lastFailureAtTicks;
    private string? _lastFailureReason;

    public HubConnectionMetrics(string? connectionId, Func<string>? stateProvider = null)
    {
        _connectionId = connectionId ?? string.Empty;
        _stateProvider = stateProvider; // Live state source; snapshots report "Unknown" without one.
        _latencyBuffer = new CircularLatencyBuffer(1000); // 1000 samples, ~4KB memory
    }

    // Properties - Thread-safe reads
    public DateTimeOffset? ConnectedAt
    {
        get
        {
            var ticks = Interlocked.Read(ref _connectedAtTicks);
            return ticks == 0 ? null : new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }

    public DateTimeOffset? LastActivityAt
    {
        get
        {
            var ticks = Interlocked.Read(ref _lastActivityAtTicks);
            return ticks == 0 ? null : new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }

    public TimeSpan Uptime
    {
        get
        {
            var connectedTicks = Interlocked.Read(ref _connectedAtTicks);
            return connectedTicks == 0
                ? TimeSpan.Zero
                : DateTimeOffset.UtcNow - new DateTimeOffset(connectedTicks, TimeSpan.Zero);
        }
    }
    public int ReconnectionCount => _reconnectionCount;
    public long MessagesSent => Interlocked.Read(ref _messagesSent);
    public long MessagesReceived => Interlocked.Read(ref _messagesReceived);
    public long BytesSent => Interlocked.Read(ref _bytesSent);
    public long BytesReceived => Interlocked.Read(ref _bytesReceived);
    public double AverageLatencyMs => _latencyBuffer.Average;
    public double P95LatencyMs => _latencyBuffer.P95;
    public double P99LatencyMs => _latencyBuffer.P99;
    public int ConsecutiveFailures => _consecutiveFailures;
    public DateTimeOffset? LastFailureAt
    {
        get
        {
            var ticks = Interlocked.Read(ref _lastFailureAtTicks);
            return ticks == 0 ? null : new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }
    public string? LastFailureReason => _lastFailureReason;

    // Recording Methods - Exception-safe, non-blocking
    public void RecordMessageSent(long bytes)
    {
        try
        {
            Interlocked.Increment(ref _messagesSent);
            Interlocked.Add(ref _bytesSent, bytes);
            Interlocked.Exchange(ref _lastActivityAtTicks, DateTimeOffset.UtcNow.Ticks);
        }
        catch
        {
            // Swallow exceptions - metrics CANNOT break message delivery
            // This is acceptable loss as per requirement
        }
    }

    public void RecordMessageReceived(long bytes)
    {
        try
        {
            Interlocked.Increment(ref _messagesReceived);
            Interlocked.Add(ref _bytesReceived, bytes);
            Interlocked.Exchange(ref _lastActivityAtTicks, DateTimeOffset.UtcNow.Ticks);
        }
        catch
        {
            // Swallow exceptions - metrics are secondary
        }
    }

    public void RecordMessageLatency(double latencyMs)
    {
        try
        {
            // Fire-and-forget - if buffer is congested, drop the sample
            _latencyBuffer.TryAdd(latencyMs);
        }
        catch
        {
            // Acceptable loss - latency stats are not critical
        }
    }

    public void RecordConnectionStarted(DateTimeOffset connectedAt)
    {
        try
        {
            Interlocked.Exchange(ref _connectedAtTicks, connectedAt.Ticks);
            Interlocked.Exchange(ref _lastActivityAtTicks, connectedAt.Ticks);
        }
        catch
        {
            // Swallow exceptions
        }
    }

    public void RecordReconnection()
    {
        try
        {
            Interlocked.Increment(ref _reconnectionCount);
        }
        catch
        {
            // Swallow exceptions
        }
    }

    public void RecordFailure(Exception exception)
    {
        try
        {
            Interlocked.Increment(ref _consecutiveFailures);
            Interlocked.Exchange(ref _lastFailureAtTicks, DateTimeOffset.UtcNow.Ticks);
            _lastFailureReason = exception?.Message ?? "Unknown error";
        }
        catch
        {
            // Swallow exceptions
        }
    }

    public void RecordSuccess()
    {
        try
        {
            Interlocked.Exchange(ref _consecutiveFailures, 0);
        }
        catch
        {
            // Swallow exceptions
        }
    }

    public HubMetricsSnapshot GetSnapshot()
    {
        try
        {
            var uptime = Uptime;
            var messagesSent = MessagesSent;
            var messagesReceived = MessagesReceived;

            // Calculate messages per second (avoid division by zero)
            var messagesPerSecond = uptime.TotalSeconds > 0
                ? (messagesSent + messagesReceived) / uptime.TotalSeconds
                : 0.0;

            return new HubMetricsSnapshot
            {
                Timestamp = DateTimeOffset.UtcNow,
                ConnectionId = _connectionId,
                State = ResolveState(), // Truthful live state; never a hardcoded "Connected".
                MessagesSent = messagesSent,
                MessagesReceived = messagesReceived,
                MessagesPerSecond = messagesPerSecond,
                AverageLatencyMs = AverageLatencyMs,
                P95LatencyMs = P95LatencyMs,
                P99LatencyMs = P99LatencyMs,
                Uptime = uptime,
                ReconnectionCount = ReconnectionCount,
                ConsecutiveFailures = ConsecutiveFailures,
                LastFailureAt = LastFailureAt,
                LastFailureReason = LastFailureReason,
                BytesSent = BytesSent,
                BytesReceived = BytesReceived
            };
        }
        catch
        {
            // Return empty snapshot if anything fails
            return new HubMetricsSnapshot
            {
                ConnectionId = _connectionId,
                State = "Unknown"
            };
        }
    }

    private string ResolveState()
    {
        try
        {
            return _stateProvider?.Invoke() ?? "Unknown";
        }
        catch
        {
            // A failing provider must not break snapshotting — report the state as unknown.
            return "Unknown";
        }
    }
}

/// <summary>
/// Lock-free circular buffer for latency samples.
/// Uses atomic operations to ensure thread safety without blocking.
/// If buffer is full, old samples are overwritten (acceptable loss).
/// </summary>
internal sealed class CircularLatencyBuffer
{
    private readonly double[] _buffer;
    private readonly int _capacity;
    private long _writeIndex;
    private long _count;

    // Lazily computed statistics: writes (per message) only flip the dirty flag; the rare readers
    // (metrics snapshots) pay for the recompute. This removes the former per-sample Task.Run sort
    // and makes stats deterministic immediately after TryAdd returns.
    private double _average;
    private double _p95;
    private double _p99;
    private int _dirty;

    public CircularLatencyBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = new double[capacity];
    }

    public double Average
    {
        get
        {
            RecomputeIfDirty();
            return _average;
        }
    }

    public double P95
    {
        get
        {
            RecomputeIfDirty();
            return _p95;
        }
    }

    public double P99
    {
        get
        {
            RecomputeIfDirty();
            return _p99;
        }
    }

    public bool TryAdd(double latency)
    {
        try
        {
            var index = Interlocked.Increment(ref _writeIndex) - 1;
            _buffer[index % _capacity] = latency;

            var currentCount = Interlocked.Increment(ref _count);
            if (currentCount > _capacity)
            {
                Interlocked.Exchange(ref _count, _capacity);
            }

            // Mark statistics stale; they are recomputed synchronously on the next property read.
            Interlocked.Exchange(ref _dirty, 1);
            return true;
        }
        catch
        {
            return false; // Acceptable loss
        }
    }

    private void RecomputeIfDirty()
    {
        // Claim the dirty flag first so concurrent readers do at most one redundant recompute each
        // and a write racing the recompute re-dirties the buffer for the next read.
        if (Interlocked.Exchange(ref _dirty, 0) == 0)
        {
            return;
        }

        UpdateStatistics();
    }

    private void UpdateStatistics()
    {
        try
        {
            var currentCount = Math.Min(_count, _capacity);
            if (currentCount == 0) return;

            // Create snapshot of current data
            var samples = new double[currentCount];
            var actualCount = 0;

            for (int i = 0; i < currentCount; i++)
            {
                var value = _buffer[i];
                if (value > 0) // Valid latency sample
                {
                    samples[actualCount++] = value;
                }
            }

            if (actualCount == 0) return;

            // Resize to actual valid samples
            if (actualCount < samples.Length)
            {
                Array.Resize(ref samples, actualCount);
            }

            // Calculate statistics
            _average = samples.Average();

            Array.Sort(samples);
            _p95 = GetPercentile(samples, 0.95);
            _p99 = GetPercentile(samples, 0.99);
        }
        catch
        {
            // Calculation failed - keep old values
        }
    }

    private static double GetPercentile(double[] sortedArray, double percentile)
    {
        if (sortedArray.Length == 0) return 0;

        var index = (int)Math.Ceiling(sortedArray.Length * percentile) - 1;
        index = Math.Max(0, Math.Min(index, sortedArray.Length - 1));

        return sortedArray[index];
    }
}
