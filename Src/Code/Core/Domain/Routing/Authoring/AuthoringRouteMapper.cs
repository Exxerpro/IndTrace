// <copyright file="AuthoringRouteMapper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing.Authoring;

/// <summary>
/// The pure-domain, total, non-throwing mapper (E11.4-2) between the node+edge authoring view-model
/// (<see cref="AuthoringRoute"/>) and the <see cref="ProductRouting"/> aggregate's persistence rows
/// (<see cref="RoutingNodeRow"/> node roles + clean interior <see cref="WorkFlow"/> edges). It is the
/// order-preserving replacement for the flat <c>IEnumerable&lt;int&gt;</c> + ascending-machine-id guess: authored
/// order and edge multiplicity survive end-to-end, and a fork (a node with more than one outgoing edge) round-trips.
/// </summary>
/// <remarks>
/// <para>
/// PURE: no EF, no <c>DbContext</c>, no repository, no I/O; NEVER throws — every failure is a
/// <see cref="Result{T}"/>. It reuses the existing domain authorities rather than reinventing legality:
/// <see cref="RoutingTransitionMapper.ToTransitions"/> → <see cref="ProductionGraph.Create"/> is the SAME
/// graph-validating read path the linear authoring path proves against, so an authored shape the read path would
/// reject is rejected here BEFORE any rows are returned.
/// </para>
/// <para>
/// <strong>Out-fan relaxation (do not invent new legality).</strong> The linear authoring path additionally runs
/// <see cref="ProductionGraph.LinearMachineSequence"/> — the linear-shape gate that also rejects any
/// multi-successor node. E11.4-2 permits out-fan by running that linear gate ONLY when the route is linear (no node
/// has more than one outgoing edge); a genuine fork is gated by <see cref="ProductionGraph.Create"/> alone — the
/// graph's own legality authority (role allow-list, reachability, dead-end, Lateral→Merger, and — since #115
/// finding 8 — ACYCLICITY, so a cyclic fork is rejected here too; cycles are illegal in fork routes exactly as in
/// linear ones, and a legal rework loop would be a future, PO-gated feature). The linear path is byte-for-byte
/// unchanged, and the fork path defers entirely to the existing <see cref="ProductionGraph"/> validation.
/// </para>
/// <para>
/// <strong>Condition-free (PO-3).</strong> Neither direction reads or writes any per-branch selection condition —
/// there is no such field on <see cref="AuthoringEdge"/>, <see cref="WorkFlow"/>, or <see cref="RoutingNodeRow"/>.
/// </para>
/// </remarks>
public static class AuthoringRouteMapper
{
    /// <summary>
    /// Maps an <see cref="AuthoringRoute"/> to its validated persistence rows (author → persist shape). Node rows
    /// carry the authored composite role; clean interior edges are emitted in author node-then-edge order (a
    /// terminal node contributes no edge). The result is validated through the graph-validating read path (and,
    /// for a linear route, the cycle gate) BEFORE it is returned; on any failure NO rows are produced.
    /// </summary>
    /// <param name="route">The authored route.</param>
    /// <returns>The validated <see cref="AuthoringRouteRows"/>, or a failure carrying the reason.</returns>
    public static Result<AuthoringRouteRows> ToPersistenceRows(AuthoringRoute route)
    {
        if (route is null)
        {
            return Result<AuthoringRouteRows>.WithFailure("The authoring route must not be null.");
        }

        if (route.Nodes is null || route.Nodes.Count == 0)
        {
            return Result<AuthoringRouteRows>.WithFailure(
                $"Product {route.ProductId}: an authoring route must have at least one node.");
        }

        var nodes = new List<RoutingNodeRow>();
        var cleanEdges = new List<WorkFlow>();
        var seenMachines = new HashSet<int>();
        var maxOutDegree = 0;

        foreach (var node in route.Nodes)
        {
            if (node is null)
            {
                return Result<AuthoringRouteRows>.WithFailure(
                    $"Product {route.ProductId}: an authoring node must not be null.");
            }

            if (node.Role is null)
            {
                return Result<AuthoringRouteRows>.WithFailure(
                    $"Product {route.ProductId}: machine {node.MachineId.Value} has a null routing role.");
            }

            if (node.MachineId.Value <= 0)
            {
                return Result<AuthoringRouteRows>.WithFailure(
                    $"Product {route.ProductId}: machine id {node.MachineId.Value} is not positive; every node machine id must be > 0.");
            }

            if (!seenMachines.Add(node.MachineId.Value))
            {
                return Result<AuthoringRouteRows>.WithFailure(
                    $"Product {route.ProductId}: machine {node.MachineId.Value} appears as more than one node; a machine may be a node only once.");
            }

            if (node.Outgoing is null)
            {
                return Result<AuthoringRouteRows>.WithFailure(
                    $"Product {route.ProductId}: machine {node.MachineId.Value} has a null outgoing-edge collection.");
            }

            nodes.Add(new RoutingNodeRow
            {
                ProductId = route.ProductId,
                MachineId = node.MachineId,
                RoleValue = node.Role.Value,
            });

            if (node.Outgoing.Count > maxOutDegree)
            {
                maxOutDegree = node.Outgoing.Count;
            }

            foreach (var edge in node.Outgoing)
            {
                if (edge is null)
                {
                    return Result<AuthoringRouteRows>.WithFailure(
                        $"Product {route.ProductId}: machine {node.MachineId.Value} has a null outgoing edge.");
                }

                if (edge.Target.Value <= 0)
                {
                    return Result<AuthoringRouteRows>.WithFailure(
                        $"Product {route.ProductId}: machine {node.MachineId.Value} has an outgoing edge to non-positive target {edge.Target.Value}; 0 is the wire boundary, not a node.");
                }

                cleanEdges.Add(new WorkFlow
                {
                    ProductId = route.ProductId,
                    LastMachineId = node.MachineId,
                    NextMachineId = edge.Target,
                });
            }
        }

        // PRIMARY GUARD: prove the graph-validating read path accepts the authored shape BEFORE returning rows.
        var transitionsResult = RoutingTransitionMapper.ToTransitions(nodes, cleanEdges);
        if (transitionsResult.IsFailure || transitionsResult.Value is null)
        {
            return Result<AuthoringRouteRows>.WithFailure(transitionsResult.Errors);
        }

        var graphResult = ProductionGraph.Create(transitionsResult.Value);
        if (graphResult.IsFailure || graphResult.Value is null)
        {
            return Result<AuthoringRouteRows>.WithFailure(graphResult.Errors);
        }

        // Out-fan relaxation: LinearMachineSequence rejects ANY multi-successor node, so it is only applicable to
        // a linear route. Run it when the route is linear (byte-identical to the existing linear authoring
        // behavior); a genuine fork is validated by ProductionGraph.Create alone — which, since #115 finding 8,
        // enforces acyclicity itself, so a cyclic fork can no longer slip through this skipped gate.
        if (maxOutDegree <= 1)
        {
            var sequenceResult = graphResult.Value.LinearMachineSequence();
            if (sequenceResult.IsFailure || sequenceResult.Value is null)
            {
                return Result<AuthoringRouteRows>.WithFailure(sequenceResult.Errors);
            }
        }

        return Result<AuthoringRouteRows>.Success(new AuthoringRouteRows(nodes, cleanEdges));
    }

