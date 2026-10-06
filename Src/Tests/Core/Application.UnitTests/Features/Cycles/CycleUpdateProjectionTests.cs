// <copyright file="CycleUpdateProjectionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Domain.Entities;

/// <summary>
/// Story 6.5 (Task 4) — byte-equality coverage for the Application-layer <see cref="CycleUpdateProjection"/>.
/// The projection must mirror <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c> field-for-field and replay the
/// unified handler's post-DECIDE write-back so the §7 PLC projection stays byte-equal to the god-object path:
/// the OK path keeps the LOAD-TIME scalar status getters (the frozen PLC contract), while the NOT-OK path echoes
/// the DERIVED entity status.
/// </summary>
public class CycleUpdateProjectionTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;
    private const int CycleId = 777;

    // LOAD-TIME scalars for an OK finish: Started / InProcess / Ok. These are SEPARATE from the entity status the
    // strategy mutates to FinishedOk / Finished / Ok.
    private static CycleUpdateLoadState BuildLoadState(
        CycleStatus loadCycleStatus,
        FlowStatus loadFlowStatus,
        PartStatus loadPartStatus,
        Cycle cycle,
        BarCode barCode,
        int cyclesOk = 5)
    {
        return new CycleUpdateLoadState(
            MachineId: MachineId,
            BarCodeId: BarCodeId,
            CycleId: CycleId,
            CyclesOk: cyclesOk,
            ShiftId: 9,
            CommandId: 42,
            ResultValidation: ResultValidation.Valid,
            Error: null,
            Label: "L1AL100003232372501",
            PartNumber: "508",
            Description: "Final Station",
            LastMachineId: 90,
            NextMachineId: MachineId,
            CycleStatus: loadCycleStatus,
            FlowStatus: loadFlowStatus,
            PartStatus: loadPartStatus,
            MachineType: MachineType.Final,
            WorkFlowType: WorkFlowType.Serial,
            Recipe: Recipe.Create(0, 0, 10, 60, 3, 5, 1).Value.ShouldNotBeNull(),
            MasterLabel: new MasterLabel(),
            References: new Dictionary<string, Register>(),
            Cycle: cycle,
            BarCode: barCode,
            Product: Product.CreateFixture(productId: 508, partNumber: "508"));
    }

    [Fact]
    public void ToResponse_OkPath_KeepsLoadTimeScalars_EvenThoughEntityMutatedToFinishedOk()
    {
        // Arrange — load-time scalars are Started / InProcess / Ok (frozen PLC contract for an OK finish), but the
        // DECIDE result's entities carry the mutated FinishedOk / Finished / Ok.
        var resultCycle = new CycleBuilder().FinishedOk(PartStatus.Ok)
            .With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(MachineId); }).Build();
        var resultBarCode = new BarCodeBuilder().Finished(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();

        var load = BuildLoadState(CycleStatus.Started, FlowStatus.InProcess, PartStatus.Ok, resultCycle, resultBarCode);
        var result = new CycleUpdateResult(resultCycle, resultBarCode, RegistersSaved: 0, CyclesOk: 3);

        // Sanity: the result entity really is FinishedOk so we prove the scalar — not the entity — surfaces.
        resultCycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        resultBarCode.FlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);

        // Act
        var dto = CycleUpdateProjection.ToResponse(load, result, CycleStatus.FinishedOk);

        // Assert — LOAD-TIME scalars survive on the OK path (no echo).
        dto.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        dto.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
        dto.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
    }

    [Fact]
    public void ToResponse_NotOkPath_EchoesDerivedEntityStatus()
    {
        // Arrange — load-time scalars are the OK-finish values, but the NOT-OK trigger must echo the derived
        // entity values onto the projection.
        var resultCycle = new CycleBuilder().FinishedNok(PartStatus.NOk)
            .With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(MachineId); }).Build();
        var resultBarCode = new BarCodeBuilder().Rejected(PartStatus.NOk)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();

        var load = BuildLoadState(CycleStatus.Started, FlowStatus.InProcess, PartStatus.Ok, resultCycle, resultBarCode);
        var result = new CycleUpdateResult(resultCycle, resultBarCode, RegistersSaved: 0, CyclesOk: null);

        // Act
        var dto = CycleUpdateProjection.ToResponse(load, result, CycleStatus.FinishedNok);

        // Assert — projected status equals the result entity values (the echo).
        dto.CycleStatus.Value.ShouldBe(resultCycle.CycleStatus.Value);
        dto.FlowStatus.Value.ShouldBe(resultBarCode.FlowStatus.Value);
        dto.PartStatus.Value.ShouldBe(resultBarCode.PartStatus.Value);
        dto.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        dto.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
    }

    [Fact]
    public void ToResponse_MapsAllScalarAndEntityFields_FromLoadAndResult()
    {
        // Arrange
        var resultCycle = new CycleBuilder().FinishedOk(PartStatus.Ok)
            .With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(MachineId); }).Build();
        var resultBarCode = new BarCodeBuilder().Finished(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();

        var load = BuildLoadState(CycleStatus.Started, FlowStatus.InProcess, PartStatus.Ok, resultCycle, resultBarCode);
        var result = new CycleUpdateResult(resultCycle, resultBarCode, RegistersSaved: 0, CyclesOk: 3);

        // Act
        var dto = CycleUpdateProjection.ToResponse(load, result, CycleStatus.FinishedOk);

        // Assert — every ToDto(IBarCodeResult) field maps 1:1.
        dto.MachineId.ShouldBe(load.MachineId);
        dto.BarCodeId.ShouldBe(load.BarCodeId);
        dto.CycleId.ShouldBe(load.CycleId);
        dto.ShiftId.ShouldBe(load.ShiftId);
        dto.CommandId.ShouldBe(load.CommandId);
        dto.ResultValidation.ShouldBe(load.ResultValidation);
        dto.LastMachineId.ShouldBe(load.LastMachineId);
        dto.NextMachineId.ShouldBe(load.NextMachineId);
        dto.MachineType.ShouldBe(load.MachineType);
        dto.WorkFlowType.ShouldBe(load.WorkFlowType);
        dto.Recipe.ShouldBeSameAs(load.Recipe);
        dto.MasterLabel.ShouldBeSameAs(load.MasterLabel);
        dto.References.ShouldBeSameAs(load.References);

        // Entity refs come from the DECIDE result (== the same tracked instances the god-object held).
        dto.Cycle.ShouldBeSameAs(resultCycle);
        dto.BarCode.ShouldBeSameAs(resultBarCode);
    }

    [Fact]
    public void ToResponse_NullCoalescesStringAndReferenceFields_LikeToDto()
    {
        // Arrange — load with null Error / Label / PartNumber / Description and null References.
        var resultCycle = new CycleBuilder().FinishedOk(PartStatus.Ok).Build();
        var resultBarCode = new BarCodeBuilder().Finished(PartStatus.Ok).Build();
        var load = new CycleUpdateLoadState(
            MachineId: MachineId, BarCodeId: BarCodeId, CycleId: CycleId, CyclesOk: 0, ShiftId: 0, CommandId: 0,
            ResultValidation: ResultValidation.Valid, Error: null, Label: null, PartNumber: null, Description: null,
            LastMachineId: 0, NextMachineId: MachineId, CycleStatus: CycleStatus.Started, FlowStatus: FlowStatus.InProcess,
            PartStatus: PartStatus.Ok, MachineType: MachineType.Final, WorkFlowType: WorkFlowType.Serial,
            Recipe: new Recipe(), MasterLabel: new MasterLabel(), References: null!,
            Cycle: resultCycle, BarCode: resultBarCode, Product: new Product());
        var result = new CycleUpdateResult(resultCycle, resultBarCode, RegistersSaved: 0);

        // Act
        var dto = CycleUpdateProjection.ToResponse(load, result, CycleStatus.FinishedOk);

        // Assert — empty strings and a fresh dictionary, exactly like ToDto(IBarCodeResult).
        dto.Error.ShouldBe(string.Empty);
        dto.Label.ShouldBe(string.Empty);
        dto.PartNumber.ShouldBe(string.Empty);
        dto.Description.ShouldBe(string.Empty);
        dto.References.ShouldNotBeNull();
        dto.References.Count.ShouldBe(0);
    }

    [Fact]
    public void ToResponse_CyclesOk_UsesResultWhenPositive_OtherwiseLoadTime()
    {
        // Arrange
        var resultCycle = new CycleBuilder().FinishedOk(PartStatus.Ok).Build();
        var resultBarCode = new BarCodeBuilder().Finished(PartStatus.Ok).Build();
        var load = BuildLoadState(CycleStatus.Started, FlowStatus.InProcess, PartStatus.Ok, resultCycle, resultBarCode, cyclesOk: 5);

        // result CyclesOk > 0 wins (mirrors SetCyclesOk).
        var withPositive = CycleUpdateProjection.ToResponse(
            load, new CycleUpdateResult(resultCycle, resultBarCode, 0, CyclesOk: 7), CycleStatus.FinishedOk);
        withPositive.CyclesOk.ShouldBe(7);

        // result CyclesOk == null keeps the load-time count.
        var withNull = CycleUpdateProjection.ToResponse(
            load, new CycleUpdateResult(resultCycle, resultBarCode, 0, CyclesOk: null), CycleStatus.FinishedOk);
        withNull.CyclesOk.ShouldBe(5);

        // result CyclesOk == 0 (not > 0) keeps the load-time count, matching SetCyclesOk's >0 guard.
        var withZero = CycleUpdateProjection.ToResponse(
            load, new CycleUpdateResult(resultCycle, resultBarCode, 0, CyclesOk: 0), CycleStatus.FinishedOk);
        withZero.CyclesOk.ShouldBe(5);
    }
}
