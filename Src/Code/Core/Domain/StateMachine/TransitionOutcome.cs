// <copyright file="TransitionOutcome.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine;

using IndTrace.Domain.Enum;

/// <summary>
/// Immutable description of the state an item should move to after a fired transition, plus the
/// <see cref="ResultValidation"/> diagnostic to publish. Story 2.1 only resolves <see cref="NextFlowStatus"/>;
/// <see cref="NextCycleStatus"/>/<see cref="NextPartStatus"/> mirror the context's current values (see Completion Notes)
/// until Stories 2.2/2.3 compute real values.
/// </summary>
/// <param name="NextFlowStatus">The resolved next <see cref="FlowStatus"/>.</param>
/// <param name="NextCycleStatus">The resolved next <see cref="CycleStatus"/>.</param>
/// <param name="NextPartStatus">The resolved next <see cref="PartStatus"/>.</param>
/// <param name="Result">The <see cref="ResultValidation"/> diagnostic to publish (negative = failure).</param>
public record TransitionOutcome(
    FlowStatus NextFlowStatus,
    CycleStatus NextCycleStatus,
    PartStatus NextPartStatus,
    ResultValidation Result);
