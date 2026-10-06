// <copyright file="GetWorkFlowDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Queries.GetDetail;

/// <summary>
/// Represents the GetWorkFlowDetailQuery.
/// </summary>
public class GetWorkFlowDetailQuery : IMonitorRequest<List<WorkFlowDetailVm>>
{
    /// <summary>
    /// Gets or sets the NoParte.
    /// </summary>
    public string? NoParte { get; set; }
}