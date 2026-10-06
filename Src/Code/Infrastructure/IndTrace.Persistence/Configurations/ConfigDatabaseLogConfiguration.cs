// <copyright file="ConfigDatabaseLogConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the ConfigDatabaseLogConfiguration.
/// </summary>

public class ConfigDatabaseLogConfiguration : IEntityTypeConfiguration<ConfigDatabaseLog>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<ConfigDatabaseLog> builder)
    {
        builder.HasKey(e => e.DatabaseLogId)
            .HasName("PK_DatabaseLog_DatabaseLogID")
            .IsClustered(false);

        builder.ToTable("Config.DatabaseLog");

        builder.Property(e => e.DatabaseLogId).HasColumnName(nameof(ConfigDatabaseLog.DatabaseLogId));

        builder.Property(e => e.DatabaseUser)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.Event)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.Object).HasMaxLength(128);

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core datetime2 type fix - PostTime needs datetime2 instead of datetime for precision
        builder.Property(e => e.PostTime).HasColumnType("datetime2");

        builder.Property(e => e.Schema).HasMaxLength(128);

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core MaxLength constraint fix - Tsql and XmlEvent missing MaxLength configuration
        builder.Property(e => e.Tsql)
            .IsRequired()
            .HasColumnName(nameof(ConfigDatabaseLog.Tsql))
            .HasMaxLength(8000);

        builder.Property(e => e.XmlEvent)
            .IsRequired()
            .HasColumnType("xml")
            .HasMaxLength(8000);
    }
}