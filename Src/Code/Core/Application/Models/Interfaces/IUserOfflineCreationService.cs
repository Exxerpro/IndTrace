// <copyright file="IUserOfflineCreationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Interfaces;

/// <summary>
/// Provides methods for offline user creation and challenge-response validation.
/// </summary>
public interface IUserOfflineCreationService
{
    /// <summary>
    /// Gets the last generated challenge strings.
    /// </summary>
    List<string> LastGeneratedChallenge { get; }

    /// <summary>
    /// Generates a new challenge string for offline user creation.
    /// </summary>
    /// <returns>The generated challenge string.</returns>
    string GenerateChallenge();

    /// <summary>
    /// Validates the response to a specific challenge.
    /// </summary>
    /// <param name="challenge">The challenge string.</param>
    /// <param name="response">The response to validate.</param>
    /// <returns>True if the response is valid; otherwise, false.</returns>
    bool ValidateResponse(string challenge, string response);

    /// <summary>
    /// Validates the response to the last generated challenge.
    /// </summary>
    /// <param name="response">The response to validate.</param>
    /// <returns>True if the response is valid; otherwise, false.</returns>
    bool ValidateResponse(string response);
}