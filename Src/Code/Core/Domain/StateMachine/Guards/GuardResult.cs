// <copyright file="GuardResult.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;

/// <summary>
/// The outcome of evaluating an <see cref="IGuard"/> (the AC1 "boolean + code" shape).
/// On success it carries <see cref="ResultValidation.Valid"/>; on failure it carries the guard's
/// specific negative <see cref="ResultValidation"/> code (the Epic 2 correctness core — analysis §6 anomaly #3).
/// <see cref="OverrideCycleStatus"/> is set ONLY by the cycle-time out-of-range override (AC5): an
/// out-of-range cycle time does not hard-fail; it forces <see cref="CycleStatus.FinishedNok"/> while still
/// publishing <see cref="ResultValidation.PartNotValid"/>.
/// </summary>
/// <param name="IsSuccess">Whether the guard permits the transition.</param>
/// <param name="Code">The <see cref="ResultValidation"/> to publish (negative = failure; <see cref="ResultValidation.Valid"/> on pass).</param>
/// <param name="OverrideCycleStatus">When set, the cycle status the engine must force (cycle-time FinishedNok override, AC5).</param>
public sealed record GuardResult(bool IsSuccess, ResultValidation Code, CycleStatus? OverrideCycleStatus = null)
{
    /// <summary>
    /// Creates a passing <see cref="GuardResult"/> carrying <see cref="ResultValidation.Valid"/>.
    /// </summary>
    /// <returns>A successful guard result.</returns>
    public static GuardResult Pass() => new(true, ResultValidation.Valid);

    /// <summary>
    /// Creates a failing <see cref="GuardResult"/> carrying the supplied specific negative code.
    /// </summary>
    /// <param name="code">The specific negative <see cref="ResultValidation"/> code to publish.</param>
    /// <param name="overrideCycleStatus">Optional cycle-status override (AC5 cycle-time FinishedNok path).</param>
    /// <returns>A failing guard result.</returns>
    public static GuardResult Fail(ResultValidation code, CycleStatus? overrideCycleStatus = null) =>
        new(false, code, overrideCycleStatus);
}
