// <copyright file="RejectedState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.States;

using IndTrace.Domain.Enum;

/// <summary>
/// Behavior for an item with <see cref="FlowStatus.Rejected"/>. Per the §4 table the only legal trigger is
/// <see cref="GatewayTask.RestorePartAsync"/> (Rejected -> InProcess by default; resolves the gated
/// <see cref="FlowStatus.Restored"/> target when <c>CompletenessOptions.EnableRestoredState</c> is ON —
/// Story 4.1/4.2, analysis §6 anomaly #5 resolved).
/// </summary>
public sealed class RejectedState : IItemState
{
    /// <inheritdoc/>
    public FlowStatus Status => FlowStatus.Rejected;

    /// <inheritdoc/>
    public IReadOnlySet<GatewayTask> PermittedOperations { get; } = new HashSet<GatewayTask>
    {
        GatewayTask.RestorePartAsync,
    };
}
