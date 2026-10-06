// <copyright file="Program.DashboardEndpoints.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Hub.Server;

using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Dashboard;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Minimal API endpoints exposing hub metrics for dashboards.
/// </summary>
public static class ProgramDashboardEndpoints
{
    /// <summary>
    /// Maps dashboard endpoints under /api/hub/metrics.
    /// TODO: Implement IHubMetricsDashboard interface before enabling these endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void MapHubMetricsEndpoints(this WebApplication app)
    {
        // TODO: Implement IHubMetricsDashboard interface
        // app.MapGet("/api/hub/metrics/aggregated", async (IHubMetricsDashboard dashboard, CancellationToken ct) =>
        // {
        //     var result = await dashboard.GetAggregatedMetricsAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
        //     return Results.Ok(result);
        // });

        // app.MapGet("/api/hub/metrics/health", async (IHubMetricsDashboard dashboard, CancellationToken ct) =>
        // {
        //     var result = await dashboard.GetHealthStatusAsync(ct).ConfigureAwait(false);
        //     return Results.Ok(result);
        // });

        // app.MapGet("/api/hub/metrics/top", async (int? limit, IHubMetricsDashboard dashboard, CancellationToken ct) =>
        // {
        //     var result = await dashboard.GetTopConnectionsByActivityAsync(limit ?? 10, ct).ConfigureAwait(false);
        //     return Results.Ok(result);
        // });

        // app.MapGet("/api/hub/metrics/performance", async (IHubMetricsDashboard dashboard, CancellationToken ct) =>
        // {
        //     var result = await dashboard.GetSystemPerformanceAsync(ct).ConfigureAwait(false);
        //     return Results.Ok(result);
        // });

        // app.MapGet("/api/hub/metrics/trend", async (int seconds, IHubMetricsDashboard dashboard, CancellationToken ct) =>
        // {
        //     var period = TimeSpan.FromSeconds(Math.Max(1, seconds));
        //     var result = await dashboard.GetTrendDataAsync(period, ct).ConfigureAwait(false);
        //     return Results.Ok(result);
        // });
    }
}


