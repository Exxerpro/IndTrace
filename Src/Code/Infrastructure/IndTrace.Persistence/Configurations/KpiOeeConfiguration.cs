// <copyright file="KpiOeeConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the KpiOeeConfiguration.
/// </summary>

public class KpiOeeConfiguration : IEntityTypeConfiguration<KpiOee>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<KpiOee> builder)
    {
        builder.ToTable("KpiOees");

        builder.HasIndex(e => e.KpiOeeId)
            .HasDatabaseName("IDX.IndTraceData.KpiOee.KpiOeeId")
            .IsUnique();

        // Configure primary key following safe refactoring pattern: {nameof(KpiOee) + "RegisterId"}
        builder.HasKey(e => e.KpiOeeId)
            .HasName("PK.IndTraceData.KpiOee.KpiOeeId");

        builder.Property(e => e.KpiOeeId)
            .HasColumnName(nameof(KpiOee.KpiOeeId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Story 26.B1 (#31 remainder): KpiOee's four metric properties were retyped off raw double onto the named
        // bounded value objects Ratio ([0,1]) and PerformanceRatio ([0,1.5]). Each maps to the SAME, UNCHANGED
        // decimal(12,4) column via a per-property EF value converter — no rename, no schema migration. The
        // provider CLR type stays double (converter emits vo.Value; EF then applies the decimal(12,4) store type
        // exactly as it did for the pre-retype double property), so persisted values are byte-identical (proven on
        // real SQL by KpiOeePersistenceTests). FromPersisted is a total, never-throwing read-path seam so a legacy
        // out-of-range row materializes without detonating the query. A ValueComparer change-tracks by .Value
        // (reference-type VOs need one, mirroring the BarCodeLabel converter from Story 27.2b-2).
        builder.Property(e => e.Oee)
            .HasColumnName(nameof(KpiOee.Oee))
            .IsRequired()
            .HasColumnType("decimal(12, 4)")
            .HasConversion(
                ratio => ratio.Value,                  // To DB: Ratio -> double (EF stores as decimal(12,4))
                value => Ratio.FromPersisted(value))   // From DB: double -> Ratio (total, no throw)
            .Metadata.SetValueComparer(RatioComparer());

        builder.Property(e => e.Availability)
            .HasColumnName(nameof(KpiOee.Availability))
            .IsRequired()
            .HasColumnType("decimal(12, 4)")
            .HasConversion(
                ratio => ratio.Value,
                value => Ratio.FromPersisted(value))
            .Metadata.SetValueComparer(RatioComparer());

        builder.Property(e => e.Performance)
            .HasColumnName(nameof(KpiOee.Performance))
            .IsRequired()
            .HasColumnType("decimal(12, 4)")
            .HasConversion(
                ratio => ratio.Value,                             // To DB: PerformanceRatio -> double
                value => PerformanceRatio.FromPersisted(value))   // From DB: double -> PerformanceRatio
            .Metadata.SetValueComparer(PerformanceRatioComparer());

        builder.Property(e => e.Quality)
            .HasColumnName(nameof(KpiOee.Quality))
            .IsRequired()
            .HasColumnType("decimal(12, 4)")
            .HasConversion(
                ratio => ratio.Value,
                value => Ratio.FromPersisted(value))
            .Metadata.SetValueComparer(RatioComparer());

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core datetime2 type fix - TimeStamp uses incorrect case "DateTime2" instead of "datetime2"
        builder.Property(e => e.TimeStamp)
            .HasColumnName(nameof(KpiOee.TimeStamp))
            .IsRequired()
            .HasColumnType("datetime2");
    }

    /// <summary>
    /// Builds the value comparer for the <see cref="Ratio"/>-typed metric columns. EF change-tracks reference-type
    /// value objects by their underlying <see cref="Ratio.Value"/> (never by reference), and snapshots by identity
    /// because the VO is immutable.
    /// </summary>
    /// <returns>A value comparer over <see cref="Ratio"/>.</returns>
    private static ValueComparer<Ratio> RatioComparer() =>
        new(
            (left, right) => (left == null && right == null)
                || (left != null && right != null && left.Value.Equals(right.Value)),
            ratio => ratio == null ? 0 : ratio.Value.GetHashCode(),
            ratio => ratio);

    /// <summary>
    /// Builds the value comparer for the <see cref="PerformanceRatio"/>-typed metric column (see
    /// <see cref="RatioComparer"/>).
    /// </summary>
    /// <returns>A value comparer over <see cref="PerformanceRatio"/>.</returns>
    private static ValueComparer<PerformanceRatio> PerformanceRatioComparer() =>
        new(
            (left, right) => (left == null && right == null)
                || (left != null && right != null && left.Value.Equals(right.Value)),
            ratio => ratio == null ? 0 : ratio.Value.GetHashCode(),
            ratio => ratio);
}