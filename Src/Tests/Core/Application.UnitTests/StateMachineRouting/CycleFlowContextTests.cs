// <copyright file="CycleFlowContextTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Cycles.Commands.UpdateCycles;
using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Domain.StateMachine;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Issue #176 — pins the accumulating flow-context records the #178/#179 pipeline refactors will thread through
/// the create/update cycle handlers: <see cref="CreateCycleFlowContext"/> and <see cref="UpdateCycleFlowContext"/>
/// accumulate state via <c>with</c> without mutating the source instance, start with every not-yet-loaded member
/// null, and carry the exact shapes the as-built handlers thread through their branches.
/// </summary>
public class CycleFlowContextTests
{
    // =================================================================================================
    // CreateCycleFlowContext (CreateCyclesCommandHandler pipeline state)
    // =================================================================================================

    [Fact]
    public void CreateContext_StartsEmpty()
    {
        var context = new CreateCycleFlowContext();

        context.Request.ShouldBeNull();
        context.Snapshot.ShouldBeNull();
        context.StationValidated.ShouldBeFalse();
        context.CycleLimitDecision.ShouldBeNull();
        context.ResolvedFlowStatus.ShouldBeNull();
        context.ResolvedCycleStatus.ShouldBeNull();
        context.ResolvedPartStatus.ShouldBeNull();
        context.CreatedCycle.ShouldBeNull();
        context.Response.ShouldBeNull();
    }

    [Fact]
    public void CreateContext_WithAccumulation_GrowsStateWithoutMutatingSource()
    {
        var request = new TaskGatewayRequest { MachineId = 100, BarCode = "WS100PART01" };
        var start = new CreateCycleFlowContext { Request = request };

        var loaded = start with { Snapshot = new BarCodeSnapshot { MachineId = 100, BarCodeId = 555 } };
        var validated = loaded with { StationValidated = true };
        var resolved = validated with
        {
            ResolvedFlowStatus = FlowStatus.InProcess,
            ResolvedCycleStatus = CycleStatus.Started,
            ResolvedPartStatus = PartStatus.Ok,
        };
        var created = resolved with { CreatedCycle = new Cycle() };
        var responded = created with { Response = new TaskGatewayResponseDto { MachineId = 100 } };

        // The source instances are untouched at every step.
        start.Snapshot.ShouldBeNull();
        start.StationValidated.ShouldBeFalse();
        loaded.StationValidated.ShouldBeFalse();
        validated.ResolvedFlowStatus.ShouldBeNull();
        resolved.CreatedCycle.ShouldBeNull();
        created.Response.ShouldBeNull();

        // The final instance carries the full accumulated state.
        responded.Request.ShouldBeSameAs(request);
        responded.Snapshot.ShouldNotBeNull().BarCodeId.ShouldBe(555);
        responded.StationValidated.ShouldBeTrue();
        responded.ResolvedFlowStatus.ShouldBe(FlowStatus.InProcess);
        responded.ResolvedCycleStatus.ShouldBe(CycleStatus.Started);
        responded.ResolvedPartStatus.ShouldBe(PartStatus.Ok);
        responded.CreatedCycle.ShouldNotBeNull();
        responded.Response.ShouldNotBeNull().MachineId.ShouldBe(100);
    }

    [Fact]
    public void CreateContext_CarriesCycleLimitDecision()
    {
        var decision = new CycleLimitDecision(false, "max cycles reached", ResultValidation.PartRejected);

        var context = new CreateCycleFlowContext() with { CycleLimitDecision = decision };

        context.CycleLimitDecision.ShouldBeSameAs(decision);
        context.CycleLimitDecision.ShouldNotBeNull().ValidationResult.ShouldBe(ResultValidation.PartRejected);
    }

    // =================================================================================================
    // UpdateCycleFlowContext (UpdateCyclesCommandHandler pipeline state)
    // =================================================================================================

