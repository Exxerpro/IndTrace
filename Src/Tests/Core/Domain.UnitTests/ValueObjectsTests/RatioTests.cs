// <copyright file="RatioTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValueObjectsTests;

/// <summary>
/// Story 26.B1 unit tests for the <see cref="Ratio"/> value object (the <c>[0,1]</c> bound shared by OEE's
/// Availability, Quality, and the composite OEE metric). <see cref="Ratio.Create"/> is the sole validating
/// railway boundary: it accepts finite in-range values (including the <c>0.0</c> and <c>1.0</c> boundaries) and
/// returns a <see cref="Result{T}"/> failure — never a throw — for out-of-range, NaN, or ±∞ input.
/// </summary>
public class RatioTests
{
    /// <summary>
    /// In-range values, including the inclusive <c>0.0</c> and <c>1.0</c> boundaries, are accepted and preserved
    /// exactly.
    /// </summary>
    /// <param name="value">The in-range proportion under test.</param>
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(0.5)]
    [InlineData(0.8500)]
    [InlineData(0.000001)]
    [InlineData(0.999999)]
    public void Create_WithInRangeValue_Succeeds_AndPreservesValue(double value)
    {
        // Act
        var result = Ratio.Create(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().Value.ShouldBe(value);
    }

    /// <summary>
    /// Values just outside <c>[0,1]</c> are rejected as a failure result (not a throw).
    /// </summary>
    /// <param name="value">The out-of-range proportion under test.</param>
    [Theory]
    [InlineData(-0.0001)]
    [InlineData(1.0001)]
    [InlineData(-1.0)]
    [InlineData(1.5)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    public void Create_WithOutOfRangeValue_Fails(double value)
    {
        // Act
        var result = Ratio.Create(value);

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
        var result = Ratio.Create(value);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Two ratios carrying the same underlying value are value-equal (the <see cref="ValueObject"/> contract).
    /// </summary>
    [Fact]
    public void Ratios_WithSameValue_AreValueEqual()
    {
        // Arrange
        var left = Ratio.Create(0.42).Value.ShouldNotBeNull();
        var right = Ratio.Create(0.42).Value.ShouldNotBeNull();
        var other = Ratio.Create(0.43).Value.ShouldNotBeNull();

        // Assert
        left.ShouldBe(right);
        left.GetHashCode().ShouldBe(right.GetHashCode());
        left.ShouldNotBe(other);
    }

    /// <summary>
    /// The published bounds match the OEE contract (<c>[0,1]</c>).
    /// </summary>
    [Fact]
    public void Bounds_MatchOeeContract()
    {
        Ratio.MinValue.ShouldBe(0.0);
        Ratio.MaxValue.ShouldBe(1.0);
    }
}
