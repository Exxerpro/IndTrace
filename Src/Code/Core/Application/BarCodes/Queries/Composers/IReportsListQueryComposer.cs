// <copyright file="IReportsListQueryComposer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.GetReportsList.GetList;
using IndTrace.Domain.Entities;

namespace IndTrace.Application.BarCodes.Queries.Composers;

/// <summary>
/// Composes filtered IQueryable for reports list with industrial validation patterns.
/// Extracted from GetReportsListMonitorQueryHandler to follow SRP principles.
/// Implements CLAUDE.md compliance with Result pattern and null safety.
/// </summary>
public interface IReportsListQueryComposer
{
    /// <summary>
    /// Composes filtered barcode query based on request criteria.
    /// Applies all filters including master labels, date ranges, products, customers, states, shifts, and lines.
    /// </summary>
    /// <param name="baseQuery">Base barcode queryable.</param>
    /// <param name="request">Filter request with validated parameters.</param>
    /// <param name="supportQueries">Supporting entity queryables.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing composed query or failure reasons.</returns>
    Task<Result<IQueryable<BarCode>>> ComposeAsync(
        IQueryable<BarCode> baseQuery,
        GetReportsListQuery request,
        ReportsSupportQueries supportQueries,
        CancellationToken cancellationToken);
}

/// <summary>
/// Record containing supporting entity queryables for reports list composition.
/// Provides structured access to all related entity data needed for filtering.
/// </summary>
/// <param name="MasterLabels">Queryable for master label filtering.</param>
/// <param name="Customers">Queryable for customer-based product filtering.</param>
/// <param name="Products">Queryable for product filtering.</param>
/// <param name="Lines">Queryable for line filtering.</param>
public record ReportsSupportQueries(
    IQueryable<MasterLabel> MasterLabels,
    IQueryable<Customer> Customers,
    IQueryable<Product> Products,
    IQueryable<Line> Lines);