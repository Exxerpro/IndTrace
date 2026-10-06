// <copyright file="IGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.StateMachine;

/// <summary>
/// A composable, first-class precondition evaluated before a <see cref="FlowTransition"/> may fire
/// (analysis §6 anomaly #3). Each guard lifts a lookup/range check out of the handlers and, on failure,
/// yields a PRECISE <see cref="Enum.ResultValidation"/> negative code instead of a generic reject.
/// </summary>
public interface IGuard
{
    /// <summary>
    /// Evaluates the guard against the supplied context.
    /// </summary>
    /// <param name="context">The transition context carrying the guard inputs.</param>
    /// <returns>
    /// A <see cref="GuardResult"/> that is success when the transition is permitted, or failure carrying
    /// this guard's specific <see cref="Enum.ResultValidation"/> negative code.
    /// </returns>
    GuardResult Evaluate(TransitionContext context);
}
