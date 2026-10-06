// <copyright file="DefectsRegisterConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the DefectsRegisterConfiguration.
/// </summary>

public class DefectsRegisterConfiguration : IEntityTypeConfiguration<DefectRegister>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<DefectRegister> builder)
    {
        // Configure primary key following safe refactoring pattern: {nameof(DefectRegister) + "RegisterId"}
        builder.HasKey(e => e.DefectRegisterId)
            .HasName("PK.IndTraceData.DefectsRegister.DefectRegisterId");

        builder.Property(e => e.DefectRegisterId)
            .HasColumnName(nameof(DefectRegister.DefectRegisterId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Story 35.D2 Cluster 1 (#35): DefectRegister.BarCodeId is the strongly-typed BarCodeId struct, mapped to the
        // SAME unchanged int column via the shared byte-preserving converter so the modeled FK below (into BarCode's
        // converted principal key) can be restored typed.
        builder.Property(e => e.BarCodeId)
            .HasColumnName(nameof(DefectRegister.BarCodeId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new BarCodeId(v))
            .IsRequired();

        builder.Property(e => e.Comment)
            .HasColumnName(nameof(DefectRegister.Comment))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.DefectId)
            .HasColumnName(nameof(DefectRegister.DefectId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName(nameof(DefectRegister.Description))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        // Story 35.D2 Cluster 5 (#35): DefectRegister.MachineId is the strongly-typed MachineId struct, mapped to the
        // SAME unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's key).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(DefectRegister.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core Precision/Scale fix - Use HasPrecision instead of HasColumnType for better validation
        builder.Property(e => e.PartsQuantity)
            .HasColumnName(nameof(DefectRegister.PartsQuantity))
            .HasPrecision(18, 4);

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core byte[] configuration fix - TimeStamp needs explicit column type for rowversion
        builder.Property(e => e.TimeStamp)
            .HasColumnName(nameof(DefectRegister.TimeStamp))
            .IsRequired()
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnType("rowversion");

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core datetime2 type fix - DefectRegister has CreatedOn/ModifiedOn DateTime properties missing configuration
        builder.Property(e => e.CreatedOn)
            .HasColumnName(nameof(DefectRegister.CreatedOn))
            .HasColumnType("datetime2");

        builder.Property(e => e.ModifiedOn)
            .HasColumnName(nameof(DefectRegister.ModifiedOn))
            .HasColumnType("datetime2");

        // Story 35.D2 Cluster 1 (#35): the modeled FK DefectRegister.BarCodeId -> BarCode.BarCodeId is RESTORED, now
        // that both sides are the strongly-typed BarCodeId struct behind a value converter (D1 had dropped it because
        // an int FK cannot target the converted principal key). Re-arms the model->DB schema-drift guard's assertion
        // that the physical FK.IndTraceData.DefectsRegister.BarCodeId exists in live QA45 (the D1-review coverage gap).
        builder.HasOne<BarCode>()
            .WithMany()
            .HasForeignKey(d => d.BarCodeId)
            .HasConstraintName("FK.IndTraceData.DefectsRegister.BarCodeId");

        builder.HasOne<Defect>()
            .WithMany()
            .HasForeignKey(d => d.DefectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.DefectsRegister.DefectId");

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(d => d.MachineId)
            .HasConstraintName("FK.IndTraceData.DefectsRegister.MachineId");

        builder.ToTable("DefectsRegister");
    }
}