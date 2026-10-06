// <copyright file="MarkInvalidCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.MarkInvalid;

/// <summary>
/// WEBAPP/monitor command (Story 5.3) that marks a barcode <c>Invalid</c> via the config-gated
/// <see cref="IndTrace.Domain.Enum.GatewayTask.MarkInvalid"/> completeness trigger (default OFF).
/// </summary>
public class MarkInvalidCommand : IMonitorRequest<BarCodeMarkedInvalidView>
{
    /// <summary>
    /// Gets or sets the Label.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkInvalidCommand"/> class.
    /// </summary>
    public MarkInvalidCommand()
    {
        this.Label = string.Empty;
    }
}
