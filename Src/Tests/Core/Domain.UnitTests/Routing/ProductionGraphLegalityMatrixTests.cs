// <copyright file="ProductionGraphLegalityMatrixTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// Story 2.3 — the complete <see cref="WorkFlowType"/> legality matrix as a ratified ALLOW-LIST. These
/// pure-domain tests assert the matrix row-for-row from a single exhaustive fixture and prove the factory
/// (<see cref="ProductionGraph.IsLegalRole(WorkFlowType)"/>) is the one normative authority — the same
/// authority Epic 5's authoring screen calls (AD-6, DECISIONS §10).
/// </summary>
/// <remarks>
/// <para>
/// The legality matrix was INVERTED from a deny-list of forbidden pairs to a ratified ALLOW-LIST: a node's
/// composite (OR-merged) role is legal iff its value is one of the 23 sanctioned values. The allow-list was
/// ratified by production-line-topology research, a bmad-party-mode panel, and product-owner sign-off
/// (DECISIONS §10, including the §10 amendment that admits <c>Serial|Parallel</c>(66) and
/// <c>Initial|Serial|Parallel</c>(67) — the load-balanced station that also does work).
/// </para>
/// <para>
/// The strongest "no unspecified combination" guarantee is the 1..127 sweep: EVERY value in that range is
/// asserted legal iff it is in the 23-value allow-list, so any drift in either direction fails.
/// </para>
/// </remarks>
public class ProductionGraphLegalityMatrixTests
{
    /// <summary>
    /// The 23 ratified sanctioned role values — the single source of truth the sweep cross-checks against.
    /// Atomic singles 1,2,4,8,16,32,64; sanctioned composites per DECISIONS §10 (incl. the §10 amendment
    /// admitting 66 and 67).
    /// </summary>
    private static readonly IReadOnlySet<int> SanctionedRoleValues = new HashSet<int>
    {
        1,  // Initial
        2,  // Serial
        4,  // Lateral
        8,  // Diverter
        16, // Merger
        32, // Final
        64, // Parallel
        3,  // Initial|Serial
        5,  // Lateral|Initial
        6,  // Lateral|Serial
        7,  // Lateral|Initial|Serial
        10, // Serial|Diverter
        11, // Initial|Serial|Diverter
        18, // Serial|Merger
        34, // Serial|Final
        35, // Initial|Serial|Final
        36, // Lateral|Final
        38, // Lateral|Serial|Final
        39, // Lateral|Initial|Serial|Final
        48, // Merger|Final
        50, // Serial|Merger|Final
        66, // Serial|Parallel (DECISIONS §10 amendment)
        67, // Initial|Serial|Parallel (DECISIONS §10 amendment, inherits 66)
    };

    /// <summary>
    /// One row of the legality matrix: a composite role and whether the factory must classify it as legal.
    /// </summary>
    /// <param name="Role">The composite <see cref="WorkFlowType"/> role under test.</param>
    /// <param name="ExpectedLegal"><see langword="true"/> when the role is a legal node role; otherwise <see langword="false"/>.</param>
    /// <param name="Label">A human-readable label naming the combination (for assertion diagnostics).</param>
    public sealed record MatrixRow(WorkFlowType Role, bool ExpectedLegal, string Label);

