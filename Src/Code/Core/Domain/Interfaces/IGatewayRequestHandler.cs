// <copyright file="IGatewayRequestHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

using IndTrace.Domain.Models;

/// <summary>
/// Gateway command handler for instructions that return no specific result.
/// </summary>
public interface IGatewayRequestHandler<TCommand>
    where TCommand : IGatewayRequest
{
    Task<Result> ProcessAsync(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Gateway command or query handler that expects a typed result.
/// </summary>
public interface IGatewayRequestHandler<TCommand, TResponse>
    where TCommand : IGatewayRequest<TResponse>
{
    Task<Result<TResponse>> ProcessAsync(TCommand command, CancellationToken cancellationToken);
}