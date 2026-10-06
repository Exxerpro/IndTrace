// <copyright file="MachineTraceabilityModeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValueObjectsTests;

using IndTrace.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="MachineTraceabilityMode"/> — the typed two-flag traceability gate.
/// Pins the fail-safe rule (disabled iff exactly (App=0, Bypass=1)) and the canonical transitions.
/// </summary>
public class MachineTraceabilityModeTests
{
    /// <summary>
    /// The canonical Enabled mode is (App=1, Bypass=0) and reads enabled.
    /// </summary>
    [Fact]
    public void Enabled_IsAppOneBypassZero_AndIsEnabled()
    {
        MachineTraceabilityMode.Enabled.AppTraceability.ShouldBe(1);
        MachineTraceabilityMode.Enabled.BypassTraceability.ShouldBe(0);
        MachineTraceabilityMode.Enabled.IsEnabled.ShouldBeTrue();
    }

    /// <summary>
    /// The canonical Disabled mode is (App=0, Bypass=1) and reads disabled.
    /// </summary>
    [Fact]
    public void Disabled_IsAppZeroBypassOne_AndIsNotEnabled()
    {
        MachineTraceabilityMode.Disabled.AppTraceability.ShouldBe(0);
        MachineTraceabilityMode.Disabled.BypassTraceability.ShouldBe(1);
        MachineTraceabilityMode.Disabled.IsEnabled.ShouldBeFalse();
    }

    /// <summary>
    /// The fail-safe gate: disabled only on the exact (App=0, Bypass=1) combination; every other
    /// combination — including non-canonical (out-of-domain) flag values — stays enabled.
    /// </summary>
    /// <param name="app">The application-traceability flag value.</param>
    /// <param name="bypass">The bypass-traceability flag value.</param>
    /// <param name="expectedEnabled">The expected gate result.</param>
    [Theory]
    [InlineData(1, 0, true)]  // canonical enabled
    [InlineData(0, 1, false)] // canonical disabled (the only disabled combination)
    [InlineData(0, 0, true)]  // not the disabled combo -> enabled (fail-safe)
    [InlineData(1, 1, true)]  // not the disabled combo -> enabled (fail-safe)
    [InlineData(2, 1, true)]  // non-canonical App, Bypass=1 -> still enabled (fail-safe)
    [InlineData(2, 0, true)]  // non-canonical App -> enabled
    public void FromFlags_IsEnabled_FollowsFailSafeRule(int app, int bypass, bool expectedEnabled)
    {
        MachineTraceabilityMode.FromFlags(app, bypass).IsEnabled.ShouldBe(expectedEnabled);
    }

    /// <summary>
    /// Two modes over the same flag pair are value-equal (value-object semantics).
    /// </summary>
    [Fact]
    public void FromFlags_WithSameFlags_AreValueEqual()
    {
        var a = MachineTraceabilityMode.FromFlags(1, 0);
        var b = MachineTraceabilityMode.FromFlags(1, 0);

        a.ShouldBe(b);
        a.Equals(MachineTraceabilityMode.Enabled).ShouldBeTrue();
    }

    /// <summary>
    /// Modes over different flag pairs are not value-equal.
    /// </summary>
    [Fact]
    public void FromFlags_WithDifferentFlags_AreNotValueEqual()
    {
        MachineTraceabilityMode.FromFlags(1, 0).Equals(MachineTraceabilityMode.FromFlags(0, 1)).ShouldBeFalse();
    }
}
