// <copyright file="PerformanceDataValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Performances;

/// <summary>
/// Unit tests for PerformanceDataValidator
/// </summary>
public class PerformanceDataValidatorTests
{
    private readonly PerformanceDataValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public PerformanceDataValidatorTests()
    {
        _validator = new PerformanceDataValidator();
    }
    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var validator = new PerformanceDataValidator();

        // Assert
        validator.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Validate_WithValidPerformanceData_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidPerformanceData_ShouldPassValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 1, // Must be non-zero to pass validation
            FaultedTime = 1, // Must be non-zero to pass validation
            StatusFaultReason = 1, // Must be non-zero to pass validation
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 1 // Must be non-zero to pass validation
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Validate_WithInvalidMachineId_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithInvalidMachineId_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 0, // Invalid: must be greater than 0
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.MachineId);
        result.Errors.ShouldContain(e => e.ErrorMessage == "MachineId must be greater than 0.");
    }
    /// <summary>
    /// Executes Validate_WithInvalidPlcId_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithInvalidPlcId_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 0, // Invalid: must be greater than 0
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.PlcId);
        result.Errors.ShouldContain(e => e.ErrorMessage == "PlcId must be greater than 0.");
    }
    /// <summary>
    /// Executes Validate_WithInvalidBarCodeId_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithInvalidBarCodeId_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 0, // Invalid: must be greater than 0
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.BarCodeId);
        result.Errors.ShouldContain(e => e.ErrorMessage == "BarCodeID must be greater than 0.");
    }
    /// <summary>
    /// Executes Validate_WithInvalidCycleId_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithInvalidCycleId_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(0), // Invalid: must be greater than 0
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.CycleId);
        result.Errors.ShouldContain(e => e.ErrorMessage == "CycleId must be greater than 0.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyTimeStamp_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyTimeStamp_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = default, // Invalid: must not be empty
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.TimeStamp);
        result.Errors.ShouldContain(e => e.ErrorMessage == "TimeStamp must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyApplicationFlag_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyApplicationFlag_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 0, // Invalid: must not be empty
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.ApplicationFlag);
        result.Errors.ShouldContain(e => e.ErrorMessage == "ApplicationFlag must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyEventCounter_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyEventCounter_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 0, // Invalid: must not be empty
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.EventCounter);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Event_Counter must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyCurrentTime_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyCurrentTime_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 0, // Invalid: must not be empty
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.CurrentTime);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Current_Time must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyRunningTime_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyRunningTime_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 0, // Invalid: must not be empty
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.RunningTime);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Running_Time must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyStoppedTime_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyStoppedTime_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0, // Invalid: must not be empty
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.StoppedTime);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Stopped_time must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyFaultedTime_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyFaultedTime_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0, // Invalid: must not be empty
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.FaultedTime);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Faulted_Time must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyStatusFaultReason_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyStatusFaultReason_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0, // Invalid: must not be empty
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.StatusFaultReason);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Status_Fault_Reason must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyTotalProduction_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyTotalProduction_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 0, // Invalid: must not be empty
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.TotalProduction);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Total_Production must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyProductionOk_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyProductionOk_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 0, // Invalid: must not be empty
            ProductionNoK = 5,
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.ProductionOk);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Production_Ok must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyProductionNoK_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyProductionNoK_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 0, // Invalid: must not be empty
            StatusFaultReject = 0
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.ProductionNoK);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Production_NoK must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithEmptyStatusFaultReject_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyStatusFaultReject_ShouldFailValidation()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 100,
            PlcId = 1,
            BarCodeId = 1,
            CycleId = new CycleId(1),
            TimeStamp = DateTime.Now,
            ApplicationFlag = 1,
            EventCounter = 1,
            CurrentTime = 100,
            RunningTime = 100,
            StoppedTime = 0,
            FaultedTime = 0,
            StatusFaultReason = 0,
            TotalProduction = 100,
            ProductionOk = 95,
            ProductionNoK = 5,
            StatusFaultReject = 0 // Invalid: must not be empty
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.StatusFaultReject);
        result.Errors.ShouldContain(e => e.ErrorMessage == "Status_Fault_Reject must not be empty.");
    }
    /// <summary>
    /// Executes Validate_WithMultipleValidationErrors_ShouldFailAllValidations operation.
    /// </summary>

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldFailAllValidations()
    {
        // Arrange
        var performanceData = new PerformanceData
        {
            MachineId = 0, // Invalid
            PlcId = 0, // Invalid
            BarCodeId = 0, // Invalid
            CycleId = new CycleId(0), // Invalid
            TimeStamp = default, // Invalid
            ApplicationFlag = 0, // Invalid
            EventCounter = 0, // Invalid
            CurrentTime = 0, // Invalid
            RunningTime = 0, // Invalid
            StoppedTime = 0, // Invalid
            FaultedTime = 0, // Invalid
            StatusFaultReason = 0, // Invalid
            TotalProduction = 0, // Invalid
            ProductionOk = 0, // Invalid
            ProductionNoK = 0, // Invalid
            StatusFaultReject = 0 // Invalid
        };

        // Act
        var result = _validator.TestValidate(performanceData);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.MachineId);
        result.ShouldHaveValidationErrorFor(p => p.PlcId);
        result.ShouldHaveValidationErrorFor(p => p.BarCodeId);
        result.ShouldHaveValidationErrorFor(p => p.CycleId);
        result.ShouldHaveValidationErrorFor(p => p.TimeStamp);
        result.ShouldHaveValidationErrorFor(p => p.ApplicationFlag);
        result.ShouldHaveValidationErrorFor(p => p.EventCounter);
        result.ShouldHaveValidationErrorFor(p => p.CurrentTime);
        result.ShouldHaveValidationErrorFor(p => p.RunningTime);
        result.ShouldHaveValidationErrorFor(p => p.StoppedTime);
        result.ShouldHaveValidationErrorFor(p => p.FaultedTime);
        result.ShouldHaveValidationErrorFor(p => p.StatusFaultReason);
        result.ShouldHaveValidationErrorFor(p => p.TotalProduction);
        result.ShouldHaveValidationErrorFor(p => p.ProductionOk);
        result.ShouldHaveValidationErrorFor(p => p.ProductionNoK);
        result.ShouldHaveValidationErrorFor(p => p.StatusFaultReject);
    }
}