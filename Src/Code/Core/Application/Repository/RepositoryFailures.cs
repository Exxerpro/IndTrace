// <copyright file="RepositoryFailures.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

/// <summary>
/// Classifies repository failure results so handlers can distinguish the single benign
/// "no rows matched" outcome from a genuine infrastructure failure (#113 F3).
/// <para>
/// The repository seams (<c>Repository.FirstOrDefaultAsync</c> / <c>ReadOnlyRepository.FirstOrDefaultAsync</c>)
/// NEVER return success-with-null: when no entity matches the specification they return a FAILURE result
/// carrying the <see cref="NotFoundSentinel"/> message. <c>IsFailure</c> therefore means EITHER "not found"
/// OR "infrastructure error", and every caller that wants to treat "not found" as a non-error MUST classify
/// the failure through <see cref="IsNotFound(IEnumerable{string}?)"/> — matching the exact sentinel, never a
/// substring such as the bare word "entity" (an infrastructure error mentioning "entity" is not a not-found).
/// </para>
/// </summary>
public static class RepositoryFailures
{
    /// <summary>
    /// The exact message the repository uses to signal "FirstOrDefault matched nothing"
    /// (see <c>Repository.cs</c> / <c>ReadOnlyRepository.cs</c>, which emit it with a trailing period).
    /// It is the ONE failure that means "empty", not "infrastructure error" — any other failure is a
    /// genuine repository error and must be propagated, never swallowed as not-found.
    /// </summary>
    public const string NotFoundSentinel = "No matching entity found";

    /// <summary>
    /// Distinguishes the repository's "no rows matched" sentinel failure from a genuine infrastructure failure.
    /// </summary>
    /// <param name="errors">The errors carried by the failed repository result.</param>
    /// <returns>True when the failure only signals that no entity matched the specification.</returns>
    public static bool IsNotFound(IEnumerable<string>? errors) =>
        errors is not null && errors.Any(e => e is not null && e.Contains(NotFoundSentinel, StringComparison.Ordinal));

    /// <summary>
    /// Distinguishes a repository result that failed with the "no rows matched" sentinel from one that
    /// failed with a genuine infrastructure error. A success result is never classified as not-found.
    /// </summary>
    /// <typeparam name="T">The entity type carried by the result.</typeparam>
    /// <param name="result">The repository result to classify.</param>
    /// <returns>True when the result is a failure that only signals that no entity matched.</returns>
    public static bool IsNotFound<T>(Result<T> result) =>
        result is not null && result.IsFailure && IsNotFound(result.Errors);
}
