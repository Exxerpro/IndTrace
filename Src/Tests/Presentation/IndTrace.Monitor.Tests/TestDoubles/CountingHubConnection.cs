// <copyright file="CountingHubConnection.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Monitor.Tests.TestDoubles;

using System.Collections.Concurrent;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Metrics;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Minimal counting test double for <see cref="IHubConnection"/>.
/// Records how many times each event handler is attached so a regression test can prove that the
/// worker registers handlers exactly once, regardless of how many watchdog passes run (issue #71).
/// </summary>
public sealed class CountingHubConnection : IHubConnection
{
    private readonly ConcurrentDictionary<string, int> onRegistrations = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public HubConnectionState State { get; private set; } = HubConnectionState.Disconnected;

    /// <inheritdoc/>
    public string? ConnectionId { get; private set; }

    /// <inheritdoc/>
    public IHubConnectionMetrics Metrics { get; } = new HubConnectionMetrics("counting-test-connection");

    /// <summary>Gets the number of times a <c>Closed</c> handler has been subscribed.</summary>
    public int ClosedSubscriptions { get; private set; }

    /// <summary>Gets the number of times a <c>Reconnected</c> handler has been subscribed.</summary>
    public int ReconnectedSubscriptions { get; private set; }

    /// <summary>Gets the number of times a <c>Reconnecting</c> handler has been subscribed.</summary>
    public int ReconnectingSubscriptions { get; private set; }

    /// <inheritdoc/>
    public event Func<Exception?, Task> Reconnecting
    {
        add => this.ReconnectingSubscriptions++;
        remove { }
    }

    /// <inheritdoc/>
    public event Func<string?, Task> Reconnected
    {
        add => this.ReconnectedSubscriptions++;
        remove { }
    }

    /// <inheritdoc/>
    public event Func<Exception?, Task> Closed
    {
        add => this.ClosedSubscriptions++;
        remove { }
    }

    /// <summary>Gets the total number of <see cref="IHubConnection.On{T1, T2}"/> registrations across all methods.</summary>
    public int TotalOnRegistrations => this.onRegistrations.Values.Sum();

    /// <summary>Gets the number of times a handler was registered for the given hub method.</summary>
    /// <param name="methodName">The hub method name.</param>
    /// <returns>The registration count for that method.</returns>
    public int OnRegistrationCountFor(string methodName) =>
        this.onRegistrations.TryGetValue(methodName, out var count) ? count : 0;

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.State = HubConnectionState.Connected;
        this.ConnectionId = Guid.NewGuid().ToString();
        return Task.CompletedTask;
    }

    /// <summary>Gets the number of times <see cref="StopAsync"/> has been invoked (issue #124 lifecycle coverage).</summary>
    public int StopAsyncCalls { get; private set; }

    /// <summary>Gets the number of times <see cref="DisposeAsync"/> has been invoked (issue #124 lifecycle coverage).</summary>
    public int DisposeAsyncCalls { get; private set; }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.StopAsyncCalls++;
        this.State = HubConnectionState.Disconnected;
        this.ConnectionId = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SendAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<T?> InvokeAsync<T>(string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(default(T));
    }

    /// <inheritdoc/>
    public IDisposable On<T>(string methodName, Func<T, Task> handler)
    {
        this.onRegistrations.AddOrUpdate(methodName, 1, (_, current) => current + 1);
        return NoopDisposable.Instance;
    }

    /// <inheritdoc/>
    public IDisposable On<T1, T2>(string methodName, Func<T1, T2, Task> handler)
    {
        this.onRegistrations.AddOrUpdate(methodName, 1, (_, current) => current + 1);
        return NoopDisposable.Instance;
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        this.DisposeAsyncCalls++;
        return ValueTask.CompletedTask;
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        public void Dispose()
        {
            // No resources to release for the counting double.
        }
    }
}
