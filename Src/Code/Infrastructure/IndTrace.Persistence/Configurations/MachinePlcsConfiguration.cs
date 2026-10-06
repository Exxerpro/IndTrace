// <copyright file="MachinePlcsConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;

public class MachinePlcsConfiguration : IEntityTypeConfiguration<MachinePlc>

{
    public void Configure(EntityTypeBuilder<MachinePlc> builder)
    {

        // Apply the base configuration
        new AuditableEntityConfiguration<MachinePlc>().Configure(builder);

        builder.HasKey(t => new { MachineId = t.MachineId, PlcId = t.PlcId });

        // Story 35.D2 Cluster 5 (#35): MachinePlc.MachineId is the strongly-typed MachineId struct (part of the
        // composite key), mapped to the SAME unchanged int column via the shared byte-preserving converter so the
        // key property and the modeled FK to Machine stay type-compatible with Machine's converted principal key.
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(MachinePlc.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v));

        // EF Core value converter: persist the ActiveStatus smart enum as the int column "IsActive"
        // (column type and stored values -1/0/1 are unchanged; behavior is byte-identical).
        builder.Property(e => e.IsActive)
            .HasColumnName(nameof(MachinePlc.IsActive))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : ActiveStatus.Invalid.Value,  // To DB: ActiveStatus → int (null → Invalid sentinel int.MinValue, never 0/None — #117 F4)
                v => (ActiveStatus)v            // From DB: int → ActiveStatus (implicit; int.MinValue → Invalid)
            );

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(pt => pt.MachineId);

        builder.HasOne<Plc>()
            .WithMany()
            .HasForeignKey(pt => pt.PlcId);

        builder.ToTable("MachinePlcs");
    }
}