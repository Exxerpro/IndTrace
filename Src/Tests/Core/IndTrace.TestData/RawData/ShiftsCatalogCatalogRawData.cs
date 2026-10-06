// <copyright file="ShiftsCatalogCatalogRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for ShiftsCatalog entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// </summary>
internal static class ShiftsCatalogCatalogRawData
{
    /// <summary>
    /// ShiftsCatalog test data
    /// </summary>
    private static readonly ImmutableDictionary<int, ShiftsCatalog> _ShiftsCatalogsDict =
        new Dictionary<int, ShiftsCatalog>
        {
            [1] = new ShiftsCatalog()
            {
                ShiftCatalogId = 1,
                ShiftName = 1.ToString(),
                PlantId = 3,
                StartBy = DateTime.ParseExact("7:00 AM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay,
                Duration = TimeSpan.FromHours(8),
                EndTime = DateTime.ParseExact("3:00 AM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay,
                //Auditable entity properties
                CreatedBy = "Admin",
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = "Admin",
                ModifiedOn = DateTime.UtcNow
            },
            [2] = new ShiftsCatalog()
            {
                //DateTime.ParseExact("8:00 AM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                ShiftCatalogId = 2,
                ShiftName = 2.ToString(),
                PlantId = 3,
                StartBy = DateTime.ParseExact("3:00 PM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay,
                Duration = TimeSpan.FromHours(8),
                EndTime = DateTime.ParseExact("11:00 PM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay,
                //Auditable entity properties
                CreatedBy = "Admin",
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = "Admin",
                ModifiedOn = DateTime.UtcNow
            },
            [3] = new ShiftsCatalog()
            {
                //DateTime.ParseExact("8:00 AM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                ShiftCatalogId = 3,
                ShiftName = 3.ToString(),
                PlantId = 3,
                StartBy = DateTime.ParseExact("11:00 PM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay,
                Duration = TimeSpan.FromHours(8),
                EndTime = DateTime.ParseExact("7:00 AM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay,
                //Auditable entity properties
                CreatedBy = "Admin",
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = "Admin",
                ModifiedOn = DateTime.UtcNow
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<ShiftsCatalog>> _fixtureCache =
        new(() => _ShiftsCatalogsDict.Values.ToList());

    /// <summary>
    /// Get all ShiftsCatalog entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<ShiftsCatalog> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific ShiftsCatalog by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static ShiftsCatalog? GetById(int id) =>
        _ShiftsCatalogsDict.TryGetValue(id, out var ShiftsCatalog) ? ShiftsCatalog : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, ShiftsCatalog> Dictionary => _ShiftsCatalogsDict;

    /// <summary>
    /// Check if a ShiftsCatalog exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _ShiftsCatalogsDict.ContainsKey(id);

    /// <summary>
    /// Get count of ShiftsCatalogs - O(1) operation
    /// </summary>
    public static int Count => _ShiftsCatalogsDict.Count;
}
