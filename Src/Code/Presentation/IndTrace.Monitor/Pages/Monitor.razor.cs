// <copyright file="Monitor.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Configuration.Services;
using IndTrace.Application.UI.Services;
using Microsoft.JSInterop;


namespace IndTrace.Monitor.Pages;

/// <summary>
/// Represents the Monitor page component that observes and displays real-time system state changes.
/// </summary>
public partial class Monitor : IObserver<StateChange>
{
    private ApplicationConfiguration? applicationConfiguration;

    private IDisposable? unsubscriber;
    private bool firstRenderCompleted;

    // #120 F6b: coalesces per-hub-message notifications into a bounded render rate (trailing edge).
    private RenderThrottle? renderThrottle;

    /// <summary>
    /// Initializes the component asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        this.IsInitialized = await this.InitializeApplicationAsync();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the component has been initialized.
    /// </summary>
    public bool IsInitialized { get; set; }

    /// <summary>
    /// Called after the component has been rendered.
    /// </summary>
    /// <param name="firstRender">True if this is the first render of the component.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this.firstRenderCompleted = true;

            try
            {
                await this.InitializeParticles();
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning(ex, "Failed to initialize particles. Possibly a prerender timing issue.");
            }

            // Optionally trigger re-render if particles affect layout
            this.StateHasChanged();
        }
    }

    /// <summary>
    /// Initializes the application configuration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns true if initialization succeeded.</returns>
    private async Task<bool> InitializeApplicationAsync()
    {
        if (this.applicationConfiguration is not null) return true;
        var result = await this.IndTraceConfigurationService.GetConfigurationAsync();
        if (result.IsSuccess)
        {
            this.applicationConfiguration = result.Value;
        }
        else
        {
            // Surface the configuration load failure instead of swallowing it silently.
            this.Logger.LogError("Failed to load application configuration: {@Result}", result);
        }

        return this.applicationConfiguration is not null;
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
    /// Called when the component is initialized.
    /// </summary>
    protected override void OnInitialized()
    {
        // #120 F6b: bursts of hub messages yield at most one StateHasChanged per throttle window
        // per circuit, and the last event of a burst always produces a final (trailing) render.
        this.renderThrottle = new RenderThrottle(
            () => this.InvokeAsync(this.StateHasChanged),
            onError: ex => this.Logger.LogError(ex, "Throttled render failed on Monitor page."));

        // Subscribe to state changes
        this.unsubscriber = this.EventsService.Subscribe(this);
    }

    /// <summary>
    /// Called when the observable sequence completes.
    /// </summary>
    public void OnCompleted()
    {
        this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Called when an error occurs in the observable sequence.
    /// </summary>
    /// <param name="error">The exception that occurred.</param>
    public void OnError(Exception error)
    {
        this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Called when a new state change is observed.
    /// </summary>
    /// <param name="value">The state change value.</param>
    public void OnNext(StateChange value)
    {
        // #120 F6a: the station mutation that used to live here moved into IndTraceEventsService,
        // which applies each ARRIVED hub event to its station exactly once. Re-applying
        // MaxBy(TimeStamp) here ran once PER OPEN CIRCUIT against the shared singleton and lost
        // near-simultaneous updates for different machines. The page only coalesces renders now.
        this.renderThrottle?.Request();
    }

    /// <summary>
    /// Gets a value indicating whether the application has a valid configuration.
    /// </summary>
    private bool HasValidConfiguration =>
        this.applicationConfiguration != null &&
        this.applicationConfiguration.MachineNames != null &&
        this.applicationConfiguration.MachineNames.Count > 0;

    /// <summary>
    /// Gets a value indicating whether there are controller monitors available.
    /// </summary>
    private bool HasControllerMonitors =>
        this.EventsService != null &&
        this.EventsService.ControllerMonitors != null &&
        this.EventsService.ControllerMonitors.Count > 0;

    /// <summary>
    /// Gets a value indicating whether there are request events available.
    /// </summary>
    private bool HasRequestEvents =>
        this.EventsService != null &&
        this.EventsService.RequestEvents != null &&
        this.EventsService.RequestEvents.Count > 0;

    /// <summary>
    /// Gets a value indicating whether there are station monitors available.
    /// </summary>
    private bool HasStationMonitors =>
        this.EventsService != null &&
        this.EventsService.StationMonitors != null &&
        this.EventsService.StationMonitors.Count > 0;

    /// <summary>
    /// Gets a value indicating whether there are messages available.
    /// </summary>
    private bool HasMessages =>
        this.EventsService != null &&
        this.EventsService.Messages != null &&
        this.EventsService.Messages.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the component can render.
    /// </summary>
    private bool CanRender =>
        this.EventsService is not null && this.HasValidConfiguration && this.IsInitialized && this.firstRenderCompleted;

    /// <summary>
    /// Gets or sets the selected customer.
    /// </summary>
    public string SelectedCustomer { get; set; } = string.Empty;

    /// <summary>
    /// Disposes the component and unsubscribes from events.
    /// </summary>
    public void Dispose()
    {
        this.unsubscriber?.Dispose();

        // #120 F6b: cancel any pending trailing render so a disposed circuit is never re-rendered.
        this.renderThrottle?.Dispose();
    }

    /// <summary>
    /// Disposes the component asynchronously and unsubscribes from events.
    /// </summary>
    /// <returns>A value task representing the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.unsubscriber is IAsyncDisposable unsubscriberAsyncDisposable)
            await unsubscriberAsyncDisposable.DisposeAsync();
        else
            this.unsubscriber?.Dispose();

        // #120 F6b: cancel any pending trailing render so a disposed circuit is never re-rendered.
        this.renderThrottle?.Dispose();
    }

    /// <summary>
    /// Gets the URL for a customer's logo image.
    /// </summary>
    /// <param name="customerName">The name of the customer.</param>
    /// <returns>The URL path to the customer's logo image.</returns>
    private string GetCustomerLogoUrl(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            return string.Empty;
        }

        // Assume logos are stored under wwwroot/images/customers/
        return $"images/customers/{customerName.ToLowerInvariant()}.png";
    }
}

