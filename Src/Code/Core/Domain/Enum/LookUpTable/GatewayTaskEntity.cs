// <copyright file="GatewayTaskEntity.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum.LookUpTable;

using IndTrace.Domain.Enum.Attributes;

/// <summary>
/// Represents the GatewayTaskEntity.
/// </summary>
[EnumLookup]
public class GatewayTaskEntity : EnumLookUpTable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayTaskEntity"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The displayName.</param>
    public GatewayTaskEntity(int id, string name, string displayName)
        : base(id, name, displayName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayTaskEntity"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GatewayTaskEntity()
    {
    }
}