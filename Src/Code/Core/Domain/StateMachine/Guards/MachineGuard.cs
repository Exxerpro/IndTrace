// <copyright file="MachineGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Fails with <see cref="ResultValidation.MachineNotFound"/> (-8) when the machine lookup did not resolve
/// (<see cref="TransitionContext.MachineFound"/> is false). Distinct from <see cref="MachineFinalGuard"/>,
/// which checks the final-machine close-out context and publishes <see cref="ResultValidation.InvalidMachine"/>.
/// </summary>
public sealed class MachineGuard : IGuard
{
    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context) =>
        context.MachineFound ? GuardResult.Pass() : GuardResult.Fail(ResultValidation.MachineNotFound);
}
