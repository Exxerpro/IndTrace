// <copyright file="UsersConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the UsersConfiguration.
/// </summary>

public class UsersConfiguration : IEntityTypeConfiguration<IndTraceUser>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<IndTraceUser> builder)
    {
        builder.HasKey(e => e.UserId)
            .HasName("PK_Users");

        builder.Property(e => e.UserName)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(120);

        builder.Property(e => e.UserId)
            .HasColumnName(nameof(IndTraceUser.UserId))
            .ValueGeneratedNever();

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core AuditableEntity configuration fix - IndTraceUser inherits from AuditableEntity
        new AuditableEntityConfiguration<IndTraceUser>().Configure(builder);

        builder.ToTable("Users");
    }
}