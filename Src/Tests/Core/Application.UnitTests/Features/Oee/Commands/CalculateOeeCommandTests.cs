// <copyright file="CalculateOeeCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Application.UnitTests.Features.Barcodes;

namespace Application.UnitTests.Features.Oee.Commands;

/// <summary>
/// Tests for CalculateOeeCommand record properties and basic functionality
/// </summary>
public class CalculateOeeCommandBasicTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstanceCorrectly operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstanceCorrectly()
    {
        // Arrange & Act
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = 480.0, // 8 hours shift
            DowntimeMinutes = 45.0,   // Planned maintenance
            IdealCycleTimeSeconds = 30.0, // 30 seconds per unit
            TotalCount = 960,         // Units produced
            DefectCount = 12,         // Quality issues
            Timestamp = new DateTime(2024, 12, 25, 8, 0, 0, DateTimeKind.Utc)
        };

        // Assert
        command.MachineId.ShouldBe(7001);
        command.TotalTimeMinutes.ShouldBe(480.0);
        command.DowntimeMinutes.ShouldBe(45.0);
        command.IdealCycleTimeSeconds.ShouldBe(30.0);
        command.TotalCount.ShouldBe(960);
        command.DefectCount.ShouldBe(12);
        command.Timestamp.ShouldBe(new DateTime(2024, 12, 25, 8, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Executes Timestamp_WithDefaultConstructor_ShouldUseUtcNow operation.
    /// </summary>

    [Fact]
    public void Timestamp_WithDefaultConstructor_ShouldUseUtcNow()
    {
        // Arrange & Act
        var beforeCreate = DateTime.UtcNow;
        var command = new CalculateOeeCommand { MachineId = 100001 };
        var afterCreate = DateTime.UtcNow;

        // Assert
        command.Timestamp.ShouldBeGreaterThanOrEqualTo(beforeCreate);
        command.Timestamp.ShouldBeLessThanOrEqualTo(afterCreate);
        command.Timestamp.Kind.ShouldBe(DateTimeKind.Utc);
    }

    /// <summary>
    /// Executes Record_WithSameValues_ShouldBeEqual operation.
    /// </summary>

    [Fact]
    public void Record_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var timestamp = new DateTime(2024, 12, 25, 10, 0, 0, DateTimeKind.Utc);

        var command1 = new CalculateOeeCommand
        {
            MachineId = 8001,
            TotalTimeMinutes = 600.0,
            DowntimeMinutes = 30.0,
            IdealCycleTimeSeconds = 45.0,
            TotalCount = 800,
            DefectCount = 8,
            Timestamp = timestamp
        };

        var command2 = new CalculateOeeCommand
        {
            MachineId = 8001,
            TotalTimeMinutes = 600.0,
            DowntimeMinutes = 30.0,
            IdealCycleTimeSeconds = 45.0,
            TotalCount = 800,
            DefectCount = 8,
            Timestamp = timestamp
        };

        // Assert
        command1.ShouldBe(command2);
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
    }
}

/// <summary>
/// Tests for CalculateOeeCommand validation rules using FluentValidation
/// </summary>
public class CalculateOeeCommandValidationTests
{
    private readonly CalculateOeeCommandValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CalculateOeeCommandValidationTests()
    {
        _validator = new CalculateOeeCommandValidator();
    }

    /// <summary>
    /// Executes Validator_WithValidCommand_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validator_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = 480.0,
            DowntimeMinutes = 45.0,
            IdealCycleTimeSeconds = 30.0,
            TotalCount = 960,
            DefectCount = 12,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validator_WithInvalidMachineId_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validator_WithInvalidMachineId_ShouldHaveValidationError(int machineId)
    {
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = machineId,
            TotalTimeMinutes = 480.0,
            DowntimeMinutes = 45.0,
            IdealCycleTimeSeconds = 30.0,
            TotalCount = 960,
            DefectCount = 12
        };

