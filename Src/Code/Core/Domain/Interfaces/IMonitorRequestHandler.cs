// <copyright file="IMonitorRequestHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces
{
    using IndTrace.Domain.Models;

    // CQRS Pattern
    // Command Handler for commands that do not return any specific result other than success or failure

    /// <summary>
    /// Defines a contract for handling Monitor commands that do not return a specific response type.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle.</typeparam>
    public interface IMonitorRequestHandler<TCommand>
        where TCommand : IMonitorRequest
    {
        /// <summary>
        /// Processes a Monitor command without expecting a specific response.
        /// </summary>
        /// <param name="command">The command to process.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the command processing.</returns>
        Task<Result> ProcessAsync(TCommand command, CancellationToken cancellationToken);
    }

    // CQRS Pattern
    // Command Handler for commands that do return a specific response type

    /// <summary>
    /// Defines a contract for handling Monitor commands that return a specific response type.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle.</typeparam>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    public interface IMonitorRequestHandler<TCommand, TResponse>
        where TCommand : IMonitorRequest<TResponse>
    {
        /// <summary>
        /// Processes a Monitor request and returns a response of the specified type.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>A task representing the asynchronous operation, containing the result with the response.</returns>
        Task<Result<TResponse>> ProcessAsync(TCommand request, CancellationToken cancellationToken);
    }
}