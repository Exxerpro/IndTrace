// <copyright file="IManualStart.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Gateway.Interfaces;

/// <summary>
/// Provides a contract for manually starting a process or service asynchronously.
/// </summary>
public interface IManualStart
{
    /// <summary>
    /// Starts the process or service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartAsync(CancellationToken cancellationToken);
}