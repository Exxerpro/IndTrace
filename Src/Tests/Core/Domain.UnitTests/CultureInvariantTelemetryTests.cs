// <copyright file="CultureInvariantTelemetryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;

namespace IndTrace.Domain.UnitTests;

/// <summary>
/// Regression guard for GitHub issue #82: on a comma-decimal deployment (the copyright holder is an
/// SA de CV whose host locale uses a comma decimal separator), culture-sensitive parsing of PLC/DB
/// numeric readings silently corrupts telemetry — a period-decimal reading such as <c>"12.5"</c> is
/// misread as <c>125</c> because the period is the group separator on a comma-decimal culture. Every
/// test here forces <see cref="CultureInfo.CurrentCulture"/> to a comma-decimal culture and asserts the
/// machine/DB numeric round-trips unchanged.
/// <para>
/// The comma-decimal culture used is <c>de-DE</c>: under .NET's ICU globalization the <c>es-MX</c>
/// culture actually formats with a period decimal separator (Mexico formats numbers like the US), so it
/// does NOT reproduce the defect; <c>de-DE</c> unambiguously uses a comma decimal separator and therefore
/// exercises the exact corruption a comma-decimal SA de CV host exhibits.
/// </para>
/// </summary>
public class CultureInvariantTelemetryTests
{
    private static readonly CultureInfo CommaCulture = new("de-DE");

    /// <summary>
    /// PLC real (double) registers parse period-decimal readings correctly under a comma-decimal culture.
    /// </summary>
    [Fact]
    public void FromPlc_RealRegisters_UnderCommaCulture_RoundTripWithPeriodDecimal()
    {
        var registers = new Dictionary<string, Register>
        {
            ["TotalProduction"] = MakeRegister("TotalProduction", "12.5"),
            ["ProductionOk"] = MakeRegister("ProductionOk", "3.14"),
            ["ProductionNoK"] = MakeRegister("ProductionNoK", "0.25"),
            ["EventCounter"] = MakeRegister("EventCounter", "42"),
        };

        RunUnderCulture(CommaCulture, () =>
        {
            var performance = PerformanceData.FromPlc(registers);

            performance.TotalProduction.ShouldBe(12.5);
            performance.ProductionOk.ShouldBe(3.14);
            performance.ProductionNoK.ShouldBe(0.25);
            performance.EventCounter.ShouldBe(42);
        });
    }

    /// <summary>
    /// <see cref="RegisterValue.ParseDouble"/> parses a period-decimal reading invariantly under a
    /// comma-decimal culture.
    /// </summary>
    [Theory]
    [InlineData("12.5", 12.5)]
    [InlineData("3.14159", 3.14159)]
    [InlineData("0.001", 0.001)]
    [InlineData("1000.75", 1000.75)]
    public void RegisterValue_ParseDouble_UnderCommaCulture_UsesPeriodDecimal(string reading, double expected)
    {
        var value = RegisterValue.Create(reading, "double").Value.ShouldNotBeNull();

        RunUnderCulture(CommaCulture, () =>
        {
            var parsed = value.ParseDouble();

            parsed.IsSuccess.ShouldBeTrue();
            parsed.Value.ShouldBe(expected);
        });
    }

    /// <summary>
    /// <see cref="RegisterValue.ParseInt"/> stays correct under a comma-decimal culture.
    /// </summary>
    [Fact]
    public void RegisterValue_ParseInt_UnderCommaCulture_RoundTrips()
    {
        var value = RegisterValue.Create("42", "int").Value.ShouldNotBeNull();

        RunUnderCulture(CommaCulture, () =>
        {
            var parsed = value.ParseInt();

            parsed.IsSuccess.ShouldBeTrue();
            parsed.Value.ShouldBe(42);
        });
    }

    /// <summary>
    /// A KPI-style double formats invariantly (period decimal) under a comma-decimal culture, matching the
    /// wire/DB/SVG formatting the production code now performs.
    /// </summary>
    [Fact]
    public void KpiDouble_FormatsInvariantly_UnderCommaCulture()
    {
        RunUnderCulture(CommaCulture, () =>
        {
            const double kpi = 87.5;

            kpi.ToString(CultureInfo.InvariantCulture).ShouldBe("87.5");
            kpi.ToString(CommaCulture).ShouldBe("87,5"); // proves the culture really is comma-decimal
        });
    }

    private static Register MakeRegister(string name, string value) =>
        Register.Create(name, name, machineId: 1, variableId: 1, cycleId: 1, value, "double", statusValueId: 0, DateTime.UnixEpoch)
            .Value.ShouldNotBeNull();

    private static void RunUnderCulture(CultureInfo culture, Action act)
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = culture;
        try
        {
            act();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