    [Fact]
    public void UpdateContext_StartsEmpty()
    {
        var context = new UpdateCycleFlowContext();

        context.MachineId.ShouldBe(0);
        context.BarCode.ShouldBeNull();
        context.PartNumber.ShouldBeNull();
        context.TargetStatus.ShouldBe(CycleStatus.None);
        context.Trigger.ShouldBe(GatewayTask.None);
        context.Load.ShouldBeNull();
        context.StationValidation.ShouldBeNull();
        context.GateOutcome.ShouldBeNull();
        context.DecideContext.ShouldBeNull();
        context.UpdateResult.ShouldBeNull();
        context.Response.ShouldBeNull();
    }

    [Fact]
    public void UpdateContext_WithAccumulation_GrowsStateWithoutMutatingSource()
    {
        var load = LoadStateFor();
        var start = new UpdateCycleFlowContext
        {
            MachineId = 100,
            BarCode = "WS100PART01",
            PartNumber = "PART01",
            TargetStatus = CycleStatus.FinishedOk,
            Trigger = GatewayTask.UpdateCycleOkAsync,
        };

        var loaded = start with { Load = load };
        var validated = loaded with { StationValidation = new StationValidationResult(true, null, ResultValidation.Valid) };
        var gated = validated with
        {
            GateOutcome = new TransitionOutcome(FlowStatus.InProcess, CycleStatus.FinishedOk, PartStatus.Ok, ResultValidation.Valid),
        };
        var decided = gated with { DecideContext = load.ToDecideContext() };
        var updated = decided with
        {
            UpdateResult = new CycleUpdateResult(load.Cycle, BarCodeFor(), RegistersSaved: 3, CyclesOk: 7),
        };
        var responded = updated with { Response = new TaskGatewayResponseDto { MachineId = 100 } };

        // The source instances are untouched at every step.
        start.Load.ShouldBeNull();
        loaded.StationValidation.ShouldBeNull();
        validated.GateOutcome.ShouldBeNull();
        gated.DecideContext.ShouldBeNull();
        decided.UpdateResult.ShouldBeNull();
        updated.Response.ShouldBeNull();

        // The final instance carries the full accumulated state.
        responded.MachineId.ShouldBe(100);
        responded.TargetStatus.ShouldBe(CycleStatus.FinishedOk);
        responded.Trigger.ShouldBe(GatewayTask.UpdateCycleOkAsync);
        responded.Load.ShouldBeSameAs(load);
        responded.StationValidation.ShouldNotBeNull().CanUpdate.ShouldBeTrue();
        responded.GateOutcome.ShouldNotBeNull().NextCycleStatus.ShouldBe(CycleStatus.FinishedOk);
        responded.DecideContext.ShouldNotBeNull().Cycle.ShouldBeSameAs(load.Cycle);
        responded.UpdateResult.ShouldNotBeNull().RegistersSaved.ShouldBe(3);
        responded.Response.ShouldNotBeNull().MachineId.ShouldBe(100);
    }

    private static CycleUpdateLoadState LoadStateFor() => new(
        MachineId: 100,
        BarCodeId: 555,
        CycleId: 9,
        CyclesOk: 1,
        ShiftId: 2,
        CommandId: 0,
        ResultValidation: ResultValidation.Valid,
        Error: null,
        Label: "WS100PART01",
        PartNumber: "PART01",
        Description: null,
        LastMachineId: 90,
        NextMachineId: 110,
        CycleStatus: CycleStatus.Started,
        FlowStatus: FlowStatus.InProcess,
        PartStatus: PartStatus.Ok,
        MachineType: MachineType.Process,
        WorkFlowType: WorkFlowType.None,
        Recipe: new Recipe(),
        MasterLabel: new MasterLabel(),
        References: new Dictionary<string, Register>(),
        Cycle: new Cycle(),
        BarCode: BarCodeFor(),
        Product: new Product());

    private static BarCode BarCodeFor() => new() { Label = BarCodeLabel.FromPersisted("WS100PART01") };
}
