// <copyright file="FlowTransitionLogConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;

/// <summary>
/// Story 3.5 (AC1) EF configuration for the additive <see cref="FlowTransitionLog"/> table. Mirrors the
/// established smart-enum mapping (<c>HasConversion</c> to the frozen <c>int</c> .Value) used by
/// <see cref="TaskRequestEventConfiguration"/> so the stored numerics match the PLC contract exactly. This
/// configuration creates a NEW, stand-alone table only — it touches no existing entity mapping (CR2/NFR2).
/// </summary>
public class FlowTransitionLogConfiguration : IEntityTypeConfiguration<FlowTransitionLog>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<FlowTransitionLog> builder)
    {
        builder.HasKey(e => e.FlowTransitionLogId)
            .HasName("PK_FlowTransitionLog_FlowTransitionLogId");

        builder.Property(e => e.FlowTransitionLogId)
            .HasColumnName(nameof(FlowTransitionLog.FlowTransitionLogId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.From)
            .HasColumnName(nameof(FlowTransitionLog.From))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : FlowStatus.Invalid.Value,  // To DB: FlowStatus → int (null → Invalid sentinel 8, never 0/None — #117 F4)
                v => (FlowStatus)v             // From DB: int → FlowStatus (implicit)
            );

        builder.Property(e => e.To)
            .HasColumnName(nameof(FlowTransitionLog.To))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : FlowStatus.Invalid.Value,  // null → Invalid sentinel 8, never 0/None (#117 F4)
                v => (FlowStatus)v
            );

        builder.Property(e => e.FromCycleStatus)
            .HasColumnName(nameof(FlowTransitionLog.FromCycleStatus))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : CycleStatus.Invalid.Value,  // To DB: CycleStatus → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => (CycleStatus)v            // From DB: int → CycleStatus (implicit)
            );

        builder.Property(e => e.Trigger)
            .HasColumnName(nameof(FlowTransitionLog.Trigger))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : GatewayTask.Invalid.Value,  // To DB: GatewayTask → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => (GatewayTask)v            // From DB: int → GatewayTask (implicit)
            );

        builder.Property(e => e.Path)
            .HasColumnName(nameof(FlowTransitionLog.Path))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion<int>(); // Plain enum → its underlying int (DB-only, never a PLC tag)

        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(FlowTransitionLog.MachineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.BarCodeId)
            .HasColumnName(nameof(FlowTransitionLog.BarCodeId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CycleId)
            .HasColumnName(nameof(FlowTransitionLog.CycleId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.ResultValidation)
            .HasColumnName(nameof(FlowTransitionLog.ResultValidation))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : ResultValidation.Invalid.Value,  // To DB: ResultValidation → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => (ResultValidation)v       // From DB: int → ResultValidation (implicit)
            );

        builder.Property(e => e.TimeStamp)
            .HasColumnName(nameof(FlowTransitionLog.TimeStamp))
            .HasColumnType("datetime2");

        // Diagnostic query support (AC6): by MachineId / BarCodeId / time range.
        builder.HasIndex(e => e.MachineId)
            .HasDatabaseName("IDX_FlowTransitionLog_MachineId");

        builder.HasIndex(e => e.BarCodeId)
            .HasDatabaseName("IDX_FlowTransitionLog_BarCodeId");

        builder.HasIndex(e => e.TimeStamp)
            .HasDatabaseName("IDX_FlowTransitionLog_TimeStamp");

        builder.ToTable("FlowTransitionLog");
    }
}
