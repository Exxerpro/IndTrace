// <copyright file="ShiftDetectionRuleTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Services;

namespace IndTrace.Domain.UnitTests.ShiftsTests;

/// <summary>
/// TDD Test bed for ShiftDetectionRule domain logic
/// Tests the fundamental shift detection business rules
/// </summary>
public class ShiftDetectionRuleTests
{
    /// <summary>
    /// Test First Shift Rule (7-15 hours, 8 hour duration)
    /// </summary>
    [Theory]
    [InlineData(7, true)]   // Start of first shift
    [InlineData(8, true)]   // Middle of first shift
    [InlineData(14, true)]  // End of first shift (before 15)
    [InlineData(6, false)]  // Before first shift
    [InlineData(15, false)] // After first shift (start of second)
    public void FirstShiftRule_AppliesTo_ShouldReturnCorrectResult(int hour, bool expected)
    {
        // Arrange
        var rule = new ShiftDetectionRule(7, 15, ShiftType.First, 8, false);

        // Act
        var result = rule.AppliesTo(hour);

        // Assert
        result.ShouldBe(expected, $"Hour {hour} should {(expected ? "" : "not ")}apply to First shift (7-15)");
    }

    /// <summary>
    /// Test Second Shift Rule (15-23 hours, 8 hour duration)
    /// </summary>
    [Theory]
    [InlineData(15, true)]  // Start of second shift
    [InlineData(16, true)]  // Middle of second shift
    [InlineData(22, true)]  // End of second shift (before 23)
    [InlineData(14, false)] // Before second shift
    [InlineData(23, false)] // After second shift (start of third)
    public void SecondShiftRule_AppliesTo_ShouldReturnCorrectResult(int hour, bool expected)
    {
        // Arrange
        var rule = new ShiftDetectionRule(15, 23, ShiftType.Second, 8, false);

        // Act
        var result = rule.AppliesTo(hour);

        // Assert
        result.ShouldBe(expected, $"Hour {hour} should {(expected ? "" : "not ")}apply to Second shift (15-23)");
    }

    /// <summary>
    /// Test Third Shift Rule (23-7 hours, spans midnight, 8 hour duration)
    /// This is the critical test for midnight boundary logic
    /// </summary>
    [Theory]
    [InlineData(23, true)]  // Start of third shift (late night)
    [InlineData(0, true)]   // Midnight (third shift continues)
    [InlineData(1, true)]   // Early morning (still third shift)
    [InlineData(6, true)]   // End of third shift (before 7)
    [InlineData(7, false)]  // After third shift (start of first)
    [InlineData(22, false)] // Before third shift
    public void ThirdShiftRule_SpansMidnight_ShouldReturnCorrectResult(int hour, bool expected)
    {
        // Arrange
        var rule = new ShiftDetectionRule(23, 7, ShiftType.Third, 8, SpansMidnight: true);

        // Act
        var result = rule.AppliesTo(hour);

        // Assert
        result.ShouldBe(expected, $"Hour {hour} should {(expected ? "" : "not ")}apply to Third shift (23-7, spans midnight)");
    }

    /// <summary>
    /// Test rule properties are correctly set
    /// </summary>
    [Fact]
    public void ShiftDetectionRule_Properties_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var rule = new ShiftDetectionRule(7, 15, ShiftType.First, 8, false);

        // Assert
        rule.StartHour.ShouldBe(7);
        rule.EndHour.ShouldBe(15);
        rule.ShiftType.ShouldBe(ShiftType.First);
        rule.DurationHours.ShouldBe(8);
        rule.SpansMidnight.ShouldBe(false);
    }

    /// <summary>
    /// Test input validation for invalid hours: an out-of-range hour must never match a rule
    /// and must NOT throw (Result-over-exceptions doctrine — the previous throw was the defect).
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(24)]
    [InlineData(25)]
    public void ShiftDetectionRule_AppliesTo_WithInvalidHour_ShouldReturnFalseWithoutThrowing(int invalidHour)
    {
        // Arrange
        var rule = new ShiftDetectionRule(7, 15, ShiftType.First, 8, false);

        // Act
        var result = rule.AppliesTo(invalidHour);

        // Assert
        result.ShouldBeFalse($"Hour {invalidHour} is out of range and cannot apply to any shift rule");
    }

    /// <summary>
    /// Test complete 3-shift pattern doesn't have gaps or overlaps
    /// Using ShiftType values: First=1, Second=2, Third=4
    /// </summary>
    [Theory]
    [InlineData(0, 4)]   // Midnight -> Third
    [InlineData(1, 4)]   // Early morning -> Third
    [InlineData(6, 4)]   // Late third shift -> Third
    [InlineData(7, 1)]   // Start first shift -> First
    [InlineData(8, 1)]   // Mid first shift -> First
    [InlineData(14, 1)]  // Late first shift -> First
    [InlineData(15, 2)]  // Start second shift -> Second
    [InlineData(16, 2)]  // Mid second shift -> Second
    [InlineData(22, 2)]  // Late second shift -> Second
    [InlineData(23, 4)]  // Start third shift -> Third
    public void Complete3ShiftPattern_ShouldCoverAllHoursWithoutOverlap(int hour, int expectedShiftValue)
    {
        // Arrange - Standard 3-shift manufacturing pattern
        var rules = new[]
        {
            new ShiftDetectionRule(7, 15, ShiftType.First, 8, false),     // 7-14
            new ShiftDetectionRule(15, 23, ShiftType.Second, 8, false),   // 15-22
            new ShiftDetectionRule(23, 7, ShiftType.Third, 8, SpansMidnight: true) // 23-6
        };

        // Act - Find which rule applies
        var matchingRule = rules.SingleOrDefault(r => r.AppliesTo(hour));
        var expectedShift = (ShiftType)expectedShiftValue;

        // Assert
        matchingRule.ShouldNotBeNull($"Hour {hour} should match exactly one shift rule");
        matchingRule.ShiftType.ShouldBe(expectedShift, $"Hour {hour} should be {expectedShift} shift");

        // Verify only one rule matches (no overlaps)
        var matchingCount = rules.Count(r => r.AppliesTo(hour));
        matchingCount.ShouldBe(1, $"Hour {hour} should match exactly one rule, but matched {matchingCount}");
    }
}