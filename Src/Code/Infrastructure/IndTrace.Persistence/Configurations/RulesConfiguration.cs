// <copyright file="RulesConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the RulesConfiguration.
/// </summary>

public class RulesConfiguration : IEntityTypeConfiguration<Rule>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Rule> builder)
    {
        // Apply the base configuration
        new AuditableEntityConfiguration<Rule>().Configure(builder);

        builder.HasKey(e => e.RuleId)
            .HasName("PK_Rules");

        builder.Property(e => e.RuleId)
            .HasColumnName(nameof(Rule.RuleId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.RuleJson)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(8000);

        builder.Property(e => e.Name)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Description)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(120);

        // Story 35.D2 Cluster 5 (#35): Rule.MachineId is the strongly-typed MachineId struct, mapped to the SAME
        // unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's converted
        // key). FK.IndTraceData.Rules.Machines is allow-listed KNOWN-DRIFT (absent in QA45) but stays modeled.
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(Rule.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(r => r.MachineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.Rules.Machines");

        // Story 35.D2 Cluster 4 (#35): Rule.ProductId is the strongly-typed ProductId struct, mapped to the SAME
        // unchanged int column via the shared byte-preserving converter, making the FK type-compatible with Product's
        // converted principal key (an int FK targeting a ProductId PK detonates the model). The modeled relationship is
        // KEPT (adopt-typed, never drop); FK.IndTraceData.Rules.Products is allow-listed KNOWN-DRIFT (absent in QA45).
        builder.Property(e => e.ProductId)
            .HasColumnName(nameof(Rule.ProductId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new ProductId(v))
            .IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.Rules.Products");

        builder.Property(e => e.Version)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();
        // Ignore complex properties that are not mapped to the database
        builder.Ignore(r => r.Components);
        builder.Ignore(r => r.RuleFunction);

        builder.ToTable("Rules");
    }
}