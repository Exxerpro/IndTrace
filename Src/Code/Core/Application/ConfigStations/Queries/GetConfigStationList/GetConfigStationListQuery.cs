// <copyright file="GetConfigStationListQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Queries.GetConfigStationList;

/// <summary>
/// Represents the GetConfigStationListQuery.
/// </summary>
public class GetConfigStationListQuery : IMonitorRequest<ApplicationConfiguration>
{
    // [Fix]
    // CLAUDE
    // Date: 21/08/2025
    // Reason: Changed PartNumber to nullable string to allow distinction between null and empty string for proper validation
    private string? partNumber = null;

    /// <summary>
    /// Gets or sets the PartNumber.
    /// </summary>
    public string? PartNumber
    {
        get => this.partNumber;
        set => this.partNumber = value;
    }
}