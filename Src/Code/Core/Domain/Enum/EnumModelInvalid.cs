// <copyright file="EnumModelInvalid.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

using System.Reflection;
using IndQuestEnums;

/// <summary>
/// Resolves the "invalid" sentinel instance for an <see cref="EnumModel"/>-derived type.
/// </summary>
/// <remarks>
/// Mirrors <c>EnumModel.InvalidValue&lt;T&gt;()</c> (revived as a method in IndQuestEnums 1.1.0):
/// it returns the type's own <c>public static readonly Invalid</c> singleton when declared.
/// Retained as the IndTrace-local entry point so consumers that need the type-specific
/// <c>Invalid</c> instance (e.g. cache JSON converters) go through this helper rather than
/// <c>FromValue&lt;T&gt;(EnumModel.InvalidState)</c>, which would mis-resolve any enum whose
/// <c>Invalid</c> value is not <c>-1</c> (e.g. <c>FlowStatus.Invalid = 8</c>) or whose
/// <c>-1</c> slot is a real member (e.g. <c>ActiveStatus.Inactive = -1</c>). See issue #37.
/// </remarks>
public static class EnumModelInvalid
{
    /// <summary>
    /// Resolves the invalid sentinel for <typeparamref name="TEnumeration"/>.
    /// </summary>
    /// <typeparam name="TEnumeration">The smart-enumeration type.</typeparam>
    /// <returns>The type's <c>Invalid</c> singleton, or a fresh sentinel instance.</returns>
    public static TEnumeration Of<TEnumeration>()
        where TEnumeration : EnumModel, new()
        => (TEnumeration)Of(typeof(TEnumeration));

    /// <summary>
    /// Resolves the invalid sentinel for <paramref name="enumType"/>.
    /// </summary>
    /// <param name="enumType">An <see cref="EnumModel"/>-derived type.</param>
    /// <returns>The type's <c>Invalid</c> singleton, or a fresh sentinel instance.</returns>
    public static EnumModel Of(Type enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        var invalidField = enumType.GetField(
            "Invalid",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (invalidField?.GetValue(null) is EnumModel invalid)
        {
            return invalid;
        }

        // No declared Invalid field: the package's parameterless ctor yields the invalid sentinel.
        if (Activator.CreateInstance(enumType) is EnumModel fresh)
        {
            return fresh;
        }

        throw new ArgumentException(
            $"Type '{enumType}' is not an instantiable EnumModel.",
            nameof(enumType));
    }
}
