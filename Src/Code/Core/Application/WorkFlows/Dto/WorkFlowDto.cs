// <copyright file="WorkFlowDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Dto;

/// <summary>
/// Data Transfer Object (DTO) for representing a workflow, including machines and edges.
/// </summary>
/// <remarks>
/// #225: all properties are init-only and <see cref="Machine"/> carries detached <see cref="MachineDto"/>
/// elements (never live EF entities). Instances of this DTO live inside the 60-minute cached
/// <c>ApplicationConfiguration</c> that <c>CacheManager</c> serves BY REFERENCE to every consumer, so the
/// object graph must be immutable and detached from the entity graph it was mapped from.
/// </remarks>
public class WorkFlowDto
{
    /// <summary>
    /// Gets the workflow identifier.
    /// </summary>
    public int WorkFlowId { get; init; }

    /// <summary>
    /// Gets the product identifier associated with the workflow.
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// Gets the identifier of the next machine in the workflow.
    /// </summary>
    public int NextMachineId { get; init; }

    /// <summary>
    /// Gets the identifier of the last machine in the workflow.
    /// </summary>
    public int LastMachineId { get; init; }

    /// <summary>
    /// Gets the rule identifier associated with the workflow.
    /// </summary>
    public int RuleId { get; init; }

    /// <summary>
    /// Gets the list of machines in the workflow, as detached <see cref="MachineDto"/> snapshots
    /// (#225 — previously live EF <c>Machine</c> entities aliased by reference from the source entity).
    /// </summary>
    public List<MachineDto> Machine { get; init; } = [];

    /// <summary>
    /// Converts a <see cref="WorkFlow"/> entity to a <see cref="WorkFlowDto"/>.
    /// </summary>
    /// <param name="src">The source <see cref="WorkFlow"/> entity.</param>
    /// <returns>A <see cref="WorkFlowDto"/> representing the entity.</returns>
    public static IndQuestResults.Result<WorkFlowDto> ToDto(WorkFlow src)
    {
        // [Fix]
        // CLAUDE
        // Date: 22/08/2025
        // Reason: Pattern 11 Fix - Updated error message to match test expectation "Parameter 'src' cannot be null"
        if (src == null)
        {
            return IndQuestResults.Result<WorkFlowDto>.WithFailure("Parameter 'src' cannot be null");
        }

        return IndQuestResults.Result<WorkFlowDto>.Success(new WorkFlowDto
        {
            WorkFlowId = src.WorkFlowId,
            ProductId = src.ProductId,
            NextMachineId = src.NextMachineId.Value,
            LastMachineId = src.LastMachineId.Value,
            RuleId = src.RuleId,

            // #225: detached element-wise projection (idiom shared with AppDetailsFactory) — never alias
            // the entity's live collection into the cached DTO graph.
            Machine = src.Machine?
                .Select(m => MachineDto.ToDto(m))
                .Where(r => r.IsSuccess)
                .Select(r => r.Value)
                .OfType<MachineDto>()
                .ToList() ?? [],
        });
    }

    /// <summary>
    /// Converts a <see cref="WorkFlowDto"/> to a <see cref="WorkFlow"/> entity.
    /// </summary>
    /// <param name="src">The source <see cref="WorkFlowDto"/>.</param>
    /// <returns>A <see cref="WorkFlow"/> entity representing the DTO. Entity-only members that
    /// <see cref="MachineDto"/> does not carry (e.g. <c>IpAddress</c>, audit columns) are defaulted.</returns>
    public static IndQuestResults.Result<WorkFlow> ToEntity(WorkFlowDto src)
    {
        // [Fix]
        // CLAUDE
        // Date: 22/08/2025
        // Reason: Pattern 11 Fix - Updated error message to match test expectation "Parameter 'src' cannot be null"
        if (src == null)
        {
            return IndQuestResults.Result<WorkFlow>.WithFailure("Parameter 'src' cannot be null");
        }

        return IndQuestResults.Result<WorkFlow>.Success(new WorkFlow
        {
            WorkFlowId = src.WorkFlowId,
            ProductId = src.ProductId,
            NextMachineId = new MachineId(src.NextMachineId),
            LastMachineId = new MachineId(src.LastMachineId),
            RuleId = src.RuleId,

            // #225: mirror of the ToDto projection — rebuild fresh Machine entities per element.
            Machine = src.Machine?
                .Select(m => MachineDto.ToEntity(m))
                .Where(r => r.IsSuccess)
                .Select(r => r.Value)
                .OfType<Machine>()
                .ToList() ?? [],
        });
    }
}
