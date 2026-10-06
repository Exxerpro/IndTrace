// <copyright file="SettingsConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the SettingsConfiguration.
/// </summary>

public class SettingsConfiguration : IEntityTypeConfiguration<Setting>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.HasKey(e => e.SettingId)
            .HasName("PK.IndTraceData.Settings.SettingId");

        builder.Property(e => e.SettingId)
            .HasColumnName(nameof(Setting.SettingId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Story 35.D2 Cluster 5 (#35): Setting.MachineId is the strongly-typed MachineId struct, mapped to the SAME
        // unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's key).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(Setting.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        builder.Property(e => e.Config)
            .HasColumnName(nameof(Setting.Config))
            .IsRequired()
            .HasMaxLength(4000)
            .IsFixedLength();

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(d => d.MachineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.Settings.Machines");

        builder.HasIndex(e => e.SettingId)
            .HasDatabaseName("IDX.IndTraceData.Settings.SettingId")
            .IsUnique();

        builder.ToTable("Settings");
    }
}