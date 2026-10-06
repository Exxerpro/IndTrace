// <copyright file="MetricsRegisteringHubConnectionFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Dashboard;

using IndTrace.HubConnection.Abstractions;

/// <summary>
/// Decorator for <see cref="IHubConnectionFactory"/> that registers created connections
/// with the <see cref="IHubMetricsDashboard"/> for observability. This class preserves
/// original factory behavior and adds non-blocking registration side-effects. It also
/// observes cache evictions so a disposed connection's registration dies with it instead
/// of rooting the corpse in the dashboard forever.
/// </summary>
public sealed class MetricsRegisteringHubConnectionFactory : IHubConnectionFactory, IHubConnectionEvictionObserver
{
    private readonly IHubConnectionFactory innerFactory;
    private readonly IHubMetricsDashboard dashboard;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsRegisteringHubConnectionFactory"/> class.
    /// </summary>
    /// <param name="innerFactory">The underlying factory that actually creates connections.</param>
    /// <param name="dashboard">The dashboard where created connections will be registered.</param>
    public MetricsRegisteringHubConnectionFactory(IHubConnectionFactory innerFactory, IHubMetricsDashboard dashboard)
    {
        this.innerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
        this.dashboard = dashboard ?? throw new ArgumentNullException(nameof(dashboard));
    }

    /// <summary>
    /// Creates a new <see cref="IHubConnection"/> and registers it with the metrics dashboard.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created connection.</returns>
    public async Task<IHubConnection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var connection = await this.innerFactory.CreateAsync(cancellationToken).ConfigureAwait(false);
        // Registration must be exception-safe and non-blocking by contract
        try
        {
            this.dashboard.RegisterConnection(connection);
        }
        catch
        {
            // Dashboard failures must never affect connection creation
        }

        return connection;
    }

    /// <summary>
    /// Releases the dashboard registration for a connection this factory created, once the shared
    /// cache has evicted and disposed it. Removal matches by instance identity, so it works
    /// regardless of the key the connection was registered under (real ConnectionId or the
    /// synthetic pending key used before StartAsync assigned one).
    /// </summary>
    /// <param name="connection">The evicted connection instance.</param>
    public void OnConnectionEvicted(IHubConnection connection)
    {
        // Unregistration must be exception-safe and non-blocking by contract
        try
        {
            this.dashboard.UnregisterConnection(connection);
        }
        catch
        {
            // Dashboard failures must never affect eviction
        }
    }
}


