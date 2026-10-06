// <copyright file="ValueObjectTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.CommonTests;
/// <summary>
/// Represents the ValueObjectTests.
/// </summary>

public class ValueObjectTests
{
    /// <summary>
    /// Executes Equals_GivenDifferentValues_ShouldReturnFalse operation.
    /// </summary>
    [Fact]
    public void Equals_GivenDifferentValues_ShouldReturnFalse()
    {
        var point1 = new Point(1, 2);
        var point2 = new Point(2, 1);

        Assert.False(point1.Equals(point2));
    }
    /// <summary>
    /// Executes Equals_GivenMatchingValues_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void Equals_GivenMatchingValues_ShouldReturnTrue()
    {
        var point1 = new Point(1, 2);
        var point2 = new Point(1, 2);

        Assert.True(point1.Equals(point2));
    }

    /// <summary>
    /// #81 — a longer value object must NOT equal a shorter prefix of itself. The pre-#81 loop short-circuited
    /// on the second MoveNext and skipped the unmatched trailing element, declaring [a,b] == [a].
    /// </summary>
    [Fact]
    public void Equals_GivenLongerVsShorterPrefix_ShouldReturnFalse()
    {
        var longer = new Sequence("a", "b");
        var shorter = new Sequence("a");

        longer.Equals(shorter).ShouldBeFalse();
    }

    /// <summary>
    /// #81 — the unequal-length check is symmetric: a shorter value object must not equal a longer one.
    /// </summary>
    [Fact]
    public void Equals_GivenShorterVsLonger_ShouldReturnFalse()
    {
        var shorter = new Sequence("a");
        var longer = new Sequence("a", "b");

        shorter.Equals(longer).ShouldBeFalse();
    }

    /// <summary>
    /// #81 — equal-length, equal-valued sequences remain equal (no regression to genuine value equality).
    /// </summary>
    [Fact]
    public void Equals_GivenSameLengthSameValues_ShouldReturnTrue()
    {
        var left = new Sequence("a", "b");
        var right = new Sequence("a", "b");

        left.Equals(right).ShouldBeTrue();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    /// <summary>
    /// #81 — the strengthened hash is order-sensitive: swapping two equal-typed atomic values that collided
    /// under the previous XOR combiner now yields distinct hash codes (and the objects are not equal).
    /// </summary>
    [Fact]
    public void GetHashCode_GivenSwappedAtomicValues_ShouldDiffer()
    {
        var forward = new Sequence("x", "y");
        var swapped = new Sequence("y", "x");

        forward.Equals(swapped).ShouldBeFalse();
        forward.GetHashCode().ShouldNotBe(swapped.GetHashCode());
    }

    private sealed class Sequence : ValueObject
    {
        private readonly IReadOnlyList<string> values;

        public Sequence(params string[] values) => this.values = values;

        protected override IEnumerable<object> GetAtomicValues()
        {
            foreach (var value in this.values)
            {
                yield return value;
            }
        }
    }

    private class Point : ValueObject
    {
        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        public int Y { get; set; }

        private Point()
        { }
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return X;
            yield return Y;
        }
    }
}