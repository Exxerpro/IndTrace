// <copyright file="KpiOeeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.KpisTest;

/// <summary>
/// Unit tests for <see cref="KpiOee"/>, covering its defaults, the Story 2.5 guarded <see cref="KpiOee.Create"/>
/// factory, the internal unguarded <see cref="KpiOee.CreateFixture"/> seam (reachable here because
/// Domain.UnitTests is an <c>InternalsVisibleTo</c> grantee), and the <see cref="KpiOee.ClassifyOeeLevel"/>
/// banding helper. Story 26.B1: the four metric properties are now the bounded value objects
/// <see cref="Ratio"/> (<c>[0,1]</c>) and <see cref="PerformanceRatio"/> (<c>[0,1.5]</c>); numeric assertions read
/// the underlying <c>double</c> via <see cref="Ratio.Value"/> / <see cref="PerformanceRatio.Value"/>.
/// </summary>
public class KpiOeeTests
{
    /// <summary>
    /// Executes KpiOee_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void KpiOee_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new KpiOee();

        // Assert
        instance.ShouldNotBeNull();
        instance.KpiOeeId.ShouldBe(default(int));
        instance.OeeRegisterId.ShouldBe(default(int));

        // Story 26.B1: metric VOs default to a zero-valued Ratio/PerformanceRatio (== the pre-retype default 0.0).
        instance.Oee.Value.ShouldBe(default(double));
        instance.Availability.Value.ShouldBe(default(double));
        instance.Performance.Value.ShouldBe(default(double));
        instance.Quality.Value.ShouldBe(default(double));
        instance.TimeStamp.ShouldBe(default(DateTime));
        instance.OeeRegister.ShouldBeNull();
        instance.ShouldBeAssignableTo<IEntityRoot>();
    }

    /// <summary>
    /// Story 2.5: invalid metrics (negative and out-of-range-high) are now rejected by the guarded
    /// <see cref="KpiOee.Create"/> factory instead of being silently stored, while the unguarded
    /// <see cref="KpiOee.CreateFixture"/> seam can still HOLD arbitrary/legacy values when a test needs them.
    /// </summary>
    [Fact]
    public void KpiOee_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Act - negative metrics are rejected.
        var negativeResult = KpiOee.Create(
            oeeRegisterId: -100,
            oee: -0.5,
            availability: -0.3,
            performance: -0.7,
            quality: -0.2,
            timeStamp: DateTime.UtcNow);

        // Assert
        negativeResult.IsSuccess.ShouldBeFalse();
        negativeResult.Errors.ShouldNotBeEmpty();

        // Act - extreme over-range metrics are rejected.
        var extremeResult = KpiOee.Create(
            oeeRegisterId: int.MaxValue,
            oee: 5.0,
            availability: 2.5,
            performance: 3.0,
            quality: 1.5,
            timeStamp: DateTime.UtcNow);

        // Assert
        extremeResult.IsSuccess.ShouldBeFalse();
        extremeResult.Errors.ShouldNotBeEmpty();

        // Act - the unguarded seam still holds those arbitrary values verbatim.
        var heldExtreme = KpiOee.CreateFixture(
            kpiOeeId: int.MaxValue,
            oeeRegisterId: int.MaxValue,
            oee: 5.0,
            availability: 2.5,
            performance: 3.0,
            quality: 1.5,
            timeStamp: DateTime.UtcNow);

        // Assert
        heldExtreme.ShouldNotBeNull();
        heldExtreme.Oee.Value.ShouldBe(5.0);
        heldExtreme.Availability.Value.ShouldBe(2.5);
        heldExtreme.Performance.Value.ShouldBe(3.0);
        heldExtreme.Quality.Value.ShouldBe(1.5);
    }

    /// <summary>
    /// Story 2.5: rejects each non-finite metric (NaN / Infinity) through the guarded factory.
    /// </summary>
    [Fact]
    public void Create_WithNonFiniteMetrics_ShouldFail()
    {
        // Act
        var nanResult = KpiOee.Create(1, double.NaN, 0.9, 0.9, 0.9, DateTime.UtcNow);
        var infinityResult = KpiOee.Create(1, 0.9, double.PositiveInfinity, 0.9, 0.9, DateTime.UtcNow);

        // Assert
        nanResult.IsSuccess.ShouldBeFalse();
        nanResult.Errors.ShouldNotBeEmpty();
        infinityResult.IsSuccess.ShouldBeFalse();
        infinityResult.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Story 2.5: <see cref="KpiOee.Create"/> succeeds for valid inputs, stores every metric exactly, and
    /// accepts performance above 100% (up to 150%). Identity (<c>KpiOeeId</c>) remains publicly settable.
    /// </summary>
    [Fact]
    public void KpiOee_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var testTimeStamp = DateTime.UtcNow;

        // Act
        var result = KpiOee.Create(
            oeeRegisterId: 67890,
            oee: 0.85,
            availability: 0.90,
            performance: 0.95,
            quality: 0.99,
            timeStamp: testTimeStamp);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var instance = result.Value.ShouldNotBeNull();
        instance.OeeRegisterId.ShouldBe(67890);
        instance.Oee.Value.ShouldBe(0.85);
        instance.Availability.Value.ShouldBe(0.90);
        instance.Performance.Value.ShouldBe(0.95);
        instance.Quality.Value.ShouldBe(0.99);
        instance.TimeStamp.ShouldBe(testTimeStamp);

        // KpiOeeId remains a publicly settable, database-assigned identity.
        instance.KpiOeeId = 12345;
        instance.KpiOeeId.ShouldBe(12345);

        // Performance can legitimately exceed 100% (better than standard, up to 150%).
        var overPerformance = KpiOee.Create(
            oeeRegisterId: 1,
            oee: 0.65,
            availability: 0.85,
            performance: 1.05,
            quality: 0.97,
            timeStamp: testTimeStamp);

        overPerformance.IsSuccess.ShouldBeTrue();
        var overPerformanceKpi = overPerformance.Value.ShouldNotBeNull();
        overPerformanceKpi.Oee.Value.ShouldBe(0.65);
        overPerformanceKpi.Availability.Value.ShouldBe(0.85);
        overPerformanceKpi.Performance.Value.ShouldBe(1.05);
        overPerformanceKpi.Quality.Value.ShouldBe(0.97);
    }

    /// <summary>
    /// Executes KpiOee_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>
    [Fact]
    public void KpiOee_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var instance = KpiOee.CreateFixture(
            kpiOeeId: 1,
            oeeRegisterId: 1001,
            oee: 0.75,
            availability: 0.85,
            performance: 0.92,
            quality: 0.96,
            timeStamp: DateTime.UtcNow);

        // Act & Assert - Test object equality (reference equality by default)
        var instance1 = KpiOee.CreateFixture(1, 0, 0.75, 0, 0, 0, default);
        var instance2 = KpiOee.CreateFixture(1, 0, 0.75, 0, 0, 0, default);
        var instance3 = instance1;

        instance1.ShouldNotBeSameAs(instance2); // Different instances
        instance1.ShouldBeSameAs(instance3); // Same reference
        (instance1 == instance2).ShouldBeFalse(); // Reference equality
        (instance1 == instance3).ShouldBeTrue(); // Same reference

        // Test GetHashCode method
        var hashCode1 = instance1.GetHashCode();
        var hashCode3 = instance3.GetHashCode();
        hashCode1.ShouldBe(hashCode3); // Same reference should have same hash code

        // Test GetType method
        var type = instance.GetType();
        type.ShouldNotBeNull();
        type.Name.ShouldBe("KpiOee");

        // Test ToString method
        var toStringResult = instance.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldContain("Kpi");

        // Test property reflection - Story 26.B1 retyped the four metric properties from double onto the bounded
        // value objects Ratio ([0,1]) and PerformanceRatio ([0,1.5]); the mapped surface is unchanged at 8
        // properties (7 scalar + 1 navigation) and ClassifyOeeLevel remains a method, not a property.
        var properties = type.GetProperties();
        properties.Length.ShouldBe(8); // 7 scalar properties + 1 navigation property

        var oeeProperty = properties.FirstOrDefault(p => p.Name == "Oee");
        oeeProperty.ShouldNotBeNull();
        oeeProperty.PropertyType.ShouldBe(typeof(Ratio));

        var availabilityProperty = properties.FirstOrDefault(p => p.Name == "Availability");
        availabilityProperty.ShouldNotBeNull();
        availabilityProperty.PropertyType.ShouldBe(typeof(Ratio));

        var performanceProperty = properties.FirstOrDefault(p => p.Name == "Performance");
        performanceProperty.ShouldNotBeNull();
        performanceProperty.PropertyType.ShouldBe(typeof(PerformanceRatio));

        var qualityProperty = properties.FirstOrDefault(p => p.Name == "Quality");
        qualityProperty.ShouldNotBeNull();
        qualityProperty.PropertyType.ShouldBe(typeof(Ratio));
    }

    /// <summary>
    /// Executes KpiOee_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>
    [Fact]
    public void KpiOee_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Automotive manufacturing OEE KPI scenarios (all within valid ranges → guarded Create).
        var worldClassKpi = KpiOee.Create(1001, 0.85, 0.90, 0.95, 0.99, DateTime.UtcNow).Value.ShouldNotBeNull();
        var averageKpi = KpiOee.Create(1002, 0.60, 0.75, 0.85, 0.94, DateTime.UtcNow.AddHours(-1)).Value.ShouldNotBeNull();
        var poorKpi = KpiOee.Create(1003, 0.40, 0.60, 0.70, 0.95, DateTime.UtcNow.AddHours(-2)).Value.ShouldNotBeNull();

        worldClassKpi.KpiOeeId = 1;
        averageKpi.KpiOeeId = 2;
        poorKpi.KpiOeeId = 3;

        // Act & Assert - Verify automotive manufacturing OEE business rules
        var kpis = new List<KpiOee> { worldClassKpi, averageKpi, poorKpi };

        // Business Rule 1: OEE should be product of Availability × Performance × Quality
        foreach (var kpi in kpis)
        {
            var calculatedOee = kpi.Availability.Value * kpi.Performance.Value * kpi.Quality.Value;
            kpi.Oee.Value.ShouldBe(calculatedOee, 0.01); // Allow small tolerance for rounding
        }

        // Business Rule 2: World-class manufacturing thresholds
        worldClassKpi.Oee.Value.ShouldBeGreaterThan(0.80); // World-class threshold
        worldClassKpi.Availability.Value.ShouldBeGreaterThan(0.85);
        worldClassKpi.Performance.Value.ShouldBeGreaterThan(0.90);
        worldClassKpi.Quality.Value.ShouldBeGreaterThan(0.95);

        // Business Rule 3: Performance can exceed 100% (better than standard)
        var highPerformanceKpi = KpiOee.Create(0, 0.0, 0.95, 1.15, 0.98, DateTime.UtcNow).Value.ShouldNotBeNull();
        highPerformanceKpi.Performance.Value.ShouldBeGreaterThan(1.0);

        // Business Rule 4: OEE classification levels
        worldClassKpi.Oee.Value.ShouldBeGreaterThanOrEqualTo(0.85); // World-class
        averageKpi.Oee.Value.ShouldBeLessThan(0.85);
        averageKpi.Oee.Value.ShouldBeGreaterThanOrEqualTo(0.50); // Average
        poorKpi.Oee.Value.ShouldBeLessThan(0.50); // Poor

        // Business Rule 5: Temporal consistency
        worldClassKpi.TimeStamp.ShouldBeGreaterThan(averageKpi.TimeStamp);
        averageKpi.TimeStamp.ShouldBeGreaterThan(poorKpi.TimeStamp);

        // Business Rule 6: Quality should typically be highest metric
        foreach (var kpi in kpis)
        {
            kpi.Quality.Value.ShouldBeGreaterThanOrEqualTo(kpi.Availability.Value);
            // Note: Performance can exceed Quality due to faster than standard cycle times
        }

        // Business Rule 7: OEE improvement tracking
        var kpiTrend = kpis.OrderBy(k => k.TimeStamp).ToList();
        var improvementTrend = kpiTrend.Select(k => k.Oee.Value).ToList();

        // Verify we have different OEE levels for comparison
        improvementTrend.ShouldContain(oee => oee > 0.80); // World-class
        improvementTrend.ShouldContain(oee => oee >= 0.50 && oee < 0.80); // Average
        improvementTrend.ShouldContain(oee => oee < 0.50); // Poor
    }

    /// <summary>
    /// Executes ManufacturingKpiScenarios_WithRealWorldData_ShouldCalculateCorrectly operation.
    /// </summary>
    [Fact]
    public void ManufacturingKpiScenarios_WithRealWorldData_ShouldCalculateCorrectly()
    {
        // Arrange - Real automotive stamping press OEE scenarios (valid ranges → guarded Create).
        var morningShiftKpi = KpiOee.Create(2001, 0.72, 0.85, 0.90, 0.94, DateTime.Today.AddHours(8)).Value.ShouldNotBeNull();
        var afternoonShiftKpi = KpiOee.Create(2001, 0.78, 0.90, 0.92, 0.94, DateTime.Today.AddHours(16)).Value.ShouldNotBeNull();
        var nightShiftKpi = KpiOee.Create(2001, 0.81, 0.92, 0.95, 0.93, DateTime.Today.AddHours(24)).Value.ShouldNotBeNull();

        morningShiftKpi.KpiOeeId = 101;
        afternoonShiftKpi.KpiOeeId = 102;
        nightShiftKpi.KpiOeeId = 103;

        // Act & Assert - Verify real-world automotive manufacturing patterns
        var shiftKpis = new List<KpiOee> { morningShiftKpi, afternoonShiftKpi, nightShiftKpi };

        // Manufacturing analysis: OEE should improve through the day as equipment warms up
        var oeeTrend = shiftKpis.OrderBy(k => k.TimeStamp).Select(k => k.Oee.Value).ToList();
        oeeTrend[0].ShouldBe(0.72); // Morning: lowest
        oeeTrend[1].ShouldBe(0.78); // Afternoon: improved
        oeeTrend[2].ShouldBe(0.81); // Night: highest

        // Availability should improve as operators become more experienced with equipment
        morningShiftKpi.Availability.Value.ShouldBe(0.85);
        afternoonShiftKpi.Availability.Value.ShouldBe(0.90);
        nightShiftKpi.Availability.Value.ShouldBe(0.92);

        // Performance should improve with equipment warm-up and operator experience
        morningShiftKpi.Performance.Value.ShouldBeLessThan(afternoonShiftKpi.Performance.Value);
        afternoonShiftKpi.Performance.Value.ShouldBeLessThan(nightShiftKpi.Performance.Value);

        // Quality should remain relatively stable (good process control)
        foreach (var kpi in shiftKpis)
        {
            kpi.Quality.Value.ShouldBeGreaterThan(0.90); // Minimum quality standard
            kpi.Quality.Value.ShouldBeLessThan(0.98); // Realistic upper bound
        }

        // Overall OEE should be in acceptable automotive range
        foreach (var kpi in shiftKpis)
        {
            kpi.Oee.Value.ShouldBeGreaterThan(0.70); // Minimum acceptable for automotive
            kpi.Oee.Value.ShouldBeLessThan(0.90); // Realistic upper bound
        }
    }

    /// <summary>
    /// Executes EdgeCaseKpiValues_ShouldBeHandledAppropriately operation.
    /// </summary>
    [Fact]
    public void EdgeCaseKpiValues_ShouldBeHandledAppropriately()
    {
        // Arrange - Edge case scenarios. Perfect/zero are within range (Create); the super-performance case
        // stores an OEE above 1.0 which only the unguarded CreateFixture seam can hold.
        var perfectKpi = KpiOee.Create(0, 1.0, 1.0, 1.0, 1.0, DateTime.UtcNow).Value.ShouldNotBeNull();
        perfectKpi.KpiOeeId = 1000;

        var zeroKpi = KpiOee.Create(0, 0.0, 0.0, 0.0, 0.0, DateTime.UtcNow).Value.ShouldNotBeNull();
        zeroKpi.KpiOeeId = 1001;

        var superPerformanceKpi = KpiOee.CreateFixture(
            kpiOeeId: 1002,
            oeeRegisterId: 0,
            oee: 1.15, // 115% OEE (possible with super performance) - out of Create's [0,1] range
            availability: 1.0,
            performance: 1.25, // 125% performance (25% faster than standard)
            quality: 0.92,
            timeStamp: DateTime.UtcNow);

        // Act & Assert - Perfect scenario
        perfectKpi.Oee.Value.ShouldBe(1.0);
        perfectKpi.Availability.Value.ShouldBe(1.0);
        perfectKpi.Performance.Value.ShouldBe(1.0);
        perfectKpi.Quality.Value.ShouldBe(1.0);

        // Zero scenario
        zeroKpi.Oee.Value.ShouldBe(0.0);
        zeroKpi.Availability.Value.ShouldBe(0.0);
        zeroKpi.Performance.Value.ShouldBe(0.0);
        zeroKpi.Quality.Value.ShouldBe(0.0);

        // Super performance scenario
        superPerformanceKpi.Performance.Value.ShouldBeGreaterThan(1.0);
        superPerformanceKpi.Oee.Value.ShouldBeGreaterThan(1.0);
        var calculatedOee = superPerformanceKpi.Availability.Value * superPerformanceKpi.Performance.Value * superPerformanceKpi.Quality.Value;
        superPerformanceKpi.Oee.Value.ShouldBe(calculatedOee, 0.001);

        // Verify super performance is mathematically consistent
        calculatedOee.ShouldBe(1.0 * 1.25 * 0.92); // = 1.15
        calculatedOee.ShouldBe(1.15, 0.001);
    }

    /// <summary>
    /// Executes KpiOeeComparison_WithDifferentTimeStamps_ShouldSupportTrendAnalysis operation.
    /// </summary>
    [Fact]
    public void KpiOeeComparison_WithDifferentTimeStamps_ShouldSupportTrendAnalysis()
    {
        // Arrange - Weekly OEE trend analysis (seeded via the unguarded fixture seam).
        var weeklyKpis = new List<KpiOee>
        {
            KpiOee.CreateFixture(1, 0, 0.65, 0, 0, 0, DateTime.Today.AddDays(-6)), // Monday
            KpiOee.CreateFixture(2, 0, 0.68, 0, 0, 0, DateTime.Today.AddDays(-5)), // Tuesday
            KpiOee.CreateFixture(3, 0, 0.72, 0, 0, 0, DateTime.Today.AddDays(-4)), // Wednesday
            KpiOee.CreateFixture(4, 0, 0.75, 0, 0, 0, DateTime.Today.AddDays(-3)), // Thursday
            KpiOee.CreateFixture(5, 0, 0.78, 0, 0, 0, DateTime.Today.AddDays(-2)), // Friday
            KpiOee.CreateFixture(6, 0, 0.73, 0, 0, 0, DateTime.Today.AddDays(-1)), // Saturday
            KpiOee.CreateFixture(7, 0, 0.70, 0, 0, 0, DateTime.Today),             // Sunday
        };

        // Act - Analyze trend
        var orderedKpis = weeklyKpis.OrderBy(k => k.TimeStamp).ToList();
        var oeeValues = orderedKpis.Select(k => k.Oee.Value).ToList();

        // Assert - Verify weekly improvement trend (with weekend reduction)
        oeeValues[0].ShouldBe(0.65); // Monday: lowest (week start)
        oeeValues[4].ShouldBe(0.78); // Friday: highest (peak efficiency)
        oeeValues[6].ShouldBe(0.70); // Sunday: reduced (weekend operation)

        // Weekday improvement trend
        var weekdayKpis = orderedKpis.Take(5).ToList(); // Monday-Friday
        var weekdayOees = weekdayKpis.Select(k => k.Oee.Value).ToList();

        for (int i = 1; i < weekdayOees.Count; i++)
        {
            weekdayOees[i].ShouldBeGreaterThan(weekdayOees[i - 1]); // Continuous improvement
        }

        // Weekend performance analysis
        var weekendAverage = orderedKpis.Skip(5).Average(k => k.Oee.Value); // Saturday + Sunday
        var weekdayAverage = weekdayKpis.Average(k => k.Oee.Value);

        weekendAverage.ShouldBeLessThan(weekdayAverage); // Weekend typically lower due to skeleton crew
        weekendAverage.ShouldBe(0.715, 0.001); // (0.73 + 0.70) / 2 = 0.715
    }

    /// <summary>
    /// Story 2.5 / 3.1: <see cref="KpiOee.ClassifyOeeLevel"/> bands the STORED OEE directly through the single
    /// banding source (<see cref="OeeMetrics.ClassifyLevel"/>), independent of A×P×Q recomposition.
    /// </summary>
    [Theory]
    [InlineData(0.90, OeePerformanceLevel.WorldClass)]
    [InlineData(0.85, OeePerformanceLevel.WorldClass)]
    [InlineData(0.70, OeePerformanceLevel.Good)]
    [InlineData(0.65, OeePerformanceLevel.Good)]
    [InlineData(0.50, OeePerformanceLevel.Fair)]
    [InlineData(0.40, OeePerformanceLevel.Fair)]
    [InlineData(0.10, OeePerformanceLevel.Poor)]
    [InlineData(0.0, OeePerformanceLevel.Poor)]
    public void ClassifyOeeLevel_ReturnsBandForStoredOee(double oee, OeePerformanceLevel expected)
    {
        // Arrange
        var kpi = KpiOee.CreateFixture(1, 1, oee, 0.9, 0.9, 0.9, DateTime.UtcNow);

        // Act & Assert
        kpi.ClassifyOeeLevel().ShouldBe(expected);
    }

    /// <summary>
    /// #91 guard: a corrupt non-finite stored OEE (NaN / +Infinity / -Infinity) — reachable only via the
    /// unguarded <see cref="KpiOee.CreateFixture"/> / EF materialization seam, since <see cref="KpiOee.Create"/>
    /// rejects non-finite metrics — must NOT throw <see cref="OverflowException"/> on the internal
    /// <c>(decimal)</c> cast. It fails SAFE to the worst band (<see cref="OeePerformanceLevel.Poor"/>).
    /// </summary>
    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ClassifyOeeLevel_NonFiniteStoredOee_FailsSafeToPoor(double oee)
    {
        // Arrange
        var kpi = KpiOee.CreateFixture(1, 1, oee, 0.9, 0.9, 0.9, DateTime.UtcNow);

        // Act & Assert — no OverflowException; classified as the worst band.
        kpi.ClassifyOeeLevel().ShouldBe(OeePerformanceLevel.Poor);
    }
}
