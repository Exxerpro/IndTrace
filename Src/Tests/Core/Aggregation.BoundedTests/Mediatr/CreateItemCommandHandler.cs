// <copyright file="CreateItemCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Mediatr;
/// <summary>
/// Represents the CreateItemCommandHandler.
/// </summary>

public class CreateItemCommandHandler<T> : IMonitorRequestHandler<CreateItemCommand<T>, T> where T : MonitorRequestDispatcherTests.IItem
{
    /// <summary>
    /// Executes Handle operation.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The result of Handle.</returns>
    public T Handle(CreateItemCommand<T> message)
    {
        return Activator.CreateInstance<T>();
    }
    /// <summary>
    /// Executes ProcessAsync operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ProcessAsync.</returns>

    public async Task<Result<T>> ProcessAsync(CreateItemCommand<T> request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result<T>.Success(Activator.CreateInstance<T>()));
    }
}