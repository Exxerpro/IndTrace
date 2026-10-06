// <copyright file="IRawDataProvider.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Immutable;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Standardized RawData Pattern for consistent test data access
/// All RawData classes should implement this pattern for maximum performance and compatibility
///
/// PATTERN REQUIREMENTS:
/// 1. Private ImmutableDictionary<int, TEntity> _entityDict for O(1) lookups
/// 2. Lazy<IReadOnlyList<TEntity>> _fixtureCache for cached List access
/// 3. Public static properties following this exact signature:
///    - IReadOnlyList<TEntity> Dict => _fixtureCache.Value (lazy-loaded List)
///    - IImmutableDictionary<int, TEntity> Dictionary => _entityDict (direct access)
///    - TEntity? GetById(int id) - O(1) lookup
///    - bool Contains(int id) - O(1) existence check
///    - int Count => _entityDict.Count - O(1) count
///
/// BENEFITS:
/// - O(1) dictionary lookups for performance-critical scenarios
/// - Cached List for backward compatibility with existing code
/// - Consistent API across all test data classes
/// - Thread-safe immutable data structures
/// - Lazy loading prevents unnecessary List creation unless needed
/// </summary>
/// <example>
/// Usage:
/// var cycle = CyclesRawData.GetById(123);           // O(1) lookup
/// var allCycles = CyclesRawData.Dict;            // Cached List
/// var hasId = CyclesRawData.Contains(123);          // O(1) check
/// var directDict = CyclesRawData.Dictionary;        // Advanced scenarios
/// </example>
internal static class RawDataPattern
{
    // This is a documentation class - not meant to be instantiated
    // All RawData classes should follow the pattern described above
}
