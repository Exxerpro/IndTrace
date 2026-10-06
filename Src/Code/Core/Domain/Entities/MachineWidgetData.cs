// <copyright file="MachineWidgetData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents widget data for a machine, including OEE and related KPIs for dashboard display.
/// </summary>
/// <remarks>
/// Set by EF or by builder on runtime, consumer must check for null before accessing.
/// This entity is safe for production use and should not contain simulation data.
/// </remarks>
public class MachineWidgetData : IEntityRoot
{
    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    /// <remarks>
    /// Set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </remarks>
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the machine status (RUNNING, STOPPED, etc.).
    /// </summary>
    /// <remarks>
    /// Set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </remarks>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Overall Equipment Effectiveness percentage.
    /// </summary>
    public double OEE { get; set; }

    /// <summary>
    /// Gets or sets the availability percentage.
    /// </summary>
    public double Availability { get; set; }

    /// <summary>
    /// Gets or sets the performance percentage.
    /// </summary>
    public double Performance { get; set; }

    /// <summary>
    /// Gets or sets the quality percentage.
    /// </summary>
    public double Quality { get; set; }

    /// <summary>
    /// Gets or sets the OEE trend data for sparkline display.
    /// </summary>
    /// <remarks>
    /// Set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </remarks>
    public List<double> OeeTrend { get; set; } = [];

    /// <summary>
    /// Returns a string representation of the machine widget data.
    /// </summary>
    /// <returns>A string containing the machine ID, status, and OEE percentage.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"Widget {this.MachineId}: {this.Status} (OEE: {this.OEE:P1})";
}
