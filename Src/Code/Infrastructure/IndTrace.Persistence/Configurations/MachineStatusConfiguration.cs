// <copyright file="MachineStatusConfiguration.cs" company="Exxerpro Solutions SA de CV">
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
/// Represents the MachineStatusConfiguration.
/// </summary>

public class MachineStatusConfiguration : IEntityTypeConfiguration<MachineStatus>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<MachineStatus> builder)
    {
        // IMPORTANT: Non-standard primary key name
        // StatusMachineId does not follow convention of {nameof(MachineStatus) + "RegisterId"} (MachineStatusId)
        // This should be reviewed and potentially renamed to MachineStatusId
        builder.HasKey(e => e.StatusMachineId);

        builder.Property(e => e.StatusMachineId)
            .HasColumnName(nameof(MachineStatus.StatusMachineId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Story 35.D2 Cluster 5 (#35): MachineStatus.MachineId is the strongly-typed MachineId struct, mapped to the
        // SAME unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's key).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(MachineStatus.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core validation fixes - BreakDownTime needs HasPrecision and UpdatedOn needs datetime2
        builder.Property(e => e.BreakDownTime)
            .HasColumnName(nameof(MachineStatus.BreakDownTime))
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(e => e.UpdatedOn)
            .HasColumnName(nameof(MachineStatus.UpdatedOn))
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(d => d.MachineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.MachineStatus.MachineId");

        builder.ToTable("MachineStatus");
    }
}