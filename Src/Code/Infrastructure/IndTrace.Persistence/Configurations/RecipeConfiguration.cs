// <copyright file="RecipeConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the RecipeConfiguration.
/// </summary>

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(e => e.RecipeId)
         .HasName("PK.IndTraceData.Recipes.RecipeId");

        builder.Property(e => e.RecipeId)
            .HasColumnName(nameof(Recipe.RecipeId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.ProductId)
            .HasColumnName(nameof(Recipe.ProductId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(Recipe.MachineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CycleTimeMinimum)
            .HasColumnName(nameof(Recipe.CycleTimeMinimum))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CycleTimeMaximum)
            .HasColumnName(nameof(Recipe.CycleTimeMaximum))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.MaxCyclesOk)
            .HasColumnName(nameof(Recipe.MaxCyclesOk))
            .HasColumnType("int")
            .IsRequired()
            .HasDefaultValue(3); // Optional for seeding/migrations

        builder.Property(e => e.MaxCyclesNOk)
            .HasColumnName(nameof(Recipe.MaxCyclesNOk))
            .HasColumnType("int")
            .IsRequired()
            .HasDefaultValue(5); // Optional for seeding/migrations

        builder.Property(e => e.Retry)
            .HasColumnName(nameof(Recipe.Retry))
            .HasColumnType("int")
            .IsRequired()
            .HasDefaultValue(1); // Optional for seeding/migrations

        builder.ToTable("Recipes");
    }
}