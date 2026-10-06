// <copyright file="StoppegesConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the StoppegesConfiguration.
/// </summary>

public class StoppegesConfiguration : IEntityTypeConfiguration<Stoppage>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Stoppage> builder)
    {
        builder.HasKey(e => e.StoppageId)
            .HasName("PK.IndTraceData.Stoppages.StoppageId");

        builder.Property(e => e.StoppageId)
            .HasColumnName(nameof(Stoppage.StoppageId))
            .HasColumnType("int")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName(nameof(Stoppage.Description))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.Description2)
            .HasColumnName(nameof(Stoppage.Description2))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.MaxValue)
            .HasColumnName(nameof(Stoppage.MaxValue))
            .HasPrecision(18, 4);

        builder.Property(e => e.MinValue)
            .HasColumnName(nameof(Stoppage.MinValue))
            .HasPrecision(18, 4);

        builder.Property(e => e.StoppageName)
            .HasColumnName(nameof(Stoppage.StoppageName))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.ShortName)
            .HasColumnName(nameof(Stoppage.ShortName))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.StoppageTypeId)
            .HasColumnName(nameof(Stoppage.StoppageTypeId))
            .HasColumnType("int")
            .IsRequired();

        builder.ToTable("Stoppages");
    }
}