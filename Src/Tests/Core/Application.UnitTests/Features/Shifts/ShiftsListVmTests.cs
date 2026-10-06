// <copyright file="ShiftsListVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Comprehensive unit tests for ShiftsListVm - Manufacturing shift list view model
/// </summary>
public class ShiftsListVmTests
{
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>
    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Act
        var viewModel = new ShiftsListVm();

        // Assert
        viewModel.ShouldNotBeNull();
        viewModel.Shifts.ShouldNotBeNull().ShouldBeEmpty();
        viewModel.Count.ShouldBe(0);
    }
    /// <summary>
    /// Executes Should_SetShiftsProperty_When_ValidShiftCollectionProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetShiftsProperty_When_ValidShiftCollectionProvided()
    {
        // Arrange
        var viewModel = new ShiftsListVm();
        var shifts = new List<ShiftsDto>
        {
            new() { ShiftId = 1001, StartBy = new DateTime(2024, 1, 15, 6, 0, 0), Duration = TimeSpan.FromHours(8) },
            new() { ShiftId = 1002, StartBy = new DateTime(2024, 1, 15, 14, 0, 0), Duration = TimeSpan.FromHours(8) },
            new() { ShiftId = 1003, StartBy = new DateTime(2024, 1, 15, 22, 0, 0), Duration = TimeSpan.FromHours(8) }
        };

        // Act
        viewModel.Shifts = shifts;

        // Assert
        viewModel.Shifts.ShouldNotBeNull();
        viewModel.Shifts.Count.ShouldBe(3);
        viewModel.Shifts.First().ShiftId.ShouldBe(1001);
        viewModel.Shifts.Last().ShiftId.ShouldBe(1003);
    }
    /// <summary>
    /// Executes Should_SetCountProperty_When_ValidValueProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetCountProperty_When_ValidValueProvided()
    {
        // Arrange
        var viewModel = new ShiftsListVm();
        const int expectedCount = 5;

        // Act
        viewModel.Count = expectedCount;

        // Assert
        viewModel.Count.ShouldBe(expectedCount);
    }
    /// <summary>
    /// Executes Should_HandleDifferentManufacturingShiftCounts_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(0, "Empty shift collection")]
    [InlineData(1, "Single shift - Night maintenance")]
    [InlineData(3, "Standard three-shift operation")]
    [InlineData(5, "Extended manufacturing operation")]
    [InlineData(24, "Continuous 24/7 production")]
    public void Should_HandleDifferentManufacturingShiftCounts_When_ValidDataProvided(
        int shiftCount, string description)
    {

        var logger = XUnitLogger.CreateLogger<ShiftsListVmTests>();
        logger.LogInformation("Testing scenario: {description} with shiftCount={shiftCount}",
            description, shiftCount);

        // Arrange
        var viewModel = new ShiftsListVm();
        var shifts = GenerateManufacturingShifts(shiftCount);

        // Act
        viewModel.Shifts = shifts;
        viewModel.Count = shiftCount;

        // Assert
        viewModel.Shifts.Count.ShouldBe(shiftCount);
        viewModel.Count.ShouldBe(shiftCount);

        if (shiftCount > 0)
        {
            viewModel.Shifts.ShouldNotBeEmpty();
            viewModel.Shifts.All(s => s.ShiftId > 0).ShouldBeTrue();
        }
    }
    /// <summary>
    /// Executes Should_HandleAutomotiveManufacturingScenario_When_FordF150Production operation.
    /// </summary>

    [Fact]
    public void Should_HandleAutomotiveManufacturingScenario_When_FordF150Production()
    {
        // Arrange - Ford Dearborn Assembly Plant shift schedule
        var viewModel = new ShiftsListVm();
        var fordShifts = new List<ShiftsDto>
        {
            new()
            {
                ShiftId = 1001,
                StartBy = new DateTime(2024, 1, 15, 6, 0, 0),
                Duration = TimeSpan.FromHours(8),
                MaxDuration = TimeSpan.FromHours(12),
                MinDuration = TimeSpan.FromHours(6),
                NormalDuration = TimeSpan.FromHours(8)
            },
            new()
            {
                ShiftId = 1002,
                StartBy = new DateTime(2024, 1, 15, 14, 0, 0),
                Duration = TimeSpan.FromHours(8),
                MaxDuration = TimeSpan.FromHours(12),
                MinDuration = TimeSpan.FromHours(6),
                NormalDuration = TimeSpan.FromHours(8)
            },
            new()
            {
                ShiftId = 1003,
                StartBy = new DateTime(2024, 1, 15, 22, 0, 0),
                Duration = TimeSpan.FromHours(8),
                MaxDuration = TimeSpan.FromHours(12),
                MinDuration = TimeSpan.FromHours(6),
                NormalDuration = TimeSpan.FromHours(8)
            }
        };

        // Act
        viewModel.Shifts = fordShifts;
        viewModel.Count = fordShifts.Count;

        // Assert - Ford F-150 production validation
        viewModel.ShouldSatisfyAllConditions(
            () => viewModel.Shifts.Count.ShouldBe(3),
            () => viewModel.Count.ShouldBe(3),
            () => viewModel.Shifts.All(s => s.Duration == TimeSpan.FromHours(8)).ShouldBeTrue(),
            () => viewModel.Shifts.All(s => s.MaxDuration == TimeSpan.FromHours(12)).ShouldBeTrue(),
            () => viewModel.Shifts.First().StartBy.Hour.ShouldBe(6), // Day shift
            () => viewModel.Shifts.Skip(1).First().StartBy.Hour.ShouldBe(14), // Evening shift  
            () => viewModel.Shifts.Last().StartBy.Hour.ShouldBe(22) // Night shift
        );
    }
    /// <summary>
    /// Executes Should_HandleNullShiftsCollection_When_SetToNull operation.
    /// </summary>

    [Fact]
    public void Should_HandleNullShiftsCollection_When_SetToNull()
    {
        // Arrange
        var viewModel = new ShiftsListVm();
        var initialShifts = GenerateManufacturingShifts(3);
        viewModel.Shifts = initialShifts;

        // Act
        viewModel.Shifts = null!;

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - Property directly set to null!, should be null (not empty collection)
        viewModel.Shifts.ShouldBeNull();
    }
    /// <summary>
    /// Executes Should_HandleDifferentCountValues_When_SetToCount operation.
    /// </summary>
    /// <param name="setValue">The setValue.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Should_HandleDifferentCountValues_When_SetToCount(int setValue)
    {
        // Using parameters: setValue
        _ = setValue; // xUnit1026 fix
        // Using parameters: setValue
        _ = setValue; // xUnit1026 fix
        // Using parameters: setValue
        _ = setValue; // xUnit1026 fix
        // Using parameters: setValue
        _ = setValue; // xUnit1026 fix
        // Using parameters: setValue
        _ = setValue; // xUnit1026 fix
        // Arrange
        var viewModel = new ShiftsListVm();

        // Act
        viewModel.Count = setValue;

        // Assert
        viewModel.Count.ShouldBe(setValue);
    }

    private static List<ShiftsDto> GenerateManufacturingShifts(int count)
    {
        var shifts = new List<ShiftsDto>();
        var baseDate = new DateTime(2024, 1, 15, 6, 0, 0);

        for (int i = 0; i < count; i++)
        {
            shifts.Add(new ShiftsDto
            {
                ShiftId = 1000 + i,
                StartBy = baseDate.AddHours(i * 8),
                Duration = TimeSpan.FromHours(8),
                MaxDuration = TimeSpan.FromHours(16),
                MinDuration = TimeSpan.FromHours(4),
                NormalDuration = TimeSpan.FromHours(8)
            });
        }

        return shifts;
    }
}
