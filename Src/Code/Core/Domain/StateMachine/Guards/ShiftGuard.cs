// <copyright file="ShiftGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Fails with <see cref="ResultValidation.ShiftInvalid"/> (-32768) when the shift lookup is invalid
/// (<see cref="TransitionContext.ShiftValid"/> is false).
/// </summary>
public sealed class ShiftGuard : IGuard
{
    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context) =>
        context.ShiftValid ? GuardResult.Pass() : GuardResult.Fail(ResultValidation.ShiftInvalid);
}
