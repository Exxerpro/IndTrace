// <copyright file="TaskGatewayResponseConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Enum.LookUpTable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the TaskGatewayResponseConfiguration.
/// </summary>

public class TaskGatewayResponseConfiguration : IEntityTypeConfiguration<TaskGatewayResponse>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<TaskGatewayResponse> builder)
    {
        // Define the primary key
        builder.HasKey(e => e.ResponseId)
            .HasName("PK.IndTraceData.TaskGatewayResponses.ResponseId");

        // Configure the relevant properties
        builder.Property(e => e.ResponseId)
            .HasColumnName(nameof(TaskGatewayResponse.ResponseId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.CommandId)
            .HasColumnName(nameof(TaskGatewayResponse.CommandId))
            .HasColumnType("int")
            .IsRequired();

        builder.HasOne<TaskGatewayRequest>()
            .WithOne()
            .HasForeignKey<TaskGatewayResponse>(e => e.CommandId)
            .HasConstraintName("FK.IndTraceData.TaskGatewayResponses.CommandId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(TaskGatewayResponse.MachineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.BarCodeId)
            .HasColumnName(nameof(TaskGatewayResponse.BarCodeId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CycleId)
            .HasColumnName(nameof(TaskGatewayResponse.CycleId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CyclesOk)
            .HasColumnName(nameof(TaskGatewayResponse.CyclesOk))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.ShiftId)
            .HasColumnName(nameof(TaskGatewayResponse.ShiftId))
            .HasColumnType("int")
            .IsRequired();

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core value converter fix - ResultValidation is a smart enum, should use .Value not (int)v
        builder.Property(e => e.ResultValidation)
            .HasColumnName(nameof(TaskGatewayResponse.ResultValidation))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : ResultValidation.Invalid.Value,  // To DB: ResultValidation → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => (ResultValidation)v       // From DB: int → ResultValidation (implicit)
            );

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core MaxLength constraint fix - Replace HasColumnType with HasMaxLength for validation compliance
        builder.Property(e => e.PartNumber)
            .HasColumnName(nameof(TaskGatewayResponse.PartNumber))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(50);

        builder.Property(e => e.Label)
            .HasColumnName(nameof(TaskGatewayResponse.Label))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(100);

        builder.Property(e => e.Error)
            .HasColumnName(nameof(TaskGatewayResponse.Error))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.LastMachineId)
            .HasColumnName(nameof(TaskGatewayResponse.LastMachineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.NextMachineId)
            .HasColumnName(nameof(TaskGatewayResponse.NextMachineId))
            .HasColumnType("int")
            .IsRequired();

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core value converter fix - Update to proper smart enum conversion pattern using .Value
        builder.Property(e => e.CycleStatus)
            .HasColumnName(nameof(TaskGatewayResponse.CycleStatus))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : CycleStatus.Invalid.Value,  // To DB: CycleStatus → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => (CycleStatus)v            // From DB: int → CycleStatus (implicit)
            );

        builder.Property(e => e.FlowStatus)
            .HasColumnName(nameof(TaskGatewayResponse.FlowStatus))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : FlowStatus.Invalid.Value,  // To DB: FlowStatus → int (null → Invalid sentinel 8, never 0/None — #117 F4)
                v => (FlowStatus)v             // From DB: int → FlowStatus (implicit)
            );

        builder.Property(e => e.PartStatus)
            .HasColumnName(nameof(TaskGatewayResponse.PartStatus))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : PartStatus.Invalid.Value,  // To DB: PartStatus → int (null → Invalid sentinel, never 0/None — #117 F4)
                v => (PartStatus)v             // From DB: int → PartStatus (implicit)
            );

        // Note: MachineType and WorkFlowType properties are ignored below, so no property configuration needed

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core datetime2 type fix - TimeStamp DateTime property missing configuration
        builder.Property(e => e.TimeStamp)
            .HasColumnName(nameof(TaskGatewayResponse.TimeStamp))
            .HasColumnType("datetime2");

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core foreign key fix - Remove foreign key relationships for smart enums to avoid type compatibility issues
        // Note: Lookup tables exist for SQL developers/admins, but EF Core foreign keys cause type mismatch
        // Smart enums provide the business logic functionality without needing EF Core relationships
        // Note: MachineType and WorkFlowType are ignored properties

        // Define the table name
        builder.ToTable("TaskGatewayResponses");

        // #32 C2: the 12 wire-only members (MachineType, Description, WorkFlowType, Recipe, Cycle, BarCode,
        // MasterLabel, References, ExecutionTime, RequestTask, Parameters, Name, PlcId) no longer exist on the
        // slimmed persisted entity — they live on TaskGatewayResponseDto — so there is nothing left to Ignore here.
        // The mapped-column surface stays exactly the 17 persisted columns (pinned by TaskGatewayResponsePersistedRowConfigTests).
    }
}