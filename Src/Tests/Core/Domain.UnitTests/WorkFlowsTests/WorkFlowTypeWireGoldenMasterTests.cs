// <copyright file="WorkFlowTypeWireGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.WorkFlowsTests;

/// <summary>
/// Golden-master (characterization) tests for the wire-emitted <see cref="WorkFlowType"/> value.
///
/// Story 1.2: every composite <see cref="WorkFlowType"/> value that MAY be emitted to the PLC is
/// pinned by a golden-master so no internal composition can change a byte on the frozen
/// <c>DB256.DINT28</c> tag. The single outbound serialization seam is
/// <see cref="ReferenceStamper.Apply"/>, which renders the
/// <c>WorkFlowType</c> register as <c>WorkFlowType.Value.ToString()</c> (invariant integer),
/// with <see langword="null"/> collapsing to <c>"0"</c>. That string is later downloaded to
/// <c>DB256.DINT28</c> as a DINT.
///
/// These tests assert the EXACT wire string. They must never be "fixed" by changing
/// <see cref="ReferenceStamper.Apply"/> - that production method is the
/// frozen wire and any divergence means the test expectation is wrong, not the code.
/// </summary>
public class WorkFlowTypeWireGoldenMasterTests
{
    /// <summary>
    /// The frozen register key emitted to the PLC for the workflow type, mirroring
    /// <c>nameof(TaskGatewayResponseDto.WorkFlowType)</c>.
    /// </summary>
    private const string WorkFlowTypeRegisterKey = "WorkFlowType";

    /// <summary>
    /// Builds the wire string for a given <see cref="WorkFlowType"/> by driving it through the
    /// real outbound serialization seam (<see cref="ReferenceStamper.Apply"/>),
    /// returning the value written into the <c>WorkFlowType</c> register.
    /// </summary>
    /// <param name="workFlowType">The workflow type to emit.</param>
    /// <returns>The exact wire string written to the register.</returns>
    private static string EmitWireValue(WorkFlowType workFlowType)
    {
        var register = Register.CreateFixture(name: WorkFlowTypeRegisterKey);
        var references = new Dictionary<string, Register>
        {
            { WorkFlowTypeRegisterKey, register },
        };

        var response = new TaskGatewayResponseDto
        {
            WorkFlowType = workFlowType,
            References = references,
        };

        var result = ReferenceStamper.Apply(response);
        result.IsSuccess.ShouldBeTrue();

        // #32 C2 / #39: ReferenceStamper.Apply returns a NEW dto whose References carry the reconstructed
        // (immutable) register with the applied value — the authoritative wire source the PLC download reads.
        // The pre-existing local `register`/`references` are intentionally stale; read the produced wire string
        // from the stamped dto. The pinned wire bytes are unchanged — only the read seam follows the projection.
        return result.Value.ShouldNotBeNull().References[WorkFlowTypeRegisterKey].Value;
    }

    /// <summary>
    /// Each ATOMIC value that may be emitted in Release A renders to its exact integer wire string.
    /// This is the byte-equal characterization of the atomic wire write to <c>DB256.DINT28</c>.
    /// </summary>
    /// <param name="value">The atomic workflow value.</param>
    /// <param name="expectedWire">The exact wire string expected on the tag.</param>
    [Theory]
    [InlineData(0, "0")] // None
    [InlineData(1, "1")] // Initial
    [InlineData(2, "2")] // Serial
    [InlineData(4, "4")] // Lateral
    [InlineData(8, "8")] // Diverter
    [InlineData(16, "16")] // Merger
    [InlineData(32, "32")] // Final
    public void ApplyReferencesValues_ForAtomicWorkFlowType_EmitsExactWireString(int value, string expectedWire)
    {
        // Arrange
        var workFlowType = WorkFlowType.From(value);

        // Act
        var wire = EmitWireValue(workFlowType);

        // Assert - byte-equal golden master.
        wire.ShouldBe(expectedWire);
    }

    /// <summary>
    /// An unassigned <see cref="WorkFlowType"/> (the property default of
    /// <see cref="WorkFlowType.None"/>) emits the wire string <c>"0"</c> - the observable wire
    /// output for the "no workflow" case, equivalent to the seam's <c>?? "0"</c> null-default.
    /// </summary>
    [Fact]
    public void ApplyReferencesValues_ForDefaultWorkFlowType_EmitsZero()
    {
        // Arrange - default property value is WorkFlowType.None; never assign a value.
        var register = Register.CreateFixture(name: WorkFlowTypeRegisterKey);
        var response = new TaskGatewayResponseDto
        {
            References = new Dictionary<string, Register>
            {
                { WorkFlowTypeRegisterKey, register },
            },
        };

        // Act
        var result = ReferenceStamper.Apply(response);

        // Assert - #32 C2 / #39: read the reconstructed register from the stamped dto's references (the local
        // `register` reference is intentionally stale under immutability); the pinned wire string "0" is unchanged.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().References[WorkFlowTypeRegisterKey].Value.ShouldBe("0");
    }

