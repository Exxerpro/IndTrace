// <copyright file="HubConnectionFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Implementations;

using System.Net.Http;
using IndTrace.Application.Models.Services;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Protocols;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Default factory that builds a configured SignalR HubConnection and adapts it to <see cref="IHubConnection"/>.
/// The connection is returned in Disconnected state; callers decide when to start.
/// </summary>
public sealed class HubConnectionFactory(IOptions<HubMonitorOptions> options, ILogger<HubConnectionFactory> logger) : IHubConnectionFactory
{
    public Task<IHubConnection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value ?? throw new InvalidOperationException("HubMonitorOptions not configured");
        var configuredUrl = settings.Url;
        if (string.IsNullOrWhiteSpace(configuredUrl))
        {
            logger.LogError("Hub URL is not configured. Set HubMonitorOptions:Url in configuration");
            throw new InvalidOperationException("Hub URL is not configured. Set HubMonitorOptions:Url in configuration.");
        }

        logger.LogInformation("Creating hub connection to URL: {HubUrl}", configuredUrl);

        var handler = new HttpClientHandler();
        if (settings.AcceptAnyServerCertificate)
        {
            logger.LogWarning("Accepting any server certificate for hub connection - this should only be used in development");
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        var reconnectDelays = new[]
        {
            TimeSpan.Zero,
            TimeSpan.FromSeconds(settings.RetryTime),
            TimeSpan.FromSeconds(settings.RetryTime * 2),
            TimeSpan.FromSeconds(settings.RetryTime * 3)
        };

        logger.LogDebug("Configuring automatic reconnect with delays: {ReconnectDelays}", reconnectDelays);

        var connection = new HubConnectionBuilder()
            .WithUrl(configuredUrl, o =>
            {
                o.HttpMessageHandlerFactory = _ => handler;
            })
            .WithAutomaticReconnect(reconnectDelays)

            // #188: hub payloads carry domain types (write-once Register, BarCodeLabel, EnumModel smart enums)
            // that default System.Text.Json cannot round-trip; both hub sides must share this configuration.
            .AddJsonProtocol(o => IndTraceHubJsonProtocol.Configure(o.PayloadSerializerOptions))
            .Build();

        logger.LogDebug("Hub connection created successfully for URL: {HubUrl}", configuredUrl);

        IHubConnection adapted = new SignalRHubConnectionAdapter(connection);
        return Task.FromResult(adapted);
    }
}
