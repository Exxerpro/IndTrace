// <copyright file="FlowTransitionTableGuardWiringTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests;

using System.Collections.Generic;
using System.Linq;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Guards;

/// <summary>
/// Story 2.2-fix — anti-regression assertion that every <see cref="FlowTransitionTable"/> row carries its
/// EXPECTED guard composition. The Story 2.4 <c>Parity_EveryTableRowHasACase</c> test only checks rows that
/// are PRESENT in the table, so it is a tautology that cannot detect a dropped, swapped, or wrong guard.
/// This test pins the wiring itself: a future change that drops a guard, attaches the wrong guard, or removes
/// a row fails here. The expected composition is derived from the AS-BUILT handlers (see
/// <see cref="FlowTransitionTable"/> row evidence comments) — each guard maps 1:1 to a lookup the handler for
/// that row actually validates.
/// </summary>
public class FlowTransitionTableGuardWiringTests
{
    /// <summary>
    /// The expected guard composition per (From, Trigger) row, by guard TYPE NAME (ordered for composites —
    /// first-failure-wins). <see cref="AlwaysPassMarker"/> denotes the shared <see cref="Guard.AlwaysPass"/>
    /// no-op guard (a private nested type, matched by its runtime type below).
    /// </summary>
    private const string AlwaysPassMarker = "<AlwaysPass>";

    /// <summary>
    /// Gets the expected wiring: each row keyed by (FromValue, TriggerValue) to its ordered guard composition.
    /// </summary>
    /// <returns>The expected row -> ordered guard type-name list.</returns>
    public static IReadOnlyList<(int FromValue, int TriggerValue, string[] ExpectedGuards)> ExpectedWiring() =>
    [
        // None + CreateBarCode: as-built validates machine (Printer/InitialPrinter), product, rule. NOT barcode (label is generated).
        (FlowStatus.None.Value, GatewayTask.CreateBarCodeAsync.Value, [nameof(MachineGuard), nameof(ProductGuard), nameof(RuleGuard)]),

        // Created/InProcess + ReadBarCode: read-only no-op.
        (FlowStatus.Created.Value, GatewayTask.ReadBarCodeAsync.Value, [AlwaysPassMarker]),
        (FlowStatus.InProcess.Value, GatewayTask.ReadBarCodeAsync.Value, [AlwaysPassMarker]),

        // Created + CreateCycle: as-built resolves barcode by label and validates station/machine. NO shift lookup here.
        (FlowStatus.Created.Value, GatewayTask.CreateCycleAsync.Value, [nameof(BarCodeGuard), nameof(MachineGuard)]),

        // Created + UpdateCycleOk (2026-07-21 virtual-PLC E2E finding): the create station finishes its own
        // cycle while the item is still Created (cmd 4 then cmd 32). Guards mirror the InProcess Ok row.
        (FlowStatus.Created.Value, GatewayTask.UpdateCycleOkAsync.Value, [nameof(MachineGuard), nameof(ShiftGuard)]),

        // Created + UpdateCycleNotOk (2026-07-23 PO ratification, issue #189): the NOK mirror of the row
        // above — a create-station cycle physically finishing NOK (cmd 4 then cmd 64). Guards wired
        // IDENTICALLY to the OK sibling (Machine, Shift).
        (FlowStatus.Created.Value, GatewayTask.UpdateCycleNotOkAsync.Value, [nameof(MachineGuard), nameof(ShiftGuard)]),

        // InProcess + UpdateCycleOk: station/machine + shift (hard precondition) ONLY.
        // D2 / FR1 (INTENT, docs/architecture/state-machine/d2-cycle-path-routing-design.md §B/§D): CycleTimeGuard
        // was DELIBERATELY removed from this row — cycle-time is now a quality VERDICT owned by Cycle.FinishOk (the
        // ratified D1 asymmetry), not a transition-legality guard. The guard itself is unchanged and still directly
        // covered by GuardTests.CycleTimeGuard_WhenCycleTimeOutOfRange_ReturnsPartNotValidAndFinishedNok.
        (FlowStatus.InProcess.Value, GatewayTask.UpdateCycleOkAsync.Value, [nameof(MachineGuard), nameof(ShiftGuard)]),

        // InProcess + UpdateCycleNotOk: SAME station/machine + SAME shift precondition as the Ok row.
        // D2 / FR1 (INTENT): CycleTimeGuard removed here too — the cycle-time anomaly stays DISREGARDED in
        // Cycle.FinishNok (D1 NotOk-success preserved), not enforced as a table guard.
        (FlowStatus.InProcess.Value, GatewayTask.UpdateCycleNotOkAsync.Value, [nameof(MachineGuard), nameof(ShiftGuard)]),

        // InProcess + EndOfProcess: machine/station.
        (FlowStatus.InProcess.Value, GatewayTask.EndOfProcessAsync.Value, [nameof(MachineGuard)]),

        // Reject / Restore: handler resolves the barcode by label.
        (FlowStatus.InProcess.Value, GatewayTask.RejectPartAsync.Value, [nameof(BarCodeGuard)]),
        (FlowStatus.Finished.Value, GatewayTask.RejectPartAsync.Value, [nameof(BarCodeGuard)]),
        (FlowStatus.Rejected.Value, GatewayTask.RestorePartAsync.Value, [nameof(BarCodeGuard)]),

        // Story 4.1 — config-gated completeness rows (default OFF). Each carries a CompletenessGateGuard
        // (the gate) plus, where applicable, an entry guard enforcing the legal source even when the gate is ON.
        (FlowStatus.Created.Value, GatewayTask.MarkInvalid.Value, [nameof(CompletenessGateGuard)]),
        (FlowStatus.InProcess.Value, GatewayTask.MarkInvalid.Value, [nameof(CompletenessGateGuard)]),
        (FlowStatus.InProcess.Value, GatewayTask.MarkScrap.Value, [nameof(CompletenessGateGuard), nameof(PartScrappableGuard)]),
        (FlowStatus.Finished.Value, GatewayTask.MarkScrap.Value, [nameof(CompletenessGateGuard), nameof(PartScrappableGuard)]),
        (FlowStatus.InProcess.Value, GatewayTask.Cancel.Value, [nameof(CompletenessGateGuard), nameof(CycleStartedGuard)]),
    ];

