// <copyright file="ITransitionPathContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

using IndTrace.Domain.Enum;

/// <summary>
/// Ambient supplier of the ORIGINATING <see cref="TransitionPath"/> for an in-flight dispatch (Story 3.5, AC4).
///
/// This is the seam that lets the <see cref="LoggingItemStateMachineDecorator"/> stamp the correct path on a
/// <see cref="IndTrace.Domain.Entities.FlowTransitionLog"/> WITHOUT changing the frozen Epic-2
/// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine.Fire"/> signature or any handler constructor.
/// The PLC dispatcher (<c>GatewayCommandDispatcher</c>) sets <see cref="TransitionPath.Plc"/> and the webapp
/// dispatcher (<c>MonitorRequestDispatcher</c>) sets <see cref="TransitionPath.Webapp"/> before invoking the
/// handler; the path flows through the async call chain to the decorator.
/// </summary>
public interface ITransitionPathContext
{
    /// <summary>
    /// Gets the path of the current dispatch, or <see cref="TransitionPath.Unknown"/> when none was set.
    /// </summary>
    TransitionPath Current { get; }

    /// <summary>
    /// Sets the path for the current ambient (async-flow) scope. Idempotent and overwrite-safe; the dispatcher
    /// calls this once before dispatching to its handler.
    /// </summary>
    /// <param name="path">The originating path to record for the current dispatch.</param>
    void Set(TransitionPath path);
}
