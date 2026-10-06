// <copyright file="VariablesGroupsConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the VariablesGroupsConfiguration.
/// </summary>

public class VariablesGroupsConfiguration : IEntityTypeConfiguration<VariablesGroup>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<VariablesGroup> builder)
    {
        builder.HasKey(e => e.VariableGroupId)
            .HasName("PK.IndTraceData.VariablesGroups.VariableGroupId");

        builder.Property(e => e.VariableGroupId)
            .HasColumnName(nameof(VariablesGroup.VariableGroupId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.VariableGroupName)
            .HasColumnName(nameof(VariablesGroup.VariableGroupName))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.ToTable("VariablesGroups");
    }
}