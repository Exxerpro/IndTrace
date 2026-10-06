// <copyright file="SingleConnectionHubConnectionFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Monitor.Tests.TestDoubles;

using IndTrace.HubConnection.Abstractions;

/// <summary>
/// Test double factory that always hands back the SAME <see cref="CountingHubConnection"/> instance,
/// modelling the worker's memoized (long-lived) connection. Also records how many times the worker
/// asked for a connection so a test can confirm the connection is built once.
/// </summary>
public sealed class SingleConnectionHubConnectionFactory : IHubConnectionFactory
{
    /// <summary>Gets the single connection instance handed to the worker.</summary>
    public CountingHubConnection Connection { get; } = new();

    /// <summary>Gets the number of times <see cref="CreateAsync"/> was invoked.</summary>
    public int CreateCallCount { get; private set; }

    /// <inheritdoc/>
    public Task<IHubConnection> CreateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.CreateCallCount++;
        return Task.FromResult<IHubConnection>(this.Connection);
    }
}
