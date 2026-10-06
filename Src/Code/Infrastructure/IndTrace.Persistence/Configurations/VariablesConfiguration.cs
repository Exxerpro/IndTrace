// <copyright file="VariablesConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the VariablesConfiguration.
/// </summary>

public class VariablesConfiguration : IEntityTypeConfiguration<Variable>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Variable> builder)
    {
        builder.HasKey(e => e.VariableId)
            .HasName("PK.IndTraceData.Variables.EntitieId");

        builder.Property(e => e.VariableId)
            .HasColumnName(nameof(Variable.VariableId))
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.MachineId)
            .HasColumnName(nameof(Variable.MachineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName(nameof(Variable.Name))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Description)
            .HasColumnName(nameof(Variable.Description))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.Address)
            .HasColumnName(nameof(Variable.Address))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Alias)
            .HasColumnName(nameof(Variable.Alias))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.NetType)
            .HasColumnName(nameof(Variable.NetType))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Direction)
            .HasColumnName(nameof(Variable.Direction))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.VariableGroupId)
            .HasColumnName(nameof(Variable.VariableGroupId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.Length)
            .HasColumnName(nameof(Variable.Length))
            .HasColumnType("int")
            .IsRequired();

        // EF Core value converter: persist the ActiveStatus smart enum as the int column "IsActive".
        // The from-DB side NORMALIZES (positive -> Active, negative -> Inactive, zero -> None) because the
        // write contract permits any non-negative Event value (CreateVariableValidator: "Event must be 0 or
        // greater"), so incoming/legacy positive "active" values (>= 2) materialize as Active instead of
        // stranding on the Invalid sentinel. Column type and the "positive = active" contract are unchanged.
        // #117 F4: a NULL enum writes the ActiveStatus Invalid sentinel (int.MinValue), never 0/None, and the
        // read maps EXACTLY that sentinel back to Invalid (an exact-match arm — every other stored int keeps
        // the legacy normalization above, so no production value 0/1/-1/legacy changes meaning).
        builder.Property(e => e.IsActive)
            .HasColumnName(nameof(Variable.IsActive))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : ActiveStatus.Invalid.Value,                              // To DB: ActiveStatus → int (null → Invalid sentinel)
                v => v == ActiveStatus.Invalid.Value ? ActiveStatus.Invalid : (v > 0 ? ActiveStatus.Active : (v < 0 ? ActiveStatus.Inactive : ActiveStatus.None))); // From DB: sentinel → Invalid; positive → Active

        builder.HasIndex(e => e.VariableId)
            .HasDatabaseName("IDX.IndTraceData.Variables.EntitieId")
            .IsUnique();

        // Add a unique constraint on the combination of MachineId, PlcId, Name, Address, and VariableGroupId
        builder.HasIndex(e => new { e.MachineId, e.PlcId, e.Name, e.Address, e.VariableGroupId })
            .HasDatabaseName("UQ_MachinePlcNameAddressVariableGroup")
            .IsUnique();

        //properties added may 1 2025  to validate the register exist on the plc
        //Add ignore to the configurator on the database on the persistence layer
        //ABR MAY 1 2025

        //public bool? Validated { get; set; }

        // Ignore the properties that are not mapped to the database

        builder.Ignore(e => e.Validated);

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core AuditableEntity configuration fix - Variable inherits from AuditableEntity
        new AuditableEntityConfiguration<Variable>().Configure(builder);

        builder.ToTable("Variables");
    }
}