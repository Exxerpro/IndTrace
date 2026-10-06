// <copyright file="ProductScalarSetterRestrictionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Reflection;
using IndTrace.Domain.Entities;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Story 26.A1 (#26) — Architecture guard mirroring the Story 6.4 lifecycle-setter keystone
/// (<c>LifecycleSetterRestrictionTests</c>). Reflects over every de-anemized <see cref="Product"/> value scalar and
/// fails if any exposes a PUBLIC setter, so the guarded <see cref="Product.Create"/> factory / <c>ApplyUpdate</c>
/// mutation seam stays the single construction-and-mutation authority: external callers must route every write
/// through a guarded method or a creation seam, never a raw property assignment. The setters MUST stay settable
/// (non-public <c>private set</c>) so EF Core can materialize them via the property mapping — this test only forbids
/// a PUBLIC setter, not a private one. The database-assigned identity <see cref="Product.ProductId"/> and the
/// navigation properties (<see cref="Product.Line"/>/<see cref="Product.Customer"/>) are intentionally excluded:
/// they keep public setters per the config/EF-fix-up precedent.
/// </summary>
public class ProductScalarSetterRestrictionTests
{
    public static IEnumerable<object[]> ProductValueScalars()
    {
        yield return new object[] { nameof(Product.PartNumber) };
        yield return new object[] { nameof(Product.ProductName) };
        yield return new object[] { nameof(Product.IsActive) };
        yield return new object[] { nameof(Product.Version) };
        yield return new object[] { nameof(Product.CustomerPartNumber) };
        yield return new object[] { nameof(Product.AliasPartNumber) };
        yield return new object[] { nameof(Product.Description) };
        yield return new object[] { nameof(Product.RuleId) };
        yield return new object[] { nameof(Product.CustomerId) };
        yield return new object[] { nameof(Product.LineId) };
        yield return new object[] { nameof(Product.CustomerName) };
    }

    [Theory]
    [MemberData(nameof(ProductValueScalars))]
    public void ProductValueScalarSetter_ShouldNotBePublic(string propertyName)
    {
        var property = typeof(Product).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        property.ShouldNotBeNull($"Product.{propertyName} must remain a public, settable property for EF materialization.");

        // Explicit narrowing guard (unreachable after ShouldNotBeNull throws) — avoids the banned null-forgiving operator.
        if (property is null)
        {
            return;
        }

        property.CanWrite.ShouldBeTrue($"Product.{propertyName} must stay settable (private set), not get-only — EF materializes it via the property mapping.");

        // The keystone assertion: a public setter would let any caller bypass the guarded Create/ApplyUpdate seams.
        (property.GetSetMethod()?.IsPublic == true).ShouldBeFalse(
            $"Product.{propertyName} must NOT have a public setter; route mutations through Product.Create/ApplyUpdate or a creation seam (Story 26.A1).");
    }
}
