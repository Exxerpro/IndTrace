// <copyright file="CycleStatusEntity.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum.LookUpTable;

using IndTrace.Domain.Enum.Attributes;
using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents the CycleStatusEntity.
/// </summary>
// [Fix]
// CLAUDE
// Date: 25/08/2025
// Reason: EF Core entity interface fix - CycleStatusEntity needs ILookupEntity for DbSet registration
[EnumLookup]
public class CycleStatusEntity : EnumLookUpTable, ILookupEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CycleStatusEntity"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    public CycleStatusEntity(int id, string name, string displayName)
        : base(id, name, displayName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleStatusEntity"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public CycleStatusEntity()
    {
    }
}