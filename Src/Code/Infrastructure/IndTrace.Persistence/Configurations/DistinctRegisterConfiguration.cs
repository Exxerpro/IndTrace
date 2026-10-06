// <copyright file="DistinctRegisterConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the DistinctRegisterConfiguration.
/// </summary>

public class DistinctRegisterConfiguration : IEntityTypeConfiguration<DistinctRegister>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<DistinctRegister> builder)
    {
        // Configure the primary key
        builder.HasKey(dr => new { dr.Name, VariableID = dr.VariableId, dr.MachineId });

        // Configure the Name column
        builder.Property(dr => dr.Name)
            .IsRequired()
            .HasMaxLength(255);

        // Configure the VariableID column
        builder.Property(dr => dr.VariableId)
            .IsRequired();

        // Configure the MachineId column
        builder.Property(dr => dr.MachineId)
            .IsRequired();

        // Set the table name explicitly. Schema omitted to match every other config (dbo is the
        // default schema, so "DistinctRegisters" and dbo.DistinctRegisters resolve identically).
        builder.ToTable("DistinctRegisters");
    }
}