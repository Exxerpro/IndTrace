// <copyright file="HubMonitorWorker.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.UI.Models;
using IndTrace.Application.UI.Services;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Extensions;
using IndTrace.Domain.Entities;
using Microsoft.AspNetCore.SignalR.Client;
using static IndTrace.HubConnection.Contracts.HubMethods;

namespace IndTrace.Monitor.Worker;

/// <summary>
/// Background service that manages SignalR hub connections for monitoring IndTrace events.
/// </summary>
/// <param name="hubConnectionFactory">Factory for creating hub connections.</param>
/// <param name="eventsService">Service for handling IndTrace events.</param>
/// <param name="logger">Logger instance for the worker.</param>
public class HubMonitorWorker(
    IHubConnectionFactory hubConnectionFactory,
    IndTraceEventsService eventsService,
    ILogger<HubMonitorWorker> logger)
    : BackgroundService
{
    private IHubConnection? hubConnection;

    /// <summary>
    /// Gets a value indicating whether the hub connection is established and connected.
    /// </summary>
    public bool IsHubConnected =>
        this.hubConnection is not null && this.hubConnection.State == HubConnectionState.Connected;

    /// <summary>
    /// Executes the background service, maintaining the hub connection and handling events.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => logger.LogInformation("Hub Monitor Worker is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await this.SetupAndStartHubConnectionAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                logger.LogInformation("Hub Monitor Worker running at: {time}", DateTimeOffset.Now.ToLocalTime());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred in the Hub Monitor Worker.");
            }
        }

        logger.LogInformation("Hub Monitor Worker is stopping.");
    }

    /// <summary>
    /// Stops the background service and disposes of the hub connection.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token for the stop operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Hub Monitor Worker is stopping.");

        // Let the base class cancel the stopping token and wait for ExecuteAsync to complete FIRST:
        // the execute loop may still be using the connection until then. Only afterwards is it safe
        // to stop and dispose the connection (asynchronously — never block on DisposeAsync, #124).
        await base.StopAsync(stoppingToken);

        var connection = this.hubConnection;
        this.hubConnection = null;
        if (connection is not null)
        {
            await connection.StopAsync(stoppingToken);
            await connection.DisposeAsync();
        }
    }

    /// <summary>
    /// Sets up and starts the SignalR hub connection with event handlers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetupAndStartHubConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await this.InitializeHubConnectionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting SignalR connection");
        }
    }

    /// <summary>
    /// Initializes the hub connection and attempts to start it if not already connected.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task InitializeHubConnectionAsync(CancellationToken cancellationToken)
    {
        // Attach the hub event and connection-state handlers exactly once, at the moment the
        // connection instance is created. SignalR's On(...) appends handlers, so re-attaching on
        // every 60s watchdog pass would stack duplicate handler copies without bound (issue #71).
        var isNewConnection = this.hubConnection is null;
        this.hubConnection ??= await hubConnectionFactory.CreateAsync(cancellationToken);

        if (isNewConnection)
        {
            this.RegisterHubEventHandlers(cancellationToken);
            this.AddConnectionHandlers(cancellationToken);
        }

        if (!this.IsHubConnected)
        {
            await this.hubConnection.TryStartHubConnectionAsync(logger, cancellationToken);
            logger.LogInformation("Attempting to start HubConnection...");
        }

        if (this.IsHubConnected)
        {
            logger.LogInformation("Hub Monitor Connected at {DateTime}", DateTimeOffset.Now.ToLocalTime());
            await this.hubConnection.SendAsync(BroadcastMessageToClients, new object?[] { "Gateway", "Connected at Hub Monitor" }, cancellationToken);
        }
    }

    /// <summary>
    /// Registers event handlers for various SignalR hub events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    private void RegisterHubEventHandlers(CancellationToken cancellationToken)
    {
        if (this.hubConnection is null) return;
        // Inline the method calls directly into the event handlers
        this.hubConnection.On<string, string>(
            BroadcastMessageToClients,
            (string user, string message) =>
            {
                try
                {
                    eventsService.PushMessage(user, message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling BroadcastMessageToClients event.");
                }
                return Task.CompletedTask;
            });

        this.hubConnection.On<int, ControllerMonitor>(
            BroadcastHeartbeatSignal,
            (int plc, ControllerMonitor controller) =>
            {
                try
                {
                    eventsService.AddOrUpdateControllerFromGateway(plc, controller);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling BroadcastHeartbeatSignal event.");
                }
                return Task.CompletedTask;
            });

        this.hubConnection.On<int, TaskGatewayRequest>(
            BroadcastTaskGatewayRequest,
            (int plc, TaskGatewayRequest request) =>
            {
                try
                {
                    eventsService.AddOrUpdateTaskGatewayRequest(plc, request);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling BroadcastTaskGatewayRequest event.");
                }
                return Task.CompletedTask;
            });

        this.hubConnection.On<int, TaskGatewayResponseDto>(
            BroadcastTaskGatewayResponse,
            (int plc, TaskGatewayResponseDto response) =>
            {
                try
                {
                    eventsService.AddOrUpdateTaskGatewayResponse(plc, response);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling BroadcastTaskGatewayResponse event.");
                }
                return Task.CompletedTask;
            });
    }

    /// <summary>
    /// Adds connection state change handlers for the hub connection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    private void AddConnectionHandlers(CancellationToken cancellationToken)
    {
        if (this.hubConnection is null) return;
        this.hubConnection.Closed += async (error) => await this.LogConnectionStateAsync("lost");
        this.hubConnection.Reconnected += async (error) => await this.LogConnectionStateAsync("reconnected", cancellationToken);
    }

    /// <summary>
    /// Logs the connection state change and optionally sends a message to the hub.
    /// </summary>
    /// <param name="state">The connection state (e.g., "lost", "reconnected").</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LogConnectionStateAsync(string state, CancellationToken? cancellationToken = null)
    {
        logger.LogInformation("Hub Monitor Connection {State} at {Timestamp}", state, DateTimeOffset.Now.ToLocalTime());
        if (state == "reconnected" && cancellationToken.HasValue && this.hubConnection is not null)
        {
            await this.hubConnection.SendAsync(BroadcastMessageToClients, new object?[] { "Gateway", "Reconnected at Hub Monitor" }, cancellationToken.Value);
        }
    }

    /// <summary>
    /// Disposes of the worker. The hub connection is normally disposed asynchronously by
    /// <see cref="StopAsync(CancellationToken)"/>, which makes this a no-op for the connection;
    /// if the host skipped the graceful stop, the connection is disposed here WITHOUT blocking —
    /// the previous <c>DisposeAsync().GetAwaiter().GetResult()</c> was sync-over-async on the host
    /// shutdown path (#124).
    /// </summary>
    public override void Dispose()
    {
        var connection = this.hubConnection;
        this.hubConnection = null;
        if (connection is not null)
        {
            // Fire-and-forget by design: Dispose must never block host shutdown. The helper
            // observes and logs any fault from the asynchronous disposal.
            _ = this.DisposeConnectionAsync(connection);
        }

        base.Dispose();
    }

    /// <summary>
    /// Disposes the given hub connection, observing (logging) any fault so the fire-and-forget
    /// disposal in <see cref="Dispose"/> can never surface an unobserved task exception.
    /// </summary>
    /// <param name="connection">The connection to dispose.</param>
    /// <returns>A task representing the asynchronous disposal.</returns>
    private async Task DisposeConnectionAsync(IHubConnection connection)
    {
        try
        {
            await connection.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing the hub connection during worker disposal.");
        }
    }
}
