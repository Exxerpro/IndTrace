// <copyright file="MachineMemberStagingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.MachinesTests;

/// <summary>
/// Unit tests for the #95 Phase 2 Slice C Machine aggregate staging surface: the load-folds
/// (<c>AttachLoadedMachinePlcs</c> / <c>AttachLoadedSettings</c>), the staged-change seams
/// (<c>StageMachinePlcAppend</c> / <c>StageMachinePlcUpdate</c> / <c>StageSettingAppend</c> /
/// <c>StageSettingUpdate</c>) with their fail-closed guards, and the consumed-on-success semantics
/// (<c>ClearStagedMachineChanges</c> is the ONLY consumer of the staged sets). All guards answer with
/// <c>Result</c> failures — never throw.
/// </summary>
public class MachineMemberStagingTests
{
    private static Machine CreateMachine(int machineId = 42) => new() { MachineId = new MachineId(machineId) };

    private static MachinePlc CreateMachinePlc(int machineId, int plcId) =>
        new(machineId, plcId, ActiveStatus.Active);

    private static Setting CreateSetting(int machineId, int settingId = 0, string config = "cfg") =>
        new() { SettingId = settingId, MachineId = new MachineId(machineId), Config = config };

    /// <summary>
    /// A fresh machine exposes empty loaded/staged sets.
    /// </summary>
    [Fact]
    public void NewMachine_HasEmptyLoadedAndStagedSets()
    {
        var machine = CreateMachine();

        machine.LoadedMachinePlcs.ShouldBeEmpty();
        machine.PendingMachinePlcAppends.ShouldBeEmpty();
        machine.PendingMachinePlcUpdates.ShouldBeEmpty();
        machine.LoadedSettings.ShouldBeEmpty();
        machine.PendingSettingAppends.ShouldBeEmpty();
        machine.PendingSettingUpdates.ShouldBeEmpty();
    }

    /// <summary>
    /// AttachLoadedMachinePlcs REPLACES the prior loaded set (idempotent hydration) and treats null as empty.
    /// </summary>
    [Fact]
    public void AttachLoadedMachinePlcs_ReplacesPriorSet_AndTreatsNullAsEmpty()
    {
        var machine = CreateMachine();

        machine.AttachLoadedMachinePlcs([CreateMachinePlc(42, 1)]);
        machine.LoadedMachinePlcs.Count.ShouldBe(1);

        machine.AttachLoadedMachinePlcs([CreateMachinePlc(42, 2), CreateMachinePlc(42, 3)]);
        machine.LoadedMachinePlcs.Count.ShouldBe(2);

        machine.AttachLoadedMachinePlcs(null);
        machine.LoadedMachinePlcs.ShouldBeEmpty();
    }

    /// <summary>
    /// AttachLoadedSettings REPLACES the prior loaded set (idempotent hydration) and treats null as empty.
    /// </summary>
    [Fact]
    public void AttachLoadedSettings_ReplacesPriorSet_AndTreatsNullAsEmpty()
    {
        var machine = CreateMachine();

        machine.AttachLoadedSettings([CreateSetting(42, settingId: 1)]);
        machine.LoadedSettings.Count.ShouldBe(1);

        machine.AttachLoadedSettings([CreateSetting(42, settingId: 2), CreateSetting(42, settingId: 3)]);
        machine.LoadedSettings.Count.ShouldBe(2);

        machine.AttachLoadedSettings(null);
        machine.LoadedSettings.ShouldBeEmpty();
    }

    /// <summary>
    /// A matching-machine association stages cleanly and is exposed read-only via PendingMachinePlcAppends.
    /// </summary>
    [Fact]
    public void StageMachinePlcAppend_MatchingMachine_Stages()
    {
        var machine = CreateMachine(42);
        var machinePlc = CreateMachinePlc(machineId: 42, plcId: 7);

        var staged = machine.StageMachinePlcAppend(machinePlc);

        staged.IsSuccess.ShouldBeTrue();
        machine.PendingMachinePlcAppends.ShouldHaveSingleItem().ShouldBe(machinePlc);
    }

