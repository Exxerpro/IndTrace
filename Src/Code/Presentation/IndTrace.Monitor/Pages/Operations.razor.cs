// <copyright file="Operations.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.UI.Models;
using IndTrace.Dependencies;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Diagnostics;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using Google.Protobuf;
using IndTrace.Application.UI.Services;
using MudBlazor;

namespace IndTrace.Monitor.Pages;

/// <summary>
/// Represents the Operations page component that observes and displays operational state changes.
/// </summary>
public partial class Operations : IObserver<StateChange>
{
    private ApplicationConfiguration? applicationConfiguration;

    private IDisposable? unsubscriber;

    // #120 F6b: coalesces per-hub-message notifications into a bounded render rate (trailing edge).
    private RenderThrottle? renderThrottle;

    /// <summary>
    /// Gets or sets a value indicating whether the component has been initialized.
    /// </summary>
    public bool IsInitialized { get; set; }

    /// <summary>
    /// Initializes the component asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        var initialized = await this.InitializeApplicationAsync();

        if (this.applicationConfiguration is not null)
        {
            this.EventsService.ApplyConfiguration(this.applicationConfiguration);
        }

        this.IsInitialized = initialized;
    }

    /// <summary>
    /// Called after the component has been rendered.
    /// </summary>
    /// <param name="firstRender">True if this is the first render of the component.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await this.InitializeParticles();

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
            // Error loading configuration details - handled by ApplicationConfiguration null check
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
            onError: ex => this.Logger.LogError(ex, "Throttled render failed on Operations page."));

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
}
