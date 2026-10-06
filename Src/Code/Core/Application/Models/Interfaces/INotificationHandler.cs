// <copyright file="INotificationHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Interfaces;

/// <summary>
/// Defines a handler for processing notification events.
/// </summary>
/// <typeparam name="TEvent">The type of the notification event.</typeparam>
public interface INotificationHandler<in TEvent> where TEvent : INotification
{
    /// <summary>
    /// Processes the specified notification event.
    /// </summary>
    /// <param name="event">The notification event to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Result> Process(TEvent @event, CancellationToken cancellationToken);
}