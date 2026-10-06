// <copyright file="EventMonitorHub.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.UI.Models;
using IndTrace.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace IndTrace.Hub.Server;

/// <summary>
/// SignalR hub for broadcasting events, heartbeats, and gateway requests/responses to connected clients.
/// </summary>
public class EventMonitorHub(ILogger<EventMonitorHub> logger) : Microsoft.AspNetCore.SignalR.Hub
{
    // Live count of connected clients. Maintained with Interlocked so OnConnected/OnDisconnected
    // (which can run concurrently) stay accurate; read via Volatile.Read in GetConnectionCount.
    private static int connectionCount;

    /// <summary>
    /// Tracks a new connection and notifies all clients of the joining connection id.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        Interlocked.Increment(ref connectionCount);
        await this.Clients.All.SendAsync("UserConnected", this.Context.ConnectionId, CancellationToken.None);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Tracks a dropped connection and notifies all clients of the leaving connection id.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Interlocked.Decrement(ref connectionCount);
        await this.Clients.All.SendAsync("UserDisconnected", this.Context.ConnectionId, CancellationToken.None);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Broadcasts a message to all clients, echoing the sender's connection id alongside it.
    /// </summary>
    public async Task SendMessage(string message)
    {
        await this.Clients.All.SendAsync("ReceiveMessage", this.Context.ConnectionId, message, CancellationToken.None);
    }

    /// <summary>
    /// Broadcasts an arbitrary typed payload to all clients under the given data-type label.
    /// </summary>
    public async Task BroadcastData(string dataType, object data)
    {
        await this.Clients.All.SendAsync("DataReceived", dataType, data, CancellationToken.None);
    }

    /// <summary>
    /// Broadcasts a user/message pair to all clients. Failures propagate to the caller (via the
    /// SignalR invocation result) rather than being swallowed — a failed fan-out is never reported
    /// as delivered. Uses <c>SendCoreAsync</c> so the arguments spread into discrete handler
    /// parameters (matching the <c>On&lt;string, string&gt;</c> client registrations).
    /// </summary>
    public async Task BroadcastMessageToClients(string user, string message)
    {
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(message))
        {
            logger.LogError("Error in BroadcastMessageToClients: user and message must not be null or empty");
            throw new ArgumentException("IndTraceUser and message must not be null or empty.");
        }

        await this.Clients.All.SendCoreAsync(nameof(this.BroadcastMessageToClients), new object?[] { user, message }, CancellationToken.None);
    }

    /// <summary>
    /// Broadcasts a heartbeat to all clients. Failures propagate to the caller rather than being
    /// swallowed. Uses <c>SendCoreAsync</c> so the arguments spread into discrete handler parameters.
    /// </summary>
    public async Task BroadcastHeartbeatSignal(int plc, ControllerMonitor controller)
    {
        if (controller is null)
        {
            logger.LogError("Error in BroadcastHeartbeatSignal: ControllerMonitor must not be null");
            throw new ArgumentException("PLC ID must be greater than zero and ControllerMonitor must not be null.");
        }

        await this.Clients.All.SendCoreAsync(nameof(this.BroadcastHeartbeatSignal), new object?[] { plc, controller }, CancellationToken.None);
    }

    /// <summary>
    /// Broadcasts a gateway request to all clients. Failures propagate to the caller rather than
    /// being swallowed. Uses <c>SendCoreAsync</c> so the arguments spread into discrete handler parameters.
    /// </summary>
    public async Task BroadcastTaskGatewayRequest(int id, TaskGatewayRequest request)
    {
        if (request is null)
        {
            logger.LogError("Error in BroadcastTaskGatewayRequest: request must not be null");
            throw new ArgumentException("ID must be greater than zero and monitorRequest must not be null.");
        }

        await this.Clients.All.SendCoreAsync(nameof(this.BroadcastTaskGatewayRequest), new object?[] { id, request }, CancellationToken.None);
    }

    /// <summary>
    /// Broadcasts a gateway response to all clients. Failures propagate to the caller rather than
    /// being swallowed. Uses <c>SendCoreAsync</c> so the arguments spread into discrete handler parameters.
    /// </summary>
    public async Task BroadcastTaskGatewayResponse(int id, TaskGatewayResponseDto response)
    {
        if (response is null)
        {
            logger.LogError("Error in BroadcastTaskGatewayResponse: response must not be null");
            throw new ArgumentException("ID must be greater than zero and TaskGatewayResponseDto must not be null.");
        }

        await this.Clients.All.SendCoreAsync(nameof(this.BroadcastTaskGatewayResponse), new object?[] { id, response }, CancellationToken.None);
    }

    /// <summary>
    /// Echo method for testing connectivity and basic hub functionality.
    /// </summary>
    public Task<string> Echo(string message)
    {
        logger.LogDebug("Echo called with message: {Message}", message);
        return Task.FromResult($"Echo: {message}");
    }

    /// <summary>
    /// Gets the current number of connected clients (tracked in OnConnected/OnDisconnected).
    /// </summary>
    public Task<int> GetConnectionCount()
    {
        return Task.FromResult(Volatile.Read(ref connectionCount));
    }
}
