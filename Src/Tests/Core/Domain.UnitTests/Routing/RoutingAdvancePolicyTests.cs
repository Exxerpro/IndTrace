// <copyright file="RoutingAdvancePolicyTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// #40 (M1): pins that <see cref="RoutingAdvancePolicy"/> — the single-source advance/disabled-cascade rule —
/// reproduces the decision the read path (<c>BarCodeResult.DetermineNextMachineId</c> +
/// <c>UpdateNextMachineIdIfDisabled</c>) makes today, and never throws (failures are <see cref="Result{T}"/>).
/// </summary>
public class RoutingAdvancePolicyTests
{
    // Linear graph: 10 (Initial|Serial) -> 20 (Serial) -> 30 (Final).
    private static ProductionGraph LinearGraph()
    {
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(20, 30, WorkFlowType.Serial),
            new RoutingTransition(30, 0, WorkFlowType.Final),
        ];
        var result = ProductionGraph.Create(transitions);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    // Diverter split: 10 (Initial) -> 20 (Diverter) chooses one of {31, 32}; both Final.
    private static ProductionGraph DiverterGraph()
    {
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.Initial),
            new RoutingTransition(20, 31, WorkFlowType.Diverter),
            new RoutingTransition(20, 32, WorkFlowType.Diverter),
            new RoutingTransition(31, 0, WorkFlowType.Final),
            new RoutingTransition(32, 0, WorkFlowType.Final),
        ];
        var result = ProductionGraph.Create(transitions);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    [Fact]
    public void DetermineNextMachine_FinishedOk_AdvancesToTheSingleSuccessor()
    {
        var next = RoutingAdvancePolicy.DetermineNextMachine(LinearGraph(), 10, CycleStatus.FinishedOk);

        next.IsSuccess.ShouldBeTrue();
        next.Value.ShouldBe(20);
    }

    [Theory]
    [InlineData(nameof(CycleStatus.Started))]
    [InlineData(nameof(CycleStatus.FinishedNok))]
    public void DetermineNextMachine_NotFinishedOk_StaysOnCurrentMachine(string cycleStatusName)
    {
        var status = EnumModel.FromName<CycleStatus>(cycleStatusName);

        var next = RoutingAdvancePolicy.DetermineNextMachine(LinearGraph(), 20, status);

        next.IsSuccess.ShouldBeTrue();
        next.Value.ShouldBe(20); // no advance
    }

    [Fact]
    public void DetermineNextMachine_FinishedOkAtFinalMachine_ReturnsEndOfLineZero()
    {
        var next = RoutingAdvancePolicy.DetermineNextMachine(LinearGraph(), 30, CycleStatus.FinishedOk);

        next.IsSuccess.ShouldBeTrue();
        next.Value.ShouldBe(0); // (Final -> 0) boundary
    }

    [Fact]
    public void DetermineNextMachine_UnknownMachine_ReturnsFailure()
    {
        var next = RoutingAdvancePolicy.DetermineNextMachine(LinearGraph(), 999, CycleStatus.FinishedOk);

        next.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ApplyDisabledCascade_CurrentNotProcess_ReturnsNextUnchanged()
    {
        var result = RoutingAdvancePolicy.ApplyDisabledCascade(
            LinearGraph(), nextMachineId: 20, currentIsProcessMachine: false, MachineType.Process, nextMachineEnabled: false);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(20);
    }

    [Fact]
    public void ApplyDisabledCascade_NextEnabledProcess_ReturnsNextUnchanged()
    {
        var result = RoutingAdvancePolicy.ApplyDisabledCascade(
            LinearGraph(), nextMachineId: 20, currentIsProcessMachine: true, MachineType.Process, nextMachineEnabled: true);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(20);
    }

    [Fact]
    public void ApplyDisabledCascade_NextDisabledProcess_CascadesOneHop()
    {
        var result = RoutingAdvancePolicy.ApplyDisabledCascade(
            LinearGraph(), nextMachineId: 20, currentIsProcessMachine: true, MachineType.Process, nextMachineEnabled: false);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(30); // hop from 20 to its successor 30
    }

    [Fact]
    public void DetermineNextMachine_NullGraph_ReturnsFailureNeverThrows()
    {
        var next = RoutingAdvancePolicy.DetermineNextMachine(graph: null!, 10, CycleStatus.FinishedOk);

        next.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void DetermineNextMachine_FinishedOkAtMultiSuccessorNode_FailsLoud()
    {
        // 56-A: advancing off a Diverter node requires runtime outcome selection (Epic 6); rather than
        // silently pick an arbitrary successor, the policy fails loud so routing never goes non-deterministic.
        var next = RoutingAdvancePolicy.DetermineNextMachine(DiverterGraph(), 20, CycleStatus.FinishedOk);

        next.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ApplyDisabledCascade_HopThroughMultiSuccessorNode_FailsLoud()
    {
        // The disabled Process next machine (20) is a Diverter with two successors; the one-hop cascade
        // cannot deterministically choose a branch, so it fails loud instead of hopping to successors[0].
        var result = RoutingAdvancePolicy.ApplyDisabledCascade(
            DiverterGraph(), nextMachineId: 20, currentIsProcessMachine: true, MachineType.Process, nextMachineEnabled: false);

        result.IsSuccess.ShouldBeFalse();
    }
}
