// <copyright file="RequestingMachine.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing;

/// <summary>
/// E6-1 role value-type (#56): the machine that is <em>asking</em> IndTrace right now — either
/// "may I work this piece?" (permit-to-work) or "what do you know about it?" (info). It is judged, never
/// commanded: IndTrace <b>tracks and validates; it does not control</b>.
/// </summary>
/// <remarks>
/// <para>
/// A <c>RequestingMachine</c>, a <see cref="LastProcessedMachine"/>, and an <see cref="AdvisoryNextMachine"/>
/// are the <b>same value space</b> (a <see cref="MachineId"/>) playing <b>different roles</b> whose values
/// coincide by state (while a cycle is in-process the requesting machine == the last-processed machine — I6).
/// Modelling each role as its OWN <c>readonly record struct</c> makes that role-confusion a <b>compile
/// error</b>, not a comment: <c>ProductRoutingState.IsLegalArrival(RequestingMachine)</c> will not accept a
/// <see cref="LastProcessedMachine"/> even though both wrap machine <c>100</c>, which closes the
/// swapped-argument transposition hole (the reason 56-A's guard exists).
/// </para>
/// <para>
/// There is deliberately <b>no implicit conversion</b> to or from any other role type or to the raw
/// <see cref="MachineId"/> — one <c>implicit operator</c> would re-open that hole. Read the wrapped id
/// explicitly through <see cref="Value"/>; assert the role explicitly through construction.
/// </para>
/// </remarks>
/// <param name="Value">The strongly-typed machine identity the request arrived on.</param>
public readonly record struct RequestingMachine(MachineId Value);
