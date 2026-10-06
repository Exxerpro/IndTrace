// <copyright file="IFlowTransitionLogSink.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

using IndTrace.Domain.Entities;

/// <summary>
/// Best-effort append seam for the additive <see cref="FlowTransitionLog"/> (Story 3.5, AC2/AC3/AC5). The
/// <see cref="LoggingItemStateMachineDecorator"/> calls <see cref="Append"/> after every fire; the
/// infrastructure implementation persists the row.
///
/// The seam is SYNCHRONOUS because the frozen Epic-2 <c>IItemStateMachine.Fire(...)</c> is synchronous
/// (returns <c>Result&lt;TransitionOutcome&gt;</c>, not a <c>Task</c>); appending on the same synchronous call
/// keeps the row tied 1:1 to the fire without changing the contract.
///
/// AC5 (result-integrity): the append is a SIDE EFFECT that must NEVER flip the transition's
/// <see cref="Result{T}"/> outcome. The implementation returns a <see cref="Result"/> describing only its OWN
/// success/failure; the decorator IGNORES that result (logging any failure and discarding it). The decorator
/// additionally wraps the call in try/catch, so a throwing or failing sink cannot corrupt the primary result.
/// </summary>
public interface IFlowTransitionLogSink
{
    /// <summary>
    /// Appends one <see cref="FlowTransitionLog"/> row. Implementations SHOULD swallow their own errors into a
    /// failure <see cref="Result"/> rather than throwing; the decorator also guards against throws (AC5).
    /// </summary>
    /// <param name="log">The row to append (already fully stamped by the decorator).</param>
    /// <returns>A <see cref="Result"/> describing only the append's own outcome (ignored by the decorator).</returns>
    Result Append(FlowTransitionLog log);
}
