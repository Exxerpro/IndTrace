// <copyright file="DateTimeMachine.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Models;

using System;
using System.Reflection;
using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents the DateTimeMachineProvider.
/// </summary>
public class DateTimeMachineProvider : TimeProvider;

/// <summary>
/// Represents the DateTimeMachine implementation for date and time operations.
/// </summary>
public class DateTimeMachine(TimeProvider? timeProvider = null) : IDateTimeMachine
{
    private readonly TimeProvider timeProvider = timeProvider ?? new DateTimeMachineProvider();

    /// <summary>
    /// Gets the current date and time in local time.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 22/08/2025
    // Reason: Pattern 17 Fix - DateTimeOffset.DateTime returns DateTimeKind.Unspecified, but tests expect DateTimeKind.Local. Fixed by explicitly setting Kind to Local.
    public DateTime Now => DateTime.SpecifyKind(this.timeProvider.GetLocalNow().DateTime, DateTimeKind.Local);

    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    public DateTime UtcNow => this.timeProvider.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Gets the current date.
    /// </summary>
    public DateTime Today => this.timeProvider.GetLocalNow().Date;

    /// <summary>
    /// Gets the current hour.
    /// </summary>
    public int Hour => this.timeProvider.GetLocalNow().Hour;

    /// <summary>
    /// Gets the current minute.
    /// </summary>
    public int Minute => this.timeProvider.GetLocalNow().Minute;

    /// <summary>
    /// Gets the current second.
    /// </summary>
    public int Second => this.timeProvider.GetLocalNow().Second;

    /// <summary>
    /// Gets the current year.
    /// </summary>
    public int Year => this.timeProvider.GetLocalNow().Year;

    /// <summary>
    /// Gets the current month.
    /// </summary>
    public int Month => this.timeProvider.GetLocalNow().Month;

    /// <summary>
    /// Gets the current day of the month.
    /// </summary>
    public int Day => this.timeProvider.GetLocalNow().Day;

    /// <summary>
    /// Gets the current day of the year.
    /// </summary>
    public int DayOfYear => this.timeProvider.GetLocalNow().DayOfYear;

    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    /// <returns>The current local date and time as a <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset GetLocalNow()
    {
        return this.timeProvider.GetLocalNow();
    }

    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <returns>The current UTC date and time as a <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset GetUtcNow()
    {
        return this.timeProvider.GetUtcNow();
    }

