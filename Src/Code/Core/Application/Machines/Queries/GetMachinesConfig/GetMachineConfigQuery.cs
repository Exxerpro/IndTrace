// <copyright file="GetMachineConfigQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Queries.GetMachinesConfig;

/// <summary>
/// Represents the GetMachineConfigQuery.
/// </summary>
public class GetMachineConfigQuery : IMonitorRequest<MachineConfigVm>
{
    /// <summary>
    /// Gets or sets the PartNumber.
    /// </summary>
    public string? PartNumber { get; set; }
}