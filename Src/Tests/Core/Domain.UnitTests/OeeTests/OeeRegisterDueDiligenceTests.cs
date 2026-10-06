// <copyright file="OeeRegisterDueDiligenceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Models;
using Shouldly;
using Xunit;

namespace IndTrace.Domain.UnitTests.OeeTests;

/// <summary>
/// Due diligence tests for critical bugs found in OeeRegister.CalculateOee method.
/// These tests target specific bugs discovered during systematic code review.
/// </summary>
public class OeeRegisterDueDiligenceTests
{
    /// <summary>
    /// Executes CalculateOee_DivisionByZero_FaultedPlusStoppedZero_ShouldNotCrash operation.
    /// </summary>
    [Fact]
    public void CalculateOee_DivisionByZero_FaultedPlusStoppedZero_ShouldNotCrash()
    {
        // Arrange - CRITICAL BUG: Division by zero when FaultedTime + StoppedTime = 0
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            CurrentTime = 100,
            FaultedTime = 0,    // Combined sum = 0
            StoppedTime = 0,    // Will cause division by zero!
            TotalProduction = 100,
            ProductionOk = 80,
            RunningTime = 80
        };

        // Act & Assert - Should not crash with DivideByZeroException
        var result = Should.NotThrow(() => OeeRegister.CalculateOee(register, data));
        result.IsSuccess.ShouldBeTrue();
    }
    /// <summary>
    /// Executes CalculateOee_DataRegisterRunningTimeMismatch_ShouldBeConsistent operation.
    /// </summary>

    [Fact]
    public void CalculateOee_DataRegisterRunningTimeMismatch_ShouldBeConsistent()
    {
        // Arrange - CRITICAL BUG: data.RunningTime updated but register.RunningTime not synchronized
        var register = new OeeRegister
        {
            PlanedProductionTime = 100,
            StandardCycleTime = 1
        };
        var data = new PerformanceData
        {
            CurrentTime = 100,
            StoppedTime = 20,
            RunningTime = 0,    // Will trigger fallback calculation
            TotalProduction = 100,
            ProductionOk = 80
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert - the register carries the computed fallback (100 - 20 = 80) while the raw PLC
        // sample keeps its original zero (#126 F1.1: CalculateOee never mutates its data input).
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.RunningTime.ShouldBe(80);
        data.RunningTime.ShouldBe(0);
    }
    /// <summary>
    /// Executes CalculateOee_RedundantNegativeCheck_ShouldNeverTrigger operation.
    /// </summary>

    [Fact]
    public void CalculateOee_RedundantNegativeCheck_ShouldNeverTrigger()
    {
        // Arrange - CRITICAL BUG: Lines 64-67 should never execute if clamping works
        var register = new OeeRegister { PlanedProductionTime = -100 }; // Negative
        var data = new PerformanceData
        {
            CurrentTime = 100,
            TotalProduction = 100,
            ProductionOk = 80,
            RunningTime = 80
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert - Should not have duplicate warning about negative PlanedProductionTime
        result.IsSuccess.ShouldBeTrue();
        var negativeWarnings = result.Warnings.Count(w => w.Contains("PlannedProductionTime") && w.Contains("negative"));
        negativeWarnings.ShouldBeLessThanOrEqualTo(1, "Should not have duplicate negative warnings");
    }
    /// <summary>
    /// Executes CalculateOee_FragileStringMatching_ShouldBeRobust operation.
    /// </summary>

    [Fact]
    public void CalculateOee_FragileStringMatching_ShouldBeRobust()
    {
        // Arrange - CRITICAL BUG: String matching for warnings is fragile
        var register = new OeeRegister { PlanedProductionTime = 0 }; // Zero, not negative
        var data = new PerformanceData
        {
            CurrentTime = 100,
            RunningTime = 80,
            StoppedTime = 10,
            FaultedTime = 10,
            TotalProduction = 100,
            ProductionOk = 80
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert - Fallback should work for zero value (not negative)
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PlanedProductionTime.ShouldBe(100); // Should be computed as 80+10+10
        result.Warnings.ShouldContain(w => w.Contains("PlanedProductionTime was zero and computed"));
    }
    /// <summary>
    /// Executes CalculateOee_NegativeRunningTimeComputation_StateMismatch operation.
    /// </summary>

    [Fact]
    public void CalculateOee_NegativeRunningTimeComputation_StateMismatch()
    {
        // Arrange - Test case where scaling prevents negative computation
        var register = new OeeRegister
        {
            PlanedProductionTime = 100,
            StandardCycleTime = 1
        };
        var data = new PerformanceData
        {
            CurrentTime = 50,
            StoppedTime = 80,   // Will be scaled down to fit within CurrentTime
            RunningTime = 0,    // Will trigger computation after scaling
            TotalProduction = 100,
            ProductionOk = 80
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.RunningTime.ShouldBe(0);

        // Since StoppedTime gets scaled down first, the computed RunningTime is 0, not negative
        result.Warnings.ShouldContain(w => w.Contains("RunningTime was zero and computed"));
        result.Warnings.ShouldContain(w => w.Contains("FaultedTime + StoppedTime exceeded CurrentTime and were proportionally scaled"));

        // The performance calculation should use the corrected RunningTime value  
        result.Value.Performance.ShouldNotBe(double.NaN);
        result.Value.Performance.ShouldNotBe(double.PositiveInfinity);
    }
    /// <summary>
    /// Executes CalculateOee_IntegerOverflowInScaling_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void CalculateOee_IntegerOverflowInScaling_ShouldHandleGracefully()
    {
        // Arrange - CRITICAL BUG: Potential integer overflow in scaling calculation
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            CurrentTime = 1,
            FaultedTime = int.MaxValue,
            StoppedTime = int.MaxValue,
            TotalProduction = 100,
            ProductionOk = 80,
            RunningTime = 50
        };

        // Act & Assert - Should not crash with overflow
        var result = Should.NotThrow(() => OeeRegister.CalculateOee(register, data));
        result.IsSuccess.ShouldBeTrue();
        var resultValue = result.Value;
        resultValue.ShouldNotBeNull();

        // Values should be reasonable after scaling - both MaxValue inputs should be scaled down
        resultValue.FaultedTime.ShouldBeGreaterThanOrEqualTo(0);
        resultValue.StoppedTime.ShouldBeGreaterThanOrEqualTo(0);
        resultValue.FaultedTime.ShouldBeLessThanOrEqualTo(data.CurrentTime);
        resultValue.StoppedTime.ShouldBeLessThanOrEqualTo(data.CurrentTime);

        // The values should have been scaled down from int.MaxValue
        resultValue.FaultedTime.ShouldBeLessThan(int.MaxValue);
        resultValue.StoppedTime.ShouldBeLessThan(int.MaxValue);
    }
    /// <summary>
    /// Executes CalculateOee_PerformanceNumeratorOverflow_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void CalculateOee_PerformanceNumeratorOverflow_ShouldHandleGracefully()
    {
        // Arrange - CRITICAL BUG: Potential overflow in performance calculation
        var register = new OeeRegister
        {
            PlanedProductionTime = 100,
            StandardCycleTime = double.MaxValue
        };
        var data = new PerformanceData
        {
            CurrentTime = 100,
            RunningTime = 1,
            TotalProduction = double.MaxValue,
            ProductionOk = 100
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert - Should handle overflow gracefully
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Performance.ShouldNotBe(double.NaN);
        result.Value.Performance.ShouldBeLessThanOrEqualTo(1.5); // Should be clamped
        result.Value.Oee.ShouldNotBe(double.NaN);
        result.Value.Oee.ShouldNotBe(double.PositiveInfinity);
    }
    /// <summary>
    /// Executes CalculateOee_EdgeCasesInScaling_ShouldNotCrash operation.
    /// </summary>
    /// <param name="faultedTime">The faultedTime.</param>
    /// <param name="stoppedTime">The stoppedTime.</param>
    /// <param name="currentTime">The currentTime.</param>

    [Theory]
    [InlineData(0, 100, 100)] // FaultedTime = 0, StoppedTime = 100, CurrentTime = 100
    [InlineData(50, 0, 100)]  // FaultedTime = 50, StoppedTime = 0, CurrentTime = 100  
    [InlineData(0, 0, 100)]   // FaultedTime = 0, StoppedTime = 0, CurrentTime = 100 (CRITICAL)
    [InlineData(0, 0, 0)]     // FaultedTime = 0, StoppedTime = 0, CurrentTime = 0 (CRITICAL)
    public void CalculateOee_EdgeCasesInScaling_ShouldNotCrash(int faultedTime, int stoppedTime, int currentTime)
    {
        // Arrange - Test edge cases in scaling calculation
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            CurrentTime = currentTime,
            FaultedTime = faultedTime,
            StoppedTime = stoppedTime,
            TotalProduction = 100,
            ProductionOk = 80,
            RunningTime = 50
        };

        // Act & Assert - Should never crash
        var result = Should.NotThrow(() => OeeRegister.CalculateOee(register, data));
        result.IsSuccess.ShouldBeTrue();
    }
}