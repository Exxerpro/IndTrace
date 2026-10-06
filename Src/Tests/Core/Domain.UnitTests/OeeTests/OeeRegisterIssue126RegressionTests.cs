// <copyright file="OeeRegisterIssue126RegressionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Shouldly;
using Xunit;

namespace IndTrace.Domain.UnitTests.OeeTests;

/// <summary>
/// Regression tests for issue #126 F1 — <see cref="OeeRegister.CalculateOee"/> defects:
/// input mutation of the raw PLC <see cref="PerformanceData"/>, the dead <c>errors</c> list
/// (impossible inputs silently clamped to a success), warning-text-sniffing control flow, and the
/// 100%-Performance fallback when no running time was observed.
/// </summary>
public class OeeRegisterIssue126RegressionTests
{
    /// <summary>
    /// #126 F1.1 — the RunningTime fallback must be computed on a local copy: the raw PLC sample
    /// (<c>data</c>) is persisted downstream and must never be rewritten by the calculation.
    /// </summary>
    [Fact]
    public void CalculateOee_RunningTimeFallback_MustNotMutatePerformanceData()
    {
        // Arrange - RunningTime = 0 triggers the CurrentTime - StoppedTime fallback (computes 80).
        var register = new OeeRegister { PlanedProductionTime = 100, StandardCycleTime = 1 };
        var data = new PerformanceData
        {
            TotalProduction = 100,
            ProductionOk = 80,
            CurrentTime = 100,
            RunningTime = 0,
            StoppedTime = 20,
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert - the register carries the sanitized/computed value; the raw sample survives untouched.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.RunningTime.ShouldBe(80);
        data.RunningTime.ShouldBe(0);
    }

    /// <summary>
    /// #126 F1.1 — the FaultedTime/StoppedTime proportional scale-down must not rewrite the raw
    /// PLC readings; only the calculated register may carry the scaled values.
    /// </summary>
    [Fact]
    public void CalculateOee_ScaleDown_MustNotMutatePerformanceData()
    {
        // Arrange - Faulted + Stopped (160) exceeds CurrentTime (100) -> proportional scaling to 50/50.
        var register = new OeeRegister { PlanedProductionTime = 100, StandardCycleTime = 1 };
        var data = new PerformanceData
        {
            TotalProduction = 100,
            ProductionOk = 80,
            CurrentTime = 100,
            RunningTime = 50,
            StoppedTime = 80,
            FaultedTime = 80,
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.FaultedTime.ShouldBe(50);
        result.Value.StoppedTime.ShouldBe(50);
        data.FaultedTime.ShouldBe(80);
        data.StoppedTime.ShouldBe(80);
    }

    /// <summary>
    /// #126 F1.2 (PO decision) — negative production counters and negative time components are
    /// impossible PLC readings; they must produce a failure Result, not a clamped success.
    /// </summary>
    /// <param name="fieldName">The PerformanceData field driven negative.</param>
    [Theory]
    [InlineData(nameof(PerformanceData.TotalProduction))]
    [InlineData(nameof(PerformanceData.ProductionOk))]
    [InlineData(nameof(PerformanceData.ProductionNoK))]
    [InlineData(nameof(PerformanceData.CurrentTime))]
    [InlineData(nameof(PerformanceData.RunningTime))]
    [InlineData(nameof(PerformanceData.StoppedTime))]
    [InlineData(nameof(PerformanceData.FaultedTime))]
    public void CalculateOee_NegativeInput_ShouldReturnFailure(string fieldName)
    {
        // Arrange - a healthy sample with exactly one impossible (negative) reading.
        var register = new OeeRegister { PlanedProductionTime = 100, StandardCycleTime = 1 };
        var data = new PerformanceData
        {
            TotalProduction = 100,
            ProductionOk = 80,
            ProductionNoK = 20,
            CurrentTime = 100,
            RunningTime = 80,
            StoppedTime = 10,
            FaultedTime = 10,
        };

        switch (fieldName)
        {
            case nameof(PerformanceData.TotalProduction):
                data.TotalProduction = -1;
                break;
            case nameof(PerformanceData.ProductionOk):
                data.ProductionOk = -1;
                break;
            case nameof(PerformanceData.ProductionNoK):
                data.ProductionNoK = -1;
                break;
            case nameof(PerformanceData.CurrentTime):
                data.CurrentTime = -1;
                break;
            case nameof(PerformanceData.RunningTime):
                data.RunningTime = -1;
                break;
            case nameof(PerformanceData.StoppedTime):
                data.StoppedTime = -1;
                break;
            case nameof(PerformanceData.FaultedTime):
                data.FaultedTime = -1;
                break;
        }

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert - impossible input -> failure carrying an explanatory error.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("negative"));
    }

    /// <summary>
    /// #126 F1.2 (PO decision) — a reported total contradicted by its own OK/NOK counts
    /// (OK + NOK &gt; TotalProduction with a positive total) is an inconsistency error, not a clamp.
    /// </summary>
    [Fact]
    public void CalculateOee_CountersExceedingReportedTotal_ShouldReturnFailure()
    {
        // Arrange - 100 OK + 50 NOK cannot come out of 100 produced parts.
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            TotalProduction = 100,
            ProductionOk = 100,
            ProductionNoK = 50,
            CurrentTime = 100,
            RunningTime = 80,
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("inconsistent"));
    }

