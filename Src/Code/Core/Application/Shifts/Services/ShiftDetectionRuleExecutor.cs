// <copyright file="ShiftDetectionRuleExecutor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Services;

public interface IShiftDetectionRuleExecutor
{
    /// <summary>
    /// Detects shift type and calculates shift start time for given datetime
    /// Uses current 3-shift rules (future: configurable per facility).
    /// </summary>
    /// <param name="currentTime">The instant to classify.</param>
    /// <returns>A <see cref="Result{T}"/> carrying the detected shift type, start time and duration, or a failure when no rule covers the hour.</returns>
    Result<(ShiftType shiftType, DateTime startTime, TimeSpan duration)> DetectShift(DateTime currentTime);

    /// <summary>
    /// Generic shift detection with custom rules (for future database configuration).
    /// </summary>
    /// <param name="currentTime">The instant to classify.</param>
    /// <param name="rules">The candidate shift rules.</param>
    /// <returns>A <see cref="Result{T}"/> carrying the detected shift type, start time and duration, or a failure when the rules are null or no rule covers the hour.</returns>
    Result<(ShiftType shiftType, DateTime startTime, TimeSpan duration)> DetectShiftWithRules(
        DateTime currentTime,
        IShiftDetectionRule[] rules);
}

/// <summary>
/// Rule executor for shift detection. The active pattern is the standard 3-shift set below;
/// <see cref="DetectShiftWithRules"/> already accepts an arbitrary rule array, so alternate
/// (2-shift, 4-shift, custom) patterns are supported by passing rules in.
/// <para>
/// DB-driven, per-facility rule loading is intentionally DEFERRED (issue #50 chunk 3b, 2026-07-01):
/// temporal correctness is already met because each <c>Shift</c> persists its own resolved type at
/// creation, and no customer currently needs non-standard shift windows. Do not add rule-catalog
/// schema on speculation — reopen as a dedicated story only when a facility actually requires it.
/// </para>
/// </summary>
public class ShiftDetectionRuleExecutor : IShiftDetectionRuleExecutor
{
    // Standard 3-shift pattern constants (current default)
    public const int FirstShiftStart = 7;

    public const int SecondShiftStart = 15;
    public const int ThirdShiftStart = 23;

    /// <summary>
    /// Standard 3-shift manufacturing pattern — the active default (see class remarks on why
    /// DB-driven per-facility rules are deferred).
    /// </summary>
    private readonly IShiftDetectionRule[] current3ShiftRules =
    [
        new ShiftDetectionRule(FirstShiftStart, SecondShiftStart, ShiftType.First, 8),    // 7-14: First (8h)
        new ShiftDetectionRule(SecondShiftStart, ThirdShiftStart, ShiftType.Second, 8),   // 15-22: Second (8h)
        new ShiftDetectionRule(ThirdShiftStart, FirstShiftStart, ShiftType.Third, 8, SpansMidnight: true) // 23-6: Third (8h, spans midnight)
    ];

    /// <summary>
    /// Example 2-shift pattern (for facilities with day/night operations).
    /// </summary>
    private readonly IShiftDetectionRule[] example2ShiftRules =
    [
        new ShiftDetectionRule(6, 18, ShiftType.First, 12),   // 6-17: Day (12h)
        new ShiftDetectionRule(18, 6, ShiftType.Second, 12, SpansMidnight: true) // 18-5: Night (12h, spans midnight)
    ];

    /// <summary>
    /// Detects shift type and calculates shift start time for given datetime
    /// Uses current 3-shift rules (future: configurable per facility).
    /// </summary>
    /// <param name="currentTime">The instant to classify.</param>
    /// <returns>A <see cref="Result{T}"/> carrying the detected shift type, start time and duration, or a failure when no rule covers the hour.</returns>
    public Result<(ShiftType shiftType, DateTime startTime, TimeSpan duration)> DetectShift(DateTime currentTime)
    {
        return this.DetectShiftWithRules(currentTime, this.current3ShiftRules);
    }

    /// <summary>
    /// Generic shift detection with custom rules (for future database configuration).
    /// </summary>
    /// <param name="currentTime">The instant to classify.</param>
    /// <param name="rules">The candidate shift rules.</param>
    /// <returns>A <see cref="Result{T}"/> carrying the detected shift type, start time and duration, or a failure when the rules are null or no rule covers the hour.</returns>
    public Result<(ShiftType shiftType, DateTime startTime, TimeSpan duration)> DetectShiftWithRules(
        DateTime currentTime,
        IShiftDetectionRule[] rules)
    {
        if (rules is null)
        {
            return Result<(ShiftType, DateTime, TimeSpan)>.WithFailure("Shift detection rules cannot be null.");
        }

        var hour = currentTime.Hour;

        // Guarded lookup: an uncovered hour is a Result failure, not a thrown exception.
        var rule = rules.FirstOrDefault(r => r.AppliesTo(hour));
        if (rule is null)
        {
            return Result<(ShiftType, DateTime, TimeSpan)>.WithFailure($"No shift rule covers hour {hour}.");
        }

        // Calculate proper shift start time with midnight boundary handling
        DateTime shiftStartTime;
        if (rule.SpansMidnight && hour < rule.StartHour)
        {
            // Early morning hours belong to shift that started previous day
            shiftStartTime = currentTime.Date.AddDays(-1).AddHours(rule.StartHour);
        }
        else
        {
            // Normal case: shift start on same day
            shiftStartTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, rule.StartHour, 0, 0);
        }

        return Result<(ShiftType shiftType, DateTime startTime, TimeSpan duration)>.Success(
            (rule.ShiftType, shiftStartTime, TimeSpan.FromHours(rule.DurationHours)));
    }
}