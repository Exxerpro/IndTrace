// <copyright file="MasterLabelRawData.cs" company="Exxerpro Solutions SA de CV">
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
/// Static test data for MasterLabel entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// </summary>
internal static class MasterLabelRawData
{
    /// <summary>
    /// MasterLabel test data
    /// </summary>
    private static readonly ImmutableDictionary<int, MasterLabel> _masterLabelsDict =
        new Dictionary<int, MasterLabel>
        {
            [1] = new MasterLabel
            {
                MasterLabelId = 1,
                MasterLabelCode = "L1AL100003232372517",
                Description = "Test Master Label"
            },
            [2] = new MasterLabel
            {
                MasterLabelId = 2,
                MasterLabelCode = "L1AL100003232372519",
                Description = "Test Master Label 2"
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<MasterLabel>> _fixtureCache =
        new(() => _masterLabelsDict.Values.ToList());

    /// <summary>
    /// Get all MasterLabel entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<MasterLabel> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific MasterLabel by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static MasterLabel? GetById(int id) =>
        _masterLabelsDict.TryGetValue(id, out var masterLabel) ? masterLabel : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, MasterLabel> Dictionary => _masterLabelsDict;

    /// <summary>
    /// Check if a MasterLabel exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _masterLabelsDict.ContainsKey(id);

    /// <summary>
    /// Get count of MasterLabels - O(1) operation
    /// </summary>
    public static int Count => _masterLabelsDict.Count;
}
