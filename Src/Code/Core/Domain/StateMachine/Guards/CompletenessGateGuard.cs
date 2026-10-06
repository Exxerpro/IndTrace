// <copyright file="CompletenessGateGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine.Config;

/// <summary>
/// Story 4.1 — fails when the relevant per-state completeness gate is OFF. A <see langword="null"/>
/// <see cref="TransitionContext.Completeness"/> is treated as <see cref="CompletenessOptions.Disabled"/>
/// (fail-closed: every gate OFF). When the gate is OFF the guard publishes <see cref="ResultValidation.OperationCancelled"/>
/// so the gated pair rejects exactly like an absent table row, keeping default-off behavior regression-equivalent.
/// </summary>
public sealed class CompletenessGateGuard : IGuard
{
    private readonly Func<CompletenessOptions, bool> isEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompletenessGateGuard"/> class.
    /// </summary>
    /// <param name="isEnabled">Selects the gate flag this guard checks from the resolved options.</param>
    public CompletenessGateGuard(Func<CompletenessOptions, bool> isEnabled)
    {
        this.isEnabled = isEnabled;
    }

    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context)
    {
        var options = context.Completeness ?? CompletenessOptions.Disabled;
        return this.isEnabled(options) ? GuardResult.Pass() : GuardResult.Fail(ResultValidation.OperationCancelled);
    }
}
