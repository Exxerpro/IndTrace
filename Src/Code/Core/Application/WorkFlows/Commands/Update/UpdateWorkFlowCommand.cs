// <copyright file="UpdateWorkFlowCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Commands.Update;

using IndTrace.Application.WorkFlows.Queries.GetDetail;

/// <summary>
/// Command for updating a workflow, specifying workflow and machine details.
/// </summary>
public class UpdateWorkFlowCommand : IMonitorRequest<WorkFlowDetailVm>
{
    /// <summary>
    /// Gets or sets the workflow identifier to update.
    /// </summary>
    public int? WorkFlowId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with the workflow.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the next machine in the workflow.
    /// </summary>
    public int? NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the last machine in the workflow.
    /// </summary>
    public int? LastMachineId { get; set; }
}