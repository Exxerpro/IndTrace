// <copyright file="ConfigDbConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the ConfigDbConfiguration.
/// </summary>

public class ConfigDbConfiguration : IEntityTypeConfiguration<ConfigDb>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<ConfigDb> builder)
    {
        // IMPORTANT: Non-standard primary key name
        // SystemInformationId does not follow convention of {nameof(ConfigDb) + "RegisterId"}
        // This should be reviewed and potentially renamed to ConfigDbId
        builder.HasKey(e => e.SystemInformationId);

        // rationale: legacy DB column is literally "Database Version" (with a space); it deliberately
        // differs from the DatabaseVersion property name, so nameof() cannot be used here.
        builder.Property(e => e.DatabaseVersion)
            .IsRequired()
            .HasColumnName("Database Version")
            .HasMaxLength(80);

        builder.Property(e => e.SystemInformationId)
            .HasColumnName(nameof(ConfigDb.SystemInformationId))
            .ValueGeneratedOnAdd();

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core datetime2 type fix - ModifiedDate and VersionDate DateTime properties missing configuration
        builder.Property(e => e.ModifiedDate)
            .HasColumnName(nameof(ConfigDb.ModifiedDate))
            .HasColumnType("datetime2");

        builder.Property(e => e.VersionDate)
            .HasColumnName(nameof(ConfigDb.VersionDate))
            .HasColumnType("datetime2");

        builder.ToTable("ConfigDbs");
    }
}