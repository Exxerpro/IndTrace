// <copyright file="OeeServiceValidationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Oee.Services;

namespace Application.UnitTests.Features.Oee.Services;

/// <summary>
/// Validation tests for OeeService focusing on parameter validation scenarios
/// </summary>
public class OeeServiceValidationTests : IDisposable
{
    private readonly ICommandHandler<CalculateOeeCommand, OeeMetrics> _calculateOeeHandler = null!;
    private readonly IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse> _getHistoryHandler = null!;
    private readonly IOeeRepository _oeeRepository = null!;
    private readonly ILogger<OeeService> _logger = null!;
    private readonly OeeService _service = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public OeeServiceValidationTests()
    {
        _calculateOeeHandler = Substitute.For<ICommandHandler<CalculateOeeCommand, OeeMetrics>>();
        _getHistoryHandler = Substitute.For<IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse>>();
        _oeeRepository = Substitute.For<IOeeRepository>();
        _logger = XUnitLogger.CreateLogger<OeeService>();

        _service = new OeeService(
            _calculateOeeHandler,
            _getHistoryHandler,
            _oeeRepository,
            _logger);
    }

    /// <summary>
    /// Executes Should_ReturnValid_When_AllParametersAreValid operation.
    /// </summary>
    /// <returns>The result of Should_ReturnValid_When_AllParametersAreValid.</returns>

    [Fact]
    public async Task Should_ReturnValid_When_AllParametersAreValid()
    {
        // Arrange - Ford F-150 engine block production parameters
        const int machineId = 101;
        const double totalTimeMinutes = 480.0;     // 8-hour shift
        const double downtimeMinutes = 30.0;       // 30 minutes maintenance
        const double idealCycleTimeSeconds = 120.0; // 2 minutes per part
        const int totalCount = 240;               // Parts produced
        const int defectCount = 8;                // Quality defects

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            machineId, totalTimeMinutes, downtimeMinutes,
            idealCycleTimeSeconds, totalCount, defectCount);

        // Assert
        isValid.ShouldBeTrue();
        errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Should_ReturnInvalid_When_MachineIdIsInvalid operation.
    /// </summary>
    /// <param name="invalidMachineId">The invalidMachineId.</param>
    /// <param name="expectedError">The expectedError.</param>
    /// <returns>The result of Should_ReturnInvalid_When_MachineIdIsInvalid.</returns>

    [Theory]
    [InlineData(-1, "Machine ID must be greater than zero")]
    [InlineData(0, "Machine ID must be greater than zero")]
    public async Task Should_ReturnInvalid_When_MachineIdIsInvalid(int invalidMachineId, string expectedError)
    {
        // Using parameters: invalidMachineId, expectedError
        _ = invalidMachineId; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidMachineId, expectedError
        _ = invalidMachineId; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidMachineId, expectedError
        _ = invalidMachineId; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidMachineId, expectedError
        _ = invalidMachineId; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidMachineId, expectedError
        _ = invalidMachineId; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Arrange
        const double totalTimeMinutes = 480.0;
        const double downtimeMinutes = 30.0;
        const double idealCycleTimeSeconds = 120.0;
        const int totalCount = 240;
        const int defectCount = 8;

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            invalidMachineId, totalTimeMinutes, downtimeMinutes,
            idealCycleTimeSeconds, totalCount, defectCount);

