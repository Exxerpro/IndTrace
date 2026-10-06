// <copyright file="IHubConnectionFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Abstractions;

/// <summary>
/// Factory that creates configured <see cref="IHubConnection"/> instances.
/// Does not start the connection; callers decide when to start.
/// </summary>
public interface IHubConnectionFactory
{
    Task<IHubConnection> CreateAsync(CancellationToken cancellationToken = default);
}

