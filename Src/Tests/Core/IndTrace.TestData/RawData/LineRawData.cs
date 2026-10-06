// <copyright file="LineRawData.cs" company="Exxerpro Solutions SA de CV">
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
/// Static test data for Line entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// </summary>
internal static class LineRawData
{
    /// <summary>
    /// Line test data
    /// </summary>
    private static readonly ImmutableDictionary<int, Line> _linesDict =
        new Dictionary<int, Line>
        {
            [1] = new Line
            {
                LineId = 1,
                Name = "Assembly Line 1",
                Description = "Main assembly line",
                IsActive = true
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<Line>> _fixtureCache =
        new(() => _linesDict.Values.ToList());

    /// <summary>
    /// Get all Line entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<Line> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific Line by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static Line? GetById(int id) =>
        _linesDict.TryGetValue(id, out var line) ? line : null;

    /// <summary>
    /// Get a specific Line by ID - O(1) lookup (backward compatibility)
    /// </summary>
    public static Line? GetLine(int id) => GetById(id);

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, Line> Dictionary => _linesDict;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (backward compatibility)
    /// </summary>
    public static IImmutableDictionary<int, Line> Lines => _linesDict;

    /// <summary>
    /// Check if a Line exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _linesDict.ContainsKey(id);

    /// <summary>
    /// Get count of Lines - O(1) operation
    /// </summary>
    public static int Count => _linesDict.Count;

    /// <summary>
    /// Get Line by Name - O(n) operation
    /// </summary>
    public static Line? GetByName(string name) =>
        _linesDict.Values.FirstOrDefault(l => l.Name == name);

    /// <summary>
    /// Get active Lines - O(n) operation
    /// </summary>
    public static IEnumerable<Line> GetActive() =>
        _linesDict.Values.Where(l => l.IsActive);
}
