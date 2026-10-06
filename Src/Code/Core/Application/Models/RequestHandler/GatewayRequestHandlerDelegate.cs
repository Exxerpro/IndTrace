// <copyright file="GatewayRequestHandlerDelegate.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.RequestHandler;

/// <summary>
/// Represents a delegate for handling a gateway request and returning a response asynchronously.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task<TResponse> GatewayRequestHandlerDelegate<TResponse>();