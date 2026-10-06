// <copyright file="CycleCompletionConfiguration.cs" company="Exxerpro Solutions SA de CV">
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
/// Story 40-B (#40 B2-idempotency): EF configuration for the <see cref="CycleCompletion"/> marker table — the
/// one-per-cycle completion fact that turns a PLC re-send into a single-row collision. Maps the surrogate
/// identity PK, the strongly-typed <see cref="CycleId"/> under the <c>UNIQUE(CycleId)</c> index
/// (<c>UX.IndTraceData.CycleCompletion.CycleId</c>) that is the idempotency key, an <c>FK CycleId → Cycles</c>
/// with <c>Restrict</c> delete, and the audit columns. The physical table itself is created by the hand-authored
/// migration under <c>Databases/Migration-BarCode-Aggregate/</c> (never <c>dotnet ef</c>).
/// </summary>
public class CycleCompletionConfiguration : IEntityTypeConfiguration<CycleCompletion>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<CycleCompletion> builder)
    {
        builder.HasKey(e => e.CycleCompletionId)
            .HasName("PK.IndTraceData.CycleCompletion.CycleCompletionId");

        builder.Property(e => e.CycleCompletionId)
            .HasColumnName(nameof(CycleCompletion.CycleCompletionId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // #40 B2: CycleCompletion.CycleId is the strongly-typed CycleId struct, mapped to the SAME unchanged int
        // column via the shared byte-preserving converter so the FK below into Cycle's converted principal key
        // stays type-compatible (an int FK targeting a CycleId PK detonates the model — mirrors Register.CycleId).
        builder.Property(e => e.CycleId)
            .HasColumnName(nameof(CycleCompletion.CycleId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new CycleId(v))
            .IsRequired();

        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(CycleCompletion.MachineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CompletedOn)
            .HasColumnName(nameof(CycleCompletion.CompletedOn))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        // #40 B2: the idempotency key — one completion per cycle. A PLC re-send collides on this single row and
        // the aggregate repository (Chunk 40-C) recognises THIS constraint name to convert the collision into an
        // idempotent no-op. The physical UNIQUE constraint of the same name is created by 01_forward.sql.
        builder.HasIndex(e => e.CycleId)
            .HasDatabaseName("UX.IndTraceData.CycleCompletion.CycleId")
            .IsUnique();

        builder.HasOne<Cycle>()
            .WithMany()
            .HasForeignKey(e => e.CycleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.CycleCompletion.Cycles");

        builder.ToTable("CycleCompletion");
    }
}
