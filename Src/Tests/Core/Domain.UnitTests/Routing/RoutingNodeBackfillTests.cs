// <copyright file="RoutingNodeBackfillTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// Pure-domain unit tests for <see cref="RoutingNodeBackfill"/> (C2 Chunk D1): the role-computation
/// service that derives the <see cref="RoutingNodeRow"/> backfill (machine roles) from a product's
/// legacy magic-0 <see cref="WorkFlow"/> edge rows, with loud guards for non-linear / malformed input.
/// Entirely in memory — no EF, no DbContext, no I/O.
/// </summary>
public class RoutingNodeBackfillTests
{
    /// <summary>
    /// Builds a legacy magic-0 <see cref="WorkFlow"/> edge from <paramref name="from"/> (Last) to
    /// <paramref name="to"/> (Next).
    /// </summary>
    /// <param name="from">The From (last) machine id; <c>0</c> is the wire boundary.</param>
    /// <param name="to">The To (next) machine id; <c>0</c> is the wire boundary.</param>
    /// <returns>The constructed edge.</returns>
    private static WorkFlow Edge(int from, int to) =>
        new() { LastMachineId = new MachineId(from), NextMachineId = new MachineId(to) };

    /// <summary>
    /// Computes the backfill, asserting success and returning the non-null node list.
    /// </summary>
    /// <param name="productId">The product id.</param>
    /// <param name="edges">The legacy magic-0 edge rows.</param>
    /// <returns>The computed routing node rows.</returns>
    private static IReadOnlyList<RoutingNodeRow> Build(int productId, IReadOnlyCollection<WorkFlow> edges)
    {
        var result = RoutingNodeBackfill.BuildNodes(productId, edges);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// Linear product 566: rows (0,100),(100,400),(400,500),(500,0) backfill to roles
    /// {100:Initial|Serial=3, 400:Serial=2, 500:Serial|Final=34}. Cross-checks the mapper + aggregate.
    /// </summary>
    [Fact]
    public void BuildNodes_ForLinear566_ProducesInitialSerialFinalRolesAndAgreesWithMapper()
    {
        // Arrange
        var edges = new[] { Edge(0, 100), Edge(100, 400), Edge(400, 500), Edge(500, 0) };

        // Act
        var nodes = Build(566, edges);

        // Assert: exactly {(566,100,3),(566,400,2),(566,500,34)}
        nodes.Count.ShouldBe(3);
        nodes.ShouldContain(n => n.ProductId == 566 && n.MachineId.Value == 100 && n.RoleValue == 3);
        nodes.ShouldContain(n => n.ProductId == 566 && n.MachineId.Value == 400 && n.RoleValue == 2);
        nodes.ShouldContain(n => n.ProductId == 566 && n.MachineId.Value == 500 && n.RoleValue == 34);
        nodes.ShouldAllBe(n => n.MachineId.Value != 0);

        // Cross-check D1 and Chunk B agree: feed the computed nodes + the REAL edges into the read-side
        // mapper, then into the aggregate, and assert it builds a valid linear sequence.
        var realEdges = new[] { Edge(100, 400), Edge(400, 500) };
        var mapped = RoutingTransitionMapper.ToTransitions(nodes, realEdges);
        mapped.IsSuccess.ShouldBeTrue();

        var graph = ProductionGraph.Create(mapped.Value.ShouldNotBeNull());
        graph.IsSuccess.ShouldBeTrue();
        var sequence = graph.Value.ShouldNotBeNull().LinearMachineSequence();
        sequence.IsSuccess.ShouldBeTrue();
        sequence.Value.ShouldNotBeNull().ShouldBe(new[] { 100, 400, 500 });
    }

    /// <summary>
    /// A lone station (first AND last): rows (0,7),(7,0) backfill to {(p,7,Initial|Serial|Final=35)}
    /// — NOT Initial|Final=33, which is illegal (P6).
    /// </summary>
    [Fact]
    public void BuildNodes_ForLoneStation_ProducesInitialSerialFinalNotInitialFinal()
    {
        // Arrange
        var edges = new[] { Edge(0, 7), Edge(7, 0) };

        // Act
        var nodes = Build(42, edges);

        // Assert
        nodes.Count.ShouldBe(1);
        nodes.ShouldContain(n => n.ProductId == 42 && n.MachineId.Value == 7 && n.RoleValue == 35);
    }

    /// <summary>
    /// Two-machine linear: rows (0,1),(1,2),(2,0) backfill to {1:Initial|Serial=3, 2:Serial|Final=34}.
    /// </summary>
    [Fact]
    public void BuildNodes_ForTwoMachineLinear_ProducesInitialSerialAndSerialFinal()
    {
        // Arrange
        var edges = new[] { Edge(0, 1), Edge(1, 2), Edge(2, 0) };

        // Act
        var nodes = Build(7, edges);

        // Assert
        nodes.Count.ShouldBe(2);
        nodes.ShouldContain(n => n.MachineId.Value == 1 && n.RoleValue == 3);
        nodes.ShouldContain(n => n.MachineId.Value == 2 && n.RoleValue == 34);
    }

    /// <summary>
    /// Guard — a (0,0) row (single-station-as-one-row, a data defect) fails and names the product.
    /// </summary>
    [Fact]
    public void BuildNodes_WhenZeroToZeroRowPresent_FailsNamingProduct()
    {
        // Arrange
        var edges = new[] { Edge(0, 0), Edge(0, 1), Edge(1, 0) };

        // Act
        var result = RoutingNodeBackfill.BuildNodes(999, edges);

        // Assert
        result.IsFailure.ShouldBeTrue();
        string.Join(" ", result.Errors).ShouldContain("999");
    }

    /// <summary>
    /// Guard — a non-linear diverter (machine 1 has two distinct real successors) fails and names
    /// machine 1: the positional rule is unsafe for a branch.
    /// </summary>
    [Fact]
    public void BuildNodes_WhenMachineHasTwoSuccessors_FailsNamingMachine()
    {
        // Arrange: (0,1),(1,2),(1,3),(2,0),(3,0) — machine 1 splits to 2 and 3.
        var edges = new[] { Edge(0, 1), Edge(1, 2), Edge(1, 3), Edge(2, 0), Edge(3, 0) };

        // Act
        var result = RoutingNodeBackfill.BuildNodes(5, edges);

        // Assert
        result.IsFailure.ShouldBeTrue();
        string.Join(" ", result.Errors).ShouldContain("1");
    }

    /// <summary>
    /// Guard — a non-linear merger (machine 3 has two distinct real predecessors) fails and names
    /// machine 3.
    /// </summary>
    [Fact]
    public void BuildNodes_WhenMachineHasTwoPredecessors_FailsNamingMachine()
    {
        // Arrange: (0,1),(0,2),(1,3),(2,3),(3,0) — machine 3 joins 1 and 2.
        var edges = new[] { Edge(0, 1), Edge(0, 2), Edge(1, 3), Edge(2, 3), Edge(3, 0) };

        // Act
        var result = RoutingNodeBackfill.BuildNodes(6, edges);

        // Assert
        result.IsFailure.ShouldBeTrue();
        string.Join(" ", result.Errors).ShouldContain("3");
    }

    /// <summary>
    /// Guard — missing the (0,*) first boundary fails (every real product is bounded by a first edge).
    /// </summary>
    [Fact]
    public void BuildNodes_WhenNoFirstBoundary_Fails()
    {
        // Arrange: no (0,*) row.
        var edges = new[] { Edge(1, 2), Edge(2, 0) };

        // Act
        var result = RoutingNodeBackfill.BuildNodes(8, edges);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// Guard — missing the (*,0) last boundary fails (every real product is bounded by a last edge).
    /// </summary>
    [Fact]
    public void BuildNodes_WhenNoLastBoundary_Fails()
    {
        // Arrange: no (*,0) row.
        var edges = new[] { Edge(0, 1), Edge(1, 2) };

        // Act
        var result = RoutingNodeBackfill.BuildNodes(9, edges);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// Guard — a disconnected / dead-end machine (not on any path from a first to a last) fails and
    /// names it. The lone machine 9 here is reachable from no start and reaches no end.
    /// </summary>
    [Fact]
    public void BuildNodes_WhenMachineIsDisconnected_FailsNamingMachine()
    {
        // Arrange: linear 1->2 bounded, plus an island edge (9,9-style) that touches neither boundary.
        // Use (8,9) island: 8 has no predecessor path from a (0,*) start and 9 reaches no (*,0) end.
        var edges = new[] { Edge(0, 1), Edge(1, 2), Edge(2, 0), Edge(8, 9) };

        // Act
        var result = RoutingNodeBackfill.BuildNodes(11, edges);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var joined = string.Join(" ", result.Errors);
        (joined.Contains('8') || joined.Contains('9')).ShouldBeTrue();
    }

    /// <summary>
    /// Null inputs / a null row fail the house guard (Result, no throw). The null SUT here is the guard.
    /// </summary>
    [Fact]
    public void BuildNodes_WhenInputsNull_Fails()
    {
        RoutingNodeBackfill.BuildNodes(1, null!).IsFailure.ShouldBeTrue();

        var edgesWithNull = new WorkFlow?[] { Edge(0, 1), null, Edge(1, 0) };
        RoutingNodeBackfill.BuildNodes(1, edgesWithNull!).IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// #126 F4 regression — the emitted rows must be in a DETERMINISTIC order (ascending machine id),
    /// independent of the edge-row insertion sequence. The chain runs 300 → 200 → 100, so the machines
    /// are first SEEN in descending order; iterating the internal <c>HashSet</c> directly (the defect)
    /// emits insertion order and breaks the byte-compare / golden-order verification the C2 migration
    /// relies on. Two row orderings of the SAME graph must emit the identical ascending sequence.
    /// </summary>
    [Fact]
    public void BuildNodes_EmitsRowsInAscendingMachineIdOrder_RegardlessOfEdgeRowOrder()
    {
        // Arrange: linear chain 300 -> 200 -> 100 (machine ids intentionally descending along the flow),
        // presented once in chain order and once shuffled.
        var chainOrder = new[] { Edge(0, 300), Edge(300, 200), Edge(200, 100), Edge(100, 0) };
        var shuffled = new[] { Edge(200, 100), Edge(0, 300), Edge(100, 0), Edge(300, 200) };

        // Act
        var fromChainOrder = Build(9, chainOrder).Select(n => n.MachineId.Value).ToArray();
        var fromShuffled = Build(9, shuffled).Select(n => n.MachineId.Value).ToArray();

        // Assert: deterministic ascending machine-id emission, identical for both input orderings.
        fromChainOrder.ShouldBe(new[] { 100, 200, 300 });
        fromShuffled.ShouldBe(new[] { 100, 200, 300 });
    }
}
