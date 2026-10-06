// <copyright file="ShiftDetectionRuleExecutorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Services;

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for <see cref="ShiftDetectionRuleExecutor"/> Result-based detection (issue #50, Chunk 1).
/// </summary>
public class ShiftDetectionRuleExecutorTests
{
    private readonly ShiftDetectionRuleExecutor executor = new();

    /// <summary>
    /// A covered hour under the standard 3-shift rules yields a successful detection.
    /// </summary>
    [Theory]
    [InlineData(8, 1, 7)]   // First shift starts at 07:00
    [InlineData(16, 2, 15)] // Second shift starts at 15:00
    [InlineData(2, 4, 23)]  // Third shift (spans midnight) started previous day at 23:00
    public void DetectShift_ForCoveredHour_ShouldReturnSuccessWithDetectedType(
        int hour, int expectedShiftTypeValue, int expectedStartHour)
    {
        // Arrange
        var currentTime = new DateTime(2024, 1, 15, hour, 0, 0);
        var expectedType = (ShiftType)expectedShiftTypeValue;

        // Act
        var result = executor.DetectShift(currentTime);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.shiftType.ShouldBe(expectedType);
        result.Value.startTime.Hour.ShouldBe(expectedStartHour);
        result.Value.duration.ShouldBe(TimeSpan.FromHours(8));
    }

    /// <summary>
    /// An hour that no rule covers returns a Result failure rather than throwing
    /// (previously <c>rules.First(...)</c> threw <see cref="InvalidOperationException"/>).
    /// </summary>
    [Fact]
    public void DetectShiftWithRules_ForUncoveredHour_ShouldReturnFailureWithoutThrowing()
    {
        // Arrange - a single narrow rule (08:00-09:00) leaves every other hour uncovered.
        var rules = new IShiftDetectionRule[]
        {
            new ShiftDetectionRule(8, 9, ShiftType.First, 1, false),
        };
        var currentTime = new DateTime(2024, 1, 15, 3, 0, 0); // 03:00 not covered

        // Act
        var result = executor.DetectShiftWithRules(currentTime, rules);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// Null rules yield a Result failure, not a NullReferenceException.
    /// </summary>
    [Fact]
    public void DetectShiftWithRules_WithNullRules_ShouldReturnFailure()
    {
        // Act
        var result = executor.DetectShiftWithRules(new DateTime(2024, 1, 15, 8, 0, 0), null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }
}
