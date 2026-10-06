// <copyright file="MessageDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Notifications.Models;

/// <summary>
/// Represents the MessageDto.
/// </summary>
public class MessageDto
{
    public static MessageDto CreateMessage<Tnotification>(INotification notification)
        where Tnotification : INotification
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Enhanced MessageDto factory to create meaningful notification messages instead of placeholder text
        var notificationTypeName = typeof(Tnotification).Name;
        var actionType = GetActionType(notificationTypeName);
        var entityName = GetEntityName(notificationTypeName);

        return new MessageDto
        {
            From = "indTrace.manufacturing@example.com",
            To = "operations@example.com",
            Subject = $"IndTrace Manufacturing Alert: {entityName} {actionType}",
            Body = $"Manufacturing System Notification:\n\n" +
                   $"Event: {entityName} {actionType}\n" +
                   $"Details: {notification.ToString()}\n" +
                   $"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
                   $"System: IndTrace Manufacturing Execution System",
        };
    }

    private static string GetActionType(string notificationTypeName)
    {
        if (notificationTypeName.Contains("Created") || notificationTypeName.Contains("Event"))
        {
            return "Created";
        }

        if (notificationTypeName.Contains("Updated"))
        {
            return "Updated";
        }

        return "Changed";
    }

    private static string GetEntityName(string notificationTypeName)
    {
        // Remove action suffixes to get clean entity name
        return notificationTypeName
            .Replace("Created", string.Empty)
            .Replace("Updated", string.Empty)
            .Replace("Event", string.Empty)
            .Replace("Handler", string.Empty);
    }

    /// <summary>
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}