// <copyright file="GetAppDetailsGuiRequestHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Configuration.Services;

using IndTrace.Application.Models.CacheServices;

/// <summary>
/// Represents the GetAppDetailsMonitorRequestHandler.
/// </summary>
public class GetAppDetailsMonitorRequestHandler(
    CacheManager<ApplicationConfiguration> cacheManager,
    AppDetailsFactory appDetailsFactory,
    ILogger<GetAppDetailsMonitorRequestHandler> logger)
    : IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>
{

    /// <inheritdoc/>
    /// <remarks>
    /// #116 fail-loud: the factory now returns a Result and the CacheManager Result overload refuses to
    /// cache failures, so a repository outage surfaces to the caller as a failed Result instead of an
    /// empty-but-successful configuration cached for the whole cache duration.
    /// </remarks>
    public async Task<Result<ApplicationConfiguration>> ProcessAsync(GetAppDetailsMonitorRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var appDetails = await cacheManager.
                GetOrRefreshAsync(
                    () => appDetailsFactory.CreateAppDetailsAsync(cancellationToken),
                    request.Refresh, cancellationToken, logger).ConfigureAwait(false);

            return appDetails;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching AppDetails.");
            return Result<ApplicationConfiguration>.WithFailure("An error occurred while fetching AppDetails.");
        }
    }
}