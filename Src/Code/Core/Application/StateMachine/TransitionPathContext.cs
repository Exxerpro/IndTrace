// <copyright file="TransitionPathContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

using System.Threading;
using IndTrace.Domain.Enum;

/// <summary>
/// <see cref="AsyncLocal{T}"/>-backed implementation of <see cref="ITransitionPathContext"/>. The value flows
/// with the async call chain, so a SINGLETON decorator can read the path that a SINGLETON dispatcher set on
/// the same logical request — no per-request DI scope is required. Registered as a singleton; the backing
/// store is per-async-flow, so concurrent requests do not see each other's path.
/// </summary>
public sealed class TransitionPathContext : ITransitionPathContext
{
    private static readonly AsyncLocal<TransitionPath> AmbientPath = new();

    /// <inheritdoc/>
    public TransitionPath Current => AmbientPath.Value;

    /// <inheritdoc/>
    public void Set(TransitionPath path) => AmbientPath.Value = path;
}
