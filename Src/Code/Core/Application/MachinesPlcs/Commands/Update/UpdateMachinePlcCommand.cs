// <copyright file="UpdateMachinePlcCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Commands.Update;

using IndTrace.Application.MachinesPlcs.Queries.GetDetail;

/// <summary>
/// Represents the UpdateMachinePlcCommand.
/// </summary>
public class UpdateMachinePlcCommand : IMonitorRequest<MachinePlcDetailVm>
{
    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the PlcId.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the IsActive.
    /// </summary>
    public int? IsActive { get; set; }

    /// <summary>
    /// Gets or sets the NewMachineId.
    /// </summary>
    public int? NewMachineId { get; set; }

    /// <summary>
    /// Gets or sets the NewPlcId.
    /// </summary>
    public int? NewPlcId { get; set; }
}