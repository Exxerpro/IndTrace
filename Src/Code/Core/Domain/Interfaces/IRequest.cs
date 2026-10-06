// <copyright file="IRequest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

// CQRS QUERY PATTERN
// The IMonitorRequest interface inherits from the IMonitorRequest interface, meaning it can be processed by a guiCommandDispatcher.
// Returns Result<TResponse> which wraps both success and failure outcomes for the guiRequest.

/// <summary>
/// Represents a marker interface for Monitor requests in the CQRS pattern.
/// </summary>
public interface IRequest;

/// <summary>
/// Represents a marker interface for Monitor requests in the CQRS pattern.
/// </summary>
public interface IRequest<TResponse>;