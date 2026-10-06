// <copyright file="GetRegistersListQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Registers.Queries.GetRegisterList;

/// <summary>
/// Represents the GetRegistersListQuery.
/// </summary>
public class GetRegistersListQuery : IMonitorRequest<IEnumerable<RegisterDto>>
{
    /// <summary>
    /// Gets or sets the RegistersName.
    /// </summary>
    public IEnumerable<string> RegistersName { get; set; } = [];

    /// <summary>
    /// Gets or sets the VariablesId.
    /// </summary>
    public IEnumerable<int> VariablesId { get; set; } = [];

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public IEnumerable<int> MachineId { get; set; } = [];

    /// <summary>
    /// Gets or sets the StartDate.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the EndDate.
    /// </summary>
    public DateTime EndDate { get; set; }
}