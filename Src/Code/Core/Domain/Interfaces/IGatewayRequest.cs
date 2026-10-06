// <copyright file="IGatewayRequest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a base interface for gateway request operations.
/// This interface serves as a marker for all gateway request types.
/// </summary>
public interface IGatewayRequest : IRequest;

/// <summary>
/// Represents a gateway request with a response type.
/// </summary>
public interface IGatewayRequest<TResponse> : IRequest<TResponse>;