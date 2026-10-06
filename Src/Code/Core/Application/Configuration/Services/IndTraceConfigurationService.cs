// <copyright file="IndTraceConfigurationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Configuration.Services;

/// <summary>
/// Provides methods to retrieve and manage the IndTrace application configuration.
/// </summary>
/// <remarks>
/// <para>
/// Issue #221: this service is a thin delegating facade over the app-details Monitor handler. It used to keep
/// its own <c>ApplicationConfiguration</c> field as a singleton cache with no expiry, layered on top of the
/// <c>CacheManager&lt;ApplicationConfiguration&gt;</c> (60-minute TTL) inside the handler — once populated,
/// consumers never saw a config refresh unless they passed <c>refresh = true</c>, so the TTL beneath was dead
/// from their perspective. The field cache (and the semaphore that guarded its writes) was removed: every call
/// delegates to the handler, and the shared <c>CacheManager</c> beneath owns caching and TTL.
/// </para>
/// <para>
/// Issue #88 (bounded-context bleed, still load-bearing): the service depends on the concrete
/// <see cref="IMonitorRequestHandler{TCommand, TResponse}"/> for <see cref="GetAppDetailsMonitorRequest"/>
/// directly — not the UI/Admin <c>IMonitorRequestDispatcher</c> — so the PLC gateway configuration path does
/// not route through, or require, the UI dispatcher.
/// </para>
/// </remarks>
public class IndTraceConfigurationService(
    IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration> appDetailsHandler)
{
    private readonly IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration> appDetailsHandler = appDetailsHandler;

    /// <summary>
    /// Asynchronously retrieves the application configuration, optionally forcing a refresh.
    /// </summary>
    /// <param name="refresh">If true, forces a refresh of the configuration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the application configuration.</returns>
    public async Task<Result<ApplicationConfiguration>> GetConfigurationAsync(bool refresh = false, CancellationToken cancellationToken = default)
    {
        if (this.appDetailsHandler is null)
        {
            return Result<ApplicationConfiguration>.WithFailure(["appDetailsHandler cannot be null."]);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ApplicationConfiguration>.WithFailure(["Operation was canceled."]);
        }

        var request = new GetAppDetailsMonitorRequest(refresh);

        return await this.appDetailsHandler.ProcessAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
