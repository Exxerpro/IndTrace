// <copyright file="IHubConnectionEvictionObserver.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Abstractions;

/// <summary>
/// Optional seam for <see cref="IHubConnectionFactory"/> implementations that track the
/// connections they create (e.g. for metrics registration): the shared connection-cache eviction
/// path notifies the factory when a connection it created is evicted and disposed, so the tracking
/// side releases the instance instead of rooting a disposed corpse forever.
/// </summary>
public interface IHubConnectionEvictionObserver
{
    /// <summary>
    /// Called when <paramref name="connection"/> has been evicted from the shared connection cache
    /// and is being disposed. Implementations must be exception-safe and non-blocking.
    /// </summary>
    /// <param name="connection">The evicted connection instance.</param>
    void OnConnectionEvicted(IHubConnection connection);
}
