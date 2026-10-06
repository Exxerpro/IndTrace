// <copyright file="OeeRegistersConfigurator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the OeeRegistersConfigurator.
/// </summary>

public class OeeRegistersConfigurator : IEntityTypeConfiguration<OeeRegister>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<OeeRegister> builder)
    {
        builder.ToTable("OeeRegisters");

        builder.HasKey(e => e.OeeRegisterId)
            .HasName("PK.IndTraceData.OeeRegisters.OeeRegisterId");

        builder.HasIndex(e => e.OeeRegisterId).
            HasDatabaseName("IDX.IndTraceData.OeeRegisters.RegisterId")
            .IsUnique();

        builder.HasIndex(e => new { e.PlcId, e.MachineId }).
            HasDatabaseName("IX_OeeRegisters_Name_MachineId");

        builder.HasIndex(e => e.TimeStamp).
            HasDatabaseName("IX_OeeRegisters_TimeStamp");

        builder.Property(e => e.OeeRegisterId)
            .HasColumnName(nameof(OeeRegister.OeeRegisterId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(OeeRegister.MachineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.PlcId)
            .HasColumnName(nameof(OeeRegister.PlcId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.TimeStamp)
            .HasColumnName(nameof(OeeRegister.TimeStamp))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        // Float fields with enforced precision
        builder.Property(e => e.Availability)
            .HasColumnName(nameof(OeeRegister.Availability))
            .HasColumnType("float")
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(e => e.Performance)
            .HasColumnName(nameof(OeeRegister.Performance))
            .HasColumnType("float")
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(e => e.Quality)
            .HasColumnName(nameof(OeeRegister.Quality))
            .HasColumnType("float")
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(e => e.Oee)
            .HasColumnName(nameof(OeeRegister.Oee))
            .HasColumnType("float")
            .HasPrecision(18, 6)
            .IsRequired();

        // Integers
        builder.Property(e => e.ApplicationFlag)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.EventCounter)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CurrentTime)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.RunningTime)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.StoppedTime)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.FaultedTime)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.StatusFaultReason)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.ProductId)
            .HasColumnType("int")
            .IsRequired();

        // More floats
        builder.Property(e => e.TotalProduction)
            .HasColumnType("float")
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(e => e.StandardCycleTime)
            .HasColumnType("float")
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(e => e.ActualCycleTime)
            .HasColumnType("float")
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(e => e.PlanedProductionTime)
            .HasColumnType("float")
            .HasPrecision(18, 6)
            .IsRequired();
    }
}