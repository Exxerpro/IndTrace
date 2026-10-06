// <copyright file="VariablesGroupRawData.cs" company="Exxerpro Solutions SA de CV">
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
/// Static test data for VariablesGroup entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// Maps to TagsGroupsEnum for PLC variable categorization.
/// </summary>
internal static class VariablesGroupRawData
{
    /// <summary>
    /// VariablesGroup test data for tag categorization
    /// </summary>
    private static readonly ImmutableDictionary<int, VariablesGroup> _variablesGroupsDict =
        new Dictionary<int, VariablesGroup>
        {
            [1] = new VariablesGroup
            {
                VariableGroupId = 1,
                VariableGroupName = "EventsTags"
            },
            [2] = new VariablesGroup
            {
                VariableGroupId = 2,
                VariableGroupName = "ReadOnlyTags"
            },
            [4] = new VariablesGroup
            {
                VariableGroupId = 4,
                VariableGroupName = "WriteOnlyTags"
            },
            [8] = new VariablesGroup
            {
                VariableGroupId = 8,
                VariableGroupName = "WriteAndReadTags"
            },
            [16] = new VariablesGroup
            {
                VariableGroupId = 16,
                VariableGroupName = "ReadCyclicTags"
            },
            [32] = new VariablesGroup
            {
                VariableGroupId = 32,
                VariableGroupName = "WriteCyclicTags"
            },
            [64] = new VariablesGroup
            {
                VariableGroupId = 64,
                VariableGroupName = "HeartbeatTags"
            },
            [128] = new VariablesGroup
            {
                VariableGroupId = 128,
                VariableGroupName = "RegisterTags"
            },
            [256] = new VariablesGroup
            {
                VariableGroupId = 256,
                VariableGroupName = "ReferenceTags"
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<VariablesGroup>> _fixtureCache =
        new(() => _variablesGroupsDict.Values.ToList());

    /// <summary>
    /// Get all VariablesGroup entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<VariablesGroup> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific VariablesGroup by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static VariablesGroup? GetById(int id) =>
        _variablesGroupsDict.TryGetValue(id, out var variablesGroup) ? variablesGroup : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, VariablesGroup> Dictionary => _variablesGroupsDict;

    /// <summary>
    /// Check if a VariablesGroup exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _variablesGroupsDict.ContainsKey(id);

    /// <summary>
    /// Get count of VariablesGroups - O(1) operation
    /// </summary>
    public static int Count => _variablesGroupsDict.Count;

    /// <summary>
    /// Get VariablesGroup by name - O(n) operation
    /// </summary>
    public static VariablesGroup? GetByName(string name) =>
        _variablesGroupsDict.Values.FirstOrDefault(vg => vg.VariableGroupName == name);
}
