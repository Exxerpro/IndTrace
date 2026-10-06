// <copyright file="MachineAggregateRepositoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Machines.Services;

/// <summary>
/// Aggregation tests (real <see cref="MachineAggregateRepository"/> over EF-Core InMemory via
/// <see cref="DependenciesFactory"/>) for the #95 Phase 2 Slice C operation-scoped unit of work: a functional
/// round-trip of <c>LoadAsync → StageMachinePlcAppend/StageSettingAppend/StageMachinePlcUpdate/StageSettingUpdate
/// → SaveAsync → LoadAsync</c>.
/// </summary>
/// <remarks>
/// EF-Core InMemory ignores the explicit transaction (it returns a no-op transaction), so this proves the
/// FUNCTIONAL persistence path only — the aggregate loads the Machine root plus its MachinePlc and Setting
/// members, the staged appends/updates persist through the single-flush
/// <see cref="MachineAggregateRepository.SaveAsync"/>, and the staged sets are consumed by EVERY save attempt
/// (cleared after a durable commit AND discarded by a failed attempt — the PR #170 contract shared with
/// <see cref="ProductAggregateRepository"/>). Atomicity/conflict proofs belong to real SQL (the
/// <see cref="ProductRoutingRepository"/> precedent).
/// </remarks>
public class MachineAggregateRepositoryTests : DependenciesFactory
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineAggregateRepositoryTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public MachineAggregateRepositoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private MachineAggregateRepository CreateRepository() =>
        new(DpIndTraceDbContextFactory, XUnitLogger.CreateLogger<MachineAggregateRepository>(_outputHelper));

    private async Task SeedMachineAsync(int machineId, CancellationToken cancellationToken)
    {
        // Machine's key is caller-supplied (ValueGeneratedNever) — high ids stay clear of the fixture machines.
        var machine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = $"AggMachine-{machineId}",
            Description = "Slice C aggregate test machine",
            Location = "TEST",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
        };

        await DpMachineRepository.AddAsync(machine, cancellationToken);
        await DpMachineRepository.CommitAsync(cancellationToken);
    }

    private static MachinePlc CreateMachinePlcFor(int machineId, int plcId) =>
        new(machineId, plcId, ActiveStatus.Active);

    private static Setting CreateSettingFor(int machineId, string config) =>
        new() { MachineId = new MachineId(machineId), Config = config };

    /// <summary>
    /// Loading an id with no Machine row is a Result failure (never a null-success, never a throw).
    /// </summary>
    [Fact]
    public async Task LoadAsync_UnknownMachine_ReturnsFailure()
    {
        await Initialization;
        var repository = CreateRepository();

        var loaded = await repository.LoadAsync(9600000, AggregateLoadOptions.Full, TestContext.Current.CancellationToken);

        loaded.IsFailure.ShouldBeTrue();
        loaded.Errors.ShouldContain(e => e.Contains("was not found"));
    }

    /// <summary>
    /// An already-canceled token short-circuits to a Result failure before any database work (never throws).
    /// </summary>
    [Fact]
    public async Task LoadAsync_CanceledToken_ReturnsFailure()
    {
        await Initialization;
        var repository = CreateRepository();
        var canceled = new CancellationToken(canceled: true);

        var loaded = await repository.LoadAsync(9600001, AggregateLoadOptions.Full, canceled);

        loaded.IsFailure.ShouldBeTrue();
        loaded.Errors.ShouldContain(e => e.Contains("canceled"));
    }

    /// <summary>
    /// Null load options are refused as a Result failure (never throws).
    /// </summary>
    [Fact]
    public async Task LoadAsync_NullOptions_ReturnsFailure()
    {
        await Initialization;
        var repository = CreateRepository();

        // NRT array-hole: a fresh array of a non-nullable reference type carries REAL null elements with a
        // non-nullable static type — exercising the null guard without the banned null-forgiving operator.
        var nullOptions = new AggregateLoadOptions[1];

        var loaded = await repository.LoadAsync(9600002, nullOptions[0], TestContext.Current.CancellationToken);

        loaded.IsFailure.ShouldBeTrue();
        loaded.Errors.ShouldContain(e => e.Contains("load options"));
    }

    /// <summary>
    /// A known machine loads with BOTH member sets folded onto the root (all MachinePlc rows and all Setting
    /// rows for the machine).
    /// </summary>
    [Fact]
    public async Task LoadAsync_KnownMachine_FoldsBothMemberSets()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int machineId = 9600010;
        await SeedMachineAsync(machineId, ct);

        // Seed one member of each type directly (the load path must not depend on the save path under test).
        await DpMachinePlcRepository.AddAsync(CreateMachinePlcFor(machineId, plcId: 1), ct);
        await DpMachinePlcRepository.CommitAsync(ct);
        await DpSettingRepository.AddAsync(CreateSettingFor(machineId, "seeded-config"), ct);
        await DpSettingRepository.CommitAsync(ct);

        var loaded = await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct);

        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();
        root.MachineId.Value.ShouldBe(machineId);
        root.LoadedMachinePlcs.ShouldHaveSingleItem().PlcId.ShouldBe(1);
        root.LoadedSettings.ShouldHaveSingleItem().Config.ShouldBe("seeded-config");
        root.PendingMachinePlcAppends.ShouldBeEmpty();
        root.PendingSettingAppends.ShouldBeEmpty();
    }

    /// <summary>
    /// The append round-trip for MachinePlc members: load an (empty) aggregate, stage two appends, save
    /// through the single transactional path, and prove a fresh load folds exactly those rows back onto the
    /// root — with the staged sets cleared after the durable commit.
    /// </summary>
    [Fact]
    public async Task LoadStageSave_AppendedMachinePlcs_RoundTripOntoTheRoot()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int machineId = 9600020;
        await SeedMachineAsync(machineId, ct);

        var loaded = await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();
        root.LoadedMachinePlcs.ShouldBeEmpty();

        root.StageMachinePlcAppend(CreateMachinePlcFor(machineId, plcId: 1)).IsSuccess.ShouldBeTrue();
        root.StageMachinePlcAppend(CreateMachinePlcFor(machineId, plcId: 2)).IsSuccess.ShouldBeTrue();
        root.PendingMachinePlcAppends.Count.ShouldBe(2);

        var saved = await repository.SaveAsync(root, ct);
        saved.IsSuccess.ShouldBeTrue();

        // Clear-after-save: a repeated save cannot double-apply.
        root.PendingMachinePlcAppends.ShouldBeEmpty();
        root.PendingMachinePlcUpdates.ShouldBeEmpty();

        var reloaded = await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct);
        reloaded.IsSuccess.ShouldBeTrue();
        var reloadedRoot = reloaded.Value.ShouldNotBeNull();
        reloadedRoot.LoadedMachinePlcs.Count.ShouldBe(2);
        reloadedRoot.LoadedMachinePlcs.ShouldAllBe(p => p.MachineId == new MachineId(machineId));
        reloadedRoot.LoadedMachinePlcs.ShouldContain(p => p.PlcId == 1);
        reloadedRoot.LoadedMachinePlcs.ShouldContain(p => p.PlcId == 2);
    }

    /// <summary>
    /// The append round-trip for Setting members: staged appends persist with store-assigned identities and
    /// the staged sets clear after the durable commit.
    /// </summary>
    [Fact]
    public async Task LoadStageSave_AppendedSettings_RoundTripOntoTheRoot()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int machineId = 9600030;
        await SeedMachineAsync(machineId, ct);

        var loaded = await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();
        root.LoadedSettings.ShouldBeEmpty();

        root.StageSettingAppend(CreateSettingFor(machineId, "config-a")).IsSuccess.ShouldBeTrue();
        root.StageSettingAppend(CreateSettingFor(machineId, "config-b")).IsSuccess.ShouldBeTrue();
        root.PendingSettingAppends.Count.ShouldBe(2);

        var saved = await repository.SaveAsync(root, ct);
        saved.IsSuccess.ShouldBeTrue();

        root.PendingSettingAppends.ShouldBeEmpty();
        root.PendingSettingUpdates.ShouldBeEmpty();

        var reloaded = await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct);
        reloaded.IsSuccess.ShouldBeTrue();
        var reloadedRoot = reloaded.Value.ShouldNotBeNull();
        reloadedRoot.LoadedSettings.Count.ShouldBe(2);
        reloadedRoot.LoadedSettings.ShouldAllBe(s => s.MachineId == new MachineId(machineId));
        reloadedRoot.LoadedSettings.ShouldAllBe(s => s.SettingId > 0);
        reloadedRoot.LoadedSettings.ShouldContain(s => s.Config == "config-a");
        reloadedRoot.LoadedSettings.ShouldContain(s => s.Config == "config-b");
    }

    /// <summary>
    /// The in-place update round-trip for BOTH member types: loaded members are mutated, staged as updates,
    /// saved through the single flush, and a fresh load observes the new values.
    /// </summary>
    [Fact]
    public async Task LoadStageSave_UpdatedMembers_PersistTheNewValues()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int machineId = 9600040;
        await SeedMachineAsync(machineId, ct);

        // Seed one member of each type through the aggregate itself.
        var seed = (await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();
        seed.StageMachinePlcAppend(CreateMachinePlcFor(machineId, plcId: 1)).IsSuccess.ShouldBeTrue();
        seed.StageSettingAppend(CreateSettingFor(machineId, "original")).IsSuccess.ShouldBeTrue();
        (await repository.SaveAsync(seed, ct)).IsSuccess.ShouldBeTrue();

        // Reload, mutate the loaded members, stage both updates.
        var working = (await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();
        var machinePlc = working.LoadedMachinePlcs.ShouldHaveSingleItem();
        machinePlc.SetActiveStatus(ActiveStatus.Inactive).IsSuccess.ShouldBeTrue();
        working.StageMachinePlcUpdate(machinePlc).IsSuccess.ShouldBeTrue();

        var setting = working.LoadedSettings.ShouldHaveSingleItem();
        setting.Config = "updated";
        working.StageSettingUpdate(setting).IsSuccess.ShouldBeTrue();

        var saved = await repository.SaveAsync(working, ct);
        saved.IsSuccess.ShouldBeTrue();
        working.PendingMachinePlcUpdates.ShouldBeEmpty();
        working.PendingSettingUpdates.ShouldBeEmpty();

        // A fresh load observes the persisted new values.
        var reloaded = (await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();
        reloaded.LoadedMachinePlcs.ShouldHaveSingleItem().IsActive.ShouldBe(ActiveStatus.Inactive);
        reloaded.LoadedSettings.ShouldHaveSingleItem().Config.ShouldBe("updated");
    }

    /// <summary>
    /// A mixed batch — appends of both member types AND updates of both member types — persists through ONE
    /// save call.
    /// </summary>
    [Fact]
    public async Task SaveAsync_MixedAppendAndUpdateBatch_PersistsEverything()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int machineId = 9600050;
        await SeedMachineAsync(machineId, ct);

        // Seed one member of each type through the aggregate.
        var seed = (await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();
        seed.StageMachinePlcAppend(CreateMachinePlcFor(machineId, plcId: 1)).IsSuccess.ShouldBeTrue();
        seed.StageSettingAppend(CreateSettingFor(machineId, "original")).IsSuccess.ShouldBeTrue();
        (await repository.SaveAsync(seed, ct)).IsSuccess.ShouldBeTrue();

        // One batch: update the existing members AND append one more of each type.
        var working = (await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();
        var existingPlc = working.LoadedMachinePlcs.ShouldHaveSingleItem();
        existingPlc.SetActiveStatus(ActiveStatus.Inactive).IsSuccess.ShouldBeTrue();
        working.StageMachinePlcUpdate(existingPlc).IsSuccess.ShouldBeTrue();

        var existingSetting = working.LoadedSettings.ShouldHaveSingleItem();
        existingSetting.Config = "updated";
        working.StageSettingUpdate(existingSetting).IsSuccess.ShouldBeTrue();

        working.StageMachinePlcAppend(CreateMachinePlcFor(machineId, plcId: 2)).IsSuccess.ShouldBeTrue();
        working.StageSettingAppend(CreateSettingFor(machineId, "appended")).IsSuccess.ShouldBeTrue();

        var saved = await repository.SaveAsync(working, ct);
        saved.IsSuccess.ShouldBeTrue();
        working.PendingMachinePlcAppends.ShouldBeEmpty();
        working.PendingMachinePlcUpdates.ShouldBeEmpty();
        working.PendingSettingAppends.ShouldBeEmpty();
        working.PendingSettingUpdates.ShouldBeEmpty();

        // Every effect of the batch is visible on a fresh load.
        var reloaded = (await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();
        reloaded.LoadedMachinePlcs.Count.ShouldBe(2);
        reloaded.LoadedMachinePlcs.Single(p => p.PlcId == 1).IsActive.ShouldBe(ActiveStatus.Inactive);
        reloaded.LoadedMachinePlcs.Single(p => p.PlcId == 2).IsActive.ShouldBe(ActiveStatus.Active);
        reloaded.LoadedSettings.Count.ShouldBe(2);
        reloaded.LoadedSettings.ShouldContain(s => s.Config == "updated");
        reloaded.LoadedSettings.ShouldContain(s => s.Config == "appended");
    }

    /// <summary>
    /// PR #170 adversarial-review contract (shared with <see cref="ProductAggregateRepository"/>): a FAILED
    /// save attempt DISCARDS the staged sets (they are consumed by the attempt), so a later save of the same
    /// root cannot silently double-apply a stale batch — the caller must re-load and re-stage to retry.
    /// Proven via a staged update whose row does not exist in the store (the InMemory provider surfaces it as
    /// the same DbUpdateConcurrencyException a vanished row raises on real SQL).
    /// </summary>
    [Fact]
    public async Task SaveAsync_FailedAttempt_DiscardsStagedChanges()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int machineId = 9600060;
        await SeedMachineAsync(machineId, ct);

        var root = (await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();

        // A structurally-valid batch that cannot be applied: the update targets a row that was never
        // persisted (a competing writer could equally have deleted it), alongside an innocent append.
        root.StageMachinePlcUpdate(CreateMachinePlcFor(machineId, plcId: 999)).IsSuccess.ShouldBeTrue();
        root.StageSettingAppend(CreateSettingFor(machineId, "hostage")).IsSuccess.ShouldBeTrue();

        var conflicted = await repository.SaveAsync(root, ct);

        conflicted.IsFailure.ShouldBeTrue();

        // ...and the failed attempt consumed the staged sets — nothing left to double-apply.
        root.PendingMachinePlcAppends.ShouldBeEmpty();
        root.PendingMachinePlcUpdates.ShouldBeEmpty();
        root.PendingSettingAppends.ShouldBeEmpty();
        root.PendingSettingUpdates.ShouldBeEmpty();

        // A repeated save on the same root is now a harmless no-op success.
        (await repository.SaveAsync(root, ct)).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// A save with nothing staged is an idempotent no-op success (nothing to write; the DB is already correct).
    /// </summary>
    [Fact]
    public async Task SaveAsync_NothingStaged_IsIdempotentNoOpSuccess()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        const int machineId = 9600070;
        await SeedMachineAsync(machineId, ct);

        var root = (await repository.LoadAsync(machineId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();

        var saved = await repository.SaveAsync(root, ct);

        saved.IsSuccess.ShouldBeTrue();
    }
}
