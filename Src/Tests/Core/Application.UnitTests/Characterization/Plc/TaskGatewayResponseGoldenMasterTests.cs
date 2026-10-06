// <copyright file="TaskGatewayResponseGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Plc;

/// <summary>
/// Story 32.C1 (AD-10) — §7 golden-master (characterization) harness that PINS the current serialized/projected
/// output of <see cref="TaskGatewayResponse"/> before anyone splits it (32.C2 DTO/entity split, 35.D1 BarCodeId
/// pilot). This file pins TWO of the three wire consumers plus BOTH <c>ToDto</c> sources and
/// <see cref="TaskGatewayResponse.ApplyReferencesValues"/>:
/// <list type="bullet">
/// <item><description><b>Projection consumer</b> — <see cref="BarCodeResultProjection.ToResponse"/> (the shipped
/// field-for-field mirror of <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c>), across the routing matrix
/// (FinishedOk-advance, NotOk-stay, disabled-cascade, end-of-line).</description></item>
/// <item><description><b>Hub payload consumer</b> — the serializer-agnostic public-property SURFACE that SignalR
/// broadcasts to monitors (<c>EventMonitorHub.BroadcastTaskGatewayResponse</c>), plus the exact wire-scalar bytes a
/// monitor reads.</description></item>
/// </list>
/// The third consumer (the persisted EF row) is pinned in the Integration suite
/// (<c>TaskGatewayResponsePersistedRowConfigTests</c> offline model + <c>…PersistedRowGoldenMasterTests</c> real SQL).
/// <para>
/// The values asserted here are LOCKED baselines: the §7 numeric/tag wire is byte-FROZEN, so the split must
/// reproduce every one of these values bit-for-bit. This is the merge gate for 32.C2 / 35.D1 — do NOT "fix"
/// a surprising value; it is pinned AS-IS. All frozen enum integers are the wire bytes the PLC/monitor read.
/// </para>
/// </summary>
public class TaskGatewayResponseGoldenMasterTests
{
    // ---- LOCKED golden fixture (checked-in expected values — the byte-identity baseline) --------------------
    private const int GoldenMachineId = 100;
    private const int GoldenBarCodeId = 555;
    private const int GoldenCycleId = 777;
    private const int GoldenCyclesOk = 3;
    private const int GoldenShiftId = 9;
    private const int GoldenCommandId = 42;
    private const int GoldenLastMachineId = 400;
    private const int GoldenNextMachineId = 500;
    private const string GoldenLabel = "L1AL100003232372501";
    private const string GoldenPartNumber = "PART01";
    private const string GoldenDescription = "desc-golden";
    private const string GoldenError = "";

    // Frozen §7 enum wire integers (see Domain/Enum/*.cs). Locked here so a renumber screams.
    private const int WireResultValidationValid = 1;   // ResultValidation.Valid
    private const int WireCycleStatusFinishedOk = 4;   // CycleStatus.FinishedOk
    private const int WireFlowStatusFinished = 4;      // FlowStatus.Finished
    private const int WirePartStatusOk = 1;            // PartStatus.Ok
    private const int WireMachineTypeFinal = 16;       // MachineType.Final
    private const int WireWorkFlowTypeSerial = 2;      // WorkFlowType.Serial

    /// <summary>
    /// The LOCKED public-property surface of <see cref="TaskGatewayResponse"/> — the exact set of members any
    /// serializer (SignalR JSON, EF, System.Text.Json) projects onto the wire. A split MUST preserve this set;
    /// dropping/renaming a member is a §7 wire break, so this list is frozen.
    /// </summary>
    private static readonly string[] LockedWireProperties =
    {
        "BarCode",
        "BarCodeId",
        "CommandId",
        "Cycle",
        "CycleId",
        "CycleStatus",
        "CyclesOk",
        "Description",
        "Error",
        "ExecutionTime",
        "FlowStatus",
        "Label",
        "LastMachineId",
        "MachineId",
        "MachineType",
        "MasterLabel",
        "Name",
        "NextMachineId",
        "Parameters",
        "PartNumber",
        "PartStatus",
        "PlcId",
        "Recipe",
        "References",
        "RequestTask",
        "ResponseId",
        "ResultValidation",
        "ShiftId",
        "TimeStamp",
        "WorkFlowType",
    };

