// <copyright file="BarCodeConfiguration.cs" company="Exxerpro Solutions SA de CV">
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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the BarCodeConfiguration.
/// </summary>

public class BarCodeConfiguration : IEntityTypeConfiguration<BarCode>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<BarCode> builder)
    {
        // Configure primary key following safe refactoring pattern: {nameof(BarCode) + "RegisterId"}
        builder.HasKey(e => e.BarCodeId)
            .HasName("PK.IndTraceData.BarCodes.BarCodeId");

        // Story 35.D1 (#35/F13): BarCode.BarCodeId is the strongly-typed BarCodeId value struct, mapped to the SAME
        // unchanged int identity column via a value-preserving EF converter (model -> DB via id.Value; DB -> model via
        // new BarCodeId(value)). No rename/schema change — the stored column and §7 wire bytes stay byte-identical.
        // The converter rides the entity KEY; the generated identity round-trips through id.Value.
        builder.Property(e => e.BarCodeId)
            .HasColumnName(nameof(BarCode.BarCodeId))
            .HasColumnType("int")
            .HasConversion(
                id => id.Value,               // To DB: BarCodeId -> int (byte-preserving)
                value => new BarCodeId(value)) // From DB: int -> BarCodeId (total, never throws)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Story 35.D2 Cluster 4 (#35): BarCode.ProductId is the strongly-typed ProductId struct, mapped to the SAME
        // unchanged int column via the shared byte-preserving converter, making the FK type-compatible with Product's
        // converted principal key (an int FK targeting a ProductId PK detonates the model). The modeled relationship
        // (FK.IndTraceData.BarCodes.Products) is KEPT (adopt-typed, never drop). MachineId stays int for Cluster 5.
        builder.Property(e => e.ProductId)
            .HasColumnName(nameof(BarCode.ProductId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new ProductId(v))
            .IsRequired();

        // Story 35.D2 Cluster 5 (#35): BarCode.MachineId is the strongly-typed MachineId struct, mapped to the SAME
        // unchanged int column via the shared byte-preserving converter, making the FK type-compatible with Machine's
        // converted principal key. The modeled relationship (FK.IndTraceData.BarCodes.Machines) is KEPT (adopt-typed).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(BarCode.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        // Story 27.2b-2 (#27/F4): BarCode.Label (the BarCodeLabel value object) is mapped to the SAME nvarchar(80)
        // column (no schema change) via an EF value converter: model -> DB via label.Value (byte-preserving), DB ->
        // model via BarCodeLabel.FromPersisted (total, never throws/normalizes — a legacy Label='' row materializes
        // without detonating the query). The ValueComparer change-tracks by .Value, snapshots by identity.
        //
        // WHY a converter and not an owned/complex type: this DbContext's relationship-discovery convention eagerly
        // registers BarCodeLabel as an entity type as soon as BarCode enters the model, so ComplexProperty is rejected
        // by EnforceModelConfiguration ("BarCodeLabel must implement IEntityRoot"), and an index on a complex-type
        // property is unsupported — only Property()/HasConversion can reconfigure the discovered member into a scalar
        // (empirically confirmed on real SQL, Story 27.2b-2). Consequence: EF value converters translate ONLY
        // whole-property equality/IN (VO-to-VO: b.Label == labelVo), NOT member access (b.Label.Value) or LIKE. The
        // Specification/EF predicate sweep therefore compares against a BarCodeLabel, and the Reports composer's
        // substring filter is applied SERVER-SIDE via a parameterized FromSql LIKE at the query root (see
        // ReportsListQueryComposer.ReRootWithLabelSubstring).
        builder.Property(e => e.Label)
            .HasColumnName(nameof(BarCode.Label))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80)
            .HasConversion(
                label => label.Value,                       // To DB: BarCodeLabel -> string (byte-preserving)
                value => BarCodeLabel.FromPersisted(value)) // From DB: string -> BarCodeLabel (total, no throw)
            .Metadata.SetValueComparer(
                new ValueComparer<BarCodeLabel>(
                    (left, right) => (left == null && right == null)
                        || (left != null && right != null && left.Value == right.Value),
                    label => label == null ? 0 : label.Value.GetHashCode(StringComparison.Ordinal),
                    label => label));

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core value converter fix - Convert EnumModel smart enums to int for database storage
        builder.Property(e => e.PartStatus)
            .HasColumnName(nameof(BarCode.PartStatus))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : PartStatus.Invalid.Value,  // To DB: PartStatus → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => (PartStatus)v             // From DB: int → PartStatus (implicit)
            );

        builder.Property(e => e.FlowStatus)
            .HasColumnName(nameof(BarCode.FlowStatus))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : FlowStatus.Invalid.Value,  // To DB: FlowStatus → int (null → Invalid sentinel 8, never 0/None — #117 F4)
                v => (FlowStatus)v             // From DB: int → FlowStatus (implicit)
            );

        builder.Property(e => e.CreatedOn)
            .HasColumnName(nameof(BarCode.CreatedOn))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.Property(e => e.ModifiedOn)
            .HasColumnName(nameof(BarCode.ModifiedOn))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        // Story 40-B (#40 B2): BarCode is one of the two rows a cycle-OK/NotOk operation mutates in place, so it
        // carries a rowversion optimistic-concurrency token (the StoppageRegister precedent). The aggregate
        // repository (Chunk 40-C) attaches the loaded value as the concurrency original so a competing writer is
        // surfaced as a Result failure rather than a lost update. DB-only column, off the §7 PLC wire.
        builder.Property(e => e.RowVersion)
            .HasColumnName(nameof(BarCode.RowVersion))
            .IsRequired()
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnType("rowversion");

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(e => e.MachineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.BarCodes.Machines");

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.BarCodes.Products");

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core foreign key fix - Remove foreign key relationships for smart enums to avoid type compatibility issues
        // Note: Lookup tables exist for SQL developers/admins, but EF Core foreign keys cause type mismatch
        // Smart enums provide the business logic functionality without needing EF Core relationships

        builder.HasIndex(e => e.BarCodeId)
            .HasDatabaseName("IDX.IndTraceData.BarCodes.BarCodeId")
            .IsUnique();

        builder.HasIndex(e => e.Label)
            .HasDatabaseName("IDX.IndTraceData.BarCodes.Label")
            .IsUnique();

        builder.HasIndex(r => r.CreatedOn)
            .HasDatabaseName("IDX.IndTraceData.BarCodes.CreatedOn");

        builder.ToTable("BarCodes");
    }
}