// <copyright file="ShiftDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for ShiftsDto data transfer object.
/// Tests the industrial shift management system for production scheduling.
/// </summary>
public class ShiftDtoTests
{
    /// <summary>
    /// Executes ShiftsDto_Constructor_ShouldInitializeWithDefaults operation.
    /// </summary>
    [Fact]
    public void ShiftsDto_Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var shiftsDto = new ShiftsDto();

        // Assert
        shiftsDto.ShiftId.ShouldBe(0);
        shiftsDto.StartBy.ShouldBe(default(DateTime));
        shiftsDto.Duration.ShouldBe(default(TimeSpan));
        shiftsDto.MaxDuration.ShouldBe(new TimeSpan(16, 0, 0));
        shiftsDto.MinDuration.ShouldBe(new TimeSpan(16, 0, 0));
        shiftsDto.NormalDuration.ShouldBe(new TimeSpan(8, 30, 0));
    }
    /// <summary>
    /// Executes ShiftsDto_Properties_ShouldAllowGetAndSet operation.
    /// </summary>

    [Fact]
    public void ShiftsDto_Properties_ShouldAllowGetAndSet()
    {
        // Arrange
        var shiftsDto = new ShiftsDto();
        const int shiftId = 12345;
        var startBy = new DateTime(2025, 1, 15, 6, 0, 0);
        var duration = new TimeSpan(8, 0, 0);
        var maxDuration = new TimeSpan(12, 0, 0);
        var minDuration = new TimeSpan(6, 0, 0);
        var normalDuration = new TimeSpan(8, 30, 0);

        // Act
        shiftsDto.ShiftId = shiftId;
        shiftsDto.StartBy = startBy;
        shiftsDto.Duration = duration;
        shiftsDto.MaxDuration = maxDuration;
        shiftsDto.MinDuration = minDuration;
        shiftsDto.NormalDuration = normalDuration;

        // Assert
        shiftsDto.ShiftId.ShouldBe(shiftId);
        shiftsDto.StartBy.ShouldBe(startBy);
        shiftsDto.Duration.ShouldBe(duration);
        shiftsDto.MaxDuration.ShouldBe(maxDuration);
        shiftsDto.MinDuration.ShouldBe(minDuration);
        shiftsDto.NormalDuration.ShouldBe(normalDuration);
    }
    /// <summary>
    /// Executes ShiftsDto_WithDifferentSchedules_ShouldHandleShiftPatterns operation.
    /// </summary>

    [Theory]
    [InlineData(1001, "06:00:00", "08:00:00", "Morning shift - 6AM to 2PM")]
    [InlineData(2002, "14:00:00", "08:00:00", "Afternoon shift - 2PM to 10PM")]
    [InlineData(3003, "22:00:00", "08:00:00", "Night shift - 10PM to 6AM")]
    [InlineData(4004, "06:30:00", "12:00:00", "Extended shift - 6:30AM to 6:30PM")]
    public void ShiftsDto_WithDifferentSchedules_ShouldHandleShiftPatterns(
        int shiftId, string startTime, string durationTime, string description)
    {
        // Arrange & Act
        description.ShouldNotBeNull(); // Validates test description parameter

        var startBy = DateTime.Today.Add(TimeSpan.Parse(startTime));
        var duration = TimeSpan.Parse(durationTime);

        var shiftsDto = new ShiftsDto
        {
            ShiftId = shiftId,
            StartBy = startBy,
            Duration = duration,
            MaxDuration = new TimeSpan(16, 0, 0),
            MinDuration = new TimeSpan(4, 0, 0),
            NormalDuration = new TimeSpan(8, 30, 0)
        };

        // Assert
        shiftsDto.ShiftId.ShouldBe(shiftId);
        shiftsDto.StartBy.TimeOfDay.ShouldBe(TimeSpan.Parse(startTime));
        shiftsDto.Duration.ShouldBe(duration);
        // Each schedule represents a valid manufacturing shift pattern
    }
    /// <summary>
    /// Executes ToDto_WithValidShift_ShouldCreateCorrectDto operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidShift_ShouldCreateCorrectDto()
    {
        // Arrange
        var shift = new Shift(new DateTimeMachine())
        {
            ShiftId = new ShiftId(99999),
            StartBy = new DateTime(2025, 1, 15, 6, 0, 0),
            Duration = new TimeSpan(8, 0, 0),
            MaxDuration = new TimeSpan(12, 0, 0),
            MinDuration = new TimeSpan(6, 0, 0),
            NormalDuration = new TimeSpan(8, 30, 0)
        };

        // Act
        var resultWrapper = ShiftsDto.ToDto(shift);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShiftId.ShouldBe(99999);
        result.StartBy.ShouldBe(new DateTime(2025, 1, 15, 6, 0, 0));
        result.Duration.ShouldBe(new TimeSpan(8, 0, 0));
        result.MaxDuration.ShouldBe(new TimeSpan(12, 0, 0));
        result.MinDuration.ShouldBe(new TimeSpan(6, 0, 0));
        result.NormalDuration.ShouldBe(new TimeSpan(8, 30, 0));
    }
    /// <summary>
    /// Executes ToDto_WithNullShift_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullShift_ShouldReturnFailureResult()
    {
        // Arrange
        Shift? nullShift = null!;

        // Act
        var result = ShiftsDto.ToDto(nullShift!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Shift source cannot be null");
    }
    /// <summary>
    /// Executes ToEntity_WithValidDto_ShouldCreateCorrectEntity operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidDto_ShouldCreateCorrectEntity()
    {
        // Arrange
        var shiftsDto = new ShiftsDto
        {
            ShiftId = 77777,
            StartBy = new DateTime(2025, 1, 15, 14, 0, 0),
            Duration = new TimeSpan(8, 0, 0),
            MaxDuration = new TimeSpan(10, 0, 0),
            MinDuration = new TimeSpan(6, 0, 0),
            NormalDuration = new TimeSpan(8, 30, 0)
        };

        // Act
        var resultWrapper = ShiftsDto.ToEntity(shiftsDto);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShiftId.Value.ShouldBe(77777);
        result.StartBy.ShouldBe(new DateTime(2025, 1, 15, 14, 0, 0));
        result.Duration.ShouldBe(new TimeSpan(8, 0, 0));
        result.MaxDuration.ShouldBe(new TimeSpan(10, 0, 0));
        result.MinDuration.ShouldBe(new TimeSpan(6, 0, 0));
        result.NormalDuration.ShouldBe(new TimeSpan(8, 30, 0));
    }
    /// <summary>
    /// Executes ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Arrange
        ShiftsDto? nullDto = null!;

        // Act
        var result = ShiftsDto.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("ShiftsDto source cannot be null");
    }
    /// <summary>
    /// Executes RoundTrip_ShiftToDto_ShouldPreserveData operation.
    /// </summary>

    [Fact]
    public void RoundTrip_ShiftToDto_ShouldPreserveData()
    {
        // Arrange
        var originalShift = new Shift(new DateTimeMachine())
        {
            ShiftId = new ShiftId(88888),
            StartBy = new DateTime(2025, 1, 15, 22, 0, 0),
            Duration = new TimeSpan(8, 0, 0),
            MaxDuration = new TimeSpan(16, 0, 0),
            MinDuration = new TimeSpan(4, 0, 0),
            NormalDuration = new TimeSpan(8, 30, 0)
        };

        // Act
        var dtoWrapper = ShiftsDto.ToDto(originalShift);
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;

        var backToEntityWrapper = ShiftsDto.ToEntity(dto);

        // Assert
        backToEntityWrapper.IsSuccess.ShouldBeTrue();
        backToEntityWrapper.Value.ShouldNotBeNull();
        var backToEntity = backToEntityWrapper.Value;
        backToEntity.ShouldNotBeNull();
        backToEntity.ShouldNotBeNull();
        backToEntity.ShouldNotBeNull();
        backToEntity.ShiftId.ShouldBe(originalShift.ShiftId);
        backToEntity.StartBy.ShouldBe(originalShift.StartBy);
        backToEntity.Duration.ShouldBe(originalShift.Duration);
        backToEntity.MaxDuration.ShouldBe(originalShift.MaxDuration);
        backToEntity.MinDuration.ShouldBe(originalShift.MinDuration);
        backToEntity.NormalDuration.ShouldBe(originalShift.NormalDuration);
    }
    /// <summary>
    /// Executes ToDtoList_WithValidShifts_ShouldCreateCorrectDtoList operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithValidShifts_ShouldCreateCorrectDtoList()
    {
        // Arrange
        var shifts = new List<Shift>
        {
            new(new DateTimeMachine()) { ShiftId = new ShiftId(1), StartBy = DateTime.Today.AddHours(6), Duration = new TimeSpan(8, 0, 0) },
            new(new DateTimeMachine()) { ShiftId = new ShiftId(2), StartBy = DateTime.Today.AddHours(14), Duration = new TimeSpan(8, 0, 0) },
            new(new DateTimeMachine()) { ShiftId = new ShiftId(3), StartBy = DateTime.Today.AddHours(22), Duration = new TimeSpan(8, 0, 0) }
        };

        // Act
        var resultWrapper = ShiftsDto.ToDtoList(shifts);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value.ToList();
        result.Count.ShouldBe(3);

        result[0].ShiftId.ShouldBe(1);
        result[0].StartBy.ShouldBe(DateTime.Today.AddHours(6));
        result[0].Duration.ShouldBe(new TimeSpan(8, 0, 0));

        result[1].ShiftId.ShouldBe(2);
        result[1].StartBy.ShouldBe(DateTime.Today.AddHours(14));
        result[1].Duration.ShouldBe(new TimeSpan(8, 0, 0));

        result[2].ShiftId.ShouldBe(3);
        result[2].StartBy.ShouldBe(DateTime.Today.AddHours(22));
        result[2].Duration.ShouldBe(new TimeSpan(8, 0, 0));
    }
    /// <summary>
    /// Executes ToDtoList_WithNullShifts_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithNullShifts_ShouldReturnFailureResult()
    {
        // Arrange
        IEnumerable<Shift>? nullShifts = null!;

        // Act
        var result = ShiftsDto.ToDtoList(nullShifts!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Shift collection cannot be null");
    }
    /// <summary>
    /// Executes ToDtoList_WithEmptyShifts_ShouldReturnEmptyList operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithEmptyShifts_ShouldReturnEmptyList()
    {
        // Arrange
        var shifts = new List<Shift>();

        // Act
        var resultWrapper = ShiftsDto.ToDtoList(shifts);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value.ToList();
        result.Count.ShouldBe(0);
    }
    /// <summary>
    /// Executes ShiftsDto_WithContinuousOperations_ShouldHandleTwentyFourSevenSchedule operation.
    /// </summary>

    [Fact]
    public void ShiftsDto_WithContinuousOperations_ShouldHandleTwentyFourSevenSchedule()
    {
        // Arrange - Continuous manufacturing operations
        var shiftsDto = new ShiftsDto
        {
            ShiftId = 9999,
            StartBy = new DateTime(2025, 1, 15, 0, 0, 0),
            Duration = new TimeSpan(24, 0, 0),
            MaxDuration = new TimeSpan(24, 0, 0),
            MinDuration = new TimeSpan(8, 0, 0),
            NormalDuration = new TimeSpan(8, 30, 0)
        };

        // Act & Assert
        shiftsDto.ShiftId.ShouldBe(9999);
        shiftsDto.Duration.ShouldBe(new TimeSpan(24, 0, 0));
        shiftsDto.MaxDuration.ShouldBe(new TimeSpan(24, 0, 0));
        shiftsDto.StartBy.Hour.ShouldBe(0);
        // Verify continuous operation capability
    }
    /// <summary>
    /// Executes ShiftsDto_WithFlexibleScheduling_ShouldSupportVariableShiftLengths operation.
    /// </summary>

    [Fact]
    public void ShiftsDto_WithFlexibleScheduling_ShouldSupportVariableShiftLengths()
    {
        // Arrange - Flexible manufacturing schedules
        var flexibleShifts = new List<ShiftsDto>
        {
            new() { ShiftId = 1, Duration = new TimeSpan(4, 0, 0), StartBy = DateTime.Today.AddHours(6) }, // Part-time
            new() { ShiftId = 2, Duration = new TimeSpan(8, 0, 0), StartBy = DateTime.Today.AddHours(10) }, // Standard
            new() { ShiftId = 3, Duration = new TimeSpan(12, 0, 0), StartBy = DateTime.Today.AddHours(18) }, // Extended
            new() { ShiftId = 4, Duration = new TimeSpan(10, 30, 0), StartBy = DateTime.Today.AddHours(6.5) } // Compressed
        };

        // Act & Assert
        flexibleShifts.ForEach(shift =>
        {
            shift.ShiftId.ShouldBeGreaterThan(0);
            shift.Duration.TotalHours.ShouldBeGreaterThan(0);
            shift.Duration.TotalHours.ShouldBeLessThanOrEqualTo(24);
            shift.StartBy.ShouldBeGreaterThan(DateTime.MinValue);
        });

        flexibleShifts.Count.ShouldBe(4);
        flexibleShifts.Sum(s => s.Duration.TotalHours).ShouldBe(34.5); // Total coverage
    }
    /// <summary>
    /// Executes ShiftsDto_WithAutomotiveManufacturing_ShouldHandleIndustryStandardShifts operation.
    /// </summary>

    [Fact]
    public void ShiftsDto_WithAutomotiveManufacturing_ShouldHandleIndustryStandardShifts()
    {
        // Arrange - Automotive industry shift patterns
        var automotiveShifts = new List<ShiftsDto>
        {
            // First shift - Day shift
            new()
            {
                ShiftId = 1001,
                StartBy = DateTime.Today.AddHours(6),
                Duration = new TimeSpan(8, 0, 0),
                NormalDuration = new TimeSpan(8, 0, 0),
                MaxDuration = new TimeSpan(10, 0, 0),
                MinDuration = new TimeSpan(6, 0, 0)
            },
            // Second shift - Afternoon shift
            new()
            {
                ShiftId = 2001,
                StartBy = DateTime.Today.AddHours(14),
                Duration = new TimeSpan(8, 0, 0),
                NormalDuration = new TimeSpan(8, 0, 0),
                MaxDuration = new TimeSpan(10, 0, 0),
                MinDuration = new TimeSpan(6, 0, 0)
            },
            // Third shift - Night shift
            new()
            {
                ShiftId = 3001,
                StartBy = DateTime.Today.AddHours(22),
                Duration = new TimeSpan(8, 0, 0),
                NormalDuration = new TimeSpan(8, 0, 0),
                MaxDuration = new TimeSpan(10, 0, 0),
                MinDuration = new TimeSpan(6, 0, 0)
            }
        };

        // Act & Assert
        automotiveShifts.All(s => s.Duration.TotalHours == 8).ShouldBeTrue();
        automotiveShifts.All(s => s.MaxDuration.TotalHours == 10).ShouldBeTrue();
        automotiveShifts.All(s => s.MinDuration.TotalHours == 6).ShouldBeTrue();

        // Verify complete 24-hour coverage
        var totalCoverage = automotiveShifts.Sum(s => s.Duration.TotalHours);
        totalCoverage.ShouldBe(24.0);

        // Verify no shift overlap (simplified check)
        var firstShiftEnd = automotiveShifts[0].StartBy.Add(automotiveShifts[0].Duration);
        var secondShiftStart = automotiveShifts[1].StartBy;
        secondShiftStart.ShouldBeGreaterThanOrEqualTo(firstShiftEnd);
    }
    /// <summary>
    /// Executes ShiftsDto_WithMaintenanceWindows_ShouldAllowScheduledDowntime operation.
    /// </summary>

    [Fact]
    public void ShiftsDto_WithMaintenanceWindows_ShouldAllowScheduledDowntime()
    {
        // Arrange - Maintenance scheduling
        var maintenanceShift = new ShiftsDto
        {
            ShiftId = 8888,
            StartBy = DateTime.Today.AddDays(1).AddHours(2), // Sunday 2 AM
            Duration = new TimeSpan(4, 0, 0), // 4-hour maintenance window
            MaxDuration = new TimeSpan(8, 0, 0),
            MinDuration = new TimeSpan(2, 0, 0),
            NormalDuration = new TimeSpan(4, 0, 0)
        };

        // Act & Assert
        maintenanceShift.ShiftId.ShouldBe(8888);
        maintenanceShift.Duration.ShouldBeLessThan(maintenanceShift.MaxDuration);
        maintenanceShift.Duration.ShouldBeGreaterThan(maintenanceShift.MinDuration);
        maintenanceShift.StartBy.Hour.ShouldBe(2); // Off-peak hours
        maintenanceShift.Duration.TotalHours.ShouldBe(4); // Sufficient for preventive maintenance
    }
}