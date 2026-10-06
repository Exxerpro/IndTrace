// <copyright file="PlcActiveStatusRoundTripTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Plcs.Commands;

/// <summary>
/// Verifies the <see cref="Plc.Enabled"/> EF value converter persists the tri-state
/// <see cref="ActiveStatus"/> as its underlying int (-1/0/1) and materializes it back, and that
/// the converter participates in query predicate translation (filtering by <see cref="ActiveStatus"/>).
/// Runs over the real EF Core (InMemory) database from <see cref="DependenciesFactory"/>.
/// </summary>
public class PlcActiveStatusRoundTripTest : DependenciesFactory
{
    public PlcActiveStatusRoundTripTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Saves a Plc for each ActiveStatus, reloads from a fresh context, and asserts the
    /// round-tripped underlying value and that a converter-based predicate filters correctly.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    [Fact]
    public async Task Enabled_ShouldRoundTripTriStateThroughConverter()
    {
        await Initialization;

        var cancellationToken = TestContext.Current.CancellationToken;

        // Distinct PlcIds keep these rows from colliding with the seeded fixture (PlcId 100).
        var active = NewPlc(995101, "RoundTrip_Active", ActiveStatus.Active);
        var none = NewPlc(995102, "RoundTrip_None", ActiveStatus.None);
        var inactive = NewPlc(995103, "RoundTrip_Inactive", ActiveStatus.Inactive);

        var repository = DpPlcRepository;
        (await repository.AddAsync(active, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(none, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(inactive, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.CommitAsync(cancellationToken)).IsSuccess.ShouldBeTrue();

        // Reload from a brand-new context so the values are re-materialized through the converter.
        var db = await DpIndTraceDbTestContextFactory.CreateDbContextAsync(cancellationToken);

        var reloadedActive = db.Set<Plc>().Single(p => p.PlcId == 995101);
        var reloadedNone = db.Set<Plc>().Single(p => p.PlcId == 995102);
        var reloadedInactive = db.Set<Plc>().Single(p => p.PlcId == 995103);

        // From-DB conversion: int column → ActiveStatus instance with the correct underlying value.
        reloadedActive.Enabled.ShouldNotBeNull();
        reloadedActive.Enabled.Value.ShouldBe(1);
        reloadedNone.Enabled.Value.ShouldBe(0);
        reloadedInactive.Enabled.Value.ShouldBe(-1);

        // Converter participates in predicate translation: filtering by the ActiveStatus member
        // matches on the underlying int value (1), not on reference identity.
        var activePlcIds = db.Set<Plc>()
            .Where(p => p.Enabled == ActiveStatus.Active)
            .Select(p => p.PlcId)
            .ToList();

        activePlcIds.ShouldContain(995101);
        activePlcIds.ShouldNotContain(995102);
        activePlcIds.ShouldNotContain(995103);

        var inactivePlcIds = db.Set<Plc>()
            .Where(p => p.Enabled == ActiveStatus.Inactive)
            .Select(p => p.PlcId)
            .ToList();

        inactivePlcIds.ShouldContain(995103);
        inactivePlcIds.ShouldNotContain(995101);
    }

    private static Plc NewPlc(int plcId, string name, ActiveStatus enabled) => Plc.CreateFixture(
        plcId: plcId,
        machineId: plcId,
        enabled: enabled,
        name: name,
        ipAddress: "192.168.0.1",
        plcType: "S7-1200",
        plcBrand: "Siemens",
        options: "[]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens");
}
