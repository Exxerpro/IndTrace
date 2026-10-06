// <copyright file="IHubTestable.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Hub.Server.Hubs;

/// <summary>
/// Provides methods for testing and controlling the connection state of a hub.
/// </summary>
public interface IHubTestable
{
    /// <summary>
    /// Pauses the hub connection until the specified timestamp or until cancellation is requested.
    /// </summary>
    /// <param name="timeStamp">The timestamp until which to pause the connection.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PauseAsync(DateTime timeStamp, CancellationToken cancellationToken);

    /// <summary>
    /// Cycles the hub connection through a series of pause/resume intervals defined by the provided timestamps.
    /// </summary>
    /// <param name="timeStamps">A collection of timestamps for pause/resume intervals.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task TransientAsync(IEnumerable<DateTime> timeStamps, CancellationToken cancellationToken);
}