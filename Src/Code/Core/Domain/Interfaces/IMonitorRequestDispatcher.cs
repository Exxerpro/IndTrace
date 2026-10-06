// <copyright file="IMonitorRequestDispatcher.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

using IndTrace.Domain.Models;

/// <summary>
/// Defines a contract for dispatching Monitor commands and queries in the application.
/// </summary>
public interface IMonitorRequestDispatcher
{
    /// <summary>
    /// Processes a Monitor command without expecting a response.
    /// </summary>
    /// <param name="command">The Monitor command to process.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the command processing.</returns>
    Task<Result> ProcessAsync(IMonitorRequest command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a Monitor request and returns a response of the specified type.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="monitorRequest">The Monitor request to process.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with the response.</returns>
    Task<Result<TResponse>> ProcessAsync<TResponse>(IMonitorRequest<TResponse> monitorRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Monitor query and returns a response of the specified type.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="monitorRequest">The Monitor query to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with the response.</returns>
    Task<Result<TResponse>> QueryAsync<TResponse>(IMonitorRequest<TResponse> monitorRequest, CancellationToken cancellationToken = default);
}