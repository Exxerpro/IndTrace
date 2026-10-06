// <copyright file="MachineRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for Machine entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// </summary>
internal static class MachineRawData
{
    private static readonly ImmutableDictionary<int, Machine> _machinesDict =
        new Dictionary<int, Machine>
        {
            [0] = new Machine
            {
                MachineId = new MachineId(0),
                Name = "End/Start Process",
                Description = "Dummy Machine",
                Location = "N/A",
                MachineType = MachineType.None,
                WorkFlowType = WorkFlowType.None,
                EnableAppTraceability = 0,
                EnableBypassTraceability = 1,
                RuleId = 0
            },
            [100] = new Machine
            {
                MachineId = new MachineId(100),
                Name = "WS100",
                Description = "HOUSING",
                Location = "HOUSING",
                MachineType = MachineType.InitialPrinter,
                WorkFlowType = WorkFlowType.Initial,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
                RuleId = 100
            },
            [200] = new Machine
            {
                MachineId = new MachineId(200),
                Name = "WS200",
                Description = "HOUSING",
                Location = "HOUSING",
                MachineType = MachineType.Process,
                WorkFlowType = WorkFlowType.Serial,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
                RuleId = 200
            },
            [300] = new Machine
            {
                MachineId = new MachineId(300),
                Name = "WS300",
                Description = "HOUSING",
                Location = "HOUSING",
                MachineType = MachineType.Process,
                WorkFlowType = WorkFlowType.Serial,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
                RuleId = 300
            },
            [400] = new Machine
            {
                MachineId = new MachineId(400),
                Name = "WS400",
                Description = "HOUSING",
                Location = "HOUSING",
                MachineType = MachineType.Process,
                WorkFlowType = WorkFlowType.Serial,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
                RuleId = 400
            },
            [500] = new Machine
            {
                MachineId = new MachineId(500),
                Name = "WS500",
                Description = "HOUSING",
                Location = "HOUSING",
                MachineType = MachineType.Final,
                WorkFlowType = WorkFlowType.Final,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
                RuleId = 500
            },
            [600] = new Machine
            {
                MachineId = new MachineId(600),
                Name = "WS600",
                Description = "HOUSING",
                Location = "HOUSING",
                MachineType = MachineType.Process,
                WorkFlowType = WorkFlowType.Serial,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
                RuleId = 600
            },
            [700] = new Machine
            {
                MachineId = new MachineId(700),
                Name = "WS700",
                Description = "HOUSING",
                Location = "HOUSING",
                MachineType = MachineType.Process,
                WorkFlowType = WorkFlowType.Serial,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
                RuleId = 700
            },
            [800] = new Machine
            {
                MachineId = new MachineId(800),
                Name = "WS800",
                Description = "HOUSING",
                Location = "HOUSING",
                MachineType = MachineType.Process,
                WorkFlowType = WorkFlowType.Serial,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
                RuleId = 800
            },
            [900] = new Machine
            {
                MachineId = new MachineId(900),
                Name = "WS900",
                Description = "HOUSING",
                Location = "HOUSING",
                MachineType = MachineType.Final,
                WorkFlowType = WorkFlowType.Final,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
                RuleId = 900
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Static list for backward compatibility
    /// </summary>
    public static readonly List<Machine> FixtureMachines = _machinesDict.Values.ToList();

    /// <summary>
    /// Sample machines for static data strategy (serialized as JSON)
    /// </summary>
    public static string SampleMachines => System.Text.Json.JsonSerializer.Serialize(FixtureMachines);

    /// <summary>
    /// Get a specific Machine by ID - O(1) lookup
    /// </summary>
    public static Machine? GetMachine(int id) =>
        _machinesDict.TryGetValue(id, out var machine) ? machine : null;

    /// <summary>
    /// Get all Machine entities
    /// </summary>
    public static IReadOnlyList<Machine> Fixture => _machinesDict.Values.ToList();

    /// <summary>
    /// Check if a Machine exists by ID
    /// </summary>
    public static bool Contains(int id) => _machinesDict.ContainsKey(id);

    /// <summary>
    /// Get count of Machines
    /// </summary>
    public static int Count => _machinesDict.Count;

    /// <summary>
    /// Get Machine by Name - O(n) operation
    /// </summary>
    public static Machine? GetByName(string name) =>
        _machinesDict.Values.FirstOrDefault(m => m.Name == name);
}
