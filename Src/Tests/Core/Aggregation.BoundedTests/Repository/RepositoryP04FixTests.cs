// <copyright file="RepositoryP04FixTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Repository;

/// <summary>
/// Aggregation tests (real EF Core InMemory) covering the P0-4 (#64) persistence-layer hardening:
/// CommitAsync honesty, inline-save semantics, composite-key updates, idempotent (zero-net-change) updates,
/// ListAsync honouring the specification's tracking flag, the fail-loud tracking toggles, DetachAsync guards,
/// and the composite-key resolver returning every key component.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="RepositoryP04FixTests"/> class.</remarks>
public class RepositoryP04FixTests(ITestOutputHelper outputHelper) : DependenciesFactory(outputHelper)
{
    // Composite-key space chosen high to avoid collision with MachinePlcRawData.Fixture. InMemory does not enforce
    // the FK to Machine/Plc, so these rows can stand alone.
    private const int BaseMachineId = 9100;

    /// <summary>
    /// P0-4 defect #1: a mutating operation persists inline (its own context saves + disposes), and the retained
    /// <c>CommitAsync</c> compatibility no-op reports Success without pretending to be a commit gate.
    /// </summary>
    [Fact]
    public async Task AddAsync_PersistsInline_And_CommitAsync_ReturnsSuccess()
    {
        var ct = TestContext.Current.CancellationToken;
        var entity = new MachinePlc(BaseMachineId + 1, 1, ActiveStatus.Active);

        var addResult = await DpMachinePlcRepository.AddAsync(entity, ct);
        addResult.IsSuccess.ShouldBeTrue();

        // Proof of inline persistence: a *separate* repository call (its own fresh context) sees the row even though
        // CommitAsync was never called.
        var beforeCommit = await ReadMachinePlcAsync(BaseMachineId + 1, 1, ct);
        beforeCommit.ShouldNotBeNull();

        // CommitAsync is an honest no-op under the stateless-per-operation design: it returns Success but is not a gate.
        var commitResult = await DpMachinePlcRepository.CommitAsync(ct);
        commitResult.IsSuccess.ShouldBeTrue();

        var afterCommit = await ReadMachinePlcAsync(BaseMachineId + 1, 1, ct);
        afterCommit.ShouldNotBeNull();
    }

    /// <summary>
    /// P0-4 defect #5: an Update on a composite-key entity must target the row identified by ALL key components.
    /// Two rows share the same MachineId but differ by PlcId; updating one must not touch the other. With the previous
    /// first-key-only resolver, FindAsync received the wrong number of key values and the update path failed/mismatched.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_CompositeKey_UpdatesOnlyTargetRow()
    {
        var ct = TestContext.Current.CancellationToken;
        var machineId = BaseMachineId + 2;

        (await DpMachinePlcRepository.AddAsync(new MachinePlc(machineId, 1, ActiveStatus.Active), ct)).IsSuccess.ShouldBeTrue();
        (await DpMachinePlcRepository.AddAsync(new MachinePlc(machineId, 2, ActiveStatus.Active), ct)).IsSuccess.ShouldBeTrue();

        // Fetch the row for (machineId, 2), mutate it via the guarded seam, and update — mirroring the real handler
        // so audit fields are preserved.
        var toUpdate = await ReadMachinePlcAsync(machineId, 2, ct);
        toUpdate.ShouldNotBeNull();
        toUpdate!.SetActiveStatus(ActiveStatus.Inactive).IsSuccess.ShouldBeTrue();

        var updateResult = await DpMachinePlcRepository.UpdateAsync(toUpdate, ct);
        updateResult.IsSuccess.ShouldBeTrue();

        var target = await ReadMachinePlcAsync(machineId, 2, ct);
        var sibling = await ReadMachinePlcAsync(machineId, 1, ct);

        target.ShouldNotBeNull();
        sibling.ShouldNotBeNull();
        target!.IsActive.ShouldBe(ActiveStatus.Inactive);   // target changed
        sibling!.IsActive.ShouldBe(ActiveStatus.Active);     // sibling (same MachineId, different PlcId) untouched
    }

