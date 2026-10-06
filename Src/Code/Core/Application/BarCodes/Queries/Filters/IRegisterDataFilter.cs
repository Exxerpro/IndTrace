// <copyright file="IRegisterDataFilter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.Filters;

/// <summary>
/// Handles register data filtering as separate concern.
/// Extracted from GetReportsListMonitorQueryHandler to follow SRP principles.
/// Implements CLAUDE.md compliance with Result pattern and null safety.
/// </summary>
public interface IRegisterDataFilter
{
    /// <summary>
    /// Gets barcode IDs matching register search criteria.
    /// Uses the existing repository method for register-based barcode filtering.
    /// #126 review C10: a <see langword="null"/> window requests the content-preserving UNBOUNDED ledger scan —
    /// used by the IsMaster report, whose barcode rows carry no date bound (#119 F6 PO ruling pending).
    /// </summary>
    /// <param name="registerSearch">Register search criteria.</param>
    /// <param name="windowStart">The inclusive start of the report's date window bounding the register ledger scan, or <see langword="null"/> for an unbounded scan.</param>
    /// <param name="windowEnd">The inclusive end of the report's date window bounding the register ledger scan, or <see langword="null"/> for an unbounded scan.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing matching barcode IDs (empty on a genuine no-match) or failure reasons on infrastructure errors.</returns>
    Task<Result<HashSet<int>>> GetMatchingBarCodeIdsAsync(
        string registerSearch,
        DateTime? windowStart,
        DateTime? windowEnd,
        CancellationToken cancellationToken);
}