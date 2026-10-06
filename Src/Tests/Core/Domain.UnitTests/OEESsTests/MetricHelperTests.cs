// <copyright file="MetricHelperTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.OEESsTests;
/// <summary>
/// Represents the MetricHelperTests.
/// </summary>

public class MetricHelperTests
{
    /// <summary>
    /// Executes SafeRatio_ShouldBehaveCorrectly operation.
    /// </summary>
    [Theory]
    [InlineData(50, 100, 0.0, 1.0, 0.5)]     // Normal case
    [InlineData(10, 100, 0.0, 1.0, 0.1)]     // Normal case
    [InlineData(100, 0, 0.0, 1.0, 0.42)]     // Zero denominator → fallback
    [InlineData(200, 100, 0.0, 1.0, 1.0)]    // Exceeds max → fallback
    [InlineData(-50, 100, 0.0, 1.0, 0.0)]   // Below min → fallback
    public void SafeRatio_ShouldBehaveCorrectly(
        double numerator, double denominator, double min, double max, double expected)
    {
        double fallback = 0.42;
        var result = OeeRegister.SafeRatio(numerator, denominator, min, max, fallback);
        result.ShouldBe(expected, 0.0001);
    }
    /// <summary>
    /// Executes ClampMetric_ShouldClampCorrectly operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="min">The min.</param>
    /// <param name="max">The max.</param>
    /// <param name="expected">The expected.</param>

    [Theory]
    [InlineData(0.95, 0.0, 1.0, 0.95)]
    [InlineData(1.2, 0.0, 1.0, 1.0)]
    [InlineData(-0.3, 0.0, 1.0, 0.0)]
    [InlineData(0.0, 0.0, 1.0, 0.0)]
    [InlineData(1.0, 0.0, 1.0, 1.0)]
    [InlineData(1.1, 0.0, 1.0, 1.0)]
    public void ClampMetric_ShouldClampCorrectly(double input, double min, double max, double expected)
    {
        var result = OeeRegister.ClampMetric(input, min, max);
        result.ShouldBe(expected, 0.0001);
    }

    // Inline helper methods (or move to a static class if preferred)
}