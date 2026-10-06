// <copyright file="ActiveStatus.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Represents the tri-state activation status of a machine-to-PLC association.
/// The underlying integer value is persisted as-is (the database column stays <c>int</c>):
/// <c>-1</c> means inactive (soft-deleted), <c>0</c> means none, and <c>1</c> means active.
/// </summary>
public class ActiveStatus : EnumModel
{
    /// <summary>
    /// Represents an out-of-band invalid activation status.
    /// Uses <see cref="int.MinValue"/> as a sentinel because the real states
    /// (<c>-1</c>, <c>0</c>, <c>1</c>) occupy the natural low integers.
    /// </summary>
    public static readonly ActiveStatus Invalid
        = new(int.MinValue, "Invalid Value");

    /// <summary>
    /// Represents an inactive (soft-deleted) association. Underlying value <c>-1</c>.
    /// </summary>
    public static readonly ActiveStatus Inactive
        = new(-1, "Inactive");

    /// <summary>
    /// Represents an association with no status. Underlying value <c>0</c>.
    /// </summary>
    public static readonly ActiveStatus None
        = new(0, "None");

    /// <summary>
    /// Represents an active association. Underlying value <c>1</c>.
    /// </summary>
    public static readonly ActiveStatus Active
        = new(1, "Active");

    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveStatus"/> class.
    /// Required by the <see cref="EnumModel"/> reflection-based lookup machinery.
    /// </summary>
    public ActiveStatus()
    {
    }

    private ActiveStatus(int value, string name, string? displayName = null)
        : base(value, name, displayName ?? string.Empty)
    {
    }

    /// <summary>
    /// Implicitly converts an <see cref="ActiveStatus"/> to its integer value.
    /// </summary>
    /// <param name="enumerator">The enumeration to convert.</param>
    public static implicit operator int(ActiveStatus enumerator) => enumerator.Value;

    /// <summary>
    /// Implicitly converts an <see cref="ActiveStatus"/> to its string value.
    /// </summary>
    /// <param name="enumerator">The enumeration to convert.</param>
    public static implicit operator string(ActiveStatus enumerator) => enumerator.Value.ToString();

    /// <summary>
    /// Implicitly converts an integer to the matching <see cref="ActiveStatus"/>,
    /// or <see cref="Invalid"/> when the value is not recognized.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator ActiveStatus(int value) => FromValue<ActiveStatus>(value);
}
