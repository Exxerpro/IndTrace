// <copyright file="LifecycleSetterRestrictionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Reflection;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Story 6.4 — Architecture Test #7 (the FR1/G3 keystone). Reflects over the four domain lifecycle properties
/// (<see cref="BarCode.FlowStatus"/>, <see cref="BarCode.PartStatus"/>, <see cref="Cycle.CycleStatus"/>,
/// <see cref="Cycle.PartStatus"/>) and fails if ANY of them exposes a PUBLIC setter. This compile-independent
/// guard ensures the domain state machine remains the single proven authority over lifecycle state: external
/// callers must route every mutation through a guarded transition/apply seam or a public creation seam, never a
/// raw property assignment. The setters MUST stay settable (non-public <c>private set</c>) so EF can materialize
/// them via the value-converter property mapping — this test only forbids a PUBLIC setter, not a private one.
/// </summary>
public class LifecycleSetterRestrictionTests
{
    public static IEnumerable<object[]> LifecycleProperties()
    {
        yield return new object[] { typeof(BarCode), nameof(BarCode.FlowStatus) };
        yield return new object[] { typeof(BarCode), nameof(BarCode.PartStatus) };
        yield return new object[] { typeof(Cycle), nameof(Cycle.CycleStatus) };
        yield return new object[] { typeof(Cycle), nameof(Cycle.PartStatus) };
    }

    [Theory]
    [MemberData(nameof(LifecycleProperties))]
    public void LifecycleSetter_ShouldNotBePublic(Type entityType, string propertyName)
    {
        var property = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        property.ShouldNotBeNull($"{entityType.Name}.{propertyName} must remain a public, settable property for EF materialization.");
        property!.CanWrite.ShouldBeTrue($"{entityType.Name}.{propertyName} must stay settable (private set), not get-only — EF materializes it via the value-converter mapping.");

        // The keystone assertion: a public setter would let any caller bypass the state machine (FR1/G3).
        (property.GetSetMethod()?.IsPublic == true).ShouldBeFalse(
            $"{entityType.Name}.{propertyName} must NOT have a public setter; route mutations through a guarded domain method or creation seam (Story 6.4).");
    }
}
