// <copyright file="PerformanceRatioTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValueObjectsTests;

/// <summary>
/// Story 26.B1 unit tests for the <see cref="PerformanceRatio"/> value object (the <c>[0,1.5]</c> bound for OEE's
/// Performance metric, allowing over-performance up to 150%). <see cref="PerformanceRatio.Create"/> is the sole
/// validating railway boundary: it accepts finite in-range values (including the <c>1.5</c> ceiling) and returns a
/// <see cref="Result{T}"/> failure — never a throw — for out-of-range, NaN, or ±∞ input.
/// </summary>
public class PerformanceRatioTests
{
    /// <summary>
    /// In-range values, including the inclusive <c>0.0</c> and <c>1.5</c> boundaries and over-100% performance,
    /// are accepted and preserved exactly.
    /// </summary>
    /// <param name="value">The in-range performance ratio under test.</param>
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(1.25)]
    [InlineData(1.4999)]
    public void Create_WithInRangeValue_Succeeds_AndPreservesValue(double value)
    {
        // Act
        var result = PerformanceRatio.Create(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().Value.ShouldBe(value);
    }

    /// <summary>
    /// Values just outside <c>[0,1.5]</c> — notably just above the <c>1.5</c> ceiling — are rejected as a failure
    /// result (not a throw).
    /// </summary>
    /// <param name="value">The out-of-range performance ratio under test.</param>
    [Theory]
    [InlineData(-0.0001)]
    [InlineData(1.5001)]
    [InlineData(-1.0)]
    [InlineData(2.0)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    public void Create_WithOutOfRangeValue_Fails(double value)
    {
        // Act
        var result = PerformanceRatio.Create(value);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Non-finite input (NaN, ±∞) is rejected as a failure result (not a throw).
    /// </summary>
    /// <param name="value">The non-finite value under test.</param>
    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_WithNonFiniteValue_Fails(double value)
    {
        // Act
        var result = PerformanceRatio.Create(value);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Two performance ratios carrying the same underlying value are value-equal (the <see cref="ValueObject"/>
    /// contract).
    /// </summary>
    [Fact]
    public void PerformanceRatios_WithSameValue_AreValueEqual()
    {
        // Arrange
        var left = PerformanceRatio.Create(1.2).Value.ShouldNotBeNull();
        var right = PerformanceRatio.Create(1.2).Value.ShouldNotBeNull();
        var other = PerformanceRatio.Create(1.3).Value.ShouldNotBeNull();

        // Assert
        left.ShouldBe(right);
        left.GetHashCode().ShouldBe(right.GetHashCode());
        left.ShouldNotBe(other);
    }

    /// <summary>
    /// The published bounds match the OEE contract (<c>[0,1.5]</c>).
    /// </summary>
    [Fact]
    public void Bounds_MatchOeeContract()
    {
        PerformanceRatio.MinValue.ShouldBe(0.0);
        PerformanceRatio.MaxValue.ShouldBe(1.5);
    }
}
