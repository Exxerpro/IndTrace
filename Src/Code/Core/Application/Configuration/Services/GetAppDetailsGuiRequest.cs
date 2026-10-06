// <copyright file="GetAppDetailsGuiRequest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Configuration.Services;

/// <summary>
/// Represents a request to get application details for the GUI, with an option to refresh.
/// </summary>
/// <summary>
/// Represents the GetAppDetailsMonitorRequest.
/// </summary>
public class GetAppDetailsMonitorRequest(bool refresh = false) : IMonitorRequest<ApplicationConfiguration>
{
    /// <summary>
    /// Gets a value indicating whether to refresh the application details.
    /// </summary>
    public bool Refresh { get; } = refresh;
}