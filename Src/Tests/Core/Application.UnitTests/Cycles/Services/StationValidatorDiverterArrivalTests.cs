// <copyright file="StationValidatorDiverterArrivalTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UnitTests.Cycles.Services;

using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// E6-1 (#56): pins the arrival gate's shift from equality against a single computed next machine to MEMBERSHIP
/// in a context-derived legal-arrival set (<see cref="StationValidator"/> :89). On a genuine multi-successor
/// diverter (<c>10 -&gt; 20</c>, <c>20 -&gt; {31, 32}</c>) an arrival report at EITHER successor is legal, a
/// report from a non-successor is rejected with the unchanged <see cref="ResultValidation.DestinationNotValid"/>
/// code, and nothing is mutated on rejection. The empty-set fallback preserves the exact legacy equality
/// (byte-identity on all linear data), which is proven separately by the linear regression cases below and the
/// full golden-master / parity suites.
/// </summary>
public class StationValidatorDiverterArrivalTests
{
    private static StationValidator NewValidator() =>
        new(Substitute.For<ILogger<StationValidator>>());

    // Builds a cycle-update load state at a station, carrying the E6-1 legal-arrival set. The validator's arrival
    // gate reads only MachineType / NextMachineId / LegalArrivalMachines / Cycle.MachineId / Cycle.CycleStatus.
    private static CycleUpdateLoadState BuildLoad(
        MachineType machineType,
        int nextMachineId,
        LegalNextMachines legalArrivalMachines,
        int cycleMachineId,
        CycleStatus cycleStatus)
    {
        var cycle = new CycleBuilder()
            .AtState(cycleStatus, PartStatus.Ok)
            .With(c => c.MachineId = new MachineId(cycleMachineId))
            .Build();

        return new CycleUpdateLoadState(
            MachineId: 0, BarCodeId: 0, CycleId: 0, CyclesOk: 0, ShiftId: 0, CommandId: 0,
            ResultValidation: ResultValidation.Valid, Error: null, Label: null, PartNumber: null,
            Description: null, LastMachineId: 20, NextMachineId: nextMachineId,
            CycleStatus: cycleStatus, FlowStatus: FlowStatus.InProcess, PartStatus: PartStatus.Ok,
            MachineType: machineType, WorkFlowType: WorkFlowType.Serial, Recipe: new Recipe(),
            MasterLabel: new MasterLabel(), References: new Dictionary<string, Register>(),
            Cycle: cycle, BarCode: null, Product: new Product(),
            LegalArrivalMachines: legalArrivalMachines);
    }

    // Diverter split: last machine 20 legally advances to either 31 or 32 (both Final). The singular
    // NextMachineId is deliberately UNRESOLVABLE on a diverter (56-A fails loud) — here it stays on 20 — so the
    // legacy equality would wrongly reject BOTH real successors; membership accepts them.
    private static LegalNextMachines DiverterSuccessors() =>
        new([new MachineId(31), new MachineId(32)]);

    [Theory]
    [InlineData(31)]
    [InlineData(32)]
    public void ValidateStation_DiverterArrivalAtEitherSuccessor_IsAccepted(int requestingMachineId)
    {
        // Arrange — a Started cycle at the requesting (successor) machine; the part legally arrived here from the
        // diverter at 20. NextMachineId is the unresolved 20 (would fail the legacy equality).
        var load = BuildLoad(
            machineType: MachineType.Process,
            nextMachineId: 20,
            legalArrivalMachines: DiverterSuccessors(),
            cycleMachineId: requestingMachineId,
            cycleStatus: CycleStatus.Started);
        var validator = NewValidator();

        // Act
        var result = validator.ValidateStation(requestingMachineId, CycleStatus.FinishedOk, load);

        // Assert — membership accepts the legal arrival that the singular equality would have rejected.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBeTrue();
        result.Value.FailureReason.ShouldBeNull();
        result.Value.Validation.ShouldBe(ResultValidation.Valid);
    }

