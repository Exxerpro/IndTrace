// <copyright file="ShiftType.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Represents the type of work shift in the system.
/// </summary>
public class ShiftType : EnumModel
{
    /// <summary>
    /// Gets the invalid shift type.
    /// </summary>
    public static readonly ShiftType Invalid
        = new(-1, "Invalid Value");

    /// <summary>
    /// Gets the none shift type.
    /// </summary>
    public static readonly ShiftType None
        = new(0, "None");

    /// <summary>
    /// Gets the first shift type.
    /// </summary>
    public static readonly ShiftType First
        = new(1, "First");

    /// <summary>
    /// Gets the second shift type.
    /// </summary>
    public static readonly ShiftType Second
        = new(2, "Second");

    /// <summary>
    /// Gets the third shift type.
    /// </summary>
    public static readonly ShiftType Third
        = new(4, "Third");

    /// <summary>
    /// Initializes a new instance of the <see cref="ShiftType"/> class.
    /// </summary>
    public ShiftType()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShiftType"/> class with specified values.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    private ShiftType(int value, string name, string? displayName = null)
        : base(value, name, displayName ?? string.Empty)
    {
    }

    /// <summary>
    /// Implicitly converts a ShiftType to its integer value.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator int(ShiftType enumerator) => enumerator.Value;

    /// <summary>
    /// Implicitly converts a ShiftType to a nullable integer value.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator int?(ShiftType enumerator) => enumerator.Value;

    /// <summary>
    /// Implicitly converts a ShiftType to its string representation.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator string(ShiftType enumerator) => enumerator.Value.ToString();

    /// <summary>
    /// Implicitly converts an integer value to a ShiftType.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ShiftType(int value) => FromValue<ShiftType>(value);

    /// <summary>
    /// Implicitly converts a nullable integer value to a ShiftType.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ShiftType(int? value) => FromValue<ShiftType>(value ?? 0);
}
