// <copyright file="EnumLookUpTable.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum.LookUpTable;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a lookup table entry with an ID, name, and display name.
/// </summary>
// [Fix]
// CLAUDE
// Date: 25/08/2025
// Reason: EF Core entity interface fix - EnumLookUpTable base class needs ILookupEntity to fix all Entity DbSet registrations
public class EnumLookUpTable : ILookUpTable, ILookupEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumLookUpTable"/> class.
    /// </summary>
    public EnumLookUpTable()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumLookUpTable"/> class with specified values.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    public EnumLookUpTable(int id, string name, string displayName)
    {
        this.Id = id;
        this.Name = name;
        this.DisplayName = displayName;
    }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Converts this lookup table to a specified upper class type.
    /// </summary>
    /// <typeparam name="TLookUpTable">The type of the upper class.</typeparam>
    /// <param name="lookUpTable">The lookup table to convert.</param>
    /// <returns>A new instance of <typeparamref name="TLookUpTable"/>.</returns>
    public static TLookUpTable ToUpperClass<TLookUpTable>(ILookUpTable lookUpTable)
        where TLookUpTable : EnumLookUpTable, ILookUpTable, new()
    {
        var (id, name, displayName) = lookUpTable;
        return new TLookUpTable { Id = id, Name = name, DisplayName = displayName };
    }

    /// <summary>
    /// Deconstructs the lookup table into its components.
    /// </summary>
    /// <param name="value">The ID value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    public void Deconstruct(out int value, out string name, out string displayName)
    {
        value = this.Id;
        name = this.Name;
        displayName = this.DisplayName;
    }
}