    /// <summary>
    /// An association belonging to ANOTHER machine is refused — an aggregate stages only its own members.
    /// </summary>
    [Fact]
    public void StageMachinePlcAppend_MismatchedMachine_FailsAndStagesNothing()
    {
        var machine = CreateMachine(42);
        var foreign = CreateMachinePlc(machineId: 43, plcId: 7);

        var staged = machine.StageMachinePlcAppend(foreign);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingMachinePlcAppends.ShouldBeEmpty();
    }

    /// <summary>
    /// A null association is refused as a Result failure (never-throw contract).
    /// </summary>
    [Fact]
    public void StageMachinePlcAppend_Null_FailsWithoutThrowing()
    {
        var machine = CreateMachine(42);
        MachinePlc? machinePlc = null;

        // The seam accepts MachinePlc? by contract (T? + guard, no null-forgiving operator needed).
        var staged = machine.StageMachinePlcAppend(machinePlc);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingMachinePlcAppends.ShouldBeEmpty();
    }

    /// <summary>
    /// A second append with the same composite key (MachineId, PlcId) is refused — a batch cannot
    /// double-attach one entity.
    /// </summary>
    [Fact]
    public void StageMachinePlcAppend_DuplicateCompositeKey_Fails()
    {
        var machine = CreateMachine(42);
        machine.StageMachinePlcAppend(CreateMachinePlc(42, plcId: 7)).IsSuccess.ShouldBeTrue();

        var duplicate = machine.StageMachinePlcAppend(CreateMachinePlc(42, plcId: 7));

        duplicate.IsFailure.ShouldBeTrue();
        machine.PendingMachinePlcAppends.Count.ShouldBe(1);
    }

    /// <summary>
    /// A matching-machine association stages for update (the composite key is always populated, so there is
    /// no separate persisted-identity guard).
    /// </summary>
    [Fact]
    public void StageMachinePlcUpdate_MatchingMachine_Stages()
    {
        var machine = CreateMachine(42);
        var machinePlc = CreateMachinePlc(machineId: 42, plcId: 7);

        var staged = machine.StageMachinePlcUpdate(machinePlc);

        staged.IsSuccess.ShouldBeTrue();
        machine.PendingMachinePlcUpdates.ShouldHaveSingleItem().ShouldBe(machinePlc);
    }

    /// <summary>
    /// A null update is refused as a Result failure (never-throw contract).
    /// </summary>
    [Fact]
    public void StageMachinePlcUpdate_Null_FailsWithoutThrowing()
    {
        var machine = CreateMachine(42);
        MachinePlc? machinePlc = null;

        var staged = machine.StageMachinePlcUpdate(machinePlc);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingMachinePlcUpdates.ShouldBeEmpty();
    }

    /// <summary>
    /// An update for another machine's association is refused.
    /// </summary>
    [Fact]
    public void StageMachinePlcUpdate_MismatchedMachine_Fails()
    {
        var machine = CreateMachine(42);
        var foreign = CreateMachinePlc(machineId: 43, plcId: 7);

        var staged = machine.StageMachinePlcUpdate(foreign);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingMachinePlcUpdates.ShouldBeEmpty();
    }

    /// <summary>
    /// An update whose composite key is already staged as an APPEND is refused — the same entity cannot be
    /// both inserted and updated in one batch.
    /// </summary>
    [Fact]
    public void StageMachinePlcUpdate_KeyAlreadyStagedAsAppend_Fails()
    {
        var machine = CreateMachine(42);
        machine.StageMachinePlcAppend(CreateMachinePlc(42, plcId: 7)).IsSuccess.ShouldBeTrue();

        var staged = machine.StageMachinePlcUpdate(CreateMachinePlc(42, plcId: 7));

        staged.IsFailure.ShouldBeTrue();
        machine.PendingMachinePlcUpdates.ShouldBeEmpty();
    }

