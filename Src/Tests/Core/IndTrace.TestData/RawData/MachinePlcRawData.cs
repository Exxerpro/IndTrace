// <copyright file="MachinePlcRawData.cs" company="Exxerpro Solutions SA de CV">
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
/// Static test data for MachinePlc entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// Maps machines to PLCs for communication configuration.
/// </summary>
internal static class MachinePlcRawData
{
    /// <summary>
    /// MachinePlc test data mapping machines to their PLCs
    /// </summary>
    private static readonly ImmutableDictionary<int, MachinePlc> _machinePlcsDict =
        new Dictionary<int, MachinePlc>
        {
            // MachineId 100 -> PlcId 100
            [100] = Seed(100, 100, 1, "Admin", new DateTime(2023, 8, 28, 17, 2, 13), "0", new DateTime(2020, 6, 26, 12, 9, 37)),

            // MachineId 200 -> PlcId 200
            [200] = Seed(200, 200, 1, "Admin", new DateTime(2024, 8, 28, 17, 2, 13), "Admin", new DateTime(2024, 8, 28, 17, 2, 13)),

            // MachineId 300 -> PlcId 300
            [300] = Seed(300, 300, 1, "Admin", new DateTime(2024, 8, 28, 17, 2, 13), "Admin", new DateTime(2024, 8, 28, 17, 2, 13)),

            // MachineId 400 -> PlcId 400
            [400] = Seed(400, 400, 1, "Admin", new DateTime(2023, 8, 28, 17, 2, 13), "0", new DateTime(2023, 8, 25, 12, 9, 37)),

            // MachineId 500 -> PlcId 500
            [500] = Seed(500, 500, 1, "Admin", new DateTime(2023, 8, 28, 17, 2, 13), "0", new DateTime(2023, 8, 25, 12, 9, 37)),

            // MachineId 600 -> PlcId 600
            [600] = Seed(600, 600, 1, "Exxerpro", new DateTime(2024, 7, 22, 16, 24, 6), "Exxerpro", new DateTime(2024, 7, 22, 16, 24, 6)),

            // MachineId 700 -> PlcId 700
            [700] = Seed(700, 700, 1, "Exxerpro", new DateTime(2024, 7, 22, 16, 24, 6), "Exxerpro", new DateTime(2024, 7, 22, 16, 24, 6)),

            // MachineId 800 -> PlcId 800
            [800] = Seed(800, 800, 1, "Exxerpro", new DateTime(2024, 7, 22, 16, 24, 6), "Exxerpro", new DateTime(2024, 7, 22, 16, 24, 6)),

            // MachineId 900 -> PlcId 900
            [900] = Seed(900, 900, 1, "Exxerpro", new DateTime(2024, 7, 22, 16, 24, 6), "Exxerpro", new DateTime(2024, 7, 22, 16, 24, 6)),
        }.ToImmutableDictionary();

    /// <summary>
    /// Story 26.A2 (#26): seeds a MachinePlc fixture. The value scalars (MachineId/PlcId/IsActive) are now
    /// <c>private set</c>, so they are seeded through the in-Domain <c>MachinePlc.CreateFixture</c> seam; the audit
    /// fields (<c>AuditableEntity</c>) keep public setters and are applied afterwards. Byte-identical to the former
    /// <c>new MachinePlc { ... }</c> object initializers.
    /// </summary>
    private static MachinePlc Seed(
        int machineId,
        int plcId,
        IndTrace.Domain.Enum.ActiveStatus isActive,
        string createdBy,
        DateTime createdOn,
        string modifiedBy,
        DateTime modifiedOn)
    {
        var machinePlc = MachinePlc.CreateFixture(machineId, plcId, isActive);
        machinePlc.CreatedBy = createdBy;
        machinePlc.CreatedOn = createdOn;
        machinePlc.ModifiedBy = modifiedBy;
        machinePlc.ModifiedOn = modifiedOn;
        return machinePlc;
    }

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<MachinePlc>> _fixtureCache =
        new(() => _machinePlcsDict.Values.ToList());

    /// <summary>
    /// Get all MachinePlc entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<MachinePlc> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific MachinePlc by MachineId - O(1) lookup (standardized pattern)
    /// </summary>
    public static MachinePlc? GetByMachineId(int machineId) =>
        _machinePlcsDict.TryGetValue(machineId, out var machinePlc) ? machinePlc : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, MachinePlc> Dictionary => _machinePlcsDict;

    /// <summary>
    /// Check if a MachinePlc exists by MachineId - O(1) lookup
    /// </summary>
    public static bool Contains(int machineId) => _machinePlcsDict.ContainsKey(machineId);

    /// <summary>
    /// Get count of MachinePlcs - O(1) operation
    /// </summary>
    public static int Count => _machinePlcsDict.Count;

    /// <summary>
    /// Get MachinePlc by PlcId - O(n) operation
    /// </summary>
    public static MachinePlc? GetByPlcId(int plcId) =>
        _machinePlcsDict.Values.FirstOrDefault(mp => mp.PlcId == plcId);

    /// <summary>
    /// Get all active MachinePlcs - O(n) operation
    /// </summary>
    public static IEnumerable<MachinePlc> GetActive() =>
        _machinePlcsDict.Values.Where(mp => mp.IsActive.Value == ActiveStatus.Active.Value);
}
