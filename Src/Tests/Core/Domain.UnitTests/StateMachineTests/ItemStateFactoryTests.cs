// <copyright file="ItemStateFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests;

using System.Linq;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.States;

/// <summary>
/// Story 2.3a — AC1/AC6 tests for the concrete <see cref="IItemState"/> behavior classes and the
/// <see cref="ItemStateFactory"/> resolver. Each state's <see cref="IItemState.PermittedOperations"/> must
/// match exactly the triggers legal from that state in the as-built §4 transition table
/// (<see cref="FlowTransitionTable"/>) — verified row-by-row (test #8) and cross-checked against the table.
/// </summary>
public class ItemStateFactoryTests
{
    /// <summary>
    /// AC1 — the factory resolves each reachable FlowStatus to the matching state type.
    /// </summary>
    [Fact]
    public void For_ResolvesEachReachableStatus_ToItsState()
    {
        ItemStateFactory.For(FlowStatus.None).ShouldBeOfType<NoneState>();
        ItemStateFactory.For(FlowStatus.Created).ShouldBeOfType<CreatedState>();
        ItemStateFactory.For(FlowStatus.InProcess).ShouldBeOfType<InProcessState>();
        ItemStateFactory.For(FlowStatus.Finished).ShouldBeOfType<FinishedState>();
        ItemStateFactory.For(FlowStatus.Rejected).ShouldBeOfType<RejectedState>();
    }

    /// <summary>
    /// AC1 — unmapped/unreachable statuses (Restored, Invalid) fall back to NoneState (default-reject query side).
    /// </summary>
    [Fact]
    public void For_UnmappedStatus_FallsBackToNoneState()
    {
        ItemStateFactory.For(FlowStatus.Restored).ShouldBeOfType<NoneState>();
        ItemStateFactory.For(FlowStatus.Invalid).ShouldBeOfType<NoneState>();
    }

    /// <summary>
    /// #126 F8 — the concrete states are stateless/immutable (get-only <see cref="IItemState.Status"/>,
    /// <see cref="IItemState.PermittedOperations"/> built once and never mutated), so the factory returns
    /// shared singletons instead of allocating a fresh state per call.
    /// </summary>
    [Fact]
    public void For_SameStatus_ReturnsSharedInstance()
    {
        ItemStateFactory.For(FlowStatus.None).ShouldBeSameAs(ItemStateFactory.For(FlowStatus.None));
        ItemStateFactory.For(FlowStatus.Created).ShouldBeSameAs(ItemStateFactory.For(FlowStatus.Created));
        ItemStateFactory.For(FlowStatus.InProcess).ShouldBeSameAs(ItemStateFactory.For(FlowStatus.InProcess));
        ItemStateFactory.For(FlowStatus.Finished).ShouldBeSameAs(ItemStateFactory.For(FlowStatus.Finished));
        ItemStateFactory.For(FlowStatus.Rejected).ShouldBeSameAs(ItemStateFactory.For(FlowStatus.Rejected));

        // Fallback statuses share the same NoneState singleton.
        ItemStateFactory.For(FlowStatus.Restored).ShouldBeSameAs(ItemStateFactory.For(FlowStatus.None));
        ItemStateFactory.For(FlowStatus.Invalid).ShouldBeSameAs(ItemStateFactory.For(FlowStatus.None));
    }

    /// <summary>
    /// AC1 — each state reports the FlowStatus it represents.
    /// </summary>
    [Fact]
    public void Status_MatchesTheRepresentedFlowStatus()
    {
        new NoneState().Status.Value.ShouldBe(FlowStatus.None.Value);
        new CreatedState().Status.Value.ShouldBe(FlowStatus.Created.Value);
        new InProcessState().Status.Value.ShouldBe(FlowStatus.InProcess.Value);
        new FinishedState().Status.Value.ShouldBe(FlowStatus.Finished.Value);
        new RejectedState().Status.Value.ShouldBe(FlowStatus.Rejected.Value);
    }