    /// <summary>
    /// A second update with the same composite key is refused — a batch cannot double-attach one entity.
    /// </summary>
    [Fact]
    public void StageMachinePlcUpdate_DuplicateCompositeKey_Fails()
    {
        var machine = CreateMachine(42);
        machine.StageMachinePlcUpdate(CreateMachinePlc(42, plcId: 7)).IsSuccess.ShouldBeTrue();

        var duplicate = machine.StageMachinePlcUpdate(CreateMachinePlc(42, plcId: 7));

        duplicate.IsFailure.ShouldBeTrue();
        machine.PendingMachinePlcUpdates.Count.ShouldBe(1);
    }

    /// <summary>
    /// An unpersisted matching setting (SettingId 0) stages for append.
    /// </summary>
    [Fact]
    public void StageSettingAppend_MatchingMachineWithoutIdentity_Stages()
    {
        var machine = CreateMachine(42);
        var setting = CreateSetting(machineId: 42, settingId: 0);

        var staged = machine.StageSettingAppend(setting);

        staged.IsSuccess.ShouldBeTrue();
        machine.PendingSettingAppends.ShouldHaveSingleItem().ShouldBe(setting);
    }

    /// <summary>
    /// A null setting is refused as a Result failure (never-throw contract).
    /// </summary>
    [Fact]
    public void StageSettingAppend_Null_FailsWithoutThrowing()
    {
        var machine = CreateMachine(42);
        Setting? setting = null;

        var staged = machine.StageSettingAppend(setting);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingSettingAppends.ShouldBeEmpty();
    }

    /// <summary>
    /// A setting belonging to ANOTHER machine is refused for append.
    /// </summary>
    [Fact]
    public void StageSettingAppend_MismatchedMachine_Fails()
    {
        var machine = CreateMachine(42);
        var foreign = CreateSetting(machineId: 43, settingId: 0);

        var staged = machine.StageSettingAppend(foreign);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingSettingAppends.ShouldBeEmpty();
    }

    /// <summary>
    /// A setting that ALREADY carries a persisted identity cannot be staged for append — SettingId is a
    /// store-generated identity column, so appends require SettingId 0.
    /// </summary>
    [Fact]
    public void StageSettingAppend_PersistedIdentity_Fails()
    {
        var machine = CreateMachine(42);
        var persisted = CreateSetting(machineId: 42, settingId: 7);

        var staged = machine.StageSettingAppend(persisted);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingSettingAppends.ShouldBeEmpty();
    }

    /// <summary>
    /// A persisted matching setting stages for update.
    /// </summary>
    [Fact]
    public void StageSettingUpdate_PersistedMatchingSetting_Stages()
    {
        var machine = CreateMachine(42);
        var setting = CreateSetting(machineId: 42, settingId: 7);

        var staged = machine.StageSettingUpdate(setting);

        staged.IsSuccess.ShouldBeTrue();
        machine.PendingSettingUpdates.ShouldHaveSingleItem().ShouldBe(setting);
    }

    /// <summary>
    /// A null setting update is refused as a Result failure (never-throw contract).
    /// </summary>
    [Fact]
    public void StageSettingUpdate_Null_FailsWithoutThrowing()
    {
        var machine = CreateMachine(42);
        Setting? setting = null;

        var staged = machine.StageSettingUpdate(setting);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingSettingUpdates.ShouldBeEmpty();
    }

    /// <summary>
    /// An update for another machine's setting is refused.
    /// </summary>
    [Fact]
    public void StageSettingUpdate_MismatchedMachine_Fails()
    {
        var machine = CreateMachine(42);
        var foreign = CreateSetting(machineId: 43, settingId: 7);

        var staged = machine.StageSettingUpdate(foreign);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingSettingUpdates.ShouldBeEmpty();
    }

