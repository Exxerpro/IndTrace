// <copyright file="MachineConfigVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Queries.GetMachinesConfig;

/// <summary>
/// Represents the MachineConfigVm.
/// </summary>
public class MachineConfigVm
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MachineConfigVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public MachineConfigVm()
    {
        this.Machines = [];
    }

    /// <summary>
    /// Gets or sets the Machines.
    /// </summary>
    public IList<MachineConfigDto> Machines { get; set; }

    /// <summary>
    /// Gets or sets the Count.
    /// </summary>
    public int Count { get; set; }
}