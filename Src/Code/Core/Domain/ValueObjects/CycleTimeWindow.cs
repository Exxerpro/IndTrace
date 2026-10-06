// <copyright file="CycleTimeWindow.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using IndTrace.Domain.Models;

/// <summary>
/// Immutable value object describing the recipe cycle-time window (in seconds) over which a measured
/// cycle time is considered acceptable.
/// <para>
/// The in-range predicate (<see cref="Contains"/>) uses <b>exclusive</b> bounds and rejects negatives:
/// a cycle time is in range only when it is non-negative, strictly greater than <see cref="Minimum"/>,
/// and strictly less than <see cref="Maximum"/>. This mirrors the as-built cycle-time rule exactly
/// (see <c>CycleTimeValidator</c>) and must not change.
/// </para>
/// </summary>
public sealed class CycleTimeWindow : ValueObject
{
    private CycleTimeWindow(int minimum, int maximum)
    {
        this.Minimum = minimum;
        this.Maximum = maximum;
    }

    /// <summary>
    /// Gets the lower bound of the cycle-time window, in seconds.
    /// </summary>
    public int Minimum { get; }

    /// <summary>
    /// Gets the upper bound of the cycle-time window, in seconds.
    /// </summary>
    public int Maximum { get; }

    /// <summary>
    /// Creates a validated <see cref="CycleTimeWindow"/>, enforcing <c>0 &lt;= minimum &lt;= maximum</c>.
    /// </summary>
    /// <param name="minimum">The lower bound, in seconds. Must be non-negative.</param>
    /// <param name="maximum">The upper bound, in seconds. Must be greater than or equal to <paramref name="minimum"/>.</param>
    /// <returns>A success result carrying the window, or a failure result describing the violated invariant.</returns>
    public static Result<CycleTimeWindow> Create(int minimum, int maximum)
    {
        if (minimum < 0)
        {
            return Result<CycleTimeWindow>.WithFailure($"Cycle time minimum cannot be negative: {minimum}s");
        }

        if (minimum > maximum)
        {
            return Result<CycleTimeWindow>.WithFailure(
                $"Cycle time minimum {minimum}s cannot exceed maximum {maximum}s");
        }

        return Result<CycleTimeWindow>.Success(new CycleTimeWindow(minimum, maximum));
    }

    /// <summary>
    /// Creates a window directly from raw recipe bounds <b>without</b> enforcing the <see cref="Create"/>
    /// invariant. Used only on the cycle-time validation path, which must preserve as-built behavior for
    /// inverted bounds (<c>maximum &lt; minimum</c>) and therefore cannot reject them.
    /// </summary>
    /// <param name="minimum">The lower bound, in seconds, exactly as stored on the recipe.</param>
    /// <param name="maximum">The upper bound, in seconds, exactly as stored on the recipe.</param>
    /// <returns>A window over the supplied bounds, unvalidated.</returns>
    internal static CycleTimeWindow ForBounds(int minimum, int maximum) => new(minimum, maximum);

    /// <summary>
    /// Determines whether the supplied cycle time falls within this window. Bounds are <b>exclusive</b>
    /// (strictly greater than <see cref="Minimum"/> and strictly less than <see cref="Maximum"/>) and
    /// negative cycle times are rejected. This is the canonical in-range predicate for cycle time.
    /// </summary>
    /// <param name="cycleTime">The measured cycle time, in seconds.</param>
    /// <returns><see langword="true"/> when the cycle time is in range; otherwise <see langword="false"/>.</returns>
    public bool Contains(int cycleTime) => cycleTime >= 0 && cycleTime > this.Minimum && cycleTime < this.Maximum;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Minimum;
        yield return this.Maximum;
    }
}
