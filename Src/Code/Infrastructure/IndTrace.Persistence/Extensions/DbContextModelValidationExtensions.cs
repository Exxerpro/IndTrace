// <copyright file="DbContextModelValidationExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IndTrace.Persistence.Interfaces;

namespace IndTrace.Persistence.Extensions;

/// <summary>
/// Provides extension methods for validating Entity Framework Core model configuration.
/// Ensures all model properties are explicitly configured to avoid implicit schema defaults.
/// </summary>
public static class DbContextModelValidationExtensions
{
    /// <summary>
    /// Enforces explicit model configuration for all entity properties.
    /// Validates that string, decimal, DateTime, and byte[] properties have proper configuration
    /// to avoid implicit EF Core defaults like nvarchar(max), datetime, and undefined decimal precision.
    /// </summary>
    /// <param name="modelBuilder">The model builder to validate.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when one or more entity properties are missing required configuration.
    /// </exception>
    /// <remarks>
    /// <para>This method enforces the following rules:</para>
    /// <list type="bullet">
    /// <item><description>string properties must have .HasMaxLength(n)</description></item>
    /// <item><description>decimal properties must have .HasPrecision(total, scale)</description></item>
    /// <item><description>DateTime properties must have .HasColumnType("datetime2")</description></item>
    /// <item><description>byte[] properties must have .HasMaxLength(n) or explicit .HasColumnType(...)</description></item>
    /// </list>
    /// <para>
    /// Additionally validates that only entities implementing IEntityRoot or ILookupEntity
    /// are registered as DbSet&lt;T&gt; in the DbContext.
    /// </para>
    /// </remarks>
    public static void EnforceModelConfiguration(this ModelBuilder modelBuilder)
    {
        var failures = new List<string>();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Validate DbSet registration (Task 2 validation)
            ValidateDbSetRegistration(entityType, failures);

            // Validate property configuration (Task 1 validation)
            ValidatePropertyConfiguration(entityType, failures);
        }

        // Log validation failures for debugging
        foreach (var fail in failures)
        {
            Console.WriteLine(fail);
        }

        ////Log
        //foreach (var fail in failures)
        //{
        //    //why we don't have a logger here?
        //}

        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(IIndTraceDbContext));

        foreach (var fail in failures)
        {
            logger.LogInformation("🚫 EF Core Model Validation failed for {failures } ", fail);
            //why we don't have a logger here?
        }
        // logger.LogInformation("🚫 EF Core Model Validation failed for {failures } ", fail);

        if (failures.Any())
        {
            throw new InvalidOperationException("🚫 EF Core Model Validation Failed:\n" + string.Join("\n", failures));
        }
    }

    /// <summary>
    /// Validates that only entities implementing IEntityRoot or ILookupEntity are registered as DbSet&lt;T&gt;.
    /// </summary>
    private static void ValidateDbSetRegistration(Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType, List<string> failures)
    {
        var clrType = entityType.ClrType;
        var isEntityRoot = typeof(IEntityRoot).IsAssignableFrom(clrType);
        var isLookupEntity = typeof(ILookupEntity).IsAssignableFrom(clrType);

        if (!isEntityRoot && !isLookupEntity)
        {
            failures.Add($"🚫 Invalid DbSet<T> registration → {clrType.Name} must implement IEntityRoot or ILookupEntity");
        }
    }

    /// <summary>
    /// Validates that all properties have explicit configuration to avoid EF Core implicit defaults.
    /// </summary>
    private static void ValidatePropertyConfiguration(Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType, List<string> failures)
    {
        foreach (var property in entityType.GetProperties())
        {
            var propertyName = $"{entityType.ClrType.Name}.{property.Name}";

            var validationError = property.ClrType switch
            {
                // String validation: requires MaxLength
                Type t when t == typeof(string) && property.GetMaxLength() is null
                    => "❌ Missing MaxLength",

                // Decimal validation: requires both Precision and Scale
                Type t when t == typeof(decimal) && (property.GetPrecision() is null || property.GetScale() is null)
                    => "❌ Missing Precision/Scale",

                // DateTime validation: requires datetime2 column type
                Type t when t == typeof(DateTime) && !IsValidDateTime2(property)
                    => "❌ Missing datetime2 type",

                // Binary data validation: requires MaxLength or explicit column type
                Type t when t == typeof(byte[]) && !HasValidBinaryConfiguration(property)
                    => "❌ Missing MaxLength or column type for byte[]",

                // All other types pass validation
                _ => null
            };

            if (validationError is not null)
            {
                failures.Add($"{validationError} → {propertyName}");
            }
        }
    }

    /// <summary>
    /// Checks if a DateTime property has valid datetime2 configuration.
    /// </summary>
    private static bool IsValidDateTime2(Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property)
    {
        var columnType = property.GetColumnType();
        return !string.IsNullOrWhiteSpace(columnType) && columnType.Contains("datetime2", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if a binary property has valid configuration (either MaxLength or explicit column type).
    /// </summary>
    private static bool HasValidBinaryConfiguration(Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property)
    {
        return property.GetMaxLength() is not null || !string.IsNullOrWhiteSpace(property.GetColumnType());
    }
}