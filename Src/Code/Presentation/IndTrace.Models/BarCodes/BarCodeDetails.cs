// <copyright file="BarCodeDetails.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.BarCodes;

using IndTrace.Application.BarCodes.Queries.GetBarCodeDetailMonitor;
using IndTrace.Application.BarCodes.Queries.GetBarCodeDetailQrCode;
using IndTrace.Domain.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides services for retrieving and managing barcode details from the command dispatcher.
/// </summary>
public class BarCodeDetails(IMonitorRequestDispatcher monitorRequestDispatcher, ILogger<BarCodeDetails> logger)
{
    private readonly IMonitorRequestDispatcher monitorRequestDispatcher = monitorRequestDispatcher ?? throw new ArgumentNullException(nameof(monitorRequestDispatcher), "_monitorRequestDispatcher cannot be null");
    private readonly ILogger<BarCodeDetails> logger = logger ?? throw new ArgumentNullException(nameof(logger), "_monitorRequestDispatcher cannot be null");

    /// <summary>
    /// Retrieves detailed information for a specific barcode.
    /// </summary>
    /// <param name="barCode">The barcode string to retrieve details for.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the barcode detail view model.</returns>
    /// <exception cref="Exception">Thrown when the response from the command dispatcher is null.</exception>
    public async Task<BarCodeDetailMonitorVm> GetBarCodeDetail(string barCode, CancellationToken cancellationToken)
    {
        var command = new GetBarCodeDetailQrCodeQuery
        {
            BarCode = barCode,
        };

        var result = await this.monitorRequestDispatcher.ProcessAsync(command, cancellationToken);
        if (result is null)
        {
            this.logger.LogError("Response to GetReadBarCode was null for barcode {BarCode}", barCode);
            throw new Exception("Response to GetReadBarCode was null");
        }
        if (result.Value is null)
        {
            this.logger.LogError("Result value was null for barcode {BarCode}", barCode);
            throw new Exception("Result value was null");
        }

        return result.Value;
    }
}
