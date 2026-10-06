// <copyright file="ProductRoutingStateTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// E6-1a (#56): pins the pure-domain arrival-validation foundation — the role value-types, the
/// <see cref="LegalNextMachines"/> membership yardstick, and the <see cref="ProductRoutingState.FromGraph"/>
/// factory that enforces invariants I1–I6. Additive and behaviour-preserving: nothing consumes these types
/// yet, so no existing golden/characterization master moves. Validation becomes <b>membership</b> in the
/// legal-successor set rather than equality against a single computed next.
/// </summary>
public class ProductRoutingStateTests
{
    // Linear graph: 10 (Initial|Serial) -> 20 (Serial) -> 30 (Final). Every node has exactly one successor,
    // so LegalNextMachines has size 1 and membership is byte-identical to the legacy equality check.
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

    // Diverter split: 10 (Initial) -> 20 (Diverter) chooses one of {31, 32}; both Final. Node 20 legally has
    // TWO possible next machines — the case equality-against-one cannot express.
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

    // Diverter split with a cascade target on one branch: 10 (Initial) -> 20 (Diverter) chooses {31, 32};
    // branch 31 (Serial) has a single successor 41 (Final), so NextMachine(31) == 41 is the one-hop cascade
    // target when 31 is a DISABLED Process machine; branch 32 (Final) is a leaf. This is the #60 fixture: a
    // disabled Process diverter branch must be folded to its cascade target across the legal set.
    private static ProductionGraph DiverterGraphWithCascadeTarget()
    {
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(10, 20, WorkFlowType.Initial),
            new RoutingTransition(20, 31, WorkFlowType.Diverter),
            new RoutingTransition(20, 32, WorkFlowType.Diverter),
            new RoutingTransition(31, 41, WorkFlowType.Serial),
            new RoutingTransition(41, 0, WorkFlowType.Final),
            new RoutingTransition(32, 0, WorkFlowType.Final),
        ];
        var result = ProductionGraph.Create(transitions);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    private static IReadOnlyDictionary<int, SuccessorMachineMetadata> Metadata(
        params (int Id, MachineType Type, bool Enabled)[] entries) =>
        entries.ToDictionary(e => e.Id, e => new SuccessorMachineMetadata(e.Type, e.Enabled));

    private static IReadOnlyDictionary<int, SuccessorMachineMetadata> EmptyMetadata() =>
        new Dictionary<int, SuccessorMachineMetadata>();

    private static LastProcessedMachine Last(int id) => new(new MachineId(id));

    private static RequestingMachine Requesting(int id) => new(new MachineId(id));

    [Fact]
    public void FromGraph_Linear_LegalNextIsSingletonAndOnlySuccessorIsLegal()
    {
        var state = ProductRoutingState.FromGraph(LinearGraph(), Last(20), CycleStatus.FinishedOk, FlowStatus.InProcess)
            .Value.ShouldNotBeNull();

        // I3: exactly one legal successor on linear routing.
        state.LegalNextMachines.Count.ShouldBe(1);
        state.LegalNextMachines.Contains(new MachineId(30)).ShouldBeTrue();

        // I4: only the single successor is a legal arrival; anything else is illegal.
        state.IsLegalArrival(Requesting(30)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(20)).ShouldBeFalse();
        state.IsLegalArrival(Requesting(99)).ShouldBeFalse();

        // I5: on linear FinishedOk the advisory advances to the single successor.
        state.AdvisoryNextMachine.ShouldNotBeNull();
        state.AdvisoryNextMachine.Value.Value.ShouldBe(new MachineId(30));
    }

    [Fact]
    public void Permits_Linear_MatchesMembershipDirectly()
    {
        var legal = ProductRoutingState.FromGraph(LinearGraph(), Last(10), CycleStatus.Started, FlowStatus.InProcess)
            .Value.ShouldNotBeNull().LegalNextMachines;

        legal.Count.ShouldBe(1);
        legal.Permits(Requesting(20)).ShouldBeTrue();
        legal.Permits(Requesting(30)).ShouldBeFalse();
    }

