// <copyright file="VariableActiveStatusRoundTripTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Variables.Commands;

/// <summary>
/// Verifies the <see cref="Variable.IsActive"/> EF value converter persists the tri-state
/// <see cref="ActiveStatus"/> as its underlying int (-1/0/1) and materializes it back, and that
/// the converter participates in query predicate translation (filtering by <see cref="ActiveStatus"/>).
/// Runs over the real EF Core (InMemory) database from <see cref="DependenciesFactory"/>.
/// </summary>
public class VariableActiveStatusRoundTripTest : DependenciesFactory
{
    public VariableActiveStatusRoundTripTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Saves a Variable for each ActiveStatus, reloads from a fresh context, and asserts the
    /// round-tripped underlying value and that a converter-based predicate filters correctly.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    [Fact]
    public async Task IsActive_ShouldRoundTripTriStateThroughConverter()
    {
        await Initialization;

        var cancellationToken = TestContext.Current.CancellationToken;

        // Distinct identifying fields keep the (MachineId, PlcId, Name, Address, VariableGroupId)
        // unique index from colliding with seeded fixtures or with each other.
        var active = NewVariable(995001, "RoundTrip_Active", ActiveStatus.Active);
        var none = NewVariable(995002, "RoundTrip_None", ActiveStatus.None);
        var inactive = NewVariable(995003, "RoundTrip_Inactive", ActiveStatus.Inactive);

        var repository = DpVariablesRepository;
        (await repository.AddAsync(active, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(none, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(inactive, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.CommitAsync(cancellationToken)).IsSuccess.ShouldBeTrue();

        // Reload from a brand-new context so the values are re-materialized through the converter.
        var db = await DpIndTraceDbTestContextFactory.CreateDbContextAsync(cancellationToken);

        var reloadedActive = db.Set<Variable>().Single(v => v.MachineId == 995001);
        var reloadedNone = db.Set<Variable>().Single(v => v.MachineId == 995002);
        var reloadedInactive = db.Set<Variable>().Single(v => v.MachineId == 995003);

        // From-DB conversion: int column → ActiveStatus instance with the correct underlying value.
        reloadedActive.IsActive.ShouldNotBeNull();
        reloadedActive.IsActive.Value.ShouldBe(1);
        reloadedNone.IsActive.Value.ShouldBe(0);
        reloadedInactive.IsActive.Value.ShouldBe(-1);

        // Converter participates in predicate translation: filtering by the ActiveStatus member
        // matches on the underlying int value (1), not on reference identity.
        var activeMachineIds = db.Set<Variable>()
            .Where(v => v.IsActive == ActiveStatus.Active)
            .Select(v => v.MachineId)
            .ToList();

        activeMachineIds.ShouldContain(995001);
        activeMachineIds.ShouldNotContain(995002);
        activeMachineIds.ShouldNotContain(995003);

        var inactiveMachineIds = db.Set<Variable>()
            .Where(v => v.IsActive == ActiveStatus.Inactive)
            .Select(v => v.MachineId)
            .ToList();

        inactiveMachineIds.ShouldContain(995003);
        inactiveMachineIds.ShouldNotContain(995001);
    }

    /// <summary>
    /// Proves the read-normalizing from-DB converter maps any positive stored int (e.g. <c>5</c>) — which the
    /// write contract permits (CreateVariableValidator: "Event must be 0 or greater") — to
    /// <see cref="ActiveStatus.Active"/>, maps negatives to <see cref="ActiveStatus.Inactive"/> and zero to
    /// <see cref="ActiveStatus.None"/>, never producing the Invalid sentinel.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    [Fact]
    public async Task IsActive_FromDbConverter_ShouldNormalizePositiveToActive()
    {
        await Initialization;

        var cancellationToken = TestContext.Current.CancellationToken;
        var db = await DpIndTraceDbTestContextFactory.CreateDbContextAsync(cancellationToken);
        if (db is null)
        {
            throw new ShouldAssertException("DbContext must not be null.");
        }

        var model = db.Model;
        if (model is null)
        {
            throw new ShouldAssertException("EF model must not be null.");
        }

        var entityType = model.FindEntityType(typeof(Variable))
            ?? throw new ShouldAssertException("Variable entity type must be mapped.");
        var property = entityType.FindProperty(nameof(Variable.IsActive))
            ?? throw new ShouldAssertException("Variable.IsActive must be a mapped property.");
        var converter = property.GetValueConverter()
            ?? throw new ShouldAssertException("Variable.IsActive must have a mapped value converter.");

        // Positive (>= 2): must normalize to Active (value 1), NOT the Invalid sentinel.
        converter.ConvertFromProvider(5).ShouldBeOfType<ActiveStatus>().Value.ShouldBe(ActiveStatus.Active.Value);

        // Exact tri-state values still map to themselves.
        converter.ConvertFromProvider(1).ShouldBeOfType<ActiveStatus>().Value.ShouldBe(ActiveStatus.Active.Value);
        converter.ConvertFromProvider(0).ShouldBeOfType<ActiveStatus>().Value.ShouldBe(ActiveStatus.None.Value);
        converter.ConvertFromProvider(-3).ShouldBeOfType<ActiveStatus>().Value.ShouldBe(ActiveStatus.Inactive.Value);

        // To-DB side stores the underlying int (-1/0/1).
        converter.ConvertToProvider(ActiveStatus.Active).ShouldBe(1);
        converter.ConvertToProvider(ActiveStatus.Inactive).ShouldBe(-1);
    }

    private static Variable NewVariable(int machineId, string name, ActiveStatus isActive) => new()
    {
        MachineId = machineId,
        PlcId = 991,
        Name = name,
        Description = name,
        Alias = name,
        Address = name,
        NetType = "INT",
        Length = 2,
        IsActive = isActive,
        Direction = 1,
        VariableGroupId = 1,
    };
}
