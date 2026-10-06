// <copyright file="RestoreBarCodeCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Restore;

/// <summary>
/// Represents the RestoreBarCodeCommand.
/// </summary>
public class RestoreBarCodeCommand : IMonitorRequest<BarCodeRestoredView>
{
    /// <summary>
    /// Gets or sets the Label.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreBarCodeCommand"/> class.
    /// </summary>
    public RestoreBarCodeCommand()
    {
        this.Label = string.Empty;
    }
}