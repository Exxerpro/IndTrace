// <copyright file="CycleTransitionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.CyclesTests;

using IndTrace.Domain.StateMachine;
using IndTrace.TestData.Builders;

/// <summary>
/// Story 2.3a — AC3/AC5 tests for the de-anemized <see cref="Cycle"/> sub-machine methods. The guarded
/// <see cref="Cycle.FinishOk"/> preserves the as-built cycle-time override (Story 2.2): an out-of-range
/// cycle time does NOT pass as FinishedOk; it forces <see cref="CycleStatus.FinishedNok"/> /
/// <see cref="PartStatus.NOk"/> and surfaces <see cref="ResultValidation.PartNotValid"/>.
/// </summary>
public class CycleTransitionTests
{
    private static Recipe Recipe(int min, int max) => IndTrace.Domain.Entities.Recipe.CreateFixture(0, 0, 0, min, max, 3, 5, 1);

    private static TransitionContext Context(int cycleTime, Recipe? recipe) =>
        new(MachineType.Process, CycleStatus.Started, PartStatus.Ok, cycleTime, recipe);

    /// <summary>
    /// AC3 — Start moves the cycle to Started.
    /// </summary>
    [Fact]
    public void Cycle_Start_BecomesStarted()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.NotStarted, PartStatus.None).Build();

        // Act
        var result = cycle.Start();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
    }

    /// <summary>
    /// AC3 — FinishOk with an in-range cycle time becomes FinishedOk / Ok.
    /// </summary>
    [Fact]
    public void Cycle_FinishOk_WhenCycleTimeInRange_BecomesFinishedOk()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.Started, PartStatus.None).Build();

        // Act — 100 is within (0, 216000)
        var result = cycle.FinishOk(Context(100, Recipe(0, 216000)));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
    }

    /// <summary>
    /// Test #5 — FinishOk with an out-of-range cycle time becomes FinishedNok (cycle-time override preserved, AC5).
    /// </summary>
    [Fact]
    public void Cycle_FinishOk_WhenCycleTimeOutOfRange_BecomesFinishedNok()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.Started, PartStatus.None).Build();

        // Act — 500000 exceeds the maximum 216000
        var result = cycle.FinishOk(Context(500000, Recipe(0, 216000)));

        // Assert
        result.IsSuccess.ShouldBeFalse();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        result.Value.ShouldNotBeNull().Value.ShouldBe(CycleStatus.FinishedNok.Value);
    }

    /// <summary>
    /// AC5 / NFR4 — FinishOk with a missing recipe forces the SAME FinishedNok / NOk override as out-of-range
    /// (as-built UpdateCyclesOkCommandHandler.cs:390-397 — IsCycleTimeInvalid is true for a null recipe), while
    /// the failure surfaces the more precise RecipeNotFound code. State must match as-built (Story 2.4 parity).
    /// </summary>
    [Fact]
    public void Cycle_FinishOk_WhenRecipeMissing_ForcesFinishedNok_WithRecipeNotFound()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.Started, PartStatus.None).Build();

        // Act
        var result = cycle.FinishOk(Context(100, recipe: null));

        // Assert — failure result, but cycle state forced to FinishedNok / NOk (as-built parity)
        result.IsSuccess.ShouldBeFalse();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
    }

    /// <summary>
    /// AC3 — FinishNok becomes FinishedNok / NOk.
    /// </summary>
    [Fact]
    public void Cycle_FinishNok_BecomesFinishedNok()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.Started, PartStatus.None).Build();

        // Act
        var result = cycle.FinishNok();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
    }

    /// <summary>
    /// AC3 — Reject becomes Rejected / Rejected.
    /// </summary>
    [Fact]
    public void Cycle_Reject_BecomesRejected()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.Started, PartStatus.None).Build();

        // Act
        var result = cycle.Reject();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Rejected.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.Rejected.Value);
    }

    /// <summary>
    /// #81 — Start is rejected (and the cycle is left UNMUTATED) when fired from an already-finished cycle
    /// (the issue's "Start a FinishedOk" case). Legal sources are None / NotStarted only.
    /// </summary>
    [Fact]
    public void Cycle_Start_FromFinishedOk_IsRejectedUnmutated()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.FinishedOk, PartStatus.Ok).Build();

        // Act
        var result = cycle.Start();

        // Assert — refused, state unchanged.
        result.IsSuccess.ShouldBeFalse();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
    }

    /// <summary>
    /// #81 — Reject is rejected (cycle UNMUTATED) when fired from a terminal Canceled cycle (the issue's
    /// "Reject a Canceled" case). Legal source is Started only.
    /// </summary>
    [Fact]
    public void Cycle_Reject_FromCanceled_IsRejectedUnmutated()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.Canceled, PartStatus.Scrap).Build();

        // Act
        var result = cycle.Reject();

        // Assert — refused, state unchanged.
        result.IsSuccess.ShouldBeFalse();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Canceled.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.Scrap.Value);
    }

    /// <summary>
    /// #81 — FinishOk is rejected (cycle UNMUTATED) when the cycle has already finished (not in Started).
    /// </summary>
    [Fact]
    public void Cycle_FinishOk_FromAlreadyFinished_IsRejectedUnmutated()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.FinishedNok, PartStatus.NOk).Build();

        // Act
        var result = cycle.FinishOk(Context(100, Recipe(0, 216000)));

        // Assert — refused, state unchanged (NOT promoted to FinishedOk).
        result.IsSuccess.ShouldBeFalse();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
    }

    /// <summary>
    /// #81 — FinishNok is rejected (cycle UNMUTATED) when fired from a terminal Rejected cycle.
    /// </summary>
    [Fact]
    public void Cycle_FinishNok_FromRejected_IsRejectedUnmutated()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.Rejected, PartStatus.Rejected).Build();

        // Act
        var result = cycle.FinishNok();

        // Assert — refused, state unchanged.
        result.IsSuccess.ShouldBeFalse();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Rejected.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.Rejected.Value);
    }

    /// <summary>
    /// #81 — Start from a NotStarted source remains legal (the sub-machine's NotStarted -> Started step).
    /// </summary>
    [Fact]
    public void Cycle_Start_FromNotStarted_IsAccepted()
    {
        // Arrange
        var cycle = new CycleBuilder().AtState(CycleStatus.NotStarted, PartStatus.None).Build();

        // Act
        var result = cycle.Start();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
    }
}
