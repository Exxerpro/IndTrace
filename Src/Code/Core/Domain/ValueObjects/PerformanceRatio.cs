// <copyright file="PerformanceRatio.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using System.Globalization;
using IndTrace.Domain.Models;

/// <summary>
/// Immutable value object for the OEE performance metric, constrained to the closed interval <c>[0, 1.5]</c>
/// (0% to 150%).
/// <para>
/// Story 26.B1 (#31 remainder): performance is the one OEE metric that can legitimately exceed 100% — a machine
/// running faster than its standard cycle time over-performs, up to a ratified ceiling of 150% (see
/// <c>KpiOee.Create</c> and <c>OeeRegister.ToKpiOee</c>). It therefore has its own bound, distinct from the
/// <c>[0, 1]</c> <see cref="Ratio"/> used for Availability, Quality, and the composite OEE. <see cref="Create"/>
/// is the sole validating railway boundary and NEVER throws — it returns a <see cref="Result{T}"/> failure for
/// NaN, ±∞, or an out-of-range value.
/// </para>
/// </summary>
public sealed class PerformanceRatio : ValueObject
{
    /// <summary>The inclusive lower bound of a valid performance ratio (0.0 = 0%).</summary>
    public const double MinValue = 0.0;

    /// <summary>The inclusive upper bound of a valid performance ratio (1.5 = 150% over-performance).</summary>
    public const double MaxValue = 1.5;

    private PerformanceRatio(double value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the underlying performance proportion in <c>[0, 1.5]</c>, preserved exactly as supplied. Exposed so
    /// existing consumers (percentage formatting via <c>:P1</c>, the QuestDB sink, the EF value converter) read
    /// the raw <see cref="double"/> without any behavioral change.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Creates a validated <see cref="PerformanceRatio"/>, enforcing that <paramref name="value"/> is a finite
    /// number within the inclusive range <c>[0, 1.5]</c>. Never throws — a violated invariant is returned as a
    /// <see cref="Result{T}"/> failure.
    /// </summary>
    /// <param name="value">The performance ratio to validate. Must be finite and within <c>[0, 1.5]</c>.</param>
    /// <returns>A success result carrying the performance ratio, or a failure result describing the violated invariant.</returns>
    public static Result<PerformanceRatio> Create(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return Result<PerformanceRatio>.WithFailure("PerformanceRatio must be a finite number");
        }

        if (value < MinValue || value > MaxValue)
        {
            return Result<PerformanceRatio>.WithFailure(
                $"PerformanceRatio must be between {MinValue} and {MaxValue}: {value.ToString(CultureInfo.InvariantCulture)}");
        }

        return Result<PerformanceRatio>.Success(new PerformanceRatio(value));
    }

    /// <summary>
    /// Materializes a <see cref="PerformanceRatio"/> from a value already sanitized by an in-Domain caller
    /// (<c>KpiOee.Create</c> validates before calling this; <c>KpiOee.CreateFixture</c> deliberately holds
    /// arbitrary/legacy values) or read from the database via the EF value converter. This is a <b>total</b> seam:
    /// it performs no validation and <b>never throws</b>, so a legacy out-of-range row materializes without
    /// detonating every query that touches it. <see cref="Create"/> remains the sole railway boundary for new
    /// values. Exposed to the EF converter (<c>IndTrace.Persistence</c>) and test seams via
    /// <c>InternalsVisibleTo</c> — NOT part of the public surface.
    /// </summary>
    /// <param name="value">The raw value read from a trusted in-Domain caller or the persisted column.</param>
    /// <returns>A <see cref="PerformanceRatio"/> carrying <paramref name="value"/> exactly.</returns>
    internal static PerformanceRatio FromPersisted(double value) => new(value);

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
