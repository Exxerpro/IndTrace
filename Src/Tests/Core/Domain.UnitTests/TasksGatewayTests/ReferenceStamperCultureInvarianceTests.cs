// <copyright file="ReferenceStamperCultureInvarianceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;

namespace IndTrace.Domain.UnitTests.TasksGatewayTests;

/// <summary>
/// Regression guard for issue #121 (F20): <see cref="ReferenceStamper.Apply"/> writes the byte-frozen §7
/// routing registers, so its scalar formatting must be culture-invariant. Under a culture whose
/// <see cref="NumberFormatInfo.NegativeSign"/> is NOT the ASCII hyphen-minus (e.g. U+2212 MINUS SIGN),
/// a parameterless <c>ToString()</c> stamps <c>−1</c> instead of <c>-1</c> and diverges from the frozen
/// wire format. The culture is constructed explicitly with a U+2212 negative sign so the test is
/// deterministic across ICU versions.
/// </summary>
public class ReferenceStamperCultureInvarianceTests
{
    /// <summary>
    /// F20: stamping a negative NextMachineId (the diverter sentinel is -1) under a non-ASCII-minus culture
    /// must still produce the ASCII string "-1" in the §7 register.
    /// </summary>
    [Fact]
    public void Apply_UnderNonAsciiNegativeSignCulture_StampsAsciiMinus()
    {
        var references = new Dictionary<string, Register>
        {
            { "NextMachineId", Register.CreateFixture(value: "0") },
            { "LastMachineId", Register.CreateFixture(value: "0") },
            { "CyclesOk", Register.CreateFixture(value: "0") },
        };

        var dto = new TaskGatewayResponseDto
        {
            References = references,
            NextMachineId = -1,
            LastMachineId = -5,
            CyclesOk = 25,
        };

        RunUnderCulture(NonAsciiMinusCulture(), () =>
        {
            var applied = ReferenceStamper.Apply(dto);

            applied.IsSuccess.ShouldBeTrue();
            var stamped = applied.Value.ShouldNotBeNull().References;
            stamped["NextMachineId"].Value.ShouldBe("-1");
            stamped["LastMachineId"].Value.ShouldBe("-5");
            stamped["CyclesOk"].Value.ShouldBe("25");
        });
    }

    private static CultureInfo NonAsciiMinusCulture()
    {
        // sv-SE uses U+2212 under some ICU builds but not all — pin the non-ASCII sign explicitly so the
        // regression is reproduced deterministically on every host.
        var culture = (CultureInfo)new CultureInfo("sv-SE").Clone();
        culture.NumberFormat.NegativeSign = "−";
        return culture;
    }

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
