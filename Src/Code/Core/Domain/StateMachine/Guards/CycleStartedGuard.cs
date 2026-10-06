// <copyright file="CycleStartedGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;

/// <summary>
/// Story 4.1 — entry guard for the cancel transition: the cycle must currently be
/// <see cref="CycleStatus.Started"/>. Any other cycle status fails with
/// <see cref="ResultValidation.OperationCancelled"/>, so a non-Started cycle is rejected even when the
/// <c>EnableCanceledState</c> gate is ON (AC8).
/// </summary>
public sealed class CycleStartedGuard : IGuard
{
    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context) =>
        context.CycleStatus.Value == CycleStatus.Started.Value
            ? GuardResult.Pass()
            : GuardResult.Fail(ResultValidation.OperationCancelled);
}
