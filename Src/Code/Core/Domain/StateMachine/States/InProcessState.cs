// <copyright file="InProcessState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.States;

using IndTrace.Domain.Enum;

/// <summary>
/// Behavior for an item with <see cref="FlowStatus.InProcess"/> — the working state. Per the §4 table the
/// legal triggers are <see cref="GatewayTask.ReadBarCodeAsync"/>, <see cref="GatewayTask.UpdateCycleOkAsync"/>,
/// <see cref="GatewayTask.UpdateCycleNotOkAsync"/>, <see cref="GatewayTask.EndOfProcessAsync"/>, and
/// <see cref="GatewayTask.RejectPartAsync"/>.
/// </summary>
public sealed class InProcessState : IItemState
{
    /// <inheritdoc/>
    public FlowStatus Status => FlowStatus.InProcess;

    /// <inheritdoc/>
    public IReadOnlySet<GatewayTask> PermittedOperations { get; } = new HashSet<GatewayTask>
    {
        GatewayTask.ReadBarCodeAsync,
        GatewayTask.UpdateCycleOkAsync,
        GatewayTask.UpdateCycleNotOkAsync,
        GatewayTask.EndOfProcessAsync,
        GatewayTask.RejectPartAsync,
    };
}
