// <copyright file="RoutingAuthoringGuardTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Routing;
using IndTrace.Domain.Routing.Authoring;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// C2 Chunk E11.0 — proves the read-side validation guards the Release-B authoring path will rely on,
/// BEFORE any authoring code exists. The E11 design (<c>docs/handoff/2026-06-26-epic3-e11-release-b-authoring-design.md</c>)
/// makes product authoring "produce the end-state D2 produces": an ordered machine sequence is turned into
/// magic-0 edges, fed through <see cref="RoutingNodeBackfill.BuildNodes"/> for roles, stripped of boundary
/// rows, mapped via <see cref="RoutingTransitionMapper"/>, and validated as a <see cref="ProductionGraph"/>
/// before persistence — so an author can never persist a product the graph-validating read path rejects.
/// </summary>
/// <remarks>
/// The bmad-party-mode Code Review Crew (2026-06-26) caught two things the design asserted but did not
/// prove. These tests pin the ground truth:
/// <list type="number">
/// <item><description>
/// <b>F1 — a duplicate-machine cycle slips <see cref="RoutingNodeBackfill.BuildNodes"/>.</b> The ordered
/// sequence <c>[100, 400, 100]</c> yields magic-0 edges in which every machine has exactly one distinct
/// successor and predecessor, so the non-linear <c>&gt; 1</c> guards pass and the node set (a HashSet)
/// dedupes machine 100 — <see cref="RoutingNodeBackfill.BuildNodes"/> returns SUCCESS for a cyclic route.
/// The cycle is caught downstream. HISTORY: originally ONLY
/// <see cref="ProductionGraph.LinearMachineSequence"/> rejected it (<see cref="ProductionGraph.Create"/>
/// succeeded), which made the linear gate load-bearing — and left the FORK authoring path (which skips the
/// linear gate) able to stage a cyclic route. Since #115 finding 8, <see cref="ProductionGraph.Create"/>
/// enforces ACYCLICITY itself, so every creation path — linear or fork — rejects the cycle.
/// </description></item>
/// <item><description>
/// <b>F2 — the lone-station / zero-edge route round-trips.</b> The single-machine sequence <c>[7]</c>
/// produces one node (role 35) and ZERO clean edges; the read path must still resolve the linear sequence
/// to <c>[7]</c> (the C3 self-loop case the C2 encoding pivot was about).
/// </description></item>
/// </list>
/// Pure-domain: no EF, no DbContext, no I/O.
/// </remarks>
public class RoutingAuthoringGuardTests
{
    private const int ProductId = 9001;

    /// <summary>
    /// Builds the legacy magic-0 edge chain (<c>0 -&gt; ids[0] -&gt; ... -&gt; ids[^1] -&gt; 0</c>) for an
    /// ordered machine sequence — the exact shape the Application-layer <c>WorkflowOrchestrator.GenerateWorkflowDtos</c>
    /// produces and the authoring path will build in memory before deriving nodes and clean edges.
    /// </summary>
    /// <param name="orderedMachineIds">The ordered machine ids (as an author would drag them).</param>
    /// <returns>The magic-0 edge rows, in chain order.</returns>
    private static IReadOnlyList<WorkFlow> MagicZeroChain(params int[] orderedMachineIds)
    {
        var edges = new List<WorkFlow>
        {
            new() { LastMachineId = new MachineId(0), NextMachineId = new MachineId(orderedMachineIds[0]) },
        };

        for (var i = 0; i < orderedMachineIds.Length - 1; i++)
        {
            edges.Add(new WorkFlow { LastMachineId = new MachineId(orderedMachineIds[i]), NextMachineId = new MachineId(orderedMachineIds[i + 1]) });
        }

        edges.Add(new WorkFlow { LastMachineId = new MachineId(orderedMachineIds[^1]), NextMachineId = new MachineId(0) });
        return edges;
    }

    /// <summary>The clean interior edges (both endpoints positive) — boundary rows stripped, as authoring persists.</summary>
    /// <param name="magicZeroEdges">The full magic-0 chain.</param>
    /// <returns>Only the real machine-to-machine edges.</returns>
    private static IReadOnlyList<WorkFlow> CleanEdges(IReadOnlyList<WorkFlow> magicZeroEdges) =>
        magicZeroEdges.Where(e => e.LastMachineId.Value > 0 && e.NextMachineId.Value > 0).ToList();

    /// <summary>
    /// F1 (premise): <see cref="RoutingNodeBackfill.BuildNodes"/> does NOT catch a duplicate-machine cycle —
    /// it returns success, assigning machine 100 the lone-station role 35 and machine 400 the Serial role 2.
    /// This documents WHY the downstream linear guard is load-bearing.
    /// </summary>
    [Fact]
    public void BuildNodes_ForDuplicateMachineCycle_ReturnsSuccess_GuardSlipsToReadPath()
    {
        // Arrange: an author lists machine 100 twice -> a 100 -> 400 -> 100 cycle.
        var magicZero = MagicZeroChain(100, 400, 100);

        // Act
        var nodes = RoutingNodeBackfill.BuildNodes(ProductId, magicZero);

        // Assert: the positional/topology guards all pass — the cycle is NOT caught here.
        nodes.IsSuccess.ShouldBeTrue();
        var rows = nodes.Value.ShouldNotBeNull();
        rows.Count.ShouldBe(2);
        rows.Single(n => n.MachineId.Value == 100).RoleValue.ShouldBe(35);
        rows.Single(n => n.MachineId.Value == 400).RoleValue.ShouldBe(2);
    }

