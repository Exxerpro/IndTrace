// <copyright file="ProductionGraphNextMachineTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// #56 (56-A): pins the additive singular <see cref="ProductionGraph.NextMachine(int)"/> advance
/// accessor. It is byte-identical to the former <c>successors[0]</c> pick on LINEAR data (zero or one
/// successor), and FAILS LOUD on a multi-successor (Diverter/Parallel/Lateral) node rather than silently
/// choosing an arbitrary branch — the latent-correctness gate before any branching routing data is authored.
/// </summary>
public class ProductionGraphNextMachineTests
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

    // Linear graph: 10 (Initial|Serial) -> 20 (Serial) -> 30 (Final).
    private static ProductionGraph LinearGraph()
    {
        return BuildGraph(
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ]);
    }

    // Diverter split: 10 (Initial) -> 20 (Diverter) chooses one of {31, 32}; both Final.
    private static ProductionGraph DiverterGraph()
    {
        return BuildGraph(
        [
            new RoutingTransition(10, 20, WorkFlowType.Initial),
            new RoutingTransition(20, 31, WorkFlowType.Diverter),
            new RoutingTransition(20, 32, WorkFlowType.Diverter),
            new RoutingTransition(31, 0, WorkFlowType.Final),
            new RoutingTransition(32, 0, WorkFlowType.Final),
        ]);
    }

    [Fact]
    public void NextMachine_ForSingleSuccessor_ReturnsThatSuccessor()
    {
        var next = LinearGraph().NextMachine(10);

        next.IsSuccess.ShouldBeTrue();
        next.Value.ShouldBe(20);
    }

    [Fact]
    public void NextMachine_ForTerminalMachine_ReturnsEndOfLineZero()
    {
        var next = LinearGraph().NextMachine(30);

        next.IsSuccess.ShouldBeTrue();
        next.Value.ShouldBe(0); // (Final -> 0) boundary, byte-identical to the legacy terminal
    }

    [Fact]
    public void NextMachine_ForMultiSuccessorNode_FailsLoudInsteadOfPickingArbitraryBranch()
    {
        var next = DiverterGraph().NextMachine(20);

        // The former successors[0] silently returned 31; the guard now refuses to advance deterministically.
        next.IsSuccess.ShouldBeFalse();
        next.Errors.ShouldContain(e => e.Contains("outcome selection", StringComparison.Ordinal));
    }

    [Fact]
    public void NextMachine_ForUnknownMachine_ReturnsFailureNeverThrows()
    {
        var next = LinearGraph().NextMachine(999);

        next.IsSuccess.ShouldBeFalse();
    }
}
