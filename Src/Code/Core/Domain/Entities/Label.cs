// <copyright file="Label.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

/// <summary>
/// Represents a label value object that provides string-like functionality with value equality semantics.
/// </summary>
public class Label : IEquatable<Label>
{
    /// <summary>
    /// The internal label value.
    /// </summary>
    private readonly string label;

    /// <summary>
    /// Implicitly converts a Label to a string.
    /// </summary>
    /// <param name="value">The Label to convert.</param>
    /// <returns>The string representation of the label.</returns>
    public static implicit operator string(Label value)
    {
        return value.label;
    }

    /// <summary>
    /// Implicitly converts a string to a Label.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A new Label instance.</returns>
    public static implicit operator Label(string value)
    {
        return new Label(value);
    }

    /// <summary>
    /// Returns the string representation of the label.
    /// </summary>
    /// <returns>The label value as a string.</returns>
    public override string ToString()
    {
        return this.label;
    }

    /// <summary>
    /// Compares this label with another object. Returns 0 when the object is not a Label instead of throwing, to keep functional semantics.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>A value indicating the relative order of the objects being compared; 0 if incomparable.</returns>
    public int CompareTo(object? obj)
    {
        if (obj is Label label)
        {
            return string.Compare(this.label, label.label, StringComparison.Ordinal);
        }

        throw new ArgumentException("Object is not a Label", nameof(obj));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> class with the specified value.
    /// </summary>
    /// <param name="label">The label value.</param>
    public Label(string label)
    {
        this.label = label;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> class with the specified value and label. Functional style: avoids throwing.
    /// </summary>
    /// <param name="value">The value to assign to the label.</param>
    /// <param name="label">The label parameter (not used in current implementation).</param>
    public Label(string value, string label)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        this.label = value;
    }

    /// <summary>
    /// Determines whether the specified Label is equal to the current Label.
    /// </summary>
    /// <param name="other">The Label to compare with the current Label.</param>
    /// <returns>True if the specified Label is equal to the current Label; otherwise, false.</returns>
    public bool Equals(Label? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.label == other.label;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Label.
    /// </summary>
    /// <param name="obj">The object to compare with the current Label.</param>
    /// <returns>True if the specified object is equal to the current Label; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Label label && this.Equals(label);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current Label.</returns>
    public override int GetHashCode()
    {
        return this.label.GetHashCode();
    }

    /// <summary>
    /// Determines whether two Label instances are equal.
    /// </summary>
    /// <param name="left">The first Label to compare.</param>
    /// <param name="right">The second Label to compare.</param>
    /// <returns>True if the Labels are equal; otherwise, false.</returns>
    public static bool operator ==(Label left, Label right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two Label instances are not equal.
    /// </summary>
    /// <param name="left">The first Label to compare.</param>
    /// <param name="right">The second Label to compare.</param>
    /// <returns>True if the Labels are not equal; otherwise, false.</returns>
    public static bool operator !=(Label left, Label right)
    {
        return !Equals(left, right);
    }
}