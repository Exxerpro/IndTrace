// <copyright file="BarCodesListQueryModel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.BarCodes;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a query model for filtering and retrieving lists of barcodes with date range validation.
/// </summary>
public class BarCodesListQueryModel
{

    private readonly IDateTimeMachine? dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodesListQueryModel"/> class.
    /// Date-range defaults are left unset; callers that own a clock should use the
    /// <see cref="IDateTimeMachine"/> constructor or call <see cref="SetDefaultDateRange"/> so the
    /// defaults stay deterministic instead of reading the ambient wall clock.
    /// </summary>
    public BarCodesListQueryModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodesListQueryModel"/> class with deterministic
    /// date-range defaults sourced from the supplied <see cref="IDateTimeMachine"/>.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic clock used to seed the default date range.</param>
    public BarCodesListQueryModel(IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(dateTimeMachine);
        this.dateTimeMachine = dateTimeMachine;
        this.SetDefaultDateRange(dateTimeMachine);
    }

    /// <summary>
    /// Sets the default initial/end date range using the supplied deterministic clock.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic clock used to seed the default date range.</param>
    public void SetDefaultDateRange(IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(dateTimeMachine);
        var now = dateTimeMachine.Now;
        this.InitialDate = Debugger.IsAttached ? now.AddDays(-150) : now.AddDays(-14);
        this.EndDate = now;
    }

    /// <summary>
    /// Gets or sets the initial date for the query range.
    /// </summary>
    [Required]
    public DateTime? InitialDate { get; set; }

    /// <summary>
    /// Gets or sets the end date for the query range.
    /// </summary>
    [Required]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this query is for master records.
    /// </summary>
    [Required]
    public bool IsMaster { get; set; }

    private readonly TimeSpan maxRangeConsult = TimeSpan.FromDays(365);

    /// <summary>
    /// Validates the date range based on which field was changed.
    /// </summary>
    /// <param name="fieldChanged">The name of the field that was changed ("StartDate" or "EndDate").</param>
    public void ValidateRangeOfDates(string fieldChanged)
    {
        if (fieldChanged == "StartDate")
        {
            this.ValidateRangeWhenInitialDateChanged();
        }
        else
        {
            this.ValidateRangeWhenEndDateChanged();
        }
    }

    private void ValidateRangeWhenEndDateChanged()
    {
        this.EndDate ??= this.dateTimeMachine?.Now;
        if (this.EndDate is null)
        {
            // No end date and no clock to anchor from; nothing to validate.
            return;
        }

        if (this.InitialDate is null)
        {
            this.InitialDate = this.EndDate.Value.AddDays(-1);
        }
        if (!this.IsRangeLesThanMaxRange)
        {
            // The end date is the anchor; clamp the start back to the last 365 days.
            this.InitialDate = AddTicksClamped(this.EndDate.Value, -this.maxRangeConsult.Ticks);
        }

        if (!this.IsRangeValidEndAfterInitialDate)
        {
            this.InitialDate = AddTicksClamped(this.EndDate.Value, -TimeSpan.TicksPerDay);
        }
    }

    private void ValidateRangeWhenInitialDateChanged()
    {
        if (this.InitialDate is null)
        {
            var anchor = this.dateTimeMachine?.Now;
            this.InitialDate = anchor?.AddDays(-1);
        }

        if (this.InitialDate is null)
        {
            // No initial date and no clock to anchor from; nothing to validate.
            return;
        }

        if (this.EndDate is null)
        {
            this.EndDate = this.InitialDate.Value.AddDays(1);
        }
        if (!this.IsRangeLesThanMaxRange)
        {
            // The start date is the anchor; clamp the end forward to at most 365 days later.
            this.EndDate = AddTicksClamped(this.InitialDate.Value, this.maxRangeConsult.Ticks);
        }

        if (!this.IsRangeValidEndAfterInitialDate)
        {
            this.EndDate = AddTicksClamped(this.InitialDate.Value, TimeSpan.TicksPerDay);
        }
    }

    /// <summary>
    /// Adds ticks to a <see cref="DateTime"/> while clamping to <see cref="DateTime.MinValue"/>/<see cref="DateTime.MaxValue"/>
    /// so that near-boundary inputs cannot throw <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    private static DateTime AddTicksClamped(DateTime value, long ticks)
    {
        if (ticks > 0 && value.Ticks > DateTime.MaxValue.Ticks - ticks)
        {
            return DateTime.MaxValue;
        }

        if (ticks < 0 && value.Ticks < DateTime.MinValue.Ticks - ticks)
        {
            return DateTime.MinValue;
        }

        return value.AddTicks(ticks);
    }

    private bool IsRangeValidEndAfterInitialDate
    {
        get
        {
            if (this.InitialDate is null || this.EndDate is null)
            {
                return false;
            }
            return this.EndDate.Value >= this.InitialDate.Value;
        }
    }

    private bool IsRangeLesThanMaxRange
    {
        get
        {
            if (this.InitialDate is null || this.EndDate is null)
            {
                return false;
            }
            return AddTicksClamped(this.InitialDate.Value, this.maxRangeConsult.Ticks) >= this.EndDate.Value;
        }
    }
}
