// <copyright file="KpiOeeRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for KpiOee entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// </summary>
internal static class KpiOeeRawData
{
    /// <summary>
    /// KpiOee test data - Manufacturing KPI OEE metrics
    /// </summary>
    private static readonly ImmutableDictionary<int, KpiOee> _kpiOeesDict =
        new Dictionary<int, KpiOee>
        {
            // Navigation property (OeeRegister) is left at its default (null!) by CreateFixture and set by EF Core.
            [1] = KpiOee.CreateFixture(
                kpiOeeId: 1,
                oeeRegisterId: 1,
                oee: Math.Round(0.85, 6), // Sanitized to 6 decimal places
                availability: Math.Round(0.90, 6),
                performance: Math.Round(0.94, 6),
                quality: Math.Round(0.95, 6),
                timeStamp: DateTime.UtcNow.AddHours(-1)),
            [2] = KpiOee.CreateFixture(
                kpiOeeId: 2,
                oeeRegisterId: 2,
                oee: Math.Round(0.82, 6),
                availability: Math.Round(0.90, 6),
                performance: Math.Round(1.0, 6), // Clamped to max 1.0 for KPI
                quality: Math.Round(0.94, 6),
                timeStamp: DateTime.UtcNow.AddHours(-2)),
            [3] = KpiOee.CreateFixture(
                kpiOeeId: 3,
                oeeRegisterId: 3,
                oee: Math.Round(0.88, 6),
                availability: Math.Round(0.90, 6),
                performance: Math.Round(0.99, 6),
                quality: Math.Round(0.96, 6),
                timeStamp: DateTime.UtcNow.AddMinutes(-30)),
            [4] = KpiOee.CreateFixture(
                kpiOeeId: 4,
                oeeRegisterId: 4,
                oee: Math.Round(0.92, 6),
                availability: Math.Round(0.90, 6),
                performance: Math.Round(1.0, 6), // Clamped to max 1.0 for KPI
                quality: Math.Round(0.96, 6),
                timeStamp: DateTime.UtcNow.AddMinutes(-15)),
            [5] = KpiOee.CreateFixture(
                kpiOeeId: 5,
                oeeRegisterId: 5,
                oee: Math.Round(0.95, 6),
                availability: Math.Round(0.90, 6),
                performance: Math.Round(1.0, 6), // Clamped to max 1.0 for KPI
                quality: Math.Round(1.0, 6),
                timeStamp: DateTime.UtcNow.AddMinutes(-5)),
            [6] = KpiOee.CreateFixture(
                kpiOeeId: 6,
                oeeRegisterId: 1, // Multiple KPIs can reference same register
                oee: Math.Round(0.83, 6),
                availability: Math.Round(0.88, 6),
                performance: Math.Round(0.95, 6),
                quality: Math.Round(0.99, 6),
                timeStamp: DateTime.UtcNow.AddMinutes(-45)),
            [7] = KpiOee.CreateFixture(
                kpiOeeId: 7,
                oeeRegisterId: 2,
                oee: Math.Round(0.79, 6),
                availability: Math.Round(0.85, 6),
                performance: Math.Round(0.98, 6),
                quality: Math.Round(0.95, 6),
                timeStamp: DateTime.UtcNow.AddMinutes(-75)),
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<KpiOee>> _fixtureCache =
        new(() => _kpiOeesDict.Values.ToList());

    /// <summary>
    /// Get all KpiOee entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<KpiOee> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific KpiOee by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static KpiOee? GetById(int id) =>
        _kpiOeesDict.TryGetValue(id, out var kpiOee) ? kpiOee : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, KpiOee> Dictionary => _kpiOeesDict;

    /// <summary>
    /// Check if a KpiOee exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _kpiOeesDict.ContainsKey(id);

    /// <summary>
    /// Get count of KpiOees - O(1) operation
    /// </summary>
    public static int Count => _kpiOeesDict.Count;

    /// <summary>
    /// Get KpiOee entries by OeeRegister ID - business query
    /// </summary>
    public static IEnumerable<KpiOee> GetByOeeRegisterId(int oeeRegisterId) =>
        _kpiOeesDict.Values.Where(k => k.OeeRegisterId == oeeRegisterId);

    /// <summary>
    /// Get recent KpiOee entries within time range
    /// </summary>
    public static IEnumerable<KpiOee> GetRecent(TimeSpan timeRange) =>
        _kpiOeesDict.Values.Where(k => DateTime.UtcNow - k.TimeStamp <= timeRange);

    /// <summary>
    /// Get KpiOee entries with OEE above threshold (high performance filter)
    /// </summary>
    public static IEnumerable<KpiOee> GetHighPerformance(double oeeThreshold = 0.85) =>
        _kpiOeesDict.Values.Where(k => k.Oee.Value >= oeeThreshold);

    /// <summary>
    /// Get KpiOee entries with OEE below threshold (needs attention filter)
    /// </summary>
    public static IEnumerable<KpiOee> GetLowPerformance(double oeeThreshold = 0.80) =>
        _kpiOeesDict.Values.Where(k => k.Oee.Value < oeeThreshold);
}
