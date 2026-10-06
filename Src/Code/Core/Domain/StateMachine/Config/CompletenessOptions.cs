// <copyright file="CompletenessOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Config;

/// <summary>
/// Story 4.1 — the per-state-family configuration gate that toggles the completeness transitions
/// (<c>Invalid</c>, <c>Scrap</c>, <c>Canceled</c>, <c>Restored</c>) which make the machine total. Every
/// flag defaults to <see langword="false"/> (OFF), so with no config the resolved outcomes are bit-for-bit
/// identical to the current customer-tuned line (PRD NFR4). A <see langword="null"/> options instance on the
/// <see cref="TransitionContext"/> is treated as <see cref="Disabled"/> (fail-closed: all gates OFF).
/// </summary>
public sealed record CompletenessOptions
{
    /// <summary>
    /// Gets a singleton with every gate OFF (current behavior). Used as the fail-closed default when no
    /// options are supplied.
    /// </summary>
    public static CompletenessOptions Disabled { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the explicit fault transition <c>* → FlowStatus.Invalid</c>
    /// (via <see cref="Enum.GatewayTask.MarkInvalid"/>) is reachable. Defaults to <see langword="false"/>.
    /// </summary>
    public bool EnableInvalidState { get; init; }

    /// <summary>
    /// Gets a value indicating whether the terminal quality transition <c>* → PartStatus.Scrap</c>
    /// (via <see cref="Enum.GatewayTask.MarkScrap"/>) is reachable. Defaults to <see langword="false"/>.
    /// </summary>
    public bool EnableScrapState { get; init; }

    /// <summary>
    /// Gets a value indicating whether the cycle cancellation transition
    /// <c>CycleStatus.Started → CycleStatus.Canceled</c> (via <see cref="Enum.GatewayTask.Cancel"/>) is
    /// reachable. Defaults to <see langword="false"/>.
    /// </summary>
    public bool EnableCanceledState { get; init; }

    /// <summary>
    /// Gets a value indicating whether the audit-bearing restore transition
    /// <c>FlowStatus.Rejected → FlowStatus.Restored</c> (via <see cref="Enum.GatewayTask.RestorePartAsync"/>)
    /// is reachable. When OFF, Restore keeps the as-built <c>Rejected → InProcess</c> behavior. Defaults to
    /// <see langword="false"/>.
    /// </summary>
    public bool EnableRestoredState { get; init; }
}
