// <copyright file="PartScrappableGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;

/// <summary>
/// Story 4.1 — entry guard for the scrap transition: the part must not already be
/// <see cref="PartStatus.Scrap"/> or <see cref="PartStatus.Rejected"/>. An already-scrapped/rejected part
/// fails with <see cref="ResultValidation.OperationCancelled"/>, so it is rejected even when the
/// <c>EnableScrapState</c> gate is ON (AC8).
/// </summary>
public sealed class PartScrappableGuard : IGuard
{
    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context) =>
        context.PartStatus.Value != PartStatus.Scrap.Value && context.PartStatus.Value != PartStatus.Rejected.Value
            ? GuardResult.Pass()
            : GuardResult.Fail(ResultValidation.OperationCancelled);
}
