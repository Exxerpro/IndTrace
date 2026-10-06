// <copyright file="IGatewayQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

using IndTrace.Domain.Models;

/// <summary>
/// Defines a contract for handling gateway queries in the CQRS pattern.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResponse">The type of response expected.</typeparam>
public interface IGatewayQueryHandler<TQuery, TResponse>
    where TQuery : IGatewayRequest<TResponse>
{
    /// <summary>
    /// Processes a gateway query and returns the corresponding result.
    /// </summary>
    /// <param name="query">The query to process.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the query processing.</returns>
    Task<Result<TResponse>> ProcessAsync(TQuery query, CancellationToken cancellationToken);
}