    /// <summary>
    /// Every table row carries EXACTLY its expected guard composition (type names, in order). Detects a dropped,
    /// swapped, reordered, or wrong guard — the gap the adversarial review found (5 orphaned guards, 1 wrong row).
    /// </summary>
    [Fact]
    public void EveryRow_CarriesItsExpectedGuardComposition()
    {
        // Arrange
        var table = new FlowTransitionTable();
        var expected = ExpectedWiring();

        // Act & Assert — every expected row exists with exactly the expected ordered guard composition.
        foreach (var (fromValue, triggerValue, expectedGuards) in expected)
        {
            GatewayTask trigger = triggerValue;
            var found = table.TryResolve(FlowStatus.FromValue(fromValue), trigger, out var row);
            found.ShouldBeTrue($"expected a table row for (From={fromValue}, Trigger={triggerValue}).");
            row.ShouldNotBeNull();

            var actualGuards = DescribeGuard(row!.Guard);
            actualGuards.ShouldBe(
                expectedGuards,
                $"row (From={fromValue}, Trigger={triggerValue}) expected guards [{string.Join(", ", expectedGuards)}] but got [{string.Join(", ", actualGuards)}].");
        }
    }

    /// <summary>
    /// The table has EXACTLY the rows we assert wiring for — no extra, unverified row can be added without a
    /// matching expectation here. Pairs with <see cref="EveryRow_CarriesItsExpectedGuardComposition"/> to make
    /// the wiring assertion total (closes the parity-harness tautology in both directions).
    /// </summary>
    [Fact]
    public void Table_HasExactlyTheVerifiedRows()
    {
        // Arrange
        var table = new FlowTransitionTable();
        var expectedKeys = ExpectedWiring().Select(e => (e.FromValue, e.TriggerValue)).ToHashSet();

        // Act
        var actualKeys = table.Transitions.Select(t => (t.From.Value, t.Trigger.Value)).ToHashSet();

        // Assert
        actualKeys.Count.ShouldBe(expectedKeys.Count);
        actualKeys.ShouldBe(expectedKeys, ignoreOrder: true);
    }

    /// <summary>
    /// CreateBarCode must NOT carry a BarCodeGuard (the wrong-row bug the review found): the as-built handler
    /// GENERATES the label, it does not look one up, so a barcode-presence precondition there is incorrect.
    /// </summary>
    [Fact]
    public void CreateBarCodeRow_DoesNotCarryBarCodeGuard()
    {
        // Arrange
        var table = new FlowTransitionTable();
        table.TryResolve(FlowStatus.None, GatewayTask.CreateBarCodeAsync, out var row);

        // Act
        var guards = DescribeGuard(row!.Guard);

        // Assert
        guards.ShouldNotContain(nameof(BarCodeGuard));
    }

    /// <summary>
    /// Flattens a guard (composite or single) into the ordered list of its constituent guard type names.
    /// The shared <see cref="Guard.AlwaysPass"/> nested type is reported as <see cref="AlwaysPassMarker"/>.
    /// </summary>
    /// <param name="guard">The guard to describe.</param>
    /// <returns>The ordered list of guard type names.</returns>
    private static string[] DescribeGuard(IGuard guard) =>
        guard switch
        {
            CompositeGuard composite => GetCompositeGuards(composite).SelectMany(DescribeGuard).ToArray(),
            BarCodeGuard => [nameof(BarCodeGuard)],
            MachineGuard => [nameof(MachineGuard)],
            ProductGuard => [nameof(ProductGuard)],
            RuleGuard => [nameof(RuleGuard)],
            ShiftGuard => [nameof(ShiftGuard)],
            RecipeGuard => [nameof(RecipeGuard)],
            CycleTimeGuard => [nameof(CycleTimeGuard)],
            MachineFinalGuard => [nameof(MachineFinalGuard)],
            _ when ReferenceEquals(guard, Guard.AlwaysPass) => [AlwaysPassMarker],
            _ => [guard.GetType().Name],
        };

    /// <summary>
    /// Reads a <see cref="CompositeGuard"/>'s ordered constituent guards via reflection (the field is private —
    /// the composite intentionally exposes no accessor; the test reaches in to verify ORDER for first-failure-wins).
    /// </summary>
    /// <param name="composite">The composite guard.</param>
    /// <returns>The ordered constituent guards.</returns>
    private static IEnumerable<IGuard> GetCompositeGuards(CompositeGuard composite)
    {
        var field = typeof(CompositeGuard).GetField("guards", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.ShouldNotBeNull("CompositeGuard.guards field not found (rename?).");
        var value = field!.GetValue(composite);
        return ((IReadOnlyList<IGuard>)value!).ToArray();
    }
}
