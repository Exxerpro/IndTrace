// <copyright file="IMonitorQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces
{
    using IndTrace.Domain.Models;

    // CQRS QUERY PATTERN
    // QueryAsync Handler responsible for handling queries of type IMonitorRequest<TResponse>
    // Extends from IMonitorRequestHandler
    // Responsible for processing a query (TQuery) and returning the corresponding result (Result<TResponse>)
    // where TQuery : IMonitorRequest<TResponse> ensures that TQuery is a valid query (i.e., it must implement IMonitorRequest<TResponse>)

    /// <summary>
    /// Defines a contract for handling Monitor queries in the CQRS pattern.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to handle.</typeparam>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    public interface IMonitorQueryHandler<TQuery, TResponse>
        where TQuery : IMonitorRequest<TResponse>
    {
        /// <summary>
        /// Processes a Monitor query and returns the corresponding result.
        /// </summary>
        /// <param name="query">The query to process.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the query processing.</returns>
        Task<Result<TResponse>> ProcessAsync(TQuery query, CancellationToken cancellationToken);
    }
}