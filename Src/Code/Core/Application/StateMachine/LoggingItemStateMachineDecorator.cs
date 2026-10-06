// <copyright file="LoggingItemStateMachineDecorator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Story 3.5 logging DECORATOR over <see cref="IItemStateMachine"/>. It wraps the real
/// <see cref="ItemStateMachine"/>, delegates <see cref="Fire"/> UNCHANGED, then appends exactly ONE additive
/// <see cref="FlowTransitionLog"/> row for the fire — on success AND on rejection — and returns the inner
/// <see cref="Result{T}"/> verbatim.
///
/// Design rationale (Dev Notes): every routed handler (Stories 3.1/3.2/3.4) fires through the injected
/// <see cref="IItemStateMachine"/>, so decorating that single DI registration captures every fire with zero
/// per-handler duplication and leaves each handler's <see cref="Result{T}"/> pipeline untouched. The frozen
/// Epic-2 <see cref="IItemStateMachine.Fire"/> signature is NOT changed; the originating <see cref="TransitionPath"/>
/// arrives out-of-band via the ambient <see cref="ITransitionPathContext"/> the dispatcher sets.
///
/// AC5 (best-effort): the append is wrapped in try/catch and its own <see cref="Result"/> is discarded, so a
/// log-write failure is logged and swallowed — it can NEVER flip the transition's success/failure outcome.
/// AC8 (flag): when <see cref="StateMachineRoutingOptions.LogTransitions"/> is OFF the append is skipped
/// entirely, with zero behavioral impact on the transition (the inner result is still returned verbatim).
/// </summary>
public sealed class LoggingItemStateMachineDecorator : IItemStateMachine
{
    private readonly IItemStateMachine inner;
    private readonly IFlowTransitionLogSink sink;
    private readonly ITransitionPathContext pathContext;
    private readonly IDateTimeMachine dateTimeMachine;
    private readonly ILogger<LoggingItemStateMachineDecorator> logger;
    private readonly StateMachineRoutingOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingItemStateMachineDecorator"/> class.
    /// </summary>
    /// <param name="inner">The real state machine being decorated.</param>
    /// <param name="sink">The best-effort append seam for the additive log.</param>
    /// <param name="pathContext">The ambient supplier of the originating <see cref="TransitionPath"/>.</param>
    /// <param name="dateTimeMachine">The deterministic clock for the row timestamp.</param>
    /// <param name="logger">The logger used to record (and discard) a log-write failure.</param>
    /// <param name="options">The routing options carrying the <c>LogTransitions</c> flag.</param>
    public LoggingItemStateMachineDecorator(
        IItemStateMachine inner,
        IFlowTransitionLogSink sink,
        ITransitionPathContext pathContext,
        IDateTimeMachine dateTimeMachine,
        ILogger<LoggingItemStateMachineDecorator> logger,
        IOptions<StateMachineRoutingOptions>? options = null)
    {
        this.inner = inner;
        this.sink = sink;
        this.pathContext = pathContext;
        this.dateTimeMachine = dateTimeMachine;
        this.logger = logger;
        this.options = options?.Value ?? new StateMachineRoutingOptions();
    }

    /// <inheritdoc/>
    public Result<TransitionOutcome> Fire(BarCode item, GatewayTask trigger, TransitionContext context)
    {
        // 1. Delegate to the real machine FIRST and capture its verbatim result. The decorator NEVER alters it.
        var result = this.inner.Fire(item, trigger, context);

        // 2. AC8: flag OFF disables the append with zero behavioral impact — return the inner result unchanged.
        if (!this.options.LogTransitions)
        {
            return result;
        }

        // 3. AC5: best-effort. A null item or any throw must not corrupt the transition outcome.
        try
        {
            if (item is not null)
            {
                var log = BuildLog(item, trigger, context, result, this.pathContext.Current, this.dateTimeMachine.Now.ToLocalTime());
                var appendResult = this.sink.Append(log);
                if (appendResult.IsFailure)
                {
                    // Logged and discarded — the transition result is returned regardless (AC5).
                    this.logger.LogWarning(
                        "FlowTransitionLog append failed (discarded, transition result unaffected): {Error}",
                        appendResult.Error);
                }
            }
        }
        catch (Exception ex)
        {
            // AC5: swallow — a logging failure must never change the transition's success/failure result.
            this.logger.LogWarning(ex, "FlowTransitionLog append threw (discarded, transition result unaffected).");
        }

        return result;
    }

    /// <summary>
    /// Builds the additive row from the fire inputs and the (verbatim) outcome. On success <c>To</c> is the
    /// resolved next status and <c>ResultValidation</c> is the outcome code; on rejection <c>To</c> equals
    /// <c>From</c> (no advance) and <c>ResultValidation</c> carries the specific negative code (AC2/AC3).
    /// </summary>
    private static FlowTransitionLog BuildLog(
        BarCode item,
        GatewayTask trigger,
        TransitionContext context,
        Result<TransitionOutcome> result,
        TransitionPath path,
        DateTime timeStamp)
    {
        var from = item.FlowStatus;
        var outcome = result.Value;

        // Success: realized transition. Rejection: no advance (To = From) with the specific negative code.
        var to = result.IsSuccess && outcome is not null ? outcome.NextFlowStatus : from;
        var resultValidation = outcome is not null
            ? outcome.Result
            : (result.IsSuccess ? ResultValidation.Valid : ResultValidation.OperationCancelled);

        return new FlowTransitionLog
        {
            From = from,
            To = to,
            FromCycleStatus = context.CycleStatus,
            Trigger = trigger,
            Path = path,
            MachineId = item.MachineId.Value,
            BarCodeId = item.BarCodeId.Value,
            CycleId = 0,
            ResultValidation = resultValidation,
            TimeStamp = timeStamp,
        };
    }
}
