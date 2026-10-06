// <copyright file="FlowStatusConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum.LookUpTable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the FlowStatusConfiguration.
/// </summary>

public class FlowStatusConfiguration : IEntityTypeConfiguration<FlowStatusEntity>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<FlowStatusEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName(nameof(FlowStatusEntity.Id))
            .HasColumnType("int")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .HasColumnName(nameof(FlowStatusEntity.Name))
            .HasMaxLength(80)
            .IsUnicode()
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.DisplayName)
            .HasColumnName(nameof(FlowStatusEntity.DisplayName))
            .HasMaxLength(80)
            .IsUnicode()
            .IsRequired()
            .ValueGeneratedNever();

        builder.ToTable("FlowStatus");
    }
}