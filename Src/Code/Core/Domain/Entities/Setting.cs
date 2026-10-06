// <copyright file="Setting.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a configuration setting entity.
/// </summary>
public class Setting : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Setting"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public Setting()
    {
        this.Config = string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the setting.
    /// </summary>
    public int SettingId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier this setting applies to. Story 35.D2 Cluster 5 (#35): retyped to the
    /// strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>).
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the configuration value or content.
    /// </summary>
    public string Config { get; set; }

    /// <summary>
    /// Returns a string representation of the Setting.
    /// </summary>
    /// <returns>A string containing the setting ID and machine ID.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"Setting {this.SettingId} (Machine {this.MachineId.Value})";
}