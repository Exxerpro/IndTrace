// <copyright file="GatewayFailureFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.StateMachine;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Issue #176 — pins the shared <see cref="GatewayFailureFactory"/> against the hand-rolled failure builders it
/// centralizes: the Create-path flag-gated <c>Fail</c>/<c>FailWithoutProjection</c> pair
/// (<c>CreateCyclesCommandHandler</c>) and the UpdateCycles always-carry <c>BuildFailureDto</c> mode. Every
/// carriage mode is exercised with the <c>SpecificDiagnostics</c> flag ON and OFF: flag-gated OFF must collapse to
/// the value-LESS failure (zero-redeploy rollback — the transport re-applies the generic <c>-1</c>), flag-gated ON
/// and always-carry must return a value-CARRYING failure whose DTO holds the specific negative code plus the
/// stamped <c>References["ResultValidation"]</c> register that survives the publish path.
/// </summary>
public class GatewayFailureFactoryTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;
    private const string Error = "something went wrong";

    private static StateMachineRoutingOptions FlagOn() => new();

    private static StateMachineRoutingOptions FlagOff() => new() { SpecificDiagnostics = false };

    private static BarCodeSnapshot SnapshotFor() => new()
    {
        MachineId = MachineId,
        BarCodeId = BarCodeId,
        Label = "WS100PART01",
    };

    // =================================================================================================
    // Flag-gated snapshot overload (CreateCyclesCommandHandler.Fail spec) — representative code spread
    // =================================================================================================

    [Fact]
    public void FailSnapshot_FlagOn_CarriesInvalidMachine() =>
        AssertSnapshotCarriesCode(ResultValidation.InvalidMachine);

    [Fact]
    public void FailSnapshot_FlagOn_CarriesPartRejected() =>
        AssertSnapshotCarriesCode(ResultValidation.PartRejected);

    [Fact]
    public void FailSnapshot_FlagOn_CarriesCycleNotFound() =>
        AssertSnapshotCarriesCode(ResultValidation.CycleNotFound);

    [Fact]
    public void FailSnapshot_FlagOn_CarriesExceptionResultValidation() =>
        AssertSnapshotCarriesCode(ResultValidation.ExceptionResultValidation);

    [Fact]
    public void FailSnapshot_FlagOn_CarriesInfrastructureFailure() =>
        AssertSnapshotCarriesCode(ResultValidation.InfrastructureFailure);

    [Fact]
    public void FailSnapshot_FlagOn_CarriesLoadCarriedDestinationNotValid() =>
        AssertSnapshotCarriesCode(ResultValidation.DestinationNotValid);

    private static void AssertSnapshotCarriesCode(ResultValidation code)
    {
        var result = GatewayFailureFactory.Fail(FlagOn(), Error, SnapshotFor(), code);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(Error);
        var dto = result.Value.ShouldNotBeNull(); // value-CARRYING failure, NOT WithFailure(errors)
        dto.ResultValidation.ShouldBe(code);
        dto.ResultValidation.Value.ShouldBeLessThan(0);
        dto.MachineId.ShouldBe(MachineId);
        dto.BarCodeId.ShouldBe(BarCodeId);
        dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();
    }

    [Fact]
    public void FailSnapshot_FlagOff_CollapsesToValuelessFailure()
    {
        var result = GatewayFailureFactory.Fail(FlagOff(), Error, SnapshotFor(), ResultValidation.InvalidMachine);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(Error);
        result.Value.ShouldBeNull(); // value-less -> transport re-applies the generic -1
    }

    // =================================================================================================
    // Flag-gated existing-projection overload (promote an already-built §7 DTO)
    // =================================================================================================

    [Fact]
    public void FailProjection_FlagOn_PromotesExistingDto()
    {
        var projection = new TaskGatewayResponseDto { MachineId = MachineId, BarCodeId = BarCodeId };

        var result = GatewayFailureFactory.Fail(FlagOn(), Error, projection, ResultValidation.CycleNotFound);

        result.IsFailure.ShouldBeTrue();
        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.CycleNotFound);
        dto.MachineId.ShouldBe(MachineId);
        dto.BarCodeId.ShouldBe(BarCodeId);
        dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();
    }

    [Fact]
    public void FailProjection_FlagOff_CollapsesToValuelessFailure()
    {
        var projection = new TaskGatewayResponseDto { MachineId = MachineId };

        var result = GatewayFailureFactory.Fail(FlagOff(), Error, projection, ResultValidation.CycleNotFound);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    // =================================================================================================
    // Flag-gated no-projection overload (CreateCyclesCommandHandler.FailWithoutProjection spec)
    // =================================================================================================

    [Fact]
    public void FailWithoutProjection_FlagOn_BuildsDiagnosticValue()
    {
        var result = GatewayFailureFactory.FailWithoutProjection(
            FlagOn(), Error, MachineId, ResultValidation.InfrastructureFailure);

        result.IsFailure.ShouldBeTrue();
        var dto = result.Value.ShouldNotBeNull();
        dto.MachineId.ShouldBe(MachineId);
        dto.ResultValidation.ShouldBe(ResultValidation.InfrastructureFailure);
        dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();
    }

    [Fact]
    public void FailWithoutProjection_FlagOff_CollapsesToValuelessFailure()
    {
        var result = GatewayFailureFactory.FailWithoutProjection(
            FlagOff(), Error, MachineId, ResultValidation.InfrastructureFailure);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void FailWithoutProjection_FlagOn_PassesReferencesThrough()
    {
        var references = ReferencesWith("SomeRegister");

        var result = GatewayFailureFactory.FailWithoutProjection(
            FlagOn(), Error, MachineId, ResultValidation.ShiftInvalid, references);

        var dto = result.Value.ShouldNotBeNull();
        dto.References.ContainsKey("SomeRegister").ShouldBeTrue(); // caller's registers survive
        dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();
        dto.ResultValidation.ShouldBe(ResultValidation.ShiftInvalid);
    }

    // =================================================================================================
    // Always-carry mode (UpdateCyclesCommandHandler.BuildFailureDto spec — intentionally ignores the flag)
    // =================================================================================================

    [Fact]
    public void FailAlways_CarriesValueWithoutAnyFlag()
    {
        // The always-carry mode has NO routing parameter at all: the UpdateCycles as-built pin is that the DTO
        // is ALWAYS carried, so the mode cannot even observe SpecificDiagnostics = false.
        var result = GatewayFailureFactory.FailAlways("Cycle not Found", MachineId);

        result.IsFailure.ShouldBeTrue();
        var dto = result.Value.ShouldNotBeNull();
        dto.MachineId.ShouldBe(MachineId);
        dto.ResultValidation.ShouldBe(ResultValidation.CycleNotFound); // Classify fallback
        dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();
    }

    [Fact]
    public void FailAlways_ExplicitCodeOverridesClassification()
    {
        // "Cycle not Found" would classify to CycleNotFound; the explicit code must win (station-validator path).
        var result = GatewayFailureFactory.FailAlways(
            "Cycle not Found", MachineId, explicitCode: ResultValidation.DestinationNotValid);

        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.DestinationNotValid);
    }

    [Fact]
    public void FailAlways_UnmatchedMessageFallsBackToInvalid()
    {
        var result = GatewayFailureFactory.FailAlways("totally unmapped reason", MachineId);

        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.Invalid);
    }

    [Fact]
    public void FailAlways_ErrorsList_PreservesAllErrorsAndClassifiesFirst()
    {
        var errors = new[] { "Cycle not Found", "secondary detail" };

        var result = GatewayFailureFactory.FailAlways(errors, MachineId);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Cycle not Found");
        result.Errors.ShouldContain("secondary detail");
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.CycleNotFound);
    }

    [Fact]
    public void FailAlways_PassesReferencesThrough()
    {
        var references = ReferencesWith("PlcRegister");

        var result = GatewayFailureFactory.FailAlways("Cannot create Shift", MachineId, references: references);

        var dto = result.Value.ShouldNotBeNull();
        dto.ResultValidation.ShouldBe(ResultValidation.ShiftInvalid);
        dto.References.ContainsKey("PlcRegister").ShouldBeTrue();
    }

    // =================================================================================================
    // GatewayFault-accepting overloads (pipeline steps return a fault; the terminal maps it)
    // =================================================================================================

    [Fact]
    public void FailFault_Snapshot_FlagOn_UsesFaultMessageAndCode()
    {
        var fault = new GatewayFault("station refused", ResultValidation.InvalidMachine);

        var result = GatewayFailureFactory.Fail(FlagOn(), fault, SnapshotFor());

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("station refused");
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.InvalidMachine);
    }

    [Fact]
    public void FailFault_Snapshot_FlagOff_CollapsesToValuelessFailure()
    {
        var fault = new GatewayFault("station refused", ResultValidation.InvalidMachine);

        var result = GatewayFailureFactory.Fail(FlagOff(), fault, SnapshotFor());

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void FailWithoutProjectionFault_FlagOn_UsesFaultMessageAndCode()
    {
        var fault = new GatewayFault("load blew up", ResultValidation.InfrastructureFailure);

        var result = GatewayFailureFactory.FailWithoutProjection(FlagOn(), fault, MachineId);

        result.Errors.ShouldContain("load blew up");
        var dto = result.Value.ShouldNotBeNull();
        dto.MachineId.ShouldBe(MachineId);
        dto.ResultValidation.ShouldBe(ResultValidation.InfrastructureFailure);
    }

    [Fact]
    public void FailAlwaysFault_FaultCodeWinsOverClassification()
    {
        // The fault's message would classify to CycleNotFound; the fault's own code is explicit and must win.
        var fault = new GatewayFault("Cycle not Found", ResultValidation.PartRejected);

        var result = GatewayFailureFactory.FailAlways(fault, MachineId);

        result.Errors.ShouldContain("Cycle not Found");
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.PartRejected);
    }

    // =================================================================================================
    // Pure DTO builder (UpdateCycles BuildFailureDto parity)
    // =================================================================================================

    [Fact]
    public void BuildFailureDto_ClassifiesMessageAndStampsReferences()
    {
        var dto = GatewayFailureFactory.BuildFailureDto("BarCode not Found", MachineId);

        dto.MachineId.ShouldBe(MachineId);
        dto.ResultValidation.ShouldBe(ResultValidation.BarCodeNotFound);
        dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();
    }

    [Fact]
    public void BuildFailureDto_ExplicitCodeAndReferencesPassThrough()
    {
        var references = ReferencesWith("Echo");

        var dto = GatewayFailureFactory.BuildFailureDto(
            "unmapped", MachineId, explicitCode: ResultValidation.RecipeNotFound, references: references);

        dto.ResultValidation.ShouldBe(ResultValidation.RecipeNotFound);
        dto.References.ContainsKey("Echo").ShouldBeTrue();
    }

    private static IDictionary<string, Register> ReferencesWith(string key)
    {
        var registerResult = Register.Create(
            name: key,
            description: string.Empty,
            machineId: MachineId,
            variableId: 0,
            cycleId: 0,
            value: "1",
            dataType: "int",
            statusValueId: 0,
            timeStamp: default);

        registerResult.IsSuccess.ShouldBeTrue();
        var register = registerResult.Value.ShouldNotBeNull();
        return new Dictionary<string, Register> { [key] = register };
    }
}