    /// <summary>
    /// #126 F1.2 — a zero TotalProduction is a tolerated missing-total case, NOT an error; the
    /// pre-#126 warning behavior must be preserved verbatim. Precisely (#126 review, LOW 6): the
    /// counts are clamped against the zero total BEFORE the TotalProduction fallback runs, so the
    /// fallback always re-derives 0 — the "computed from OK + NOK counts" warning text is historical
    /// and the total is never actually resurrected from the counts.
    /// </summary>
    [Fact]
    public void CalculateOee_ZeroTotalWithCounts_StaysToleratedWarning()
    {
        // Arrange - total not reported; counts present (they get clamped against the zero total, as before).
        var register = new OeeRegister { PlanedProductionTime = 100 };
        var data = new PerformanceData
        {
            TotalProduction = 0,
            ProductionOk = 50,
            CurrentTime = 100,
            RunningTime = 80,
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert - success with the historical warnings, never a failure.
        result.IsSuccess.ShouldBeTrue();
        result.Warnings.ShouldContain("ProductionOk was outside valid range and was clamped.");
        result.Warnings.ShouldContain("TotalProduction was zero and computed from OK + NOK counts.");
    }

    /// <summary>
    /// #126 F1.4 (PO decision) — when no running time was observed (zero even after the
    /// CurrentTime - StoppedTime fallback), Performance must be reported as 0 with a warning,
    /// never as the flattering 100% fallback.
    /// </summary>
    [Fact]
    public void CalculateOee_NoObservableRunningTime_PerformanceIsZeroWithWarning()
    {
        // Arrange - CurrentTime 0 means the fallback also computes RunningTime = 0.
        var register = new OeeRegister { PlanedProductionTime = 100, StandardCycleTime = 1 };
        var data = new PerformanceData
        {
            TotalProduction = 100,
            ProductionOk = 80,
            CurrentTime = 0,
            RunningTime = 0,
            StoppedTime = 0,
            FaultedTime = 0,
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Performance.ShouldBe(0.0);
        result.Warnings.ShouldContain(w => w.Contains("Performance reported as 0"));
    }

    /// <summary>
    /// #126 F1.3 — pins the negative-PlannedProductionTime control flow (previously steered by
    /// sniffing warning message text): a negative planned time is clamped to zero and the
    /// time-aggregate fallback must NOT fire for it.
    /// </summary>
    [Fact]
    public void CalculateOee_NegativePlannedTime_ClampsWithoutAggregateFallback()
    {
        // Arrange
        var register = new OeeRegister { PlanedProductionTime = -480, StandardCycleTime = 1 };
        var data = new PerformanceData
        {
            TotalProduction = 100,
            ProductionOk = 80,
            CurrentTime = 100,
            RunningTime = 80,
            StoppedTime = 10,
            FaultedTime = 10,
        };

        // Act
        var result = OeeRegister.CalculateOee(register, data);

        // Assert - clamped to zero, aggregate fallback suppressed, availability uses its 0.0 fallback.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Warnings.ShouldContain("PlannedProductionTime was negative and clamped to zero.");
        result.Warnings.ShouldNotContain("PlanedProductionTime was zero and computed from time aggregates.");
        result.Value.PlanedProductionTime.ShouldBe(0);
        result.Value.Availability.ShouldBe(0.0);
    }
}