    /// <summary>
    /// The ratified legality matrix as an explicit fixture. Every one of the 23 sanctioned values appears as
    /// a LEGAL row, and a comprehensive set of representative ILLEGAL values appears (covering each governing
    /// principle P1–P4/P6, the PO-rejected zero-work entry-routers, and the generic fallback).
    /// </summary>
    public static TheoryData<MatrixRow> Matrix
    {
        get
        {
            var data = new TheoryData<MatrixRow>();

            // --- The 23 sanctioned values (LEGAL). Atomic singles. ---
            data.Add(Row(1, true, "Initial"));
            data.Add(Row(2, true, "Serial"));
            data.Add(Row(4, true, "Lateral"));
            data.Add(Row(8, true, "Diverter"));
            data.Add(Row(16, true, "Merger"));
            data.Add(Row(32, true, "Final"));
            data.Add(Row(64, true, "Parallel"));

            // Sanctioned composites.
            data.Add(Row(3, true, "Initial|Serial"));
            data.Add(Row(5, true, "Lateral|Initial"));
            data.Add(Row(6, true, "Lateral|Serial"));
            data.Add(Row(7, true, "Lateral|Initial|Serial"));
            data.Add(Row(10, true, "Serial|Diverter"));
            data.Add(Row(11, true, "Initial|Serial|Diverter"));
            data.Add(Row(18, true, "Serial|Merger"));
            data.Add(Row(34, true, "Serial|Final"));
            data.Add(Row(35, true, "Initial|Serial|Final"));
            data.Add(Row(36, true, "Lateral|Final"));
            data.Add(Row(38, true, "Lateral|Serial|Final"));
            data.Add(Row(39, true, "Lateral|Initial|Serial|Final"));
            data.Add(Row(48, true, "Merger|Final"));
            data.Add(Row(50, true, "Serial|Merger|Final"));
            data.Add(Row(66, true, "Serial|Parallel"));
            data.Add(Row(67, true, "Initial|Serial|Parallel"));

            // --- Representative ILLEGAL values (each must FAIL). ---
            // P1 — Diverter & Parallel both set.
            data.Add(Row(72, false, "Diverter|Parallel (P1)"));

            // P2 — Merger & (Diverter | Parallel).
            data.Add(Row(24, false, "Merger|Diverter (P2)"));
            data.Add(Row(80, false, "Merger|Parallel (P2)"));

            // P3a — Initial & Merger.
            data.Add(Row(17, false, "Initial|Merger (P3a)"));

            // P3b — Final & (Diverter | Parallel).
            data.Add(Row(40, false, "Final|Diverter (P3b)"));
            data.Add(Row(96, false, "Final|Parallel (P3b)"));

            // P4 — Lateral & (Diverter | Parallel | Merger).
            data.Add(Row(20, false, "Merger|Lateral (P4)"));
            data.Add(Row(12, false, "Lateral|Diverter (P4)"));
            data.Add(Row(68, false, "Lateral|Parallel (P4)"));

            // P6 — Initial & Final without Serial.
            data.Add(Row(33, false, "Initial|Final without Serial (P6)"));

            // PO-rejected zero-work entry-routers (generic fallback — not a P-pair).
            data.Add(Row(9, false, "Initial|Diverter (PO-rejected)"));
            data.Add(Row(65, false, "Initial|Parallel (PO-rejected)"));

            return data;
        }
    }

    /// <summary>
    /// Row-for-row matrix assertion: the single factory authority classifies every fixture row exactly
    /// as the ratified allow-list prescribes.
    /// </summary>
    /// <param name="row">The matrix row under test.</param>
    [Theory]
    [MemberData(nameof(Matrix))]
    public void IsLegalRole_ClassifiesEachMatrixRow_AsRatified(MatrixRow row)
    {
        ProductionGraph.IsLegalRole(row.Role).ShouldBe(row.ExpectedLegal, row.Label);
    }

    /// <summary>
    /// The strongest "no unspecified combination" guarantee: EVERY value in 1..127 is legal iff it is in the
    /// ratified 23-value allow-list. This proves the allow-list is the authority and nothing outside it leaks
    /// through — and that exactly 23 of the 127 values are legal.
    /// </summary>
    [Fact]
    public void IsLegalRole_OverFullOneToOneHundredTwentySevenSpace_MatchesAllowListExactly()
    {
        var legalCount = 0;

        for (var value = 1; value <= 127; value++)
        {
            var expectedLegal = SanctionedRoleValues.Contains(value);
            ProductionGraph.IsLegalRole(WorkFlowType.From(value))
                .ShouldBe(expectedLegal, $"value {value} (role {WorkFlowType.From(value).Name})");

            if (expectedLegal)
            {
                legalCount++;
            }
        }

        legalCount.ShouldBe(23);
    }

