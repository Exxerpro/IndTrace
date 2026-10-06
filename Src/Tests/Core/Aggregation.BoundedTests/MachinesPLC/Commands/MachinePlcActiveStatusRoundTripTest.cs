// <copyright file="MachinePlcActiveStatusRoundTripTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.MachinesPLC.Commands;

/// <summary>
/// Verifies the <see cref="MachinePlc.IsActive"/> EF value converter persists the tri-state
/// <see cref="ActiveStatus"/> as its underlying int (-1/0/1) and materializes it back, and that
/// the converter participates in query predicate translation (filtering by <see cref="ActiveStatus"/>).
/// Runs over the real EF Core (InMemory) database from <see cref="DependenciesFactory"/>.
/// </summary>
public class MachinePlcActiveStatusRoundTripTest : DependenciesFactory
{
    public MachinePlcActiveStatusRoundTripTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Saves a MachinePlc for each ActiveStatus, reloads from a fresh context, and asserts the
    /// round-tripped underlying value and that a converter-based predicate filters correctly.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    [Fact]
    public async Task IsActive_ShouldRoundTripTriStateThroughConverter()
    {
        await Initialization;

        var cancellationToken = TestContext.Current.CancellationToken;

        // Unique composite keys that do not collide with seeded fixtures.
        const int plcId = 77;
        var active = new MachinePlc(990001, plcId, ActiveStatus.Active);
        var none = new MachinePlc(990002, plcId, ActiveStatus.None);
        var inactive = new MachinePlc(990003, plcId, ActiveStatus.Inactive);

        var repository = DpMachinePlcRepository;
        (await repository.AddAsync(active, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(none, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(inactive, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.CommitAsync(cancellationToken)).IsSuccess.ShouldBeTrue();

        // Reload from a brand-new context so the values are re-materialized through the converter.
        var db = await DpIndTraceDbTestContextFactory.CreateDbContextAsync(cancellationToken);

        var reloadedActive = db.Set<MachinePlc>().Single(mp => mp.MachineId == new MachineId(990001) && mp.PlcId == plcId);
        var reloadedNone = db.Set<MachinePlc>().Single(mp => mp.MachineId == new MachineId(990002) && mp.PlcId == plcId);
        var reloadedInactive = db.Set<MachinePlc>().Single(mp => mp.MachineId == new MachineId(990003) && mp.PlcId == plcId);

        // From-DB conversion: int column → ActiveStatus instance with the correct underlying value.
        reloadedActive.IsActive.ShouldNotBeNull();
        reloadedActive.IsActive.Value.ShouldBe(1);
        reloadedNone.IsActive.Value.ShouldBe(0);
        reloadedInactive.IsActive.Value.ShouldBe(-1);

        // Converter participates in predicate translation: filtering by the ActiveStatus member
        // matches on the underlying int value (1), not on reference identity.
        var activeMachineIds = db.Set<MachinePlc>()
            .Where(mp => mp.IsActive == ActiveStatus.Active)
            .Select(mp => mp.MachineId)
            .ToList();

        activeMachineIds.ShouldContain(new MachineId(990001));
        activeMachineIds.ShouldNotContain(new MachineId(990002));
        activeMachineIds.ShouldNotContain(new MachineId(990003));

        var inactiveMachineIds = db.Set<MachinePlc>()
            .Where(mp => mp.IsActive == ActiveStatus.Inactive)
            .Select(mp => mp.MachineId)
            .ToList();

        inactiveMachineIds.ShouldContain(new MachineId(990003));
        inactiveMachineIds.ShouldNotContain(new MachineId(990001));
    }
}
