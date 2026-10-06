// <copyright file="ProductionGraphTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// Pure-domain unit tests for the <see cref="ProductionGraph"/> aggregate and its validating
/// factory (Story 2.1). These tests prove construction-level validity, the typed-node
/// reconstruction, and the AD-5 traversal projections — entirely in memory, with no EF or
/// infrastructure type involved.
/// </summary>
public class ProductionGraphTests
{
    /// <summary>
    /// A degenerate linear route: Initial -> Serial -> Final, expressed as transitions whose
    /// role is keyed on the <c>From</c> machine of each out-transition.
    /// </summary>
    /// <returns>The transitions describing machines 10 -> 20 -> 30.</returns>
    private static IReadOnlyList<RoutingTransition> LinearTransitions() =>
    [
        new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
        new RoutingTransition(20, 30, WorkFlowType.Serial),
        new RoutingTransition(30, 0, WorkFlowType.Final),
    ];

    /// <summary>
    /// Creates a graph from the supplied transitions, asserting success and returning the non-null
    /// aggregate (no null-forgiving operator).
    /// </summary>
    /// <param name="transitions">The transitions to build from.</param>
    /// <returns>The constructed <see cref="ProductionGraph"/>.</returns>
    private static ProductionGraph BuildGraph(IReadOnlyList<RoutingTransition> transitions)
    {
        var result = ProductionGraph.Create(transitions);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// The factory returns a success <see cref="Result{ProductionGraph}"/> for a well-formed
    /// linear route and never throws.
    /// </summary>
    [Fact]
    public void Create_ForLinearRoute_ShouldSucceed()
    {
        // Act
        var result = ProductionGraph.Create(LinearTransitions());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// The factory rejects a null transition collection without throwing.
    /// </summary>
    [Fact]
    public void Create_ForNullTransitions_ShouldFail()
    {
        // Act
        var result = ProductionGraph.Create(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("transitions", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The factory rejects an empty transition collection (a fundamental, blocking failure).
    /// </summary>
    [Fact]
    public void Create_ForEmptyTransitions_ShouldFail()
    {
        // Act
        var result = ProductionGraph.Create([]);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// A transition carrying a machine id below zero is malformed and is rejected at construction.
    /// </summary>
    [Fact]
    public void Create_ForNegativeMachineId_ShouldFail()
    {
        // Arrange
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(-1, 20, WorkFlowType.Initial),
        ];

        // Act
        var result = ProductionGraph.Create(transitions);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// A transition carrying the <see cref="WorkFlowType.Invalid"/> role is malformed and is
    /// rejected at construction (the role bits were out of range).
    /// </summary>
    [Fact]
    public void Create_ForInvalidRole_ShouldFail()
    {
        // Arrange
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.Invalid),
        ];

        // Act
        var result = ProductionGraph.Create(transitions);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// Success exposes the typed <see cref="RoutingNode"/>s reconstructed from the transitions —
    /// one node per distinct machine id, carrying the OR-merged role from its out-transitions.
    /// </summary>
    [Fact]
    public void Create_ForLinearRoute_ShouldExposeTypedNodes()
    {
        // Act
        var graph = BuildGraph(LinearTransitions());

        // Assert — three real machine nodes (the wire-only 0 boundary is NOT a node here).
        graph.Nodes.Select(n => n.MachineId).ShouldBe([10, 20, 30], ignoreOrder: true);

        var initialNode = graph.Nodes.Single(n => n.MachineId == 10);
        initialNode.Role.Has(WorkFlowType.Initial).ShouldBeTrue();
        initialNode.Role.Has(WorkFlowType.Serial).ShouldBeTrue();
    }

    /// <summary>
    /// <see cref="ProductionGraph.LinearMachineSequence"/> returns the ordered machine-id sequence
    /// for a linear route — the degenerate single-path case equivalent to the legacy walk.
    /// </summary>
    [Fact]
    public void LinearMachineSequence_ForLinearRoute_ShouldReturnOrderedIds()
    {
        // Arrange
        var graph = BuildGraph(LinearTransitions());

        // Act
        var sequence = graph.LinearMachineSequence();

        // Assert
        sequence.IsSuccess.ShouldBeTrue();
        sequence.Value.ShouldNotBeNull().ShouldBe([10, 20, 30]);
    }

    /// <summary>
    /// <see cref="ProductionGraph.IsInitialMachine"/> and <see cref="ProductionGraph.IsFinalMachine"/>
    /// are derived from the <see cref="WorkFlowType.Initial"/> / <see cref="WorkFlowType.Final"/>
    /// roles on the node, NOT from any magic-0 machine id.
    /// </summary>
    [Fact]
    public void InitialAndFinal_ShouldBeDerivedFromWorkFlowTypeRoles()
    {
        // Arrange
        var graph = BuildGraph(LinearTransitions());

        // Act & Assert
        graph.IsInitialMachine(10).ShouldBeTrue();
        graph.IsInitialMachine(20).ShouldBeFalse();
        graph.IsInitialMachine(30).ShouldBeFalse();

        graph.IsFinalMachine(30).ShouldBeTrue();
        graph.IsFinalMachine(10).ShouldBeFalse();
        graph.IsFinalMachine(20).ShouldBeFalse();
    }

    /// <summary>
    /// <see cref="ProductionGraph.Predecessors"/> returns the predecessor machine-id set for a node.
    /// </summary>
    [Fact]
    public void Predecessors_ShouldReturnPredecessorIds()
    {
        // Arrange
        var graph = BuildGraph(LinearTransitions());

        // Act & Assert
        graph.Predecessors(20).Value.ShouldNotBeNull().ShouldBe([10]);
        graph.Predecessors(10).Value.ShouldNotBeNull().ShouldBeEmpty();
        graph.Predecessors(30).Value.ShouldNotBeNull().ShouldBe([20]);
    }

    /// <summary>
    /// The aggregate's node collection is exposed as a read-only projection (AD-1: no public mutable
    /// collections).
    /// </summary>
    [Fact]
    public void Nodes_ShouldBeReadOnly()
    {
        // Arrange
        var graph = BuildGraph(LinearTransitions());

        // Assert — the compile-time type is read-only; assert it is not a mutable List/array facade.
        graph.Nodes.ShouldBeAssignableTo<IReadOnlyCollection<RoutingNode>>();
    }
}
