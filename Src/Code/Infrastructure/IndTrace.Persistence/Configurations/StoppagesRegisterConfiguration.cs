// <copyright file="StoppagesRegisterConfiguration.cs" company="Exxerpro Solutions SA de CV">
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
/// Represents the StoppagesRegisterConfiguration.
/// </summary>

public class StoppagesRegisterConfiguration : IEntityTypeConfiguration<StoppageRegister>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<StoppageRegister> builder)
    {
        builder.HasKey(e => e.StoppageRegisterId)
            .HasName("PK.IndTraceData.StoppagesRegister.StoppageRegisterId");

        builder.Property(e => e.StoppageRegisterId)
            .HasColumnName(nameof(StoppageRegister.StoppageRegisterId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.ProductionOrderId)
            .HasColumnName(nameof(StoppageRegister.ProductionOrderId))
            .HasColumnType("int")
            .IsRequired();

        // Story 35.D2 Cluster 5 (#35): StoppageRegister.MachineId is the strongly-typed MachineId struct, mapped to the
        // SAME unchanged int column via the shared byte-preserving converter (FK type-compatible with Machine's key).
        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(StoppageRegister.MachineId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new MachineId(v))
            .IsRequired();

        builder.Property(e => e.StoppageId)
            .HasColumnName(nameof(StoppageRegister.StoppageId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName(nameof(StoppageRegister.Description))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.Comment)
            .HasColumnName(nameof(StoppageRegister.Comment))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core byte[] configuration fix - TimeStamp needs explicit column type for rowversion
        builder.Property(e => e.TimeStamp)
            .HasColumnName(nameof(StoppageRegister.TimeStamp))
            .IsRequired()
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnType("rowversion");

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core Precision/Scale fix - Use HasPrecision instead of HasColumnType for better validation
        builder.Property(e => e.StoppedTime)
            .HasColumnName(nameof(StoppageRegister.StoppedTime))
            .HasPrecision(10, 4);

        //[Fix] 
        //CLAUDE
        //Date: 25/08/2025 
        //Reason: EF Core datetime2 type fix - StartedOn and RegistedOn DateTime properties missing datetime2 configuration
        builder.Property(e => e.StartedOn)
            .HasColumnName(nameof(StoppageRegister.StartedOn))
            .HasColumnType("datetime2")
            .HasDefaultValueSql("(getdate())");

        builder.Property(e => e.FinishedOn)
            .HasColumnName(nameof(StoppageRegister.FinishedOn))
            .HasColumnType("datetime2");

        builder.Property(e => e.RegistedOn)
            .HasColumnName(nameof(StoppageRegister.RegistedOn))
            .HasColumnType("datetime2");

        builder.HasOne<Machine>()
            .WithMany()
            .HasForeignKey(d => d.MachineId)
            .HasConstraintName("FK.IndTraceData.RegisterStoppages.MachineId");

        builder.HasOne<Stoppage>()
            .WithMany()
            .HasForeignKey(d => d.StoppageId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK.IndTraceData.RegisterStoppages.StoppageId");

        builder.ToTable("RegisterStoppages");
    }
}