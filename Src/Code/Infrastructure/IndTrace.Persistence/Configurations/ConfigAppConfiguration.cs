// <copyright file="ConfigAppConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the ConfigAppConfiguration.
/// </summary>

public class ConfigAppConfiguration : IEntityTypeConfiguration<ConfigApp>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<ConfigApp> builder)
    {
        builder.HasKey(e => e.AppId)
            .HasName("PK.IndTraceData.ConfigApps.AppId");

        builder.Property(e => e.AppId)
            .HasColumnName(nameof(ConfigApp.AppId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(ConfigApp.MachineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.PlcId)
            .HasColumnName(nameof(ConfigApp.PlcId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.Client)
            .HasColumnName(nameof(ConfigApp.Client))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Factory)
            .HasColumnName(nameof(ConfigApp.Factory))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Line)
            .HasColumnName(nameof(ConfigApp.Line))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Project)
            .HasColumnName(nameof(ConfigApp.Project))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Machine)
            .HasColumnName(nameof(ConfigApp.Machine))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Pc)
            .HasColumnName(nameof(ConfigApp.Pc))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Version)
            .HasColumnName(nameof(ConfigApp.Version))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core MaxLength constraint fix - ConfigAppId string property missing MaxLength configuration
        builder.Property(e => e.ConfigAppId)
            .HasColumnName(nameof(ConfigApp.ConfigAppId))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(50);

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: Apply AuditableEntity configuration pattern - ConfigApp inherits from AuditableEntity
        new AuditableEntityConfiguration<ConfigApp>().Configure(builder);

        builder.Property(e => e.CreatedOn)
            .HasColumnName(nameof(ConfigApp.CreatedOn))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.Property(e => e.ModifiedOn)
            .HasColumnName(nameof(ConfigApp.ModifiedOn))
            .HasColumnType("datetime2(7)");

        builder.HasIndex(e => e.AppId)
            .HasDatabaseName("IDX.IndTraceData.ConfigApps.AppId")
            .IsUnique();

        builder.ToTable("ConfigApps");
    }
}