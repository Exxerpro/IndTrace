// <copyright file="OeeRegisterCriticalTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Models;
using Shouldly;
using Xunit;

namespace IndTrace.Domain.UnitTests.OeeTests;

/// <summary>
/// Critical edge case tests for OeeRegister.CalculateOee method.
/// These tests act as manual mutation testing to catch potential bugs.
/// </summary>
public class OeeRegisterCriticalTests
{
    /// <summary>
    /// Executes CalculateOee_DivisionByZero_FaultedPlusStoppedTime_ShouldNotCrash operation.
    /// </summary>
    [Fact]
    public void CalculateOee_DivisionByZero_FaultedPlusStoppedTime_ShouldNotCrash()
    {
        // Arrange - Test the division by zero scenario at line 73
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            CurrentTime = 0, // This will cause division by zero
            FaultedTime = 50,
            StoppedTime = 60,
            TotalProduction = 100,
            ProductionOk = 80
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // Should not crash and should handle gracefully
    }
    /// <summary>
    /// Executes CalculateOee_NegativeRunningTimeComputation_ShouldClampToZero operation.
    /// </summary>

    [Fact]
    public void CalculateOee_NegativeRunningTimeComputation_ShouldClampToZero()
    {
        // Arrange - Test negative RunningTime computation
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            CurrentTime = 50,
            StoppedTime = 100, // Larger than CurrentTime, should be clamped first
            RunningTime = 0,   // Force fallback computation
            TotalProduction = 100,
            ProductionOk = 80
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.RunningTime.ShouldBe(0);
        // The test should check for the correct warning message based on actual behavior
        result.Warnings.ShouldContain(w => w.Contains("RunningTime was zero and computed") ||
                                          w.Contains("Computed RunningTime was negative"));
    }
    /// <summary>
    /// Executes CalculateOee_ProductionNoKEdgeCase_WhenTotalEqualsOk_ShouldBeZero operation.
    /// </summary>

    /// <summary>
    /// #81 — when the TotalProduction fallback fires (TotalProduction &lt;= 0 -> ProductionOk + ProductionNoK),
    /// the computed total must be synchronized back onto the <see cref="OeeRegister"/> (mirroring the RunningTime
    /// fallback fix), so the persisted register's TotalProduction never diverges from the total the Quality /
    /// Performance metrics were computed from. Before #81 only <c>data</c> was updated; the register kept its
    /// pre-fallback value.
    /// </summary>
    [Fact]
    public void CalculateOee_TotalProductionFallback_SynchronizesRegisterWithData()
    {
        // Arrange - force the TotalProduction <= 0 fallback branch.
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            TotalProduction = 0, // Force fallback -> ProductionOk + ProductionNoK
            ProductionOk = 0,
            ProductionNoK = 0,
            CurrentTime = 100,
            RunningTime = 80,
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert - the fallback fired and the register carries the fallback total (in sync with data), never a
        // stale divergent value.
        result.IsSuccess.ShouldBeTrue();
        var updated = result.Value.ShouldNotBeNull();
        result.Warnings.ShouldContain(w => w.Contains("TotalProduction was zero and computed"));
        updated.TotalProduction.ShouldBe(data.ProductionOk + data.ProductionNoK);
        updated.TotalProduction.ShouldBe(data.TotalProduction);
    }