    // ---- Projection consumer: the routing matrix (mirrors the shipped BarCodeResult golden master) ----------

    /// <summary>
    /// FinishedOk-advance: the projection carries the load-time routing scalars onto the §7 wire — Last = current
    /// station (400), Next = the workflow successor (500). Pins every wire scalar to the locked baseline.
    /// </summary>
    [Fact]
    public void Projection_FinishedOkAdvance_PinsWireScalars()
    {
        var response = BarCodeResultProjection.ToResponse(
            GoldenSnapshot() with { CycleStatus = CycleStatus.FinishedOk, NextMachineId = 500 });

        AssertGoldenWireScalars(response, expectedNextMachineId: 500, expectedCycleStatusWire: WireCycleStatusFinishedOk);
    }

    /// <summary>
    /// NotOk-stay: the part stays on the same station, so Next == the current machine (400). Pinned AS-IS.
    /// </summary>
    [Fact]
    public void Projection_NotOkStay_NextIsCurrentMachine()
    {
        var response = BarCodeResultProjection.ToResponse(
            GoldenSnapshot() with { CycleStatus = CycleStatus.FinishedNok, NextMachineId = GoldenLastMachineId });

        response.NextMachineId.ShouldBe(GoldenLastMachineId);
        response.LastMachineId.ShouldBe(GoldenLastMachineId);
        response.CycleStatus.Value.ShouldBe(8); // FinishedNok wire byte
    }

    /// <summary>
    /// disabled-cascade: the disabled successor is skipped to the successor's successor (600). Pinned AS-IS.
    /// </summary>
    [Fact]
    public void Projection_DisabledCascade_NextSkipsToSuccessorsSuccessor()
    {
        var response = BarCodeResultProjection.ToResponse(
            GoldenSnapshot() with { CycleStatus = CycleStatus.FinishedOk, NextMachineId = 600 });

        response.NextMachineId.ShouldBe(600);
    }

    /// <summary>
    /// end-of-line: the last station forces Next == 0 (no successor). Pinned AS-IS.
    /// </summary>
    [Fact]
    public void Projection_EndOfLine_NextIsZero()
    {
        var response = BarCodeResultProjection.ToResponse(
            GoldenSnapshot() with { CycleStatus = CycleStatus.FinishedOk, NextMachineId = 0 });

        response.NextMachineId.ShouldBe(0);
    }

    /// <summary>
    /// The shipped projection seam is byte-equal to the god-object read path: for the golden fixture,
    /// <see cref="BarCodeResultProjection.ToResponse"/> produces the SAME field values as
    /// <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c>. This is the invariant 32.C2 must preserve.
    /// </summary>
    [Fact]
    public void Projection_MirrorsToDtoFromBarCodeResult_FieldForFieldEqual()
    {
        var viaProjection = BarCodeResultProjection.ToResponse(GoldenSnapshot());
        var viaToDto = TaskGatewayResponseDto.From(GoldenBarCodeResult()).Value.ShouldNotBeNull();

        AssertResponsesFieldEqual(viaProjection, viaToDto);
    }

    // ---- Both ToDto sources -------------------------------------------------------------------------------

