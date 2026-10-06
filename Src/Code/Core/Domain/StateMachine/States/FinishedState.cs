// <copyright file="FinishedState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.States;

using IndTrace.Domain.Enum;

/// <summary>
/// Behavior for an item with <see cref="FlowStatus.Finished"/> — the lifecycle has closed. Per the §4 table
/// the only legal trigger is <see cref="GatewayTask.RejectPartAsync"/> (Finished -> Rejected).
/// </summary>
public sealed class FinishedState : IItemState
{
    /// <inheritdoc/>
    public FlowStatus Status => FlowStatus.Finished;

    /// <inheritdoc/>
    public IReadOnlySet<GatewayTask> PermittedOperations { get; } = new HashSet<GatewayTask>
    {
        GatewayTask.RejectPartAsync,
    };
}
