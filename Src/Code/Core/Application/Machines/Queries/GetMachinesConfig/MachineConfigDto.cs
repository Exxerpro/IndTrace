// <copyright file="MachineConfigDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Queries.GetMachinesConfig;

/// <summary>
/// Represents the MachineConfigDto.
/// </summary>
public class MachineConfigDto
{
    /// <summary>
    /// Gets or sets the RegisterId.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Location.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MachineType.
    /// </summary>
    public int MachineType { get; set; }

    /// <summary>
    /// Gets or sets the IpAddress.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the EnableAppTraceability.
    /// </summary>
    public int EnableAppTraceability { get; set; }

    /// <summary>
    /// Gets or sets the EnableBypassTraceability.
    /// </summary>
    public int EnableBypassTraceability { get; set; }

    /// <summary>
    /// Gets or sets the BarCodes.
    /// </summary>
    public virtual ICollection<BarCode> BarCodes { get; set; } = new HashSet<BarCode>();

    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public virtual ICollection<Cycle> Cycles { get; set; } = new HashSet<Cycle>();

    /// <summary>
    /// Gets or sets the WorkFlows.
    /// </summary>
    public virtual ICollection<WorkFlow> WorkFlows { get; set; } = new HashSet<WorkFlow>();

    /// <summary>
    /// Gets or sets the DefectRegisters.
    /// </summary>
    public virtual ICollection<DefectRegister> DefectRegisters { get; set; } = new HashSet<DefectRegister>();

    /// <summary>
    /// Gets or sets the Settings.
    /// </summary>
    public virtual ICollection<Setting> Settings { get; set; } = new HashSet<Setting>();

    /// <summary>
    /// Gets or sets the Variables.
    /// </summary>
    public virtual ICollection<Variable> Variables { get; set; } = new HashSet<Variable>();

    /// <summary>
    /// Gets or sets the MachinesPlc.
    /// </summary>
    public virtual ICollection<MachinePlc> MachinesPlc { get; set; } = new HashSet<MachinePlc>();

    /// <summary>
    /// Gets or sets the Plcs.
    /// </summary>
    public virtual ICollection<Plc> Plcs { get; set; } = new HashSet<Plc>();

    /// <summary>
    /// Converts a Machine entity to MachineConfigDto.
    /// </summary>
    /// <param name="src">The source Machine entity.</param>
    /// <returns>A Result containing the MachineConfigDto or failure information.</returns>
    public static Result<MachineConfigDto> ToDto(Machine src)
    {
        if (src == null)
        {
            return Result<MachineConfigDto>.WithFailure($"Parameter '{nameof(src)}' cannot be null");
        }

        return Result<MachineConfigDto>.Success(new MachineConfigDto
        {
            Id = src.MachineId.Value,
            MachineId = src.MachineId.Value,
            Name = src.Name,
            Location = src.Location,
            MachineType = src.MachineType,
            EnableAppTraceability = src.EnableAppTraceability,
            EnableBypassTraceability = src.EnableBypassTraceability,
        });
    }

    /// <summary>
    /// Converts a MachineConfigDto to Machine entity.
    /// </summary>
    /// <param name="src">The source MachineConfigDto.</param>
    /// <returns>A Result containing the Machine entity or failure information.</returns>
    public static Result<Machine> ToEntity(MachineConfigDto src)
    {
        if (src == null)
        {
            return Result<Machine>.WithFailure($"Parameter '{nameof(src)}' cannot be null");
        }

        return Result<Machine>.Success(new Machine
        {
            MachineId = new MachineId(src.MachineId),
            Name = src.Name,
            Location = src.Location,
            MachineType = src.MachineType,
            EnableAppTraceability = src.EnableAppTraceability,
            EnableBypassTraceability = src.EnableBypassTraceability,
        });
    }
}