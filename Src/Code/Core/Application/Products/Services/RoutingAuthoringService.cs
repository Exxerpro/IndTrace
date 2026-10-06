// <copyright file="RoutingAuthoringService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Routing.Authoring;

namespace IndTrace.Application.Products.Services;

/// <summary>
/// Pure Application service (C2 Chunk E11.1) that turns an author's ordered machine sequence into the
/// C2-clean routing draft. See <see cref="IRoutingAuthoringService"/> for the contract.
/// </summary>
/// <remarks>
/// The implementation reuses the existing domain authorities rather than reinventing role logic:
/// <list type="number">
/// <item><description>
/// it builds the legacy magic-0 edge chain in memory (the same shape
/// <c>WorkflowOrchestrator.GenerateWorkflowDtos</c> produces), preserving the author's order;
/// </description></item>
/// <item><description>
/// it derives node roles via <see cref="RoutingNodeBackfill.BuildNodes"/> — the single role oracle the D2
/// migration uses, so authored output is shape-identical to migrated output;
/// </description></item>
/// <item><description>
/// it strips the boundary (magic-0) rows to clean interior edges; and
/// </description></item>
/// <item><description>
/// it proves the read path will accept the result BEFORE returning, by running
/// <see cref="RoutingTransitionMapper.ToTransitions"/> → <see cref="ProductionGraph.Create"/> →
/// <see cref="ProductionGraph.LinearMachineSequence"/>. The linear-sequence check is the load-bearing
/// cycle gate (a duplicate-machine cycle slips both <c>BuildNodes</c> and <c>Create</c>); it must never be
/// removed.
/// </description></item>
/// </list>
/// The service performs NO I/O and never throws — every failure is a <see cref="Result{T}"/>.
/// </remarks>
public class RoutingAuthoringService : IRoutingAuthoringService
{
    /// <inheritdoc />
    public Result<RoutingAuthoringDraft> BuildRouting(int productId, IReadOnlyList<int> orderedMachineIds)
    {
        // 1. Guards: a non-null, non-empty, strictly-positive ordered sequence.
        if (orderedMachineIds is null || orderedMachineIds.Count == 0)
        {
            return Result<RoutingAuthoringDraft>.WithFailure(
                $"Product {productId}: an ordered machine sequence with at least one machine is required.");
        }

        foreach (var machineId in orderedMachineIds)
        {
            if (machineId <= 0)
            {
                return Result<RoutingAuthoringDraft>.WithFailure(
                    $"Product {productId}: machine id {machineId} is not positive; every ordered machine id must be > 0.");
            }
        }

        // 2. Build the legacy magic-0 edge chain in memory (author order preserved). NEVER persisted.
        var magicZeroEdges = BuildMagicZeroChain(orderedMachineIds);

        // 3. Derive node roles via the single backfill oracle (+ its linearity/boundary guards).
        var nodesResult = RoutingNodeBackfill.BuildNodes(productId, magicZeroEdges);
        if (nodesResult.IsFailure || nodesResult.Value is null)
        {
            return Result<RoutingAuthoringDraft>.WithFailure(nodesResult.Errors);
        }

        var nodes = nodesResult.Value;

        // 4. Strip the boundary rows to clean interior edges; stamp ProductId only (RuleId/audit later).
        var cleanEdges = magicZeroEdges
            .Where(e => e.LastMachineId.Value > 0 && e.NextMachineId.Value > 0)
            .Select(e => new WorkFlow
            {
                ProductId = productId,
                LastMachineId = e.LastMachineId,
                NextMachineId = e.NextMachineId,
            })
            .ToList();

        // 5. PRIMARY GUARD: prove the graph-validating read path accepts the draft BEFORE returning.
        var transitionsResult = RoutingTransitionMapper.ToTransitions(nodes, cleanEdges);
        if (transitionsResult.IsFailure || transitionsResult.Value is null)
        {
            return Result<RoutingAuthoringDraft>.WithFailure(transitionsResult.Errors);
        }

        var graphResult = ProductionGraph.Create(transitionsResult.Value);
        if (graphResult.IsFailure || graphResult.Value is null)
        {
            return Result<RoutingAuthoringDraft>.WithFailure(graphResult.Errors);
        }

        // LinearMachineSequence is the load-bearing cycle gate — Create alone does NOT reject a cycle.
        var sequenceResult = graphResult.Value.LinearMachineSequence();
        if (sequenceResult.IsFailure || sequenceResult.Value is null)
        {
            return Result<RoutingAuthoringDraft>.WithFailure(sequenceResult.Errors);
        }

        // 6. All passed — return the typed clean-only draft.
        return Result<RoutingAuthoringDraft>.Success(new RoutingAuthoringDraft(nodes, cleanEdges));
    }

    /// <inheritdoc />
    public Result<RoutingAuthoringDraft> BuildRouting(AuthoringRoute route)
    {
        // Delegate the whole node+edge shape mapping + fork-aware validation gate to the pure-domain
        // AuthoringRouteMapper (the SAME gate ProductRouting.ReplaceWith uses), then project to the typed
        // clean-only draft. No ascending-machine-id guess and no linear-only cycle gate on this path.
        var rows = AuthoringRouteMapper.ToPersistenceRows(route);
        if (rows.IsFailure || rows.Value is null)
        {
            return Result<RoutingAuthoringDraft>.WithFailure(rows.Errors);
        }

        return Result<RoutingAuthoringDraft>.Success(new RoutingAuthoringDraft(rows.Value.Nodes, rows.Value.CleanEdges));
    }

    /// <summary>
    /// Builds the legacy magic-0 edge chain (<c>0 -&gt; ids[0] -&gt; ... -&gt; ids[^1] -&gt; 0</c>) for an
    /// ordered machine sequence, preserving the author's order. For a single id the chain is just
    /// <c>(0 -&gt; id), (id -&gt; 0)</c>. This is an in-memory derivation only — these rows are NEVER persisted.
    /// </summary>
    /// <param name="orderedMachineIds">The validated, strictly-positive ordered machine ids.</param>
    /// <returns>The magic-0 edge rows, in chain order.</returns>
    private static IReadOnlyList<WorkFlow> BuildMagicZeroChain(IReadOnlyList<int> orderedMachineIds)
    {
        var edges = new List<WorkFlow>
        {
            new() { LastMachineId = new MachineId(0), NextMachineId = new MachineId(orderedMachineIds[0]) },
        };

        for (var i = 0; i < orderedMachineIds.Count - 1; i++)
        {
            edges.Add(new WorkFlow { LastMachineId = new MachineId(orderedMachineIds[i]), NextMachineId = new MachineId(orderedMachineIds[i + 1]) });
        }

        edges.Add(new WorkFlow { LastMachineId = new MachineId(orderedMachineIds[^1]), NextMachineId = new MachineId(0) });
        return edges;
    }
}
