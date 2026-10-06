// <copyright file="ValueObject.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Models;

/// <summary>
/// Represents a base class for value objects in domain-driven design.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current value object.</param>
    /// <returns>True if equal, otherwise false.</returns>
    public override bool Equals(object? obj) // Updated to allow nullable object
    {
        if (obj == null || obj.GetType() != this.GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        using var thisValues = this.GetAtomicValues().GetEnumerator();
        using var otherValues = other.GetAtomicValues().GetEnumerator();

        while (true)
        {
            var thisMoved = thisValues.MoveNext();
            var otherMoved = otherValues.MoveNext();

            // Unequal-length sequences are never equal: if exactly one enumerator advanced, the value
            // objects differ in their atomic-value count. (The previous loop short-circuited on the
            // second MoveNext and could skip an unmatched trailing element, declaring [a,b] == [a].)
            if (thisMoved != otherMoved)
            {
                return false;
            }

            // Both exhausted at the same position: every atomic value matched.
            if (!thisMoved)
            {
                return true;
            }

            if (thisValues.Current is null ^ otherValues.Current is null)
            {
                return false;
            }

            if (thisValues.Current != null &&
                !thisValues.Current.Equals(otherValues.Current))
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns a hash code for the value object.
    /// </summary>
    /// <returns>A hash code for the value object.</returns>
    public override int GetHashCode()
    {
        // Order-sensitive, non-XOR combination (System.HashCode): distinguishes value objects whose atomic
        // values are permutations of each other, and avoids the self-cancelling collision the previous XOR
        // produced when two atomic values were equal (e.g. Value == DataType). HashCode.Add tolerates nulls.
        var hash = default(HashCode);
        foreach (var value in this.GetAtomicValues())
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Checks equality between two value objects using the equality operator.
    /// </summary>
    /// <param name="left">The left value object.</param>
    /// <param name="right">The right value object.</param>
    /// <returns>True if equal, otherwise false.</returns>
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (left is null ^ right is null)
        {
            return false;
        }

        return left?.Equals(right) != false;
    }

    /// <summary>
    /// Checks inequality between two value objects using the inequality operator.
    /// </summary>
    /// <param name="left">The left value object.</param>
    /// <param name="right">The right value object.</param>
    /// <returns>True if not equal, otherwise false.</returns>
    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !EqualOperator(left, right);
    }

    /// <summary>
    /// Gets the atomic values of the value object for equality comparison.
    /// </summary>
    /// <returns>An enumerable of atomic values.</returns>
    protected abstract IEnumerable<object> GetAtomicValues();
}
