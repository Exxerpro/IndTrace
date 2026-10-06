// <copyright file="OeeRegisterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.OeeTests;
/// <summary>
/// Represents the OeeRegisterTests.
/// </summary>

public class OeeRegisterTests
{
    private OeeRegister _register;
    private PerformanceData _data;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public OeeRegisterTests()
    {
        _register = new OeeRegister
        {
            PlanedProductionTime = 480, // 8 hours
            StandardCycleTime = 1.0     // 1 unit per minute
        };

        _data = new PerformanceData
        {
            RunningTime = 420,
            StoppedTime = 30,
            FaultedTime = 30,
            TotalProduction = 400,
            ProductionOk = 380,
            ProductionNoK = 20,
            CurrentTime = 480,
            ActualCycleTime = 1.05
        };
    }
    /// <summary>
    /// Executes CalculateOee_WithIdealValues_ShouldCalculateCorrectly operation.
    /// </summary>

    [Fact]
    public void CalculateOee_WithIdealValues_ShouldCalculateCorrectly()
    {
        // Act
        var result = OeeRegister.CalculateOee(_register, _data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var oee = result.Value;
        oee.ShouldNotBeNull();
        oee.ShouldNotBeNull();
        oee.Quality.ShouldBe(0.95, 0.001); // 380 / 400
        oee.Availability.ShouldBe(0.875, 0.001); // 420 / 480
        oee.Performance.ShouldBe(0.952, 0.001); // (1.0 * 400) / 420
        oee.Oee.ShouldBe(0.791, 0.001); // 0.95 * 0.875 * 0.952
    }
    /// <summary>
    /// Executes CalculateOee_WithNullInputs_ShouldReturnFailure operation.
    /// </summary>

    [Fact]
    public void CalculateOee_WithNullInputs_ShouldReturnFailure()
    {
        // Act
        var result1 = OeeRegister.CalculateOee(null!, _data);
        var result2 = OeeRegister.CalculateOee(_register, null!);

        // Assert
        result1.IsSuccess.ShouldBeFalse();
        result1.Errors.ShouldContain("Inputs cannot be null");

        result2.IsSuccess.ShouldBeFalse();
        result2.Errors.ShouldContain("Inputs cannot be null");
    }
    /// <summary>
    /// Executes CalculateOee_WithZeroPlannedTime_ShouldUseAggregatedTimeAndWarn operation.
    /// </summary>

    [Fact]
    public void CalculateOee_WithZeroPlannedTime_ShouldUseAggregatedTimeAndWarn()
    {
        // Arrange
        _register.PlanedProductionTime = 0;
        // 420 (run) + 30 (stop) + 30 (fault) = 480
        var expectedPlannedTime = _data.RunningTime + _data.StoppedTime + _data.FaultedTime;

        // Act
        var result = OeeRegister.CalculateOee(_register, _data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.HasWarnings.ShouldBeTrue();
        result.Warnings.ShouldContain("PlanedProductionTime was zero and computed from time aggregates.");
        result.Confidence.ShouldBeGreaterThan(0.0);
        result.Confidence.ShouldBeLessThanOrEqualTo(1.0);
        result.MissingDataRatio.ShouldBeGreaterThanOrEqualTo(0.0);
        result.MissingDataRatio.ShouldBeLessThanOrEqualTo(1.0);
        result.Value.ShouldNotBeNull();
        result.Value.Availability.ShouldBe(0.875d);
    }
    /// <summary>
    /// Executes CalculateOee_WithZeroRunningTime_ShouldClampPerformanceAndWarn operation.
    /// </summary>

    [Fact]
    public void CalculateOee_WithZeroRunningTime_ShouldClampPerformanceAndWarn()
    {
        // Arrange
        _data.RunningTime = 0;

        // Act
        var result = OeeRegister.CalculateOee(_register, _data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.HasWarnings.ShouldBeTrue();
        result.Warnings.ShouldContain("RunningTime was zero and computed from CurrentTime - StoppedTime.");
        result.Confidence.ShouldBeGreaterThan(0.0);
        result.MissingDataRatio.ShouldBeGreaterThanOrEqualTo(0.0);
        result.Value.ShouldNotBeNull();
        result.Value.Performance.ShouldBe(0.8888888888888888d); // Fall back was aplied
        result.Value.Availability.ShouldBe(0.9375d);
    }
    /// <summary>
    /// #126 F1.2 (PO decision) — negative PLC counters/times are impossible readings and now
    /// produce a failure Result instead of the old clamp-and-warn success. The raw sample is no
    /// longer mutated (#126 F1.1).
    /// </summary>
    [Fact]
    public void CalculateOee_WithNegativeValues_ShouldReturnFailure()
    {
        // Arrange
        _data.TotalProduction = -10;
        _data.ProductionOk = -5;
        _data.RunningTime = -100;
        _register.PlanedProductionTime = -480;

        // Act
        var result = OeeRegister.CalculateOee(_register, _data);

        // Assert - impossible inputs are rejected loudly; the raw PLC readings survive untouched.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("TotalProduction was negative"));
        result.Errors.ShouldContain(e => e.Contains("ProductionOk was negative"));
        result.Errors.ShouldContain(e => e.Contains("RunningTime was negative"));
        _data.TotalProduction.ShouldBe(-10);
        _data.ProductionOk.ShouldBe(-5);
        _data.RunningTime.ShouldBe(-100);
    }
    /// <summary>
    /// Executes SafeRatio_WithZeroDenominator_ShouldReturnFallback operation.
    /// </summary>

    [Fact]
    public void SafeRatio_WithZeroDenominator_ShouldReturnFallback()
    {
        // Act
        var result = OeeRegister.SafeRatio(100, 0, 0, 1, 0.5);

        // Assert
        result.ShouldBe(0.5);
    }
    /// <summary>
    /// Executes ClampMetric_ShouldClampValueWithinRange operation.
    /// </summary>

    [Fact]
    public void ClampMetric_ShouldClampValueWithinRange()
    {
        // Act
        var lower = OeeRegister.ClampMetric(-0.5, 0, 1);
        var upper = OeeRegister.ClampMetric(1.5, 0, 1);
        var middle = OeeRegister.ClampMetric(0.5, 0, 1);

        // Assert
        lower.ShouldBe(0);
        upper.ShouldBe(1);
        middle.ShouldBe(0.5);
    }
    /// <summary>
    /// Executes ToKpiOee_ShouldMapAndRoundValuesCorrectly operation.
    /// </summary>

    [Fact]
    public void ToKpiOee_ShouldMapAndRoundValuesCorrectly()
    {
        // Arrange
        _register.Quality = 0.987654321;
        _register.Availability = 0.876543219;
        _register.Performance = 0.765432198;
        _register.Oee = 0.654321987;

        // Act
        var kpi = OeeRegister.ToKpiOee(_register);

        // Assert
        kpi.Quality.Value.ShouldBe(0.987654);
        kpi.Availability.Value.ShouldBe(0.876543);
        kpi.Performance.Value.ShouldBe(0.765432);
        kpi.Oee.Value.ShouldBe(0.654322); // Note rounding up
        kpi.TimeStamp.ShouldBeInRange(DateTime.Now.AddSeconds(-1), DateTime.Now.AddSeconds(1));
    }
}