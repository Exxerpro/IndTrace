// <copyright file="AuditableEntityConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the AuditableEntityConfiguration.
/// </summary>

public class AuditableEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : AuditableEntity
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(e => e.CreatedBy)
            .HasColumnName(nameof(AuditableEntity.CreatedBy))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.ModifiedBy)
            .HasColumnName(nameof(AuditableEntity.ModifiedBy))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.CreatedOn)
            .HasColumnName(nameof(AuditableEntity.CreatedOn))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.Property(e => e.ModifiedOn)
            .HasColumnName(nameof(AuditableEntity.ModifiedOn))
            .HasColumnType("datetime2(7)");
    }
}
