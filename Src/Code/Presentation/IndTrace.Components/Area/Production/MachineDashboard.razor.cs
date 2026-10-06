// <copyright file="MachineDashboard.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace IndTrace.Components.Area.Production;

/// <summary>
/// Machine Dashboard component for displaying OEE metrics across multiple machines.
/// </summary>
/// <remarks>
/// Set by EF or by builder on runtime, consumer must check for null before accessing.
/// </remarks>
public partial class MachineDashboard
{
    private MudTheme theme = new();
    private bool isDarkMode;

    /// <summary>
    /// Gets or sets the callback for theme change events.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnThemeChanged { get; set; }


    /// <summary>
    /// Handles theme change events and updates the MudBlazor theme accordingly.
    /// </summary>
    /// <param name="theme">The theme name to apply.</param>
    private void HandleThemeChange(string theme)
    {
        this.isDarkMode = theme == "dark";

        this.theme = new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Background = "#f4f4f4",
                Primary = "#003366",
                AppbarBackground = "#e0e0e0",
            },
            PaletteDark = new PaletteDark
            {
                Background = "#2a2a35",
                Primary = "#00ff99",
                AppbarBackground = "#1f1f1f"
            },
        };

        // Optionally customize the blue variant via dark/light toggle
        if (theme == "blue")
        {
            this.theme.PaletteDark.Primary = "#00cfff";
            this.theme.PaletteDark.Background = "#001f3f";
        }
    }

    /// <summary>
    /// Gets or sets the selected theme for the dashboard.
    /// </summary>
    public string SelectedTheme { get; set; } = "dark";

    /// <summary>
    /// Gets or sets the collection of machines to display in the dashboard.
    /// </summary>
    /// <remarks>
    /// DEMO/PLACEHOLDER DATA. These are hard-coded sample machines (MC-101/202/303) with fabricated
    /// OEE figures — the dashboard is not yet wired to a live OEE data source. The page shows a
    /// "Demo data" banner so operators do not mistake these for real production values. Replace this
    /// seed with a real data feed when the OEE source is available.
    /// </remarks>
    public List<MachineWidgetData> Machines { get; set; } =
    [
        new MachineWidgetData
        {
            MachineId = "MC-101",
            Status = "Running",
            OEE = 82.4,
            Availability = 90.0,
            Performance = 85.0,
            Quality = 95.0,
            OeeTrend = [78, 79, 81, 82, 84, 83, 82],
        },

        new MachineWidgetData
        {
            MachineId = "MC-202",
            Status = "Stopped",
            OEE = 45.0,
            Availability = 50.0,
            Performance = 48.0,
            Quality = 90.0,
            OeeTrend = [50, 48, 47, 46, 45, 44, 43],
        },

        new MachineWidgetData
        {
            MachineId = "MC-303",
            Status = "Running",
            OEE = 68.3,
            Availability = 70.0,
            Performance = 65.0,
            Quality = 90.0,
            OeeTrend = [65, 66, 67, 68, 69, 68, 68],
        }
    ];
}