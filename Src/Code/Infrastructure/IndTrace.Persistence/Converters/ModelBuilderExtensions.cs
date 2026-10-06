// <copyright file="ModelBuilderExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace IndTrace.Persistence.Converters;

/// <summary>
/// Provides extension methods for configuring value converters in Entity Framework model builders.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies a value converter for a specific type to all matching properties in the model.
    /// </summary>
    /// <typeparam name="T">The type to apply the converter to.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="converter">The value converter to use.</param>
    /// <returns>The updated model builder.</returns>
    public static ModelBuilder UseValueConverterForType<T>(this ModelBuilder modelBuilder, ValueConverter converter)
    {
        return modelBuilder.UseValueConverterForType(typeof(T), converter);
    }

    /// <summary>
    /// Applies a value converter for a specific type to all matching properties in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="type">The type to apply the converter to.</param>
    /// <param name="converter">The value converter to use.</param>
    /// <returns>The updated model builder.</returns>
    public static ModelBuilder UseValueConverterForType(this ModelBuilder modelBuilder, Type type, ValueConverter converter)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // note that entityType.GetProperties() will throw an exception, so we have to use reflection
            var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == type);
            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.Name).Property(property.Name)
                    .HasConversion(converter);
            }
        }

        return modelBuilder;
    }
}