// <copyright file="TaskRequestEventConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the TaskRequestEventConfiguration.
/// </summary>
// Sole canonical EF configuration for the TaskGatewayRequests table (#46 collision resolved: the duplicate
// TaskGatewayRequestConfiguration was deleted). Matches the live QA schema and the §7 PLC write path.
public class TaskRequestEventConfiguration : IEntityTypeConfiguration<TaskGatewayRequest>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<TaskGatewayRequest> builder)
    {
        builder.HasKey(e => e.CommandId)
            .HasName("PK.IndTraceData.TaskGatewayRequests.CommandId");

        builder.Property(e => e.CommandId)
            .HasColumnName(nameof(TaskGatewayRequest.CommandId))
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(TaskGatewayRequest.MachineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.BarCodeId)
            .HasColumnName(nameof(TaskGatewayRequest.BarCodeId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CycleId)
            .HasColumnName(nameof(TaskGatewayRequest.CycleId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CycleStatus)
            .HasColumnName(nameof(TaskGatewayRequest.CycleStatus))
            .HasConversion(
                v => (int)v, // Implicit conversion to int
                v => v)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.PartStatus)
            .HasColumnName(nameof(TaskGatewayRequest.PartStatus))
            .HasConversion(
                v => (int)v, // Implicit conversion to int
                v => v)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.FlowStatus)
            .HasColumnName(nameof(TaskGatewayRequest.FlowStatus))
            .HasConversion(
                v => (int)v, // Implicit conversion to int
                v => v)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.ResultValidation)
            .HasColumnName(nameof(TaskGatewayRequest.ResultValidation))
            .HasConversion(
                v => (int)v, // Implicit conversion to int
                v => v)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.GatewayTask)
            .HasColumnName(nameof(TaskGatewayRequest.GatewayTask))
            .HasConversion(
                v => (int)v, // Implicit conversion to int
                v => v)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.Comment)
            .HasColumnName(nameof(TaskGatewayRequest.Comment))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(120);

        builder.Property(e => e.TimeStamp)
            .HasColumnName(nameof(TaskGatewayRequest.TimeStamp))
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.HasIndex(e => e.CycleId)
            .HasDatabaseName("IDX.IndTraceData.TaskGatewayRequests.CycleId");

        builder.HasIndex(e => e.BarCodeId)
            .HasDatabaseName("IDX.IndTraceData.TaskGatewayRequests.BarCodeId");

        builder.ToTable("TaskGatewayRequests");

        builder.Ignore(e => e.BarCode); // Ignore BarCode property
        builder.Ignore(e => e.PartNumber); // Ignore PartNumber property
        builder.Ignore(e => e.Description); // Ignore Description property
        builder.Ignore(e => e.Name); // Ignore Description property
        builder.Ignore(e => e.MachineType); // Ignore MachineType property
        builder.Ignore(e => e.Registers); // Ignore Registers property
        builder.Ignore(e => e.BarCode); // Ignore Label property
        builder.Ignore(e => e.IsEnabled); // Ignore Enabled property
        builder.Ignore(e => e.EventStatus); // Ignore EventStatus property
        builder.Ignore(e => e.StatusColor); // Ignore StatusColor property
        builder.Ignore(e => e.Parameters); // Ignore Parameters property
        builder.Ignore(e => e.Error); // Ignore ErrorMessage property
        builder.Ignore(e => e.RequestTask); // Ignore ErrorMessage property
        builder.Ignore(e => e.Parameters);
        builder.Ignore(e => e.WatchDogTime);
    }
}