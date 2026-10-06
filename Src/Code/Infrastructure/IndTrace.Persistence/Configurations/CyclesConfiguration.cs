// <copyright file="CyclesConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Enum.LookUpTable;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the CyclesConfiguration.
/// </summary>

public class CyclesConfiguration : IEntityTypeConfiguration<Cycle>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Cycle> builder)
    {
        // Configure primary key following safe refactoring pattern: {nameof(Cycle) + "RegisterId"}
        builder.HasKey(e => e.CycleId)
            .HasName("PK.IndTraceData.Cycles.CycleId");

        // Story 35.D2 Cluster 3 (#35): Cycle.CycleId is the strongly-typed CycleId struct KEY, mapped to the SAME
        // unchanged int identity column via the shared byte-preserving converter (model -> DB via id.Value; DB ->
        // model via new CycleId(value)). The generated identity round-trips through id.Value. Making the PK the
        // converted struct is what lets the inbound Register/PerformanceData FKs be adopted typed (an int FK targeting
        // a CycleId principal key detonates the model).
        builder.Property(e => e.CycleId)
            .HasColumnName(nameof(Cycle.CycleId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new CycleId(v))
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Story 35.D2 Cluster 5 (#35): Cycle.MachineId is the strongly-typed MachineId struct, mapped to the SAME
        // unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's converted
        // key). The modeled FK.IndTraceData.Cycles.Machines relationship is KEPT (adopt-typed).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(Cycle.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        // Story 35.D2 Cluster 1 (#35): Cycle.BarCodeId is the strongly-typed BarCodeId struct, mapped to the SAME
        // unchanged int column via the shared byte-preserving converter (model -> DB via id.Value; DB -> model via
        // new BarCodeId(value)). This makes the FK property type-compatible with BarCode's converted principal key so
        // the modeled relationship below can be restored (an int FK targeting a BarCodeId PK detonates the model).
        builder.Property(e => e.BarCodeId)
            .HasColumnName(nameof(Cycle.BarCodeId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new BarCodeId(v))
            .IsRequired();

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core value converter fix - Convert EnumModel smart enums to int for database storage
        builder.Property(e => e.CycleStatus)
            .HasColumnName(nameof(Cycle.CycleStatus))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : CycleStatus.Invalid.Value,  // To DB: CycleStatus → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => (CycleStatus)v            // From DB: int → CycleStatus (implicit)
            );

        builder.Property(e => e.PartStatus)
            .HasColumnName(nameof(Cycle.PartStatus))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : PartStatus.Invalid.Value,  // To DB: PartStatus → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => (PartStatus)v             // From DB: int → PartStatus (implicit)
            );

        builder.Property(e => e.CycleTime)
            .HasColumnName(nameof(Cycle.CycleTime))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.TaktTime)
            .HasColumnName(nameof(Cycle.TaktTime))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.StartedOn)
            .HasColumnName(nameof(Cycle.StartedOn))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.Property(e => e.FinishedOn)
            .HasColumnName(nameof(Cycle.FinishedOn))
            .HasColumnType("datetime2(7)");

        // Story 40-B (#40 B2): Cycle is one of the two rows a cycle-OK/NotOk operation mutates in place, so it
        // carries a rowversion optimistic-concurrency token (the StoppageRegister precedent). The aggregate
        // repository (Chunk 40-C) attaches the loaded value as the concurrency original so a competing writer is
        // surfaced as a Result failure rather than a lost update. DB-only column, off the §7 PLC wire.
        builder.Property(e => e.RowVersion)
            .HasColumnName(nameof(Cycle.RowVersion))
            .IsRequired()
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnType("rowversion");

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(c => c.MachineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.Cycles.Machines");

        // Story 35.D2 Cluster 1 (#35): the modeled FK Cycle.BarCodeId -> BarCode.BarCodeId is RESTORED, now that both
        // sides are the strongly-typed BarCodeId struct behind a value converter (D1 had dropped it because an int FK
        // cannot target the converted principal key). Re-arms the model->DB schema-drift guard's assertion that the
        // physical FK.IndTraceData.Cycles.BarCodes exists in live QA45 (the D1-review coverage gap).
        builder.HasOne<BarCode>()
            .WithMany()
            .HasForeignKey(c => c.BarCodeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.Cycles.BarCodes");

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core foreign key fix - Remove foreign key relationships for smart enums to avoid type compatibility issues
        // Note: Lookup tables exist for SQL developers/admins, but EF Core foreign keys cause type mismatch
        // Smart enums provide the business logic functionality without needing EF Core relationships

        builder.HasIndex(e => e.CycleId)
            .HasDatabaseName("IDX.IndTraceData.Cycles.CycleId")
            .IsUnique();

        builder.HasIndex(e => e.BarCodeId)
            .HasDatabaseName("IDX.IndTraceData.Cycles.BarCodeId");

        builder.ToTable("Cycles");
    }
}