// <copyright file="RoutingTransitionMapper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;

namespace IndTrace.Domain.Routing;

/// <summary>
/// The pure-domain read mapper (C2 Chunk B) that turns persisted routing nodes
/// (<see cref="RoutingNodeRow"/>, the role source of truth) plus the product's real edges
/// (<see cref="WorkFlow"/>) into the in-memory <see cref="RoutingTransition"/> list consumed by
/// <see cref="ProductionGraph.Create(System.Collections.Generic.IReadOnlyCollection{RoutingTransition})"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is the SHARED read component the four routing consumers will call (a later chunk). It is
/// PURE: no EF, no <c>DbContext</c>, no repository, no I/O — it operates on the POCO entities directly
/// so it is unit-testable with hand-built <see cref="RoutingNodeRow"/> / <see cref="WorkFlow"/>
/// instances. It NEVER throws: every failure is a <see cref="Result{T}"/> carrying violation messages.
/// </para>
/// <para>
/// The reconstruction has two parts, mirroring the AD-15 / <see cref="RoutingTransition"/> convention:
/// <list type="number">
/// <item><description>
/// <strong>Real-edge transitions.</strong> For each real edge <c>(From, To)</c> with both endpoints
/// positive, the From node's FULL composite role (from its node row) rides the out-edge, so the
/// aggregate OR-merges it back onto the From node. Initial therefore rides the first real out-edge
/// automatically with no special case.
/// </description></item>
/// <item><description>
/// <strong>Final-terminal synthesis.</strong> Under C2 a Final node has no real out-edge, yet the
/// domain requires the Final role to be authored on a terminal <c>(C, 0, Final)</c> out-transition.
/// So for each node whose role carries <see cref="WorkFlowType.Final"/> the mapper emits
/// <c>(node, 0, role)</c>. The <c>0</c> is generated HERE, at the mapper boundary, and the aggregate
/// drops it as "no successor" — there is NO self-loop and NO stored magic-0.
/// </description></item>
/// </list>
/// A lone machine (role <c>Initial|Serial|Final</c>, no edges) is covered by step 2 alone:
/// it emits <c>(M, 0, role)</c>, which the aggregate reads as both first and last machine.
/// </para>
/// </remarks>
public static class RoutingTransitionMapper
{
    /// <summary>
    /// Maps persisted routing node rows and real workflow edges into the typed
    /// <see cref="RoutingTransition"/> list. Total and non-throwing.
    /// </summary>
    /// <param name="nodes">
    /// The routing node rows for one product — the role source of truth (one row per machine). Every
    /// machine referenced by an edge MUST have a node row here (post-backfill invariant).
    /// </param>
    /// <param name="edges">
    /// The product's REAL edges (after the C2 data migration the magic-0 pseudo-edges are gone). Any
    /// residual non-real edge (a zero From or To) is defensively skipped.
    /// </param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the transition list, or a failure carrying clear
    /// messages: a null input, or an edge whose endpoint has no node row (a data inconsistency that
    /// names the offending machine id).
    /// </returns>
    public static Result<IReadOnlyList<RoutingTransition>> ToTransitions(
        IReadOnlyCollection<RoutingNodeRow> nodes,
        IReadOnlyCollection<WorkFlow> edges)
    {
        if (nodes is null)
        {
            return Result<IReadOnlyList<RoutingTransition>>.WithFailure(
                "The routing node rows collection must not be null.");
        }

        if (edges is null)
        {
            return Result<IReadOnlyList<RoutingTransition>>.WithFailure(
                "The routing edges collection must not be null.");
        }

        // Step 1: build the machine -> role bitmask lookup from the node table (the role authority).
        // A null row, or two rows for the same machine, are data defects we surface rather than throw.
        var roleByMachine = new Dictionary<int, int>();
        foreach (var node in nodes)
        {
            if (node is null)
            {
                return Result<IReadOnlyList<RoutingTransition>>.WithFailure(
                    "A routing node row must not be null.");
            }

            if (!roleByMachine.TryAdd(node.MachineId.Value, node.RoleValue))
            {
                return Result<IReadOnlyList<RoutingTransition>>.WithFailure(
                    $"Machine {node.MachineId.Value} has more than one routing node row; expected exactly one.");
            }
        }

        var transitions = new List<RoutingTransition>();

        // Step 2: real-edge transitions. The From node's full composite role rides its out-edge.
        foreach (var edge in edges)
        {
            if (edge is null)
            {
                return Result<IReadOnlyList<RoutingTransition>>.WithFailure(
                    "A routing edge must not be null.");
            }

            // Defensively ignore any residual non-real edge (a zero endpoint is the wire boundary,
            // not a real machine-to-machine edge).
            if (edge.LastMachineId.Value <= 0 || edge.NextMachineId.Value <= 0)
            {
                continue;
            }

            // Both endpoints are nodes (To is another node); a missing node row is a data inconsistency.
            if (!roleByMachine.TryGetValue(edge.LastMachineId.Value, out var fromRole))
            {
                return Result<IReadOnlyList<RoutingTransition>>.WithFailure(
                    $"Edge from machine {edge.LastMachineId.Value} to {edge.NextMachineId.Value} references From " +
                    $"machine {edge.LastMachineId.Value}, which has no routing node row.");
            }

            if (!roleByMachine.ContainsKey(edge.NextMachineId.Value))
            {
                return Result<IReadOnlyList<RoutingTransition>>.WithFailure(
                    $"Edge from machine {edge.LastMachineId.Value} to {edge.NextMachineId.Value} references To " +
                    $"machine {edge.NextMachineId.Value}, which has no routing node row.");
            }

            transitions.Add(new RoutingTransition(
                edge.LastMachineId.Value,
                edge.NextMachineId.Value,
                WorkFlowType.From(fromRole)));
        }

        // Step 3: Final-terminal synthesis. Every Final node gets the terminal (node, 0, role)
        // out-transition the domain requires; the 0 is generated here and dropped as "no successor".
        foreach (var (machineId, roleValue) in roleByMachine)
        {
            if (WorkFlowType.From(roleValue).Has(WorkFlowType.Final))
            {
                transitions.Add(new RoutingTransition(machineId, 0, WorkFlowType.From(roleValue)));
            }
        }

        return Result<IReadOnlyList<RoutingTransition>>.Success(transitions.AsReadOnly());
    }
}
