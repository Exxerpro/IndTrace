// <copyright file="CreatedState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.States;

using IndTrace.Domain.Enum;

/// <summary>
/// Behavior for an item with <see cref="FlowStatus.Created"/>. Per the §4 table the legal triggers are
/// <see cref="GatewayTask.ReadBarCodeAsync"/> (no-change re-read), <see cref="GatewayTask.CreateCycleAsync"/>
/// (Created -> InProcess), <see cref="GatewayTask.UpdateCycleOkAsync"/> (Created -> InProcess — the create
/// station finishing its own cycle; 2026-07-21 virtual-PLC E2E finding, see the FlowTransitionTable row's
/// evidence comment), and <see cref="GatewayTask.UpdateCycleNotOkAsync"/> (Created -> InProcess — the NOK
/// mirror; PO-ratified 2026-07-23, issue #189, see that row's evidence comment).
/// </summary>
public sealed class CreatedState : IItemState
{
    /// <inheritdoc/>
    public FlowStatus Status => FlowStatus.Created;

    /// <inheritdoc/>
    public IReadOnlySet<GatewayTask> PermittedOperations { get; } = new HashSet<GatewayTask>
    {
        GatewayTask.ReadBarCodeAsync,
        GatewayTask.CreateCycleAsync,
        GatewayTask.UpdateCycleOkAsync,
        GatewayTask.UpdateCycleNotOkAsync,
    };
}
