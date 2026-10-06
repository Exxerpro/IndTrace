// <copyright file="NullFlowTransitionLogSink.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

using IndTrace.Domain.Entities;

/// <summary>
/// No-op <see cref="IFlowTransitionLogSink"/> fallback. Registered via <c>TryAddSingleton</c> so a host that
/// does NOT wire the infrastructure (EF-backed) sink — e.g. a unit test, or a deployment that intentionally
/// has no persistence for the additive log — still resolves the decorator without error. It silently succeeds
/// and writes nothing, which is exactly the AC8 "log OFF / additive-ignorable" posture.
/// </summary>
public sealed class NullFlowTransitionLogSink : IFlowTransitionLogSink
{
    /// <inheritdoc/>
    public Result Append(FlowTransitionLog log) => Result.Success();
}
