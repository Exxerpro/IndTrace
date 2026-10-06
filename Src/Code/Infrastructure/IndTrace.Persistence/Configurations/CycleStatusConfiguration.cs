// <copyright file="CycleStatusConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum.LookUpTable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;

/// <summary>
/// Configures the CycleStatusEntity for Entity Framework, including table and property mappings.
/// </summary>
public class CycleStatusConfiguration : IEntityTypeConfiguration<CycleStatusEntity>
{
    /// <summary>
    /// Configures the CycleStatusEntity type.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<CycleStatusEntity> builder)
    {
        builder.HasKey(e => e.Id)
            .HasName("PK.IndTraceData.CycleStatus.id");

        builder.Property(e => e.Id)
            .HasColumnName(nameof(CycleStatusEntity.Id))
            .HasColumnType("int")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .HasColumnName(nameof(CycleStatusEntity.Name))
            .HasMaxLength(80)
            .IsUnicode()
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.DisplayName)
            .HasColumnName(nameof(CycleStatusEntity.DisplayName))
            .HasMaxLength(80)
            .IsUnicode()
            .IsRequired()
            .ValueGeneratedNever();

        builder.ToTable("CycleStatus");
    }
}