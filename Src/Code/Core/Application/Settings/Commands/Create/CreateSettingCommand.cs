// <copyright file="CreateSettingCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Settings.Commands.Create;

/// <summary>
/// Represents the CreateSettingCommand.
/// </summary>
public class CreateSettingCommand : IMonitorRequest<SettingCreatedEvent>
{
    /// <summary>
    /// Gets or sets the SettingId.
    /// </summary>
    public int SettingId { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    public string Setting { get; set; } = string.Empty;
}