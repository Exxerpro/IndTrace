// <copyright file="RoutingTransitionMapperTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// Pure-domain unit tests for <see cref="RoutingTransitionMapper"/> (C2 Chunk B): the read-side
/// mapper that turns persisted <see cref="RoutingNodeRow"/>s plus real <see cref="WorkFlow"/> edges
/// into the <see cref="RoutingTransition"/> list consumed by <see cref="ProductionGraph.Create"/>.
/// Entirely in memory — no EF, no DbContext, no I/O.
/// </summary>
public class RoutingTransitionMapperTests
{
    /// <summary>
    /// Builds a <see cref="RoutingNodeRow"/> for the given machine and role bitmask.
    /// </summary>
    /// <param name="machineId">The machine id.</param>
    /// <param name="roleValue">The composite role bitmask.</param>
    /// <returns>The constructed node row.</returns>
    private static RoutingNodeRow Node(int machineId, int roleValue) =>
        new() { MachineId = new MachineId(machineId), RoleValue = roleValue };

    /// <summary>
    /// Builds a real <see cref="WorkFlow"/> edge from <paramref name="from"/> to <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The From (last) machine id.</param>
    /// <param name="to">The To (next) machine id.</param>
    /// <returns>The constructed edge.</returns>
    private static WorkFlow Edge(int from, int to) =>
        new() { LastMachineId = new MachineId(from), NextMachineId = new MachineId(to) };

