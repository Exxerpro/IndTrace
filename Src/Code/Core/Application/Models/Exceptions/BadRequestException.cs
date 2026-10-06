// <copyright file="BadRequestException.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Exceptions;

/// <summary>
/// Represents an exception that is thrown for bad requests.
/// </summary>
/// <param name="message">The error message.</param>
public class BadRequestException(string message) : Exception(message);