    /// <summary>
    /// P0-4 defect #6: an idempotent re-send (updating a row to the values it already has) must NOT be misreported as
    /// a concurrency failure. A genuine optimistic-concurrency conflict surfaces as an exception, not a silent 0-row.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_IdempotentResend_ReturnsSuccess()
    {
        var ct = TestContext.Current.CancellationToken;
        var machineId = BaseMachineId + 3;

        (await DpMachinePlcRepository.AddAsync(new MachinePlc(machineId, 1, ActiveStatus.Active), ct)).IsSuccess.ShouldBeTrue();

        // First update to Inactive (fetch-mutate-update, preserving audit fields).
        var first = await ReadMachinePlcAsync(machineId, 1, ct);
        first.ShouldNotBeNull();
        first!.SetActiveStatus(ActiveStatus.Inactive).IsSuccess.ShouldBeTrue();
        (await DpMachinePlcRepository.UpdateAsync(first, ct)).IsSuccess.ShouldBeTrue();

        // Re-send the SAME (already-Inactive) values: EF detects no net change (0 rows), which must be reported as a
        // success — NOT a concurrency failure.
        var again = await ReadMachinePlcAsync(machineId, 1, ct);
        again.ShouldNotBeNull();
        again!.SetActiveStatus(ActiveStatus.Inactive).IsSuccess.ShouldBeTrue();
        var resend = await DpMachinePlcRepository.UpdateAsync(again, ct);
        resend.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// P0-4 defect #4: ListAsync must honour the specification's tracking flag rather than force-appending
    /// AsNoTracking. Both a tracking spec (default) and a no-tracking spec must return the correct filtered rows.
    /// </summary>
    [Fact]
    public async Task ListAsync_HonoursSpecificationTracking_ReturnsCorrectData()
    {
        var ct = TestContext.Current.CancellationToken;
        var machineId = BaseMachineId + 4;

        (await DpMachinePlcRepository.AddAsync(new MachinePlc(machineId, 7, ActiveStatus.Active), ct)).IsSuccess.ShouldBeTrue();

        // Default specification: IsTracking == true (previously silently overridden to no-tracking).
        var trackingSpec = new Specification<MachinePlc>(mp => mp.MachineId == new MachineId(machineId) && mp.PlcId == 7);
        trackingSpec.IsTracking.ShouldBeTrue();
        var tracked = await DpMachinePlcRepository.ListAsync(trackingSpec, ct);
        tracked.IsSuccess.ShouldBeTrue();
        tracked.Value.ShouldNotBeNull();
        tracked.Value.ShouldNotBeNull().Count().ShouldBe(1);

        // No-tracking specification must also return the same row.
        var noTrackingSpec = new Specification<MachinePlc>(mp => mp.MachineId == new MachineId(machineId) && mp.PlcId == 7);
        noTrackingSpec.ApplyNoTracking();
        noTrackingSpec.IsTracking.ShouldBeFalse();
        var untracked = await DpMachinePlcRepository.ListAsync(noTrackingSpec, ct);
        untracked.IsSuccess.ShouldBeTrue();
        untracked.Value.ShouldNotBeNull();
        untracked.Value.ShouldNotBeNull().Count().ShouldBe(1);
    }

    /// <summary>
    /// P0-4 defect #3: the repository-wide tracking-mode toggles cannot work under the stateless-per-operation design
    /// (they previously mutated a throwaway context). They now fail loud instead of returning a deceptive Success.
    /// </summary>
    [Fact]
    public async Task TrackingToggles_FailLoud()
    {
        var ct = TestContext.Current.CancellationToken;

        var noTracking = await DpMachinePlcRepository.ApplyNoTrackingAsync(ct);
        noTracking.IsFailure.ShouldBeTrue();
        noTracking.Errors.ShouldNotBeEmpty();

        var tracking = await DpMachinePlcRepository.ApplyTrackingAsync(ct);
        tracking.IsFailure.ShouldBeTrue();
        tracking.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// P0-4 defect #3: DetachAsync is an idempotent no-op (Success) for a valid entity — the entity is not tracked by
    /// any live context — while preserving the null and cancellation guards.
    /// </summary>
    [Fact]
    public async Task DetachAsync_IdempotentSuccess_And_Guards()
    {
        var ct = TestContext.Current.CancellationToken;

        var ok = await DpMachinePlcRepository.DetachAsync(new MachinePlc(BaseMachineId + 5, 1, ActiveStatus.Active), ct);
        ok.IsSuccess.ShouldBeTrue();

        var nullGuard = await DpMachinePlcRepository.DetachAsync((MachinePlc)null!, ct);
        nullGuard.IsFailure.ShouldBeTrue();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var cancelGuard = await DpMachinePlcRepository.DetachAsync(new MachinePlc(BaseMachineId + 5, 1, ActiveStatus.Active), cts.Token);
        cancelGuard.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// P0-4 defect #5 (resolver): the key resolver returns EVERY primary-key component for a composite key, in model
    /// order, so the update path can build the full FindAsync argument array.
    /// </summary>
    [Fact]
    public void GetPrimaryKeyValues_CompositeKey_ReturnsAllComponents()
    {
        var entity = new MachinePlc(4242, 77, ActiveStatus.Active);

        var result = EntityKeyResolver.GetPrimaryKeyValues(entity, DpIndTraceContext);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().Length.ShouldBe(2); // MachineId + PlcId
        result.Value.ShouldNotBeNull()[0].ShouldBe(new MachineId(4242));
        result.Value.ShouldNotBeNull()[1].ShouldBe(77);
    }

    private async Task<MachinePlc?> ReadMachinePlcAsync(int machineId, int plcId, CancellationToken ct)
    {
        var spec = new Specification<MachinePlc>(mp => mp.MachineId == new MachineId(machineId) && mp.PlcId == plcId);
        spec.ApplyNoTracking();
        var result = await DpMachinePlcRepository.ListAsync(spec, ct);
        return result.IsSuccess && result.Value is not null ? result.Value.FirstOrDefault() : null;
    }
}
