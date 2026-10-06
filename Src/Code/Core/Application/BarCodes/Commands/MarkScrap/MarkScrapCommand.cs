// <copyright file="MarkScrapCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.MarkScrap;

/// <summary>
/// WEBAPP/monitor command (Story 5.3) that scraps a barcode's part via the config-gated
/// <see cref="IndTrace.Domain.Enum.GatewayTask.MarkScrap"/> completeness trigger (default OFF).
/// </summary>
public class MarkScrapCommand : IMonitorRequest<BarCodeMarkedScrapView>
{
    /// <summary>
    /// Gets or sets the Label.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkScrapCommand"/> class.
    /// </summary>
    public MarkScrapCommand()
    {
        this.Label = string.Empty;
    }
}
