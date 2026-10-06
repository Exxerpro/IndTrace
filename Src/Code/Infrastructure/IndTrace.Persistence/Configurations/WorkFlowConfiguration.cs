// <copyright file="WorkFlowConfiguration.cs" company="Exxerpro Solutions SA de CV">
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
/// Represents the WorkFlowConfiguration.
/// </summary>

public class WorkFlowConfiguration : IEntityTypeConfiguration<WorkFlow>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<WorkFlow> builder)
    {
        // Apply the base configuration
        new AuditableEntityConfiguration<WorkFlow>().Configure(builder);


        builder.HasKey(e => e.WorkFlowId)
            .HasName("PK.IndTraceData.WorkFlows.WorkFlowId");

        builder.Property(e => e.WorkFlowId)
            .HasColumnName(nameof(WorkFlow.WorkFlowId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.ProductId)
            .HasColumnName(nameof(WorkFlow.ProductId))
            .HasColumnType("int")
            .IsRequired();

        // Story 35.D2 Cluster 5 (#35): WorkFlow.NextMachineId is the strongly-typed MachineId struct, mapped to the
        // SAME unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's key).
        builder.Property(e => e.NextMachineId)
            .HasColumnName(nameof(WorkFlow.NextMachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        // Story 35.D2 Cluster 5 (#35): WorkFlow.LastMachineId is the strongly-typed MachineId struct, mapped to the
        // SAME unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's key).
        builder.Property(e => e.LastMachineId)
            .HasColumnName(nameof(WorkFlow.LastMachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(d => d.NextMachineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.WorkFlows.Machines.NextMachineId");

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(d => d.LastMachineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.WorkFlows.Machines.LastMachineId");

        //builder.HasOne<Rule>()
        //    .WithOne()
        //    .HasForeignKey<Rule>(r => r.RuleId)
        //    .OnDelete(DeleteBehavior.Restrict)
        //    .HasConstraintName("FK.IndTraceData.WorkFlows.Rules.RuleId");

        builder.Property(e => e.RuleId)
            .HasColumnName(nameof(WorkFlow.RuleId))
            .HasColumnType("int")
            .IsRequired();

        // #41: SQL Server rowversion optimistic-concurrency token (mirrors the StoppageRegister.TimeStamp
        // precedent verbatim). The aggregate repository's delete-batch carries this as the conflict predicate.
        builder.Property(e => e.RowVersion)
            .HasColumnName(nameof(WorkFlow.RowVersion))
            .IsRequired()
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnType("rowversion");

        builder.ToTable("WorkFlows");
    }
}