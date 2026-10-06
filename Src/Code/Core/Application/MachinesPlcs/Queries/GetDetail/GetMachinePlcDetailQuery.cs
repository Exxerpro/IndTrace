// <copyright file="GetMachinePlcDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Queries.GetDetail;

/// <summary>
/// QueryAsync to retrieve the details of a machine PLC by machine and PLC identifiers.
/// </summary>
public class GetMachinePlcDetailQuery : IMonitorRequest<MachinePlcDetailVm>
{
    /// <summary>
    /// Gets or sets the unique identifier of the machine.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the PLC.
    /// </summary>
    public int PlcId { get; set; }
}