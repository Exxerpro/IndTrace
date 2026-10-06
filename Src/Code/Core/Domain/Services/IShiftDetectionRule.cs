// <copyright file="IShiftDetectionRule.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Services;

using IndTrace.Domain.Enum;

/// <summary>
/// Interface for shift detection rules - enables database-configurable shift patterns
/// Domain service for manufacturing shift pattern detection.
/// </summary>
public interface IShiftDetectionRule
{
    /// <summary>
    /// Gets the shift type for hours that this rule applies to.
    /// </summary>
    ShiftType ShiftType { get; }

    /// <summary>
    /// Gets the start hour for this shift (0-23).
    /// </summary>
    int StartHour { get; }

    /// <summary>
    /// Gets the duration in hours for this shift.
    /// </summary>
    int DurationHours { get; }

    /// <summary>
    /// Gets a value indicating whether indicates if this shift spans midnight (for proper date calculation).
    /// </summary>
    bool SpansMidnight { get; }

    /// <summary>
    /// Determines if this rule applies to the given hour (0-23).
    /// </summary>
    /// <param name="hour">The hour to check (0-23).</param>
    /// <returns></returns>
    bool AppliesTo(int hour);
}

/// <summary>
/// Domain service interface for shift detection logic.
/// </summary>
public interface IShiftDetectionService
{
    /// <summary>
    /// Detects shift type and calculates shift start time for given datetime.
    /// </summary>
    /// <param name="currentTime">The current datetime to evaluate.</param>
    /// <returns></returns>
    (ShiftType shiftType, DateTime startTime, TimeSpan duration) DetectShift(DateTime currentTime);
}