    [Fact]
    public void FromGraph_Diverter_BothSuccessorsAreLegalNonSuccessorIsIllegal()
    {
        // A part in-process at the diverter (Started): the singular advisory stays on 20, but the legal SET
        // still carries BOTH branches — the exact multi-successor case E6-1 makes validatable.
        var state = ProductRoutingState.FromGraph(DiverterGraph(), Last(20), CycleStatus.Started, FlowStatus.InProcess)
            .Value.ShouldNotBeNull();

        state.LegalNextMachines.Count.ShouldBe(2);
        state.LegalNextMachines.Contains(new MachineId(31)).ShouldBeTrue();
        state.LegalNextMachines.Contains(new MachineId(32)).ShouldBeTrue();

        // I4: either branch is a legal arrival; a non-successor is rejected.
        state.IsLegalArrival(Requesting(31)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(32)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(99)).ShouldBeFalse();
    }

    [Fact]
    public void FromGraph_Diverter_FinishedOk_BuildsWithLegalSetButUnresolvableAdvisory()
    {
        // The whole point of E6-1: at FinishedOk on a diverter the SINGULAR advisory is unresolvable
        // (56-A fails loud, deferred to E6-2), yet the legal-successor SET is still available for validation.
        // The state must build (never fail merely because the singular hint cannot pick a branch).
        var state = ProductRoutingState.FromGraph(DiverterGraph(), Last(20), CycleStatus.FinishedOk, FlowStatus.InProcess)
            .Value.ShouldNotBeNull();

        state.AdvisoryNextMachine.ShouldBeNull(); // singular hint unresolvable on a diverter
        state.LegalNextMachines.Count.ShouldBe(2);
        state.IsLegalArrival(Requesting(31)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(32)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(99)).ShouldBeFalse();
    }

    [Fact]
    public void FromGraph_FinalNode_LegalNextIsTerminalZeroAndAdvisoryResolvesToZero()
    {
        // I3: NextMachines(30) is EMPTY (the (Final -> 0) edge carries no successor node); the factory adds
        // the terminal 0 sentinel — at end-of-line the only legal "next" is to leave the line.
        var state = ProductRoutingState.FromGraph(LinearGraph(), Last(30), CycleStatus.FinishedOk, FlowStatus.Finished)
            .Value.ShouldNotBeNull();

        state.LegalNextMachines.Count.ShouldBe(1);
        state.LegalNextMachines.Contains(new MachineId(0)).ShouldBeTrue();

        // I5: advisory resolves to 0 at end-of-line, which is a member of the legal set.
        state.AdvisoryNextMachine.ShouldNotBeNull();
        state.AdvisoryNextMachine.Value.Value.ShouldBe(new MachineId(0));

        // No real machine can legally arrive after a Final node.
        state.IsLegalArrival(Requesting(30)).ShouldBeFalse();
        state.IsLegalArrival(Requesting(99)).ShouldBeFalse();
    }

