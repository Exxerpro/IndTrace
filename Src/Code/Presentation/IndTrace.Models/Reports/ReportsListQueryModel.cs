// <copyright file="ReportsListQueryModel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Reports;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using IndTrace.Application.BarCodes.Queries.GetReportsList.FiltersInfo;
using IndTrace.Application.Models.RequestHandler;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a query model for filtering and retrieving reports with various filter options and date range validation.
/// </summary>
public class ReportsListQueryModel
{
    private readonly IDateTimeMachine? dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsListQueryModel"/> class.
    /// Date-range defaults are left unset; callers that own a clock should use the
    /// <see cref="IDateTimeMachine"/> constructor or call <see cref="SetDefaultDateRange"/> so the
    /// defaults stay deterministic instead of reading the ambient wall clock.
    /// </summary>
    public ReportsListQueryModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsListQueryModel"/> class with deterministic
    /// date-range defaults sourced from the supplied <see cref="IDateTimeMachine"/>.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic clock used to seed the default date range.</param>
    public ReportsListQueryModel(IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(dateTimeMachine);
        this.dateTimeMachine = dateTimeMachine;
        this.SetDefaultDateRange(dateTimeMachine);
    }

    /// <summary>
    /// Sets the default start/end date range using the supplied deterministic clock.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic clock used to seed the default date range.</param>
    public void SetDefaultDateRange(IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(dateTimeMachine);
        var now = dateTimeMachine.Now;
        this.StartDate = Debugger.IsAttached ? now.AddDays(-150) : now.AddDays(-14);
        this.EndDate = now;
    }

    private readonly TimeSpan maxRangeConsult = TimeSpan.FromDays(365);

    /// <summary>
    /// Gets or sets the start date for the reports query range.
    /// </summary>
    [Required]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date for the reports query range.
    /// </summary>
    [Required]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to filter for master records only.
    /// </summary>
    [Required]
    public bool IsMaster { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to apply product filtering.
    /// </summary>
    [Required]
    public bool FilterByProduct { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to apply shift filtering.
    /// </summary>
    [Required]
    public bool FilterByShift { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to apply line filtering.
    /// </summary>
    [Required]
    public bool FilterByLine { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to apply state filtering.
    /// </summary>
    [Required]
    public bool FilterByState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to apply register filtering.
    /// </summary>
    [Required]
    public bool FilterByRegister { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to apply customer filtering.
    /// </summary>
    [Required]
    public bool FilterByCustomer { get; set; }

    /// <summary>
    /// Gets or sets the selected product for filtering.
    /// </summary>
    public string SelectedProduct { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected state for filtering.
    /// </summary>
    public string SelectedState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected customer for filtering.
    /// </summary>
    public string SelectedCustomer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected line for filtering.
    /// </summary>
    public string SelectedLine { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected register for filtering.
    /// </summary>
    public string SelectedRegister { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected shift for filtering.
    /// </summary>
    public int SelectedShift { get; set; }

    /// <summary>
    /// Gets or sets the list of available products for filtering.
    /// </summary>
    public List<string> Products { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of available customers for filtering.
    /// </summary>
    public List<string> Customers { get; set; } = [];

    /// <summary>
    /// Gets the list of customer products for filtering.
    /// </summary>
    public List<CustomerProduct> CustomerProducts { get; private set; } = [];

    /// <summary>
    /// Gets or sets the list of available states for filtering.
    /// </summary>
    public List<string> StatesList { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of available shifts for filtering.
    /// </summary>
    public List<int> Shifts { get; set; } = [];

    private IMonitorRequestDispatcher? monitorRequestDispatcher;

    /// <summary>
    /// Gets or sets a value indicating whether the model has been initialized.
    /// </summary>
    public bool IsInitialized { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsListQueryModel"/> class with a command dispatcher
    /// and a deterministic clock.
    /// </summary>
    /// <param name="monitorRequestDispatcher">The monitor request dispatcher for executing queries.</param>
    /// <param name="dateTimeMachine">The deterministic clock used to seed the default date range.</param>
    public ReportsListQueryModel(IMonitorRequestDispatcher monitorRequestDispatcher, IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(monitorRequestDispatcher);
        ArgumentNullException.ThrowIfNull(dateTimeMachine);
        this.monitorRequestDispatcher = monitorRequestDispatcher;
        this.dateTimeMachine = dateTimeMachine;
        var now = dateTimeMachine.Now;
        this.StartDate = now.AddDays(-7);
        this.EndDate = now;
        this.SelectedProduct = string.Empty;
        this.SelectedState = string.Empty;
        this.SelectedShift = 0;
        this.Products = [];
        this.StatesList = [];
        this.Customers = [];
        this.Shifts = [];
    }

    private async Task<ReportsFilterInfoVm> GetInfoFilterReports(IMonitorRequestDispatcher monitorRequestDispatcher)
    {
        this.monitorRequestDispatcher = monitorRequestDispatcher;
        if (this.dateTimeMachine is null)
        {
            return new ReportsFilterInfoVm();
        }

        var now = this.dateTimeMachine.Now;
        var request = new GetReportsFilterInfoQuery(false, now.AddDays(-1).AddTicks(-1), now);

        var result = await this.monitorRequestDispatcher.QueryAsync(request);
        return result.Value ?? new ReportsFilterInfoVm();
    }

    /// <summary>
    /// Initializes the model asynchronously by loading filter information.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        if (this.monitorRequestDispatcher is null)
        {
            // Not initialized with a dispatcher; leave defaults and mark not initialized.
            this.IsInitialized = false;
            return;
        }

        var result = await this.GetInfoFilterReports(this.monitorRequestDispatcher);
        this.Products = result.Products;
        this.StatesList = result.States;
        this.Shifts = result.Shifts;
        this.Customers = result.Customers;
        this.CustomerProducts = result.CustomerProducts;

        this.IsInitialized = true;
    }

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
        if (!this.IsRangeLesThanMaxRange)
        {
            if (this.EndDate is DateTime end)
            {
                // The end date is the anchor; clamp the start back to the last 365 days.
                this.StartDate = AddTicksClamped(end, -this.maxRangeConsult.Ticks);
            }
        }

        if (!this.IsRangeValidEndAfterInitialDate)
        {
            if (this.EndDate is DateTime end)
            {
                this.StartDate = AddTicksClamped(end, -TimeSpan.TicksPerDay);
            }
        }
    }

    private void ValidateRangeWhenInitialDateChanged()
    {
        if (!this.IsRangeLesThanMaxRange)
        {
            if (this.StartDate is DateTime start)
            {
                // The start date is the anchor; clamp the end forward to at most 365 days later.
                this.EndDate = AddTicksClamped(start, this.maxRangeConsult.Ticks);
            }
        }

        if (!this.IsRangeValidEndAfterInitialDate)
        {
            if (this.StartDate is DateTime start)
            {
                this.EndDate = AddTicksClamped(start, TimeSpan.TicksPerDay);
            }
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

    private bool IsRangeValidEndAfterInitialDate => (this.EndDate ?? DateTime.MinValue) >= (this.StartDate ?? DateTime.MinValue);

    private bool IsRangeLesThanMaxRange => AddTicksClamped(this.StartDate ?? DateTime.MinValue, this.maxRangeConsult.Ticks) >= (this.EndDate ?? DateTime.MinValue);

}
