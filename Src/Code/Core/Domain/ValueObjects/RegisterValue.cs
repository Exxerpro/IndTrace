// <copyright file="RegisterValue.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using System.Globalization;
using IndTrace.Domain.Models;

/// <summary>
/// Immutable value object wrapping the reading of a <c>Register</c>: its <see cref="Value"/> (the raw
/// string reading) together with the <see cref="DataType"/> describing the originating tag.
/// <para>
/// Both strings are preserved <b>exactly</b> as supplied to <see cref="Create"/> (no trimming,
/// case-folding, or normalization), so the value object never alters the persisted/produced bytes. The
/// <see cref="DataType"/> is deliberately kept as an <b>opaque string at rest</b>: it is tag metadata,
/// not part of the write-once audit reading, and the persisted audit row may carry it empty — therefore
/// empty/whitespace <see cref="DataType"/> is a legal state. Strictly typing the data type (an EnumModel)
/// is explicitly out of scope and tracked separately under GitHub #43.
/// </para>
/// <para>
/// The invariant is intentionally minimal: only <see langword="null"/> input is rejected. Empty and
/// whitespace-only <see cref="Value"/> readings are legal because registers legitimately carry empty
/// readings. The typed parse accessors (<see cref="ParseInt"/>, <see cref="ParseDouble"/>,
/// <see cref="ParseBool"/>) mirror how <c>Register.Value</c> is consumed today (see
/// <c>PerformanceData.FromPlc</c>) and are non-throwing, returning a failure <see cref="Result{T}"/>
/// rather than a sentinel when the reading does not parse.
/// </para>
/// </summary>
public sealed class RegisterValue : ValueObject
{
    private RegisterValue(string value, string dataType)
    {
        this.Value = value;
        this.DataType = dataType;
    }

    /// <summary>
    /// Gets the raw register reading, preserved exactly as supplied to <see cref="Create"/>
    /// (no trimming, case-folding, or normalization). May be empty.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the data type describing the originating tag, carried as an opaque string and preserved
    /// exactly as supplied to <see cref="Create"/>. May be empty (the persisted audit row drops it).
    /// </summary>
    public string DataType { get; }

    /// <summary>
    /// Creates a validated <see cref="RegisterValue"/>. Only <see langword="null"/> input is rejected;
    /// empty and whitespace-only <paramref name="value"/> and <paramref name="dataType"/> are legal
    /// states. Both strings are preserved exactly.
    /// </summary>
    /// <param name="value">The raw register reading. Must be non-null; may be empty.</param>
    /// <param name="dataType">The opaque data type of the reading. Must be non-null; may be empty.</param>
    /// <returns>A success result carrying the reading, or a failure result describing the null input.</returns>
    public static Result<RegisterValue> Create(string? value, string? dataType)
    {
        if (value is null)
        {
            return Result<RegisterValue>.WithFailure("Register value cannot be null");
        }

        if (dataType is null)
        {
            return Result<RegisterValue>.WithFailure("Register data type cannot be null");
        }

        return Result<RegisterValue>.Success(new RegisterValue(value, dataType));
    }

    /// <summary>
    /// Parses the reading as a 32-bit integer. Non-throwing: returns a failure result when the reading is
    /// not a valid integer. Mirrors the integer consumption of <c>Register.Value</c> in
    /// <c>PerformanceData.FromPlc</c>.
    /// </summary>
    /// <returns>A success result carrying the parsed integer, or a failure result.</returns>
    public Result<int> ParseInt() =>
        int.TryParse(this.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? Result<int>.Success(parsed)
            : Result<int>.WithFailure($"Register value '{this.Value}' is not a valid integer");

    /// <summary>
    /// Parses the reading as a double-precision number. Non-throwing: returns a failure result when the
    /// reading is not a valid number. Mirrors the numeric consumption of <c>Register.Value</c> in
    /// <c>PerformanceData.FromPlc</c>.
    /// </summary>
    /// <returns>A success result carrying the parsed number, or a failure result.</returns>
    public Result<double> ParseDouble() =>
        double.TryParse(this.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsed)
            ? Result<double>.Success(parsed)
            : Result<double>.WithFailure($"Register value '{this.Value}' is not a valid number");

    /// <summary>
    /// Parses the reading as a boolean. Non-throwing: returns a failure result when the reading is not a
    /// valid boolean (<c>"true"</c>/<c>"false"</c>, case-insensitive).
    /// </summary>
    /// <returns>A success result carrying the parsed boolean, or a failure result.</returns>
    public Result<bool> ParseBool() =>
        bool.TryParse(this.Value, out var parsed)
            ? Result<bool>.Success(parsed)
            : Result<bool>.WithFailure($"Register value '{this.Value}' is not a valid boolean");

    /// <summary>
    /// Returns the raw reading string.
    /// </summary>
    /// <returns>The preserved <see cref="Value"/>.</returns>
    public override string ToString() => this.Value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
        yield return this.DataType;
    }
}
