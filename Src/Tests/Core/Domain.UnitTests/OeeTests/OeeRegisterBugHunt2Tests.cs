// <copyright file="OeeRegisterBugHunt2Tests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Models;
using Shouldly;
using Xunit;

namespace IndTrace.Domain.UnitTests.OeeTests;

/// <summary>
/// Bug Hunt Round 2: Tests for critical bugs discovered in helper methods and related functionality.
/// These tests target specific edge cases in SafeRatio, ClampMetric, ToKpiOee, and PerformanceData.
/// </summary>
public class OeeRegisterBugHunt2Tests
{
    /// <summary>
    /// Executes SafeRatio_WithNaNOrInfinity_ShouldUseFallback operation.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    [Theory]
    [InlineData(double.NaN, 1.0)]
    [InlineData(1.0, double.NaN)]
    [InlineData(double.NaN, double.NaN)]
    [InlineData(double.PositiveInfinity, 1.0)]
    [InlineData(1.0, double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity, 1.0)]
    [InlineData(1.0, double.NegativeInfinity)]
    public void SafeRatio_WithNaNOrInfinity_ShouldUseFallback(double numerator, double denominator)
    {
        // Act
        var result = OeeRegister.SafeRatio(numerator, denominator, 0.0, 1.0, 0.5);

        // Assert - Should never return NaN or Infinity
        result.ShouldNotBe(double.NaN, "SafeRatio should never return NaN");
        result.ShouldNotBe(double.PositiveInfinity, "SafeRatio should never return PositiveInfinity");
        result.ShouldNotBe(double.NegativeInfinity, "SafeRatio should never return NegativeInfinity");

        // Should return fallback for problematic inputs
        if (double.IsNaN(denominator) || double.IsInfinity(denominator) || denominator <= 0)
        {
            result.ShouldBe(0.5, "Should return fallback for invalid denominator");
        }
    }
    /// <summary>
    /// Executes ClampMetric_WithSpecialValues_ShouldHandleGracefully operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ClampMetric_WithSpecialValues_ShouldHandleGracefully(double value)
    {
        // Act
        var result = OeeRegister.ClampMetric(value, 0.0, 1.0);

        // Assert - Should never pass through NaN or Infinity
        result.ShouldNotBe(double.NaN, "ClampMetric should never return NaN");
        result.ShouldNotBe(double.PositiveInfinity, "ClampMetric should never return PositiveInfinity");
        result.ShouldNotBe(double.NegativeInfinity, "ClampMetric should never return NegativeInfinity");

        // Should clamp to reasonable bounds
        result.ShouldBeGreaterThanOrEqualTo(0.0);
        result.ShouldBeLessThanOrEqualTo(1.0);
    }
    /// <summary>
    /// Executes SafeRatio_DivisionResultingInNaN_ShouldUseFallback operation.
    /// </summary>

    [Fact]
    public void SafeRatio_DivisionResultingInNaN_ShouldUseFallback()
    {
        // Arrange - Operations that could result in NaN
        double numerator = 0.0;
        double denominator = 0.0; // This would cause 0/0 = NaN if not caught

        // Act
        var result = OeeRegister.SafeRatio(numerator, denominator, 0.0, 1.0, 0.75);

        // Assert
        result.ShouldBe(0.75, "Should use fallback for 0/0 case");
        result.ShouldNotBe(double.NaN);
    }
    /// <summary>
    /// Executes SafeRatio_DivisionResultingInInfinity_ShouldClampProperly operation.
    /// </summary>

    [Fact]
    public void SafeRatio_DivisionResultingInInfinity_ShouldClampProperly()
    {
        // Arrange - Operations that result in infinity
        double numerator = 1.0;
        double denominator = double.Epsilon; // Very small positive number

        // Act
        var result = OeeRegister.SafeRatio(numerator, denominator, 0.0, 1.0, 0.5);

        // Assert - Should use fallback for very large division results  
        result.ShouldBe(0.5, "Should return fallback for potential infinity division");
        result.ShouldNotBe(double.PositiveInfinity);
    }
    /// <summary>
    /// Executes ToKpiOee_WithNaNValues_ShouldNotPropagateNaN operation.
    /// </summary>

    [Fact]
    public void ToKpiOee_WithNaNValues_ShouldNotPropagateNaN()
    {
        // Arrange
        var register = new OeeRegister
        {
            Quality = double.NaN,
            Availability = double.NaN,
            Performance = double.NaN,
            Oee = double.NaN
        };

        // Act
        var result = OeeRegister.ToKpiOee(register);

        // Assert - Should not propagate NaN values
        result.Quality.Value.ShouldNotBe(double.NaN, "Quality should not be NaN");
        result.Availability.Value.ShouldNotBe(double.NaN, "Availability should not be NaN");
        result.Performance.Value.ShouldNotBe(double.NaN, "Performance should not be NaN");
        result.Oee.Value.ShouldNotBe(double.NaN, "Oee should not be NaN");
    }
    /// <summary>
    /// Executes ToKpiOee_WithInfinityValues_ShouldNotPropagateInfinity operation.
    /// </summary>