        // Act & Assert
        _validator
            .TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.MachineId);
    }

    /// <summary>
    /// Executes Validator_WithInvalidTotalTimeMinutes_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="totalTime">The totalTime.</param>

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(-100.5)]
    public void Validator_WithInvalidTotalTimeMinutes_ShouldHaveValidationError(double totalTime)
    {
        // Using parameters: totalTime
        _ = totalTime; // xUnit1026 fix
        // Using parameters: totalTime
        _ = totalTime; // xUnit1026 fix
        // Using parameters: totalTime
        _ = totalTime; // xUnit1026 fix
        // Using parameters: totalTime
        _ = totalTime; // xUnit1026 fix
        // Using parameters: totalTime
        _ = totalTime; // xUnit1026 fix
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = totalTime,
            DowntimeMinutes = 0.0,
            IdealCycleTimeSeconds = 30.0,
            TotalCount = 960,
            DefectCount = 12
        };

        // Act & Assert
        _validator
            .TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.TotalTimeMinutes);
    }

    /// <summary>
    /// Executes Validator_WithNegativeDowntimeMinutes_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="downtime">The downtime.</param>

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-100.5)]
    public void Validator_WithNegativeDowntimeMinutes_ShouldHaveValidationError(double downtime)
    {
        // Using parameters: downtime
        _ = downtime; // xUnit1026 fix
        // Using parameters: downtime
        _ = downtime; // xUnit1026 fix
        // Using parameters: downtime
        _ = downtime; // xUnit1026 fix
        // Using parameters: downtime
        _ = downtime; // xUnit1026 fix
        // Using parameters: downtime
        _ = downtime; // xUnit1026 fix
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = 480.0,
            DowntimeMinutes = downtime,
            IdealCycleTimeSeconds = 30.0,
            TotalCount = 960,
            DefectCount = 12
        };

        // Act & Assert
        _validator
            .TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.DowntimeMinutes);
    }

    /// <summary>
    /// Executes Validator_WithInvalidIdealCycleTimeSeconds_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="cycleTime">The cycleTime.</param>

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(-50.5)]
    public void Validator_WithInvalidIdealCycleTimeSeconds_ShouldHaveValidationError(double cycleTime)
    {
        // Using parameters: cycleTime
        _ = cycleTime; // xUnit1026 fix
        // Using parameters: cycleTime
        _ = cycleTime; // xUnit1026 fix
        // Using parameters: cycleTime
        _ = cycleTime; // xUnit1026 fix
        // Using parameters: cycleTime
        _ = cycleTime; // xUnit1026 fix
        // Using parameters: cycleTime
        _ = cycleTime; // xUnit1026 fix
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = 480.0,
            DowntimeMinutes = 45.0,
            IdealCycleTimeSeconds = cycleTime,
            TotalCount = 960,
            DefectCount = 12
        };

        // Act & Assert
        _validator
            .TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.IdealCycleTimeSeconds);
    }

    /// <summary>
    /// Executes Validator_WithNegativeTotalCount_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="totalCount">The totalCount.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validator_WithNegativeTotalCount_ShouldHaveValidationError(int totalCount)
    {
        // Using parameters: totalCount
        _ = totalCount; // xUnit1026 fix
        // Using parameters: totalCount
        _ = totalCount; // xUnit1026 fix
        // Using parameters: totalCount
        _ = totalCount; // xUnit1026 fix
        // Using parameters: totalCount
        _ = totalCount; // xUnit1026 fix
        // Using parameters: totalCount
        _ = totalCount; // xUnit1026 fix
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = 480.0,
            DowntimeMinutes = 45.0,
            IdealCycleTimeSeconds = 30.0,
            TotalCount = totalCount,
            DefectCount = 0
        };

        // Act & Assert
        _validator
            .TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.TotalCount);
    }

    /// <summary>
    /// Executes Validator_WithNegativeDefectCount_ShouldHaveValidationError operation.
    /// </summary>
    /// <param name="defectCount">The defectCount.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Validator_WithNegativeDefectCount_ShouldHaveValidationError(int defectCount)
    {
        // Using parameters: defectCount
        _ = defectCount; // xUnit1026 fix
        // Using parameters: defectCount
        _ = defectCount; // xUnit1026 fix
        // Using parameters: defectCount
        _ = defectCount; // xUnit1026 fix
        // Using parameters: defectCount
        _ = defectCount; // xUnit1026 fix
        // Using parameters: defectCount
        _ = defectCount; // xUnit1026 fix
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = 480.0,
            DowntimeMinutes = 45.0,
            IdealCycleTimeSeconds = 30.0,
            TotalCount = 960,
            DefectCount = defectCount
        };

        // Act & Assert
        _validator
            .TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.DefectCount);
    }

    /// <summary>
    /// Executes Validator_WithDowntimeExceedingTotalTime_ShouldHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validator_WithDowntimeExceedingTotalTime_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = 480.0, // 8 hours
            DowntimeMinutes = 500.0,  // More than total time!
            IdealCycleTimeSeconds = 30.0,
            TotalCount = 960,
            DefectCount = 12
        };

        // Act & Assert
        _validator
            .TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x);
    }

    /// <summary>
    /// Executes Validator_WithDefectCountExceedingTotalCount_ShouldHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validator_WithDefectCountExceedingTotalCount_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = 480.0,
            DowntimeMinutes = 45.0,
            IdealCycleTimeSeconds = 30.0,
            TotalCount = 960,
            DefectCount = 1000 // More defects than total production!
        };

        // Act & Assert
        _validator
            .TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x);
    }

    /// <summary>
    /// Executes Validator_WithEmptyTimestamp_ShouldHaveValidationError operation.
    /// </summary>

    [Fact]
    public void Validator_WithEmptyTimestamp_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = 7001,
            TotalTimeMinutes = 480.0,
            DowntimeMinutes = 45.0,
            IdealCycleTimeSeconds = 30.0,
            TotalCount = 960,
            DefectCount = 12,
            Timestamp = default(DateTime) // Empty timestamp
        };

        // Act & Assert
        _validator
            .TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Timestamp);
    }
}

