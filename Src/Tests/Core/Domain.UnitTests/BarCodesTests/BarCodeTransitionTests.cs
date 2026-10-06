// <copyright file="BarCodeTransitionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.BarCodesTests;

using IndTrace.Domain.StateMachine;
using IndTrace.TestData.Builders;

/// <summary>
/// Story 2.3a — AC2/AC5/AC6 tests for the de-anemized <see cref="BarCode"/> guarded transition methods.
/// The methods delegate to the (pure-domain) item state machine and apply the resolved outcome to the
/// barcode's own status fields ONLY on success; on failure the entity is left unmutated and the specific
/// <see cref="ResultValidation"/> code is surfaced via <c>result.Value.Result</c>. The Restore->InProcess
/// anomaly is preserved (the barcode NEVER becomes <see cref="FlowStatus.Restored"/>).
/// </summary>
public class BarCodeTransitionTests
{
    private static Recipe ValidRecipe() => Recipe.CreateFixture(0, 0, 0, 0, 216000, 3, 5, 1);

    // Story 2.2-fix: the context is FAIL-CLOSED (presence flags default FALSE). Legal transitions must
    // EXPLICITLY set the presence flags true so their wired guards (e.g. CreateCycle -> BarCode+Machine+Shift)
    // pass; the illegal-transition tests below reject at the table (flags irrelevant).
    private static TransitionContext FinalOkContext() =>
        new(MachineType.Final, CycleStatus.FinishedOk, PartStatus.Ok, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    private static TransitionContext NonFinalContext() =>
        new(MachineType.Process, CycleStatus.Started, PartStatus.Ok, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    /// <summary>
    /// Test #1 — UpdateCycleOk on a Final machine with a FinishedOk cycle transitions the barcode to Finished.
    /// </summary>
    [Fact]
    public void BarCode_UpdateCycleOk_WhenFinalAndOk_TransitionsToFinished()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.None).Build();

        // Act
        var result = barCode.UpdateCycleOk(FinalOkContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
    }

    /// <summary>
    /// Test #1 (companion) — UpdateCycleOk on a non-final machine keeps the barcode InProcess.
    /// </summary>
    [Fact]
    public void BarCode_UpdateCycleOk_WhenNonFinal_StaysInProcess()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.None).Build();

        // Act
        var result = barCode.UpdateCycleOk(NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
    }

    /// <summary>
    /// Test #2 — Reject from InProcess transitions the barcode to Rejected.
    /// </summary>
    [Fact]
    public void BarCode_Reject_FromInProcess_TransitionsToRejected()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.None).Build();

        // Act
        var result = barCode.Reject(NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.Rejected.Value);
    }

    /// <summary>
    /// Test #2 — Reject from Finished transitions the barcode to Rejected.
    /// </summary>
    [Fact]
    public void BarCode_Reject_FromFinished_TransitionsToRejected()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.Finished, PartStatus.Ok).Build();

        // Act
        var result = barCode.Reject(NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.Rejected.Value);
    }

    /// <summary>
    /// Test #3 — Restore from Rejected returns the barcode to InProcess (NOT Restored) — preserved anomaly (AC5).
    /// </summary>
    [Fact]
    public void BarCode_Restore_FromRejected_TransitionsToInProcess()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.Rejected, PartStatus.Rejected).Build();

        // Act
        var result = barCode.Restore(NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
        barCode.FlowStatus.Value.ShouldNotBe(FlowStatus.Restored.Value);
    }

    /// <summary>
    /// Test #4 — UpdateCycleOk from Rejected is illegal: it fails, leaves the barcode unchanged, and carries
    /// OperationCancelled.
    /// </summary>
    [Fact]
    public void BarCode_UpdateCycleOk_FromRejected_Fails_AndStateUnchanged()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.Rejected, PartStatus.Rejected).Build();

        // Act
        var result = barCode.UpdateCycleOk(NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeFalse();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.Rejected.Value);
        result.Value.ShouldNotBeNull().Result.Value.ShouldBe(ResultValidation.OperationCancelled.Value);
    }

    /// <summary>
    /// AC2 — CreateCycle from Created transitions the barcode to InProcess.
    /// </summary>
    [Fact]
    public void BarCode_CreateCycle_FromCreated_TransitionsToInProcess()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.Created, PartStatus.None).Build();

        // Act
        var result = barCode.CreateCycle(NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
    }

    /// <summary>
    /// AC2 — EndOfProcess from InProcess transitions the barcode to Finished.
    /// </summary>
    [Fact]
    public void BarCode_EndOfProcess_FromInProcess_TransitionsToFinished()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.Ok).Build();

        // Act
        var result = barCode.EndOfProcess(NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
    }

    /// <summary>
    /// Story 6.2 — MarkPartNok forces PartStatus.NOk and leaves FlowStatus unchanged (guarded part demotion
    /// replacing the anemic inline `barCode.PartStatus = NOk` in the cycle-update strategies).
    /// </summary>
    [Fact]
    public void BarCode_MarkPartNok_SetsPartNok_AndLeavesFlowStatusUnchanged()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.Ok).Build();

        // Act
        var result = barCode.MarkPartNok();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().Value.ShouldBe(PartStatus.NOk.Value);
        barCode.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value); // FlowStatus untouched
    }

    /// <summary>
    /// Test #6 — CanFire answers the permitted-operations query without firing, for InProcess and Rejected.
    /// </summary>
    /// <param name="fromValue">The barcode's current FlowStatus value.</param>
    /// <param name="triggerValue">The trigger value being tested.</param>
    /// <param name="expected">Whether the trigger is expected to be permitted from that state.</param>
    [Theory]
    [InlineData(2, 32, true)]    // InProcess, UpdateCycleOkAsync -> permitted
    [InlineData(2, 4, false)]    // InProcess, CreateBarCodeAsync -> not permitted
    [InlineData(2, 256, true)]   // InProcess, RejectPartAsync -> permitted
    [InlineData(32, 1024, true)]  // Rejected, RestorePartAsync=1024 -> permitted (the only one)
    [InlineData(32, 32, false)]  // Rejected, UpdateCycleOkAsync -> not permitted
    [InlineData(32, 256, false)] // Rejected, RejectPartAsync -> not permitted
    public void BarCode_CanFire_ReturnsPermittedOperations(int fromValue, int triggerValue, bool expected)
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.FromValue(fromValue), PartStatus.None).Build();
        GatewayTask trigger = triggerValue;

        // Act & Assert
        barCode.CanFire(trigger).ShouldBe(expected);
    }

    /// <summary>
    /// Test #6 — CanFire does NOT mutate the barcode.
    /// </summary>
    [Fact]
    public void BarCode_CanFire_DoesNotMutateState()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.Ok).Build();

        // Act
        _ = barCode.CanFire(GatewayTask.UpdateCycleOkAsync);

        // Assert
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
        barCode.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
    }
}
