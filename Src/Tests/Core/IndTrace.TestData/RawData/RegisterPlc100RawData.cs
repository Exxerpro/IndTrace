// <copyright file="RegisterPlc100RawData.cs" company="Exxerpro Solutions SA de CV">
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
/// Static test data for Register entities with O(1) lookup.
/// IMPORTED: Contains all 2 entities from RegisterPlc100.json
/// Generated on: 2025-09-03 06:01:53
/// </summary>
internal static class RegisterPlc100RawData
{
    private static readonly ImmutableDictionary<int, Register> _registersDict =
        new Dictionary<int, Register>
        {
            [5512] = Register.CreateFixture(registerId: 5512, name: "PartStatusPlc", variableId: 121, cycleId: 1184, value: "1", dataType: "System.Int16", statusValueId: 1),
            [5554] = Register.CreateFixture(registerId: 5554, name: "CycleStatusPlc", variableId: 122, cycleId: 1195, value: "4", dataType: "System.Int16", statusValueId: 1)
        }.ToImmutableDictionary();

    private static readonly Lazy<IReadOnlyList<Register>> _fixtureCache =
        new(() => _registersDict.Values.ToList());

    public static IReadOnlyList<Register> Fixture => _fixtureCache.Value;

    public static Register? GetById(int id) => _registersDict.TryGetValue(id, out var register) ? register : null;

    public static Register? GetRegister(int id) => GetById(id);

    public static IImmutableDictionary<int, Register> Dictionary => _registersDict;

    public static bool Contains(int id) => _registersDict.ContainsKey(id);

    public static int Count => _registersDict.Count;
}