    /// <summary>
    /// Test #8 — NoneState permits exactly { CreateBarCodeAsync }.
    /// </summary>
    [Fact]
    public void NoneState_PermittedOperations_MatchesTable()
    {
        AssertPermitted(new NoneState(), GatewayTask.CreateBarCodeAsync);
    }

    /// <summary>
    /// Test #8 — CreatedState permits exactly { ReadBarCodeAsync, CreateCycleAsync, UpdateCycleOkAsync,
    /// UpdateCycleNotOkAsync } (the cycle-update pair per the 2026-07-21 virtual-PLC E2E finding — the create
    /// station finishes its own cycle — and its 2026-07-23 #189-ratified NOK mirror).
    /// </summary>
    [Fact]
    public void CreatedState_PermittedOperations_MatchesTable()
    {
        AssertPermitted(new CreatedState(), GatewayTask.ReadBarCodeAsync, GatewayTask.CreateCycleAsync, GatewayTask.UpdateCycleOkAsync, GatewayTask.UpdateCycleNotOkAsync);
    }

    /// <summary>
    /// Test #8 — InProcessState permits exactly the five working-state triggers.
    /// </summary>
    [Fact]
    public void InProcessState_PermittedOperations_MatchesTable()
    {
        AssertPermitted(
            new InProcessState(),
            GatewayTask.ReadBarCodeAsync,
            GatewayTask.UpdateCycleOkAsync,
            GatewayTask.UpdateCycleNotOkAsync,
            GatewayTask.EndOfProcessAsync,
            GatewayTask.RejectPartAsync);
    }

    /// <summary>
    /// Test #8 — FinishedState permits exactly { RejectPartAsync }.
    /// </summary>
    [Fact]
    public void FinishedState_PermittedOperations_MatchesTable()
    {
        AssertPermitted(new FinishedState(), GatewayTask.RejectPartAsync);
    }

    /// <summary>
    /// Test #8 — RejectedState permits exactly { RestorePartAsync }.
    /// </summary>
    [Fact]
    public void RejectedState_PermittedOperations_MatchesTable()
    {
        AssertPermitted(new RejectedState(), GatewayTask.RestorePartAsync);
    }

    /// <summary>
    /// Story 4.1 — the off-bus, config-gated completeness triggers. They are conditionally legal (gated, default
    /// OFF) rather than unconditionally permitted, so they are intentionally NOT part of any state's
    /// <see cref="IItemState.PermittedOperations"/> (which lists the always-legal PLC-bus operations).
    /// </summary>
    private static readonly int[] GatedCompletenessTriggers =
    [
        GatewayTask.MarkInvalid.Value,
        GatewayTask.MarkScrap.Value,
        GatewayTask.Cancel.Value,
    ];

    /// <summary>
    /// AC1 — cross-check: each state's PermittedOperations equals exactly the §4 table triggers whose
    /// From == that state (excluding the Story-4.1 off-bus gated completeness triggers, which are
    /// conditionally legal). This guarantees the hand-listed sets cannot drift from the authoritative table.
    /// </summary>
    [Fact]
    public void EveryState_PermittedOperations_EqualsTableTriggersForThatFromState()
    {
        var table = new FlowTransitionTable();
        IItemState[] states =
        [
            new NoneState(),
            new CreatedState(),
            new InProcessState(),
            new FinishedState(),
            new RejectedState(),
        ];

        foreach (var state in states)
        {
            var tableTriggers = table.Transitions
                .Where(t => t.From.Value == state.Status.Value)
                .Select(t => t.Trigger.Value)
                .Where(v => !GatedCompletenessTriggers.Contains(v))
                .OrderBy(v => v)
                .ToArray();

            var stateTriggers = state.PermittedOperations
                .Select(t => t.Value)
                .OrderBy(v => v)
                .ToArray();

            stateTriggers.ShouldBe(tableTriggers, $"{state.GetType().Name} must match the §4 table rows.");
        }
    }

    private static void AssertPermitted(IItemState state, params GatewayTask[] expected)
    {
        var actual = state.PermittedOperations.Select(t => t.Value).OrderBy(v => v).ToArray();
        var want = expected.Select(t => t.Value).OrderBy(v => v).ToArray();
        actual.ShouldBe(want);
    }
}
