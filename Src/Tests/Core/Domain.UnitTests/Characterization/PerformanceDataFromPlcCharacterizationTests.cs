// <copyright file="PerformanceDataFromPlcCharacterizationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.Characterization;

/// <summary>
/// Characterization (golden-master) tests for <see cref="PerformanceData.FromPlc"/> — the pure value-mapping
/// seam at the heart of the OEE PLC register-producing path (#39 Task 6, design §2a / §5). The OEE
/// <c>PlcProcessor.ReadPerformanceDataFromPlcAsync</c> mutates a dictionary of live <see cref="Register"/>
/// tag carriers and then projects it through <see cref="PerformanceData.FromPlc"/>. This seam takes a
/// hand-built register dictionary and pins the EXACT parsed numeric output, independent of any PLC plumbing,
/// so the upcoming mutate-in-place -&gt; reconstruction rework can be proven behaviour-preserving for the
/// ephemeral OEE projection.
///
/// <para>
/// Pinned contract: <c>FromPlc</c> keys registers by the PerformanceData PROPERTY NAME (e.g. the register
/// keyed "TotalProduction" feeds <see cref="PerformanceData.TotalProduction"/>); int fields go through
/// <c>int.TryParse</c> and double fields through <c>double.TryParse</c>, each defaulting to 0 / 0.0 on a
/// missing key or unparseable value. Whole-number strings are used to keep parsing culture-invariant.
/// </para>
/// </summary>
public sealed class PerformanceDataFromPlcCharacterizationTests
{
    private static Register Reg(string name, string value) => Register.CreateFixture(name: name, value: value);

    /// <summary>
    /// Golden master: every numeric field maps from its property-name-keyed register. Pins both the int
    /// (TryParse) and double (TryParse) mapping and the per-key routing.
    /// </summary>
    [Fact]
    public void FromPlc_AllFieldsPresent_MapsEachByPropertyNameKey()
    {
        var registers = new Dictionary<string, Register>
        {
            [nameof(PerformanceData.ApplicationFlag)] = Reg(nameof(PerformanceData.ApplicationFlag), "7"),
            [nameof(PerformanceData.EventCounter)] = Reg(nameof(PerformanceData.EventCounter), "150"),
            [nameof(PerformanceData.CurrentTime)] = Reg(nameof(PerformanceData.CurrentTime), "3600"),
            [nameof(PerformanceData.RunningTime)] = Reg(nameof(PerformanceData.RunningTime), "3000"),
            [nameof(PerformanceData.StoppedTime)] = Reg(nameof(PerformanceData.StoppedTime), "400"),
            [nameof(PerformanceData.FaultedTime)] = Reg(nameof(PerformanceData.FaultedTime), "200"),
            [nameof(PerformanceData.StatusFaultReason)] = Reg(nameof(PerformanceData.StatusFaultReason), "12"),
            [nameof(PerformanceData.TotalProduction)] = Reg(nameof(PerformanceData.TotalProduction), "1500"),
            [nameof(PerformanceData.ProductionOk)] = Reg(nameof(PerformanceData.ProductionOk), "1450"),
            [nameof(PerformanceData.ProductionNoK)] = Reg(nameof(PerformanceData.ProductionNoK), "50"),
            [nameof(PerformanceData.StatusFaultReject)] = Reg(nameof(PerformanceData.StatusFaultReject), "3"),
            [nameof(PerformanceData.RejectEventCounter)] = Reg(nameof(PerformanceData.RejectEventCounter), "9"),
            [nameof(PerformanceData.StatusReject)] = Reg(nameof(PerformanceData.StatusReject), "1"),
            [nameof(PerformanceData.RejectQuantityUnits)] = Reg(nameof(PerformanceData.RejectQuantityUnits), "25"),
            [nameof(PerformanceData.StandardCycleTime)] = Reg(nameof(PerformanceData.StandardCycleTime), "12"),
            [nameof(PerformanceData.ActualCycleTime)] = Reg(nameof(PerformanceData.ActualCycleTime), "13"),
            [nameof(PerformanceData.PlanedProductionTime)] = Reg(nameof(PerformanceData.PlanedProductionTime), "28800"),
        };

        var performance = PerformanceData.FromPlc(registers);

        // int fields (int.TryParse).
        performance.ApplicationFlag.ShouldBe(7);
        performance.EventCounter.ShouldBe(150);
        performance.CurrentTime.ShouldBe(3600);
        performance.RunningTime.ShouldBe(3000);
        performance.StoppedTime.ShouldBe(400);
        performance.FaultedTime.ShouldBe(200);
        performance.StatusFaultReason.ShouldBe(12);
        performance.StatusFaultReject.ShouldBe(3);
        performance.RejectEventCounter.ShouldBe(9);
        performance.StatusReject.ShouldBe(1);

        // double fields (double.TryParse).
        performance.TotalProduction.ShouldBe(1500d);
        performance.ProductionOk.ShouldBe(1450d);
        performance.ProductionNoK.ShouldBe(50d);
        performance.RejectQuantityUnits.ShouldBe(25d);
        performance.StandardCycleTime.ShouldBe(12d);
        performance.ActualCycleTime.ShouldBe(13d);
        performance.PlanedProductionTime.ShouldBe(28800d);
    }

    /// <summary>
    /// Golden master: a missing key yields the field default (0 / 0.0), never a throw. Pins the
    /// <c>TryGetValue</c>-then-default behaviour.
    /// </summary>
    [Fact]
    public void FromPlc_MissingKeys_DefaultToZero()
    {
        var registers = new Dictionary<string, Register>
        {
            [nameof(PerformanceData.TotalProduction)] = Reg(nameof(PerformanceData.TotalProduction), "100"),
        };

        var performance = PerformanceData.FromPlc(registers);

        performance.TotalProduction.ShouldBe(100d);
        performance.ApplicationFlag.ShouldBe(0);
        performance.EventCounter.ShouldBe(0);
        performance.ProductionOk.ShouldBe(0d);
        performance.ProductionNoK.ShouldBe(0d);
        performance.ActualCycleTime.ShouldBe(0d);
    }

    /// <summary>
    /// Golden master: an unparseable value yields the field default (0 / 0.0), never a throw. Pins the
    /// graceful TryParse-failure behaviour the refactor must preserve.
    /// </summary>
    [Fact]
    public void FromPlc_UnparseableValues_DefaultToZero()
    {
        var registers = new Dictionary<string, Register>
        {
            [nameof(PerformanceData.ApplicationFlag)] = Reg(nameof(PerformanceData.ApplicationFlag), "not-a-number"),
            [nameof(PerformanceData.TotalProduction)] = Reg(nameof(PerformanceData.TotalProduction), string.Empty),
            [nameof(PerformanceData.EventCounter)] = Reg(nameof(PerformanceData.EventCounter), "42"),
        };

        var performance = PerformanceData.FromPlc(registers);

        performance.ApplicationFlag.ShouldBe(0);
        performance.TotalProduction.ShouldBe(0d);
        performance.EventCounter.ShouldBe(42);
    }
}
