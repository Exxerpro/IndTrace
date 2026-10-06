// <copyright file="TransitionPath.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Identifies the ORIGINATING path of an <c>IItemStateMachine.Fire(...)</c> for the additive
/// <see cref="IndTrace.Domain.Entities.FlowTransitionLog"/> (Story 3.5, AC4).
///
/// This is an INTERNAL, DB-ONLY discriminator: it is NEVER written to a PLC reference tag and is NOT part of
/// any frozen PLC contract (CR4/NFR1). Its numeric values are deliberately small and stand-alone — they do
/// NOT participate in (and therefore cannot collide with) the frozen <see cref="GatewayTask"/>/<see cref="FlowStatus"/>
/// PLC-contract numerics, because the column stores a separate, new field.
/// </summary>
public enum TransitionPath
{
    /// <summary>
    /// Unknown / not-yet-supplied origin. Used as the fail-safe default when no dispatcher set the path.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The fire originated from an <c>IGatewayRequestHandler</c> via the <c>GatewayCommandDispatcher</c> (PLC path).
    /// </summary>
    Plc = 1,

    /// <summary>
    /// The fire originated from an <c>IMonitorRequestHandler</c> via the <c>MonitorRequestDispatcher</c>
    /// (webapp/monitor path — Reject/Restore).
    /// </summary>
    Webapp = 2,
}
