// <copyright file="ShiftDetectionRule.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Services;

using IndTrace.Domain.Enum;

/// <summary>
/// Concrete shift detection rule implementation
/// Domain entity representing a manufacturing shift pattern rule.
/// </summary>
public record ShiftDetectionRule(
    int StartHour,
    int EndHour,
    ShiftType ShiftType,
    int DurationHours,
    bool SpansMidnight = false) : IShiftDetectionRule
{
    /// <summary>
    /// Determines if this rule applies to the given hour with proper midnight boundary handling.
    /// </summary>
    /// <param name="hour">The hour to check (0-23).</param>
    /// <returns><see langword="true"/> when the rule covers the hour; otherwise <see langword="false"/>. An out-of-range hour (outside 0-23) never matches and returns <see langword="false"/> rather than throwing.</returns>
    public bool AppliesTo(int hour)
    {
        // A time-of-day hour is always 0-23; anything outside that range simply cannot be
        // covered by a rule (non-throwing per the Result-over-exceptions doctrine).
        if (hour < 0 || hour > 23)
        {
            return false;
        }

        if (!this.SpansMidnight)
        {
            // Normal range (e.g., 7-15, 15-23)
            return hour >= this.StartHour && hour < this.EndHour;
        }

        // Handle midnight-spanning shifts (e.g., 23-7)
        // This means: hour >= 23 OR hour < 7
        if (this.StartHour > this.EndHour)
        {
            return hour >= this.StartHour || hour < this.EndHour;
        }

        // Fallback to normal range if misconfigured
        return hour >= this.StartHour && hour < this.EndHour;
    }
}