// <copyright file="ProductionGraphNextMachinesConsumerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// Consumer-shaped tests (panel directive, Story 2.1) that exercise
/// <see cref="ProductionGraph.NextMachines(int)"/> exactly the way <c>BarCodeResult</c> will in
/// Epic 4. Today <c>BarCodeResult</c> resolves the next machine as a SINGLE int
/// (<c>vmFt[lastMachineId].NextMachineId</c>); these tests prove the projection's set shape is
/// consumer-correct for BOTH the single-successor case (so the consumer can <c>.Single()</c> it,
/// as it does today) and the new multi-successor fan-out — before Epic 4 migrates the consumer.
/// </summary>
public class ProductionGraphNextMachinesConsumerTests
{
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
    /// Single-successor: a linear <see cref="WorkFlowType.Serial"/> route. The consumer reads the
    /// one next machine the way it does today — by taking the single element of the successor set.
    /// </summary>
    [Fact]
    public void NextMachines_ForSerialRoute_ReturnsExactlyOneSuccessor()
    {
        // Arrange — Initial(10) -> Serial(20) -> Final(30).
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ];
        var graph = BuildGraph(transitions);

        // Act
        var successors = graph.NextMachines(10);

        // Assert — the consumer-shaped read: exactly one, take it via Single() as BarCodeResult will.
        successors.IsSuccess.ShouldBeTrue();
        var ids = successors.Value.ShouldNotBeNull();
        ids.Count.ShouldBe(1);
        ids.Single().ShouldBe(20);
    }

    /// <summary>
    /// Multi-successor: a <see cref="WorkFlowType.Parallel"/> throughput fan-out. The same projection
    /// returns the multiple successor ids — the shape NEW to the system that single-int consumers
    /// could never represent.
    /// </summary>
    [Fact]
    public void NextMachines_ForParallelFanOut_ReturnsAllSuccessors()
    {
        // Arrange — the sanctioned Initial|Serial|Parallel(67) entry station (§10 amendment: a working
        // station that also load-balances) splits across two parallel lanes 21 and 22, re-merged at 30.
        // (The bare Initial|Parallel(65) zero-work entry-router is PO-rejected by the ratified allow-list.)
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 21, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial | WorkFlowType.Parallel)),
            new RoutingTransition(10, 22, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial | WorkFlowType.Parallel)),
            new RoutingTransition(21, 30, WorkFlowType.Serial),
            new RoutingTransition(22, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.From(WorkFlowType.Merger | WorkFlowType.Final)),
        ];
        var graph = BuildGraph(transitions);

        // Act
        var successors = graph.NextMachines(10);

        // Assert — both lanes are returned; the consumer must NOT collapse this to one.
        successors.IsSuccess.ShouldBeTrue();
        successors.Value.ShouldNotBeNull().ShouldBe([21, 22], ignoreOrder: true);
    }

    /// <summary>
    /// Multi-successor: a <see cref="WorkFlowType.Diverter"/> conditional split. For Story 2.1 the
    /// projection returns ALL topological successors (Diverter branch-selection by outcome is Epic 6);
    /// the set shape is identical to the Parallel fan-out, proving the signature does not need
    /// reshaping when Epic 4/6 arrive.
    /// </summary>
    [Fact]
    public void NextMachines_ForDiverterSplit_ReturnsAllTopologicalSuccessors()
    {
        // Arrange — Initial(10) -> Diverter(20) chooses one of {31, 32} at runtime; both Final.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.Initial),
            new RoutingTransition(20, 31, WorkFlowType.Diverter),
            new RoutingTransition(20, 32, WorkFlowType.Diverter),
            new RoutingTransition(31, 0, WorkFlowType.Final),
            new RoutingTransition(32, 0, WorkFlowType.Final),
        ];
        var graph = BuildGraph(transitions);

        // Act
        var successors = graph.NextMachines(20);

        // Assert — both branches are topological successors for this story.
        successors.IsSuccess.ShouldBeTrue();
        successors.Value.ShouldNotBeNull().ShouldBe([31, 32], ignoreOrder: true);
    }

    /// <summary>
    /// A final machine has no successors — the projection returns an empty (successful) set, mirroring
    /// the legacy <c>NextMachineId == 0</c> end-of-line at the aggregate level (the magic-0 lives only
    /// at the PLC boundary, not here).
    /// </summary>
    [Fact]
    public void NextMachines_ForFinalMachine_ReturnsEmptySet()
    {
        // Arrange
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.Initial),
            new RoutingTransition(20, 0, WorkFlowType.Final),
        ];
        var graph = BuildGraph(transitions);

        // Act
        var successors = graph.NextMachines(20);

        // Assert
        successors.IsSuccess.ShouldBeTrue();
        successors.Value.ShouldNotBeNull().ShouldBeEmpty();
    }

    /// <summary>
    /// Querying successors of a machine id not present in the graph fails (it is not a node).
    /// </summary>
    [Fact]
    public void NextMachines_ForUnknownMachine_ShouldFail()
    {
        // Arrange
        var graph = BuildGraph(
        [
            new RoutingTransition(10, 20, WorkFlowType.Initial),
            new RoutingTransition(20, 0, WorkFlowType.Final),
        ]);

        // Act
        var successors = graph.NextMachines(999);

        // Assert
        successors.IsFailure.ShouldBeTrue();
    }
}
