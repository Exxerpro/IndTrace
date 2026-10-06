// <copyright file="RoutingNodeConfiguration.cs" company="Exxerpro Solutions SA de CV">
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
/// Represents the RoutingNodeConfiguration. Maps <see cref="RoutingNodeRow"/> to the "RoutingNodes"
/// table — a first-class routing node keyed by (product, machine) carrying the node's composite
/// WorkFlowType bitmask in the "Role" column.
/// </summary>
public class RoutingNodeConfiguration : IEntityTypeConfiguration<RoutingNodeRow>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<RoutingNodeRow> builder)
    {
        // Apply the base configuration
        new AuditableEntityConfiguration<RoutingNodeRow>().Configure(builder);

        builder.HasKey(e => e.RoutingNodeId)
            .HasName("PK.IndTraceData.RoutingNodes.RoutingNodeId");

        builder.Property(e => e.RoutingNodeId)
            .HasColumnName(nameof(RoutingNodeRow.RoutingNodeId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.ProductId)
            .HasColumnName(nameof(RoutingNodeRow.ProductId))
            .HasColumnType("int")
            .IsRequired();

        // Story 35.D2 Cluster 5 (#35): RoutingNodeRow.MachineId is the strongly-typed MachineId struct, mapped to the
        // SAME unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's key).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(RoutingNodeRow.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(d => d.MachineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.RoutingNodes.Machines.MachineId");

        // rationale: DB column is "Role"; it deliberately differs from the RoleValue property name
        // (the property carries the composite WorkFlowType bitmask), so nameof() cannot be used here.
        builder.Property(e => e.RoleValue)
            .HasColumnName("Role")
            .HasColumnType("int")
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(e => new { e.ProductId, e.MachineId })
            .IsUnique()
            .HasDatabaseName("UX.IndTraceData.RoutingNodes.ProductId.MachineId");

        // #41: SQL Server rowversion optimistic-concurrency token (mirrors the StoppageRegister.TimeStamp
        // precedent verbatim). The aggregate repository's delete-batch carries this as the conflict predicate.
        builder.Property(e => e.RowVersion)
            .HasColumnName(nameof(RoutingNodeRow.RowVersion))
            .IsRequired()
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnType("rowversion");

        builder.ToTable("RoutingNodes");
    }
}
