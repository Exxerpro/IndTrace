// <copyright file="VariablesData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Metrics;

using IndTrace.Application.Registers.Services;
using MudBlazor;

/// <summary>
/// Represents variable data for machine monitoring and metrics collection.
/// </summary>
public class VariablesData
{
    /// <summary>
    /// Gets or sets the register record associated with this variable data.
    /// </summary>
    public RegistersRecords Register { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the variable.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the machine identifier associated with this variable.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the type of value stored in this variable.
    /// </summary>
    public string ValueType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unit of measurement for this variable.
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for this variable.
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// Gets or sets the array of data values for this variable.
    /// </summary>
    public double[] Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the color used for displaying this variable in charts or graphs.
    /// </summary>
    public Color Color { get; set; } = Color.Default;

    /// <summary>
    /// Gets or sets the timestamp when this variable data was recorded.
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.MinValue;
}
