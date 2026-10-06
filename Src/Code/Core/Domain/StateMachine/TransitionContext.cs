// <copyright file="TransitionContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine;

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine.Config;

/// <summary>
/// Carries the inputs that guards and computed transitions need to resolve an outcome.
/// The lookup-presence flags (Story 2.2) are what the handlers currently compute inline; population from
/// real repositories is Epic 3. The flags default to <see langword="false"/> (FAIL-CLOSED, Story 2.2-fix):
/// an unpopulated context REJECTS rather than silently passing, so a missing/invalid shift / product / rule /
/// recipe / barcode / machine cannot slip through. A legal transition must EXPLICITLY set the flags its
/// attached guard(s) require to <see langword="true"/> — for unit tests, set the flags directly.
/// </summary>
/// <param name="MachineType">The machine type processing the item; <see cref="Enum.MachineType.Final"/> closes the lifecycle.</param>
/// <param name="CycleStatus">The current cycle status driving the computed UpdateCycleOk transition.</param>
/// <param name="PartStatus">The current part status (used by Story 2.2 guards).</param>
/// <param name="CycleTime">The measured cycle time (used by the <see cref="Guards.CycleTimeGuard"/>).</param>
/// <param name="Recipe">The recipe constraining the cycle (consumed by the <see cref="Guards.CycleTimeGuard"/>).</param>
/// <param name="BarCodeFound">Whether the barcode lookup succeeded (<see cref="Guards.BarCodeGuard"/>).</param>
/// <param name="MachineFound">Whether the machine lookup succeeded (<see cref="Guards.MachineGuard"/>).</param>
/// <param name="ProductFound">Whether the product lookup succeeded (<see cref="Guards.ProductGuard"/>).</param>
/// <param name="RuleFound">Whether the rule lookup succeeded (<see cref="Guards.RuleGuard"/>).</param>
/// <param name="RecipeFound">Whether the recipe lookup succeeded (<see cref="Guards.RecipeGuard"/>).</param>
/// <param name="ShiftValid">Whether the shift lookup is valid (<see cref="Guards.ShiftGuard"/>).</param>
/// <param name="Completeness">
/// Story 4.1 — the config-gated completeness toggles. <see langword="null"/> is treated as
/// <see cref="CompletenessOptions.Disabled"/> (fail-closed: every completeness gate OFF), so existing
/// positional callers keep compiling and default behavior is unchanged.
/// </param>
public record TransitionContext(
    MachineType MachineType,
    CycleStatus CycleStatus,
    PartStatus PartStatus,
    int CycleTime,
    Recipe? Recipe,
    bool BarCodeFound = false,
    bool MachineFound = false,
    bool ProductFound = false,
    bool RuleFound = false,
    bool RecipeFound = false,
    bool ShiftValid = false,
    CompletenessOptions? Completeness = null);