    /// <summary>
    /// Reconstructs an <see cref="AuthoringRoute"/> from persisted rows (persist → author shape, the reload for
    /// edit). Node identity/role come from the <see cref="RoutingNodeRow"/> table; each node's outgoing edges are
    /// the clean edge rows whose <see cref="WorkFlow.LastMachineId"/> is the node, projected to
    /// <see cref="AuthoringEdge"/> carrying the from-node's role (the AD-15 convention). Node order is reconstructed
    /// STRUCTURALLY — a forward traversal from the Initial node(s) following the stored edges — so a reload does not
    /// depend on row order and never re-sorts by machine id.
    /// </summary>
    /// <param name="productId">The product whose routing is being reloaded.</param>
    /// <param name="nodes">The persisted routing node rows (one per machine).</param>
    /// <param name="edges">The persisted clean interior edge rows.</param>
    /// <returns>The reconstructed <see cref="AuthoringRoute"/>, or a failure carrying the reason.</returns>
    public static Result<AuthoringRoute> FromPersisted(
        int productId,
        IReadOnlyCollection<RoutingNodeRow> nodes,
        IReadOnlyCollection<WorkFlow> edges)
    {
        if (nodes is null)
        {
            return Result<AuthoringRoute>.WithFailure(
                $"Product {productId}: the persisted routing node collection must not be null.");
        }

        if (edges is null)
        {
            return Result<AuthoringRoute>.WithFailure(
                $"Product {productId}: the persisted routing edge collection must not be null.");
        }

        // Role table (the role authority) + first-seen node order (a deterministic fallback for any node the
        // structural traversal cannot reach, e.g. a route with no Initial — surfaced later by validation, not here).
        var roleByMachine = new Dictionary<int, WorkFlowType>();
        var rowOrder = new List<int>();
        foreach (var node in nodes)
        {
            if (node is null)
            {
                return Result<AuthoringRoute>.WithFailure(
                    $"Product {productId}: a persisted routing node row must not be null.");
            }

            if (!roleByMachine.TryAdd(node.MachineId.Value, WorkFlowType.From(node.RoleValue)))
            {
                return Result<AuthoringRoute>.WithFailure(
                    $"Product {productId}: machine {node.MachineId.Value} has more than one routing node row; expected exactly one.");
            }

            rowOrder.Add(node.MachineId.Value);
        }

        // Successor lists per from-machine, in stored edge order (branch order is display-only, PO-2).
        var successors = new Dictionary<int, List<int>>();
        foreach (var edge in edges)
        {
            if (edge is null)
            {
                return Result<AuthoringRoute>.WithFailure(
                    $"Product {productId}: a persisted routing edge row must not be null.");
            }

            var from = edge.LastMachineId.Value;
            var to = edge.NextMachineId.Value;

            // Defensively ignore any residual boundary edge (a zero endpoint is the wire boundary, not a node).
            if (from <= 0 || to <= 0)
            {
                continue;
            }

            if (!roleByMachine.ContainsKey(from))
            {
                return Result<AuthoringRoute>.WithFailure(
                    $"Product {productId}: edge from machine {from} to {to} references From machine {from}, which has no routing node row.");
            }

            if (!roleByMachine.ContainsKey(to))
            {
                return Result<AuthoringRoute>.WithFailure(
                    $"Product {productId}: edge from machine {from} to {to} references To machine {to}, which has no routing node row.");
            }

            if (!successors.TryGetValue(from, out var succ))
            {
                succ = new List<int>();
                successors[from] = succ;
            }

            succ.Add(to);
        }

        // Structural node order: forward preorder from each Initial node, then any node not reached (row order).
        var ordered = new List<int>();
        var visited = new HashSet<int>();
        foreach (var machineId in rowOrder)
        {
            if (roleByMachine[machineId].Has(WorkFlowType.Initial))
            {
                VisitForward(machineId, successors, visited, ordered);
            }
        }

        foreach (var machineId in rowOrder)
        {
            VisitForward(machineId, successors, visited, ordered);
        }

        var authoringNodes = new List<AuthoringNode>(ordered.Count);
        foreach (var machineId in ordered)
        {
            var role = roleByMachine[machineId];
            var outgoing = successors.TryGetValue(machineId, out var succ)
                ? succ.Select(target => new AuthoringEdge(new MachineId(target), role)).ToList()
                : [];
            authoringNodes.Add(new AuthoringNode(new MachineId(machineId), role, outgoing));
        }

        return Result<AuthoringRoute>.Success(new AuthoringRoute(productId, authoringNodes));
    }

    /// <summary>
    /// Iterative forward preorder traversal: appends <paramref name="machineId"/> (if unvisited) to
    /// <paramref name="ordered"/>, then its successors in stored edge order, so the reconstructed node order follows
    /// the route spine rather than any storage row order or id magnitude.
    /// </summary>
    /// <param name="machineId">The traversal start machine.</param>
    /// <param name="successors">The from → ordered-successors adjacency.</param>
    /// <param name="visited">The shared visited set (accumulated across all traversal roots).</param>
    /// <param name="ordered">The accumulating node order.</param>
    private static void VisitForward(
        int machineId,
        IReadOnlyDictionary<int, List<int>> successors,
        HashSet<int> visited,
        List<int> ordered)
    {
        // Explicit stack (no recursion): a right-to-left push yields a left-to-right preorder over successors.
        var stack = new Stack<int>();
        stack.Push(machineId);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            ordered.Add(current);

            if (successors.TryGetValue(current, out var succ))
            {
                for (var i = succ.Count - 1; i >= 0; i--)
                {
                    if (!visited.Contains(succ[i]))
                    {
                        stack.Push(succ[i]);
                    }
                }
            }
        }
    }
}
