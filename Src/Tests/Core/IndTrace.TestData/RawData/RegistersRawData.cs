// <copyright file="RegistersRawData.cs" company="Exxerpro Solutions SA de CV">
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
/// RELATIONAL: Generated from BarCode→Cycles relational cascade.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// </summary>
internal static class RegistersRawData
{
    /// <summary>
    /// Relational test data for registers linked to test Cycles
    /// </summary>
    private static readonly ImmutableDictionary<int, Register> _registersDict =
        new Dictionary<int, Register>
        {
            [1] = Register.CreateFixture(registerId: 1, name: "REG_0001", description: "Test register 1", machineId: 100, variableId: 1, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [2] = Register.CreateFixture(registerId: 2, name: "REG_0002", description: "Test register 2", machineId: 100, variableId: 2, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [3] = Register.CreateFixture(registerId: 3, name: "REG_0003", description: "Test register 3", machineId: 100, variableId: 3, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [4] = Register.CreateFixture(registerId: 4, name: "REG_0004", description: "Test register 4", machineId: 100, variableId: 4, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [5] = Register.CreateFixture(registerId: 5, name: "REG_0005", description: "Test register 5", machineId: 100, variableId: 5, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [10] = Register.CreateFixture(registerId: 10, name: "REG_0010", description: "Test register 10", machineId: 100, variableId: 10, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [11] = Register.CreateFixture(registerId: 11, name: "REG_0011", description: "Test register 11", machineId: 100, variableId: 11, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [12] = Register.CreateFixture(registerId: 12, name: "REG_0012", description: "Test register 12", machineId: 100, variableId: 12, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [15] = Register.CreateFixture(registerId: 15, name: "REG_0015", description: "Test register 15", machineId: 100, variableId: 15, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [20] = Register.CreateFixture(registerId: 20, name: "REG_0020", description: "Test register 20", machineId: 100, variableId: 20, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),

            [25] = Register.CreateFixture(registerId: 25, name: "REG_0025", description: "Test register 25", machineId: 100, variableId: 25, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [30] = Register.CreateFixture(registerId: 30, name: "REG_0030", description: "Test register 30", machineId: 100, variableId: 30, cycleId: 1, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [100] = Register.CreateFixture(registerId: 100, name: "Original Complex Entity", variableId: 100, cycleId: 1000, value: "4", dataType: "System.Int16", statusValueId: 1),
            [480] = Register.CreateFixture(registerId: 480, name: "REG_0480", description: "Test register 480", machineId: 300, variableId: 480, cycleId: 92, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [481] = Register.CreateFixture(registerId: 481, name: "REG_0481", description: "Test register 481", machineId: 300, variableId: 481, cycleId: 92, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [482] = Register.CreateFixture(registerId: 482, name: "REG_0482", description: "Test register 482", machineId: 300, variableId: 482, cycleId: 92, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            // Bridge entries to align RawData with canonical JSON register names used by tests
            [20001] = Register.CreateFixture(registerId: 20001, name: "PartStatusPlc", description: "PartStatusPlc", machineId: 100, variableId: 1, cycleId: 1, value: "1", dataType: "System.Int16", statusValueId: 1, timeStamp: new DateTime(2023, 8, 27, 0, 50, 34, DateTimeKind.Utc)),
            [20002] = Register.CreateFixture(registerId: 20002, name: "CycleStatusPlc", description: "CycleStatusPlc", machineId: 100, variableId: 2, cycleId: 1, value: "2", dataType: "System.Int16", statusValueId: 1, timeStamp: new DateTime(2023, 8, 27, 0, 50, 34, DateTimeKind.Utc)),
            [20003] = Register.CreateFixture(registerId: 20003, name: "PartStatusPlc", description: "PartStatusPlc", machineId: 400, variableId: 1, cycleId: 1, value: "1", dataType: "System.Int16", statusValueId: 1, timeStamp: new DateTime(2023, 8, 27, 0, 50, 34, DateTimeKind.Utc)),
            [20004] = Register.CreateFixture(registerId: 20004, name: "CycleStatusPlc", description: "CycleStatusPlc", machineId: 400, variableId: 2, cycleId: 1, value: "2", dataType: "System.Int16", statusValueId: 1, timeStamp: new DateTime(2023, 8, 27, 0, 50, 34, DateTimeKind.Utc)),
            [555] = Register.CreateFixture(registerId: 555, name: "Original Complex Entity", variableId: 555, cycleId: 555, value: "4", dataType: "System.Int16", statusValueId: 1),
            [666] = Register.CreateFixture(registerId: 666, name: "Original Complex Entity", variableId: 666, cycleId: 666, value: "4", dataType: "System.Int16", statusValueId: 1),
            [777] = Register.CreateFixture(registerId: 777, name: "Original Complex Entity", variableId: 777, cycleId: 777, value: "4", dataType: "System.Int16", statusValueId: 1),
            [888] = Register.CreateFixture(registerId: 888, name: "Exception Test", variableId: 888, cycleId: 888, value: "4", dataType: "System.Int16", statusValueId: 1),
            [920] = Register.CreateFixture(registerId: 920, name: "REG_0920", description: "Test register 920", machineId: 500, variableId: 920, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [921] = Register.CreateFixture(registerId: 921, name: "REG_0921", description: "Test register 921", machineId: 500, variableId: 921, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [922] = Register.CreateFixture(registerId: 922, name: "REG_0922", description: "Test register 922", machineId: 500, variableId: 922, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1000] = Register.CreateFixture(registerId: 1000, name: "Original Complex Entity", variableId: 1000, cycleId: 1000, value: "4", dataType: "System.Int16", statusValueId: 1),
            [1090] = Register.CreateFixture(registerId: 1090, name: "REG_1090", description: "Test register 1090", machineId: 500, variableId: 1090, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1091] = Register.CreateFixture(registerId: 1091, name: "REG_1091", description: "Test register 1091", machineId: 500, variableId: 1091, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1092] = Register.CreateFixture(registerId: 1092, name: "REG_1092", description: "Test register 1092", machineId: 500, variableId: 1092, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1360] = Register.CreateFixture(registerId: 1360, name: "REG_1360", description: "Test register 1360", machineId: 500, variableId: 1360, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1361] = Register.CreateFixture(registerId: 1361, name: "REG_1361", description: "Test register 1361", machineId: 500, variableId: 1361, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1362] = Register.CreateFixture(registerId: 1362, name: "REG_1362", description: "Test register 1362", machineId: 500, variableId: 1362, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),

            [1550] = Register.CreateFixture(registerId: 1550, name: "REG_1550", description: "Test register 1550", machineId: 500, variableId: 1550, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1551] = Register.CreateFixture(registerId: 1551, name: "REG_1551", description: "Test register 1551", machineId: 500, variableId: 1551, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1552] = Register.CreateFixture(registerId: 1552, name: "REG_1552", description: "Test register 1552", machineId: 500, variableId: 1552, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),

            [1520] = Register.CreateFixture(registerId: 1520, name: "REG_1520", description: "Test register 1520", machineId: 500, variableId: 1520, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1521] = Register.CreateFixture(registerId: 1521, name: "REG_1521", description: "Test register 1521", machineId: 500, variableId: 1521, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [1522] = Register.CreateFixture(registerId: 1522, name: "REG_1522", description: "Test register 1522", machineId: 500, variableId: 1522, cycleId: 152, value: "0", dataType: "INT", statusValueId: 1, timeStamp: DateTime.UtcNow),
            [999999] = Register.CreateFixture(registerId: 999999, name: "Original Complex Entity", variableId: 100, cycleId: 1000, value: "4", dataType: "System.Int16", statusValueId: 1),
            [2147483646] = Register.CreateFixture(registerId: 2147483646, name: "Original Complex Entity", variableId: 100, cycleId: 1000, value: "4", dataType: "System.Int16", statusValueId: 1),
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<Register>> _fixtureCache =
        new(() => _registersDict.Values.ToList());

    /// <summary>
    /// Get all Register entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<Register> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific Register by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static Register? GetById(int id) =>
        _registersDict.TryGetValue(id, out var register) ? register : null;

    /// <summary>
    /// Get a specific Register by ID - O(1) lookup (backward compatibility)
    /// </summary>
    public static Register? GetRegister(int id) => GetById(id);

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, Register> Dictionary => _registersDict;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (backward compatibility)
    /// </summary>
    public static IImmutableDictionary<int, Register> Registers => _registersDict;

    /// <summary>
    /// Check if a Register exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _registersDict.ContainsKey(id);

    /// <summary>
    /// Get count of Registers - O(1) operation
    /// </summary>
    public static int Count => _registersDict.Count;

    /// <summary>
    /// Get Register by MachineId - O(n) operation
    /// </summary>
    public static IEnumerable<Register> GetByMachineId(int machineId) =>
        _registersDict.Values.Where(r => r.MachineId == machineId);

    /// <summary>
    /// Get Register by VariableId - O(n) operation
    /// </summary>
    public static IEnumerable<Register> GetByVariableId(int variableId) =>
        _registersDict.Values.Where(r => r.VariableId == variableId);

    /// <summary>
    /// Get Register by CycleId - O(n) operation
    /// </summary>
    public static IEnumerable<Register> GetByCycleId(int cycleId) =>
        _registersDict.Values.Where(r => r.CycleId.Value == cycleId);

    /// <summary>
    /// Get Register by Name - O(n) operation
    /// </summary>
    public static Register? GetByName(string name) =>
        _registersDict.Values.FirstOrDefault(r => r.Name == name);
}
