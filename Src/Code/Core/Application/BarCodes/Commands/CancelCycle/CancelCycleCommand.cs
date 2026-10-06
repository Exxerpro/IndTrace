// <copyright file="CancelCycleCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.CancelCycle;

/// <summary>
/// WEBAPP/monitor command (Story 5.3) that cancels a barcode's latest (Started) cycle via the config-gated
/// <see cref="IndTrace.Domain.Enum.GatewayTask.Cancel"/> completeness trigger (default OFF).
/// </summary>
public class CancelCycleCommand : IMonitorRequest<CycleCanceledView>
{
    /// <summary>
    /// Gets or sets the Label.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelCycleCommand"/> class.
    /// </summary>
    public CancelCycleCommand()
    {
        this.Label = string.Empty;
    }
}
