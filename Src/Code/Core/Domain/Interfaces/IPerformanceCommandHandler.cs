// <copyright file="IPerformanceCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

using IndTrace.Domain.Models;

/// <summary>
/// Defines the contract for command handlers that process commands without returning a result.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public interface IPerformanceCommandHandler<in TCommand>
{
    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProcessAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for command handlers that process commands and return a result.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResult">The type of result to return.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
{
    /// <summary>
    /// Handles the specified command and returns a result.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with a result.</returns>
    Task<Result<TResult>> ProcessAsync(TCommand command, CancellationToken cancellationToken = default);
}