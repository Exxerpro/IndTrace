// <copyright file="OeeRegisterDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Registers;

/// <summary>
/// Unit tests for OeeRegisterDto
/// </summary>
public class OeeRegisterDtoTests
{
    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var instance = new OeeRegisterDto();

        // Assert
        instance.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new OeeRegisterDto();
        var oeeRegisterId = 1;
        var machineId = 100;
        var plcId = 200;
        var timeStamp = DateTime.Now;
        var applicationFlag = 1;
        var eventCounter = 50;
        var currentTime = 1000;
        var runningTime = 800;
        var stoppedTime = 100;
        var faultedTime = 100;
        var statusFaultReason = 0;
        var productId = 300;
        var totalProduction = 100.5;
        var standardCycleTime = 10.5;
        var actualCycleTime = 11.2;
        var planedProductionTime = 480.0;
        var rejectEventCounter = 5;
        var statusReject = 0;
        var rejectQuantityUnits = 2.5;
        var productionOk = 95.0;
        var productionNoK = 5.0;
        var oee = 85.5;
        var availability = 90.0;
        var performance = 95.0;
        var quality = 95.0;

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
        instance.ProductId = productId;
        instance.TotalProduction = totalProduction;
        instance.StandardCycleTime = standardCycleTime;
        instance.ActualCycleTime = actualCycleTime;
        instance.PlanedProductionTime = planedProductionTime;
        instance.RejectEventCounter = rejectEventCounter;
        instance.StatusReject = statusReject;
        instance.RejectQuantityUnits = rejectQuantityUnits;
        instance.ProductionOk = productionOk;
        instance.ProductionNoK = productionNoK;
        instance.Oee = oee;
        instance.Availability = availability;
        instance.Performance = performance;
        instance.Quality = quality;

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
        instance.ProductId.ShouldBe(productId);
        instance.TotalProduction.ShouldBe(totalProduction);
        instance.StandardCycleTime.ShouldBe(standardCycleTime);
        instance.ActualCycleTime.ShouldBe(actualCycleTime);
        instance.PlanedProductionTime.ShouldBe(planedProductionTime);
        instance.RejectEventCounter.ShouldBe(rejectEventCounter);
        instance.StatusReject.ShouldBe(statusReject);
        instance.RejectQuantityUnits.ShouldBe(rejectQuantityUnits);
        instance.ProductionOk.ShouldBe(productionOk);
        instance.ProductionNoK.ShouldBe(productionNoK);
        instance.Oee.ShouldBe(oee);
        instance.Availability.ShouldBe(availability);
        instance.Performance.ShouldBe(performance);
        instance.Quality.ShouldBe(quality);
    }
    /// <summary>
    /// Executes ToDto_WithValidEntity_ShouldReturnCorrectDto operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidEntity_ShouldReturnCorrectDto()
    {
        // Arrange
        var entity = new OeeRegister
        {
            OeeRegisterId = 1,
            MachineId = 10000,
            PlcId = 200,
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 50,
            CurrentTime = 1000,
            RunningTime = 800,
            StoppedTime = 100,
            FaultedTime = 100,
            StatusFaultReason = 0,
            ProductId = 300,
            TotalProduction = 100.5,
            StandardCycleTime = 10.5,
            ActualCycleTime = 11.2,
            PlanedProductionTime = 480.0,
            RejectEventCounter = 5,
            StatusReject = 0,
            RejectQuantityUnits = 2.5,
            ProductionOk = 95.0,
            ProductionNoK = 5.0,
            Oee = 85.5,
            Availability = 90.0,
            Performance = 95.0,
            Quality = 95.0
        };

        // Act
        var dtoWrapper = OeeRegisterDto.ToDto(entity);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.OeeRegisterId.ShouldBe(entity.OeeRegisterId);
        dto.MachineId.ShouldBe(entity.MachineId);
        dto.PlcId.ShouldBe(entity.PlcId);
        dto.TimeStamp.ShouldBe(entity.TimeStamp);
        dto.ApplicationFlag.ShouldBe(entity.ApplicationFlag);
        dto.EventCounter.ShouldBe(entity.EventCounter);
        dto.CurrentTime.ShouldBe(entity.CurrentTime);
        dto.RunningTime.ShouldBe(entity.RunningTime);
        dto.StoppedTime.ShouldBe(entity.StoppedTime);
        dto.FaultedTime.ShouldBe(entity.FaultedTime);
        dto.StatusFaultReason.ShouldBe(entity.StatusFaultReason);
        dto.ProductId.ShouldBe(entity.ProductId);
        dto.TotalProduction.ShouldBe(entity.TotalProduction);
        dto.StandardCycleTime.ShouldBe(entity.StandardCycleTime);
        dto.ActualCycleTime.ShouldBe(entity.ActualCycleTime);
        dto.PlanedProductionTime.ShouldBe(entity.PlanedProductionTime);
        dto.RejectEventCounter.ShouldBe(entity.RejectEventCounter);
        dto.StatusReject.ShouldBe(entity.StatusReject);
        dto.RejectQuantityUnits.ShouldBe(entity.RejectQuantityUnits);
        dto.ProductionOk.ShouldBe(entity.ProductionOk);
        dto.ProductionNoK.ShouldBe(entity.ProductionNoK);
        dto.Oee.ShouldBe(entity.Oee);
        dto.Availability.ShouldBe(entity.Availability);
        dto.Performance.ShouldBe(entity.Performance);
        dto.Quality.ShouldBe(entity.Quality);
    }
    /// <summary>
    /// Executes ToDto_WithNullEntity_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullEntity_ShouldReturnFailureResult()
    {
        // Arrange
        OeeRegister? entity = null!;

        // Act
        var result = OeeRegisterDto.ToDto(entity!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'entity' cannot be null");
    }
    /// <summary>
    /// Executes Properties_WhenSetToZero_ShouldReturnZero operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetToZero_ShouldReturnZero()
    {
        // Arrange
        var instance = new OeeRegisterDto();

        // Act
        instance.OeeRegisterId = 0;
        instance.MachineId = 0;
        instance.PlcId = 0;
        instance.ApplicationFlag = 0;
        instance.EventCounter = 0;
        instance.CurrentTime = 0;
        instance.RunningTime = 0;
        instance.StoppedTime = 0;
        instance.FaultedTime = 0;
        instance.StatusFaultReason = 0;
        instance.ProductId = 0;
        instance.TotalProduction = 0.0;
        instance.StandardCycleTime = 0.0;
        instance.ActualCycleTime = 0.0;
        instance.PlanedProductionTime = 0.0;
        instance.RejectEventCounter = 0;
        instance.StatusReject = 0;
        instance.RejectQuantityUnits = 0.0;
        instance.ProductionOk = 0.0;
        instance.ProductionNoK = 0.0;
        instance.Oee = 0.0;
        instance.Availability = 0.0;
        instance.Performance = 0.0;
        instance.Quality = 0.0;

        // Assert
        instance.OeeRegisterId.ShouldBe(0);
        instance.MachineId.ShouldBe(0);
        instance.PlcId.ShouldBe(0);
        instance.ApplicationFlag.ShouldBe(0);
        instance.EventCounter.ShouldBe(0);
        instance.CurrentTime.ShouldBe(0);
        instance.RunningTime.ShouldBe(0);
        instance.StoppedTime.ShouldBe(0);
        instance.FaultedTime.ShouldBe(0);
        instance.StatusFaultReason.ShouldBe(0);
        instance.ProductId.ShouldBe(0);
        instance.TotalProduction.ShouldBe(0.0);
        instance.StandardCycleTime.ShouldBe(0.0);
        instance.ActualCycleTime.ShouldBe(0.0);
        instance.PlanedProductionTime.ShouldBe(0.0);
        instance.RejectEventCounter.ShouldBe(0);
        instance.StatusReject.ShouldBe(0);
        instance.RejectQuantityUnits.ShouldBe(0.0);
        instance.ProductionOk.ShouldBe(0.0);
        instance.ProductionNoK.ShouldBe(0.0);
        instance.Oee.ShouldBe(0.0);
        instance.Availability.ShouldBe(0.0);
        instance.Performance.ShouldBe(0.0);
        instance.Quality.ShouldBe(0.0);
    }
    /// <summary>
    /// Executes Properties_WhenSetToNegativeValues_ShouldReturnNegativeValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetToNegativeValues_ShouldReturnNegativeValues()
    {
        // Arrange
        var instance = new OeeRegisterDto();

        // Act
        instance.OeeRegisterId = -1;
        instance.MachineId = -100;
        instance.PlcId = -200;
        instance.ApplicationFlag = -1;
        instance.EventCounter = -50;
        instance.CurrentTime = -1000;
        instance.RunningTime = -800;
        instance.StoppedTime = -100;
        instance.FaultedTime = -100;
        instance.StatusFaultReason = -1;
        instance.ProductId = -300;
        instance.TotalProduction = -100.5;
        instance.StandardCycleTime = -10.5;
        instance.ActualCycleTime = -11.2;
        instance.PlanedProductionTime = -480.0;
        instance.RejectEventCounter = -5;
        instance.StatusReject = -1;
        instance.RejectQuantityUnits = -2.5;
        instance.ProductionOk = -95.0;
        instance.ProductionNoK = -5.0;
        instance.Oee = -85.5;
        instance.Availability = -90.0;
        instance.Performance = -95.0;
        instance.Quality = -95.0;

        // Assert
        instance.OeeRegisterId.ShouldBe(-1);
        instance.MachineId.ShouldBe(-100);
        instance.PlcId.ShouldBe(-200);
        instance.ApplicationFlag.ShouldBe(-1);
        instance.EventCounter.ShouldBe(-50);
        instance.CurrentTime.ShouldBe(-1000);
        instance.RunningTime.ShouldBe(-800);
        instance.StoppedTime.ShouldBe(-100);
        instance.FaultedTime.ShouldBe(-100);
        instance.StatusFaultReason.ShouldBe(-1);
        instance.ProductId.ShouldBe(-300);
        instance.TotalProduction.ShouldBe(-100.5);
        instance.StandardCycleTime.ShouldBe(-10.5);
        instance.ActualCycleTime.ShouldBe(-11.2);
        instance.PlanedProductionTime.ShouldBe(-480.0);
        instance.RejectEventCounter.ShouldBe(-5);
        instance.StatusReject.ShouldBe(-1);
        instance.RejectQuantityUnits.ShouldBe(-2.5);
        instance.ProductionOk.ShouldBe(-95.0);
        instance.ProductionNoK.ShouldBe(-5.0);
        instance.Oee.ShouldBe(-85.5);
        instance.Availability.ShouldBe(-90.0);
        instance.Performance.ShouldBe(-95.0);
        instance.Quality.ShouldBe(-95.0);
    }
    /// <summary>
    /// Executes TimeStamp_WhenSetToMinValue_ShouldReturnMinValue operation.
    /// </summary>

    [Fact]
    public void TimeStamp_WhenSetToMinValue_ShouldReturnMinValue()
    {
        // Arrange
        var instance = new OeeRegisterDto();

        // Act
        instance.TimeStamp = DateTime.MinValue;

        // Assert
        instance.TimeStamp.ShouldBe(DateTime.MinValue);
    }
    /// <summary>
    /// Executes TimeStamp_WhenSetToMaxValue_ShouldReturnMaxValue operation.
    /// </summary>

    [Fact]
    public void TimeStamp_WhenSetToMaxValue_ShouldReturnMaxValue()
    {
        // Arrange
        var instance = new OeeRegisterDto();

        // Act
        instance.TimeStamp = DateTime.MaxValue;

        // Assert
        instance.TimeStamp.ShouldBe(DateTime.MaxValue);
    }
    /// <summary>
    /// Executes ToDto_WithEntityHavingZeroValues_ShouldReturnDtoWithZeroValues operation.
    /// </summary>

    [Fact]
    public void ToDto_WithEntityHavingZeroValues_ShouldReturnDtoWithZeroValues()
    {
        // Arrange
        var entity = new OeeRegister
        {
            OeeRegisterId = 0,
            MachineId = 0,
            PlcId = 0,
            TimeStamp = DateTime.MinValue,
            ApplicationFlag = 0,
            EventCounter = 0,
            CurrentTime = 0,
            RunningTime = 0,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            ProductId = 0,
            TotalProduction = 0.0,
            StandardCycleTime = 0.0,
            ActualCycleTime = 0.0,
            PlanedProductionTime = 0.0,
            RejectEventCounter = 0,
            StatusReject = 0,
            RejectQuantityUnits = 0.0,
            ProductionOk = 0.0,
            ProductionNoK = 0.0,
            Oee = 0.0,
            Availability = 0.0,
            Performance = 0.0,
            Quality = 0.0
        };

        // Act
        var dtoWrapper = OeeRegisterDto.ToDto(entity);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.OeeRegisterId.ShouldBe(0);
        dto.MachineId.ShouldBe(0);
        dto.PlcId.ShouldBe(0);
        dto.TimeStamp.ShouldBe(DateTime.MinValue);
        dto.ApplicationFlag.ShouldBe(0);
        dto.EventCounter.ShouldBe(0);
        dto.CurrentTime.ShouldBe(0);
        dto.RunningTime.ShouldBe(0);
        dto.StoppedTime.ShouldBe(0);
        dto.FaultedTime.ShouldBe(0);
        dto.StatusFaultReason.ShouldBe(0);
        dto.ProductId.ShouldBe(0);
        dto.TotalProduction.ShouldBe(0.0);
        dto.StandardCycleTime.ShouldBe(0.0);
        dto.ActualCycleTime.ShouldBe(0.0);
        dto.PlanedProductionTime.ShouldBe(0.0);
        dto.RejectEventCounter.ShouldBe(0);
        dto.StatusReject.ShouldBe(0);
        dto.RejectQuantityUnits.ShouldBe(0.0);
        dto.ProductionOk.ShouldBe(0.0);
        dto.ProductionNoK.ShouldBe(0.0);
        dto.Oee.ShouldBe(0.0);
        dto.Availability.ShouldBe(0.0);
        dto.Performance.ShouldBe(0.0);
        dto.Quality.ShouldBe(0.0);
    }
}