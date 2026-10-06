// <copyright file="IBarCodeService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services;

/// <summary>
/// Service interface for BarCode-related operations, replacing BarCodeRepositoryExtensions.
/// Enables proper dependency injection and testability.
/// </summary>
public interface IBarCodeService
{
    /// <summary>
    /// Gets the next consecutive sequence value for a barcode label, scoped to the product identified by
    /// <paramref name="partNumber"/>. The value is derived from the highest existing label in that scope and
    /// incremented by one; an empty scope seeds at the first consecutive. Exhausting the fixed-width sequence
    /// field FAILS LOUD (a failure <see cref="Result{T}"/>) rather than wrapping into a duplicate identity, and
    /// an infrastructure failure is surfaced as a failure — never masked as an empty sequence.
    /// </summary>
    /// <param name="partNumber">The part number whose product scopes the sequence.</param>
    /// <param name="masterLabel">The list of master labels for the part (scope companion).</param>
    /// <param name="rule">The label-generation rule that mints labels for this product (the same rule the caller
    /// will hand to the formatter). When present, its trailing auto-increment component's configured length is the
    /// authoritative consecutive-field width — required for legacy corpora whose labels are ALL digits after the
    /// prefix and therefore carry no non-digit boundary before the counter (issue #186). May be
    /// <see langword="null"/> for callers without a rule in hand; derivation then falls back to the
    /// boundary-based trailing-digit-run heuristic.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The next consecutive value, or a failure result on exhaustion, missing product, or infrastructure error.</returns>
    Task<Result<int>> GetConsecutiveByBarCodeLabelAsync(
        string partNumber,
        List<string> masterLabel,
        Rule? rule,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a BarCode entity by its label.
    /// </summary>
    /// <param name="label">The label to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The BarCode entity if found, or a failure result.</returns>
    Task<Result<BarCode>> GetBarCodeByLabelAsync(
        BarCodeLabel label,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a BarCode entity by its unique identifier.
    /// </summary>
    /// <param name="barCodeId">The BarCode ID to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The BarCode entity if found, or a failure result.</returns>
    Task<Result<BarCode>> GetBarCodeByIdAsync(
        int barCodeId,
        CancellationToken cancellationToken);

    // #119 (F2): GetBarCodeByRegisterDataAsync was removed from this service. It was a dead DUPLICATE of the
    // live BarCodeRepositoryExtensions.GetBarCodeByRegisterDataAsync (the Reports path) with the same unbounded
    // ledger-scan and failure-collapsing defects, and had no production caller — only its own unit test.
}