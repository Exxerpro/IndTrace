// <copyright file="Ratio.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using System.Globalization;
using IndTrace.Domain.Models;

/// <summary>
/// Immutable value object for a proportion constrained to the closed interval <c>[0, 1]</c> (0% to 100%).
/// <para>
/// Story 26.B1 (#31 remainder): the OEE metrics Availability, Quality, and the composite OEE are ratios that can
/// never legitimately exceed 100%, so the bound lives with the type instead of being re-checked at every call
/// site. <see cref="Create"/> is the sole validating railway boundary (it NEVER throws — it returns a
/// <see cref="Result{T}"/> failure for NaN, ±∞, or an out-of-range value). Performance, which allows
/// over-performance up to 150%, uses the separate <see cref="PerformanceRatio"/> value object instead.
/// </para>
/// </summary>
public sealed class Ratio : ValueObject
{
    /// <summary>The inclusive lower bound of a valid ratio (0.0 = 0%).</summary>
    public const double MinValue = 0.0;

    /// <summary>The inclusive upper bound of a valid ratio (1.0 = 100%).</summary>
    public const double MaxValue = 1.0;

    private Ratio(double value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the underlying proportion in <c>[0, 1]</c>, preserved exactly as supplied. Exposed so existing
    /// consumers (percentage formatting via <c>:P1</c>, the QuestDB sink, the EF value converter) read the raw
    /// <see cref="double"/> without any behavioral change.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Creates a validated <see cref="Ratio"/>, enforcing that <paramref name="value"/> is a finite number within
    /// the inclusive range <c>[0, 1]</c>. Never throws — a violated invariant is returned as a
    /// <see cref="Result{T}"/> failure.
    /// </summary>
    /// <param name="value">The proportion to validate. Must be finite and within <c>[0, 1]</c>.</param>
    /// <returns>A success result carrying the ratio, or a failure result describing the violated invariant.</returns>
    public static Result<Ratio> Create(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return Result<Ratio>.WithFailure("Ratio must be a finite number");
        }

        if (value < MinValue || value > MaxValue)
        {
            return Result<Ratio>.WithFailure(
                $"Ratio must be between {MinValue} and {MaxValue}: {value.ToString(CultureInfo.InvariantCulture)}");
        }

        return Result<Ratio>.Success(new Ratio(value));
    }

    /// <summary>
    /// Materializes a <see cref="Ratio"/> from a value already sanitized by an in-Domain caller
    /// (<c>KpiOee.Create</c> validates before calling this; <c>KpiOee.CreateFixture</c> deliberately holds
    /// arbitrary/legacy values) or read from the database via the EF value converter. This is a <b>total</b>
    /// seam: it performs no validation and <b>never throws</b>, so a legacy out-of-range row materializes without
    /// detonating every query that touches it. <see cref="Create"/> remains the sole railway boundary for new
    /// values. Exposed to the EF converter (<c>IndTrace.Persistence</c>) and test seams via
    /// <c>InternalsVisibleTo</c> — NOT part of the public surface.
    /// </summary>
    /// <param name="value">The raw value read from a trusted in-Domain caller or the persisted column.</param>
    /// <returns>A <see cref="Ratio"/> carrying <paramref name="value"/> exactly.</returns>
    internal static Ratio FromPersisted(double value) => new(value);

    /// <summary>
    /// Returns the invariant-culture string form of the underlying <see cref="Value"/>.
    /// </summary>
    /// <returns>The preserved <see cref="Value"/> as a string.</returns>
    public override string ToString() => this.Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}
