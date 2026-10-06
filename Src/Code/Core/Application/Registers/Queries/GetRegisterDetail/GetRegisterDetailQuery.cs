// <copyright file="GetRegisterDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Registers.Queries.GetRegisterDetail;

/// <summary>
/// Represents the GetRegisterDetailQuery.
/// </summary>
public class GetRegisterDetailQuery : IMonitorRequest<RegisterDto>
{
    /// <summary>
    /// Gets or sets the RegisterId.
    /// </summary>
    public int RegisterId { get; set; }
}