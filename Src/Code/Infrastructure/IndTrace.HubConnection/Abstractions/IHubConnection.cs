// <copyright file="IHubConnection.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Abstractions;

using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Abstraction over SignalR HubConnection for DI and testability.
/// Responsibility: manage a single connection lifecycle and messaging.
/// </summary>
public interface IHubConnection : IAsyncDisposable
{
    HubConnectionState State { get; }
    string? ConnectionId { get; }

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);

    Task SendAsync(string methodName, object?[] args, CancellationToken cancellationToken = default);
    Task<T?> InvokeAsync<T>(string methodName, object?[] args, CancellationToken cancellationToken = default);

    IDisposable On<T>(string methodName, Func<T, Task> handler);
    IDisposable On<T1, T2>(string methodName, Func<T1, T2, Task> handler);

    event Func<Exception?, Task> Reconnecting;
    event Func<string?, Task> Reconnected;
    event Func<Exception?, Task> Closed;

    // Metrics Access - Non-blocking infrastructure monitoring
    IHubConnectionMetrics Metrics { get; }
}

