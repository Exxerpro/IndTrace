// <copyright file="RegistersConfiguration.cs" company="Exxerpro Solutions SA de CV">
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
/// Represents the RegistersConfiguration.
/// </summary>

public class RegistersConfiguration : IEntityTypeConfiguration<Register>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Register> builder)
    {
        builder.HasKey(e => e.RegisterId)
            .HasName("PK.IndTraceData.Registers.RegisterId");

        builder.HasIndex(r => r.VariableId)
            .HasDatabaseName("IX_Registers_VariableID");

        builder.HasIndex(r => new { r.Name, r.MachineId })
            .HasDatabaseName("IX_Registers_Name_MachineId");

        builder.HasIndex(r => r.TimeStamp)
            .HasDatabaseName("IX_Registers_TimeStamp");

        builder.Property(e => e.RegisterId)
            .HasColumnName(nameof(Register.RegisterId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Story 35.D2 Cluster 3 (#35): Register.CycleId is the strongly-typed CycleId struct, mapped to the SAME
        // unchanged int column via the shared byte-preserving converter so the modeled FK below into Cycle's
        // converted principal key stays type-compatible (an int FK targeting a CycleId PK detonates the model).
        builder.Property(e => e.CycleId)
            .HasColumnName(nameof(Register.CycleId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new CycleId(v))
            .IsRequired();

        builder.Property(e => e.Value)
            .HasColumnName(nameof(Register.Value))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Name)
            .HasColumnName(nameof(Register.Name))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.StatusValueId)
            .HasColumnName(nameof(Register.StatusValueId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.VariableId)
            .HasColumnName(nameof(Register.VariableId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.DataType)
            .HasColumnName(nameof(Register.DataType))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core MaxLength constraint fix - Register.Description property missing configuration
        builder.Property(e => e.Description)
            .HasColumnName(nameof(Register.Description))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.TimeStamp)
            .HasColumnName(nameof(Register.TimeStamp))
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.HasOne<Variable>()
            .WithMany()
            .HasForeignKey(v => v.VariableId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.Registers.Variables");

        builder.HasOne<Cycle>()
            .WithMany()
            .HasForeignKey(d => d.CycleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.Registers.Cycles");

        builder.HasIndex(e => e.RegisterId)
            .HasDatabaseName("IDX.IndTraceData.Registers.RegisterId")
            .IsUnique();

        //[Fix]
        //CLAUDE
        //Date: 23/07/2026
        //Reason: Issue #184 — dbo.Registers is an APPEND-ONLY LEDGER table (#39), and SQL Server rejects ANY
        //MERGE against it with error 37359 ("Updates are not allowed for the append only Ledger table"), even a
        //pure-insert MERGE. Disabling the OUTPUT clause is the documented ledger/trigger accommodation and keeps
        //small (below MinBatchSize) EF inserts on plain INSERT + identity read-back. It is NOT sufficient on its
        //own: for >=4 identity-key inserts the provider still emits MERGE ... OUTPUT INTO (EF has no ledger-aware
        //pipeline yet — dotnet/efcore#33226), so the production register append bypasses the EF save pipeline
        //with a set-based plain INSERT (see BarCodeAggregateRepository.SaveAsync). Provider-specific annotation —
        //EF-InMemory (Aggregation.BoundedTests) ignores it.
        builder.ToTable("Registers", tb => tb.UseSqlOutputClause(false));
    }
}