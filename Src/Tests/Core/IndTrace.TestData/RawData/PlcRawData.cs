// <copyright file="PlcRawData.cs" company="Exxerpro Solutions SA de CV">
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
/// Static test data for Plc entities with O(1) lookup.
/// IMPORTED: Contains all 1 entities from Plc100.json
/// Generated on: 2025-09-03 06:03:46
/// </summary>
internal static class PlcRawData
{
    private static readonly ImmutableDictionary<int, Plc> _plcsDict =
        new Dictionary<int, Plc>
        {
            // Story 26.A2 (#26): the Plc value scalars are now private set, so this fixture is seeded through the
            // in-Domain Plc.CreateFixture seam. Byte-identical to the former object initializer (unset fields keep
            // their defaults: Enabled = ActiveStatus.None, MachineId = 0).
            [100] = Plc.CreateFixture(
                plcId: 100,
                machineId: 0,
                enabled: IndTrace.Domain.Enum.ActiveStatus.None,
                name: "S7-1200",
                ipAddress: "192.168.0.100",
                plcType: "S7-1200",
                plcBrand: "Siemens",
                options: " [\n  {\n  \"Rack\": 0,\n  \"Slot\": 1,\n  \"TSAP\" : \"FD.01\"\n  }\n  ] ",
                commLibrary: "S7-Link",
                brandOwner: "Siemens")
        }.ToImmutableDictionary();

    private static readonly Lazy<IReadOnlyList<Plc>> _fixtureCache =
        new(() => _plcsDict.Values.ToList());

    public static IReadOnlyList<Plc> Fixture => _fixtureCache.Value;
    public static Plc? GetById(int id) => _plcsDict.TryGetValue(id, out var plc) ? plc : null;
    public static Plc? GetPlc(int id) => GetById(id);
    public static IImmutableDictionary<int, Plc> Dictionary => _plcsDict;
    public static bool Contains(int id) => _plcsDict.ContainsKey(id);
    public static int Count => _plcsDict.Count;
}
