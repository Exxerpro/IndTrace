// <copyright file="StateMachineParityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests.Parity;

using System.Linq;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.StateMachine;
using IndTrace.TestData.Builders;

/// <summary>
/// Story 2.4 — the parity harness. Feeds each Epic-1 as-built golden-master input
/// (<see cref="GoldenMasterCases"/>) into <see cref="IItemStateMachine.Fire"/> and asserts the new engine
/// reproduces the recorded as-built outputs. The three PLC-visible STATE fields (FlowStatus, CycleStatus,
/// PartStatus) are asserted with STRICT parity (NFR4 — frozen numerics); any divergence there is a BLOCKING
/// engine bug. ResultValidation parity is asserted on the success / default-reject cases; cases where Story 2.2
/// intentionally emits a NEW precise code (FR4 groundwork) are flagged via
/// <see cref="ParityCase.ResultValidationDivergesFromAsBuilt"/> and assert the engine's actual code.
///
/// Oracle: the committed characterization tests under
/// <c>Src/Tests/Core/Application.UnitTests/Characterization/**</c> plus
/// <c>docs/architecture/state-machine-analysis.md</c> §4/§6. Each case cites its originating golden master.
/// </summary>
public class StateMachineParityTests
{
    /// <summary>
    /// Gets the case names as serializable theory data (xUnit v3 — pass primitives, resolve the rich case by name).
    /// </summary>
    public static TheoryData<string> CaseNames
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var c in GoldenMasterCases.All())
            {
                data.Add(c.Name);
            }

            return data;
        }
    }

    private static IItemStateMachine NewMachine() => new ItemStateMachine();

    private static ParityCase Case(string name) =>
        GoldenMasterCases.All().Single(c => c.Name == name);

    /// <summary>
    /// Builds the barcode for a case. The incoming PartStatus is seeded from the context's PartStatus so the
    /// engine's reject-path echo (NextPartStatus == item.PartStatus) and success-path echo
    /// (NextPartStatus == context.PartStatus) are both deterministic and realistic (the item's quality state
    /// equals the inputs the handler computed).
    /// </summary>
    // Story 6.4: the status setters are private set; seed the arbitrary (From, PartStatus) parity pair through
    // the builder's AtState seam (byte-equal — it reaches arbitrary pairs via the same internal CreateFixture).
    private static BarCode BarCodeFor(ParityCase c) =>
        new BarCodeBuilder().AtState(c.From, c.Context.PartStatus).Build();

    /// <summary>
    /// Testing #1 (AC1, AC2, AC5) — every golden-master case produces identical STATE outputs. Asserts the
    /// three PLC-visible state fields STRICTLY; the failure message names the case and the diverging field.
    /// </summary>
    /// <param name="caseName">The golden-master case name.</param>
    [Theory]
    [MemberData(nameof(CaseNames))]
    public void Parity_AllGoldenMasterCases_ProduceIdenticalOutputs(string caseName)
    {
        // Arrange
        var c = Case(caseName);
        var machine = NewMachine();
        var barCode = BarCodeFor(c);

        // Act
        var result = machine.Fire(barCode, c.Trigger, c.Context);

        // Assert — success/failure shape matches as-built.
        result.IsSuccess.ShouldBe(!c.IsFailure, $"case={c.Name} expected IsSuccess={!c.IsFailure} got {result.IsSuccess}");

        // The outcome (carried on success and on the engine's failure path) holds the resolved state.
        var outcome = result.Value;
        outcome.ShouldNotBeNull($"case={c.Name} expected an outcome on Value");

        // STRICT parity on the three PLC-visible STATE fields (NFR4). Compare by .Value (smart-enum numeric).
        outcome!.NextFlowStatus.Value.ShouldBe(
            c.ExpectedFlowStatus.Value,
            $"case={c.Name} expected FlowStatus={c.ExpectedFlowStatus.Name} got {outcome.NextFlowStatus.Name}");

        outcome.NextCycleStatus.Value.ShouldBe(
            c.ExpectedCycleStatus.Value,
            $"case={c.Name} expected CycleStatus={c.ExpectedCycleStatus.Name} got {outcome.NextCycleStatus.Name}");

        outcome.NextPartStatus.Value.ShouldBe(
            c.ExpectedPartStatus.Value,
            $"case={c.Name} expected PartStatus={c.ExpectedPartStatus.Name} got {outcome.NextPartStatus.Name}");

        // ResultValidation parity: assert the engine's expected code for THIS case. For divergent cases the
        // expected code is the engine's NEW precise code (not as-built) — and the divergence is documented in
        // GoldenMasterCases + reported in the dedicated divergence test below.
        outcome.Result.Value.ShouldBe(
            c.ExpectedResult.Value,
            $"case={c.Name} expected ResultValidation={c.ExpectedResult.Name} got {outcome.Result.Name}");
    }

    /// <summary>
    /// Testing #2 (AC3) — EndOfProcess reproduces the PERSISTED truth Finished / FinishedOk / Ok (anomaly §6 #4),
    /// NOT the transient request projection EndOfProcess / NOk.
    /// </summary>
    [Fact]
    public void Parity_EndOfProcess_ReproducesPersistedFinishedOk()
    {
        // Arrange
        var c = Case("EndOfProcess_PersistedFinishedOk");
        var barCode = BarCodeFor(c);

        // Act
        var result = NewMachine().Fire(barCode, c.Trigger, c.Context);

        // Assert — persisted truth (NOT EndOfProcess(16) / NOk(2)).
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
        result.Value.NextCycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        result.Value.NextPartStatus.Value.ShouldBe(PartStatus.Ok.Value);

        // The projection's divergent values are explicitly NOT what the engine reproduces.
        result.Value.NextCycleStatus.Value.ShouldNotBe(CycleStatus.EndOfProcess.Value);
        result.Value.NextPartStatus.Value.ShouldNotBe(PartStatus.NOk.Value);
    }

    /// <summary>
    /// Testing #3 — D2 / FR1 (INTENT, docs/architecture/state-machine/d2-cycle-path-routing-design.md §B/§C):
    /// CycleTimeGuard was DELIBERATELY removed from the UpdateCycleOk table row — cycle-time is now a quality VERDICT
    /// owned by <c>Cycle.FinishOk</c> (the ratified D1 asymmetry), NOT a transition-legality guard. So firing
    /// UpdateCycleOk on a legal InProcess barcode now SUCCEEDS regardless of cycle time or recipe presence (only the
    /// <c>(Machine, Shift)</c> guards remain, both satisfied here). The STATE tuple the machine echoes is unchanged
    /// (FlowStatus stays InProcess non-final, CycleStatus/PartStatus echo the context = FinishedNok/NOk), but the
    /// result is now SUCCESS with <see cref="ResultValidation.Valid"/>. The cycle-time verdict and its recipe-aware
    /// PartNotValid/RecipeNotFound codes are still pinned at the ENTITY/handler level
    /// (UpdateCyclesGoldenMasterTests / CycleTimeOverrideAnomalyTests) and the guard itself by GuardTests.
    /// </summary>
    /// <param name="caseName">The out-of-range / null-recipe case name.</param>
    [Theory]
    [InlineData("CycleTimeOutOfRange_BelowMin")]
    [InlineData("CycleTimeOutOfRange_EqualsMin")]
    [InlineData("CycleTimeOutOfRange_EqualsMax")]
    [InlineData("CycleTimeOutOfRange_AboveMax")]
    [InlineData("CycleTimeNullRecipe")]
    public void Parity_CycleTimeOutOfRange_NoLongerGuarded_Succeeds(string caseName)
    {
        // Arrange
        var c = Case(caseName);
        var barCode = BarCodeFor(c);

        // Act
        var result = NewMachine().Fire(barCode, c.Trigger, c.Context);

        // Assert — D2: cycle-time is no longer a machine-table guard, so the fire SUCCEEDS with the echoed tuple.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
        result.Value.NextCycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        result.Value.NextPartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        result.Value.Result.Value.ShouldBe(ResultValidation.Valid.Value);
    }

    /// <summary>
    /// Testing #4 (AC3) — Restore reproduces FlowStatus.InProcess (NOT FlowStatus.Restored), anomaly §6 #5.
    /// </summary>
    [Fact]
    public void Parity_Restore_ReproducesInProcessNotRestored()
    {
        // Arrange
        var c = Case("Restore_FromRejected_InProcessNotRestored");
        var barCode = BarCodeFor(c);

        // Act
        var result = NewMachine().Fire(barCode, c.Trigger, c.Context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
        result.Value.NextFlowStatus.Value.ShouldNotBe(FlowStatus.Restored.Value);
    }

    /// <summary>
    /// Testing #6 (AC2, AC5) — reject / illegal-pair parity: default-reject reproduces
    /// <see cref="ResultValidation.OperationCancelled"/> exactly as Epic 1, with NO state advance (FlowStatus
    /// unchanged, the engine never mutates the item).
    /// </summary>
    /// <param name="caseName">The illegal-pair case name.</param>
    [Theory]
    [InlineData("Illegal_Rejected_UpdateCycleOk")]
    [InlineData("Illegal_Finished_UpdateCycleOk")]
    [InlineData("Illegal_None_ReadBarCode")]
    [InlineData("Illegal_InProcess_Restore")]
    [InlineData("Illegal_Finished_CreateCycle")]
    public void Parity_IllegalPair_ReproducesOperationCancelled_NoAdvance(string caseName)
    {
        // Arrange
        var c = Case(caseName);
        var barCode = BarCodeFor(c);
        var originalFlow = barCode.FlowStatus.Value;

        // Act
        var result = NewMachine().Fire(barCode, c.Trigger, c.Context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldNotBeNull().Result.Value.ShouldBe(ResultValidation.OperationCancelled.Value);
        result.Value.NextFlowStatus.Value.ShouldBe(c.From.Value);

        // No advance: the passed barcode is not mutated by Fire (pure read).
        barCode.FlowStatus.Value.ShouldBe(originalFlow);
    }

    /// <summary>
    /// Story 4.1 — the off-bus, config-gated completeness triggers. These rows are NEW (they did not exist in
    /// Epic 1), so they have no Epic-1 golden-master case by definition; their behavior is covered by
    /// <c>CompletenessGatingTests</c>. They are excluded from the as-built golden-master coverage cross-check.
    /// </summary>
    private static readonly int[] GatedCompletenessTriggers =
    [
        GatewayTask.MarkInvalid.Value,
        GatewayTask.MarkScrap.Value,
        GatewayTask.Cancel.Value,
    ];

    /// <summary>
    /// Testing #5 (AC4) — coverage cross-check: every AS-BUILT row in the Story 2.1
    /// <see cref="FlowTransitionTable"/> appears at least once in the golden-master case set (no untested
    /// legal transition). The Story-4.1 gated completeness rows are excluded (new, not part of the Epic-1
    /// golden master).
    /// </summary>
    [Fact]
    public void Parity_EveryTableRowHasACase()
    {
        // Arrange
        var table = new FlowTransitionTable();
        var cases = GoldenMasterCases.All().ToArray();

        // Act & Assert — for each as-built table row there is at least one case whose (From, Trigger) matches it.
        foreach (var row in table.Transitions.Where(r => !GatedCompletenessTriggers.Contains(r.Trigger.Value)))
        {
            var covered = cases.Any(c =>
                c.From.Value == row.From.Value && c.Trigger.Value == row.Trigger.Value);

            covered.ShouldBeTrue(
                $"transition-table row ({row.From.Name}, {row.Trigger.Name}) -> {row.To.Name} has no parity case.");
        }
    }
}
