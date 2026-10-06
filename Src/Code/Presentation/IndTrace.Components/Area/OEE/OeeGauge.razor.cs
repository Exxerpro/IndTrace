// <copyright file="OeeGauge.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace IndTrace.Components.Area.OEE
{
    /// <summary>
    /// A gauge component for displaying OEE (Overall Equipment Effectiveness) values with visual indicators.
    /// </summary>
    public partial class OeeGauge
    {
        /// <summary>
        /// Gets or sets the value to display on the gauge (0-100).
        /// </summary>
        [Parameter]
        public double Value { get; set; }
        /// <summary>
        /// Gets or sets the theme for the gauge appearance.
        /// </summary>
        [Parameter]
        public string Theme { get; set; } = "dark";

        /// <summary>
        /// Gets the color for the gauge based on the current value.
        /// </summary>
        private string GaugeColor => this.Value switch
        {
            < 50 => "red",
            < 75 => "orange",
            _ => "green",
        };

        /// <summary>
        /// Gets the theme-specific color for the gauge.
        /// </summary>
        /// <returns>The color string for the current theme.</returns>
        private string GetThemeColor() => this.Theme switch
        {
            "dark" => "limegreen",
            "light" => "green",
            "blue" => "#00cfff",
            _ => "gray",
        };

        /// <summary>
        /// Gets the X coordinate for the needle tip.
        /// </summary>
        private double NeedleX => 100 + (90 * Math.Cos(this.NeedleAngle * Math.PI / 180));
        /// <summary>
        /// Gets the Y coordinate for the needle tip.
        /// </summary>
        private double NeedleY => 100 + (90 * Math.Sin(this.NeedleAngle * Math.PI / 180));

        /// <summary>
        /// Gets the needle tip X coordinate formatted invariantly for use in the SVG markup
        /// (a comma decimal separator would corrupt the coordinate on a comma-decimal host).
        /// </summary>
        private string NeedleXText => this.NeedleX.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the needle tip Y coordinate formatted invariantly for use in the SVG markup
        /// (a comma decimal separator would corrupt the coordinate on a comma-decimal host).
        /// </summary>
        private string NeedleYText => this.NeedleY.ToString(CultureInfo.InvariantCulture);
        /// <summary>
        /// Gets the SVG path string for the value arc.
        /// </summary>
        private string ValueArc => this.DescribeArc(100, 100, 90, 180, 180 + (this.SafeValue / 100.0 * 180));

        /// <summary>
        /// Gets the value clamped between 0 and 100.
        /// </summary>
        private double SafeValue => Math.Clamp(this.Value, 0, 100);
        /// <summary>
        /// Gets the angle for the needle position in degrees.
        /// </summary>
        private double NeedleAngle => 180 + (this.SafeValue / 100.0 * 180);

        /// <summary>
        /// Creates an SVG path string for drawing an arc.
        /// </summary>
        /// <param name="cx">Center X coordinate.</param>
        /// <param name="cy">Center Y coordinate.</param>
        /// <param name="r">Radius of the arc.</param>
        /// <param name="startAngle">Start angle in degrees.</param>
        /// <param name="endAngle">End angle in degrees.</param>
        /// <returns>SVG path string for the arc.</returns>
        private string DescribeArc(double cx, double cy, double r, double startAngle, double endAngle)
        {
            var startX = cx + (r * Math.Cos(startAngle * Math.PI / 180));
            var startY = cy + (r * Math.Sin(startAngle * Math.PI / 180));
            var endX = cx + (r * Math.Cos(endAngle * Math.PI / 180));
            var endY = cy + (r * Math.Sin(endAngle * Math.PI / 180));
            var largeArc = (endAngle - startAngle) > 180 ? 1 : 0;
            return FormattableString.Invariant($"M{startX},{startY} A{r},{r} 0 {largeArc},1 {endX},{endY}");
        }
    }
}