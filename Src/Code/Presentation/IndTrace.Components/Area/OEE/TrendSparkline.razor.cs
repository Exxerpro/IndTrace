// <copyright file="TrendSparkline.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace IndTrace.Components.Area.OEE;

/// <summary>
/// A sparkline component for displaying trend data in a compact format.
/// </summary>
public partial class TrendSparkline
{
    /// <summary>
    /// Gets or sets the data points to display in the sparkline.
    /// </summary>
    [Parameter]
    public List<double> Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the theme for the sparkline appearance.
    /// </summary>
    [Parameter]
    public string Theme { get; set; } = "dark";

    /// <summary>
    /// Generates the SVG points for the sparkline polyline.
    /// </summary>
    /// <returns>An enumerable of point coordinates as strings.</returns>
    private IEnumerable<string> GetPoints()
    {
        if (this.Data == null || this.Data.Count < 2)
            yield break;

        var max = this.Data.Max();
        var min = this.Data.Min();
        var range = max - min;

        // If all values are equal or nearly equal, add a small artificial range
        if (range < 0.01)
        {
            range = 1;
            min = this.Data.Min() - 0.5;
        }

        for (var i = 0; i < this.Data.Count; i++)
        {
            var x = (100.0 / (this.Data.Count - 1)) * i;
            var normalizedY = (this.Data[i] - min) / range;
            var y = 25 - (normalizedY * 25);  // SVG y-axis goes down
            yield return FormattableString.Invariant($"{x:F2},{y:F2}");
        }
    }

    /// <summary>
    /// Gets the stroke color based on the current theme.
    /// </summary>
    /// <returns>The stroke color as a hex string.</returns>
    private string GetStroke() => this.Theme switch
    {
        "dark" => "#55aaff",
        "light" => "#003366",
        "blue" => "#00ccff",
        _ => "#888",
    };
}