    /// <summary>
    /// Pins <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c> — the exact fields it copies from the read source and
    /// their locked values (the §7 read-path projection).
    /// </summary>
    [Fact]
    public void ToDtoFromBarCodeResult_PinsAllCopiedFields()
    {
        var response = TaskGatewayResponseDto.From(GoldenBarCodeResult()).Value.ShouldNotBeNull();

        response.MachineId.ShouldBe(GoldenMachineId);
        response.BarCodeId.ShouldBe(GoldenBarCodeId);
        response.CycleId.ShouldBe(GoldenCycleId);
        response.CyclesOk.ShouldBe(GoldenCyclesOk);
        response.ShiftId.ShouldBe(GoldenShiftId);
        response.CommandId.ShouldBe(GoldenCommandId);
        response.ResultValidation.Value.ShouldBe(WireResultValidationValid);
        string.Equals(response.Error, GoldenError, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(response.Label, GoldenLabel, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(response.PartNumber, GoldenPartNumber, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(response.Description, GoldenDescription, StringComparison.Ordinal).ShouldBeTrue();
        response.LastMachineId.ShouldBe(GoldenLastMachineId);
        response.NextMachineId.ShouldBe(GoldenNextMachineId);
        response.CycleStatus.Value.ShouldBe(WireCycleStatusFinishedOk);
        response.FlowStatus.Value.ShouldBe(WireFlowStatusFinished);
        response.PartStatus.Value.ShouldBe(WirePartStatusOk);
        response.MachineType.Value.ShouldBe(WireMachineTypeFinal);
        response.WorkFlowType.Value.ShouldBe(WireWorkFlowTypeSerial);
        response.References.ShouldNotBeNull();
    }

    /// <summary>
    /// Pins the null-coalescing branches of <c>ToDto(IBarCodeResult)</c>: when the source's nullable string/dictionary
    /// members are null, the wire emits <see cref="string.Empty"/> / an empty dictionary (never null). Frozen §7.
    /// </summary>
    [Fact]
    public void ToDtoFromBarCodeResult_NullSourceStrings_EmitEmptyWireBytes()
    {
        var src = Substitute.For<IBarCodeResult>();
        src.Error.Returns((string?)null);
        src.Label.Returns((string?)null);
        src.PartNumber.Returns((string?)null);
        src.Description.Returns((string?)null);
        src.References.Returns((IDictionary<string, Register>?)null);
        src.ResultValidation.Returns(ResultValidation.None);
        src.CycleStatus.Returns(CycleStatus.None);
        src.FlowStatus.Returns(FlowStatus.None);
        src.PartStatus.Returns(PartStatus.None);
        src.MachineType.Returns(MachineType.None);
        src.WorkFlowType.Returns(WorkFlowType.None);

        var response = TaskGatewayResponseDto.From(src).Value.ShouldNotBeNull();

        response.Error.ShouldBe(string.Empty);
        response.Label.ShouldBe(string.Empty);
        response.PartNumber.ShouldBe(string.Empty);
        response.Description.ShouldBe(string.Empty);
        response.References.ShouldNotBeNull();
        response.References.Count.ShouldBe(0);
    }

    /// <summary>
    /// Pins <c>TaskGatewayResponse.ToDto(TaskGatewayRequest)</c> — the DISTINCT (smaller) field set it copies from the
    /// request source, and that unmapped members keep their construction defaults. Frozen §7.
    /// </summary>
    [Fact]
    public void ToDtoFromTaskGatewayRequest_PinsCopiedFields_AndDefaults()
    {
        var request = new TaskGatewayRequest
        {
            MachineId = GoldenMachineId,
            BarCodeId = GoldenBarCodeId,
            CycleId = GoldenCycleId,
            CommandId = GoldenCommandId,
            PartNumber = GoldenPartNumber,
            Description = GoldenDescription,
            CycleStatus = CycleStatus.FinishedOk,
            FlowStatus = FlowStatus.Finished,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Final,
        };

        var response = TaskGatewayResponseDto.From(request).Value.ShouldNotBeNull();

        // Copied fields.
        response.MachineId.ShouldBe(GoldenMachineId);
        response.BarCodeId.ShouldBe(GoldenBarCodeId);
        response.CycleId.ShouldBe(GoldenCycleId);
        response.CommandId.ShouldBe(GoldenCommandId);
        string.Equals(response.PartNumber, GoldenPartNumber, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(response.Description, GoldenDescription, StringComparison.Ordinal).ShouldBeTrue();
        response.CycleStatus.Value.ShouldBe(WireCycleStatusFinishedOk);
        response.FlowStatus.Value.ShouldBe(WireFlowStatusFinished);
        response.PartStatus.Value.ShouldBe(WirePartStatusOk);
        response.MachineType.Value.ShouldBe(WireMachineTypeFinal);

        // NOT copied by this overload → construction defaults (frozen).
        response.Label.ShouldBe(string.Empty);
        response.Error.ShouldBe(string.Empty);
        response.CyclesOk.ShouldBe(0);
        response.ShiftId.ShouldBe(0);
        response.LastMachineId.ShouldBe(0);
        response.NextMachineId.ShouldBe(0);
        response.ResultValidation.Value.ShouldBe(0); // ResultValidation.None
        response.WorkFlowType.Value.ShouldBe(0);      // WorkFlowType.None
    }

    // ---- ApplyReferencesValues -----------------------------------------------------------------------------

    /// <summary>
    /// Pins <see cref="TaskGatewayResponse.ApplyReferencesValuesResult"/>: it projects the 13 §7 scalar fields into
    /// their matching reference registers as strings, BYTE-FOR-BYTE, rebuilding each register (issue #39 immutable
    /// Register) while preserving all non-value metadata. These 13 wire strings are the LOCKED baseline.
    /// </summary>
    [Fact]
    public void ApplyReferencesValues_ProjectsScalarsIntoRegisters_ByteEqual()
    {
        var response = GoldenResponse() with
        {
            References = BuildReferenceRegisters(
                nameof(TaskGatewayResponseDto.LastMachineId),
                nameof(TaskGatewayResponseDto.NextMachineId),
                nameof(TaskGatewayResponseDto.CycleStatus),
                nameof(TaskGatewayResponseDto.FlowStatus),
                nameof(TaskGatewayResponseDto.PartStatus),
                nameof(TaskGatewayResponseDto.MachineType),
                nameof(TaskGatewayResponseDto.WorkFlowType),
                nameof(TaskGatewayResponseDto.BarCodeId),
                nameof(TaskGatewayResponseDto.CycleId),
                nameof(TaskGatewayResponseDto.Label),
                nameof(TaskGatewayResponseDto.CyclesOk),
                nameof(TaskGatewayResponseDto.ShiftId),
                nameof(TaskGatewayResponseDto.ResultValidation)),
        };

        var applied = ReferenceStamper.Apply(response);

        applied.IsSuccess.ShouldBeTrue();
        var stamped = applied.Value.ShouldNotBeNull();

        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [nameof(TaskGatewayResponseDto.LastMachineId)] = "400",
            [nameof(TaskGatewayResponseDto.NextMachineId)] = "500",
            [nameof(TaskGatewayResponseDto.CycleStatus)] = "4",
            [nameof(TaskGatewayResponseDto.FlowStatus)] = "4",
            [nameof(TaskGatewayResponseDto.PartStatus)] = "1",
            [nameof(TaskGatewayResponseDto.MachineType)] = "16",
            [nameof(TaskGatewayResponseDto.WorkFlowType)] = "2",
            [nameof(TaskGatewayResponseDto.BarCodeId)] = "555",
            [nameof(TaskGatewayResponseDto.CycleId)] = "777",
            [nameof(TaskGatewayResponseDto.Label)] = GoldenLabel,
            [nameof(TaskGatewayResponseDto.CyclesOk)] = "3",
            [nameof(TaskGatewayResponseDto.ShiftId)] = "9",
            [nameof(TaskGatewayResponseDto.ResultValidation)] = "1",
        };

        foreach (var (key, wire) in expected)
        {
            string.Equals(stamped.References[key].Value, wire, StringComparison.Ordinal)
                .ShouldBeTrue($"reference '{key}' should carry the frozen wire byte '{wire}'.");
        }
    }

    /// <summary>
    /// Pins the guarded failures of <see cref="TaskGatewayResponse.ApplyReferencesValuesResult"/>: a null or empty
    /// references dictionary is a <see cref="Result"/> failure (never a throw). Frozen §7 railway behaviour.
    /// </summary>
    [Fact]
    public void ApplyReferencesValues_NullOrEmpty_Fails()
    {
        var emptyRefs = GoldenResponse() with { References = new Dictionary<string, Register>() };
        ReferenceStamper.Apply(emptyRefs).IsFailure.ShouldBeTrue();
    }

    // ---- Hub payload consumer: serializer-agnostic wire surface + scalar bytes -----------------------------

    /// <summary>
    /// Pins the SignalR broadcast SURFACE: the exact set of public readable properties SignalR serializes onto the
    /// monitor wire. A 32.C2 split that drops or renames any of these breaks the Hub payload — this locked set is
    /// the gate.
    /// </summary>
    [Fact]
    public void HubPayload_PublicPropertySurface_IsLocked()
    {
        var actual = typeof(TaskGatewayResponseDto)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 && p.GetMethod is not null)
            .Select(p => p.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        actual.ShouldBe(LockedWireProperties);
    }

    /// <summary>
    /// Pins the exact wire-scalar BYTES a monitor reads from the broadcast payload for the golden fixture: the enum
    /// members serialize as their frozen integer <c>Value</c>, and ids/strings pass through verbatim.
    /// </summary>
    [Fact]
    public void HubPayload_WireScalarBytes_AreLocked()
    {
        var response = GoldenResponse();

        response.MachineId.ShouldBe(GoldenMachineId);
        response.BarCodeId.ShouldBe(GoldenBarCodeId);
        response.CycleId.ShouldBe(GoldenCycleId);
        response.CyclesOk.ShouldBe(GoldenCyclesOk);
        response.ShiftId.ShouldBe(GoldenShiftId);
        response.CommandId.ShouldBe(GoldenCommandId);
        response.LastMachineId.ShouldBe(GoldenLastMachineId);
        response.NextMachineId.ShouldBe(GoldenNextMachineId);
        response.ResultValidation.Value.ShouldBe(WireResultValidationValid);
        response.CycleStatus.Value.ShouldBe(WireCycleStatusFinishedOk);
        response.FlowStatus.Value.ShouldBe(WireFlowStatusFinished);
        response.PartStatus.Value.ShouldBe(WirePartStatusOk);
        response.MachineType.Value.ShouldBe(WireMachineTypeFinal);
        response.WorkFlowType.Value.ShouldBe(WireWorkFlowTypeSerial);
        string.Equals(response.Label, GoldenLabel, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(response.PartNumber, GoldenPartNumber, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(response.Description, GoldenDescription, StringComparison.Ordinal).ShouldBeTrue();
    }

    /// <summary>
    /// Pins <see cref="TaskGatewayResponse.EnsureIsValidToRenderAndPersist"/>: it is the pre-render/persist null-guard
    /// that returns <see langword="true"/> and leaves already-populated enum members intact (no wire mutation). 32.C2
    /// moves this defaulting into construction — the observable output must stay identical.
    /// </summary>
    [Fact]
    public void EnsureIsValidToRenderAndPersist_ReturnsTrue_AndPreservesWireScalars()
    {
        var response = GoldenResponse();

        // 32.C2: the retired EnsureIsValidToRenderAndPersist null-coalesce is now done at DTO construction, so the
        // enums are non-null by construction and the observable wire scalars are unchanged (the pin below is intact).
        //
        // Issue #126 (F3), PO decision 2026-07-15: TaskGatewayRequest.EnsureIsValidToRenderAndPersist is no longer
        // unconditionally true — it fails loud when a required non-enum member (BarCode/PartNumber/Comment/Error)
        // is null after materialization. That change is REQUEST-side only; this DTO pin carries no unconditional-true
        // arm on materialization-null inputs, so every wire-scalar-preservation assertion below stays byte-identical.
        response.CycleStatus.Value.ShouldBe(WireCycleStatusFinishedOk);
        response.FlowStatus.Value.ShouldBe(WireFlowStatusFinished);
        response.PartStatus.Value.ShouldBe(WirePartStatusOk);
        response.MachineType.Value.ShouldBe(WireMachineTypeFinal);
        response.WorkFlowType.Value.ShouldBe(WireWorkFlowTypeSerial);
        response.ResultValidation.Value.ShouldBe(WireResultValidationValid);
    }

    // ---- Fixtures -----------------------------------------------------------------------------------------

    /// <summary>Builds the LOCKED, fully-populated <see cref="BarCodeSnapshot"/> golden fixture.</summary>
    /// <returns>The golden snapshot.</returns>
    private static BarCodeSnapshot GoldenSnapshot() => new()
    {
        MachineId = GoldenMachineId,
        BarCodeId = GoldenBarCodeId,
        CycleId = GoldenCycleId,
        CyclesOk = GoldenCyclesOk,
        ShiftId = GoldenShiftId,
        CommandId = GoldenCommandId,
        ResultValidation = ResultValidation.Valid,
        Error = GoldenError,
        Label = GoldenLabel,
        PartNumber = GoldenPartNumber,
        Description = GoldenDescription,
        LastMachineId = GoldenLastMachineId,
        NextMachineId = GoldenNextMachineId,
        CycleStatus = CycleStatus.FinishedOk,
        FlowStatus = FlowStatus.Finished,
        PartStatus = PartStatus.Ok,
        MachineType = MachineType.Final,
        WorkFlowType = WorkFlowType.Serial,
        Recipe = new Recipe(),
        Cycle = new Cycle(),
        BarCode = null,
        MasterLabel = new MasterLabel(),
        References = new Dictionary<string, Register>(),
    };

    /// <summary>Builds the golden fixture as a <see cref="TaskGatewayResponse"/> (via the pinned projection).</summary>
    /// <returns>The golden response.</returns>
    private static TaskGatewayResponseDto GoldenResponse() => BarCodeResultProjection.ToResponse(GoldenSnapshot());

    /// <summary>Builds an <see cref="IBarCodeResult"/> substitute carrying the LOCKED golden field values.</summary>
    /// <returns>The golden read-source substitute.</returns>
    private static IBarCodeResult GoldenBarCodeResult()
    {
        var src = Substitute.For<IBarCodeResult>();
        src.MachineId.Returns(GoldenMachineId);
        src.BarCodeId.Returns(GoldenBarCodeId);
        src.CycleId.Returns(GoldenCycleId);
        src.CyclesOk.Returns(GoldenCyclesOk);
        src.ShiftId.Returns(GoldenShiftId);
        src.CommandId.Returns(GoldenCommandId);
        src.ResultValidation.Returns(ResultValidation.Valid);
        src.Error.Returns(GoldenError);
        src.Label.Returns(GoldenLabel);
        src.PartNumber.Returns(GoldenPartNumber);
        src.Description.Returns(GoldenDescription);
        src.LastMachineId.Returns(GoldenLastMachineId);
        src.NextMachineId.Returns(GoldenNextMachineId);
        src.CycleStatus.Returns(CycleStatus.FinishedOk);
        src.FlowStatus.Returns(FlowStatus.Finished);
        src.PartStatus.Returns(PartStatus.Ok);
        src.MachineType.Returns(MachineType.Final);
        src.WorkFlowType.Returns(WorkFlowType.Serial);
        src.Recipe.Returns(new Recipe());
        src.Cycle.Returns(new Cycle());
        src.BarCode.Returns((BarCode?)null);
        src.MasterLabel.Returns(new MasterLabel());
        src.References.Returns(new Dictionary<string, Register>());
        return src;
    }

    /// <summary>Builds a references dictionary with one reference register per requested key (value seeded empty).</summary>
    /// <param name="keys">The reference keys to seed.</param>
    /// <returns>The seeded references dictionary.</returns>
    private static IDictionary<string, Register> BuildReferenceRegisters(params string[] keys)
    {
        var references = new Dictionary<string, Register>(StringComparer.Ordinal);
        var registerId = 1;
        foreach (var key in keys)
        {
            var register = Register.Create(
                name: key,
                description: string.Empty,
                machineId: 0,
                variableId: registerId,
                cycleId: 0,
                value: string.Empty,
                dataType: "string",
                statusValueId: 1,
                timeStamp: default,
                registerId: registerId);
            register.IsSuccess.ShouldBeTrue();
            references[key] = register.Value.ShouldNotBeNull();
            registerId++;
        }

        return references;
    }

    /// <summary>Asserts the golden §7 wire scalars on a projected response, allowing the routing next-machine to vary.</summary>
    /// <param name="response">The response under test.</param>
    /// <param name="expectedNextMachineId">The expected next-machine routing scalar for this branch.</param>
    /// <param name="expectedCycleStatusWire">The expected frozen cycle-status wire integer for this branch.</param>
    private static void AssertGoldenWireScalars(TaskGatewayResponseDto response, int expectedNextMachineId, int expectedCycleStatusWire)
    {
        response.MachineId.ShouldBe(GoldenMachineId);
        response.BarCodeId.ShouldBe(GoldenBarCodeId);
        response.CycleId.ShouldBe(GoldenCycleId);
        response.CyclesOk.ShouldBe(GoldenCyclesOk);
        response.ShiftId.ShouldBe(GoldenShiftId);
        response.CommandId.ShouldBe(GoldenCommandId);
        response.LastMachineId.ShouldBe(GoldenLastMachineId);
        response.NextMachineId.ShouldBe(expectedNextMachineId);
        response.CycleStatus.Value.ShouldBe(expectedCycleStatusWire);
        response.FlowStatus.Value.ShouldBe(WireFlowStatusFinished);
        response.PartStatus.Value.ShouldBe(WirePartStatusOk);
        response.MachineType.Value.ShouldBe(WireMachineTypeFinal);
        response.WorkFlowType.Value.ShouldBe(WireWorkFlowTypeSerial);
        response.ResultValidation.Value.ShouldBe(WireResultValidationValid);
        string.Equals(response.Label, GoldenLabel, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(response.PartNumber, GoldenPartNumber, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(response.Description, GoldenDescription, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(response.Error, GoldenError, StringComparison.Ordinal).ShouldBeTrue();
    }

    /// <summary>Asserts two responses are field-for-field equal across the full §7 <c>ToDto</c> projection surface.</summary>
    /// <param name="left">The first response.</param>
    /// <param name="right">The second response.</param>
    private static void AssertResponsesFieldEqual(TaskGatewayResponseDto left, TaskGatewayResponseDto right)
    {
        left.MachineId.ShouldBe(right.MachineId);
        left.BarCodeId.ShouldBe(right.BarCodeId);
        left.CycleId.ShouldBe(right.CycleId);
        left.CyclesOk.ShouldBe(right.CyclesOk);
        left.ShiftId.ShouldBe(right.ShiftId);
        left.CommandId.ShouldBe(right.CommandId);
        left.ResultValidation.Value.ShouldBe(right.ResultValidation.Value);
        string.Equals(left.Error, right.Error, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(left.Label, right.Label, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(left.PartNumber, right.PartNumber, StringComparison.Ordinal).ShouldBeTrue();
        string.Equals(left.Description, right.Description, StringComparison.Ordinal).ShouldBeTrue();
        left.LastMachineId.ShouldBe(right.LastMachineId);
        left.NextMachineId.ShouldBe(right.NextMachineId);
        left.CycleStatus.Value.ShouldBe(right.CycleStatus.Value);
        left.FlowStatus.Value.ShouldBe(right.FlowStatus.Value);
        left.PartStatus.Value.ShouldBe(right.PartStatus.Value);
        left.MachineType.Value.ShouldBe(right.MachineType.Value);
        left.WorkFlowType.Value.ShouldBe(right.WorkFlowType.Value);
    }
}