    [Fact]
    public void FromGraph_LastProcessedNotANode_FailsLoudNeverThrows()
    {
        // I2: a last-processed machine that is not a graph node is a fail-loud Result failure (no throw).
        var result = ProductRoutingState.FromGraph(LinearGraph(), Last(999), CycleStatus.FinishedOk, FlowStatus.InProcess);

        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void FromGraph_LastProcessedIsWireSentinelZero_FailsLoud()
    {
        // I1: machine 0 is the wire boundary sentinel, never a real routing node.
        var result = ProductRoutingState.FromGraph(LinearGraph(), Last(0), CycleStatus.FinishedOk, FlowStatus.InProcess);

        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void FromGraph_NullArguments_FailLoudNeverThrow()
    {
        ProductRoutingState.FromGraph(graph: null!, Last(20), CycleStatus.FinishedOk, FlowStatus.InProcess)
            .IsSuccess.ShouldBeFalse();
        ProductRoutingState.FromGraph(LinearGraph(), Last(20), cycleStatus: null!, FlowStatus.InProcess)
            .IsSuccess.ShouldBeFalse();
        ProductRoutingState.FromGraph(LinearGraph(), Last(20), CycleStatus.FinishedOk, flowStatus: null!)
            .IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void EmptyLegalNextMachines_PermitsNothing()
    {
        // An empty legal set is allowed (end-of-line) and permits no arrival.
        var empty = new LegalNextMachines(new List<MachineId>().AsReadOnly());

        empty.Count.ShouldBe(0);
        empty.Permits(Requesting(31)).ShouldBeFalse();
        empty.Contains(new MachineId(0)).ShouldBeFalse();
    }

    [Fact]
    public void RoleTypes_WrappingSameMachineId_AreDistinctAndNonInterchangeable()
    {
        // I6: RequestingMachine and LastProcessedMachine wrap the SAME MachineId value (they coincide by
        // state) but are DISTINCT types that cannot be silently interchanged.
        var sameId = new MachineId(100);
        var requesting = new RequestingMachine(sameId);
        var last = new LastProcessedMachine(sameId);

        // Value coincidence by state (I6): the wrapped ids are equal...
        requesting.Value.ShouldBe(last.Value);

        // ...but the wrapper types are distinct, so the compiler treats them as unrelated. `IsLegalArrival`
        // takes a RequestingMachine; a LastProcessedMachine could not be passed to it (compile error), which
        // is the load-bearing anti-transposition guarantee. We prove non-interchangeability at runtime by
        // asserting NO conversion operator exists on any role type (a single implicit/explicit operator is
        // exactly what would re-open the swapped-argument hole).
        typeof(RequestingMachine).ShouldNotBe(typeof(LastProcessedMachine));
        AssertNoConversionOperators(typeof(RequestingMachine));
        AssertNoConversionOperators(typeof(LastProcessedMachine));
        AssertNoConversionOperators(typeof(AdvisoryNextMachine));
    }

    [Fact]
    public void FromGraph_4Arg_StillRawSuccessorSet()
    {
        // #60 regression guard: the 4-arg overload forwards to the 6-arg with empty metadata + false, so it
        // must produce EXACTLY the same raw successor set and advisory as the 6-arg with empty metadata.
        var fourArg = ProductRoutingState.FromGraph(
                LinearGraph(), Last(20), CycleStatus.FinishedOk, FlowStatus.InProcess)
            .Value.ShouldNotBeNull();

        var sixArgEmpty = ProductRoutingState.FromGraph(
                LinearGraph(), Last(20), CycleStatus.FinishedOk, FlowStatus.InProcess,
                EmptyMetadata(), currentIsProcessMachine: false)
            .Value.ShouldNotBeNull();

        fourArg.LegalNextMachines.Count.ShouldBe(sixArgEmpty.LegalNextMachines.Count);
        fourArg.LegalNextMachines.Contains(new MachineId(30)).ShouldBeTrue();
        sixArgEmpty.LegalNextMachines.Contains(new MachineId(30)).ShouldBeTrue();
        fourArg.AdvisoryNextMachine.ShouldNotBeNull();
        sixArgEmpty.AdvisoryNextMachine.ShouldNotBeNull();
        fourArg.AdvisoryNextMachine.Value.Value.ShouldBe(sixArgEmpty.AdvisoryNextMachine.Value.Value);
    }

    [Fact]
    public void FromGraph_6Arg_Linear_EnabledSuccessor_NoCascade()
    {
        // An ENABLED Process successor does not cascade: the singleton legal set is the raw successor.
        var state = ProductRoutingState.FromGraph(
                LinearGraph(), Last(20), CycleStatus.FinishedOk, FlowStatus.InProcess,
                Metadata((30, MachineType.Process, Enabled: true)), currentIsProcessMachine: true)
            .Value.ShouldNotBeNull();

        state.LegalNextMachines.Count.ShouldBe(1);
        state.LegalNextMachines.Contains(new MachineId(30)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(30)).ShouldBeTrue();
    }

    [Fact]
    public void FromGraph_6Arg_Linear_DisabledProcessSuccessor_CascadesOneHop()
    {
        // last = 10, its successor 20 is a DISABLED Process machine => the legal set folds to 20's one-hop
        // cascade target (NextMachine(20) == 30), and the advisory is folded through the SAME resolver so I5
        // holds against the folded set.
        var state = ProductRoutingState.FromGraph(
                LinearGraph(), Last(10), CycleStatus.FinishedOk, FlowStatus.InProcess,
                Metadata((20, MachineType.Process, Enabled: false)), currentIsProcessMachine: true)
            .Value.ShouldNotBeNull();

        state.LegalNextMachines.Count.ShouldBe(1);
        state.LegalNextMachines.Contains(new MachineId(30)).ShouldBeTrue();
        state.LegalNextMachines.Contains(new MachineId(20)).ShouldBeFalse();
        state.IsLegalArrival(Requesting(30)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(20)).ShouldBeFalse();

        // Advisory folded to the cascade target 30 (I5 passes against the folded legal set).
        state.AdvisoryNextMachine.ShouldNotBeNull();
        state.AdvisoryNextMachine.Value.Value.ShouldBe(new MachineId(30));
    }

    [Fact]
    public void FromGraph_6Arg_Diverter_OneBranchDisabled_CascadesThatBranchOnly()
    {
        // Diverter at 20 => {31, 32}; branch 31 is a DISABLED Process machine (cascades one hop to 41),
        // branch 32 is enabled (unchanged). The gap #60 closes: the now-skipped disabled 31 is NOT a legal
        // arrival; its cascade target 41 and the untouched 32 are.
        var state = ProductRoutingState.FromGraph(
                DiverterGraphWithCascadeTarget(), Last(20), CycleStatus.FinishedOk, FlowStatus.InProcess,
                Metadata(
                    (31, MachineType.Process, Enabled: false),
                    (32, MachineType.Process, Enabled: true)),
                currentIsProcessMachine: true)
            .Value.ShouldNotBeNull();

        state.LegalNextMachines.Count.ShouldBe(2);
        state.LegalNextMachines.Contains(new MachineId(41)).ShouldBeTrue();
        state.LegalNextMachines.Contains(new MachineId(32)).ShouldBeTrue();

        state.IsLegalArrival(Requesting(41)).ShouldBeTrue();  // the cascade target
        state.IsLegalArrival(Requesting(32)).ShouldBeTrue();  // the untouched enabled branch
        state.IsLegalArrival(Requesting(31)).ShouldBeFalse(); // the closed gap: skipped disabled branch
    }

    [Fact]
    public void FromGraph_6Arg_Diverter_currentNotProcess_NoCascade()
    {
        // currentIsProcessMachine == false => every cascade is a no-op, so the raw diverter set is emitted.
        var state = ProductRoutingState.FromGraph(
                DiverterGraphWithCascadeTarget(), Last(20), CycleStatus.FinishedOk, FlowStatus.InProcess,
                Metadata(
                    (31, MachineType.Process, Enabled: false),
                    (32, MachineType.Process, Enabled: true)),
                currentIsProcessMachine: false)
            .Value.ShouldNotBeNull();

        state.LegalNextMachines.Count.ShouldBe(2);
        state.LegalNextMachines.Contains(new MachineId(31)).ShouldBeTrue();
        state.LegalNextMachines.Contains(new MachineId(32)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(31)).ShouldBeTrue();
    }

    [Fact]
    public void FromGraph_6Arg_MissingMetadata_LeavesSuccessorUncascaded()
    {
        // last = 10, successor 20 would cascade IF it had (Process, disabled) metadata, but it is absent from
        // the map => it is emitted RAW (the safe no-op default).
        var state = ProductRoutingState.FromGraph(
                LinearGraph(), Last(10), CycleStatus.FinishedOk, FlowStatus.InProcess,
                EmptyMetadata(), currentIsProcessMachine: true)
            .Value.ShouldNotBeNull();

        state.LegalNextMachines.Count.ShouldBe(1);
        state.LegalNextMachines.Contains(new MachineId(20)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(20)).ShouldBeTrue();
    }

    [Fact]
    public void FromGraph_6Arg_NullMetadata_FailsLoud()
    {
        // A null metadata map is a fail-loud Result failure (no throw), like the other null guards.
        var result = ProductRoutingState.FromGraph(
            LinearGraph(), Last(20), CycleStatus.FinishedOk, FlowStatus.InProcess,
            successorMetadata: null!, currentIsProcessMachine: true);

        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void FromGraph_6Arg_TerminalZero_NeverCascades()
    {
        // A Final node has no successor; the terminal 0 sentinel is added and NEVER cascades, regardless of
        // any metadata supplied.
        var state = ProductRoutingState.FromGraph(
                LinearGraph(), Last(30), CycleStatus.FinishedOk, FlowStatus.Finished,
                Metadata((0, MachineType.Process, Enabled: false)), currentIsProcessMachine: true)
            .Value.ShouldNotBeNull();

        state.LegalNextMachines.Count.ShouldBe(1);
        state.LegalNextMachines.Contains(new MachineId(0)).ShouldBeTrue();
        state.IsLegalArrival(Requesting(30)).ShouldBeFalse();
    }

    private static void AssertNoConversionOperators(Type roleType)
    {
        var conversionOperators = roleType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name is "op_Implicit" or "op_Explicit")
            .ToList();

        conversionOperators.ShouldBeEmpty(
            $"{roleType.Name} must declare NO conversion operators; one would re-open the role-transposition hole (I6).");
    }
}
