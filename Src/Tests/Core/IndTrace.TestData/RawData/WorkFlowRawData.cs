// <copyright file="WorkFlowRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for WorkFlow entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// </summary>
internal static class WorkFlowRawData
{
    /// <summary>
    /// WorkFlow test data
    /// </summary>
    private static readonly ImmutableDictionary<int, WorkFlow> _workFlowsDict =
        new Dictionary<int, WorkFlow>
        {
            [1] = new WorkFlow
            {
                WorkFlowId = 1,
                ProductId = 1,
                LastMachineId = new MachineId(100),
                NextMachineId = new MachineId(300)
            },
            [2] = new WorkFlow
            {
                WorkFlowId = 2,
                ProductId = 1,
                LastMachineId = new MachineId(300),
                NextMachineId = new MachineId(500)
            },
            [3] = new WorkFlow
            {
                WorkFlowId = 3,
                ProductId = 508,
                LastMachineId = new MachineId(100),
                NextMachineId = new MachineId(300)
            },

            // Missing WorkFlows from JSON with LastMachineId=100 (needed by tests)
            [109] = new WorkFlow
            {
                WorkFlowId = 109,
                ProductId = 629,
                NextMachineId = new MachineId(300),
                LastMachineId = new MachineId(100),
                RuleId = 12,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 23),
                ModifiedBy = "0",
                ModifiedOn = new DateTime(2023, 8, 28, 17, 2, 23)
            },
            [113] = new WorkFlow
            {
                WorkFlowId = 113,
                ProductId = 508,
                NextMachineId = new MachineId(300),
                LastMachineId = new MachineId(200),
                RuleId = 22,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 23),
                ModifiedBy = "0",
                ModifiedOn = new DateTime(2023, 8, 28, 17, 2, 23)
            },

            // Missing WorkFlow for ProductId 508 with LastMachineId=300
            [114] = new WorkFlow
            {
                WorkFlowId = 114,
                ProductId = 508,
                NextMachineId = new MachineId(500),
                LastMachineId = new MachineId(300),
                RuleId = 23,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 23),
                ModifiedBy = "0",
                ModifiedOn = new DateTime(2023, 8, 28, 17, 2, 23)
            },

            // Complete workflow chains for ProductIds 629 and 508
            [108] = new WorkFlow
            {
                WorkFlowId = 108,
                ProductId = 629,
                NextMachineId = new MachineId(100),
                LastMachineId = new MachineId(0),
                RuleId = 11,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 23),
                ModifiedBy = "0",
                ModifiedOn = new DateTime(2023, 8, 28, 17, 2, 23)
            },
            [110] = new WorkFlow
            {
                WorkFlowId = 110,
                ProductId = 629,
                NextMachineId = new MachineId(500),
                LastMachineId = new MachineId(300),
                RuleId = 13,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 23),
                ModifiedBy = "0",
                ModifiedOn = new DateTime(2023, 8, 28, 17, 2, 23)
            },
            [111] = new WorkFlow
            {
                WorkFlowId = 111,
                ProductId = 629,
                NextMachineId = new MachineId(0),
                LastMachineId = new MachineId(500),
                RuleId = 14,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 23),
                ModifiedBy = "0",
                ModifiedOn = new DateTime(2023, 8, 28, 17, 2, 23)
            },
            [112] = new WorkFlow
            {
                WorkFlowId = 112,
                ProductId = 508,
                NextMachineId = new MachineId(100),
                LastMachineId = new MachineId(0),
                RuleId = 21,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 23),
                ModifiedBy = "0",
                ModifiedOn = new DateTime(2023, 8, 28, 17, 2, 23)
            },
            [115] = new WorkFlow
            {
                WorkFlowId = 115,
                ProductId = 508,
                NextMachineId = new MachineId(0),
                LastMachineId = new MachineId(500),
                RuleId = 24,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 23),
                ModifiedBy = "0",
                ModifiedOn = new DateTime(2023, 8, 28, 17, 2, 23)
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<WorkFlow>> _fixtureCache =
        new(() => _workFlowsDict.Values.ToList());

    /// <summary>
    /// Get all WorkFlow entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<WorkFlow> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific WorkFlow by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static WorkFlow? GetById(int id) =>
        _workFlowsDict.TryGetValue(id, out var workFlow) ? workFlow : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, WorkFlow> Dictionary => _workFlowsDict;

    /// <summary>
    /// Check if a WorkFlow exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _workFlowsDict.ContainsKey(id);

    /// <summary>
    /// Get count of WorkFlows - O(1) operation
    /// </summary>
    public static int Count => _workFlowsDict.Count;
}
