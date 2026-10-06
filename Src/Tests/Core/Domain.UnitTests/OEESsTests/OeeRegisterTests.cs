// <copyright file="OeeRegisterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.OEESsTests;

/// <summary>
/// Unit tests for OeeRegister
/// </summary>
public class OeeRegisterTests
{
    /// <summary>
    /// Executes OeeRegister_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void OeeRegister_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new OeeRegister();

        // Assert
        instance.ShouldNotBeNull();
        instance.OeeRegisterId.ShouldBe(default(int));
        instance.MachineId.ShouldBe(default(int));
        instance.PlcId.ShouldBe(default(int));
        instance.TimeStamp.ShouldBe(default(DateTime));
        instance.ApplicationFlag.ShouldBe(default(int));
        instance.EventCounter.ShouldBe(default(int));
        instance.CurrentTime.ShouldBe(default(int));
        instance.RunningTime.ShouldBe(default(int));
        instance.StoppedTime.ShouldBe(default(int));
        instance.FaultedTime.ShouldBe(default(int));
        instance.StatusFaultReason.ShouldBe(default(int));
        instance.ProductId.ShouldBe(default(int));
        instance.TotalProduction.ShouldBe(default(double));
        instance.StandardCycleTime.ShouldBe(default(double));
        instance.ActualCycleTime.ShouldBe(default(double));
        instance.PlanedProductionTime.ShouldBe(default(double));
        instance.RejectEventCounter.ShouldBe(default(int));
        instance.StatusReject.ShouldBe(default(int));
        instance.RejectQuantityUnits.ShouldBe(default(double));
        instance.ProductionOk.ShouldBe(default(double));
        instance.ProductionNoK.ShouldBe(default(double));
        instance.KpiOee.ShouldBeNull();
        instance.Oee.ShouldBe(default(double));
        instance.Availability.ShouldBe(default(double));
        instance.Performance.ShouldBe(default(double));
        instance.Quality.ShouldBe(default(double));
    }
    /// <summary>
    /// Executes OeeRegister_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void OeeRegister_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange
        var validRegister = new OeeRegister();

        // Act - Testing edge case scenarios for OEE register constraints
        validRegister.TotalProduction = -100.0; // Negative production should be handled
        validRegister.ProductionOk = -50.0; // Negative OK production
        validRegister.ProductionNoK = -25.0; // Negative NOK production
        validRegister.StandardCycleTime = -10.0; // Negative cycle time
        validRegister.ActualCycleTime = -15.0; // Negative actual cycle time
        validRegister.RunningTime = -3600; // Negative running time
        validRegister.StoppedTime = -1800; // Negative stopped time
        validRegister.FaultedTime = -900; // Negative faulted time

        // Assert - OeeRegister should gracefully handle negative values
        validRegister.ShouldNotBeNull();
        validRegister.TotalProduction.ShouldBe(-100.0);
        validRegister.ProductionOk.ShouldBe(-50.0);
        validRegister.ProductionNoK.ShouldBe(-25.0);
        validRegister.StandardCycleTime.ShouldBe(-10.0);
        validRegister.ActualCycleTime.ShouldBe(-15.0);
        validRegister.RunningTime.ShouldBe(-3600);
        validRegister.StoppedTime.ShouldBe(-1800);
        validRegister.FaultedTime.ShouldBe(-900);
    }
    /// <summary>
    /// Executes OeeRegister_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void OeeRegister_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var instance = new OeeRegister();
        int oeeRegisterId = 1;
        int machineId = 2;
        int plcId = 3;
        DateTime timeStamp = DateTime.Now;
        int applicationFlag = 4;
        int eventCounter = 5;
        int currentTime = 6;
        int runningTime = 7;
        int stoppedTime = 8;
        int faultedTime = 9;
        int statusFaultReason = 10;

        // Act
        instance.OeeRegisterId = oeeRegisterId;
        instance.MachineId = machineId;
        instance.PlcId = plcId;
        instance.TimeStamp = timeStamp;
        instance.ApplicationFlag = applicationFlag;
        instance.EventCounter = eventCounter;
        instance.CurrentTime = currentTime;
        instance.RunningTime = runningTime;
        instance.StoppedTime = stoppedTime;
        instance.FaultedTime = faultedTime;
        instance.StatusFaultReason = statusFaultReason;

        // Assert
        instance.OeeRegisterId.ShouldBe(oeeRegisterId);
        instance.MachineId.ShouldBe(machineId);
        instance.PlcId.ShouldBe(plcId);
        instance.TimeStamp.ShouldBe(timeStamp);
        instance.ApplicationFlag.ShouldBe(applicationFlag);
        instance.EventCounter.ShouldBe(eventCounter);
        instance.CurrentTime.ShouldBe(currentTime);
        instance.RunningTime.ShouldBe(runningTime);
        instance.StoppedTime.ShouldBe(stoppedTime);
        instance.FaultedTime.ShouldBe(faultedTime);
        instance.StatusFaultReason.ShouldBe(statusFaultReason);
    }
    /// <summary>
    /// Executes OeeRegister_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void OeeRegister_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var register = new OeeRegister
        {
            Oee = 0.85,
            Availability = 0.90,
            Performance = 0.95,
            Quality = 0.99
        };

        // Act - Test ToKpiOee static method
        var kpiOee = OeeRegister.ToKpiOee(register);

        // Assert
        kpiOee.ShouldNotBeNull();
        kpiOee.Quality.Value.ShouldBe(0.99, 0.000001);
        kpiOee.Availability.Value.ShouldBe(0.90, 0.000001);
        kpiOee.Performance.Value.ShouldBe(0.95, 0.000001);
        kpiOee.Oee.Value.ShouldBe(0.85, 0.000001);
        kpiOee.TimeStamp.ShouldBeGreaterThan(DateTime.Now.AddMinutes(-1));

        // Test static helper methods
        var clampedValue = OeeRegister.ClampMetric(1.5, 0.0, 1.0);
        clampedValue.ShouldBe(1.0);

        var clampedLow = OeeRegister.ClampMetric(-0.5, 0.0, 1.0);
        clampedLow.ShouldBe(0.0);

        var safeRatio = OeeRegister.SafeRatio(80.0, 100.0, 0.0, 1.0, 0.5);
        safeRatio.ShouldBe(0.8);

        var safeRatioZeroDenominator = OeeRegister.SafeRatio(80.0, 0.0, 0.0, 1.0, 0.5);
        safeRatioZeroDenominator.ShouldBe(0.5); // Fallback value
    }
    /// <summary>
    /// Executes OeeRegister_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void OeeRegister_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Automotive manufacturing scenario
        var register = new OeeRegister
        {
            OeeRegisterId = 1001,
            MachineId = 2001,
            PlcId = 3001,
            ProductId = 4001,
            StandardCycleTime = 60.0, // 1 minute standard cycle
            ActualCycleTime = 58.0, // 58 seconds actual (good performance)
            PlanedProductionTime = 28800.0 // 8 hours planned
        };

        var performanceData = new PerformanceData
        {
            TotalProduction = 480.0, // Expected production for 8 hours
            ProductionOk = 456.0, // 95% quality
            ProductionNoK = 24.0, // 5% defects
            CurrentTime = 28800, // 8 hours
            RunningTime = 25200, // 7 hours running
            StoppedTime = 2400, // 40 minutes stops
            FaultedTime = 1200, // 20 minutes faults
            ApplicationFlag = 1,
            EventCounter = 240,
            StatusFaultReason = 0
        };

        // Act
        var result = OeeRegister.CalculateOee(register, performanceData);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        var calculatedRegister = result.Value;
        calculatedRegister.ShouldNotBeNull();
        calculatedRegister.ShouldNotBeNull();

        // Verify calculated OEE components
        calculatedRegister.Quality.ShouldBe(0.95, 0.01); // 456/480 = 95%
        calculatedRegister.Availability.ShouldBe(0.875, 0.01); // 25200/28800 = 87.5%
        calculatedRegister.Performance.ShouldBeGreaterThan(0.8); // Should be good due to faster cycle time

        // Overall OEE should be reasonable for automotive
        calculatedRegister.Oee.ShouldBeGreaterThan(0.70);
        calculatedRegister.Oee.ShouldBeLessThanOrEqualTo(1.0);

        // Verify data transfer
        calculatedRegister.TotalProduction.ShouldBe(480.0);
        calculatedRegister.CurrentTime.ShouldBe(28800);
        calculatedRegister.RunningTime.ShouldBe(25200);
        calculatedRegister.ApplicationFlag.ShouldBe(1);
        calculatedRegister.EventCounter.ShouldBe(240);
    }
    /// <summary>
    /// Executes CalculateOee_WithNullInputs_ShouldReturnFailure operation.
    /// </summary>

    [Fact]
    public void CalculateOee_WithNullInputs_ShouldReturnFailure()
    {
        // Arrange
        var validRegister = new OeeRegister();
        var validData = new PerformanceData();

        // Act & Assert - Null register
        var result1 = OeeRegister.CalculateOee(null!, validData);
        result1.IsSuccess.ShouldBeFalse();
        result1.Errors.ShouldContain("Inputs cannot be null");

        // Act & Assert - Null data
        var result2 = OeeRegister.CalculateOee(validRegister, null!);
        result2.IsSuccess.ShouldBeFalse();
        result2.Errors.ShouldContain("Inputs cannot be null");

        // Act & Assert - Both null
        var result3 = OeeRegister.CalculateOee(null!, null!);
        result3.IsSuccess.ShouldBeFalse();
        result3.Errors.ShouldContain("Inputs cannot be null");
    }
    /// <summary>
    /// #126 F1.2 — negative (impossible) PLC data must fail loudly instead of being clamped.
    /// </summary>
    [Fact]
    public void CalculateOee_WithInvalidData_ShouldReturnFailure()
    {
        // Arrange - Data with negative and invalid values
        var register = new OeeRegister
        {
            StandardCycleTime = -10.0, // Invalid
            ActualCycleTime = -5.0, // Invalid
            PlanedProductionTime = -100.0 // Invalid
        };

        var performanceData = new PerformanceData
        {
            TotalProduction = -50.0, // Invalid
            ProductionOk = -20.0, // Invalid
            ProductionNoK = -10.0, // Invalid
            CurrentTime = -3600, // Invalid
            RunningTime = -1800, // Invalid
            StoppedTime = -900, // Invalid
            FaultedTime = -450 // Invalid
        };

        // Act
        var result = OeeRegister.CalculateOee(register, performanceData);

        // Assert - #126 F1.2 (PO decision): negative PLC counters/times are impossible readings
        // and now produce a failure Result instead of the old clamp-and-warn success.
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.Contains("TotalProduction was negative"));
        result.Errors.ShouldContain(e => e.Contains("CurrentTime was negative"));
        result.Errors.ShouldContain(e => e.Contains("RunningTime was negative"));
        result.Errors.ShouldContain(e => e.Contains("StoppedTime was negative"));
        result.Errors.ShouldContain(e => e.Contains("FaultedTime was negative"));

        // #126 F1.1 - the raw PLC sample is never mutated by the calculation.
        performanceData.TotalProduction.ShouldBe(-50.0);
        performanceData.ProductionOk.ShouldBe(-20.0);
        performanceData.ProductionNoK.ShouldBe(-10.0);
        performanceData.CurrentTime.ShouldBe(-3600);
        performanceData.RunningTime.ShouldBe(-1800);
        performanceData.StoppedTime.ShouldBe(-900);
        performanceData.FaultedTime.ShouldBe(-450);
    }
    /// <summary>
    /// Executes CalculateOee_WithPerfectScenario_ShouldReturn100PercentOee operation.
    /// </summary>

    [Fact]
    public void CalculateOee_WithPerfectScenario_ShouldReturn100PercentOee()
    {
        // Arrange - Perfect production scenario
        var register = new OeeRegister
        {
            StandardCycleTime = 60.0,
            ActualCycleTime = 60.0, // Same as standard
            PlanedProductionTime = 28800.0 // 8 hours
        };

        var performanceData = new PerformanceData
        {
            TotalProduction = 480.0, // Perfect production
            ProductionOk = 480.0, // 100% quality
            ProductionNoK = 0.0, // No defects
            CurrentTime = 28800, // 8 hours
            RunningTime = 28800, // 100% availability
            StoppedTime = 0, // No stops
            FaultedTime = 0 // No faults
        };

        // Act
        var result = OeeRegister.CalculateOee(register, performanceData);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        var calculatedRegister = result.Value;
        calculatedRegister.ShouldNotBeNull();
        calculatedRegister.Quality.ShouldBe(1.0, 0.001); // 100% quality
        calculatedRegister.Availability.ShouldBe(1.0, 0.001); // 100% availability
        calculatedRegister.Performance.ShouldBe(1.0, 0.001); // 100% performance
        calculatedRegister.Oee.ShouldBe(1.0, 0.001); // Perfect OEE
    }
    /// <summary>
    /// Executes ToKpiOee_WithNullRegister_ShouldThrowArgumentNullException operation.
    /// </summary>

    [Fact]
    public void ToKpiOee_WithNullRegister_ShouldThrowArgumentNullException()
    {
        // Arrange
        OeeRegister nullRegister = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => OeeRegister.ToKpiOee(nullRegister!));
    }
    /// <summary>
    /// Executes ManufacturingOeeScenarios_WithRealWorldData_ShouldCalculateCorrectly operation.
    /// </summary>

    [Fact]
    public void ManufacturingOeeScenarios_WithRealWorldData_ShouldCalculateCorrectly()
    {
        // Arrange - Real automotive stamping press scenario
        var register = new OeeRegister
        {
            MachineId = 5001, // Stamping press
            ProductId = 8001, // Automotive part
            StandardCycleTime = 45.0, // 45 seconds standard
            ActualCycleTime = 42.0, // 42 seconds actual (good performance)
            PlanedProductionTime = 32400.0 // 9 hours planned
        };

        var performanceData = new PerformanceData
        {
            TotalProduction = 720.0, // Target for 9 hours
            ProductionOk = 684.0, // 95% quality (world-class)
            ProductionNoK = 36.0, // 5% defects
            CurrentTime = 32400, // 9 hours
            RunningTime = 29160, // 8.1 hours running (90% availability)
            StoppedTime = 2430, // 40.5 minutes planned stops
            FaultedTime = 810 // 13.5 minutes unplanned stops
        };

        // Act
        var result = OeeRegister.CalculateOee(register, performanceData);

        // Assert - Verify world-class automotive manufacturing metrics
        result.IsSuccess.ShouldBeTrue();
        var calculatedRegister = result.Value;
        calculatedRegister.ShouldNotBeNull();

        // Quality should be 95% (world-class for automotive)
        calculatedRegister.Quality.ShouldBe(0.95, 0.01);

        // Availability should be 90% (29160/32400)
        calculatedRegister.Availability.ShouldBe(0.90, 0.01);

        // Performance should be > 100% due to faster cycle time
        calculatedRegister.Performance.ShouldBeGreaterThan(1.0);
        calculatedRegister.Performance.ShouldBeLessThan(1.5); // Clamped at 1.5

        // Overall OEE should be excellent for automotive
        calculatedRegister.Oee.ShouldBeGreaterThan(0.80); // World-class OEE
        calculatedRegister.Oee.ShouldBeLessThanOrEqualTo(1.0);

        // Verify realistic automotive production rates
        var actualCycleTimeFromProduction = calculatedRegister.RunningTime / calculatedRegister.TotalProduction;
        actualCycleTimeFromProduction.ShouldBe(40.5, 1.0); // Should be close to actual cycle time
    }
}
