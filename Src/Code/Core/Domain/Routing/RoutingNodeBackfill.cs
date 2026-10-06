// <copyright file="RoutingNodeBackfill.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing;

/// <summary>
/// The pure-domain backfill service (C2 Chunk D1) that computes a product's routing-node roles from its
/// legacy magic-0 <see cref="WorkFlow"/> edge rows. The destructive C2 migration (a later chunk) calls
/// this to derive the <see cref="RoutingNodeRow"/> rows it inserts; isolating the positional role logic
/// here makes it fully unit-testable.
/// </summary>
/// <remarks>
/// <para>
/// This service is PURE: no EF, no <c>DbContext</c>, no repository, no I/O. It operates on the POCO
/// <see cref="WorkFlow"/> rows directly and NEVER throws — every failure is a <see cref="Result{T}"/>
/// carrying violation messages. It emits the <see cref="RoutingNodeRow.ProductId"/> and
/// <see cref="RoutingNodeRow.RoleValue"/> only; <see cref="RoutingNodeRow.RoutingNodeId"/> and the audit
/// fields are left at their defaults for the migration to fill.
/// </para>
/// <para>
/// <strong>Magic-0 convention.</strong> An edge row carries a direction From = <see cref="WorkFlow.LastMachineId"/>,
/// To = <see cref="WorkFlow.NextMachineId"/>. A row <c>(0, A)</c> is the initial pseudo-edge (A is a first
/// machine), <c>(C, 0)</c> the final pseudo-edge (C is a last machine), and <c>(A, B)</c> with both
/// endpoints positive a real machine-to-machine edge. The id <c>0</c> is the wire boundary, never a node.
/// </para>
/// <para>
/// <strong>Positional roles (linear data).</strong> Every real machine does work, so its base role is
/// <see cref="WorkFlowType.Serial"/>. A first machine additionally carries <see cref="WorkFlowType.Initial"/>,
/// a last machine <see cref="WorkFlowType.Final"/>. A lone station that is BOTH first and last becomes
/// <c>Initial|Serial|Final</c> (35), never <c>Initial|Final</c> (33, illegal per ProductionGraph P6). The
/// composed roles for a linear line are therefore 3 (first), 2 (interior), 34 (last), 35 (lone).
/// </para>
/// <para>
/// <strong>Loud guards.</strong> The positional rule is only valid for linear topology, so the service
/// fails (returning a violation, writing NO partial nodes) on: a <c>(0,0)</c> row; any real machine with
/// more than one distinct real successor or predecessor (a branch/merge a human must author); a missing
/// first or last boundary; a disconnected / dead-end machine not on any path from a first to a last; and
/// the null guards. These never trigger on today's linear data; they protect against malformed input.
/// </para>
/// </remarks>
public static class RoutingNodeBackfill
{
    /// <summary>
    /// Computes the routing-node backfill rows for ONE product from its legacy magic-0
    /// <see cref="WorkFlow"/> edge rows. Total and non-throwing.
    /// </summary>
    /// <param name="productId">The product whose nodes are being backfilled (named in any failure message).</param>
    /// <param name="productEdges">
    /// The product's legacy magic-0 edge rows (initial/final pseudo-edges plus real edges). Must include a
    /// <c>(0, *)</c> first boundary and a <c>(*, 0)</c> last boundary.
    /// </param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping exactly one <see cref="RoutingNodeRow"/> per real
    /// machine, emitted in deterministic ascending machine-id order regardless of edge-row order
    /// (<see cref="RoutingNodeRow.RoleValue"/> and <see cref="RoutingNodeRow.ProductId"/> set;
    /// id and audit left default), or a failure carrying clear messages when the input is null, contains a
    /// null row, has a <c>(0,0)</c> row, is non-linear (a branch or merge), is missing a boundary, or has a
    /// disconnected / dead-end machine.
    /// </returns>
    public static Result<IReadOnlyList<RoutingNodeRow>> BuildNodes(
        int productId,
        IReadOnlyCollection<WorkFlow> productEdges)
    {
        if (productEdges is null)
        {
            return Result<IReadOnlyList<RoutingNodeRow>>.WithFailure(
                $"Product {productId}: the workflow edge collection must not be null.");
        }

        // Boundary and adjacency sets, built in a single pass with a null-row guard.
        var firstMachines = new HashSet<int>();
        var lastMachines = new HashSet<int>();
        var realMachines = new HashSet<int>();
        var successors = new Dictionary<int, HashSet<int>>();
        var predecessors = new Dictionary<int, HashSet<int>>();

        foreach (var edge in productEdges)
        {
            if (edge is null)
            {
                return Result<IReadOnlyList<RoutingNodeRow>>.WithFailure(
                    $"Product {productId}: a workflow edge row must not be null.");
            }

            var from = edge.LastMachineId.Value;
            var to = edge.NextMachineId.Value;

            // Guard 1: a (0,0) row is a single-station-as-one-row data defect (none exist today).
            if (from == 0 && to == 0)
            {
                return Result<IReadOnlyList<RoutingNodeRow>>.WithFailure(
                    $"Product {productId}: a (0,0) workflow edge row is a data defect (single-station-as-one-row).");
            }

            if (from == 0)
            {
                // Initial pseudo-edge: To is a first machine.
                firstMachines.Add(to);
            }
            else if (to == 0)
            {
                // Final pseudo-edge: From is a last machine.
                lastMachines.Add(from);
            }
            else
            {
                // A real machine-to-machine edge.
                AddAdjacency(successors, from, to);
                AddAdjacency(predecessors, to, from);
            }

            if (from > 0)
            {
                realMachines.Add(from);
            }

            if (to > 0)
            {
                realMachines.Add(to);
            }
        }

        // Product-level boundary guard: every real product is bounded by both a first and a last edge.
        if (firstMachines.Count == 0)
        {
            return Result<IReadOnlyList<RoutingNodeRow>>.WithFailure(
                $"Product {productId}: no initial (0,*) boundary edge was found; the routing is unbounded.");
        }

        if (lastMachines.Count == 0)
        {
            return Result<IReadOnlyList<RoutingNodeRow>>.WithFailure(
                $"Product {productId}: no final (*,0) boundary edge was found; the routing is unbounded.");
        }

        // Guard 2: non-linear topology — any real machine with more than one DISTINCT real successor or
        // predecessor. The positional rule is unsafe for branches/merges; a human must author the roles.
        foreach (var (machine, outs) in successors)
        {
            if (outs.Count > 1)
            {
                return Result<IReadOnlyList<RoutingNodeRow>>.WithFailure(
                    $"Product {productId}: machine {machine} has {outs.Count} distinct real successors " +
                    "(non-linear branch); a human must author its routing role.");
            }
        }

        foreach (var (machine, ins) in predecessors)
        {
            if (ins.Count > 1)
            {
                return Result<IReadOnlyList<RoutingNodeRow>>.WithFailure(
                    $"Product {productId}: machine {machine} has {ins.Count} distinct real predecessors " +
                    "(non-linear merge); a human must author its routing role.");
            }
        }

        // Guard 4: disconnected / dead-end — every real machine must lie on a path from a first machine
        // to a last machine, i.e. be both forward-reachable from a first and able to reach a last.
        var reachableFromFirst = ForwardClosure(firstMachines, successors);
        var reachesLast = ForwardClosure(lastMachines, predecessors);

        foreach (var machine in realMachines)
        {
            if (!reachableFromFirst.Contains(machine) || !reachesLast.Contains(machine))
            {
                return Result<IReadOnlyList<RoutingNodeRow>>.WithFailure(
                    $"Product {productId}: machine {machine} is disconnected (not on any path from a first " +
                    "machine to a last machine).");
            }
        }

        // Compose roles. Every real machine does work (Serial); first adds Initial, last adds Final.
        // Emission order is DETERMINISTIC — ascending machine id — never raw HashSet iteration order
        // (#126 F4): the C2 migration byte-compares / golden-orders the emitted rows, so the order must
        // not depend on hash-set internals or edge-row insertion sequence.
        var nodes = new List<RoutingNodeRow>();
        foreach (var machine in realMachines.Order())
        {
            // Guard 3: the positional logic must never point a node at the wire boundary.
            if (machine == 0)
            {
                return Result<IReadOnlyList<RoutingNodeRow>>.WithFailure(
                    $"Product {productId}: the backfill produced a node for machine id 0 (the wire boundary).");
            }

            var role = WorkFlowType.Serial.Value;
            if (firstMachines.Contains(machine))
            {
                role |= WorkFlowType.Initial.Value;
            }

            if (lastMachines.Contains(machine))
            {
                role |= WorkFlowType.Final.Value;
            }

            nodes.Add(new RoutingNodeRow
            {
                ProductId = productId,
                MachineId = new MachineId(machine),
                RoleValue = role,
            });
        }

        return Result<IReadOnlyList<RoutingNodeRow>>.Success(nodes.AsReadOnly());
    }

