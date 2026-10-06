// <copyright file="IItemStateMachine.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine;

using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;

/// <summary>
/// Single engine that resolves legal item lifecycle transitions and rejects illegal
/// <c>(sourceState, trigger)</c> pairs, without mutating the item. No production traffic is routed
/// through this engine in Story 2.1 (that lands in Epic 3); applying an outcome lands in Story 2.3.
/// </summary>
public interface IItemStateMachine
{
    /// <summary>
    /// Resolves the transition for the item's current <see cref="BarCode.FlowStatus"/> and the given trigger.
    /// </summary>
    /// <param name="item">The item to evaluate. This method NEVER mutates the item on any path.</param>
    /// <param name="trigger">The <see cref="GatewayTask"/> being fired.</param>
    /// <param name="context">The transition context carrying guard/computation inputs.</param>
    /// <returns>
    /// <see cref="Result{T}.Success(T)"/> with the resolved <see cref="TransitionOutcome"/> for a legal pair;
    /// <see cref="Result{T}"/> failure carrying <see cref="ResultValidation.OperationCancelled"/> for an illegal pair.
    /// </returns>
    Result<TransitionOutcome> Fire(BarCode item, GatewayTask trigger, TransitionContext context);
}
