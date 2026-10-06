// <copyright file="CompositeGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.StateMachine;

/// <summary>
/// Composes an ordered set of guards (AC7). Guards are evaluated in order; the FIRST failure wins and its
/// specific <see cref="Enum.ResultValidation"/> code is the one published. If every guard passes, the
/// composite passes.
/// </summary>
public sealed class CompositeGuard : IGuard
{
    private readonly IReadOnlyList<IGuard> guards;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeGuard"/> class.
    /// </summary>
    /// <param name="guards">The ordered guards to evaluate (first failure wins).</param>
    public CompositeGuard(params IGuard[] guards)
    {
        this.guards = guards;
    }

    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context)
    {
        foreach (var guard in this.guards)
        {
            var result = guard.Evaluate(context);
            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return GuardResult.Pass();
    }
}
