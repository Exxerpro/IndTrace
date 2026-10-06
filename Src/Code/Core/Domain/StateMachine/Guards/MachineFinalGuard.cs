// <copyright file="MachineFinalGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Validates the final-machine close-out context using the as-built finished-flow predicate
/// (AC4 — reuses <see cref="FlowStatusCalculator"/>.IsFlowFinished semantics:
/// <see cref="MachineType.Final"/> &amp;&amp; <see cref="CycleStatus.FinishedOk"/>). On an invalid final-machine
/// context it publishes <see cref="ResultValidation.InvalidMachine"/> (-4096) — distinct from the plain
/// machine-lookup guard's <see cref="ResultValidation.MachineNotFound"/> (-8).
/// </summary>
public sealed class MachineFinalGuard : IGuard
{
    private readonly IFlowStatusCalculator flowStatusCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineFinalGuard"/> class.
    /// </summary>
    /// <param name="flowStatusCalculator">The reused finished-flow calculator.</param>
    public MachineFinalGuard(IFlowStatusCalculator flowStatusCalculator)
    {
        this.flowStatusCalculator = flowStatusCalculator;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineFinalGuard"/> class with the default calculator.
    /// </summary>
    public MachineFinalGuard()
        : this(new FlowStatusCalculator())
    {
    }

    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context)
    {
        // IsFlowFinished is private on FlowStatusCalculator; reuse it via Calculate (Final && FinishedOk => Finished).
        var flowStatus = this.flowStatusCalculator.Calculate(context.MachineType, context.CycleStatus, context.PartStatus);

        return flowStatus.Value == FlowStatus.Finished.Value
            ? GuardResult.Pass()
            : GuardResult.Fail(ResultValidation.InvalidMachine);
    }
}
