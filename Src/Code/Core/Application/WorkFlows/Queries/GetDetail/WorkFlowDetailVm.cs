// <copyright file="WorkFlowDetailVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Queries.GetDetail;

/// <summary>
/// ViewModel representing the details of a workflow, including machines and edges.
/// </summary>
public class WorkFlowDetailVm
{
    /// <summary>
    /// Gets or sets the workflow identifier.
    /// </summary>
    public int WorkFlowId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with the workflow.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the next machine in the workflow.
    /// </summary>
    public int NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the last machine in the workflow.
    /// </summary>
    public int LastMachineId { get; set; }

    /// <summary>
    /// Gets or sets the list of machines in the workflow.
    /// </summary>
    public List<Machine> Machine { get; set; } = [];

    /// <summary>
    /// Converts a <see cref="WorkFlow"/> entity to a <see cref="WorkFlowDetailVm"/>.
    /// </summary>
    /// <param name="src">The source <see cref="WorkFlow"/> entity.</param>
    /// <returns>A <see cref="WorkFlowDetailVm"/> representing the entity.</returns>
    public static IndQuestResults.Result<WorkFlowDetailVm> ToDto(WorkFlow src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<WorkFlowDetailVm>.WithFailure("WorkFlow source cannot be null");
        }

        return IndQuestResults.Result<WorkFlowDetailVm>.Success(new WorkFlowDetailVm
        {
            WorkFlowId = src.WorkFlowId,
            ProductId = src.ProductId,
            NextMachineId = src.NextMachineId.Value,
            LastMachineId = src.LastMachineId.Value,
            Machine = src.Machine?.ToList() ?? [],
        });
    }

    /// <summary>
    /// Converts a collection of <see cref="WorkFlow"/> entities to a list of <see cref="WorkFlowDetailVm"/>.
    /// </summary>
    /// <param name="src">The source collection of <see cref="WorkFlow"/> entities.</param>
    /// <returns>A list of <see cref="WorkFlowDetailVm"/> representing the entities.</returns>
    public static IndQuestResults.Result<List<WorkFlowDetailVm>> ToDtoList(IEnumerable<WorkFlow> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<List<WorkFlowDetailVm>>.WithFailure("WorkFlow collection cannot be null");
        }

        var list = src.Select(s => new WorkFlowDetailVm
        {
            WorkFlowId = s.WorkFlowId,
            ProductId = s.ProductId,
            NextMachineId = s.NextMachineId.Value,
            LastMachineId = s.LastMachineId.Value,
            Machine = s.Machine?.ToList() ?? [],
        }).ToList();
        return IndQuestResults.Result<List<WorkFlowDetailVm>>.Success(list);
    }
}