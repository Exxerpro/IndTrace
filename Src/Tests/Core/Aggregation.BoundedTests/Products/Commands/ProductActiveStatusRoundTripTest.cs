// <copyright file="ProductActiveStatusRoundTripTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Products.Commands;

/// <summary>
/// Verifies the <see cref="Product.IsActive"/> EF value converter persists the tri-state
/// <see cref="ActiveStatus"/> as its underlying int (-1/0/1) and materializes it back, that the
/// converter participates in query predicate translation (filtering by <see cref="ActiveStatus"/>),
/// and that the from-DB side NORMALIZES legacy positive values (>= 2) to <see cref="ActiveStatus.Active"/>
/// rather than stranding them on the Invalid sentinel.
/// Runs over the real EF Core (InMemory) database from <see cref="DependenciesFactory"/>.
/// </summary>
public class ProductActiveStatusRoundTripTest : DependenciesFactory
{
    public ProductActiveStatusRoundTripTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Saves a Product for each ActiveStatus, reloads from a fresh context, and asserts the
    /// round-tripped underlying value and that a converter-based predicate filters correctly.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    [Fact]
    public async Task IsActive_ShouldRoundTripTriStateThroughConverter()
    {
        await Initialization;

        var cancellationToken = TestContext.Current.CancellationToken;

        // Distinct part numbers keep these rows identifiable and clear of seeded fixtures.
        var active = NewProduct("RoundTrip_Active_Product", ActiveStatus.Active);
        var none = NewProduct("RoundTrip_None_Product", ActiveStatus.None);
        var inactive = NewProduct("RoundTrip_Inactive_Product", ActiveStatus.Inactive);

        var repository = DpProductRepository;
        (await repository.AddAsync(active, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(none, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.AddAsync(inactive, cancellationToken)).IsSuccess.ShouldBeTrue();
        (await repository.CommitAsync(cancellationToken)).IsSuccess.ShouldBeTrue();

        // Reload from a brand-new context so the values are re-materialized through the converter.
        var db = await DpIndTraceDbTestContextFactory.CreateDbContextAsync(cancellationToken);

        var reloadedActive = db.Set<Product>().Single(p => p.PartNumber == "RoundTrip_Active_Product");
        var reloadedNone = db.Set<Product>().Single(p => p.PartNumber == "RoundTrip_None_Product");
        var reloadedInactive = db.Set<Product>().Single(p => p.PartNumber == "RoundTrip_Inactive_Product");

        // From-DB conversion: int column → ActiveStatus instance with the correct underlying value.
        reloadedActive.IsActive.ShouldNotBeNull();
        reloadedActive.IsActive.Value.ShouldBe(1);
        reloadedNone.IsActive.Value.ShouldBe(0);
        reloadedInactive.IsActive.Value.ShouldBe(-1);

        // Converter participates in predicate translation: filtering by the ActiveStatus member
        // matches on the underlying int value (1), not on reference identity.
        var activePartNumbers = db.Set<Product>()
            .Where(p => p.IsActive == ActiveStatus.Active)
            .Select(p => p.PartNumber)
            .ToList();

        activePartNumbers.ShouldContain("RoundTrip_Active_Product");
        activePartNumbers.ShouldNotContain("RoundTrip_None_Product");
        activePartNumbers.ShouldNotContain("RoundTrip_Inactive_Product");

        var inactivePartNumbers = db.Set<Product>()
            .Where(p => p.IsActive == ActiveStatus.Inactive)
            .Select(p => p.PartNumber)
            .ToList();

        inactivePartNumbers.ShouldContain("RoundTrip_Inactive_Product");
        inactivePartNumbers.ShouldNotContain("RoundTrip_Active_Product");
    }

    /// <summary>
    /// Proves the read-normalizing from-DB converter maps any legacy positive stored int (e.g. <c>5</c>)
    /// to <see cref="ActiveStatus.Active"/> — honoring the historical "positive = active" contract — and
    /// maps negatives to <see cref="ActiveStatus.Inactive"/> and zero to <see cref="ActiveStatus.None"/>,
    /// never producing the Invalid sentinel.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    [Fact]
    public async Task IsActive_FromDbConverter_ShouldNormalizeLegacyPositiveToActive()
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

        var entityType = model.FindEntityType(typeof(Product))
            ?? throw new ShouldAssertException("Product entity type must be mapped.");
        var property = entityType.FindProperty(nameof(Product.IsActive))
            ?? throw new ShouldAssertException("Product.IsActive must be a mapped property.");
        var converter = property.GetValueConverter()
            ?? throw new ShouldAssertException("Product.IsActive must have a mapped value converter.");

        // Legacy positive (>= 2): must normalize to Active (value 1), NOT the Invalid sentinel.
        converter.ConvertFromProvider(5).ShouldBeOfType<ActiveStatus>().Value.ShouldBe(ActiveStatus.Active.Value);

        // Exact tri-state values still map to themselves.
        converter.ConvertFromProvider(1).ShouldBeOfType<ActiveStatus>().Value.ShouldBe(ActiveStatus.Active.Value);
        converter.ConvertFromProvider(0).ShouldBeOfType<ActiveStatus>().Value.ShouldBe(ActiveStatus.None.Value);
        converter.ConvertFromProvider(-3).ShouldBeOfType<ActiveStatus>().Value.ShouldBe(ActiveStatus.Inactive.Value);

        // To-DB side stores the underlying int (-1/0/1).
        converter.ConvertToProvider(ActiveStatus.Active).ShouldBe(1);
        converter.ConvertToProvider(ActiveStatus.Inactive).ShouldBe(-1);
    }

    private static Product NewProduct(string partNumber, ActiveStatus isActive) =>
        Product.CreateFixture(
            partNumber: partNumber,
            productName: partNumber,
            description: partNumber,
            customerPartNumber: partNumber,
            aliasPartNumber: partNumber,
            customerName: partNumber,
            isActive: isActive,
            version: 1,
            customerId: 1,
            ruleId: 1,
            lineId: 1);
}