        // Assert
        isValid.ShouldBeFalse();
        errors.ShouldContain(expectedError!);
    }

    /// <summary>
    /// Executes Should_ReturnInvalid_When_TotalTimeIsInvalid operation.
    /// </summary>
    /// <param name="invalidTotalTime">The invalidTotalTime.</param>
    /// <param name="expectedError">The expectedError.</param>
    /// <returns>The result of Should_ReturnInvalid_When_TotalTimeIsInvalid.</returns>

    [Theory]
    [InlineData(-1.0, "Total time must be greater than zero")]
    [InlineData(0.0, "Total time must be greater than zero")]
    public async Task Should_ReturnInvalid_When_TotalTimeIsInvalid(double invalidTotalTime, string expectedError)
    {
        // Using parameters: invalidTotalTime, expectedError
        _ = invalidTotalTime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidTotalTime, expectedError
        _ = invalidTotalTime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidTotalTime, expectedError
        _ = invalidTotalTime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidTotalTime, expectedError
        _ = invalidTotalTime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidTotalTime, expectedError
        _ = invalidTotalTime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Arrange - Tesla Model Y battery pack assembly
        const int machineId = 201;
        const double downtimeMinutes = 15.0;
        const double idealCycleTimeSeconds = 180.0;
        const int totalCount = 160;
        const int defectCount = 3;

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            machineId, invalidTotalTime, downtimeMinutes,
            idealCycleTimeSeconds, totalCount, defectCount);

        // Assert
        isValid.ShouldBeFalse();
        errors.ShouldContain(expectedError!);
    }

    /// <summary>
    /// Executes Should_ReturnInvalid_When_DowntimeIsNegative operation.
    /// </summary>
    /// <param name="invalidDowntime">The invalidDowntime.</param>
    /// <param name="expectedError">The expectedError.</param>
    /// <returns>The result of Should_ReturnInvalid_When_DowntimeIsNegative.</returns>

    [Theory]
    [InlineData(-1.0, "Downtime cannot be negative")]
    [InlineData(-10.5, "Downtime cannot be negative")]
    public async Task Should_ReturnInvalid_When_DowntimeIsNegative(double invalidDowntime, string expectedError)
    {
        // Using parameters: invalidDowntime, expectedError
        _ = invalidDowntime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidDowntime, expectedError
        _ = invalidDowntime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidDowntime, expectedError
        _ = invalidDowntime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidDowntime, expectedError
        _ = invalidDowntime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidDowntime, expectedError
        _ = invalidDowntime; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Arrange - iPhone PCB manufacturing
        const int machineId = 301;
        const double totalTimeMinutes = 420.0;
        const double idealCycleTimeSeconds = 90.0;
        const int totalCount = 280;
        const int defectCount = 12;

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            machineId, totalTimeMinutes, invalidDowntime,
            idealCycleTimeSeconds, totalCount, defectCount);

        // Assert
        isValid.ShouldBeFalse();
        errors.ShouldContain(expectedError!);
    }

    /// <summary>
    /// Executes Should_ReturnInvalid_When_DowntimeExceedsTotalTime operation.
    /// </summary>
    /// <returns>The result of Should_ReturnInvalid_When_DowntimeExceedsTotalTime.</returns>

    [Fact]
    public async Task Should_ReturnInvalid_When_DowntimeExceedsTotalTime()
    {
        // Arrange - Impossible scenario: downtime longer than total time
        const int machineId = 401;
        const double totalTimeMinutes = 240.0;
        const double downtimeMinutes = 300.0; // More downtime than total time
        const double idealCycleTimeSeconds = 150.0;
        const int totalCount = 80;
        const int defectCount = 5;

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            machineId, totalTimeMinutes, downtimeMinutes,
            idealCycleTimeSeconds, totalCount, defectCount);

        // Assert
        isValid.ShouldBeFalse();
        errors.ShouldContain("Downtime cannot exceed total time");
    }

    /// <summary>
    /// Executes Should_ReturnInvalid_When_IdealCycleTimeIsInvalid operation.
    /// </summary>
    /// <returns>The result of Should_ReturnInvalid_When_IdealCycleTimeIsInvalid.</returns>

    [Theory]
    [InlineData(-1.0, "Ideal cycle time must be greater than zero")]
    [InlineData(0.0, "Ideal cycle time must be greater than zero")]
    public async Task Should_ReturnInvalid_When_IdealCycleTimeIsInvalid(
        double invalidCycleTime, string expectedError)
    {
        // Arrange - Pharmaceutical tablet production
        const int machineId = 501;
        const double totalTimeMinutes = 360.0;
        const double downtimeMinutes = 45.0;
        const int totalCount = 1200;
        const int defectCount = 18;

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            machineId, totalTimeMinutes, downtimeMinutes,
            invalidCycleTime, totalCount, defectCount);

        // Assert
        isValid.ShouldBeFalse();
        errors.ShouldContain(expectedError!);
    }

    /// <summary>
    /// Executes Should_ReturnInvalid_When_TotalCountIsNegative operation.
    /// </summary>
    /// <param name="invalidTotalCount">The invalidTotalCount.</param>
    /// <param name="expectedError">The expectedError.</param>
    /// <returns>The result of Should_ReturnInvalid_When_TotalCountIsNegative.</returns>

    [Theory]
    [InlineData(-1, "Total count cannot be negative")]
    [InlineData(-100, "Total count cannot be negative")]
    public async Task Should_ReturnInvalid_When_TotalCountIsNegative(int invalidTotalCount, string expectedError)
    {
        // Using parameters: invalidTotalCount, expectedError
        _ = invalidTotalCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidTotalCount, expectedError
        _ = invalidTotalCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidTotalCount, expectedError
        _ = invalidTotalCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidTotalCount, expectedError
        _ = invalidTotalCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidTotalCount, expectedError
        _ = invalidTotalCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Arrange - BMW engine assembly
        const int machineId = 601;
        const double totalTimeMinutes = 480.0;
        const double downtimeMinutes = 25.0;
        const double idealCycleTimeSeconds = 200.0;
        const int defectCount = 5;

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            machineId, totalTimeMinutes, downtimeMinutes,
            idealCycleTimeSeconds, invalidTotalCount, defectCount);

        // Assert
        isValid.ShouldBeFalse();
        errors.ShouldContain(expectedError!);
    }

    /// <summary>
    /// Executes Should_ReturnInvalid_When_DefectCountIsNegative operation.
    /// </summary>
    /// <param name="invalidDefectCount">The invalidDefectCount.</param>
    /// <param name="expectedError">The expectedError.</param>
    /// <returns>The result of Should_ReturnInvalid_When_DefectCountIsNegative.</returns>

    [Theory]
    [InlineData(-1, "Defect count cannot be negative")]
    [InlineData(-50, "Defect count cannot be negative")]
    public async Task Should_ReturnInvalid_When_DefectCountIsNegative(int invalidDefectCount, string expectedError)
    {
        // Using parameters: invalidDefectCount, expectedError
        _ = invalidDefectCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidDefectCount, expectedError
        _ = invalidDefectCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidDefectCount, expectedError
        _ = invalidDefectCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidDefectCount, expectedError
        _ = invalidDefectCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Using parameters: invalidDefectCount, expectedError
        _ = invalidDefectCount; // xUnit1026 fix
        _ = expectedError; // xUnit1026 fix
        // Arrange - Coca-Cola bottling line
        const int machineId = 701;
        const double totalTimeMinutes = 600.0;
        const double downtimeMinutes = 20.0;
        const double idealCycleTimeSeconds = 2.0;
        const int totalCount = 18000;

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            machineId, totalTimeMinutes, downtimeMinutes,
            idealCycleTimeSeconds, totalCount, invalidDefectCount);

        // Assert
        isValid.ShouldBeFalse();
        errors.ShouldContain(expectedError!);
    }

    /// <summary>
    /// Executes Should_ReturnInvalid_When_DefectCountExceedsTotalCount operation.
    /// </summary>
    /// <returns>The result of Should_ReturnInvalid_When_DefectCountExceedsTotalCount.</returns>

    [Fact]
    public async Task Should_ReturnInvalid_When_DefectCountExceedsTotalCount()
    {
        // Arrange - Impossible scenario: more defects than total production
        const int machineId = 801;
        const double totalTimeMinutes = 480.0;
        const double downtimeMinutes = 30.0;
        const double idealCycleTimeSeconds = 120.0;
        const int totalCount = 200;
        const int defectCount = 250; // More defects than total count

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            machineId, totalTimeMinutes, downtimeMinutes,
            idealCycleTimeSeconds, totalCount, defectCount);

        // Assert
        isValid.ShouldBeFalse();
        errors.ShouldContain("Defect count cannot exceed total count");
    }

    /// <summary>
    /// Executes Should_ReturnMultipleErrors_When_MultipleParametersAreInvalid operation.
    /// </summary>
    /// <returns>The result of Should_ReturnMultipleErrors_When_MultipleParametersAreInvalid.</returns>

    [Fact]
    public async Task Should_ReturnMultipleErrors_When_MultipleParametersAreInvalid()
    {
        // Arrange - Multiple validation failures
        const int machineId = -1;              // Invalid machine ID
        const double totalTimeMinutes = -100.0; // Invalid total time
        const double downtimeMinutes = -50.0;   // Invalid downtime
        const double idealCycleTimeSeconds = 0.0; // Invalid cycle time
        const int totalCount = -10;            // Invalid total count
        const int defectCount = -5;            // Invalid defect count

        // Act
        var (isValid, errors) = await _service.ValidateOeeParametersAsync(
            machineId, totalTimeMinutes, downtimeMinutes,
            idealCycleTimeSeconds, totalCount, defectCount);

        // Assert
        isValid.ShouldBeFalse();
        errors.Count().ShouldBeGreaterThan(1);
        errors.ShouldContain("Machine ID must be greater than zero");
        errors.ShouldContain("Total time must be greater than zero");
        errors.ShouldContain("Downtime cannot be negative");
        errors.ShouldContain("Ideal cycle time must be greater than zero");
        errors.ShouldContain("Total count cannot be negative");
        errors.ShouldContain("Defect count cannot be negative");
    }

    /// <summary>
    /// Executes Should_AllowEdgeCases_When_ValuesAreEqual operation.
    /// </summary>
    /// <param name="value1">The value1.</param>
    /// <param name="value2">The value2.</param>
    /// <returns>The result of Should_AllowEdgeCases_When_ValuesAreEqual.</returns>

    [Theory]
    [InlineData(480.0, 480.0)] // Downtime equals total time
    [InlineData(100, 100)]     // Defects equal total count
    public async Task Should_AllowEdgeCases_When_ValuesAreEqual(double value1, double value2)
    {
        // Using parameters: value1, value2
        _ = value1; // xUnit1026 fix
        _ = value2; // xUnit1026 fix
        // Using parameters: value1, value2
        _ = value1; // xUnit1026 fix
        _ = value2; // xUnit1026 fix
        // Using parameters: value1, value2
        _ = value1; // xUnit1026 fix
        _ = value2; // xUnit1026 fix
        // Using parameters: value1, value2
        _ = value1; // xUnit1026 fix
        _ = value2; // xUnit1026 fix
        // Using parameters: value1, value2
        _ = value1; // xUnit1026 fix
        _ = value2; // xUnit1026 fix
        // Arrange - Edge cases that should be valid
        const int machineId = 901;

        // Test scenario 1: Downtime equals total time (100% downtime)
        if (value1 == 480.0)
        {
            // Act
            var (isValid, errors) = await _service.ValidateOeeParametersAsync(
                machineId, value1, value2, 120.0, 200, 10);

            // Assert
            isValid.ShouldBeTrue("Downtime equal to total time should be valid");
        }

        // Test scenario 2: Defects equal total count (100% defect rate)
        if (value1 == 100)
        {
            // Act
            var (isValid, errors) = await _service.ValidateOeeParametersAsync(
                machineId, 480.0, 30.0, 120.0, (int)value1, (int)value2);

            // Assert
            isValid.ShouldBeTrue("Defects equal to total count should be valid");
        }
    }

    /// <summary>
    /// Executes Should_HandleRealisticManufacturingScenarios_When_ValidatingParameters operation.
    /// </summary>
    /// <returns>The result of Should_HandleRealisticManufacturingScenarios_When_ValidatingParameters.</returns>

    [Fact]
    public async Task Should_HandleRealisticManufacturingScenarios_When_ValidatingParameters()
    {
        // Arrange - Collection of realistic manufacturing validation scenarios
        var scenarios = new[]
        {
			// High-volume automotive: Ford F-150 production
			(MachineId: 1001, TotalTime: 480.0, Downtime: 24.0, CycleTime: 120.0, Total: 230, Defects: 5),

			// Precision electronics: iPhone PCB assembly
			(MachineId: 2001, TotalTime: 420.0, Downtime: 60.0, CycleTime: 90.0, Total: 280, Defects: 12),

			// Pharmaceutical: FDA-regulated tablet production
			(MachineId: 3001, TotalTime: 360.0, Downtime: 45.0, CycleTime: 1.8, Total: 12000, Defects: 60),

			// Food & beverage: High-speed bottling
			(MachineId: 4001, TotalTime: 600.0, Downtime: 20.0, CycleTime: 2.0, Total: 18000, Defects: 90)
        };

        foreach (var scenario in scenarios)
        {
            // Act
            var (isValid, errors) = await _service.ValidateOeeParametersAsync(
                scenario.MachineId, scenario.TotalTime, scenario.Downtime,
                scenario.CycleTime, scenario.Total, scenario.Defects);

            // Assert
            isValid.ShouldBeTrue($"Scenario for Machine {scenario.MachineId} should be valid");
            errors.ShouldBeEmpty($"Scenario for Machine {scenario.MachineId} should have no errors");
        }
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}