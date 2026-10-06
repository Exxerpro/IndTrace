// <copyright file="MachinePlcAndPlcScalarSetterRestrictionTests.cs" company="Exxerpro Solutions SA de CV">
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
/// Story 26.A2 (#26) — Architecture guard mirroring <c>ProductScalarSetterRestrictionTests</c> (Story 26.A1) for the
/// two PLC-config aggregates <see cref="MachinePlc"/> and <see cref="Plc"/>. Reflects over every de-anemized value
/// scalar and fails if any exposes a PUBLIC setter, so the guarded <c>Create</c> factory / mutation seams
/// (<see cref="MachinePlc.SetActiveStatus"/>, <see cref="Plc.ApplyUpdate"/>) / creation seams stay the single
/// construction-and-mutation authority. The setters MUST stay settable (non-public <c>private set</c>) so EF Core can
/// materialize them via the property mapping — this test only forbids a PUBLIC setter, not a private one. The
/// database-assigned identity <see cref="Plc.PlcId"/> is intentionally excluded: it keeps a public setter per the
/// config/EF-fix-up precedent (mirroring <c>Product.ProductId</c>). <see cref="MachinePlc"/>'s composite key
/// (<c>MachineId</c>/<c>PlcId</c>) is caller-supplied, not database-generated, so it is de-anemized like any other
/// value scalar.
/// </summary>
public class MachinePlcAndPlcScalarSetterRestrictionTests
{
    public static IEnumerable<object[]> MachinePlcValueScalars()
    {
        yield return new object[] { nameof(MachinePlc.MachineId) };
        yield return new object[] { nameof(MachinePlc.PlcId) };
        yield return new object[] { nameof(MachinePlc.IsActive) };
    }

    public static IEnumerable<object[]> PlcValueScalars()
    {
        yield return new object[] { nameof(Plc.MachineId) };
        yield return new object[] { nameof(Plc.Enabled) };
        yield return new object[] { nameof(Plc.Name) };
        yield return new object[] { nameof(Plc.IpAddress) };
        yield return new object[] { nameof(Plc.PlcType) };
        yield return new object[] { nameof(Plc.PlcBrand) };
        yield return new object[] { nameof(Plc.Options) };
        yield return new object[] { nameof(Plc.CommLibrary) };
        yield return new object[] { nameof(Plc.BrandOwner) };
    }

    [Theory]
    [MemberData(nameof(MachinePlcValueScalars))]
    public void MachinePlcValueScalarSetter_ShouldNotBePublic(string propertyName) =>
        AssertScalarSetterIsNotPublic(typeof(MachinePlc), propertyName, "MachinePlc.Create/SetActiveStatus or a creation seam");

    [Theory]
    [MemberData(nameof(PlcValueScalars))]
    public void PlcValueScalarSetter_ShouldNotBePublic(string propertyName) =>
        AssertScalarSetterIsNotPublic(typeof(Plc), propertyName, "Plc.Create/ApplyUpdate or a creation seam");

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

        // The keystone assertion: a public setter would let any caller bypass the guarded construction/mutation seams.
        (property.GetSetMethod()?.IsPublic == true).ShouldBeFalse(
            $"{entityType.Name}.{propertyName} must NOT have a public setter; route mutations through {mutationRoute} (Story 26.A2).");
    }
}
