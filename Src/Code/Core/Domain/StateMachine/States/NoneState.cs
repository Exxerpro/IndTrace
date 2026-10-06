// <copyright file="NoneState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.States;

using IndTrace.Domain.Enum;

/// <summary>
/// The initial/default state for an item with <see cref="FlowStatus.None"/>. The only legal trigger
/// is <see cref="GatewayTask.CreateBarCodeAsync"/> (None -> Created), per the §4 table.
/// </summary>
public sealed class NoneState : IItemState
{
    /// <inheritdoc/>
    public FlowStatus Status => FlowStatus.None;

    /// <inheritdoc/>
    public IReadOnlySet<GatewayTask> PermittedOperations { get; } = new HashSet<GatewayTask>
    {
        GatewayTask.CreateBarCodeAsync,
    };
}
