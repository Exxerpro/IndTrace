// <copyright file="WorkerHubServer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Hub.Server
{
    using System.Diagnostics;
    using IndTrace.Hub.Server;
    using IndTrace.HubConnection.Abstractions;
    using IndTrace.HubConnection.Extensions;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.SignalR.Client;

    /// <summary>
    /// Represents the WorkerHubServer.
    /// </summary>
    public class WorkerHubServer(ILogger<WorkerHubServer> logger, IHubContext<EventMonitorHub> hubContext, IHubConnectionFactory connectionFactory, Microsoft.Extensions.Options.IOptions<WorkerHubServerOptions> options, IndTrace.Domain.Interfaces.IDateTimeMachine dateTimeMachine)
        : BackgroundService
    {
        private readonly WorkerHubServerOptions workerOptions = options.Value ?? new WorkerHubServerOptions();

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("WorkerHubServer process running at: {time}", dateTimeMachine.GetLocalNow());
                    }

                    var delayMs = Math.Max(1_000, workerOptions.HeartbeatIntervalSeconds * 1_000);
                    await Task.Delay(delayMs, stoppingToken);
                    await EnsureIsConnectedAsync(stoppingToken);

                    if (workerOptions.EnableHeartbeat && this.connection is not null)
                    {
                        // EventMonitorHub.SendMessage(string) takes exactly ONE argument; a failed
                        // heartbeat is logged and skipped, never allowed to kill the host.
                        await this.connection.TrySendAsync("SendMessage", new object?[] { "hub-server heartbeat" }, logger, stoppingToken);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // The heartbeat host must survive any transient failure: log and keep the loop
                    // alive (pacing comes from the heartbeat-interval delay at the top of the loop).
                    // Cancellation (host shutdown) propagates.
                    logger.LogError(ex, "WorkerHubServer heartbeat pass failed; retrying on next interval.");
                }
            }
        }

        private IHubConnection? connection; // set during EnsureIsConnectedAsync before any use

        // The connection instance the reconnect/message handlers are currently attached to.
        // Handlers are attached exactly once per connection instance; see RegisterConnectionHandlers.
        private IHubConnection? handlersRegisteredFor;

        // Ensure primary-ctor parameter 'hubContext' is considered used to satisfy CS9113 without behavior change
        private readonly IHubContext<EventMonitorHub>? _ = hubContext;

        private async ValueTask EnsureIsConnectedAsync(CancellationToken cancellationToken)
        {
            // Standardize: use EnsureHubConnectionIsValid + TryStartHubConnectionAsync from IndTrace.HubConnection.Extensions
            this.connection = await this.connection.EnsureHubConnectionIsValid(connectionFactory, logger, cancellationToken)
                ?? this.connection; // if null on failure, keep previous reference (will retry next loop)
            if (this.connection is null)
            {
                return;
            }
            await this.connection.TryStartHubConnectionAsync(logger, cancellationToken);

            // Attach reconnect/message handlers exactly once per connection instance. SignalR's On(...)
            // and the event handlers append, so re-attaching on every heartbeat pass would stack duplicate
            // handlers without bound (issue #71). Only (re)register when the connection instance is new —
            // e.g. when EnsureHubConnectionIsValid rebuilds a fresh connection after a failure.
            if (!ReferenceEquals(this.handlersRegisteredFor, this.connection))
            {
                this.RegisterConnectionHandlers(this.connection);
                this.handlersRegisteredFor = this.connection;
            }
        }

        private void RegisterConnectionHandlers(IHubConnection hubConnection)
        {
            hubConnection.Reconnecting += error =>
            {
                Debug.Assert(hubConnection.State == HubConnectionState.Reconnecting);

                // Notify users the connection was lost and the client is reconnecting.
                // Start queuing or dropping messages.
                return Task.CompletedTask;
            };

            hubConnection.Reconnected += connectionId =>
            {
                Debug.Assert(hubConnection.State == HubConnectionState.Connected);

                // Notify users the connection was reestablished.
                // Start dequeuing messages queued while reconnecting if any.
                return Task.CompletedTask;
            };

            hubConnection.On<string, string>("BroadcastMessageToClients", (user, message) =>
            {
                var newMessage = $" ReceiveMessage {user}: {message}";
                logger.LogInformation(newMessage);
                return Task.CompletedTask;
            });
        }

    }
}