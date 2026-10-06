// <copyright file="ShiftsDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Comprehensive unit tests for ShiftsDto - Manufacturing shift data transfer object
/// </summary>
public class ShiftsDtoTests
{
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>
    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Act
        var dto = new ShiftsDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.ShiftId.ShouldBe(0);
        dto.StartBy.ShouldBe(default(DateTime));
        dto.Duration.ShouldBe(default(TimeSpan));
        dto.MaxDuration.ShouldBe(new TimeSpan(16, 0, 0));
        dto.MinDuration.ShouldBe(new TimeSpan(16, 0, 0));
        dto.NormalDuration.ShouldBe(new TimeSpan(8, 30, 0));
    }
    /// <summary>
    /// Executes Should_SetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var dto = new ShiftsDto();
        const int expectedShiftId = 1001;
        var expectedStartBy = new DateTime(2024, 1, 15, 6, 0, 0);
        var expectedDuration = TimeSpan.FromHours(8);
        var expectedMaxDuration = TimeSpan.FromHours(12);
        var expectedMinDuration = TimeSpan.FromHours(6);
        var expectedNormalDuration = TimeSpan.FromHours(8);

        // Act
        dto.ShiftId = expectedShiftId;
        dto.StartBy = expectedStartBy;
        dto.Duration = expectedDuration;
        dto.MaxDuration = expectedMaxDuration;
        dto.MinDuration = expectedMinDuration;
        dto.NormalDuration = expectedNormalDuration;

        // Assert
        dto.ShiftId.ShouldBe(expectedShiftId);
        dto.StartBy.ShouldBe(expectedStartBy);
        dto.Duration.ShouldBe(expectedDuration);
        dto.MaxDuration.ShouldBe(expectedMaxDuration);
        dto.MinDuration.ShouldBe(expectedMinDuration);
        dto.NormalDuration.ShouldBe(expectedNormalDuration);
    }
    /// <summary>
    /// Executes Should_HandleDifferentManufacturingShifts_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(1001, "2024-01-15 06:00:00", 8, 12, 6, 8, "Ford F-150 day shift")]
    [InlineData(1002, "2024-01-15 14:00:00", 8, 12, 6, 8, "Ford F-150 evening shift")]
    [InlineData(1003, "2024-01-15 22:00:00", 8, 12, 6, 8, "Ford F-150 night shift")]
    [InlineData(2001, "2024-01-16 06:00:00", 10, 16, 4, 10, "Tesla Model Y extended shift")]
    [InlineData(3001, "2024-01-17 12:00:00", 12, 16, 8, 12, "Boeing 777X continuous production")]
    public void Should_HandleDifferentManufacturingShifts_When_ValidDataProvided(
        int shiftId, string startByStr, int durationHours, int maxHours, int minHours, int normalHours, string description)
    {
        // Arrange
        description.ShouldNotBeNull(); // Validates test description parameter

        var dto = new ShiftsDto();
        var startBy = DateTime.Parse(startByStr);
        var duration = TimeSpan.FromHours(durationHours);
        var maxDuration = TimeSpan.FromHours(maxHours);
        var minDuration = TimeSpan.FromHours(minHours);
        var normalDuration = TimeSpan.FromHours(normalHours);

        // Act
        dto.ShiftId = shiftId;
        dto.StartBy = startBy;
        dto.Duration = duration;
        dto.MaxDuration = maxDuration;
        dto.MinDuration = minDuration;
        dto.NormalDuration = normalDuration;

        // Assert
        dto.ShiftId.ShouldBe(shiftId);
        dto.StartBy.ShouldBe(startBy);
        dto.Duration.ShouldBe(duration);
        dto.MaxDuration.ShouldBe(maxDuration);
        dto.MinDuration.ShouldBe(minDuration);
        dto.NormalDuration.ShouldBe(normalDuration);

        // Manufacturing validation
        dto.ShouldSatisfyAllConditions(
            () => dto.ShiftId.ShouldBeGreaterThan(0),
            () => dto.Duration.TotalHours.ShouldBeGreaterThan(0),
            () => dto.MaxDuration.TotalHours.ShouldBeGreaterThanOrEqualTo(dto.Duration.TotalHours),
            () => dto.MinDuration.TotalHours.ShouldBeLessThanOrEqualTo(dto.Duration.TotalHours)
        );
    }
    /// <summary>
    /// Executes Should_ConvertToDto_When_ValidShiftEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToDto_When_ValidShiftEntityProvided()
    {
        // Arrange - Ford Dearborn Assembly Plant day shift
        var shift = new Shift(new DateTimeMachine())
        {
            ShiftId = new ShiftId(1001),
            StartBy = new DateTime(2024, 1, 15, 6, 0, 0),
            Duration = TimeSpan.FromHours(8),
            MaxDuration = TimeSpan.FromHours(12),
            MinDuration = TimeSpan.FromHours(6),
            NormalDuration = TimeSpan.FromHours(8)
        };

        // Act
        var dtoWrapper = ShiftsDto.ToDto(shift);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShiftId.ShouldBe(shift.ShiftId.Value);
        dto.StartBy.ShouldBe(shift.StartBy);
        dto.Duration.ShouldBe(shift.Duration);
        dto.MaxDuration.ShouldBe(shift.MaxDuration);
        dto.MinDuration.ShouldBe(shift.MinDuration);
        dto.NormalDuration.ShouldBe(shift.NormalDuration);
    }
    /// <summary>
    /// Executes Should_ConvertToEntity_When_ValidShiftDtoProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToEntity_When_ValidShiftDtoProvided()
    {
        // Arrange - Tesla Fremont Factory evening shift
        var dto = new ShiftsDto
        {
            ShiftId = 2002,
            StartBy = new DateTime(2024, 1, 15, 14, 0, 0),
            Duration = TimeSpan.FromHours(10),
            MaxDuration = TimeSpan.FromHours(16),
            MinDuration = TimeSpan.FromHours(8),
            NormalDuration = TimeSpan.FromHours(10)
        };

        // Act
        var entityWrapper = ShiftsDto.ToEntity(dto);

        // Assert
        entityWrapper.IsSuccess.ShouldBeTrue();
        entityWrapper.Value.ShouldNotBeNull();
        var entity = entityWrapper.Value;
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShiftId.Value.ShouldBe(dto.ShiftId);
        entity.StartBy.ShouldBe(dto.StartBy);
        entity.Duration.ShouldBe(dto.Duration);
        entity.MaxDuration.ShouldBe(dto.MaxDuration);
        entity.MinDuration.ShouldBe(dto.MinDuration);
        entity.NormalDuration.ShouldBe(dto.NormalDuration);
    }
    /// <summary>
    /// Executes Should_HandleAutomotiveManufacturingScenario_When_FordF150Production operation.
    /// </summary>

    [Fact]
    public void Should_HandleAutomotiveManufacturingScenario_When_FordF150Production()
    {
        // Arrange - Ford Dearborn Assembly Plant complete shift schedule
        var dto = new ShiftsDto();

        // Act - Configure Ford F-150 SuperCrew 4x4 day shift
        dto.ShiftId = 1001;
        dto.StartBy = new DateTime(2024, 1, 15, 6, 0, 0); // 6 AM start
        dto.Duration = TimeSpan.FromHours(8); // 8-hour shift
        dto.MaxDuration = TimeSpan.FromHours(12); // Maximum 12 hours with overtime
        dto.MinDuration = TimeSpan.FromHours(6); // Minimum 6 hours
        dto.NormalDuration = TimeSpan.FromHours(8); // Standard 8 hours

        // Assert - Ford manufacturing validation
        dto.ShouldSatisfyAllConditions(
            () => dto.ShiftId.ShouldBe(1001),
            () => dto.StartBy.Hour.ShouldBe(6), // Day shift starts at 6 AM
            () => dto.Duration.TotalHours.ShouldBe(8),
            () => dto.MaxDuration.TotalHours.ShouldBe(12),
            () => dto.MinDuration.TotalHours.ShouldBe(6),
            () => dto.NormalDuration.TotalHours.ShouldBe(8),
            () => dto.Duration.TotalHours.ShouldBeLessThanOrEqualTo(dto.MaxDuration.TotalHours),
            () => dto.Duration.TotalHours.ShouldBeGreaterThanOrEqualTo(dto.MinDuration.TotalHours)
        );
    }
}
