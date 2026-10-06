// <copyright file="CycleTimeGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Wraps <see cref="ICycleTimeValidator"/> (AC4 — does NOT re-implement range logic) to enforce the
/// recipe cycle-time window. Two distinct failure shapes (analysis §4 sub-machine / §6, Story 1.3 anomaly B)
/// that share ONE as-built STATE outcome (Story 2.4 NFR4 parity): both force <see cref="CycleStatus.FinishedNok"/>.
/// <list type="bullet">
/// <item>NULL recipe → <see cref="ResultValidation.RecipeNotFound"/> (-512): a missing recipe, not a bad part —
/// the precise diagnostic is an intended FR4 improvement. As-built treated null recipe as invalid and forced
/// the SAME FinishedNok override, so the STATE must match (NFR4): this path also forces
/// <see cref="CycleStatus.FinishedNok"/>.</item>
/// <item>Out of range → <see cref="ResultValidation.PartNotValid"/> (-64) with the same
/// <see cref="CycleStatus.FinishedNok"/> override (AC5): an out-of-range cycle time does NOT hard-fail the
/// transition; it forces the FinishedNok outcome.</item>
/// </list>
/// </summary>
public sealed class CycleTimeGuard : IGuard
{
    private readonly ICycleTimeValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleTimeGuard"/> class.
    /// </summary>
    /// <param name="validator">The reused cycle-time validator (range logic lives there).</param>
    public CycleTimeGuard(ICycleTimeValidator validator)
    {
        this.validator = validator;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleTimeGuard"/> class with the default validator.
    /// </summary>
    public CycleTimeGuard()
        : this(new CycleTimeValidator())
    {
    }

    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context)
    {
        // Distinguish "no recipe" from "time out of range" BEFORE delegating verdicts (spec Dev Notes).
        // STATE parity (Story 2.4 NFR4): as-built treated a null recipe as invalid and forced the SAME
        // FinishedNok cycle override as out-of-range; reproduce that override here while keeping the more
        // precise RecipeNotFound (-512) diagnostic (intended FR4 improvement — only the STATE must match).
        if (context.Recipe is null)
        {
            return GuardResult.Fail(ResultValidation.RecipeNotFound, CycleStatus.FinishedNok);
        }

        var validation = this.validator.Validate(context.CycleTime, context.Recipe);
        if (validation.IsValid)
        {
            return GuardResult.Pass();
        }

        // Out-of-range override (AC5): force FinishedNok, publish PartNotValid — not a hard lookup failure.
        return GuardResult.Fail(ResultValidation.PartNotValid, CycleStatus.FinishedNok);
    }
}