    /// <summary>
    /// An UNPERSISTED setting (SettingId &lt;= 0) cannot be staged for update — there is no row to update.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void StageSettingUpdate_UnpersistedSetting_Fails(int settingId)
    {
        var machine = CreateMachine(42);
        var unpersisted = CreateSetting(machineId: 42, settingId: settingId);

        var staged = machine.StageSettingUpdate(unpersisted);

        staged.IsFailure.ShouldBeTrue();
        machine.PendingSettingUpdates.ShouldBeEmpty();
    }

    /// <summary>
    /// A second update with the same SettingId is refused — a batch cannot double-attach one entity.
    /// </summary>
    [Fact]
    public void StageSettingUpdate_DuplicateIdentity_Fails()
    {
        var machine = CreateMachine(42);
        machine.StageSettingUpdate(CreateSetting(42, settingId: 7)).IsSuccess.ShouldBeTrue();

        var duplicate = machine.StageSettingUpdate(CreateSetting(42, settingId: 7));

        duplicate.IsFailure.ShouldBeTrue();
        machine.PendingSettingUpdates.Count.ShouldBe(1);
    }

    /// <summary>
    /// Domain half of the staging contract: staging NEVER clears — the staged sets accumulate across seam
    /// calls and survive REFUSED stage attempts. Only the explicit clear seam consumes them (the aggregate
    /// repository calls it after every save attempt — durable commit or failure — per the consumed-by-the-
    /// attempt contract).
    /// </summary>
    [Fact]
    public void StagedSets_SurviveUntilExplicitlyCleared()
    {
        var machine = CreateMachine(42);
        machine.StageMachinePlcAppend(CreateMachinePlc(42, plcId: 1)).IsSuccess.ShouldBeTrue();
        machine.StageMachinePlcUpdate(CreateMachinePlc(42, plcId: 2)).IsSuccess.ShouldBeTrue();
        machine.StageSettingAppend(CreateSetting(42, settingId: 0)).IsSuccess.ShouldBeTrue();
        machine.StageSettingUpdate(CreateSetting(42, settingId: 7)).IsSuccess.ShouldBeTrue();

        // A refused stage consumes nothing already staged.
        machine.StageSettingUpdate(CreateSetting(43, settingId: 8)).IsFailure.ShouldBeTrue();

        machine.PendingMachinePlcAppends.Count.ShouldBe(1);
        machine.PendingMachinePlcUpdates.Count.ShouldBe(1);
        machine.PendingSettingAppends.Count.ShouldBe(1);
        machine.PendingSettingUpdates.Count.ShouldBe(1);
    }

    /// <summary>
    /// Clear-after-save: all four staged sets empty, while the loaded folds are intentionally left untouched
    /// (they reflect the load-time snapshot; the next LoadAsync refreshes them).
    /// </summary>
    [Fact]
    public void ClearStagedMachineChanges_EmptiesStagedSets_KeepsLoadedFolds()
    {
        var machine = CreateMachine(42);
        machine.AttachLoadedMachinePlcs([CreateMachinePlc(42, 1)]);
        machine.AttachLoadedSettings([CreateSetting(42, settingId: 1)]);
        machine.StageMachinePlcAppend(CreateMachinePlc(42, plcId: 2)).IsSuccess.ShouldBeTrue();
        machine.StageMachinePlcUpdate(CreateMachinePlc(42, plcId: 1)).IsSuccess.ShouldBeTrue();
        machine.StageSettingAppend(CreateSetting(42, settingId: 0)).IsSuccess.ShouldBeTrue();
        machine.StageSettingUpdate(CreateSetting(42, settingId: 1)).IsSuccess.ShouldBeTrue();

        machine.ClearStagedMachineChanges();

        machine.PendingMachinePlcAppends.ShouldBeEmpty();
        machine.PendingMachinePlcUpdates.ShouldBeEmpty();
        machine.PendingSettingAppends.ShouldBeEmpty();
        machine.PendingSettingUpdates.ShouldBeEmpty();
        machine.LoadedMachinePlcs.Count.ShouldBe(1);
        machine.LoadedSettings.Count.ShouldBe(1);
    }
}
