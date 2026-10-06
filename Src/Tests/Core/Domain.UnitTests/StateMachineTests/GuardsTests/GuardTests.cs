// <copyright file="GuardTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests.GuardsTests;

using IndTrace.Domain.Entities;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Guards;
using IndTrace.TestData.Builders;

/// <summary>
/// Story 2.2 — one test per row of the guard-failure → <see cref="ResultValidation"/> mapping table
/// (the Epic 2 correctness core), plus CompositeGuard first-failure-wins, the Fire-guard-fail no-mutation
/// integration test, and the negative-convention [Theory]. xUnit + Shouldly + the boolean+code GuardResult shape.
/// </summary>
public class GuardTests
{
    private static Recipe ValidRecipe() => Recipe.CreateFixture(0, 0, 0, 10, 100, 3, 5, 1);

    // Story 2.2-fix: the context is FAIL-CLOSED (presence flags default FALSE). A "satisfying" context must
    // EXPLICITLY set every presence flag true; the negative tests below override individual flags to false.
    private static TransitionContext SatisfyingContext() =>
        new(MachineType.Process, CycleStatus.Started, PartStatus.Ok, 50, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    // ---- 1. CycleTimeGuard: out of range -> PartNotValid (-64) + FinishedNok override (AC5) ----

    /// <summary>
    /// Out-of-range cycle time publishes <see cref="ResultValidation.PartNotValid"/> (-64) and forces
    /// the <see cref="CycleStatus.FinishedNok"/> override (AC5), rather than hard-failing like a lookup miss.
    /// </summary>
    [Fact]
    public void CycleTimeGuard_WhenCycleTimeOutOfRange_ReturnsPartNotValidAndFinishedNok()
    {
        // Arrange
        var guard = new CycleTimeGuard();
        var context = SatisfyingContext() with { CycleTime = 5_000 }; // above the recipe maximum (100)

        // Act
        var result = guard.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.PartNotValid);
        result.Code.Value.ShouldBe(-64);
        result.OverrideCycleStatus.ShouldNotBeNull();
        result.OverrideCycleStatus!.Value.ShouldBe(CycleStatus.FinishedNok.Value);
    }

    /// <summary>
    /// In-range cycle time with a valid recipe passes.
    /// </summary>
    [Fact]
    public void CycleTimeGuard_WhenCycleTimeInRange_Passes()
    {
        // Arrange
        var guard = new CycleTimeGuard();

        // Act
        var result = guard.Evaluate(SatisfyingContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Code.ShouldBe(ResultValidation.Valid);
    }

    // ---- 2. CycleTimeGuard: null recipe -> RecipeNotFound (-512), NOT PartNotValid ----

    /// <summary>
    /// A null recipe is distinguished from an out-of-range time and publishes the precise
    /// <see cref="ResultValidation.RecipeNotFound"/> (-512) code (intended FR4 diagnostic improvement),
    /// while still forcing the SAME <see cref="CycleStatus.FinishedNok"/> override as the out-of-range path
    /// so the STATE outcome matches as-built (Story 2.4 NFR4 parity).
    /// </summary>
    [Fact]
    public void CycleTimeGuard_WhenRecipeNull_ReturnsRecipeNotFound()
    {
        // Arrange
        var guard = new CycleTimeGuard();
        var context = SatisfyingContext() with { Recipe = null };

        // Act
        var result = guard.Evaluate(context);

        // Assert — precise code RecipeNotFound (FR4) BUT the as-built FinishedNok state override (NFR4 parity).
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.RecipeNotFound);
        result.Code.Value.ShouldBe(-512);
        result.OverrideCycleStatus.ShouldNotBeNull();
        result.OverrideCycleStatus!.Value.ShouldBe(CycleStatus.FinishedNok.Value);
    }

    // ---- 3. MachineFinalGuard -> InvalidMachine (-4096); pass on Final + FinishedOk ----

