// <copyright file="ShiftsConfiguration.cs" company="Exxerpro Solutions SA de CV">
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
/// Represents the ShiftsConfiguration.
/// </summary>

public class ShiftsConfiguration : IEntityTypeConfiguration<Shift>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        // Apply the base configuration
        new AuditableEntityConfiguration<Shift>().Configure(builder);

        builder.HasKey(e => e.ShiftId)
            .HasName("PK.IndTraceData.Shifts.ShiftID");

        // Story 35.D2 (#35, Cluster 2): Shift.ShiftId is the strongly-typed ShiftId value struct, mapped to the SAME
        // unchanged int identity column via the shared value-preserving converter (model -> DB via id.Value; DB ->
        // model via new ShiftId(value)). No rename/schema change — the stored column stays byte-identical. The
        // converter rides the entity KEY; the generated identity round-trips through id.Value.
        builder.Property(e => e.ShiftId)
            .HasColumnName(nameof(Shift.ShiftId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new ShiftId(v))
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.StartBy)
            .HasColumnName(nameof(Shift.StartBy))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.Property(e => e.EndTime)
            .HasColumnName(nameof(Shift.EndTime))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core MaxLength constraint fix - Shift.ShiftType property missing configuration
        builder.Property(e => e.ShiftType)
            .HasColumnName(nameof(Shift.ShiftType))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(50);

        // [Fix]
        // Chunk 2c (issue #50)
        // Reason: PO decision — the live dbo.Shifts has no "Type" column. Shift.Type is now a
        // derived (never-persisted) projection of the string ShiftType, so it must NOT be mapped.
        // Mapping it previously broke every real-SQL insert with "Invalid column name 'Type'".
        builder.Ignore(e => e.Type);

        // [Fix]
        // Chunk 2c (issue #50)
        // Reason: Shifts are tracked per machine. MachineId is a plain int column (no FK) backed by
        // the live unique index UQ.IndTraceData.Shifts.MachineId_StartBy on (MachineId, StartBy).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(Shift.MachineId))
            .HasColumnType("int")
            .IsRequired();

        // TODO(#50): live-DB index is still physically named UQ.IndTraceData.Shifts.MachineId_StartBy;
        // the model uses the house UX. prefix (matches the dotted identifier scheme). Renaming the
        // physical index UQ.->UX. is deferred to a future hand-authored migration.
        builder.HasIndex(e => new { e.MachineId, e.StartBy })
            .IsUnique()
            .HasDatabaseName("UX.IndTraceData.Shifts.MachineId_StartBy");

        builder.ToTable("Shifts");
    }
}