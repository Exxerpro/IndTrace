// <copyright file="DeleteFailureException.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Exceptions;

/// <summary>
/// Represents an exception that is thrown when deletion of an entity fails.
/// </summary>
/// <param name="name">The name of the entity.</param>
/// <param name="key">The key of the entity.</param>
/// <param name="message">The error message.</param>
public class DeleteFailureException(string name, object key, string message)
    : Exception($"Deletion of entity \"{name}\" ({key}) failed. {message}");