    [Fact]
    public void ValidateStation_DiverterArrivalAtNonSuccessor_IsRejectedWithDestinationNotValid()
    {
        // Arrange — a request from 99, which is NOT a legal successor of the diverter at 20.
        const int nonSuccessor = 99;
        var load = BuildLoad(
            machineType: MachineType.Process,
            nextMachineId: 20,
            legalArrivalMachines: DiverterSuccessors(),
            cycleMachineId: nonSuccessor,
            cycleStatus: CycleStatus.Started);
        var validator = NewValidator();

        var cycleStatusBefore = load.Cycle.CycleStatus;
        var cycleMachineBefore = load.Cycle.MachineId;

        // Act
        var result = validator.ValidateStation(nonSuccessor, CycleStatus.FinishedOk, load);

        // Assert — rejected with the SAME code/message as the legacy equality, as a fail-loud Result (no throw).
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBeFalse();
        result.Value.FailureReason.ShouldNotBeNull();
        result.Value.FailureReason.ShouldContain("created on another station");
        result.Value.Validation.ShouldBe(ResultValidation.DestinationNotValid);

        // Nothing is mutated on a rejected arrival (the validator is pure).
        load.Cycle.CycleStatus.ShouldBe(cycleStatusBefore);
        load.Cycle.MachineId.ShouldBe(cycleMachineBefore);
    }

    [Fact]
    public void ValidateStation_StayAtCurrentMachine_ValidatesAsBefore()
    {
        // Arrange — a not-FinishedOk (Started) request at the current machine; the singleton legal-arrival set is
        // exactly { 100 }, so membership is byte-identical to the legacy equality.
        var load = BuildLoad(
            machineType: MachineType.Process,
            nextMachineId: 100,
            legalArrivalMachines: new LegalNextMachines([new MachineId(100)]),
            cycleMachineId: 100,
            cycleStatus: CycleStatus.Started);
        var validator = NewValidator();

        // Act
        var result = validator.ValidateStation(100, CycleStatus.FinishedOk, load);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBeTrue();
        result.Value.Validation.ShouldBe(ResultValidation.Valid);
    }

    [Theory]
    [InlineData(100, true)]   // singleton { 100 }: request 100 accepted (== legacy equality pass)
    [InlineData(200, false)]  // singleton { 200 }: request 100 rejected (== legacy equality miss)
    public void ValidateStation_SingletonSet_IsByteIdenticalToLegacyEquality(int nextMachineId, bool expectedCanUpdate)
    {
        // Arrange — a populated singleton set { nextMachineId }; membership must reproduce `nextMachineId == 100`.
        var load = BuildLoad(
            machineType: MachineType.Process,
            nextMachineId: nextMachineId,
            legalArrivalMachines: new LegalNextMachines([new MachineId(nextMachineId)]),
            cycleMachineId: 100,
            cycleStatus: CycleStatus.Started);
        var validator = NewValidator();

        // Act
        var result = validator.ValidateStation(100, CycleStatus.FinishedOk, load);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBe(expectedCanUpdate);
        if (!expectedCanUpdate)
        {
            result.Value.Validation.ShouldBe(ResultValidation.DestinationNotValid);
        }
    }

    [Theory]
    [InlineData(100, true)]   // empty set -> fallback equality: 100 == 100 pass
    [InlineData(200, false)]  // empty set -> fallback equality: 200 != 100 reject
    public void ValidateStation_EmptySet_FallsBackToLegacyEquality(int nextMachineId, bool expectedCanUpdate)
    {
        // Arrange — a legacy load with the DEFAULT (empty) set; the arrival gate must fall back to the exact
        // legacy `nextMachineId == machineId` equality so nothing changes for pre-E6-1 constructions.
        var load = BuildLoad(
            machineType: MachineType.Process,
            nextMachineId: nextMachineId,
            legalArrivalMachines: default,
            cycleMachineId: 100,
            cycleStatus: CycleStatus.Started);
        var validator = NewValidator();

        // Act
        var result = validator.ValidateStation(100, CycleStatus.FinishedOk, load);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBe(expectedCanUpdate);
        if (!expectedCanUpdate)
        {
            result.Value.Validation.ShouldBe(ResultValidation.DestinationNotValid);
        }
    }
}
