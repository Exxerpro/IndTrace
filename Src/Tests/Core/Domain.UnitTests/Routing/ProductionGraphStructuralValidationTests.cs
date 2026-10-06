// <copyright file="ProductionGraphStructuralValidationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// Story 2.2 — structural well-formedness validation. These pure-domain tests prove the five
/// structural invariants (≥1 <see cref="WorkFlowType.Initial"/>, ≥1 <see cref="WorkFlowType.Final"/>,
/// every node reachable from an Initial, every node reaches a Final, every
/// <see cref="WorkFlowType.Lateral"/> terminates at a <see cref="WorkFlowType.Merger"/>) and the
/// collect-all / fundamental-short-circuit policy (AD-6, DECISIONS §7). They also prove that the
/// offending node id is carried structurally for Epic 5 via
/// <see cref="ProductionGraph.Validate(System.Collections.Generic.IReadOnlyCollection{RoutingTransition})"/>.
/// </summary>
public class ProductionGraphStructuralValidationTests
{
    /// <summary>
    /// A well-formed linear route passes structural validation with no violations.
    /// </summary>
    [Fact]
    public void Validate_ForLinearRoute_ReturnsNoViolations()
    {
        // Arrange — Initial(10) -> Serial(20) -> Final(30).
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert
        violations.ShouldBeEmpty();
        ProductionGraph.Create(transitions).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// A routing with no <see cref="WorkFlowType.Final"/> node fails with a single graph-level
    /// violation (no specific node, so <see cref="RoutingViolation.NodeId"/> is <see langword="null"/>).
    /// </summary>
    [Fact]
    public void Validate_ForNoFinal_ReportsGraphLevelViolationWithNullNode()
    {
        // Arrange — Initial(10) -> Serial(20) -> (20 has no Final, but is not a dead-end loop).
        // 20 -> 30 where 30 carries no Final role; 30 is then a dead-end too, but the "no Final"
        // graph-level violation must appear with a null node id.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Serial),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — at least the no-Final graph-level violation with a null node id.
        violations.ShouldContain(v => v.NodeId == null && v.Reason.Contains("Final", StringComparison.OrdinalIgnoreCase));
        ProductionGraph.Create(transitions).IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// An unreachable node (not reachable by forward traversal from any Initial) produces a
    /// violation naming that node id.
    /// </summary>
    [Fact]
    public void Validate_ForUnreachableNode_NamesThatNode()
    {
        // Arrange — Initial(10) -> Final(20); island 99 -> Final(20) is unreachable from any Initial.
        // 99 reaches a Final (via 20) so it is not a dead-end; its only defect is unreachability.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 0, WorkFlowType.Final),
            new RoutingTransition(99, 20, WorkFlowType.Serial),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert
        violations.ShouldContain(v => v.NodeId == 99 && v.Reason.Contains("reachable", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// A dead-end node (one from which no Final is reachable by forward traversal) produces a
    /// violation naming that node id.
    /// </summary>
    [Fact]
    public void Validate_ForDeadEndNode_NamesThatNode()
    {
        // Arrange — Initial(10) -> Final(20); 10 also fans out to 50 which has no successor and is
        // not a Final, so 50 is a reachable dead-end.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(10, 50, WorkFlowType.Initial),
            new RoutingTransition(20, 0, WorkFlowType.Final),
            new RoutingTransition(50, 0, WorkFlowType.Serial),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert
        violations.ShouldContain(v => v.NodeId == 50 && v.Reason.Contains("dead", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// A <see cref="WorkFlowType.Lateral"/> node whose forward path never reaches a
    /// <see cref="WorkFlowType.Merger"/> produces a violation naming that node id.
    /// </summary>
    [Fact]
    public void Validate_ForLateralNotEndingAtMerger_NamesThatNode()
    {
        // Arrange — main line Initial(10) -> Merger(20) -> Final(30). Feeder 40 is Lateral and joins
        // at 50 (a plain Serial that reaches Final), so 40's forward path never hits a Merger.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.From(WorkFlowType.Merger | WorkFlowType.Serial)),
            new RoutingTransition(30, 0, WorkFlowType.Final),
            new RoutingTransition(40, 50, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Lateral)),
            new RoutingTransition(50, 30, WorkFlowType.Serial),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert
        violations.ShouldContain(v => v.NodeId == 40 && v.Reason.Contains("Merger", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// A valid <see cref="WorkFlowType.Lateral"/> feeder that DOES reach a <see cref="WorkFlowType.Merger"/>
    /// raises no Lateral-termination violation.
    /// </summary>
    [Fact]
    public void Validate_ForLateralEndingAtMerger_RaisesNoLateralViolation()
    {
        // Arrange — main line Initial(10) -> Merger(20) -> Final(30); feeder Lateral(40) -> Merger(20).
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.From(WorkFlowType.Merger | WorkFlowType.Serial)),
            new RoutingTransition(30, 0, WorkFlowType.Final),
            new RoutingTransition(40, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Lateral)),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — no Lateral-termination violation; the graph is well-formed.
        violations.ShouldBeEmpty();
        ProductionGraph.Create(transitions).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// COLLECT-ALL: a fixture with multiple independent violations returns them ALL together, each
    /// naming its offending node (DECISIONS §7).
    /// </summary>
    [Fact]
    public void Validate_ForMultipleIndependentViolations_ReturnsThemAll()
    {
        // Arrange — one graph carrying three independent defects:
        //  - unreachable node 99 (joins the line at 20 but no Initial reaches it),
        //  - dead-end node 50 (reachable from Initial(10) but reaches no Final),
        //  - Lateral node 40 whose forward path (40 -> 60 -> Final) never hits a Merger.
        // Main line: Initial(10) -> 20 -> Final(30); plus the three defects.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Final),

            // Unreachable island that still reaches a Final (so its only defect is unreachability).
            new RoutingTransition(99, 20, WorkFlowType.Serial),

            // Reachable dead-end: 10 fans out to 50, which goes nowhere Final.
            new RoutingTransition(10, 50, WorkFlowType.Initial),
            new RoutingTransition(50, 0, WorkFlowType.Serial),

            // Lateral feeder 40 -> 60 -> Final(30): reachable & reaches Final, but no Merger on path.
            new RoutingTransition(40, 60, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Lateral)),
            new RoutingTransition(60, 30, WorkFlowType.Serial),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — all three independent, node-named violations are present together.
        violations.ShouldContain(v => v.NodeId == 99 && v.Reason.Contains("reachable", StringComparison.OrdinalIgnoreCase));
        violations.ShouldContain(v => v.NodeId == 50 && v.Reason.Contains("dead", StringComparison.OrdinalIgnoreCase));
        violations.ShouldContain(v => v.NodeId == 40 && v.Reason.Contains("Merger", StringComparison.OrdinalIgnoreCase));

        // And Create fails carrying all of them rendered as messages.
        var result = ProductionGraph.Create(transitions);
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// FUNDAMENTAL SHORT-CIRCUIT: a graph with no <see cref="WorkFlowType.Initial"/> node reports ONLY
    /// the single blocking violation, not the downstream reachability/dead-end noise (which would be
    /// meaningless without an Initial to traverse from).
    /// </summary>
    [Fact]
    public void Validate_ForNoInitial_ReturnsOnlyTheBlocker()
    {
        // Arrange — a connected line that lacks any Initial role: 10 -> 20 -> Final(30).
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.Serial),
            new RoutingTransition(20, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — exactly one violation: the no-Initial blocker (graph-level, null node id).
        violations.Count.ShouldBe(1);
        violations[0].NodeId.ShouldBeNull();
        violations[0].Reason.Contains("Initial", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();

        var result = ProductionGraph.Create(transitions);
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBe(1);
    }

    /// <summary>
    /// FUNDAMENTAL SHORT-CIRCUIT: an empty graph reports only the single blocking violation.
    /// </summary>
    [Fact]
    public void Validate_ForEmptyGraph_ReturnsOnlyTheBlocker()
    {
        // Act
        var violations = ProductionGraph.Validate([]);

        // Assert
        violations.Count.ShouldBe(1);
        violations[0].Reason.Contains("empty", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
    }

    /// <summary>
    /// FIX C — a transition whose <see cref="RoutingTransition.FromMachineId"/> equals its
    /// <see cref="RoutingTransition.ToMachineId"/> routes a part back to itself: a malformed routing.
    /// It must produce a violation naming that node.
    /// </summary>
    [Fact]
    public void Validate_ForSelfLoopTransition_NamesThatNode()
    {
        // Arrange — Initial(10) -> 20; node 20 has a self-loop (20 -> 20) and then 20 -> Final(30).
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 20, WorkFlowType.Serial),
            new RoutingTransition(20, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — node 20 is named with a self-loop violation.
        violations.ShouldContain(v =>
            v.NodeId == 20 && v.Reason.Contains("self", StringComparison.OrdinalIgnoreCase));
        ProductionGraph.Create(transitions).IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// FIX E (1/2) — pins the AD-15 Final convention: a Final authored on a terminal
    /// <c>(id, 0, Final)</c> row makes <see cref="ProductionGraph.IsFinalMachine(int)"/> true for that
    /// node and raises NO dead-end violation (0 is the wire boundary, not a node).
    /// </summary>
    [Fact]
    public void Validate_ForFinalAuthoredAsTerminalZeroRow_IsFinalAndNotDeadEnd()
    {
        // Arrange — Initial(10) -> 30; 30 is Final via the terminal (30, 0, Final) row.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 30, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);
        var result = ProductionGraph.Create(transitions);

        // Assert — well-formed; 30 is Final; no dead-end on 30.
        violations.ShouldBeEmpty();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().IsFinalMachine(30).ShouldBeTrue();
    }

    /// <summary>
    /// FIX E (2/2) — pins the corollary: a pure-sink machine that appears ONLY as a
    /// <see cref="RoutingTransition.ToMachineId"/>, with no <c>(id, 0, Final)</c> row, gets role
    /// <see cref="WorkFlowType.None"/>, is NOT Final, and is (correctly, by design) flagged a dead-end.
    /// </summary>
    [Fact]
    public void Validate_ForPureSinkWithoutTerminalFinalRow_IsFlaggedDeadEnd()
    {
        // Arrange — Initial(10) -> 40; 40 appears ONLY as a To-id (no (40, 0, Final) row authored).
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 40, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial | WorkFlowType.Final)),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — 40 is not Final and is flagged a dead-end (no Final reachable from it).
        violations.ShouldContain(v =>
            v.NodeId == 40 && v.Reason.Contains("dead", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Issue #115 finding 8 — ACYCLICITY: a multi-node cycle inside a forked routing (which the
    /// linear-only <see cref="ProductionGraph.LinearMachineSequence"/> gate never sees, because the fork
    /// authoring path skips it for out-degree &gt; 1) must be a structural violation naming the
    /// cycle-closing node, and <see cref="ProductionGraph.Create"/> must fail. Cycles are ILLEGAL in fork
    /// routes exactly as in linear ones — the system-wide invariant is acyclic routing; a legal rework
    /// loop would be a future, PO-gated feature.
    /// </summary>
    [Fact]
    public void Validate_ForForkWithCycle_NamesTheCycleClosingNode()
    {
        // Arrange — fork Initial|Serial|Diverter(10) -> {20, 30}; 20 -> 10 closes the 10 -> 20 -> 10
        // cycle; Final(30). Every OTHER structural rule passes: all nodes reachable from the Initial,
        // no dead-end (20 reaches Final 30 via 10), roles sanctioned, no self-loop.
        var forkRole = WorkFlowType.From(
            WorkFlowType.Initial.Value | WorkFlowType.Serial.Value | WorkFlowType.Diverter.Value);
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, forkRole),
            new RoutingTransition(10, 30, forkRole),
            new RoutingTransition(20, 10, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — the cycle-closing edge (20 -> 10) is reported against node 20, and creation fails.
        violations.ShouldContain(v => v.NodeId == 20 && v.Reason.Contains("cycle", StringComparison.OrdinalIgnoreCase));
        ProductionGraph.Create(transitions).IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// A node that is BOTH unreachable AND a dead-end yields two distinct violations (one per
    /// orthogonal invariant) — the resolution chosen for this story.
    /// </summary>
    [Fact]
    public void Validate_ForNodeBothUnreachableAndDeadEnd_ReportsBothViolations()
    {
        // Arrange — main line Initial(10) -> Final(20). Island node 77 -> 78 (78 not Final, no successor);
        // 77 is unreachable from any Initial AND reaches no Final.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 0, WorkFlowType.Final),
            new RoutingTransition(77, 78, WorkFlowType.Serial),
            new RoutingTransition(78, 0, WorkFlowType.Serial),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — 77 appears as both an unreachable violation and a dead-end violation.
        violations.ShouldContain(v => v.NodeId == 77 && v.Reason.Contains("reachable", StringComparison.OrdinalIgnoreCase));
        violations.ShouldContain(v => v.NodeId == 77 && v.Reason.Contains("dead", StringComparison.OrdinalIgnoreCase));
    }
}
