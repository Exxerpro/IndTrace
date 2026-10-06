// <copyright file="ToolingConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the ToolingConfiguration.
/// </summary>

public class ToolingConfiguration : IEntityTypeConfiguration<Tooling>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Tooling> builder)
    {
        builder.HasKey(e => e.ToolId)
            .HasName("PK.IndTraceData.Toolings.ToolId");

        builder.Property(e => e.ToolId)
            .HasColumnName(nameof(Tooling.ToolId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName(nameof(Tooling.Name))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.ToTable("Toolings");



    }
}