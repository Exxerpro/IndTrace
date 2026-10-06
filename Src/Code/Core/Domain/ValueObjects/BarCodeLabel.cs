// <copyright file="BarCodeLabel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using IndTrace.Domain.Models;

/// <summary>
/// Immutable value object representing a part barcode label as carried across the application boundary.
/// <para>
/// The invariant is intentionally minimal: a label must be non-empty (rejecting null, empty, and
/// whitespace-only input) and must not exceed 80 characters, which matches the storage column width.
/// </para>
/// <para>
/// The supplied string is preserved <b>exactly</b>: the value object never trims, case-folds, or
/// otherwise normalizes the label. Barcode lookups are byte-equal string comparisons against persisted
/// data, so any normalization here would silently change which row a label matches (or fails to match).
/// Validation therefore gates the value without ever altering it.
/// </para>
/// </summary>
public sealed class BarCodeLabel : ValueObject
{
    private BarCodeLabel(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the validated label string, preserved exactly as supplied to <see cref="Create"/>
    /// (no trimming, case-folding, or normalization).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a validated <see cref="BarCodeLabel"/>, enforcing the non-empty and maximum-length
    /// (80 characters) invariant while preserving the input string exactly.
    /// </summary>
    /// <param name="label">The raw label string. Must be non-empty and at most 80 characters long.</param>
    /// <returns>
    /// A success result carrying the label, or a failure result describing the violated invariant.
    /// </returns>
    public static Result<BarCodeLabel> Create(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return Result<BarCodeLabel>.WithFailure("BarCode label cannot be empty");
        }

        if (label.Length > 80)
        {
            return Result<BarCodeLabel>.WithFailure($"BarCode label cannot exceed 80 characters: length {label.Length}");
        }

        return Result<BarCodeLabel>.Success(new BarCodeLabel(label));
    }

    /// <summary>
    /// Materializes a <see cref="BarCodeLabel"/> from a value already persisted in the database, trusting the
    /// schema-enforced <c>NOT NULL nvarchar(80)</c> column contract. This is the <b>read path</b> and is
    /// deliberately <b>total</b>: it performs no validation, <b>never throws</b>, and <b>never normalizes</b>
    /// (the returned <see cref="Value"/> is ordinal byte-equal to <paramref name="value"/>).
    /// <para>
    /// A single legacy <c>Label = ''</c> row must materialize without detonating every query that touches it,
    /// so this factory never runs <see cref="Create"/>. <see cref="Create"/> remains the <b>sole railway
    /// boundary</b> for <i>new</i> labels; <c>FromPersisted</c> only reconstructs what the schema already
    /// physically guarantees. Exposed to the EF converter (<c>IndTrace.Persistence</c>) and its proof harness
    /// via <c>InternalsVisibleTo</c> — NOT part of the public surface. This mirrors the existing smart-enum
    /// converters that materialize reference types from persisted primitives with an unchecked cast.
    /// </para>
    /// </summary>
    /// <param name="value">The raw label string read from the persisted column.</param>
    /// <returns>A <see cref="BarCodeLabel"/> carrying <paramref name="value"/> exactly.</returns>
    internal static BarCodeLabel FromPersisted(string value) => new(value);

    /// <summary>
    /// Returns the exact label string.
    /// </summary>
    /// <returns>The preserved <see cref="Value"/>.</returns>
    public override string ToString() => this.Value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}