    /// <summary>
    /// Maps the inputs, asserting success and returning the non-null transition list.
    /// </summary>
    /// <param name="nodes">The node rows.</param>
    /// <param name="edges">The real edges.</param>
    /// <returns>The mapped transitions.</returns>
    private static IReadOnlyList<RoutingTransition> Map(
        IReadOnlyCollection<RoutingNodeRow> nodes,
        IReadOnlyCollection<WorkFlow> edges)
    {
        var result = RoutingTransitionMapper.ToTransitions(nodes, edges);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// A linear route (product 566): three nodes, two real edges, one synthesized Final terminal.
    /// </summary>
    [Fact]
    public void ToTransitions_ForLinearRoute_ProducesExactTransitionsAndLinearSequence()
    {
        // Arrange
        var nodes = new[] { Node(100, 3), Node(400, 2), Node(500, 34) };
        var edges = new[] { Edge(100, 400), Edge(400, 500) };

        // Act
        var transitions = Map(nodes, edges);

        // Assert: exactly {(100,400,3),(400,500,2),(500,0,34)}
        transitions.Count.ShouldBe(3);
        transitions.ShouldContain(t => t.FromMachineId == 100 && t.ToMachineId == 400 && t.Role.Value == 3);
        transitions.ShouldContain(t => t.FromMachineId == 400 && t.ToMachineId == 500 && t.Role.Value == 2);
        transitions.ShouldContain(t => t.FromMachineId == 500 && t.ToMachineId == 0 && t.Role.Value == 34);

        var graph = ProductionGraph.Create(transitions);
        graph.IsSuccess.ShouldBeTrue();
        var sequence = graph.Value.ShouldNotBeNull().LinearMachineSequence();
        sequence.IsSuccess.ShouldBeTrue();
        sequence.Value.ShouldNotBeNull().ShouldBe(new[] { 100, 400, 500 });
    }

    /// <summary>
    /// A lone machine (Initial|Serial|Final = 35) with no edges yields a single (M,0,35) terminal.
    /// </summary>
    [Fact]
    public void ToTransitions_ForLoneMachine_ProducesSingleTerminalAndIsInitialAndFinal()
    {
        // Arrange
        var nodes = new[] { Node(7, 35) };
        var edges = System.Array.Empty<WorkFlow>();

        // Act
        var transitions = Map(nodes, edges);

        // Assert
        transitions.Count.ShouldBe(1);
        transitions.ShouldContain(t => t.FromMachineId == 7 && t.ToMachineId == 0 && t.Role.Value == 35);

        var graph = ProductionGraph.Create(transitions);
        graph.IsSuccess.ShouldBeTrue();
        var built = graph.Value.ShouldNotBeNull();
        built.IsInitialMachine(7).ShouldBeTrue();
        built.IsFinalMachine(7).ShouldBeTrue();
    }

    /// <summary>
    /// A synthetic diverter route (role 10 = Serial|Diverter) maps and builds successfully.
    /// </summary>
    [Fact]
    public void ToTransitions_ForDiverter_ProducesRealEdgesPlusTerminalsAndBuilds()
    {
        // Arrange
        var nodes = new[] { Node(1, 3), Node(2, 10), Node(3, 34), Node(4, 34) };
        var edges = new[] { Edge(1, 2), Edge(2, 3), Edge(2, 4) };

        // Act
        var transitions = Map(nodes, edges);

        // Assert: 3 real edges + 2 terminals (nodes 3, 4 are Final)
        transitions.Count.ShouldBe(5);
        transitions.ShouldContain(t => t.FromMachineId == 1 && t.ToMachineId == 2 && t.Role.Value == 3);
        transitions.ShouldContain(t => t.FromMachineId == 2 && t.ToMachineId == 3 && t.Role.Value == 10);
        transitions.ShouldContain(t => t.FromMachineId == 2 && t.ToMachineId == 4 && t.Role.Value == 10);
        transitions.ShouldContain(t => t.FromMachineId == 3 && t.ToMachineId == 0 && t.Role.Value == 34);
        transitions.ShouldContain(t => t.FromMachineId == 4 && t.ToMachineId == 0 && t.Role.Value == 34);

        ProductionGraph.Create(transitions).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// A synthetic merger route (two Initials, merger role 18, node 3 with two predecessors) builds.
    /// </summary>
    [Fact]
    public void ToTransitions_ForMerger_BuildsSuccessfully()
    {
        // Arrange
        var nodes = new[] { Node(1, 3), Node(2, 3), Node(3, 18), Node(4, 34) };
        var edges = new[] { Edge(1, 3), Edge(2, 3), Edge(3, 4) };

        // Act
        var transitions = Map(nodes, edges);

        // Assert
        transitions.ShouldContain(t => t.FromMachineId == 1 && t.ToMachineId == 3 && t.Role.Value == 3);
        transitions.ShouldContain(t => t.FromMachineId == 2 && t.ToMachineId == 3 && t.Role.Value == 3);
        transitions.ShouldContain(t => t.FromMachineId == 3 && t.ToMachineId == 4 && t.Role.Value == 18);
        transitions.ShouldContain(t => t.FromMachineId == 4 && t.ToMachineId == 0 && t.Role.Value == 34);

        var graph = ProductionGraph.Create(transitions);
        graph.IsSuccess.ShouldBeTrue();
        var preds = graph.Value.ShouldNotBeNull().Predecessors(3);
        preds.IsSuccess.ShouldBeTrue();
        preds.Value.ShouldNotBeNull().Count.ShouldBe(2);
    }

    /// <summary>
    /// The mapper never emits a self-loop transition (From == To), which would re-introduce a marker.
    /// </summary>
    [Fact]
    public void ToTransitions_NeverProducesSelfLoop()
    {
        // Arrange
        var nodes = new[] { Node(1, 3), Node(2, 10), Node(3, 34), Node(4, 34) };
        var edges = new[] { Edge(1, 2), Edge(2, 3), Edge(2, 4) };

        // Act
        var transitions = Map(nodes, edges);

        // Assert
        transitions.ShouldAllBe(t => t.FromMachineId != t.ToMachineId);
    }

    /// <summary>
    /// An edge whose endpoint has no node row is a data inconsistency: the mapper fails and names it.
    /// </summary>
    [Fact]
    public void ToTransitions_WhenEdgeReferencesMachineWithNoNode_Fails()
    {
        // Arrange: node 999 referenced by an edge has no node row.
        var nodes = new[] { Node(100, 3), Node(400, 34) };
        var edges = new[] { Edge(100, 400), Edge(400, 999) };

        // Act
        var result = RoutingTransitionMapper.ToTransitions(nodes, edges);

        // Assert
        result.IsFailure.ShouldBeTrue();
        string.Join(" ", result.Errors).ShouldContain("999");
    }

    /// <summary>
    /// Null inputs fail the house guard (Result, no throw).
    /// </summary>
    [Fact]
    public void ToTransitions_WhenInputsNull_Fails()
    {
        RoutingTransitionMapper.ToTransitions(null!, System.Array.Empty<WorkFlow>()).IsFailure.ShouldBeTrue();
        RoutingTransitionMapper.ToTransitions(System.Array.Empty<RoutingNodeRow>(), null!).IsFailure.ShouldBeTrue();
    }
}
