// <copyright file="Machines.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Machines.Queries.GetMachinesList;
using IndTrace.Domain.Enum;
using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;

namespace IndTrace.Monitor.Pages;

/// <summary>
/// Represents the Machines page component that displays and manages machine configurations.
/// </summary>
public partial class Machines
{

    /// <summary>
    /// Gets or sets the error content to display when errors occur.
    /// </summary>
    [Parameter]
    public RenderFragment? ErrorContent { get; set; }

    /// <summary>
    /// Gets or sets the list of machines to display.
    /// </summary>
    [Parameter]
    public IEnumerable<MachineDto> MachineList { get; set; } = [];

    /// <summary>
    /// Gets or sets the application configuration containing machine settings.
    /// </summary>
    [Parameter]
    public ApplicationConfiguration? ApplicationConfiguration { get; set; }

    private string message = "loading";

    /// <summary>
    /// Initializes the component asynchronously by loading configuration data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        await this.LoadConfiguration();

        if (this.ApplicationConfiguration is null)
        {
            this.message = "Error loading configuration details.";
        }
        else
        {
            // Check if there are multiple initial printers or if there are multiple clients
            if (this.ApplicationConfiguration.LineConfiguration.HasMultipleInitialPrinters || this.ApplicationConfiguration.LineConfiguration.HasMultipleClients)
            {
                // Subscribe to the events
            }
        }
    }

    private IEnumerable<MachineDto> initialMachines = Enumerable.Empty<MachineDto>();

    /// <summary>
    /// Loads the application configuration and initializes machine data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadConfiguration()
    {
        var result = await this.IndTraceConfigurationService.GetConfigurationAsync();

        if (result is { IsSuccess: true, Value: not null })
        {
            this.ApplicationConfiguration = result.Value;
            this.message = "Getting configuration details";

            var machines = this.ApplicationConfiguration.Machines ?? Enumerable.Empty<MachineDto>();
            this.initialMachines = machines.Where(m => Equals(m.MachineType, MachineType.InitialPrinter));
            this.message = this.initialMachines.Count() > 1 ? "Initial machines found" : "No initial machines found";
        }
        else
        {
            this.message = "Error loading configuration details.";
        }
    }

    /// <summary>
    /// Handles machine events by reloading configuration and updating the UI.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OnMachineEvent()
    {
        await this.LoadConfiguration();
        await this.InvokeAsync(this.StateHasChanged);
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
            this.MachineList = this.ApplicationConfiguration?.Machines ?? Enumerable.Empty<MachineDto>();
            // Now it's safe to call JavaScript, focus elements, etc.
            // await JS.InvokeVoidAsync("someJsFunction");

            await this.InvokeAsync(this.StateHasChanged);
        }
    }
}

