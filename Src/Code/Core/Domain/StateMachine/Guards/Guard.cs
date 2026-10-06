// <copyright file="Guard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.StateMachine;

/// <summary>
/// Provides shared <see cref="IGuard"/> instances used by the transition table.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Gets a guard that always permits the transition. It is the read-only / no-op guard used by the
    /// ReadBarCode row (and any row that carries no first-class precondition).
    /// </summary>
    public static IGuard AlwaysPass { get; } = new AlwaysPassGuard();

    /// <summary>
    /// A guard that unconditionally permits the transition.
    /// </summary>
    private sealed class AlwaysPassGuard : IGuard
    {
        /// <inheritdoc/>
        public GuardResult Evaluate(TransitionContext context) => GuardResult.Pass();
    }
}
