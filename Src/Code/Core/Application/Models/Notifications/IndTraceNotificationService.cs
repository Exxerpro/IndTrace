// <copyright file="IndTraceNotificationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Notifications;

/// <summary>
/// Provides a notification service implementation for IndTrace notifications.
/// </summary>
public class IndTraceNotificationService : INotificationService
{
    // TODO [IMPROVEMENT][ABEL][22/AUG/2025]
    // Implement actual notification sending logic (e.g., email, SMS, push notification).
    // SignalR is already on the tech stack. Until a real transport is chosen, this is a
    // terminal no-op: notifications are a prototype and do not leave the process (see the
    // EventStore comment in appsettings.json).
    //
    // [Fix] CLAUDE Date: 08/JUL/2026 Reason: [Code-bug] The previous ctor injected
    // INotificationService into itself (a placeholder for "the real service"), which registered
    // as AddTransient<INotificationService, IndTraceNotificationService> produced a DI self-cycle
    // (crashing the Monitor's ValidateOnBuild) and, had it ever resolved, unbounded recursion in
    // SendAsync. Removed the self-dependency; SendAsync is now a self-contained no-op that keeps
    // the many domain-event handlers (which depend on INotificationService) resolvable.

    /// <summary>
    /// Delivers a notification. This prototype implementation performs no transport and simply
    /// reports success for a well-formed message; wire a real notifier here when one is chosen.
    /// </summary>
    /// <param name="message">The message to deliver.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful <see cref="Result"/> for a non-null message; a failure otherwise.</returns>
    public Task<Result> SendAsync(MessageDto message, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Result.WithFailure("Operation was cancelled."));
        }

        if (message == null)
        {
            return Task.FromResult(Result.WithFailure("Message cannot be null"));
        }

        return Task.FromResult(Result.Success());
    }
}