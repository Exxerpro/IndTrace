// <copyright file="LastProcessedMachine.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing;

/// <summary>
/// E6-1 role value-type (#56): the machine that <em>most recently processed</em> the part — the routing
/// reference point. It is <b>derived from history, never invented</b> (I2): it is the machine id of the
/// latest processing cycle (<c>Cycle.MachineId</c>), and only a <c>FinishedOk</c> cycle advances it
/// (<see cref="RoutingAdvancePolicy.DetermineNextMachine"/>).
/// </summary>
/// <remarks>
/// Distinct <c>readonly record struct</c> from <see cref="RequestingMachine"/> and
/// <see cref="AdvisoryNextMachine"/> on purpose: the three roles share the <see cref="MachineId"/> value
/// space but are NOT interchangeable, and there is <b>no implicit conversion</b> between them (see the
/// <see cref="RequestingMachine"/> remarks for the full rationale). Read the wrapped id explicitly through
/// <see cref="Value"/>.
/// </remarks>
/// <param name="Value">The strongly-typed identity of the last machine that processed the part.</param>
public readonly record struct LastProcessedMachine(MachineId Value);
