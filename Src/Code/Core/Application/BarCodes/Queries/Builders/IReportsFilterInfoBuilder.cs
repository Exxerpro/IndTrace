// <copyright file="IReportsFilterInfoBuilder.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.GetReportsList.FiltersInfo;

namespace IndTrace.Application.BarCodes.Queries.Builders;

/// <summary>
/// Builds filter information for reports using parallel data loading.
/// Extracted from GetReportsFilterInfoMonitorQueryHandler to follow SRP principles.
/// Implements CLAUDE.md compliance with Result pattern and null safety.
/// </summary>
public interface IReportsFilterInfoBuilder
{
    /// <summary>
    /// Builds comprehensive filter information for reports.
    /// Uses parallel loading for optimal performance across all filter categories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing filter info view model or failure reasons.</returns>
    Task<Result<ReportsFilterInfoVm>> BuildAsync(CancellationToken cancellationToken);
}