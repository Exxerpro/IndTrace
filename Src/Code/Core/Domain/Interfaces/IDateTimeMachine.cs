// <copyright file="IDateTimeMachine.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

/// <summary>
/// Provides a mockable interface for date and time operations, allowing for time manipulation in testing scenarios.
/// NOTE (#126 F2.1): each component property (<see cref="Hour"/>, <see cref="Year"/>, ...) reads the clock
/// independently — reading two or more in sequence can tear across a midnight/new-year rollover. Callers that
/// need multiple components of the SAME instant must take one snapshot (<see cref="Now"/> or
/// <see cref="GetLocalNow"/>) and read the components off it.
/// </summary>
public interface IDateTimeMachine
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current date.
    /// </summary>
    DateTime Today { get; }

    /// <summary>
    /// Gets the current hour (0-23).
    /// </summary>
    int Hour { get; }

    /// <summary>
    /// Gets the current minute (0-59).
    /// </summary>
    int Minute { get; }

    /// <summary>
    /// Gets the current second (0-59).
    /// </summary>
    int Second { get; }

    /// <summary>
    /// Gets the current year.
    /// </summary>
    int Year { get; }

    /// <summary>
    /// Gets the current month (1-12).
    /// </summary>
    int Month { get; }

    /// <summary>
    /// Gets the current day of the month (1-31).
    /// </summary>
    int Day { get; }

    /// <summary>
    /// Gets the current day of the year (1-366).
    /// </summary>
    int DayOfYear { get; }

    /// <summary>
    /// Advances the current time by the specified time span.
    /// </summary>
    /// <param name="timeSpan">The time span to advance by.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure of the operation.</returns>
    Result AdvanceTime(TimeSpan timeSpan);

    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    /// <returns>The current local date and time.</returns>
    DateTimeOffset GetLocalNow();

    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <returns>The current UTC date and time.</returns>
    DateTimeOffset GetUtcNow();

    /// <summary>
    /// Adds the specified number of days to a date and time.
    /// </summary>
    /// <param name="dateTime">The base date and time.</param>
    /// <param name="days">The number of days to add.</param>
    /// <returns>The resulting date and time.</returns>
    DateTimeOffset AddDays(DateTimeOffset dateTime, int days);

    /// <summary>
    /// Adds the specified number of hours to a date and time.
    /// </summary>
    /// <param name="dateTime">The base date and time.</param>
    /// <param name="hours">The number of hours to add.</param>
    /// <returns>The resulting date and time.</returns>
    DateTimeOffset AddHours(DateTimeOffset dateTime, int hours);

    /// <summary>
    /// Adds the specified number of minutes to a date and time.
    /// </summary>
    /// <param name="dateTime">The base date and time.</param>
    /// <param name="minutes">The number of minutes to add.</param>
    /// <returns>The resulting date and time.</returns>
    DateTimeOffset AddMinutes(DateTimeOffset dateTime, int minutes);

    /// <summary>
    /// Adds the specified number of seconds to a date and time.
    /// </summary>
    /// <param name="dateTime">The base date and time.</param>
    /// <param name="seconds">The number of seconds to add.</param>
    /// <returns>The resulting date and time.</returns>
    DateTimeOffset AddSeconds(DateTimeOffset dateTime, int seconds);

    /// <summary>
    /// Adds the specified number of milliseconds to a date and time.
    /// </summary>
    /// <param name="dateTime">The base date and time.</param>
    /// <param name="milliseconds">The number of milliseconds to add.</param>
    /// <returns>The resulting date and time.</returns>
    DateTimeOffset AddMilliseconds(DateTimeOffset dateTime, double milliseconds);

    /// <summary>
    /// Adds the specified number of ticks to a date and time.
    /// </summary>
    /// <param name="dateTime">The base date and time.</param>
    /// <param name="ticks">The number of ticks to add.</param>
    /// <returns>The resulting date and time.</returns>
    DateTimeOffset AddTicks(DateTimeOffset dateTime, long ticks);

    /// <summary>
    /// Adds the specified number of months to a date and time.
    /// </summary>
    /// <param name="dateTime">The base date and time.</param>
    /// <param name="months">The number of months to add.</param>
    /// <returns>The resulting date and time.</returns>
    DateTimeOffset AddMonths(DateTimeOffset dateTime, int months);

    /// <summary>
    /// Adds the specified number of years to a date and time.
    /// </summary>
    /// <param name="dateTime">The base date and time.</param>
    /// <param name="years">The number of years to add.</param>
    /// <returns>The resulting date and time.</returns>
    DateTimeOffset AddYears(DateTimeOffset dateTime, int years);

    /// <summary>
    /// Calculates the time span between two date and time values.
    /// </summary>
    /// <param name="from">The start date and time.</param>
    /// <param name="to">The end date and time.</param>
    /// <returns>The time span between the two dates.</returns>
    TimeSpan GetTimeSpan(DateTimeOffset from, DateTimeOffset to);

    /// <summary>
    /// Gets the day of the month from a date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to extract the day from.</param>
    /// <returns>The day of the month (1-31).</returns>
    int GetDay(DateTimeOffset dateTime);

    /// <summary>
    /// Gets the month from a date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to extract the month from.</param>
    /// <returns>The month (1-12).</returns>
    int GetMonth(DateTimeOffset dateTime);

    /// <summary>
    /// Gets the year from a date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to extract the year from.</param>
    /// <returns>The year.</returns>
    int GetYear(DateTimeOffset dateTime);

    /// <summary>
    /// Gets the day of the week from a date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to extract the day of week from.</param>
    /// <returns>The day of the week.</returns>
    DayOfWeek GetDayOfWeek(DateTimeOffset dateTime);

    /// <summary>
    /// Gets the day of the year from a date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to extract the day of year from.</param>
    /// <returns>The day of the year (1-366).</returns>
    int GetDayOfYear(DateTimeOffset dateTime);
}
