// <copyright file="IProductRoutingVersionProbe.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// #224: the cross-process staleness seam for the per-product routing-graph cache. Returns the product's
/// current <b>routing version</b> — a monotonic scalar derived from the database's own change stamps (the max
/// SQL Server <c>rowversion</c> across the product's <c>RoutingNodes</c> and <c>WorkFlows</c> rows) — so a
/// cached <see cref="ProductionGraphCacheEntry"/> stamped with the version its rows were read at can be
/// validated for freshness with one cheap indexed read instead of trusting a per-process, no-TTL cache.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IProductionGraphCache"/> is a per-process singleton: before #224, a whole-route replace
/// committed by one host (routing authored in Monitor) invalidated only THAT host's cache, and every other
/// host (notably the Communications PLC gateway) kept validating arrivals against the pre-edit graph until
/// restart. Because SQL Server bumps <c>rowversion</c> on every insert/update database-wide, ANY committed
/// routing edit — from any process — changes the product's max rowversion, so a version mismatch is a
/// reliable cross-process staleness signal.
/// </para>
/// <para>
/// Providers without real rowversions (the EF InMemory test provider leaves the arrays empty) yield a
/// constant version of 0 for every probe, which makes every cached entry permanently "fresh" — exactly the
/// pre-#224 single-process cache behaviour the test beds rely on.
/// </para>
/// </remarks>
public interface IProductRoutingVersionProbe
{
    /// <summary>
    /// Returns the product's current routing version: the maximum <c>rowversion</c> (interpreted as a
    /// big-endian <see cref="ulong"/>) across the product's <c>RoutingNodes</c> and <c>WorkFlows</c> rows,
    /// or <c>0</c> when the product has no routing rows (or the provider carries no real rowversions).
    /// </summary>
    /// <param name="productId">The product whose routing version is probed.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the current version, or a failure when the probe could not
    /// read the rows — callers MUST treat a failure as "freshness unknown" (bypass the cache and rebuild;
    /// never serve or populate a cached entry on a failed probe). Never throws across the boundary.
    /// </returns>
    Task<Result<ulong>> GetVersionAsync(int productId, CancellationToken cancellationToken);
}
