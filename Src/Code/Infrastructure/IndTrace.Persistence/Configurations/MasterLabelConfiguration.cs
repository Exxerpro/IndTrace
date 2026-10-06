// <copyright file="MasterLabelConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the MasterLabelConfiguration.
/// </summary>

public class MasterLabelConfiguration : IEntityTypeConfiguration<MasterLabel>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<MasterLabel> builder)
    {
        // Configure primary key following safe refactoring pattern
        builder.HasKey(e => e.MasterLabelId);

        builder.Property(e => e.MasterLabelId)
            .HasColumnName(nameof(MasterLabel.MasterLabelId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName(nameof(MasterLabel.Description))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.MasterLabelCode)
            .HasColumnName(nameof(MasterLabel.MasterLabelCode))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.ToTable("MasterLabel");
    }
}