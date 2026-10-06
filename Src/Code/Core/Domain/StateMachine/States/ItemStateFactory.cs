// <copyright file="ItemStateFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.States;

using IndTrace.Domain.Enum;

/// <summary>
/// Resolves the concrete <see cref="IItemState"/> behavior for a given <see cref="FlowStatus"/>. Smart-enums
/// compare by <c>.Value</c>, so resolution is keyed on the numeric value. Any unreachable/invalid status
/// (e.g. <see cref="FlowStatus.Restored"/>, <see cref="FlowStatus.Invalid"/>) falls back to the
/// <see cref="NoneState"/> (empty <see cref="IItemState.PermittedOperations"/>), keeping the query side
/// default-reject just like the machine's transition resolution.
/// </summary>
public static class ItemStateFactory
{
    // #126 F8: every concrete state is sealed and immutable (get-only Status, PermittedOperations built once
    // in the initializer and never mutated afterwards — no mutable instance fields), so a single shared
    // instance per state is safe. For() previously allocated a fresh state on every call.
    private static readonly CreatedState CachedCreated = new();
    private static readonly InProcessState CachedInProcess = new();
    private static readonly FinishedState CachedFinished = new();
    private static readonly RejectedState CachedRejected = new();
    private static readonly NoneState CachedNone = new();

    /// <summary>
    /// Returns the state behavior for the supplied <see cref="FlowStatus"/>.
    /// </summary>
    /// <param name="flowStatus">The current flow status of the item.</param>
    /// <returns>The matching <see cref="IItemState"/>; <see cref="NoneState"/> for any unmapped status.</returns>
    public static IItemState For(FlowStatus flowStatus)
    {
        return flowStatus.Value switch
        {
            _ when flowStatus.Value == FlowStatus.Created.Value => CachedCreated,
            _ when flowStatus.Value == FlowStatus.InProcess.Value => CachedInProcess,
            _ when flowStatus.Value == FlowStatus.Finished.Value => CachedFinished,
            _ when flowStatus.Value == FlowStatus.Rejected.Value => CachedRejected,
            _ => CachedNone,
        };
    }
}
