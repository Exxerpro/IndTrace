// <copyright file="AdvisoryNextMachine.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing;

/// <summary>
/// E6-1 role value-type (#56): the singular §7 PLC routing hint — the current
/// <c>NextMachineId</c> value the read path computes via
/// <see cref="RoutingAdvancePolicy.DetermineNextMachine"/>. It is <b>advice, not instruction</b>: IndTrace
/// tracks and validates, it does not control, so this value never gates an arrival — the
/// <see cref="LegalNextMachines"/> set is the validation yardstick.
/// </summary>
/// <remarks>
/// <para>
/// On linear routing this is the single successor (or <c>0</c> at end-of-line). On a multi-successor
/// diverter the singular hint is deliberately <b>unresolvable</b> (56-A fails loud rather than pick an
/// arbitrary branch); choosing what the singular §7 tag carries on a diverter is out of scope for E6-1 and
/// deferred to E6-2 (<c>plc-gated</c>). Where the hint cannot be resolved the
/// <see cref="ProductRoutingState.AdvisoryNextMachine"/> is simply absent (<see langword="null"/>), while
/// <see cref="LegalNextMachines"/> still carries the full legal set for validation.
/// </para>
/// <para>
/// Distinct <c>readonly record struct</c> from <see cref="RequestingMachine"/> and
/// <see cref="LastProcessedMachine"/>, with <b>no implicit conversion</b> between them. Read the wrapped id
/// explicitly through <see cref="Value"/>.
/// </para>
/// </remarks>
/// <param name="Value">The strongly-typed identity of the advisory next machine (§7 hint).</param>
public readonly record struct AdvisoryNextMachine(MachineId Value);
