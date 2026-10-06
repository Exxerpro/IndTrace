// <copyright file="ConfigAppRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for ConfigApp entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// </summary>
internal static class ConfigAppRawData
{
    private static readonly ImmutableDictionary<int, ConfigApp> _configAppsDict =
        new Dictionary<int, ConfigApp>
        {
            [1] = new ConfigApp
            {
                AppId = 1,
                ConfigAppId = "IndTrace L1A",
                MachineId = 100,
                PlcId = 100,
                Pc = "1",
                Client = "Acme",
                Factory = "Acme",
                Line = "CHMSL",
                Project = "IndTrace",
                Version = "3",
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 31, 10, 46, 18),
                ModifiedBy = "Admin",
                ModifiedOn = new DateTime(2023, 8, 31, 10, 46, 18)
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Get a specific ConfigApp by ID - O(1) lookup
    /// </summary>
    public static ConfigApp? GetConfigApp(int id) => _configAppsDict.TryGetValue(id, out var configApp) ? configApp : null;

    /// <summary>
    /// Get all ConfigApp entities
    /// </summary>
    public static IReadOnlyList<ConfigApp> Fixture => _configAppsDict.Values.ToList();

    /// <summary>
    /// Direct dictionary access for advanced scenarios
    /// </summary>
    public static IImmutableDictionary<int, ConfigApp> ConfigApps => _configAppsDict;

    /// <summary>
    /// Get count of ConfigApps
    /// </summary>
    public static int Count => _configAppsDict.Count;
}
