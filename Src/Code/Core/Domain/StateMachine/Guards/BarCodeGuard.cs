// <copyright file="BarCodeGuard.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine.Guards;

using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Fails with <see cref="ResultValidation.BarCodeNotFound"/> (-2) when the barcode lookup (by label)
/// did not resolve (<see cref="TransitionContext.BarCodeFound"/> is false).
/// </summary>
public sealed class BarCodeGuard : IGuard
{
    /// <inheritdoc/>
    public GuardResult Evaluate(TransitionContext context) =>
        context.BarCodeFound ? GuardResult.Pass() : GuardResult.Fail(ResultValidation.BarCodeNotFound);
}