    /// <summary>
    /// Records a directed adjacency <paramref name="from"/> -&gt; <paramref name="to"/> in
    /// <paramref name="adjacency"/>, keeping the successor set distinct.
    /// </summary>
    /// <param name="adjacency">The adjacency map to extend.</param>
    /// <param name="from">The source machine id (the map key).</param>
    /// <param name="to">The destination machine id added to the source's set.</param>
    private static void AddAdjacency(Dictionary<int, HashSet<int>> adjacency, int from, int to)
    {
        if (!adjacency.TryGetValue(from, out var set))
        {
            set = new HashSet<int>();
            adjacency[from] = set;
        }

        set.Add(to);
    }

    /// <summary>
    /// Returns the set of machines reachable from any seed by following <paramref name="adjacency"/>
    /// (the seeds themselves are included). Used both forward (over successors, seeded by first machines)
    /// and backward (over predecessors, seeded by last machines).
    /// </summary>
    /// <param name="seeds">The traversal start machines.</param>
    /// <param name="adjacency">The directed adjacency to traverse.</param>
    /// <returns>The reachable closure including the seeds.</returns>
    private static HashSet<int> ForwardClosure(
        IEnumerable<int> seeds,
        IReadOnlyDictionary<int, HashSet<int>> adjacency)
    {
        var visited = new HashSet<int>();
        var stack = new Stack<int>(seeds);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            if (adjacency.TryGetValue(current, out var next))
            {
                foreach (var neighbour in next)
                {
                    if (!visited.Contains(neighbour))
                    {
                        stack.Push(neighbour);
                    }
                }
            }
        }

        return visited;
    }
}
