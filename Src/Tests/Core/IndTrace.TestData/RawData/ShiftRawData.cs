// <copyright file="ShiftRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for Shift entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// </summary>
internal static class ShiftRawData
{
    /// <summary>
    /// Shift test data
    /// </summary>
    private static readonly ImmutableDictionary<int, Shift> _shiftsDict =
        new Dictionary<int, Shift>
        {
            [1] = new Shift(new DateTimeMachine())
            {
                ShiftId = new ShiftId(1),
                ShiftType = ShiftType.First.Name,
                StartBy = DateTime.Today + DateTime.ParseExact("7:00 AM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay,
                Duration = TimeSpan.FromHours(8),
                // Type is derived (never persisted) from ShiftType; no longer settable.
                //Auditable entity properties
                CreatedBy = "Admin",
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = "Admin",
                ModifiedOn = DateTime.UtcNow
            },
            [2] = new Shift(new DateTimeMachine())
            {
                ShiftId = new ShiftId(2),
                ShiftType = ShiftType.Second.Name,
                StartBy = DateTime.Today + DateTime.ParseExact("3:00 PM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay,
                Duration = TimeSpan.FromHours(8),
                // Type is derived (never persisted) from ShiftType; no longer settable.
                //Auditable entity properties
                CreatedBy = "Admin",
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = "Admin",
                ModifiedOn = DateTime.UtcNow
            },
            [3] = new Shift(new DateTimeMachine())
            {
                ShiftId = new ShiftId(3),
                ShiftType = ShiftType.Third.Name,
                StartBy = DateTime.Today + DateTime.ParseExact("11:00 PM", "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay,
                Duration = TimeSpan.FromHours(8),
                // Type is derived (never persisted) from ShiftType; no longer settable.
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
    private static readonly Lazy<IReadOnlyList<Shift>> _fixtureCache =
        new(() => _shiftsDict.Values.ToList());

    /// <summary>
    /// Get all Shift entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<Shift> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific Shift by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static Shift? GetById(int id) =>
        _shiftsDict.TryGetValue(id, out var shift) ? shift : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, Shift> Dictionary => _shiftsDict;

    /// <summary>
    /// Check if a Shift exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _shiftsDict.ContainsKey(id);

    /// <summary>
    /// Get count of Shifts - O(1) operation
    /// </summary>
    public static int Count => _shiftsDict.Count;
}
