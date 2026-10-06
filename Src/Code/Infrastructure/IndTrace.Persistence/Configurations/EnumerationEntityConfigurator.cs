// <copyright file="EnumerationEntityConfigurator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections;
using System.Reflection;
using IndTrace.Domain.Enum.LookUpTable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;

/// <summary>
/// Configures entity types for enumerations in Entity Framework, including seeding initial data.
/// </summary>
public class EnumerationEntityConfigurator<TEnumeration, TLookUp> : IEntityTypeConfiguration<TLookUp>
    where TLookUp : EnumLookUpTable, ILookUpTable, new()
{
    // Fix for CS0080: Remove the constraint from the method declaration.
    // The generic constraint should only be on the class, not on the method.

#pragma warning disable CS0518

    //EnumLoouUpTable Has ID, Name And Display Name Because Implement ILookUpTable
    public void Configure(EntityTypeBuilder<TLookUp> builder)
    {
        // Your common configurations for all EnumModel entities here.
        // For example, setting the primary key:
        builder.HasKey(x => x.Id);

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core MaxLength constraint fix - Enum Name and DisplayName properties missing MaxLength configuration
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(120);

#pragma warning restore CS0518
        //EnumLoouUpTable Has ID, Name And Display Name Because Implement ILookUpTable
        // builder.ToTable(typeof(TLookUp).Name + "s");
        // Use reflection to populate the initial data.
        // Since TEntity is no longer constrained, you have to call your static methods dynamically.
        var getAllMethod = typeof(TEnumeration).GetMethod("GetAll", BindingFlags.Public | BindingFlags.Static);
        if (getAllMethod is null)
        {
            throw new InvalidOperationException("The GetAll method was not found on the EnumModel type.");
        }

        var enumerationValues = (IEnumerable?)getAllMethod.Invoke(null, null);

        if (enumerationValues is null)
        {
            return;
        }

        foreach (var enumValue in enumerationValues)
        {
            if (enumValue is null)
            {
                continue;
            }

            var valueProperty = typeof(TEnumeration).GetProperty("Value");
            var nameProperty = typeof(TEnumeration).GetProperty("Name");
            var displayNameProperty = typeof(TEnumeration).GetProperty("DisplayName");

            if (valueProperty?.GetValue(enumValue) is int value &&
                nameProperty?.GetValue(enumValue) is string name &&
                displayNameProperty?.GetValue(enumValue) is string displayName)
            {
                builder.HasData(new TLookUp { Id = value, Name = name, DisplayName = displayName });
            }
        }
    }
}

