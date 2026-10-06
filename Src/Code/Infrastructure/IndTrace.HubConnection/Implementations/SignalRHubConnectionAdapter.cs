// <copyright file="SignalRHubConnectionAdapter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Implementations;

using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Metrics;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Thin adapter that delegates to a concrete SignalR HubConnection.
/// </summary>
public sealed class SignalRHubConnectionAdapter : IHubConnection
{
    private readonly HubConnection inner;
    private readonly IHubConnectionMetrics metrics;

    public SignalRHubConnectionAdapter(HubConnection inner)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));

        // The state provider keeps snapshots truthful: metrics report the LIVE connection state
        // instead of a hardcoded value captured at construction time.
        this.metrics = new HubConnectionMetrics(inner.ConnectionId ?? string.Empty, () => inner.State.ToString());
    }

    public HubConnectionState State => this.inner.State;
    public string? ConnectionId => this.inner.ConnectionId;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        var t = this.inner.StartAsync(cancellationToken);
        this.metrics.RecordConnectionStarted(DateTimeOffset.UtcNow);
        return t;
    }
    public Task StopAsync(CancellationToken cancellationToken = default) => this.inner.StopAsync(cancellationToken);

    public Task SendAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        this.metrics.RecordMessageSent(0);

        //[Fix]
        //CLAUDE
        //Date: 23/06/2026
        //Reason: [Hub arg marshalling] inner.SendAsync(method, object?[], ct) binds to the
        //        SendAsync(method, object? arg1, ct) overload, passing the WHOLE array as a single
        //        argument so multi-parameter hub methods (e.g. SendMessage/BroadcastData) never bind.
        //        SendCoreAsync is the array overload that spreads args into the hub method parameters.
        return this.inner.SendCoreAsync(methodName, args, cancellationToken);
    }

    public async Task<T?> InvokeAsync<T>(string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        //[Fix]
        //CLAUDE
        //Date: 23/06/2026
        //Reason: [Hub arg marshalling] Same array-as-single-arg defect as SendAsync. InvokeCoreAsync
        //        spreads the args correctly; this also removes the prior Echo special-case that faked
        //        the response client-side (masking the real bug) and let it hit the actual hub method.
        var result = await this.inner.InvokeCoreAsync(methodName, typeof(T), args, cancellationToken).ConfigureAwait(false);
        return result is null ? default : (T?)result;
    }

    public IDisposable On<T>(string methodName, Func<T, Task> handler)
        => this.inner.On(methodName, handler);

    public IDisposable On<T1, T2>(string methodName, Func<T1, T2, Task> handler)
        => this.inner.On(methodName, handler);

    public event Func<Exception?, Task> Reconnecting
    {
        add => this.inner.Reconnecting += value;
        remove => this.inner.Reconnecting -= value;
    }

    public event Func<string?, Task> Reconnected
    {
        add => this.inner.Reconnected += value;
        remove => this.inner.Reconnected -= value;
    }

    public event Func<Exception?, Task> Closed
    {
        add => this.inner.Closed += value;
        remove => this.inner.Closed -= value;
    }

    public ValueTask DisposeAsync() => this.inner.DisposeAsync();

    public IHubConnectionMetrics Metrics => this.metrics;
}