    /// <summary>
    /// A non-final / not-finished context publishes <see cref="ResultValidation.InvalidMachine"/> (-4096).
    /// </summary>
    [Fact]
    public void MachineFinalGuard_WhenFinalContextInvalid_ReturnsInvalidMachine()
    {
        // Arrange
        var guard = new MachineFinalGuard();
        var context = SatisfyingContext() with { MachineType = MachineType.Process, CycleStatus = CycleStatus.Started };

        // Act
        var result = guard.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.InvalidMachine);
        result.Code.Value.ShouldBe(-4096);
    }

    /// <summary>
    /// Final machine + FinishedOk cycle succeeds via the IsFlowFinished predicate.
    /// </summary>
    [Fact]
    public void MachineFinalGuard_WhenFinalAndFinishedOk_Passes()
    {
        // Arrange
        var guard = new MachineFinalGuard();
        var context = SatisfyingContext() with { MachineType = MachineType.Final, CycleStatus = CycleStatus.FinishedOk };

        // Act
        var result = guard.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    // ---- 4. ShiftGuard -> ShiftInvalid (-32768) ----

    /// <summary>
    /// An invalid shift publishes <see cref="ResultValidation.ShiftInvalid"/> (-32768).
    /// </summary>
    [Fact]
    public void ShiftGuard_WhenShiftInvalid_ReturnsShiftInvalid()
    {
        // Arrange
        var guard = new ShiftGuard();
        var context = SatisfyingContext() with { ShiftValid = false };

        // Act
        var result = guard.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.ShiftInvalid);
        result.Code.Value.ShouldBe(-32768);
    }

    /// <summary>
    /// A valid shift passes.
    /// </summary>
    [Fact]
    public void ShiftGuard_WhenShiftValid_Passes()
    {
        new ShiftGuard().Evaluate(SatisfyingContext()).IsSuccess.ShouldBeTrue();
    }

    // ---- 5. RecipeGuard -> RecipeNotFound (-512) ----

    /// <summary>
    /// A missing recipe publishes <see cref="ResultValidation.RecipeNotFound"/> (-512).
    /// </summary>
    [Fact]
    public void RecipeGuard_WhenRecipeMissing_ReturnsRecipeNotFound()
    {
        // Arrange
        var guard = new RecipeGuard();
        var context = SatisfyingContext() with { RecipeFound = false, Recipe = null };

        // Act
        var result = guard.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.RecipeNotFound);
        result.Code.Value.ShouldBe(-512);
    }

    /// <summary>
    /// A present recipe passes.
    /// </summary>
    [Fact]
    public void RecipeGuard_WhenRecipePresent_Passes()
    {
        new RecipeGuard().Evaluate(SatisfyingContext()).IsSuccess.ShouldBeTrue();
    }

    // ---- 6. ProductGuard -> ProductNotFound (-16384) ----

    /// <summary>
    /// A missing product publishes <see cref="ResultValidation.ProductNotFound"/> (-16384).
    /// </summary>
    [Fact]
    public void ProductGuard_WhenProductMissing_ReturnsProductNotFound()
    {
        // Arrange
        var guard = new ProductGuard();
        var context = SatisfyingContext() with { ProductFound = false };

        // Act
        var result = guard.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.ProductNotFound);
        result.Code.Value.ShouldBe(-16384);
    }

    /// <summary>
    /// A present product passes.
    /// </summary>
    [Fact]
    public void ProductGuard_WhenProductPresent_Passes()
    {
        new ProductGuard().Evaluate(SatisfyingContext()).IsSuccess.ShouldBeTrue();
    }

    // ---- 7. RuleGuard -> RuleNotFound (-8192) ----

    /// <summary>
    /// A missing rule publishes <see cref="ResultValidation.RuleNotFound"/> (-8192).
    /// </summary>
    [Fact]
    public void RuleGuard_WhenRuleMissing_ReturnsRuleNotFound()
    {
        // Arrange
        var guard = new RuleGuard();
        var context = SatisfyingContext() with { RuleFound = false };

        // Act
        var result = guard.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.RuleNotFound);
        result.Code.Value.ShouldBe(-8192);
    }

    /// <summary>
    /// A present rule passes.
    /// </summary>
    [Fact]
    public void RuleGuard_WhenRulePresent_Passes()
    {
        new RuleGuard().Evaluate(SatisfyingContext()).IsSuccess.ShouldBeTrue();
    }

    // ---- 8. BarCodeGuard -> BarCodeNotFound (-2) ----

    /// <summary>
    /// A missing barcode publishes <see cref="ResultValidation.BarCodeNotFound"/> (-2).
    /// </summary>
    [Fact]
    public void BarCodeGuard_WhenBarCodeMissing_ReturnsBarCodeNotFound()
    {
        // Arrange
        var guard = new BarCodeGuard();
        var context = SatisfyingContext() with { BarCodeFound = false };

        // Act
        var result = guard.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.BarCodeNotFound);
        result.Code.Value.ShouldBe(-2);
    }

    /// <summary>
    /// A present barcode passes.
    /// </summary>
    [Fact]
    public void BarCodeGuard_WhenBarCodePresent_Passes()
    {
        new BarCodeGuard().Evaluate(SatisfyingContext()).IsSuccess.ShouldBeTrue();
    }

    // ---- 9. MachineGuard -> MachineNotFound (-8) ----

    /// <summary>
    /// A missing machine publishes <see cref="ResultValidation.MachineNotFound"/> (-8).
    /// </summary>
    [Fact]
    public void MachineGuard_WhenMachineMissing_ReturnsMachineNotFound()
    {
        // Arrange
        var guard = new MachineGuard();
        var context = SatisfyingContext() with { MachineFound = false };

        // Act
        var result = guard.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.MachineNotFound);
        result.Code.Value.ShouldBe(-8);
    }

    /// <summary>
    /// A present machine passes.
    /// </summary>
    [Fact]
    public void MachineGuard_WhenMachinePresent_Passes()
    {
        new MachineGuard().Evaluate(SatisfyingContext()).IsSuccess.ShouldBeTrue();
    }

    // ---- 10. CompositeGuard first-failure-wins (AC7) ----

    /// <summary>
    /// With barcode-missing ordered before recipe-missing, the composite publishes the FIRST failure code
    /// (<see cref="ResultValidation.BarCodeNotFound"/>), not the later one.
    /// </summary>
    [Fact]
    public void CompositeGuard_WhenFirstGuardFails_PublishesFirstFailureCode()
    {
        // Arrange — both barcode and recipe are missing; barcode guard is first.
        var composite = new CompositeGuard(new BarCodeGuard(), new RecipeGuard());
        var context = SatisfyingContext() with { BarCodeFound = false, RecipeFound = false, Recipe = null };

        // Act
        var result = composite.Evaluate(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(ResultValidation.BarCodeNotFound);
        result.Code.ShouldNotBe(ResultValidation.RecipeNotFound);
    }

    /// <summary>
    /// When all composed guards pass, the composite passes.
    /// </summary>
    [Fact]
    public void CompositeGuard_WhenAllGuardsPass_Passes()
    {
        var composite = new CompositeGuard(new BarCodeGuard(), new MachineGuard(), new RecipeGuard());
        composite.Evaluate(SatisfyingContext()).IsSuccess.ShouldBeTrue();
    }

    // ---- 11. Fire integration: guard fail -> failure with guard code, no mutation (AC6) ----

    /// <summary>
    /// A legal (From, Trigger) pair whose guard fails returns failure carrying the guard's specific code,
    /// and leaves the BarCode unmutated (AC6). Story 2.2-fix: the CreateBarCode row is wired to
    /// Machine + Product + Rule (the as-built CreateBarCodeCommandHandler validates a machine, product, and
    /// rule — it GENERATES the label, it does not look one up), so a missing machine fails it with the
    /// first-failure-wins MachineNotFound code.
    /// </summary>
    [Fact]
    public void Fire_WhenGuardFails_DoesNotMutateBarCode()
    {
        // Arrange — legal pair (None, CreateBarCodeAsync) but the machine lookup fails the MachineGuard.
        var machine = new ItemStateMachine();
        var barCode = new BarCodeBuilder().AtState(FlowStatus.None, PartStatus.None).Build();
        var originalFlow = barCode.FlowStatus.Value;
        var originalPart = barCode.PartStatus.Value;
        var context = SatisfyingContext() with { MachineFound = false };

        // Act
        var result = machine.Fire(barCode, GatewayTask.CreateBarCodeAsync, context);

        // Assert — failure carries the guard's specific code (NOT OperationCancelled).
        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldNotBeNull().Result.ShouldBe(ResultValidation.MachineNotFound);
        result.Value.ShouldNotBeNull().Result.ShouldNotBe(ResultValidation.OperationCancelled);

        // No-mutation proof.
        barCode.FlowStatus.Value.ShouldBe(originalFlow);
        barCode.PartStatus.Value.ShouldBe(originalPart);
    }

    /// <summary>
    /// D2 / FR1 (docs/architecture/state-machine/d2-cycle-path-routing-design.md §B/§C) — INTENT CHANGE, not
    /// convenience. The UpdateCycleOk table row DELIBERATELY no longer carries <see cref="CycleTimeGuard"/>:
    /// cycle-time is a quality VERDICT owned by <c>Cycle.FinishOk</c> (which expresses the ratified D1
    /// OK-failure/NotOk-success asymmetry), NOT a transition-legality guard. A guard-reject here would be a
    /// <c>Fire</c> failure, which is incompatible with that asymmetry, so the guard was removed from the row.
    ///
    /// Consequently, firing <see cref="GatewayTask.UpdateCycleOkAsync"/> on a legal <c>InProcess</c> barcode now
    /// SUCCEEDS regardless of cycle time (only the <c>(Machine, Shift)</c> guards remain, both satisfied here).
    /// The CycleTimeGuard ITSELF is unchanged and still directly covered by
    /// <see cref="CycleTimeGuard_WhenCycleTimeOutOfRange_ReturnsPartNotValidAndFinishedNok"/> — this test now pins
    /// that the GUARD is no longer wired onto the UpdateCycleOk ROW (the previous assertion that the fire FAILED is
    /// the byte the design intentionally deletes from the machine table; see design §C and
    /// FlowTransitionTableGuardWiringTests).
    /// </summary>
    [Fact]
    public void Fire_UpdateCycleOk_OutOfRangeCycleTime_NoLongerGuarded_Succeeds()
    {
        // Arrange — out-of-range cycle time on an InProcess barcode (the same input that previously FAILED).
        var machine = new ItemStateMachine();
        var barCode = new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.None).Build();
        var context = SatisfyingContext() with { CycleTime = 5_000 };

        // Act
        var result = machine.Fire(barCode, GatewayTask.UpdateCycleOkAsync, context);

        // Assert — D2: cycle-time is no longer a transition-legality guard on this row, so the fire SUCCEEDS.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value); // non-final OK -> InProcess
        result.Value.ShouldNotBeNull().Result.Value.ShouldBe(ResultValidation.Valid.Value);
    }

    // ---- 12. Negative-convention [Theory] (AC9) ----

    /// <summary>
    /// Every guard-failure code honors the negative = failure convention (AC9).
    /// </summary>
    /// <param name="code">The guard-failure code under test.</param>
    [Theory]
    [MemberData(nameof(GuardFailureCodes))]
    public void EveryGuardFailureCode_IsNegative(ResultValidation code)
    {
        code.Value.ShouldBeLessThan(0);
    }

    /// <summary>
    /// Gets the full set of guard-failure codes used by Story 2.2 guards (mapping-table column).
    /// </summary>
    public static TheoryData<ResultValidation> GuardFailureCodes() =>
    [
        ResultValidation.PartNotValid,
        ResultValidation.InvalidMachine,
        ResultValidation.ShiftInvalid,
        ResultValidation.RecipeNotFound,
        ResultValidation.ProductNotFound,
        ResultValidation.RuleNotFound,
        ResultValidation.BarCodeNotFound,
        ResultValidation.MachineNotFound,
    ];
}
