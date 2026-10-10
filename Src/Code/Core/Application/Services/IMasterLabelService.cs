// <copyright file="IMasterLabelService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services;

/// <summary>
/// Service interface for master label operations.
/// </summary>
public interface IMasterLabelService
{
    /// <summary>
    /// Gets a list of master label codes by part number, using cache if available.
    /// </summary>
    /// <param name="partNumber">The part number to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching master label codes, empty when the part number has none; a failure only when the lookup fails.
    /// </returns>
    Task<Result<List<string>>> GetMasterLabelByPartNumberAsync(string partNumber, CancellationToken cancellationToken);
}