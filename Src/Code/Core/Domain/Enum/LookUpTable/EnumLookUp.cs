// <copyright file="EnumLookUp.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum.LookUpTable;

using System.Reflection;
using IndQuestEnums;

/// <summary>
/// Provides projections from <see cref="EnumModel"/>-derived smart enumerations into
/// IndTrace's domain lookup-table type (<see cref="EnumLookUpTable"/>).
/// </summary>
/// <remarks>
/// These projections live in the Domain layer (not in the dependency-free <c>IndQuestEnums</c>
/// package) because they return IndTrace's own <see cref="EnumLookUpTable"/> domain type.
/// </remarks>
public static class EnumLookUp
{
    /// <summary>
    /// Projects all declared members of <typeparamref name="TEnumeration"/> (those with a
    /// non-negative value) into a list of the strongly-typed lookup table
    /// <typeparamref name="TLookUpTable"/>.
    /// </summary>
    /// <typeparam name="TLookUpTable">The concrete lookup-table type to produce.</typeparam>
    /// <typeparam name="TEnumeration">The smart-enumeration type to project.</typeparam>
    /// <returns>A list of <typeparamref name="TLookUpTable"/> instances.</returns>
    public static IList<TLookUpTable> ToLookUpTable<TLookUpTable, TEnumeration>()
        where TLookUpTable : EnumLookUpTable, ILookUpTable, new()
        where TEnumeration : EnumModel, new()
    {
        var type = typeof(TEnumeration);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        // #126 F8: the three property lookups are hoisted out of the per-member loop — fetched once per
        // projection and reused for every member (previously GetProperty ran per element, per property).
        var valueProperty = type.GetProperty("Value");
        var nameProperty = type.GetProperty("Name");
        var displayNameProperty = type.GetProperty("DisplayName");

        return [.. from info in fields
                select info.GetValue(null)
            into enumeration
                where enumeration is TEnumeration
                let value = valueProperty?.GetValue(enumeration, null) as int?
                let name = nameProperty?.GetValue(enumeration, null) as string
                let displayName = displayNameProperty?.GetValue(enumeration, null) as string
                where value.HasValue && value.Value >= 0
                select new EnumLookUpTable(value ?? 0, name ?? string.Empty, displayName ?? string.Empty) into lookUpTable
                select EnumLookUpTable.ToUpperClass<TLookUpTable>(lookUpTable)];
    }

    /// <summary>
    /// Projects all declared members of <typeparamref name="TEnumeration"/> (those with a
    /// non-negative value) into a list of <see cref="EnumLookUpTable"/>.
    /// </summary>
    /// <typeparam name="TEnumeration">The smart-enumeration type to project.</typeparam>
    /// <returns>A list of <see cref="EnumLookUpTable"/> instances.</returns>
    public static IList<EnumLookUpTable> ToLookUpTable<TEnumeration>()
        where TEnumeration : EnumModel, new()
    {
        var type = typeof(TEnumeration);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        // #126 F8: the three property lookups are hoisted out of the per-member loop — fetched once per
        // projection and reused for every member (previously GetProperty ran per element, per property).
        var valueProperty = type.GetProperty("Value");
        var nameProperty = type.GetProperty("Name");
        var displayNameProperty = type.GetProperty("DisplayName");

        return [.. from info in fields
                select info.GetValue(null)
            into enumeration
                where enumeration is TEnumeration
                let value = valueProperty?.GetValue(enumeration, null) as int?
                let name = nameProperty?.GetValue(enumeration, null) as string
                let displayName = displayNameProperty?.GetValue(enumeration, null) as string
                where value.HasValue && value.Value >= 0
                select new EnumLookUpTable(value ?? 0, name ?? string.Empty, displayName ?? string.Empty)];
    }
}
