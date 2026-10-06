// <copyright file="MachinesConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Enum.LookUpTable;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the MachinesConfiguration.
/// </summary>

public class MachinesConfiguration : IEntityTypeConfiguration<Machine>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Machine> builder)
    {
        // Configure primary key following safe refactoring pattern: {nameof(Machine) + "RegisterId"}
        builder.HasKey(e => e.MachineId)
            .HasName("PK.IndTraceData.Machines.MachineId");

        // Story 35.D2 Cluster 5 (#35): Machine.MachineId is the strongly-typed MachineId struct KEY, mapped to the SAME
        // unchanged int key column via the shared byte-preserving converter (model -> DB via id.Value; DB -> model via
        // new MachineId(value)). The key is CALLER-SUPPLIED (ValueGeneratedNever), not identity-generated, so the raw
        // int round-trips through id.Value unchanged. Making the PK the converted struct is what lets the fourteen
        // inbound FKs be adopted typed (an int FK targeting a MachineId principal key detonates the model).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(Machine.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Ignore(e => e.Result);
        builder.Ignore(e => e.IsEnabled);
        builder.Ignore(e => e.TraceabilityMode);

        builder.Property(e => e.Name)
            .HasColumnName(nameof(Machine.Name))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        //4 Aug 2025
        //Corrected the name of the column to match the property name
        //builder.Property(e => e.Name) was the previous name
        //this was causing the table was two columns with almost the same name Description and Description1
        //and many confusions along the way

        builder.Property(e => e.Description)
            .HasColumnName(nameof(Machine.Description))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Location)
            .HasColumnName(nameof(Machine.Location))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core value converter fix - Convert EnumModel smart enums to int for database storage
        builder.Property(e => e.MachineType)
            .HasColumnName(nameof(Machine.MachineType))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v.Value,  // To DB: MachineType → int
                v => EnumModel.FromValue<MachineType>(v)  // From DB: int → MachineType (FIXED)
            );

        builder.Property(e => e.WorkFlowType)
            .HasColumnName(nameof(Machine.WorkFlowType))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : WorkFlowType.Invalid.Value,  // To DB: WorkFlowType → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => EnumModel.FromValue<WorkFlowType>(v) // From DB: int → WorkFlowType (FIXED)
            );

        builder.Property(e => e.EnableAppTraceability)
            .HasColumnName(nameof(Machine.EnableAppTraceability))
            .HasColumnType("int")
            .IsRequired()
            .HasDefaultValueSql("((1))");

        builder.Property(e => e.EnableBypassTraceability)
            .HasColumnName(nameof(Machine.EnableBypassTraceability))
            .HasColumnType("int")
            .IsRequired()
            .HasDefaultValueSql("((0))");

        builder.Property(e => e.Retry)
            .HasColumnName(nameof(Machine.Retry))
            .HasColumnType("int")
            .IsRequired()
            .HasDefaultValueSql("((1))");

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core foreign key fix - Remove foreign key relationships for smart enums to avoid type compatibility issues
        // Note: Lookup tables exist for SQL developers/admins, but EF Core foreign keys cause type mismatch
        // Smart enums provide the business logic functionality without needing EF Core relationships

        //builder.HasOne<Rule>()
        //    .WithOne()
        //    .HasForeignKey<Rule>(r => r.RuleId)
        //    .OnDelete(DeleteBehavior.Restrict)
        //    .HasConstraintName("FK.IndTraceData.Machines.Rules.RuleId");

        builder.Property(e => e.RuleId)
            .HasColumnName(nameof(Machine.RuleId))
            .HasColumnType("int")
            .IsRequired();

        builder.HasIndex(e => e.MachineId)
            .HasDatabaseName("IDX.IndTraceData.Machines.MachineId")
            .IsUnique();

        builder.ToTable("Machines");
    }
}