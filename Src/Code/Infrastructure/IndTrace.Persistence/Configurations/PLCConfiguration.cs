// <copyright file="PLCConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the PlcConfiguration.
/// </summary>

public class PlcConfiguration : IEntityTypeConfiguration<Plc>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Plc> builder)
    {
        builder.HasKey(e => e.PlcId)
            .HasName("PK.IndTraceData.Plcs.PlcId");

        builder.Property(e => e.PlcId)
            .HasColumnName(nameof(Plc.PlcId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName(nameof(Plc.Name))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.PlcBrand)
            .HasColumnName(nameof(Plc.PlcBrand))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.IpAddress)
            .HasColumnName(nameof(Plc.IpAddress))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.PlcType)
            .HasColumnName(nameof(Plc.PlcType))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Options)
            .HasColumnName(nameof(Plc.Options))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.CommLibrary)
            .HasColumnName(nameof(Plc.CommLibrary))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.BrandOwner)
            .HasColumnName(nameof(Plc.BrandOwner))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);


        // EF Core value converter: persist the ActiveStatus smart enum as the int column "Enabled"
        // (column type and stored values -1/0/1 are unchanged; behavior is byte-identical). The
        // store-generated default stays "enabled" (ActiveStatus.Active → 1) as before.
        builder.Property(e => e.Enabled)
            .HasColumnName(nameof(Plc.Enabled))
            .HasColumnType("int")
            .HasDefaultValue(ActiveStatus.Active)
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : ActiveStatus.Invalid.Value,  // To DB: ActiveStatus → int (null → Invalid sentinel int.MinValue, never 0/None — #117 F4)
                v => (ActiveStatus)v            // From DB: int → ActiveStatus (implicit; int.MinValue → Invalid)
            );

        builder.HasIndex(e => e.PlcId)
            .HasDatabaseName("IDX.IndTraceData.Plcs.PlcId")
            .IsUnique();

        builder.ToTable("Plcs");
    }
}