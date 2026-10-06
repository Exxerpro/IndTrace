// <copyright file="LifecycleApplySeamVisibilityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Audit finding D3 — companion to <see cref="LifecycleSetterRestrictionTests"/>. The de-anemization keystone
/// (private-set lifecycle fields) is only meaningful if the trusted "apply" seams that write those fields are
/// ALSO sealed off from outer layers. These five methods —
/// <c>BarCode.ApplyFlowStatus</c> / <c>ApplyPartStatus</c> / <c>ApplyFlowAndPartStatus</c> and
/// <c>Cycle.ApplyCycleStatus</c> / <c>ApplyCycleAndPartStatus</c> — must be <c>internal</c>, not public, so
/// presentation (or any layer outside <c>IndTrace.Domain</c>/<c>IndTrace.Application</c>, the InternalsVisibleTo
/// grantees) cannot bypass the domain guards by calling a renamed public setter. A public Apply* method would
/// re-open exactly the FR1/G3 hole the private setters closed.
/// </summary>
public class LifecycleApplySeamVisibilityTests
{
    public static IEnumerable<object[]> ApplyMethods()
    {
        // Referenced by STRING (not nameof): these methods are internal and this test assembly is deliberately
        // NOT an InternalsVisibleTo grantee — being unable to name them here is itself part of the guarantee.
        yield return new object[] { typeof(BarCode), "ApplyFlowStatus" };
        yield return new object[] { typeof(BarCode), "ApplyPartStatus" };
        yield return new object[] { typeof(BarCode), "ApplyFlowAndPartStatus" };
        yield return new object[] { typeof(Cycle), "ApplyCycleStatus" };
        yield return new object[] { typeof(Cycle), "ApplyCycleAndPartStatus" };
    }

    [Theory]
    [MemberData(nameof(ApplyMethods))]
    public void ApplySeam_ShouldNotBePublic(Type entityType, string methodName)
    {
        // A public method would be visible to InternalsVisibleTo-by-name resolution too, so look it up across
        // public AND non-public so the test can prove it exists yet is not public.
        var method = entityType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        method.ShouldNotBeNull($"{entityType.Name}.{methodName} must exist as the trusted apply seam.");

        // The keystone assertion: a public Apply* method would let any outer layer bypass the state machine (D3).
        method!.IsPublic.ShouldBeFalse(
            $"{entityType.Name}.{methodName} must NOT be public; the trusted apply seam stays internal so only the " +
            "domain and InternalsVisibleTo-granted Application layer can write lifecycle state (audit finding D3).");

        method.IsAssembly.ShouldBeTrue(
            $"{entityType.Name}.{methodName} must be internal (assembly-visible), reachable by IndTrace.Application via InternalsVisibleTo.");
    }

    [Fact]
    public void AllApplyMethodsOnLifecycleEntities_ShouldBeNonPublic()
    {
        // Belt-and-braces: catch any FUTURE Apply* method added to these entities, not just the five enumerated.
        var offenders = new[] { typeof(BarCode), typeof(Cycle) }
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => m.Name.StartsWith("Apply", StringComparison.Ordinal))
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")
            .ToList();

        offenders.ShouldBeEmpty(
            "No Apply* method on BarCode/Cycle may be public; keep every lifecycle apply seam internal (audit finding D3).");
    }
}
