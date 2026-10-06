// <copyright file="MachineUpdateCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Commands.Update;

using IndTrace.Application.Machines.Queries.GetDetail;

/// <summary>
/// Represents the MachineUpdateCommand.
/// </summary>
public class MachineUpdateCommand : IMonitorRequest<MachineDto>
{
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
    public MachineType? MachineType { get; set; }

    /// <summary>
    /// Gets or sets the WorkFlowType.
    /// </summary>
    public WorkFlowType? WorkFlowType { get; set; }
}