    /// <summary>
    /// Named-singleton atomic members emit the same wire string as their raw value, proving the
    /// named members and the composable <see cref="WorkFlowType.From(int)"/> path agree on the wire.
    /// </summary>
    [Fact]
    public void ApplyReferencesValues_ForNamedAtomicSingletons_EmitsExpectedWireStrings()
    {
        // Arrange, Act, Assert
        EmitWireValue(WorkFlowType.None).ShouldBe("0");
        EmitWireValue(WorkFlowType.Initial).ShouldBe("1");
        EmitWireValue(WorkFlowType.Serial).ShouldBe("2");
        EmitWireValue(WorkFlowType.Lateral).ShouldBe("4");
        EmitWireValue(WorkFlowType.Diverter).ShouldBe("8");
        EmitWireValue(WorkFlowType.Merger).ShouldBe("16");
        EmitWireValue(WorkFlowType.Final).ShouldBe("32");
    }

    /// <summary>
    /// The composites enumerated by Story 1.1 (Parallel(64) and every non-atomic combination) are
    /// faithfully serializable IF they ever reached the seam - so the seam itself imposes no limit.
    /// The proof that they are NOT emitted in Release A is a behavioral assertion on the live
    /// emission flows (see the Application.UnitTests Plc characterization suite), NOT here. This
    /// test pins only that the seam renders a composite as its raw integer, should one ever arrive.
    /// </summary>
    /// <param name="value">The composite (or Parallel) workflow value.</param>
    /// <param name="expectedWire">The integer wire string the seam would emit.</param>
    [Theory]
    [InlineData(64, "64")] // Parallel
    [InlineData(18, "18")] // Merger | Serial
    [InlineData(66, "66")] // Parallel | Serial
    [InlineData(127, "127")] // every atomic bit set
    public void ApplyReferencesValues_ForComposite_EmitsRawIntegerWireString(int value, string expectedWire)
    {
        // Arrange
        var workFlowType = WorkFlowType.From(value);

        // Act
        var wire = EmitWireValue(workFlowType);

        // Assert
        wire.ShouldBe(expectedWire);
    }

    /// <summary>
    /// Lossy-reentry guard: every ATOMIC emitted value round-trips wire-int to <see cref="WorkFlowType"/>
    /// through the inbound <see cref="EnumModel.FromValue{TEnumeration}(int)"/> path (the path
    /// <c>RegisterVm</c> and the EF value converter use; since #126 F6 the implicit
    /// <c>operator WorkFlowType(int)</c> routes through the lossless <see cref="WorkFlowType.From(int)"/>
    /// instead), preserving both the <see cref="EnumModel.Value"/> and the named member.
    /// </summary>
    /// <param name="value">The atomic wire integer.</param>
    /// <param name="expectedName">The expected named member after re-entry.</param>
    [Theory]
    [InlineData(0, "None")]
    [InlineData(1, "Initial")]
    [InlineData(2, "Serial")]
    [InlineData(4, "Lateral")]
    [InlineData(8, "Diverter")]
    [InlineData(16, "Merger")]
    [InlineData(32, "Final")]
    public void FromValue_ForAtomicWireInt_RoundTripsLosslessly(int value, string expectedName)
    {
        // Arrange, Act
        var reentered = EnumModel.FromValue<WorkFlowType>(value);

        // Assert
        reentered.Value.ShouldBe(value);
        reentered.Name.ShouldBe(expectedName);
    }

    /// <summary>
    /// <see cref="WorkFlowType.Parallel"/>(64) is a DEFINED atomic member, so a raw 64 re-entering
    /// via the inbound <see cref="EnumModel.FromValue{TEnumeration}(int)"/> path round-trips
    /// losslessly to the named <c>Parallel</c> singleton - even though Release A never emits it.
    /// Pinned to distinguish the "defined-but-unemitted" Parallel from the genuinely lossy
    /// non-atomic composites below.
    /// </summary>
    [Fact]
    public void FromValue_ForParallelWireInt_RoundTripsToParallelSingleton()
    {
        // Arrange, Act
        var reentered = EnumModel.FromValue<WorkFlowType>(64);

        // Assert
        reentered.Value.ShouldBe(64);
        reentered.Name.ShouldBe("Parallel");
    }

    /// <summary>
    /// Pinned known limitation: a NON-ATOMIC composite wire integer (a value not matching any single
    /// defined member) re-entering via the inbound <see cref="EnumModel.FromValue{TEnumeration}(int)"/>
    /// path does NOT round-trip - it collapses to <see cref="WorkFlowType.Invalid"/> (Value -1). This is
    /// exactly why Story 1.2 demands a tested silence proving composites never reach the wire in Release A:
    /// were such a composite ever emitted and then re-read through this path, the value would be lost. The
    /// lossless path is <see cref="WorkFlowType.From(int)"/>, NOT this one - and this test pins that
    /// contrast without changing either.
    /// </summary>
    /// <param name="composite">A non-atomic composite wire integer.</param>
    [Theory]
    [InlineData(18)] // Merger | Serial
    [InlineData(66)] // Parallel | Serial
    [InlineData(127)] // every atomic bit set
    public void FromValue_ForCompositeWireInt_CollapsesToInvalid(int composite)
    {
        // Arrange, Act
        var reentered = EnumModel.FromValue<WorkFlowType>(composite);

        // Assert - the known lossy collapse (the WHY behind the tested silence).
        reentered.Value.ShouldBe(WorkFlowType.Invalid.Value);

        // And the lossless path proves the value is NOT intrinsically lost - only this path loses it.
        WorkFlowType.From(composite).Value.ShouldBe(composite);
    }
}
