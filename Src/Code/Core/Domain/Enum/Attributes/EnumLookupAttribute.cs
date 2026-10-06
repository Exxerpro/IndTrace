// <copyright file="EnumLookupAttribute.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum.Attributes
{
    using System;

    /// <summary>
    /// Indicates that the EnumModel should have a generated lookup table provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EnumLookupAttribute : Attribute
    {
    }
}