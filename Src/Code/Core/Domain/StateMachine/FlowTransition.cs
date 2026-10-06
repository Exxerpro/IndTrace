// <copyright file="FlowTransition.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine;

using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine.Guards;

/// <summary>
/// Represents a single as-built lifecycle transition for <c>BarCode.FlowStatus</c>
/// (analysis §4): a legal <c>(From, Trigger)</c> pair resolving to a <c>To</c> state under a guard.
/// </summary>
/// <param name="From">The source <see cref="FlowStatus"/> the item must currently be in.</param>
/// <param name="Trigger">The <see cref="GatewayTask"/> that drives the transition.</param>
/// <param name="To">The resolved next <see cref="FlowStatus"/> for the simple (non-computed) case.</param>
/// <param name="Guard">The precondition that must hold for the transition to fire (Story 2.2 fills the bodies).</param>
public record FlowTransition(FlowStatus From, GatewayTask Trigger, FlowStatus To, IGuard Guard);
