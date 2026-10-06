// <copyright file="DefectsConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the DefectsConfiguration.
/// </summary>

public class DefectsConfiguration : IEntityTypeConfiguration<Defect>

{
    public void Configure(EntityTypeBuilder<Defect> builder)
    {
        // Configure primary key following safe refactoring pattern: {nameof(Defect) + "RegisterId"}
        builder.HasKey(e => e.DefectId)
            .HasName("PK.IndTraceData.Defects.DefectId");

        builder.Property(e => e.DefectId)
            .HasColumnName(nameof(Defect.DefectId))
            .HasColumnType("int")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.DefectTypeId)
            .HasColumnName(nameof(Defect.DefectTypeId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName(nameof(Defect.Description))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.Name)
            .HasColumnName(nameof(Defect.Name))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.ShortName)
            .HasColumnName(nameof(Defect.ShortName))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.HasIndex(e => e.DefectId)
            .HasDatabaseName("IDX.IndTraceData.Defects.DefectId")
            .IsUnique();

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IDX.IndTraceData.Defects.Name")
            .IsUnique();

        builder.ToTable("Defects");
    }
}