// <copyright file="CycleTimeWindowTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValueObjectsTests;

/// <summary>
/// Unit tests for the <see cref="CycleTimeWindow"/> value object, covering the
/// <see cref="CycleTimeWindow.Create"/> invariant and the canonical exclusive-bounds
/// <see cref="CycleTimeWindow.Contains"/> predicate.
/// </summary>
public class CycleTimeWindowTests
{
    /// <summary>
    /// Tests that Create succeeds for a valid 0 &lt;= minimum &lt;= maximum window and exposes the bounds.
    /// </summary>
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 100)]
    [InlineData(20, 100)]
    [InlineData(50, 50)]
    public void Create_WithValidBounds_ShouldSucceed(int minimum, int maximum)
    {
        // Act
        var result = CycleTimeWindow.Create(minimum, maximum);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var window = result.Value.ShouldNotBeNull();
        window.Minimum.ShouldBe(minimum);
        window.Maximum.ShouldBe(maximum);
    }

    /// <summary>
    /// Tests that Create fails when the minimum is negative.
    /// </summary>
    [Theory]
    [InlineData(-1, 100)]
    [InlineData(-200, 0)]
    public void Create_WithNegativeMinimum_ShouldFail(int minimum, int maximum)
    {
        // Act
        var result = CycleTimeWindow.Create(minimum, maximum);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"Cycle time minimum cannot be negative: {minimum}s");
    }

    /// <summary>
    /// Tests that Create fails when the minimum exceeds the maximum (inverted bounds).
    /// </summary>
    [Theory]
    [InlineData(100, 20)]
    [InlineData(50, 40)]
    public void Create_WithInvertedBounds_ShouldFail(int minimum, int maximum)
    {
        // Act
        var result = CycleTimeWindow.Create(minimum, maximum);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"Cycle time minimum {minimum}s cannot exceed maximum {maximum}s");
    }

    /// <summary>
    /// Tests that Create accepts minimum equal to maximum (degenerate but valid window).
    /// </summary>
    [Fact]
    public void Create_WithMinimumEqualToMaximum_ShouldSucceed()
    {
        // Act
        var result = CycleTimeWindow.Create(50, 50);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var window = result.Value.ShouldNotBeNull();
        window.Minimum.ShouldBe(50);
        window.Maximum.ShouldBe(50);
    }

    /// <summary>
    /// Tests that Contains is exclusive at the minimum bound (value equal to minimum is out of range).
    /// </summary>
    [Fact]
    public void Contains_AtMinimumBound_ShouldBeFalse()
    {
        // Arrange
        var window = CycleTimeWindow.Create(20, 100).Value.ShouldNotBeNull();

        // Act & Assert
        window.Contains(20).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that Contains is exclusive at the maximum bound (value equal to maximum is out of range).
    /// </summary>
    [Fact]
    public void Contains_AtMaximumBound_ShouldBeFalse()
    {
        // Arrange
        var window = CycleTimeWindow.Create(20, 100).Value.ShouldNotBeNull();

        // Act & Assert
        window.Contains(100).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that Contains is true one unit inside each bound and mid-range.
    /// </summary>
    [Theory]
    [InlineData(21)]
    [InlineData(50)]
    [InlineData(99)]
    public void Contains_StrictlyInsideBounds_ShouldBeTrue(int cycleTime)
    {
        // Arrange
        var window = CycleTimeWindow.Create(20, 100).Value.ShouldNotBeNull();

        // Act & Assert
        window.Contains(cycleTime).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that Contains rejects negative cycle times.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Contains_WithNegativeCycleTime_ShouldBeFalse(int cycleTime)
    {
        // Arrange
        var window = CycleTimeWindow.Create(0, 100).Value.ShouldNotBeNull();

        // Act & Assert
        window.Contains(cycleTime).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that zero is out of range when the minimum is zero (exclusive lower bound).
    /// </summary>
    [Fact]
    public void Contains_ZeroWithZeroMinimum_ShouldBeFalse()
    {
        // Arrange
        var window = CycleTimeWindow.Create(0, 100).Value.ShouldNotBeNull();

        // Act & Assert
        window.Contains(0).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that one is the smallest in-range value when the minimum is zero.
    /// </summary>
    [Fact]
    public void Contains_OneWithZeroMinimum_ShouldBeTrue()
    {
        // Arrange
        var window = CycleTimeWindow.Create(0, 100).Value.ShouldNotBeNull();

        // Act & Assert
        window.Contains(1).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that a degenerate window (minimum equals maximum) contains no value.
    /// </summary>
    [Fact]
    public void Contains_DegenerateWindow_ShouldContainNothing()
    {
        // Arrange
        var window = CycleTimeWindow.Create(50, 50).Value.ShouldNotBeNull();

        // Act & Assert
        window.Contains(50).ShouldBeFalse();
        window.Contains(49).ShouldBeFalse();
        window.Contains(51).ShouldBeFalse();
    }

    /// <summary>
    /// Tests value-object equality: two windows with identical bounds are equal.
    /// </summary>
    [Fact]
    public void Equals_WithIdenticalBounds_ShouldBeEqual()
    {
        // Arrange
        var first = CycleTimeWindow.Create(20, 100).Value.ShouldNotBeNull();
        var second = CycleTimeWindow.Create(20, 100).Value.ShouldNotBeNull();

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }
}
