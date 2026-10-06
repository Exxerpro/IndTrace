// <copyright file="INotificationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Interfaces;

/// <summary>
/// Provides methods for sending notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result> SendAsync(MessageDto message, CancellationToken cancellationToken = default);
}