    /// <summary>
    /// The allow-list is internally consistent with the governing principles: every sanctioned value carries
    /// none of the forbidden flag pairs (P1–P4) and is not an Initial|Final-without-Serial (P6). This guards
    /// against a future edit that admits a value contradicting the principles.
    /// </summary>
    [Fact]
    public void SanctionedValues_AreInternallyConsistentWithGoverningPrinciples()
    {
        foreach (var value in SanctionedRoleValues)
        {
            var role = WorkFlowType.From(value);

            var hasDiverter = role.Has(WorkFlowType.Diverter);
            var hasParallel = role.Has(WorkFlowType.Parallel);
            var hasMerger = role.Has(WorkFlowType.Merger);
            var hasLateral = role.Has(WorkFlowType.Lateral);
            var hasInitial = role.Has(WorkFlowType.Initial);
            var hasFinal = role.Has(WorkFlowType.Final);
            var hasSerial = role.Has(WorkFlowType.Serial);

            (hasDiverter && hasParallel).ShouldBeFalse($"value {value} violates P1");
            (hasMerger && (hasDiverter || hasParallel)).ShouldBeFalse($"value {value} violates P2");
            (hasInitial && hasMerger).ShouldBeFalse($"value {value} violates P3a");
            (hasFinal && (hasDiverter || hasParallel)).ShouldBeFalse($"value {value} violates P3b");
            (hasLateral && (hasDiverter || hasParallel || hasMerger)).ShouldBeFalse($"value {value} violates P4");
            (hasInitial && hasFinal && !hasSerial).ShouldBeFalse($"value {value} violates P6");
        }
    }

    /// <summary>
    /// The specific diagnostic for an illegal combination matches the governing principle that explains it,
    /// so the Epic-5 authoring screen surfaces a helpful, principle-specific reason.
    /// </summary>
    /// <param name="value">The illegal composite role value under test.</param>
    /// <param name="expectedFragment">A fragment that must appear in the reason (the principle's hallmark).</param>
    [Theory]
    [InlineData(72, "Diverter|Parallel")]
    [InlineData(24, "Merger")]
    [InlineData(80, "Merger")]
    [InlineData(17, "Initial")]
    [InlineData(40, "Final")]
    [InlineData(96, "Final")]
    [InlineData(20, "Lateral")]
    [InlineData(12, "Lateral")]
    [InlineData(68, "Lateral")]
    [InlineData(33, "Initial|Final")]
    public void IsLegalRole_ForIllegalValue_IsRejectedWithPrincipleSpecificReason(int value, string expectedFragment)
    {
        var role = WorkFlowType.From(value);

        ProductionGraph.IsLegalRole(role).ShouldBeFalse($"value {value}");

        // The factory renders the reason through Validate; assert the principle hallmark surfaces.
        var reason = ProductionGraph.IllegalRoleReason(role);
        reason.ShouldNotBeNull($"value {value} must carry a reason");
        reason!.Contains(expectedFragment, StringComparison.OrdinalIgnoreCase)
            .ShouldBeTrue($"value {value} reason '{reason}' should mention '{expectedFragment}'");
    }

