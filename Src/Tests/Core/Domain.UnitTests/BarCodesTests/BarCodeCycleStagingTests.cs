// <copyright file="BarCodeCycleStagingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.BarCodesTests;

/// <summary>
/// Unit tests for the #95 Phase 2 Slice E BarCode aggregate member-write staging surface: the append seam
/// (<c>StageNewCycle</c> — the CycleCreator initial-INSERT migration), the status-update seam
/// (<c>StageCycleStatusUpdate</c> — the CancelCycle migration) with their fail-closed guards, and the
/// consumed-by-the-attempt contract (<c>ClearStagedCycleChanges</c> is the ONLY consumer of the two staged
/// sets — the repository calls it after EVERY save attempt, success AND failure, per PR #170). All guards
/// answer with <c>Result</c> failures — never throw.
/// </summary>
public class BarCodeCycleStagingTests
{
    private const int OwnBarCodeId = 900;
    private const int MachineId = 100;

    private static readonly DateTime FixedNow = new(2026, 7, 20, 8, 0, 0, DateTimeKind.Local);

    private static BarCode CreateRoot(int barCodeId = OwnBarCodeId)
    {
        var barCode = BarCode.Create($"STAGE-{barCodeId}", productId: 1, machineId: MachineId, FixedNow, FixedNow);
        barCode.BarCodeId = new BarCodeId(barCodeId);
        return barCode;
    }

    private static Cycle CreateNewCycle(int barCodeId = OwnBarCodeId) =>
        Cycle.CreateStarted(MachineId, barCodeId, cyclesOk: 0, FixedNow, FixedNow);

    private static Cycle CreatePersistedCycle(int cycleId, int barCodeId = OwnBarCodeId)
    {
        var cycle = Cycle.CreateStarted(MachineId, barCodeId, cyclesOk: 0, FixedNow, FixedNow);
        cycle.CycleId = new CycleId(cycleId);
        return cycle;
    }

    /// <summary>A fresh barcode exposes empty Slice E staged sets.</summary>
    [Fact]
    public void NewBarCode_HasEmptyStagedCycleSets()
    {
        var root = CreateRoot();

        root.PendingNewCycles.ShouldBeEmpty();
        root.PendingCycleUpdates.ShouldBeEmpty();
    }

    // =================================================================================================
    // StageNewCycle (append)
    // =================================================================================================

    /// <summary>A matching, identity-less cycle stages cleanly and is exposed via PendingNewCycles.</summary>
    [Fact]
    public void StageNewCycle_MatchingBarCode_Stages()
    {
        var root = CreateRoot();
        var cycle = CreateNewCycle();

        var staged = root.StageNewCycle(cycle);

        staged.IsSuccess.ShouldBeTrue();
        root.PendingNewCycles.Count.ShouldBe(1);
        root.PendingNewCycles[0].ShouldBeSameAs(cycle);
    }

    /// <summary>A null cycle is refused as a Result failure (never a throw) and stages nothing.</summary>
    [Fact]
    public void StageNewCycle_Null_FailsAndStagesNothing()
    {
        var root = CreateRoot();

        var staged = root.StageNewCycle(null);

        staged.IsFailure.ShouldBeTrue();
        staged.Errors.ShouldContain(e => e.Contains("null cycle"));
        root.PendingNewCycles.ShouldBeEmpty();
    }

    /// <summary>A cycle belonging to a different barcode is refused (aggregate-membership invariant).</summary>
    [Fact]
    public void StageNewCycle_ForeignBarCode_FailsAndStagesNothing()
    {
        var root = CreateRoot(OwnBarCodeId);
        var foreignCycle = CreateNewCycle(barCodeId: OwnBarCodeId + 1);

        var staged = root.StageNewCycle(foreignCycle);

        staged.IsFailure.ShouldBeTrue();
        staged.Errors.ShouldContain(e => e.Contains("must match this aggregate's barcode"));
        root.PendingNewCycles.ShouldBeEmpty();
    }

    /// <summary>
    /// A cycle already carrying a persisted identity is refused — CycleId is a store-generated identity
    /// column, so an append MUST carry CycleId 0.
    /// </summary>
    [Fact]
    public void StageNewCycle_PersistedIdentity_FailsAndStagesNothing()
    {
        var root = CreateRoot();
        var persisted = CreatePersistedCycle(cycleId: 777);

        var staged = root.StageNewCycle(persisted);

        staged.IsFailure.ShouldBeTrue();
        staged.Errors.ShouldContain(e => e.Contains("already carries a persisted identity"));
        root.PendingNewCycles.ShouldBeEmpty();
    }

