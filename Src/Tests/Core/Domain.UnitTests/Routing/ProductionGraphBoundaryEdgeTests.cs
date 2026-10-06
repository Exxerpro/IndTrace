// <copyright file="ProductionGraphBoundaryEdgeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// Pure-domain tests for the single boundary-reconstruction authority added in C2 Chunk E14:
/// <see cref="ProductionGraph.IncomingBoundaryEdge"/> / <see cref="ProductionGraph.OutgoingBoundaryEdge"/>.
/// These encode the legacy magic-0 boundary rule — an Initial machine's virtual <c>(0 -&gt; M)</c> incoming
/// edge and a Final machine's virtual <c>(M -&gt; 0)</c> outgoing edge, with interior machines using the
/// graph's predecessor/successor — that both the simulator snapshot seam and the barcode lookup now defer to
/// (previously each re-encoded it). No EF, no I/O.
/// </summary>
public class ProductionGraphBoundaryEdgeTests
{
    private static RoutingNodeRow Node(int machineId, int roleValue) =>
        new() { MachineId = new MachineId(machineId), RoleValue = roleValue };

    private static WorkFlow Edge(int from, int to) =>
        new() { LastMachineId = new MachineId(from), NextMachineId = new MachineId(to) };

    private static ProductionGraph BuildGraph(
        IReadOnlyCollection<RoutingNodeRow> nodes, IReadOnlyCollection<WorkFlow> edges)
    {
        var transitions = RoutingTransitionMapper.ToTransitions(nodes, edges).Value.ShouldNotBeNull();
        return ProductionGraph.Create(transitions).Value.ShouldNotBeNull();
    }

    /// <summary>
    /// A linear route: the Initial machine's incoming boundary is <c>(0 -&gt; first)</c>, interior machines use
    /// their predecessor/successor, and the Final machine's outgoing boundary is <c>(last -&gt; 0)</c>.
    /// </summary>
    [Fact]
    public void BoundaryEdges_ForLinearRoute_ReproduceMagicZeroBoundaries()
    {
        // Arrange: 100 (Initial|Serial) -> 400 (Serial) -> 500 (Serial|Final).
        var graph = BuildGraph(
            new[] { Node(100, 3), Node(400, 2), Node(500, 34) },
            new[] { Edge(100, 400), Edge(400, 500) });

        // Incoming boundaries.
        graph.IncomingBoundaryEdge(100).ShouldBe(new RoutingBoundaryEdge(0, 100));
        graph.IncomingBoundaryEdge(400).ShouldBe(new RoutingBoundaryEdge(100, 400));
        graph.IncomingBoundaryEdge(500).ShouldBe(new RoutingBoundaryEdge(400, 500));

        // Outgoing boundaries.
        graph.OutgoingBoundaryEdge(100).ShouldBe(new RoutingBoundaryEdge(100, 400));
        graph.OutgoingBoundaryEdge(400).ShouldBe(new RoutingBoundaryEdge(400, 500));
        graph.OutgoingBoundaryEdge(500).ShouldBe(new RoutingBoundaryEdge(500, 0));
    }

    /// <summary>
    /// A lone station (Initial|Serial|Final = 35, no edges) has the virtual <c>(0 -&gt; M)</c> incoming and
    /// <c>(M -&gt; 0)</c> outgoing boundaries.
    /// </summary>
    [Fact]
    public void BoundaryEdges_ForLoneStation_ReproduceBothWireBoundaries()
    {
        var graph = BuildGraph(new[] { Node(7, 35) }, System.Array.Empty<WorkFlow>());

        graph.IncomingBoundaryEdge(7).ShouldBe(new RoutingBoundaryEdge(0, 7));
        graph.OutgoingBoundaryEdge(7).ShouldBe(new RoutingBoundaryEdge(7, 0));
    }

    /// <summary>
    /// A machine that is not a node in the graph has no incoming or outgoing boundary edge (null), matching
    /// the old "degrade to the non-boundary default path" behavior.
    /// </summary>
    [Fact]
    public void BoundaryEdges_ForUnknownMachine_ReturnNull()
    {
        var graph = BuildGraph(
            new[] { Node(100, 3), Node(500, 34) },
            new[] { Edge(100, 500) });

        graph.IncomingBoundaryEdge(999).ShouldBeNull();
        graph.OutgoingBoundaryEdge(999).ShouldBeNull();
    }

    /// <summary>
    /// #91: a Merger node has MULTIPLE predecessors. The former <c>found[0]</c> pick depended on transition
    /// insertion order, so the reconstructed incoming boundary edge was non-deterministic. The edge is now the
    /// MINIMUM predecessor id, chosen deterministically regardless of the order the edges were fed in.
    /// </summary>
    [Fact]
    public void IncomingBoundaryEdge_ForMergerNode_DeterministicallySelectsMinimumPredecessor()
    {
        // Arrange: 100 (Initial|Serial) and 200 (Initial|Serial) both feed 500 (Serial|Merger|Final = 50).
        // Edges are fed 200-first so a naive found[0] would yield 200, not the deterministic minimum 100.
        var graph = BuildGraph(
            new[] { Node(100, 3), Node(200, 3), Node(500, 50) },
            new[] { Edge(200, 500), Edge(100, 500) });

        graph.IncomingBoundaryEdge(500).ShouldBe(new RoutingBoundaryEdge(100, 500));

        // Feeding the edges in the opposite order yields the SAME edge — order independence.
        var graphReordered = BuildGraph(
            new[] { Node(100, 3), Node(200, 3), Node(500, 50) },
            new[] { Edge(100, 500), Edge(200, 500) });

        graphReordered.IncomingBoundaryEdge(500).ShouldBe(new RoutingBoundaryEdge(100, 500));
    }

    /// <summary>
    /// #91: a Diverter node has MULTIPLE successors. The former <c>found[0]</c> pick depended on transition
    /// insertion order. The reconstructed outgoing boundary edge is now the MINIMUM successor id, chosen
    /// deterministically regardless of edge feed order.
    /// </summary>
    [Fact]
    public void OutgoingBoundaryEdge_ForDiverterNode_DeterministicallySelectsMinimumSuccessor()
    {
        // Arrange: 100 (Initial|Serial|Diverter = 11) diverges to 400 and 500 (both Serial|Final = 34).
        // Edges are fed 500-first so a naive found[0] would yield 500, not the deterministic minimum 400.
        var graph = BuildGraph(
            new[] { Node(100, 11), Node(400, 34), Node(500, 34) },
            new[] { Edge(100, 500), Edge(100, 400) });

        graph.OutgoingBoundaryEdge(100).ShouldBe(new RoutingBoundaryEdge(100, 400));

        var graphReordered = BuildGraph(
            new[] { Node(100, 11), Node(400, 34), Node(500, 34) },
            new[] { Edge(100, 400), Edge(100, 500) });

        graphReordered.OutgoingBoundaryEdge(100).ShouldBe(new RoutingBoundaryEdge(100, 400));
    }
}
