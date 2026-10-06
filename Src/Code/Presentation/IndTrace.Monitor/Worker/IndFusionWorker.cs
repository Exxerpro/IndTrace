// <copyright file="IndFusionWorker.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.UI.Models.Performance;

namespace IndTrace.Monitor.Worker;

/// <summary>
/// Background service that manages OEE (Overall Equipment Effectiveness) data updates for IndTrace.
/// </summary>
/// <param name="logger">Logger instance for the worker.</param>
/// <param name="oeeState">The OEE state manager for tracking machine performance.</param>
public class IndFusionWorker(ILogger<IndFusionWorker> logger, OeeState oeeState) : BackgroundService
{
    /// <summary>
    /// Executes the background service, periodically updating OEE data every 5 minutes.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker starting at {Time}", DateTime.Now.ToLocalTime());

        foreach (var machineOee in oeeState.Machines)
        {
            logger.LogInformation(" Machine updated at {Time} {Machine}", DateTime.Now.ToLocalTime(), machineOee.ToString());
        }
        while (!stoppingToken.IsCancellationRequested)
        {
            this.UpdateOeeData();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Update every 5 minutes
        }
    }

    /// <summary>
    /// Updates the OEE data for all machines and logs the changes.
    /// </summary>
    private void UpdateOeeData()
    {
        oeeState.GenerateNewData();
        logger.LogInformation("OEE data updated at {Time}", DateTime.Now.ToLocalTime());
        foreach (var machineOee in oeeState.Machines)
        {
            logger.LogInformation(" Machine updated at {Time} {Machine}", DateTime.Now.ToLocalTime(), machineOee.ToString());
        }
    }
}