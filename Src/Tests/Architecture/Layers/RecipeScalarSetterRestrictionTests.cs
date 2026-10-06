// <copyright file="RecipeScalarSetterRestrictionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using IndTrace.Domain.Entities;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Story 26.A3 (#26) — Architecture guard mirroring <c>ProductScalarSetterRestrictionTests</c> (26.A1) and
/// <c>MachinePlcAndPlcScalarSetterRestrictionTests</c> (26.A2), closing the #26 entity-shape sweep by pinning
/// <see cref="Recipe"/>'s already-shipped Story 2.3 de-anemization. Reflects over every de-anemized
/// <see cref="Recipe"/> value scalar (the cycle-time window and the cycle/retry counters) and fails if any exposes a
/// PUBLIC setter, so the guarded <see cref="Recipe.Create"/> factory / <c>CreateFixture</c> seam / EF materialization
/// stay the single construction authority and the <c>0 &lt;= minimum &lt;= maximum</c> / non-negative invariants can
/// never be violated post-construction by a raw property assignment. The setters MUST stay settable (non-public
/// <c>private set</c>) so EF Core can materialize them via the property mapping — this test only forbids a PUBLIC
/// setter, not a private one. The database-assigned identity <see cref="Recipe.RecipeId"/> and the foreign-key
/// associations <see cref="Recipe.ProductId"/>/<see cref="Recipe.MachineId"/> are intentionally excluded: they keep
/// public setters per the config/EF-fix-up precedent (mirroring <c>Product.ProductId</c> and the identity/FK
/// exclusions in 26.A1/26.A2). Recipe has NO guarded post-creation mutator because no caller writes it after
/// construction (AD-4 — none invented); <see cref="Recipe.GetCycleTimeWindow"/> is a computed read accessor, not a
/// mutation seam.
/// </summary>
public class RecipeScalarSetterRestrictionTests
{
    public static IEnumerable<object[]> RecipeValueScalars()
    {
        yield return new object[] { nameof(Recipe.CycleTimeMinimum) };
        yield return new object[] { nameof(Recipe.CycleTimeMaximum) };
        yield return new object[] { nameof(Recipe.MaxCyclesOk) };
        yield return new object[] { nameof(Recipe.MaxCyclesNOk) };
        yield return new object[] { nameof(Recipe.Retry) };
    }

    [Theory]
    [MemberData(nameof(RecipeValueScalars))]
    public void RecipeValueScalarSetter_ShouldNotBePublic(string propertyName) =>
        AssertScalarSetterIsNotPublic(typeof(Recipe), propertyName, "Recipe.Create or the CreateFixture seam");

    private static void AssertScalarSetterIsNotPublic(Type entityType, string propertyName, string mutationRoute)
    {
        var property = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        property.ShouldNotBeNull($"{entityType.Name}.{propertyName} must remain a public, settable property for EF materialization.");

        // Explicit narrowing guard (unreachable after ShouldNotBeNull throws) — avoids the banned null-forgiving operator.
        if (property is null)
        {
            return;
        }

        property.CanWrite.ShouldBeTrue($"{entityType.Name}.{propertyName} must stay settable (private set), not get-only — EF materializes it via the property mapping.");

        // The keystone assertion: a public setter would let any caller bypass the guarded construction seams.
        (property.GetSetMethod()?.IsPublic == true).ShouldBeFalse(
            $"{entityType.Name}.{propertyName} must NOT have a public setter; route construction through {mutationRoute} (Story 26.A3).");
    }
}
