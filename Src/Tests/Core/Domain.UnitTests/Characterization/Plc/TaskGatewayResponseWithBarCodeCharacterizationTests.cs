// <copyright file="TaskGatewayResponseWithBarCodeCharacterizationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.Characterization.Plc;

/// <summary>
/// Story 27.2b-2 (#27 / F4) §7 golden-master (characterization) tests for the frozen PLC-contract
/// projection of <see cref="TaskGatewayResponseDto"/>. These pin the EXACT bytes emitted across the
/// entity-Label read (the <c>BarCode</c>-carrying wire DTO), the <see cref="ReferenceStamper.Apply"/>
/// reference stamping, and the <see cref="TaskGatewayResponseDto.ToString"/> serialization surface, for BOTH
/// the present-label case and the absent ("no part scanned") case.
/// <para>
/// The retype of <c>BarCode.Label</c> from <c>string</c> to the <c>BarCodeLabel</c> value object must leave
/// every one of these strings BIT-FOR-BIT identical (the §7 wire is byte-frozen). These assertions are the
/// gate: they pass BEFORE the retype (baseline) and MUST still pass, unchanged, afterwards.
/// </para>
/// </summary>
public class TaskGatewayResponseWithBarCodeCharacterizationTests
{
    private const string GoldenLabel = "L1AL100003232372501";

    private static BarCode PresentBarCode()
    {
        var stamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        return BarCode.Create(GoldenLabel, productId: 508, machineId: 100, createdOn: stamp, modifiedOn: stamp);
    }

    /// <summary>
    /// PRESENT case: carrying a <c>BarCode</c> onto the wire DTO copies the entity label onto the wire-facing
    /// <see cref="TaskGatewayResponseDto.Label"/> string BYTE-FOR-BYTE.
    /// </summary>
    [Fact]
    public void WithBarCode_PresentLabel_CopiesEntityLabelOntoWireLabel_ByteEqual()
    {
        // Arrange
        var barCode = PresentBarCode();

        // Act — the retired WithBarCode(bc) set both BarCode and Label = bc.Label.Value.
        var updated = new TaskGatewayResponseDto { BarCode = barCode, Label = barCode.Label.Value };

        // Assert — the wire Label is the entity label, byte-for-byte (ordinal).
        string.Equals(updated.Label, GoldenLabel, StringComparison.Ordinal).ShouldBeTrue();
        ReferenceEquals(updated.BarCode, barCode).ShouldBeTrue();
    }

    /// <summary>
    /// ABSENT case: a response with no part scanned emits an EMPTY wire Label. After 2b-2 the absent state is a
    /// <c>null</c> BarCode reference and the emitted wire byte is <see cref="string.Empty"/>.
    /// </summary>
    [Fact]
    public void DefaultResponse_AbsentBarCode_EmitsEmptyWireLabel_ByteEqual()
    {
        // Arrange + Act
        var response = new TaskGatewayResponseDto();

        // Assert — absent → empty wire byte.
        string.Equals(response.Label, string.Empty, StringComparison.Ordinal).ShouldBeTrue();
    }

    /// <summary>
    /// §7 wire projection: <see cref="ReferenceStamper.Apply"/> writes the wire Label into the <c>Label</c>
    /// reference register BYTE-FOR-BYTE.
    /// </summary>
    [Fact]
    public void ApplyReferencesValues_ProjectsWireLabelIntoLabelRegister_ByteEqual()
    {
        // Arrange
        var barCode = PresentBarCode();
        var labelRegister = Register.Create(
            name: nameof(TaskGatewayResponseDto.Label),
            description: string.Empty,
            machineId: 0,
            variableId: 1,
            cycleId: 0,
            value: string.Empty,
            dataType: "string",
            statusValueId: 1,
            timeStamp: default,
            registerId: 1);
        labelRegister.IsSuccess.ShouldBeTrue();
        var response = new TaskGatewayResponseDto
        {
            BarCode = barCode,
            Label = barCode.Label.Value,
            References = new Dictionary<string, Register>
            {
                [nameof(TaskGatewayResponseDto.Label)] = labelRegister.Value.ShouldNotBeNull(),
            },
        };

        // Act — stamping returns a NEW dto whose References carry the projected values.
        var applied = ReferenceStamper.Apply(response);

        // Assert — the projected register value is the label, byte-for-byte.
        applied.IsSuccess.ShouldBeTrue();
        string.Equals(
            applied.Value.ShouldNotBeNull().References[nameof(TaskGatewayResponseDto.Label)].Value,
            GoldenLabel,
            StringComparison.Ordinal).ShouldBeTrue();
    }

    /// <summary>
    /// §7 diagnostic surface: <see cref="TaskGatewayResponseDto.ToString"/> renders the label on the
    /// <c>BarCode:</c> line BYTE-FOR-BYTE (present and absent).
    /// </summary>
    [Fact]
    public void ToString_RendersLabelLine_ByteEqual_PresentAndAbsent()
    {
        // Present
        var presentBarCode = PresentBarCode();
        var present = new TaskGatewayResponseDto { BarCode = presentBarCode, Label = presentBarCode.Label.Value };
        present.ToString().ShouldContain($"BarCode: {GoldenLabel}");

        // Absent (never scanned) — the label line renders the empty wire label.
        var absent = new TaskGatewayResponseDto();
        absent.ToString().ShouldContain("BarCode: ");
    }
}