    /// <summary>
    /// A legal co-occurrence passes through <see cref="ProductionGraph.Validate"/> and
    /// <see cref="ProductionGraph.Create"/> with no illegal-combination violation: the first working
    /// station <c>Initial|Serial</c> on a well-formed line.
    /// </summary>
    [Fact]
    public void Validate_ForLegalInitialSerial_RaisesNoIllegalCombinationViolation()
    {
        // Arrange — Initial|Serial(10) -> Serial|Final(20).
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 0, WorkFlowType.From(WorkFlowType.Serial | WorkFlowType.Final)),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — no illegal-combination violation, and Create succeeds.
        violations.ShouldNotContain(v => v.Reason.Contains("combination", StringComparison.OrdinalIgnoreCase));
        ProductionGraph.Create(transitions).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// The single-station sanctioned case <c>Initial|Final|Serial</c> passes through the factory with no
    /// illegal-combination violation.
    /// </summary>
    [Fact]
    public void Validate_ForSingleStationInitialFinalSerial_IsLegal()
    {
        // Arrange — one row that is both first and last and does work.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(
                10,
                0,
                WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Final | WorkFlowType.Serial)),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert
        violations.ShouldBeEmpty();
        ProductionGraph.Create(transitions).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// An illegal node role (<c>Diverter|Parallel</c>) surfaces as a structured
    /// <see cref="RoutingViolation"/> naming the offending node via <see cref="ProductionGraph.Validate"/>,
    /// and is rendered into the <see cref="ProductionGraph.Create"/> failure.
    /// </summary>
    [Fact]
    public void Validate_ForDiverterParallelNode_NamesThatNodeAsIllegalCombination()
    {
        // Arrange — node 10 carries the illegal Diverter|Parallel composite (fan-out to 20 and 30).
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Diverter)),
            new RoutingTransition(10, 30, WorkFlowType.Parallel),
            new RoutingTransition(20, 0, WorkFlowType.Final),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — node 10 is named with an illegal-combination violation.
        violations.ShouldContain(v =>
            v.NodeId == 10 && v.Reason.Contains("combination", StringComparison.OrdinalIgnoreCase));

        var result = ProductionGraph.Create(transitions);
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("combination", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// An illegal <c>Diverter|Merger</c> composite on one node names that node as an illegal-combination
    /// violation (P2: a split and a join are distinct stations).
    /// </summary>
    [Fact]
    public void Validate_ForDiverterMergerNode_NamesThatNodeAsIllegalCombination()
    {
        // Arrange — node 20 merges (from 10 and 15) AND diverts out (to 30 and 40): Diverter|Merger.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(15, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.From(WorkFlowType.Merger | WorkFlowType.Diverter)),
            new RoutingTransition(20, 40, WorkFlowType.From(WorkFlowType.Merger | WorkFlowType.Diverter)),
            new RoutingTransition(30, 0, WorkFlowType.Final),
            new RoutingTransition(40, 0, WorkFlowType.Final),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert
        violations.ShouldContain(v =>
            v.NodeId == 20 && v.Reason.Contains("combination", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// An illegal <c>Diverter|Lateral</c> composite on one node names that node as an illegal-combination
    /// violation (P4: Lateral marks feeder-membership, not a branch/join).
    /// </summary>
    [Fact]
    public void Validate_ForDiverterLateralNode_NamesThatNodeAsIllegalCombination()
    {
        // Arrange — node 40 is both a feeder marker and a conditional split: Diverter|Lateral.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 0, WorkFlowType.From(WorkFlowType.Merger | WorkFlowType.Final)),
            new RoutingTransition(40, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Diverter | WorkFlowType.Lateral)),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert
        violations.ShouldContain(v =>
            v.NodeId == 40 && v.Reason.Contains("combination", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// An illegal-combination violation is COLLECTED, not a fundamental short-circuit: a graph with a
    /// legal Initial but an illegal node role returns that violation alongside any other structural
    /// violations (the policy from Story 2.2 is intact).
    /// </summary>
    [Fact]
    public void Validate_ForIllegalCombination_IsCollectedNotShortCircuited()
    {
        // Arrange — valid Initial(10), but node 20 carries the illegal Initial|Merger(17) and node 50 is a
        // reachable dead-end. Both must be reported together.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Merger)),
            new RoutingTransition(30, 0, WorkFlowType.Final),
            new RoutingTransition(10, 50, WorkFlowType.Initial),
            new RoutingTransition(50, 0, WorkFlowType.Serial),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert — both the illegal-combination (node 20) and the dead-end (node 50) are present.
        violations.ShouldContain(v =>
            v.NodeId == 20 && v.Reason.Contains("combination", StringComparison.OrdinalIgnoreCase));
        violations.ShouldContain(v =>
            v.NodeId == 50 && v.Reason.Contains("dead", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// P6 — <c>Initial|Final</c> WITHOUT <see cref="WorkFlowType.Serial"/> is illegal: a single station
    /// that is both the first and last machine must do work, so the boundary pair REQUIRES Serial.
    /// </summary>
    [Fact]
    public void IsLegalRole_ForInitialFinalWithoutSerial_IsIllegal()
    {
        var role = WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Final);

        ProductionGraph.IsLegalRole(role).ShouldBeFalse();
    }

    /// <summary>
    /// <c>Initial|Final|Serial</c> (the single station that does work) is sanctioned and stays legal.
    /// </summary>
    [Fact]
    public void IsLegalRole_ForInitialFinalSerial_IsLegal()
    {
        var role = WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Final | WorkFlowType.Serial);

        ProductionGraph.IsLegalRole(role).ShouldBeTrue();
    }

    /// <summary>
    /// A node carrying the illegal <c>Initial|Final</c> boundary (no Serial) surfaces as a structured
    /// illegal-combination violation naming that node.
    /// </summary>
    [Fact]
    public void Validate_ForInitialFinalWithoutSerialNode_NamesThatNodeAsIllegalCombination()
    {
        // Arrange — single node 10 is both first and last but does no work.
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 0, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Final)),
        ];

        // Act
        var violations = ProductionGraph.Validate(transitions);

        // Assert
        violations.ShouldContain(v =>
            v.NodeId == 10 && v.Reason.Contains("combination", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The PO-rejected zero-work entry-router <c>Initial|Parallel</c>(65) is illegal: an Initial station that
    /// only load-balances out (does no Serial work) was rejected by the product owner.
    /// </summary>
    [Fact]
    public void IsLegalRole_ForInitialParallelEntryRouter_IsIllegal()
    {
        ProductionGraph.IsLegalRole(WorkFlowType.From(65)).ShouldBeFalse();
    }

    /// <summary>
    /// The §10-amendment <c>Serial|Parallel</c>(66) — a load-balanced station that also does work — is legal.
    /// </summary>
    [Fact]
    public void IsLegalRole_ForSerialParallelWorkingStation_IsLegal()
    {
        ProductionGraph.IsLegalRole(WorkFlowType.From(66)).ShouldBeTrue();
    }

    /// <summary>
    /// <see cref="ProductionGraph.IsLegalRole(WorkFlowType)"/> is the public authority Epic 5's authoring
    /// screen calls; passing a null role must NOT throw (the aggregate never throws) and a null role is not
    /// a legal role, so it returns <see langword="false"/>.
    /// </summary>
    [Fact]
    public void IsLegalRole_ForNullRole_ReturnsFalseWithoutThrowing()
    {
        Should.NotThrow(() => ProductionGraph.IsLegalRole(null!));

        ProductionGraph.IsLegalRole(null!).ShouldBeFalse();
    }

    /// <summary>
    /// <c>None</c>(0) is preserved as NON-illegal at the matrix level: a pure-sink node's OR-merged role is
    /// <c>None</c>, and its well-formedness is covered by the structural dead-end rule, NOT the matrix. So
    /// <see cref="ProductionGraph.IllegalRoleReason(WorkFlowType)"/> returns null for None — even though None
    /// is not in the allow-list, it must not be reported as an illegal-combination violation.
    /// </summary>
    [Fact]
    public void IllegalRoleReason_ForNone_IsNullToPreserveSinkScope()
    {
        ProductionGraph.IllegalRoleReason(WorkFlowType.None).ShouldBeNull();
    }

    private static MatrixRow Row(int value, bool expectedLegal, string label) =>
        new(WorkFlowType.From(value), expectedLegal, label);
}
