// <copyright file="RuleGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Fails with <see cref="ResultValidation.RuleNotFound"/> (-8192) when the rule lookup did not resolve
/// (<see cref="TransitionContext.RuleFound"/> is false).
/// </summary>
public sealed class RuleGuard : IGuard
{
    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context) =>
        context.RuleFound ? GuardResult.Pass() : GuardResult.Fail(ResultValidation.RuleNotFound);
}
