// <copyright file="Notifications.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using EventStore.Client;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.Json;
using System.Threading;

namespace IndTrace.Monitor.Pages;

/// <summary>
/// Represents the Notifications page component that handles event notifications using EventStore.
/// </summary>
public partial class Notifications : ComponentBase
{
    //[Inject]
    //private EventStoreClient _eventStoreClient { get; set; }

    /// <summary>
    /// Sends an event to the EventStore.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendEvent()
    {
        // SECURITY (#70): the EventStore connection string (which carries credentials) is read from
        // configuration ("EventStore:ConnectionString") supplied via environment variables / user-secrets
        // at deploy time, never hardcoded. When it is not configured the prototype no-ops and logs a warning
        // instead of connecting with a default administrator credential.
        var connectionString = Configuration["EventStore:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Logger.LogWarning(
                "Notifications.SendEvent skipped: no 'EventStore:ConnectionString' configured. Set it at deploy time (environment variables / user-secrets) to enable event publishing.");
            return;
        }

        // #124 F8: the append is awaited (a fire-and-forget write could fail silently) and the gRPC
        // client is disposed instead of leaking one channel per click.
        try
        {
            var settings = EventStoreClientSettings.Create(connectionString);

            await using var client = new EventStoreClient(settings);

            var evt = new TestEvent()
            {
                EntityId = Guid.CreateVersion7().ToString("N"),
                ImportantData = "CreateCodeTaskAsync",
            };

            var eventData = new EventData(
                Uuid.NewUuid(),
                                "Command",
                                JsonSerializer.SerializeToUtf8Bytes(evt));

            await client.AppendToStreamAsync(
                "IndTrace",
                                StreamState.Any,
                                new[] { eventData });

            Snackbar.Add("Event published to EventStore.", Severity.Success);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Notifications.SendEvent failed to append the event to the 'IndTrace' stream.");
            Snackbar.Add($"Failed to publish the event: {ex.Message}", Severity.Error);
        }
    }
}

/// <summary>
/// Represents a test event for EventStore operations.
/// </summary>
internal class TestEvent
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the important data associated with the event.
    /// </summary>
    public string ImportantData { get; set; } = string.Empty;
}
