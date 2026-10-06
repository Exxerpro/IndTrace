// <copyright file="IItemState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.States;

using IndTrace.Domain.Enum;

/// <summary>
/// A concrete behavior for a single reachable <see cref="FlowStatus"/> in the item lifecycle.
/// Each state exposes the set of <see cref="GatewayTask"/> triggers that are legal to fire from it,
/// derived from the as-built §4 transition table (<see cref="FlowTransitionTable"/>). Querying
/// <see cref="PermittedOperations"/> answers "is trigger X legal from this state?" without firing
/// the machine (Story 2.3 AC1/AC6). State objects are pure domain — no infrastructure, no mutation.
/// </summary>
public interface IItemState
{
    /// <summary>
    /// Gets the <see cref="FlowStatus"/> this state represents.
    /// </summary>
    FlowStatus Status { get; }

    /// <summary>
    /// Gets the triggers legal to fire from this state (per the §4 transition table). Any trigger
    /// not present here is rejected by the machine's default-reject behavior.
    /// </summary>
    IReadOnlySet<GatewayTask> PermittedOperations { get; }
}
