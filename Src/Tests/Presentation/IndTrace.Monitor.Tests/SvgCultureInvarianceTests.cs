// <copyright file="SvgCultureInvarianceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using AngleSharp.Dom;
using IndTrace.Components.Area.OEE;
using Shouldly;

namespace IndTrace.Monitor.Tests;

/// <summary>
/// Regression guard for GitHub issue #82 (SVG rendering half of the defect). SVG coordinate strings MUST
/// use a period decimal separator to be valid; on a comma-decimal deployment, culture-sensitive numeric
/// formatting turns coordinates such as <c>33.33,12.50</c> into <c>33,33,12,50</c>, corrupting the geometry.
/// These tests render the real <see cref="OeeGauge"/> and <see cref="TrendSparkline"/> components with the
/// current culture forced to a comma-decimal culture (<c>de-DE</c>, which under .NET's ICU globalization
/// unambiguously uses a comma decimal separator — unlike <c>es-MX</c>, which formats with a period) and
/// assert that the emitted SVG geometry stays period-decimal and identical to the invariant-culture render.
/// <para>
/// The gauge's percentage <c>&lt;text&gt;</c> label is intentionally left localized (a comma there is a
/// cosmetic display choice, not a structural coordinate), so the assertions target the coordinate-bearing
/// attributes (<c>path/@d</c>, <c>line/@x2</c>, <c>line/@y2</c>, <c>polyline/@points</c>) rather than the
/// full markup.
/// </para>
/// </summary>
public class SvgCultureInvarianceTests : BUnitTestBase
{
    private static readonly CultureInfo CommaCulture = new("de-DE");

    /// <summary>
    /// The sparkline polyline <c>points</c> attribute uses period decimals under a comma-decimal culture.
    /// </summary>
    [Fact]
    public void TrendSparkline_UnderCommaCulture_PolylinePointsUsePeriodDecimal()
    {
        var data = new List<double> { 1.5, 2.25, 3.75, 0.5 };

        var commaPoints = RenderUnderCulture(CommaCulture, () =>
            RenderComponent<TrendSparkline>(p => p.Add(x => x.Data, data))
                .Find("polyline").GetAttribute("points") ?? string.Empty);
        var invariantPoints = RenderUnderCulture(CultureInfo.InvariantCulture, () =>
            RenderComponent<TrendSparkline>(p => p.Add(x => x.Data, data))
                .Find("polyline").GetAttribute("points") ?? string.Empty);

        // The X coordinates depend only on the point count (100 / (n-1) * i), so these are deterministic.
        commaPoints.ShouldContain("33.33");
        commaPoints.ShouldContain("66.67");
        commaPoints.ShouldContain("100.00");
        // A comma-decimal formatting would have emitted "33,33" — assert that corruption is absent.
        commaPoints.ShouldNotContain("33,33");
        // Culture must not change the emitted coordinates.
        commaPoints.ShouldBe(invariantPoints);
    }

    /// <summary>
    /// The gauge SVG arc path and needle coordinates render identically under a comma-decimal culture and the
    /// invariant culture (i.e. culture cannot corrupt the coordinates), and carry period decimals.
    /// </summary>
    [Fact]
    public void OeeGauge_UnderCommaCulture_CoordinatesAreCultureInvariant()
    {
        const double value = 87.5;

        var comma = RenderUnderCulture(CommaCulture, () => ExtractGaugeCoordinates(value));
        var invariant = RenderUnderCulture(CultureInfo.InvariantCulture, () => ExtractGaugeCoordinates(value));

        // Coordinates must be byte-identical regardless of culture.
        comma.ShouldBe(invariant);

        // The dynamic value-arc path (second <path>) and needle line must carry a period-decimal coordinate.
        comma.ValueArcPath.ShouldContain(".");
        comma.ValueArcPath.ShouldStartWith("M");
        comma.NeedleX.ShouldNotBeNullOrWhiteSpace();
        comma.NeedleY.ShouldNotBeNullOrWhiteSpace();
        // A comma-decimal corruption of the arc would contain "0,00" style fragments where a coordinate
        // component is fractional; the period-decimal render never does for this value.
        comma.ValueArcPath.ShouldContain("183.");
    }

    private (string ValueArcPath, string NeedleX, string NeedleY) ExtractGaugeCoordinates(double value)
    {
        var rendered = RenderComponent<OeeGauge>(p => p.Add(x => x.Value, value));
        var paths = rendered.FindAll("path");
        var line = rendered.Find("line");

        // paths[0] is the static background arc; paths[1] is the dynamic value arc built from doubles.
        var valueArc = paths[paths.Count - 1].GetAttribute("d") ?? string.Empty;
        var needleX = line.GetAttribute("x2") ?? string.Empty;
        var needleY = line.GetAttribute("y2") ?? string.Empty;
        return (valueArc, needleX, needleY);
    }

    private static T RenderUnderCulture<T>(CultureInfo culture, Func<T> render)
    {
        var previousCurrent = CultureInfo.CurrentCulture;
        var previousDefault = CultureInfo.DefaultThreadCurrentCulture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        try
        {
            return render();
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCurrent;
            CultureInfo.DefaultThreadCurrentCulture = previousDefault;
        }
    }
}