    /// <summary>
    /// Advances the current time by the specified time span (for testing purposes).
    /// </summary>
    /// <param name="timeSpan">The time span to advance.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure of the operation.</returns>
    public Result AdvanceTime(TimeSpan timeSpan)
    {
#if DEBUG
        try
        {
            var timeProviderType = this.timeProvider.GetType();
            if (timeProviderType.Name == "FakeTimeProvider")
            {
                var advanceMethod = timeProviderType.GetMethod("Advance");
                if (advanceMethod != null)
                {
                    advanceMethod.Invoke(this.timeProvider, new object[] { timeSpan });
                }
                else
                {
                    System.Diagnostics.Trace.TraceWarning("The method 'Advance' was not found on the FakeTimeProvider.");
                    return Result.WithFailure("Advance method not found on FakeTimeProvider");
                }
            }
        }
        catch (TargetInvocationException ex)
        {
            System.Diagnostics.Trace.TraceWarning($"An error occurred while advancing time: {ex.Message}");
            return Result.WithFailure($"An error occurred while advancing time: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Trace.TraceWarning($"An invalid argument was provided to the 'Advance' method: {ex.Message}");
            return Result.WithFailure($"Invalid argument for 'Advance': {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceWarning($"An unexpected error occurred while advancing time: {ex.Message}");
            return Result.WithFailure($"Unexpected error while advancing time: {ex.Message}");
        }

        // Success path (only reachable in DEBUG builds)
        return Result.Success();
#else
        System.Diagnostics.Trace.TraceWarning("AdvanceTime not supported in production");
        return Result.WithFailure("AdvanceTime not supported in production");
#endif
    }

    /// <summary>
    /// Sets the current date and time to the specified value (for testing purposes).
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public Result SetDateTimeNow(DateTimeOffset dateTime)
    {
#if DEBUG
        // #126 F2.3 — the reflective Invoke is guarded exactly like its sibling AdvanceTime above:
        // a faulting provider surfaces as a failure Result, never as an exception across the boundary.
        try
        {
            var offset = dateTime - this.timeProvider.GetUtcNow();

            var timeProviderType = this.timeProvider.GetType();
            if (timeProviderType.Name == "FakeTimeProvider")
            {
                var advanceMethod = timeProviderType.GetMethod("Advance");
                if (advanceMethod != null)
                {
                    advanceMethod.Invoke(this.timeProvider, new object[] { offset });
                    return Result.Success();
                }
                else
                {
                    System.Diagnostics.Trace.TraceWarning("The method 'Advance' was not found on the FakeTimeProvider.");
                    return Result.WithFailure("Advance method not found on FakeTimeProvider");
                }
            }

            return Result.WithFailure("TimeProvider is not FakeTimeProvider");
        }
        catch (TargetInvocationException ex)
        {
            System.Diagnostics.Trace.TraceWarning($"An error occurred while setting time: {ex.Message}");
            return Result.WithFailure($"An error occurred while setting time: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Trace.TraceWarning($"An invalid argument was provided to the 'Advance' method: {ex.Message}");
            return Result.WithFailure($"Invalid argument for 'Advance': {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceWarning($"An unexpected error occurred while setting time: {ex.Message}");
            return Result.WithFailure($"Unexpected error while setting time: {ex.Message}");
        }
#else
      System.Diagnostics.Trace.TraceWarning("SetDateTimeNow not supported in production");
      return Result.WithFailure("SetDateTimeNow not supported in production");
#endif
    }

    // The following methods should be outside of SetDateTineNow

    /// <summary>
    /// Adds the specified number of days to the given date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to add days to.</param>
    /// <param name="days">The number of days to add.</param>
    /// <returns>The resulting <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset AddDays(DateTimeOffset dateTime, int days)
    {
        return dateTime.AddDays(days);
    }

    /// <summary>
    /// Adds the specified number of hours to the given date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to add hours to.</param>
    /// <param name="hours">The number of hours to add.</param>
    /// <returns>The resulting <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset AddHours(DateTimeOffset dateTime, int hours)
    {
        return dateTime.AddHours(hours);
    }

    /// <summary>
    /// Adds the specified number of minutes to the given date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to add minutes to.</param>
    /// <param name="minutes">The number of minutes to add.</param>
    /// <returns>The resulting <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset AddMinutes(DateTimeOffset dateTime, int minutes)
    {
        return dateTime.AddMinutes(minutes);
    }

    /// <summary>
    /// Adds the specified number of seconds to the given date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to add seconds to.</param>
    /// <param name="seconds">The number of seconds to add.</param>
    /// <returns>The resulting <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset AddSeconds(DateTimeOffset dateTime, int seconds)
    {
        return dateTime.AddSeconds(seconds);
    }

    /// <summary>
    /// Adds the specified number of milliseconds to the given date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to add milliseconds to.</param>
    /// <param name="milliseconds">The number of milliseconds to add.</param>
    /// <returns>The resulting <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset AddMilliseconds(DateTimeOffset dateTime, double milliseconds)
    {
        return dateTime.AddMilliseconds(milliseconds);
    }

    /// <summary>
    /// Adds the specified number of ticks to the given date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to add ticks to.</param>
    /// <param name="ticks">The number of ticks to add.</param>
    /// <returns>The resulting <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset AddTicks(DateTimeOffset dateTime, long ticks)
    {
        return dateTime.AddTicks(ticks);
    }

    /// <summary>
    /// Adds the specified number of months to the given date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to add months to.</param>
    /// <param name="months">The number of months to add.</param>
    /// <returns>The resulting <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset AddMonths(DateTimeOffset dateTime, int months)
    {
        return dateTime.AddMonths(months);
    }

    /// <summary>
    /// Adds the specified number of years to the given date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to add years to.</param>
    /// <param name="years">The number of years to add.</param>
    /// <returns>The resulting <see cref="DateTimeOffset"/>.</returns>
    public DateTimeOffset AddYears(DateTimeOffset dateTime, int years)
    {
        return dateTime.AddYears(years);
    }

    /// <summary>
    /// Gets the time span between two date and time values.
    /// </summary>
    /// <param name="from">The starting date and time.</param>
    /// <param name="to">The ending date and time.</param>
    /// <returns>The time span between the two dates.</returns>
    public TimeSpan GetTimeSpan(DateTimeOffset from, DateTimeOffset to)
    {
        return to - from;
    }

    /// <summary>
    /// Gets the day of the month from the specified date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to get the day from.</param>
    /// <returns>The day of the month.</returns>
    public int GetDay(DateTimeOffset dateTime)
    {
        return dateTime.Day;
    }

    /// <summary>
    /// Gets the month from the specified date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to get the month from.</param>
    /// <returns>The month.</returns>
    public int GetMonth(DateTimeOffset dateTime)
    {
        return dateTime.Month;
    }

    /// <summary>
    /// Gets the year from the specified date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to get the year from.</param>
    /// <returns>The year.</returns>
    public int GetYear(DateTimeOffset dateTime)
    {
        return dateTime.Year;
    }

    /// <summary>
    /// Gets the day of the week from the specified date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to get the day of the week from.</param>
    /// <returns>The day of the week.</returns>
    public DayOfWeek GetDayOfWeek(DateTimeOffset dateTime)
    {
        return dateTime.DayOfWeek;
    }

    /// <summary>
    /// Gets the day of the year from the specified date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to get the day of the year from.</param>
    /// <returns>The day of the year.</returns>
    public int GetDayOfYear(DateTimeOffset dateTime)
    {
        return dateTime.DayOfYear;
    }
}
