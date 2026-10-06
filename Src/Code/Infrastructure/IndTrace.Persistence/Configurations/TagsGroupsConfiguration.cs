// <copyright file="TagsGroupsConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum.LookUpTable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the TagsGroupsConfiguration.
/// </summary>

public class TagsGroupsConfiguration : IEntityTypeConfiguration<TagsGroupEntity>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<TagsGroupEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName(nameof(TagsGroupEntity.Id))
            .HasColumnType("int")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .HasColumnName(nameof(TagsGroupEntity.Name))
            .HasColumnType("nvarchar")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired()
            .ValueGeneratedNever();

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core validation fix - Missing MaxLength constraint for TagsGroupEntity.DisplayName property
        builder.Property(e => e.DisplayName)
            .HasColumnName(nameof(TagsGroupEntity.DisplayName))
            .HasColumnType("nvarchar")
            .HasMaxLength(120)
            .IsUnicode(true)
            .IsRequired()
            .ValueGeneratedNever();

        builder.ToTable("TagsGroups");
    }
}