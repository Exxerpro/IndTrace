// <copyright file="TestHubConnection.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.TestDoubles;

using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Metrics;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Test double for IHubConnection following Uncle Bob's test principles.
/// FAST - No network calls
/// INDEPENDENT - No external dependencies  
/// REPEATABLE - Same result every time
/// </summary>
public class TestHubConnection : IHubConnection
{
    private readonly List<(string method, object?[] args)> _sentMessages = new();
    private readonly Dictionary<string, List<Delegate>> _handlers = new();
    private readonly IHubConnectionMetrics _metrics;
    private bool _disposed;

    public TestHubConnection()
    {
        _metrics = new HubConnectionMetrics("test-connection");
    }

    public HubConnectionState State { get; set; } = HubConnectionState.Disconnected;
    public string? ConnectionId { get; set; }
    public virtual IHubConnectionMetrics Metrics => _metrics;

    /// <summary>
    /// Gets a value indicating whether <see cref="DisposeAsync"/> has been called.
    /// Lets tests verify that evicted connections are actually disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    public event Func<Exception?, Task>? Reconnecting;
    public event Func<string?, Task>? Reconnected;
    public event Func<Exception?, Task>? Closed;

    public virtual Task StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        State = HubConnectionState.Connected;
        ConnectionId = Guid.NewGuid().ToString();
        _metrics.RecordConnectionStarted(DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        State = HubConnectionState.Disconnected;
        ConnectionId = null;
        return Task.CompletedTask;
    }

    public virtual Task SendAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        if (State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException($"Cannot send when state is {State}");
        }

        _sentMessages.Add((methodName, args));
        _metrics.RecordMessageSent(100); // Estimated 100 bytes per message
        return Task.CompletedTask;
    }

    public virtual Task<T?> InvokeAsync<T>(string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        if (State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException($"Cannot invoke when state is {State}");
        }

        _sentMessages.Add((methodName, args));
        return Task.FromResult(default(T));
    }

    public IDisposable On<T>(string methodName, Func<T, Task> handler)
    {
        ThrowIfDisposed();

        if (!_handlers.ContainsKey(methodName))
        {
            _handlers[methodName] = new List<Delegate>();
        }

        _handlers[methodName].Add(handler);

        return new HandlerDisposable(() =>
        {
            if (_handlers.TryGetValue(methodName, out var handlers))
            {
                handlers.Remove(handler);
            }
        });
    }

    public IDisposable On<T1, T2>(string methodName, Func<T1, T2, Task> handler)
    {
        ThrowIfDisposed();

        if (!_handlers.ContainsKey(methodName))
        {
            _handlers[methodName] = new List<Delegate>();
        }

        _handlers[methodName].Add(handler);

        return new HandlerDisposable(() =>
        {
            if (_handlers.TryGetValue(methodName, out var handlers))
            {
                handlers.Remove(handler);
            }
        });
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;

        _disposed = true;
        State = HubConnectionState.Disconnected;
        ConnectionId = null;
        _handlers.Clear();
        _sentMessages.Clear();

        return ValueTask.CompletedTask;
    }

    // Test helpers
    public void SimulateReconnecting(Exception? error = null)
    {
        State = HubConnectionState.Reconnecting;
        Reconnecting?.Invoke(error);
    }

    public void SimulateReconnected()
    {
        State = HubConnectionState.Connected;
        ConnectionId = Guid.NewGuid().ToString();
        Reconnected?.Invoke(ConnectionId);
    }

    public void SimulateConnectionClosed(Exception? error = null)
    {
        State = HubConnectionState.Disconnected;
        ConnectionId = null;
        Closed?.Invoke(error);
    }

    public async Task SimulateMessage<T>(string methodName, T data, CancellationToken cancellationToken = default)
    {
        if (_handlers.TryGetValue(methodName, out var handlers))
        {
            foreach (var handler in handlers.OfType<Func<T, Task>>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await handler(data).ConfigureAwait(false);
            }
        }
    }

    public async Task SimulateMessage<T1, T2>(string methodName, T1 arg1, T2 arg2, CancellationToken cancellationToken = default)
    {
        if (_handlers.TryGetValue(methodName, out var handlers))
        {
            foreach (var handler in handlers.OfType<Func<T1, T2, Task>>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await handler(arg1, arg2).ConfigureAwait(false);
            }
        }
    }

    // Verification helpers
    public bool WasMessageSent(string methodName) =>
        _sentMessages.Any(m => m.method == methodName);

    public T? GetLastSentMessageArg<T>(string methodName, int argIndex = 0) =>
        _sentMessages
            .Where(m => m.method == methodName)
            .Select(m => m.args.Length > argIndex ? (T?)m.args[argIndex] : default)
            .LastOrDefault();

    public int GetMessageCount(string methodName) =>
        _sentMessages.Count(m => m.method == methodName);

    public IReadOnlyList<(string method, object?[] args)> GetAllSentMessages() =>
        _sentMessages.AsReadOnly();

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TestHubConnection));
        }
    }

    private sealed class HandlerDisposable : IDisposable
    {
        private readonly Action _dispose;

        public HandlerDisposable(Action dispose) => _dispose = dispose;

        public void Dispose() => _dispose();
    }
}