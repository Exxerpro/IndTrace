// <copyright file="ShiftsCatalogConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace IndTrace.Persistence.Configurations;

// rationale: ShiftsCatalog is the sole CLR type mapped to the "ShiftsCatalog" table. It is applied
// explicitly first in OnModelCreating and excluded from the ApplyConfigurationsFromAssembly scan so it
// is mapped exactly once. (A former duplicate type, ShiftDefinition, that also mapped to this table was
// retired in #50 chunk 3; see docs/handbook/ef-configuration-conventions.md.)
public class ShiftsCatalogConfiguration : IEntityTypeConfiguration<ShiftsCatalog>
{
    public void Configure(EntityTypeBuilder<ShiftsCatalog> builder)
    {
        // Apply the base configuration for auditable properties
        new AuditableEntityConfiguration<ShiftsCatalog>().Configure(builder);

        builder.HasKey(e => e.ShiftCatalogId);

        builder.Property(e => e.ShiftCatalogId)
              .ValueGeneratedOnAdd();

        builder.Property(e => e.PlantId)
              .IsRequired();

        builder.Property(e => e.ShiftName)
              .IsRequired()
              .IsUnicode(true)
              .HasMaxLength(100);

        builder.Property(e => e.StartBy)
              .IsRequired()
              .HasColumnType("time");

        builder.Property(e => e.Duration)
              .IsRequired()
              .HasColumnType("time");

        builder.Property(e => e.EndTime)
              .IsRequired()
              .HasColumnType("time");

        builder.ToTable("ShiftsCatalog");
    }
}

//[Fix] 
//CLAUDE
//Date: 02/09/2025 
//Reason: [VALIDATOR MISFIRING] - Fixed reflection caching issue by using AuditableEntityConfiguration base class instead of manual property configuration