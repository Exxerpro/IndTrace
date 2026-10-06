// <copyright file="RecipeGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Fails with <see cref="ResultValidation.RecipeNotFound"/> (-512) when the recipe lookup did not resolve
/// (<see cref="TransitionContext.RecipeFound"/> is false, or no <see cref="TransitionContext.Recipe"/> is present).
/// </summary>
public sealed class RecipeGuard : IGuard
{
    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context) =>
        context.RecipeFound && context.Recipe is not null
            ? GuardResult.Pass()
            : GuardResult.Fail(ResultValidation.RecipeNotFound);
}
