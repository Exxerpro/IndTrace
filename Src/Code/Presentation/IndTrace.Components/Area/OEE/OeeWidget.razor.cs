// <copyright file="OeeWidget.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace IndTrace.Components.Area.OEE;

/// <summary>
/// A widget component for displaying OEE (Overall Equipment Effectiveness) data with machine information and trend data.
/// </summary>
public partial class OeeWidget
{
    /// <summary>
    /// Gets or sets the OEE value to display.
    /// </summary>
    [Parameter]
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the historical data points for trend visualization.
    /// </summary>
    [Parameter]
    public List<double> Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the machine data to display in the widget.
    /// </summary>
    [Parameter]
    public MachineWidgetData MachineData { get; set; } = new();

    /// <summary>
    /// Gets or sets the theme for the widget appearance.
    /// </summary>
    [Parameter]
    public string Theme { get; set; } = "dark";
}