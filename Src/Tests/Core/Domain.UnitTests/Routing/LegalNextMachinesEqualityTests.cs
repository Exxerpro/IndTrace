// <copyright file="LegalNextMachinesEqualityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// #126 F5 regression — <see cref="LegalNextMachines"/> wraps an <c>IReadOnlyList</c>, so the
/// compiler-synthesized <c>record struct</c> equality compared the list by REFERENCE: two instances
/// with identical contents were unequal, and <see cref="ProductRoutingState"/> (a record holding one)
/// inherited the broken equality. These tests pin value equality by CONTENTS, order-insensitive,
/// because the type is a legal-successor <b>set</b> (consumed only through membership —
/// <see cref="LegalNextMachines.Contains"/> / <see cref="LegalNextMachines.Permits"/>), so the
/// traversal order the constructor happened to see is an implementation detail, never domain meaning.
/// </summary>
public class LegalNextMachinesEqualityTests
{
    private static IReadOnlyList<MachineId> Ids(params int[] values) =>
        values.Select(v => new MachineId(v)).ToList().AsReadOnly();

    /// <summary>
    /// Same contents in the same order but DISTINCT list instances must be equal — with equal hash
    /// codes and agreeing <c>==</c> / <c>!=</c> operators. This is the exact defect: the synthesized
    /// equality compared the wrapped list by reference and returned false here.
    /// </summary>
    [Fact]
    public void Equals_SameContentsDistinctListInstances_AreEqualWithEqualHashCodes()
    {
        // Arrange: two separately allocated lists with identical contents.
        var left = new LegalNextMachines(Ids(10, 20, 30));
        var right = new LegalNextMachines(Ids(10, 20, 30));

        // Act, Assert
        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    /// <summary>
    /// The legal-successor SET is order-insensitive: the same members seen in a different traversal
    /// order are the same set (equal, with equal hash codes).
    /// </summary>
    [Fact]
    public void Equals_SameContentsDifferentOrder_AreEqualWithEqualHashCodes()
    {
        // Arrange
        var left = new LegalNextMachines(Ids(31, 32));
        var right = new LegalNextMachines(Ids(32, 31));

        // Act, Assert
        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    /// <summary>
    /// Different contents — a differing member, a differing count, or differing duplicate
    /// multiplicity — must be UNEQUAL.
    /// </summary>
    [Fact]
    public void Equals_DifferentContents_AreNotEqual()
    {
        // Arrange, Act, Assert: differing member.
        new LegalNextMachines(Ids(10, 20)).Equals(new LegalNextMachines(Ids(10, 30))).ShouldBeFalse();

        // Differing count (subset is not equality).
        new LegalNextMachines(Ids(10, 20)).Equals(new LegalNextMachines(Ids(10))).ShouldBeFalse();
        (new LegalNextMachines(Ids(10, 20)) != new LegalNextMachines(Ids(10))).ShouldBeTrue();

        // Differing multiplicity at equal count (multiset compare, no false positive).
        new LegalNextMachines(Ids(1, 1, 2)).Equals(new LegalNextMachines(Ids(1, 2, 2))).ShouldBeFalse();
    }

    /// <summary>
    /// A <c>default</c> value (null wrapped list) and an instance wrapping an EMPTY list both mean
    /// "no legal successor" — every member already treats them identically (<c>Count == 0</c>,
    /// <c>Contains == false</c>), so equality must agree.
    /// </summary>
    [Fact]
    public void Equals_DefaultAndEmptyList_AreEqualWithEqualHashCodes()
    {
        // Arrange
        var defaultValue = default(LegalNextMachines);
        var empty = new LegalNextMachines(Ids());

        // Act, Assert
        defaultValue.Equals(empty).ShouldBeTrue();
        (defaultValue == empty).ShouldBeTrue();
        defaultValue.GetHashCode().ShouldBe(empty.GetHashCode());
    }

    /// <summary>
    /// #126 F5 (record-level) — two <see cref="ProductRoutingState"/> instances computed from two
    /// independently created (but identical) graphs differ ONLY by distinct-but-equal-content legal-set
    /// lists; the states must compare equal. Under the synthesized reference equality they did not.
    /// </summary>
    [Fact]
    public void ProductRoutingState_DifferingOnlyByDistinctEqualContentLists_AreEqual()
    {
        // Arrange: the same linear topology built twice — FromGraph allocates a fresh internal list each time.
        var first = ProductRoutingState.FromGraph(
            LinearGraph(), new LastProcessedMachine(new MachineId(20)), CycleStatus.FinishedOk, FlowStatus.InProcess);
        var second = ProductRoutingState.FromGraph(
            LinearGraph(), new LastProcessedMachine(new MachineId(20)), CycleStatus.FinishedOk, FlowStatus.InProcess);
        var left = first.Value.ShouldNotBeNull();
        var right = second.Value.ShouldNotBeNull();

        // Sanity: the wrapped lists really are distinct instances (the equality must be by value, not identity).
        ReferenceEquals(left.LegalNextMachines.Machines, right.LegalNextMachines.Machines).ShouldBeFalse();

        // Act, Assert
        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    /// <summary>
    /// Builds the E6-1 linear fixture 10 (Initial|Serial) → 20 (Serial) → 30 (Final).
    /// </summary>
    /// <returns>The validated graph.</returns>
    private static ProductionGraph LinearGraph()
    {
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ];
        var result = ProductionGraph.Create(transitions);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }
}