    /// <summary>
    /// #126 F1.2 (PO decision) — OK + NOK counts exceeding a positive reported total are an
    /// inconsistency error (previously silently clamped to a success). The raw sample survives.
    /// </summary>
    [Fact]
    public void CalculateOee_ProductionNoKEdgeCase_WhenCountsExceedTotal_ShouldReturnFailure()
    {
        // Arrange - 100 OK + 50 NOK cannot come out of 100 produced parts.
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            TotalProduction = 100,
            ProductionOk = 100,   // Equal to total
            ProductionNoK = 50,   // Contradicts the reported total
            CurrentTime = 100,
            RunningTime = 80
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("inconsistent"));
        data.ProductionNoK.ShouldBe(50); // raw PLC reading untouched (#126 F1.1)
    }
    /// <summary>
    /// Executes CalculateOee_AllZeroInputs_ShouldUseFallbacksWithoutCrashing operation.
    /// </summary>

    [Fact]
    public void CalculateOee_AllZeroInputs_ShouldUseFallbacksWithoutCrashing()
    {
        // Arrange - Test all zero inputs to trigger all fallback logic
        var register = new OeeRegister
        {
            PlanedProductionTime = 0,
            StandardCycleTime = 0,
            ActualCycleTime = 0
        };
        var data = new PerformanceData
        {
            TotalProduction = 0,
            ProductionOk = 0,
            ProductionNoK = 0,
            CurrentTime = 0,
            RunningTime = 0,
            StoppedTime = 0,
            FaultedTime = 0
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Warnings.ShouldNotBeEmpty();
        // Should not crash and should have reasonable fallback values
        result.Value.ShouldNotBeNull();
        result.Value.Oee.ShouldBeGreaterThanOrEqualTo(0);
        result.Value.Oee.ShouldBeLessThanOrEqualTo(1);
    }
    /// <summary>
    /// Executes CalculateOee_ExtremeDoubleValues_ShouldHandleGracefully operation.
    /// </summary>
    /// <param name="extremeValue">The extremeValue.</param>

    [Theory]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.NaN)]
    public void CalculateOee_ExtremeDoubleValues_ShouldHandleGracefully(double extremeValue)
    {
        // Arrange - Test extreme double values
        var register = new OeeRegister
        {
            PlanedProductionTime = extremeValue,
            StandardCycleTime = 1,
            ActualCycleTime = 1
        };
        var data = new PerformanceData
        {
            TotalProduction = extremeValue,
            ProductionOk = 80,
            CurrentTime = 100,
            RunningTime = 80
        };

        // Act & Assert - Should not crash. #126 F1.2: negative extremes are impossible PLC
        // readings and now surface as a failure Result instead of a clamped success.
        var result = OeeRegister.CalculateOee(register, data);
        result.ShouldNotBeNull();
        if (extremeValue < 0)
        {
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain(e => e.Contains("TotalProduction was negative"));
        }
        else
        {
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
        }
    }
    /// <summary>
    /// Executes CalculateOee_SafeRatio_ZeroDenominatorInQuality_ShouldUseFallback operation.
    /// </summary>

    [Fact]
    public void CalculateOee_SafeRatio_ZeroDenominatorInQuality_ShouldUseFallback()
    {
        // Arrange - Test SafeRatio with zero denominator in Quality calculation
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            TotalProduction = 0, // Zero denominator for Quality
            ProductionOk = 50,   // Non-zero numerator
            CurrentTime = 100,
            RunningTime = 80
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Quality.ShouldBe(1.0); // Should use fallback value
    }
    /// <summary>
    /// Executes CalculateOee_SafeRatio_ZeroDenominatorInAvailability_ShouldUseFallbackLogic operation.
    /// </summary>

    [Fact]
    public void CalculateOee_SafeRatio_ZeroDenominatorInAvailability_ShouldUseFallbackLogic()
    {
        // Arrange - Test behavior when PlanedProductionTime is zero
        var register = new OeeRegister { PlanedProductionTime = 0 }; // Will trigger fallback calculation
        var data = new PerformanceData
        {
            TotalProduction = 100,
            ProductionOk = 80,
            CurrentTime = 100,
            RunningTime = 80,
            StoppedTime = 10,
            FaultedTime = 10
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // PlanedProductionTime should be recalculated as RunningTime + StoppedTime + FaultedTime = 100
        result.Value.PlanedProductionTime.ShouldBe(100);
        result.Value.Availability.ShouldBe(0.8); // 80/100
        result.Warnings.ShouldContain(w => w.Contains("PlanedProductionTime was zero and computed"));
    }
    /// <summary>
    /// Executes CalculateOee_SafeRatio_ZeroDenominatorInPerformance_ShouldUseFallbackLogic operation.
    /// </summary>

    [Fact]
    public void CalculateOee_SafeRatio_ZeroDenominatorInPerformance_ShouldUseFallbackLogic()
    {
        // Arrange - Test behavior when RunningTime is zero
        var register = new OeeRegister
        {
            PlanedProductionTime = 100,
            StandardCycleTime = 10
        };
        var data = new PerformanceData
        {
            TotalProduction = 100,
            ProductionOk = 80,
            CurrentTime = 100,
            RunningTime = 0, // Will trigger fallback calculation
            StoppedTime = 20
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        result.Warnings.ShouldContain(w => w.Contains("RunningTime was zero and computed"));
        // The actual behavior depends on the fallback logic implementation
        // If RunningTime becomes negative (CurrentTime - StoppedTime < 0), it gets clamped to 0
        if (result.Value.RunningTime == 0)
        {
            // This means the computed value was negative and got clamped
            result.Warnings.ShouldContain(w => w.Contains("Computed RunningTime was negative"));
        }
        else
        {
            // This means the computed value was positive (CurrentTime - StoppedTime = 80)
            result.Value.RunningTime.ShouldBe(80);
        }
    }
    /// <summary>
    /// Executes CalculateOee_IntegerOverflow_InScaling_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void CalculateOee_IntegerOverflow_InScaling_ShouldHandleGracefully()
    {
        // Arrange - Test potential integer overflow in scaling calculation
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            CurrentTime = 1,
            FaultedTime = int.MaxValue / 2,
            StoppedTime = int.MaxValue / 2,
            TotalProduction = 100,
            ProductionOk = 80
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Should not crash and values should be reasonable
        result.Value.FaultedTime.ShouldBeGreaterThanOrEqualTo(0);
        result.Value.StoppedTime.ShouldBeGreaterThanOrEqualTo(0);
    }
    /// <summary>
    /// Executes CalculateOee_ClampMetric_ExtremeOeeValue_ShouldClampTo01Range operation.
    /// </summary>

    [Fact]
    public void CalculateOee_ClampMetric_ExtremeOeeValue_ShouldClampTo01Range()
    {
        // Arrange - Test ClampMetric with extreme OEE calculation
        var register = new OeeRegister
        {
            PlanedProductionTime = 1,
            StandardCycleTime = 1000000 // Large value to force high Performance
        };
        var data = new PerformanceData
        {
            TotalProduction = 1000000,
            ProductionOk = 1000000,
            CurrentTime = 100,
            RunningTime = 1
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        result.Value.Oee.ShouldBeGreaterThanOrEqualTo(0.0);
        result.Value.Oee.ShouldBeLessThanOrEqualTo(1.0);
    }
    /// <summary>
    /// Executes CalculateOee_NullInputs_ShouldReturnFailure operation.
    /// </summary>

    [Fact]
    public void CalculateOee_NullInputs_ShouldReturnFailure()
    {
        // Arrange & Act & Assert
        OeeRegister.CalculateOee(null!, new PerformanceData()).IsSuccess.ShouldBeFalse();
        OeeRegister.CalculateOee(new OeeRegister(), null!).IsSuccess.ShouldBeFalse();
        OeeRegister.CalculateOee(null!, null!).IsSuccess.ShouldBeFalse();
    }
    /// <summary>
    /// Executes CalculateOee_PerformanceClampingUpperBound_ShouldNotExceed15 operation.
    /// </summary>

    [Fact]
    public void CalculateOee_PerformanceClampingUpperBound_ShouldNotExceed15()
    {
        // Arrange - Test Performance upper bound clamping (max 1.5)
        var register = new OeeRegister
        {
            PlanedProductionTime = 100,
            StandardCycleTime = 1000 // Very high to force high performance
        };
        var data = new PerformanceData
        {
            TotalProduction = 1000,
            ProductionOk = 1000,
            CurrentTime = 100,
            RunningTime = 1 // Small value to maximize performance ratio
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Performance.ShouldBeLessThanOrEqualTo(1.5);
    }
}