    /// <summary>The same instance cannot be staged for append twice (no double-insert in one batch).</summary>
    [Fact]
    public void StageNewCycle_SameInstanceTwice_FailsSecondStage()
    {
        var root = CreateRoot();
        var cycle = CreateNewCycle();

        root.StageNewCycle(cycle).IsSuccess.ShouldBeTrue();
        var second = root.StageNewCycle(cycle);

        second.IsFailure.ShouldBeTrue();
        second.Errors.ShouldContain(e => e.Contains("already staged for append"));
        root.PendingNewCycles.Count.ShouldBe(1);
    }

    /// <summary>Two DISTINCT new cycles may be staged in one batch (the insert set composes).</summary>
    [Fact]
    public void StageNewCycle_TwoDistinctCycles_BothStage()
    {
        var root = CreateRoot();

        root.StageNewCycle(CreateNewCycle()).IsSuccess.ShouldBeTrue();
        root.StageNewCycle(CreateNewCycle()).IsSuccess.ShouldBeTrue();

        root.PendingNewCycles.Count.ShouldBe(2);
    }

    // =================================================================================================
    // StageCycleStatusUpdate (the CancelCycle path)
    // =================================================================================================

    /// <summary>
    /// A persisted, matching cycle stages cleanly: the already-resolved status is applied through the trusted
    /// seam (byte-equal to the retired inline handler write) and the cycle joins PendingCycleUpdates.
    /// </summary>
    [Fact]
    public void StageCycleStatusUpdate_MatchingPersistedCycle_AppliesStatusAndStages()
    {
        var root = CreateRoot();
        var cycle = CreatePersistedCycle(cycleId: 951);

        var staged = root.StageCycleStatusUpdate(cycle, CycleStatus.Canceled);

        staged.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Canceled.Value);
        root.PendingCycleUpdates.Count.ShouldBe(1);
        root.PendingCycleUpdates[0].ShouldBeSameAs(cycle);
    }

    /// <summary>A null cycle is refused; nothing staged, nothing mutated.</summary>
    [Fact]
    public void StageCycleStatusUpdate_NullCycle_FailsAndStagesNothing()
    {
        var root = CreateRoot();

        var staged = root.StageCycleStatusUpdate(null, CycleStatus.Canceled);

        staged.IsFailure.ShouldBeTrue();
        staged.Errors.ShouldContain(e => e.Contains("null cycle"));
        root.PendingCycleUpdates.ShouldBeEmpty();
    }

    /// <summary>A null resolved status is refused; the cycle's status stays unmutated.</summary>
    [Fact]
    public void StageCycleStatusUpdate_NullStatus_FailsWithoutMutating()
    {
        var root = CreateRoot();
        var cycle = CreatePersistedCycle(cycleId: 952);

        var staged = root.StageCycleStatusUpdate(cycle, null);

        staged.IsFailure.ShouldBeTrue();
        staged.Errors.ShouldContain(e => e.Contains("resolved cycle status is required"));
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        root.PendingCycleUpdates.ShouldBeEmpty();
    }

    /// <summary>A cycle belonging to a different barcode is refused WITHOUT mutating its status.</summary>
    [Fact]
    public void StageCycleStatusUpdate_ForeignBarCode_FailsWithoutMutating()
    {
        var root = CreateRoot(OwnBarCodeId);
        var foreignCycle = CreatePersistedCycle(cycleId: 953, barCodeId: OwnBarCodeId + 1);

        var staged = root.StageCycleStatusUpdate(foreignCycle, CycleStatus.Canceled);

        staged.IsFailure.ShouldBeTrue();
        staged.Errors.ShouldContain(e => e.Contains("must match this aggregate's barcode"));
        foreignCycle.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        root.PendingCycleUpdates.ShouldBeEmpty();
    }

    /// <summary>An identity-less cycle is refused — there is no persisted row to update.</summary>
    [Fact]
    public void StageCycleStatusUpdate_NoPersistedIdentity_FailsWithoutMutating()
    {
        var root = CreateRoot();
        var unpersisted = CreateNewCycle();

        var staged = root.StageCycleStatusUpdate(unpersisted, CycleStatus.Canceled);

        staged.IsFailure.ShouldBeTrue();
        staged.Errors.ShouldContain(e => e.Contains("no persisted identity"));
        unpersisted.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        root.PendingCycleUpdates.ShouldBeEmpty();
    }

    /// <summary>One entity cannot be both inserted and updated in one batch (append/update cross-guard).</summary>
    [Fact]
    public void StageCycleStatusUpdate_CycleStagedForAppend_Fails()
    {
        var root = CreateRoot();
        var cycle = CreateNewCycle();
        root.StageNewCycle(cycle).IsSuccess.ShouldBeTrue();

        var staged = root.StageCycleStatusUpdate(cycle, CycleStatus.Canceled);

        staged.IsFailure.ShouldBeTrue();
        staged.Errors.ShouldContain(e => e.Contains("already staged for append"));
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        root.PendingCycleUpdates.ShouldBeEmpty();
    }

    /// <summary>A duplicate status update for the same cycle identity is refused (no double-attach).</summary>
    [Fact]
    public void StageCycleStatusUpdate_DuplicateCycleId_FailsSecondStage()
    {
        var root = CreateRoot();
        var cycle = CreatePersistedCycle(cycleId: 954);

        root.StageCycleStatusUpdate(cycle, CycleStatus.Canceled).IsSuccess.ShouldBeTrue();
        var second = root.StageCycleStatusUpdate(cycle, CycleStatus.Canceled);

        second.IsFailure.ShouldBeTrue();
        second.Errors.ShouldContain(e => e.Contains("already staged"));
        root.PendingCycleUpdates.Count.ShouldBe(1);
    }

    // =================================================================================================
    // ClearStagedCycleChanges (the consumed-by-the-attempt contract, PR #170)
    // =================================================================================================

    /// <summary>
    /// ClearStagedCycleChanges empties BOTH Slice E staged sets — the repository invokes it after EVERY save
    /// attempt (success AND failure), so a later save can never double-apply a stale batch.
    /// </summary>
    [Fact]
    public void ClearStagedCycleChanges_EmptiesBothStagedSets()
    {
        var root = CreateRoot();
        root.StageNewCycle(CreateNewCycle()).IsSuccess.ShouldBeTrue();
        root.StageCycleStatusUpdate(CreatePersistedCycle(cycleId: 955), CycleStatus.Canceled).IsSuccess.ShouldBeTrue();

        root.ClearStagedCycleChanges();

        root.PendingNewCycles.ShouldBeEmpty();
        root.PendingCycleUpdates.ShouldBeEmpty();
    }

    /// <summary>The staging-reset contract: after a clear, the SAME members can be re-staged for a retry.</summary>
    [Fact]
    public void ClearStagedCycleChanges_AllowsRestagingForRetry()
    {
        var root = CreateRoot();
        var newCycle = CreateNewCycle();
        var persisted = CreatePersistedCycle(cycleId: 956);
        root.StageNewCycle(newCycle).IsSuccess.ShouldBeTrue();
        root.StageCycleStatusUpdate(persisted, CycleStatus.Canceled).IsSuccess.ShouldBeTrue();

        root.ClearStagedCycleChanges();

        root.StageNewCycle(newCycle).IsSuccess.ShouldBeTrue();
        root.StageCycleStatusUpdate(persisted, CycleStatus.Canceled).IsSuccess.ShouldBeTrue();
        root.PendingNewCycles.Count.ShouldBe(1);
        root.PendingCycleUpdates.Count.ShouldBe(1);
    }

    /// <summary>
    /// The Slice E clear seam does NOT touch the #40 completion staging surface (AppliedCycle /
    /// PendingRegisters / CompletionMarker keep their replace-on-next-apply lifecycle).
    /// </summary>
    [Fact]
    public void ClearStagedCycleChanges_DoesNotTouchCompletionStaging()
    {
        var root = CreateRoot();
        root.StageNewCycle(CreateNewCycle()).IsSuccess.ShouldBeTrue();

        root.ClearStagedCycleChanges();

        root.AppliedCycle.ShouldBeNull();
        root.PendingRegisters.ShouldBeEmpty();
        root.CompletionMarker.ShouldBeNull();
        root.LastCompletionWasIdempotentNoOp.ShouldBeFalse();
    }
}
