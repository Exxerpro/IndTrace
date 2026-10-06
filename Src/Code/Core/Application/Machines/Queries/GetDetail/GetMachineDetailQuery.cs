// <copyright file="GetMachineDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Queries.GetDetail;

/// <summary>
/// Represents the GetMachineDetailQuery.
/// </summary>
public class GetMachineDetailQuery : IMonitorRequest<MachineDto>
{
    /// <summary>
    /// Gets or sets the RegisterId.
    /// </summary>
    public int Id { get; set; }
}