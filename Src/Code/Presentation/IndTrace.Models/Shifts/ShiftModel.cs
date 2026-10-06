// <copyright file="ShiftModel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Shifts;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using IndTrace.Application.Shifts.Commands.Create;
using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a shift model for managing production shifts with duration, timing, and production tracking.
/// </summary>
public class ShiftModel(IMonitorRequestDispatcher monitorRequestDispatcher, CreateShiftCommand createShiftCommand, IDateTimeMachine dateTimeMachine)
{
    private bool isInitialized = false;

    /// <summary>
    /// Gets or sets the duration of the shift.
    /// </summary>
    [Required(ErrorMessage = "Duration is required.")]
    [Range(typeof(TimeSpan), "00:01", "16:00", ErrorMessage = "Duration must be between 1 minute and 16 hours.")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "<Pending>")]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the duration as a formatted string (hh:mm).
    /// </summary>
    public string DurationString
    {
        get => this.Duration.ToString(@"hh\:mm");
        set { this.Duration = TimeSpan.TryParse(value, out var newDuration) ? newDuration : this.NormalDuration; }
    }

    /// <summary>
    /// Gets the current hourly production rate calculated from elapsed time and total production.
    /// </summary>
    public double HourlyProduction
    {
        get
        {
            if (!this.IsRunningNow)
            {
                return 0;
            }

            var elapsed = dateTimeMachine.Now - this.StartBy;
            double hoursElapsed = elapsed.TotalHours;

            return this.ShiftProduction / hoursElapsed;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the shift is currently running.
    /// </summary>
    public bool IsRunningNow => this.StartBy <= dateTimeMachine.Now && dateTimeMachine.Now <= this.StartBy + this.Duration;

    /// <summary>
    /// Gets or sets the maximum allowed duration for a shift.
    /// </summary>
    public TimeSpan MaxDuration { get; set; } = new TimeSpan(16, 0, 0);

    /// <summary>
    /// Gets or sets the minimum allowed duration for a shift.
    /// </summary>
    public TimeSpan MinDuration { get; set; } = new TimeSpan(16, 0, 0);

    /// <summary>
    /// Gets or sets the normal/default duration for a shift.
    /// </summary>
    public TimeSpan NormalDuration { get; set; } = new TimeSpan(8, 30, 0);

    /// <summary>
    /// Gets or sets the unique identifier of the shift.
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// Gets or sets the total production count for the shift.
    /// </summary>
    public int ShiftProduction { get; set; }

    /// <summary>
    /// Gets or sets the start time of the shift.
    /// </summary>
    [Required(ErrorMessage = "Start time is required.")]
    [DataType(DataType.DateTime, ErrorMessage = "Invalid date format.")]
    public DateTime StartBy { get; set; }

    /// <summary>
    /// Initializes the shift model asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result> InitializeAsync()
    {
        if (this.isInitialized)
        {
            this.StartBy = dateTimeMachine.Now;
            this.Duration = this.NormalDuration;
            return Result.Success();
        }
        else
        {
            var result = await this.RequestInfoShiftAsync();
            return result;
        }
    }

    /// <summary>
    /// Requests shift information from the command dispatcher.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result> RequestInfoShiftAsync()
    {
        createShiftCommand.StartBy = dateTimeMachine.Now;
        createShiftCommand.Duration = this.Duration;
        createShiftCommand.ShiftProduction = this.ShiftProduction;

        var resultshift = await monitorRequestDispatcher.ProcessAsync(createShiftCommand);

        if (resultshift.IsSuccess && resultshift.Value is not null)
        {
            var shift = resultshift.Value;

            this.ShiftId = shift.ShiftId;
            this.ShiftProduction = shift.CyclesOk;
            this.StartBy = shift.StartBy;
            this.Duration = shift.Duration;

            this.isInitialized = true;
            return Result.Success();
        }
        else
        {
            this.isInitialized = false;
            return Result.WithFailure("Shift was not Found");
        }
    }

}

