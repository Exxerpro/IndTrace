// <copyright file="OeeMetricsClassifyLevelTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValueObjectsTests;

/// <summary>
/// Story 3.1: pins the boundary semantics of <see cref="OeeMetrics.ClassifyLevel"/>, the single source of OEE
/// banding. The bands use inclusive lower bounds (<c>&gt;=</c>): World-class &gt;= 0.85, Good &gt;= 0.65,
/// Fair &gt;= 0.40, otherwise Poor.
/// </summary>
public class OeeMetricsClassifyLevelTests
{
    /// <summary>
    /// Verifies the classification at and around every band boundary.
    /// </summary>
    /// <param name="oee">The OEE value to classify.</param>
    /// <param name="expected">The expected performance band.</param>
    [Theory]
    [InlineData(1.0, OeePerformanceLevel.WorldClass)]
    [InlineData(0.85, OeePerformanceLevel.WorldClass)]
    [InlineData(0.8499, OeePerformanceLevel.Good)]
    [InlineData(0.65, OeePerformanceLevel.Good)]
    [InlineData(0.6499, OeePerformanceLevel.Fair)]
    [InlineData(0.64, OeePerformanceLevel.Fair)]
    [InlineData(0.40, OeePerformanceLevel.Fair)]
    [InlineData(0.39, OeePerformanceLevel.Poor)]
    [InlineData(0.0, OeePerformanceLevel.Poor)]
    public void ClassifyLevel_AtBandBoundaries_ReturnsExpectedLevel(decimal oee, OeePerformanceLevel expected)
    {
        // Act
        var level = OeeMetrics.ClassifyLevel(oee);

        // Assert
        level.ShouldBe(expected);
    }

    /// <summary>
    /// Verifies the instance <see cref="OeeMetrics.PerformanceLevel"/> accessor delegates to the same banding
    /// source, so a metric whose computed OEE lands in a band reports that band.
    /// </summary>
    [Fact]
    public void PerformanceLevel_DelegatesToClassifyLevel()
    {
        // Arrange - Availability × Performance × Quality = 1.0 × 1.0 × 0.90 = 0.90 → World-class.
        var metrics = new OeeMetrics(1.0m, 1.0m, 0.90m);

        // Act & Assert
        metrics.PerformanceLevel.ShouldBe(OeeMetrics.ClassifyLevel(metrics.Oee));
        metrics.PerformanceLevel.ShouldBe(OeePerformanceLevel.WorldClass);
    }
}
