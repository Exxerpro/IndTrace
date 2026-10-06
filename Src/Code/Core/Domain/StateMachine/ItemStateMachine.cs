// <copyright file="ItemStateMachine.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine;

using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.StateMachine.Config;
using IndTrace.Domain.StateMachine.Guards;

/// <summary>
/// Default <see cref="IItemStateMachine"/> implementation. Resolves transitions via
/// <see cref="FlowTransitionTable"/>; rejects any pair absent from the table (default-reject).
/// <see cref="Fire"/> is a pure read of <paramref name="item"/> — it never mutates it on any path.
/// </summary>
public sealed class ItemStateMachine : IItemStateMachine
{
    private readonly FlowTransitionTable transitionTable;
    private readonly IFlowStatusCalculator flowStatusCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemStateMachine"/> class with the as-built table.
    /// </summary>
    public ItemStateMachine()
        : this(new FlowTransitionTable(), new FlowStatusCalculator())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemStateMachine"/> class.
    /// </summary>
    /// <param name="transitionTable">The transition table to resolve against.</param>
    /// <param name="flowStatusCalculator">Reuses the as-built finished-flow rule (Final &amp;&amp; FinishedOk).</param>
    public ItemStateMachine(FlowTransitionTable transitionTable, IFlowStatusCalculator flowStatusCalculator)
    {
        this.transitionTable = transitionTable;
        this.flowStatusCalculator = flowStatusCalculator;
    }

    /// <inheritdoc/>
    public Result<TransitionOutcome> Fire(BarCode item, GatewayTask trigger, TransitionContext context)
    {
        // Default-reject: any (From, Trigger) absent from the §4 table fails without mutating the item.
        if (!this.transitionTable.TryResolve(item.FlowStatus, trigger, out var transition) || transition is null)
        {
            return Result<TransitionOutcome>.Failure(
                $"Illegal transition: ({item.FlowStatus.Name}, {trigger.Name}) is not in the as-built table.",
                BuildRejectOutcome(item));
        }

        // Story 2.2: evaluate the transition's first-class guard(s). On failure, carry the guard's SPECIFIC
        // ResultValidation code (not the default OperationCancelled) and do NOT mutate the item. The
        // cycle-time out-of-range override (AC5) is a failure that nonetheless forces FinishedNok.
        var guardResult = transition.Guard.Evaluate(context);
        if (!guardResult.IsSuccess)
        {
            return Result<TransitionOutcome>.Failure(
                $"Guard rejected transition: ({item.FlowStatus.Name}, {trigger.Name}) -> {guardResult.Code.Name}.",
                BuildGuardRejectOutcome(item, guardResult));
        }

        var nextFlowStatus = this.ResolveNextFlowStatus(transition, trigger, context);

        // Story 2.1 skeleton: Cycle/Part outcomes mirror the context's current values; real values land in 2.2/2.3.
        // Story 4.1: the gated completeness triggers compute a Part/Cycle outcome (Scrap/Canceled) and a
        // negative diagnostic (MarkInvalid carries the fault reason on the SUCCESS outcome, AC3).
        var outcome = new TransitionOutcome(
            nextFlowStatus,
            ResolveNextCycleStatus(trigger, context),
            ResolveNextPartStatus(trigger, context),
            ResolveResult(trigger));

        return Result<TransitionOutcome>.Success(outcome);
    }

    private FlowStatus ResolveNextFlowStatus(FlowTransition transition, GatewayTask trigger, TransitionContext context)
    {
        // UpdateCycleOkAsync is the only computed row: Finished if Final && FinishedOk, else InProcess.
        // Reuse FlowStatusCalculator (mirrors IsFlowFinished) rather than duplicating the rule.
        if (trigger.Value == GatewayTask.UpdateCycleOkAsync.Value)
        {
            return this.flowStatusCalculator.Calculate(context.MachineType, context.CycleStatus, context.PartStatus);
        }

        // Story 4.1: gated computed Restore target. When EnableRestoredState is ON, Rejected + RestorePartAsync
        // resolves to FlowStatus.Restored; otherwise it stays the as-built Rejected -> InProcess (transition.To).
        // This mirrors the UpdateCycleOk computed pattern and adds NO second table row (AC6).
        if (trigger.Value == GatewayTask.RestorePartAsync.Value
            && transition.From.Value == FlowStatus.Rejected.Value
            && (context.Completeness ?? CompletenessOptions.Disabled).EnableRestoredState)
        {
            return FlowStatus.Restored;
        }

        return transition.To;
    }

    // Story 4.1: the Cancel trigger sets the cycle terminal Canceled; every other trigger mirrors the context.
    private static CycleStatus ResolveNextCycleStatus(GatewayTask trigger, TransitionContext context) =>
        trigger.Value == GatewayTask.Cancel.Value ? CycleStatus.Canceled : context.CycleStatus;

    // Story 4.1: the MarkScrap trigger sets the terminal Scrap part status; every other trigger mirrors the context.
    private static PartStatus ResolveNextPartStatus(GatewayTask trigger, TransitionContext context) =>
        trigger.Value == GatewayTask.MarkScrap.Value ? PartStatus.Scrap : context.PartStatus;

    // Story 4.1: a successful MarkInvalid still carries a negative diagnostic (the fault reason) on the
    // outcome even though the Result<TransitionOutcome> is a success (AC3). Every other trigger is Valid.
    private static ResultValidation ResolveResult(GatewayTask trigger) =>
        trigger.Value == GatewayTask.MarkInvalid.Value ? ResultValidation.Invalid : ResultValidation.Valid;

    private static TransitionOutcome BuildRejectOutcome(BarCode item) =>
        new(item.FlowStatus, CycleStatus.None, item.PartStatus, ResultValidation.OperationCancelled);

    private static TransitionOutcome BuildGuardRejectOutcome(BarCode item, GuardResult guardResult) =>
        new(
            item.FlowStatus,
            guardResult.OverrideCycleStatus ?? CycleStatus.None,
            item.PartStatus,
            guardResult.Code);
}