    /// <summary>
    /// F1 (the guard): for the same cycle, <see cref="ProductionGraph.Create"/> itself REJECTS the route
    /// with an acyclicity violation. HISTORY: Create originally succeeded here and only
    /// <see cref="ProductionGraph.LinearMachineSequence"/> caught the cycle — a gate the FORK authoring
    /// path skips, which is exactly how #115 finding 8 let a cyclic fork be staged. Acyclicity now lives
    /// in Create (the one authority every creation path runs), for linear and fork routes alike; a legal
    /// rework loop would be a future, PO-gated feature.
    /// </summary>
    [Fact]
    public void AuthoringPipeline_ForDuplicateMachineCycle_CreateRejectsWithAcyclicityViolation()
    {
        // Arrange: derive nodes + clean edges exactly as the authoring path will.
        var magicZero = MagicZeroChain(100, 400, 100);
        var nodes = RoutingNodeBackfill.BuildNodes(ProductId, magicZero).Value.ShouldNotBeNull();
        var cleanEdges = CleanEdges(magicZero);

        var transitions = RoutingTransitionMapper.ToTransitions(nodes, cleanEdges).Value.ShouldNotBeNull();

        // Act
        var graph = ProductionGraph.Create(transitions);

        // Assert: Create itself rejects the cycle — no creation path (linear or fork) can slip it anymore.
        graph.IsFailure.ShouldBeTrue();
        string.Join(" ", graph.Errors).ShouldContain("cycle");
    }

    /// <summary>
    /// F2: the lone-station route (one machine, zero clean edges) round-trips through the full pipeline to a
    /// single-element linear sequence <c>[7]</c>.
    /// </summary>
    [Fact]
    public void AuthoringPipeline_ForLoneStation_RoundTripsToSingleMachineSequence()
    {
        // Arrange: a single-machine route -> node role 35, no interior edges.
        var magicZero = MagicZeroChain(7);
        var nodes = RoutingNodeBackfill.BuildNodes(ProductId, magicZero).Value.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);
        nodes.Single().RoleValue.ShouldBe(35);

        var cleanEdges = CleanEdges(magicZero);
        cleanEdges.Count.ShouldBe(0);

        var transitions = RoutingTransitionMapper.ToTransitions(nodes, cleanEdges).Value.ShouldNotBeNull();

        // Act
        var graph = ProductionGraph.Create(transitions);

        // Assert
        graph.IsSuccess.ShouldBeTrue();
        var sequence = graph.Value.ShouldNotBeNull().LinearMachineSequence();
        sequence.IsSuccess.ShouldBeTrue();
        sequence.Value.ShouldNotBeNull().ShouldBe(new[] { 7 });
    }

    /// <summary>
    /// Issue #115 finding 8 — the FORK authoring path must reject a cyclic route. A fork (out-degree &gt; 1)
    /// intentionally skips <see cref="ProductionGraph.LinearMachineSequence"/> (which rejects any
    /// multi-successor node), so acyclicity must be enforced by <see cref="ProductionGraph.Create"/> itself.
    /// Cycles are ILLEGAL in fork routes exactly as in linear routes — the system-wide invariant is acyclic
    /// routing; a legal rework loop would be a future, PO-gated feature.
    /// </summary>
    [Fact]
    public void ToPersistenceRows_ForForkRouteWithCycle_FailsWithAcyclicityViolation()
    {
        // Arrange: fork 100 -> {200, 300}; 200 -> 100 closes the 100 -> 200 -> 100 cycle; 300 is Final.
        // Every pre-#115 Create rule passes (roles sanctioned, all reachable, no dead-end, no self-loop),
        // and maxOutDegree == 2 skips the linear cycle gate — so only a Create-level acyclicity check
        // can reject this shape.
        var forkRole = WorkFlowType.From(
            WorkFlowType.Initial.Value | WorkFlowType.Serial.Value | WorkFlowType.Diverter.Value); // 11
        var route = new AuthoringRoute(ProductId, new List<AuthoringNode>
        {
            new(new MachineId(100), forkRole, [new(new MachineId(200), forkRole), new(new MachineId(300), forkRole)]),
            new(new MachineId(200), WorkFlowType.Serial, [new(new MachineId(100), WorkFlowType.Serial)]),
            new(new MachineId(300), WorkFlowType.From(WorkFlowType.Serial.Value | WorkFlowType.Final.Value), []),
        });

        // Act
        var rows = AuthoringRouteMapper.ToPersistenceRows(route);

        // Assert: the cyclic fork must NOT produce persistable rows.
        rows.IsFailure.ShouldBeTrue();
        rows.Value.ShouldBeNull();
        string.Join(" ", rows.Errors).ShouldContain("cycle");
    }

    /// <summary>
    /// A genuinely linear authored route round-trips to its ordered sequence — the happy path the cycle and
    /// lone-station cases are guarding (control assertion that the pipeline itself is sound).
    /// </summary>
    [Fact]
    public void AuthoringPipeline_ForLinearRoute_RoundTripsToOrderedSequence()
    {
        // Arrange
        var magicZero = MagicZeroChain(100, 400, 500);
        var nodes = RoutingNodeBackfill.BuildNodes(ProductId, magicZero).Value.ShouldNotBeNull();
        var cleanEdges = CleanEdges(magicZero);
        var transitions = RoutingTransitionMapper.ToTransitions(nodes, cleanEdges).Value.ShouldNotBeNull();

        // Act
        var graph = ProductionGraph.Create(transitions);

        // Assert
        graph.IsSuccess.ShouldBeTrue();
        var sequence = graph.Value.ShouldNotBeNull().LinearMachineSequence();
        sequence.IsSuccess.ShouldBeTrue();
        sequence.Value.ShouldNotBeNull().ShouldBe(new[] { 100, 400, 500 });
    }
}
