// <copyright file="RestException.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Exceptions;

using System.Net;

/// <summary>
/// Represents an exception for REST API errors, including HTTP status code and message.
/// </summary>
public class RestException(HttpStatusCode code, object? message = null) : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with the exception.
    /// </summary>
    public HttpStatusCode Code { get; } = code;

    /// <summary>
    /// Gets the error message associated with the exception.
    /// </summary>
    public new object Message { get; } = message ?? "No message provided";
}