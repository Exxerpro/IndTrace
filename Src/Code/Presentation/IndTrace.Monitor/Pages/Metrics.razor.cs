// <copyright file="Metrics.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using IndTrace.Application.Registers.Services;
using IndTrace.UI.Models.Metrics;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System.ComponentModel.Design;
using Castle.Components.DictionaryAdapter.Xml;
using DocumentFormat.OpenXml.Bibliography;

namespace IndTrace.Monitor.Pages;

/// <summary>
/// Represents the Metrics page component that displays time series data and variable metrics.
/// </summary>
public partial class Metrics
{
    /// <summary>
    /// #119 (F3): component-lifetime cancellation source. Its token is forwarded into every
    /// register-service query so in-flight trend/catalog work is aborted when the user navigates
    /// away (the queries were previously uncancellable — CancellationToken.None end to end).
    /// </summary>
    private readonly CancellationTokenSource componentLifetimeCts = new();

    /// <summary>
    /// Gets or sets the dataset containing variables data for display.
    /// </summary>
    public List<VariablesData> DataSet { get; private set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the component has been initialized.
    /// </summary>
    public bool IsComponentInitialized { get; set; }

    /// <summary>
    /// Gets or sets the list of available register records.
    /// </summary>
    public List<RegistersRecords> VariablesList { get; private set; } = new();

    /// <summary>
    /// Transforms a dictionary of time series data into a list of VariablesData objects.
    /// </summary>
    /// <param name="timeSeriesData">The dictionary containing time series data keyed by machine ID and name.</param>
    /// <returns>A list of VariablesData objects transformed from the input dictionary.</returns>
    public List<VariablesData> TransformDictionaryToDataSet(
        Dictionary<(int MachineId, string Name), IEnumerable<TimeSeriesDataPoint>> timeSeriesData)
    {
        return timeSeriesData.Select(entry => new VariablesData
        {
            Name = $"Machine{entry.Key.MachineId}:{entry.Key.Name}",  // Format the name
            Color = Color.Success,  // Assuming a default color; customize as needed
            Data = entry.Value
                .OrderBy(dataPoint => dataPoint.TimeStamp)  // Order by timestamp
                .Select(dataPoint => this.ParseValue(dataPoint.Value, dataPoint.ValueType))  // Parse value to double
                .Where(parsedValue => parsedValue.HasValue)  // Skip unparseable points instead of fabricating 0
                .Select(parsedValue => parsedValue.GetValueOrDefault())  // Safe: nulls filtered above
                .ToArray(),
        }).ToList();
    }

    /// <summary>
    /// Called after the component has been rendered.
    /// </summary>
    /// <param name="firstRender">True if this is the first render of the component.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await this.InitializeParticles();
            this.IsComponentInitialized = true;
            var restored = await this.PopulateDataFromLocalStorage();
            if (restored)
            {
                // #126 review C8: OnAfterRenderAsync mutations do not re-render on their own — without
                // this the restored VariablesList/DataSet never reached the UI. Marshalled through
                // InvokeAsync (the component's render idiom for out-of-render-cycle updates).
                await this.InvokeAsync(this.StateHasChanged);
            }
        }
    }

    /// <summary>
    /// Populates the component data from local storage.
    /// #126 review C8: runs from OnAfterRenderAsync (AFTER the first render), so any restored state must
    /// be followed by an explicit re-render — the caller triggers StateHasChanged when this returns true.
    /// </summary>
    /// <returns><see langword="true"/> when at least one collection was restored from local storage; otherwise <see langword="false"/>.</returns>
    private async Task<bool> PopulateDataFromLocalStorage()
    {
        var restoredAnything = false;
        try
        {
            // Retrieve the data from local storage
            var storedVariablesList = await this.LocalStorage.GetAsync<List<RegistersRecords>>(nameof(this.VariablesList));

            if (storedVariablesList.Success && storedVariablesList.Value is not null)
            {
                // Populate the VariablesList with the retrieved data
                this.VariablesList = storedVariablesList.Value;
                restoredAnything = true;
                this.Logger.LogInformation("Data loaded successfully from local storage.");
            }
            else
            {
                this.Logger.LogWarning("No data found in local storage.");
            }

            // Retrieve the DataSet from local storage
            var storedDataSet = await this.LocalStorage.GetAsync<List<VariablesData>>(nameof(this.DataSet));

            if (storedDataSet.Success && storedDataSet.Value is not null)
            {
                this.DataSet = storedDataSet.Value;
                restoredAnything = true;
                this.Logger.LogInformation("DataSet loaded successfully from local storage.");
            }
            else
            {
                this.Logger.LogWarning("No DataSet data found in local storage.");
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "An error occurred while loading data from local storage.");
            // Clear the local storage when an exception occurs
            this.ClearLocalStorage();
        }

        return restoredAnything;
    }
    /// <summary>
    /// Clears all data from local storage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private void ClearLocalStorage()
    {
        try
        {
            // Clear all data from local storage using JavaScript interop
            //TODO DELETE THE LOCAL STORAGE
            // await JSRuntime.InvokeAsync<object>("localStorage.clear", default);

            this.Logger.LogInformation("Local storage cleared successfully due to an error.");
        }
        catch (Exception ex)
        {
            // Log any errors that occur while clearing the local storage
            this.Logger.LogError(ex, "An error occurred while clearing local storage.");
        }
    }

    // Timer field removed - implementation is currently commented out

    /// <summary>
    /// Initializes the component asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override Task OnInitializedAsync()
    {
        //TODO ENABLE THIS FEATURE WHEN READY
        return Task.CompletedTask;

        // Code disabled for future implementation:
        // // Initialize the timer with a 2-second interval
        // _timer = new System.Timers.Timer(2000); // 2000 milliseconds = 2 seconds
        // _timer.Elapsed += async (sender, e) => await OnTimerElapsed();
        // _timer.AutoReset = false; // Ensures the timer runs only once
        // _timer.Start(); // Start the timer
    }

    /// <summary>
    /// Handles the timer elapsed event to refresh register data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OnTimerElapsed()
    {
        // Perform your method logic after the delay
        var result = await this.RegisterService.GetListOfAvailableRegisters(this.componentLifetimeCts.Token);
        if (result.IsSuccess && result.Value is not null)
        {
            this.VariablesList = result.Value.ToList();
        }
        await this.PopulateData(this.VariablesList);

        // Timer disposal removed - timer implementation is currently commented out
    }

    /// <summary>
    /// Handles changes to the selected variables.
    /// </summary>
    /// <param name="variables">The set of selected register records.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SelectedValuesChanged(HashSet<RegistersRecords> variables)
    {
        this.VariablesList = variables.ToList();
        await this.PopulateData(this.VariablesList);

    }

    /// <summary>
    /// Populates the data based on the selected variables.
    /// </summary>
    /// <param name="variables">The collection of variables to populate data for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task PopulateData(IEnumerable<RegistersRecords> variables)
    {
        // Fetching data from the database
        var result = await this.RegisterService.GetListRegisterTrends(variables, cancellationToken: this.componentLifetimeCts.Token);

        if (result.IsSuccess && result.Value is not null)
        {
            var timeSeriesData = result.Value;

            // Group by MachineId and Name, then order by TimeStamp and parse the values
            this.DataSet = this.TransformDictionaryToDataSet(timeSeriesData);
        }
    }

    /// <summary>
    /// Handles the deletion of a data series.
    /// </summary>
    /// <param name="series">The series to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task OnSeriesDeleted(VariablesData series)
    {
        var set = this.DataSet.FirstOrDefault(x =>
                                x.Name == series.Name
                                && x.MachineId == series.MachineId);
        if (set is not null)
            this.DataSet.Remove(set);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Parses a string value to a double based on the specified value type.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="valueType">The type of the value (int, double, etc.).</param>
    /// <returns>The parsed double value, or null if parsing fails so the caller can skip the point instead of plotting a fabricated 0.</returns>
    private double? ParseValue(string value, string? valueType)
    {
        try
        {
            return valueType switch
            {
                "int" => Convert.ToDouble(int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)),
                "double" => double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture),
                _ => double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture),
            };
        }
        catch (Exception e)
        {
            this.Logger.LogError(e, "Error parsing metric value '{Value}' as type '{ValueType}'; skipping data point", value, valueType);
        }
        return null;
    }

    /// <summary>
    /// Initializes the particles.js library for visual effects.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task InitializeParticles()
    {
        this.Logger.LogInformation("Starting Home page and particles");
        try
        {
            await this.JsRuntime.InvokeVoidAsync("particlesJS.load", "particles-js", "/particles.json");
            this.Logger.LogInformation("particlesJS.load Invoked");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error loading particles.js");
        }
    }



    /// <summary>
    /// Refreshes the available register values from the service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RefreshValues()
    {
        var result = await this.RegisterService.GetListOfAvailableRegisters(this.componentLifetimeCts.Token);
        if (result.IsSuccess && result.Value is not null)
        {
            this.VariablesList = result.Value.ToList();
        }

    }



    /// <summary>
    /// Disposes the component asynchronously, saving data to local storage.
    /// </summary>
    /// <returns>A value task representing the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        try
        {
            // #119 (F3): abort any in-flight register queries before persisting component state.
            await this.componentLifetimeCts.CancelAsync();

            // Save the VariablesList to local storage
            await this.LocalStorage.SetAsync(nameof(this.VariablesList), this.VariablesList);

            this.Logger.LogInformation("VariablesList saved to local storage.");

            // Save the DataSet to local storage so PopulateDataFromLocalStorage can restore it
            await this.LocalStorage.SetAsync(nameof(this.DataSet), this.DataSet);

            this.Logger.LogInformation("DataSet saved to local storage.");
        }
        catch (Exception ex)
        {
            // ProcessAsync any exceptions that occur during the save operation
            this.Logger.LogError(ex, "An error occurred while saving component state to local storage.");
        }
        finally
        {
            this.componentLifetimeCts.Dispose();
        }
    }


}