    [Fact]
    public void ToKpiOee_WithInfinityValues_ShouldNotPropagateInfinity()
    {
        // Arrange
        var register = new OeeRegister
        {
            Quality = double.PositiveInfinity,
            Availability = double.NegativeInfinity,
            Performance = double.PositiveInfinity,
            Oee = double.NegativeInfinity
        };

        // Act
        var result = OeeRegister.ToKpiOee(register);

        // Assert - Should not propagate Infinity values
        result.Quality.Value.ShouldNotBe(double.PositiveInfinity, "Quality should not be PositiveInfinity");
        result.Availability.Value.ShouldNotBe(double.NegativeInfinity, "Availability should not be NegativeInfinity");
        result.Performance.Value.ShouldNotBe(double.PositiveInfinity, "Performance should not be PositiveInfinity");
        result.Oee.Value.ShouldNotBe(double.NegativeInfinity, "Oee should not be NegativeInfinity");

        // Should have reasonable bounded values
        result.Quality.Value.ShouldBeGreaterThanOrEqualTo(0.0);
        result.Quality.Value.ShouldBeLessThanOrEqualTo(1.0);
    }
    /// <summary>
    /// Executes PerformanceData_FromPlc_SilentFailures_ShouldBeDocumented operation.
    /// </summary>
    /// <param name="invalidValue">The invalidValue.</param>
    /// <param name="expectedDefault">The expectedDefault.</param>

    [Theory]
    [InlineData("invalid_number", 0)]
    [InlineData("999999999999999999999", 0)] // Too large for int
    [InlineData("-999999999999999999999", 0)] // Too small for int
    [InlineData("", 0)]
    public void PerformanceData_FromPlc_SilentFailures_ShouldBeDocumented(string invalidValue, int expectedDefault)
    {
        // Arrange (null Value is no longer representable — Register.Create rejects null and the
        // column is NOT NULL; empty/unparseable strings are the reachable bad-data states.)
        var registers = new Dictionary<string, Register>
        {
            ["CurrentTime"] = Register.CreateFixture(value: invalidValue)
        };

        // Act
        var result = PerformanceData.FromPlc(registers);

        // Assert - Currently returns 0 for invalid data (potential data corruption)
        result.CurrentTime.ShouldBe(expectedDefault);

        // NOTE: This test documents current behavior which might be a bug
        // Silent failures could lead to data corruption in production
    }
    /// <summary>
    /// Executes PerformanceData_FromPlc_MissingKeys_ShouldUseDefaults operation.
    /// </summary>

    [Fact]
    public void PerformanceData_FromPlc_MissingKeys_ShouldUseDefaults()
    {
        // Arrange - Empty dictionary (all keys missing)
        var registers = new Dictionary<string, Register>();

        // Act
        var result = PerformanceData.FromPlc(registers);

        // Assert - Should use default values, not crash
        result.CurrentTime.ShouldBe(0);
        result.RunningTime.ShouldBe(0);
        result.TotalProduction.ShouldBe(0.0);
        result.StandardCycleTime.ShouldBe(0.0);
    }
    /// <summary>
    /// Executes PerformanceData_FromPlc_EmptyOrMissingRegisterValues_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void PerformanceData_FromPlc_EmptyOrMissingRegisterValues_ShouldHandleGracefully()
    {
        // Arrange — an empty reading and a missing key are the reachable "no data" states
        // (null Value / a null register object are no longer representable post-lockdown).
        var registers = new Dictionary<string, Register>
        {
            ["CurrentTime"] = Register.CreateFixture(value: string.Empty)
            // TotalProduction key intentionally absent — missing key must default gracefully.
        };

        // Act & Assert - Should not crash
        var result = Should.NotThrow(() => PerformanceData.FromPlc(registers));
        result.CurrentTime.ShouldBe(0);
        result.TotalProduction.ShouldBe(0.0);
    }
    /// <summary>
    /// Executes SafeRatio_PrecisionLoss_VerySmallValues_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void SafeRatio_PrecisionLoss_VerySmallValues_ShouldHandleCorrectly()
    {
        // Arrange - Very small values that might cause precision issues
        double numerator = double.Epsilon;
        double denominator = double.MaxValue;

        // Act
        var result = OeeRegister.SafeRatio(numerator, denominator, 0.0, 1.0, 0.5);

        // Assert - Should handle precision gracefully
        result.ShouldNotBe(double.NaN);
        result.ShouldBeGreaterThanOrEqualTo(0.0);
        result.ShouldBeLessThanOrEqualTo(1.0);
    }
    /// <summary>
    /// Executes SafeRatio_PrecisionLoss_VeryLargeValues_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void SafeRatio_PrecisionLoss_VeryLargeValues_ShouldHandleCorrectly()
    {
        // Arrange - Very large values that might cause precision issues
        double numerator = double.MaxValue;
        double denominator = double.Epsilon;

        // Act
        var result = OeeRegister.SafeRatio(numerator, denominator, 0.0, 1.0, 0.5);

        // Assert - Should use fallback for large division results that would cause infinity
        result.ShouldBe(0.5);
        result.ShouldNotBe(double.PositiveInfinity);
    }
}