/// <summary>
/// Tests for CalculateOeeCommand with realistic manufacturing scenarios
/// </summary>
public class CalculateOeeCommandManufacturingScenarioTests
{
    private readonly CalculateOeeCommandValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CalculateOeeCommandManufacturingScenarioTests()
    {
        _validator = new CalculateOeeCommandValidator();
    }

    /// <summary>
    /// Executes Command_WithAutomotiveEngineBlockProduction_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Command_WithAutomotiveEngineBlockProduction_ShouldPassValidation()
    {
        // Arrange - Automotive engine block CNC machining scenario
        var command = new CalculateOeeCommand
        {
            MachineId = 7001, // CNC Milling Center
            TotalTimeMinutes = 1440.0, // 24-hour operation
            DowntimeMinutes = 120.0,   // 2 hours planned maintenance + changeover
            IdealCycleTimeSeconds = 1800.0, // 30 minutes per engine block
            TotalCount = 46,           // 46 engine blocks produced
            DefectCount = 2,           // 2 blocks failed quality inspection
            Timestamp = new DateTime(2024, 12, 25, 6, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify automotive manufacturing characteristics
        (command.TotalTimeMinutes / 60).ShouldBe(24.0); // 24-hour operation
        (command.IdealCycleTimeSeconds / 60).ShouldBe(30.0); // 30 min per block
        (command.DefectCount / (double)command.TotalCount).ShouldBeLessThan(0.05); // <5% defect rate
    }

    /// <summary>
    /// Executes Command_WithElectronicsPCBAssembly_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Command_WithElectronicsPCBAssembly_ShouldPassValidation()
    {
        // Arrange - Electronics PCB SMT line scenario
        var command = new CalculateOeeCommand
        {
            MachineId = 8001, // SMT Pick & Place Line
            TotalTimeMinutes = 600.0,    // 10-hour shift
            DowntimeMinutes = 45.0,      // Setup + minor adjustments
            IdealCycleTimeSeconds = 25.0, // 25 seconds per PCB
            TotalCount = 1440,           // High-volume PCB production
            DefectCount = 15,            // 15 PCBs failed AOI test
            Timestamp = new DateTime(2024, 12, 25, 14, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify electronics manufacturing characteristics
        command.TotalCount.ShouldBeGreaterThan(1000); // High-volume production
        command.IdealCycleTimeSeconds.ShouldBeLessThan(60.0); // Fast cycle time
        (command.DefectCount / (double)command.TotalCount).ShouldBeLessThan(0.02); // <2% defect rate
    }

    /// <summary>
    /// Executes Command_WithPharmaceuticalFillLine_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Command_WithPharmaceuticalFillLine_ShouldPassValidation()
    {
        // Arrange - Pharmaceutical sterile fill line scenario
        var command = new CalculateOeeCommand
        {
            MachineId = 9001, // Sterile Fill Line
            TotalTimeMinutes = 720.0,    // 12-hour sterile operation
            DowntimeMinutes = 90.0,      // Changeover + sterilization
            IdealCycleTimeSeconds = 2.0,  // 2 seconds per vial (high speed)
            TotalCount = 12960,          // Very high volume pharmaceutical
            DefectCount = 25,            // 25 vials rejected by vision system
            Timestamp = new DateTime(2024, 12, 25, 2, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify pharmaceutical manufacturing characteristics
        command.TotalCount.ShouldBeGreaterThan(10000); // Very high volume
        command.IdealCycleTimeSeconds.ShouldBeLessThan(5.0); // Very fast filling
        (command.DefectCount / (double)command.TotalCount).ShouldBeLessThan(0.003); // <0.3% defect rate (pharma quality)
    }

    /// <summary>
    /// Executes Command_WithVariousManufacturingScenarios_ShouldPassValidation operation.
    /// </summary>

    [Theory]
    [InlineData(7001, 480.0, 24.0, 120.0, 24, 1, "Automotive slow production")]    // Low volume automotive
    [InlineData(8001, 600.0, 18.0, 15.0, 2000, 20, "Electronics high volume")]     // High volume electronics
    [InlineData(9001, 540.0, 1.5, 5.0, 21600, 10, "Pharmaceutical ultra-fast")]    // Pharmaceutical high speed
    [InlineData(10001, 720.0, 600.0, 60.0, 720, 5, "Food packaging standard")]     // Food & beverage packaging
    public void Command_WithVariousManufacturingScenarios_ShouldPassValidation(
        int machineId, double totalTime, double cycleTime, double downtime,
        int totalCount, int defectCount, string scenarioDescription)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeRejectedViewTests>();
        logger.LogInformation("Testing scenario: {Scenario}", scenarioDescription);
        // Arrange
        var command = new CalculateOeeCommand
        {
            MachineId = machineId,
            TotalTimeMinutes = totalTime,
            DowntimeMinutes = downtime,
            IdealCycleTimeSeconds = cycleTime,
            TotalCount = totalCount,
            DefectCount = defectCount,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify business constraints
        command.DowntimeMinutes.ShouldBeLessThanOrEqualTo(command.TotalTimeMinutes);
        command.DefectCount.ShouldBeLessThanOrEqualTo(command.TotalCount);
        command.MachineId.ShouldBeGreaterThan(0);
    }
}