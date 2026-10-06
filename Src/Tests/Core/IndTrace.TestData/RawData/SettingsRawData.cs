// <copyright file="SettingsRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

//[Fix]
//CLAUDE
//Date: 27/08/2025
//Reason: [Pattern Exact JSON] - Convert Settings from JSON to C# maintaining exact data

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for Setting entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// EXACT copy from Settings.json - maintains all configuration data for machine settings.
/// </summary>
internal static class SettingsRawData
{
    /// <summary>
    /// Setting test data - EXACT copy from Settings.json
    /// </summary>
    private static readonly ImmutableDictionary<int, Setting> _settingsDict =
        new Dictionary<int, Setting>
        {
            [1] = new Setting
            {
                SettingId = 1,
                MachineId = new MachineId(150),
                Config = "[\r\n\t{\r\n\t\tcolor: \"red\",\r\n\t\tvalue: \"#f00\"\r\n\t},\r\n\t{\r\n\t\tcolor: \"green\",\r\n\t\tvalue: \"#0f0\"\r\n\t},\r\n\t{\r\n\t\tcolor: \"blue\",\r\n\t\tvalue: \"#00f\"\r\n\t},\r\n\t{\r\n\t\tcolor: \"cyan\",\r\n\t\tvalue: \"#0ff\"\r\n\t},\r\n\t{\r\n\t\tcolor: \"magenta\",\r\n\t\tvalue: \"#f0f\"\r\n\t},\r\n\t{\r\n\t\tcolor: \"yellow\",\r\n\t\tvalue: \"#ff0\"\r\n\t},\r\n\t{\r\n\t\tcolor: \"black\",\r\n\t\tvalue: \"#000\"\r\n\t}\r\n]"
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<Setting>> _fixtureCache =
        new(() => _settingsDict.Values.ToList());

    /// <summary>
    /// Get all Setting entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<Setting> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific Setting by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static Setting? GetById(int id) =>
        _settingsDict.TryGetValue(id, out var setting) ? setting : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, Setting> Dictionary => _settingsDict;

    /// <summary>
    /// Check if a Setting exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _settingsDict.ContainsKey(id);

    /// <summary>
    /// Get count of Settings - O(1) operation
    /// </summary>
    public static int Count => _settingsDict.Count;

    /// <summary>
    /// Gets ALL Setting test data exactly as defined in Settings.json.
    /// Maintains exact Config JSON strings as defined in source. (backward compatibility)
    /// </summary>
    public static List<Setting> GetSettings() => Fixture.ToList();
}
