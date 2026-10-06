// <copyright file="ConnectionStatusConfiguration.cs" company="Exxerpro Solutions SA de CV">
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
/// Represents the ConnectionStatusConfiguration.
/// </summary>

public class ConnectionStatusConfiguration : IEntityTypeConfiguration<ConnectionStatus>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<ConnectionStatus> builder)
    {
        // TODO: CRITICAL - Primary key design needed
        // ConnectionStatus entity lacks primary key property (ConnectionStatusId)
        // Current design suggests composite key (MachineId, Status) or synthetic key needed
        // This violates {nameof(ConnectionStatus)}RegisterId convention
        builder.HasNoKey();

        builder.Property(e => e.Status)
            .HasColumnType("Status")
            .HasColumnType("int")
            .IsRequired();

        // Story 35.D2 Cluster 5 (#35): ConnectionStatus.MachineId is the strongly-typed MachineId struct, mapped to the
        // SAME unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's key).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(ConnectionStatus.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        builder.Property(e => e.Message)
            .HasColumnName(nameof(ConnectionStatus.Message))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core AuditableEntity configuration fix - ConnectionStatus inherits from AuditableEntity
        new AuditableEntityConfiguration<ConnectionStatus>().Configure(builder);

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(d => d.MachineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.StatusConnections.MachineId");

        builder.ToTable("StatusConnections");
    }
}