// <copyright file="ProductGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Fails with <see cref="ResultValidation.ProductNotFound"/> (-16384) when the product lookup did not
/// resolve (<see cref="TransitionContext.ProductFound"/> is false).
/// </summary>
public sealed class ProductGuard : IGuard
{
    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context) =>
        context.ProductFound ? GuardResult.Pass() : GuardResult.Fail(ResultValidation.ProductNotFound);
}
